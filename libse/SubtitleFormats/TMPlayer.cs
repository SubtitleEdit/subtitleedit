using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TMPlayer : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d\d:\d\d[: ].*$", RegexOptions.Compiled); // accept a " " instead of the last ":" too

        public override string Extension => ".txt";

        public override string Name => "TMPlayer";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);

            if (subtitle.Paragraphs.Count > 4)
            {
                bool allStartWithNumber = true;
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    if (p.Text.Length > 1 && !Utilities.IsInteger(p.Text.Substring(0, 2)))
                    {
                        allStartWithNumber = false;
                        break;
                    }
                }
                if (allStartWithNumber)
                {
                    return false;
                }
            }
            if (subtitle.Paragraphs.Count > _errorCount)
            {
                if (new UnknownSubtitle33().IsMine(lines, fileName) || new UnknownSubtitle36().IsMine(lines, fileName))
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, "|");
                sb.AppendLine($"{p.StartTime.Hours:00}:{p.StartTime.Minutes:00}:{p.StartTime.Seconds:00}:{text}");
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        { // 0:02:36:You've returned to the village|after 2 years, Shekhar.
            // 00:00:50:America has made my fortune.
            _errorCount = 0;
            foreach (string line in lines)
            {
                bool success = false;
                if (line.IndexOf(':') > 0 && RegexTimeCodes.Match(line).Success)
                {
                    try
                    {
                        string s = line;
                        if (line.Length > 9 && line[8] == ' ')
                        {
                            s = line.Substring(0, 8) + ":" + line.Substring(9);
                        }

                        string[] parts = s.Split(':');
                        if (parts.Length > 3)
                        {
                            int hours = int.Parse(parts[0]);
                            int minutes = int.Parse(parts[1]);
                            int seconds = int.Parse(parts[2]);
                            string text = string.Empty;
                            for (int i = 3; i < parts.Length; i++)
                            {
                                if (text.Length == 0)
                                {
                                    text = parts[i];
                                }
                                else
                                {
                                    text += ":" + parts[i];
                                }
                            }
                            text = text.Replace("|", Environment.NewLine);
                            var start = new TimeCode(hours, minutes, seconds, 0);
                            double duration = Utilities.GetOptimalDisplayMilliseconds(text);
                            var end = new TimeCode(start.TotalMilliseconds + duration);

                            var p = new Paragraph(start, end, text);
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
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(index + 1);
                if (next != null && next.StartTime.TotalMilliseconds <= p.EndTime.TotalMilliseconds)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }

                index++;
                p.Number = index;
            }
        }
    }
}
