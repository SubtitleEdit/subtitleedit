
using Nikse.SubtitleEdit.Core.Romanize;

namespace LibSETests.Romanize;

/// <summary>
/// Test cases for the Encyclopaedia Aethiopica (EAe) romanization system for Ge'ez.
/// Source: Siegbert Uhlig et al. (eds.), Encyclopaedia Aethiopica, vol. 1: A-C
/// (Wiesbaden: Harrassowitz Verlag, 2003), pp. xx-xxi -- table cross-referenced via
/// secondary academic sources (the primary table sits behind a robots-disallowed
/// page and could not be fetched directly). Confirmed via multiple independent
/// descriptions of the EAe system (Wikipedia "Geʽez", "Encyclopaedia Aethiopica").
///
/// KEY DISTINGUISHING FEATURE of EAe vs. other academic systems (e.g. Lambdin):
/// EAe uses ä for the 1st (inherent) vowel order and a for the 4th order, whereas
/// many other systems use a/ā for the same contrast. EAe also uses plain e/o
/// (no macrons/circumflexes) for the 5th/7th orders, and ə for the 6th-order schwa.
///
/// Ge'ez is an abugida (syllabary): each of the ~26 base consonant symbols has a
/// glyph for each of 7 vowel "orders". Confidence on individual glyph-to-Latin
/// mappings for uncommon consonants is HIGH (cross-verified); confidence on the
/// exact vowel-order Latin letters (e/ē, o/ō) is MEDIUM since the primary table
/// could not be directly inspected.
///
/// </summary>
public class GeezTests
{
    public static readonly GeezRomanizer Romanizer = new();
    public static readonly IList<object[]> Data = 
    [
        // 1. The seven vowel orders, demonstrated on the consonant ሀ (h)
        //    Order 1 = inherent/citation form (ä) -- EAe's signature distinguishing choice
        [ "ሀ", "hä" ],   // order 1 (inherent ä, not a)
        [ "ሁ", "hu" ],   // order 2
        [ "ሂ", "hi" ],   // order 3
        [ "ሃ", "ha" ],   // order 4 (plain a -- contrasts with order 1's ä)
        [ "ሄ", "he" ],   // order 5
        [ "ህ", "hə" ],   // order 6 (schwa / often near-silent in modern pronunciation)
        [ "ሆ", "ho" ],   // order 7

        // 2. Base consonant inventory (citation form = order 1, with inherent ä)
        [ "ለ", "lä" ],
        [ "ሐ", "ḥä" ],   // pharyngeal ḥ (distinct from ሀ h)
        [ "መ", "mä" ],
        [ "ሠ", "śä" ],   // distinct grapheme from ሰ (s); merged in pronunciation, kept distinct in EAe transliteration
        [ "ረ", "rä" ],
        [ "ሰ", "sä" ],
        [ "ቀ", "qä" ],   // ejective /kʼ/
        [ "በ", "bä" ],
        [ "ተ", "tä" ],
        [ "ኀ", "ḫä" ],   // voiceless uvular/velar fricative, distinct from ሐ ḥ
        [ "ነ", "nä" ],
        [ "አ", "ʾä" ],   // glottal stop (alef)
        [ "ከ", "kä" ],
        [ "ወ", "wä" ],
        [ "ዐ", "ʿä" ],   // voiced pharyngeal (ayin)
        [ "ዘ", "zä" ],
        [ "የ", "yä" ],
        [ "ደ", "dä" ],
        [ "ገ", "gä" ],
        [ "ጠ", "ṭä" ],   // ejective /tʼ/
        [ "ጰ", "p̣ä" ],   // ejective /pʼ/
        [ "ጸ", "ṣä" ],   // ejective /sʼ/
        [ "ፀ", "ḍä" ],   // 4th ejective, merged with ጸ in pronunciation but distinct grapheme
        [ "ፈ", "fä" ],
        [ "ፐ", "pä" ],

        // 3. Labialized (wa-series) consonants -- consonant + place sign for -ʷa
        //    Directly confirmed by the OpenEdition source table description.
        [ "ሏ", "lʷa" ],
        [ "ቧ", "bʷa" ],
        [ "ቷ", "tʷa" ],
        [ "ሯ", "rʷa" ],
        [ "ኧ", "ʾʷa" ],

        // 4. Gemination -- Ge'ez script does not mark consonant doubling
        //    orthographically, so a converter should NOT invent gemination
        //    marks that aren't attested in the source text.

        // 5. Real attested words / names (directly cited or well-documented in
        //    academic Ethiopic-studies sources, high confidence)
        [ "ግዕዝ", "Gəʿəz" ],      // "Ge'ez" (the language's own name)
        [ "ምኒልክ", "Mənilək" ],   // Emperor Menelik II -- explicitly
                                                    // cited on the EAe Wikipedia page as
                                                    // an example of the system's output
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public void Test(string input, string result)
    {
        string romanized = Romanizer.Romanize(input);

        Assert.Equal(result, romanized);
    }
}