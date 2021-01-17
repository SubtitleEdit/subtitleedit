using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    //Subtitle number: 1
    //Start time (or frames): 00:00:48,862:0000001222
    //End time (or frames): 00:00:50,786:0000001270
    //Subtitle text: In preajma lacului Razel,
    public class UnknownSubtitle2 : SubtitleFormat
    {
        private enum ExpectingLine
        {
            Number,
            StartTime,
            EndTime,
            Text
        }

        private Paragraph _paragraph;
        private ExpectingLine _expecting = ExpectingLine.Number;

        public override string Extension => ".txt";

        public override string Name => "Unknown 2";

        public override string ToText(Subtitle subtitle, string title)
        {
            //Subtitle number: 1
            //Start time (or frames): 00:00:48,862:0000001222
            //End time (or frames): 00:00:50,786:0000001270
            //Subtitle text: In preajma lacului Razel,

            const string paragraphWriteFormat = "Subtitle number: {0}\r\nStart time (or frames): {1}\r\nEnd time (or frames): {2}\r\nSubtitle text: {3}\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format("{0:00}:{1:00}:{2:00},{3:00}:0000000000", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                string timeOut = string.Format("{0:00}:{1:00}:{2:00},{3:00}:0000000000", p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);
                sb.AppendLine(string.Format(paragraphWriteFormat, p.Number, startTime, timeOut, p.Text.Replace(Environment.NewLine, "|")));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _paragraph = new Paragraph();
            _expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                ReadLine(subtitle, line);
            }
            if (!string.IsNullOrWhiteSpace(_paragraph.Text))
            {
                subtitle.Paragraphs.Add(_paragraph);
            }

            subtitle.Renumber();
        }

        private void ReadLine(Subtitle subtitle, string line)
        {
            //Subtitle number: 1
            //Start time (or frames): 00:00:48,862:0000001222
            //End time (or frames): 00:00:50,786:0000001270
            //Subtitle text: In preajma lacului Razel,

            switch (_expecting)
            {
                case ExpectingLine.Number:
                    if (line.StartsWith("Subtitle number: "))
                    {
                        _expecting = ExpectingLine.StartTime;
                    }
                    break;
                case ExpectingLine.StartTime:
                    if (line.StartsWith("Start time (or frames): "))
                    {
                        TryReadTimeCodesLine(line.Substring(23), _paragraph, true);
                        _expecting = ExpectingLine.EndTime;
                    }
                    break;
                case ExpectingLine.EndTime:
                    if (line.StartsWith("End time (or frames): "))
                    {
                        TryReadTimeCodesLine(line.Substring(21), _paragraph, false);
                        _expecting = ExpectingLine.Text;
                    }
                    break;
                case ExpectingLine.Text:
                    if (line.StartsWith("Subtitle text: "))
                    {
                        string text = line.Substring(14).Trim();
                        text = text.Replace("|", Environment.NewLine);
                        _paragraph.Text = text;
                        subtitle.Paragraphs.Add(_paragraph);
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.Number;
                    }
                    break;
            }
        }

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph, bool start)
        {
            line = line.Trim();

            //00:00:48,862:0000001222
            line = line.Replace(",", ":");
            string[] parts = line.Split(':');
            try
            {
                int startHours = int.Parse(parts[0]);
                int startMinutes = int.Parse(parts[1]);
                int startSeconds = int.Parse(parts[2]);
                int startMilliseconds = int.Parse(parts[3]);

                if (start)
                {
                    paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);
                }
                else
                {
                    paragraph.EndTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
