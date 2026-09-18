using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.AudioToText
{
    public class GermanNouns
    {
        private readonly List<string> _germanNouns;
        private readonly Dictionary<Regex, string> _regularExpressionList;

        // FixCasing searches the text for every name it is given, and the noun list has over
        // 4600 entries - per subtitle line. A noun made of letters only can match nothing but a
        // whole word (FixCasing wants a non-letter on both sides), so those are looked up by the
        // words of the line instead. The few others ("New York", "U-Bahn") are always passed on.
        private readonly Dictionary<string, int> _letterOnlyNounIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly List<int> _otherNounIndexes = new List<int>();

        public GermanNouns()
        {
            _germanNouns = new List<string>();
            var inputFile = Path.Combine(Configuration.DictionariesDirectory, "deu_Nouns.txt");
            if (File.Exists(inputFile))
            {
                _germanNouns = FileUtil.ReadAllLinesShared(inputFile, Encoding.UTF8).Select(p => p.Trim()).Where(p => p.Length > 1).ToList();
            }

            for (var i = 0; i < _germanNouns.Count; i++)
            {
                var noun = _germanNouns[i];
                if (!IsLettersOnly(noun) || !_letterOnlyNounIndexes.TryAdd(noun, i))
                {
                    _otherNounIndexes.Add(i);
                }
            }

            _regularExpressionList = new Dictionary<Regex, string>
            {
                { new Regex(@"\bDas essen\b", RegexOptions.Compiled), "Das Essen" },
                { new Regex(@"\bdas essen\b", RegexOptions.Compiled), "das Essen" }
            };
        }

        public string UppercaseNouns(string text)
        {
            var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
            if (textNoTags != textNoTags.ToUpperInvariant() && !string.IsNullOrEmpty(text))
            {
                var st = new StrippableText(text);

                st.FixCasing(GetNounsThatCanMatch(st.StrippedText), true, false, false, string.Empty);

                foreach (var regex in _regularExpressionList.Keys)
                {
                    st.StrippedText = regex.Replace(st.StrippedText, _regularExpressionList[regex]);
                }

                return st.MergedString;
            }

            return text;
        }

        /// <summary>
        /// The nouns FixCasing could find in <paramref name="text"/>, in noun list order.
        /// </summary>
        private List<string> GetNounsThatCanMatch(string text)
        {
            var indexes = new List<int>(_otherNounIndexes);
            var lookup = _letterOnlyNounIndexes.GetAlternateLookup<ReadOnlySpan<char>>();
            var span = text.AsSpan();
            var i = 0;
            while (i < span.Length)
            {
                if (!char.IsLetter(span[i]))
                {
                    i++;
                    continue;
                }

                var start = i;
                while (i < span.Length && char.IsLetter(span[i]))
                {
                    i++;
                }

                if (lookup.TryGetValue(span.Slice(start, i - start), out var index))
                {
                    indexes.Add(index);
                }
            }

            indexes.Sort();
            var nouns = new List<string>(indexes.Count);
            var last = -1;
            foreach (var index in indexes)
            {
                if (index != last)
                {
                    nouns.Add(_germanNouns[index]);
                    last = index;
                }
            }

            return nouns;
        }

        private static bool IsLettersOnly(string s)
        {
            foreach (var c in s)
            {
                if (!char.IsLetter(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
