using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Shared "favorite languages" used to bubble the user's preferred languages to the top of
    /// the various language combo boxes (auto-translate, speech-to-text, OCR, spell check, ...).
    /// Favorites are stored as a ";"-separated list of two-letter ISO 639-1 codes in
    /// <see cref="GeneralSettings.FavoriteLanguages"/>.
    /// </summary>
    public static class LanguageFavorites
    {
        /// <summary>
        /// Parses a ";"-separated list of favorite language codes into normalized, distinct,
        /// two-letter (lower-case) codes, preserving order.
        /// </summary>
        public static List<string> ParseCodes(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<string>();
            }

            return raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeTwoLetter)
                .Where(s => s.Length > 0)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Returns the items ordered so favorite languages come first (in the user's favorite order),
        /// while everything else keeps its original order. Matching is tolerant: two-letter, three-letter
        /// and region-qualified codes (e.g. "pt-BR") all match on the base two-letter code.
        /// </summary>
        /// <param name="items">The items to order.</param>
        /// <param name="codeSelector">Returns an item's language code (any ISO form).</param>
        /// <param name="favoriteCodes">The favorite language codes, in priority order.</param>
        /// <param name="pinTop">Optional predicate for items that must stay above the favorites
        /// (e.g. an "Auto detect" entry). These keep their relative order at the very top.</param>
        public static List<T> OrderWithFavoritesFirst<T>(
            IEnumerable<T> items,
            Func<T, string> codeSelector,
            IList<string> favoriteCodes,
            Func<T, bool> pinTop = null)
        {
            var list = items.ToList();
            var favorites = (favoriteCodes ?? new List<string>())
                .Select(NormalizeTwoLetter)
                .Where(s => s.Length > 0)
                .ToList();
            if (favorites.Count == 0)
            {
                return list;
            }

            int Rank(T item)
            {
                if (pinTop != null && pinTop(item))
                {
                    return -1; // above the favorites block
                }

                var code = NormalizeTwoLetter(codeSelector(item));
                if (code.Length == 0)
                {
                    return int.MaxValue;
                }

                var index = favorites.IndexOf(code);
                return index >= 0 ? index : int.MaxValue;
            }

            // Stable ordering: sort by rank, preserving the original order within the same rank.
            return list
                .Select((item, originalIndex) => (item, originalIndex))
                .OrderBy(x => Rank(x.item))
                .ThenBy(x => x.originalIndex)
                .Select(x => x.item)
                .ToList();
        }

        /// <summary>
        /// Normalizes an ISO language code to a comparable lower-case two-letter code.
        /// Handles two-letter ("en"), three-letter ("eng") and region-qualified ("pt-BR") forms.
        /// Returns an empty string when the code cannot be mapped.
        /// </summary>
        public static string NormalizeTwoLetter(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return string.Empty;
            }

            code = code.Trim().ToLowerInvariant();

            // strip a region/script qualifier, e.g. "pt-br" or "zh_cn"
            var separatorIndex = code.IndexOfAny(new[] { '-', '_' });
            if (separatorIndex > 0)
            {
                code = code.Substring(0, separatorIndex);
            }

            if (code.Length == 2)
            {
                return code;
            }

            if (code.Length == 3)
            {
                var twoLetter = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(code);
                return string.IsNullOrEmpty(twoLetter) ? code : twoLetter.ToLowerInvariant();
            }

            return code;
        }
    }
}
