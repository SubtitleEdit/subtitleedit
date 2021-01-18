using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SubViewer10 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\[\d\d:\d\d:\d\d\]$", RegexOptions.Compiled);

        private enum ExpectingLine
        {
            TimeStart,
            Text,
            TimeEnd,
        }

        public override string Extension => ".sub";

        public override string Name => "SubViewer 1.0";

        public override string ToText(Subtitle subtitle, string title)
        {
            //[00:02:14]
            //Yes a new line|Line number 2
            //[00:02:15]
            string paragraphWriteFormat = "[{0:00}:{1:00}:{2:00}]" + Environment.NewLine +
                                          "{3}" + Environment.NewLine +
                                          "[{4:00}:{5:00}:{6:00}]";
            const string header = @"[TITLE]
{0}
[AUTHOR]
[SOURCE]
[PRG]
[FILEPATH]
[DELAY]
0
[CD TRACK]
0
[BEGIN]
******** START SCRIPT ********
";
            const string footer = @"[end]
******** END SCRIPT ********
";
            var sb = new StringBuilder();
            sb.AppendFormat(header, title);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "|"));

                sb.AppendLine(string.Format(paragraphWriteFormat,
                                        p.StartTime.Hours,
                                        p.StartTime.Minutes,
                                        p.StartTime.Seconds,
                                        text,
                                        p.EndTime.Hours,
                                        p.EndTime.Minutes,
                                        p.EndTime.Seconds));
                sb.AppendLine();
            }
            sb.Append(footer);
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            ExpectingLine expecting = ExpectingLine.TimeStart;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            char[] splitChars = { ':', ']', '[', ' ' };
            foreach (string line in lines)
            {
                if (line.StartsWith('[') && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        try
                        {
                            int startHours = int.Parse(parts[0]);
                            int startMinutes = int.Parse(parts[1]);
                            int startSeconds = int.Parse(parts[2]);
                            var tc = new TimeCode(startHours, startMinutes, startSeconds, 0);
                            if (expecting == ExpectingLine.TimeStart)
                            {
                                paragraph = new Paragraph();
                                paragraph.StartTime = tc;
                                expecting = ExpectingLine.Text;
                            }
                            else if (expecting == ExpectingLine.TimeEnd)
                            {
                                paragraph.EndTime = tc;
                                expecting = ExpectingLine.TimeStart;
                                subtitle.Paragraphs.Add(paragraph);
                                paragraph = new Paragraph();
                            }
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.TimeStart;
                        }
                    }
                }
                else
                {
                    if (expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string text = line.Replace("|", Environment.NewLine);
                            paragraph.Text = text;
                            expecting = ExpectingLine.TimeEnd;
                        }
                    }
                }
            }
            subtitle.Renumber();
        }
    }
}
