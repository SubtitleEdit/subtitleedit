using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle57 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d\.\d\d \d\d:\d\d:\d\d\.\d\d .+", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 57";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:00:54.08 00:00:58.06 - Saucers... - ... a dry lake bed.  (newline is //)
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)} {EncodeTimeCode(p.EndTime)} {HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "//")}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15.22 (last is frame)
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15.22 00:03:23.10 This is line one.//This is line two.
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            char[] splitChars = { ':', '.' };
            foreach (string line in lines)
            {
                var match = RegexTimeCodes.Match(line);
                if (match.Success)
                {
                    string temp = line.Substring(0, match.Length);
                    if (line.Length >= 23)
                    {
                        string text = line.Remove(0, 23).Trim();
                        if (!text.Contains(Environment.NewLine))
                        {
                            text = text.Replace("//", Environment.NewLine);
                        }

                        p = new Paragraph(DecodeTimeCodeFrames(temp.Substring(0, 11), splitChars), DecodeTimeCodeFrames(temp.Substring(12, 11), splitChars), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                }
                else if (p != null)
                {
                    if (p.Text.Length < 200)
                    {
                        p.Text = (p.Text + Environment.NewLine + line).Trim();
                    }
                }
            }

            subtitle.Renumber();
        }
    }
}
