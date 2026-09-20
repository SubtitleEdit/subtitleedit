using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    public static class StringWithoutSpaceSplitToWords
    {
        public static string SplitWord(string[] words, string input, string threeLetterIsoLanguageName)
        {
            if (!Configuration.Settings.Tools.OcrUseWordSplitList)
            {
                return input;
            }

            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var wordSet = GetWordSet(words);
            if (wordSet.Words.Contains(input) || IsProperCaseToKeep(input, threeLetterIsoLanguageName))
            {
                return input;
            }

            // Word break by dynamic programming: wordCount[i] is the fewest list words that
            // input.Substring(i) can be split into, wordLength[i] the length of the first of them.
            // Masking the longest word first (the overload below) can take a word that blocks the
            // only valid split, and it scans the whole list for every input.
            var n = input.Length;
            var maxLength = Math.Min(wordSet.MaxLength, n - 1); // a word must be shorter than the input
            var wordCount = new int[n + 1];
            var wordLength = new int[n + 1];
            for (var i = n - 1; i >= 0; i--)
            {
                wordCount[i] = int.MaxValue;
                for (var len = Math.Min(maxLength, n - i); len >= 1; len--) // longest first wins a tie
                {
                    var rest = wordCount[i + len];
                    if (rest == int.MaxValue || rest + 1 >= wordCount[i])
                    {
                        continue;
                    }

                    if (wordSet.Words.Contains(input.Substring(i, len)))
                    {
                        wordCount[i] = rest + 1;
                        wordLength[i] = len;
                    }
                }
            }

            if (wordCount[0] == int.MaxValue)
            {
                return input;
            }

            var sb = new StringBuilder(n + wordCount[0]);
            for (var i = 0; i < n; i += wordLength[i])
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(input, i, wordLength[i]);
            }

            return sb.ToString();
        }

        private sealed class WordSet
        {
            public string[] Source { get; set; }
            public HashSet<string> Words { get; set; }
            public int MaxLength { get; set; }
        }

        private static WordSet _lastWordSet;

        /// <summary>
        /// The list as a set, cached per array instance (callers load the list once and pass the
        /// same array for every word).
        /// </summary>
        private static WordSet GetWordSet(string[] words)
        {
            var cached = _lastWordSet;
            if (cached != null && ReferenceEquals(cached.Source, words))
            {
                return cached;
            }

            var set = new HashSet<string>(words, StringComparer.Ordinal);
            var maxLength = 0;
            foreach (var word in words)
            {
                if (word.Length > maxLength)
                {
                    maxLength = word.Length;
                }
            }

            cached = new WordSet { Source = words, Words = set, MaxLength = maxLength };
            _lastWordSet = cached;
            return cached;
        }

        public static string SplitWord(string[] words, string input, string ignoreWord, List<string> usedWords, string threeLetterIsoLanguageName)
        {
            var s = input;
            var check = s;
            var spaces = new List<int>();

            if (words.Contains(input))
            {
                return input;
            }

            if (IsProperCaseToKeep(input, threeLetterIsoLanguageName))
            {
                return input;
            }

            for (var i = 0; i < words.Length; i++)
            {
                var w = words[i];
                if (w.Length >= input.Length)
                {
                    continue;
                }

                var idx = check.IndexOf(w, StringComparison.Ordinal);
                while (idx != -1 && w != ignoreWord)
                {
                    usedWords.Add(w);
                    spaces.Add(idx);
                    spaces.Add(idx + w.Length);
                    check = check.Remove(idx, w.Length).Insert(idx, string.Empty.PadLeft(w.Length, '¤'));
                    idx = check.IndexOf(w, idx + w.Length - 1, StringComparison.Ordinal);
                }
            }

            if (check.Trim('¤', ' ').Length > 0)
            {
                return input;
            }

            var last = -1;
            spaces = spaces.OrderBy(p => p).ToList();
            for (var i = spaces.Count - 1; i >= 0; i--)
            {
                var idx = spaces[i];
                if (idx != last)
                {
                    s = s.Insert(idx, " ");
                }

                last = idx;
            }

            return s.Trim();
        }

        private static bool IsProperCaseToKeep(string input, string threeLetterIsoLanguageName)
        {
            if (input.Length > 1 && // Configuration.Settings.Tools.OcrUseWordSplitListAvoidPropercase &&
                input.StartsWith(input[0].ToString().ToUpperInvariant()) &&
                input != input.ToLowerInvariant() && input != input.ToUpperInvariant() &&
                input.Length < 12)
            {
                if (input.StartsWith("Mc") || input.StartsWith("Mac"))
                {
                    return true;
                }

                if (input[0] == 'I' && threeLetterIsoLanguageName == "eng")
                {
                    // Allow split if the first letter is "I" (e.g. Iam)
                }
                else if (input[0] == 'A' && threeLetterIsoLanguageName == "eng")
                {
                    // Allow split if the first letter is "A" (e.g. Acat -> "A cat"). Testing the
                    // lowercase 'a' could never match: the enclosing guard only admits words whose
                    // first character already equals its own uppercase form. The 'I' sibling above
                    // uses the uppercase letter and works.
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] LoadWordSplitList(string dictionaryFolder, string threeLetterIsoLanguageName, List<string> names)
        {
            var fileName = Path.Combine(dictionaryFolder, $"{threeLetterIsoLanguageName}_WordSplitList.txt");
            if (!File.Exists(fileName))
            {
                return Array.Empty<string>();
            }

            var wordList = File.ReadAllText(fileName).SplitToLines().Where(p => p.Trim().Length > 0).ToList();

            if (threeLetterIsoLanguageName == "eng")
            {
                wordList.AddRange(new List<string>
                {
                    // Ignore list
                    "Andor", "honour", "honours", "putain", "whoah", "eastside", "Starpath", "comlink", "Taamet",
                    "Atwater", "Lakeview", "Glassman", "Starfleet", "Coulda", "Woulda", "percenters",
                    "starbase", "damnit", "Goddamnit", "Goodfellas", "Stillwater", "ahold", "Coldplay",
                });
            }

            wordList.AddRange(names.Where(p => p.Length > 4));

            return wordList.OrderByDescending(p => p.Length).ToArray();
        }
    }
}
