using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

public class SpeechToTextTimingFixerTests
{
    private static Subtitle MakeSubtitle(params (string Text, double StartMs, double EndMs)[] paragraphs)
    {
        var subtitle = new Subtitle();
        foreach (var p in paragraphs)
        {
            subtitle.Paragraphs.Add(new Paragraph(p.Text, p.StartMs, p.EndMs));
        }

        return subtitle;
    }

    [Fact]
    public void SortAndRemoveOverlaps_AlreadyInOrder_LeavesTimingsAlone()
    {
        var subtitle = MakeSubtitle(
            ("one", 1000, 2000),
            ("two", 2000, 3000),
            ("three", 4000, 5000));

        var result = SpeechToTextTimingFixer.SortAndRemoveOverlaps(subtitle);

        Assert.Equal(new[] { "one", "two", "three" }, result.Paragraphs.Select(p => p.Text));
        Assert.Equal(new double[] { 1000, 2000, 4000 }, result.Paragraphs.Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(new double[] { 2000, 3000, 5000 }, result.Paragraphs.Select(p => p.EndTime.TotalMilliseconds));
    }

    [Fact]
    public void SortAndRemoveOverlaps_OutOfOrderParagraph_IsMovedBackIntoPlace()
    {
        // Crisp ASR + Parakeet emits a segment that belongs seconds earlier
        // (issue #13548).
        var subtitle = MakeSubtitle(
            ("late", 42980, 43300),
            ("early", 33180, 34540));

        var result = SpeechToTextTimingFixer.SortAndRemoveOverlaps(subtitle);

        Assert.Equal(new[] { "early", "late" }, result.Paragraphs.Select(p => p.Text));
        Assert.Equal(33180, result.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(42980, result.Paragraphs[1].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void SortAndRemoveOverlaps_OverlappingNeighbor_TruncatesTheEarlierParagraph()
    {
        var subtitle = MakeSubtitle(
            ("first", 32659, 43300),
            ("second", 33180, 35541));

        var result = SpeechToTextTimingFixer.SortAndRemoveOverlaps(subtitle);

        Assert.Equal(33180, result.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(33180, result.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(35541, result.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SortAndRemoveOverlaps_SharedStartTime_MovesTheLongerParagraphAndKeepsItsDuration()
    {
        // Truncating would wipe out the shorter paragraph, so the longer one is
        // pushed past it instead.
        var subtitle = MakeSubtitle(
            ("long", 5000, 6000),
            ("short", 5000, 5500));

        var result = SpeechToTextTimingFixer.SortAndRemoveOverlaps(subtitle);

        Assert.Equal(new[] { "short", "long" }, result.Paragraphs.Select(p => p.Text));
        Assert.Equal(5000, result.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(5500, result.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(5500, result.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6500, result.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SortAndRemoveOverlaps_ChainOfOverlaps_LeavesNothingOverlapping()
    {
        var subtitle = MakeSubtitle(
            ("a", 1000, 9000),
            ("b", 2000, 8000),
            ("c", 3000, 4000),
            ("d", 3500, 12000));

        var result = SpeechToTextTimingFixer.SortAndRemoveOverlaps(subtitle);

        for (var i = 1; i < result.Paragraphs.Count; i++)
        {
            Assert.True(
                result.Paragraphs[i].StartTime.TotalMilliseconds >= result.Paragraphs[i - 1].EndTime.TotalMilliseconds,
                $"Paragraph {i} overlaps its predecessor");
        }
    }

    [Fact]
    public void SortAndRemoveOverlaps_SingleParagraph_IsUnchanged()
    {
        var subtitle = MakeSubtitle(("only", 1000, 2000));

        var result = SpeechToTextTimingFixer.SortAndRemoveOverlaps(subtitle);

        Assert.Single(result.Paragraphs);
        Assert.Equal(1000, result.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(2000, result.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SortAndRemoveOverlaps_DoesNotMutateTheInput()
    {
        var subtitle = MakeSubtitle(
            ("first", 32659, 43300),
            ("second", 33180, 35541));

        SpeechToTextTimingFixer.SortAndRemoveOverlaps(subtitle);

        Assert.Equal(43300, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }
}
