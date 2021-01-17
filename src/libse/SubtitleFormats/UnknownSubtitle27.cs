using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle27 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:  ", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 27";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:18:02:  (斉藤)失礼な大人って！  (悠子)何言ってんのあんた？
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, "  ");
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}  {text}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            int sec = time.Seconds;
            if (time.Milliseconds >= 500)
            {
                sec++;
            }

            return $"{time.Hours:00}:{time.Minutes:00}:{sec:00}:";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //  00:18:02:  (斉藤)失礼な大人って！  (悠子)何言ってんのあんた？
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCodes.IsMatch(s))
                {
                    var temp = s.Substring(0, 8);

                    string[] startParts = temp.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 3)
                    {
                        try
                        {
                            string text = s.Remove(0, 10).Trim();
                            text = text.Replace("  ", Environment.NewLine);
                            p = new Paragraph(DecodeTimeCode(startParts), new TimeCode(), text);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip empty lines
                }
                else if (p != null)
                {
                    _errorCount++;
                }
            }

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph current = subtitle.Paragraphs[i];
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null)
                {
                    current.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
                else
                {
                    current.EndTime.TotalMilliseconds = current.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(current.Text);
                }

                if (current.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    current.EndTime.TotalMilliseconds = current.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), 0);
        }

    }
}
