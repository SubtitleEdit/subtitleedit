using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle65 : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        public override string Extension => ".txt";

        public override string Name => "Unknown 65";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0:00}:{1:00}:{2:00},{3:00}:{4:00}:{5:00}{6}{7}";

            //00:00:08,00:00:13
            //The 8.7 update will bring the British self-propelled guns, the map, called Severogorsk,

            //00:00:13,00:00:18
            //the soviet light tank MT-25 and the new German premium TD, the E25.

            //00:00:18,00:00:22
            //We will tell you about this and lots of other things in our review.

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text.Replace(Environment.NewLine, " ");

                sb.AppendLine(string.Format(paragraphWriteFormat,
                                        p.StartTime.Hours,
                                        p.StartTime.Minutes,
                                        RoundSeconds(p.StartTime),
                                        p.EndTime.Hours,
                                        p.EndTime.Minutes,
                                        RoundSeconds(p.EndTime),
                                        Environment.NewLine,
                                        text));
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private static int RoundSeconds(TimeCode tc)
        {
            return (int)Math.Round(tc.Seconds + tc.Milliseconds / TimeCode.BaseUnit);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d,\d\d:\d\d:\d\d$", RegexOptions.Compiled);

            var paragraph = new Paragraph();
            ExpectingLine expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (line.Length == 17 && regexTimeCodes.IsMatch(line))
                {
                    string[] parts = line.Split(new[] { ':', ',', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 6)
                    {
                        try
                        {
                            int startHours = int.Parse(parts[0]);
                            int startMinutes = int.Parse(parts[1]);
                            int startSeconds = int.Parse(parts[2]);
                            int endHours = int.Parse(parts[3]);
                            int endMinutes = int.Parse(parts[4]);
                            int endSeconds = int.Parse(parts[5]);
                            paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, 0);
                            paragraph.EndTime = new TimeCode(endHours, endMinutes, endSeconds, 0);
                            expecting = ExpectingLine.Text;
                        }
                        catch
                        {
                            expecting = ExpectingLine.TimeCodes;
                        }
                    }
                }
                else
                {
                    if (expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string text = Utilities.AutoBreakLine(line.Trim());
                            paragraph.Text = text;
                            subtitle.Paragraphs.Add(paragraph);
                            paragraph = new Paragraph();
                            expecting = ExpectingLine.TimeCodes;
                        }
                    }
                }
            }
            subtitle.Renumber();
        }
    }
}
