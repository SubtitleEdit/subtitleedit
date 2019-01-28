using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Sptx : SubtitleFormat
    {
        public override string Extension => ".sptx";

        public static string NameOfFormat => "sptx";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    var fi = new FileInfo(fileName);
                    if (fi.Length > 100 && fi.Length < 1024000) // not too small or too big
                    {
                        if (fileName.EndsWith(".sptx", StringComparison.OrdinalIgnoreCase))
                        {
                            return base.IsMine(lines, fileName);
                        }
                    }
                }
                catch
                {
                    return false;
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
            subtitle.Paragraphs.Clear();
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            int index = buffer[0]; // go to first subtitle paragraph
            while (index < buffer.Length)
            {
                Paragraph p = GetSptxParagraph(ref index, buffer);
                if (p != null)
                {
                    subtitle.Paragraphs.Add(p);
                }
                else
                {
                    break;
                }
            }
            subtitle.Renumber();
        }

        private static Paragraph GetSptxParagraph(ref int index, byte[] buffer)
        {
            while (index + 50 <= buffer.Length)
            {
                bool ok = true;
                for (int i = 0; i < 16; i++)
                {
                    if (buffer[index + i] < 0x30 || buffer[index + i] > 0x39)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    var allItalic = buffer[index + 16 + 4] == 1;
                    var length1 = buffer[index + 16 + 20];
                    var length2 = buffer[index + 16 + 26];
                    var length = length1;
                    if (length == 0 || length > 200 && length2 < 200)
                    {
                        length = length2;
                    }

                    if (index + 16 + 34 + length < buffer.Length)
                    {
                        var text = string.Empty;
                        if (length > 0)
                        {
                            text = Encoding.Unicode.GetString(buffer, index + 16 + 34, length);
                        }
                        var p = new Paragraph
                        {
                            StartTime = GetTimeCode(Encoding.UTF8.GetString(buffer, index, 8)),
                            EndTime = GetTimeCode(Encoding.UTF8.GetString(buffer, index + 8, 8)),
                            Text = FixItalics(text)
                        };
                        if (allItalic)
                        {
                            p.Text = "<i>" + p.Text.Trim() + "</i>";
                        }

                        index += 16;
                        return p;
                    }
                }
                index++;
            }

            return null;
        }

        internal static string FixItalics(string text)
        {
            if (text.Contains('<') && text.Contains('>'))
            {
                var sb = new StringBuilder();
                foreach (var ch in text)
                {
                    if (ch == '<')
                    {
                        sb.Append(" <i>");
                    }
                    else if (ch == '>')
                    {
                        sb.Append("</i> ");
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                return sb.ToString()
                         .Replace("  ", " ")
                         .Replace(Environment.NewLine + " ", Environment.NewLine)
                         .Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
            }
            return text;
        }

        internal static TimeCode GetTimeCode(string timeCode)
        {
            int hour = int.Parse(timeCode.Substring(0, 2));
            int minute = int.Parse(timeCode.Substring(2, 2));
            int second = int.Parse(timeCode.Substring(4, 2));
            int frames = int.Parse(timeCode.Substring(6, 2));

            int milliseconds = (int)Math.Round(1000.0 / Configuration.Settings.General.CurrentFrameRate * frames);
            if (milliseconds > 999)
            {
                milliseconds = 999;
            }

            return new TimeCode(hour, minute, second, milliseconds);
        }

    }
}
