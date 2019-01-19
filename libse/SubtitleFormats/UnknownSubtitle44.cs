using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle44 : SubtitleFormat
    {

        //>>> "COMMON GROUND" IS FUNDED BY  10:01:04:12                         1
        //THE MINNESOTA ARTS AND CULTURAL   10:01:07:09
        private static readonly Regex RegexTimeCodes1 = new Regex(@" \d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodes2 = new Regex(@" \d\d:\d\d:\d\d:\d\d         +\d+$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 44";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            int index2 = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                var text = new StringBuilder();
                text.Append(HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " ")));
                while (text.Length < 34)
                {
                    text.Append(' ');
                }

                sb.AppendFormat("{0}{1}", text, EncodeTimeCode(p.StartTime));
                if (index % 50 == 1)
                {
                    index2++;
                    sb.Append(new string(' ', 25) + index2);
                }
                sb.AppendLine();
                Paragraph next = subtitle.GetParagraphOrDefault(index);
                if (next != null && next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > 150)
                {
                    text.Clear();
                    while (text.Length < 34)
                    {
                        text.Append(' ');
                    }

                    sb.AppendLine($"{text}{EncodeTimeCode(p.EndTime)}");
                }
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = RegexTimeCodes2.Match(s);
                if (match.Success)
                {
                    s = s.Substring(0, match.Index + 13).Trim();
                }
                match = RegexTimeCodes1.Match(s);
                if (match.Success && match.Index > 13)
                {
                    string text = s.Substring(0, match.Index).Trim();
                    string timeCode = s.Substring(match.Index).Trim();

                    string[] startParts = timeCode.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4)
                    {
                        try
                        {
                            p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), new TimeCode(), text);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line) || RegexTimeCodes1.IsMatch("   " + s))
                {
                    // skip empty lines
                }
                else if (!string.IsNullOrWhiteSpace(line) && p != null)
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
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
