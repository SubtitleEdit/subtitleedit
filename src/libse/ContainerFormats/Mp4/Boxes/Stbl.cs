using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

                        var offSets = new List<uint>();
                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = GetUInt(8 + i * 4);
                            offSets.Add(offset);
                        }

                        // Try to figure out if we allow two tries to get text size...
                        var nonPrintableTexts = 0;
                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = offSets[i];
                            if (HasNonPrintableCharsInSecond(fs, offset, i))
                            {
                                nonPrintableTexts++;
                            }
                        }
                        var allowSecondRead = !(nonPrintableTexts > 1);


                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = offSets[i];
                            var nextOffset = i + 1 < totalEntries ? offSets[i + 1] : offset + 500;
                            var text = ReadText(fs, offset, nextOffset, handlerType, i, allowSecondRead);
                            Texts.Add(text);
                        }
                    }
                }
                else if (Name == "co64") // 64-bit
                {
                    if (handlerType != "vide" && handlerType != "soun")
                    {
                        Buffer = new byte[Size - 4];
                        fs.Read(Buffer, 0, Buffer.Length);
                        int version = Buffer[0];
                        var totalEntries = GetUInt(4);

                        var offSets = new List<ulong>();
                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = GetUInt64(8 + i * 8);
                            offSets.Add(offset);
                        }

                        // Try to figure out if we allow two tries to get text size...
                        var nonPrintableTexts = 0;
                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = offSets[i];
                            if (HasNonPrintableCharsInSecond(fs, offset, i))
                            {
                                nonPrintableTexts++;
                            }
                        }
                        var allowSecondRead = !(nonPrintableTexts > 1);

                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = offSets[i];
                            var nextOffset = i + 1 < totalEntries ? offSets[i + 1] : offset + 500;
                            var text = ReadText(fs, offset, nextOffset, handlerType, i, allowSecondRead);
                            Texts.Add(text);
                        }
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

        private static bool HasNonPrintableCharsInSecond(Stream fs, ulong offset, int index)
        {
            fs.Seek((long)offset, SeekOrigin.Begin);
            var data = new byte[2];
            fs.Read(data, 0, 2);
            var textSize = (uint)GetWord(data, 0);
            if (textSize != 0)
            {
                return false;
            }

            fs.Read(data, 0, 2);
            textSize = (uint)GetWord(data, 0);
            if (textSize > 0 && textSize < 500)
            {
                data = new byte[textSize];
                fs.Read(data, 0, data.Length);
                var s = GetString(data, 0, data.Length);
                foreach (var ch in s)
                {
                    var nonRenderingCategories = new[] 
                    {
                        UnicodeCategory.Control,
                        UnicodeCategory.OtherNotAssigned,
                        UnicodeCategory.Surrogate,
                    };

                    var isPrintable = char.IsWhiteSpace(ch) || !nonRenderingCategories.Contains(char.GetUnicodeCategory(ch));
                    if (!isPrintable)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private ChunkText ReadText(Stream fs, ulong offset, ulong nextOffset, string handlerType, int index, bool allowSecondRead)
        {
            fs.Seek((long)offset, SeekOrigin.Begin);
            var data = new byte[4];
            fs.Read(data, 0, 2);
            var textSize = (uint)GetWord(data, 0);

            var maxSize = nextOffset - offset;
            if (maxSize <= 2)
            {
                return new ChunkText { Size = 2 };
            }

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

                if (textSize == 0 && allowSecondRead)
                {
                    fs.Read(data, 0, 2);
                    textSize = (uint)GetWord(data, 0); // don't get it exactly - seems like mp4box sometimes uses 2 bytes length field (first text record only)... handbrake uses 4 bytes
                }

                if (textSize > 0 && textSize < 500 && textSize <= maxSize)
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
                    return new ChunkText { Size = textSize + 2, Text = text };
                }
            }

            return new ChunkText { Size = 2, Text = null };
        }

        public List<Paragraph> GetParagraphs()
        {
            var paragraphs = new List<Paragraph>();

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

            var index = 0;
            var textIndex = 0;
            double totalTime = 0;

            if (SampleSizes.Count > 2 && SampleSizes[0] == 2 && SampleSizes[1] == 2 &&
                Texts.Count > 2 && string.IsNullOrEmpty(Texts[0].Text) && !string.IsNullOrEmpty(Texts[1].Text))
            {
                index++;
                totalTime += Ssts[0].SampleDelta / (double)TimeScale;
            }

            while (index < Ssts.Count)
            {
                var before = totalTime;
                totalTime += Ssts[index].SampleDelta / (double)TimeScale;

                if (Ssts[index].SampleDelta == 0 && index + 1 < SampleSizes.Count && SampleSizes[index] == 2)
                {
                    index++;
                    before = totalTime;
                    totalTime += Ssts[index].SampleDelta / (double)TimeScale;

                    if (textIndex < Texts.Count)
                    {
                        var text = Texts[textIndex];
                        if (text.Size <= 2 && string.IsNullOrEmpty(text.Text) && textIndex + 1 < Texts.Count)
                        {
                            textIndex++;
                        }
                    }
                }

                if (index + 1 < SampleSizes.Count && SampleSizes[index] == 2)
                {
                    index++;
                    before = totalTime;
                    totalTime += Ssts[index].SampleDelta / (double)TimeScale;
                }

                if (_mdia.IsVobSubSubtitle && SubPictures.Count > textIndex)
                {
                    paragraphs.Add(new Paragraph(string.Empty, before * 1000.0, totalTime * 1000.0));
                }
                else if (Texts.Count > textIndex)
                {
                    var text = Texts[textIndex];

                    if (text.Size <= 2 && string.IsNullOrEmpty(text.Text) && textIndex + 1 < Texts.Count)
                    {
                        textIndex++;
                        text = Texts[textIndex];
                    }

                    if (!string.IsNullOrEmpty(text.Text))
                    {
                        paragraphs.Add(new Paragraph(text.Text, before * 1000.0, totalTime * 1000.0));
                    }
                }

                index++;
                textIndex++;
            }

            if (index <= Ssts.Count && textIndex < Texts.Count && index > 0)
            {
                var text = Texts[textIndex];
                if (!string.IsNullOrEmpty(text.Text))
                {
                    var before = totalTime - Ssts[index - 1].SampleDelta / (double)TimeScale;
                    paragraphs.Add(new Paragraph(text.Text, before * 1000.0, totalTime * 1000.0));
                }
            }

            return paragraphs;
        }
    }
}
