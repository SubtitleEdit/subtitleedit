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

            for (var i = start; i < end; i++)
            {
                var item = letters[i];
                if (item.SpecialCharacter == " " && item.NikseBitmap == null && item.SpacePixels < threshold)
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
