using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class ElrPrint : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d+ - \d \d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d \d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "ELR Studio print";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            string s = sb.ToString();
            if (s.Contains("#PE2"))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{0} - 0 {1} {2} {3}{4}{5}";
            var sb = new StringBuilder();
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = subtitle.Paragraphs[index];
                sb.AppendLine(string.Format(writeFormat, index + 1, p.StartTime.ToHHMMSSFF(), p.EndTime.ToHHMMSSFF(), GetDuration(p), Environment.NewLine, p.Text));
            }

            return sb.ToString().Trim();
        }

        private object GetDuration(Paragraph p)
        {
            string s;
            var ts = p.Duration.TimeSpan;
            var frames = Math.Round(ts.Milliseconds / (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
            {
                s = $"{ts.Seconds + 1:00}:{0:00}";
            }
            else
            {
                s = $"{ts.Seconds:00}:{MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}";
            }
            if (p.Duration.TotalMilliseconds >= 0)
            {
                return s;
            }

            return "-" + s.RemoveChar('-');
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                var s = line.Trim();
                if (RegexTimeCode.IsMatch(s))
                {
                    var arr = s.Split();
                    var start = TimeCode.ParseHHMMSSFFToMilliseconds(arr[3]);
                    var end = TimeCode.ParseHHMMSSFFToMilliseconds(arr[4]);
                    paragraph = new Paragraph(string.Empty, start, end);
                    subtitle.Paragraphs.Add(paragraph);
                }
                else if (paragraph != null)
                {
                    paragraph.Text = (paragraph.Text + Environment.NewLine + line).Trim();
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

    }
}
