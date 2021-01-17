using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle1 : SubtitleFormat
    {
        //0:01 – 0:11
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d\d – \d+:\d\d ", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 1";

        public override string ToText(Subtitle subtitle, string title)
        {
            //0:01 – 0:11 "My vengeance needs blood!" -Marquis De Sade
            //[Laughter, thunder]
            //0:17 – 0:19 - On this 5th day of December -
            //0:19 – 0:22 in the year of our Lord 1648, -

            const string paragraphWriteFormat = "{0} – {1} {2}";

            var sb = new StringBuilder();
            const string format = "{0:0}:{1:00}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                int seconds = p.StartTime.Seconds;
                if (p.StartTime.Milliseconds >= 500)
                {
                    seconds++;
                }

                string startTime = string.Format(format, p.StartTime.Minutes + p.StartTime.Hours * 60, seconds);

                seconds = p.EndTime.Seconds;
                if (p.EndTime.Milliseconds >= 500)
                {
                    seconds++;
                }

                string timeOut = string.Format(format, p.EndTime.Minutes + p.EndTime.Hours * 60, seconds);

                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, timeOut, p.Text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            var sb = new StringBuilder();
            char[] splitChars = { '–', ' ' };
            foreach (string line in lines)
            {
                Match match;
                if (line.Length > 11 && (match = RegexTimeCodes.Match(line)).Success)
                {
                    if (p != null)
                    {
                        p.Text = (p.Text + Environment.NewLine + sb).Trim();
                    }

                    var parts = line.Substring(0, match.Length).Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        p = new Paragraph { StartTime = DecodeTimeCode(parts[0]), EndTime = DecodeTimeCode(parts[1]) };
                        p.Text = line.Substring(match.Length).Trim();
                        subtitle.Paragraphs.Add(p);
                        sb.Clear();
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
                    sb.AppendLine(line);
                }
                if (_errorCount > 20)
                {
                    return;
                }
            }
            if (p != null)
            {
                p.Text = (p.Text + Environment.NewLine + sb).Trim();
            }

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
