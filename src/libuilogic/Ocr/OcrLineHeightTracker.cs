namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Tracks the average glyph height across an OCR run so the line splitter can be given an
/// adaptive minimum line height (SE 4's GetMinLineHeight/UpdateLineHeights). A hardcoded
/// value is simultaneously too large for DVD-sized fonts (a whole line fits under it, so
/// two-line subtitles merge into one) and too small for 4K-sized fonts (the accent band on
/// top of a line exceeds it and splits off as its own bogus line).
/// </summary>
public class OcrLineHeightTracker
{
    // Enough samples to be stable; matches SE 4's cap so one long movie doesn't keep summing.
    private const int MaxSamples = 1000;

    private readonly Lock _lock = new();
    private long _total;
    private int _count;

    /// <summary>
    /// Minimum line height used until enough letters have been seen:
    /// 25 for Blu-ray sized sources, 12 otherwise (SE 4's values).
    /// </summary>
    public int FallbackMinLineHeight { get; set; } = 12;

    public void Update(List<ImageSplitterItem2> letters)
    {
        lock (_lock)
        {
            if (_count >= MaxSamples)
            {
                return;
            }

            foreach (var letter in letters)
            {
                if (letter.NikseBitmap != null)
                {
                    _total += letter.NikseBitmap.Height;
                    _count++;
                }
            }
        }
    }

    public int GetMinLineHeight()
    {
        lock (_lock)
        {
            if (_count > 20)
            {
                return (int)Math.Round(_total / (double)_count * 0.9);
            }

            return FallbackMinLineHeight;
        }
    }

    /// <summary>
    /// Average glyph height, or -1 until enough letters have been seen. Passing this to the
    /// splitter enables its aggressive rescue for lines whose glyphs touch across the gap.
    /// </summary>
    public double GetAverageLineHeight()
    {
        lock (_lock)
        {
            return _count > 20 ? _total / (double)_count : -1;
        }
    }
}
