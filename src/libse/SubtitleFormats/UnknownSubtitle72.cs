using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle72 : SubtitleFormat
    {
        //00:00:02.000
        //Junior Semifinal, part 1
        //Aidiba Talamunuer, Berezan
        //Bogdan Voloshin, Yaroslavl
        //Alexandr Doronin, Almaty

        //00:04:41.480
        //G. Zhubanova
        //«Kui»
        //Aidiba Talamunuer, Berezan

        //00:05:55.000
        //N. Mendigaliev
        //«Steppe»
        //Bogdan Voloshin, Yaroslavl

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d.\d{1,3}$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 72";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}\r\n{1}\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, p.StartTime.ToString().Replace(",", "."), p.Text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].TrimEnd();

                if (line.Contains(':') && RegexTimeCodes.IsMatch(line))
                {
                    if (paragraph != null && string.IsNullOrEmpty(paragraph.Text))
                    {
                        _errorCount++;
                    }

                    paragraph = new Paragraph();
                    if (TryReadTimeCodesLine(line, paragraph))
                    {
                        subtitle.Paragraphs.Add(paragraph);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (paragraph != null && paragraph.Text.Length < 1000)
                {
                    paragraph.Text = (paragraph.Text + Environment.NewLine + line).Trim();
                }
                else
                {
                    _errorCount++;
                }
            }

            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                Paragraph nextParagraph = subtitle.GetParagraphOrDefault(index);
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text) + 100;
                if (nextParagraph != null && p.EndTime.TotalMilliseconds >= nextParagraph.StartTime.TotalMilliseconds)
                {
                    p.EndTime.TotalMilliseconds = nextParagraph.StartTime.TotalMilliseconds - 1;
                }
            }
            subtitle.Renumber();
        }

        private bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            string[] parts = line.Split(':', '.');
            try
            {
                int startHours = int.Parse(parts[0]);
                int startMinutes = int.Parse(parts[1]);
                int startSeconds = int.Parse(parts[2]);
                int startMilliseconds = int.Parse(parts[3]);

                if (parts[3].Length == 2)
                {
                    _errorCount++;
                }

                paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
