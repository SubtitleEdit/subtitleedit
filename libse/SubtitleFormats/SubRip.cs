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

        private enum ExpectingLine
        {
            Number,
            TimeCodes,
            Text
        }

        private Paragraph _paragraph;
        private Paragraph _lastParagraph;
        private ExpectingLine _expecting = ExpectingLine.Number;
        private static readonly Regex RegexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+[:,]-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+[:,]-?\d+$", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodes2 = new Regex(@"^\d+:\d+:\d+,\d+\s*-->\s*\d+:\d+:\d+,\d+$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".srt"; }
        }

        public const string NameOfFormat = "SubRip";

        public override string Name
        {
            get { return NameOfFormat; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0].StartsWith("WEBVTT", StringComparison.OrdinalIgnoreCase))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            Errors = null;
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}\r\n{1} --> {2}\r\n{3}\r\n\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //string s = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                sb.AppendFormat(paragraphWriteFormat, p.Number, p.StartTime, p.EndTime, p.Text);
            }
            return sb.ToString().Trim() + Environment.NewLine + Environment.NewLine;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            bool doRenum = false;
            _errors = new StringBuilder();
            _lineNumber = 0;

            _paragraph = new Paragraph();
            _expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                _lineNumber++;
                string line = lines[i].TrimEnd();
                line = line.Trim('\u007F'); // 127=delete acscii

                string next = string.Empty;
                if (i + 1 < lines.Count)
                    next = lines[i + 1];

                string nextNext = string.Empty;
                if (i + 2 < lines.Count)
                    nextNext = lines[i + 2];

                // A new line is missing between two paragraphs (buggy srt file)
                if (_expecting == ExpectingLine.Text && i + 1 < lines.Count &&
                    _paragraph != null && !string.IsNullOrEmpty(_paragraph.Text) && Utilities.IsInteger(line) &&
                    RegexTimeCodes.IsMatch(lines[i + 1]))
                {
                    ReadLine(subtitle, string.Empty, string.Empty, string.Empty);
                }
                if (_expecting == ExpectingLine.Number && RegexTimeCodes.IsMatch(line))
                {
                    _expecting = ExpectingLine.TimeCodes;
                    doRenum = true;
                }

                ReadLine(subtitle, line, next, nextNext);
            }
            if (_paragraph != null && _paragraph.EndTime.TotalMilliseconds > _paragraph.StartTime.TotalMilliseconds)
                subtitle.Paragraphs.Add(_paragraph);

            //foreach (Paragraph p in subtitle.Paragraphs)
            //    p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            if (doRenum)
                subtitle.Renumber();

            Errors = _errors.ToString();
        }

        private void ReadLine(Subtitle subtitle, string line, string next, string nextNext)
        {
            switch (_expecting)
            {
                case ExpectingLine.Number:
                    int number;
                    if (int.TryParse(line, out number))
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
                                _errors.AppendLine(string.Format(Configuration.Settings.Language.Main.LineNumberXExpectedNumberFromSourceLineY, _lineNumber, line));
                            _errorCount++;
                        }
                    }
                    break;
                case ExpectingLine.TimeCodes:
                    if (TryReadTimeCodesLine(line, _paragraph))
                    {
                        _paragraph.Text = string.Empty;
                        _expecting = ExpectingLine.Text;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (_errors.Length < 2000)
                            _errors.AppendLine(string.Format(Configuration.Settings.Language.Main.LineNumberXErrorReadingTimeCodeFromSourceLineY, _lineNumber, line));
                        _errorCount++;
                        _expecting = ExpectingLine.Number; // lets go to next paragraph
                    }
                    break;
                case ExpectingLine.Text:
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (_paragraph.Text.Length > 0)
                            _paragraph.Text += Environment.NewLine;
                        _paragraph.Text += RemoveBadChars(line).TrimEnd().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }
                    else if (IsText(next))
                    {
                        if (_paragraph.Text.Length > 0)
                            _paragraph.Text += Environment.NewLine;
                        _paragraph.Text += RemoveBadChars(line).TrimEnd().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }
                    else if (string.IsNullOrEmpty(line) && string.IsNullOrEmpty(_paragraph.Text))
                    {
                        _paragraph.Text = string.Empty;
                        if (!string.IsNullOrEmpty(next) && (Utilities.IsInteger(next) || RegexTimeCodes.IsMatch(next)))
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
            if (string.IsNullOrWhiteSpace(text) || Utilities.IsInteger(text) || RegexTimeCodes.IsMatch(text))
                return false;
            return true;
        }

        private static string RemoveBadChars(string line)
        {
            return line.Replace('\0', ' ');
        }

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            line = line.Replace('،', ',');
            line = line.Replace('', ',');
            line = line.Replace('¡', ',');

            const string defaultSeparator = " --> ";
            // Fix some badly formatted separator sequences - anything can happen if you manually edit ;)
            line = line.Replace(" -> ", defaultSeparator); // I've seen this
            line = line.Replace(" - > ", defaultSeparator);
            line = line.Replace(" ->> ", defaultSeparator);
            line = line.Replace(" -- > ", defaultSeparator);
            line = line.Replace(" - -> ", defaultSeparator);
            line = line.Replace(" -->> ", defaultSeparator);
            line = line.Replace(" ---> ", defaultSeparator);

            // Removed stuff after timecodes - like subtitle position
            //  - example of position info: 00:02:26,407 --> 00:02:31,356  X1:100 X2:100 Y1:100 Y2:100
            if (line.Length > 30 && line[29] == ' ')
                line = line.Substring(0, 29);

            // removes all extra spaces
            line = line.Replace(" ", string.Empty).Replace("-->", defaultSeparator).Trim();

            // Fix a few more cases of wrong time codes, seen this: 00.00.02,000 --> 00.00.04,000
            line = line.Replace('.', ':');
            if (line.Length >= 29 && (line[8] == ':' || line[8] == ';'))
                line = line.Substring(0, 8) + ',' + line.Substring(8 + 1);
            if (line.Length >= 29 && line.Length <= 30 && (line[25] == ':' || line[25] == ';'))
                line = line.Substring(0, 25) + ',' + line.Substring(25 + 1);

            if (RegexTimeCodes.IsMatch(line) || RegexTimeCodes2.IsMatch(line))
            {
                string[] parts = line.Replace("-->", ":").Replace(" ", string.Empty).Split(':', ',');
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
                    if (parts[0].StartsWith('-') && paragraph.StartTime.TotalMilliseconds > 0)
                        paragraph.StartTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds * -1;

                    paragraph.EndTime = new TimeCode(endHours, endMinutes, endSeconds, endMilliseconds);
                    if (parts[4].StartsWith('-') && paragraph.EndTime.TotalMilliseconds > 0)
                        paragraph.EndTime.TotalMilliseconds = paragraph.EndTime.TotalMilliseconds * -1;

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public override List<string> AlternateExtensions
        {
            get
            {
                return new List<string> { ".wsrt" };
            }
        }
    }
}