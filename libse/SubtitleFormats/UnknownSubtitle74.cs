using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle74 : SubtitleFormat
    {
        //07:02:27
        //>> GOOD MORNING AND WELCOME TO THE FALL 2014 COMMENCEMENT CEREMONY, A TIME TO RECOGNIZE OUR GRADUATING SENIORS.
        //07:02:43
        //DURING YOUR TIME HERE, MICHIGAN STATE UNIVERSITY HAS POLLEDLY RECAST ITS LAND-GRANT MISSION TO MEET NEW CHALLENGES AND OPPORTUNITIES AND TO INNOVATE OUR FUTURE.
        //07:02:54
        //OUR PLANS AND ACTIONS STEM FROM OUR CORE INTERWOVEN VALUES OF QUALITY, INCLUSION AND CONNECTIVITY.
        //07:03:02

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 74";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}\r\n{1}\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendFormat(paragraphWriteFormat, GetTimeCode(p), p.Text);
            }
            return sb.ToString().Trim();
        }

        private static string GetTimeCode(Paragraph p)
        {
            int seconds = p.StartTime.Seconds;
            if (p.StartTime.Milliseconds >= 500)
            {
                seconds++;
            }

            return $"{p.StartTime.Hours:00}:{p.StartTime.Minutes:00}:{seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            int i = 0;
            Paragraph paragraph = null;
            while (i < lines.Count)
            {
                string line = lines[i].TrimEnd();
                string next = string.Empty;
                if (i + 1 < lines.Count)
                {
                    next = lines[i + 1];
                }

                if (line.Length == 8 && line[2] == ':' && RegexTimeCodes.IsMatch(line) && !RegexTimeCodes.IsMatch(next))
                {
                    paragraph = new Paragraph();
                    if (TryReadTimeCodesLine(line, paragraph))
                    {
                        paragraph.Text = next;
                        if (!string.IsNullOrWhiteSpace(paragraph.Text))
                        {
                            subtitle.Paragraphs.Add(paragraph);
                            i++;
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    if (paragraph != null && paragraph.Text.Length < 500)
                    {
                        paragraph.Text = paragraph.Text + Environment.NewLine + line;
                    }
                    else
                    {
                        _errorCount++;
                        return;
                    }
                }
                i++;
            }

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

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            string[] parts = line.Split(':');
            try
            {
                int startHours = int.Parse(parts[0]);
                int startMinutes = int.Parse(parts[1]);
                int startSeconds = int.Parse(parts[2]);
                paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
