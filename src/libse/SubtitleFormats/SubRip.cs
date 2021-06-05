using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SubRip : SubtitleFormat
    {
        public string Errors { get; private set; }
        private StringBuilder _errors;
        private int _lineNumber;
        private bool _isMsFrames;
        private bool _isWsrt;
        private static Regex _regExWsrtItalicStart;
        private static Regex _regExWsrtItalicEnd;

        private enum ExpectingLine
        {
            Number,
            TimeCodes,
            Text
        }

        private Paragraph _paragraph;
        private Paragraph _lastParagraph;
        private ExpectingLine _expecting;

        public override string Extension => ".srt";

        public const string NameOfFormat = "SubRip";

        public override string Name => NameOfFormat;

        public override List<string> AlternateExtensions => new List<string> { ".wsrt" };

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0].StartsWith("WEBVTT", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            Errors = null;
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}{4}{1} --> {2}{4}{3}{4}{4}";

            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendFormat(paragraphWriteFormat, p.Number, p.StartTime, p.EndTime, p.Text, Environment.NewLine);
            }
            return sb.ToString().Trim() + Environment.NewLine + Environment.NewLine;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var doRenumber = false;
            _errors = new StringBuilder();
            _lineNumber = 0;
            _isMsFrames = true;
            _isWsrt = fileName != null && fileName.EndsWith(".wsrt", StringComparison.OrdinalIgnoreCase);
            _paragraph = new Paragraph();
            _expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            for (var i = 0; i < lines.Count; i++)
            {
                _lineNumber++;
                var line = lines[i].TrimEnd();
                line = line.Trim('\u007F'); // 127=delete ascii

                var next = string.Empty;
                if (i + 1 < lines.Count)
                {
                    next = lines[i + 1];
                }

                var nextNext = string.Empty;
                if (i + 2 < lines.Count)
                {
                    nextNext = lines[i + 2];
                }

                var nextNextNext = string.Empty;
                if (i + 3 < lines.Count)
                {
                    nextNextNext = lines[i + 3];
                }

                // A new line is missing between two paragraphs or no line number (buggy file)
                if (_expecting == ExpectingLine.Text && i + 1 < lines.Count && !string.IsNullOrEmpty(_paragraph?.Text) &&
                    Utilities.IsInteger(line) && TryReadTimeCodesLine(line.Trim(), null, false))
                {
                    if (!string.IsNullOrEmpty(_paragraph.Text))
                    {
                        subtitle.Paragraphs.Add(_paragraph);
                        _lastParagraph = _paragraph;
                        _paragraph = new Paragraph();
                    }
                    _expecting = ExpectingLine.Number;
                }
                if (_expecting == ExpectingLine.Number && TryReadTimeCodesLine(line.Trim(), null, false))
                {
                    _expecting = ExpectingLine.TimeCodes;
                    doRenumber = true;
                }
                else if (!string.IsNullOrEmpty(_paragraph?.Text) && _expecting == ExpectingLine.Text && TryReadTimeCodesLine(line.Trim(), null, false))
                {
                    subtitle.Paragraphs.Add(_paragraph);
                    _lastParagraph = _paragraph;
                    _paragraph = new Paragraph();
                    _expecting = ExpectingLine.TimeCodes;
                    doRenumber = true;
                }

                ReadLine(subtitle, line, next, nextNext, nextNextNext);
            }

            if (_paragraph?.IsDefault == false || _paragraph != null && _expecting == ExpectingLine.Text)
            {
                subtitle.Paragraphs.Add(_paragraph);
            }

            if (doRenumber)
            {
                subtitle.Renumber();
            }

            foreach (var p in subtitle.Paragraphs)
            {
                if (_isMsFrames)
                {
                    p.StartTime.Milliseconds = FramesToMillisecondsMax999(p.StartTime.Milliseconds);
                    p.EndTime.Milliseconds = FramesToMillisecondsMax999(p.EndTime.Milliseconds);
                }
                p.Text = p.Text.TrimEnd();
            }

            Errors = _errors.ToString();
        }

        private void ReadLine(Subtitle subtitle, string line, string next, string nextNext, string nextNextNext)
        {
            switch (_expecting)
            {
                case ExpectingLine.Number:
                    if (int.TryParse(line, out var number))
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
                            if (_errors.Length < 2000)
                            {
                                _errors.AppendLine(string.Format(FormatLanguage.LineNumberXExpectedNumberFromSourceLineY, _lineNumber, line));
                            }

                            _errorCount++;
                        }
                    }
                    break;
                case ExpectingLine.TimeCodes:
                    if (TryReadTimeCodesLine(line, _paragraph, true))
                    {
                        _paragraph.Text = string.Empty;
                        _expecting = ExpectingLine.Text;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (_errors.Length < 2000)
                        {
                            _errors.AppendLine(string.Format(FormatLanguage.LineNumberXErrorReadingTimeCodeFromSourceLineY, _lineNumber, line));
                        }

                        _errorCount++;
                        _expecting = ExpectingLine.Number; // lets go to next paragraph
                    }
                    break;
                case ExpectingLine.Text:
                    if (Utilities.IsInteger(line) && (TryReadTimeCodesLine(next, null, false) || string.IsNullOrEmpty(next) && TryReadTimeCodesLine(nextNext, null, false)))
                    {
                        // line is integer and time code follows - could loose a number from text...
                        subtitle.Paragraphs.Add(_paragraph);
                        _lastParagraph = _paragraph;
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.Number;
                        if (int.TryParse(line, out var n))
                        {
                            _paragraph.Number = n;
                        }
                        if (_errors.Length < 2000 && line.Trim() != GetLastNumber(_paragraph))
                        {
                            _errors.AppendLine(string.Format(FormatLanguage.LineNumberXExpectedEmptyLine, _lineNumber, line));
                        }
                    }
                    else if (TryReadTimeCodesLine(line, null, false))
                    {
                        if (_paragraph != null && _paragraph.EndTime.TotalMilliseconds > 0 ||
                            _paragraph != null && !string.IsNullOrEmpty(_paragraph.Text))
                        {
                            subtitle.Paragraphs.Add(_paragraph);
                            _lastParagraph = _paragraph;
                        }
                        _paragraph = new Paragraph();
                        TryReadTimeCodesLine(line, _paragraph, false);
                        _expecting = ExpectingLine.Text;
                    }
                    else if (!string.IsNullOrWhiteSpace(line) || IsText(next) || IsText(nextNext) || nextNextNext == GetLastNumber(_paragraph))
                    {
                        if (_isWsrt && !string.IsNullOrEmpty(line) && line.Contains("<3", StringComparison.Ordinal))
                        {
                            if (_regExWsrtItalicStart == null)
                            {
                                _regExWsrtItalicStart = new Regex(@"<3\d>", RegexOptions.Compiled);
                            }

                            if (_regExWsrtItalicEnd == null)
                            {
                                _regExWsrtItalicEnd = new Regex(@"</3\d>", RegexOptions.Compiled);
                            }

                            line = _regExWsrtItalicStart.Replace(line, "<i>");
                            line = _regExWsrtItalicEnd.Replace(line, "</i>");
                        }

                        if (_paragraph.Text.Length > 0)
                        {
                            _paragraph.Text += Environment.NewLine;
                        }

                        _paragraph.Text += RemoveBadChars(line).TrimEnd();
                    }
                    else if (string.IsNullOrEmpty(line) && string.IsNullOrEmpty(_paragraph.Text))
                    {
                        _paragraph.Text = string.Empty;
                        if (!string.IsNullOrEmpty(next) && (Utilities.IsInteger(next) || TryReadTimeCodesLine(next, null, false)))
                        {
                            subtitle.Paragraphs.Add(_paragraph);
                            _lastParagraph = _paragraph;
                            _paragraph = new Paragraph();
                            _expecting = ExpectingLine.Number;
                        }
                    }
                    else if (string.IsNullOrEmpty(line) && string.IsNullOrEmpty(next))
                    {
                        _paragraph.Text += Environment.NewLine + RemoveBadChars(line).TrimEnd();
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

        private static string GetLastNumber(Paragraph p)
        {
            return p == null ? "1" : (p.Number + 1).ToString(CultureInfo.InvariantCulture);
        }

        private bool IsText(string text)
        {
            return !(string.IsNullOrWhiteSpace(text) || Utilities.IsInteger(text) || TryReadTimeCodesLine(text.Trim(), null, false));
        }

        private static string RemoveBadChars(string line)
        {
            return line.Replace('\0', ' ');
        }

        private bool TryReadTimeCodesLine(string input, Paragraph paragraph, bool validate)
        {
            var s = input.TrimStart('-', ' ');
            if (s.Length < 10 || !char.IsDigit(s[0]))
            {
                return false;
            }

            const string defaultSeparator = " --> ";
            // Fix some badly formatted separator sequences - anything can happen if you manually edit ;)
            var line = input.Replace('،', ',')
                .Replace('', ',')
                .Replace('¡', ',')
                .Replace(" -> ", defaultSeparator)
                .Replace(" - > ", defaultSeparator)
                .Replace(" ->> ", defaultSeparator)
                .Replace(" -- > ", defaultSeparator)
                .Replace(" - -> ", defaultSeparator)
                .Replace(" -->> ", defaultSeparator)
                .Replace(" ---> ", defaultSeparator)
                .Replace(": ", ":").Trim();

            // Removed stuff after time codes - like subtitle position
            //  - example of position info: 00:02:26,407 --> 00:02:31,356  X1:100 X2:100 Y1:100 Y2:100
            if (line.Length > 30)
            {
                if (line[29] == ' ')
                {
                    line = line.Substring(0, 29);
                }
                else if (line[28] == ' ')
                {
                    line = line.Substring(0, 28);
                }
                else if (line[27] == ' ')
                {
                    line = line.Substring(0, 27);
                }
            }

            // removes all extra spaces
            line = line.RemoveChar(' ').Replace("-->", defaultSeparator).Trim();

            // Fix a few more cases of wrong time codes, seen this: 00.00.02,000 --> 00.00.04,000
            line = line.Replace('.', ':');
            if (line.Length >= 29 && (line[8] == ':' || line[8] == ';'))
            {
                line = line.Substring(0, 8) + ',' + line.Substring(8 + 1);
            }

            if (line.Length >= 29 && line.Length <= 30 && (line[25] == ':' || line[25] == ';'))
            {
                line = line.Substring(0, 25) + ',' + line.Substring(25 + 1);
            }

            // allow missing hours as some (buggy) websites generate these time codes: 04:48,460 --> 04:52,364
            if (line.Length == 23 && line[2] == ':' && line[5] == ',' && line[9] == ' ' && line[12] == '>' && line[13] == ' ' && line[16] == ':' && line[19] == ',')
            {
                line = "00:" + line.Insert(14, "00:");
            }

            if (IsValidTimeCode(line))
            {
                string[] parts = line.Replace("-->", ":").RemoveChar(' ').Split(':', ',');
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

                    if (validate && _errors.Length < 2000 &&
                        (startHours > 99 || startMinutes > 99 || startSeconds > 99 || startMilliseconds > 999 ||
                        endHours > 99 || endMinutes > 99 || endSeconds > 99 || endMilliseconds > 999))
                    {
                        _errors.AppendLine(string.Format(FormatLanguage.LineNumberXErrorReadingTimeCodeFromSourceLineY, _lineNumber, line));
                    }

                    if (_isMsFrames && (parts[3].Length != 2 || startMilliseconds > 30 || parts[7].Length != 2 || endMilliseconds > 30))
                    {
                        _isMsFrames = false;
                    }

                    if (paragraph != null)
                    {
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

        /// <summary>
        /// Optimized validation of time codes
        /// Old Regex was:
        ///  - ^-?\d+:-?\d+:-?\d+[:,]-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+[:,]-?\d+$
        ///  - ^\d+:\d+:\d+,\d+\s*-->\s*\d+:\d+:\d+,\d+$
        /// <returns>True if valid srt time code</returns>
        /// </summary>
        private static bool IsValidTimeCode(string line)
        {
            int step = 0;
            var max = line.Length;
            for (int i = 0; i < max; i++)
            {
                var ch = line[i];
                if (char.IsWhiteSpace(ch))
                {
                    continue;
                }

                if (step == 0 || step == 2 || step == 4 || step == 9 || step == 11) // start numbers
                {
                    if (ch == '-' || char.IsDigit(ch))
                    {
                        step++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (step == 1 || step == 3 || step == 10 || step == 12) // number
                {
                    if (char.IsDigit(ch))
                    {
                        // ok
                    }
                    else if (ch == ':')
                    {
                        step++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (step == 5 || step == 13) // seconds
                {
                    if (char.IsDigit(ch))
                    {
                        // ok
                    }
                    else if (ch == ',' || ch == ':')
                    {
                        step++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (step == 6 || step == 14) // milliseconds
                {
                    if (char.IsDigit(ch))
                    {
                        // ok
                    }
                    else if (ch == '-')
                    {
                        step++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (step == 7) // separator char 2
                {
                    if (ch == '-')
                    {
                        step++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (step == 8) // separator char 3
                {
                    if (ch == '>')
                    {
                        step++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (step > 14)
                {
                    return false;
                }
            }
            return true;
        }
    }
}