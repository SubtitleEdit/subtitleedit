using Nikse.SubtitleEdit.Logic.Media;

namespace Tests.Logic;

public class FfmpegProgressTrackerTests
{
    [Fact]
    public void OutTime_AdvancesToNewPercent()
    {
        var tracker = new FfmpegProgressTracker(100); // 100 s total => 1 s = 1 %

        Assert.True(tracker.TryGetNewPercent("out_time_us=25000000", out var percent));
        Assert.Equal(25, percent);
    }

    [Fact]
    public void Percent_IsMonotonic_NeverGoesBackwards()
    {
        var tracker = new FfmpegProgressTracker(100);

        Assert.True(tracker.TryGetNewPercent("out_time_us=50000000", out _));
        Assert.False(tracker.TryGetNewPercent("out_time_us=40000000", out _)); // lower
        Assert.False(tracker.TryGetNewPercent("out_time_us=50400000", out _)); // same whole percent
        Assert.True(tracker.TryGetNewPercent("out_time_us=51000000", out var percent));
        Assert.Equal(51, percent);
    }

    [Fact]
    public void OvershootAndGarbage_AreHandled()
    {
        var tracker = new FfmpegProgressTracker(100);

        Assert.True(tracker.TryGetNewPercent("out_time_us=250000000", out var percent)); // past the end (VFR estimate)
        Assert.Equal(100, percent);

        Assert.False(tracker.TryGetNewPercent("out_time_us=N/A", out _));
        Assert.False(tracker.TryGetNewPercent("out_time_us=-1", out _));
        Assert.False(tracker.TryGetNewPercent(null, out _));
        Assert.False(tracker.TryGetNewPercent("frame=123", out _));
    }

    [Fact]
    public void ZeroDuration_NeverReportsProgress()
    {
        var tracker = new FfmpegProgressTracker(0);
        Assert.False(tracker.TryGetNewPercent("out_time_us=1000000", out _));
    }

    [Theory]
    [InlineData("frame=123", true, 123L)]
    [InlineData("frame=  456 fps= 25 q=28.0 size=1024KiB", true, 456L)] // stderr stats fallback
    [InlineData("Frame=7", true, 7L)]
    [InlineData("frame=", false, 0L)]
    [InlineData("out_time_us=1000", false, 0L)]
    [InlineData(null, false, 0L)]
    public void TryGetFrame_ParsesProgressAndStatsForms(string? line, bool expected, long expectedFrame)
    {
        Assert.Equal(expected, FfmpegProgressTracker.TryGetFrame(line, out var frame));
        Assert.Equal(expectedFrame, frame);
    }

    [Theory]
    [InlineData("frame=123", true)]
    [InlineData("fps=25.0", true)]
    [InlineData("out_time_us=1000", true)]
    [InlineData("out_time=00:00:01.000000", true)]
    [InlineData("speed=12.3x", true)]
    [InlineData("progress=continue", true)]
    [InlineData("stream_0_0_q=28.0", true)]
    [InlineData("[Parsed_showinfo_1 @ 0x1] n: 0 pts_time:1.23", false)]
    public void IsProgressLine_FiltersOnlyProgressBlockKeys(string line, bool expected)
    {
        Assert.Equal(expected, FfmpegProgressTracker.IsProgressLine(line));
    }
}
