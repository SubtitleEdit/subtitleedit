using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class SwiftText : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        private Paragraph _paragraph;
        private StringBuilder _text = new StringBuilder();
        private ExpectingLine _expecting = ExpectingLine.TimeCodes;

        private static readonly Regex RegexTimeCodes = new Regex(@"^TIMEIN:\s*[0123456789-]+:[0123456789-]+:[0123456789-]+:[0123456789-]+\s*DURATION:\s*[0123456789-]+:[0123456789-]+\s*TIMEOUT:\s*[0123456789-]+:[0123456789-]+:[0123456789-]+:[0123456789-]+$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Swift text"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines == null || lines.Count > 2 && !string.IsNullOrEmpty(lines[0]) && lines[0].Contains("{QTtext}"))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //TIMEIN: 01:00:01:09   DURATION: 01:20 TIMEOUT: --:--:--:--
            //Broadcasting
            //from an undisclosed location...

            //TIMEIN: 01:00:04:12   DURATION: 04:25 TIMEOUT: 01:00:09:07

            const string paragraphWriteFormat = "TIMEIN: {0}\tDURATION: {1}\tTIMEOUT: {2}\r\n{3}\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds));
                string timeOut = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds));
                string timeDuration = string.Format("{0:00}:{1:00}", p.Duration.Seconds, MillisecondsToFramesMaxFrameRate(p.Duration.Milliseconds));
                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, timeDuration, timeOut, p.Text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _paragraph = new Paragraph();
            _expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                ReadLine(subtitle, line);
                if (_text.Length > 1000)
                    return;
            }
            if (_text != null && _text.ToString().TrimStart().Length > 0)
            {
                _paragraph.Text = _text.ToString().Trim();
                subtitle.Paragraphs.Add(_paragraph);
            }

            subtitle.Renumber(1);
        }

        private void ReadLine(Subtitle subtitle, string line)
        {
            switch (_expecting)
            {
                case ExpectingLine.TimeCodes:
                    if (TryReadTimeCodesLine(line, _paragraph))
                    {
                        _text = new StringBuilder();
                        _expecting = ExpectingLine.Text;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        _errorCount++;
                        _expecting = ExpectingLine.Text; // lets go to next paragraph
                    }
                    break;
                case ExpectingLine.Text:
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        _text.AppendLine(line.TrimEnd());
                    }
                    else if (_paragraph != null && _paragraph.EndTime.TotalMilliseconds > 0)
                    {
                        _paragraph.Text = _text.ToString().Trim();
                        subtitle.Paragraphs.Add(_paragraph);
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.TimeCodes;
                    }
                    else
                    {
                        _errorCount++;
                    }
                    break;
            }
        }

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            line = line.Trim();
            if (RegexTimeCodes.IsMatch(line))
            {
                // TIMEIN: 01:00:04:12   DURATION: 04:25 TIMEOUT: 01:00:09:07
                var timestamp = line.Replace("TIMEIN:", string.Empty).Replace("DURATION", string.Empty).Replace("TIMEOUT", string.Empty).Replace(" ", string.Empty).Replace("\t", string.Empty);
                var tokens = timestamp.Split(':');
                paragraph.StartTime = TimeCode.FromFrameTokens(tokens[0], tokens[1], tokens[2], tokens[3]);
                paragraph.EndTime = TimeCode.FromFrameTokens(tokens[6], tokens[7], tokens[8], tokens[9]);
                return true;
            }
            return false;
        }
    }
}
