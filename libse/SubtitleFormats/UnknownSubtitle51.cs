using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle51 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d+:\d+:\d+  ,  \d+:\d+:\d+:\d+  , .*$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 51";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0] != null && lines[0].StartsWith("{\\rtf1"))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}  ,  {1}  , {2}\r\n";
            const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds));
                string endTime = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds));
                sb.AppendFormat(paragraphWriteFormat, startTime, endTime, HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " | ")));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            int number = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || string.IsNullOrWhiteSpace(line.Trim('-')))
                {
                    continue;
                }

                if (RegexTimeCodes.Match(line).Success)
                {
                    string[] threePart = line.Split(new[] { ',' }, StringSplitOptions.None);
                    p = new Paragraph();
                    if (threePart.Length > 2 &&
                        line.Length > 32 &&
                        GetTimeCode(p.StartTime, threePart[0].Trim()) &&
                        GetTimeCode(p.EndTime, threePart[1].Trim()))
                    {
                        number++;
                        p.Number = number;
                        p.Text = line.Remove(0, 31).Trim().Replace(" | ", Environment.NewLine).Replace("|", Environment.NewLine);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (line.StartsWith("//", StringComparison.Ordinal))
                {
                    // comment
                }
                else if (p != null && p.Text.Length < 200)
                {
                    p.Text = (p.Text + Environment.NewLine + line.Trim()).Trim();
                }
                else
                {
                    _errorCount++;
                }
            }
        }

        private static bool GetTimeCode(TimeCode timeCode, string timeString)
        {
            try
            {
                string[] timeParts = timeString.Split(':');
                timeCode.Hours = int.Parse(timeParts[0]);
                timeCode.Minutes = int.Parse(timeParts[1]);
                timeCode.Seconds = int.Parse(timeParts[2]);
                timeCode.Milliseconds = FramesToMillisecondsMax999(int.Parse(timeParts[3]));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
