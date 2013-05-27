using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Nuendo : SubtitleFormat
    {
        private const string Seperator = ";";
        private const string NewLineSeperator = "   ";
        static readonly Regex CsvLine = new Regex(@"^\""[^""]*\""" + Seperator + @"\d\d:\d\d:\d\d:\d\d" + Seperator + @"\d\d:\d\d:\d\d:\d\d" + Seperator + @"[^;]*" + Seperator + @"[^;]*" + Seperator + @"[^;]*" + Seperator + @"\"".*\""$", RegexOptions.Compiled);
        static readonly Regex CsvLineEmpty = new Regex(@"^\""[^""]*\""" + Seperator + @"\d\d:\d\d:\d\d:\d\d" + Seperator + @"\d\d:\d\d:\d\d:\d\d" + Seperator + @"[^;]*" + Seperator + @"[^;]*" + Seperator + @"[^;]*" + Seperator + "$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".csv"; }
        }

        public override string Name
        {
            get { return "Nuendo"; }
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
                if (CsvLine.IsMatch(line) || CsvLineEmpty.IsMatch(line))
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

            sb.AppendLine(string.Format(format, Seperator, "Track name", "\"Timecode In\"", "\"Timecode Out\"", "\"Description\"", "\"Length\"", "Character", "Dialogue"));
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string actor = string.Empty;
                if (p.Actor != null)
                    actor = p.Actor;
                sb.AppendLine(string.Format(format, Seperator, title, EncodeTime(p.StartTime), EncodeTime(p.EndTime), "", "1", actor, p.Text.Replace(Environment.NewLine, NewLineSeperator).Trim()));
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
                if (CsvLine.IsMatch(line) || CsvLineEmpty.IsMatch(line))
                {
                    string[] parts = line.Split(Seperator.ToCharArray(),  StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 6)
                    try
                    {
                        TimeCode start = DecodeTime(Utilities.FixQuotes(parts[1]));
                        TimeCode end = DecodeTime(Utilities.FixQuotes(parts[2]));
                        string text = Utilities.FixQuotes(parts[5]);
                        if (text.Contains(NewLineSeperator))
                            text = text.Replace(NewLineSeperator, Environment.NewLine);
                        else
                            text = Utilities.AutoBreakLine(text);
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        var p = new Paragraph(start, end, text);
                        string actor = Utilities.FixQuotes(parts[4]);
                        if (actor.Trim().Length > 0)
                            p.Actor = actor;
                        p.Extra = p.Actor;

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

        public override bool HasStyleSupport
        {
            get { return true; }
        }

        public static List<string> GetStylesFromHeader(Subtitle subtitle)
        {
            var list = new List<string>();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (!string.IsNullOrEmpty(p.Actor))
                {
                    if (list.IndexOf(p.Actor) < 0)
                        list.Add(p.Actor);
                }
            }
            return list;
        }

    }
}
