using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Csv3 : SubtitleFormat
    {
        private const string Separator = ",";

        //01:00:10:03,01:00:15:25,"I thought I should let my sister-in-law know.", ""
        private static readonly Regex CsvLine = new Regex(@"^\d\d:\d\d:\d\d:\d\d" + Separator + @"\d\d:\d\d:\d\d:\d\d" + Separator, RegexOptions.Compiled);

        public override string Extension => ".csv";

        public override string Name => "Csv3";

        public override bool IsMine(List<string> lines, string fileName)
        {
            int fine = 0;
            int failed = 0;
            bool continuation = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("$FontName", StringComparison.Ordinal) || line.StartsWith("$ColorIndex1", StringComparison.Ordinal))
                {
                    return false;
                }

                Match m = null;
                if (line.Length > 8 && line[2] == ':')
                {
                    m = CsvLine.Match(line);
                }

                if (m != null && m.Success)
                {
                    // Reject multi-column CSVs (Csv3 expects start, end, text, optional secondary text).
                    var fields = CsvUtil.CsvSplit(line, false, out _, ',');
                    if (fields.Length > 4)
                    {
                        return false;
                    }

                    fine++;
                    string s = line.Remove(0, m.Length);
                    continuation = s.StartsWith('"');
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    if (continuation)
                    {
                        continuation = false;
                    }
                    else
                    {
                        failed++;
                    }
                }
            }
            if (failed > 20)
            {
                return false;
            }

            return fine > failed;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "{1}{0}{2}{0}\"{3}\"{0}\"{4}\"";
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(format, Separator, "Start time (hh:mm:ss:ff)", "End time (hh:mm:ss:ff)", "Line 1", "Line 2"));
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // The format has two text columns, so rebalance at THREE lines - the old
                // ">3" guard let a three-line subtitle through and then wrote only the first
                // two lines, silently dropping the third.
                var arr = p.Text.Trim().SplitToLines();
                if (arr.Count > 2)
                {
                    string s = Utilities.AutoBreakLine(p.Text);
                    arr = s.Trim().SplitToLines();
                }

                string line1 = arr[0];
                string line2 = string.Empty;
                if (arr.Count > 1)
                {
                    // Anything that still does not fit in two lines is appended to the second
                    // column rather than thrown away.
                    line2 = string.Join(" ", arr.Skip(1));
                }

                line1 = line1.Replace("\"", "\"\"");
                line2 = line2.Replace("\"", "\"\"");
                sb.AppendLine(string.Format(format, Separator, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), line1, line2));
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            char[] splitChars = { '.', ':' };
            foreach (string line in lines)
            {
                Match m = CsvLine.Match(line);
                if (m.Success)
                {
                    string[] parts = line.Substring(0, m.Length).Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        try
                        {
                            var start = DecodeTimeCodeFrames(parts[0], splitChars);
                            var end = DecodeTimeCodeFrames(parts[1], splitChars);
                            string text = ReadText(line.Remove(0, m.Length));
                            var p = new Paragraph(start, end, text);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static string ReadText(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return string.Empty;
            }

            // No pre-collapse of "" here: ToText escapes an embedded quote by doubling it, and
            // collapsing before the walk below left the loop unable to tell an escaped quote
            // from a field delimiter - so quoted words came back with their quotes eaten.
            var sb = new StringBuilder();
            csv = csv.Trim();
            if (csv.StartsWith('"'))
            {
                csv = csv.Remove(0, 1);
            }

            if (csv.EndsWith('"'))
            {
                csv = csv.Remove(csv.Length - 1, 1);
            }

            bool isBreak = false;
            for (int i = 0; i < csv.Length; i++)
            {
                var s = csv[i];
                if (s == '"' && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    // An escaped quote - emit one and consume BOTH characters.
                    sb.Append('"');
                    i++;
                }
                else if (s == '"')
                {
                    if (isBreak)
                    {
                        isBreak = false;
                    }
                    else if (i == 0 || i == csv.Length - 1 || sb.ToString().EndsWith(Environment.NewLine, StringComparison.Ordinal))
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

    }
}
