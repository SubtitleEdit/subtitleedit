
using Nikse.SubtitleEdit.Core.Romanize;

namespace LibSETests.Romanize;

/// <summary>
/// Test cases for Revised Hepburn romanization of Japanese kana (hiragana/katakana).
/// Source: https://en.wikipedia.org/wiki/Romanization_of_Japanese
/// and standard Revised Hepburn conventions (macron long vowels, apostrophe for
/// syllabic ん disambiguation).
/// Each object[]: [0] = Kana input, [1] = Expected Hepburn output
/// </summary>
public class KanaTests
{
    public static readonly KanaRomanizer Romanizer = new();
    public static readonly IList<object[]> Data = 
    [
        // 1. Basic vowels
        [ "あ", "a" ],
        [ "い", "i" ],
        [ "う", "u" ],
        [ "え", "e" ],
        [ "お", "o" ],

        // 2. Basic gojūon (plain consonant rows)
        [ "か", "ka" ], [ "き", "ki" ], [ "く", "ku" ], [ "け", "ke" ], [ "こ", "ko" ],
        [ "さ", "sa" ], [ "し", "shi" ], [ "す", "su" ], [ "せ", "se" ], [ "そ", "so" ],
        [ "た", "ta" ], [ "ち", "chi" ], [ "つ", "tsu" ], [ "て", "te" ], [ "と", "to" ],
        [ "な", "na" ], [ "に", "ni" ], [ "ぬ", "nu" ], [ "ね", "ne" ], [ "の", "no" ],
        [ "は", "ha" ], [ "ひ", "hi" ], [ "ふ", "fu" ], [ "へ", "he" ], [ "ほ", "ho" ],
        [ "ま", "ma" ], [ "み", "mi" ], [ "む", "mu" ], [ "め", "me" ], [ "も", "mo" ],
        [ "や", "ya" ], [ "ゆ", "yu" ], [ "よ", "yo" ],
        [ "ら", "ra" ], [ "り", "ri" ], [ "る", "ru" ], [ "れ", "re" ], [ "ろ", "ro" ],
        [ "わ", "wa" ], [ "を", "o" ],   // を romanized as "o" (particle usage), not "wo"
        [ "ん", "n" ],

        // 3. Dakuten (voiced) rows
        [ "が", "ga" ], [ "ぎ", "gi" ], [ "ぐ", "gu" ], [ "げ", "ge" ], [ "ご", "go" ],
        [ "ざ", "za" ], [ "じ", "ji" ], [ "ず", "zu" ], [ "ぜ", "ze" ], [ "ぞ", "zo" ],
        [ "だ", "da" ], [ "ぢ", "ji" ], [ "づ", "zu" ], [ "で", "de" ], [ "ど", "do" ],
        [ "ば", "ba" ], [ "び", "bi" ], [ "ぶ", "bu" ], [ "べ", "be" ], [ "ぼ", "bo" ],

        // 4. Handakuten (semi-voiced) row
        [ "ぱ", "pa" ], [ "ぴ", "pi" ], [ "ぷ", "pu" ], [ "ぺ", "pe" ], [ "ぽ", "po" ],

        // 5. Yōon (palatalized combinations with small ゃゅょ)
        [ "きゃ", "kya" ], [ "きゅ", "kyu" ], [ "きょ", "kyo" ],
        [ "しゃ", "sha" ], [ "しゅ", "shu" ], [ "しょ", "sho" ],
        [ "ちゃ", "cha" ], [ "ちゅ", "chu" ], [ "ちょ", "cho" ],
        [ "にゃ", "nya" ], [ "にゅ", "nyu" ], [ "にょ", "nyo" ],
        [ "ひゃ", "hya" ], [ "ひゅ", "hyu" ], [ "ひょ", "hyo" ],
        [ "みゃ", "mya" ], [ "みゅ", "myu" ], [ "みょ", "myo" ],
        [ "りゃ", "rya" ], [ "りゅ", "ryu" ], [ "りょ", "ryo" ],
        [ "ぎゃ", "gya" ], [ "ぎゅ", "gyu" ], [ "ぎょ", "gyo" ],
        [ "じゃ", "ja" ], [ "じゅ", "ju" ], [ "じょ", "jo" ],
        [ "びゃ", "bya" ], [ "びゅ", "byu" ], [ "びょ", "byo" ],
        [ "ぴゃ", "pya" ], [ "ぴゅ", "pyu" ], [ "ぴょ", "pyo" ],

        // 6. Sokuon (small っ) - doubles the following consonant
        [ "がっこう", "gakkō" ],      // school - kk
        [ "きって", "kitte" ],         // stamp - tt
        [ "ざっし", "zasshi" ],        // magazine - ssh (shi -> sshi)
        [ "けっか", "kekka" ],         // result - kk
        [ "いっぽん", "ippon" ],       // one (long object) - pp

        // 7. Long vowels - macron rules
        [ "おかあさん", "okāsan" ],    // あ + あ -> ā
        [ "おにいさん", "oniisan" ],   // い + い -> ii (NOT macron - i-row long vowel stays as ii)
        [ "くうき", "kūki" ],          // う + う -> ū
        [ "おねえさん", "onēsan" ],    // え + え -> ē (rare, mostly in this word)
        [ "とおい", "tōi" ],           // お + お -> ō
        [ "とうきょう", "Tōkyō" ],      // お + う (お-row + う) -> ō (long o via u-kana)
        [ "ゆうめい", "yūmei" ],        // う-row long vowel -> ū

        // 8. Moraic ん assimilation before b/m/p -> written as "m" in Hepburn
        [ "しんぶん", "shimbun" ],     // ん before ぶ(b) -> m
        [ "さんぽ", "sampo" ],         // ん before ぽ(p) -> m
        [ "えんぴつ", "empitsu" ],     // ん before ぴ(p) -> m
        [ "かんぱい", "kampai" ],      // ん before ぱ(p) -> m
        [ "にんじん", "ninjin" ],      // ん before じ (not b/m/p) -> stays n

        // 9. Apostrophe disambiguation for syllabic ん before vowel/y
        [ "じゅんいちろう", "Jun'ichirō" ],  // ん + い disambiguated with apostrophe
        [ "ほんや", "hon'ya" ],              // ん + や (bookstore, vs ほにゃ)
        [ "きんえん", "kin'en" ],            // ん + え (no smoking)

        // 10. Particle exceptions (kana pronounced differently as grammatical particles)
        [ "わたしは", "watashi wa" ],  // は as topic particle -> wa (not ha)
        [ "がっこうへ", "gakkō e" ],   // へ as directional particle -> e (not he)
        [ "ほんをよむ", "hon o yomu" ], // を as object particle -> o (not wo)

        // 11. Katakana - basic (same phonetic values as hiragana)
        [ "ア", "a" ], [ "カ", "ka" ], [ "サ", "sa" ], [ "タ", "ta" ], [ "ナ", "na" ],
        [ "ハ", "ha" ], [ "マ", "ma" ], [ "ヤ", "ya" ], [ "ラ", "ra" ], [ "ワ", "wa" ],

        // 12. Katakana extended combinations for foreign loanwords
        [ "ファ", "fa" ],   // f + a (e.g. ファイル file)
        [ "フィ", "fi" ],
        [ "フェ", "fe" ],
        [ "フォ", "fo" ],
        [ "ティ", "ti" ],   // e.g. パーティー party
        [ "ディ", "di" ],   // e.g. ディズニー Disney
        [ "トゥ", "tu" ],
        [ "ドゥ", "du" ],
        [ "ウィ", "wi" ],
        [ "ウェ", "we" ],
        [ "ウォ", "wo" ],
        [ "ヴァ", "va" ],
        [ "ヴィ", "vi" ],
        [ "ヴ", "vu" ],
        [ "ヴェ", "ve" ],
        [ "ヴォ", "vo" ],
        [ "チェ", "che" ],
        [ "シェ", "she" ],
        [ "ジェ", "je" ],

        // 13. Katakana long vowel mark (ー) - chōonpu
        [ "コーヒー", "kōhī" ],        // coffee
        [ "パーティー", "pātī" ],       // party
        [ "スーパー", "sūpā" ],         // supermarket
        [ "チーズ", "chīzu" ],          // cheese

        // 14. Full word / combined stress tests
        [ "にほん", "Nihon" ],          // Japan
        [ "とうきょうと", "Tōkyō-to" ], // Tokyo (metropolis)
        [ "しんじゅく", "Shinjuku" ],
        [ "コンピューター", "konpyūtā" ], // computer - loanword with sokuon-like double-length vowel
        [ "ありがとうございます", "arigatō gozaimasu" ],
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public void Test(string input, string result)
    {
        string romanized = Romanizer.Romanize(input);

        Assert.Equal(result, romanized);
    }
}