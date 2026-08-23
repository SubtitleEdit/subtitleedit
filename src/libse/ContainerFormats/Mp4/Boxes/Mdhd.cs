using Nikse.SubtitleEdit.Core.Common;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Media Header Box.
    /// </summary>
    public class Mdhd : Box
    {
        public readonly ulong CreationTime;
        public readonly ulong ModificationTime;
        public readonly ulong TimeScale;
        public readonly ulong Duration;
        public readonly string Iso639ThreeLetterCode;

        // A media header is well under this; anything larger means the size field was misread.
        private const ulong MaxSize = 1024 * 1024;

        public Mdhd(Stream fs, ulong size)
        {
            // "size" comes straight from the file - unsigned arithmetic on a too-small value used
            // to underflow into a ~18 exabyte allocation.
            if (size < 26 || size > MaxSize)
            {
                return;
            }

            Buffer = new byte[size - 4];
            var bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return;
            }

            var languageIndex = 20;
            int version = Buffer[0];
            if (version != 0 && Buffer.Length < 34)
            {
                return; // the 64-bit layout does not fit in what the size field declared
            }

            if (version == 0)
            {
                CreationTime = GetUInt(4);
                ModificationTime = GetUInt(8);
                TimeScale = GetUInt(12);
                Duration = GetUInt(16);
            }
            else
            {
                CreationTime = GetUInt64(4); // 64-bit
                ModificationTime = GetUInt64(12); // 64-bit
                TimeScale = GetUInt(20); // 32-bit
                Duration = GetUInt64(24); // 64-bit
                languageIndex = 32;
            }

            // language code = skip first byte, 5 bytes + 5 bytes + 5 bytes (add 0x60 to get ascii value)
            var languageByte1 = ((Buffer[languageIndex] << 1) >> 3) + 0x60;
            var languageByte2 = ((Buffer[languageIndex] & 0x3) << 3) + (Buffer[languageIndex + 1] >> 5) + 0x60;
            var languageByte3 = (Buffer[languageIndex + 1] & 0x1f) + 0x60;
            var x1 = (char)languageByte1;
            var x2 = (char)languageByte2;
            var x3 = (char)languageByte3;

            // QuickTime writes 0x7FFF for "unspecified" (ffmpeg does this for every .mov track),
            // which unpacks to three DEL characters. Anything that is not three lowercase
            // letters is not a language code - report it as absent so callers fall back
            // instead of showing, and putting in file names, control characters.
            Iso639ThreeLetterCode = IsLowerCaseLetter(x1) && IsLowerCaseLetter(x2) && IsLowerCaseLetter(x3)
                ? x1.ToString(CultureInfo.InvariantCulture) + x2.ToString(CultureInfo.InvariantCulture) + x3.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static bool IsLowerCaseLetter(char c) => c >= 'a' && c <= 'z';

        public string LanguageString
        {
            get
            {
                // mdhd carries either the ISO 639-2/T (terminology) or the 639-2/B
                // (bibliographic) code - MP4Box writes whatever "lang=" was given, and
                // "fre"/"ger"/"dut" are as common in the wild as "fra"/"deu"/"nld".
                var language = Iso639Dash2LanguageCode.List.FirstOrDefault(p =>
                    p.ThreeLetterCode == Iso639ThreeLetterCode || p.BibliographicCode == Iso639ThreeLetterCode);
                return language == null ? "Any" : language.EnglishName;
            }
        }
    }
}
