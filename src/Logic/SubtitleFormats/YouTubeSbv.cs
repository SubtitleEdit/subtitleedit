using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class YouTubeSbv : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        private Paragraph _paragraph;
        private ExpectingLine _expecting = ExpectingLine.TimeCodes;
        private static readonly Regex RegexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+[:,.]-?\d+,\d+:-?\d+:-?\d+[:,.]-?\d+$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".sbv"; }
        }

        public override string Name
        {
            get { return "YouTube sbv"; }
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
            const string paragraphWriteFormat = "{0},{1}\r\n{2}\r\n\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendFormat(paragraphWriteFormat, FormatTime(p.StartTime), FormatTime(p.EndTime), p.Text);
            }
            return sb.ToString().Trim();
        }

        private static string FormatTime(TimeCode timeCode)
        {
            return string.Format("{0}:{1:00}:{2:00}.{3:000}", timeCode.Hours, timeCode.Minutes, timeCode.Seconds, timeCode.Milliseconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0:00:07.500,0:00:13.500
            //In den Bergen über Musanze in Ruanda feiert die Trustbank (Kreditnehmer-Gruppe)  "Trususanze" ihren Erfolg.

            //0:00:14.000,0:00:17.000
            //Indem sie ihre Zukunft einander anvertraut haben, haben sie sich

            _paragraph = new Paragraph();
            _expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].TrimEnd();
                string next = string.Empty;
                if (i + 1 < lines.Count)
                    next = lines[i + 1];

                // A new line is missing between two paragraphs (buggy srt file)
                if (_expecting == ExpectingLine.Text && i + 1 < lines.Count &&
                    _paragraph != null && !string.IsNullOrEmpty(_paragraph.Text) &&
                    RegexTimeCodes.IsMatch(lines[i]))
                {
                    ReadLine(subtitle, string.Empty, string.Empty);
                }

                ReadLine(subtitle, line, next);
            }
            if (!string.IsNullOrWhiteSpace(_paragraph.Text))
                subtitle.Paragraphs.Add(_paragraph);

            foreach (Paragraph p in subtitle.Paragraphs)
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            subtitle.Renumber(1);
        }

        private void ReadLine(Subtitle subtitle, string line, string next)
        {
            switch (_expecting)
            {
                case ExpectingLine.TimeCodes:
                    if (TryReadTimeCodesLine(line, _paragraph))
                    {
                        _paragraph.Text = string.Empty;
                        _expecting = ExpectingLine.Text;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        _errorCount++;
                        _expecting = ExpectingLine.TimeCodes; // lets go to next paragraph
                    }
                    break;
                case ExpectingLine.Text:
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (_paragraph.Text.Length > 0)
                            _paragraph.Text += Environment.NewLine;
                        _paragraph.Text += RemoveBadChars(line).TrimEnd();
                    }
                    else if (IsText(next))
                    {
                        if (_paragraph.Text.Length > 0)
                            _paragraph.Text += Environment.NewLine;
                        _paragraph.Text += RemoveBadChars(line).TrimEnd();
                    }
                    else
                    {
                        subtitle.Paragraphs.Add(_paragraph);
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.TimeCodes;
                    }
                    break;
            }
        }

        private static bool IsText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (Utilities.IsInteger(text))
                return false;

            if (RegexTimeCodes.IsMatch(text))
                return false;

            return true;
        }

        private static string RemoveBadChars(string line)
        {
            line = line.Replace("\0", " ");

            return line;
        }

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            line = line.Replace(".", ":");
            line = line.Replace("،", ",");
            line = line.Replace("¡", ":");

            if (RegexTimeCodes.IsMatch(line))
            {
                var parts = line.Split(':', ',');
                paragraph.StartTime = TimeCode.FromTimestampTokens(parts[0], parts[1], parts[2], parts[3]);
                paragraph.EndTime = TimeCode.FromTimestampTokens(parts[4], parts[5], parts[6], parts[7]);
                return true;
            }
            return false;
        }
    }
}
