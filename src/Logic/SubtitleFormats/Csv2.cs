using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Csv2 : SubtitleFormat
    {
        private const string Seperator = ";";
        // \"[^""]*\";\d\d:\d\d:\d\d:\d\d;\d\d:\d\d:\d\d:\d\d;[^""]*;[^""]*;\"[^""]*\"
        static readonly Regex CsvLine = new Regex(@"^\""[^""]*\""" + Seperator + @"\d\d:\d\d:\d\d:\d\d" + Seperator + @"\d\d:\d\d:\d\d:\d\d" + Seperator + @"[^;]*" + Seperator + @"[^;]*" + Seperator + @"[^;]*" + Seperator + @"\""[^""]*\""$", RegexOptions.Compiled);

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
            foreach (string line in lines)
            {
                if (CsvLine.IsMatch(line))
                    fine++;
                else
                    failed++;

            }
            return fine > failed;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "\"{1}\"{0}{2}{0}{3}{0}{4}{0}{5}{0}\"{6}\"{0}\"{7}\"";
            var sb = new StringBuilder();
            
            sb.AppendLine(string.Format(format, Seperator, "Track name", "Timecode In", "Timecode Out", "Description", "Length", "Character", "Dialogue"));
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string actor = string.Empty;
                if (p.Actor != null)
                    actor = p.Actor;
                sb.AppendLine(string.Format(format, Seperator, title, EncodeTime(p.StartTime), EncodeTime(p.EndTime), "", "1", actor, p.Text.Replace(Environment.NewLine, "\n")));
            }
            return sb.ToString().Trim();
        }

        private string EncodeTime(TimeCode timeCode)
        {
            return timeCode.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            foreach (string line in lines)
            {
                if (CsvLine.IsMatch(line))
                {
                    string[] parts = line.Split(Seperator.ToCharArray(),  StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 6)
                    try
                    {
                        TimeCode start = DecodeTime(FixQuotes(parts[1]));
                        TimeCode end = DecodeTime(FixQuotes(parts[2]));
                        string text = FixQuotes(parts[5]);
                        var p = new Paragraph(start, end, text);
                        string actor = FixQuotes(parts[4]);
                        if (actor.Trim().Length > 0)
                            p.Actor = actor;

                        subtitle.Paragraphs.Add(p);
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

        private TimeCode DecodeTime(string part)
        {
            //00:00:07:12
            var parts = part.Split(":;.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

        private static string FixQuotes(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith("\"") && text.Length > 1)
                text = text.Substring(1);

            if (text.EndsWith("\"") && text.Length > 1)
                text = text.Substring(0, text.Length-1);

            return text.Replace("\"\"", "\"");
        }
    }
}
