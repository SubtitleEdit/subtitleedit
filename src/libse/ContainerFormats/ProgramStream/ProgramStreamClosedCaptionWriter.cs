using Nikse.SubtitleEdit.Core.Cea608;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream
{
    /// <summary>
    /// Embeds closed captions in the MPEG-2 video of an MPEG program stream (.mpg) as ATSC A/53
    /// "GA94" picture user data - CEA-608 (what ffmpeg, VLC, MediaInfo and broadcast decoders read
    /// as "EIA-608") and CEA-708. Only the video packets change: the caption data goes in front of
    /// each picture's first slice, and the packets keep their timestamps, so audio and video are
    /// copied as they are.
    /// </summary>
    public static class ProgramStreamClosedCaptionWriter
    {
        /// <summary>
        /// A/53 allows 600 cc_data triplets per second (CEA-708's 9600 bit/s plus CEA-608):
        /// cc_count 20 at 29.97 fps, 10 at 59.94, 25 at 23.976.
        /// </summary>
        private const double MaxTripletsPerMillisecond = 0.6;

        /// <summary>
        /// Writes <paramref name="inputFileName"/> with captions to <paramref name="outputFileName"/>.
        /// Caption user data the video already has is replaced.
        /// </summary>
        public static void Write(string inputFileName, string outputFileName, ClosedCaptionBytes captions, Action<double> progress)
        {
            using (var input = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024))
            using (var output = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024))
            {
                Write(input, output, captions, progress);
            }
        }

        /// <param name="input">MPEG program stream with MPEG-2 video</param>
        /// <param name="output">The same program stream with the captions</param>
        /// <param name="field1">Byte pairs for field 1 (CC1/CC2)</param>
        /// <param name="field2">Byte pairs for field 2 (CC3/CC4) - may be empty</param>
        /// <param name="progress">Called with 0.0 - 1.0</param>
        /// <exception cref="InvalidDataException">No MPEG-2 video in a program stream</exception>
        public static void Write(Stream input, Stream output, IReadOnlyList<SccBytePairs.TimedPair> field1, IReadOnlyList<SccBytePairs.TimedPair> field2, Action<double> progress)
        {
            var captions = new ClosedCaptionBytes
            {
                Field1 = new List<SccBytePairs.TimedPair>(field1 ?? Array.Empty<SccBytePairs.TimedPair>()),
                Field2 = new List<SccBytePairs.TimedPair>(field2 ?? Array.Empty<SccBytePairs.TimedPair>()),
            };
            Write(input, output, captions, progress);
        }

        /// <param name="input">MPEG program stream with MPEG-2 video</param>
        /// <param name="output">The same program stream with the captions</param>
        /// <param name="captions">CEA-608 fields 1 and 2 and CEA-708 caption data</param>
        /// <param name="progress">Called with 0.0 - 1.0</param>
        /// <exception cref="InvalidDataException">No MPEG-2 video in a program stream</exception>
        public static void Write(Stream input, Stream output, ClosedCaptionBytes captions, Action<double> progress)
        {
            var length = Math.Max(1, input.Length);
            var analyzer = new VideoAnalyzer();
            var videoStreamId = ReadVideo(input, analyzer, p => progress?.Invoke(0.5 * p / length));
            analyzer.Finish();
            if (videoStreamId < 0 || !analyzer.IsMpeg2 || analyzer.Pictures.Count == 0)
            {
                throw new InvalidDataException("No MPEG-2 video found in the program stream - A/53 closed captions need MPEG-2 video.");
            }

            var edits = BuildEdits(analyzer, captions);
            input.Seek(0, SeekOrigin.Begin);
            Rewrite(input, output, videoStreamId, edits, p => progress?.Invoke(0.5 + 0.5 * p / length));
            progress?.Invoke(1.0);
        }

        /// <summary>
        /// Feeds the elementary stream of the first video stream to the analyzer.
        /// </summary>
        /// <returns>The video stream id, or -1 if there is none</returns>
        private static int ReadVideo(Stream input, VideoAnalyzer analyzer, Action<long> progress)
        {
            var reader = new PacketReader(input);
            var videoStreamId = -1;
            long packets = 0;
            while (reader.Next())
            {
                if (IsVideoPacket(reader, ref videoStreamId, out var dataStart))
                {
                    analyzer.Feed(reader.Buffer.AsSpan(6 + dataStart, reader.Length - 6 - dataStart));
                }

                if (++packets % 10000 == 0)
                {
                    progress(input.Position);
                }
            }

            return videoStreamId;
        }

        /// <summary>
        /// True for a PES packet of the video stream with elementary stream data in it (from
        /// <paramref name="dataStart"/>, relative to the 6-byte packet start/length). The first
        /// video stream found is the one used - both passes must pick the same packets.
        /// </summary>
        private static bool IsVideoPacket(PacketReader reader, ref int videoStreamId, out int dataStart)
        {
            dataStart = -1;
            var streamId = reader.StreamId;
            if (streamId < 0xE0 || streamId > 0xEF || reader.Length <= 6 || (videoStreamId >= 0 && videoStreamId != streamId))
            {
                return false;
            }

            videoStreamId = streamId;
            dataStart = ProgramStreamClosedCaptionReader.GetPesDataStart(reader.Buffer.AsSpan(6, reader.Length - 6), out _);
            return dataStart >= 0 && 6 + dataStart <= reader.Length;
        }

        private sealed class Edit
        {
            public long Offset;
            public long DeleteLength;
            public byte[] Insert;
        }

        /// <summary>
        /// One caption user data block per frame, in front of its first slice. Every frame gets the
        /// CEA-608 slots that start while it is shown (one per frame at 29.97 fps, sometimes two at
        /// 23.976, none every other frame at 59.94), then the CEA-708 data that is due, as much as
        /// fits. Frames are filled in display order - decoders put the cc_data in display order, so
        /// that is the order the DTVCC bytes must have.
        /// </summary>
        private static List<Edit> BuildEdits(VideoAnalyzer analyzer, ClosedCaptionBytes captions)
        {
            var pairs1 = ToDictionary(captions.Field1);
            var pairs2 = ToDictionary(captions.Field2);
            var dtvcc = captions.Dtvcc ?? new List<ClosedCaptionBytes.DtvccTriplet>();
            var dtvccIndex = 0;
            var frameMs = analyzer.FrameMilliseconds;

            // display times: frames sorted by display position, each shown for its own number of frame periods
            var frames = analyzer.Pictures.FindAll(p => !p.IsSecondField && p.InsertOffset >= 0);
            var displayOrder = new List<Picture>(frames);
            displayOrder.Sort((a, b) => a.DisplayIndex != b.DisplayIndex ? a.DisplayIndex.CompareTo(b.DisplayIndex) : a.CodedIndex.CompareTo(b.CodedIndex));

            var edits = new List<Edit>(frames.Count + analyzer.Deletions.Count);
            foreach (var deletion in analyzer.Deletions)
            {
                edits.Add(new Edit { Offset = deletion.Start, DeleteLength = deletion.End - deletion.Start });
            }

            double periods = 0;
            foreach (var picture in displayOrder)
            {
                var startMs = periods * frameMs;
                periods += picture.Periods;
                var endMs = periods * frameMs;
                var firstSlot = (long)Math.Ceiling(startMs / SccBytePairs.SlotMilliseconds - 0.001);
                var endSlot = (long)Math.Ceiling(endMs / SccBytePairs.SlotMilliseconds - 0.001);
                var slotCount = (int)Math.Min(15, Math.Max(0, endSlot - firstSlot));

                // CEA-708 data sent by now, up to the frame's share of the A/53 bandwidth
                var maxCcCount = Math.Min(31, Math.Max(2 * slotCount + 1, (int)(MaxTripletsPerMillisecond * (endMs - startMs))));
                var dtvccStart = dtvccIndex;
                while (dtvccIndex < dtvcc.Count && dtvcc[dtvccIndex].Milliseconds < endMs && 2 * slotCount + dtvccIndex - dtvccStart < maxCcCount)
                {
                    dtvccIndex++;
                }

                var userData = MakeUserData(firstSlot, slotCount, pairs1, pairs2, dtvcc, dtvccStart, dtvccIndex - dtvccStart);
                edits.Add(new Edit { Offset = picture.InsertOffset, Insert = userData });
            }

            // deletions before insertions at the same offset - both only touch the picture's user data
            edits.Sort((a, b) => a.Offset != b.Offset ? a.Offset.CompareTo(b.Offset) : (a.Insert == null ? 0 : 1).CompareTo(b.Insert == null ? 0 : 1));
            return edits;
        }

        private static Dictionary<long, SccBytePairs.TimedPair> ToDictionary(IReadOnlyList<SccBytePairs.TimedPair> pairs)
        {
            var result = new Dictionary<long, SccBytePairs.TimedPair>();
            if (pairs != null)
            {
                foreach (var pair in pairs)
                {
                    result[pair.Slot] = pair;
                }
            }

            return result;
        }

        /// <summary>
        /// ATSC A/53 picture user data: user_data_start_code, "GA94", user_data_type_code 3,
        /// process_cc_data_flag + cc_count, em_data, cc_count x (marker/cc_valid/cc_type, two bytes),
        /// marker_bits. CEA-608 comes first, one field 1 and one field 2 pair per slot - a slot with
        /// no byte pair sends the null pair 0x80 0x80. A frame without slot or CEA-708 data gets
        /// padding with cc_valid = 0.
        /// </summary>
        private static byte[] MakeUserData(long firstSlot, int slotCount, Dictionary<long, SccBytePairs.TimedPair> field1, Dictionary<long, SccBytePairs.TimedPair> field2,
            List<ClosedCaptionBytes.DtvccTriplet> dtvcc, int dtvccStart, int dtvccCount)
        {
            var padding = slotCount == 0 && dtvccCount == 0;
            var ccCount = padding ? 2 : 2 * slotCount + dtvccCount;
            var data = new byte[11 + ccCount * 3 + 1];
            data[2] = 1;
            data[3] = 0xB2;
            data[4] = (byte)'G';
            data[5] = (byte)'A';
            data[6] = (byte)'9';
            data[7] = (byte)'4';
            data[8] = 0x03;
            data[9] = (byte)(0xC0 | ccCount); // reserved, process_cc_data_flag, additional_data_flag = 0
            data[10] = 0xFF; // em_data
            var index = 11;
            if (padding)
            {
                AddCcData(data, ref index, 0xF8, 0x80, 0x80);
                AddCcData(data, ref index, 0xF9, 0x80, 0x80);
            }

            for (var slot = firstSlot; slot < firstSlot + slotCount; slot++)
            {
                var hasPair1 = field1.TryGetValue(slot, out var pair1);
                AddCcData(data, ref index, 0xFC, hasPair1 ? pair1.Data1 : (byte)0x80, hasPair1 ? pair1.Data2 : (byte)0x80);
                var hasPair2 = field2.TryGetValue(slot, out var pair2);
                AddCcData(data, ref index, 0xFD, hasPair2 ? pair2.Data1 : (byte)0x80, hasPair2 ? pair2.Data2 : (byte)0x80);
            }

            for (var i = dtvccStart; i < dtvccStart + dtvccCount; i++)
            {
                var triplet = dtvcc[i];
                AddCcData(data, ref index, (byte)(0xFC | triplet.Type), triplet.Data1, triplet.Data2);
            }

            data[index] = 0xFF; // marker_bits
            return data;
        }

        private static void AddCcData(byte[] data, ref int index, byte header, byte data1, byte data2)
        {
            data[index++] = header;
            data[index++] = data1;
            data[index++] = data2;
        }

        /// <summary>
        /// Copies the program stream, applying the edits (by elementary stream offset) to the
        /// video packets. A packet that grows past the 16-bit PES length is split in two.
        /// </summary>
        private static void Rewrite(Stream input, Stream output, int videoStreamId, List<Edit> edits, Action<long> progress)
        {
            var reader = new PacketReader(input);
            var es = new byte[65536 + 4096];
            var editIndex = 0;
            long esPosition = 0;
            long deleteUntil = 0;
            long packets = 0;
            var streamId = videoStreamId;
            while (reader.Next())
            {
                if (!IsVideoPacket(reader, ref streamId, out var dataStart))
                {
                    output.Write(reader.Buffer, 0, reader.Length);
                    continue;
                }

                var esStart = esPosition;
                var esEnd = esStart + reader.Length - 6 - dataStart;
                esPosition = esEnd;
                var esLength = 0;
                var current = esStart;
                while (current < esEnd)
                {
                    if (deleteUntil > current)
                    {
                        current = Math.Min(deleteUntil, esEnd);
                        continue;
                    }

                    if (editIndex < edits.Count && edits[editIndex].Offset <= current)
                    {
                        var edit = edits[editIndex++];
                        if (edit.Insert != null)
                        {
                            Append(ref es, ref esLength, edit.Insert, 0, edit.Insert.Length);
                        }
                        else
                        {
                            deleteUntil = Math.Max(deleteUntil, edit.Offset + edit.DeleteLength);
                        }

                        continue;
                    }

                    var next = editIndex < edits.Count ? Math.Min(edits[editIndex].Offset, esEnd) : esEnd;
                    Append(ref es, ref esLength, reader.Buffer, (int)(6 + dataStart + current - esStart), (int)(next - current));
                    current = next;
                }

                WriteVideoPacket(output, reader.Buffer, dataStart, es, esLength);

                if (++packets % 10000 == 0)
                {
                    progress(input.Position);
                }
            }
        }

        private static void Append(ref byte[] buffer, ref int length, byte[] data, int offset, int count)
        {
            if (length + count > buffer.Length)
            {
                Array.Resize(ref buffer, Math.Max(buffer.Length * 2, length + count));
            }

            Buffer.BlockCopy(data, offset, buffer, length, count);
            length += count;
        }

        /// <summary>
        /// Writes the packet's own start code and PES header with the new data. Data that does
        /// not fit in one packet continues in packets without timestamps.
        /// </summary>
        private static void WriteVideoPacket(Stream output, byte[] packet, int headerLength, byte[] es, int esLength)
        {
            const int maxPacketLength = 65535;
            var isMpeg2 = headerLength >= 3 && (packet[6] & 0xC0) == 0x80;
            var header = packet;
            var headerOffset = 6;
            var position = 0;
            do
            {
                var count = Math.Min(esLength - position, maxPacketLength - headerLength);
                var packetLength = headerLength + count;
                output.WriteByte(0);
                output.WriteByte(0);
                output.WriteByte(1);
                output.WriteByte(packet[3]);
                output.WriteByte((byte)(packetLength >> 8));
                output.WriteByte((byte)packetLength);
                output.Write(header, headerOffset, headerLength);
                output.Write(es, position, count);
                position += count;

                // continuation: no PTS/DTS, no data_alignment_indicator
                header = isMpeg2 ? new[] { (byte)(packet[6] & 0xFB), (byte)0, (byte)0 } : new byte[] { 0x0F };
                headerOffset = 0;
                headerLength = header.Length;
            } while (position < esLength);
        }

        /// <summary>
        /// Reads a program stream one unit at a time: pack header, system header/PES packet,
        /// program end code - or a single byte that is not part of any.
        /// </summary>
        private sealed class PacketReader
        {
            private readonly Stream _stream;
            private readonly byte[] _pushBack = new byte[3];
            private int _pushBackLength;

            public PacketReader(Stream stream)
            {
                _stream = stream;
            }

            public byte[] Buffer { get; } = new byte[6 + 65535];
            public int Length { get; private set; }

            /// <summary>
            /// Stream id of the unit (0xB9 - 0xFF), or -1 for a byte outside of any.
            /// </summary>
            public int StreamId { get; private set; }

            public bool Next()
            {
                var read = Read(0, 4);
                if (read == 0)
                {
                    return false;
                }

                if (read < 4 || Buffer[0] != 0 || Buffer[1] != 0 || Buffer[2] != 1 || Buffer[3] < 0xB9)
                {
                    return Junk(read);
                }

                StreamId = Buffer[3];
                if (StreamId == 0xB9) // program end
                {
                    Length = 4;
                    return true;
                }

                if (StreamId == 0xBA) // pack header
                {
                    if (Read(4, 1) < 1)
                    {
                        return Junk(4);
                    }

                    if ((Buffer[4] & 0xC0) == 0x40) // MPEG-2: 14 bytes + stuffing
                    {
                        if (Read(5, 9) < 9)
                        {
                            return Junk(5);
                        }

                        var stuffing = Buffer[13] & 0x07;
                        Length = 14 + Read(14, stuffing);
                        return true;
                    }

                    Length = 5 + Read(5, 7); // MPEG-1: 12 bytes
                    return true;
                }

                if (Read(4, 2) < 2)
                {
                    return Junk(4);
                }

                var packetLength = (Buffer[4] << 8) | Buffer[5];
                Length = 6 + Read(6, packetLength);
                return true;
            }

            private bool Junk(int read)
            {
                // one byte out, look for a start code from the next one
                StreamId = -1;
                Length = 1;
                _pushBackLength = read - 1;
                Array.Copy(Buffer, 1, _pushBack, 0, _pushBackLength);
                return true;
            }

            private int Read(int offset, int count)
            {
                var total = 0;
                while (total < count && _pushBackLength > 0)
                {
                    Buffer[offset + total++] = _pushBack[0];
                    _pushBackLength--;
                    Array.Copy(_pushBack, 1, _pushBack, 0, _pushBackLength);
                }

                while (total < count)
                {
                    var n = _stream.Read(Buffer, offset + total, count - total);
                    if (n <= 0)
                    {
                        break;
                    }

                    total += n;
                }

                return total;
            }
        }

        private sealed class Picture
        {
            public int CodedIndex;
            public long DisplayIndex;
            public int TemporalReference;
            public long InsertOffset = -1; // elementary stream offset of the first slice
            public double Periods = 1; // frame periods shown (repeat_first_field makes it 1.5, 2 or 3)
            public bool IsSecondField;
        }

        /// <summary>
        /// Finds the pictures of an MPEG-2 video elementary stream fed in pieces: display position
        /// (GOP + temporal_reference), display duration, where its slices start, and the A/53
        /// caption user data it already has.
        /// </summary>
        private sealed class VideoAnalyzer
        {
            private const int HeadLength = 8;

            private long _position; // elementary stream offset of the next byte fed
            private int _zeros;
            private bool _awaitCode;
            private long _startCodeOffset;
            private int _code = -1; // unit whose first bytes are collected, -1 = none
            private long _codeOffset;
            private readonly byte[] _head = new byte[HeadLength];
            private int _headLength;

            private Picture _picture;
            private bool _awaitSlice;
            private Picture _firstField;
            private long? _deletionStart;
            private long _gopBase;
            private long _gopFrames;
            private int _frameRateCode = 4;
            private bool _progressiveSequence;

            public bool IsMpeg2 { get; private set; }
            public List<Picture> Pictures { get; } = new List<Picture>();
            public List<(long Start, long End)> Deletions { get; } = new List<(long Start, long End)>();

            public double FrameMilliseconds
            {
                get
                {
                    switch (_frameRateCode)
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

            public void Feed(ReadOnlySpan<byte> data)
            {
                var i = 0;
                while (i < data.Length)
                {
                    if (_awaitCode || _code >= 0)
                    {
                        Step(data[i++]);
                        continue;
                    }

                    // fast path: skip to the next 0x01 that ends a start code
                    var index = data.Slice(i).IndexOf((byte)1);
                    if (index < 0)
                    {
                        _zeros = CountZeros(data, i, data.Length);
                        _position += data.Length - i;
                        return;
                    }

                    var j = i + index;
                    _zeros = CountZeros(data, i, j);
                    _position += j - i;
                    i = j;
                    Step(data[i++]);
                }
            }

            /// <summary>
            /// Zero bytes (at most 2 matter) before <paramref name="end"/>, counting the ones before
            /// <paramref name="start"/> when all bytes in between are zero.
            /// </summary>
            private int CountZeros(ReadOnlySpan<byte> data, int start, int end)
            {
                var zeros = 0;
                for (var k = end - 1; k >= start && zeros < 2; k--)
                {
                    if (data[k] != 0)
                    {
                        return zeros;
                    }

                    zeros++;
                }

                return Math.Min(2, zeros + (end - start == zeros ? _zeros : 0));
            }

            private void Step(byte b)
            {
                if (_awaitCode)
                {
                    _awaitCode = false;
                    StartCode(b, _startCodeOffset);
                }
                else if (b == 1 && _zeros >= 2)
                {
                    FinishHead();
                    _awaitCode = true;
                    _startCodeOffset = _position - 2;
                }
                else if (_code >= 0)
                {
                    _head[_headLength++] = b;
                    if (_headLength == HeadLength)
                    {
                        FinishHead();
                    }
                }

                _zeros = b == 0 ? Math.Min(2, _zeros + 1) : 0;
                _position++;
            }

            public void Finish()
            {
                FinishHead();
                CloseDeletion(_position);
            }

            private void CloseDeletion(long end)
            {
                if (_deletionStart.HasValue)
                {
                    Deletions.Add((_deletionStart.Value, end));
                    _deletionStart = null;
                }
            }

            private void StartCode(int code, long offset)
            {
                CloseDeletion(offset);
                if (code >= 0x01 && code <= 0xAF)
                {
                    if (_awaitSlice)
                    {
                        _picture.InsertOffset = offset;
                        _awaitSlice = false;
                    }
                }
                else if (code == 0xB8) // GOP: temporal_reference starts over
                {
                    _gopBase += _gopFrames;
                    _gopFrames = 0;
                    _awaitSlice = false;
                }
                else if (code == 0xB3 || code == 0xB7)
                {
                    _awaitSlice = false;
                }

                if (code == 0x00 || code == 0xB2 || code == 0xB3 || code == 0xB5)
                {
                    _code = code;
                    _codeOffset = offset;
                    _headLength = 0;
                }
            }

            private void FinishHead()
            {
                if (_code < 0)
                {
                    return;
                }

                var code = _code;
                _code = -1;
                var head = _head.AsSpan(0, _headLength);
                switch (code)
                {
                    case 0x00 when head.Length >= 2: // picture header
                        _picture = new Picture
                        {
                            CodedIndex = Pictures.Count,
                            TemporalReference = (head[0] << 2) | (head[1] >> 6),
                        };
                        _picture.DisplayIndex = _gopBase + _picture.TemporalReference;
                        Pictures.Add(_picture);
                        _awaitSlice = true;
                        _gopFrames = Math.Max(_gopFrames, _picture.TemporalReference + 1);
                        break;
                    case 0xB3 when head.Length >= 4: // sequence header
                        _frameRateCode = head[3] & 0x0F;
                        break;
                    case 0xB5 when head.Length >= 2 && head[0] >> 4 == 1: // sequence extension
                        IsMpeg2 = true;
                        _progressiveSequence = (head[1] & 0x08) != 0;
                        break;
                    case 0xB5 when head.Length >= 4 && head[0] >> 4 == 8 && _awaitSlice: // picture coding extension
                        PictureCodingExtension(head);
                        break;
                    case 0xB2 when _awaitSlice && head.Length >= 5 && head[0] == 'G' && head[1] == 'A' && head[2] == '9' && head[3] == '4' && head[4] == 0x03:
                        _deletionStart = _codeOffset; // caption data already there - ours replaces it
                        break;
                }
            }

            private void PictureCodingExtension(ReadOnlySpan<byte> head)
            {
                var pictureStructure = head[2] & 0x03;
                var topFieldFirst = (head[3] & 0x80) != 0;
                var repeatFirstField = (head[3] & 0x02) != 0;
                if (pictureStructure != 3)
                {
                    // field pictures: two picture headers with the same temporal_reference make one frame
                    if (_firstField != null && _firstField.TemporalReference == _picture.TemporalReference)
                    {
                        _picture.IsSecondField = true;
                        _firstField = null;
                    }
                    else
                    {
                        _firstField = _picture;
                    }

                    return;
                }

                _firstField = null;
                if (!repeatFirstField)
                {
                    return;
                }

                // progressive_sequence: the frame is shown two or three times, else one field more
                _picture.Periods = _progressiveSequence ? (topFieldFirst ? 3 : 2) : 1.5;
            }
        }
    }
}
