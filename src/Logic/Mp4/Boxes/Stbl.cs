using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Stbl : Box
    {

        public readonly List<string> Texts = new List<string>();
        public readonly List<double> StartTimeCodes = new List<double>();
        public readonly List<double> EndTimeCodes = new List<double>();

        public Stbl(FileStream fs, ulong maximumLength, UInt32 timeScale, string handlerType)
        {
            pos = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (name == "stco") // 32-bit
                {
                    buffer = new byte[size - 4];
                    fs.Read(buffer, 0, buffer.Length);
                    int version = buffer[0];
                    uint totalEntries = GetUInt(4);

                    uint lastOffset = 0;
                    for (int i = 0; i < totalEntries; i++)
                    {
                        uint offset = GetUInt(8 + i * 4);
                        if (lastOffset + 5 < offset)
                        {
                            ReadText(fs, offset);
                        }
                        lastOffset = offset;
                    }
                }
                else if (name == "co64") // 64-bit
                {
                    buffer = new byte[size - 4];
                    fs.Read(buffer, 0, buffer.Length);
                    int version = buffer[0];
                    uint totalEntries = GetUInt(4);

                    ulong lastOffset = 0;
                    for (int i = 0; i < totalEntries; i++)
                    {
                        ulong offset = GetUInt64(8 + i * 8);
                        if (lastOffset + 8 < offset)
                        {
                            ReadText(fs, offset);
                        }
                        lastOffset = offset;
                    }
                }
                else if (name == "stsz") // sample sizes
                {
                    buffer = new byte[size - 4];
                    fs.Read(buffer, 0, buffer.Length);
                    int version = buffer[0];
                    uint uniformSizeOfEachSample = GetUInt(4);
                    uint numberOfSampleSizes = GetUInt(8);
                    for (int i = 0; i < numberOfSampleSizes; i++)
                    {
                        uint sampleSize = GetUInt(12 + i * 4);
                    }
                }
                else if (name == "stts") // sample table time to sample map
                {
                    buffer = new byte[size - 4];
                    fs.Read(buffer, 0, buffer.Length);
                    int version = buffer[0];
                    uint numberOfSampleTimes = GetUInt(4);
                    double totalTime = 0;
                    for (int i = 0; i < numberOfSampleTimes; i++)
                    {
                        uint sampleCount = GetUInt(8 + i * 8);
                        uint sampleDelta = GetUInt(12 + i * 8);
                        totalTime += (double)(sampleDelta / (double)timeScale);
                        if (StartTimeCodes.Count <= EndTimeCodes.Count)
                            StartTimeCodes.Add(totalTime);
                        else
                            EndTimeCodes.Add(totalTime);
                    }
                }
                else if (name == "stsc") // sample table sample to chunk map
                {
                    buffer = new byte[size - 4];
                    fs.Read(buffer, 0, buffer.Length);
                    int version = buffer[0];
                    uint numberOfSampleTimes = GetUInt(4);
                    for (int i = 0; i < numberOfSampleTimes; i++)
                    {
                        uint firstChunk = GetUInt(8 + i * 12);
                        uint samplesPerChunk = GetUInt(12 + i * 12);
                        uint sampleDescriptionIndex = GetUInt(16 + i * 12);
                    }
                }

                fs.Seek((long)pos, SeekOrigin.Begin);
            }
        }

        private void ReadText(FileStream fs, ulong offset)
        {
            fs.Seek((long)offset, SeekOrigin.Begin);
            byte[] data = new byte[150];
            fs.Read(data, 0, data.Length);

            uint textSize = (uint)GetWord(data, 0); // don't get it exactly - seems like mp4box sometimes uses 2 bytes length field... handbrake uses 4 bytes
            if (textSize > 0)
            {
                if (textSize < data.Length - 2)
                {
                    string text = GetString(data, 2, (int)textSize).TrimEnd();
                    Texts.Add(text);
                }
            }
            else
            {
                textSize = GetUInt(data, 0);
                if (textSize > 0 && textSize < data.Length - 4)
                {
                    string text = GetString(data, 4, (int)textSize).TrimEnd();
                    Texts.Add(text);
                }
            }
        }

    }
}
