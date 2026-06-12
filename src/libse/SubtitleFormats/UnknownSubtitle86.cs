using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle86 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\[\d\d\.\d\d\.\d\d\] ", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 86";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, " ");
                sb.AppendLine($"[{p.StartTime.Hours:00}.{p.StartTime.Minutes:00}.{p.StartTime.Seconds:00}] {text}");
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                bool success = false;
                string s = line.TrimStart();
                if (s.StartsWith('[') && RegexTimeCodes.Match(s).Success)
                {
                    try
                    {
                        string[] parts = s.Substring(1, 8).Split('.');
                        if (parts.Length == 3)
                        {
                            int hours = int.Parse(parts[0]);
                            int minutes = int.Parse(parts[1]);
                            int seconds = int.Parse(parts[2]);
                            string text = s.Remove(0, 10).TrimStart();
                            text = text.Replace("|", Environment.NewLine);
                            var start = new TimeCode(hours, minutes, seconds, 0);
                            double duration = Utilities.GetOptimalDisplayMilliseconds(text);
                            var end = new TimeCode(start.TotalMilliseconds + duration);

                            var p = new Paragraph(start, end, Utilities.AutoBreakLine(text));
                            subtitle.Paragraphs.Add(p);
                            success = true;
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                if (!success)
                {
                    _errorCount++;
                }
            }

            int index = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (next != null && next.StartTime.TotalMilliseconds <= p.EndTime.TotalMilliseconds)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }

                if (p.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                }

                index++;
                p.Number = index;
            }
        }
    }
}
