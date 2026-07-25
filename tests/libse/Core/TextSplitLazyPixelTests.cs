using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// TextSplitResult measures its pixel widths on first read rather than in the constructor, and
/// TextSplit puts one shared instance in both of its candidate lists. These pin the observable
/// behaviour that has to survive both: the measured values, the shape of the list when a line is
/// longer than the pixel-measuring cutoff, and the resulting line breaks.
/// </summary>
public class TextSplitLazyPixelTests : IDisposable
{
    private readonly bool _originalUsePixelWidth = Configuration.Settings.Tools.AutoBreakUsePixelWidth;

    public void Dispose()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = _originalUsePixelWidth;
    }

    private static TextSplitResult Make(string line1, string line2)
        => new TextSplitResult(new List<string> { line1, line2 });

    [Fact]
    public void LengthPixels_IsMeasuredOnFirstRead()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;
        var result = Make("Hello there", "General Kenobi");

        Assert.Equal(2, result.LengthPixels.Count);
        Assert.All(result.LengthPixels, p => Assert.True(p > 0));

        // Repeated reads are stable (measured once, not re-measured).
        var first = result.LengthPixels;
        Assert.Same(first, result.LengthPixels);
    }

    [Fact]
    public void LengthPixels_IsEmptyWhenPixelWidthIsOff()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = false;
        var result = Make("Hello there", "General Kenobi");

        Assert.Empty(result.LengthPixels);
    }

    [Fact]
    public void LengthCharacters_IsAvailableWithoutMeasuring()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;
        var result = Make("abc", "defgh");

        Assert.Equal(new List<int> { 3, 5 }, result.LengthCharacters);
    }

    /// <summary>
    /// A line over the 1000 character cutoff contributes no entry to LengthPixels - long-standing
    /// behaviour that callers like IsBottomHeavy depend on the shape of.
    /// </summary>
    [Fact]
    public void LengthPixels_SkipsLinesOverTheCutoff()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;
        var result = Make(new string('x', 1200), "short");

        Assert.Single(result.LengthPixels);
    }

    [Fact]
    public void DiffFromAverage_NeedsNoPixelMeasurement()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = false;
        var balanced = Make("abcde", "fghij");
        var lopsided = Make("a", "bcdefghij");

        Assert.True(balanced.DiffFromAverage() < lopsided.DiffFromAverage());
    }

    [Theory]
    [InlineData("It was the best of times, it was the worst of times, it was the age of wisdom.")]
    [InlineData("- Are you coming with us? - No, I think I will stay right here and wait.")]
    [InlineData("Mr. Smith went to Washington and then he went home again to see his family.")]
    public void AutoBreak_ProducesTwoBalancedLines(string text)
    {
        var split = new TextSplit(text, 43, "en");
        var result = split.AutoBreak(true, true, true, true);

        Assert.NotNull(result);
        var lines = result.SplitToLines();
        Assert.Equal(2, lines.Count);
        Assert.Equal(text.Replace(" ", string.Empty), result.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty));
    }

    [Fact]
    public void AutoBreak_FallsBackToLengthSplitWhenNothingCanBreak()
    {
        // No spaces at all: no candidates, so AutoBreak has nothing to return.
        var split = new TextSplit("abcdefghij", 5, "en");
        Assert.Null(split.AutoBreak(true, true, true, true));
    }
}
