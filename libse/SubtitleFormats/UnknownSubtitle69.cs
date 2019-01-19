using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle69 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d+\) \d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d Durée : \d\d:\d\d", RegexOptions.Compiled); //10:00:02F00

        public override string Extension => ".txt";

        public override string Name => "Unknown 69";

        public override string ToText(Subtitle subtitle, string title)
        {
            //1) 00:00:06:14 00:00:07:07 Durée : 00:18 Lisibilité : 011 Intervalle : 06:14 Nbc : 018
            //text
            //line2

            //2) 00:00:07:14 00:00:09:02 Durée : 01:13 Lisibilité : 023 Intervalle : 00:07 Nbc : 026
            //text
            var sb = new StringBuilder();
            string paragraphWriteFormat = "{0}) {1} {2} Durée : {3} Lisibilité : {4} Intervalle : {5} Nbc : {6}" + Environment.NewLine + "{7}";
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                string start = p.StartTime.ToHHMMSSFF();
                string end = p.EndTime.ToHHMMSSFF();
                string duration = $"{p.Duration.Seconds:00}:{MillisecondsToFramesMaxFrameRate(p.Duration.Milliseconds):00}";
                const string readability = "011";
                const string interval = "06:14";
                string nbc = text.Length.ToString().PadLeft(3, '0');
                sb.AppendLine(string.Format(paragraphWriteFormat, count, start, end, duration, readability, interval, nbc, text));
                sb.AppendLine();
                count++;
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var text = new StringBuilder();
            Paragraph p = null;
            char[] splitChars = { ':', 'F' };
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                {
                    if (p != null)
                    {
                        p.Text = text.ToString().Trim();
                    }
                }
                else if (RegexTimeCode.IsMatch(line))
                {
                    var timeParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (timeParts.Length > 4)
                    {
                        try
                        {
                            string start = timeParts[1];
                            string end = timeParts[2];
                            p = new Paragraph();
                            p.StartTime = DecodeTimeCodeFrames(start.Substring(0, 11), splitChars);
                            p.EndTime = DecodeTimeCodeFrames(end.Substring(0, 11), splitChars);
                            subtitle.Paragraphs.Add(p);
                            text.Clear();
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else
                {
                    text.AppendLine(line);
                    if (text.Length > 5000)
                    {
                        return;
                    }
                }
            }
            if (p != null)
            {
                p.Text = text.ToString().Trim();
            }

            subtitle.Renumber();
        }

    }
}
