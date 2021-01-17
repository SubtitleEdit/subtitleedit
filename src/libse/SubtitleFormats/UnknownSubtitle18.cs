using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle18 : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeCodes,
            BlankBeforeText,
            Text
        }

        private Paragraph _paragraph;
        private StringBuilder _text = new StringBuilder();
        private ExpectingLine _expecting = ExpectingLine.TimeCodes;

        // 0001 01:00:15:08 01:00:18:05
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d \d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 18";

        public override string ToText(Subtitle subtitle, string title)
        {
            //0001 01:00:15:08 01:00:18:05
            //
            //PUHDASTA LÄHDEVETTÄ
            //SUORAAN OVELLE TUOTUNA.
            //
            //0002 01:00:18:07 01:00:20:18
            //
            //MAKU, JONKA MUISTAT LAPSUUDESTA.

            const string paragraphWriteFormat = "{3:0000} {0} {1} \r\n\r\n{2}\r\n";

            var sb = new StringBuilder();
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = $"{p.StartTime.Hours:00}:{p.StartTime.Minutes:00}:{p.StartTime.Seconds:00}:{MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds):00}";
                string timeOut = $"{p.EndTime.Hours:00}:{p.EndTime.Minutes:00}:{p.EndTime.Seconds:00}:{MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds):00}";
                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, timeOut, p.Text, count));
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
            }
            if (!string.IsNullOrWhiteSpace(_text.ToString()))
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
                        _expecting = ExpectingLine.BlankBeforeText;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        _errorCount++;
                    }
                    break;
                case ExpectingLine.BlankBeforeText:
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        _text = new StringBuilder();
                        _expecting = ExpectingLine.Text;
                    }
                    else
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
                    else
                    {
                        _paragraph.Text = _text.ToString().Trim();
                        subtitle.Paragraphs.Add(_paragraph);
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.TimeCodes;
                        _text = new StringBuilder();
                    }
                    break;
            }
        }

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            line = line.Trim();
            if (RegexTimeCodes.IsMatch(line))
            {
                //0001 01:00:15:08 01:00:18:05
                try
                {
                    string start = line.Substring(5, 11);
                    var parts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    int startHours = int.Parse(parts[0]);
                    int startMinutes = int.Parse(parts[1]);
                    int startSeconds = int.Parse(parts[2]);
                    int startMilliseconds = FramesToMillisecondsMax999(int.Parse(parts[3]));

                    string end = line.Substring(17, 11);
                    parts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    int endHours = int.Parse(parts[0]);
                    int endMinutes = int.Parse(parts[1]);
                    int endSeconds = int.Parse(parts[2]);
                    int endMilliseconds = FramesToMillisecondsMax999(int.Parse(parts[3]));

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
