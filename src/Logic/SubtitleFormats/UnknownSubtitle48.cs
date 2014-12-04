using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
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

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 48"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0] != null && lines[0].StartsWith("{\\rtf1"))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
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
                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, endTime, Utilities.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
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
                    var parts = line.Split(' ');
                    if (parts.Length > 2)
                    {
                        subtitle.Paragraphs.Add(new Paragraph(GetTimeCode(parts[0]), GetTimeCode(parts[1]), line.Substring(26).Trim()));
                    }
                }
                else
                {
                    _errorCount += 10;
                }
            }
            subtitle.Renumber(1);
        }

        private static TimeCode GetTimeCode(string timestamp)
        {
            var parts = timestamp.Split(':', '.');
            return TimeCode.FromTimestampTokens(parts[0], parts[1], parts[2], parts[3]);
        }
    }
}
