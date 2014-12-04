using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class SonyDVDArchitectWithLineNumbers : SubtitleFormat
    {
        private static Regex regexTimeCode = new Regex(@"^\d\d\d\d  \d\d:\d\d:\d\d:\d\d  \d\d:\d\d:\d\d:\d\d", RegexOptions.Compiled);
        private static Regex regex1DigitMilliseconds = new Regex(@"^\d\d\d\d  \d\d\d:\d\d:\d\d:\d  \d\d\d:\d\d:\d\d:\d", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".sub"; }
        }

        public override string Name
        {
            get { return "Sony DVDArchitect w. line#"; }
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
                sb.AppendLine(string.Format("{9:0000}  {0:00}:{1:00}:{2:00}:{3:00}  {4:00}:{5:00}:{6:00}:{7:00}    \t{8}" + Environment.NewLine,
                                            p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10,
                                            p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10,
                                            text, p.Number));
            }
            return sb.ToString().Trim() + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {   // 00:04:10:92 - 00:04:13:32    Raise Yourself To Help Mankind
            // 00:04:27:92 - 00:04:30:92    الجهة المتولية للمسئولية الاجتماعية لشركتنا.

            _errorCount = 0;
            Paragraph lastParagraph = null;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                bool success = false;
                if (line.IndexOf(':') > 0)
                {
                    string s = line;
                    var match = regexTimeCode.Match(s);
                    var match1DigitMilliseconds = regex1DigitMilliseconds.Match(s);
                    if (s.Length > 31 && match.Success)
                    {
                        s = s.Substring(5, match.Length - 5).TrimStart();
                        s = s.Replace("  ", ":");
                        s = s.Replace(" ", string.Empty);
                        var parts = s.Split(':');
                        if (parts.Length == 8)
                        {
                            var start = TimeCode.FromTimestampTokens(parts[0], parts[1], parts[2], parts[3]);
                            var end = TimeCode.FromTimestampTokens(parts[4], parts[5], parts[6], parts[7]);

                            string text = line.Replace("\0", string.Empty).Substring(match.Length).TrimStart();
                            text = text.Replace("|", Environment.NewLine);

                            lastParagraph = new Paragraph(start, end, text);
                            subtitle.Paragraphs.Add(lastParagraph);
                            success = true;
                        }
                    }
                    else if (s.Length > 29 && match1DigitMilliseconds.Success)
                    {
                        s = s.Substring(5, match1DigitMilliseconds.Length - 5).TrimStart();
                        s = s.Replace("  ", ":");
                        s = s.Replace(" ", string.Empty);
                        var parts = s.Split(':');
                        if (parts.Length == 8)
                        {
                            var start = TimeCode.FromTimestampTokens(parts[0], parts[1], parts[2], parts[3]);
                            var end = TimeCode.FromTimestampTokens(parts[4], parts[5], parts[6], parts[7]);

                            string text = line.Replace("\0", string.Empty).Substring(match1DigitMilliseconds.Length).TrimStart();
                            text = text.Replace("|", Environment.NewLine);

                            lastParagraph = new Paragraph(start, end, text);
                            subtitle.Paragraphs.Add(lastParagraph);
                            success = true;
                        }
                    }
                    else if (lastParagraph != null && Utilities.CountTagInText(lastParagraph.Text, Environment.NewLine) < 4)
                    {
                        lastParagraph.Text += Environment.NewLine + line.Trim();
                        success = true;
                    }
                }
                else if (lastParagraph != null && Utilities.CountTagInText(lastParagraph.Text, Environment.NewLine) < 4)
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
