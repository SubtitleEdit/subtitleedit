using System.Globalization;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace SeConv.Core;

/// <summary>
/// Parses <c>--pac-codepage</c> values. Accepts named identifiers (case-insensitive)
/// matching the PAC code page constants, plus the numeric index 0-12.
/// </summary>
internal static class PacCodepageParser
{
    private static readonly Dictionary<string, int> Named = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Latin"] = Pac.CodePageLatin,
        ["Greek"] = Pac.CodePageGreek,
        ["LatinCzech"] = Pac.CodePageLatinCzech,
        ["Czech"] = Pac.CodePageLatinCzech,
        ["Arabic"] = Pac.CodePageArabic,
        ["Hebrew"] = Pac.CodePageHebrew,
        ["Thai"] = Pac.CodePageThai,
        ["Cyrillic"] = Pac.CodePageCyrillic,
        ["ChineseTraditional"] = Pac.CodePageChineseTraditional,
        ["ChineseSimplified"] = Pac.CodePageChineseSimplified,
        ["Korean"] = Pac.CodePageKorean,
        ["Japanese"] = Pac.CodePageJapanese,
        ["LatinTurkish"] = Pac.CodePageLatinTurkish,
        ["Turkish"] = Pac.CodePageLatinTurkish,
        ["LatinPortuguese"] = Pac.CodePageLatinPortuguese,
        ["Portuguese"] = Pac.CodePageLatinPortuguese,
    };

    public static int Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new FormatException("PAC code page is empty.");
        }

        var s = input.Trim();
        if (Named.TryGetValue(s, out var named))
        {
            return named;
        }

        if (int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var num) &&
            num >= Pac.CodePageLatin && num <= Pac.CodePageLatinPortuguese)
        {
            return num;
        }

        throw new FormatException(
            $"PAC code page '{input}' is invalid. Expected a name (Latin, Greek, ...) or a number 0-12.");
    }
}
