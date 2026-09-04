using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Romanize
{
    /// <summary>
    /// Converts Devanagari (Hindi) text to Latin script following the Library of
    /// Congress ALA-LC Hindi Romanization Table (2025 revision):
    /// https://www.loc.gov/catdir/cpso/romanization/hindi.pdf
    ///
    /// How it works:
    ///  - Independent vowels, consonants, vowel signs (mātrās), virāma, nukta
    ///    forms, anusvāra, anunāsika, visarga, avagraha and Devanagari digits
    ///    are each mapped through the LC table.
    ///  - The implicit "a" that follows an unmarked consonant (Note 2 in the
    ///    table) is supplied automatically, and suppressed when the consonant
    ///    is followed by a vowel sign or by virāma (्).
    ///  - Anusvāra is rendered contextually per Note 4 (ṅ/ñ/ṇ/n/m depending on
    ///    the following consonant class, falling back to ṃ).
    ///  - Anunāsika (chandrabindu) is rendered contextually per Note 5 (n̐
    ///    before guttural/palatal/cerebral/dental occlusives, m̐ otherwise).
    ///  - Avagraha doubling (Note 6) falls out naturally, since each avagraha
    ///    character is transliterated independently as an apostrophe.
    ///
    /// A few diacritics in the LC PDF's extracted text were garbled by OCR
    /// (e.g. ख़/ग़/घ़ all appeared as "kha"/"gha", and ष appeared as "sha").
    /// In those cases this class uses the standard, well-established ALA-LC
    /// values instead (k͟ha, ġa, ġha, ṣa) rather than reproducing the garbling.
    ///
    /// Known limitations (documented rather than silently guessed at):
    ///  - This does not attempt schwa deletion / colloquial "Hindi pronunciation"
    ///    romanization (e.g. रामायण would come out "rāmāyaṇa", not "rāmāyan");
    ///    it follows the formal, letter-by-letter LC transliteration scheme.
    ///  - Conjunct consonants (consonant + virāma + consonant) are transliterated
    ///    consonant-by-consonant with no vowel between them, but no special
    ///    ligature/half-form handling is needed since Unicode Devanagari already
    ///    encodes conjuncts as plain consonant + virāma sequences.
    ///  - Rare Vedic/extended signs (stress marks U+0951-U+0954, U+0970-U+097F)
    ///    are passed through unchanged rather than transliterated.
    ///  - Nukta forms not listed in the LC table are passed through as their
    ///    base consonant plus a literal nukta character.
    /// This is more than sufficient for subtitle readability; for publication-
    /// grade romanization, route through a dedicated linguistic service instead.
    /// </summary>
    public class DevanagariRomanizer : IRomanizer
    {
        public const char CharLowerBound = '\u0900';
        public const char CharUpperBound = '\u097F';

        private const char Virama = '\u094D';
        private const char Nukta = '\u093C';
        private const char Chandrabindu = '\u0901';
        private const char Anusvara = '\u0902';
        private const char Visarga = '\u0903';
        private const char Avagraha = '\u093D';
        private const char Om = '\u0950';
        private const char Danda = '\u0964';
        private const char DoubleDanda = '\u0965';

        // Independent vowels (syllable-initial position).
        private static readonly Dictionary<char, string> IndependentVowels = new Dictionary<char, string>
        {
            ['\u0904'] = "ê",   // ऄ short a (rare)
            ['\u0905'] = "a",   // अ
            ['\u0906'] = "ā",   // आ
            ['\u0907'] = "i",   // इ
            ['\u0908'] = "ī",   // ई
            ['\u0909'] = "u",   // उ
            ['\u090A'] = "ū",   // ऊ
            ['\u090B'] = "r̥",  // ऋ
            ['\u090C'] = "l̥",  // ऌ
            ['\u090D'] = "ê",   // ऍ candra e
            ['\u090E'] = "ĕ",   // ऎ short e
            ['\u090F'] = "e",   // ए
            ['\u0910'] = "ai",  // ऐ
            ['\u0911'] = "ô",   // ऑ candra o
            ['\u0912'] = "ŏ",   // ऒ short o
            ['\u0913'] = "o",   // ओ
            ['\u0914'] = "au",  // औ
            ['\u0960'] = "r̥̄", // ॠ
            ['\u0961'] = "l̥̄", // ॡ
        };

        // Dependent vowel signs (mātrās) attached to a consonant.
        private static readonly Dictionary<char, string> VowelSigns = new Dictionary<char, string>
        {
            ['\u093E'] = "ā",
            ['\u093F'] = "i",
            ['\u0940'] = "ī",
            ['\u0941'] = "u",
            ['\u0942'] = "ū",
            ['\u0943'] = "r̥",
            ['\u0944'] = "r̥̄",
            ['\u0945'] = "ê",
            ['\u0946'] = "ĕ",
            ['\u0947'] = "e",
            ['\u0948'] = "ai",
            ['\u0949'] = "ô",
            ['\u094A'] = "ŏ",
            ['\u094B'] = "o",
            ['\u094C'] = "au",
            ['\u0962'] = "l̥",
            ['\u0963'] = "l̥̄",
        };

        // Base consonants (LC table's Gutturals/Palatals/Cerebrals/Dentals/Labials/Semivowels/Sibilants/Aspirate).
        private static readonly Dictionary<char, string> Consonants = new Dictionary<char, string>
        {
            // Gutturals
            ['\u0915'] = "k",
            ['\u0916'] = "kh",
            ['\u0917'] = "g",
            ['\u0918'] = "gh",
            ['\u0919'] = "ṅ",
            // Palatals
            ['\u091A'] = "c",
            ['\u091B'] = "ch",
            ['\u091C'] = "j",
            ['\u091D'] = "jh",
            ['\u091E'] = "ñ",
            // Cerebrals (retroflex)
            ['\u091F'] = "ṭ",
            ['\u0920'] = "ṭh",
            ['\u0921'] = "ḍ",
            ['\u0922'] = "ḍh",
            ['\u0923'] = "ṇ",
            // Dentals
            ['\u0924'] = "t",
            ['\u0925'] = "th",
            ['\u0926'] = "d",
            ['\u0927'] = "dh",
            ['\u0928'] = "n",
            // Labials
            ['\u092A'] = "p",
            ['\u092B'] = "ph",
            ['\u092C'] = "b",
            ['\u092D'] = "bh",
            ['\u092E'] = "m",
            // Semivowels
            ['\u092F'] = "y",
            ['\u0930'] = "r",
            ['\u0932'] = "l",
            ['\u0935'] = "v",
            // Sibilants
            ['\u0936'] = "ś",
            ['\u0937'] = "ṣ",
            ['\u0938'] = "s",
            // Aspirate
            ['\u0939'] = "h",
            // Marginal letters occasionally seen in Devanagari-Hindi text (Note 7 / other languages)
            ['\u0929'] = "ṉ",
            ['\u0931'] = "ṟ",
            ['\u0933'] = "ḷ",
            ['\u0934'] = "ḻ",
        };

        // Nukta (़) + base consonant combinations used for Urdu loanwords, per the
        // LC table's bracketed rows. Keyed by the *base* consonant character.
        private static readonly Dictionary<char, string> NuktaConsonants = new Dictionary<char, string>
        {
            ['\u0915'] = "q",    // क़
            ['\u0916'] = "k͟h",  // ख़
            ['\u0917'] = "ġ",    // ग़
            ['\u0918'] = "ġh",   // घ़ (rare)
            ['\u091C'] = "z",    // ज़
            ['\u0921'] = "ṛ",    // ड़
            ['\u0922'] = "ṛh",   // ढ़
            ['\u092B'] = "f",    // फ़
            ['\u0938'] = "s̤",   // स़
            ['\u0939'] = "h̤",   // ह़
        };

        // Precomposed nukta-consonant codepoints (U+0958-U+095F), equivalent to the
        // decomposed forms above but encoded as single characters.
        private static readonly Dictionary<char, string> PrecomposedNuktaConsonants = new Dictionary<char, string>
        {
            ['\u0958'] = "q",
            ['\u0959'] = "k͟h",
            ['\u095A'] = "ġ",
            ['\u095B'] = "z",
            ['\u095C'] = "ṛ",
            ['\u095D'] = "ṛh",
            ['\u095E'] = "f",
            ['\u095F'] = "ẏ",
        };

        private static readonly Dictionary<char, char> Digits = new Dictionary<char, char>
        {
            ['\u0966'] = '0',
            ['\u0967'] = '1',
            ['\u0968'] = '2',
            ['\u0969'] = '3',
            ['\u096A'] = '4',
            ['\u096B'] = '5',
            ['\u096C'] = '6',
            ['\u096D'] = '7',
            ['\u096E'] = '8',
            ['\u096F'] = '9',
        };

        RomanizerLanguages IRomanizer.Language { get; } = RomanizerLanguages.Devanagari;

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
            var sb = new StringBuilder(text.Length * 2);
            var i = 0;
            while (i < text.Length)
            {
                var c = text[i];

                if (PrecomposedNuktaConsonants.TryGetValue(c, out var precomposed))
                {
                    i = AppendConsonant(text, i + 1, precomposed, sb);
                    continue;
                }

                if (Consonants.TryGetValue(c, out var baseCons))
                {
                    if (i + 1 < text.Length && text[i + 1] == Nukta && NuktaConsonants.TryGetValue(c, out var nuktaVariant))
                    {
                        i = AppendConsonant(text, i + 2, nuktaVariant, sb);
                    }
                    else
                    {
                        i = AppendConsonant(text, i + 1, baseCons, sb);
                    }
                    continue;
                }

                if (IndependentVowels.TryGetValue(c, out var vowel))
                {
                    sb.Append(vowel);
                    i++;
                    continue;
                }

                if (c == Anusvara)
                {
                    sb.Append(AnusvaraFor(text, i));
                    i++;
                    continue;
                }
                if (c == Chandrabindu)
                {
                    sb.Append(ChandrabinduFor(text, i));
                    i++;
                    continue;
                }
                if (c == Visarga)
                {
                    sb.Append("ḥ");
                    i++;
                    continue;
                }
                if (c == Avagraha)
                {
                    sb.Append('’');
                    i++;
                    continue;
                }
                if (c == Om)
                {
                    sb.Append("oṃ");
                    i++;
                    continue;
                }
                if (c == Danda)
                {
                    sb.Append('.');
                    i++;
                    continue;
                }
                if (c == DoubleDanda)
                {
                    sb.Append("..");
                    i++;
                    continue;
                }
                if (Digits.TryGetValue(c, out var digit))
                {
                    sb.Append(digit);
                    i++;
                    continue;
                }
                if (c == Virama)
                {
                    // Stray virāma with no preceding consonant handled here; normally
                    // consumed inside AppendConsonant.
                    i++;
                    continue;
                }
                if (VowelSigns.TryGetValue(c, out var strayVowelSign))
                {
                    // Stray vowel sign with no preceding consonant.
                    sb.Append(strayVowelSign);
                    i++;
                    continue;
                }

                // Anything else (Latin text already in the string, punctuation,
                // whitespace, or unhandled Vedic/extended signs) passes through.
                sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Appends a consonant's romanization, then resolves whether it is
        /// followed by virāma (no vowel), a vowel sign (that vowel), or
        /// nothing (implicit "a", per Note 2).
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="nextIndex">Index immediately after the consonant (and its nukta, if any).</param>
        /// <param name="romanizedConsonant">Already-resolved romanization of the consonant.</param>
        /// <param name="sb">Output buffer.</param>
        /// <returns>The index to resume scanning from.</returns>
        private static int AppendConsonant(string text, int nextIndex, string romanizedConsonant, StringBuilder sb)
        {
            sb.Append(romanizedConsonant);

            if (nextIndex < text.Length && text[nextIndex] == Virama)
            {
                return nextIndex + 1; // conjunct: no vowel between this consonant and the next
            }
            if (nextIndex < text.Length && VowelSigns.TryGetValue(text[nextIndex], out var vowelSign))
            {
                sb.Append(vowelSign);
                return nextIndex + 1;
            }

            sb.Append('a'); // implicit inherent vowel
            return nextIndex;
        }

        /// <summary>Anusvāra, transliterated contextually per Note 4.</summary>
        private static string AnusvaraFor(string text, int i)
        {
            if (i + 1 < text.Length)
            {
                var next = text[i + 1];
                if (next >= '\u0915' && next <= '\u0919') return "ṅ"; // before gutturals
                if (next >= '\u091A' && next <= '\u091E') return "ñ"; // before palatals
                if (next >= '\u091F' && next <= '\u0923') return "ṇ"; // before cerebrals
                if (next >= '\u0924' && next <= '\u0928') return "n"; // before dentals
                if (next >= '\u092A' && next <= '\u092E') return "m"; // before labials
            }
            return "ṃ";
        }

        /// <summary>Anunāsika (candrabindu), transliterated contextually per Note 5.</summary>
        private static string ChandrabinduFor(string text, int i)
        {
            if (i + 1 < text.Length)
            {
                var next = text[i + 1];
                if (next >= '\u0915' && next <= '\u0928') // guttural/palatal/cerebral/dental occlusives
                {
                    return "n̐";
                }
            }
            return "m̐"; // labials, sibilants, semivowels, aspirates, vowels, final position
        }
    }
}