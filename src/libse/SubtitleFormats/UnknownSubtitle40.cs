using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle40 : SubtitleFormat
    {
        // 0:01 – 0:03
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d\d – \d+:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 40";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)} – {EncodeTimeCode(p.EndTime)}{Environment.NewLine}{HtmlUtil.RemoveHtmlTags(p.Text)}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours * 60 + time.Minutes}:{time.Seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string[] temp = line.Split('–');
                    string start = temp[0].Trim();
                    string end = temp[1].Trim();

                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 2 && endParts.Length == 2)
                    {
                        p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Text = line;
                    }
                    else
                    {
                        p.Text = p.Text.TrimEnd() + Environment.NewLine + line;
                    }

                    if (p.Text.Length > 500)
                    {
                        return;
                    }
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            var minutes = int.Parse(parts[0]);
            var seconds = int.Parse(parts[1]);
            return new TimeCode(0, minutes, seconds, 0);
        }

    }
}
