using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class YouTubeTranscript : SubtitleFormat
    {
        static Regex regexTimeCodes = new Regex(@"^\d{1,2}:\d\d$", RegexOptions.Compiled);

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
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0}" + Environment.NewLine + "{1}", EncodeTimeCode(p.StartTime), Utilities.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
                index++;
            }
            return sb.ToString();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0}:{1:00}", time.Hours / 60 + time.Minutes , time.Seconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (regexTimeCodes.IsMatch(line))
                {
                    p = new Paragraph(DecodeTimeCode(line), new TimeCode(0,0,0,0), string.Empty);
                    subtitle.Paragraphs.Add(p);
                }
                else if (line.Trim().Length == 0)
                {
                    // skip these lines
                }
                else if (line.Trim().Length > 0 && p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line;
                    else
                        p.Text = p.Text + Environment.NewLine + line;
                }
            }
            foreach (Paragraph p2 in subtitle.Paragraphs)
            {
                p2.Text = Utilities.AutoBreakLine(p2.Text);
            }
            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds, null);
            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string s)
        {
            string[] parts = s.Split(':');

            string minutes = parts[0];
            string seconds = parts[1];

            return new TimeCode(0, int.Parse(minutes), int.Parse(seconds), 0);
        }

    }
}

