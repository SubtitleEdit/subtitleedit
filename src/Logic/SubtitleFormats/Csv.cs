using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Csv : SubtitleFormat
    {
        private string _seperator = ";";

        public override string Extension
        {
            get { return ".csv"; }
        }

        public override string Name
        {
            get { return "Csv"; }
        }

        public override bool HasLineNumber
        {
            get { return true; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Regex csvLine = new Regex(@"^""?\d+""?" + _seperator + @"""?\d+""?" + _seperator + @"""?\d+""?" + _seperator + @"""?[^""]*""?$", RegexOptions.Compiled);
            int fine = 0;
            int failed = 0;
            foreach (string line in lines)
            {
                if (csvLine.IsMatch(line))
                    fine++;
                else
                    failed++;

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
                sb.AppendLine(string.Format(format, _seperator, p.Number, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, p.Text.Replace(Environment.NewLine, "\n")));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Regex csvLine = new Regex(@"^""?\d+""?" + _seperator + @"""?\d+""?" + _seperator + @"""?\d+""?" + _seperator + @"""?[^""]*""?$", RegexOptions.Compiled);
            _errorCount = 0;

            foreach (string line in lines)
            {
                if (csvLine.IsMatch(line))
                {
                    string[] parts = line.Split(_seperator.ToCharArray(),  StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    try
                    {
                        int start = Convert.ToInt32(FixQuotes(parts[1]));
                        int end = Convert.ToInt32(FixQuotes(parts[2]));
                        string text = FixQuotes(parts[3]);

                        subtitle.Paragraphs.Add(new Paragraph(text, start, end));
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private static string FixQuotes(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith("\"") && text.Length > 1)
                text = text.Substring(1);

            if (text.EndsWith("\"") && text.Length > 1)
                text = text.Substring(0, text.Length-2);

            return text.Replace("\"\"", "\"");
        }
    }
}
