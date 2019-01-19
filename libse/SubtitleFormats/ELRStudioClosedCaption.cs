using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class ELRStudioClosedCaption : SubtitleFormat
    {
        public override string Extension => ".elr";

        public override string Name => "ELRStudio Closed Caption";

        public static void Save(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                //...
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.EndsWith(".elr", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
                        byte[] compareBuffer = { 0x05, 0x01, 0x0D, 0x15, 0x11, 0x00, 0xA9, 0x00, 0x45, 0x00, 0x6C, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x20, 0x00, 0x53, 0x00, 0x74, 0x00, 0x75, 0x00, 0x64, 0x00, 0x69, 0x00, 0x6F, 0x00 };

                        for (int i = 6; i < compareBuffer.Length; i++)
                        {
                            if (buffer[i] != compareBuffer[i])
                            {
                                return false;
                            }
                        }

                        var sub = new Subtitle();
                        LoadSubtitle(sub, lines, fileName);
                        return sub.Paragraphs.Count > 0;
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            int i = 128;
            while (i < buffer.Length - 40)
            {
                try
                {
                    if ((buffer[i] == 0xc4 || buffer[i] == 0x5d) && buffer[i + 1] == 9 && buffer[i + 2] == 0 && buffer[i + 3] == 0x10) // start time (hopefully)
                    {
                        var p = new Paragraph { StartTime = GetTimeCode(buffer, i + 4) };
                        i += 7;

                        // seek to endtime
                        while (i < buffer.Length - 10 && !((buffer[i] == 0xc4 || buffer[i] == 0x5d) && buffer[i + 1] == 9 && buffer[i + 2] == 0 && buffer[i + 3] == 0x10))
                        {
                            i++;
                        }
                        if (buffer[i] == 0xc4 && buffer[i + 1] == 9 && buffer[i + 2] == 0 && buffer[i + 3] == 0x10)
                        {
                            p.EndTime = GetTimeCode(buffer, i + 4);
                            i += 7;
                        }
                        if (Math.Abs(p.EndTime.TotalMilliseconds) < 0.001)
                        {
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 2000;
                        }

                        // seek to text
                        var sb = new StringBuilder();
                        int min = 4;
                        while (min > 0 || i < buffer.Length - 10 && !((buffer[i] == 0xc4 || buffer[i] == 0x5d) && buffer[i + 1] == 9 && buffer[i + 2] == 0 && buffer[i + 3] == 0x10))
                        {
                            min--;
                            if (buffer[i] == 9 && buffer[i + 1] == 0 && buffer[i + 2] == 0x44)
                            {
                                var length = buffer[i - 1];
                                i += 12;
                                for (int j = i; j < i + length * 4; j += 4)
                                {
                                    sb.Append(Encoding.GetEncoding(1252).GetString(buffer, j, 1));
                                }
                                sb.AppendLine();
                            }
                            else
                            {
                                i++;
                            }
                        }
                        p.Text = (p.Text + " " + sb).Trim();
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        i++;
                    }
                }
                catch
                {
                    i += 5;
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode GetTimeCode(byte[] buffer, int idx)
        {
            try
            {
                const string format = "X4";
                int frames = int.Parse(buffer[idx].ToString(format));
                int seconds = int.Parse(buffer[idx + 1].ToString(format));
                int minutes = int.Parse(buffer[idx + 2].ToString(format));
                int hours = int.Parse(buffer[idx + 3].ToString(format));
                return new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));
            }
            catch
            {
                return new TimeCode();
            }
        }

    }
}
