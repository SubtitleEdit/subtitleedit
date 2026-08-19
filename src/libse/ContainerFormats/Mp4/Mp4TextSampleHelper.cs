using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    /// <summary>
    /// Readers for the plain-text subtitle sample formats in MP4, shared by the progressive
    /// (stbl) and the fragmented (moof) parsers: 3GPP timed text ("tx3g") and the ISO/IEC
    /// 14496-30 text streams ("stxt" and "sbtt").
    /// </summary>
    public static class Mp4TextSampleHelper
    {
        private static readonly char[] LineBreakChars = { '\r', '\n' };

        /// <summary>
        /// True for the ISO/IEC 14496-30 text stream sample entries, where the sample is the
        /// text itself - unlike tx3g there is no 16-bit length in front of it. MP4Box writes
        /// these for <c>:sopt:stxtmod=stxt</c> ("stxt", markup stripped) and
        /// <c>:sopt:stxtmod=sbtt</c> ("sbtt", markup kept).
        /// </summary>
        public static bool IsSimpleTextCodec(string stsdCodec)
        {
            return stsdCodec == "stxt" || stsdCodec == "sbtt";
        }

        /// <summary>
        /// An "stxt"/"sbtt" sample is the whole text of one cue, UTF-8 or UTF-16 with BOM.
        /// </summary>
        public static string ReadSimpleTextSample(byte[] sample)
        {
            if (sample == null || sample.Length == 0)
            {
                return null;
            }

            var length = sample.Length;
            while (length > 0 && sample[length - 1] == 0) // some writers pad the sample with NULs
            {
                length--;
            }

            if (length == 0)
            {
                return null;
            }

            return NormalizeLineBreaks(DecodeText(sample, 0, length))?.TrimEnd();
        }

        /// <summary>
        /// A tx3g sample is a 16-bit text length, the text itself, then optional modifier
        /// boxes (styl/hlit/hclr/...). Style records are turned into the markup tags used
        /// by the rest of Subtitle Edit.
        /// </summary>
        public static string ReadTx3gSampleText(byte[] sample)
        {
            if (sample == null || sample.Length < 2)
            {
                return null;
            }

            int textSize = BinaryPrimitives.ReadUInt16BigEndian(sample.AsSpan(0, 2));
            if (textSize == 0 || textSize > sample.Length - 2)
            {
                return null;
            }

            // styl offsets index the raw text, so styling must be applied before the line
            // breaks are normalized (that can change the character count)
            var text = DecodeText(sample, 2, textSize);
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            text = ApplyStyleRecords(text, sample, 2 + textSize);
            return NormalizeLineBreaks(text)?.TrimEnd();
        }

        private static string DecodeText(byte[] data, int index, int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }

            // 3GPP TS 26.245 allows UTF-16 text, which is then prefixed with a BOM
            if (count >= 2 && data[index] == 0xFE && data[index + 1] == 0xFF)
            {
                return Encoding.BigEndianUnicode.GetString(data, index + 2, count - 2);
            }

            if (count >= 2 && data[index] == 0xFF && data[index + 1] == 0xFE)
            {
                return Encoding.Unicode.GetString(data, index + 2, count - 2);
            }

            if (count >= 3 && data[index] == 0xEF && data[index + 1] == 0xBB && data[index + 2] == 0xBF)
            {
                index += 3;
                count -= 3;
            }

            return Encoding.UTF8.GetString(data, index, count);
        }

        private static string NormalizeLineBreaks(string text)
        {
            if (text == null || text.IndexOfAny(LineBreakChars) < 0)
            {
                return text;
            }

            return string.Join(Environment.NewLine, text.SplitToLines());
        }

        /// <summary>
        /// Wraps the parts of the text covered by "styl" records in italic/bold/underline
        /// and font color tags. Records may overlap, so the style is resolved per character
        /// and tags are opened and closed where it changes.
        /// </summary>
        private static string ApplyStyleRecords(string text, byte[] sample, int index)
        {
            var records = ReadStyleRecords(sample, index);
            if (records == null)
            {
                return text;
            }

            var styles = new StyleRecord[text.Length];
            var styled = false;
            foreach (var record in records)
            {
                if (!record.HasStyle)
                {
                    continue;
                }

                var start = Math.Max(0, record.StartChar);
                var end = Math.Min(text.Length, record.EndChar);
                for (var i = start; i < end; i++)
                {
                    styles[i] = styles[i].Merge(record);
                    styled = true;
                }
            }

            if (!styled)
            {
                return text;
            }

            var sb = new StringBuilder(text.Length + 16);
            var current = new StyleRecord();
            for (var i = 0; i < text.Length; i++)
            {
                var next = styles[i];
                if (!next.Equals(current))
                {
                    AppendCloseTags(sb, current);
                    AppendOpenTags(sb, next);
                    current = next;
                }

                sb.Append(text[i]);
            }

            AppendCloseTags(sb, current);
            return sb.ToString();
        }

        private static void AppendOpenTags(StringBuilder sb, StyleRecord style)
        {
            if (style.Color != null)
            {
                sb.Append("<font color=\"").Append(style.Color).Append("\">");
            }

            if (style.Bold)
            {
                sb.Append("<b>");
            }

            if (style.Italic)
            {
                sb.Append("<i>");
            }

            if (style.Underline)
            {
                sb.Append("<u>");
            }
        }

        private static void AppendCloseTags(StringBuilder sb, StyleRecord style)
        {
            if (style.Underline)
            {
                sb.Append("</u>");
            }

            if (style.Italic)
            {
                sb.Append("</i>");
            }

            if (style.Bold)
            {
                sb.Append("</b>");
            }

            if (style.Color != null)
            {
                sb.Append("</font>");
            }
        }

        /// <summary>
        /// The "styl" box among the tx3g modifier boxes: a 16-bit record count followed by
        /// 12-byte records. QuickTime text tracks use the same box name with 14-byte records,
        /// so the record count must account for the whole box or the box is left alone.
        /// </summary>
        private static List<StyleRecord> ReadStyleRecords(byte[] sample, int index)
        {
            const int recordSize = 12;
            List<StyleRecord> records = null;
            while (index + 8 <= sample.Length)
            {
                var boxSize = (long)BinaryPrimitives.ReadUInt32BigEndian(sample.AsSpan(index, 4));
                if (boxSize < 8 || index + boxSize > sample.Length)
                {
                    break;
                }

                if (Encoding.ASCII.GetString(sample, index + 4, 4) == "styl" && boxSize >= 10)
                {
                    int count = BinaryPrimitives.ReadUInt16BigEndian(sample.AsSpan(index + 8, 2));
                    if (10 + count * recordSize == boxSize)
                    {
                        records = records ?? new List<StyleRecord>(count);
                        for (var i = 0; i < count; i++)
                        {
                            var entry = index + 10 + i * recordSize;
                            records.Add(new StyleRecord
                            {
                                StartChar = BinaryPrimitives.ReadUInt16BigEndian(sample.AsSpan(entry, 2)),
                                EndChar = BinaryPrimitives.ReadUInt16BigEndian(sample.AsSpan(entry + 2, 2)),
                                Bold = (sample[entry + 6] & 1) != 0,
                                Italic = (sample[entry + 6] & 2) != 0,
                                Underline = (sample[entry + 6] & 4) != 0,
                                Color = GetColor(sample, entry + 8),
                            });
                        }
                    }
                }

                index += (int)boxSize;
            }

            return records;
        }

        /// <summary>
        /// Text color as RGBA. White and fully transparent colors are the tx3g default and
        /// are left out, so an ordinary subtitle does not get a font tag on every line.
        /// </summary>
        private static string GetColor(byte[] sample, int index)
        {
            var r = sample[index];
            var g = sample[index + 1];
            var b = sample[index + 2];
            var a = sample[index + 3];
            if (a == 0 || (r == 0xFF && g == 0xFF && b == 0xFF))
            {
                return null;
            }

            return $"#{r:x2}{g:x2}{b:x2}";
        }

        private struct StyleRecord
        {
            public int StartChar;
            public int EndChar;
            public bool Bold;
            public bool Italic;
            public bool Underline;
            public string Color;

            public bool HasStyle => Bold || Italic || Underline || Color != null;

            public StyleRecord Merge(StyleRecord other)
            {
                return new StyleRecord
                {
                    Bold = Bold || other.Bold,
                    Italic = Italic || other.Italic,
                    Underline = Underline || other.Underline,
                    Color = Color ?? other.Color,
                };
            }

            public bool Equals(StyleRecord other)
            {
                return Bold == other.Bold && Italic == other.Italic && Underline == other.Underline && Color == other.Color;
            }
        }
    }
}
