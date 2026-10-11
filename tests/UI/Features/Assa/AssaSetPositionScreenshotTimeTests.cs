using Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Assa;

/// <summary>
/// Issue #15817: "Set position" (and Image color picker) always grabbed the background frame at the
/// line start; a player paused inside the line should be used instead, like SE4.
/// </summary>
public class AssaSetPositionScreenshotTimeTests
{
    private static SubtitleLineViewModel MakeLine() => new()
    {
        StartTime = System.TimeSpan.FromSeconds(10),
        EndTime = System.TimeSpan.FromSeconds(14),
    };

    [Theory]
    [InlineData(12.5, 12.5)]
    [InlineData(10.0, 10.0)]
    [InlineData(14.0, 14.0)]
    [InlineData(9.99, 10.0)]
    [InlineData(14.01, 10.0)]
    [InlineData(0.0, 10.0)]
    public void UsesPlayerPositionOnlyInsideLine(double position, double expected)
    {
        Assert.Equal(expected, AssaSetPositionViewModel.GetScreenshotSeconds(MakeLine(), position), 3);
    }

    [Fact]
    public void NoPlayerPositionFallsBackToLineStart()
    {
        Assert.Equal(10.0, AssaSetPositionViewModel.GetScreenshotSeconds(MakeLine(), null), 3);
    }
}
