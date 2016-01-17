using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DigiBeta : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d \d\d \d\d \d\d\t\d\d \d\d \d\d \d\d\t", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "DigiBeta"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //10 01 37 23   10 01 42 01 Makkhi  (newline is TAB)
            const string paragraphWriteFormat = "{0}\t{1}\t{2}";

            StringBuilder sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text.Replace(Environment.NewLine, "\t")));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (RegexTimeCode.IsMatch(line) && line.Length > 24)
                {
                    string[] parts = line.Substring(0, 11).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var start = DecodeTimeCode(parts);
                            parts = line.Substring(12, 11).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var end = DecodeTimeCode(parts);
                            paragraph = new Paragraph();
                            paragraph.StartTime = start;
                            paragraph.EndTime = end;

                            paragraph.Text = line.Substring(24).Trim().Replace("\t", Environment.NewLine);
                            subtitle.Paragraphs.Add(paragraph);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00} {1:00} {2:00} {3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

    }
}