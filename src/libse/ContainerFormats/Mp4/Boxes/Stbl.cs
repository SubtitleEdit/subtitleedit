using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
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

        /// <summary>
        /// A QuickTime "c608" sample is a list of atoms: "cdat" holds the field 1 byte pairs
        /// (CC1/CC2), "cdt2" the field 2 ones (CC3/CC4). Reading the whole sample as byte pairs
        /// decoded the atom header too, so every caption ended in "cdat" (#15382). Only field 1 is
        /// decoded; a sample that is not atom-wrapped is read as bare byte pairs.
        /// The pairs of a sample go out one per video frame from the sample time, see
        /// <see cref="AddCcPairs"/>.
        /// </summary>
        private void AddC608SampleCcData(byte[] sampleData, ulong time, ulong durationTicks)
        {
            var pos = 0;
            var foundAtom = false;
            while (pos + 8 <= sampleData.Length)
            {
                var atomSize = (int)BinaryPrimitives.ReadUInt32BigEndian(sampleData.AsSpan(pos));
                var atomType = Encoding.ASCII.GetString(sampleData, pos + 4, 4);
                if (atomSize < 8 || pos + atomSize > sampleData.Length || (atomType != "cdat" && atomType != "cdt2"))
                {
                    break;
                }

                foundAtom = true;
                if (atomType == "cdat")
                {
                    AddCcPairs(sampleData, pos + 8, pos + atomSize, time, durationTicks);
                }

                pos += atomSize;
            }

            if (!foundAtom)
            {
                AddCcPairs(sampleData, 0, sampleData.Length, time, durationTicks);
            }
        }

        /// <summary>
        /// The cc_data of a QuickTime "c708" caption track (CEA-608 pairs and CEA-708 packets,
        /// timed in this track's ticks), for <see cref="MP4Parser"/> to decode like the captions
        /// of a video stream. Empty for other tracks.
        /// </summary>
        public List<CcData> C708CcData { get; } = new List<CcData>();

        /// <summary>
        /// A "c708" sample holds one frame's SMPTE 334 caption distribution packet in a "ccdp"
        /// atom: 96 69, length, frame rate, flags, sequence counter, an optional time code
        /// section (flags bit 7), then 0x72 and cc_count cc_data triplets. It used to be read as
        /// tx3g text - a zero length, so the whole track came out empty.
        /// </summary>
        private void AddC708SampleCcData(byte[] sampleData, ulong time)
        {
            var pos = 0;
            while (pos + 8 <= sampleData.Length)
            {
                var atomSize = (int)BinaryPrimitives.ReadUInt32BigEndian(sampleData.AsSpan(pos));
                if (atomSize < 8 || pos + atomSize > sampleData.Length)
                {
                    return;
                }

                var p = pos + 8;
                var end = pos + atomSize;
                if (Encoding.ASCII.GetString(sampleData, pos + 4, 4) == "ccdp" && p + 7 <= end &&
                    sampleData[p] == 0x96 && sampleData[p + 1] == 0x69)
                {
                    var q = p + 7 + ((sampleData[p + 4] & 0x80) != 0 ? 5 : 0);
                    if (q + 2 <= end && sampleData[q] == 0x72)
                    {
                        var frame = new List<CcData>();
                        GetCcDataHelper.AddCcTriplets(sampleData.AsSpan(0, end), q + 2, sampleData[q + 1] & 0x1F, frame);
                        foreach (var cc in frame)
                        {
                            cc.Time = time;
                        }

                        C708CcData.AddRange(frame);
                    }
                }

                pos = end;
            }
        }

        /// <summary>
        /// A sample holds the byte pairs of many frames - one pair per NTSC frame, starting at the
        /// sample time. Giving them all the sample time made a caption that is shown and replaced
        /// within one sample a zero-length cue. The pairs are spaced a frame apart, closer when
        /// they would not fit in the sample, so they never run into the next one.
        /// </summary>
        private void AddCcPairs(byte[] data, int start, int end, ulong time, ulong durationTicks)
        {
            var pairCount = (end - start) / 2;
            var frameTicks = TimeScale * 1001.0 / 30000.0;
            var step = pairCount > 0 ? Math.Min(frameTicks, durationTicks / (double)pairCount) : 0;
            for (var k = 0; k < pairCount; k++)
            {
                var d1 = data[start + k * 2];
                var d2 = data[start + k * 2 + 1];
                if (d1 != 0 || d2 != 0)
                {
                    _cea608CcData.Add(new CcData(0, d1, d2) { Time = time + (ulong)(k * step) });
                }
            }
        }

        /// <summary>
        /// Color lookup table for <see cref="SubPictures"/>, when the VobSub sample entry
        /// carries one - null means the four default colors are used.
        /// </summary>
        public List<SKColor> VobSubPalette { get; private set; }

        private List<Cea608.CcData> _cea608CcData = new List<Cea608.CcData>();
        private Dictionary<uint, SampleToChunkMap> _stscLookup;

        // Cap for the stts/ctts run-length expansions: a malformed sample count (up to
        // 0xFFFFFFFF) must not expand into an OOM-sized list. Far above any real sample
        // count (5M samples is ~23 hours at 60 fps).
        private const int MaxRunLengthEntries = 5_000_000;

        // Text subtitle samples are small; a larger declared size is malformed
        private const uint MaxTextSampleSize = 10_000_000;

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

                        // Entry 0 is GetUInt(8), i.e. bytes 8..11 - the helper wants the LAST byte
                        // of the first entry (the other call sites pass it correctly). With 8 the
                        // clamp allowed one entry too many and a short/truncated stco threw.
                        var entries = ClampEntries(totalEntries, 11, 4);
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

                        // Entry 0 is GetUInt64(8), i.e. bytes 8..15 - see the stco note above.
                        var entries = ClampEntries(totalEntries, 15, 8);
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
                else if (Name == "stz2") // compact sample sizes
                {
                    // ISO/IEC 14496-12 8.7.3.3: same information as "stsz", with the entries
                    // packed into 4, 8 or 16 bits each instead of a full word. Bento4's
                    // mp4compact rewrites a file this way, and without this the track came out
                    // with no sample sizes at all, i.e. no subtitles.
                    Buffer = new byte[Size - 4];
                    fs.ReadFully(Buffer, 0, Buffer.Length);
                    var fieldSize = Buffer[7]; // 3 bytes reserved, then the field size
                    var sampleCount = GetUInt(8);
                    StszSampleCount = sampleCount;

                    if (fieldSize == 4 || fieldSize == 8 || fieldSize == 16)
                    {
                        var available = Buffer.Length - 12;
                        var maxEntries = fieldSize == 4 ? available * 2 : available / (fieldSize / 8);
                        var entries = (int)Math.Min(Math.Min(sampleCount, (uint)maxEntries), MaxRunLengthEntries);
                        SampleSizes.Capacity = entries;
                        for (var i = 0; i < entries; i++)
                        {
                            switch (fieldSize)
                            {
                                case 4:
                                    var b = Buffer[12 + i / 2];
                                    SampleSizes.Add((uint)(i % 2 == 0 ? b >> 4 : b & 0x0F));
                                    break;
                                case 8:
                                    SampleSizes.Add(Buffer[12 + i]);
                                    break;
                                default:
                                    SampleSizes.Add((uint)GetWord(12 + i * 2));
                                    break;
                            }
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

            if (handlerType == "subp" && Stsd?.Name == "mp4s")
            {
                VobSubPalette = Mp4VobSubPalette.FromMp4sSampleEntry(Stsd.SampleEntryPayload);
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
                    var beforeTicks = totalTicks;
                    totalTicks += sampleTime;

                    // From the integer tick count, multiplying before dividing: summing
                    // per-sample seconds as doubles drifted just below whole milliseconds
                    // (19.53 s became 19529.99 ms), which displays as 19,529.
                    var startMs = beforeTicks * 1000.0 / TimeScale;
                    var endMs = totalTicks * 1000.0 / TimeScale;

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
                                AddC608SampleCcData(sampleData, beforeTicks, sampleTime);
                            }
                        }
                        else if (handlerType == "clcp" && stsdCodec == "c708")
                        {
                            var sampleData = new byte[sampleSize];
                            fs.Seek((long)sampleOffset, SeekOrigin.Begin);
                            if (fs.Read(sampleData, 0, sampleData.Length) == sampleData.Length)
                            {
                                AddC708SampleCcData(sampleData, beforeTicks);
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
                                paragraphs.Add(new Paragraph(wvttText.ToString(), startMs, endMs));
                            }
                        }
                        else if (stsdCodec == "stpp") // TTML/IMSC1 in MP4 (ISO 14496-30)
                        {
                            if (sampleSize <= MaxTextSampleSize)
                            {
                                var sampleData = new byte[sampleSize];
                                fs.Seek((long)sampleOffset, SeekOrigin.Begin);
                                if (fs.Read(sampleData, 0, sampleData.Length) == sampleData.Length)
                                {
                                    AddTtmlSample(sampleData, startMs, endMs - startMs, paragraphs);
                                }
                            }
                        }
                        else if (Mp4TextSampleHelper.IsSimpleTextCodec(stsdCodec)) // text stream in MP4 (ISO 14496-30)
                        {
                            if (sampleSize <= MaxTextSampleSize)
                            {
                                var sampleData = new byte[sampleSize];
                                fs.Seek((long)sampleOffset, SeekOrigin.Begin);
                                if (fs.Read(sampleData, 0, sampleData.Length) == sampleData.Length)
                                {
                                    var text = Mp4TextSampleHelper.ReadSimpleTextSample(sampleData);
                                    if (!string.IsNullOrEmpty(text))
                                    {
                                        paragraphs.Add(new Paragraph(text, startMs, endMs));
                                    }
                                }
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
                                p.StartTime.TotalMilliseconds = startMs;
                                p.EndTime.TotalMilliseconds = endMs;

                                if (handlerType == "subp") // VobSub created with Mp4Box
                                {
                                    if (textSize > 100)
                                    {
                                        buffer = new byte[textSize + 2];
                                        fs.Seek((long)sampleOffset, SeekOrigin.Begin);
                                        fs.ReadFully(buffer, 0, buffer.Length);
                                        SubPictures.Add(new SubPicture(buffer));
                                        paragraphs.Add(p);
                                    }
                                }
                                else if (_mdia.IsClosedCaption)
                                {
                                    buffer = new byte[textSize];
                                    fs.ReadFully(buffer, 0, buffer.Length);
                                    p.Text = MakeScenaristText(buffer);

                                    if (!string.IsNullOrEmpty(p.Text))
                                    {
                                        paragraphs.Add(p);
                                    }
                                }
                                else if (sampleSize <= MaxTextSampleSize)
                                {
                                    // the whole sample, so the tx3g modifier boxes after the text can be read too
                                    var sampleData = new byte[sampleSize];
                                    fs.Seek((long)sampleOffset, SeekOrigin.Begin);
                                    if (fs.Read(sampleData, 0, sampleData.Length) == sampleData.Length)
                                    {
                                        p.Text = Mp4TextSampleHelper.ReadTx3gSampleText(sampleData);
                                        if (!string.IsNullOrEmpty(p.Text))
                                        {
                                            paragraphs.Add(p);
                                        }
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
                    var startMs = data.Start * 1000.0 / TimeScale;
                    var endMs = data.End * 1000.0 / TimeScale;
                    Cea608CueBuilder.Add(paragraphs, SerializedScreenText.GetText(data.Screen), startMs, endMs);
                };
                foreach (var cc in _cea608CcData)
                {
                    cea608Parser.AddData((int)cc.Time, new[] { cc.Data1, cc.Data2 });
                }
            }

            return paragraphs;
        }

        private readonly HashSet<string> _addedTtmlCues = new HashSet<string>();

        /// <summary>
        /// An stpp sample is a complete TTML document covering the sample's time window;
        /// cue times inside the document are media times (or sample-relative for Smooth
        /// Streaming style documents), and cues spanning several samples are repeated in
        /// each document, so duplicates are dropped.
        /// </summary>
        private void AddTtmlSample(byte[] sampleData, double sampleStartMs, double sampleDurationMs, List<Paragraph> paragraphs)
        {
            var xml = Encoding.UTF8.GetString(sampleData);
            foreach (var doc in Mp4TtmlHelper.SplitTtmlDocuments(xml))
            {
                var docParagraphs = Mp4TtmlHelper.ParseTtmlDocument(doc);
                if (docParagraphs.Count == 0)
                {
                    continue;
                }

                if (Mp4TtmlHelper.AreTimesSampleRelative(docParagraphs, sampleStartMs, sampleDurationMs))
                {
                    foreach (var p in docParagraphs)
                    {
                        p.StartTime.TotalMilliseconds += sampleStartMs;
                        p.EndTime.TotalMilliseconds += sampleStartMs;
                    }
                }

                foreach (var p in docParagraphs)
                {
                    if (_addedTtmlCues.Add($"{p.StartTime.TotalMilliseconds:0}|{p.EndTime.TotalMilliseconds:0}|{p.Text}"))
                    {
                        paragraphs.Add(p);
                    }
                }
            }
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
