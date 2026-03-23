using Nikse.SubtitleEdit.Core.Common;
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
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".elr", StringComparison.OrdinalIgnoreCase) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    var buffer = FileUtil.ReadAllBytesShared(fileName);
                    byte[] compareBuffer = { 0x05, 0x01, 0x0D, 0x15, 0x11, 0x00, 0xA9, 0x00, 0x45, 0x00, 0x6C, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x20, 0x00, 0x53, 0x00, 0x74, 0x00, 0x75, 0x00, 0x64, 0x00, 0x69, 0x00, 0x6F, 0x00 };

                    for (var i = 6; i < compareBuffer.Length; i++)
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
            var buffer = FileUtil.ReadAllBytesShared(fileName);
            var i = 128;
            while (i < buffer.Length - 40)
            {
                // 00 00 FE FF FF FF (00 00 01 = sub number 1)
                try
                {
                    if (IsSubNumber(buffer, i, out var number))
                    {
                        var p = new Paragraph
                        {
                            Number = number,
                            StartTime = GetTimeCode(buffer, i - 12),
                            EndTime = GetTimeCode(buffer, i - 4),
                        };
                        i += 9;

                        // seek to text
                        var sb = new StringBuilder();
                        while (i < buffer.Length - 10 && !IsSubNumber(buffer, i, out _))
                        {
                            if (buffer[i] >= 9 && buffer[i + 1] == 0 && buffer[i + 2] == 0x44)
                            {
                                i = ReadText(buffer, i, sb);
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

        private static int ReadText(byte[] buffer, int i, StringBuilder sb)
        {
            var length = buffer[i - 1];
            i += 12;
            for (var j = i; j < i + length * 4; j += 4)
            {
                sb.Append(Encoding.Unicode.GetString(buffer, j, 2));
            }

            sb.AppendLine();
            return i;
        }

        private static bool IsSubNumber(byte[] buffer, int i, out int number)
        {
            number = 0;

            if (buffer[i] == 0 && buffer[i + 1] == 0 && buffer[i + 2] == 0xfe && buffer[i + 3] == 0xff && buffer[i + 4] == 0xff && buffer[i + 5] == 0xff)
            {
                number = (buffer[i + 6] << 16) + (buffer[i + 7] << 8) + buffer[i + 8];
                return true;
            }

            return false;
        }

        private static TimeCode GetTimeCode(byte[] buffer, int idx)
        {
            try
            {
                const string format = "X4";
                var frames = int.Parse(buffer[idx].ToString(format));
                var seconds = int.Parse(buffer[idx + 1].ToString(format));
                var minutes = int.Parse(buffer[idx + 2].ToString(format));
                var hours = int.Parse(buffer[idx + 3].ToString(format));
                return new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));
            }
            catch
            {
                return new TimeCode();
            }
        }
    }
}
