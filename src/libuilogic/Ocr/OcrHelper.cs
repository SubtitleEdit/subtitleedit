using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Ocr
{
    public static class OcrHelper
    {
        public static string PostOcr(string input, string language)
        {
            return FixInvalidCarriageReturnLineFeedCharacters(input);
        }

        /// <summary>
        /// Collapses the stray spaces the AI OCR engines leave around punctuation.
        /// The space before "!" and "?" is kept for the languages that require it - see
        /// <see cref="UsesSpaceBeforeQuestionAndExclamationMark"/>.
        /// </summary>
        /// <param name="input">Raw text as returned by the OCR engine.</param>
        /// <param name="language">OCR language, either an English name ("French") or a two letter code ("fr").</param>
        public static string FixAiOcrPunctuationSpaces(string? input, string? language)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var text = input;
            text = text.Replace(" ,", ",");
            text = text.Replace(" .", ".");

            if (!UsesSpaceBeforeQuestionAndExclamationMark(language))
            {
                text = text.Replace(" !", "!");
                text = text.Replace(" ?", "?");
            }

            text = text.Replace("( ", "(");
            text = text.Replace(" )", ")");
            text = text.Replace("\\\"", "\"");

            if (text.EndsWith("!'", StringComparison.Ordinal))
            {
                text = text.TrimEnd('\'');
            }

            return text;
        }

        /// <summary>
        /// Trims every line of an AI OCR result, not only the whole text. LightOnOCR-3 ends each
        /// line but the last with a markdown hard break (two spaces), which would otherwise stay in
        /// the subtitle text.
        /// </summary>
        public static string TrimAiOcrLines(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var lines = input.SplitToLines();
            for (var i = 0; i < lines.Count; i++)
            {
                lines[i] = lines[i].Trim();
            }

            return string.Join(Environment.NewLine, lines).Trim();
        }

        /// <summary>
        /// French and Breton typography puts a real space before "?" and "!", so removing it
        /// damages correct OCR output. Same rule as Utilities.FixOcrErrors in libse.
        /// </summary>
        /// <param name="language">OCR language, either an English name ("French") or a two letter code ("fr").</param>
        public static bool UsesSpaceBeforeQuestionAndExclamationMark(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return false;
            }

            var code = language.Trim();
            if (code.Length > 3)
            {
                code = Iso639Dash2LanguageCode.GetTwoLetterCodeFromEnglishName(code);
            }
            else if (code.Length == 3)
            {
                code = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(code);
            }

            return code.Equals("fr", StringComparison.OrdinalIgnoreCase) ||
                   code.Equals("br", StringComparison.OrdinalIgnoreCase);
        }

        private static string FixInvalidCarriageReturnLineFeedCharacters(string input)
        {
            return string.Join(Environment.NewLine, input.SplitToLines()).Trim();
        }
    }
}
