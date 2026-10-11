namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public class NOcrCaseFixer : INOcrCaseFixer
{
    /// <summary>
    /// Cyrillic letters whose lowercase is a smaller copy of the uppercase in the usual subtitle
    /// fonts, as lowercase/uppercase pairs. "д", "у", "ц" and "щ" have a descender, so their case
    /// shows in the height above the baseline, not in the whole height.
    /// </summary>
    public const string CyrillicCaseTwins = "вВгГдДжЖзЗиИкКлЛмМнНоОпПсСтТуУхХцЦчЧшШщЩъЪыЫьЬэЭюЮяЯєЄѕЅљЉњЊџЏ";

    private static readonly HashSet<string> UppercaseLikeLowercase = new() { "V", "W", "U", "S", "Z", "O", "X", "Ø", "C" };
    private static readonly HashSet<string> LowercaseLikeUppercase = new() { "v", "w", "u", "s", "z", "o", "x", "ø", "c" };
    private static readonly HashSet<string> UppercaseWithAccent = new() { "Č", "Š", "Ž", "Ś", "Ż", "Ö", "Ü", "Ú", "Ï", "Í", "Ç", "Ì", "Ò", "Ù", "Ó", "Й" };
    private static readonly HashSet<string> LowercaseWithAccent = new() { "č", "š", "ž", "ś", "ż", "ö", "ü", "ú", "ï", "í", "ç", "ì", "ò", "ù", "ó", "й" };

    private long _ocrLowercaseHeightsTotal;
    private int _ocrLowercaseHeightsTotalCount;
    private long _ocrUppercaseHeightsTotal;
    private int _ocrUppercaseHeightsTotalCount;

    // Heights above the baseline of x-height and cap-height reference letters.
    private long _xHeightTotal;
    private int _xHeightCount;
    private long _capHeightTotal;
    private int _capHeightCount;

    public bool HasWarmedUp { get; set; }

    /// <summary>
    /// Fix uppercase/lowercase issues (not I/l)
    /// </summary>
    public string FixUppercaseLowercaseIssues(ImageSplitterItem2 targetItem, NOcrChar result)
    {
        if (targetItem.NikseBitmap == null)
        {
            return result.Text;
        }

        // Reference heights: letters with no lookalike in the other case.
        if (result.Text is "e" or "a" or "d" or "t" or "а" or "е")
        {
            _ocrLowercaseHeightsTotalCount++;
            _ocrLowercaseHeightsTotal += targetItem.NikseBitmap.Height;
            if (_ocrUppercaseHeightsTotalCount < 3)
            {
                _ocrUppercaseHeightsTotalCount++;
                _ocrUppercaseHeightsTotal += targetItem.NikseBitmap.Height + 10;
            }
        }

        if (result.Text is "E" or "H" or "R" or "D" or "T" or "M" or "Б" or "Е" or "Р" or "А")
        {
            _ocrUppercaseHeightsTotalCount++;
            _ocrUppercaseHeightsTotal += targetItem.NikseBitmap.Height;
            if (_ocrLowercaseHeightsTotalCount < 3 && targetItem.NikseBitmap.Height > 20)
            {
                _ocrLowercaseHeightsTotalCount++;
                _ocrLowercaseHeightsTotal += targetItem.NikseBitmap.Height - 10;
            }
        }

        if (result.Text is "e" or "a" or "а" or "е")
        {
            _xHeightTotal += targetItem.HeightAboveBaseline;
            _xHeightCount++;
        }
        else if (result.Text is "E" or "H" or "R" or "D" or "T" or "M" or "Б" or "Е" or "Р" or "А")
        {
            _capHeightTotal += targetItem.HeightAboveBaseline;
            _capHeightCount++;
        }

        // Merged letter pairs ("гу", "жд" drawn as one connected glyph) are case twins as a
        // whole when both letters are twins of the same case.
        if (result.Text.Length > 0 && result.Text.All(c => CyrillicCaseTwins.Contains(c)) &&
            (result.Text.All(char.IsLower) || result.Text.All(char.IsUpper)))
        {
            return FixCyrillicCase(targetItem, result);
        }

        if (_ocrLowercaseHeightsTotalCount <= 2 || _ocrUppercaseHeightsTotalCount <= 2)
        {
            return result.Text;
        }

        // Latin letters where lowercase versions look like uppercase version
        if (UppercaseLikeLowercase.Contains(result.Text))
        {
            var averageLowercase = _ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount;
            var averageUppercase = _ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount;
            if (Math.Abs(averageLowercase - targetItem.NikseBitmap.Height) < Math.Abs(averageUppercase - targetItem.NikseBitmap.Height))
            {
                return result.Text.ToLowerInvariant();
            }

            return result.Text;
        }

        if (LowercaseLikeUppercase.Contains(result.Text))
        {
            var averageLowercase = _ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount;
            var averageUppercase = _ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount;
            if (Math.Abs(averageLowercase - targetItem.NikseBitmap.Height) > Math.Abs(averageUppercase - targetItem.NikseBitmap.Height))
            {
                return result.Text.ToUpperInvariant();
            }

            return result.Text;
        }

        if (UppercaseWithAccent.Contains(result.Text))
        {
            var averageUppercase = _ocrUppercaseHeightsTotal / (double)_ocrUppercaseHeightsTotalCount;
            if (targetItem.NikseBitmap.Height < averageUppercase + 3)
            {
                return result.Text.ToLowerInvariant();
            }

            return result.Text;
        }

        if (LowercaseWithAccent.Contains(result.Text))
        {
            var averageUppercase = _ocrUppercaseHeightsTotal / (double)_ocrUppercaseHeightsTotalCount;
            if (targetItem.NikseBitmap.Height > averageUppercase + 4)
            {
                return result.Text.ToUpperInvariant();
            }
        }

        return result.Text;
    }

    /// <summary>
    /// Picks the case of a Cyrillic case-twin letter from its height above the baseline: closer
    /// to the x-height of "а"/"е" is lowercase, closer to the cap height of "Б"/"Е"/"Р"/"А" is
    /// uppercase. Until both have been seen a few times the matcher's choice stands, and so does
    /// a lowercase entry that was trained as tall as a capital: that shape is lowercase only (the
    /// "∂"-like "д" of some fonts, which has no descender and reaches the cap height).
    /// </summary>
    private string FixCyrillicCase(ImageSplitterItem2 targetItem, NOcrChar result)
    {
        var text = result.Text;
        if (_xHeightCount <= 2 || _capHeightCount <= 2 ||
            char.IsLower(text[0]) && result.MarginTop * 10 <= result.Height)
        {
            return text;
        }

        var xHeight = _xHeightTotal / (double)_xHeightCount;
        var capHeight = _capHeightTotal / (double)_capHeightCount;
        var height = targetItem.HeightAboveBaseline;
        return Math.Abs(height - xHeight) < Math.Abs(height - capHeight)
            ? text.ToLowerInvariant()
            : text.ToUpperInvariant();
    }
}
