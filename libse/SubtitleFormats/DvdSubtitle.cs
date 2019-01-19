using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DvdSubtitle : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^\{T \d+:\d+:\d+:\d+$", RegexOptions.Compiled);

        public override string Extension => ".sub";

        public override string Name => "DVDSubtitle";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "T {0}\r\n{1}\r\n";
            const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";
            const string header = @"{HEAD
DISCID=
DVDTITLE=
CODEPAGE=1250
FORMAT=ASCII
LANG=
TITLE=1
ORIGINAL=ORIGINAL
AUTHOR=
WEB=
INFO=
LICENSE=
}";

            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                int milliseconds = p.StartTime.Milliseconds / 10;
                string time = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, milliseconds);
                sb.AppendLine("{" + string.Format(paragraphWriteFormat, time, p.Text) + "}");

                milliseconds = p.EndTime.Milliseconds / 10;
                time = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, milliseconds);
                sb.AppendLine("{" + string.Format(paragraphWriteFormat, time, string.Empty) + "}");
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //{T 00:03:14:27
            //Some text
            //}
            _errorCount = 0;
            bool textOn = false;
            string text = string.Empty;
            TimeCode start = new TimeCode();
            TimeCode end = new TimeCode();
            foreach (string line in lines)
            {
                if (textOn)
                {
                    if (line.Trim() == "}")
                    {
                        Paragraph p = new Paragraph
                        {
                            Text = text,
                            StartTime = new TimeCode(start.TotalMilliseconds),
                            EndTime = new TimeCode(end.TotalMilliseconds)
                        };

                        subtitle.Paragraphs.Add(p);

                        text = string.Empty;
                        start = new TimeCode();
                        end = new TimeCode();
                        textOn = false;
                    }
                    else
                    {
                        if (text.Length == 0)
                        {
                            text = line;
                        }
                        else
                        {
                            text += Environment.NewLine + line;
                        }
                    }
                }
                else
                {
                    if (RegexTimeCodes.Match(line).Success)
                    {
                        try
                        {
                            textOn = true;
                            string[] arr = line.Substring(3).Trim().Split(':');
                            if (arr.Length == 4)
                            {
                                int hours = int.Parse(arr[0]);
                                int minutes = int.Parse(arr[1]);
                                int seconds = int.Parse(arr[2]);
                                int milliseconds = int.Parse(arr[3]);
                                if (arr[3].Length == 2)
                                {
                                    milliseconds *= 10;
                                }

                                start = new TimeCode(hours, minutes, seconds, milliseconds);
                            }
                        }
                        catch
                        {
                            textOn = false;
                            _errorCount++;
                        }
                    }
                }
            }

            int index = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(index);
                if (next != null)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
                index++;
            }

            subtitle.RemoveEmptyLines();

            subtitle.Renumber();
        }
    }
}
