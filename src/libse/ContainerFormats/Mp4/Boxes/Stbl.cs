using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Stbl : Box
    {
        public Stsd Stsd { get; set; }
        public List<SubPicture> SubPictures;
        public ulong StszSampleCount;
        public ulong TimeScale { get; set; }
        private readonly Mdia _mdia;
        public List<uint> SampleSizes;
        public List<uint> Ssts { get; set; }
        public List<int> Ctts { get; set; }
        public List<SampleToChunkMap> Stsc { get; set; }
        public List<ulong> ChunkOffsets;
        public List<Paragraph> Paragraphs;
        public List<Paragraph> GetParagraphs() => Paragraphs;

        private List<Cea608.CcData> _cea608CcData = new List<Cea608.CcData>();
        private Dictionary<uint, SampleToChunkMap> _stscLookup;

        // Cap for the stts/ctts run-length expansions: a malformed sample count (up to
        // 0xFFFFFFFF) must not expand into an OOM-sized list. Far above any real sample
        // count (5M samples is ~23 hours at 60 fps).
        private const int MaxRunLengthEntries = 5_000_000;

        public Stbl(Stream fs, ulong maximumLength, ulong timeScale, string handlerType, Mdia mdia)
        {
            TimeScale = timeScale;
            _mdia = mdia;
            Position = (ulong)fs.Position;
            Ssts = new List<uint>();
            Ctts = new List<int>();
            Stsc = new List<SampleToChunkMap>();
            SampleSizes = new List<uint>();
            ChunkOffsets = new List<ulong>();
            SubPictures = new List<SubPicture>();
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "stsd") // Sample Description Box
                {
                    Stsd = new Stsd(fs, Position);
                }
                else if (Name == "stco") // 32-bit - chunk offset
                {
                    if (handlerType != "soun")
                    {
                        Buffer = new byte[Size - 4];
                        fs.ReadFully(Buffer, 0, Buffer.Length);
                        int version = Buffer[0];
                        var totalEntries = GetUInt(4);

                        var entries = ClampEntries(totalEntries, 8, 4);
                        ChunkOffsets.Capacity = ChunkOffsets.Count + entries;
                        for (var i = 0; i < entries; i++)
                        {
                            var offset = GetUInt(8 + i * 4);
                            ChunkOffsets.Add(offset);
                        }
                    }
                }
                else if (Name == "co64") // 64-bit
                {
                    if (handlerType != "soun")
                    {
                        Buffer = new byte[Size - 4];
                        fs.ReadFully(Buffer, 0, Buffer.Length);
                        int version = Buffer[0];
                        var totalEntries = GetUInt(4);

                        var entries = ClampEntries(totalEntries, 8, 8);
                        ChunkOffsets.Capacity = ChunkOffsets.Count + entries;
                        for (var i = 0; i < entries; i++)
                        {
                            var offset = GetUInt64(8 + i * 8);
                            ChunkOffsets.Add(offset);
                        }
                    }
                }
                else if (Name == "stsz") // sample sizes
                {
                    Buffer = new byte[Size - 4];
                    fs.ReadFully(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var uniformSizeOfEachSample = GetUInt(4);
                    var numberOfSampleSizes = GetUInt(8);
                    StszSampleCount = numberOfSampleSizes;

                    if (uniformSizeOfEachSample != 0)
                    {
                        // A non-zero sample_size means every sample has that size and no
                        // entry table follows (ISO/IEC 14496-12 8.7.3.2). The table read
                        // below would run off the end of the box, leaving SampleSizes
                        // empty and silently yielding no subtitles for such a track.
                        var count = (int)Math.Min(numberOfSampleSizes, MaxRunLengthEntries);
                        SampleSizes.Capacity = count;
                        for (var i = 0; i < count; i++)
                        {
                            SampleSizes.Add(uniformSizeOfEachSample);
                        }
                    }
                    else
                    {
                        var entries = ClampEntries(numberOfSampleSizes, 15, 4);
                        SampleSizes.Capacity = entries;
                        for (var i = 0; i < entries; i++)
                        {
                            SampleSizes.Add(GetUInt(12 + i * 4));
                        }
                    }
                }
                else if (Name == "stts") // sample table time to sample map
                {
                    //https://developer.apple.com/library/mac/#documentation/QuickTime/QTFF/QTFFChap2/qtff2.html#//apple_ref/doc/uid/TP40000939-CH204-SW1

                    Buffer = new byte[Size - 4];
                    fs.ReadFully(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var numberOfSampleTimes = GetUInt(4);
                    var entries = ClampEntries(numberOfSampleTimes, 15, 8);

                    // Cheap pre-pass over the run lengths: the expansion below can reach
                    // millions of entries, and growing there from nothing means ~20 array
                    // doublings and copies of a multi-MB array.
                    Ssts.Capacity = SumSampleCounts(entries, 8, 8);
                    for (var i = 0; i < entries; i++)
                    {
                        var sampleCount = GetUInt(8 + i * 8);
                        var sampleDelta = GetUInt(12 + i * 8);
                        for (var j = 0; j < sampleCount && Ssts.Count < MaxRunLengthEntries; j++)
                        {
                            Ssts.Add(sampleDelta);
                        }

                        if (Ssts.Count >= MaxRunLengthEntries)
                        {
                            break;
                        }
                    }
                }
                else if (Name == "ctts") // composition time offset (PTS = DTS + offset); needed when B-frames make storage order differ from display order
                {
                    Buffer = new byte[Size - 4];
                    fs.ReadFully(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var numberOfEntries = GetUInt(4);
                    var entries = ClampEntries(numberOfEntries, 15, 8);
                    Ctts.Capacity = SumSampleCounts(entries, 8, 8);
                    for (var i = 0; i < entries; i++)
                    {
                        var sampleCount = GetUInt(8 + i * 8);
                        var offsetRaw = GetUInt(12 + i * 8);
                        var sampleOffset = version == 1 ? unchecked((int)offsetRaw) : (int)offsetRaw;
                        for (var j = 0; j < sampleCount && Ctts.Count < MaxRunLengthEntries; j++)
                        {
                            Ctts.Add(sampleOffset);
                        }

                        if (Ctts.Count >= MaxRunLengthEntries)
                        {
                            break;
                        }
                    }
                }
                else if (Name == "stsc") // sample table sample to chunk map
                {
                    Buffer = new byte[Size - 4];
                    fs.ReadFully(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var numberOfSampleTimes = GetUInt(4);
                    var entries = ClampEntries(numberOfSampleTimes, 20, 12);
                    Stsc.Capacity = entries;
                    for (var i = 0; i < entries; i++)
                    {
                        var firstChunk = GetUInt(8 + i * 12);
                        var samplesPerChunk = GetUInt(12 + i * 12);
                        var sampleDescriptionIndex = GetUInt(16 + i * 12);
                        Stsc.Add(new SampleToChunkMap { FirstChunk = firstChunk, SamplesPerChunk = samplesPerChunk, SampleDescriptionIndex = sampleDescriptionIndex });
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }

            if (handlerType != "soun")
            {
                Paragraphs = GetParagraphs(fs, handlerType);
            }
        }

        /// <summary>
        /// first_chunk -> entry lookup for the sample-to-chunk table, built once and cached.
        /// Duplicate first_chunk values are malformed - ISO/IEC 14496-12 8.7.4 requires them
        /// to increase - but they do occur in the wild, and ToDictionary throws on the second
        /// one, which took down the whole parse. Keep the first entry for a chunk instead.
        /// </summary>
        public Dictionary<uint, SampleToChunkMap> GetStscLookup()
        {
            if (_stscLookup == null)
            {
                _stscLookup = new Dictionary<uint, SampleToChunkMap>(Stsc.Count);
                foreach (var entry in Stsc)
                {
                    _stscLookup.TryAdd(entry.FirstChunk, entry);
                }
            }

            return _stscLookup;
        }

        /// <summary>
        /// How many table entries actually fit in <see cref="Box.Buffer"/>, i.e. the number of
        /// i >= 0 with <c>lastByteOfFirstEntry + i * stride &lt; Buffer.Length</c>. Hoisting the
        /// bound out of the loop lets the entry count also pre-size the destination list, and
        /// keeps a bogus declared count (up to 0xFFFFFFFF) from overflowing the index maths.
        /// </summary>
        private int ClampEntries(uint declaredEntries, int lastByteOfFirstEntry, int stride)
        {
            if (Buffer.Length <= lastByteOfFirstEntry)
            {
                return 0;
            }

            var fits = (Buffer.Length - lastByteOfFirstEntry + stride - 1) / stride;
            return declaredEntries < (uint)fits ? (int)declaredEntries : fits;
        }

        /// <summary>
        /// Total of the run-length sample counts, so the run-length expansion can allocate once.
        /// </summary>
        private int SumSampleCounts(int entries, int firstCountOffset, int stride)
        {
            ulong total = 0;
            for (var i = 0; i < entries; i++)
            {
                total += GetUInt(firstCountOffset + i * stride);
                if (total >= MaxRunLengthEntries)
                {
                    return MaxRunLengthEntries;
                }
            }

            return (int)total;
        }

        private List<Paragraph> GetParagraphs(Stream fs, string handlerType)
        {
            var stsdCodec = Stsd?.Name ?? "null";
            var paragraphs = new List<Paragraph>();
            uint samplesPerChunk = 1;
            var max = ChunkOffsets.Count;
            var index = 0;
            double totalTime = 0;
            ulong totalTicks = 0;
            var stscLookup = GetStscLookup();
            for (var chunkIndex = 0; chunkIndex < max; chunkIndex++)
            {
                if (stscLookup.TryGetValue((uint)chunkIndex + 1, out var newSamplesPerChunk))
                {
                    samplesPerChunk = newSamplesPerChunk.SamplesPerChunk;
                }

                var chunkOffset = ChunkOffsets[chunkIndex];
                var sampleOffset = chunkOffset; // tracks the byte position of the current sample within the chunk
                for (var i = 0; i < samplesPerChunk; i++)
                {
                    if (index >= SampleSizes.Count || index >= Ssts.Count)
                    {
                        return paragraphs;
                    }

                    var sampleSize = SampleSizes[index];
                    var sampleTime = Ssts[index];
                    var before = totalTime;
                    var beforeTicks = totalTicks;
                    totalTime += sampleTime / (double)TimeScale;
                    totalTicks += sampleTime;

                    if (sampleSize > 2)
                    {
                        if (handlerType == "vide")
                        {
                            //TODO: cea 608 or 708 cc? What is the content?
                        }
                        else if (handlerType == "clcp" && stsdCodec == "c608")
                        {
                            var sampleData = new byte[sampleSize];
                            fs.Seek((long)sampleOffset, SeekOrigin.Begin);
                            if (fs.Read(sampleData, 0, sampleData.Length) == sampleData.Length)
                            {
                                for (var j = 0; j + 1 < sampleData.Length; j += 2)
                                {
                                    var d1 = sampleData[j];
                                    var d2 = sampleData[j + 1];
                                    if (d1 != 0 || d2 != 0)
                                    {
                                        _cea608CcData.Add(new CcData(0, d1, d2) { Time = beforeTicks });
                                    }
                                }
                            }
                        }
                        else if (stsdCodec == "wvtt") // WebVTT in MP4 (ISO 14496-30)
                        {
                            var sampleEnd = sampleOffset + sampleSize;
                            fs.Seek((long)sampleOffset, SeekOrigin.Begin);

                            var wvttText = new StringBuilder();
                            while ((ulong)fs.Position < sampleEnd)
                            {
                                var boxStart = (ulong)fs.Position;
                                var boxHeader = new byte[8];
                                if (fs.Read(boxHeader, 0, 8) < 8)
                                    break;

                                var boxSize = BinaryPrimitives.ReadUInt32BigEndian(boxHeader.AsSpan(0, 4));
                                var boxName = GetString(boxHeader, 4, 4);
                                if (boxSize < 8)
                                    break;

                                var boxEnd = boxStart + boxSize;
                                if (boxEnd > sampleEnd)
                                    break; // malformed box extends beyond sample

                                if (boxName == "vttc")
                                {
                                    // Vttc parses payl (cue payload) and sttg (cue settings) sub-boxes
                                    var vttcBox = new Vttc(fs, boxEnd);
                                    if (!string.IsNullOrEmpty(vttcBox.Data?.Payload))
                                    {
                                        if (wvttText.Length > 0)
                                            wvttText.AppendLine();
                                        wvttText.Append(vttcBox.Data.Payload);
                                    }
                                }
                                // vtte = empty cue (gap marker, 8 bytes), vtta = additional text - skip both

                                fs.Seek((long)boxEnd, SeekOrigin.Begin);
                            }

                            if (wvttText.Length > 0)
                            {
                                paragraphs.Add(new Paragraph(wvttText.ToString(), before * 1000.0, totalTime * 1000.0));
                            }
                        }
                        else
                        {
                            fs.Seek((long)sampleOffset, SeekOrigin.Begin);
                            var buffer = new byte[2];
                            fs.ReadFully(buffer, 0, buffer.Length);
                            var textSize = (uint)BinaryPrimitives.ReadUInt16BigEndian(buffer);

                            if (textSize > 0)
                            {
                                var p = new Paragraph();
                                p.StartTime.TotalSeconds = before;
                                p.EndTime.TotalSeconds = totalTime;

                                if (handlerType == "subp") // VobSub created with Mp4Box
                                {
                                    if (textSize > 100)
                                    {
                                        buffer = new byte[textSize + 2];
                                        fs.Seek((long)sampleOffset, SeekOrigin.Begin);
                                        fs.ReadFully(buffer, 0, buffer.Length);
                                        SubPictures.Add(new SubPicture(buffer)); // TODO: Where is palette?
                                        paragraphs.Add(p);
                                    }
                                }
                                else
                                {
                                    buffer = new byte[textSize];
                                    fs.ReadFully(buffer, 0, buffer.Length);
                                    p.Text = GetString(buffer, 0, (int)textSize).TrimEnd();

                                    if (_mdia.IsClosedCaption)
                                    {
                                        p.Text = MakeScenaristText(buffer);
                                    }

                                    if (!string.IsNullOrEmpty(p.Text))
                                    {
                                        paragraphs.Add(p);
                                    }
                                }
                            }
                        }
                    }

                    index++;
                    sampleOffset += sampleSize; // advance to next sample within this chunk
                }
            }

            if (_cea608CcData.Count > 0)
            {
                var cea608Parser = new CcDataC608Parser();
                cea608Parser.DisplayScreen += data =>
                {
                    var startMs = data.Start / (double)TimeScale * 1000.0;
                    var endMs = data.End / (double)TimeScale * 1000.0;
                    var text = GetC608Text(data.Screen);
                    if (!string.IsNullOrEmpty(text))
                    {
                        paragraphs.Add(new Paragraph(text, startMs, endMs));
                    }
                };
                foreach (var cc in _cea608CcData)
                {
                    cea608Parser.AddData((int)cc.Time, new[] { cc.Data1, cc.Data2 });
                }
            }

            return paragraphs;
        }

        private static string GetC608Text(SerializedRow[] screen)
        {
            var sb = new StringBuilder();
            foreach (var row in screen)
            {
                foreach (var column in row.Columns)
                {
                    sb.Append(column.Character);
                }
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private static string MakeScenaristText(byte[] buffer)
        {
            const string hexDigits = "0123456789abcdef";
            var sb = new StringBuilder();
            for (var j = 8; j < buffer.Length - 3; j++)
            {
                // Append the two nibbles directly; ToString("X2") + ToLowerInvariant()
                // allocated two strings for every byte of every caption sample.
                var b = buffer[j];
                sb.Append(hexDigits[b >> 4]);
                sb.Append(hexDigits[b & 0x0F]);
                if (j % 2 == 1)
                {
                    sb.Append(' ');
                }
            }

            var hex = sb.ToString();
            var errorCount = 0;
            var text = ScenaristClosedCaptions.GetSccText(hex, ref errorCount);
            if (text.StartsWith('n') && text.Length > 1)
            {
                text = "<i>" + text.Substring(1) + "</i>";
            }

            if (text.StartsWith("-n", StringComparison.Ordinal))
            {
                text = text.Remove(0, 2);
            }

            if (text.StartsWith("-N", StringComparison.Ordinal))
            {
                text = text.Remove(0, 2);
            }

            if (text.StartsWith('-') && !text.Contains(Environment.NewLine + "-"))
            {
                text = text.Remove(0, 1);
            }

            return text;
        }
    }
}
