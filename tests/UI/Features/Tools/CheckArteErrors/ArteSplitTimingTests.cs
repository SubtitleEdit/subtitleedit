using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

namespace UITests.Features.Tools.CheckArteErrors;

public class ArteSplitTimingTests
{
    [Fact]
    public void Fit_PreservesOuterFramesGapAndMinimumDuration()
    {
        var parts = new[] { new Paragraph("Short", 0, 0), new Paragraph(new string('x', 40), 0, 0) };
        Assert.True(ArteSplitTiming.TryFit(parts, 1000, 5000, 1000, 7000, 20, 5, 15, false, 18, out var error), error);
        Assert.Equal(1000, parts[0].StartTime.TotalMilliseconds);
        Assert.Equal(5000, parts[1].EndTime.TotalMilliseconds);
        Assert.Equal(200, parts[1].StartTime.TotalMilliseconds - parts[0].EndTime.TotalMilliseconds);
        Assert.True(parts[0].Duration.TotalMilliseconds >= 1000);
        Assert.True(parts[1].Duration.TotalMilliseconds >= 2000);
    }

    [Fact]
    public void TightTime_UsesToleranceFloorInsteadOfOneFrameFallback()
    {
        var parts = new[] { new Paragraph("Short", 0, 0), new Paragraph(new string('x', 40), 0, 0) };
        Assert.True(ArteSplitTiming.TryFit(parts, 1000, 3880, 1000, 7000, 20, 5, 15, false, 18, out var note), note);
        Assert.Contains("Zeitlich knappe Darstellung", note);
        Assert.Equal(200, parts[1].StartTime.TotalMilliseconds - parts[0].EndTime.TotalMilliseconds);
        Assert.True(parts[0].Duration.TotalMilliseconds >= 840);
        Assert.True(parts[1].Duration.TotalMilliseconds >= 1680);
    }

    [Fact]
    public void AcceptShort_UsesConfiguredShortMinimum()
    {
        var parts = new[] { new Paragraph("One", 0, 0), new Paragraph("Two", 0, 0) };
        Assert.True(ArteSplitTiming.TryFit(parts, 1000, 2640, 1000, 7000, 20, 5, 15, true, 18, out var note), note);
        Assert.Equal(200, parts[1].StartTime.TotalMilliseconds - parts[0].EndTime.TotalMilliseconds);
        Assert.All(parts, p => Assert.True(p.Duration.TotalMilliseconds >= 720));
    }

    [Fact]
    public void AcceptShort_RejectsRangeBelowConfiguredShortMinimum()
    {
        var parts = new[] { new Paragraph("One", 200, 400), new Paragraph("Two", 600, 800) };
        Assert.False(ArteSplitTiming.TryFit(parts, 1000, 2600, 1000, 7000, 20, 5, 15, true, 18, out var error));
        Assert.NotEmpty(error);
        Assert.Equal(200, parts[0].StartTime.TotalMilliseconds);
        Assert.Equal(800, parts[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void CannotFit_DoesNotChangeTimes()
    {
        var parts = new[] { new Paragraph("Short", 200, 400), new Paragraph(new string('x', 40), 600, 800) };
        Assert.False(ArteSplitTiming.TryFit(parts, 1000, 1200, 1000, 7000, 20, 5, 15, false, 18, out var error));
        Assert.NotEmpty(error);
        Assert.Equal(200, parts[0].StartTime.TotalMilliseconds);
        Assert.Equal(800, parts[1].EndTime.TotalMilliseconds);
    }
}
