using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    /// <summary>
    /// Reads XSUB ("DivX subtitles", stream handler <c>DXSB</c>) out of an .avi/.divx file.
    /// <para>
    /// The RIFF structure is walked so each subtitle packet is read with its exact length from
    /// its own <c>movi</c> chunk - including the OpenDML <c>AVIX</c> segments that files above
    /// 2 GB are split into. Files whose index/headers are damaged (or muxers that write the
    /// packets somewhere unexpected) fall back to scanning the whole file for packet headers.
    /// </para>
    /// </summary>
    public static class XSubParser
    {
        // "[00:00:00.000-00:00:00.000]"
        private const int TimeCodeLength = 27;

        // width, height, left, top, right, bottom, second field offset - all little endian UInt16
        private const int PacketHeaderLength = 14;

        // Four RGB triplets: background, pattern, emphasis 1, emphasis 2
        private const int PaletteLength = 12;

        private const int MinPacketLength = TimeCodeLength + PacketHeaderLength + PaletteLength;

        /// <summary>
        /// Upper bound for the RLE payload of a packet found by the fallback scan, where the
        /// real length is unknown (there is no chunk header to read it from). Well above any
        /// real subtitle bitmap, but small enough that a stray "[00:00:00.000-...]" in the
        /// video data cannot make us read hundreds of megabytes.
        /// </summary>
        private const int MaxScannedPacketLength = 1024 * 1024;

        /// <summary>
        /// Sanity cap for a packet read from its own chunk, where the length is known. Only
        /// guards against a corrupt chunk header claiming an absurd size.
        /// </summary>
        private const int MaxPacketLength = 16 * 1024 * 1024;

        /// <summary>
        /// One XSUB stream of an .avi - a file can carry several (one per language).
        /// </summary>
        public sealed class XSubTrack
        {
            public XSubTrack(int streamNumber, List<XSub> subtitles)
            {
                StreamNumber = streamNumber;
                Subtitles = subtitles;
            }

            /// <summary>
            /// Zero-based AVI stream number, or -1 when the packets were recovered by the
            /// fallback scan and cannot be attributed to a stream.
            /// </summary>
            public int StreamNumber { get; }

            public List<XSub> Subtitles { get; }
        }

        public sealed class AviResult
        {
            public AviResult(List<XSubTrack> tracks, int videoWidth, int videoHeight)
            {
                Tracks = tracks;
                VideoWidth = videoWidth;
                VideoHeight = videoHeight;
            }

            public List<XSubTrack> Tracks { get; }

            /// <summary>Every subtitle of every stream, in stream then time order.</summary>
            public List<XSub> Subtitles
            {
                get
                {
                    var all = new List<XSub>();
                    foreach (var track in Tracks)
                    {
                        all.AddRange(track.Subtitles);
                    }

                    return all;
                }
            }

            /// <summary>Video frame width from the AVI main header, or 0 when unknown.</summary>
            public int VideoWidth { get; }

            /// <summary>Video frame height from the AVI main header, or 0 when unknown.</summary>
            public int VideoHeight { get; }
        }

        /// <summary>
        /// All XSUB subtitle streams in an .avi/.divx file, plus the video frame size the
        /// subtitle coordinates are relative to (0x0 when the file has no readable main header).
        /// </summary>
        public static AviResult ParseAvi(string fileName)
        {
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return ParseAvi(stream);
            }
        }

        public static AviResult ParseAvi(Stream stream)
        {
            var state = new RiffState();
            var walkFailed = false;
            try
            {
                stream.Position = 0;
                ReadElements(stream, 0, stream.Length, state);
            }
            catch (Exception)
            {
                // Damaged/odd RIFF structure - whatever was collected so far is kept and the
                // scan below fills in the rest.
                walkFailed = true;
            }

            var tracks = new List<XSubTrack>();
            foreach (var streamNumber in SortedKeys(state.Subtitles))
            {
                tracks.Add(new XSubTrack(streamNumber, state.Subtitles[streamNumber]));
            }

            // Scanning means reading the whole file, so only do it when the RIFF walk could
            // not answer the question: it broke down, it found no streams at all, or it found
            // a subtitle stream whose packets it could not reach. An ordinary video .avi -
            // headers read fine, no subtitle stream declared - is answered from the headers.
            if (tracks.Count == 0 && (walkFailed || state.StreamCount == 0 || state.SubtitleStreams.Count > 0))
            {
                var scanned = ScanForPackets(stream);
                if (scanned.Count > 0)
                {
                    tracks.Add(new XSubTrack(-1, scanned));
                }
            }

            return new AviResult(tracks, state.VideoWidth, state.VideoHeight);
        }

        /// <summary>
        /// Convenience overload for callers that want every subtitle regardless of stream.
        /// </summary>
        public static List<XSub> ParseAviSubtitles(string fileName)
        {
            return ParseAvi(fileName).Subtitles;
        }

        private static List<int> SortedKeys(Dictionary<int, List<XSub>> subtitles)
        {
            var keys = new List<int>(subtitles.Keys);
            keys.Sort();
            return keys;
        }

        private class RiffState
        {
            public int VideoWidth { get; set; }
            public int VideoHeight { get; set; }

            /// <summary>Number of "strl" lists seen so far - the next one gets this stream number.</summary>
            public int StreamCount { get; set; }

            /// <summary>Stream numbers whose "strh" declares an XSUB/DXSB subtitle stream.</summary>
            public HashSet<int> SubtitleStreams { get; } = new HashSet<int>();

            /// <summary>Parsed packets per stream number.</summary>
            public Dictionary<int, List<XSub>> Subtitles { get; } = new Dictionary<int, List<XSub>>();

            public void Add(int streamNumber, XSub xSub)
            {
                if (!Subtitles.TryGetValue(streamNumber, out var list))
                {
                    list = new List<XSub>();
                    Subtitles.Add(streamNumber, list);
                }

                list.Add(xSub);
            }
        }

        /// <summary>
        /// Walks the chunks/lists between <paramref name="position"/> and <paramref name="end"/>,
        /// recursing into the lists that can hold headers or subtitle data.
        /// </summary>
        private static void ReadElements(Stream stream, long position, long end, RiffState state)
        {
            var header = new byte[8];
            while (position + 8 <= end)
            {
                stream.Position = position;
                if (!ReadExactly(stream, header, 8))
                {
                    return;
                }

                var id = Encoding.ASCII.GetString(header, 0, 4);
                var size = BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(header, 4, 4));
                var dataStart = position + 8;
                var dataEnd = dataStart + size;
                if (dataEnd > end)
                {
                    // Truncated element (happens with partially downloaded/damaged files) - read
                    // what is there instead of giving up on the rest of the file.
                    dataEnd = end;
                }

                if (id == "RIFF" || id == "LIST")
                {
                    var listType = ReadFourCc(stream, dataStart);
                    if (listType == "AVI " || listType == "AVIX" || listType == "hdrl" || listType == "strl" || listType == "movi" || listType == "rec ")
                    {
                        if (listType == "strl")
                        {
                            state.StreamCount++;
                        }

                        ReadElements(stream, dataStart + 4, dataEnd, state);
                    }
                }
                else if (id == "avih")
                {
                    ReadMainHeader(stream, dataStart, dataEnd, state);
                }
                else if (id == "strh")
                {
                    ReadStreamHeader(stream, dataStart, dataEnd, state);
                }
                else if (IsStreamDataChunk(id, out var streamNumber) && state.SubtitleStreams.Contains(streamNumber))
                {
                    var length = (int)Math.Min(dataEnd - dataStart, MaxPacketLength);
                    var data = new byte[length];
                    stream.Position = dataStart;
                    if (ReadExactly(stream, data, length))
                    {
                        var xSub = TryParsePacket(data, length);
                        if (xSub != null)
                        {
                            state.Add(streamNumber, xSub);
                        }
                    }
                }

                // Chunks are padded to a WORD boundary; lists are not (their size includes the padding of their children).
                position = dataEnd + (size & 1);
            }
        }

        /// <summary>
        /// Video frame size from the AVI main header (dwWidth/dwHeight at offset 32/36) - the
        /// coordinate space the subtitle rectangles are in.
        /// </summary>
        private static void ReadMainHeader(Stream stream, long dataStart, long dataEnd, RiffState state)
        {
            var avih = new byte[40];
            if (dataEnd - dataStart < avih.Length)
            {
                return;
            }

            stream.Position = dataStart;
            if (!ReadExactly(stream, avih, avih.Length))
            {
                return;
            }

            state.VideoWidth = (int)BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(avih, 32, 4));
            state.VideoHeight = (int)BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(avih, 36, 4));
        }

        /// <summary>
        /// Remembers the current stream number when its header declares XSUB. The handler is
        /// what identifies it - "DXSB" - since the stream type is usually "vids" (that is what
        /// ffmpeg and DivX itself write); "txts" is accepted too for muxers that use it.
        /// </summary>
        private static void ReadStreamHeader(Stream stream, long dataStart, long dataEnd, RiffState state)
        {
            if (dataEnd - dataStart < 8)
            {
                return;
            }

            var strh = new byte[8];
            stream.Position = dataStart;
            if (!ReadExactly(stream, strh, strh.Length))
            {
                return;
            }

            var streamType = Encoding.ASCII.GetString(strh, 0, 4);
            var handler = Encoding.ASCII.GetString(strh, 4, 4);
            if (handler.Equals("DXSB", StringComparison.OrdinalIgnoreCase) ||
                streamType.Equals("txts", StringComparison.OrdinalIgnoreCase))
            {
                // StreamCount was incremented when the enclosing "strl" list was entered, so the
                // stream this header belongs to is numbered from zero as StreamCount - 1.
                state.SubtitleStreams.Add(state.StreamCount - 1);
            }
        }

        /// <summary>
        /// True for a "movi" data chunk id like "01sb"/"00dc": two digits naming the stream,
        /// then two characters naming the payload type.
        /// </summary>
        private static bool IsStreamDataChunk(string id, out int streamNumber)
        {
            streamNumber = -1;
            if (id.Length != 4 || !IsDigit(id[0]) || !IsDigit(id[1]))
            {
                return false;
            }

            streamNumber = (id[0] - '0') * 10 + (id[1] - '0');
            return true;
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static string ReadFourCc(Stream stream, long position)
        {
            var buffer = new byte[4];
            stream.Position = position;
            return ReadExactly(stream, buffer, 4) ? Encoding.ASCII.GetString(buffer, 0, 4) : string.Empty;
        }

        private static bool ReadExactly(Stream stream, byte[] buffer, int count)
        {
            var read = 0;
            while (read < count)
            {
                var n = stream.Read(buffer, read, count - read);
                if (n <= 0)
                {
                    return false;
                }

                read += n;
            }

            return true;
        }

        /// <summary>
        /// Fallback for files whose RIFF structure does not lead to the subtitle packets:
        /// scan the whole file for packet headers ("[hh:mm:ss.fff-hh:mm:ss.fff]") and treat the
        /// bytes up to the next header (or the end of the file) as the packet payload.
        /// </summary>
        private static List<XSub> ScanForPackets(Stream stream)
        {
            var subtitles = new List<XSub>();
            var positions = FindPacketPositions(stream);
            for (var i = 0; i < positions.Count; i++)
            {
                var start = positions[i];
                var limit = i + 1 < positions.Count ? positions[i + 1] : stream.Length;
                var length = (int)Math.Min(limit - start, MaxScannedPacketLength);
                var data = new byte[length];
                stream.Position = start;
                if (!ReadExactly(stream, data, length))
                {
                    continue;
                }

                var xSub = TryParsePacket(data, length);
                if (xSub != null)
                {
                    subtitles.Add(xSub);
                }
            }

            return subtitles;
        }

        private static List<long> FindPacketPositions(Stream stream)
        {
            var positions = new List<long>();
            var buffer = new byte[64 * 1024];
            var overlap = TimeCodeLength;
            var blockStart = 0L;

            // Blocks overlap so a header split across two reads is still found; this is where
            // the next unscanned byte is, so the overlap cannot report a header twice.
            var nextUnscanned = 0L;

            stream.Position = 0;
            while (blockStart + MinPacketLength <= stream.Length)
            {
                stream.Position = blockStart;
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead < MinPacketLength)
                {
                    break;
                }

                var span = new ReadOnlySpan<byte>(buffer, 0, bytesRead);
                for (var i = 0; i <= bytesRead - TimeCodeLength; i++)
                {
                    var position = blockStart + i;
                    if (position >= nextUnscanned && IsTimeCodeHeader(span.Slice(i, TimeCodeLength)))
                    {
                        positions.Add(position);
                        i += TimeCodeLength - 1;
                        nextUnscanned = position + TimeCodeLength;
                    }
                }

                if (bytesRead <= overlap)
                {
                    break;
                }

                blockStart += bytesRead - overlap;
            }

            return positions;
        }

        /// <summary>
        /// Validates the fixed "[hh:mm:ss.fff-hh:mm:ss.fff]" shape, digits included - checking
        /// only the separators matched often enough inside compressed video data to produce
        /// phantom subtitles.
        /// </summary>
        private static bool IsTimeCodeHeader(ReadOnlySpan<byte> s)
        {
            if (s.Length < TimeCodeLength || s[0] != (byte)'[' || s[26] != (byte)']' || s[13] != (byte)'-')
            {
                return false;
            }

            return IsTimePart(s.Slice(1, 12)) && IsTimePart(s.Slice(14, 12));
        }

        // "hh:mm:ss.fff"
        private static bool IsTimePart(ReadOnlySpan<byte> s)
        {
            for (var i = 0; i < 12; i++)
            {
                var c = s[i];
                var expected = i == 2 || i == 5 ? (byte)':' : i == 8 ? (byte)'.' : (byte)0;
                if (expected != 0)
                {
                    if (c != expected)
                    {
                        return false;
                    }
                }
                else if (c < (byte)'0' || c > (byte)'9')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Turns one raw XSUB packet into an <see cref="XSub"/>, or null when the bytes are not
        /// a usable packet. Everything after the 53-byte header is the RLE payload; the packet
        /// header's last field says where its second (odd lines) field starts within it.
        /// </summary>
        private static XSub TryParsePacket(byte[] data, int length)
        {
            if (length <= MinPacketLength || !IsTimeCodeHeader(new ReadOnlySpan<byte>(data, 0, TimeCodeLength)))
            {
                return null;
            }

            var start = DecodeTimeCode(data, 1);
            var end = DecodeTimeCode(data, 14);

            var meta = new ReadOnlySpan<byte>(data, TimeCodeLength, PacketHeaderLength);
            int width = BinaryPrimitives.ReadUInt16LittleEndian(meta.Slice(0, 2));
            int height = BinaryPrimitives.ReadUInt16LittleEndian(meta.Slice(2, 2));
            int left = BinaryPrimitives.ReadUInt16LittleEndian(meta.Slice(4, 2));
            int top = BinaryPrimitives.ReadUInt16LittleEndian(meta.Slice(6, 2));
            int secondFieldOffset = BinaryPrimitives.ReadUInt16LittleEndian(meta.Slice(12, 2));
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            var colors = new byte[PaletteLength];
            Buffer.BlockCopy(data, TimeCodeLength + PacketHeaderLength, colors, 0, PaletteLength);

            var rle = new byte[length - MinPacketLength];
            Buffer.BlockCopy(data, MinPacketLength, rle, 0, rle.Length);

            return new XSub(start, end, width, height, left, top, colors, rle, secondFieldOffset);
        }

        /// <summary>
        /// "hh:mm:ss.fff" at <paramref name="offset"/> - already validated by
        /// <see cref="IsTimeCodeHeader"/>, so the digits are known to be digits.
        /// </summary>
        private static TimeCode DecodeTimeCode(byte[] data, int offset)
        {
            var hours = TwoDigits(data, offset);
            var minutes = TwoDigits(data, offset + 3);
            var seconds = TwoDigits(data, offset + 6);
            var milliseconds = TwoDigits(data, offset + 9) * 10 + (data[offset + 11] - '0');
            return new TimeCode(hours, minutes, seconds, milliseconds);
        }

        private static int TwoDigits(byte[] data, int offset)
        {
            return (data[offset] - '0') * 10 + (data[offset + 1] - '0');
        }
    }
}
