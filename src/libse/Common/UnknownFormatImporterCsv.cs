using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly List<string> _startNames = new List<string> { "start", "start time", "in", "begin", "starttime", "start_time", "starttime", "startmillis", "start_millis", "startms", "start_ms", "startms", "startmilliseconds", "start_millisesonds", "startmilliseconds", "from", "fromtime", "from_ms", "fromms", "frommilliseconds", "from_milliseconds", "tc-in", "tc in", "show", "timecode" };
        private readonly List<string> _endNames = new List<string> { "end", "end time", "out", "stop", "endtime", "end_time", "endtime", "endmillis", "end_millis", "endms", "end_ms", "endmilliseconds", "end_millisesonds", "endmilliseconds", "to", "totime", "to_ms", "toms", "tomilliseconds", "to_milliseconds", "tc-out", "tc out", "hide" };
        private readonly List<string> _durationNames = new List<string> { "duration", "durationms", "dur" };
        private readonly List<string> _textNames = new List<string> { "text", "content", "value", "caption", "sentence", "dialog", "dialogue" };
        private readonly List<string> _characterNames = new List<string> { "speaker", "voice", "character", "role", "name", "actor", "rolle", "character name" };

        public Subtitle AutoGuessImport(List<string> lines)
        {
            if (lines == null || lines.Count < 2)
            {
                return new Subtitle();
            }

            if (string.IsNullOrWhiteSpace(lines[0]) || lines[0].Trim() == "Dialogue List,,")
            {
                lines.RemoveAt(0);
            }

            if (lines.Count < 2)
            {
                return new Subtitle();
            }

            var separator = DetectSeparator(lines);

            var headers = lines[0].RemoveChar('"').ToLowerInvariant().Split(separator).ToList();
            if (!HasValidHeader(headers))
            {
                return new Subtitle();
            }

            var csvLines = ReadCsvLines(headers, lines, separator);

            var subtitle = MakeSubtitle(csvLines);

            return subtitle;
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
            var avarageTextLength = csvLines.Select(p => p.Text.Length).Sum() / csvLines.Count;
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
            var gaps = new List<double>();
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
            foreach (var s in toList)
            {
                if (s == null)
                {
                    return false;
                }

                var parts = s.Split(TimeCode.TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4 || parts[3].Trim().Length != 2)
                {
                    return false;
                }
            }

            return true;
        }

        private List<CsvLine> ReadCsvLines(List<string> headers, List<string> lines, char separator)
        {
            var startIndex = headers.IndexOf(_startNames.FirstOrDefault(headers.Contains));
            var endIndex = headers.IndexOf(_endNames.FirstOrDefault(headers.Contains));
            var durationIndex = headers.IndexOf(_durationNames.FirstOrDefault(headers.Contains));
            var textIndex = headers.IndexOf(_textNames.FirstOrDefault(headers.Contains));
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
                        double.TryParse(csvLine.Start, out var n))
                    {
                        var msNames = new List<string> { "startmillis", "start_millis", "startms", "start_ms", "startms", "startmilliseconds", "start_millisesonds", "startmilliseconds", "from_ms", "fromms", "frommilliseconds", "from_milliseconds" };
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
                        double.TryParse(csvLine.End, out var n))
                    {
                        var msNames = new List<string> { "endmillis", "end_millis", "endms", "end_ms", "endmilliseconds", "end_millisesonds", "endmilliseconds", "to_ms", "toms", "tomilliseconds", "to_milliseconds" };
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
                        double.TryParse(csvLine.Duration, out var n))
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
                var firstLineParts = noSpaceLines[0].Split(separator);
                var secondLineParts = noSpaceLines[1].Split(separator);

                if (firstLineParts.Length > 1 && secondLineParts.Length == firstLineParts.Length)
                {
                    return separator;
                }
            }

            return defaultSeparator;
        }
    }
}
