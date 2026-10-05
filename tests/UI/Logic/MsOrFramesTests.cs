using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Tool dialog boxes hold frames in frame mode and milliseconds otherwise, while the setting
/// behind them stays in milliseconds - converted the way Bridge gaps does it.
/// </summary>
public class MsOrFramesTests : IDisposable
{
    private readonly double _currentFrameRate = Configuration.Settings.General.CurrentFrameRate;

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _currentFrameRate;
    }

    [Theory]
    [InlineData(25, 1000, 25)]
    [InlineData(25, 250, 6)]
    [InlineData(23.976, 1200, 29)]
    [InlineData(29.97, 1000, 30)]
    public void FromMilliseconds_InFrameModeIsFramesAtTheCurrentFrameRate(double frameRate, int ms, int expectedFrames)
    {
        Configuration.Settings.General.CurrentFrameRate = frameRate;

        Assert.Equal(expectedFrames, MsOrFrames.FromMilliseconds(ms, frameMode: true));
        Assert.Equal(ms, MsOrFrames.FromMilliseconds(ms, frameMode: false));
    }

    [Theory]
    [InlineData(25, 25, 1000)]
    [InlineData(25, 6, 240)]
    [InlineData(23.976, 29, 1210)]
    public void ToMilliseconds_InFrameModeConvertsFrames(double frameRate, int frames, int expectedMs)
    {
        Configuration.Settings.General.CurrentFrameRate = frameRate;

        Assert.Equal(expectedMs, MsOrFrames.ToMilliseconds(frames, frameMode: true));
        Assert.Equal(frames, MsOrFrames.ToMilliseconds(frames, frameMode: false));
    }

    [Fact]
    public void ToMillisecondsForSave_KeepsTheStoredValueWhenTheFrameCountIsUnchanged()
    {
        Configuration.Settings.General.CurrentFrameRate = 23.976;

        // 1200 ms is shown as 29 frames; saving 29 back must not round it to 1210 ms.
        Assert.Equal(1200, MsOrFrames.ToMillisecondsForSave(29, frameMode: true, storedMilliseconds: 1200));

        // A changed frame count is converted.
        Assert.Equal(1251, MsOrFrames.ToMillisecondsForSave(30, frameMode: true, storedMilliseconds: 1200));

        // Millisecond mode saves what was typed.
        Assert.Equal(1234, MsOrFrames.ToMillisecondsForSave(1234, frameMode: false, storedMilliseconds: 1200));
    }

    [Fact]
    public void FrameModeComparesInFramesAt2997()
    {
        Configuration.Settings.General.CurrentFrameRate = 29.97;

        // One frame is 33 ms, but 34 ms is also one frame.
        Assert.False(MsOrFrames.IsAbove(34, 1, frameMode: true));
        Assert.True(MsOrFrames.IsAbove(67, 1, frameMode: true));
        Assert.False(MsOrFrames.IsBelow(66, 2, frameMode: true));
        Assert.True(MsOrFrames.IsBelow(33, 2, frameMode: true));

        // Millisecond mode is a plain comparison.
        Assert.True(MsOrFrames.IsAbove(34, 33, frameMode: false));
        Assert.True(MsOrFrames.IsBelow(66, 67, frameMode: false));
    }

    [Theory]
    [InlineData(29.97, 0, 16)]
    [InlineData(29.97, 1, 50)]
    [InlineData(25, 1, 59)]
    [InlineData(25, 6, 259)]
    public void MaxGapMillisecondsIsTheLargestGapStillWithinTheFrames(double frameRate, int frames, int expectedMs)
    {
        Configuration.Settings.General.CurrentFrameRate = frameRate;

        Assert.Equal(expectedMs, MsOrFrames.ToMaxGapMilliseconds(frames, frameMode: true));
        Assert.Equal(frames, MsOrFrames.ToMaxGapMilliseconds(frames, frameMode: false));
    }
}
