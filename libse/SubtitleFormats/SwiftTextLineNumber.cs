using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SwiftTextLineNumber : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        private Paragraph _paragraph;
        private StringBuilder _text = new StringBuilder();
        private ExpectingLine _expecting = ExpectingLine.TimeCodes;

        private static readonly Regex RegexTimeCodes = new Regex(@"^SUBTITLE: \d+\s+TIMEIN:\s*[0123456789-]+:[0123456789-]+:[0123456789-]+:[0123456789-]+\s*TIMEOUT:\s*[0123456789-]+:[0123456789-]+:[0123456789-]+:[0123456789-]+$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Swift text line#";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines == null || lines.Count > 2 && !string.IsNullOrEmpty(lines[0]) && lines[0].Contains("{QTtext}"))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //SUBTITLE: 1   TIMEIN: 00:00:00:00 TIMEOUT: 00:00:04:00
            //Voor de oorlog

            //SUBTITLE: 2   TIMEIN: 00:00:05:12 TIMEOUT: 00:00:10:02
            //Ik ben Marie Pinhas. Ik ben geboren
            //in Thessaloniki in Griekenland,

            //SUBTITLE: 3   TIMEIN: 00:00:10:06 TIMEOUT: 00:00:15:17
            //op 6 maart '31,
            //in een heel oude Griekse familie.

            const string paragraphWriteFormat = "SUBTITLE: {1}\tTIMEIN: {0}\tTIMEOUT: {2}\r\n{3}\r\n";

            var sb = new StringBuilder();
            int count = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                string startTime = $"{p.StartTime.Hours:00}:{p.StartTime.Minutes:00}:{p.StartTime.Seconds:00}:{MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds):00}";
                string timeOut = $"{p.EndTime.Hours:00}:{p.EndTime.Minutes:00}:{p.EndTime.Seconds:00}:{MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds):00}";
                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, count, timeOut, p.Text));
                count++;
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
                {
                    return;
                }
            }
            if (_text != null && _text.ToString().TrimStart().Length > 0)
            {
                _paragraph.Text = _text.ToString().Trim();
                subtitle.Paragraphs.Add(_paragraph);
            }

            subtitle.Renumber();
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
            if (line.Length > 20 && line.StartsWith("SUBTITLE:", StringComparison.Ordinal) && RegexTimeCodes.IsMatch(line))
            {
                //SUBTITLE: 59  TIMEIN: 00:04:28:06 TIMEOUT: 00:04:32:12
                string s = line.Replace("SUBTITLE:", string.Empty).Replace("TIMEIN", string.Empty).Replace("TIMEOUT", string.Empty).RemoveChar(' ').Replace("\t", string.Empty);
                string[] parts = s.Split(':');
                try
                {
                    int startHours = int.Parse(parts[1]);
                    int startMinutes = int.Parse(parts[2]);
                    int startSeconds = int.Parse(parts[3]);
                    int startMilliseconds = FramesToMillisecondsMax999(int.Parse(parts[4]));

                    int endHours = 0;
                    if (parts[5] != "--")
                    {
                        endHours = int.Parse(parts[5]);
                    }

                    int endMinutes = 0;
                    if (parts[6] != "--")
                    {
                        endMinutes = int.Parse(parts[6]);
                    }

                    int endSeconds = 0;
                    if (parts[7] != "--")
                    {
                        endSeconds = int.Parse(parts[7]);
                    }

                    int endMilliseconds = 0;
                    if (parts[8] != "--")
                    {
                        endMilliseconds = FramesToMillisecondsMax999(int.Parse(parts[8]));
                    }

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
