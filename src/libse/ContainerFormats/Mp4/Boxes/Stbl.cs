using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Stbl : Box
    {
        public List<SubPicture> SubPictures;
        public ulong StszSampleCount;
        public ulong TimeScale { get; set; }
        private readonly Mdia _mdia;
        public List<uint> SampleSizes;
        public List<SampleTimeInfo> Ssts { get; set; }
        public List<SampleToChunkMap> Stsc { get; set; }
        public List<ChunkText> Texts;

        public Stbl(Stream fs, ulong maximumLength, ulong timeScale, string handlerType, Mdia mdia)
        {
            TimeScale = timeScale;
            _mdia = mdia;
            Position = (ulong)fs.Position;
            Ssts = new List<SampleTimeInfo>();
            Stsc = new List<SampleToChunkMap>();
            SampleSizes = new List<uint>();
            Texts = new List<ChunkText>();
            SubPictures = new List<SubPicture>();
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "stco") // 32-bit - chunk offset
                {
                    if (handlerType != "vide" && handlerType != "soun")
                    {
                        Buffer = new byte[Size - 4];
                        fs.Read(Buffer, 0, Buffer.Length);
                        int version = Buffer[0];
                        var totalEntries = GetUInt(4);
                        uint lastOffset = 0;
                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = GetUInt(8 + i * 4);
                            if (lastOffset + 5 < offset)
                            {
                                var text = ReadText(fs, offset, handlerType, i);
                                Texts.Add(text);
                            }
                            else
                            {
                                Texts.Add(new ChunkText { Size = 2, Text = null });
                            }

                            lastOffset = offset;
                        }
                    }
                }
                else if (Name == "co64") // 64-bit
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var totalEntries = GetUInt(4);
                    ulong lastOffset = 0;
                    for (var i = 0; i < totalEntries; i++)
                    {
                        var offset = GetUInt64(8 + i * 8);
                        if (lastOffset + 8 < offset)
                        {
                            var text = ReadText(fs, offset, handlerType, i);
                            Texts.Add(text);
                        }
                        else
                        {
                            Texts.Add(new ChunkText { Size = 2, Text = null });
                        }

                        lastOffset = offset;
                    }
                }
                else if (Name == "stsz") // sample sizes
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var uniformSizeOfEachSample = GetUInt(4);
                    var numberOfSampleSizes = GetUInt(8);
                    StszSampleCount = numberOfSampleSizes;
                    for (var i = 0; i < numberOfSampleSizes; i++)
                    {
                        if (12 + i * 4 + 4 < Buffer.Length)
                        {
                            var sampleSize = GetUInt(12 + i * 4);
                            SampleSizes.Add(sampleSize);
                        }
                    }
                }
                else if (Name == "stts") // sample table time to sample map
                {
                    //https://developer.apple.com/library/mac/#documentation/QuickTime/QTFF/QTFFChap2/qtff2.html#//apple_ref/doc/uid/TP40000939-CH204-SW1

                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var numberOfSampleTimes = GetUInt(4);
                    for (var i = 0; i < numberOfSampleTimes; i++)
                    {
                        var sampleCount = GetUInt(8 + i * 8);
                        var sampleDelta = GetUInt(12 + i * 8);
                        Ssts.Add(new SampleTimeInfo { SampleCount = sampleCount, SampleDelta = sampleDelta });
                    }

                }
                else if (Name == "stsc") // sample table sample to chunk map
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var numberOfSampleTimes = GetUInt(4);
                    for (int i = 0; i < numberOfSampleTimes; i++)
                    {
                        if (16 + i * 12 + 4 < Buffer.Length)
                        {
                            var firstChunk = GetUInt(8 + i * 12);
                            var samplesPerChunk = GetUInt(12 + i * 12);
                            var sampleDescriptionIndex = GetUInt(16 + i * 12);
                            Stsc.Add(new SampleToChunkMap { FirstChunk = firstChunk, SamplesPerChunk = samplesPerChunk, SampleDescriptionIndex = sampleDescriptionIndex });
                        }
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

        private ChunkText ReadText(Stream fs, ulong offset, string handlerType, int index)
        {
            if (handlerType == "vide")
            {
                return null;
            }

            if (handlerType == "soun")
            {
                return null;
            }

            fs.Seek((long)offset, SeekOrigin.Begin);
            var data = new byte[4];
            fs.Read(data, 0, 2);
            var textSize = (uint)GetWord(data, 0);

            if (handlerType == "subp") // VobSub created with Mp4Box
            {
                if (textSize > 100)
                {
                    fs.Seek((long)offset, SeekOrigin.Begin);
                    data = new byte[textSize + 2];
                    fs.Read(data, 0, data.Length);
                    SubPictures.Add(new SubPicture(data)); // TODO: Where is palette?
                }
            }
            else
            {
                if (handlerType == "text" && index + 1 < SampleSizes.Count && SampleSizes[index + 1] <= 2)
                {
                    return new ChunkText { Size = 2, Text = string.Empty };
                }

                if (textSize == 0)
                {
                    fs.Read(data, 2, 2);
                    textSize = GetUInt(data, 0); // don't get it exactly - seems like mp4box sometimes uses 2 bytes length field (first text record only)... handbrake uses 4 bytes
                }

                if (textSize > 0 && textSize < 500)
                {
                    data = new byte[textSize];
                    fs.Read(data, 0, data.Length);
                    var text = GetString(data, 0, (int)textSize).TrimEnd();

                    if (_mdia.IsClosedCaption)
                    {
                        var sb = new StringBuilder();
                        for (var j = 8; j < data.Length - 3; j++)
                        {
                            var h = data[j].ToString("X2").ToLowerInvariant();
                            if (h.Length < 2)
                            {
                                h = "0" + h;
                            }

                            sb.Append(h);
                            if (j % 2 == 1)
                            {
                                sb.Append(' ');
                            }
                        }

                        var hex = sb.ToString();
                        var errorCount = 0;
                        text = ScenaristClosedCaptions.GetSccText(hex, ref errorCount);
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
                    }

                    text = text.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine);
                    return new ChunkText { Size = textSize, Text = text };
                }
            }

            return new ChunkText { Size = 2, Text = null };
        }

        public class ExpandedSample
        {
            public uint SampleDelta { get; set; }
            public uint SampleSize { get; set; }

            public static List<ExpandedSample> From(List<SampleTimeInfo> stss, List<uint> stsz)
            {
                var result = new List<ExpandedSample>();
                for (var index = 0; index < stss.Count; index++)
                {
                    var timeToSample = stss[index];

                    for (var i = 0; i < timeToSample.SampleCount; i++)
                    {
                        result.Add(new ExpandedSample
                        {
                            SampleDelta = timeToSample.SampleDelta,
                            SampleSize = index < stsz.Count ? stsz[index] : 0,
                        });
                    }
                }

                return result;
            }
        }

        public List<Paragraph> GetParagraphsNew()
        {
            var paragraphs = new List<Paragraph>();
            var timeSamples = ExpandedSample.From(Ssts, SampleSizes);
            double totalTime = 0;
            var textIndex = 0;
            for (var index = 0; index < timeSamples.Count; index++)
            {
                var timeSample = timeSamples[index];
                var before = totalTime;
                totalTime += timeSample.SampleDelta / (double)TimeScale;
                if (textIndex < Texts.Count)
                {
                    var text = Texts[textIndex];
                    //if (text.Text == "In the tunnel.")
                    //{

                    //}

                    if (timeSample.SampleSize <= 2)
                    {
                        if (text.Size <= 2 && string.IsNullOrEmpty(text.Text))
                        {
                            textIndex++;
                        }
                    }
                    else
                    {
                        if (text.Size > 2 && !string.IsNullOrEmpty(text.Text))
                        {
                            paragraphs.Add(new Paragraph(text.Text, before * 1000.0, totalTime * 1000.0));
                        }

                        textIndex++;
                    }
                }
            }

            return paragraphs;
        }

        public List<Paragraph> GetParagraphs2()
        {
            // expand time codes
            var ssts = new List<SampleTimeInfo>();
            foreach (var timeInfo in Ssts)
            {
                for (var i = 0; i < timeInfo.SampleCount; i++)
                {
                    ssts.Add(new SampleTimeInfo { SampleCount = 1, SampleDelta = timeInfo.SampleDelta });
                }
            }
            Ssts = ssts;

            var paragraphs = new List<Paragraph>();
            var textIndex = 0;
            double totalTime = 0;
            uint samplesPerChunk = 1;
            var index = 0;

            var firstText = Texts.FirstOrDefault();
            var firstSampleSize = SampleSizes.FirstOrDefault();
            if (firstText != null && firstSampleSize != null && firstText.Size > firstSampleSize && firstSampleSize == 2 && !string.IsNullOrEmpty(firstText.Text))
            {
                var timeInfo = Ssts[index];
                totalTime += timeInfo.SampleDelta / (double)TimeScale;
                index++;
            }

            while (index < Ssts.Count)
            {
                var timeInfo = Ssts[index];

                var samplesPerChunkHit = Stsc.FirstOrDefault(p => p.FirstChunk == index + 1);
                if (samplesPerChunkHit != null)
                {
                    samplesPerChunk = samplesPerChunkHit.SamplesPerChunk;
                    if (samplesPerChunk == 0)
                    {
                        SeLogger.Error("MP4 has unexpected samples per chunk with zero");
                        samplesPerChunk = 1;
                    }
                }

                var before = totalTime;
                totalTime += timeInfo.SampleDelta / (double)TimeScale;

                var newSamplesPerChunk = samplesPerChunk;
                for (var i = 1; i < samplesPerChunk; i++) // extra
                {
                    index++;

                    samplesPerChunkHit = Stsc.FirstOrDefault(p => p.FirstChunk == index + 1);
                    if (samplesPerChunkHit != null)
                    {
                        newSamplesPerChunk = samplesPerChunkHit.SamplesPerChunk;
                        if (samplesPerChunk == 0)
                        {
                            SeLogger.Error("MP4 has unexpected samples per chunk with zero");
                            newSamplesPerChunk = 1;
                        }
                    }

                    if (index < Ssts.Count)
                    {
                        timeInfo = Ssts[index];
                        totalTime += timeInfo.SampleDelta / (double)TimeScale;
                    }
                }
                samplesPerChunk = newSamplesPerChunk;

                if (textIndex < Texts.Count)
                {
                    var text = Texts[textIndex];

                    if (text.Text != null && text.Text.Contains("How are we today", StringComparison.OrdinalIgnoreCase))
                    {
                    }

                    if (!string.IsNullOrEmpty(text.Text))
                    {
                        paragraphs.Add(new Paragraph(text.Text, before * 1000.0, totalTime * 1000.0));
                    }

                    textIndex++;
                }

                index++;
            }

            return paragraphs;
        }

        public List<Paragraph> GetParagraphs()
        {
            var paragraphs = new List<Paragraph>();
            double totalTime = 0;
            var allTimes = new List<double>();

            // expand time codes
            foreach (var timeInfo in Ssts)
            {
                for (var i = 0; i < timeInfo.SampleCount; i++)
                {
                    totalTime += timeInfo.SampleDelta / (double)TimeScale;
                    allTimes.Add(totalTime);
                }
            }

            var index = 0;
            var textIndex = 0;
            while (index < allTimes.Count - 1)
            {
                if (index > 0 && index + 1 < SampleSizes.Count && SampleSizes[index + 1] == 2)
                {
                    index++;
                }

                var timeStart = allTimes[index];
                var timeEnd = timeStart + 2;
                if (index + 1 < allTimes.Count)
                {
                    timeEnd = allTimes[index + 1];
                }

                if (_mdia.IsVobSubSubtitle && SubPictures.Count > textIndex)
                {
                    paragraphs.Add(new Paragraph(string.Empty, timeStart * 1000.0, timeEnd * 1000.0));
                }
                else if (Texts.Count > textIndex)
                {
                    var text = Texts[textIndex];

                    if (text.Size <= 2 && text.Text == null && textIndex +1 < Texts.Count)
                    {
                        textIndex++;
                        text = Texts[textIndex];
                    }

                    if (!string.IsNullOrEmpty(text.Text))
                    {
                        paragraphs.Add(new Paragraph(text.Text, timeStart * 1000.0, timeEnd * 1000.0));
                    }
                }

                index++;
                textIndex++;
            }

            return paragraphs;
        }
    }
}
