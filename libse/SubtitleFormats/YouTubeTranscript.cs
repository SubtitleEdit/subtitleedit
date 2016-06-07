using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class YouTubeTranscript : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d{1,3}:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "YouTube Transcript"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0}" + Environment.NewLine + "{1}", EncodeTimeCode(p.StartTime), HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0}:{1:00}", time.Hours * 60 + time.Minutes, time.Seconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    p = new Paragraph(DecodeTimeCode(line), new TimeCode(0, 0, 0, 0), string.Empty);
                    subtitle.Paragraphs.Add(p);
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line;
                    else
                        p.Text = p.Text + Environment.NewLine + line;

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
            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds, null);
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
