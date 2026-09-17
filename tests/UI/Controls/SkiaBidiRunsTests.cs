using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using System.Linq;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The Skia waveform renderer shapes text with HarfBuzz, which does no bidi reordering: a right to
/// left line with Latin words or digits has to be split into directional runs first. These pin the
/// visual order those runs come back in.
/// </summary>
public class SkiaBidiRunsTests
{
    [Fact]
    public void HebrewLineWithNumberAndLatinWordIsSplitIntoRunsInVisualOrder()
    {
        // Logical: "שלום 12 abc" read right to left is: שלום, then 12, then abc - so drawn left to
        // right the Latin word comes first and the Hebrew last, with the digits still reading "12".
        var runs = SkiaBidiRuns.Split("שלום 12 abc", rightToLeftParagraph: true);

        Assert.Equal(new[] { "abc", " ", "12", "שלום " }, runs.Select(r => r.Text).ToArray());
        Assert.Equal(new[] { false, true, false, true }, runs.Select(r => r.RightToLeft).ToArray());
    }

    [Fact]
    public void ArabicLineWithDigitsKeepsTheDigitsLeftToRight()
    {
        var runs = SkiaBidiRuns.Split("مرحبا 12", rightToLeftParagraph: true);

        Assert.Equal(new[] { "12", "مرحبا " }, runs.Select(r => r.Text).ToArray());
        Assert.False(runs[0].RightToLeft);
        Assert.True(runs[1].RightToLeft);
    }

    [Fact]
    public void NumberSeparatorsStayInsideTheNumber()
    {
        var runs = SkiaBidiRuns.Split("שלום 12:30", rightToLeftParagraph: true);

        Assert.Equal(new[] { "12:30", "שלום " }, runs.Select(r => r.Text).ToArray());
    }

    [Fact]
    public void LatinWordFollowedByDigitsInRightToLeftParagraphStaysOneRun()
    {
        // W7: a number after Latin text belongs to it - "abc 12" must not turn into "12 abc".
        var runs = SkiaBidiRuns.Split("abc 12", rightToLeftParagraph: true);

        Assert.Single(runs);
        Assert.Equal("abc 12", runs[0].Text);
        Assert.False(runs[0].RightToLeft);
    }

    [Fact]
    public void TrailingPunctuationInRightToLeftParagraphTakesTheParagraphDirection()
    {
        var runs = SkiaBidiRuns.Split("שלום!", rightToLeftParagraph: true);

        Assert.Single(runs);
        Assert.Equal("שלום!", runs[0].Text);
        Assert.True(runs[0].RightToLeft);
    }

    [Fact]
    public void HebrewWordInsideLeftToRightParagraphIsOneRightToLeftRunInPlace()
    {
        var runs = SkiaBidiRuns.Split("say שלום now", rightToLeftParagraph: false);

        Assert.Equal(new[] { "say ", "שלום", " now" }, runs.Select(r => r.Text).ToArray());
        Assert.Equal(new[] { false, true, false }, runs.Select(r => r.RightToLeft).ToArray());
    }

    [Fact]
    public void PureLeftToRightTextIsOneRun()
    {
        var runs = SkiaBidiRuns.Split("Hello there, 42 times!", rightToLeftParagraph: false);

        Assert.Single(runs);
        Assert.Equal("Hello there, 42 times!", runs[0].Text);
        Assert.False(runs[0].RightToLeft);
        Assert.False(SkiaBidiRuns.HasStrongRightToLeft("Hello there, 42 times!"));
    }

    [Fact]
    public void RunTextsAlwaysCoverTheWholeLine()
    {
        foreach (var text in new[] { "שלום 12 abc", "abc שלום 12", "a", "1", "שׁ", "12 שלום, 3.5 ok", "ab-cd עב" })
        {
            foreach (var rtl in new[] { true, false })
            {
                var runs = SkiaBidiRuns.Split(text, rtl);
                var joined = string.Concat(rtl ? runs.AsEnumerable().Reverse().Select(r => r.Text) : runs.Select(r => r.Text));
                Assert.Equal(text, joined);
            }
        }
    }
}
