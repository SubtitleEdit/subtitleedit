using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Stbl : Box
    {
        public List<string> Texts = new List<string>();
        public List<SubPicture> SubPictures = new List<SubPicture>();
        public List<double> StartTimeCodes = new List<double>();
        public List<double> EndTimeCodes = new List<double>();
        public ulong StszSampleCount = 0;
        private Mdia _mdia;

        public Stbl(FileStream fs, ulong maximumLength, ulong timeScale, string handlerType, Mdia mdia)
        {
            _mdia = mdia;
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (Name == "stco") // 32-bit - chunk offset
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    uint totalEntries = GetUInt(4);

                    uint lastOffset = 0;
                    for (int i = 0; i < totalEntries; i++)
                    {
                        uint offset = GetUInt(8 + i * 4);
                        if (lastOffset + 5 < offset)
                            ReadText(fs, offset, handlerType);
                        lastOffset = offset;
                    }
                }
                else if (Name == "co64") // 64-bit
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    uint totalEntries = GetUInt(4);

                    ulong lastOffset = 0;
                    for (int i = 0; i < totalEntries; i++)
                    {
                        ulong offset = GetUInt64(8 + i * 8);
                        if (lastOffset + 8 < offset)
                            ReadText(fs, offset, handlerType);
                        lastOffset = offset;
                    }
                }
                else if (Name == "stsz") // sample sizes
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    uint uniformSizeOfEachSample = GetUInt(4);
                    uint numberOfSampleSizes = GetUInt(8);
                    StszSampleCount = numberOfSampleSizes;
                    for (int i = 0; i < numberOfSampleSizes; i++)
                    {
                        if (12 + i * 4 + 4 < Buffer.Length)
                        {
                            uint sampleSize = GetUInt(12 + i * 4);
                        }
                    }
                }
                else if (Name == "stts") // sample table time to sample map
                {
                    //https://developer.apple.com/library/mac/#documentation/QuickTime/QTFF/QTFFChap2/qtff2.html#//apple_ref/doc/uid/TP40000939-CH204-SW1

                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    uint numberOfSampleTimes = GetUInt(4);
                    double totalTime = 0;
                    if (_mdia.IsClosedCaption)
                    {
                        for (int i = 0; i < numberOfSampleTimes; i++)
                        {
                            uint sampleCount = GetUInt(8 + i * 8);
                            uint sampleDelta = GetUInt(12 + i * 8);
                            for (int j = 0; j < sampleCount; j++)
                            {
                                totalTime += sampleDelta / (double)timeScale;
                                if (StartTimeCodes.Count > 0)
                                    EndTimeCodes[EndTimeCodes.Count - 1] = totalTime - 0.001;
                                StartTimeCodes.Add(totalTime);
                                EndTimeCodes.Add(totalTime + 2.5);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < numberOfSampleTimes; i++)
                        {
                            uint sampleCount = GetUInt(8 + i * 8);
                            uint sampleDelta = GetUInt(12 + i * 8);
                            totalTime += sampleDelta / (double)timeScale;
                            if (StartTimeCodes.Count <= EndTimeCodes.Count)
                                StartTimeCodes.Add(totalTime);
                            else
                                EndTimeCodes.Add(totalTime);
                        }
                    }
                }
                else if (Name == "stsc") // sample table sample to chunk map
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    int version = Buffer[0];
                    uint numberOfSampleTimes = GetUInt(4);
                    for (int i = 0; i < numberOfSampleTimes; i++)
                    {
                        if (16 + i * 12 + 4 < Buffer.Length)
                        {
                            uint firstChunk = GetUInt(8 + i * 12);
                            uint samplesPerChunk = GetUInt(12 + i * 12);
                            uint sampleDescriptionIndex = GetUInt(16 + i * 12);
                        }
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

        private void ReadText(FileStream fs, ulong offset, string handlerType)
        {
            fs.Seek((long)offset, SeekOrigin.Begin);
            var data = new byte[4];
            fs.Read(data, 0, 2);
            uint textSize = (uint)GetWord(data, 0);

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
                if (textSize == 0)
                {
                    fs.Read(data, 2, 2);
                    textSize = GetUInt(data, 0); // don't get it exactly - seems like mp4box sometimes uses 2 bytes length field (first text record only)... handbrake uses 4 bytes
                }
                if (textSize > 0 && textSize < 500)
                {
                    data = new byte[textSize];
                    fs.Read(data, 0, data.Length);
                    string text = GetString(data, 0, (int)textSize).TrimEnd();

                    if (_mdia.IsClosedCaption)
                    {
                        var sb = new StringBuilder();
                        for (int j = 8; j < data.Length - 3; j++)
                        {
                            string h = data[j].ToString("X2").ToLower();
                            if (h.Length < 2)
                                h = "0" + h;
                            sb.Append(h);
                            if (j % 2 == 1)
                                sb.Append(' ');
                        }
                        string hex = sb.ToString();
                        int errorCount = 0;
                        text = ScenaristClosedCaptions.GetSccText(hex, ref errorCount);
                        if (text.StartsWith('n') && text.Length > 1)
                            text = "<i>" + text.Substring(1) + "</i>";
                        if (text.StartsWith("-n", StringComparison.Ordinal))
                            text = text.Remove(0, 2);
                        if (text.StartsWith("-N"))
                            text = text.Remove(0, 2);
                        if (text.StartsWith('-') && !text.Contains(Environment.NewLine + "-"))
                            text = text.Remove(0, 1);
                    }
                    Texts.Add(text.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine));
                }
                else
                {
                    Texts.Add(string.Empty);
                }
            }
        }

    }
}
