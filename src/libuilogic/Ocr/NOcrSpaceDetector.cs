namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Decides which gaps between glyphs are word spaces, relative to the text size instead of a
/// fixed pixel count. A fixed "pixels are space" value cannot fit every source: DVD-sized text
/// has 7-9 px word spaces (all lost at the default 12), while widely tracked all-caps Blu-ray
/// text has letter gaps of 12 px. Measured on real subtitles, letter gaps stay below ~0.19 of
/// the line's ink height and word spaces start at ~0.22 - except for tracked all-caps lines,
/// where letters sit up to 0.32 apart and spaces start at 0.47. So the threshold is found per
/// image from the biggest jump in its sorted gap sizes, and falls back to the median of earlier
/// images (one-word lines have no jump) or to <see cref="DefaultRelativeThreshold"/>.
/// <para>
/// Split with <see cref="SplitPixelsAreSpace"/> so every gap becomes a space item, then call
/// <see cref="RemoveFalseSpaces"/>. One instance per OCR run, like <see cref="OcrLineHeightTracker"/>.
/// </para>
/// </summary>
public class NOcrSpaceDetector
{
    /// <summary>"Pixels are space" value to split with, so that every gap is reported.</summary>
    public const int SplitPixelsAreSpace = 1;

    private const double DefaultRelativeThreshold = 0.22;
    private const double MinJumpRatio = 1.5;
    private const double MinJumpSize = 0.08;
    private const double MinSpace = 0.18;
    private const double MaxLetterGap = 0.5;
    private const int MaxHistory = 100;

    private readonly Lock _lock = new();
    private readonly List<double> _history = new();

    /// <summary>
    /// Removes the space items of <paramref name="letters"/> that are only gaps between letters.
    /// </summary>
    /// <param name="letters">Split with <see cref="SplitPixelsAreSpace"/>.</param>
    /// <param name="pixelsAreSpace">The threshold used, in pixels (of the first line), for code
    /// that still needs a pixel value.</param>
    public List<ImageSplitterItem2> RemoveFalseSpaces(List<ImageSplitterItem2> letters, out int pixelsAreSpace)
    {
        var lines = GetLines(letters);
        var relativeThreshold = FindRelativeThreshold(letters, lines);

        var result = new List<ImageSplitterItem2>(letters.Count);
        pixelsAreSpace = -1;
        foreach (var (start, end, height) in lines)
        {
            var threshold = Math.Max(2, (int)Math.Round(height * relativeThreshold, MidpointRounding.AwayFromZero));
            if (pixelsAreSpace < 0 && height > 0)
            {
                pixelsAreSpace = threshold;
            }

            var gaps = GetEffectiveGaps(letters, start, end, height, threshold);
            for (var i = start; i < end; i++)
            {
                var item = letters[i];
                if (item.SpecialCharacter == " " && item.NikseBitmap == null && gaps[i - start] < threshold)
                {
                    continue;
                }

                result.Add(item);
            }

            if (end < letters.Count)
            {
                result.Add(letters[end]); // the line break
            }
        }

        if (pixelsAreSpace < 0)
        {
            pixelsAreSpace = 12;
        }

        return result;
    }

    /// <summary>
    /// Drops word spaces inside numbers. The narrow "1" leaves most of its advance width empty
    /// on both sides, so the gap next to it beats the space threshold while the gaps between
    /// the other digits of the same number do not ("1 5", "1 940", "61 1", "15 1"). Between two
    /// digits (italic too), and from an upright "1" to a period, comma or colon ("1 .21",
    /// "1 :18"), the gap may be up to twice the threshold (a real space there is rare); from an
    /// upright "1" to anything else only slightly above it ("1 van" and "141 - RAF" have a
    /// full word space). Other italic gaps are left alone: the slant already closes them.
    /// <paramref name="pixelsAreSpace"/> is the threshold the line was split with.
    /// </summary>
    public static List<NOcrChar> RemoveFalseSpacesInNumbers(List<NOcrChar> matches, int pixelsAreSpace)
    {
        List<NOcrChar>? deleteItems = null;
        for (var i = 0; i < matches.Count - 1; i++)
        {
            var match = matches[i];
            if (match.Text.Length == 0 || !char.IsDigit(match.Text[match.Text.Length - 1]))
            {
                continue;
            }

            var nextMatch = matches[i + 1];
            var space = nextMatch.ImageSplitterItem;
            if (space == null || space.SpecialCharacter != " " || space.NikseBitmap != null)
            {
                continue;
            }

            var endsWithOne = match.Text.EndsWith('1');
            var following = i + 2 < matches.Count ? matches[i + 2].Text : string.Empty;
            var next = following.Length > 0 ? following[0] : '\0';
            int limit;
            if (char.IsDigit(next) && (endsWithOne || next == '1'))
            {
                limit = pixelsAreSpace * 2;
            }
            else if (match.Italic || !endsWithOne)
            {
                continue;
            }
            else if (next is '.' or ',' or ':')
            {
                limit = pixelsAreSpace * 2;
            }
            else
            {
                limit = pixelsAreSpace + Math.Max(1, (int)Math.Round(pixelsAreSpace * 0.3, MidpointRounding.AwayFromZero));
            }

            if (space.SpacePixels < limit)
            {
                deleteItems ??= new List<NOcrChar>();
                deleteItems.Add(nextMatch);
            }
        }

        if (deleteItems != null)
        {
            foreach (var deleteItem in deleteItems)
            {
                matches.Remove(deleteItem);
            }
        }

        return matches;
    }

