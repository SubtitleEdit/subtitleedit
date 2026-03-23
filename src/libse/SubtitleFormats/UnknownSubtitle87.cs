using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle87 : SubtitleFormat
    {
        private enum ExpectingLine
        {
            Number,
            TimeCodes,
            Text
        }

        private Paragraph _paragraph;
        private Paragraph _lastParagraph;
        private ExpectingLine _expecting = ExpectingLine.Number;
        private static readonly Regex RegexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+:-?\d\d\d\d\s[–-]\s-?\d+:-?\d+:-?\d+:-?\d\d\d\d", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 87";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}.\r\n{1} – {2}\r\n{3}\r\n\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendFormat(paragraphWriteFormat, p.Number, EncodeTime(p.StartTime), EncodeTime(p.EndTime), p.Text);
            }
            return sb.ToString().Trim() + Environment.NewLine;
        }

        private string EncodeTime(TimeCode tc)
        {
            return $"{tc.Hours:00}:{tc.Minutes:00}:{tc.Seconds:00}:{tc.Milliseconds:000}0";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _paragraph = new Paragraph();
            _expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].TrimEnd();
                line = line.Trim('\u007F'); // 127=delete acscii

                string next = string.Empty;
                if (i + 1 < lines.Count)
                {
                    next = lines[i + 1];
                }

                string nextNext = string.Empty;
                if (i + 2 < lines.Count)
                {
                    nextNext = lines[i + 2];
                }

                // A new line is missing between two paragraphs (buggy srt file)
                if (_expecting == ExpectingLine.Text && i + 1 < lines.Count &&
                    _paragraph != null && !string.IsNullOrEmpty(_paragraph.Text) && Utilities.IsInteger(line.TrimEnd('.')) &&
                    RegexTimeCodes.IsMatch(lines[i + 1]))
                {
                    ReadLine(subtitle, string.Empty, string.Empty, string.Empty);
                }
                if (_expecting == ExpectingLine.Number && RegexTimeCodes.IsMatch(line))
                {
                    _expecting = ExpectingLine.TimeCodes;
                }
                ReadLine(subtitle, line, next, nextNext);
            }
            if (_paragraph != null && _paragraph.EndTime.TotalMilliseconds > _paragraph.StartTime.TotalMilliseconds)
            {
                subtitle.Paragraphs.Add(_paragraph);
            }

            subtitle.Renumber();
        }

        private void ReadLine(Subtitle subtitle, string line, string next, string nextNext)
        {
            switch (_expecting)
            {
                case ExpectingLine.Number:
                    if (int.TryParse(line.TrimEnd('.'), out var number))
                    {
                        _paragraph.Number = number;
                        _expecting = ExpectingLine.TimeCodes;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (_lastParagraph != null && nextNext != null && (_lastParagraph.Number + 1).ToString(CultureInfo.InvariantCulture) == nextNext)
                        {
                            _lastParagraph.Text = (_lastParagraph.Text + Environment.NewLine + line.Trim()).Trim();
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }
                    break;
                case ExpectingLine.TimeCodes:
                    if (TryReadTimeCodesLine(line, _paragraph))
                    {
                        _paragraph.Text = string.Empty;
                        var match = RegexTimeCodes.Match(line);
                        if (match.Success)
                        {
                            var rest = line.Remove(0, match.Length).Trim();
                            if (rest.StartsWith("0 "))
                            {
                                rest = rest.Remove(0, 2);
                            }

                            if (rest.Length > 0 && rest != "0" && rest != ":")
                            {
                                _paragraph.Text = rest;
                            }
                        }
                        _expecting = ExpectingLine.Text;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        _errorCount++;
                        _expecting = ExpectingLine.Number; // lets go to next paragraph
                    }
                    break;
                case ExpectingLine.Text:
                    if (!string.IsNullOrWhiteSpace(line) || IsText(next))
                    {
                        if (_paragraph.Text.Length > 0)
                        {
                            _paragraph.Text += Environment.NewLine;
                        }

                        _paragraph.Text += RemoveBadChars(line).TrimEnd().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }
                    else if (string.IsNullOrEmpty(line) && string.IsNullOrEmpty(_paragraph.Text))
                    {
                        _paragraph.Text = string.Empty;
                        if (!string.IsNullOrEmpty(next) && (Utilities.IsInteger(next.TrimEnd('.')) || RegexTimeCodes.IsMatch(next)))
                        {
                            subtitle.Paragraphs.Add(_paragraph);
                            _lastParagraph = _paragraph;
                            _paragraph = new Paragraph();
                            _expecting = ExpectingLine.Number;
                        }
                    }
                    else
                    {
                        subtitle.Paragraphs.Add(_paragraph);
                        _lastParagraph = _paragraph;
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.Number;
                    }
                    break;
            }
        }

        private static bool IsText(string text)
        {
            return !(string.IsNullOrWhiteSpace(text) || Utilities.IsInteger(text.TrimEnd('.')) || RegexTimeCodes.IsMatch(text));
        }

        private static string RemoveBadChars(string line)
        {
            return line.Replace('\0', ' ');
        }

        private bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            var match = RegexTimeCodes.Match(line);
            if (match.Success)
            {
                line = line.Substring(0, match.Length);
                string[] parts = line.Replace("–", ":").Replace("-", ":").RemoveChar(' ').Split(':', ',');
                try
                {
                    int startHours = int.Parse(parts[0]);
                    int startMinutes = int.Parse(parts[1]);
                    int startSeconds = int.Parse(parts[2]);
                    int startMilliseconds = int.Parse(parts[3]) / 10;
                    int endHours = int.Parse(parts[4]);
                    int endMinutes = int.Parse(parts[5]);
                    int endSeconds = int.Parse(parts[6]);
                    int endMilliseconds = int.Parse(parts[7]) / 10;

                    paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);
                    if (parts[0].StartsWith('-') && paragraph.StartTime.TotalMilliseconds > 0)
                    {
                        paragraph.StartTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds * -1;
                    }

                    paragraph.EndTime = new TimeCode(endHours, endMinutes, endSeconds, endMilliseconds);
                    if (parts[4].StartsWith('-') && paragraph.EndTime.TotalMilliseconds > 0)
                    {
                        paragraph.EndTime.TotalMilliseconds = paragraph.EndTime.TotalMilliseconds * -1;
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}