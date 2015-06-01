using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle1 : SubtitleFormat
    {
        //0:01 – 0:11
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d\d – \d+:\d\d ", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 1"; }
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
            //0:01 – 0:11 "My vengeance needs blood!" -Marquis De Sade
            //[Laughter, thunder]
            //0:17 – 0:19 - On this 5th day of December -
            //0:19 – 0:22 in the year of our Lord 1648, -

            const string paragraphWriteFormat = "{0} – {1} {2}";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                int seconds = p.StartTime.Seconds;
                if (p.StartTime.Milliseconds >= 500)
                    seconds++;
                string startTime = string.Format("{0:0}:{1:00}", (int)(p.StartTime.Minutes + p.StartTime.Hours * 60), seconds);

                seconds = p.EndTime.Seconds;
                if (p.EndTime.Milliseconds >= 500)
                    seconds++;
                string timeOut = string.Format("{0:0}:{1:00}", (int)(p.EndTime.Minutes + p.EndTime.Hours * 60), seconds);

                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, timeOut, p.Text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            var text = new StringBuilder();

            foreach (string line in lines)
            {
                var match = RegexTimeCodes.Match(line);
                if (match.Success)
                {
                    if (p != null)
                        p.Text = (p.Text + Environment.NewLine + text.ToString().Trim()).Trim();
                    var parts = line.Substring(0, match.Length).Trim().Split(new[] { '–', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        p = new Paragraph();
                        p.StartTime = DecodeTimeCode(parts[0]);
                        p.EndTime = DecodeTimeCode(parts[1]);
                        p.Text = line.Substring(match.Length - 1).Trim();
                        subtitle.Paragraphs.Add(p);
                        text = new StringBuilder();
                    }
                    catch
                    {
                        p = null;
                        _errorCount++;
                    }

                }
                else if (p == null)
                {
                    _errorCount++;
                }
                else
                {
                    text.AppendLine(line);
                }
                if (_errorCount > 20)
                    return;
            }
            if (p != null)
                p.Text = (p.Text + Environment.NewLine + text.ToString().Trim()).Trim();

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string code)
        {
            //68:20  (minutes:seconds)
            var parts = code.Trim().Split(':');
            return new TimeCode(0, int.Parse(parts[0]), int.Parse(parts[1]), 0);
        }
    }
}