    private double FindRelativeThreshold(List<ImageSplitterItem2> letters, List<(int Start, int End, int Height)> lines)
    {
        var gaps = new List<double>();
        foreach (var (start, end, height) in lines)
        {
            if (height <= 0)
            {
                continue;
            }

            for (var i = start; i < end; i++)
            {
                var item = letters[i];
                if (item.SpecialCharacter == " " && item.NikseBitmap == null)
                {
                    gaps.Add(item.SpacePixels / (double)height);
                }
            }
        }

        gaps.Sort();
        var bestRatio = 0.0;
        var bestThreshold = -1.0;
        for (var i = 0; i + 1 < gaps.Count; i++)
        {
            var low = gaps[i];
            var high = gaps[i + 1];
            if (high < MinSpace || low > MaxLetterGap || high - low < MinJumpSize)
            {
                continue;
            }

            var ratio = high / Math.Max(low, 0.02);
            if (ratio > bestRatio)
            {
                bestRatio = ratio;
                bestThreshold = (low + high) / 2;
            }
        }

        lock (_lock)
        {
            if (bestRatio >= MinJumpRatio)
            {
                if (_history.Count >= MaxHistory)
                {
                    _history.RemoveAt(0);
                }

                _history.Add(bestThreshold);
                return bestThreshold;
            }

            if (_history.Count > 0)
            {
                var sorted = _history.OrderBy(p => p).ToList();
                return sorted[sorted.Count / 2];
            }
        }

        return DefaultRelativeThreshold;
    }

    // Share of the extra width the glyph bodies leave that counts towards the gap.
    private const double BodyGapWeight = 0.25;

    // The same share once the box gap itself is most of a space already and a glyph overhangs it.
    private const double WideBodyGapWeight = 0.5;
    private const double WideBoxGapShare = 0.6;
    private const double MinOverhangShare = 0.1;

    /// <summary>
    /// Width of each gap item of one text line (indexed from <paramref name="start"/>), widened
    /// by part of the room between the glyph bodies on either side. Overhangs - the hook of
    /// "f", the tail of "y"/"j", the bar of "T" - reach into the space, so the gap between the
    /// boxes understates it ("ofyou", "ifwe", "wasjust"). Only a quarter of the extra counts,
    /// so a kerned pair inside a word ("To", "Ty", box gap near zero) stays a letter gap, and
    /// only between letter-sized glyphs. Once the box gap is already most of the threshold
    /// and a glyph really overhangs it - the arm of "f"/"r"/"T" over its own foot, the hook of
    /// "j" under its own stem (a bold "f je", "r jou", "t jij": box 0.65-0.8, bodies 1.2-1.7 of
    /// the threshold) - half of the extra counts; a kerned pair never gets that far on the
    /// boxes alone. The body gap of "e?" or "n?" is as wide (the hook of "?" is hollow), but
    /// neither glyph overhangs, so that pair keeps the quarter. The threshold itself still
    /// comes from the box gaps.
    /// </summary>
    private static double[] GetEffectiveGaps(List<ImageSplitterItem2> letters, int start, int end, int height, int threshold)
    {
        var gaps = new double[end - start];
        for (var i = start; i < end; i++)
        {
            var item = letters[i];
            if (item.SpecialCharacter != " " || item.NikseBitmap != null)
            {
                continue;
            }

            gaps[i - start] = item.SpacePixels;
            var before = i - 1;
            while (before >= start && letters[before].NikseBitmap == null)
            {
                before--;
            }

            var after = i + 1;
            while (after < end && letters[after].NikseBitmap == null)
            {
                after++;
            }

            // Letters on both sides: an apostrophe, quote or period shares few rows with its
            // neighbour, and a colon is two dots - their "body gap" says nothing about the space.
            if (before >= start && after < end &&
                IsLetterSized(letters[before].NikseBitmap!, height) &&
                IsLetterSized(letters[after].NikseBitmap!, height))
            {
                var body = BodyGap(letters[before], letters[after]);
                if (body > item.SpacePixels)
                {
                    var weight = BodyGapWeight;
                    if (item.SpacePixels >= threshold * WideBoxGapShare &&
                        RightOverhang(letters[before].NikseBitmap!) + LeftOverhang(letters[after].NikseBitmap!) >= Math.Max(2, height * MinOverhangShare))
                    {
                        weight = WideBodyGapWeight;
                    }

                    gaps[i - start] = item.SpacePixels + weight * (body - item.SpacePixels);
                }
            }
        }

        return gaps;
    }

