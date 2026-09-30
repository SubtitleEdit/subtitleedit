using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream
{
    /// <summary>
    /// Reads CEA-608 closed captions from the MPEG video of an MPEG program stream (DVD .vob, .mpg):
    /// DVD style Line 21 captions ("CC" GOP user data), ATSC A/53 ("GA94") and SCTE 20 picture
    /// user data. The video is streamed - nothing but the small header/user data units is kept.
    /// </summary>
    public static class ProgramStreamClosedCaptionReader
    {
        public delegate void ProgressCallback(long position, long total);

        /// <summary>
        /// Stop reading when this much video (from its first picture) had no captions - a file
        /// without captions is then not read to the end.
        /// </summary>
        public const long DefaultProbeMilliseconds = 60_000;

        private const long PtsWrap = 1L << 33;
        private const long PtsDiscontinuity = 10 * 90_000; // 10 seconds backwards is a new PTS base, not frame reordering
        private const long PtsFrame = 3003; // one NTSC frame at 90 kHz

        /// <summary>
        /// True if the stream starts with an MPEG pack header (00 00 01 BA).
        /// </summary>
        public static bool IsProgramStream(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                return fs.ReadFully(buffer, 0, 4) == 4 && buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 1 && buffer[3] == 0xBA;
            }
        }

        /// <summary>
        /// True if the file is a bare MPEG-1/2 video elementary stream (.m2v/.m1v), i.e. starts with a
        /// sequence header (00 00 01 B3) instead of a pack header.
        /// </summary>
        public static bool IsVideoElementaryStream(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                return fs.ReadFully(buffer, 0, 4) == 4 && buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 1 && buffer[3] == 0xB3;
            }
        }

        /// <summary>
        /// Reads the closed captions of a bare MPEG video elementary stream. There are no PES
        /// timestamps, so picture times come from the frame rate and the GOP structure alone,
        /// counted from the first picture.
        /// </summary>
        /// <returns>Paragraphs per track key (1-4 = CC1-CC4, see <see cref="ClosedCaptionDecoder"/>), empty if none</returns>
        public static SortedDictionary<int, List<Paragraph>> ReadElementaryStream(Stream stream, long probeMilliseconds, ProgressCallback progressCallback)
        {
            var scanner = new VideoScanner(probeMilliseconds);
            var buffer = ArrayPool<byte>.Shared.Rent(65536);
            try
            {
                var length = stream.Length;
                long chunks = 0;
                int read;
                while (!scanner.Stop && (read = stream.Read(buffer, 0, 65536)) > 0)
                {
                    scanner.Feed(buffer.AsSpan(0, read), null);
                    if (++chunks % 100 == 0)
                    {
                        progressCallback?.Invoke(stream.Position, length);
                    }
                }

                progressCallback?.Invoke(length, length);
                return scanner.Finish();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static SortedDictionary<int, List<Paragraph>> Read(string fileName, long probeMilliseconds, ProgressCallback progressCallback)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 1024))
            {
                return IsVideoElementaryStream(fileName)
                    ? ReadElementaryStream(fs, probeMilliseconds, progressCallback)
                    : Read(fs, probeMilliseconds, progressCallback);
            }
        }

        /// <summary>
        /// Reads the closed captions of the first video stream.
        /// </summary>
        /// <returns>Paragraphs per track key (1-4 = CC1-CC4, see <see cref="ClosedCaptionDecoder"/>), empty if none</returns>
        public static SortedDictionary<int, List<Paragraph>> Read(Stream stream, long probeMilliseconds, ProgressCallback progressCallback)
        {
            var scanner = new VideoScanner(probeMilliseconds);
            var header = new byte[16];
            var payload = ArrayPool<byte>.Shared.Rent(65536);
            try
            {
                int? videoStreamId = null;
                long lastPts = -1;
                long ptsOffset = 0;
                var length = stream.Length;
                long packets = 0;
                while (!scanner.Stop && FindNextStartCode(stream, header))
                {
                    var streamId = header[3];
                    if (streamId == 0xBA) // pack header
                    {
                        if (stream.ReadFully(header, 4, 1) < 1)
                        {
                            break;
                        }

                        if ((header[4] & 0xC0) == 0x40) // MPEG-2: 14 bytes + stuffing
                        {
                            if (stream.ReadFully(header, 5, 9) < 9)
                            {
                                break;
                            }

                            stream.Seek(header[13] & 0x07, SeekOrigin.Current);
                        }
                        else // MPEG-1: 12 bytes
                        {
                            stream.Seek(7, SeekOrigin.Current);
                        }

                        continue;
                    }

                    if (streamId == 0xB9) // program end
                    {
                        continue;
                    }

                    if (streamId < 0xBB) // not a PES/system header - resync
                    {
                        continue;
                    }

                    if (stream.ReadFully(header, 4, 2) < 2)
                    {
                        break;
                    }

                    var packetLength = (header[4] << 8) | header[5];
                    var isVideo = streamId >= 0xE0 && streamId <= 0xEF && (videoStreamId == null || videoStreamId == streamId);
                    if (!isVideo || packetLength == 0)
                    {
                        stream.Seek(packetLength, SeekOrigin.Current);
                        continue;
                    }

                    videoStreamId = streamId;
                    var read = stream.ReadFully(payload, 0, packetLength);
                    var dataStart = GetPesDataStart(payload.AsSpan(0, read), out var rawPts);
                    long? ptsMs = null;
                    if (rawPts.HasValue)
                    {
                        var pts = rawPts.Value + ptsOffset;
                        if (lastPts >= 0 && pts < lastPts - PtsWrap / 2)
                        {
                            // the 33-bit PTS wrapped (every ~26.5 hours)
                            ptsOffset += PtsWrap;
                            pts += PtsWrap;
                        }
                        else if (lastPts >= 0 && pts > lastPts + PtsWrap / 2)
                        {
                            pts -= PtsWrap; // a reordered frame from just before the wrap
                        }
                        else if (lastPts >= 0 && pts < lastPts - PtsDiscontinuity)
                        {
                            // PTS reset (e.g. a new VOB ID in a DVD .vob) - continue after the previous part
                            var shift = lastPts + PtsFrame - pts;
                            ptsOffset += shift;
                            pts += shift;
                        }

                        lastPts = Math.Max(lastPts, pts);
                        ptsMs = pts / 90;
                    }

                    if (dataStart >= 0 && dataStart < read)
                    {
                        scanner.Feed(payload.AsSpan(dataStart, read - dataStart), ptsMs);
                    }

                    if (++packets % 10000 == 0)
                    {
                        progressCallback?.Invoke(stream.Position, length);
                    }
                }

                progressCallback?.Invoke(length, length);
                return scanner.Finish();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(payload);
            }
        }

        /// <summary>
        /// Positions the stream after the next 00 00 01 xx start code; header[0..3] gets the code.
        /// </summary>
        private static bool FindNextStartCode(Stream stream, byte[] header)
        {
            var state = 0xFFFFFF;
            int b;
            while ((b = stream.ReadByte()) >= 0)
            {
                if (state == 0x000001)
                {
                    header[0] = 0;
                    header[1] = 0;
                    header[2] = 1;
                    header[3] = (byte)b;
                    return true;
                }

                state = ((state << 8) | b) & 0xFFFFFF;
            }

            return false;
        }

        /// <summary>
        /// Where the elementary stream data starts in a PES packet body (after the 6-byte PES
        /// start/length) - MPEG-2 or MPEG-1 PES header - and its raw 33-bit PTS (90 kHz), if any.
        /// </summary>
        internal static int GetPesDataStart(ReadOnlySpan<byte> data, out long? pts)
        {
            pts = null;
            var length = data.Length;
            if (length < 3)
            {
                return -1;
            }

            if ((data[0] & 0xC0) == 0x80) // MPEG-2 PES header
            {
                var headerDataLength = data[2];
                if ((data[1] & 0x80) != 0 && length >= 8)
                {
                    pts = ReadPts(data, 3);
                }

                return 3 + headerDataLength;
            }

            // MPEG-1: stuffing, optional STD buffer, then PTS / PTS+DTS / nothing
            var i = 0;
            while (i < length && data[i] == 0xFF && i < 16)
            {
                i++;
            }

            if (i < length && (data[i] & 0xC0) == 0x40)
            {
                i += 2;
            }

            if (i >= length)
            {
                return -1;
            }

            if ((data[i] & 0xF0) == 0x20 && i + 5 <= length)
            {
                pts = ReadPts(data, i);
                return i + 5;
            }

            if ((data[i] & 0xF0) == 0x30 && i + 10 <= length)
            {
                pts = ReadPts(data, i);
                return i + 10;
            }

            return i + 1; // 0x0F = no timestamps
        }

        private static long ReadPts(ReadOnlySpan<byte> data, int index)
        {
            return ((long)(data[index] & 0x0E) << 29) | ((long)data[index + 1] << 22) | ((long)(data[index + 2] & 0xFE) << 14) |
                   ((long)data[index + 3] << 7) | ((long)data[index + 4] >> 1);
        }

        /// <summary>
        /// Scans the MPEG video elementary stream across PES packet boundaries, keeping only the
        /// units that matter for captions.
        /// </summary>
        private sealed class VideoScanner
        {
            private const int MaxUnitLength = 8192;
            private static readonly byte[] StartCode = { 0, 0, 1 };

            private readonly long _probeMilliseconds;
            private readonly ClosedCaptionDecoder _decoder = new ClosedCaptionDecoder();
            private readonly byte[] _carry = new byte[3];
            private int _carryLength;
            private byte[] _work = new byte[65536 + 3];
            private readonly byte[] _unit = new byte[MaxUnitLength];
            private int _unitLength;
            private int _unitCode = -1; // start code value of the unit being collected, -1 = not collecting
            private long? _ptsForNextPicture;
            private long? _picturePts; // PTS of the picture whose header is being collected
            private bool _accessUnitStarted;
            private long? _pictureMs; // display time of the current picture
            private long? _firstPictureMs;
            private double? _gopBaseMs; // display time of the GOP's first frame (temporal_reference 0)
            private double? _previousGopBaseMs;
            private int _gopFrameCount;
            private int _previousGopFrameCount;
            private int _gopFieldCount; // displayed fields, so 3:2 pulldown (repeat_first_field) counts
            private int _previousGopFieldCount;
            private double _frameMs = 1001.0 / 30;
            private bool _topFieldFirst = true;
            private List<CcData[]> _pendingDvdFrames;
            private readonly List<CcData> _ccData = new List<CcData>();
            private readonly CcDataParseState _parseState = new CcDataParseState();

            public VideoScanner(long probeMilliseconds)
            {
                _probeMilliseconds = probeMilliseconds;
            }

            public bool Stop { get; private set; }

            public void Feed(ReadOnlySpan<byte> payload, long? ptsMs)
            {
                if (ptsMs.HasValue)
                {
                    _ptsForNextPicture = ptsMs; // the PTS belongs to the first picture starting in this packet
                }

                var workLength = _carryLength + payload.Length;
                if (_work.Length < workLength)
                {
                    _work = new byte[workLength];
                }

                Array.Copy(_carry, _work, _carryLength);
                payload.CopyTo(_work.AsSpan(_carryLength));
                var work = _work.AsSpan(0, workLength);

                var position = 0;
                while (true)
                {
                    var index = work.Slice(position).IndexOf(StartCode);
                    if (index < 0 || position + index + 3 >= work.Length)
                    {
                        // keep a possible partial start code for the next packet
                        var end = index >= 0 ? position + index : Math.Max(position, work.Length - 3);
                        AppendToUnit(work.Slice(position, end - position));
                        _carryLength = work.Length - end;
                        work.Slice(end).CopyTo(_carry);
                        return;
                    }

                    var start = position + index;
                    AppendToUnit(work.Slice(position, start - position));
                    FinishUnit();
                    var code = work[start + 3];
                    _unitCode = code == 0x00 || code == 0xB2 || code == 0xB3 || code == 0xB5 ? code : -1;
                    _unitLength = 0;
                    // A PES packet's PTS belongs to the access unit (picture) that starts in it - and
                    // a picture starts with its sequence/GOP header when it has one, which can be in
                    // the packet before the one with its picture start code.
                    if ((code == 0x00 || code == 0xB3 || code == 0xB8) && !_accessUnitStarted)
                    {
                        _picturePts = _ptsForNextPicture;
                        _ptsForNextPicture = null;
                        _accessUnitStarted = true;
                    }

                    if (code == 0x00)
                    {
                        _accessUnitStarted = false;
                    }
                    else if (code == 0xB8)
                    {
                        StartGop();
                    }

                    position = start + 4;
                }
            }

            public SortedDictionary<int, List<Paragraph>> Finish()
            {
                AppendToUnit(_carry.AsSpan(0, _carryLength)); // the last bytes of the stream
                _carryLength = 0;
                FinishUnit();
                EmitPendingDvdFrames();
                return _decoder.HasData
                    ? _decoder.Finish(_firstPictureMs ?? 0)
                    : new SortedDictionary<int, List<Paragraph>>();
            }

            private void AppendToUnit(ReadOnlySpan<byte> data)
            {
                if (_unitCode < 0 || data.Length == 0)
                {
                    return;
                }

                var count = Math.Min(data.Length, MaxUnitLength - _unitLength);
                data.Slice(0, count).CopyTo(_unit.AsSpan(_unitLength));
                _unitLength += count;
            }

            private void StartGop()
            {
                EmitPendingDvdFrames(); // the previous GOP had no PTS - use its estimated start

                if (_gopBaseMs.HasValue)
                {
                    _previousGopBaseMs = _gopBaseMs;
                    _previousGopFrameCount = _gopFrameCount;
                    _previousGopFieldCount = _gopFieldCount;
                }

                _gopBaseMs = null;
                _gopFrameCount = 0;
                _gopFieldCount = 0;
            }

            /// <summary>
            /// DVD captions come as GOP user data before the GOP's first picture - one caption
            /// block per frame in display order, from the GOP's first frame.
            /// </summary>
            private void EmitPendingDvdFrames()
            {
                if (_pendingDvdFrames == null || _gopBaseMs == null)
                {
                    return;
                }

                for (var i = 0; i < _pendingDvdFrames.Count; i++)
                {
                    _decoder.AddFrame((long)Math.Round(_gopBaseMs.Value + i * _frameMs), _pendingDvdFrames[i]);
                }

                _pendingDvdFrames = null;
            }

            /// <summary>
            /// Picture header: temporal_reference (10 bits) is the display position in the GOP, so
            /// every picture - also those without a PTS of their own - gets its display time from
            /// the GOP's first frame.
            /// </summary>
            private void Picture(int temporalReference)
            {
                _gopFrameCount = Math.Max(_gopFrameCount, temporalReference + 1);
                _gopFieldCount += 2; // a frame picture; the picture coding extension corrects it
                if (_picturePts.HasValue)
                {
                    _gopBaseMs = _picturePts.Value - temporalReference * _frameMs;
                }
                else if (_gopBaseMs == null)
                {
                    // No PTS yet in this GOP - estimate from the previous one. Its length is its displayed
                    // fields: with 3:2 pulldown 9 film pictures last 11 frames, and counting 9 made the
                    // GOPs overlap, interleaving their DVD caption bytes (a bare .m2v has no PTS at all).
                    var previousGopFrames = _previousGopFieldCount > 0 ? _previousGopFieldCount / 2.0 : _previousGopFrameCount;
                    _gopBaseMs = _previousGopBaseMs.HasValue ? _previousGopBaseMs + previousGopFrames * _frameMs : 0;
                }

                _pictureMs = (long)Math.Round(_gopBaseMs.Value + temporalReference * _frameMs);
                if (_firstPictureMs == null || _pictureMs < _firstPictureMs)
                {
                    _firstPictureMs = _pictureMs;
                }

                if (_picturePts.HasValue)
                {
                    EmitPendingDvdFrames(); // the GOP's start is known exactly now
                }

                // caption user data with only padding so far still means captions may come later
                if (!_parseState.CaptionDataSeen && !_decoder.HasData && _pictureMs - _firstPictureMs > _probeMilliseconds)
                {
                    Stop = true;
                }
            }

            private void FinishUnit()
            {
                var unit = _unit.AsSpan(0, _unitLength);
                switch (_unitCode)
                {
                    case 0x00 when unit.Length >= 2: // picture header
                        Picture((unit[0] << 2) | (unit[1] >> 6));
                        break;
                    case 0xB3 when unit.Length >= 4: // sequence header: frame_rate_code
                        _frameMs = GetFrameMilliseconds(unit[3] & 0x0F);
                        break;
                    case 0xB5 when unit.Length >= 4 && (unit[0] >> 4) == 8: // picture coding extension
                        _topFieldFirst = (unit[3] & 0x80) != 0;
                        if ((unit[2] & 0x03) != 0x03)
                        {
                            _gopFieldCount--; // field picture: one field
                        }
                        else if ((unit[3] & 0x02) != 0)
                        {
                            _gopFieldCount++; // repeat_first_field
                        }

                        break;
                    case 0xB2:
                        var dvdFrames = GetCcDataHelper.ParseDvdCaptionUserData(unit);
                        if (dvdFrames.Count > 0)
                        {
                            _pendingDvdFrames = dvdFrames;
                            _parseState.CaptionDataSeen = true;
                        }
                        else if (_pictureMs.HasValue)
                        {
                            _ccData.Clear();
                            GetCcDataHelper.ParseCcDataFromMpeg2UserData(unit, _topFieldFirst, _ccData, _parseState);
                            _decoder.AddFrame(_pictureMs.Value, _ccData.ToArray());
                        }

                        break;
                }

                _unitCode = -1;
                _unitLength = 0;
            }

            private static double GetFrameMilliseconds(int frameRateCode)
            {
                switch (frameRateCode)
                {
                    case 1: return 1001.0 / 24;
                    case 2: return 1000.0 / 24;
                    case 3: return 1000.0 / 25;
                    case 5: return 1000.0 / 30;
                    case 6: return 1000.0 / 50;
                    case 7: return 1001.0 / 60;
                    case 8: return 1000.0 / 60;
                    default: return 1001.0 / 30;
                }
            }
        }
    }
}
