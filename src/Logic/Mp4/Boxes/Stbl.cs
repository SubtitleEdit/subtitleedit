using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Stbl
    {

        public readonly List<string> Texts = new List<string>();
        public readonly List<double> StartTimeCodes = new List<double>();
        public readonly List<double> EndTimeCodes = new List<double>();

        public Stbl(FileStream fs, ulong maximumLength, UInt32 timeScale)
        {
            var buffer = new byte[8];
            ulong pos = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                int bytesRead;
                fs.Seek((long)pos, SeekOrigin.Begin);
                ulong size = Helper.ReadSize(buffer, out bytesRead, fs);
                if (bytesRead < buffer.Length)
                    return;
                string name = Helper.GetString(buffer, 4, 4);
                pos = ((ulong)(fs.Position)) + size - 8;

                if (name == "stco") // 32-bit
                {
                    byte[] b = new byte[size - 4];
                    fs.Read(b, 0, b.Length);
                    int version = b[0];
                    uint totalEntries = Helper.GetUInt(b, 4);

                    uint lastOffset = 0;
                    for (int i = 0; i < totalEntries; i++)
                    {
                        uint offset = Helper.GetUInt(b, 8 + i * 4);
                        if (lastOffset + 5 < offset)
                        {
                            fs.Seek(offset, SeekOrigin.Begin);
                            byte[] data = new byte[150];
                            fs.Read(data, 0, data.Length);
                            uint textSize = Helper.GetUInt(data, 0);
                            if (textSize < data.Length - 4)
                            {
                                string text = Helper.GetString(data, 4, (int)textSize - 1);
                                Texts.Add(text);
                            }
                        }
                        lastOffset = offset;
                    }
                }
                else if (name == "co64") // 64-bit
                {
                    byte[] b = new byte[size - 4];
                    fs.Read(b, 0, b.Length);
                    int version = b[0];
                    uint totalEntries = Helper.GetUInt(b, 4);

                    ulong lastOffset = 0;
                    for (int i = 0; i < totalEntries; i++)
                    {
                        ulong offset = Helper.GetUInt64(b, 8 + i * 8);
                        if (lastOffset + 8 < offset)
                        {
                            fs.Seek((long)offset, SeekOrigin.Begin);
                            byte[] data = new byte[150];
                            fs.Read(data, 0, data.Length);
                            uint textSize = Helper.GetUInt(data, 0);
                            if (textSize < data.Length - 4)
                            {
                                string text = Helper.GetString(data, 4, (int)textSize - 1);
                                Texts.Add(text);
                            }
                        }
                        lastOffset = offset;
                    }
                }
                else if (name == "stsz") // sample sizes
                {
                    byte[] b = new byte[size - 4];
                    fs.Read(b, 0, b.Length);
                    int version = b[0];
                    uint uniformSizeOfEachSample = Helper.GetUInt(b, 4);
                    uint numberOfSampleSizes = Helper.GetUInt(b, 8);
                    for (int i = 0; i < numberOfSampleSizes; i++)
                    {
                        uint sampleSize = Helper.GetUInt(b, 12 + i * 4);
                    }
                }
                else if (name == "stts") // sample table time to sample map
                {
                    byte[] b = new byte[size - 4];
                    fs.Read(b, 0, b.Length);
                    int version = b[0];
                    uint numberOfSampleTimes = Helper.GetUInt(b, 4);
                    double totalTime = 0;
                    for (int i = 0; i < numberOfSampleTimes; i++)
                    {
                        uint sampleCount = Helper.GetUInt(b, 8 + i * 8);
                        uint sampleDelta = Helper.GetUInt(b, 12 + i * 8);
                        totalTime += (double)(sampleDelta / (double)timeScale);
                        if (StartTimeCodes.Count <= EndTimeCodes.Count)
                            StartTimeCodes.Add(totalTime);
                        else
                            EndTimeCodes.Add(totalTime);
                    }
                }
                else if (name == "stsc") // sample table sample to chunk map
                {
                    byte[] b = new byte[size - 4];
                    fs.Read(b, 0, b.Length);
                    int version = b[0];
                    uint numberOfSampleTimes = Helper.GetUInt(b, 4);
                    for (int i = 0; i < numberOfSampleTimes; i++)
                    {
                        uint firstChunk = Helper.GetUInt(b, 8 + i * 12);
                        uint samplesPerChunk = Helper.GetUInt(b, 12 + i * 12);
                        uint sampleDescriptionIndex = Helper.GetUInt(b, 16 + i * 12);
                    }
                }
            }
        }

    }
}
