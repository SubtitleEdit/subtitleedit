using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    //00:01:27.703 00:01:29.514 Okay.
    //00:01:29.259 00:01:31.514 Okaaayyyy.
    //00:01:32.534 00:01:34.888 Let's go over this once again.
    //00:01:35.446 00:01:38.346 Pick up the bread, walk the dog, go to the dry cleaners,
    //00:01:38.609 00:01:41.471 pick up the bread, walk the dog, go thoughtless,
    //00:01:42.247 00:01:43.915 pick up the cake

    public class UnknownSubtitle48 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d.\d\d\d \d\d:\d\d:\d\d.\d\d\d .*$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 48";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0] != null && lines[0].StartsWith("{\\rtf1", StringComparison.Ordinal))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0} {1} {2}";
            const string timeFormat = "{0:00}:{1:00}:{2:00}.{3:000}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                string endTime = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, endTime, HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.Match(line).Success)
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.None);
                    var p = new Paragraph();
                    if (parts.Length > 2 &&
                        GetTimeCode(p.StartTime, parts[0].Trim()) &&
                        GetTimeCode(p.EndTime, parts[1].Trim()))
                    {
                        p.Text = line.Remove(0, 25).Trim();
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else
                {
                    _errorCount += 10;
                }
            }
            subtitle.Renumber();
        }

        private static bool GetTimeCode(TimeCode timeCode, string timeString)
        {
            try
            {
                string[] timeParts = timeString.Split(':', '.');
                timeCode.Hours = int.Parse(timeParts[0]);
                timeCode.Minutes = int.Parse(timeParts[1]);
                timeCode.Seconds = int.Parse(timeParts[2]);
                timeCode.Milliseconds = int.Parse(timeParts[3]);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
