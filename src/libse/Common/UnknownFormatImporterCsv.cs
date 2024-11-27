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
        }

        private readonly List<string> _startNames = new List<string> { "start", "start time", "in", "begin", "starttime", "start_time", "starttime", "startmillis", "start_millis", "startms", "start_ms", "startms", "startmilliseconds", "start_millisesonds", "startmilliseconds", "from", "fromtime", "from_ms", "fromms", "frommilliseconds", "from_milliseconds", "tc-in", "tc in" };
        private readonly List<string> _endNames = new List<string> { "end", "end time", "out", "stop", "endtime", "end_time", "endtime", "endmillis", "end_millis", "endms", "end_ms", "endmilliseconds", "end_millisesonds", "endmilliseconds", "to", "totime", "to_ms", "toms", "tomilliseconds", "to_milliseconds", "tc-out", "tc out" };
        private readonly List<string> _durationNames = new List<string> { "duration", "durationMs", "dur" };
        private readonly List<string> _textNames = new List<string> { "text", "content", "value", "caption", "sentence", "dialog" };
        private readonly List<string> _characterNames = new List<string> { "character", "role", "name", "actor", "rolle" };

        public Subtitle AutoGuessImport(List<string> lines)
        {
            if (lines == null || lines.Count < 2)
            {
                return new Subtitle();
            }

            var separator = DetectSeparator(lines);

            var headers = lines[0].ToLowerInvariant().Split(separator).ToList();
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

            var subtitle = new Subtitle();
            foreach (var csvLine in csvLines)
            {
                double start = 0;
                double end = 0;
                double duration = 0;
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

            return subtitle;
        }

        private bool DetectIsFrames(List<string> toList)
        {
            return false;
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
            foreach (var line in lines)
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                var fields = line.Split(separator).ToList();
                var csvLine = new CsvLine();
                if (startIndex >= 0)
                {
                    csvLine.Start = fields[startIndex];
                }
                
                if (endIndex >= 0)
                {
                    csvLine.End = fields[endIndex];
                }
                
                if (durationIndex >= 0)
                {
                    csvLine.Duration = fields[durationIndex];
                }
                
                if (textIndex >= 0)
                {
                    csvLine.Text = fields[textIndex];
                }
                
                if (characterIndex >= 0)
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

            return hits >= 3;
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
