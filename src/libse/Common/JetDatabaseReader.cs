using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Minimal read-only reader for Jet 4 database files (Microsoft Access 2000-2003 ".mdb" layout).
    /// Some subtitle tools store their project in a renamed Jet database, e.g. CANVASs SSTG1 ".sdb".
    ///
    /// Only what a subtitle importer needs is implemented: the catalog (user tables), table definitions,
    /// data pages, and the column types long/int/byte/double/float/bool/text/memo. Jet 3 (Access 97,
    /// 2 KB pages) and password-protected files with encrypted pages are not supported.
    ///
    /// Layout reference: mdbtools (HACKING.md) and Jackcess.
    /// </summary>
    public class JetDatabaseReader
    {
        private const int PageSize = 4096;
        private const int OffsetMask = 0x1FFF;
        private const int RowCountOffset = 0x0C;
        private const int MaxPagesToFollow = 4096;

        private const byte PageTypeData = 0x01;
        private const byte PageTypeTableDefinition = 0x02;
        private const int SystemObjectsPage = 2;

        public const int ColumnTypeBool = 1;
        public const int ColumnTypeByte = 2;
        public const int ColumnTypeInt = 3;
        public const int ColumnTypeLong = 4;
        public const int ColumnTypeFloat = 6;
        public const int ColumnTypeDouble = 7;
        public const int ColumnTypeText = 10;
        public const int ColumnTypeMemo = 12;

        private static readonly byte[] HeaderKey = { 0xC7, 0xDA, 0x39, 0x6B };

        private readonly byte[] _data;
        private readonly int _pageCount;
        private readonly uint _dbKey;
        private Dictionary<string, int> _tables;

        public class Column
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public int ColumnNumber { get; set; }
            public int VariableColumnNumber { get; set; }
            public bool IsFixed { get; set; }
            public int FixedOffset { get; set; }
            public int Size { get; set; }
        }

        public class Table
        {
            public string Name { get; set; }
            public int DefinitionPage { get; set; }
            public int VariableColumnCount { get; set; }
            public uint UsageMapPointer { get; set; }
            public List<Column> Columns { get; } = new List<Column>();
        }

        /// <summary>
        /// True when the buffer starts like a Jet 4 file: page type 0 and Jet version 1 at 0x14.
        /// The 16-byte signature at offset 4 ("Standard Jet DB") is deliberately not checked, as
        /// applications overwrite it with their own text.
        /// </summary>
        public static bool IsJet4(byte[] buffer)
        {
            return buffer != null &&
                   buffer.Length >= PageSize * 3 &&
                   buffer[0] == 0 &&
                   buffer[1] == 1 &&
                   buffer[0x14] == 1;
        }

        public JetDatabaseReader(byte[] data)
        {
            if (!IsJet4(data))
            {
                throw new InvalidOperationException("Not a Jet 4 database");
            }

            _data = data;
            _pageCount = data.Length / PageSize;

            // Page 0 bytes 0x18..0x98 are RC4 encrypted with a fixed key; the per-page encryption key
            // (non-zero only for "encrypted" databases) lives in the decrypted area at 0x3E.
            var header = new byte[128];
            Array.Copy(data, 0x18, header, 0, header.Length);
            Rc4(HeaderKey, header);
            _dbKey = ReadUInt32(header, 0x3E - 0x18);
        }

        /// <summary>Whether every page (except the header) is RC4 encrypted - not supported by this reader.</summary>
        public bool IsEncrypted => _dbKey != 0;

        /// <summary>User table names mapped to their definition page.</summary>
        public IReadOnlyDictionary<string, int> GetTables()
        {
            if (_tables != null)
            {
                return _tables;
            }

            var tables = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var systemObjects = ReadTableDefinition(SystemObjectsPage, "MSysObjects");
            foreach (var row in ReadRows(systemObjects))
            {
                if (!(row.TryGetValue("Type", out var typeObj) && typeObj is int type) ||
                    !(row.TryGetValue("Id", out var idObj) && idObj is int id) ||
                    !(row.TryGetValue("Name", out var nameObj) && nameObj is string name))
                {
                    continue;
                }

                var flags = row.TryGetValue("Flags", out var flagsObj) && flagsObj is int f ? f : 0;
                if ((type & 0x7F) == 1 && (flags & unchecked((int)0x80000002)) == 0 && !string.IsNullOrEmpty(name))
                {
                    tables[name] = id & 0x00FFFFFF;
                }
            }

            _tables = tables;
            return tables;
        }

        public bool HasTable(string name)
        {
            return GetTables().ContainsKey(name);
        }

        public Table GetTable(string name)
        {
            return GetTables().TryGetValue(name, out var page) ? ReadTableDefinition(page, name) : null;
        }

        /// <summary>
        /// All rows of a table, each as a column name → value map (long → int, text/memo → string,
        /// bool → bool, null → null). Returns an empty list for unknown tables.
        /// </summary>
        public List<Dictionary<string, object>> ReadTable(string name)
        {
            var table = GetTable(name);
            return table == null ? new List<Dictionary<string, object>>() : ReadRows(table);
        }

        private byte[] GetPage(int pageNumber)
        {
            if (pageNumber < 0 || pageNumber >= _pageCount)
            {
                return null;
            }

            var page = new byte[PageSize];
            Array.Copy(_data, pageNumber * PageSize, page, 0, PageSize);
            if (pageNumber != 0 && _dbKey != 0)
            {
                var key = BitConverter.GetBytes(_dbKey ^ (uint)pageNumber);
                Rc4(key, page);
            }

            return page;
        }

        private Table ReadTableDefinition(int pageNumber, string name)
        {
            var first = GetPage(pageNumber);
            if (first == null || first[0] != PageTypeTableDefinition)
            {
                return null;
            }

            // A long definition continues on further pages, linked via the pointer at offset 4; the
            // continuation pages contribute their bytes from offset 8.
            var stream = new List<byte>(first);
            var visited = new HashSet<int> { pageNumber };
            var next = (int)ReadUInt32(first, 4);
            while (next != 0 && visited.Add(next) && visited.Count < MaxPagesToFollow)
            {
                var page = GetPage(next);
                if (page == null)
                {
                    break;
                }

                for (var i = 8; i < PageSize; i++)
                {
                    stream.Add(page[i]);
                }

                next = (int)ReadUInt32(page, 4);
            }

            var buffer = stream.ToArray();
            var table = new Table
            {
                Name = name,
                DefinitionPage = pageNumber,
                VariableColumnCount = ReadUInt16(buffer, 43),
                UsageMapPointer = ReadUInt32(buffer, 55),
            };
            int columnCount = ReadUInt16(buffer, 45);
            var realIndexCount = (int)ReadUInt32(buffer, 51);
            var pos = 63 + realIndexCount * 12;

            for (var i = 0; i < columnCount; i++)
            {
                if (pos + 25 > buffer.Length)
                {
                    return null;
                }

                var type = buffer[pos];
                table.Columns.Add(new Column
                {
                    Type = type,
                    ColumnNumber = buffer[pos + 5],
                    VariableColumnNumber = ReadUInt16(buffer, pos + 7),
                    IsFixed = (buffer[pos + 15] & 0x01) != 0,
                    FixedOffset = ReadUInt16(buffer, pos + 21),
                    Size = type == ColumnTypeBool ? 0 : ReadUInt16(buffer, pos + 23),
                });
                pos += 25;
            }

            foreach (var column in table.Columns)
            {
                if (pos + 2 > buffer.Length)
                {
                    return null;
                }

                int nameLength = ReadUInt16(buffer, pos);
                pos += 2;
                if (pos + nameLength > buffer.Length)
                {
                    return null;
                }

                column.Name = DecodeText(buffer, pos, nameLength);
                pos += nameLength;
            }

            table.Columns.Sort((a, b) => a.ColumnNumber.CompareTo(b.ColumnNumber));
            return table;
        }

        /// <summary>Page numbers owned by a table, from its usage map (type 0: inline bitmap, type 1: bitmap pages).</summary>
        private List<int> GetUsagePages(uint usageMapPointer)
        {
            var pages = new List<int>();
            if (!TryFindRow(usageMapPointer, out var buffer, out var start, out var length) || length < 1)
            {
                return pages;
            }

            var mapType = buffer[start];
            if (mapType == 0)
            {
                if (length < 5)
                {
                    return pages;
                }

                var basePage = (int)ReadUInt32(buffer, start + 1);
                var bitCount = (length - 5) * 8;
                for (var i = 0; i < bitCount; i++)
                {
                    if ((buffer[start + 5 + i / 8] & (1 << (i % 8))) != 0)
                    {
                        pages.Add(basePage + i);
                    }
                }
            }
            else if (mapType == 1)
            {
                var bitsPerMapPage = (PageSize - 4) * 8;
                var mapPageCount = (length - 1) / 4;
                for (var mapIndex = 0; mapIndex < mapPageCount; mapIndex++)
                {
                    var mapPageNumber = (int)ReadUInt32(buffer, start + 1 + mapIndex * 4);
                    if (mapPageNumber == 0)
                    {
                        continue;
                    }

                    var mapPage = GetPage(mapPageNumber);
                    if (mapPage == null)
                    {
                        continue;
                    }

                    for (var i = 0; i < bitsPerMapPage; i++)
                    {
                        if ((mapPage[4 + i / 8] & (1 << (i % 8))) != 0)
                        {
                            pages.Add(mapIndex * bitsPerMapPage + i);
                        }
                    }
                }
            }

            return pages;
        }

        private List<Dictionary<string, object>> ReadRows(Table table)
        {
            var rows = new List<Dictionary<string, object>>();
            if (table == null)
            {
                return rows;
            }

            foreach (var pageNumber in GetUsagePages(table.UsageMapPointer))
            {
                var page = GetPage(pageNumber);
                if (page == null || page[0] != PageTypeData || ReadUInt32(page, 4) != (uint)table.DefinitionPage)
                {
                    continue;
                }

                int rowCount = ReadUInt16(page, RowCountOffset);
                for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    if (!TryFindRowOnPage(page, rowIndex, out var flags, out var start, out var length) || length == 0)
                    {
                        continue;
                    }

                    // 0x8000: deleted row, 0x4000: row moved to an overflow page (the slot holds a pointer).
                    // Both are skipped - subtitle rows are far too small to overflow a 4 KB page.
                    if ((flags & 0xC000) != 0)
                    {
                        continue;
                    }

                    var row = CrackRow(table, page, start, length);
                    if (row != null)
                    {
                        rows.Add(row);
                    }
                }
            }

            return rows;
        }

        private static bool TryFindRowOnPage(byte[] page, int rowIndex, out int flags, out int start, out int length)
        {
            flags = 0;
            start = 0;
            length = 0;
            var slot = RowCountOffset + 2 + rowIndex * 2;
            if (rowIndex < 0 || slot + 2 > PageSize)
            {
                return false;
            }

            int raw = ReadUInt16(page, slot);
            flags = raw & ~OffsetMask;
            start = raw & OffsetMask;
            var end = rowIndex == 0 ? PageSize : ReadUInt16(page, slot - 2) & OffsetMask;
            if (start >= PageSize || start > end || end > PageSize)
            {
                return false;
            }

            length = end - start;
            return true;
        }

        /// <summary>Resolves a "page/row" pointer (row number in the low byte, page number above it).</summary>
        private bool TryFindRow(uint pageRow, out byte[] page, out int start, out int length)
        {
            page = GetPage((int)(pageRow >> 8));
            start = 0;
            length = 0;
            return page != null && TryFindRowOnPage(page, (int)(pageRow & 0xFF), out _, out start, out length);
        }

        private Dictionary<string, object> CrackRow(Table table, byte[] page, int rowStart, int rowLength)
        {
            var rowEnd = rowStart + rowLength - 1;
            int rowColumnCount = ReadUInt16(page, rowStart);
            var nullMaskSize = (rowColumnCount + 7) / 8;
            if (nullMaskSize + 1 >= rowLength)
            {
                return null;
            }

            var nullMaskStart = rowEnd - nullMaskSize + 1;
            var rowVariableColumnCount = 0;
            int[] variableOffsets = null;
            if (table.VariableColumnCount > 0)
            {
                if (nullMaskSize + 3 > rowLength)
                {
                    return null;
                }

                rowVariableColumnCount = ReadUInt16(page, rowEnd - nullMaskSize - 1);
                if (nullMaskSize + 3 + rowVariableColumnCount * 2 + 2 > rowLength)
                {
                    return null;
                }

                variableOffsets = new int[rowVariableColumnCount + 1];
                for (var i = 0; i <= rowVariableColumnCount; i++)
                {
                    variableOffsets[i] = ReadUInt16(page, rowEnd - nullMaskSize - 3 - i * 2);
                }
            }

            var rowFixedColumnCount = rowColumnCount - rowVariableColumnCount;
            var fixedColumnsFound = 0;
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in table.Columns)
            {
                var maskByte = column.ColumnNumber / 8;
                var notNull = maskByte < nullMaskSize && (page[nullMaskStart + maskByte] & (1 << (column.ColumnNumber % 8))) != 0;

                if (column.Type == ColumnTypeBool)
                {
                    row[column.Name] = notNull; // booleans live in the null mask
                    continue;
                }

                int start;
                int size;
                if (column.IsFixed && fixedColumnsFound < rowFixedColumnCount)
                {
                    start = rowStart + 2 + column.FixedOffset;
                    size = column.Size;
                    fixedColumnsFound++;
                }
                else if (!column.IsFixed && variableOffsets != null && column.VariableColumnNumber < rowVariableColumnCount)
                {
                    start = rowStart + variableOffsets[column.VariableColumnNumber];
                    size = variableOffsets[column.VariableColumnNumber + 1] - variableOffsets[column.VariableColumnNumber];
                }
                else
                {
                    notNull = false;
                    start = 0;
                    size = 0;
                }

                if (!notNull || size < 0 || start + size > rowStart + rowLength)
                {
                    row[column.Name] = null;
                    continue;
                }

                row[column.Name] = ReadValue(column.Type, page, start, size);
            }

            return row;
        }

        private object ReadValue(int type, byte[] page, int start, int size)
        {
            switch (type)
            {
                case ColumnTypeByte:
                    return size >= 1 ? (object)(int)page[start] : null;
                case ColumnTypeInt:
                    return size >= 2 ? (object)(int)BitConverter.ToInt16(page, start) : null;
                case ColumnTypeLong:
                    return size >= 4 ? (object)BitConverter.ToInt32(page, start) : null;
                case ColumnTypeFloat:
                    return size >= 4 ? (object)(double)BitConverter.ToSingle(page, start) : null;
                case ColumnTypeDouble:
                    return size >= 8 ? (object)BitConverter.ToDouble(page, start) : null;
                case ColumnTypeText:
                    return DecodeText(page, start, size);
                case ColumnTypeMemo:
                    return ReadMemo(page, start, size);
                default:
                    var bytes = new byte[size];
                    Array.Copy(page, start, bytes, 0, size);
                    return bytes;
            }
        }

        /// <summary>
        /// A memo value is a 12-byte header (length + flags, page/row pointer, unused) followed by
        /// inline text (flag 0x80), or a pointer to one LVAL page row (0x40), or to a chain of
        /// LVAL rows that each start with the pointer to the next one (no flag).
        /// </summary>
        private string ReadMemo(byte[] page, int start, int size)
        {
            if (size < 12)
            {
                return string.Empty;
            }

            var lengthAndFlags = ReadUInt32(page, start);
            var pointer = ReadUInt32(page, start + 4);
            if ((lengthAndFlags & 0x80000000) != 0)
            {
                return DecodeText(page, start + 12, size - 12);
            }

            if ((lengthAndFlags & 0x40000000) != 0)
            {
                return TryFindRow(pointer, out var lvalPage, out var lvalStart, out var lvalLength)
                    ? DecodeText(lvalPage, lvalStart, lvalLength)
                    : string.Empty;
            }

            if ((lengthAndFlags & 0xFF000000) != 0)
            {
                return string.Empty;
            }

            var total = (int)(lengthAndFlags & 0x00FFFFFF);
            var chunks = new List<byte>(total);
            var visited = new HashSet<uint>();
            while (pointer != 0 && visited.Add(pointer) && visited.Count < MaxPagesToFollow)
            {
                if (!TryFindRow(pointer, out var lvalPage, out var lvalStart, out var lvalLength) || lvalLength < 4)
                {
                    break;
                }

                if (chunks.Count + lvalLength - 4 > total)
                {
                    break;
                }

                for (var i = lvalStart + 4; i < lvalStart + lvalLength; i++)
                {
                    chunks.Add(lvalPage[i]);
                }

                pointer = ReadUInt32(lvalPage, lvalStart);
            }

            var bytes = chunks.ToArray();
            return DecodeText(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Jet 4 text is UTF-16LE, optionally "Unicode compressed": an FF FE prefix, then one byte per
        /// character while in compressed mode; a 0x00 byte toggles between one- and two-byte mode.
        /// </summary>
        public static string DecodeText(byte[] buffer, int start, int length)
        {
            if (length <= 0 || start < 0 || start + length > buffer.Length)
            {
                return string.Empty;
            }

            if (length >= 2 && buffer[start] == 0xFF && buffer[start + 1] == 0xFE)
            {
                var expanded = new List<byte>(length * 2);
                var compressed = true;
                var i = start + 2;
                var end = start + length;
                while (i < end)
                {
                    var b = buffer[i];
                    if (b == 0)
                    {
                        compressed = !compressed;
                        i++;
                    }
                    else if (compressed)
                    {
                        expanded.Add(b);
                        expanded.Add(0);
                        i++;
                    }
                    else if (i + 1 < end)
                    {
                        expanded.Add(b);
                        expanded.Add(buffer[i + 1]);
                        i += 2;
                    }
                    else
                    {
                        break;
                    }
                }

                var bytes = expanded.ToArray();
                return Encoding.Unicode.GetString(bytes, 0, bytes.Length);
            }

            return Encoding.Unicode.GetString(buffer, start, length - length % 2);
        }

        private static void Rc4(byte[] key, byte[] data)
        {
            var s = new byte[256];
            for (var i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
            }

            var j = 0;
            for (var i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[i % key.Length]) & 0xFF;
                var tmp = s[i];
                s[i] = s[j];
                s[j] = tmp;
            }

            var x = 0;
            var y = 0;
            for (var k = 0; k < data.Length; k++)
            {
                x = (x + 1) & 0xFF;
                y = (y + s[x]) & 0xFF;
                var tmp = s[x];
                s[x] = s[y];
                s[y] = tmp;
                data[k] ^= s[(s[x] + s[y]) & 0xFF];
            }
        }

        private static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
        }

        private static uint ReadUInt32(byte[] buffer, int offset)
        {
            return (uint)(buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24));
        }
    }
}
