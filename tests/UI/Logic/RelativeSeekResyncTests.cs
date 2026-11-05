using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Logic;

public class RelativeSeekResyncTests
{
    [Theory]
    [InlineData(500)]
    [InlineData(-500)]
    [InlineData(1000)]
    [InlineData(2000)]
    [InlineData(-5000)]
    public void InFlightSeek_OneStepBehind_KeepsTrackedTarget(int ms)
    {
        // The player still reports the previous position: exactly one step behind the target (#16469).
        var lag = Math.Abs(ms) / 1000.0;
        Assert.True(lag < MainViewModel.RelativeSeekResyncSeconds(ms));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(40)]
    [InlineData(100)]
    public void SmallSteps_KeepAboutHalfSecondGuard(int ms)
    {
        var resync = MainViewModel.RelativeSeekResyncSeconds(ms);
        Assert.True(resync > 0.5);
        Assert.True(resync <= 0.6);
    }

    [Fact]
    public void FloatRoundedHalfSecondLag_StillContinues()
    {
        // 12.345 + 0.5 - 12.345 is not exactly 0.5 in doubles; the old "< 0.5" guard flipped on this.
        var tracked = 12.345 + 0.5;
        var actual = 12.345;
        Assert.True(Math.Abs(tracked - actual) < MainViewModel.RelativeSeekResyncSeconds(500));
    }
}
