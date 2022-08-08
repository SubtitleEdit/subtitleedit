using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle106 : SubtitleFormat
    {
        //7:00:01AM
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\:\d\d\:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 106";

        public override bool IsMine(List<string> lines, string fileName)
        {
            return base.IsMine(lines, fileName) &&
                   !new TimeCodesOnly2().IsMine(lines, fileName) &&
                   !new UnknownSubtitle47().IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}\t{p.Text.Replace(Environment.NewLine, " ")}");
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return $"{timeCode.Hours}:{timeCode.Minutes:00}:{timeCode.Seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (RegexTimeCodes.Match(s).Success)
                {
                    var arr = s.Substring(0, 7).Split(':');
                    if (arr.Length == 3 &&
                        int.TryParse(arr[0], out var hours) &&
                        int.TryParse(arr[1], out var minutes) &&
                        int.TryParse(arr[2], out var seconds))
                    {
                        var p = new Paragraph
                        {
                            StartTime = new TimeCode(hours, minutes, seconds, 0),
                            Text = s.Remove(0, 7).Trim()
                        };
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (s.Length > 0)
                {
                    _errorCount++;
                }
            }

            var index = 1;
            foreach (var paragraph in subtitle.Paragraphs)
            {
                var next = subtitle.GetParagraphOrDefault(index);
                if (next != null)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
                else
                {
                    paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Configuration.Settings.General.NewEmptyDefaultMs;
                }

                if (paragraph.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                }
                index++;
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }
    }
}
