using Nikse.SubtitleEdit.Core.Cea608;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    /// <summary>
    /// Embeds closed captions in the H.264/H.265 video of an MP4/MOV file as ATSC A/53 SEI
    /// messages (ITU-T T.35 user data, "GA94") - CEA-608 (what ffmpeg, VLC, MediaInfo and
    /// YouTube read as "EIA-608") and CEA-708. Each video sample gets one SEI NAL unit in front of
    /// its first slice, and caption SEI the video already has is removed. Everything else is
    /// copied as it is: the file keeps its box layout, only the video sample sizes and the chunk
    /// offsets of all tracks change. Fragmented MP4 is not supported.
    /// </summary>
    public static class Mp4ClosedCaptionWriter
    {
        private const int MaxMoovSize = 512 * 1024 * 1024;

        /// <summary>
        /// Writes <paramref name="inputFileName"/> with captions to <paramref name="outputFileName"/>.
        /// </summary>
        public static void Write(string inputFileName, string outputFileName, ClosedCaptionBytes captions, Action<double> progress)
        {
            using (var input = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024))
            using (var output = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024))
            {
                Write(input, output, captions, progress);
            }
        }

        /// <param name="input">MP4/MOV file with H.264 or H.265 video (seekable)</param>
        /// <param name="output">The same file with the captions</param>
        /// <param name="captions">CEA-608 fields 1 and 2 and CEA-708 caption data</param>
        /// <param name="progress">Called with 0.0 - 1.0</param>
        /// <exception cref="InvalidDataException">Not an MP4 file, or no H.264/H.265 video</exception>
        /// <exception cref="NotSupportedException">Fragmented MP4</exception>
        public static void Write(Stream input, Stream output, ClosedCaptionBytes captions, Action<double> progress)
        {
            var topBoxes = ReadTopLevelBoxes(input);
            var moovBox = topBoxes.Find(b => b.Type == "moov") ?? throw new InvalidDataException("No 'moov' box found - not an MP4 file.");
            if (topBoxes.Exists(b => b.Type == "moof"))
            {
                throw new NotSupportedException("Fragmented MP4 is not supported.");
            }

            if (moovBox.Size > MaxMoovSize)
            {
                throw new InvalidDataException("The 'moov' box is too large.");
            }

            input.Seek(moovBox.Offset, SeekOrigin.Begin);
            var moovBytes = new byte[moovBox.Size];
            ReadExactly(input, moovBytes, 0, moovBytes.Length);
            var moov = Node.Parse(moovBytes, 0, moovBytes.Length);
            if (moov.Find("mvex") != null)
            {
                throw new NotSupportedException("Fragmented MP4 is not supported.");
            }

            var movieTimeScale = ReadMvhdTimeScale(moov);
            var tracks = new List<TrackTables>();
            foreach (var trak in moov.FindAll("trak"))
            {
                var stbl = trak.Find("mdia")?.Find("minf")?.Find("stbl");
                var chunkOffsetBox = stbl?.Find("stco") ?? stbl?.Find("co64");
                if (chunkOffsetBox != null)
                {
                    tracks.Add(new TrackTables(trak, stbl, chunkOffsetBox));
                }
            }

            var video = tracks.Find(t => t.HandlerType == "vide") ?? throw new InvalidDataException("No video track found.");
            var codec = video.ReadVideoCodec();
            var samples = video.ReadSamples(movieTimeScale);
            CheckSamplesInMdat(samples, topBoxes);

            // pass 1: the new samples' sizes
            var builder = new A53CcDataBuilder(captions);
            var displayOrder = new List<Sample>(samples);
            displayOrder.Sort((a, b) => a.PresentationMs != b.PresentationMs ? a.PresentationMs.CompareTo(b.PresentationMs) : a.Index.CompareTo(b.Index));
            for (var i = 0; i < displayOrder.Count; i++)
            {
                var sample = displayOrder[i];
                var endMs = i + 1 < displayOrder.Count ? displayOrder[i + 1].PresentationMs : sample.PresentationMs + sample.DurationMs;
                sample.CcData = builder.NextFrame(sample.PresentationMs, endMs);
            }

            var storageOrder = new List<Sample>(samples);
            storageOrder.Sort((a, b) => a.Offset.CompareTo(b.Offset));
            var totalBytes = 0.0;
            foreach (var sample in storageOrder)
            {
                totalBytes += sample.Size;
            }

            var buffer = new byte[64 * 1024];
            var done = 0.0;
            foreach (var sample in storageOrder)
            {
                input.Seek(sample.Offset, SeekOrigin.Begin);
                EnsureSize(ref buffer, (int)sample.Size);
                ReadExactly(input, buffer, 0, (int)sample.Size);
                sample.NewSize = RewriteSample(buffer, (int)sample.Size, codec, sample.CcData, null);
                done += sample.Size;
                progress?.Invoke(0.5 * done / Math.Max(1, totalBytes));
            }

            // new layout: the moov tables are rebuilt until the chunk offsets fit their boxes
            // (a moov in front of the media data moves the samples when its size changes - the
            // tables only grow, from a sample size table or from stco to co64, so this settles)
            var layout = new Layout(topBoxes, storageOrder, moovBox);
            byte[] newMoov;
            var attempts = 0;
            do
            {
                newMoov = BuildMoov(moov, tracks, video, samples, layout);
            } while (layout.SetMoovSize(newMoov.Length) && ++attempts < 4);

            // pass 2: copy the file with the new moov and samples
            done = 0;
            var sampleIndex = 0;
            foreach (var box in topBoxes)
            {
                if (box == moovBox)
                {
                    output.Write(newMoov, 0, newMoov.Length);
                    continue;
                }

                if (box.Type != "mdat")
                {
                    CopyRange(input, output, box.Offset, box.Size, ref buffer);
                    continue;
                }

                WriteBoxHeader(output, "mdat", layout.GetNewPayloadSize(box), layout.GetNewHeaderSize(box), box.SizeToEnd);
                var position = box.Offset + box.HeaderSize;
                var end = box.Offset + box.Size;
                while (sampleIndex < storageOrder.Count && storageOrder[sampleIndex].Offset < end)
                {
                    var sample = storageOrder[sampleIndex++];
                    CopyRange(input, output, position, sample.Offset - position, ref buffer);
                    input.Seek(sample.Offset, SeekOrigin.Begin);
                    EnsureSize(ref buffer, (int)sample.Size);
                    ReadExactly(input, buffer, 0, (int)sample.Size);
                    RewriteSample(buffer, (int)sample.Size, codec, sample.CcData, output);
                    position = sample.Offset + sample.Size;
                    done += sample.Size;
                    progress?.Invoke(0.5 + 0.5 * done / Math.Max(1, totalBytes));
                }

                CopyRange(input, output, position, end - position, ref buffer);
            }

            progress?.Invoke(1.0);
        }

        private sealed class TopBox
        {
            public string Type;
            public long Offset;
            public long Size;
            public int HeaderSize;
            public bool SizeToEnd; // size field 0: the box runs to the end of the file
        }

        private static List<TopBox> ReadTopLevelBoxes(Stream input)
        {
            var boxes = new List<TopBox>();
            var length = input.Length;
            var header = new byte[16];
            long position = 0;
            while (position + 8 <= length)
            {
                input.Seek(position, SeekOrigin.Begin);
                ReadExactly(input, header, 0, 8);
                long size = BinaryPrimitives.ReadUInt32BigEndian(header);
                var type = Encoding.ASCII.GetString(header, 4, 4);
                var headerSize = 8;
                var sizeToEnd = false;
                if (size == 1)
                {
                    ReadExactly(input, header, 8, 8);
                    size = (long)BinaryPrimitives.ReadUInt64BigEndian(header.AsSpan(8));
                    headerSize = 16;
                }
                else if (size == 0)
                {
                    size = length - position;
                    sizeToEnd = true;
                }

                if (size < headerSize || position + size > length)
                {
                    if (boxes.Count > 0 && position + 8 <= length && type == "mdat")
                    {
                        size = length - position; // truncated mdat: keep what is there
                    }
                    else
                    {
                        throw new InvalidDataException($"Invalid MP4 box '{type}' at offset {position}.");
                    }
                }

                boxes.Add(new TopBox { Type = type, Offset = position, Size = size, HeaderSize = headerSize, SizeToEnd = sizeToEnd });
                position += size;
            }

            if (position < length)
            {
                // trailing bytes that are not a box - copied as they are
                boxes.Add(new TopBox { Type = "\0\0\0\0", Offset = position, Size = length - position });
            }

            return boxes;
        }

        private static void CheckSamplesInMdat(List<Sample> samples, List<TopBox> topBoxes)
        {
            var sorted = new List<Sample>(samples);
            sorted.Sort((a, b) => a.Offset.CompareTo(b.Offset));
            long previousEnd = 0;
            var boxIndex = 0;
            foreach (var sample in sorted)
            {
                if (sample.Offset < previousEnd)
                {
                    throw new InvalidDataException("Video samples overlap.");
                }

                while (boxIndex < topBoxes.Count && topBoxes[boxIndex].Offset + topBoxes[boxIndex].Size <= sample.Offset)
                {
                    boxIndex++;
                }

                var box = boxIndex < topBoxes.Count ? topBoxes[boxIndex] : null;
                if (box == null || box.Type != "mdat" || sample.Offset < box.Offset + box.HeaderSize || sample.Offset + sample.Size > box.Offset + box.Size)
                {
                    throw new InvalidDataException("A video sample is outside the 'mdat' box.");
                }

                previousEnd = sample.Offset + sample.Size;
            }
        }

        private sealed class Sample
        {
            public int Index;
            public long Offset;
            public long Size;
            public double PresentationMs;
            public double DurationMs;
            public byte[] CcData;
            public long NewSize;
        }

        private enum VideoCodec
        {
            H264,
            H265,
        }

        private sealed class CodecInfo
        {
            public VideoCodec Codec;
            public int NalLengthSize;
        }

        /// <summary>
        /// Where every top-level box goes in the output, and the output offset of any input offset
        /// in it - the video samples before it change size.
        /// </summary>
        private sealed class Layout
        {
            private readonly List<TopBox> _boxes;
            private readonly long[] _sampleOffsets;
            private readonly long[] _deltaBefore; // sum of the size changes of the samples before index i
            private readonly Dictionary<TopBox, long> _newStart = new Dictionary<TopBox, long>();
            private readonly Dictionary<TopBox, int> _newHeaderSize = new Dictionary<TopBox, int>();
            private readonly TopBox _moov;
            private long _moovSize;

            public Layout(List<TopBox> boxes, List<Sample> storageOrder, TopBox moov)
            {
                _boxes = boxes;
                _moov = moov;
                _moovSize = moov.Size;
                _sampleOffsets = new long[storageOrder.Count];
                _deltaBefore = new long[storageOrder.Count + 1];
                for (var i = 0; i < storageOrder.Count; i++)
                {
                    _sampleOffsets[i] = storageOrder[i].Offset;
                    _deltaBefore[i + 1] = _deltaBefore[i] + storageOrder[i].NewSize - storageOrder[i].Size;
                }

                Compute();
            }

            /// <returns>True when the moov size changed - the offsets in it must be built again</returns>
            public bool SetMoovSize(long moovSize)
            {
                if (moovSize == _moovSize)
                {
                    return false;
                }

                _moovSize = moovSize;
                Compute();
                return true;
            }

            private void Compute()
            {
                long position = 0;
                foreach (var box in _boxes)
                {
                    _newStart[box] = position;
                    long size;
                    if (box == _moov)
                    {
                        size = _moovSize;
                    }
                    else if (box.Type == "mdat")
                    {
                        var payload = GetNewPayloadSize(box);
                        var headerSize = box.HeaderSize == 16 || payload + 8 > uint.MaxValue ? 16 : 8;
                        _newHeaderSize[box] = headerSize;
                        size = headerSize + payload;
                    }
                    else
                    {
                        size = box.Size;
                    }

                    position += size;
                }
            }

            public long GetNewPayloadSize(TopBox mdat)
            {
                var payloadStart = mdat.Offset + mdat.HeaderSize;
                var end = mdat.Offset + mdat.Size;
                return end - payloadStart + DeltaBefore(end) - DeltaBefore(payloadStart);
            }

            public int GetNewHeaderSize(TopBox mdat) => _newHeaderSize[mdat];

            /// <summary>
            /// Output offset of the input offset <paramref name="offset"/> - a sample start or
            /// another chunk start in an mdat box.
            /// </summary>
            public long Map(long offset)
            {
                foreach (var box in _boxes)
                {
                    if (offset >= box.Offset && offset < box.Offset + box.Size)
                    {
                        var headerSize = box.Type == "mdat" ? _newHeaderSize[box] : box.HeaderSize;
                        var payloadStart = box.Offset + box.HeaderSize;
                        return _newStart[box] + headerSize + (offset - payloadStart) + DeltaBefore(offset) - DeltaBefore(payloadStart);
                    }
                }

                return offset; // a broken offset stays broken
            }

            /// <summary>
            /// Size change of the samples that start before <paramref name="offset"/>.
            /// </summary>
            private long DeltaBefore(long offset)
            {
                var index = Array.BinarySearch(_sampleOffsets, offset);
                if (index < 0)
                {
                    index = ~index;
                }

                return _deltaBefore[index];
            }
        }

        /// <summary>
        /// The tables of one track that matter here.
        /// </summary>
        private sealed class TrackTables
        {
            public TrackTables(Node trak, Node stbl, Node chunkOffsetBox)
            {
                Trak = trak;
                Stbl = stbl;
                ChunkOffsetBox = chunkOffsetBox;
                var hdlr = trak.Find("mdia")?.Find("hdlr");
                HandlerType = hdlr != null && hdlr.Payload.Length >= 12 ? Encoding.ASCII.GetString(hdlr.Payload, 8, 4) : string.Empty;
                ChunkOffsets = ReadChunkOffsets(chunkOffsetBox);
            }

            public Node Trak { get; }
            public Node Stbl { get; }
            public Node ChunkOffsetBox { get; }
            public string HandlerType { get; }
            public long[] ChunkOffsets { get; }

            private static long[] ReadChunkOffsets(Node box)
            {
                var p = box.Payload;
                var count = (int)Math.Min(BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(4)), int.MaxValue);
                var entrySize = box.Type == "co64" ? 8 : 4;
                count = Math.Min(count, (p.Length - 8) / entrySize);
                var offsets = new long[count];
                for (var i = 0; i < count; i++)
                {
                    offsets[i] = entrySize == 8
                        ? (long)BinaryPrimitives.ReadUInt64BigEndian(p.AsSpan(8 + i * 8))
                        : BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(8 + i * 4));
                }

                return offsets;
            }

            public CodecInfo ReadVideoCodec()
            {
                var stsd = Stbl.Find("stsd") ?? throw new InvalidDataException("No 'stsd' box in the video track.");
                var p = stsd.Payload;
                if (p.Length < 16)
                {
                    throw new InvalidDataException("Invalid 'stsd' box.");
                }

                var entrySize = (int)Math.Min(BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(8)), (uint)(p.Length - 8));
                var entryType = Encoding.ASCII.GetString(p, 12, 4);
                VideoCodec codec;
                string configType;
                switch (entryType)
                {
                    case "avc1":
                    case "avc3":
                        codec = VideoCodec.H264;
                        configType = "avcC";
                        break;
                    case "hvc1":
                    case "hev1":
                        codec = VideoCodec.H265;
                        configType = "hvcC";
                        break;
                    default:
                        throw new InvalidDataException($"Closed captions in an MP4 file need H.264 or H.265 video, and the video is '{entryType.Trim()}'.");
                }

                // VisualSampleEntry: 8 byte box header + 78 bytes, then the child boxes
                var nalLengthSize = 4;
                var entry = Node.Parse(p, 8, entrySize, isSampleEntry: true);
                var config = entry.Find(configType);
                if (config != null)
                {
                    var c = config.Payload;
                    if (codec == VideoCodec.H264 && c.Length >= 5)
                    {
                        nalLengthSize = (c[4] & 0x03) + 1;
                    }
                    else if (codec == VideoCodec.H265 && c.Length >= 22)
                    {
                        nalLengthSize = (c[21] & 0x03) + 1;
                    }
                }

                if (nalLengthSize == 3)
                {
                    throw new InvalidDataException("Invalid NAL unit length size.");
                }

                return new CodecInfo { Codec = codec, NalLengthSize = nalLengthSize };
            }

            public List<Sample> ReadSamples(ulong movieTimeScale)
            {
                var mdhd = Trak.Find("mdia")?.Find("mdhd");
                ulong timeScale = 0;
                if (mdhd != null && mdhd.Payload.Length >= 24)
                {
                    timeScale = mdhd.Payload[0] == 1
                        ? BinaryPrimitives.ReadUInt32BigEndian(mdhd.Payload.AsSpan(20))
                        : BinaryPrimitives.ReadUInt32BigEndian(mdhd.Payload.AsSpan(12));
                }

                if (timeScale == 0)
                {
                    timeScale = movieTimeScale;
                }

                var sizes = ReadSampleSizes();
                var durations = ExpandRuns(Stbl.Find("stts"), sizes.Length, signed: false);
                var compositionOffsets = ExpandRuns(Stbl.Find("ctts"), sizes.Length, signed: true);
                var editListOffsetMs = GetEditListOffsetMs(movieTimeScale, timeScale);

                var samples = new List<Sample>(sizes.Length);
                var stsc = ReadStsc();
                var stscIndex = 0;
                long decodeTime = 0;
                for (var chunk = 0; chunk < ChunkOffsets.Length && samples.Count < sizes.Length; chunk++)
                {
                    while (stscIndex + 1 < stsc.Count && stsc[stscIndex + 1].FirstChunk <= chunk + 1)
                    {
                        stscIndex++;
                    }

                    var samplesPerChunk = stsc.Count > 0 ? stsc[stscIndex].SamplesPerChunk : 1;
                    var offset = ChunkOffsets[chunk];
                    for (var i = 0; i < samplesPerChunk && samples.Count < sizes.Length; i++)
                    {
                        var index = samples.Count;
                        var pts = decodeTime + compositionOffsets[index];
                        samples.Add(new Sample
                        {
                            Index = index,
                            Offset = offset,
                            Size = sizes[index],
                            PresentationMs = pts * 1000.0 / timeScale + editListOffsetMs,
                            DurationMs = durations[index] * 1000.0 / timeScale,
                        });
                        offset += sizes[index];
                        decodeTime += durations[index];
                    }
                }

                if (samples.Count < sizes.Length)
                {
                    throw new InvalidDataException("The video sample table is incomplete.");
                }

                return samples;
            }

            private long[] ReadSampleSizes()
            {
                var stsz = Stbl.Find("stsz");
                if (stsz != null && stsz.Payload.Length >= 12)
                {
                    var p = stsz.Payload;
                    var sampleSize = BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(4));
                    var count = (int)Math.Min(BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(8)), int.MaxValue);
                    if (sampleSize == 0)
                    {
                        count = Math.Min(count, (p.Length - 12) / 4);
                    }

                    var sizes = new long[count];
                    for (var i = 0; i < count; i++)
                    {
                        sizes[i] = sampleSize != 0 ? sampleSize : BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(12 + i * 4));
                    }

                    return sizes;
                }

                var stz2 = Stbl.Find("stz2");
                if (stz2 != null && stz2.Payload.Length >= 12)
                {
                    var p = stz2.Payload;
                    var fieldSize = p[7];
                    var count = (int)Math.Min(BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(8)), int.MaxValue);
                    count = Math.Min(count, (int)Math.Min(int.MaxValue, (p.Length - 12L) * 8 / Math.Max(1, (int)fieldSize)));
                    var sizes = new long[count];
                    for (var i = 0; i < count; i++)
                    {
                        switch (fieldSize)
                        {
                            case 4:
                                var b = p[12 + i / 2];
                                sizes[i] = i % 2 == 0 ? b >> 4 : b & 0x0F;
                                break;
                            case 8:
                                sizes[i] = p[12 + i];
                                break;
                            case 16:
                                sizes[i] = BinaryPrimitives.ReadUInt16BigEndian(p.AsSpan(12 + i * 2));
                                break;
                            default:
                                throw new InvalidDataException("Invalid 'stz2' field size.");
                        }
                    }

                    return sizes;
                }

                throw new InvalidDataException("No sample size table in the video track.");
            }

            /// <summary>
            /// The per-sample values of an stts/ctts run table, zero for samples it does not cover.
            /// </summary>
            private static long[] ExpandRuns(Node box, int sampleCount, bool signed)
            {
                var values = new long[sampleCount];
                if (box == null || box.Payload.Length < 8)
                {
                    return values;
                }

                var p = box.Payload;
                var entries = BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(4));
                var index = 0;
                for (var e = 0L; e < entries && 8 + e * 8 + 8 <= p.Length && index < sampleCount; e++)
                {
                    var count = BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan((int)(8 + e * 8)));
                    long value = signed
                        ? (long)BinaryPrimitives.ReadInt32BigEndian(p.AsSpan((int)(12 + e * 8)))
                        : BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan((int)(12 + e * 8)));
                    for (var i = 0L; i < count && index < sampleCount; i++)
                    {
                        values[index++] = value;
                    }
                }

                return values;
            }

            private List<(uint FirstChunk, uint SamplesPerChunk)> ReadStsc()
            {
                var result = new List<(uint FirstChunk, uint SamplesPerChunk)>();
                var stsc = Stbl.Find("stsc");
                if (stsc == null || stsc.Payload.Length < 8)
                {
                    return result;
                }

                var p = stsc.Payload;
                var entries = BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(4));
                for (var e = 0L; e < entries && 8 + e * 12 + 12 <= p.Length; e++)
                {
                    result.Add((BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan((int)(8 + e * 12))), BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan((int)(12 + e * 12)))));
                }

                return result;
            }

            /// <summary>
            /// Offset the edit list puts between the media timeline and the presentation timeline -
            /// the same leading edits <see cref="MP4Parser"/> honours when it reads the captions.
            /// </summary>
            private double GetEditListOffsetMs(ulong movieTimeScale, ulong mediaTimeScale)
            {
                var elst = Trak.Find("edts")?.Find("elst");
                if (elst == null || elst.Payload.Length < 8)
                {
                    return 0;
                }

                var p = elst.Payload;
                var version = p[0];
                var entrySize = version == 1 ? 20 : 12;
                var entries = BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(4));
                var offsetMs = 0.0;
                for (var e = 0L; e < entries && 8 + e * entrySize + entrySize <= p.Length; e++)
                {
                    var at = (int)(8 + e * entrySize);
                    var segmentDuration = version == 1 ? BinaryPrimitives.ReadUInt64BigEndian(p.AsSpan(at)) : BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(at));
                    var mediaTime = version == 1 ? BinaryPrimitives.ReadInt64BigEndian(p.AsSpan(at + 8)) : BinaryPrimitives.ReadInt32BigEndian(p.AsSpan(at + 4));
                    if (mediaTime < 0)
                    {
                        offsetMs += segmentDuration / (double)movieTimeScale * 1000.0;
                        continue;
                    }

                    offsetMs -= mediaTime / (double)mediaTimeScale * 1000.0;
                    break;
                }

                return offsetMs;
            }
        }

        private static ulong ReadMvhdTimeScale(Node moov)
        {
            var mvhd = moov.Find("mvhd");
            if (mvhd != null && mvhd.Payload.Length >= 24)
            {
                var timeScale = mvhd.Payload[0] == 1
                    ? BinaryPrimitives.ReadUInt32BigEndian(mvhd.Payload.AsSpan(20))
                    : BinaryPrimitives.ReadUInt32BigEndian(mvhd.Payload.AsSpan(12));
                if (timeScale > 0)
                {
                    return timeScale;
                }
            }

            return 1000;
        }

        /// <summary>
        /// The moov box with new chunk offsets for every track and new sample sizes for the video.
        /// A track whose offsets no longer fit in 32 bits gets a "co64" box instead of "stco".
        /// </summary>
        private static byte[] BuildMoov(Node moov, List<TrackTables> tracks, TrackTables video, List<Sample> samples, Layout layout)
        {
            foreach (var track in tracks)
            {
                var offsets = new long[track.ChunkOffsets.Length];
                var needs64 = track.ChunkOffsetBox.Type == "co64";
                for (var i = 0; i < offsets.Length; i++)
                {
                    offsets[i] = layout.Map(track.ChunkOffsets[i]);
                    needs64 |= offsets[i] > uint.MaxValue;
                }

                var entrySize = needs64 ? 8 : 4;
                var payload = new byte[8 + offsets.Length * entrySize];
                BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4), (uint)offsets.Length);
                for (var i = 0; i < offsets.Length; i++)
                {
                    if (needs64)
                    {
                        BinaryPrimitives.WriteUInt64BigEndian(payload.AsSpan(8 + i * 8), (ulong)offsets[i]);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8 + i * 4), (uint)offsets[i]);
                    }
                }

                track.ChunkOffsetBox.Type = needs64 ? "co64" : "stco";
                track.ChunkOffsetBox.Payload = payload;
            }

            // video sample sizes: always a full "stsz" table
            var stsz = new byte[12 + samples.Count * 4];
            BinaryPrimitives.WriteUInt32BigEndian(stsz.AsSpan(8), (uint)samples.Count);
            foreach (var sample in samples)
            {
                BinaryPrimitives.WriteUInt32BigEndian(stsz.AsSpan(12 + sample.Index * 4), (uint)sample.NewSize);
            }

            var sizeBox = video.Stbl.Find("stsz") ?? video.Stbl.Find("stz2");
            sizeBox.Type = "stsz";
            sizeBox.Payload = stsz;

            using (var ms = new MemoryStream())
            {
                moov.WriteTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Writes the sample (length-prefixed NAL units) with the caption SEI in front of its
        /// first slice - after an access unit delimiter, parameter sets and other SEI - and without
        /// caption SEI it had.
        /// </summary>
        /// <returns>The size of the new sample</returns>
        private static long RewriteSample(byte[] sample, int length, CodecInfo codec, byte[] ccData, Stream output)
        {
            var lengthSize = codec.NalLengthSize;
            var headerSize = codec.Codec == VideoCodec.H265 ? 2 : 1;
            byte[] sei = null;
            long newSize = 0;
            var inserted = false;
            var position = 0;
            while (position + lengthSize + headerSize <= length)
            {
                var nalSize = ReadNalLength(sample, position, lengthSize);
                if (nalSize < headerSize || position + lengthSize + nalSize > length)
                {
                    break; // not NAL units - the rest is copied as it is
                }

                var nal = position + lengthSize;
                if (!inserted && IsVcl(sample, nal, codec.Codec))
                {
                    sei = MakeSeiNal(ccData, codec, sample, nal);
                    newSize += Write(output, sei, 0, sei.Length);
                    inserted = true;
                }

                if (!IsCaptionOnlySei(sample, nal, (int)nalSize, codec.Codec))
                {
                    newSize += Write(output, sample, position, lengthSize + (int)nalSize);
                }

                position += lengthSize + (int)nalSize;
            }

            if (!inserted)
            {
                // no slice found: the captions go after the NAL units that were found
                sei = MakeSeiNal(ccData, codec, null, 0);
                newSize += Write(output, sei, 0, sei.Length);
            }

            newSize += Write(output, sample, position, length - position);
            return newSize;
        }

        private static long Write(Stream output, byte[] data, int offset, int count)
        {
            if (count > 0)
            {
                output?.Write(data, offset, count);
            }

            return Math.Max(0, count);
        }

        private static long ReadNalLength(byte[] data, int position, int lengthSize)
        {
            switch (lengthSize)
            {
                case 1: return data[position];
                case 2: return BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(position));
                default: return BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(position));
            }
        }

        private static bool IsVcl(byte[] data, int nal, VideoCodec codec)
        {
            if (codec == VideoCodec.H265)
            {
                return ((data[nal] >> 1) & 0x3F) < 32;
            }

            var type = data[nal] & 0x1F;
            return type >= 1 && type <= 5;
        }

        /// <summary>
        /// True for an SEI NAL unit whose messages are all A/53 caption data - the captions
        /// being replaced. An SEI with other messages too is kept.
        /// </summary>
        private static bool IsCaptionOnlySei(byte[] data, int nal, int nalSize, VideoCodec codec)
        {
            var headerSize = codec == VideoCodec.H265 ? 2 : 1;
            var isSei = codec == VideoCodec.H265
                ? ((data[nal] >> 1) & 0x3F) == 39 || ((data[nal] >> 1) & 0x3F) == 40
                : (data[nal] & 0x1F) == 6;
            if (!isSei || nalSize <= headerSize)
            {
                return false;
            }

            var rbsp = Unescape(data, nal + headerSize, nalSize - headerSize);
            var i = 0;
            var found = false;
            while (i < rbsp.Count && !(rbsp[i] == 0x80 && i == rbsp.Count - 1))
            {
                var payloadType = 0;
                while (i < rbsp.Count && rbsp[i] == 0xFF)
                {
                    payloadType += 255;
                    i++;
                }

                if (i >= rbsp.Count)
                {
                    return false;
                }

                payloadType += rbsp[i++];
                var payloadSize = 0;
                while (i < rbsp.Count && rbsp[i] == 0xFF)
                {
                    payloadSize += 255;
                    i++;
                }

                if (i >= rbsp.Count)
                {
                    return false;
                }

                payloadSize += rbsp[i++];
                if (i + payloadSize > rbsp.Count)
                {
                    return false;
                }

                var isCaption = payloadType == 4 && payloadSize >= 8 &&
                                rbsp[i] == 0xB5 && rbsp[i + 1] == 0x00 && rbsp[i + 2] == 0x31 &&
                                rbsp[i + 3] == 'G' && rbsp[i + 4] == 'A' && rbsp[i + 5] == '9' && rbsp[i + 6] == '4' && rbsp[i + 7] == 0x03;
                if (!isCaption)
                {
                    return false;
                }

                found = true;
                i += payloadSize;
            }

            return found;
        }

        private static List<byte> Unescape(byte[] data, int offset, int count)
        {
            var result = new List<byte>(count);
            var zeros = 0;
            for (var i = offset; i < offset + count; i++)
            {
                var b = data[i];
                if (zeros >= 2 && b == 0x03)
                {
                    zeros = 0;
                    continue;
                }

                result.Add(b);
                zeros = b == 0 ? zeros + 1 : 0;
            }

            return result;
        }

        /// <summary>
        /// A length-prefixed SEI NAL unit with one user_data_registered_itu_t_t35 message: country
        /// code 0xB5 (USA), provider code 0x0031 (ATSC) and the A/53 caption data. An H.265 prefix
        /// SEI takes the TemporalId of the slice it goes in front of.
        /// </summary>
        private static byte[] MakeSeiNal(byte[] ccData, CodecInfo codec, byte[] sample, int sliceNal)
        {
            var rbsp = new List<byte>(ccData.Length + 8)
            {
                4, // payloadType: user_data_registered_itu_t_t35
            };
            var payloadSize = 3 + ccData.Length;
            while (payloadSize >= 255)
            {
                rbsp.Add(0xFF);
                payloadSize -= 255;
            }

            rbsp.Add((byte)payloadSize);
            rbsp.Add(0xB5);
            rbsp.Add(0x00);
            rbsp.Add(0x31);
            rbsp.AddRange(ccData);
            rbsp.Add(0x80); // rbsp_trailing_bits

            var nal = new List<byte>(rbsp.Count + 8);
            if (codec.Codec == VideoCodec.H265)
            {
                var temporalIdPlus1 = sample != null ? sample[sliceNal + 1] & 0x07 : 1;
                nal.Add(39 << 1); // PREFIX_SEI_NUT, nuh_layer_id 0
                nal.Add((byte)Math.Max(1, temporalIdPlus1));
            }
            else
            {
                nal.Add(0x06); // nal_ref_idc 0, SEI
            }

            // emulation prevention: no 0x000000 - 0x000003 in the NAL unit payload
            var zeros = 0;
            foreach (var b in rbsp)
            {
                if (zeros >= 2 && b <= 0x03)
                {
                    nal.Add(0x03);
                    zeros = 0;
                }

                nal.Add(b);
                zeros = b == 0 ? zeros + 1 : 0;
            }

            var lengthSize = codec.NalLengthSize;
            var result = new byte[lengthSize + nal.Count];
            switch (lengthSize)
            {
                case 1:
                    result[0] = (byte)nal.Count;
                    break;
                case 2:
                    BinaryPrimitives.WriteUInt16BigEndian(result, (ushort)nal.Count);
                    break;
                default:
                    BinaryPrimitives.WriteUInt32BigEndian(result, (uint)nal.Count);
                    break;
            }

            nal.CopyTo(result, lengthSize);
            return result;
        }

        private static void WriteBoxHeader(Stream output, string type, long payloadSize, int headerSize, bool sizeToEnd)
        {
            var header = new byte[headerSize];
            if (headerSize == 16)
            {
                BinaryPrimitives.WriteUInt32BigEndian(header, 1);
                BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(8), (ulong)(payloadSize + 16));
            }
            else
            {
                BinaryPrimitives.WriteUInt32BigEndian(header, sizeToEnd ? 0 : (uint)(payloadSize + 8));
            }

            Encoding.ASCII.GetBytes(type, 0, 4, header, 4);
            output.Write(header, 0, header.Length);
        }

        private static void CopyRange(Stream input, Stream output, long offset, long count, ref byte[] buffer)
        {
            if (count <= 0)
            {
                return;
            }

            EnsureSize(ref buffer, 1024 * 1024);
            input.Seek(offset, SeekOrigin.Begin);
            while (count > 0)
            {
                var n = (int)Math.Min(buffer.Length, count);
                ReadExactly(input, buffer, 0, n);
                output.Write(buffer, 0, n);
                count -= n;
            }
        }

        private static void EnsureSize(ref byte[] buffer, int size)
        {
            if (buffer.Length < size)
            {
                buffer = new byte[Math.Max(size, buffer.Length * 2)];
            }
        }

        private static void ReadExactly(Stream input, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                var n = input.Read(buffer, offset, count);
                if (n <= 0)
                {
                    throw new EndOfStreamException();
                }

                offset += n;
                count -= n;
            }
        }

        /// <summary>
        /// A box of the moov tree: the containers on the way to the sample tables are parsed, any
        /// other box is kept as its payload bytes.
        /// </summary>
        private sealed class Node
        {
            private static readonly HashSet<string> Containers = new HashSet<string> { "moov", "trak", "mdia", "minf", "stbl", "edts" };

            public string Type;
            public byte[] Payload; // leaf boxes
            public byte[] EntryHead; // sample entry: the 78 bytes before its child boxes
            public List<Node> Children;

            public static Node Parse(byte[] data, int offset, int length, bool isSampleEntry = false)
            {
                var size = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
                var type = Encoding.ASCII.GetString(data, offset + 4, 4);
                var headerSize = 8;
                if (size == 1)
                {
                    headerSize = 16;
                }

                var node = new Node { Type = type };
                var payloadStart = offset + headerSize;
                var payloadLength = length - headerSize;
                if (isSampleEntry || Containers.Contains(type))
                {
                    var childStart = payloadStart;
                    if (isSampleEntry)
                    {
                        if (payloadLength < 78)
                        {
                            throw new InvalidDataException("Invalid video sample entry.");
                        }

                        node.EntryHead = new byte[78];
                        Array.Copy(data, payloadStart, node.EntryHead, 0, 78);
                        childStart += 78;
                    }

                    node.Children = new List<Node>();
                    var end = payloadStart + payloadLength;
                    var position = childStart;
                    while (position + 8 <= end)
                    {
                        long childSize = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(position));
                        if (childSize == 1 && position + 16 <= end)
                        {
                            childSize = (long)BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(position + 8));
                        }
                        else if (childSize == 0)
                        {
                            childSize = end - position;
                        }

                        if (childSize < 8 || position + childSize > end)
                        {
                            // not a box (e.g. padding): kept as it is
                            node.Children.Add(new Node { Type = null, Payload = Slice(data, position, end - position) });
                            position = end;
                            break;
                        }

                        node.Children.Add(Parse(data, position, (int)childSize));
                        position += (int)childSize;
                    }

                    if (position < end)
                    {
                        node.Children.Add(new Node { Type = null, Payload = Slice(data, position, end - position) });
                    }
                }
                else
                {
                    node.Payload = Slice(data, payloadStart, payloadLength);
                }

                return node;
            }

            private static byte[] Slice(byte[] data, int offset, int count)
            {
                var result = new byte[count];
                Array.Copy(data, offset, result, 0, count);
                return result;
            }

            public Node Find(string type) => Children?.Find(c => c.Type == type);

            public List<Node> FindAll(string type) => Children?.FindAll(c => c.Type == type) ?? new List<Node>();

            public void WriteTo(Stream output)
            {
                if (Type == null)
                {
                    output.Write(Payload, 0, Payload.Length);
                    return;
                }

                byte[] payload;
                if (Children != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        if (EntryHead != null)
                        {
                            ms.Write(EntryHead, 0, EntryHead.Length);
                        }

                        foreach (var child in Children)
                        {
                            child.WriteTo(ms);
                        }

                        payload = ms.ToArray();
                    }
                }
                else
                {
                    payload = Payload;
                }

                var header = new byte[8];
                BinaryPrimitives.WriteUInt32BigEndian(header, (uint)(payload.Length + 8));
                Encoding.ASCII.GetBytes(Type, 0, 4, header, 4);
                output.Write(header, 0, 8);
                output.Write(payload, 0, payload.Length);
            }
        }
    }
}
