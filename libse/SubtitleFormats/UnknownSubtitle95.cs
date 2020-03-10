using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle95 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d+\s+\d\d:\d\d:\d\d.\d\d\s+\d\d:\d\d:\d\d\.\d\d\s+\d\d\:\d\d\:\d\d\.\d\d\s+#[0-9A-F]\s+[0-9A-F]+\s+#[0-9A-F]$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 95";

        public override string ToText(Subtitle subtitle, string title)
        {
            //001	10:00:00.22	10:00:04.22	00:00:04.00	#F CC00000D0	#C 
            //Studio presents
            //002 10:00:13.02 10:00:15.22 00:00:02.20	#F CC00000D0	#C 
            //What does my stuff
            //mean to me ?

            const string writeFormat = "{0:000} {1} {2} {3}	#F CC00000D0	#C{4}{5}";
            var sb = new StringBuilder();
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                sb.AppendLine(string.Format(writeFormat, index + 1, p.StartTime.ToHHMMSSPeriodFF(), p.EndTime.ToHHMMSSPeriodFF(), p.Duration.ToHHMMSSPeriodFF(), Environment.NewLine, p.Text));
            }

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (RegexTimeCode.IsMatch(s))
                {
                    var arr = s.Split();
                    var start = TimeCode.ParseHHMMSSFFToMilliseconds(arr[1]);
                    var end = TimeCode.ParseHHMMSSFFToMilliseconds(arr[2]);
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
