using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SonyDVDArchitectTabs : SubtitleFormat
    {
        private static readonly Regex Regex = new Regex(@"^\d\d:\d\d:\d\d:\d\d[ \t]+\d\d:\d\d:\d\d:\d\d[ \t]+", RegexOptions.Compiled);

        public override string Extension => ".sub";

        public override string Name => "Sony DVDArchitect Tabs";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            const string writeFormat = "{0:00}:{1:00}:{2:00}:{3:00}\t{4:00}:{5:00}:{6:00}:{7:00}\t{8}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, "\r");
                sb.AppendLine(string.Format(writeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10,
                                            p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10,
                                            text));
            }
            return sb.ToString().Trim() + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {   //00:02:09:34   00:02:13:07 - Hvad mener du så om konkurrencen?- Jo, det er helt fint.
            //00:02:14:02   00:02:16:41 - Var det den rigtige der vandt?- Ja, bestemt.
            //newline = \r (0D)

            _errorCount = 0;
            Paragraph lastParagraph = null;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                bool success = false;
                bool isTimeCode = false;
                if (line.Length > 26 && line.IndexOf(':') == 2)
                {
                    var match = Regex.Match(line);
                    if (match.Success)
                    {
                        isTimeCode = true;
                        string s = line.Substring(0, match.Length);
                        s = s.Replace('\t', ':');
                        s = s.RemoveChar(' ');
                        s = s.Trim().TrimEnd(':').TrimEnd();
                        string[] parts = s.Split(':');
                        if (parts.Length == 8)
                        {
                            int hours = int.Parse(parts[0]);
                            int minutes = int.Parse(parts[1]);
                            int seconds = int.Parse(parts[2]);
                            int milliseconds = int.Parse(parts[3]) * 10;
                            var start = new TimeCode(hours, minutes, seconds, milliseconds);

                            hours = int.Parse(parts[4]);
                            minutes = int.Parse(parts[5]);
                            seconds = int.Parse(parts[6]);
                            milliseconds = int.Parse(parts[7]) * 10;
                            var end = new TimeCode(hours, minutes, seconds, milliseconds);

                            string text = line.Substring(match.Length).TrimStart();
                            text = text.Replace("|", Environment.NewLine);

                            lastParagraph = new Paragraph(start, end, text);
                            subtitle.Paragraphs.Add(lastParagraph);
                            success = true;
                        }
                    }
                }
                if (!isTimeCode && lastParagraph != null && Utilities.GetNumberOfLines(lastParagraph.Text) < 5)
                {
                    lastParagraph.Text += Environment.NewLine + line.Trim();
                    success = true;
                }
                if (!success)
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }
    }
}
