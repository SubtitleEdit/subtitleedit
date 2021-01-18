using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle97 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d:\d\d:\d\d\s+.+", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 97";

        public override string ToText(Subtitle subtitle, string title)
        {
            //00:00:20 I think we’ve heard enough.
            //
            //00:00:22 - Wait, he’s the one that…
            //\t\t-ENOUGH!
            const string writeFormat = "{0:00}:{1:00}:{2:00} {3}";
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                var textBuilder = new StringBuilder();
                var list = p.Text.SplitToLines();
                for (var index = 0; index < list.Count; index++)
                {
                    var line = list[index];
                    if (index > 0)
                    {
                        textBuilder.Append("\t\t");
                    }
                    textBuilder.AppendLine(line);
                }

                sb.AppendLine(string.Format(writeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, textBuilder));
            }
            return sb.ToString().Trim() + Environment.NewLine;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (string.IsNullOrEmpty(s))
                {
                    // ignore
                }
                else
                {
                    var match = RegexTimeCode.Match(s);
                    if (match.Success)
                    {
                        var arr = match.Value.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr.Length >= 2)
                        {
                            var start = GetTimeCode(arr[0].Trim());
                            var text = s.Remove(0, 8).Trim();
                            paragraph = new Paragraph(start, new TimeCode(), text);
                            paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                            subtitle.Paragraphs.Add(paragraph);
                        }
                    }
                    else if (paragraph != null)
                    {
                        paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                        paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
            new FixOverlappingDisplayTimes().Fix(subtitle, new EmptyFixCallback());
            subtitle.Renumber();
        }

        private static TimeCode GetTimeCode(string code)
        {
            var arr = code.Split(':');
            if (arr.Length == 3 && int.TryParse(arr[0], out int hours) && int.TryParse(arr[1], out int minutes) && int.TryParse(arr[2], out int seconds))
            {
                return new TimeCode(hours, minutes, seconds, 0);
            }
            return new TimeCode();
        }
    }
}
