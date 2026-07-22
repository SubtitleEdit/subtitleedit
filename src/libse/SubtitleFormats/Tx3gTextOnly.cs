using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Raw 3GPP timed text ("tx3g") sample stream, as written by e.g. "MP4Box -raw &lt;track&gt;".
    ///
    /// Layout per 3GPP TS 26.245 / ISO-IEC 14496-17: a tx3g sample description, followed by the
    /// track's samples back to back. Each sample is a 16-bit big-endian byte count followed by
    /// that many bytes of text, optionally followed by modifier boxes ("styl", "hlit", ...).
    ///
    /// A zero-length sample is normal and frequent - timed text clears the screen between cues by
    /// emitting an empty sample.
    ///
    /// The raw sample stream carries no timing information (in an MP4 that lives in the track's
    /// "stts"/"stsc" tables, which a raw export drops), so this format recovers text only.
    /// </summary>
    public class Tx3GTextOnly : SubtitleFormat
    {
        public override string Extension => ".tx3g";

        public override string Name => "tx3g";

        // displayFlags(4) + horizontal-justification(1) + vertical-justification(1) +
        // background-color-rgba(4) + BoxRecord(8) + StyleRecord(12)
        private const int SampleDescriptionFixedLength = 30;

        // Modifier boxes that may trail a text sample (TS 26.245).
        private static readonly HashSet<string> ModifierBoxNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "styl", "hlit", "hclr", "krok", "dlay", "href", "tbox", "blnk", "twrp"
        };

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;

            if (string.IsNullOrEmpty(fileName) ||
                !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(fileName))
            {
                return;
            }

            var buffer = FileUtil.ReadAllBytesShared(fileName);
            var index = GetSampleDescriptionLength(buffer);
            if (index < 0)
            {
                _errorCount = 1;
                return;
            }

            while (index + 2 <= buffer.Length)
            {
                var textLength = GetWord(buffer, index);
                index += 2;
                if (index + textLength > buffer.Length)
                {
                    break;
                }

                if (textLength > 0)
                {
                    var text = DecodeText(buffer, index, textLength);
                    subtitle.Paragraphs.Add(new Paragraph { Text = string.Join(Environment.NewLine, text.SplitToLines()) });
                    index += textLength;
                }

                index = SkipModifierBoxes(buffer, index);
            }

            subtitle.Renumber();
        }

        /// <summary>
        /// Length of the leading tx3g sample description, or -1 if the buffer is too short or malformed.
        /// The fixed fields are followed by a font table: a 16-bit entry count, then per entry a
        /// 16-bit font id, an 8-bit name length and that many name bytes.
        /// </summary>
        private static int GetSampleDescriptionLength(byte[] buffer)
        {
            var index = SampleDescriptionFixedLength;
            if (index + 2 > buffer.Length)
            {
                return -1;
            }

            var fontCount = GetWord(buffer, index);
            index += 2;
            for (var i = 0; i < fontCount; i++)
            {
                if (index + 3 > buffer.Length)
                {
                    return -1;
                }

                index += 2; // font id
                index += 1 + buffer[index]; // name length + name
                if (index > buffer.Length)
                {
                    return -1;
                }
            }

            return index;
        }

        private static int SkipModifierBoxes(byte[] buffer, int index)
        {
            while (index + 8 <= buffer.Length)
            {
                if (!ModifierBoxNames.Contains(Encoding.ASCII.GetString(buffer, index + 4, 4)))
                {
                    break;
                }

                var size = GetUInt(buffer, index);
                if (size < 8 || index + size > buffer.Length)
                {
                    break;
                }

                index += size;
            }

            return index;
        }

        /// <summary>
        /// Text is UTF-8, or UTF-16 when it opens with a byte order mark (TS 26.245).
        /// </summary>
        private static string DecodeText(byte[] buffer, int index, int length)
        {
            if (length >= 2)
            {
                if (buffer[index] == 0xFE && buffer[index + 1] == 0xFF)
                {
                    return Encoding.BigEndianUnicode.GetString(buffer, index + 2, length - 2);
                }

                if (buffer[index] == 0xFF && buffer[index + 1] == 0xFE)
                {
                    return Encoding.Unicode.GetString(buffer, index + 2, length - 2);
                }
            }

            return Encoding.UTF8.GetString(buffer, index, length);
        }

        private static int GetWord(byte[] buffer, int index)
        {
            return (buffer[index] << 8) | buffer[index + 1];
        }

        private static int GetUInt(byte[] buffer, int index)
        {
            // Sizes are 32-bit unsigned on the wire; anything past int.MaxValue cannot address
            // the buffer anyway, so clamp negatives to 0 and let the caller reject them.
            var value = (buffer[index] << 24) | (buffer[index + 1] << 16) | (buffer[index + 2] << 8) | buffer[index + 3];
            return value < 0 ? 0 : value;
        }
    }
}
