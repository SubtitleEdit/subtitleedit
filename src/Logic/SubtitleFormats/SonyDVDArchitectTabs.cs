using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class SonyDVDArchitectTabs : SubtitleFormat
    {
        private static Regex regex = new Regex(@"^\d\d:\d\d:\d\d:\d\d[ \t]+\d\d:\d\d:\d\d:\d\d[ \t]+", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".sub"; }
        }

        public override string Name
        {
            get { return "Sony DVDArchitect Tabs"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = Utilities.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, "\r");
                sb.AppendLine(string.Format("{0:00}:{1:00}:{2:00}:{3:00}\t{4:00}:{5:00}:{6:00}:{7:00}\t{8}",
                                            p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10,
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
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        isTimeCode = true;
                        string s = line.Substring(0, match.Length);
                        s = s.Replace("\t", ":");
                        s = s.Replace(" ", string.Empty);
                        s = s.Trim().TrimEnd(':').TrimEnd();
                        string[] parts = s.Split(':');
                        if (parts.Length == 8)
                        {
                            var start = TimeCode.FromTimestampTokens(parts);
                            var end = TimeCode.FromTimestampTokens(parts[4], parts[5], parts[6], parts[7]);

                            string text = line.Substring(match.Length).TrimStart();
                            text = text.Replace("|", Environment.NewLine);

                            lastParagraph = new Paragraph(start, end, text);
                            subtitle.Paragraphs.Add(lastParagraph);
                            success = true;
                        }
                    }
                }
                if (!isTimeCode && lastParagraph != null && Utilities.CountTagInText(lastParagraph.Text, Environment.NewLine) < 4)
                {
                    lastParagraph.Text += Environment.NewLine + line.Trim();
                    success = true;
                }
                if (!success)
                    _errorCount++;
            }
            subtitle.Renumber(1);
        }
    }
}
