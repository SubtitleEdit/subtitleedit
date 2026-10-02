using SkiaSharp;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream
{
    /// <summary>
    /// A subtitle picture of a PSP UMD Video stream: a palette PNG shown at a position on the
    /// 720x480 video frame.
    /// </summary>
    public class UmdVideoSubtitle
    {
        public const int ScreenWidth = 720;
        public const int ScreenHeight = 480;

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public byte[] Png { get; set; }

        public SKBitmap GetBitmap() => SKBitmap.Decode(Png);
    }

    /// <summary>
    /// Reads the subtitles of PSP UMD Video (.MPS) and PSP movies (.PMF, an MPEG program stream
    /// after a "PSMF" header), and the ".subs" dumps some demuxers write of them.
    ///
    /// The subtitles are private stream 1 sub-streams 0x80-0x9F. A picture is one record split
    /// over as many 2048-byte packs as it needs - the first PES carries the PTS - and the record is:
    /// sub-stream id, 00, the length of the rest (32 bits), then a 16-byte header - four ASCII
    /// digits ("0088"), the duration (32 bits, 90 kHz), 00 10, x and y (16 bits each), 00 00 - and
    /// a PNG. A ".subs" dump is the records back to back, each after the 8 bytes of its PES
    /// header data (PTS first); its writer leaves 10 bytes of each continuation PES header in the
    /// record, which are removed when the PNG does not check out without them.
    /// </summary>
    public static class UmdVideoSubtitleReader
    {
        private const int RecordHeaderLength = 16; // digits, duration, 00 10, x, y, 00 00
        private const int SubsPesHeaderLength = 8; // PTS + the rest of the PES header data

        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public static bool IsSubtitleSubStream(int subStreamId) => subStreamId >= 0x80 && subStreamId <= 0x9F;

        /// <summary>
        /// Subtitle pictures per sub-stream id (0x80 = first subtitle stream). Empty if there are
        /// none - also for a file that is not a UMD Video/PSMF stream or a ".subs" dump.
        /// </summary>
        public static SortedDictionary<int, List<UmdVideoSubtitle>> Read(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return fileName.EndsWith(".subs", StringComparison.OrdinalIgnoreCase)
                    ? ReadSubsDump(stream)
                    : ReadProgramStream(stream);
            }
        }

        /// <summary>
        /// Demuxes the subtitle records from an MPEG program stream, optionally after a PSMF header.
        /// </summary>
        public static SortedDictionary<int, List<UmdVideoSubtitle>> ReadProgramStream(Stream stream)
        {
            var result = new SortedDictionary<int, List<UmdVideoSubtitle>>();
            var start = GetProgramStreamStart(stream);
            if (start < 0)
            {
                return result;
            }

            var pending = new Dictionary<int, (long Pts, List<byte> Data)>();
            var header = new byte[6];
            stream.Position = start;
            while (FindStartCode(stream, header))
            {
                var streamId = header[3];
                if (streamId == 0xBA) // pack header
                {
                    if (!SkipPackHeader(stream))
                    {
                        break;
                    }

                    continue;
                }

                if (streamId < 0xBB)
                {
                    continue; // not a PES - keep looking for a start code
                }

                if (stream.Read(header, 4, 2) < 2)
                {
                    break;
                }

                var length = (header[4] << 8) | header[5];
                if (streamId != 0xBD)
                {
                    stream.Seek(length, SeekOrigin.Current);
                    continue;
                }

                var body = new byte[length];
                if (stream.Read(body, 0, length) < length)
                {
                    break;
                }

                AddPrivateStream1Packet(body, pending, result);
            }

            foreach (var item in pending)
            {
                AddRecord(item.Key, item.Value.Pts, item.Value.Data, result);
            }

            return result;
        }

        private static void AddPrivateStream1Packet(byte[] body, Dictionary<int, (long Pts, List<byte> Data)> pending, SortedDictionary<int, List<UmdVideoSubtitle>> result)
        {
            if (body.Length < 3 || (body[0] & 0xC0) != 0x80) // MPEG-2 PES header
            {
                return;
            }

            var payloadStart = 3 + body[2];
            if (payloadStart + 2 > body.Length || !IsSubtitleSubStream(body[payloadStart]))
            {
                return;
            }

            var subStreamId = body[payloadStart];
            var hasPts = (body[1] & 0x80) != 0 && body[2] >= 5;
            if (hasPts)
            {
                if (pending.TryGetValue(subStreamId, out var previous))
                {
                    AddRecord(subStreamId, previous.Pts, previous.Data, result);
                }

                var data = new List<byte>(body.Length);
                for (var i = payloadStart + 2; i < body.Length; i++)
                {
                    data.Add(body[i]);
                }

                pending[subStreamId] = (ReadPts(body, 3), data);
            }
            else if (pending.TryGetValue(subStreamId, out var current))
            {
                for (var i = payloadStart + 2; i < body.Length; i++)
                {
                    current.Data.Add(body[i]);
                }
            }
        }

        /// <param name="subStreamId">The subtitle stream (0x80 = the first)</param>
        /// <param name="pts">Presentation time stamp of the record (90 kHz)</param>
        /// <param name="data">The record after sub-stream id and 00: length (32 bits), header, PNG</param>
        /// <param name="result">Pictures per sub-stream id</param>
        private static void AddRecord(int subStreamId, long pts, List<byte> data, SortedDictionary<int, List<UmdVideoSubtitle>> result)
        {
            if (data.Count < 4)
            {
                return;
            }

            var length = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
            if (length <= RecordHeaderLength || length > data.Count - 4)
            {
                return;
            }

            var record = new byte[length];
            data.CopyTo(4, record, 0, length);
            var subtitle = MakeSubtitle(record, pts);
            if (subtitle != null)
            {
                GetOrAdd(result, subStreamId).Add(subtitle);
            }
        }

        /// <summary>
        /// Reads a ".subs" dump - the records of one sub-stream, each after 8 bytes of PES header.
        /// </summary>
        public static SortedDictionary<int, List<UmdVideoSubtitle>> ReadSubsDump(Stream stream)
        {
            var result = new SortedDictionary<int, List<UmdVideoSubtitle>>();
            var bytes = new byte[stream.Length];
            stream.Position = 0;
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
            {
                return result;
            }

            var pos = 0;
            while (pos + SubsPesHeaderLength + 6 + RecordHeaderLength <= bytes.Length)
            {
                var subStreamId = bytes[pos + SubsPesHeaderLength];
                if (!IsSubtitleSubStream(subStreamId) || (bytes[pos] & 0xF0) != 0x20)
                {
                    break; // not a dump of UMD subtitles (or lost track of the records)
                }

                var pts = ReadPts(bytes, pos);
                var length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(pos + SubsPesHeaderLength + 2));
                var recordStart = pos + SubsPesHeaderLength + 6;
                if (length <= RecordHeaderLength || length > bytes.Length - recordStart)
                {
                    break;
                }

                var record = bytes.AsSpan(recordStart, length).ToArray();
                var end = recordStart + length;
                if (!IsValidPng(record, RecordHeaderLength))
                {
                    (record, end) = RemoveContinuationHeaders(bytes, recordStart, length, subStreamId);
                }

                var subtitle = MakeSubtitle(record, pts);
                if (subtitle != null)
                {
                    GetOrAdd(result, subStreamId).Add(subtitle);
                }

                pos = end;
            }

            return result;
        }

        /// <summary>
        /// The record with the 10-byte continuation PES header remnants ("00 01 BD len len 80 00 00
        /// sub-stream 00") taken out; returns the record and where the next one starts.
        /// </summary>
        private static (byte[] Record, int End) RemoveContinuationHeaders(byte[] bytes, int recordStart, int length, int subStreamId)
        {
            var record = new byte[length];
            var count = 0;
            var i = recordStart;
            while (count < length && i < bytes.Length)
            {
                if (i + 10 <= bytes.Length && bytes[i + 1] == 0x01 && bytes[i + 2] == 0xBD &&
                    bytes[i + 5] == 0x80 && bytes[i + 6] == 0x00 && bytes[i + 7] == 0x00 &&
                    bytes[i + 8] == subStreamId && bytes[i + 9] == 0x00)
                {
                    i += 10;
                    continue;
                }

                record[count++] = bytes[i++];
            }

            return (record, i);
        }

        private static UmdVideoSubtitle MakeSubtitle(byte[] record, long pts)
        {
            if (record.Length <= RecordHeaderLength || !IsValidPng(record, RecordHeaderLength))
            {
                return null;
            }

            for (var i = 0; i < 4; i++)
            {
                if (record[i] < '0' || record[i] > '9')
                {
                    return null;
                }
            }

            var duration = BinaryPrimitives.ReadUInt32BigEndian(record.AsSpan(4));
            var startMs = pts / 90.0;
            return new UmdVideoSubtitle
            {
                StartTime = TimeSpan.FromMilliseconds(startMs),
                EndTime = TimeSpan.FromMilliseconds(startMs + duration / 90.0),
                X = BinaryPrimitives.ReadUInt16BigEndian(record.AsSpan(10)),
                Y = BinaryPrimitives.ReadUInt16BigEndian(record.AsSpan(12)),
                Png = record.AsSpan(RecordHeaderLength).ToArray(),
            };
        }

        /// <summary>
        /// A PNG signature followed by chunks whose CRCs check out, up to IEND.
        /// </summary>
        private static bool IsValidPng(byte[] data, int offset)
        {
            if (data.Length < offset + PngSignature.Length || !data.AsSpan(offset, PngSignature.Length).SequenceEqual(PngSignature))
            {
                return false;
            }

            var p = offset + PngSignature.Length;
            while (p + 12 <= data.Length)
            {
                var chunkLength = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(p));
                if (chunkLength < 0 || p + 12L + chunkLength > data.Length)
                {
                    return false;
                }

                var crc = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(p + 8 + chunkLength));
                if (crc != Crc32(data, p + 4, chunkLength + 4))
                {
                    return false;
                }

                if (data[p + 4] == 'I' && data[p + 5] == 'E' && data[p + 6] == 'N' && data[p + 7] == 'D')
                {
                    return true;
                }

                p += 12 + chunkLength;
            }

            return false;
        }

        private static uint[] _crcTable;

        private static uint Crc32(byte[] data, int offset, int count)
        {
            if (_crcTable == null)
            {
                var table = new uint[256];
                for (uint n = 0; n < 256; n++)
                {
                    var c = n;
                    for (var k = 0; k < 8; k++)
                    {
                        c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                    }

                    table[n] = c;
                }

                _crcTable = table;
            }

            var crc = 0xFFFFFFFFu;
            for (var i = offset; i < offset + count; i++)
            {
                crc = _crcTable[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }

            return crc ^ 0xFFFFFFFFu;
        }

        /// <summary>
        /// Where the program stream starts: after a PSMF header (.pmf), else at a pack header at
        /// the start of the file. -1 if it is neither.
        /// </summary>
        private static long GetProgramStreamStart(Stream stream)
        {
            var buffer = new byte[16];
            stream.Position = 0;
            if (stream.Read(buffer, 0, buffer.Length) < buffer.Length)
            {
                return -1;
            }

            if (buffer[0] == 'P' && buffer[1] == 'S' && buffer[2] == 'M' && buffer[3] == 'F')
            {
                var offset = BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan(8));
                return offset < stream.Length ? (long)offset : -1;
            }

            return buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 1 && buffer[3] == 0xBA ? 0 : -1;
        }

        /// <summary>
        /// Moves the stream to after the next 00 00 01 xx; header[0..3] gets the start code.
        /// </summary>
        private static bool FindStartCode(Stream stream, byte[] header)
        {
            var zeros = 0;
            int b;
            while ((b = stream.ReadByte()) >= 0)
            {
                if (b == 1 && zeros >= 2)
                {
                    var code = stream.ReadByte();
                    if (code < 0)
                    {
                        return false;
                    }

                    header[0] = 0;
                    header[1] = 0;
                    header[2] = 1;
                    header[3] = (byte)code;
                    return true;
                }

                zeros = b == 0 ? zeros + 1 : 0;
            }

            return false;
        }

        private static bool SkipPackHeader(Stream stream)
        {
            var first = stream.ReadByte();
            if (first < 0)
            {
                return false;
            }

            if ((first & 0xC0) == 0x40) // MPEG-2: 10 bytes, the last with the stuffing length
            {
                var rest = new byte[9];
                if (stream.Read(rest, 0, rest.Length) < rest.Length)
                {
                    return false;
                }

                stream.Seek(rest[8] & 0x07, SeekOrigin.Current);
                return true;
            }

            stream.Seek(7, SeekOrigin.Current); // MPEG-1: 8 bytes
            return true;
        }

        private static long ReadPts(byte[] buffer, int index)
        {
            return ((long)((buffer[index] >> 1) & 0x07) << 30) |
                   ((long)buffer[index + 1] << 22) |
                   ((long)(buffer[index + 2] >> 1) << 15) |
                   ((long)buffer[index + 3] << 7) |
                   ((long)buffer[index + 4] >> 1);
        }

        private static List<UmdVideoSubtitle> GetOrAdd(SortedDictionary<int, List<UmdVideoSubtitle>> result, int subStreamId)
        {
            if (!result.TryGetValue(subStreamId, out var list))
            {
                list = new List<UmdVideoSubtitle>();
                result.Add(subStreamId, list);
            }

            return list;
        }
    }
}
