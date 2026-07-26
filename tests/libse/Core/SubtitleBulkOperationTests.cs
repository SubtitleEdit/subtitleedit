using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// AdjustDisplayTimeUsingPercent and SetFixedDuration probe a set of selected indices rather than
/// the caller's list, and RemoveEmptyLines compacts in one pass. These pin the behaviour that
/// makes those safe: only selected lines move, the ascending walk still sees the neighbour it just
/// adjusted, indices outside the subtitle are ignored, and renumbering survives.
/// </summary>
public class SubtitleBulkOperationTests
{
    private static Subtitle Make(params (double startMs, double endMs)[] times)
    {
        var s = new Subtitle();
        var number = 1;
        foreach (var (startMs, endMs) in times)
        {
            s.Paragraphs.Add(new Paragraph("Line " + number, startMs, endMs) { Number = number });
            number++;
        }

        return s;
    }

    [Fact]
    public void AdjustDisplayTimeUsingPercent_OnlyTouchesSelectedLines()
    {
        var s = Make((0, 1000), (5000, 6000), (10_000, 11_000));

        s.AdjustDisplayTimeUsingPercent(200, new List<int> { 1 }, enforceDurationLimits: false);

        Assert.Equal(1000, s.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(7000, s.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal(11_000, s.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void AdjustDisplayTimeUsingPercent_IgnoresIndicesOutsideTheSubtitle()
    {
        var s = Make((0, 1000), (5000, 6000));

        s.AdjustDisplayTimeUsingPercent(200, new List<int> { -5, 0, 99 }, enforceDurationLimits: false);

        Assert.Equal(2000, s.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(6000, s.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void AdjustDisplayTimeUsingPercent_AcceptsAnUnsortedSelection()
    {
        var ascending = Make((0, 1000), (5000, 6000), (10_000, 11_000));
        var descending = Make((0, 1000), (5000, 6000), (10_000, 11_000));

        ascending.AdjustDisplayTimeUsingPercent(150, new List<int> { 0, 1, 2 }, enforceDurationLimits: false);
        descending.AdjustDisplayTimeUsingPercent(150, new List<int> { 2, 1, 0 }, enforceDurationLimits: false);

        for (var i = 0; i < ascending.Paragraphs.Count; i++)
        {
            Assert.Equal(ascending.Paragraphs[i].EndTime.TotalMilliseconds, descending.Paragraphs[i].EndTime.TotalMilliseconds);
        }
    }

    [Fact]
    public void SetFixedDuration_OnlyTouchesSelectedLines()
    {
        var s = Make((0, 1000), (5000, 6000), (10_000, 11_000));

        s.SetFixedDuration(new List<int> { 2 }, 2500);

        Assert.Equal(1000, s.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(6000, s.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal(12_500, s.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SetFixedDuration_NullSelectionMeansEveryLine()
    {
        var s = Make((0, 1000), (5000, 6000));

        s.SetFixedDuration(null, 2000);

        Assert.Equal(2000, s.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(7000, s.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void RemoveEmptyLines_RemovesBlankAndControlCharacterLinesAndRenumbers()
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("first", 0, 1000) { Number = 5 });
        s.Paragraphs.Add(new Paragraph("   ", 2000, 3000) { Number = 6 });
        s.Paragraphs.Add(new Paragraph(string.Empty, 4000, 5000) { Number = 7 });
        s.Paragraphs.Add(new Paragraph("\t\r\n", 6000, 7000) { Number = 8 });
        s.Paragraphs.Add(new Paragraph("last", 8000, 9000) { Number = 9 });

        var removed = s.RemoveEmptyLines();

        Assert.Equal(3, removed);
        Assert.Equal(2, s.Paragraphs.Count);
        Assert.Equal("first", s.Paragraphs[0].Text);
        Assert.Equal("last", s.Paragraphs[1].Text);

        // Renumbered from the first paragraph's original number.
        Assert.Equal(5, s.Paragraphs[0].Number);
        Assert.Equal(6, s.Paragraphs[1].Number);
    }

    [Fact]
    public void RemoveEmptyLines_LeavesANonEmptySubtitleAlone()
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("a", 0, 1000) { Number = 3 });
        s.Paragraphs.Add(new Paragraph("b", 2000, 3000) { Number = 4 });

        Assert.Equal(0, s.RemoveEmptyLines());
        Assert.Equal(2, s.Paragraphs.Count);
        Assert.Equal(3, s.Paragraphs[0].Number); // no renumber when nothing was removed
    }
}