    /// <summary>
    /// How far the glyph's box reaches right of its ink in the lower half of its rows: the arm
    /// of "f", "r" and "t", the bar of "T", the open top of "V" and "W". Zero for "n", "e", "o".
    /// </summary>
    private static int RightOverhang(NikseBitmap2 bitmap)
    {
        var right = -1;
        for (var y = bitmap.Height / 2; y < bitmap.Height; y++)
        {
            for (var x = bitmap.Width - 1; x > right; x--)
            {
                if (bitmap.GetAlpha(x, y) > 150)
                {
                    right = x;
                    break;
                }
            }
        }

        return right < 0 ? 0 : bitmap.Width - 1 - right;
    }

    /// <summary>
    /// How far the glyph's box reaches left of its ink in the upper half of its rows: the hook
    /// of "j" and "J" under the stem. Zero for "?", "a", "v" and the tail of "y" (its arms span
    /// the box).
    /// </summary>
    private static int LeftOverhang(NikseBitmap2 bitmap)
    {
        var left = int.MaxValue;
        for (var y = 0; y < bitmap.Height / 2; y++)
        {
            for (var x = 0; x < left && x < bitmap.Width; x++)
            {
                if (bitmap.GetAlpha(x, y) > 150)
                {
                    left = x;
                    break;
                }
            }
        }

        return left == int.MaxValue ? 0 : left;
    }

    /// <summary>At least 45% of the line height, with ink on at least three quarters of its rows.</summary>
    private static bool IsLetterSized(NikseBitmap2 bitmap, int lineHeight)
    {
        if (bitmap.Height * 100 < lineHeight * 45)
        {
            return false;
        }

        var inkRows = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetAlpha(x, y) > 150)
                {
                    inkRows++;
                    break;
                }
            }
        }

        return inkRows * 4 >= bitmap.Height * 3;
    }

    /// <summary>
    /// Closest horizontal distance between the ink of <paramref name="left"/> and
    /// <paramref name="right"/> on the rows where both have ink, or -1 when no row has both.
    /// </summary>
    private static int BodyGap(ImageSplitterItem2 left, ImageSplitterItem2 right)
    {
        var a = left.NikseBitmap!;
        var b = right.NikseBitmap!;
        var top = Math.Max(left.Top, right.Top);
        var bottom = Math.Min(left.Top + a.Height, right.Top + b.Height);
        var best = int.MaxValue;
        for (var y = top; y < bottom; y++)
        {
            var rightEdge = -1;
            for (var x = a.Width - 1; x >= 0; x--)
            {
                if (a.GetAlpha(x, y - left.Top) > 150)
                {
                    rightEdge = left.X + x;
                    break;
                }
            }

            if (rightEdge < 0)
            {
                continue;
            }

            for (var x = 0; x < b.Width; x++)
            {
                if (b.GetAlpha(x, y - right.Top) > 150)
                {
                    best = Math.Min(best, right.X + x - rightEdge - 1);
                    break;
                }
            }
        }

        return best == int.MaxValue ? -1 : best;
    }

    /// <summary>Item ranges per text line (end exclusive, at the line break) with the line's ink height.</summary>
    private static List<(int Start, int End, int Height)> GetLines(List<ImageSplitterItem2> letters)
    {
        var lines = new List<(int, int, int)>();
        var start = 0;
        while (start <= letters.Count)
        {
            var end = start;
            var top = int.MaxValue;
            var bottom = int.MinValue;
            while (end < letters.Count && letters[end].SpecialCharacter != Environment.NewLine)
            {
                var bitmap = letters[end].NikseBitmap;
                if (bitmap != null)
                {
                    top = Math.Min(top, letters[end].Top);
                    bottom = Math.Max(bottom, letters[end].Top + bitmap.Height);
                }

                end++;
            }

            lines.Add((start, end, bottom > top ? bottom - top : 0));
            if (end >= letters.Count)
            {
                break;
            }

            start = end + 1;
        }

        return lines;
    }
}
