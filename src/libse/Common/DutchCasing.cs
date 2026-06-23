using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Dutch-specific capitalization rules:
    ///  - The "IJ" digraph is capitalized as two letters: "ijsland" -> "IJsland" (not "Ijsland").
    ///  - Sentence-initial contractions ('s, 't, 'n, 'k, 'm, 'r) keep their lowercase letter and the
    ///    following word takes the capital instead: "'s morgens" -> "'s Morgens".
    /// Callers must gate on <see cref="IsDutch"/>; the methods themselves do not check the language.
    /// </summary>
    public static class DutchCasing
    {
        public const string LanguageCode = "nl";

        private static readonly char[] Apostrophes = { '\'', '’' }; // straight + typographic
        private const string ContractionLetters = "stnkmr"; // 's 't 'n 'k 'm 'r

        public static bool IsDutch(string language)
            => string.Equals(language, LanguageCode, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Capitalizes the first letter of <paramref name="word"/> (which must begin with the letter
        /// to capitalize), applying the Dutch IJ-digraph rule. Idempotent and safe on empty input.
        /// </summary>
        public static string CapitalizeFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return word;
            }

            // Word-initial "ij" is the IJ digraph - capitalize both letters. Skip when already "IJ".
            if (word.Length >= 2 &&
                (word[0] == 'i' || word[0] == 'I') &&
                (word[1] == 'j' || word[1] == 'J') &&
                !(word[0] == 'I' && word[1] == 'J'))
            {
                return "IJ" + word.Substring(2);
            }

            return char.ToUpper(word[0]) + word.Substring(1);
        }

        /// <summary>
        /// Applies Dutch sentence-start casing to <paramref name="text"/>, which must begin at the
        /// first character to consider (a leading apostrophe contraction is allowed; leading tags and
        /// other punctuation must already be stripped by the caller). Handles 's/'t/etc. contractions
        /// (the contraction stays lowercase and the next word is capitalized) and the IJ digraph for
        /// the word that ends up capitalized. Returns the input unchanged when nothing applies.
        /// </summary>
        public static string FixSentenceStart(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // "'s morgens" / "'S morgens" -> "'s Morgens": keep the contraction lowercase and move
            // the capital to the following word. A leading apostrophe that is NOT one of these
            // contractions (e.g. an opening quote) falls through to normal capitalization.
            if (text.Length > 3 &&
                Array.IndexOf(Apostrophes, text[0]) >= 0 &&
                ContractionLetters.IndexOf(char.ToLowerInvariant(text[1])) >= 0 &&
                text[2] == ' ')
            {
                var contraction = text[0] + char.ToLowerInvariant(text[1]).ToString();
                return contraction + " " + CapitalizeFirstLetter(text.Substring(3));
            }

            return CapitalizeFirstLetter(text);
        }
    }
}
