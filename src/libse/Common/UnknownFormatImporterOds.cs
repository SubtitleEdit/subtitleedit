using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class UnknownFormatImporterOds
    {
        private static readonly XNamespace OfficeNs = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
        private static readonly XNamespace TableNs = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
        private static readonly XNamespace TextNs = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

        private const int MaxColumnRepeat = 256;

        public Subtitle AutoGuessImport(string fileName)
        {
            var lines = ReadLinesFromFile(fileName);
            if (lines == null || lines.Count < 2)
            {
                return new Subtitle();
            }

            return new UnknownFormatImporterCsv().AutoGuessImport(lines);
        }

        public List<string> ReadLinesFromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return null;
            }

            try
            {
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var entry = archive.GetEntry("content.xml");
                    if (entry == null)
                    {
                        return null;
                    }

                    using (var entryStream = entry.Open())
                    {
                        var doc = XDocument.Load(entryStream);
                        var table = doc.Descendants(TableNs + "table").FirstOrDefault();
                        if (table == null)
                        {
                            return null;
                        }

                        return ReadTable(table);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static List<string> ReadTable(XElement table)
        {
            var lines = new List<string>();
            foreach (var row in table.Elements(TableNs + "table-row"))
            {
                var cells = new List<string>();

                // covered-table-cell too, not just table-cell: a merged cell is written as one
                // table-cell plus one covered-table-cell per column it swallows, and skipping the
                // placeholders pulled every later column one to the left - so the text column of
                // a row with a merge was read as the end time.
                foreach (var cell in row.Elements().Where(e => e.Name == TableNs + "table-cell" || e.Name == TableNs + "covered-table-cell"))
                {
                    var value = cell.Name == TableNs + "covered-table-cell" ? string.Empty : GetCellValue(cell);
                    var repeat = ParseRepeat(cell.Attribute(TableNs + "number-columns-repeated")?.Value);

                    // Honour the repeat count for empty cells too: LibreOffice writes an
                    // *interior* run of blanks that way, and collapsing it to one cell shifted
                    // every following column left, so a row with a gap lost its text column.
                    // Trailing blank runs are still handled by the trim below, and ParseRepeat
                    // already caps the count, so this cannot blow up on a "blank to column 1024" row.
                    for (var i = 0; i < repeat; i++)
                    {
                        cells.Add(value ?? string.Empty);
                    }
                }

                // Trim trailing empty cells so a single huge "blank repeat" row doesn't drown out real content
                while (cells.Count > 0 && cells[cells.Count - 1].Length == 0)
                {
                    cells.RemoveAt(cells.Count - 1);
                }

                lines.Add(string.Join("\t", cells));
            }

            return lines;
        }

        private static int ParseRepeat(string value)
        {
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out var n) || n < 1)
            {
                return 1;
            }
            return Math.Min(n, MaxColumnRepeat);
        }

        private static string GetCellValue(XElement cell)
        {
            var valueType = (string)cell.Attribute(OfficeNs + "value-type") ?? string.Empty;

            if (valueType == "float" || valueType == "percentage" || valueType == "currency")
            {
                var v = (string)cell.Attribute(OfficeNs + "value");
                if (!string.IsNullOrEmpty(v))
                {
                    return v;
                }
            }

            if (valueType == "date")
            {
                var v = (string)cell.Attribute(OfficeNs + "date-value");
                if (!string.IsNullOrEmpty(v))
                {
                    return v;
                }
            }

            if (valueType == "time")
            {
                var v = (string)cell.Attribute(OfficeNs + "time-value");
                if (!string.IsNullOrEmpty(v))
                {
                    return ConvertIsoDurationToTimeCode(v);
                }
            }

            if (valueType == "boolean")
            {
                var v = (string)cell.Attribute(OfficeNs + "boolean-value");
                if (!string.IsNullOrEmpty(v))
                {
                    return string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) ? "TRUE" : "FALSE";
                }
            }

            return SanitizeForTab(GetText(cell));
        }

        private static string GetText(XElement cell)
        {
            var paragraphs = cell.Elements(TextNs + "p").ToList();
            if (paragraphs.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < paragraphs.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(GetInlineText(paragraphs[i]));
            }
            return sb.ToString();
        }

        private static string GetInlineText(XElement element)
        {
            var sb = new StringBuilder();
            foreach (var node in element.Nodes())
            {
                switch (node)
                {
                    case XText textNode:
                        sb.Append(textNode.Value);
                        break;
                    case XElement child when child.Name == TextNs + "tab":
                        sb.Append(' ');
                        break;
                    case XElement child when child.Name == TextNs + "line-break":
                        sb.Append(' ');
                        break;
                    case XElement child when child.Name == TextNs + "s":
                        var count = 1;
                        var c = (string)child.Attribute(TextNs + "c");
                        if (!string.IsNullOrEmpty(c) && int.TryParse(c, out var n) && n > 0)
                        {
                            count = n;
                        }
                        sb.Append(' ', count);
                        break;
                    case XElement child:
                        sb.Append(GetInlineText(child));
                        break;
                }
            }
            return sb.ToString();
        }

        private static string ConvertIsoDurationToTimeCode(string iso)
        {
            // ISO 8601 duration like "PT1H2M3.456S" — convert to "HH:MM:SS,fff"
            if (string.IsNullOrEmpty(iso) || !iso.StartsWith("PT", StringComparison.Ordinal))
            {
                return iso;
            }

            var hours = 0;
            var minutes = 0;
            var seconds = 0.0;
            var num = new StringBuilder();
            for (var i = 2; i < iso.Length; i++)
            {
                var ch = iso[i];
                if (char.IsDigit(ch) || ch == '.' || ch == ',')
                {
                    num.Append(ch == ',' ? '.' : ch);
                }
                else
                {
                    var text = num.ToString();
                    num.Clear();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (ch == 'H' && int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var h))
                    {
                        hours = h;
                    }
                    else if (ch == 'M' && int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var m))
                    {
                        minutes = m;
                    }
                    else if (ch == 'S' && double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var s))
                    {
                        seconds = s;
                    }
                }
            }

            var totalMs = (long)Math.Round(((hours * 3600) + (minutes * 60) + seconds) * 1000);
            return new TimeCode(totalMs).ToString(false);
        }

        private static string SanitizeForTab(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            return s.Replace('\t', ' ').Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
        }
    }
}
