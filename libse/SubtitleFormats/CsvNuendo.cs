using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CsvNuendo : SubtitleFormat
    {
        private static readonly Regex CsvLine = new Regex("^.*,[+\\d+:]+,[+\\d+:]+,\".+", RegexOptions.Compiled);
        private const string LineFormat = "{1}{0}{2}{0}{3}{0}{4}";
        private static string Header = string.Format(LineFormat, ",", "\"Character\"", "\"Timecode In\"", "\"Timecode Out\"", "\"Dialogue\"");

        public override string Extension => ".csv";

        public override string Name => "Csv Nuendo";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0)
            {
                if (lines[0].Contains(Header))
                {
                    return true;
                }
            }
            int matchCount = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                if (CsvLine.IsMatch(lines[i]))
                {
                    matchCount++;
                }
            }
            return matchCount > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = string.IsNullOrEmpty(p.Text) ? string.Empty : "\"" + p.Text.Replace("\"", "\"\"").Replace(Environment.NewLine, "\n") + "\"";
                string actor = string.IsNullOrEmpty(p.Actor) ? string.Empty : "\"" + p.Actor.Replace(",", " ").Replace("\"", string.Empty) + "\"";
                sb.AppendLine(string.Format(LineFormat, ",", actor, p.StartTime.ToHHMMSSFF(), p.EndTime.ToHHMMSSFF(), text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool continuation = false;
            Paragraph p = null;

            // Token indices
            const int Actor = 0;
            const int StartTime = 1;
            const int EndTime = 2;
            const int Text = 3;

            foreach (string line in lines)
            {
                if (CsvLine.IsMatch(line))
                {
                    string[] tokens = line.Split(',');
                    try
                    {
                        var actor = Utilities.FixQuotes(tokens[Actor]);
                        var start = DecodeTime(tokens[StartTime]);
                        var end = DecodeTime(tokens[EndTime]);
                        string text = Utilities.FixQuotes(tokens[Text]);
                        p = new Paragraph(start, end, text);
                        if (!string.IsNullOrEmpty(actor))
                        {
                            p.Actor = actor;
                        }

                        subtitle.Paragraphs.Add(p);
                        continuation = tokens[Text].StartsWith('"') && !tokens[Text].EndsWith('"');
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    if (continuation)
                    {
                        if (p.Text.Length < 300)
                        {
                            p.Text = (p.Text + Environment.NewLine + line.TrimEnd('"')).Trim();
                        }

                        continuation = !line.TrimEnd().EndsWith('"');
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }

        private TimeCode DecodeTime(string s)
        {
            return DecodeTimeCodeFramesFourParts(s.Split(new char[] { ':' }));
        }
    }
}
