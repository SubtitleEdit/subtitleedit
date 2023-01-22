using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CsvNuendo : SubtitleFormat
    {
        private static readonly Regex CsvLine = new Regex("^(\"(.*)\")*,\\d+:\\d+:\\d+:\\d+,\\d+:\\d+:\\d+:\\d+,(\"(.*)\")*", RegexOptions.Compiled);
        private const string LineFormat = "{1}{0}{2}{0}{3}{0}{4}";
        private static readonly string Header = string.Format(LineFormat, ",", "\"Character\"", "\"Timecode In\"", "\"Timecode Out\"", "\"Dialogue\"");

        public override string Extension => ".csv";

        public override string Name => "Csv Nuendo";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var fine = 0;
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(line);
                if (line.IndexOf(':') > 0 && CsvLine.IsMatch(line))
                {
                    fine++;
                }
            }

            return fine > 0 && sb.ToString().Contains(Header);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            foreach (var p in subtitle.Paragraphs)
            {
                var text = string.IsNullOrEmpty(p.Text) ? string.Empty : "\"" + p.Text.Replace("\"", "\"\"").Replace(Environment.NewLine, "\n") + "\"";
                var actor = string.IsNullOrEmpty(p.Actor) ? string.Empty : "\"" + p.Actor.Replace(",", " ").Replace("\"", string.Empty) + "\"";
                sb.AppendLine(string.Format(LineFormat, ",", actor, p.StartTime.ToHHMMSSFF(), p.EndTime.ToHHMMSSFF(), text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var continuation = false;
            Paragraph p = null;
            foreach (var line in lines)
            {
                if (CsvLine.IsMatch(line))
                {
                    var parts = CsvUtil.CsvSplit(line, false, out var con);
                    continuation = con;
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var actor = parts[0];
                            var start = DecodeTime(parts[1]);
                            var end = DecodeTime(parts[2]);
                            var text = parts[3];
                            p = new Paragraph(start, end, text);
                            if (!string.IsNullOrEmpty(actor))
                            {
                                p.Actor = actor;
                            }

                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else
                {
                    if (continuation)
                    {
                        var parts = CsvUtil.CsvSplit(line, true, out var con);
                        continuation = con;
                        if (parts.Length == 1 && p.Text.Length < 300)
                        {
                            p.Text = (p.Text + Environment.NewLine + parts[0]).Trim();
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTime(string s)
        {
            return DecodeTimeCodeFramesFourParts(s.Split(':'));
        }
    }
}
