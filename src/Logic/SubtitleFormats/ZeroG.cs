using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class ZeroG : SubtitleFormat
    {

        public override string Extension
        {
            get { return ".zeg"; }
        }

        public override string Name
        {
            get { return "Zero G"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //% Zero G 1.0

            //E 1 0:50:20.22 0:50:21.38 Default NTP Die Frage ist:
            //E 1 0:50:21.54 0:50:25.86 Default NTP Wieso habe ich überlebt?
            //E 1 0:50:27.30 0:50:30.78 Default NTP Was habe ich richtig gemacht?  \n Ich weiß es nicht.
            const string paragraphWriteFormat = "E 1 {0} {1} Default NTP {2}";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("% Zero G 1.0");
            sb.AppendLine();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text.Replace(Environment.NewLine, " \\n ");
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var regexTimeCode = new Regex(@"^E 1 \d:\d\d:\d\d.\d\d \d:\d\d:\d\d.\d\d Default NTP ", RegexOptions.Compiled);
            //E 1 0:50:05.42 0:50:10.06 Default NTP

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (regexTimeCode.IsMatch(line))
                {
                    try
                    {
                        string timePart = line.Substring(4, 10).Trim();
                        var start = DecodeTimeCode(timePart);
                        timePart = line.Substring(15, 10).Trim();
                        var end = DecodeTimeCode(timePart);
                        var paragraph = new Paragraph();
                        paragraph.StartTime = start;
                        paragraph.EndTime = end;
                        paragraph.Text = line.Substring(38).Replace(" \\n ", Environment.NewLine).Replace("\\n", Environment.NewLine);
                        subtitle.Paragraphs.Add(paragraph);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //0:50:05.42
            return string.Format("{0:0}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 10);
        }

        private static TimeCode DecodeTimeCode(string timePart)
        {
            string[] parts = timePart.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);
            int milliseconds = int.Parse(parts[3]) * 10;
            return new TimeCode(hours, minutes, seconds, milliseconds);
        }

    }
}