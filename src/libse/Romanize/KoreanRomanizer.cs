using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Romanize
{
    /// <summary>
    /// Converts Hangul text to Latin script following (an approximation of)
    /// South Korea's Revised Romanization of Korean (RR).
    ///
    /// How it works:
    ///  - Every precomposed Hangul syllable (U+AC00..U+D7A3) is algorithmically
    ///    decomposed into initial / medial / final jamo using the standard
    ///    Unicode formula, then each part is mapped through the official RR
    ///    tables.
    ///  - The "linking" rule (연음) is applied: when a syllable's final consonant
    ///    is followed by a syllable with a null (ㅇ) initial, the consonant is
    ///    pronounced - and romanized - as that next syllable's onset instead of
    ///    as a coda (e.g. 국어 -> "gugeo", not "gukeo").
    ///
    /// Known limitations (documented rather than silently guessed at):
    ///  - Consonant assimilation across syllables (e.g. ㄱ+ㄴ -> ㅇ+ㄴ) is not
    ///    applied, so some words will romanize slightly differently than the
    ///    "hear it spoken" pronunciation would suggest.
    ///  - Linking is only applied for simple (single-jamo) finals; the eleven
    ///    consonant-cluster finals (ㄳ, ㄵ, ㄶ, ㄺ, ㄻ, ㄼ, ㄽ, ㄾ, ㄿ, ㅀ, ㅄ) are
    ///    romanized in place instead of being split across the syllable boundary.
    ///  - Standalone (non-precomposed) jamo, and proper-noun capitalization
    ///    conventions, are not specially handled.
    /// This is more than sufficient for subtitle readability; for publication-
    /// grade romanization, route through a dedicated linguistic service instead.
    /// </summary>
    public class KoreanRomanizer : IRomanizer
    {
        public const char CharLowerBound = '\uAC00';
        public const char CharUpperBound = '\uD7A3';

        public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("ko");

        private class Syllable
        {
            public int Initial;
            public int Medial;
            public int Final;
            public string OverrideInitialText;
            public bool SuppressFinal;
        }

        private const int SBase = 0xAC00;
        private const int LCount = 19;
        private const int VCount = 21;
        private const int TCount = 28;
        private const int NCount = VCount * TCount;
        private const int SCount = LCount * NCount;
        private const int NullInitialIndex = 11; // ㅇ

        private static readonly string[] Initials = 
        {
            "g", "kk", "n", "d", "tt", "r", "m", "b", "pp", "s",
            "ss", "", "j", "jj", "ch", "k", "t", "p", "h"
        };
        private static readonly string[] Medials = 
        {
            "a", "ae", "ya", "yae", "eo", "e", "yeo", "ye", "o", "wa",
            "wae", "oe", "yo", "u", "wo", "we", "wi", "yu", "eu", "ui", "i"
        };
        private static readonly string[] Finals = 
        {
            // Index 0 = no final consonant.

            "", "k", "k", "k", "n", "n", "n", "t", "l", "k",
            "m", "l", "l", "l", "p", "l", "m", "p", "p", "t",
            "t", "ng", "t", "t", "k", "t", "p", "t"
        };

        private static readonly Dictionary<int, string> LinkableFinalToOnset = new Dictionary<int, string>
        {
            // Simple (non-cluster) finals that can be relinked as the next
            // syllable's onset consonant when that syllable has a null initial.

            [1] = "g",   // ㄱ
            [4] = "n",   // ㄴ
            [7] = "d",   // ㄷ
            [8] = "r",   // ㄹ
            [16] = "m",  // ㅁ
            [17] = "b",  // ㅂ
            [19] = "s",  // ㅅ
            [20] = "ss", // ㅆ
            [22] = "j",  // ㅈ
            [23] = "ch", // ㅊ
            [24] = "k",  // ㅋ
            [25] = "t",  // ㅌ
            [26] = "p",  // ㅍ
            [27] = "h",  // ㅎ
        };

        private static void ApplyLinking(List<object> tokens)
        {
            for (int i = 0; i < tokens.Count - 1; i++)
            {
                if (tokens[i] is Syllable cur && tokens[i + 1] is Syllable next)
                {
                    if (cur.Final != 0
                        && next.Initial == NullInitialIndex
                        && LinkableFinalToOnset.TryGetValue(cur.Final, out var onset))
                    {
                        next.OverrideInitialText = onset;
                        cur.SuppressFinal = true;
                    }
                }
            }
        }
        private static List<object> Decompose(string text)
        {
            var tokens = new List<object>(text.Length);
            foreach (var ch in text)
            {
                int code = ch;
                if (code >= SBase && code < SBase + SCount)
                {
                    int sIndex = code - SBase;
                    int li = sIndex / NCount;
                    int vi = (sIndex % NCount) / TCount;
                    int ti = sIndex % TCount;
                    tokens.Add(new Syllable { Initial = li, Medial = vi, Final = ti });
                }
                else
                {
                    tokens.Add(ch);
                }
            }
            return tokens;
        }
        private static string Render(List<object> tokens)
        {
            var sb = new StringBuilder();
            foreach (var token in tokens)
            {
                if (token is char c)
                {
                    sb.Append(c);
                }
                else if (token is Syllable s)
                {
                    sb.Append(s.OverrideInitialText ?? Initials[s.Initial]);
                    sb.Append(Medials[s.Medial]);
                    if (!s.SuppressFinal) sb.Append(Finals[s.Final]);
                }
            }
            return sb.ToString();
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
            var tokens = Decompose(text);
            ApplyLinking(tokens);
            return Render(tokens);
        }
    }
}