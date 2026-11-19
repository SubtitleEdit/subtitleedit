using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Reads the main text of a Word .docx (word/document.xml) as lines, like
    /// <see cref="WordDocumentReader"/> does for .doc: one line per paragraph (line breaks
    /// split it further) and table rows flattened to tab separated lines.
    /// Tracked deletions and field codes are skipped.
    /// </summary>
    public static class WordDocxReader
    {
        private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private const long MaxFileSize = 50_000_000;
        private const long MaxDocumentXmlSize = 200_000_000;

        public static bool IsDocx(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) ||
                !fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(fileName))
            {
                return false;
            }

            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return fs.Length > 100 && fs.Length < MaxFileSize && fs.ReadByte() == 'P' && fs.ReadByte() == 'K';
                }
            }
            catch
            {
                return false;
            }
        }

        public static List<string> GetLines(string fileName)
        {
            if (!IsDocx(fileName))
            {
                return null;
            }

            try
            {
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var entry = archive.GetEntry("word/document.xml") ??
                                archive.Entries.FirstOrDefault(e => e.FullName.StartsWith("word/", StringComparison.OrdinalIgnoreCase) &&
                                                                    e.FullName.EndsWith("document.xml", StringComparison.OrdinalIgnoreCase));
                    if (entry == null || entry.Length > MaxDocumentXmlSize)
                    {
                        return null;
                    }

                    XDocument xml;
                    using (var entryStream = entry.Open())
                    {
                        xml = XDocument.Load(entryStream);
                    }

                    var body = xml.Root?.Element(W + "body");
                    if (body == null)
                    {
                        return null;
                    }

                    var lines = new List<string>();
                    AddBlocks(body, lines);
                    return lines;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Paragraphs, tables and content controls in document order.
        /// </summary>
        private static void AddBlocks(XElement container, List<string> lines)
        {
            foreach (var element in container.Elements())
            {
                if (element.Name == W + "p")
                {
                    lines.AddRange(GetParagraphLines(element));
                }
                else if (element.Name == W + "tbl")
                {
                    AddTable(element, lines);
                }
                else if (element.Name == W + "sdt")
                {
                    var content = element.Element(W + "sdtContent");
                    if (content != null)
                    {
                        AddBlocks(content, lines);
                    }
                }
                else if (element.Name == W + "customXml" || element.Name == W + "ins")
                {
                    AddBlocks(element, lines);
                }
            }
        }

        private static void AddTable(XElement table, List<string> lines)
        {
            foreach (var row in table.Elements(W + "tr"))
            {
                var cells = new List<List<string>>();
                foreach (var cell in row.Elements(W + "tc"))
                {
                    var cellLines = new List<string>();
                    AddBlocks(cell, cellLines);
                    // a nested table ends with an empty separator line - empty paragraphs before
                    // text are kept, they place the text on the right line (start/end rows)
                    while (cellLines.Count > 1 && cellLines[cellLines.Count - 1].Length == 0)
                    {
                        cellLines.RemoveAt(cellLines.Count - 1);
                    }

                    if (cellLines.Count == 0)
                    {
                        cellLines.Add(string.Empty);
                    }

                    cells.Add(cellLines);
                }

                if (cells.Count > 0)
                {
                    WordDocumentReader.AddRow(lines, cells);
                }
            }
        }

        private static List<string> GetParagraphLines(XElement paragraph)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            var inFieldCode = false;
            foreach (var element in paragraph.Descendants())
            {
                var name = element.Name;
                if (name.Namespace != W || IsInsideDeletionOrNestedParagraph(element, paragraph))
                {
                    continue;
                }

                switch (name.LocalName)
                {
                    case "fldChar":
                        var type = (string)element.Attribute(W + "fldCharType");
                        if (type == "begin")
                        {
                            inFieldCode = true;
                        }
                        else if (type == "separate" || type == "end")
                        {
                            inFieldCode = false;
                        }

                        break;
                    case "t":
                        if (!inFieldCode)
                        {
                            sb.Append(element.Value);
                        }

                        break;
                    case "tab":
                        if (element.Parent?.Name == W + "r")
                        {
                            sb.Append('\t');
                        }

                        break;
                    case "br":
                    case "cr":
                        if (element.Parent?.Name == W + "r")
                        {
                            result.Add(sb.ToString());
                            sb.Clear();
                        }

                        break;
                    case "noBreakHyphen":
                        sb.Append('-');
                        break;
                }
            }

            result.Add(sb.ToString().Replace(' ', ' '));
            return result;
        }

        /// <summary>
        /// Skip tracked deletions and text from text boxes etc. that hold their own paragraphs.
        /// </summary>
        private static bool IsInsideDeletionOrNestedParagraph(XElement element, XElement paragraph)
        {
            for (var parent = element.Parent; parent != null && parent != paragraph; parent = parent.Parent)
            {
                if (parent.Name == W + "del" || parent.Name == W + "moveFrom" || parent.Name == W + "p")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
