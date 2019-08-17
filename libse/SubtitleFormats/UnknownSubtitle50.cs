using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    //00.00.05.09-00.00.08.29
    //We don't have a fancy stock abbreviation
    //to go alongside our name in the press.
    //00.00.09.15-00.00.11.09

    //We don't have a profit margin.
    //00.00.11.12-00.00.13.29
    //We don't have sacred rock stars
    //that we put above others.

    public class UnknownSubtitle50 : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeCodes,
            Text1,
            Text2
        }

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\.\d\d\.\d\d\.\d\d-\d\d\.\d\d\.\d\d\.\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 50";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}-{1}\r\n{2}";
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text;
                if (Utilities.GetNumberOfLines(text) > 2)
                {
                    text = Utilities.AutoBreakLine(text);
                }

                text = HtmlUtil.RemoveHtmlTags(text, true);
                if (p.Text.Contains("<i>"))
                {
                    if (Utilities.CountTagInText(p.Text, "<i>") == 1 && Utilities.CountTagInText(p.Text, "</i>") == 1 &&
                        p.Text.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) &&
                        p.Text.EndsWith("</i>", StringComparison.OrdinalIgnoreCase))
                    {
                        text = "||" + text.Replace(Environment.NewLine, "||" + Environment.NewLine + "||") + "||";
                    }
                    else if (Utilities.CountTagInText(p.Text, "<i>") == 2 && Utilities.CountTagInText(p.Text, "</i>") == 2 &&
                             p.Text.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) &&
                             p.Text.EndsWith("</i>", StringComparison.OrdinalIgnoreCase) &&
                             p.Text.Contains("</i>" + Environment.NewLine + "<i>"))
                    {
                        text = "||" + text.Replace(Environment.NewLine, "||" + Environment.NewLine + "||") + "||";
                    }
                }

                if (!text.Contains(Environment.NewLine))
                {
                    text = Environment.NewLine + text;
                }

                sb.AppendLine(string.Format(paragraphWriteFormat, FormatTime(p.StartTime), FormatTime(p.EndTime), text));
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private static string FormatTime(TimeCode timeCode)
        {
            return $"{timeCode.Hours:00}.{timeCode.Minutes:00}.{timeCode.Seconds:00}.{MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = new Paragraph();
            var expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (expecting == ExpectingLine.TimeCodes && RegexTimeCodes.IsMatch(line))
                {
                    if (p.Text.Length > 0 || p.EndTime.TotalMilliseconds > 0.1)
                    {
                        subtitle.Paragraphs.Add(p);
                        p = new Paragraph();
                    }
                    if (TryReadTimeCodesLine(line, p))
                    {
                        expecting = ExpectingLine.Text1;
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (expecting == ExpectingLine.Text1)
                {
                    if (p.Text.Length > 500)
                    {
                        _errorCount += 100;
                        return;
                    }
                    else
                    {
                        if (line.StartsWith("||"))
                        {
                            line = "<i>" + line.Replace("||", string.Empty) + "</i>";
                        }

                        p.Text = line.Trim();
                        expecting = ExpectingLine.Text2;
                    }
                }
                else if (expecting == ExpectingLine.Text2)
                {
                    if (p.Text.Length > 500)
                    {
                        _errorCount += 100;
                        return;
                    }
                    else
                    {
                        if (line.StartsWith("||", StringComparison.Ordinal))
                        {
                            line = "<i>" + line.Replace("||", string.Empty) + "</i>";
                        }

                        p.Text = (p.Text + Environment.NewLine + line).Trim();
                        expecting = ExpectingLine.TimeCodes;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(p.Text))
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            string[] parts = line.Replace("-", ".").Split('.');
            try
            {
                int startHours = int.Parse(parts[0]);
                int startMinutes = int.Parse(parts[1]);
                int startSeconds = int.Parse(parts[2]);
                int startMilliseconds = int.Parse(parts[3]);
                int endHours = int.Parse(parts[4]);
                int endMinutes = int.Parse(parts[5]);
                int endSeconds = int.Parse(parts[6]);
                int endMilliseconds = int.Parse(parts[7]);
                paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);
                paragraph.EndTime = new TimeCode(endHours, endMinutes, endSeconds, endMilliseconds);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
