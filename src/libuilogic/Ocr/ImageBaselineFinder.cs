using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Finds how far the last text line of a subtitle image hangs below its baseline, so
/// "Bottom align (baseline)" can put "abc" and "jabc" on the same line: a plain bottom align
/// lines up the image edges, which lifts every line that has a descender (g, j, p, q, y, comma).
/// Uses the nOCR letter splitter, so no OCR database is needed.
/// </summary>
public static class ImageBaselineFinder
{
    // Same as the OCR two-color threshold: the fill is kept, dark outlines and shadows are not,
    // so touching outlines don't merge the letters into one blob.
    private const int TwoColorMinRgb = 200;

    // Under 45% of the tallest glyph is a floating mark (quote, apostrophe, dash), as in the splitter.
    private const int MarkHeightPercent = 45;

    // Descenders reach ~25% of the line height; more than this is a misread, not a descender.
    private const double MaxDescentRatio = 0.35;

    /// <summary>
    /// Returns the number of pixels between the baseline of the image's last text line and the
    /// lowest letter pixel of that line, or 0 when there is no descender or it can't be told.
    /// </summary>
    /// <param name="bitmap">The subtitle image.</param>
    /// <param name="minLineHeight">Minimum line height for the splitter: 25 for Blu-ray sized images, 12 otherwise.</param>
    public static int GetDescent(SKBitmap bitmap, int minLineHeight)
    {
        if (bitmap.Width <= 0 || bitmap.Height <= 0)
        {
            return 0;
        }

        var descent = GetDescent(new NikseBitmap2(bitmap), TwoColorMinRgb, minLineHeight);
        if (descent < 0)
        {
            // Dark text (black on a light box): fall back to every visible pixel.
            descent = GetDescent(new NikseBitmap2(bitmap), 0, minLineHeight);
        }

        return Math.Max(0, descent);
    }

    /// <summary>Returns the descent, or -1 when the image has no letters at this threshold.</summary>
    private static int GetDescent(NikseBitmap2 nikseBitmap, int minRgb, int minLineHeight)
    {
        nikseBitmap.MakeTwoColor(minRgb);
        var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nikseBitmap, 1, false, true, minLineHeight, true);

        // The splitter can end on a line break with nothing after it, so take the last line
        // break that has a glyph after it.
        var lastGlyph = letters.FindLastIndex(p => p.NikseBitmap != null);
        if (lastGlyph < 0)
        {
            return -1;
        }

        var lastLineStart = letters.FindLastIndex(lastGlyph, p => p.SpecialCharacter == Environment.NewLine) + 1;
        var glyphs = letters
            .Skip(lastLineStart)
            .Where(p => p.NikseBitmap != null)
            .Select(p => (Top: p.Top, Bottom: p.Top + p.NikseBitmap!.Height, p.NikseBitmap!.Height))
            .ToList();

        var maxHeight = glyphs.Max(p => p.Height);
        var letterBottoms = glyphs
            .Where(p => p.Height * 100 >= maxHeight * MarkHeightPercent)
            .Select(p => p.Bottom)
            .ToList();
        if (letterBottoms.Count == 0)
        {
            return 0;
        }

        // Every letter that isn't a descender sits on the baseline, so the highest letter bottom
        // is the baseline - also in a line where most letters descend ("gypsy"), which fools
        // a median. Floating marks are left out by their height; a comma doesn't set the
        // baseline, but it does count for how far the line hangs below it.
        var baseline = letterBottoms.Min();
        var lineBottom = glyphs.Max(p => p.Bottom);
        var lineTop = glyphs.Min(p => p.Top);
        var descent = lineBottom - baseline;
        return descent > (lineBottom - lineTop) * MaxDescentRatio ? 0 : descent;
    }
}
