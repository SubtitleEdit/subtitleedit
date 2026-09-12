
using Nikse.SubtitleEdit.Core.Romanize;

namespace LibSETests.Romanize;

/// <summary>
/// Test cases for ISO 9:1995 transliteration of Cyrillic characters into Latin.
/// Source: https://en.wikipedia.org/wiki/ISO_9
/// Each object[]: [0] = Cyrillic input, [1] = Expected ISO 9 Latin output
/// Note: several outputs require combining diacritical marks (not precomposed
/// Unicode codepoints), since ISO 9 is a strict one-to-one reversible mapping.
/// </summary>
public class CyrillicTests
{
    public static readonly CyrillicRomanizer Romanizer = new();
    public static readonly IList<object[]> Data = 
    [
        // Core Slavic alphabet (lowercase)
        [ "а", "a" ],
        [ "б", "b" ],
        [ "в", "v" ],
        [ "г", "g" ],
        [ "д", "d" ],
        [ "е", "e" ],
        [ "ё", "ë" ],
        [ "ж", "ž" ],
        [ "з", "z" ],
        [ "и", "i" ],
        [ "й", "j" ],
        [ "к", "k" ],
        [ "л", "l" ],
        [ "м", "m" ],
        [ "н", "n" ],
        [ "о", "o" ],
        [ "п", "p" ],
        [ "р", "r" ],
        [ "с", "s" ],
        [ "т", "t" ],
        [ "у", "u" ],
        [ "ф", "f" ],
        [ "х", "h" ],
        [ "ц", "c" ],
        [ "ч", "č" ],
        [ "ш", "š" ],
        [ "щ", "ŝ" ],
        [ "ъ", "ʺ" ],
        [ "ы", "y" ],
        [ "ь", "ʹ" ],
        [ "э", "è" ],
        [ "ю", "û" ],
        [ "я", "â" ],

        // Core Slavic alphabet (uppercase)
        [ "А", "A" ],
        [ "Б", "B" ],
        [ "В", "V" ],
        [ "Г", "G" ],
        [ "Д", "D" ],
        [ "Е", "E" ],
        [ "Ё", "Ë" ],
        [ "Ж", "Ž" ],
        [ "З", "Z" ],
        [ "И", "I" ],
        [ "Й", "J" ],
        [ "К", "K" ],
        [ "Л", "L" ],
        [ "М", "M" ],
        [ "Н", "N" ],
        [ "О", "O" ],
        [ "П", "P" ],
        [ "Р", "R" ],
        [ "С", "S" ],
        [ "Т", "T" ],
        [ "У", "U" ],
        [ "Ф", "F" ],
        [ "Х", "H" ],
        [ "Ц", "C" ],
        [ "Ч", "Č" ],
        [ "Ш", "Š" ],
        [ "Щ", "Ŝ" ],
        [ "Ъ", "ʺ" ],
        [ "Ы", "Y" ],
        [ "Ь", "ʹ" ],
        [ "Э", "È" ],
        [ "Ю", "Û" ],
        [ "Я", "Â" ],

        // Non-Russian Slavic letters (Ukrainian / Belarusian / Serbian / Macedonian)
        [ "ґ", "g̀" ],   // Ukrainian g
        [ "є", "ê" ],   // Ukrainian ye
        [ "і", "ì" ],   // Ukrainian/Belarusian i
        [ "ї", "ï" ],   // Ukrainian yi
        [ "ў", "ŭ" ],   // Belarusian short u
        [ "ђ", "đ" ],   // Serbian đe
        [ "ј", "ǰ" ],   // Serbian je
        [ "љ", "l̂" ],   // Serbian lje
        [ "њ", "n̂" ],   // Serbian nje
        [ "ћ", "ć" ],   // Serbian tshe
        [ "џ", "d̂" ],   // Serbian dzhe
        [ "ѕ", "ẑ" ],   // Macedonian dze
        [ "ќ", "ḱ" ],   // Macedonian kje
        [ "Ґ", "G̀" ],
        [ "Є", "Ê" ],
        [ "І", "Ì" ],
        [ "Ї", "Ï" ],
        [ "Ў", "Ŭ" ],
        [ "Ђ", "Đ" ],
        [ "Ј", "J̌" ],
        [ "Љ", "L̂" ],
        [ "Њ", "N̂" ],
        [ "Ћ", "Ć" ],
        [ "Џ", "D̂" ],
        [ "Ѕ", "Ẑ" ],
        [ "Ќ", "Ḱ" ],

        // Archaic / pre-reform Slavic letters
        [ "ѣ", "ě" ],   // yat
        [ "ѫ", "ǎ" ],   // big yus
        [ "ѳ", "f̀" ],   // fita
        [ "ѵ", "ỳ" ],   // izhitsa

        // Full-word / phrase example (Bulgarian, from ISO 9 Wikipedia article,
        // fragment of the Universal Declaration of Human Rights preamble)
        [ "като", "kato" ],
        [ "взе", "vze" ],
        [ "предвид", "predvid" ],
        [ "човешкия", "čoveškiâ" ],
        [ "род", "rod" ],
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public void Test(string input, string result) 
    {
        string romanized = Romanizer.Romanize(input);

        Assert.Equal(result, romanized);
    }
}