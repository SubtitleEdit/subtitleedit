using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.MergeContinuationLines;

namespace LibUiLogicTests.MergeContinuationLines;

public class MergeContinuationLinesUtilsTests
{
    private static Subtitle MakeSubtitle(params (string Text, double StartMs, double EndMs)[] lines)
    {
        var subtitle = new Subtitle();
        foreach (var (text, startMs, endMs) in lines)
        {
            subtitle.Paragraphs.Add(new Paragraph(text, startMs, endMs));
        }

        subtitle.Renumber();
        return subtitle;
    }

    [Fact]
    public void MergeContinuationLinesInSubtitle_RunOfContinuationLines_MergedIntoOne()
    {
        var subtitle = MakeSubtitle(
            ("an", 1000, 1100),
            ("illusion,", 1120, 1300),
            ("an", 1320, 1400),
            ("echo.", 1420, 1600));

        var merged = MergeContinuationLinesUtils.MergeContinuationLinesInSubtitle(subtitle, "en", 500, 200);

        var p = Assert.Single(merged.Paragraphs);
        Assert.Equal("an illusion, an echo.", p.Text);
        Assert.Equal(1000, p.StartTime.TotalMilliseconds);
        Assert.Equal(1600, p.EndTime.TotalMilliseconds);
    }

    [Fact]
    public void MergeContinuationLinesInSubtitle_SentenceEndings_NotMerged()
    {
        var subtitle = MakeSubtitle(
            ("Hello there.", 1000, 2000),
            ("How are you?", 2100, 3000));

        var merged = MergeContinuationLinesUtils.MergeContinuationLinesInSubtitle(subtitle, "en", 500, 200);

        Assert.Equal(new[] { "Hello there.", "How are you?" }, merged.Paragraphs.Select(p => p.Text));
    }

    [Theory]
    [InlineData(400, 500, 1)]
    [InlineData(500, 500, 2)] // the gap must be below the limit
    public void MergeContinuationLinesInSubtitle_RespectsMaxGap(double gapMs, int maxGapMs, int expectedCount)
    {
        var subtitle = MakeSubtitle(
            ("I was going", 1000, 2000),
            ("home.", 2000 + gapMs, 3000 + gapMs));

        var merged = MergeContinuationLinesUtils.MergeContinuationLinesInSubtitle(subtitle, "en", maxGapMs, 200);

        Assert.Equal(expectedCount, merged.Paragraphs.Count);
    }

    [Fact]
    public void MergeContinuationLinesInSubtitle_RespectsMaxTotalLength()
    {
        // "I was going" + "home." is 16 characters; the total must be below the limit.
        var subtitle = MakeSubtitle(
            ("I was going", 1000, 2000),
            ("home.", 2100, 3000));

        Assert.Equal(2, MergeContinuationLinesUtils.MergeContinuationLinesInSubtitle(subtitle, "en", 500, 16).Paragraphs.Count);
        Assert.Single(MergeContinuationLinesUtils.MergeContinuationLinesInSubtitle(subtitle, "en", 500, 17).Paragraphs);
    }

    [Fact]
    public void MergeContinuationLinesInSubtitle_DialogLines_NotMerged()
    {
        var subtitle = MakeSubtitle(
            ("- Are you", 1000, 2000),
            ("- Yes.", 2100, 3000));

        var merged = MergeContinuationLinesUtils.MergeContinuationLinesInSubtitle(subtitle, "en", 500, 200);

        Assert.Equal(2, merged.Paragraphs.Count);
    }

    [Fact]
    public void MergeContinuationLinesInSubtitle_KeepsFirstParagraphProperties_AndLeavesInputUnchanged()
    {
        var subtitle = MakeSubtitle(
            ("I was going", 1000, 2000),
            ("home.", 2100, 3000));
        subtitle.Paragraphs[0].Extra = "Signs";
        subtitle.Paragraphs[0].Actor = "Bob";

        var merged = MergeContinuationLinesUtils.MergeContinuationLinesInSubtitle(subtitle, "en", 500, 200);

        var p = Assert.Single(merged.Paragraphs);
        Assert.Equal("Signs", p.Extra);
        Assert.Equal("Bob", p.Actor);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("I was going", subtitle.Paragraphs[0].Text);
        Assert.Equal(2000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }
}
