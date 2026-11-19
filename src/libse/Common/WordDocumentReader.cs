using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Minimal reader for the main text of a binary Word 97-2003 document (.doc).
    /// Reads the OLE compound file, the "WordDocument" stream and the piece table in the
    /// table stream. Table rows are flattened like a plain text export: cells are separated
    /// by tabs, and a cell with several paragraphs spreads over several lines.
    /// </summary>
    public static class WordDocumentReader
    {
        private static readonly byte[] CompoundFileSignature = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
        private const int MaxFileSize = 50_000_000;
        private const uint EndOfChain = 0xFFFFFFFE;

        public static bool IsWordDocument(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) ||
                !fileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(fileName))
            {
                return false;
            }

            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length < 1536 || fs.Length > MaxFileSize)
                    {
                        return false;
                    }

                    var buffer = new byte[CompoundFileSignature.Length];
                    if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                    {
                        return false;
                    }

                    for (var i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i] != CompoundFileSignature[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the document text as lines, or null if the file is not a readable Word 97-2003 document.
        /// </summary>
        public static List<string> GetLines(string fileName)
        {
            if (!IsWordDocument(fileName))
            {
                return null;
            }

            try
            {
                var bytes = FileUtil.ReadAllBytesShared(fileName);
                var text = GetText(bytes);
                return text == null ? null : ToLines(text);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Raw main document text with Word control characters (\r paragraph end, \a cell/row end, ...).
        /// </summary>
        public static string GetText(byte[] file)
        {
            var cfb = new CompoundFile(file);
            var wordDocument = cfb.ReadStream("WordDocument");
            if (wordDocument == null || wordDocument.Length < 0x1AA || BitConverter.ToUInt16(wordDocument, 0) != 0xA5EC)
            {
                return null;
            }

            var flags = BitConverter.ToUInt16(wordDocument, 0x0A);
            if ((flags & 0x0100) != 0)
            {
                return null; // encrypted
            }

            var table = cfb.ReadStream((flags & 0x0200) != 0 ? "1Table" : "0Table");
            if (table == null)
            {
                return null;
            }

            var ccpText = BitConverter.ToInt32(wordDocument, 0x4C);
            var fcClx = BitConverter.ToInt32(wordDocument, 0x01A2);
            var lcbClx = BitConverter.ToInt32(wordDocument, 0x01A6);
            if (ccpText < 0 || fcClx < 0 || lcbClx <= 0 || fcClx + (long)lcbClx > table.Length)
            {
                return null;
            }

            // Clx: skip Prc entries (0x01), find the Pcdt (0x02)
            var pos = fcClx;
            var end = fcClx + lcbClx;
            while (pos < end && table[pos] == 0x01)
            {
                pos += 3 + BitConverter.ToInt16(table, pos + 1);
            }

            if (pos >= end || table[pos] != 0x02)
            {
                return null;
            }

            var lcb = BitConverter.ToInt32(table, pos + 1);
            var plcPcd = pos + 5;
            var pieceCount = (lcb - 4) / 12;
            if (pieceCount <= 0 || plcPcd + lcb > table.Length)
            {
                return null;
            }

            var sb = new StringBuilder(ccpText);
            for (var i = 0; i < pieceCount && sb.Length < ccpText; i++)
            {
                var cpStart = BitConverter.ToInt32(table, plcPcd + i * 4);
                var cpEnd = BitConverter.ToInt32(table, plcPcd + (i + 1) * 4);
                var pcd = plcPcd + (pieceCount + 1) * 4 + i * 8;
                var fc = BitConverter.ToUInt32(table, pcd + 2);
                var count = Math.Min(cpEnd - cpStart, ccpText - sb.Length);
                if (count <= 0)
                {
                    continue;
                }

                if ((fc & 0x40000000) != 0)
                {
                    var offset = (int)((fc & 0x3FFFFFFF) / 2);
                    for (var j = 0; j < count && offset + j < wordDocument.Length; j++)
                    {
                        sb.Append(DecodeCompressed(wordDocument[offset + j]));
                    }
                }
                else
                {
                    var offset = (int)fc;
                    for (var j = 0; j < count && offset + j * 2 + 1 < wordDocument.Length; j++)
                    {
                        sb.Append((char)BitConverter.ToUInt16(wordDocument, offset + j * 2));
                    }
                }
            }

            return sb.ToString();
        }

        private static readonly char[] Cp1252High =
        {
            '€', '\u0081', '‚', 'ƒ', '„', '…', '†', '‡', 'ˆ', '‰', 'Š', '‹', 'Œ', '\u008D', 'Ž', '\u008F',
            '\u0090', '‘', '’', '“', '”', '•', '–', '—', '˜', '™', 'š', '›', 'œ', '\u009D', 'ž', 'Ÿ',
        };

        private static char DecodeCompressed(byte b)
        {
            return b >= 0x80 && b <= 0x9F ? Cp1252High[b - 0x80] : (char)b;
        }

        /// <summary>
        /// Turns Word text into plain lines. Table rows (cells end with \a, a row ends with an
        /// extra \a) become one line per paragraph index with the cells joined by tabs.
        /// </summary>
        public static List<string> ToLines(string text)
        {
            text = RemoveFieldCodes(text);
            var lines = new List<string>();
            var paragraph = new StringBuilder();
            var cells = new List<List<string>>();
            var cellParagraphs = new List<string>();

            foreach (var ch in text)
            {
                switch (ch)
                {
                    case '\r':
                        if (cells.Count > 0 || cellParagraphs.Count > 0)
                        {
                            cellParagraphs.Add(paragraph.ToString());
                        }
                        else
                        {
                            lines.Add(paragraph.ToString());
                        }

                        paragraph.Clear();
                        break;
                    case '\a':
                        if (paragraph.Length == 0 && cellParagraphs.Count == 0 && cells.Count > 0)
                        {
                            AddRow(lines, cells); // row end mark
                            cells.Clear();
                        }
                        else
                        {
                            cellParagraphs.Add(paragraph.ToString());
                            paragraph.Clear();
                            cells.Add(cellParagraphs);
                            cellParagraphs = new List<string>();
                        }

                        break;
                    case '\v':
                    case '\f':
                        if (cells.Count > 0 || cellParagraphs.Count > 0)
                        {
                            cellParagraphs.Add(paragraph.ToString());
                        }
                        else
                        {
                            lines.Add(paragraph.ToString());
                        }

                        paragraph.Clear();
                        break;
                    case '\u001E':
                        paragraph.Append('-');
                        break;
                    case ' ':
                        paragraph.Append(' ');
                        break;
                    case '\t':
                        paragraph.Append('\t');
                        break;
                    default:
                        if (ch >= ' ')
                        {
                            paragraph.Append(ch);
                        }

                        break;
                }
            }

            if (cellParagraphs.Count > 0 || paragraph.Length > 0 && cells.Count > 0)
            {
                cellParagraphs.Add(paragraph.ToString());
                cells.Add(cellParagraphs);
                paragraph.Clear();
            }

            if (cells.Count > 0)
            {
                AddRow(lines, cells);
            }

            if (paragraph.Length > 0)
            {
                lines.Add(paragraph.ToString());
            }

            return lines;
        }

        private static void AddRow(List<string> lines, List<List<string>> cells)
        {
            var rowLines = 0;
            foreach (var cell in cells)
            {
                rowLines = Math.Max(rowLines, cell.Count);
            }

            for (var i = 0; i < rowLines; i++)
            {
                var parts = new string[cells.Count];
                for (var c = 0; c < cells.Count; c++)
                {
                    parts[c] = i < cells[c].Count ? cells[c][i] : string.Empty;
                }

                lines.Add(string.Join("\t", parts));
            }

            lines.Add(string.Empty);
        }

        /// <summary>
        /// Fields are stored as \x13 code \x14 result \x15 - keep the result only.
        /// </summary>
        private static string RemoveFieldCodes(string text)
        {
            if (text.IndexOf('\u0013') < 0)
            {
                return text;
            }

            var sb = new StringBuilder(text.Length);
            var depth = 0;
            var inCode = new Stack<bool>();
            foreach (var ch in text)
            {
                if (ch == '\u0013')
                {
                    depth++;
                    inCode.Push(true);
                }
                else if (ch == '\u0014' && depth > 0)
                {
                    inCode.Pop();
                    inCode.Push(false);
                }
                else if (ch == '\u0015' && depth > 0)
                {
                    depth--;
                    inCode.Pop();
                }
                else if (depth == 0 || !inCode.Contains(true))
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        private sealed class CompoundFile
        {
            private readonly byte[] _file;
            private readonly int _sectorSize;
            private readonly int _miniSectorSize;
            private readonly uint _miniStreamCutoff;
            private readonly List<uint> _fat = new List<uint>();
            private readonly List<uint> _miniFat = new List<uint>();
            private readonly List<DirectoryEntry> _entries = new List<DirectoryEntry>();
            private byte[] _miniStream;

            private sealed class DirectoryEntry
            {
                public string Name;
                public byte Type;
                public uint StartSector;
                public long Size;
            }

            public CompoundFile(byte[] file)
            {
                _file = file;
                _sectorSize = 1 << BitConverter.ToUInt16(file, 0x1E);
                _miniSectorSize = 1 << BitConverter.ToUInt16(file, 0x20);
                if (_sectorSize != 512 && _sectorSize != 4096)
                {
                    throw new InvalidDataException("Bad sector size");
                }

                var fatSectorCount = BitConverter.ToUInt32(file, 0x2C);
                var directoryStart = BitConverter.ToUInt32(file, 0x30);
                _miniStreamCutoff = BitConverter.ToUInt32(file, 0x38);
                var miniFatStart = BitConverter.ToUInt32(file, 0x3C);
                var difatStart = BitConverter.ToUInt32(file, 0x44);

                // DIFAT: the first 109 FAT sector numbers are in the header, the rest in a chain
                var fatSectors = new List<uint>();
                for (var i = 0; i < 109 && fatSectors.Count < fatSectorCount; i++)
                {
                    fatSectors.Add(BitConverter.ToUInt32(file, 0x4C + i * 4));
                }

                var difat = difatStart;
                var guard = 0;
                while (fatSectors.Count < fatSectorCount && difat < EndOfChain && guard++ < 100_000)
                {
                    var offset = SectorOffset(difat);
                    var perSector = _sectorSize / 4 - 1;
                    for (var i = 0; i < perSector && fatSectors.Count < fatSectorCount; i++)
                    {
                        fatSectors.Add(BitConverter.ToUInt32(file, offset + i * 4));
                    }

                    difat = BitConverter.ToUInt32(file, offset + perSector * 4);
                }

                foreach (var sector in fatSectors)
                {
                    var offset = SectorOffset(sector);
                    for (var i = 0; i < _sectorSize / 4; i++)
                    {
                        _fat.Add(BitConverter.ToUInt32(file, offset + i * 4));
                    }
                }

                var directory = ReadChain(directoryStart, -1);
                for (var offset = 0; offset + 128 <= directory.Length; offset += 128)
                {
                    var nameLength = BitConverter.ToUInt16(directory, offset + 64);
                    var name = nameLength >= 2 && nameLength <= 64
                        ? Encoding.Unicode.GetString(directory, offset, nameLength - 2)
                        : string.Empty;
                    _entries.Add(new DirectoryEntry
                    {
                        Name = name,
                        Type = directory[offset + 66],
                        StartSector = BitConverter.ToUInt32(directory, offset + 116),
                        Size = BitConverter.ToUInt32(directory, offset + 120),
                    });
                }

                if (miniFatStart < EndOfChain)
                {
                    var miniFat = ReadChain(miniFatStart, -1);
                    for (var i = 0; i + 4 <= miniFat.Length; i += 4)
                    {
                        _miniFat.Add(BitConverter.ToUInt32(miniFat, i));
                    }
                }
            }

            public byte[] ReadStream(string name)
            {
                foreach (var entry in _entries)
                {
                    if (entry.Type != 2 || !string.Equals(entry.Name, name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (entry.Size < _miniStreamCutoff)
                    {
                        return ReadMiniChain(entry.StartSector, entry.Size);
                    }

                    return ReadChain(entry.StartSector, entry.Size);
                }

                return null;
            }

            private int SectorOffset(uint sector)
            {
                var offset = (long)(sector + 1) * _sectorSize;
                if (offset + _sectorSize > _file.Length)
                {
                    throw new InvalidDataException("Sector out of range");
                }

                return (int)offset;
            }

            private byte[] ReadChain(uint start, long size)
            {
                using (var ms = new MemoryStream())
                {
                    var sector = start;
                    var guard = 0;
                    while (sector < EndOfChain && guard++ <= _fat.Count)
                    {
                        ms.Write(_file, SectorOffset(sector), _sectorSize);
                        if (sector >= _fat.Count)
                        {
                            break;
                        }

                        sector = _fat[(int)sector];
                    }

                    var bytes = ms.ToArray();
                    if (size >= 0 && size < bytes.Length)
                    {
                        Array.Resize(ref bytes, (int)size);
                    }

                    return bytes;
                }
            }

            private byte[] ReadMiniChain(uint start, long size)
            {
                if (_miniStream == null)
                {
                    var root = _entries.Count > 0 ? _entries[0] : null;
                    _miniStream = root == null ? new byte[0] : ReadChain(root.StartSector, root.Size);
                }

                using (var ms = new MemoryStream())
                {
                    var sector = start;
                    var guard = 0;
                    while (sector < EndOfChain && guard++ <= _miniFat.Count)
                    {
                        var offset = (long)sector * _miniSectorSize;
                        if (offset + _miniSectorSize > _miniStream.Length || sector >= _miniFat.Count)
                        {
                            break;
                        }

                        ms.Write(_miniStream, (int)offset, _miniSectorSize);
                        sector = _miniFat[(int)sector];
                    }

                    var bytes = ms.ToArray();
                    if (size < bytes.Length)
                    {
                        Array.Resize(ref bytes, (int)size);
                    }

                    return bytes;
                }
            }
        }
    }
}
