using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// A name list prepared for <see cref="StrippableText.FixCasing(NameListIndex, bool, bool, bool, string, double)"/>.
    /// The casing pass searched the line once for every name - thousands per line - although a
    /// name can only be replaced where it is a whole word. A name without separator characters
    /// can only match a whole token of the line, and a name like "Mr. Smith" only where its first
    /// word is a token, so names are looked up by the tokens of the line here. Build it once per
    /// name list.
    /// </summary>
    public sealed class NameListIndex
    {
        /// <summary>
        /// Every character a name match may start after (StrippableText.IsNameStartBoundary) or
        /// end before (StrippableText.NameEndChars). None of them changes under case folding.
        /// </summary>
        private const string Separators = " -\"'>[“,.!?:;)]<”\r\n";
        private static readonly char[] SeparatorChars = Separators.ToCharArray();

        private readonly Dictionary<string, List<int>> _tokenNames = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        private readonly int[] _alwaysSearch;

        public List<string> Names { get; }

        public NameListIndex(IEnumerable<string> names)
        {
            Names = new List<string>(names);
            var alwaysSearch = new List<int>();
            for (var i = 0; i < Names.Count; i++)
            {
                // A match starts at a token start and the name's first word (up to its first
                // separator) is followed by that same separator in the line, so the token there
                // equals the first word - index the name by it. A name starting with a separator
                // has no first word and is always searched.
                var name = Names[i];
                var separatorIndex = string.IsNullOrEmpty(name) ? 0 : name.IndexOfAny(SeparatorChars);
                if (separatorIndex == 0)
                {
                    alwaysSearch.Add(i);
                    continue;
                }

                var firstWord = separatorIndex < 0 ? name : name.Substring(0, separatorIndex);
                if (!_tokenNames.TryGetValue(firstWord, out var positions))
                {
                    positions = new List<int>(1);
                    _tokenNames.Add(firstWord, positions);
                }

                positions.Add(i);
            }

            _alwaysSearch = alwaysSearch.ToArray();
        }

        /// <summary>
        /// Fills <paramref name="candidates"/> with the positions (ascending, so names are tried in
        /// list order) of the names that can match in <paramref name="text"/>.
        /// </summary>
        internal void GetCandidates(string text, List<int> candidates)
        {
            candidates.Clear();
            candidates.AddRange(_alwaysSearch);
            var added = false;
            var i = 0;
            while (i < text.Length)
            {
                if (Separators.IndexOf(text[i]) >= 0)
                {
                    i++;
                    continue;
                }

                var end = i + 1;
                while (end < text.Length && Separators.IndexOf(text[end]) < 0)
                {
                    end++;
                }

                if (_tokenNames.TryGetValue(text.Substring(i, end - i), out var positions))
                {
                    candidates.AddRange(positions);
                    added = true;
                }

                i = end;
            }

            if (!added)
            {
                return;
            }

            candidates.Sort();
            var count = 1;
            for (var k = 1; k < candidates.Count; k++)
            {
                if (candidates[k] != candidates[count - 1])
                {
                    candidates[count++] = candidates[k];
                }
            }

            candidates.RemoveRange(count, candidates.Count - count);
        }
    }
}
