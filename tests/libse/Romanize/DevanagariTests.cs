
using Nikse.SubtitleEdit.Core.Romanize;

namespace LibSETests.Romanize;

/// <summary>
/// Test cases for the Hunterian transliteration system (national system of
/// romanization of India, adopted by the Government of India, 1954 revision
/// with macrons for long vowels).
/// Source: https://en.wikipedia.org/wiki/Hunterian_transliteration
///
/// IMPORTANT CAVEAT: the official Hunterian system, as criticized on its own
/// Wikipedia page, does NOT distinguish retroflex from dental consonants
/// (द/ड both -> d, त/ट both -> t, न/ण both -> n). This is a real, documented
/// ambiguity in the standard, not an omission here. Many modern practical
/// implementations use an unofficial extension with an under-dot diacritic
/// (ṭ ḍ ṇ ṛ) to disambiguate. Both variants are included below and labeled,
/// so you can pick which behavior your program is meant to implement.
/// Each object[]: [0] = Devanagari input, [1] = Expected Hunterian output
/// </summary>
public class DevanagariTests
{
    public static readonly DevanagariRomanizer Romanizer = new();
    public static readonly IList<object[]> Data = 
    [
        // 1. Independent vowels
        [ "अ", "a" ],
        [ "आ", "ā" ],
        [ "इ", "i" ],
        [ "ई", "ī" ],
        [ "उ", "u" ],
        [ "ऊ", "ū" ],
        [ "ऋ", "ri" ],
        [ "ए", "e" ],
        [ "ऐ", "ai" ],
        [ "ओ", "o" ],
        [ "औ", "au" ],

        // 2. Consonants with inherent schwa (क-series through ह)
        [ "क", "ka" ],
        [ "ख", "kha" ],
        [ "ग", "ga" ],
        [ "घ", "gha" ],
        [ "ङ", "nga" ],   // official Hunterian: nga (not ṅa)
        [ "च", "cha" ],
        [ "छ", "chha" ],
        [ "ज", "ja" ],
        [ "झ", "jha" ],
        [ "ञ", "nya" ],   // official Hunterian: nya (not ña)
        [ "त", "ta" ],
        [ "थ", "tha" ],
        [ "द", "da" ],
        [ "ध", "dha" ],
        [ "न", "na" ],
        [ "प", "pa" ],
        [ "फ", "pha" ],
        [ "ब", "ba" ],
        [ "भ", "bha" ],
        [ "म", "ma" ],
        [ "य", "ya" ],
        [ "र", "ra" ],
        [ "ल", "la" ],
        [ "व", "va" ],
        [ "स", "sa" ],
        [ "ह", "ha" ],

        // 3. Retroflex consonants
        //    - OfficialCollapsed: matches dental per strict Hunterian (documented flaw)
        //    - Diacritic variant (common unofficial extension): use these instead if
        //      your implementation targets the disambiguated form
        [ "ट", "ta" ],       // official: collapses with त. Diacritic form: "ṭa"
        [ "ठ", "tha" ],      // official: collapses with थ. Diacritic form: "ṭha"
        [ "ड", "da" ],       // official: collapses with द. Diacritic form: "ḍa"
        [ "ढ", "dha" ],      // official: collapses with ध. Diacritic form: "ḍha"
        [ "ण", "na" ],       // official: collapses with न. Diacritic form: "ṇa"

        // 4. Sibilants श/ष (also officially collapsed to same Latin form)
        [ "श", "sha" ],      // official: sha (or ś with diacritics)
        [ "ष", "sha" ],      // official: collapses with श. Diacritic form: "ṣa"

        // 5. Nukta (borrowed Persian/Arabic sounds via Urdu) consonants
        [ "क़", "qa" ],
        [ "ख़", "k͟ha" ],     // sometimes simplified to "kha" without diacritics
        [ "ग़", "ġa" ],      // sometimes simplified to "gha"
        [ "ज़", "za" ],
        [ "ड़", "ṛa" ],       // flapped r, sometimes "ra" without diacritics
        [ "फ़", "fa" ],

        // 6. Dependent vowel signs (mātrā) attached to a consonant (क)
        [ "का", "kā" ],
        [ "कि", "ki" ],
        [ "की", "kī" ],
        [ "कु", "ku" ],
        [ "कू", "kū" ],
        [ "के", "ke" ],
        [ "कै", "kai" ],
        [ "को", "ko" ],
        [ "कौ", "kau" ],
        [ "कृ", "kri" ],

        // 7. Anusvara / visarga / chandrabindu
        [ "कं", "kaṃ" ],     // anusvara -- diacritic-free form often just "kan"/"kam" per context
        [ "कः", "kaḥ" ],     // visarga -- diacritic-free form often just "kah"
        [ "कँ", "kam̐" ],     // chandrabindu (nasalization)

        // 8. Virama / halant (consonant cluster without inherent vowel)
        [ "क्", "k" ],        // halant strips the inherent 'a'
        [ "क्ष", "ksha" ],    // conjunct kṣa

        // 9. Schwa deletion rule (Hindi drops medial/final schwa; Sanskrit keeps it)
        [ "कानपुर", "kānpur" ],  // Hindi: schwa after न and after र dropped -> NOT kānapura
        [ "क्रम", "krama" ],      // Sanskrit: schwa retained -> krama, not kram
        [ "भारत", "bhārat" ],     // Hindi: final schwa dropped -> NOT bhārata
        [ "दिल्ली", "dillī" ],    // Hindi: Delhi

        // 10. Full-word examples (verified against the official Wikipedia example
        //     passage for Hunterian, "with diacritics" column)
        [ "मैं", "maĩ" ],
        [ "अपने", "apne" ],
        [ "संबंधी", "sambandhī" ],
        [ "मिला", "milā" ],
        [ "उसने", "usne" ],
        [ "मुझे", "mujhe" ],
        [ "चाय", "chāy" ],
        [ "पिलाई", "pilāī" ],
        [ "बारिश", "bāriś" ],
        [ "कारण", "kāraṇ" ],
        [ "वजह", "vajah" ],
        [ "नमस्ते", "namaste" ],
        [ "हिन्दी", "hindī" ],
        [ "मुंबई", "mumbaī" ],
        [ "गुजरात", "gujarāt" ],
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public void Test(string input, string result)
    {
        string romanized = Romanizer.Romanize(input);

        Assert.Equal(result, romanized);
    }
}