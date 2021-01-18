using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class YouTubeTranscript : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d{1,3}:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "YouTube Transcript";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0}" + Environment.NewLine + "{1}", EncodeTimeCode(p.StartTime), HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
            }
            return sb.ToString();
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (new UnknownSubtitle88().IsMine(lines, fileName))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours * 60 + time.Minutes}:{time.Seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                var s = line.TrimEnd();
                if (RegexTimeCodes.IsMatch(s))
                {
                    p = new Paragraph(DecodeTimeCode(s), new TimeCode(), string.Empty);
                    subtitle.Paragraphs.Add(p);
                }
                else if (string.IsNullOrWhiteSpace(s))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Text = s;
                    }
                    else
                    {
                        p.Text = p.Text + Environment.NewLine + s;
                    }

                    if (p.Text.Length > 800)
                    {
                        _errorCount++;
                        return;
                    }
                }
            }
            foreach (Paragraph p2 in subtitle.Paragraphs)
            {
                p2.Text = Utilities.AutoBreakLine(p2.Text);
            }
            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds, null, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            string[] parts = s.Split(':');

            var minutes = int.Parse(parts[0]);
            var seconds = int.Parse(parts[1]);

            return new TimeCode(0, minutes, seconds, 0);
        }

    }
}
