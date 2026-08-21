using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Romanize
{
    /// <summary>
    /// Converts modern (monotonic) Greek text to a Latin transliteration for readability,
    /// following the "transcription" mode of ELOT 743 (= ISO 843) - the phonetic system
    /// used by the Greek government for passports, IDs, and road signs. This is deliberately
    /// the phonetic mode rather than the strict reversible "transliteration" mode also
    /// defined by the same standard (that mode maps every letter 1:1, e.g. μπ always -> "mp",
    /// αυ always -> "au", with no context-sensitivity - the transcription mode used here is
    /// closer to how the word actually sounds).
    ///
    /// Unlike Cyrillic/Ge'ez, Greek needs real multi-character logic:
    ///  - Vowel pairs αι/ει/οι/ου are single sounds and merge into one Latin digraph.
    ///  - αυ/ευ/ηυ merge into "av/af", "ev/ef", "iv/if" depending on whether the *next* sound
    ///    is voiceless (θ κ ξ π σ τ φ χ ψ) or not - this needs a one-character lookahead past
    ///    the digraph itself.
    ///  - A dialytika (diaeresis, ϊ/ϋ) on the second vowel cancels all of the above - it's a
    ///    signal that the vowels are pronounced separately, not as a merged sound.
    ///  - μπ/ντ/γγ/γκ/γξ/γχ ("nasal + stop") consonant clusters merge into a single sound,
    ///    and μπ additionally depends on whether it's at the start of a word (isWordStart) or
    ///    not - mirroring the Cyrillic class's use of the *previous* character, just applied
    ///    to a two-character window instead of one.
    ///
    /// Case handling follows the same simplification as CyrillicRomanizer's PreserveCase:
    /// case is taken from the first source letter only, and only its first output letter is
    /// capitalized (so an all-caps Greek word won't produce an all-caps Latin word for
    /// multi-letter outputs like "th"/"ch"/"ps"/"mp" - that's a known, accepted simplification,
    /// consistent with how the other Romanize classes in this project behave).
    ///
    /// Scope: only the core monotonic Greek block (U+0370-U+03FF) is covered. Polytonic Greek
    /// (breathing marks, circumflex, iota subscript etc., in the Greek Extended block
    /// U+1F00-U+1FFF) is out of scope, same spirit as skipping Ethiopic's multiplicative
    /// numerals or Arabic's presentation-form blocks in the other Romanize classes.
    /// </summary>
    public class GreekRomanizer : IRomanizer
    {
        public const char CharLowerBound = '\u0370';
        public const char CharUpperBound = '\u03FF';

        // Maps every monotonic Greek code point (plain, accented, and/or with dialytika,
        // upper or lower) to its plain lowercase base letter, so digraph detection doesn't
        // need to special-case every accented variant separately.
        private static readonly Dictionary<char, char> NormalizeBase = new Dictionary<char, char>
        {
            // Uppercase base letters
            ['\u0391'] = 'α',
            ['\u0392'] = 'β',
            ['\u0393'] = 'γ',
            ['\u0394'] = 'δ',
            ['\u0395'] = 'ε',
            ['\u0396'] = 'ζ',
            ['\u0397'] = 'η',
            ['\u0398'] = 'θ',
            ['\u0399'] = 'ι',
            ['\u039A'] = 'κ',
            ['\u039B'] = 'λ',
            ['\u039C'] = 'μ',
            ['\u039D'] = 'ν',
            ['\u039E'] = 'ξ',
            ['\u039F'] = 'ο',
            ['\u03A0'] = 'π',
            ['\u03A1'] = 'ρ',
            ['\u03A3'] = 'σ',
            ['\u03A4'] = 'τ',
            ['\u03A5'] = 'υ',
            ['\u03A6'] = 'φ',
            ['\u03A7'] = 'χ',
            ['\u03A8'] = 'ψ',
            ['\u03A9'] = 'ω',

            // Lowercase base letters (ς, final sigma, normalizes the same as σ)
            ['\u03B1'] = 'α',
            ['\u03B2'] = 'β',
            ['\u03B3'] = 'γ',
            ['\u03B4'] = 'δ',
            ['\u03B5'] = 'ε',
            ['\u03B6'] = 'ζ',
            ['\u03B7'] = 'η',
            ['\u03B8'] = 'θ',
            ['\u03B9'] = 'ι',
            ['\u03BA'] = 'κ',
            ['\u03BB'] = 'λ',
            ['\u03BC'] = 'μ',
            ['\u03BD'] = 'ν',
            ['\u03BE'] = 'ξ',
            ['\u03BF'] = 'ο',
            ['\u03C0'] = 'π',
            ['\u03C1'] = 'ρ',
            ['\u03C2'] = 'σ',
            ['\u03C3'] = 'σ',
            ['\u03C4'] = 'τ',
            ['\u03C5'] = 'υ',
            ['\u03C6'] = 'φ',
            ['\u03C7'] = 'χ',
            ['\u03C8'] = 'ψ',
            ['\u03C9'] = 'ω',

            // Accented (tonos) forms - uppercase
            ['\u0386'] = 'α',
            ['\u0388'] = 'ε',
            ['\u0389'] = 'η',
            ['\u038A'] = 'ι',
            ['\u038C'] = 'ο',
            ['\u038E'] = 'υ',
            ['\u038F'] = 'ω',

            // Accented (tonos) forms - lowercase
            ['\u03AC'] = 'α',
            ['\u03AD'] = 'ε',
            ['\u03AE'] = 'η',
            ['\u03AF'] = 'ι',
            ['\u03CC'] = 'ο',
            ['\u03CD'] = 'υ',
            ['\u03CE'] = 'ω',

            // Dialytika (diaeresis) forms - uppercase
            ['\u03AA'] = 'ι',
            ['\u03AB'] = 'υ',

            // Dialytika (diaeresis) forms - lowercase
            ['\u03CA'] = 'ι',
            ['\u03CB'] = 'υ',

            // Dialytika + tonos combined - lowercase only
            ['\u0390'] = 'ι',
            ['\u03B0'] = 'υ',
        };

        // Characters that carry a dialytika (diaeresis) - signals "pronounce this vowel
        // separately", which cancels vowel-digraph merging.
        private static readonly HashSet<char> DiaeresisSet = new HashSet<char>
        {
            '\u03AA', '\u03AB', '\u03CA', '\u03CB', '\u0390', '\u03B0',
        };

        private static readonly Dictionary<char, string> SingleMap = new Dictionary<char, string>
        {
            ['α'] = "a",
            ['β'] = "v",
            ['γ'] = "g",
            ['δ'] = "d",
            ['ε'] = "e",
            ['ζ'] = "z",
            ['η'] = "i",
            ['θ'] = "th",
            ['ι'] = "i",
            ['κ'] = "k",
            ['λ'] = "l",
            ['μ'] = "m",
            ['ν'] = "n",
            ['ξ'] = "x",
            ['ο'] = "o",
            ['π'] = "p",
            ['ρ'] = "r",
            ['σ'] = "s",
            ['τ'] = "t",
            ['υ'] = "y",
            ['φ'] = "f",
            ['χ'] = "ch",
            ['ψ'] = "ps",
            ['ω'] = "o",
        };

        // Voiceless consonants - determines whether a following αυ/ευ/ηυ resolves to "f" (before
        // a voiceless sound) or "v" (before a voiced sound, a vowel, or end of word).
        private static readonly HashSet<char> VoicelessConsonants = new HashSet<char>
        {
            'θ', 'κ', 'ξ', 'π', 'σ', 'τ', 'φ', 'χ', 'ψ',
        };

        private static readonly Dictionary<char, string> PunctuationMap = new Dictionary<char, string>
        {
            ['\u037E'] = "?",  // Greek question mark (looks like a semicolon)
            ['\u0387'] = ";",  // Greek ano teleia (raised dot, functions like a semicolon)
        };

        RomanizerLanguages IRomanizer.Language { get; } = RomanizerLanguages.Greek;

        public bool IsValid(char chr)
        {
            return (chr >= CharLowerBound) && (chr <= CharUpperBound);
        }

        public bool IsValid(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text.Any(IsValid);
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

                if (!NormalizeBase.TryGetValue(current, out var baseA))
                {
                    if (PunctuationMap.TryGetValue(current, out var punct))
                    {
                        sb.Append(punct);
                        continue;
                    }

                    // Not a Greek letter - pass through unchanged (spaces, Latin text,
                    // ordinary punctuation, etc.)
                    sb.Append(current);
                    continue;
                }

                var isUpperA = char.IsUpper(current);

                // Try a two-character digraph/cluster first.
                if (i + 1 < text.Length && NormalizeBase.TryGetValue(text[i + 1], out var baseB))
                {
                    var hasDiaeresisB = DiaeresisSet.Contains(text[i + 1]);
                    var isWordStart = i == 0 || !NormalizeBase.ContainsKey(text[i - 1]);
                    var voicelessFollows = i + 2 < text.Length
                        && NormalizeBase.TryGetValue(text[i + 2], out var baseC)
                        && VoicelessConsonants.Contains(baseC);

                    var merged = TryMergeDigraph(baseA, baseB, hasDiaeresisB, isWordStart, voicelessFollows);
                    if (merged != null)
                    {
                        sb.Append(ApplyCase(merged, isUpperA));
                        i++; // consumed the second character too
                        continue;
                    }
                }

                if (SingleMap.TryGetValue(baseA, out var single))
                {
                    sb.Append(ApplyCase(single, isUpperA));
                    continue;
                }

                sb.Append(current);
            }

            return sb.ToString();
        }

        private static string TryMergeDigraph(char baseA, char baseB, bool hasDiaeresisB, bool isWordStart, bool voicelessFollows)
        {
            if (hasDiaeresisB && (baseA == 'α' || baseA == 'ε' || baseA == 'η' || baseA == 'ο') && (baseB == 'ι' || baseB == 'υ'))
            {
                // Dialytika present - the vowels are pronounced separately, so no digraph merge.
                return null;
            }

            switch (baseA, baseB)
            {
                case ('α', 'ι'): return "ai";
                case ('ε', 'ι'): return "ei";
                case ('ο', 'ι'): return "oi";
                case ('ο', 'υ'): return "ou";
                case ('α', 'υ'): return voicelessFollows ? "af" : "av";
                case ('ε', 'υ'): return voicelessFollows ? "ef" : "ev";
                case ('η', 'υ'): return voicelessFollows ? "if" : "iv";
                case ('μ', 'π'): return isWordStart ? "b" : "mp";
                case ('ν', 'τ'): return "nt";
                case ('γ', 'γ'): return "ng";
                case ('γ', 'κ'): return "gk";
                case ('γ', 'ξ'): return "nx";
                case ('γ', 'χ'): return "nch";
                case ('τ', 'σ'): return "ts";
                case ('τ', 'ζ'): return "tz";
                default: return null;
            }
        }

        private static string ApplyCase(string translit, bool isUpperSource)
        {
            if (!isUpperSource || string.IsNullOrEmpty(translit))
            {
                return translit;
            }

            if (translit.Length == 1)
            {
                return translit.ToUpperInvariant();
            }

            return char.ToUpperInvariant(translit[0]) + translit.Substring(1);
        }
    }
}