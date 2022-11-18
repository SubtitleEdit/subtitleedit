using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
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
        public List<uint> Ssts { get; set; }
        public List<SampleToChunkMap> Stsc { get; set; }
        public List<ChunkText> Texts;
        public List<Paragraph> Paragraphs;
        public List<Paragraph> GetParagraphs() => Paragraphs;

        public Stbl(Stream fs, ulong maximumLength, ulong timeScale, string handlerType, Mdia mdia)
        {
            TimeScale = timeScale;
            _mdia = mdia;
            Position = (ulong)fs.Position;
            Ssts = new List<uint>();
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

                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = GetUInt(8 + i * 4);
                            Texts.Add(new ChunkText { Offset = offset });
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

                        for (var i = 0; i < totalEntries; i++)
                        {
                            var offset = GetUInt64(8 + i * 8);
                            Texts.Add(new ChunkText { Offset = offset });
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
                        for (var j = 0; j < sampleCount; j++)
                        {
                            Ssts.Add(sampleDelta);
                        }
                    }
                }
                else if (Name == "stsc") // sample table sample to chunk map
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    var numberOfSampleTimes = GetUInt(4);
                    for (var i = 0; i < numberOfSampleTimes; i++)
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

            if (handlerType != "vide" && handlerType != "soun")
            {
                Paragraphs = GetParagraphs(fs, handlerType == "subp");
            }
        }

        private List<Paragraph> GetParagraphs(Stream fs, bool subtitlePicture)
        {
            var paragraphs = new List<Paragraph>();
            uint samplesPerChunk = 1;
            var max = Texts.Count;
            var index = 0;
            double totalTime = 0;
            for (var chunkIndex = 0; chunkIndex < max; chunkIndex++)
            {
                var newSamplesPerChunk = Stsc.FirstOrDefault(item => item.FirstChunk == chunkIndex + 1);
                if (newSamplesPerChunk != null)
                {
                    samplesPerChunk = newSamplesPerChunk.SamplesPerChunk;
                }

                var chunk = Texts[chunkIndex];
                var p = new Paragraph();
                for (var i = 0; i < samplesPerChunk; i++)
                {
                    if (index >= SampleSizes.Count || index >= Ssts.Count)
                    {
                        return paragraphs;
                    }

                    var sampleSize = SampleSizes[index];
                    var sampleTime = Ssts[index];
                    var before = totalTime;
                    totalTime += sampleTime / (double)TimeScale;

                    if (sampleSize > 2)
                    {
                        p.StartTime.TotalSeconds = before;

                        fs.Seek((long)chunk.Offset, SeekOrigin.Begin);
                        var buffer = new byte[2];
                        fs.Read(buffer, 0, buffer.Length);
                        var textSize = (uint)GetWord(buffer, 0);
                        if (textSize == 0)
                        {
                            //TODO: only if samples per chunk > 2 ?
                            fs.Read(buffer, 0, buffer.Length);
                            textSize = (uint)GetWord(buffer, 0);
                        }

                        if (textSize > 0)
                        {
                            if (subtitlePicture) // VobSub created with Mp4Box
                            {
                                if (textSize > 100)
                                {
                                    buffer = new byte[textSize + 2];
                                    fs.Seek((long)chunk.Offset, SeekOrigin.Begin);
                                    fs.Read(buffer, 0, buffer.Length);
                                    SubPictures.Add(new SubPicture(buffer)); // TODO: Where is palette?
                                    paragraphs.Add(p);
                                }
                            }
                            else
                            {
                                buffer = new byte[textSize];
                                fs.Read(buffer, 0, buffer.Length);
                                p.Text = GetString(buffer, 0, (int)textSize).TrimEnd();

                                if (_mdia.IsClosedCaption)
                                {
                                    p.Text = MakeScenaristText(buffer);
                                }

                                chunk.Text = p.Text;
                            }
                        }
                    }

                    p.EndTime.TotalSeconds = totalTime;
                    index++;
                }

                if (!string.IsNullOrEmpty(p.Text))
                {
                    paragraphs.Add(p);
                }
            }

            return paragraphs;
        }

        private static string MakeScenaristText(byte[] buffer)
        {
            var sb = new StringBuilder();
            for (var j = 8; j < buffer.Length - 3; j++)
            {
                var h = buffer[j].ToString("X2").ToLowerInvariant();
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
