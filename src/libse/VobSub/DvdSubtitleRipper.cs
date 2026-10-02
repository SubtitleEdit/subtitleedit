using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// Rips (demuxes) the subpicture packs out of the VOB files of a DVD title. The VOBs are read
    /// in big sequential chunks and only the 2048 byte sectors that are subtitle packs are kept, so
    /// this runs at disk speed. The PTS restarts at every cell, so each subtitle pack gets its PTS
    /// rewritten onto one continuous timeline starting at zero:
    /// - with a program chain from the IFO: only that chain's cells are read, and a subtitle is
    ///   placed at the cell's start time + its offset from the cell's first NAV pack (exact);
    /// - without one: every sector of every VOB is read and the restarts are guessed from the
    ///   NAV packs' VOBU start/end PTS (the SE 4 way).
    /// </summary>
    public static class DvdSubtitleRipper
    {
        private const int SectorSize = 0x800;
        private const int SectorsPerRead = 1024; // 2 MB
        private const int Mpeg2HeaderLength = 14;
        private const int PesIndex = Mpeg2HeaderLength;
        private const int PtsIndex = Mpeg2HeaderLength + 9;
        private const long TicksPerMillisecond = 90;

        /// <summary>
        /// Rips the cells of <paramref name="programChain"/> from the title's VOB files
        /// (VTS_xx_1.VOB, VTS_xx_2.VOB, ... in order - sector numbers run across them).
        /// Cells in VOB files that are missing are skipped.
        /// </summary>
        /// <param name="vobFileNames">The title's VOB files in order.</param>
        /// <param name="programChain">The program chain (from <see cref="IfoParser"/>) to rip.</param>
        /// <param name="progress">Called with (sectors done, total sectors) - from the calling thread.</param>
        /// <param name="cancellationToken">Checked between reads.</param>
        public static List<VobSubPack> Rip(IReadOnlyList<string> vobFileNames, IfoParser.ProgramChain programChain, Action<long, long> progress = null, CancellationToken cancellationToken = default)
        {
            var packs = new List<VobSubPack>();
            using (var reader = new VobSetReader(vobFileNames))
            {
                long total = 0;
                foreach (var cell in programChain.Cells)
                {
                    total += reader.ClampedSectorCount(cell.FirstSector, cell.LastSector);
                }

                var buffer = new byte[SectorsPerRead * SectorSize];
                long done = 0;
                foreach (var cell in programChain.Cells)
                {
                    var cellStartTicks = (long)Math.Round(cell.Start.TotalMilliseconds * TicksPerMillisecond);
                    long cellOriginPts = -1;
                    var inCell = true;
                    var sector = cell.FirstSector;
                    var lastSector = Math.Min(cell.LastSector, reader.SectorCount - 1);
                    while (sector <= lastSector)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var count = (int)Math.Min(SectorsPerRead, lastSector - sector + 1);
                        reader.ReadSectors(sector, count, buffer);
                        for (var i = 0; i < count; i++)
                        {
                            var offset = i * SectorSize;
                            if (!IsPackHeader(buffer, offset))
                            {
                                continue;
                            }

                            if (IsNavPack(buffer, offset))
                            {
                                // an interleaved (multi angle) block also holds other cells' VOBUs - skip those
                                var vobId = (buffer[offset + 0x41F] << 8) | buffer[offset + 0x420];
                                var cellId = buffer[offset + 0x422];
                                inCell = vobId == 0 || (vobId == cell.VobId && cellId == cell.CellId);
                                if (inCell && cellOriginPts < 0)
                                {
                                    cellOriginPts = Helper.GetEndian(buffer, offset + 0x39, 4);
                                }
                            }
                            else if (inCell && cellOriginPts >= 0 && IsSubtitlePack(buffer, offset))
                            {
                                packs.Add(MakePack(buffer, offset, pts => cellStartTicks + pts - cellOriginPts));
                            }
                        }

                        sector += count;
                        done += count;
                        progress?.Invoke(done, total);
                    }
                }
            }

            return packs;
        }

        /// <summary>
        /// Rips every subtitle pack of the VOB files, guessing the PTS restarts from the NAV packs
        /// (use the program chain overload when the IFO is available).
        /// </summary>
        /// <param name="vobFileNames">The VOB files in play order.</param>
        /// <param name="progress">Called with (bytes read, total bytes) - from the calling thread.</param>
        /// <param name="cancellationToken">Checked between reads.</param>
        public static List<VobSubPack> Rip(IReadOnlyList<string> vobFileNames, Action<long, long> progress = null, CancellationToken cancellationToken = default)
        {
            long total = 0;
            foreach (var fileName in vobFileNames)
            {
                total += new FileInfo(fileName).Length;
            }

            var state = new NavPtsState();
            var packs = new List<VobSubPack>();
            var buffer = new byte[SectorsPerRead * SectorSize];
            long done = 0;
            for (var vobNumber = 0; vobNumber < vobFileNames.Count; vobNumber++)
            {
                state.FirstNavStartPts = 0;
                using (var fs = OpenSequential(vobFileNames[vobNumber]))
                {
                    int bytesRead;
                    while ((bytesRead = ReadFull(fs, buffer, 0, buffer.Length)) > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // a short last sector is zero padded
                        var end = (bytesRead + SectorSize - 1) / SectorSize * SectorSize;
                        Array.Clear(buffer, bytesRead, end - bytesRead);
                        for (var offset = 0; offset < end; offset += SectorSize)
                        {
                            if (!IsPackHeader(buffer, offset))
                            {
                                continue;
                            }

                            if (IsSubtitlePack(buffer, offset))
                            {
                                var accumulated = state.AccumulatedPts;
                                packs.Add(accumulated == 0 ? MakePack(buffer, offset, null) : MakePack(buffer, offset, pts => pts + accumulated));
                            }
                            else if (IsNavPack(buffer, offset))
                            {
                                state.Update(buffer, offset, vobNumber);
                            }
                        }

                        done += bytesRead;
                        progress?.Invoke(Math.Min(done, total), total);
                    }
                }

                state.LastVobPts = state.LastPts;
            }

            return packs;
        }

        /// <summary>
        /// Number of packs that are still CSS encrypted (PES scrambling control set) - a VOB copied
        /// from the disc without decrypting. Past the first 128 bytes of such a sector the data is
        /// scrambled, so the sub picture it belongs to decodes as noise. SE does not decrypt CSS.
        /// </summary>
        public static int CountEncrypted(IEnumerable<VobSubPack> packs)
        {
            var count = 0;
            foreach (var pack in packs)
            {
                if (pack.PacketizedElementaryStream?.ScramblingControl > 0)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// SE 4's PTS stitching from the NAV packs' VOBU start/end PTS.
        /// </summary>
        private sealed class NavPtsState
        {
            public long LastPts;
            public long LastVobPts;
            public long AccumulatedPts;
            public long FirstNavStartPts;
            private long _lastNavEndPts;

            public void Update(byte[] buffer, int offset, int vobNumber)
            {
                long vobuStartPts = Helper.GetEndian(buffer, offset + 0x39, 4);
                long vobuEndPts = Helper.GetEndian(buffer, offset + 0x3D, 4);
                LastPts = vobuEndPts;

                if (FirstNavStartPts == 0)
                {
                    FirstNavStartPts = vobuStartPts;
                    if (vobNumber == 0)
                    {
                        AccumulatedPts = -vobuStartPts;
                    }
                }

                if (vobuStartPts + FirstNavStartPts + AccumulatedPts < LastVobPts)
                {
                    AccumulatedPts += _lastNavEndPts - vobuStartPts;
                }
                else if (_lastNavEndPts > vobuEndPts)
                {
                    AccumulatedPts += _lastNavEndPts - vobuStartPts;
                }

                _lastNavEndPts = vobuEndPts;
            }
        }

        /// <summary>
        /// The title's VOB files as one run of sectors.
        /// </summary>
        private sealed class VobSetReader : IDisposable
        {
            private readonly string[] _fileNames;
            private readonly long[] _firstSectors;
            private readonly FileStream[] _streams;

            public long SectorCount { get; }

            public VobSetReader(IReadOnlyList<string> fileNames)
            {
                _fileNames = new string[fileNames.Count];
                _firstSectors = new long[fileNames.Count + 1];
                _streams = new FileStream[fileNames.Count];
                for (var i = 0; i < fileNames.Count; i++)
                {
                    _fileNames[i] = fileNames[i];
                    _firstSectors[i + 1] = _firstSectors[i] + (new FileInfo(fileNames[i]).Length + SectorSize - 1) / SectorSize;
                }

                SectorCount = _firstSectors[fileNames.Count];
            }

            public long ClampedSectorCount(long first, long last)
            {
                return Math.Max(0, Math.Min(last, SectorCount - 1) - first + 1);
            }

            public void ReadSectors(long sector, int count, byte[] buffer)
            {
                var bufferOffset = 0;
                while (count > 0)
                {
                    var file = Array.BinarySearch(_firstSectors, sector);
                    file = file >= 0 ? file : ~file - 1;

                    // an empty VOB has the same first sector as the next one - move past it
                    while (file < _fileNames.Length - 1 && _firstSectors[file + 1] <= sector)
                    {
                        file++;
                    }

                    var sectorsInFile = file < _fileNames.Length ? (int)Math.Min(count, _firstSectors[file + 1] - sector) : 0;
                    if (sectorsInFile <= 0)
                    {
                        // past the last VOB
                        Array.Clear(buffer, bufferOffset, count * SectorSize);
                        break;
                    }

                    var stream = _streams[file] ?? (_streams[file] = OpenSequential(_fileNames[file]));
                    var position = (sector - _firstSectors[file]) * SectorSize;
                    if (stream.Position != position)
                    {
                        stream.Position = position;
                    }

                    var byteCount = sectorsInFile * SectorSize;
                    var read = ReadFull(stream, buffer, bufferOffset, byteCount);
                    Array.Clear(buffer, bufferOffset + read, byteCount - read);
                    bufferOffset += byteCount;
                    sector += sectorsInFile;
                    count -= sectorsInFile;
                }
            }

            public void Dispose()
            {
                foreach (var stream in _streams)
                {
                    stream?.Dispose();
                }
            }
        }

        private static FileStream OpenSequential(string fileName)
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, FileOptions.SequentialScan);
        }

        private static int ReadFull(Stream stream, byte[] buffer, int offset, int count)
        {
            var total = 0;
            while (total < count)
            {
                var read = stream.Read(buffer, offset + total, count - total);
                if (read <= 0)
                {
                    break;
                }

                total += read;
            }

            return total;
        }

        /// <summary>MPEG-2 pack header 00 00 01 BA.</summary>
        private static bool IsPackHeader(byte[] buffer, int offset)
        {
            return buffer[offset] == 0 && buffer[offset + 1] == 0 && buffer[offset + 2] == 1 && buffer[offset + 3] == 0xBA;
        }

        /// <summary>Private stream 1 right after the pack header with a sub stream id of 0x20-0x3f.</summary>
        private static bool IsSubtitlePack(byte[] buffer, int offset)
        {
            var pes = offset + PesIndex;
            if (buffer[pes] != 0 || buffer[pes + 1] != 0 || buffer[pes + 2] != 1 || buffer[pes + 3] != 0xBD)
            {
                return false;
            }

            var subStreamIdIndex = pes + 9 + buffer[pes + 8];
            return subStreamIdIndex < offset + SectorSize && VobSubParser.IsSubtileStreamId(buffer[subStreamIdIndex]);
        }

        /// <summary>System header, then PCI (private stream 2 at 0x26) and DSI (private stream 2 at 0x400).</summary>
        private static bool IsNavPack(byte[] buffer, int offset)
        {
            return buffer[offset + PesIndex + 3] == 0xBB &&
                   Helper.GetEndian(buffer, offset + 0x26, 4) == 0x1BF &&
                   Helper.GetEndian(buffer, offset + 0x400, 4) == 0x1BF;
        }

        private static VobSubPack MakePack(byte[] buffer, int offset, Func<long, long> mapPts)
        {
            var sector = new byte[SectorSize];
            Buffer.BlockCopy(buffer, offset, sector, 0, SectorSize);
            var pack = new VobSubPack(sector, null);
            var pts = pack.PacketizedElementaryStream.PresentationTimestamp;
            if (mapPts == null || !pts.HasValue)
            {
                return pack;
            }

            WritePresentationTimestamp(sector, Math.Max(0, mapPts((long)pts.Value)), pack.PacketizedElementaryStream.PresentationTimestampDecodeTimestampFlags);
            return new VobSubPack(sector, null);
        }

        /// <summary>
        /// Writes a 33 bit PTS into the PES header ('0010'/'0011' + 3 bits, marker, 15 bits, marker, 15 bits, marker).
        /// </summary>
        private static void WritePresentationTimestamp(byte[] buffer, long pts, int ptsDtsFlags)
        {
            var prefix = ptsDtsFlags == 0b11 ? 0b0011_0000 : 0b0010_0000;
            buffer[PtsIndex] = (byte)(prefix | (int)((pts >> 29) & 0b1110) | 1);
            buffer[PtsIndex + 1] = (byte)(pts >> 22);
            buffer[PtsIndex + 2] = (byte)(((pts >> 14) & 0b1111_1110) | 1);
            buffer[PtsIndex + 3] = (byte)(pts >> 7);
            buffer[PtsIndex + 4] = (byte)(((pts << 1) & 0b1111_1110) | 1);
        }
    }
}
