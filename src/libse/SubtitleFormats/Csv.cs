using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Csv : SubtitleFormat
    {
        private const string Separator = ";";
        private static readonly Regex CsvLine = new Regex(@"^""?\d+""?" + Separator + @"""?\d+""?" + Separator + @"""?\d+""?" + Separator + @"""?[^""]*""?$", RegexOptions.Compiled);

        public override string Extension => ".csv";

        public override string Name => "Csv";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var fine = 0;
            var failed = 0;
            foreach (var line in lines)
            {
                if (CsvLine.IsMatch(line))
                {
                    fine++;
                }
                else
                {
                    failed++;
                }
            }
            return fine > failed;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "{1}{0}{2}{0}{3}{0}\"{4}\"";
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(format, Separator, "Number", "Start time in milliseconds", "End time in milliseconds", "Text"));
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(format, Separator, p.Number, (long)Math.Round(p.StartTime.TotalMilliseconds, MidpointRounding.AwayFromZero), (long)Math.Round(p.EndTime.TotalMilliseconds, MidpointRounding.AwayFromZero), p.Text.Replace(Environment.NewLine, "\n")));
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
                    var parts = line.Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var start = Convert.ToInt32(Utilities.FixQuotes(parts[1]));
                            var end = Convert.ToInt32(Utilities.FixQuotes(parts[2]));
                            var text = Utilities.FixQuotes(parts[3]);
                            p = new Paragraph(text, start, end);
                            subtitle.Paragraphs.Add(p);
                            continuation = parts[3].StartsWith('"') && !parts[3].EndsWith('"');
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
    }
}
