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
    /// A line over the 1000 character cutoff is estimated rather than measured, but it still
    /// contributes an entry - IsBottomHeavy and DiffFromAveragePixelBottomHeavy index both slots.
    /// </summary>
    [Fact]
    public void LengthPixels_EstimatesLinesOverTheCutoff()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;
        var result = Make(new string('x', 1200), "short");

        Assert.Equal(2, result.LengthPixels.Count);
        Assert.Equal(1200 * 7, result.LengthPixels[0]);
        Assert.True(result.LengthPixels[1] > 0);

        // Both slots present means these no longer throw.
        Assert.False(result.IsBottomHeavy);
        Assert.True(result.DiffFromAveragePixelBottomHeavy() > 0);
    }

    /// <summary>
    /// SpaceLengthPixels is a process-wide static holding the width of one space, and
    /// TotalLengthPixels subtracts it. An over-long line used to overwrite it with a line
    /// length, skewing every later line break in the process.
    /// </summary>
    [Fact]
    public void SpaceLengthPixels_IsNotClobberedByAnOverLongLine()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;

        // Force it to be initialised from a normal split first.
        _ = Make("hello", "world").LengthPixels;
        var spaceWidth = TextSplitResult.SpaceLengthPixels;
        Assert.True(spaceWidth > 0 && spaceWidth < 100, $"expected a space width, got {spaceWidth}");

        _ = Make(new string('x', 1200), new string('y', 1500)).LengthPixels;

        Assert.Equal(spaceWidth, TextSplitResult.SpaceLengthPixels);
    }

    [Fact]
    public void AutoBreak_AfterAnOverLongLine_IsUnaffected()
    {
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = true;
        const string text = "It was the best of times, it was the worst of times, it was the age of wisdom.";

        var before = new TextSplit(text, 43, "en").AutoBreak(true, true, true, true);

        // Previously this poisoned SpaceLengthPixels for the rest of the process.
        _ = new TextSplit(new string('x', 1200) + " " + new string('y', 1200), 43, "en")
            .AutoBreak(true, true, true, true);

        var after = new TextSplit(text, 43, "en").AutoBreak(true, true, true, true);

        Assert.Equal(before, after);
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
