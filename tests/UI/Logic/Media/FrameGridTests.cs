using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// #17149: frame snapping must count from the video's first frame, which sits at the file's start
/// time on the player's timeline - not from 0.
/// </summary>
public class FrameGridTests
{
    private const double FrameDuration = 0.04; // 25 fps

    [Theory]
    [InlineData(24.29, 0.02, 24.30)]
    [InlineData(24.31, 0.02, 24.30)]
    [InlineData(21.42, 0.02, 21.42)]
    [InlineData(21.42, 0.0, 21.44)]
    [InlineData(0.0, 0.02, 0.02)]
    public void Snap_UsesGridStartingAtOffset(double seconds, double offset, double expected)
    {
        Assert.Equal(expected, FrameGrid.Snap(seconds, FrameDuration, offset), 6);
    }

    [Fact]
    public void Snap_WorksInMilliseconds()
    {
        Assert.Equal(24_300, FrameGrid.Snap(24_290, 40, 20), 6);
    }

    [Fact]
    public void Snap_NeverReturnsNegativeTime()
    {
        Assert.True(FrameGrid.Snap(0.0, FrameDuration, 0.0) >= 0);
    }

    [Theory]
    [InlineData(0.02, 0.02)]
    [InlineData(0.0, 0.0)]
    [InlineData(-0.5, 0.0)]
    [InlineData(double.NaN, 0.0)]
    [InlineData(double.PositiveInfinity, 0.0)]
    public void NormalizeOffset_KeepsOnlyPositiveFiniteOffsets(double offset, double expected)
    {
        Assert.Equal(expected, FrameGrid.NormalizeOffset(offset));
    }
}
