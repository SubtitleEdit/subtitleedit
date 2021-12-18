using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    public static class StringWithoutSpaceSplitToWords
    {
        public static string SplitWord(string[] words, string input)
        {
            var usedWords = new List<string>();
            var result = SplitWord(words, input, string.Empty, usedWords);
            if (result != input)
            {
                return result;
            }

            foreach (var usedWord in usedWords)
            {
                result = SplitWord(words, input, usedWord, new List<string>());
                if (result != input)
                {
                    return result;
                }
            }

            return input;
        }

        public static string SplitWord(string[] words, string input, string ignoreWord, List<string> usedWords)
        {
            var s = input;
            var check = s;
            var spaces = new List<int>();
            for (int i = 0; i < words.Length; i++)
            {
                var w = words[i];
                var idx = check.IndexOf(w, StringComparison.Ordinal);
                while (idx != -1 && w != ignoreWord)
                {
                    usedWords.Add(w);
                    spaces.Add(idx);
                    spaces.Add(idx + w.Length);
                    check = check.Remove(idx, w.Length).Insert(idx, string.Empty.PadLeft(w.Length, '¤'));
                    idx = check.IndexOf(w, idx + w.Length - 1);
                }
            }

            var last = -1;
            spaces = spaces.OrderBy(p => p).ToList();
            for (int i = spaces.Count - 1; i >= 0; i--)
            {
                var idx = spaces[i];
                if (idx != last)
                {
                    s = s.Insert(idx, " ");
                }

                last = idx;
            }

            return check.Trim('¤', ' ').Length == 0 ? s.Trim() : input;
        }

    }
}
