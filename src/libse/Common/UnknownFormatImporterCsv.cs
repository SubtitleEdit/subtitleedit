using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class UnknownFormatImporterCsv
    {
        public class CsvLine
        {
            public string Start { get; set; }
            public string End { get; set; }
            public string Duration { get; set; }
            public string Text { get; set; }
            public string Character { get; set; }
            public bool Play { get; set; }
        }

        private readonly List<string> _startNames = new List<string> { "start", "start time", "in", "begin", "starttime", "start_time", "starttime", "startmillis", "start_millis", "startms", "start_ms", "startms", "startmilliseconds", "start_millisesonds", "startmilliseconds", "from", "fromtime", "from_ms", "fromms", "frommilliseconds", "from_milliseconds", "tc-in", "tc in", "show", "timecode", "timecode in", "start time (hh:mm:ss:ff)", "start time in milliseconds", "start tc", "start-tc", "tc start", "tc-start" };
        private readonly List<string> _endNames = new List<string> { "end", "end time", "out", "stop", "endtime", "end_time", "endtime", "endmillis", "end_millis", "endms", "end_ms", "endmilliseconds", "end_millisesonds", "endmilliseconds", "to", "totime", "to_ms", "toms", "tomilliseconds", "to_milliseconds", "tc-out", "tc out", "hide", "timecode out", "end time (hh:mm:ss:ff)", "end time in milliseconds", "end tc", "end-tc", "tc end", "tc-end" };
        private readonly List<string> _durationNames = new List<string> { "duration", "durationms", "dur" };
        private readonly List<string> _textNames = new List<string> { "text", "content", "value", "caption", "sentence", "dialog", "dialogue" };
        private readonly List<string> _characterNames = new List<string> { "speaker", "voice", "character", "role", "name", "actor", "rolle", "character name", "sprecher" };

        public Subtitle AutoGuessImport(List<string> lines)
        {
            if (lines == null || lines.Count < 2)
            {
                return new Subtitle();
            }

            if (string.IsNullOrWhiteSpace(lines[0]) || lines[0].Trim() == "Dialogue List,,")
            {
                // On a copy: the caller (UnknownFormatImporter) keeps using the list it passed in
                // for the other parsers when this one finds nothing, and dropping a line from
                // under it lost a subtitle.
                lines = new List<string>(lines);
                lines.RemoveAt(0);
            }

            if (lines.Count < 2)
            {
                return new Subtitle();
            }

            var commentHeaderIndex = FindCommentHeader(lines);
            if (commentHeaderIndex >= 0)
            {
                lines = lines.GetRange(commentHeaderIndex, lines.Count - commentHeaderIndex);
                lines[0] = lines[0].TrimStart().Substring(1);
            }

            var separator = commentHeaderIndex >= 0 ? ',' : DetectSeparator(lines);

            // Split the header exactly like the data rows (quote-aware) and trim each cell:
            // a naive Split broke on a quoted separator inside a header name - skewing every
            // column index - and an unpadded "Start, End, Text" failed to match at all.
            var headers = CsvUtil.CsvSplitLines(new List<string> { lines[0] }, separator)
                .FirstOrDefault()?
                .Select(p => p.Trim().Trim('"').ToLowerInvariant())
                .ToList() ?? new List<string>();
            if (commentHeaderIndex >= 0 && headers.Contains("position") && headers.Contains("comment"))
            {
                headers[headers.IndexOf("position")] = "start";
                headers[headers.IndexOf("comment")] = "text";
            }

            if (!HasValidHeader(headers))
            {
                return new Subtitle();
            }

            var csvLines = ReadCsvLines(headers, lines, separator);
            csvLines.RemoveAll(line => !IsSupportedTime(line.Start) ||
                                       !IsSupportedTime(line.End) ||
                                       !IsSupportedTime(line.Duration));
            ConvertPlainNumberTimes(csvLines);

            var subtitle = MakeSubtitle(csvLines);

            return subtitle;
        }

        internal static Subtitle ImportUnlabelledRows(IEnumerable<string> lines, bool integerTimes, bool useFrames)
        {
            var subtitle = new Subtitle();
            foreach (var fields in CsvUtil.CsvSplitLinesLazy(lines, ','))
            {
                if (fields.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                Paragraph cue;
                if (integerTimes && fields.Count == 3 &&
                    int.TryParse(fields[0], out var first) && int.TryParse(fields[1], out var last) &&
                    first >= 0 && last > first)
                {
                    var start = useFrames ? SubtitleFormat.FramesToMilliseconds(first) : first;
                    var end = useFrames ? SubtitleFormat.FramesToMilliseconds(last) : last;
                    cue = new Paragraph(fields[2].Replace("|", Environment.NewLine).Trim(), start, end);
                }
                else if (!integerTimes && fields.Count == 6 && int.TryParse(fields[0], out _) &&
                         bool.TryParse(fields[5], out var play) && fields[1].Contains(':') && fields[2].Contains(':'))
                {
                    var start = UnknownFormatImporterXml.TryParseTime(fields[1]);
                    var end = UnknownFormatImporterXml.TryParseTime(fields[2]);
                    if (!start.HasValue || !end.HasValue || end <= start)
                    {
                        return null;
                    }

                    cue = new Paragraph(fields[4].Trim(), start.Value, end.Value)
                    {
                        Actor = fields[3],
                        Effect = play ? "True" : "False",
                    };
                }
                else
                {
                    return null;
                }

                subtitle.Paragraphs.Add(cue);
            }

            if (subtitle.Paragraphs.Count < 2)
            {
                return null;
            }

            subtitle.Renumber();
            return subtitle;
        }

        private static readonly Regex NumberedLineHeader = new Regex(@"^line [1-9][0-9]*$", RegexOptions.Compiled);
        private static readonly Regex PlainNumberRegex = new Regex(@"^\d+([.,]\d+)?$", RegexOptions.Compiled);

        private static bool IsSupportedTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            // Only reject values that parse but cannot become a time (NaN, infinity, out of
            // range). Values in an unrecognised format ("01:23", "1.5s") keep their row, as
            // they always did - the text is still imported and the times are recalculated.
            if (TryParseNumber(value, out var number))
            {
                return !double.IsNaN(number) && !double.IsInfinity(number) &&
                       Math.Abs(number) <= TimeCode.MaxSupportedTotalMilliseconds;
            }

            return !TimeCode.IsOutOfRangeTimeCode(value);
        }

        /// <summary>
        /// Start/end/duration given as plain numbers ("1.5", "1,5", "1500") instead of time codes.
        /// Decimals are seconds; whole numbers are milliseconds when they would make cues last
        /// over 100 seconds on average, otherwise seconds. Columns already converted from an
        /// "_ms" header are time codes and leave this alone.
        /// </summary>
        private static void ConvertPlainNumberTimes(List<CsvLine> csvLines)
        {
            var values = csvLines.SelectMany(l => new[] { l.Start, l.End, l.Duration }).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
            if (values.Count == 0 || !values.All(v => PlainNumberRegex.IsMatch(v.Trim())))
            {
                return;
            }

            var factor = 1000.0;
            if (!values.Any(v => v.IndexOf('.') >= 0 || v.IndexOf(',') >= 0))
            {
                var durations = new List<double>();
                foreach (var l in csvLines)
                {
                    if (TryParseNumber(l.Duration, out var d))
                    {
                        durations.Add(d);
                    }
                    else if (TryParseNumber(l.Start, out var s) && TryParseNumber(l.End, out var e))
                    {
                        durations.Add(e - s);
                    }
                }

                if (durations.Count > 0 && durations.Average() > 100)
                {
                    factor = 1;
                }
            }

            csvLines.RemoveAll(l => ExceedsTimeLimit(l.Start, factor) ||
                                    ExceedsTimeLimit(l.End, factor) ||
                                    ExceedsTimeLimit(l.Duration, factor));
            foreach (var l in csvLines)
            {
                l.Start = ConvertNumber(l.Start, factor);
                l.End = ConvertNumber(l.End, factor);
                l.Duration = ConvertNumber(l.Duration, factor);
            }
        }

        private static bool ExceedsTimeLimit(string value, double factor)
        {
            return TryParseNumber(value, out var number) && number * factor > TimeCode.MaxSupportedTotalMilliseconds;
        }

        private static bool TryParseNumber(string value, out double number)
        {
            number = 0;
            return !string.IsNullOrWhiteSpace(value) &&
                   double.TryParse(value.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }

        private static string ConvertNumber(string value, double factor)
        {
            return TryParseNumber(value, out var number) ? new TimeCode(number * factor).ToString(false) : value;
        }

        private Subtitle MakeSubtitle(List<CsvLine> csvLines)
        {
            var isStartTimeFrames = DetectIsFrames(csvLines.Select(p => p.Start).ToList());
            var isEndTimeFrames = DetectIsFrames(csvLines.Select(p => p.End).ToList());
            var isDurationFrames = DetectIsFrames(csvLines.Select(p => p.Duration).ToList());
            FixStartTimeWithoutMs(csvLines, isStartTimeFrames);

            var subtitle = new Subtitle();
            foreach (var csvLine in csvLines)
            {
                double start;
                double end;
                double duration;
                var text = csvLine.Text ?? string.Empty;
                var character = csvLine.Character ?? string.Empty;
                var startText = csvLine.Start ?? string.Empty;
                var endText = csvLine.End ?? string.Empty;
                var durationText = csvLine.Duration ?? string.Empty;

                if (isStartTimeFrames)
                {
                    start = TimeCode.ParseHHMMSSFFToMilliseconds(startText);
                }
                else
                {
                    start = TimeCode.ParseToMilliseconds(startText);
                }

                if (isEndTimeFrames)
                {
                    end = TimeCode.ParseHHMMSSFFToMilliseconds(endText);
                }
                else
                {
                    end = TimeCode.ParseToMilliseconds(endText);
                }

                if (isDurationFrames)
                {
                    duration = TimeCode.ParseHHMMSSFFToMilliseconds(durationText);
                }
                else
                {
                    duration = TimeCode.ParseToMilliseconds(durationText);
                }

                if (duration > 0 && end == 0)
                {
                    end = start + duration;
                }

                var paragraph = new Paragraph(text.Trim(), start, end)
                {
                    Actor = character.Trim(),
                };

                subtitle.Paragraphs.Add(paragraph);
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();

            var isAllEndTimesZero = subtitle.Paragraphs.All(p => Math.Abs(p.EndTime.TotalMilliseconds) < 0.01);
            if (isAllEndTimesZero)
            {
                subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds, null, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
            }

            return subtitle;
        }

        private void FixStartTimeWithoutMs(List<CsvLine> csvLines, bool isStartTimeFrames)
        {
            if (csvLines.Count == 0)
            {
                // No data rows at all - e.g. an unterminated quote in the header line makes the
                // header swallow the rest of the file. The averages below divide by this count,
                // and the file-open path has no catch for the exception (DivideByZeroException).
                return;
            }

            var isEndTimeNull = csvLines.All(p => string.IsNullOrEmpty(p.End));
            if (isStartTimeFrames || !isEndTimeNull)
            {
                return;
            }

            var isStartTimeThreePart = csvLines.All(p => p.Start != null && p.Start.Split(TimeCode.TimeSplitChars, StringSplitOptions.RemoveEmptyEntries).Length == 3);
            if (!isStartTimeThreePart)
            {
                return;
            }

            var lengthsOfLastTimePart = csvLines.Select(p => p.Start.Split(TimeCode.TimeSplitChars, StringSplitOptions.RemoveEmptyEntries).Last().Length).ToList();
            if (!lengthsOfLastTimePart.All(p => p == 2))
            {
                return;
            }

            var startGaps = GetGaps(csvLines.Select(p => p.Start).ToArray());
            var avarageStartGap = startGaps.Sum() / startGaps.Length;
            // Text stays null when no text column was recognised (HasValidHeader only needs two
            // hits across all five name lists, so a text column is not required) - dereferencing
            // it threw straight out of the file-open path, which has no catch.
            var avarageTextLength = csvLines.Select(p => p.Text?.Length ?? 0).Sum() / csvLines.Count;
            if (avarageStartGap >= 500 || avarageTextLength <= 90)
            {
                return;
            }

            foreach (var line in csvLines)
            {
                line.Start += ":000";
            }
        }

        private double[] GetGaps(string[] strings)
        {
            if (strings == null || strings.Length < 2)
            {
                return Array.Empty<double>();
            }

            var gaps = new List<double>(strings.Length - 1);
            for (var i = 1; i < strings.Length; i++)
            {
                var start = TimeCode.ParseToMilliseconds(strings[i - 1]);
                var end = TimeCode.ParseToMilliseconds(strings[i]);
                gaps.Add(end - start);
            }

            return gaps.ToArray();
        }

        private bool DetectIsFrames(List<string> toList)
        {
            // Blank values carry no information either way - a single empty cell, or the blank row
            // a trailing newline produces, used to veto the whole column and silently reinterpret
            // HH:MM:SS:FF as HH:MM:SS.ms (12 frames became 12 ms). Decide on the values that are
            // actually present. Same rule as the import window's own detection.
            var considered = 0;
            foreach (var s in toList)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    continue;
                }

                considered++;
                var parts = s.Split(TimeCode.TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4 || parts[3].Trim().Length != 2)
                {
                    return false;
                }
            }

            return considered > 0;
        }

        private List<CsvLine> ReadCsvLines(List<string> headers, List<string> lines, char separator)
        {
            var startIndex = headers.IndexOf(_startNames.FirstOrDefault(headers.Contains));
            var endIndex = headers.IndexOf(_endNames.FirstOrDefault(headers.Contains));
            var durationIndex = headers.IndexOf(_durationNames.FirstOrDefault(headers.Contains));
            var textIndex = headers.IndexOf(_textNames.FirstOrDefault(headers.Contains));
            var lineIndexes = textIndex < 0 && headers.Contains("line 1")
                ? headers.Select((header, index) => (header, index))
                    .Where(column => NumberedLineHeader.IsMatch(column.header))
                    .Select(column => column.index)
                    .ToList()
                : new List<int>();
            var characterIndex = headers.IndexOf(_characterNames.FirstOrDefault(headers.Contains));
            var isHeader = true;
            var csvLines = new List<CsvLine>();
            var linesArray = CsvUtil.CsvSplitLines(lines, separator);

            foreach (var fields in linesArray)
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                var csvLine = new CsvLine();
                if (startIndex >= 0 && startIndex < fields.Count)
                {
                    csvLine.Start = fields[startIndex];
                    if (!string.IsNullOrEmpty(csvLine.Start) &&
                        !csvLine.Start.Contains(',') &&
                        !csvLine.Start.Contains('.') &&
                        !csvLine.Start.Contains(':') &&
                        !csvLine.Start.Contains(';') &&
                        TryParseNumber(csvLine.Start, out var n) && Math.Abs(n) <= TimeCode.MaxSupportedTotalMilliseconds)
                    {
                        var msNames = new List<string> { "start time in milliseconds", "startmillis", "start_millis", "startms", "start_ms", "startms", "startmilliseconds", "start_millisesonds", "startmilliseconds", "from_ms", "fromms", "frommilliseconds", "from_milliseconds" };
                        if (msNames.Contains(headers[startIndex].ToLowerInvariant()))
                        {
                            csvLine.Start = new TimeCode(n).ToString(false);
                        }
                    }
                }

                if (endIndex >= 0 && endIndex < fields.Count)
                {
                    csvLine.End = fields[endIndex];
                    if (!string.IsNullOrEmpty(csvLine.End) &&
                        !csvLine.End.Contains(',') &&
                        !csvLine.End.Contains('.') &&
                        !csvLine.End.Contains(':') &&
                        !csvLine.End.Contains(';') &&
                        TryParseNumber(csvLine.End, out var n) && Math.Abs(n) <= TimeCode.MaxSupportedTotalMilliseconds)
                    {
                        var msNames = new List<string> { "end time in milliseconds", "endmillis", "end_millis", "endms", "end_ms", "endmilliseconds", "end_millisesonds", "endmilliseconds", "to_ms", "toms", "tomilliseconds", "to_milliseconds" };
                        if (msNames.Contains(headers[endIndex].ToLowerInvariant()))
                        {
                            csvLine.End = new TimeCode(n).ToString(false);
                        }
                    }
                }

                if (durationIndex >= 0 && durationIndex < fields.Count)
                {
                    csvLine.Duration = fields[durationIndex];
                    if (!string.IsNullOrEmpty(csvLine.Duration) &&
                        !csvLine.Duration.Contains(',') &&
                        !csvLine.Duration.Contains('.') &&
                        !csvLine.Duration.Contains(':') &&
                        !csvLine.Duration.Contains(';') &&
                        TryParseNumber(csvLine.Duration, out var n) && Math.Abs(n) <= TimeCode.MaxSupportedTotalMilliseconds)
                    {
                        var msNames = new List<string> { "durationms" };
                        if (msNames.Contains(headers[durationIndex].ToLowerInvariant()))
                        {
                            csvLine.Duration = new TimeCode(n).ToString(false);
                        }
                    }
                }

                if (textIndex >= 0 && textIndex < fields.Count)
                {
                    csvLine.Text = fields[textIndex];
                }

                if (lineIndexes.Count > 0)
                {
                    csvLine.Text = string.Join(Environment.NewLine, lineIndexes
                        .Where(index => index < fields.Count && !string.IsNullOrWhiteSpace(fields[index]))
                        .Select(index => fields[index]));
                }

                if (characterIndex >= 0 && characterIndex < fields.Count)
                {
                    csvLine.Character = fields[characterIndex];
                }

                csvLines.Add(csvLine);
            }

            return csvLines;
        }

        private bool HasValidHeader(List<string> headers)
        {
            var hits = 0;
            foreach (var header in headers)
            {
                if (_startNames.Contains(header))
                {
                    hits++;
                }
                else if (_endNames.Contains(header))
                {
                    hits++;
                }
                else if (_durationNames.Contains(header))
                {
                    hits++;
                }
                else if (_textNames.Contains(header))
                {
                    hits++;
                }
                else if (_characterNames.Contains(header))
                {
                    hits++;
                }
            }

            return hits >= 2;
        }

        private static int FindCommentHeader(List<string> lines)
        {
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index].TrimStart();
                if (line.Length == 0)
                {
                    continue;
                }

                if (!line.StartsWith('#'))
                {
                    break;
                }

                // Position and duration columns identify marker lists whose CSV header
                // is part of the introductory comment block.
                var fields = CsvUtil.CsvSplitLinesLazy(new[] { line.Substring(1) }, ',').FirstOrDefault();
                if (fields != null && HasField("Position") && HasField("Duration") && HasField("Comment"))
                {
                    return index;
                }

                bool HasField(string name) => fields.Any(field => field.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            return -1;
        }

        private static char DetectSeparator(List<string> lines)
        {
            const char defaultSeparator = ',';
            char[] potentialSeparators = { defaultSeparator, ';', '\t' };

            if (lines == null || lines.Count < 2)
            {
                return defaultSeparator;
            }

            var noSpaceLines = lines.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            foreach (var separator in potentialSeparators)
            {
                var rows = CsvUtil.CsvSplitLinesLazy(noSpaceLines, separator).Take(2).ToList();
                if (rows.Count == 2 && rows[0].Count > 1 && rows[1].Count == rows[0].Count)
                {
                    return separator;
                }
            }

            return defaultSeparator;
        }
    }
}
