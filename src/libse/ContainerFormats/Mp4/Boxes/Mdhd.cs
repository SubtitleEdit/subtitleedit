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

        public Mdhd(Stream fs, ulong size)
        {
            Buffer = new byte[size - 4];
            var bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return;
            }

            var languageIndex = 20;
            int version = Buffer[0];
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
            Iso639ThreeLetterCode = x1.ToString(CultureInfo.InvariantCulture) + x2.ToString(CultureInfo.InvariantCulture) + x3.ToString(CultureInfo.InvariantCulture);
        }

        public string LanguageString
        {
            get
            {
                var language = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.ThreeLetterCode == Iso639ThreeLetterCode);
                return language == null ? "Any" : language.EnglishName;
            }
        }
    }
}
