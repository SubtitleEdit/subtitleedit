using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class SwiftText : SubtitleFormat
    {
        enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        Paragraph _paragraph;
        StringBuilder _text = new StringBuilder();
        ExpectingLine _expecting = ExpectingLine.TimeCodes;


        static readonly Regex RegexTimeCodes = new Regex(@"^TIMEIN:\s*[0123456789-]+:[0123456789-]+:[0123456789-]+:[0123456789-]+\s*DURATION:\s*[0123456789-]+:[0123456789-]+\s*TIMEOUT:\s*[0123456789-]+:[0123456789-]+:[0123456789-]+:[0123456789-]+$", RegexOptions.Compiled);

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
                string startTime = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                string duration = string.Format("{0:00}:{1:00}", p.Duration.Seconds, p.Duration.Milliseconds / 10);
                string timeOut = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);
                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, duration, timeOut, p.Text));
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
                if (_text.Length > 10000)
                    return;
            }
            if (_paragraph.Text.Trim().Length > 0)
                subtitle.Paragraphs.Add(_paragraph);

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
                    else if (line.Trim().Length > 0)
                    {
                        _errorCount++;
                        _expecting = ExpectingLine.Text; // lets go to next paragraph
                    }
                    break;
                case ExpectingLine.Text:
                    if (line.Trim().Length > 0)
                    {
                        _text.AppendLine(line.TrimEnd());
                    }
                    else
                    {
                        _paragraph.Text = _text.ToString().Trim();
                        subtitle.Paragraphs.Add(_paragraph);
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.TimeCodes;
                    }
                    break;
            }
        }

        private bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            line = line.Trim();
            if (RegexTimeCodes.IsMatch(line))
            {

                //TIMEIN: 01:00:04:12   DURATION: 04:25 TIMEOUT: 01:00:09:07
                string s = line.Replace("TIMEIN:", string.Empty).Replace("DURATION", string.Empty).Replace("TIMEOUT", string.Empty).Replace(" ", string.Empty).Replace("\t", string.Empty);
                string[] parts = s.Split(':');
                try
                {
                    int startHours = int.Parse(parts[0]);
                    int startMinutes = int.Parse(parts[1]);
                    int startSeconds = int.Parse(parts[2]);
                    int startMilliseconds = int.Parse(parts[3]) * 10;

                    int durationSeconds = 0;
                    if (parts[4] != "-")
                        durationSeconds = int.Parse(parts[4]);
                    int durationMilliseconds = 0;
                    if (parts[5] != "--")
                        durationMilliseconds = int.Parse(parts[5]) * 10;

                    int endHours = 0;
                    if (parts[6] != "--")
                        endHours = int.Parse(parts[6]);
                    int endMinutes = 0;
                    if (parts[7] != "--")
                        endMinutes = int.Parse(parts[7]);
                    int endSeconds = 0;
                    if (parts[8] != "--")
                        endSeconds = int.Parse(parts[8]);
                    int endMilliseconds = 0;
                    if (parts[9] != "--")
                        endMilliseconds = int.Parse(parts[9]) * 10;

                    paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);

                    if (durationSeconds > 0 || durationMilliseconds > 0)
                        paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + (durationSeconds * 1000 + durationMilliseconds);
                    else
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
