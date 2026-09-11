using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle8 : SubtitleFormat
    {
        //00:04:04.219
        //The city council of long beach

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d.\d\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 8";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}\r\n{1}\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendFormat(paragraphWriteFormat, p.StartTime.ToString().Replace(',', '.'), p.Text);
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            // The text line(s) after a time code used to be counted as errors (and only the first
            // one was read), so errors always matched cues and the format could never be detected.
            Paragraph paragraph = null;
            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();
                if (RegexTimeCodes.IsMatch(line))
                {
                    AddIfText(subtitle, paragraph);
                    paragraph = new Paragraph();
                    if (!TryReadTimeCodesLine(line, paragraph))
                    {
                        _errorCount++;
                        paragraph = null;
                    }
                }
                else if (paragraph != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        paragraph.Text = (paragraph.Text + Environment.NewLine + line).Trim();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    _errorCount++;
                }
            }

            AddIfText(subtitle, paragraph);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }

            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                Paragraph nextParagraph = subtitle.GetParagraphOrDefault(index);
                if (nextParagraph != null)
                {
                    p.EndTime.TotalMilliseconds = nextParagraph.StartTime.TotalMilliseconds - 1;
                }
                else
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 2500;
                }

                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }

            subtitle.Renumber();
        }

        private static void AddIfText(Subtitle subtitle, Paragraph paragraph)
        {
            if (paragraph != null && !string.IsNullOrWhiteSpace(paragraph.Text))
            {
                subtitle.Paragraphs.Add(paragraph);
            }
        }

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            string[] parts = line.Split(':', '.');
            try
            {
                int startHours = int.Parse(parts[0]);
                int startMinutes = int.Parse(parts[1]);
                int startSeconds = int.Parse(parts[2]);
                int startMilliseconds = int.Parse(parts[3]);
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
