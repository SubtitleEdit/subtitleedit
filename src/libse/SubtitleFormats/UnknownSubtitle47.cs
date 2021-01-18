using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle47 : SubtitleFormat
    {
        //7:00:01:27AM
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\:\d\d\:\d\d\:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 47";

        public override bool IsMine(List<string> lines, string fileName)
        {
            return base.IsMine(lines, fileName) && !new TimeCodesOnly2().IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}\t{p.Text.Replace(Environment.NewLine, " ")}");
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return $"{timeCode.Hours}:{timeCode.Minutes:00}:{timeCode.Seconds:00}:{MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCodes.Match(s).Success)
                {
                    try
                    {
                        var arr = s.Substring(0, 10).Split(':');
                        if (arr.Length == 4)
                        {
                            var p = new Paragraph();
                            p.StartTime = DecodeTimeCodeFramesFourParts(arr);
                            p.Text = s.Remove(0, 10).Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (s.Length > 0)
                {
                    _errorCount++;
                }
            }

            int index = 1;
            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(index);
                if (next != null)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
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
