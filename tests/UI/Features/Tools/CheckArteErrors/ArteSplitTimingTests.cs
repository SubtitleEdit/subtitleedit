using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

namespace UITests.Features.Tools.CheckArteErrors;

public class ArteSplitTimingTests
{
    [Fact]
    public void Fit_PreservesOuterFramesGapAndMinimumDuration()
    {
        var parts = new[] { new Paragraph("Short", 0, 0), new Paragraph(new string('x', 40), 0, 0) };
        Assert.True(ArteSplitTiming.TryFit(parts, 1000, 5000, 1000, 7000, 20, out var error), error);
        Assert.Equal(1000, parts[0].StartTime.TotalMilliseconds);
        Assert.Equal(5000, parts[1].EndTime.TotalMilliseconds);
        Assert.Equal(200, parts[1].StartTime.TotalMilliseconds - parts[0].EndTime.TotalMilliseconds);
        Assert.True(parts[0].Duration.TotalMilliseconds >= 1000);
        Assert.True(parts[1].Duration.TotalMilliseconds >= 2000);
        Assert.All(parts, p => Assert.Equal(0, p.EndTime.TotalMilliseconds % 40));
    }

    [Theory]
    [InlineData(1000, 3000)]
    [InlineData(1001, 3001)]
    public void TightTime_FitsAndReportsWarning(double start, double end)
    {
        var parts = new[] { new Paragraph("Short", 0, 0), new Paragraph(new string('x', 40), 0, 0) };
        Assert.True(ArteSplitTiming.TryFit(parts, start, end, 1000, 7000, 20, out var note));
        Assert.Contains("Zeitlich knappe Darstellung", note);
        Assert.Equal(1000, parts[0].StartTime.TotalMilliseconds);
        Assert.Equal(3000, parts[1].EndTime.TotalMilliseconds);
        Assert.Equal(200, parts[1].StartTime.TotalMilliseconds - parts[0].EndTime.TotalMilliseconds);
        Assert.All(parts, p => Assert.True(p.Duration.TotalMilliseconds >= 40));
    }

    [Theory]
    [InlineData(1000, 1200, 1000, 7000, 20)] // no room after five-frame gap
    public void CannotFit_DoesNotChangeTimes(double start, double end, double minimum, double maximum, double cps)
    {
        var parts = new[] { new Paragraph("Short", 200, 400), new Paragraph(new string('x', 40), 600, 800) };
        Assert.False(ArteSplitTiming.TryFit(parts, start, end, minimum, maximum, cps, out var error));
        Assert.NotEmpty(error);
        Assert.Equal(200, parts[0].StartTime.TotalMilliseconds);
        Assert.Equal(800, parts[1].EndTime.TotalMilliseconds);
    }
}
