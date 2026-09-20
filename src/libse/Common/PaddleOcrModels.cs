using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Maps a PaddleOCR language code to the detection/recognition model names shipped in the
    /// "PaddleOCR.PP-OCRv6.support.files" bundle (standalone PaddleOCR 3.7). Shared by the GUI
    /// engine and seconv so both launch the same models for the same language.
    ///
    /// Only the models in that bundle are on disk - nothing is fetched per language. Returning
    /// a name that is not in the bundle points at a folder that does not exist, and the run then
    /// fails when PaddleX tries to read the model's inference.yml.
    /// </summary>
    public static class PaddleOcrModels
    {
        public const string TextlineOrientationModelName = "PP-LCNet_x1_0_textline_ori";

        // The script groups below mirror LATIN_LANGS/ARABIC_LANGS/ESLAV_LANGS/CYRILLIC_LANGS/
        // DEVANAGARI_LANGS in PaddleOCR 3.7 (paddleocr/_utils/langs.py) - the version the
        // bundled standalone engine is built from. Keep them in sync with the GUI's language
        // list; a code offered in the dropdown but missing from every group here silently falls
        // through to the Latin recognition model and OCRs to garbage.
        private static readonly HashSet<string> LatinLanguageCodes = new HashSet<string>
        {
            "af", "az", "bs", "ca", "cs", "cy", "da", "de", "es", "et", "eu",
            "fi", "fr", "ga", "gl", "hr", "hu", "id", "is", "it", "ku", "la",
            "lb", "lt", "lv", "mi", "ms", "mt", "nl", "no", "oc", "pi", "pl",
            "pt", "qu", "rm", "ro", "rs_latin", "sk", "sl", "sq", "sv", "sw",
            "tl", "tr", "uz", "vi", "french", "german"
        };

        private static readonly HashSet<string> ArabicLanguageCodes = new HashSet<string>
        {
            "ar", "bal", "fa", "ps", "sd", "ug", "ur"
        };

        private static readonly HashSet<string> EslavLanguageCodes = new HashSet<string>
        {
            "ru", "be", "uk"
        };

        private static readonly HashSet<string> CyrillicLanguageCodes = new HashSet<string>
        {
            "rs_cyrillic", "bg", "mn", "abq", "ady", "kbd", "ava", "dar",
            "inh", "che", "lbe", "lez", "tab", "ba", "bua", "cv", "kaa",
            "kk", "kv", "ky", "mhr", "mk", "mo", "os", "sah", "tg", "tt",
            "tyv", "udm", "xal"
        };

        private static readonly HashSet<string> DevanagariLanguageCodes = new HashSet<string>
        {
            "hi", "mr", "ne", "bh", "mai", "ang", "bho", "mah",
            "sck", "new", "gom", "bgc", "sa"
        };

        // The languages with their own single-language PP-OCRv5 recognition model.
        private static readonly HashSet<string> OwnModelLanguageCodes = new HashSet<string>
        {
            "el", "ta", "te", "th"
        };

        // Pali is the one Latin language PP-OCRv6 does not cover, so it stays on the PP-OCRv5
        // Latin model (_PPOCRV6_UNSUPPORTED_LATIN_LANGS in paddleocr/_pipelines/ocr.py).
        private const string PaliLanguageCode = "pi";

        /// <summary>
        /// True for the languages PP-OCRv6 (new in PaddleOCR 3.7) recognizes with its single
        /// unified model: Chinese, English, Japanese and the Latin languages except Pali - the
        /// _PPOCRV6_LANGS set in paddleocr/_pipelines/ocr.py. No non-Latin script has a v6 model
        /// at all, so every other language keeps the PP-OCRv5 (Georgian: PP-OCRv3) models that
        /// the support-files bundle still ships alongside the v6 pair.
        /// </summary>
        public static bool IsPpOcrV6Language(string language)
        {
            if (language == "ch" || language == "chinese_cht" || language == "en" || language == "japan")
            {
                return true;
            }

            return LatinLanguageCodes.Contains(language) && language != PaliLanguageCode;
        }

        /// <summary>Arabic-script languages, whose words read right-to-left.</summary>
        public static bool IsArabicScript(string language) => ArabicLanguageCodes.Contains(language);

        // PP-OCRv6 replaced the mobile/server pair with tiny/small/medium tiers, and the bundle
        // ships small and medium - so the saved mode picks between those two.
        private static string PpOcrV6Tier(string mode) => mode == "server" ? "medium" : "small";

        /// <param name="language">PaddleOCR language code (e.g. "en", "korean", "rs_cyrillic").</param>
        /// <param name="mode">"mobile" or "server" - the GUI's saved model-size setting.</param>
        public static string GetRecName(string language, string mode)
        {
            if (IsPpOcrV6Language(language))
            {
                return $"PP-OCRv6_{PpOcrV6Tier(mode)}_rec";
            }

            if (ArabicLanguageCodes.Contains(language))
            {
                return "arabic_PP-OCRv5_mobile_rec";
            }

            if (EslavLanguageCodes.Contains(language))
            {
                return "eslav_PP-OCRv5_mobile_rec";
            }

            if (CyrillicLanguageCodes.Contains(language))
            {
                return "cyrillic_PP-OCRv5_mobile_rec";
            }

            if (DevanagariLanguageCodes.Contains(language))
            {
                return "devanagari_PP-OCRv5_mobile_rec";
            }

            if (language == "korean")
            {
                return "korean_PP-OCRv5_mobile_rec";
            }

            if (OwnModelLanguageCodes.Contains(language))
            {
                return $"{language}_PP-OCRv5_mobile_rec";
            }

            if (language == "ka")
            {
                // Georgian has no PP-OCRv5 recognition model yet.
                return "ka_PP-OCRv3_mobile_rec";
            }

            // Pali, plus the safety net for a code no script group claims - the v6 bundle
            // no longer has a general-purpose PP-OCRv5 model to fall back on.
            return "latin_PP-OCRv5_mobile_rec";
        }

        /// <inheritdoc cref="GetRecName"/>
        public static string GetDetectionName(string language, string mode)
        {
            // Georgian is the one remaining PP-OCRv3 language.
            if (language == "ka")
            {
                return "PP-OCRv3_mobile_det";
            }

            // Detector and recognition model come from the same PaddleOCR generation, which is
            // how upstream pairs them (PP-OCRv6 languages get the v6 detector of the same tier).
            return IsPpOcrV6Language(language)
                ? $"PP-OCRv6_{PpOcrV6Tier(mode)}_det"
                : $"PP-OCRv5_{mode}_det";
        }

        public static IReadOnlyCollection<string> LatinLanguageCodesForTest => LatinLanguageCodes;

        public static IEnumerable<string> AllScriptGroupCodesForTest =>
            LatinLanguageCodes
                .Concat(ArabicLanguageCodes)
                .Concat(EslavLanguageCodes)
                .Concat(CyrillicLanguageCodes)
                .Concat(DevanagariLanguageCodes)
                .Concat(OwnModelLanguageCodes)
                .Distinct();
    }
}
