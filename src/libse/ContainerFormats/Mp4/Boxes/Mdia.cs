using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Media Box
    /// </summary>
    public class Mdia : Box
    {
        public Mdhd Mdhd;
        public Minf Minf;
        public readonly string HandlerType;
        public readonly string HandlerName = string.Empty;

        /// <summary>
        /// BCP 47 language tag from the Extended Language Tag box (elng), e.g. "zh-Hans" or
        /// "pt-BR". Empty when absent. When present it overrides the mdhd language.
        /// </summary>
        public readonly string ExtendedLanguage = string.Empty;

        // "subt" is the handler for stpp (TTML/IMSC1) tracks, ISO/IEC 14496-30
        public bool IsTextSubtitle => HandlerType == "sbtl" || HandlerType == "text" || HandlerType == "subt";

        public bool IsVobSubSubtitle => HandlerType == "subp";

        public bool IsClosedCaption => HandlerType == "clcp";

        public bool IsVideo => HandlerType == "vide";

        public bool IsAudio => HandlerType == "soun";

        /// <summary>
        /// English name of the track language - from elng when present ("Chinese (zh-Hans)"),
        /// else from mdhd ("Korean"). Empty when the track is unlabeled ("und" or unset).
        /// </summary>
        public string LanguageDisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(ExtendedLanguage))
                {
                    var primary = ExtendedLanguage.Split('-')[0].ToLowerInvariant();
                    var name = FindEnglishName(primary);
                    if (name == null)
                    {
                        return ExtendedLanguage;
                    }

                    return primary.Length == ExtendedLanguage.Length ? name : $"{name} ({ExtendedLanguage})";
                }

                var code = Mdhd?.Iso639ThreeLetterCode;
                return string.IsNullOrEmpty(code) ? string.Empty : FindEnglishName(code) ?? string.Empty;
            }
        }

        private static string FindEnglishName(string code)
        {
            if (code.Length != 2 && code.Length != 3)
            {
                return null;
            }

            var englishName = Iso639Dash2LanguageCode.List.FirstOrDefault(p =>
                p.TwoLetterCode == code || p.ThreeLetterCode == code || p.BibliographicCode == code)?.EnglishName;
            return FirstName(englishName);
        }

        /// <summary>"Spanish; Castilian" -> "Spanish".</summary>
        internal static string FirstName(string englishName)
        {
            if (englishName == null)
            {
                return null;
            }

            var index = englishName.IndexOf(';');
            return index > 0 ? englishName.Substring(0, index).Trim() : englishName;
        }

        public Mdia(Stream fs, ulong maximumLength)
        {
            HandlerType = string.Empty;
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                // Parens matter: without them, `IsVobSubSubtitle || IsClosedCaption
                // || IsVideo` bind independently of `Name == "minf"`, so e.g. a
                // mdhd box on a video track was being parsed as if it were minf —
                // which left Mdhd unset and forced the default 90000 timescale
                // for video tracks (incorrect MP4 timing for CEA-608/708).
                if (Name == "minf" && (IsTextSubtitle || IsVobSubSubtitle || IsClosedCaption || IsVideo))
                {
                    ulong timeScale = 90000;
                    if (Mdhd != null)
                    {
                        timeScale = Mdhd.TimeScale;
                    }

                    var tempMinf = new Minf(fs, Position, timeScale, HandlerType, this);
                    if (tempMinf.Stbl != null)
                    {
                        Minf = tempMinf;
                    }
                }
                else if (Name == "hdlr")
                {
                    Buffer = ReadPayload(fs, Size - 4);
                    HandlerType = GetString(8, 4);
                    if (Size > 25)
                    {
                        HandlerName = GetString(24, Buffer.Length - (24 + 5)); // TODO: How to find this?
                    }
                }
                else if (Name == "mdhd")
                {
                    Mdhd = new Mdhd(fs, Size);
                }
                else if (Name == "elng" && Size > 12 && Size < 512)
                {
                    // full box: version + flags, then a null-terminated tag
                    Buffer = ReadPayload(fs, Size - 8);
                    var end = System.Array.IndexOf(Buffer, (byte)0, 4);
                    var tag = GetString(4, (end < 0 ? Buffer.Length : end) - 4).Trim();
                    if (tag.Length > 0 && tag != "und")
                    {
                        ExtendedLanguage = tag;
                    }
                }
                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

    }
}
