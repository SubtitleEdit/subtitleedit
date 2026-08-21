using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The line splitter's minimum line height was hardcoded to 20, which is simultaneously too
/// large for DVD-sized fonts (a whole line fits under it, so two-line subtitles merged into a
/// handful of unreadable blobs) and too small for 4K-sized fonts (the accent band split off as
/// a bogus extra line). The tracker restores SE 4's adaptive behavior.
/// </summary>
public class OcrLineHeightTrackerTests
{
    private static ImageSplitterItem2 Letter(int height)
    {
        return new ImageSplitterItem2(0, 0, new NikseBitmap2(8, height));
    }

    [Fact]
    public void BeforeAdaptation_UsesFallback()
    {
        var tracker = new OcrLineHeightTracker { FallbackMinLineHeight = 25 };

        Assert.Equal(25, tracker.GetMinLineHeight());
        Assert.Equal(-1, tracker.GetAverageLineHeight());
    }

    [Fact]
    public void NeedsMoreThanTwentyLetters_ToAdapt()
    {
        var tracker = new OcrLineHeightTracker();
        tracker.Update(Enumerable.Range(0, 20).Select(_ => Letter(40)).ToList());

        Assert.Equal(12, tracker.GetMinLineHeight()); // still the fallback
        Assert.Equal(-1, tracker.GetAverageLineHeight());
    }

    [Fact]
    public void AfterAdaptation_ReturnsNinetyPercentOfAverage()
    {
        var tracker = new OcrLineHeightTracker();
        tracker.Update(Enumerable.Range(0, 30).Select(_ => Letter(40)).ToList());

        Assert.Equal(36, tracker.GetMinLineHeight()); // 40 * 0.9
        Assert.Equal(40, tracker.GetAverageLineHeight());
    }

    [Fact]
    public void SpacesAndNewlines_DoNotCountAsLetters()
    {
        var tracker = new OcrLineHeightTracker();
        var letters = new List<ImageSplitterItem2> { new(" "), new(Environment.NewLine) };
        for (var i = 0; i < 25; i++)
        {
            letters.Add(Letter(30));
        }

        tracker.Update(letters);

        Assert.Equal(27, tracker.GetMinLineHeight()); // 30 * 0.9, specials ignored
    }
}
