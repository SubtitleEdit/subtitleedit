using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class VobSubParser
    {
        public bool IsPal { get; private set; }
        public List<VobSubPack> VobSubPacks { get; private set; }
        public List<SKColor> IdxPalette { get; private set; } = new List<SKColor>();
        public List<string> IdxLanguages { get; private set; } = new List<string>();

        private const int PacketizedElementaryStreamMaximumLength = 2028;

        /// <summary>
        /// Minimum length of a releavent sub packet.
        /// mpeg header length (14) + pes header length (6) + mpeg2 extension (3) + 1 because we care about the stream id.
        /// </summary>
        private static readonly int s_minMpeg2SectionLength = Mpeg2Header.Length + PacketizedElementaryStream.HeaderLength + 3 + 1;

        public VobSubParser(bool isPal)
        {
            IsPal = isPal;
            VobSubPacks = new List<VobSubPack>();
        }

        public void Open(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Open(fs);
            }
        }


        /// <summary>
        /// Can be used with e.g. MemoryStream or FileStream
        /// </summary>
        /// <param name="ms"></param>
        public void Open(Stream ms)
        {
            VobSubPacks = new List<VobSubPack>();
            ms.Position = 0;
            long position = 0;
            while (position + s_minMpeg2SectionLength < ms.Length)
            {
                var header = new byte[4];
                ms.Seek(position, SeekOrigin.Begin);
                ms.Read(header, 0, header.Length);
                if (IsMpeg2PackHeader(header))
                {
                    if (!IsStartOfMpeg2Pack(ms, position))
                    {
                        long newPos = FindNextMpeg2PackHeader(ms, position + 1);
                        if (newPos > position)
                        {
                            position = newPos;
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    // find how much stuffing there is
                    byte[] mpeg2HeaderBuffer = new byte[Mpeg2Header.Length];
                    ms.Seek(position, SeekOrigin.Begin);
                    ms.Read(mpeg2HeaderBuffer, 0, mpeg2HeaderBuffer.Length);
                    int stuffingBytes = mpeg2HeaderBuffer[13] & 0b111;
                    ms.Seek(stuffingBytes, SeekOrigin.Current);

                    // skip stuffing and go to pes packet length
                    byte[] pesPacketHeaderBuffer = new byte[6];
                    ms.Read(pesPacketHeaderBuffer, 0, pesPacketHeaderBuffer.Length);
                    int packetLength = (int)(Helper.GetEndian(pesPacketHeaderBuffer, 4, 2) & 0xffff);

                    // create a new clean buffer without the stuffing.
                    byte[] cleanBuffer = new byte[mpeg2HeaderBuffer.Length + pesPacketHeaderBuffer.Length + packetLength];
                    Buffer.BlockCopy(mpeg2HeaderBuffer, 0, cleanBuffer, 0, mpeg2HeaderBuffer.Length);
                    Buffer.BlockCopy(pesPacketHeaderBuffer, 0, cleanBuffer, mpeg2HeaderBuffer.Length, pesPacketHeaderBuffer.Length);
                    ms.Read(cleanBuffer, mpeg2HeaderBuffer.Length + pesPacketHeaderBuffer.Length, packetLength);
                    // since we are cutting out the mpeg header stuffing bytes
                    // update the cleaned data to say we don't have any.
                    cleanBuffer[13] = (byte)(cleanBuffer[13] & 0b1111_1000);
                    VobSubPacks.Add(new VobSubPack(cleanBuffer, null));
                    position += cleanBuffer.Length;
                }
                else if (IsProgramEnd(header, 0))
                {
                    // end of program, should take exactly 4 bytes
                    position += 4;
                }
                else if (IsPaddingStream(header, 0))
                {
                    byte[] pesPacketLengthBuffer = new byte[2];
                    ms.Read(pesPacketLengthBuffer, 0, pesPacketLengthBuffer.Length);
                    position += PacketizedElementaryStream.HeaderLength + Helper.GetEndian(pesPacketLengthBuffer, 0, pesPacketLengthBuffer.Length);
                }
                else
                {
                    long newPos = FindNextMpeg2PackHeader(ms, position + 1);
                    if (newPos > position)
                    {
                        position = newPos;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        private static bool IsStartOfMpeg2Pack(Stream ms, long position)
        {
            // we won't be able to do anything if the private stream is empty.
            if (position + s_minMpeg2SectionLength >= ms.Length)
            {
                return false;
            }

            // fingerprint the mpeg2 header
            var buffer = new byte[Mpeg2Header.Length];
            ms.Seek(position, SeekOrigin.Begin);
            ms.Read(buffer, 0, buffer.Length);
            if (!IsMpeg2PackHeader(buffer))
            {
                return false;
            }

            if ((buffer[4] & 0b1100_0100) != 0b0100_0100)
            {
                return false;
            }
            // byte 6 and 8 needs xxxx_x1xx
            if ((buffer[6] & 0b0000_0100) != 0b0000_0100)
            {
                return false;
            }

            if ((buffer[8] & 0b0000_0100) != 0b0000_0100)
            {
                return false;
            }
            // byte 9 needs xxxx_xxx1
            if ((buffer[9] & 0b0000_0001) != 0b0000_0001)
            {
                return false;
            }
            // byte 12 has a hard req of xxxx_xx11
            if ((buffer[12] & 0b0000_0011) != 0b0000_0011)
            {
                return false;
            }
            // mux value can't be 0
            if (buffer[10] == 0x00 && buffer[11] == 0x00 && buffer[12] == 0x03)
            {
                return false;
            }
            // there should be at least room for the whole private header (8 bytes) + the stream id byte
            if (ms.Position + (buffer[13] & 0b111) + 9 >= ms.Length)
            {
                return false;
            }

            // fingerprint the private stream 1
            var privateStreamBuffer = new byte[9];
            ms.Seek(buffer[13] & 0b111, SeekOrigin.Current);
            ms.Read(privateStreamBuffer, 0, privateStreamBuffer.Length);
            if (!IsPrivateStream1(privateStreamBuffer, 0))
            {
                return false;
            }
            // byte 6 needs 10xx_xxxx
            if ((privateStreamBuffer[6] & 0b1100_0000) != 0b1000_0000)
            {
                return false;
            }
            // check if we have enough data for this packet
            if (ms.Position - 3 + Helper.GetEndian(privateStreamBuffer, 4, 2) > ms.Length)
            {
                return false;
            }
            // check if we have enough data for the header data + stream id
            if (ms.Position + privateStreamBuffer[8] + 1 > ms.Length)
            {
                return false;
            }
            // check for a valid private stream id.
            ms.Seek(privateStreamBuffer[8], SeekOrigin.Current);
            if (!IsSubtileStreamId(ms.ReadByte() & 0xff))
            {
                return false;
            }

            // all good
            return true;
        }


        private static long FindNextMpeg2PackHeader(Stream ms, long position)
        {
            // If it doesn't have the pes start code? Just assume something went really wrong.
            // scan until we can fingerprint an other mpeg2 packet header.
            while (position + s_minMpeg2SectionLength < ms.Length)
            {
                if (IsStartOfMpeg2Pack(ms, position))
                {
                    return position;
                }
                position++;
            }

            return -1;
        }

        public void OpenSubIdx(string vobSubFileName, string idxFileName)
        {
            VobSubPacks = new List<VobSubPack>();

            if (!string.IsNullOrEmpty(idxFileName) && File.Exists(idxFileName))
            {
                var idx = new Idx(idxFileName);
                IdxPalette = idx.Palette;
                IdxLanguages = idx.Languages;
                if (idx.IdxParagraphs.Count > 0)
                {
                    var buffer = new byte[0x800]; // 2048
                    using (var fs = new FileStream(vobSubFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        foreach (var p in idx.IdxParagraphs)
                        {
                            if (p.FilePosition + 100 < fs.Length)
                            {
                                long position = p.FilePosition;
                                fs.Seek(position, SeekOrigin.Begin);
                                fs.Read(buffer, 0, 0x0800);
                                if (IsSubtitlePack(buffer) || IsPrivateStream1(buffer, 0))
                                {
                                    var vsp = new VobSubPack(buffer, p);
                                    VobSubPacks.Add(vsp);

                                    if (IsPrivateStream1(buffer, 0))
                                    {
                                        position += vsp.PacketizedElementaryStream.Length + 6;
                                    }
                                    else
                                    {
                                        position += 0x800;
                                    }

                                    int currentSubPictureStreamId = 0;
                                    if (vsp.PacketizedElementaryStream.SubPictureStreamId != null)
                                    {
                                        currentSubPictureStreamId = vsp.PacketizedElementaryStream.SubPictureStreamId.Value;
                                    }

                                    while (vsp.PacketizedElementaryStream != null &&
                                           vsp.PacketizedElementaryStream.SubPictureStreamId.HasValue &&
                                           (vsp.PacketizedElementaryStream.Length == PacketizedElementaryStreamMaximumLength ||
                                            currentSubPictureStreamId != vsp.PacketizedElementaryStream.SubPictureStreamId.Value) && position < fs.Length)
                                    {
                                        fs.Seek(position, SeekOrigin.Begin);
                                        fs.Read(buffer, 0, 0x800);
                                        vsp = new VobSubPack(buffer, p); // idx position?

                                        if (vsp.PacketizedElementaryStream != null && vsp.PacketizedElementaryStream.SubPictureStreamId.HasValue && currentSubPictureStreamId == vsp.PacketizedElementaryStream.SubPictureStreamId.Value)
                                        {
                                            VobSubPacks.Add(vsp);

                                            if (IsPrivateStream1(buffer, 0))
                                            {
                                                position += vsp.PacketizedElementaryStream.Length + 6;
                                            }
                                            else
                                            {
                                                position += 0x800;
                                            }
                                        }
                                        else
                                        {
                                            position += 0x800;
                                            fs.Seek(position, SeekOrigin.Begin);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return;
                }
            }

            // No valid idx file found - just open like vob file
            Open(vobSubFileName);
        }

        /// <summary>
        /// Demultiplex multiplexed packs together each streamId at a time + removing bad packs + fixing displaytimes
        /// </summary>
        /// <returns>List of complete packs each with a complete sub image</returns>
        public List<VobSubMergedPack> MergeVobSubPacks()
        {
            var list = new List<VobSubMergedPack>();
            var pts = new TimeSpan();
            var ms = new MemoryStream();
            int streamId = 0;

            float ticksPerMillisecond = 90.000F;
            if (!IsPal)
            {
                ticksPerMillisecond = 90.090F * (23.976F / 24F);
            }

            // get unique streamIds
            var uniqueStreamIds = new List<int>();
            foreach (var p in VobSubPacks)
            {
                if (p.PacketizedElementaryStream != null &&
                    p.PacketizedElementaryStream.SubPictureStreamId.HasValue &&
                    !uniqueStreamIds.Contains(p.PacketizedElementaryStream.SubPictureStreamId.Value))
                {
                    uniqueStreamIds.Add(p.PacketizedElementaryStream.SubPictureStreamId.Value);
                }
            }

            IdxParagraph lastIdxParagraph = null;
            foreach (int uniqueStreamId in uniqueStreamIds) // packets must be merged in streamId order (so they don't get mixed)
            {
                foreach (var p in VobSubPacks)
                {
                    if (p.PacketizedElementaryStream != null && p.PacketizedElementaryStream.SubPictureStreamId.HasValue &&
                        p.PacketizedElementaryStream.SubPictureStreamId.Value == uniqueStreamId)
                    {
                        if (p.PacketizedElementaryStream.PresentationTimestampDecodeTimestampFlags > 0)
                        {
                            if (lastIdxParagraph == null || p.IdxLine.FilePosition != lastIdxParagraph.FilePosition)
                            {
                                if (ms.Length > 0)
                                {
                                    list.Add(new VobSubMergedPack(ms.ToArray(), pts, streamId, lastIdxParagraph));
                                }

                                ms.Close();
                                ms = new MemoryStream();
                                pts = TimeSpan.FromMilliseconds(Convert.ToDouble(p.PacketizedElementaryStream.PresentationTimestamp / ticksPerMillisecond)); //90000F * 1000)); (PAL)
                                streamId = p.PacketizedElementaryStream.SubPictureStreamId.Value;
                            }
                        }
                        lastIdxParagraph = p.IdxLine;
                        p.PacketizedElementaryStream.WriteToStream(ms);
                    }
                }
                if (ms.Length > 0)
                {
                    list.Add(new VobSubMergedPack(ms.ToArray(), pts, streamId, lastIdxParagraph));
                    ms.Close();
                    ms = new MemoryStream();
                }
            }
            ms.Close();

            // Remove any bad packs
            for (int i = list.Count - 1; i >= 0; i--)
            {
                VobSubMergedPack pack = list[i];
                if (pack.SubPicture == null || pack.SubPicture.ImageDisplayArea.Width <= 3 || pack.SubPicture.ImageDisplayArea.Height <= 2)
                {
                    list.RemoveAt(i);
                }
                else if (pack.EndTime.TotalSeconds - pack.StartTime.TotalSeconds < 0.1 && pack.SubPicture.ImageDisplayArea.Width <= 10 && pack.SubPicture.ImageDisplayArea.Height <= 10)
                {
                    list.RemoveAt(i);
                }
            }

            // Fix subs with no duration (completely normal) or negative duration or duration > 10 seconds
            for (int i = 0; i < list.Count; i++)
            {
                VobSubMergedPack pack = list[i];
                if (pack.SubPicture.Delay.TotalMilliseconds > 0)
                {
                    pack.EndTime = pack.StartTime.Add(pack.SubPicture.Delay);
                }

                if (pack.EndTime < pack.StartTime || pack.EndTime.TotalMilliseconds - pack.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    if (i + 1 < list.Count)
                    {
                        pack.EndTime = TimeSpan.FromMilliseconds(list[i + 1].StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines);
                        if (pack.EndTime.TotalMilliseconds - pack.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            pack.EndTime = TimeSpan.FromMilliseconds(pack.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds);
                        }
                    }
                    else
                    {
                        pack.EndTime = TimeSpan.FromMilliseconds(pack.StartTime.TotalMilliseconds + 3000);
                    }
                }
            }

            return list;
        }

        public static bool IsMpeg2PackHeader(byte[] buffer)
        {
            return buffer.Length >= 4 &&
                   buffer[0] == 0 &&
                   buffer[1] == 0 &&
                   buffer[2] == 1 &&
                   buffer[3] == 0xba; // 0xba == 186 - MPEG-2 Pack Header
        }

        public static bool IsPrivateStream1(byte[] buffer, int index)
        {
            return buffer.Length >= index + 4 &&
                   buffer[index + 0] == 0 &&
                   buffer[index + 1] == 0 &&
                   buffer[index + 2] == 1 &&
                   buffer[index + 3] == 0xbd; // 0xbd == 189 - MPEG-2 Private stream 1 (non MPEG audio, subpictures)
        }

        public static bool IsPaddingStream(byte[] buffer, int index)
        {
            return buffer.Length >= index + 4 &&
                   buffer[index + 0] == 0 &&
                   buffer[index + 1] == 0 &&
                   buffer[index + 2] == 1 &&
                   buffer[index + 3] == 0xbe; // 0xbd == 190 - padding stream
        }

        public static bool IsProgramEnd(byte[] buffer, int index)
        {
            return buffer.Length >= index + 4 &&
                   buffer[index + 0] == 0 &&
                   buffer[index + 1] == 0 &&
                   buffer[index + 2] == 1 &&
                   buffer[index + 3] == 0xb9; // 0xbd == 190 - padding stream
        }

        public static bool IsPrivateStream2(byte[] buffer, int index)
        {
            return buffer.Length >= index + 4 &&
                   buffer[index + 0] == 0 &&
                   buffer[index + 1] == 0 &&
                   buffer[index + 2] == 1 &&
                   buffer[index + 3] == 0xbf; // 0xbf == 191 - MPEG-2 Private stream 2
        }

        public static bool IsSubtitlePack(byte[] buffer)
        {
            if (IsMpeg2PackHeader(buffer) && IsPrivateStream1(buffer, Mpeg2Header.Length))
            {
                byte pesHeaderDataLength = buffer[Mpeg2Header.Length + 8];
                byte streamId = buffer[Mpeg2Header.Length + 8 + 1 + pesHeaderDataLength];
                return IsSubtileStreamId(streamId);
            }

            return false;
        }
        public static bool IsSubtileStreamId(int streamId)
        {
            // Subtitle IDs allowed (or x3f to x40?)
            return streamId >= 0x20 && streamId <= 0x3f;
        }

    }
}
