using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// YouTube "SubViewer" format... I think YouTube tried to add "SubViewer 2.0" support but instread they created their own format... nice ;)
    /// </summary>
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

        public override string Extension => ".sbv";

        public override string Name => "YouTube sbv";

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
            return $"{timeCode.Hours}:{timeCode.Minutes:00}:{timeCode.Seconds:00}.{timeCode.Milliseconds:000}";
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
                {
                    next = lines[i + 1];
                }

                // A new line is missing between two paragraphs (buggy srt file)
                if (_expecting == ExpectingLine.Text && i + 1 < lines.Count &&
                    _paragraph != null && !string.IsNullOrEmpty(_paragraph.Text) &&
                    RegexTimeCodes.IsMatch(lines[i]))
                {
                    ReadLine(subtitle, string.Empty, string.Empty);
                }

                ReadLine(subtitle, line, next);
            }
            if (_paragraph != null && !string.IsNullOrWhiteSpace(_paragraph.Text))
            {
                subtitle.Paragraphs.Add(_paragraph);
            }

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }

            subtitle.Renumber();
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
                    }
                    break;
                case ExpectingLine.Text:
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (_paragraph.Text.Length > 0)
                        {
                            _paragraph.Text += Environment.NewLine;
                        }

                        _paragraph.Text += RemoveBadChars(line).TrimEnd();
                    }
                    else if (IsText(next))
                    {
                        if (_paragraph.Text.Length > 0)
                        {
                            _paragraph.Text += Environment.NewLine;
                        }

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
            if (string.IsNullOrWhiteSpace(text) || Utilities.IsInteger(text) || RegexTimeCodes.IsMatch(text))
            {
                return false;
            }

            return true;
        }

        private static string RemoveBadChars(string line)
        {
            return line.Replace('\0', ' ');
        }

        private static bool TryReadTimeCodesLine(string inputLine, Paragraph paragraph)
        {
            var line = inputLine.Replace('.', ':').Replace('،', ',').Replace('¡', ':');

            if (RegexTimeCodes.IsMatch(line))
            {
                line = line.Replace(',', ':');
                string[] parts = line.RemoveChar(' ').Split(':', ',');
                try
                {
                    int startHours = int.Parse(parts[0]);
                    int startMinutes = int.Parse(parts[1]);
                    int startSeconds = int.Parse(parts[2]);
                    int startMilliseconds = int.Parse(parts[3]);
                    int endHours = int.Parse(parts[4]);
                    int endMinutes = int.Parse(parts[5]);
                    int endSeconds = int.Parse(parts[6]);
                    int endMilliseconds = int.Parse(parts[7]);
                    paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);
                    paragraph.EndTime = new TimeCode(endHours, endMinutes, endSeconds, endMilliseconds);
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
