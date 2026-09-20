using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// A spreadsheet friendly csv: every field is quoted, the first row names the columns, and the
    /// time codes carry milliseconds so nothing is lost when the file is edited in Excel/Sheets and
    /// read back (issue #14321).
    /// </summary>
    public class CsvExcel : SubtitleFormat
    {
        private const char Separator = ',';
        private const int NumberIndex = 0;
        private const int StartIndex = 1;
        private const int EndIndex = 2;
        private const int TextIndex = 3;
        private const int ActorIndex = 4;
        private const int ForcedIndex = 5;

        private static readonly string[] HeaderFields = { "Number", "Start time", "End time", "Text", "Actor", "Forced" };

        public override string Extension => ".csv";

        public override string Name => "Csv Excel";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && !fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var first = lines.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p));
            if (first == null || !IsHeaderLine(first))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(Separator.ToString(), HeaderFields.Select(Quote)));
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Join(Separator.ToString(),
                    Quote(p.Number.ToString(CultureInfo.InvariantCulture)),
                    Quote(EncodeTimeCode(p.StartTime)),
                    Quote(EncodeTimeCode(p.EndTime)),
                    Quote(p.Text ?? string.Empty),
                    Quote(p.Actor ?? string.Empty),
                    Quote(p.Forced ? "True" : "False")));
            }

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var rows = CsvUtil.CsvSplitLines(lines, Separator);
            foreach (var fields in rows)
            {
                if (fields.Count == 0)
                {
                    continue;
                }

                if (IsHeaderRow(fields))
                {
                    continue;
                }

                if (fields.Count <= EndIndex ||
                    !ParseTimeCode(fields[StartIndex], out var start) ||
                    !ParseTimeCode(fields[EndIndex], out var end))
                {
                    _errorCount++;
                    if (_errorCount > 20)
                    {
                        return;
                    }

                    continue;
                }

                var p = new Paragraph(start, end, fields.Count > TextIndex ? fields[TextIndex] : string.Empty);
                if (fields.Count > ActorIndex && !string.IsNullOrWhiteSpace(fields[ActorIndex]))
                {
                    p.Actor = fields[ActorIndex];
                }

                if (fields.Count > ForcedIndex)
                {
                    p.Forced = IsTrue(fields[ForcedIndex]);
                }

                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        private static bool IsHeaderLine(string line)
        {
            var fields = CsvUtil.CsvSplit(line, false, out _, Separator);
            return IsHeaderRow(fields.ToList());
        }

        private static bool IsHeaderRow(IReadOnlyList<string> fields)
        {
            if (fields.Count < HeaderFields.Length)
            {
                return false;
            }

            for (var i = 0; i < HeaderFields.Length; i++)
            {
                if (!string.Equals(fields[i].Trim(), HeaderFields[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsTrue(string value)
        {
            var s = value.Trim();
            return s.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                   s.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                   s.Equals("Forced", StringComparison.OrdinalIgnoreCase) ||
                   s == "1";
        }

        /// <summary>
        /// Reads "hh:mm:ss.mmm" (also with a comma as decimal separator) - and "hh:mm:ss:ff", so a
        /// file where the milliseconds were replaced by frames still loads.
        /// </summary>
        private static bool ParseTimeCode(string s, out TimeCode tc)
        {
            tc = new TimeCode();
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            var arr = s.Trim().Split(new[] { ':', ';', '.', ',' }, StringSplitOptions.None);
            if (arr.Length != 4 ||
                !int.TryParse(arr[0], NumberStyles.None, CultureInfo.InvariantCulture, out var hours) ||
                !int.TryParse(arr[1], NumberStyles.None, CultureInfo.InvariantCulture, out var minutes) ||
                !int.TryParse(arr[2], NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) ||
                !int.TryParse(arr[3], NumberStyles.None, CultureInfo.InvariantCulture, out var last))
            {
                return false;
            }

            var trimmed = s.Trim();
            var separatorBeforeLast = trimmed[trimmed.Length - arr[3].Length - 1];
            int milliseconds;
            if (separatorBeforeLast == ':' || separatorBeforeLast == ';')
            {
                milliseconds = FramesToMillisecondsMax999(last);
            }
            else
            {
                // A decimal fraction, so ".05" is 50 ms and not 5 ms - the writer always emits
                // three digits, but a hand-edited (or spreadsheet-reformatted) file may not.
                var fraction = arr[3].Length > 3 ? arr[3].Substring(0, 3) : arr[3].PadRight(3, '0');
                milliseconds = int.Parse(fraction, CultureInfo.InvariantCulture);
            }

            tc = new TimeCode(hours, minutes, seconds, milliseconds);
            return true;
        }
    }
}
