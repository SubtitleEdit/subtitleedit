using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// EZTitles binary project file (.ezt) - read only.
    ///
    /// Layout (reverse engineered from an EZTitles 5.3 file, cross-checked against the .eztxml
    /// export of the same project):
    ///   "EZTZ" + zlib stream. Decompressed:
    ///     header (fonts, safe area, media file name, ...) of variable size, ending with
    ///     int32 subtitle count.
    ///     subtitle records: int32 size (excluding itself), int16 number, byte flags,
    ///     start time as 4 bytes (frames, seconds, minutes, hours), end time (same),
    ///     7 bytes of layout, byte row count, then per row a small header ending in 02 03,
    ///     int32 char count, UTF-32LE text, the same char count again and per-character
    ///     attribute blobs of varying size.
    /// The header size is not fixed, so the reader locates the first record by its text row
    /// instead of by offset, and rows are found by their "length + UTF-32 text + length" shape.
    /// </summary>
    public class EZTitlesBinary : SubtitleFormat
    {
        private const int MaxRowLength = 500;

        public override string Extension => ".ezt";

        public override string Name => "EZTitles binary";

        public override bool IsTextBased => false;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName) ||
                !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                var buffer = FileUtil.ReadBytesShared(fileName, 6);
                return IsEztZlib(buffer);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsEztZlib(byte[] buffer)
        {
            return buffer.Length >= 6 &&
                   buffer[0] == 'E' && buffer[1] == 'Z' && buffer[2] == 'T' && buffer[3] == 'Z' &&
                   buffer[4] == 0x78; // zlib header
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            byte[] data;
            try
            {
                var buffer = FileUtil.ReadAllBytesShared(fileName);
                if (!IsEztZlib(buffer))
                {
                    _errorCount = 1;
                    return;
                }

                data = Decompress(buffer);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount = 1;
                return;
            }

            var records = ReadRecords(data);
            if (records.Count == 0)
            {
                _errorCount = 1;
                return;
            }

            // Time codes are hh:mm:ss:ff, but the file does not carry a usable frame rate for
            // the ff part (a 23.976 fps project may still use "30drop" time codes). Pick the
            // rate from the largest frame number actually used and fall back to the current one.
            var maxFrame = 0;
            foreach (var r in records)
            {
                maxFrame = Math.Max(maxFrame, Math.Max(r.StartFrames, r.EndFrames));
            }

            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            if (maxFrame >= 30)
            {
                frameRate = 60;
            }
            else if (maxFrame >= 25)
            {
                frameRate = 29.97;
            }
            else if (maxFrame == 24 && frameRate < 25)
            {
                frameRate = 25;
            }

            foreach (var r in records)
            {
                var text = string.Join(Environment.NewLine, r.Rows).Trim();
                var p = new Paragraph(text, r.StartMs + FramesToMilliseconds(r.StartFrames, frameRate), r.EndMs + FramesToMilliseconds(r.EndFrames, frameRate));
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        private static byte[] Decompress(byte[] buffer)
        {
            // Skip "EZTZ" and the two zlib header bytes - DeflateStream wants the raw stream.
            using (var inStream = new MemoryStream(buffer, 6, buffer.Length - 6))
            using (var outStream = new MemoryStream(buffer.Length * 4))
            using (var deflateStream = new DeflateStream(inStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }

        private class Record
        {
            public int StartMs;
            public int StartFrames;
            public int EndMs;
            public int EndFrames;
            public List<string> Rows = new List<string>();
        }

        private static List<Record> ReadRecords(byte[] data)
        {
            var records = new List<Record>();
            var firstRow = FindRow(data, 0, data.Length, out _, out _);
            if (firstRow < 0)
            {
                return records;
            }

            // The row header before the first text is fixed size (see class comment), so walk
            // back from it to the record's size field.
            var pos = FindRecordStart(data, firstRow);
            if (pos < 0)
            {
                return records;
            }

            // Subtitles without text (timing templates) have no row to find, so the first text
            // record may not be the first record: chain backwards through any records that end
            // exactly where the known one starts.
            pos = FindPrecedingRecords(data, pos);

            while (pos + 4 <= data.Length)
            {
                var size = BitConverter.ToInt32(data, pos);
                if (size < 19 || pos + 4 + size > data.Length)
                {
                    break;
                }

                var recordStart = pos + 4;
                var recordEnd = recordStart + size;
                var record = new Record
                {
                    StartFrames = data[recordStart + 3],
                    StartMs = data[recordStart + 4] * 1000 + data[recordStart + 5] * 60000 + data[recordStart + 6] * 3600000,
                    EndFrames = data[recordStart + 7],
                    EndMs = data[recordStart + 8] * 1000 + data[recordStart + 9] * 60000 + data[recordStart + 10] * 3600000,
                };

                var i = recordStart + 19;
                while (i < recordEnd)
                {
                    var rowPos = FindRow(data, i, recordEnd, out var text, out var next);
                    if (rowPos < 0)
                    {
                        break;
                    }

                    record.Rows.Add(text);
                    i = next;
                }

                records.Add(record);
                pos = recordEnd;
            }

            return records;
        }

        private static int FindPrecedingRecords(byte[] data, int sizePos)
        {
            var nextNumber = BitConverter.ToInt16(data, sizePos + 4);
            var found = true;
            while (found && sizePos > 0)
            {
                found = false;
                for (var p = sizePos - 4 - 19; p >= 0; p--)
                {
                    var size = BitConverter.ToInt32(data, p);
                    if (p + 4 + size != sizePos || !IsPlausibleRecordHeader(data, p + 4))
                    {
                        continue;
                    }

                    var number = BitConverter.ToInt16(data, p + 4);
                    if (number < 1 || number > nextNumber)
                    {
                        continue;
                    }

                    sizePos = p;
                    nextNumber = number;
                    found = true;
                    break;
                }
            }

            return sizePos;
        }

        private static bool IsPlausibleRecordHeader(byte[] data, int recordStart)
        {
            return data[recordStart + 3] < 60 && data[recordStart + 4] < 60 && data[recordStart + 5] < 60 &&
                   data[recordStart + 7] < 60 && data[recordStart + 8] < 60 && data[recordStart + 9] < 60 &&
                   data[recordStart + 18] <= 8; // row count
        }

        private static int FindRecordStart(byte[] data, int firstRowLengthPos)
        {
            // rowLengthPos points at the int32 char count. Before it: 02 03, then 8 bytes of row
            // header, before that the record header (19 bytes) and the int32 size.
            var recordStart = firstRowLengthPos - 2 - 8 - 19;
            var sizePos = recordStart - 4;
            if (sizePos < 0)
            {
                return -1;
            }

            var size = BitConverter.ToInt32(data, sizePos);
            if (size < 19 || recordStart + size > data.Length)
            {
                return -1;
            }

            return sizePos;
        }

        /// <summary>
        /// Finds the next text row at or after <paramref name="start"/>: marker 02 03, int32 char
        /// count, UTF-32LE chars, the same char count again. Returns the position of the first
        /// char count, or -1.
        /// </summary>
        private static int FindRow(byte[] data, int start, int end, out string text, out int next)
        {
            text = null;
            next = -1;
            for (var i = Math.Max(start, 4); i + 10 <= end; i++)
            {
                if (data[i - 2] != 0x02 || data[i - 1] != 0x03 || data[i - 3] != 0 || data[i - 4] != 0)
                {
                    continue;
                }

                var length = BitConverter.ToInt32(data, i);
                if (length <= 0 || length > MaxRowLength)
                {
                    continue;
                }

                var textStart = i + 4;
                var textEnd = textStart + length * 4;
                if (textEnd + 4 > end || BitConverter.ToInt32(data, textEnd) != length)
                {
                    continue;
                }

                var ok = true;
                for (var j = textStart; j < textEnd; j += 4)
                {
                    var c = BitConverter.ToInt32(data, j);
                    if (c < 0x20 || c > 0x10FFFF)
                    {
                        ok = false;
                        break;
                    }
                }

                if (!ok)
                {
                    continue;
                }

                text = Encoding.UTF32.GetString(data, textStart, length * 4);
                next = textEnd + 4;
                return i;
            }

            return -1;
        }
    }
}
