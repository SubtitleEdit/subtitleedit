using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Csv2 : SubtitleFormat
    {

        private const string _seperator = ",";

        //1,01:00:10:03,01:00:15:25,I thought I should let my sister-in-law know.
        static Regex csvLine = new Regex(@"^\d+" + _seperator + @"\d\d:\d\d:\d\d:\d\d" + _seperator + @"\d\d:\d\d:\d\d:\d\d" + _seperator, RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".csv"; }
        }

        public override string Name
        {
            get { return "Csv2"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            int fine = 0;
            int failed = 0;
            bool continuation = false;
            foreach (string line in lines)
            {
                Match m = csvLine.Match(line);
                if (m.Success)
                {
                    fine++;
                    string s = line.Remove(0, m.Length);
                    continuation = s.StartsWith("\"");
                }
                else if (line.Trim().Length > 0)
                {
                    if (continuation)
                        continuation = false;
                    else
                        failed++;
                }

            }
            return fine > failed;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string format = "{1}{0}{2}{0}{3}{0}\"{4}\"";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(format, _seperator, "Number", "Start time in milliseconds", "End time in milliseconds", "Text"));
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(format, _seperator, p.Number, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text));
            }
            return sb.ToString().Trim();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool continuation = false;
            Paragraph p = null;
            foreach (string line in lines)
            {
                Match m = csvLine.Match(line);
                if (m.Success)
                {
                    string[] parts = line.Substring(0, m.Length).Split(_seperator.ToCharArray(),  StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    try
                    {
                        var start = DecodeTimeCode(parts[1]);
                        var end = DecodeTimeCode(parts[2]);
                        string text = line.Remove(0, m.Length);
                        continuation = text.StartsWith("\"") && !text.EndsWith("\"");
                        text = text.Trim('"');
                        p = new Paragraph(start, end, text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (line.Trim().Length > 0)
                {
                    if (continuation)
                    {
                        if (p != null && p.Text.Length < 300)
                            p.Text = (p.Text + Environment.NewLine + line.TrimEnd('"')).Trim();
                        continuation = !line.Trim().EndsWith("\"");
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string part)
        {
            string[] parts = part.Split(".:".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}
