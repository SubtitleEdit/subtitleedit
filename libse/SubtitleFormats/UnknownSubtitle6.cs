using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle6 : SubtitleFormat
    {
        private static readonly Regex RegexBeforeText = new Regex(@"^\d\s+\d\s+\d\s+\d\s+\d\s+\d$", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+\s+\d+$", RegexOptions.Compiled);

        private enum ExpectingLine
        {
            TimeCodes,
            BeforeText,
            Text
        }

        public override string Extension
        {
            get { return ".titl"; }
        }

        public override string Name
        {
            get { return "Unknown 6"; }
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
            var sb = new StringBuilder();

            sb.Append(' ');
            sb.Append(subtitle.Paragraphs.Count);
            sb.AppendLine("           4             1234 ");
            sb.AppendLine(@"NORMAL
00:00:00.00

SRPSKI

00:00:00.00
26.11.2008  18:54:15");

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string firstLine = string.Empty;
                string secondLine = string.Empty;
                var lines = p.Text.SplitToLines();
                if (lines.Length > 2)
                {
                    lines = Utilities.AutoBreakLine(p.Text).SplitToLines();
                }
                if (lines.Length > 0)
                    firstLine = lines[0];
                if (lines.Length > 1)
                    secondLine = lines[1];

                sb.AppendLine(string.Format(" {0}          {1} " + Environment.NewLine +
                                            "1    0    0    0    0    0" + Environment.NewLine +
                                            "{2}" + Environment.NewLine +
                                            "{3}", p.StartTime.TotalMilliseconds / 10, p.EndTime.TotalMilliseconds / 10, firstLine, secondLine));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            ExpectingLine expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCodes.IsMatch(s))
                {
                    if (!string.IsNullOrEmpty(paragraph.Text))
                        subtitle.Paragraphs.Add(paragraph);

                    paragraph = new Paragraph();
                    string[] parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        try
                        {
                            paragraph.StartTime.TotalMilliseconds = long.Parse(parts[0]) * 10;
                            paragraph.EndTime.TotalMilliseconds = long.Parse(parts[1]) * 10;
                            expecting = ExpectingLine.BeforeText;
                        }
                        catch
                        {
                            expecting = ExpectingLine.TimeCodes;
                        }
                    }
                }
                else if (RegexBeforeText.IsMatch(s))
                {
                    expecting = ExpectingLine.Text;
                }
                else
                {
                    if (expecting == ExpectingLine.Text)
                    {
                        if (s.Length > 0)
                        {
                            if (!string.IsNullOrEmpty(paragraph.Text))
                                paragraph.Text += Environment.NewLine + s;
                            else
                                paragraph.Text = s;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(paragraph.Text))
                subtitle.Paragraphs.Add(paragraph);
            subtitle.Renumber();
        }
    }
}
