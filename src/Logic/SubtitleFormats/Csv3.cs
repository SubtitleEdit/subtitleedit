using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Csv3 : SubtitleFormat
    {

        private const string Separator = ",";

        //01:00:10:03,01:00:15:25,"I thought I should let my sister-in-law know.", ""
        private static readonly Regex CsvLine = new Regex(@"^\d\d:\d\d:\d\d:\d\d" + Separator + @"\d\d:\d\d:\d\d:\d\d" + Separator, RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".csv"; }
        }

        public override string Name
        {
            get { return "Csv3"; }
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
                if (line.StartsWith("$FontName") || line.StartsWith("$ColorIndex1"))
                    return false;
                Match m = null;
                if (line.Length > 8 && line[2] == ':')
                    m = CsvLine.Match(line);
                if (m != null && m.Success)
                {
                    fine++;
                    string s = line.Remove(0, m.Length);
                    continuation = s.StartsWith('"');
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    if (continuation)
                        continuation = false;
                    else
                        failed++;
                }

            }
            if (failed > 20)
                return false;

            return fine > failed;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "{1}{0}{2}{0}\"{3}\"{0}\"{4}\"";
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(format, Separator, "Start time (hh:mm:ss:ff)", "End time (hh:mm:ss:ff)", "Line 1", "Line 2"));
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var arr = p.Text.Trim().Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 3)
                {
                    string s = Utilities.AutoBreakLine(p.Text);
                    arr = s.Trim().Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                }
                string line1 = string.Empty;
                string line2 = string.Empty;
                if (arr.Length > 0)
                    line1 = arr[0];
                if (arr.Length > 1)
                    line2 = arr[1];
                line1 = line1.Replace("\"", "\"\"");
                line2 = line2.Replace("\"", "\"\"");
                sb.AppendLine(string.Format(format, Separator, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), line1, line2));
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                Match m = CsvLine.Match(line);
                if (m.Success)
                {
                    string[] parts = line.Substring(0, m.Length).Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                        try
                        {
                            var start = DecodeTimeCode(parts[0]);
                            var end = DecodeTimeCode(parts[1]);
                            string text = ReadText(line.Remove(0, m.Length));
                            var p = new Paragraph(start, end, text);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private static string ReadText(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                return string.Empty;

            csv = csv.Replace("\"\"", "\"");

            var sb = new StringBuilder();
            csv = csv.Trim();
            if (csv.StartsWith('"'))
                csv = csv.Remove(0, 1);
            if (csv.EndsWith('"'))
                csv = csv.Remove(csv.Length - 1, 1);
            bool isBreak = false;
            for (int i = 0; i < csv.Length; i++)
            {
                var s = csv[i];
                if (s == '"' && csv.Substring(i).StartsWith("\"\""))
                {
                    sb.Append('"');
                }
                else if (s == '"')
                {
                    if (isBreak)
                    {
                        isBreak = false;
                    }
                    else if (i == 0 || i == csv.Length - 1 || sb.ToString().EndsWith(Environment.NewLine))
                    {
                        sb.Append('"');
                    }
                    else
                    {
                        isBreak = true;
                    }
                }
                else
                {
                    if (isBreak && s == ' ')
                    {
                    }
                    else if (isBreak && s == ',')
                    {
                        sb.Append(Environment.NewLine);
                    }
                    else
                    {
                        isBreak = false;
                        sb.Append(s);
                    }
                }
            }
            return sb.ToString().Trim();
        }

        private static TimeCode DecodeTimeCode(string part)
        {
            string[] parts = part.Split(new[] { '.', ':' }, StringSplitOptions.RemoveEmptyEntries);

            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}
