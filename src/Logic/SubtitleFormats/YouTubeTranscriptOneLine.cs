using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class YouTubeTranscriptOneLine : SubtitleFormat
    {
        private static readonly Regex regexTimeCodes = new Regex(@"^\d{1,3}:\d\d.+$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "YouTube Transcript one line"; }
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
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0}{1}", EncodeTimeCode(p.StartTime), HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
                index++;
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
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (regexTimeCodes.IsMatch(line))
                {
                    int splitter = line.IndexOf(':') + 3;
                    string text = line.Remove(0, splitter);
                    var p = new Paragraph(DecodeTimeCode(line.Substring(0, splitter)), new TimeCode(0, 0, 0, 0), text);
                    subtitle.Paragraphs.Add(p);
                    text = text.Trim().Trim('–', '.', ';', ':').Trim();
                    if (text.Length > 0 && char.IsDigit(text[0]))
                        _errorCount++;
                }
                else
                {
                    _errorCount += 2;
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

            string minutes = parts[0];
            string seconds = parts[1];

            return new TimeCode(0, int.Parse(minutes), int.Parse(seconds), 0);
        }

    }
}
