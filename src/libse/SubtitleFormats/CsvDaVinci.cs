using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CsvDaVinci : SubtitleFormat
    {
        private const string LineFormat = "\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"";

        public override string Extension => ".csv";

        public override string Name => "Csv DaVinci";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                var text = string.IsNullOrEmpty(p.Text) ? string.Empty : p.Text.Replace("\"", "\"\"");
                var actor = string.IsNullOrEmpty(p.Actor) ? string.Empty : p.Actor.Replace(",", " ").Replace("\"", string.Empty);
                sb.AppendLine(
                    string.Format(
                        LineFormat,
                        (index + 1).ToString(CultureInfo.InvariantCulture),
                        p.StartTime.ToHHMMSSFF(),
                        p.EndTime.ToHHMMSSFF(),
                        actor,
                        text,
                        bool.TryParse(p.Effect, out var s) ? s.ToString().ToUpperInvariant() : "False"));
            }

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var csvLines = ReadCsvLines(lines, ',');
            foreach (var line in csvLines)
            {
                if (ParseTimeCode(line.Start, out var start) &&
                    ParseTimeCode(line.End, out var end))
                {
                    var p = new Paragraph(start, end, line.Text)
                    {
                        Actor = line.Character,
                        Effect = line.Play.ToString().ToUpperInvariant()
                    };
                    subtitle.Paragraphs.Add(p);
                }
                else
                {
                    _errorCount++;
                    if (_errorCount > 10)
                    {
                        return;
                    }
                }
            }

            subtitle.Renumber();
        }

        private static bool ParseTimeCode(string s, out TimeCode tc)
        {
            if (string.IsNullOrEmpty(s))
            {
                tc = new TimeCode();
                return false;
            }

            var arr = s.Split(new[] { ':', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 4)
            {
                try
                {
                    tc = DecodeTimeCodeFramesFourParts(arr);
                    return true;
                }
                catch
                {
                    tc = new TimeCode(0, 0, 0, 0);
                    return false;
                }
            }

            tc = new TimeCode(0, 0, 0, 0);
            return false;
        }

        private List<UnknownFormatImporterCsv.CsvLine> ReadCsvLines(List<string> lines, char separator)
        {
            const int startIndex = 1;
            const int endIndex = 2;
            const int characterIndex = 3;
            const int textIndex = 4;
            const int playIndex = 5;
            var csvLines = new List<UnknownFormatImporterCsv.CsvLine>();
            var linesArray = CsvUtil.CsvSplitLines(lines.Where(p => !string.IsNullOrEmpty(p)).ToList(), separator);

            foreach (var fields in linesArray)
            {
                var csvLine = new UnknownFormatImporterCsv.CsvLine();
                if (startIndex < fields.Count)
                {
                    csvLine.Start = fields[startIndex];
                }

                if (endIndex < fields.Count)
                {
                    csvLine.End = fields[endIndex];
                }

                if (characterIndex < fields.Count)
                {
                    csvLine.Character = fields[characterIndex];
                }

                if (textIndex < fields.Count)
                {
                    csvLine.Text = fields[textIndex];
                }

                if (playIndex < fields.Count && bool.TryParse(fields[textIndex], out var doPlay))
                {
                    csvLine.Play = doPlay;
                }

                csvLines.Add(csvLine);
            }

            return csvLines;
        }
    }
}
