using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// "0" and "O" are the same ring in most subtitle fonts, and the image matchers decide each glyph
/// on its own. The neighbours settle it: a run of digits is a number ("5O" -> "50", "1O:3O" ->
/// "10:30"), and a "0" inside a word of letters is a letter ("0f" -> "of", "g0t" -> "got").
/// </summary>
public static partial class OcrDigitLetterFixer
{
    // Digits and capital O's, optionally grouped by . , : ("1,OOO", "1O:3O"), not touching letters.
    [GeneratedRegex(@"(?<![\p{L}\d])[\dO]+(?:[.,:][\dO]+)*(?![\p{L}\d])")]
    private static partial Regex NumberLikeRun();

    // A word of letters with zeros in it and no other digits.
    [GeneratedRegex(@"(?<![\p{L}\d])[\p{L}0]*0[\p{L}0]*(?![\p{L}\d])")]
    private static partial Regex WordWithZero();

    // A percent sign split into its parts: ring, slash, ring ("7o/o" -> "7%").
    [GeneratedRegex(@"(?<=\d) ?[oO0]/[oO0](?![\p{L}\d])")]
    private static partial Regex SplitPercent();

    // Letters that pass for digits, so "З000" (3000 with a misread "3") stays a number.
    private const string DigitLookalikes = "ЗзOoIlSsBbZzбg";

    [GeneratedRegex(@"(?:^|[.!?])[\s\-–—""'“‘]*(?:<[^>]+>[\s\-–—""'“‘]*)*$")]
    private static partial Regex SentenceStartBefore();

    public static string Fix(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        if (text.Contains('/'))
        {
            text = SplitPercent().Replace(text, "%");
        }

        if (text.Contains('O'))
        {
            text = NumberLikeRun().Replace(text, m =>
                m.Value.Contains('O') && m.Value.Any(char.IsDigit) ? m.Value.Replace('O', '0') : m.Value);
        }

        if (text.Contains('0'))
        {
            text = WordWithZero().Replace(text, m =>
            {
                var word = m.Value;
                var letters = word.Where(char.IsLetter).ToList();
                if (letters.Count == 0 || word.Count(c => c == '0') > letters.Count && letters.All(c => DigitLookalikes.Contains(c)))
                {
                    // Mostly zeros next to a digit lookalike is a number with a misread digit
                    // ("З000" = 3000), not a word - but "D00-wah" is "Doo".
                    return word;
                }

                var upper = letters.Count > 1 && letters.All(char.IsUpper);
                var chars = word.ToCharArray();
                for (var i = 0; i < chars.Length; i++)
                {
                    if (chars[i] != '0')
                    {
                        continue;
                    }

                    var capital = upper ||
                                  i == 0 && letters.Count == 1 && char.IsUpper(letters[0]) ||
                                  i == 0 && SentenceStartBefore().IsMatch(text.Substring(0, m.Index));
                    chars[i] = capital ? 'O' : 'o';
                }

                return new string(chars);
            });
        }

        return text;
    }
}
