using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class WinCaps32 : SubtitleFormat
    {
        public override string Extension => ".w32";
        public override string Name => "Wincaps W32";
        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName) && fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

                        return buffer[0] == 0x46 &&  // FCX
                               buffer[1] == 0x43 &&
                               buffer[2] == 0x58;
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            const int blockSize = 528;
            int i = 100;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
            while (i < buffer.Length - blockSize)
            {
                if (buffer[i] == 0x10 && buffer[i + 1] == 0 && buffer[i + 2] == 1) // subtitle block
                {
                    var p = new Paragraph();
                    var sb = new StringBuilder();
                    int idx = i;
                    int nextPosition = idx + blockSize;
                    while (idx < nextPosition)
                    {
                        if (buffer[idx] == 0x4f && buffer[idx + 1] == 0x46 && buffer[idx + 2] == 0x66) // textblock = OFf
                        {
                            int length = buffer[idx + 4] - 2;
                            sb.Append(Encoding.Unicode.GetString(buffer, idx + 6, length) + " ");
                            idx += length + 4;
                        }
                        else if (buffer[idx] == 0x54 &&
                                 buffer[idx + 1] == 0x41 &&
                                 buffer[idx + 2] == 0x45 &&
                                 buffer[idx + 3] == 0x4d &&
                                 buffer[idx + 4] == 0x49 &&
                                 buffer[idx + 5] == 0x54) // time codes = TAEMIT
                        {
                            p.StartTime = DecodeTime(buffer, idx + 10);
                            p.EndTime = DecodeTime(buffer, idx + 14);
                            idx += 15;
                        }
                        else
                        {
                            idx++;
                        }
                    }
                    p.Text = Utilities.AutoBreakLine(sb.ToString().TrimEnd());
                    if (p.Text.Length > 0)
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    i = nextPosition;
                }
                else
                {
                    i++;
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTime(byte[] buffer, int idx)
        {
            int hours = 0;
            int frames = buffer[idx];
            int seconds = buffer[idx + 1];
            int minutes = buffer[idx + 2];
            return new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));
        }

    }
}
