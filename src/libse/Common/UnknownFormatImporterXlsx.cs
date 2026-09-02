using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class UnknownFormatImporterXlsx
    {
        private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace RelationshipsNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PackageRelationshipsNs = "http://schemas.openxmlformats.org/package/2006/relationships";

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
                    var sharedStrings = ReadSharedStrings(archive);
                    var sheetEntry = FindFirstSheet(archive);
                    if (sheetEntry == null)
                    {
                        return null;
                    }

                    return ReadSheet(sheetEntry, sharedStrings);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The worksheet the user sees first: the first &lt;sheet&gt; in xl/workbook.xml, resolved
        /// through the workbook relationships. That is not necessarily sheet1.xml - moving a sheet
        /// in Excel reorders the workbook entries but never renames the parts - so picking the
        /// first worksheet file imported the wrong sheet. Falls back to the first worksheet part
        /// when the workbook or its relationships cannot be read.
        /// </summary>
        private static ZipArchiveEntry FindFirstSheet(ZipArchive archive)
        {
            var worksheets = archive.Entries
                .Where(e => e.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase) &&
                            e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (worksheets.Count <= 1)
            {
                return worksheets.FirstOrDefault();
            }

            try
            {
                var workbookEntry = archive.GetEntry("xl/workbook.xml");
                var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
                if (workbookEntry != null && relsEntry != null)
                {
                    string relationshipId;
                    using (var stream = workbookEntry.Open())
                    {
                        relationshipId = (string)XDocument.Load(stream).Root?
                            .Element(Ns + "sheets")?
                            .Elements(Ns + "sheet")
                            .FirstOrDefault()?
                            .Attribute(RelationshipsNs + "id");
                    }

                    if (!string.IsNullOrEmpty(relationshipId))
                    {
                        using (var stream = relsEntry.Open())
                        {
                            var target = XDocument.Load(stream).Root?
                                .Elements(PackageRelationshipsNs + "Relationship")
                                .FirstOrDefault(r => (string)r.Attribute("Id") == relationshipId)?
                                .Attribute("Target")?.Value;
                            if (!string.IsNullOrEmpty(target))
                            {
                                var fullName = "xl/" + target.TrimStart('/').Replace("../", string.Empty);
                                var match = worksheets.FirstOrDefault(e => e.FullName.Equals(fullName, StringComparison.OrdinalIgnoreCase));
                                if (match != null)
                                {
                                    return match;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // fall through to the first worksheet part
            }

            return worksheets[0];
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var result = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return result;
            }

            using (var stream = entry.Open())
            {
                var doc = XDocument.Load(stream);
                if (doc.Root == null)
                {
                    return result;
                }

                foreach (var si in doc.Root.Elements(Ns + "si"))
                {
                    result.Add(GetStringValue(si));
                }
            }

            return result;
        }

        private static string GetStringValue(XElement element)
        {
            var sb = new StringBuilder();
            foreach (var t in element.Elements(Ns + "t"))
            {
                sb.Append(t.Value);
            }
            foreach (var r in element.Elements(Ns + "r"))
            {
                foreach (var t in r.Elements(Ns + "t"))
                {
                    sb.Append(t.Value);
                }
            }
            return sb.ToString();
        }

        private static List<string> ReadSheet(ZipArchiveEntry sheetEntry, List<string> sharedStrings)
        {
            var lines = new List<string>();
            using (var stream = sheetEntry.Open())
            {
                var doc = XDocument.Load(stream);
                var sheetData = doc.Root?.Element(Ns + "sheetData");
                if (sheetData == null)
                {
                    return lines;
                }

                foreach (var row in sheetData.Elements(Ns + "row"))
                {
                    var cells = new List<string>();
                    foreach (var c in row.Elements(Ns + "c"))
                    {
                        var reference = (string)c.Attribute("r") ?? string.Empty;
                        var columnIndex = ColumnIndexFromReference(reference);
                        while (cells.Count < columnIndex)
                        {
                            cells.Add(string.Empty);
                        }

                        cells.Add(GetCellValue(c, sharedStrings));
                    }

                    lines.Add(string.Join("\t", cells));
                }
            }

            return lines;
        }

        private static string GetCellValue(XElement cell, List<string> sharedStrings)
        {
            var type = (string)cell.Attribute("t") ?? string.Empty;

            if (type == "s")
            {
                var v = cell.Element(Ns + "v")?.Value;
                if (int.TryParse(v, out var index) && index >= 0 && index < sharedStrings.Count)
                {
                    return SanitizeForTab(sharedStrings[index]);
                }
                return string.Empty;
            }

            if (type == "inlineStr")
            {
                var inline = cell.Element(Ns + "is");
                return inline != null ? SanitizeForTab(GetStringValue(inline)) : string.Empty;
            }

            if (type == "str")
            {
                return SanitizeForTab(cell.Element(Ns + "v")?.Value ?? string.Empty);
            }

            if (type == "b")
            {
                var v = cell.Element(Ns + "v")?.Value;
                return v == "1" ? "TRUE" : "FALSE";
            }

            return cell.Element(Ns + "v")?.Value ?? string.Empty;
        }

        private static string SanitizeForTab(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            return s.Replace('\t', ' ').Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
        }

        private static int ColumnIndexFromReference(string reference)
        {
            if (string.IsNullOrEmpty(reference))
            {
                return 0;
            }

            var col = 0;
            foreach (var ch in reference)
            {
                if (ch >= 'A' && ch <= 'Z')
                {
                    col = col * 26 + (ch - 'A' + 1);
                }
                else if (ch >= 'a' && ch <= 'z')
                {
                    col = col * 26 + (ch - 'a' + 1);
                }
                else
                {
                    break;
                }
            }
            return col - 1;
        }
    }
}
