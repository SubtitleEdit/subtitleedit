using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Romanize
{
    /// <summary>
    /// Converts Cyrillic text to a Latin transliteration for readability.
    /// This implementation preserves basic case and applies a simple rule for
    /// "е"/"Е" romanization at word boundaries.
    /// </summary>
    public class RussianRomanizer : IRomanizer
    {
        public const char CharLowerBound = '\u0400';
        public const char CharUpperBound = '\u04FF';

        public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("ru");

        private static readonly Dictionary<char, string> BaseMap = new Dictionary<char, string>
        {
            ['а'] = "a",
            ['б'] = "b",
            ['в'] = "v",
            ['г'] = "g",
            ['д'] = "d",
            ['е'] = "e",
            ['ё'] = "yo",
            ['ж'] = "zh",
            ['з'] = "z",
            ['и'] = "i",
            ['й'] = "y",
            ['к'] = "k",
            ['л'] = "l",
            ['м'] = "m",
            ['н'] = "n",
            ['о'] = "o",
            ['п'] = "p",
            ['р'] = "r",
            ['с'] = "s",
            ['т'] = "t",
            ['у'] = "u",
            ['ф'] = "f",
            ['х'] = "kh",
            ['ц'] = "ts",
            ['ч'] = "ch",
            ['ш'] = "sh",
            ['щ'] = "shch",
            ['ъ'] = "",
            ['ы'] = "y",
            ['ь'] = "",
            ['э'] = "e",
            ['ю'] = "yu",
            ['я'] = "ya",
        };
        private static readonly HashSet<char> Vowels = new HashSet<char>
        {
            'а', 'э', 'ы', 'о', 'у', 'и', 'е', 'ё', 'ю', 'я'
        };
        private static readonly HashSet<char> SignLetters = new HashSet<char> { 'ъ', 'ь' };

        private static bool IsCyrillicLetter(char ch)
        {
            var lower = char.ToLowerInvariant(ch);
            return lower >= 'а' && lower <= 'я' || lower == 'ё';
        }
        private static string PreserveCase(char original, string translit)
        {
            if (!char.IsUpper(original) || string.IsNullOrEmpty(translit))
            {
                return translit;
            }

            if (translit.Length == 1)
            {
                return translit.ToUpperInvariant();
            }

            return char.ToUpperInvariant(translit[0]) + translit.Substring(1);
        }
        private static bool TryMapCyrillic(char current, char previous, out string translit)
        {
            var lower = char.ToLowerInvariant(current);

            if (lower == 'е')
            {
                var useYe = previous == '\0'
                    || !IsCyrillicLetter(previous)
                    || Vowels.Contains(char.ToLowerInvariant(previous))
                    || SignLetters.Contains(char.ToLowerInvariant(previous));

                translit = PreserveCase(current, useYe ? "ye" : "e");
                return true;
            }

            if (BaseMap.TryGetValue(lower, out var mapped))
            {
                translit = PreserveCase(current, mapped);
                return true;
            }

            translit = null;
            return false;
        }

        CultureInfo IRomanizer.Culture { get; } = Culture;

        public bool IsValid(char chr)
        {
            return (chr >= CharLowerBound) && (chr <= CharUpperBound);
        }
        public bool IsValid(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text.Any(_ => IsValid(_));
        }
        public string Romanize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length * 2);
            for (var i = 0; i < text.Length; i++)
            {
                var current = text[i];
                var previous = i > 0 ? text[i - 1] : '\0';

                if (TryMapCyrillic(current, previous, out var translit))
                {
                    sb.Append(translit);
                    continue;
                }

                sb.Append(current);
            }

            return sb.ToString();
        }
    }
}