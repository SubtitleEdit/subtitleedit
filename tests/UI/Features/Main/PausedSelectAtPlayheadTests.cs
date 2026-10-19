using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

/// <summary>
/// Issue #16048: seek / frame / millisecond / second step commands did not select the line under
/// the play-head while paused, because paused selection also required the waveform "center" and
/// "center also when paused" options. Only the two select options gate it now.
/// </summary>
public class PausedSelectAtPlayheadTests
{
    [Fact]
    public void SelectsWhenBothSelectOptionsOnAndPositionMoved()
    {
        Assert.True(MainViewModel.ShouldSelectAtPlayheadWhilePaused(true, true, 10.040, 10.0));
    }

    [Fact]
    public void SelectsOnFirstPausedTick()
    {
        Assert.True(MainViewModel.ShouldSelectAtPlayheadWhilePaused(true, true, 0.0, -1));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void DoesNotSelectWhenASelectOptionIsOff(bool selectWhilePaused, bool selectWhilePlaying)
    {
        Assert.False(MainViewModel.ShouldSelectAtPlayheadWhilePaused(selectWhilePaused, selectWhilePlaying, 12.0, 10.0));
    }

    [Fact]
    public void DoesNotSelectWhenPositionUnchanged()
    {
        // Keeps a line picked in the grid while paused from being stolen back by the next tick.
        Assert.False(MainViewModel.ShouldSelectAtPlayheadWhilePaused(true, true, 10.0, 10.0005));
    }
}
