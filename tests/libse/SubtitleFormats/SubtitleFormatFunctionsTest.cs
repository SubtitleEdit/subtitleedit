using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

[Collection("NonParallelTests")]
public class SubtitleFormatFunctionsTest
{
    [Fact]
    public void MillisecondsToFrames1()
    {
        var frames = SubtitleFormat.MillisecondsToFrames(500, 25);
        Assert.Equal(13, frames);
    }

    [Fact]
    public void MillisecondsToFrames2()
    {
        var frames = SubtitleFormat.MillisecondsToFrames(499, 25);
        Assert.Equal(12, frames);
    }

    [Fact]
    public void FramesToMilliseconds1()
    {
        Configuration.Settings.General.CurrentFrameRate = 25.0;
        var ms = SubtitleFormat.FramesToMilliseconds(1);
        Assert.Equal(40, ms);
    }

    [Theory]
    [InlineData(23.976, 86400, 3603600)]
    [InlineData(29.97, 108000, 3603600)]
    [InlineData(59.94, 216000, 3603600)]
    [InlineData(25, 90000, 3600000)]
    public void FramesToMillisecondsWithoutSmpteUsesRealTime(double frameRate, int frames, int expectedMs)
    {
        Configuration.Settings.General.CurrentVideoIsSmpte = false;
        Assert.Equal(expectedMs, SubtitleFormat.FramesToMilliseconds(frames, frameRate));
        Assert.Equal(frames, SubtitleFormat.MillisecondsToFrames(expectedMs, frameRate));
    }

    [Theory]
    [InlineData(23.976, 86400)]
    [InlineData(23.976023976, 86400)]
    [InlineData(29.97, 108000)]
    [InlineData(59.94, 216000)]
    public void SmpteTimingCountsNonDropFrameTimeCode(double frameRate, int framesInOneHour)
    {
        // #15753: in SMPTE timing frame 86400 at 23.976 is time code 01:00:00:00 (the video is
        // stretched by 1.001 to match), so frame numbers convert at the whole-number rate.
        var old = Configuration.Settings.General.CurrentVideoIsSmpte;
        try
        {
            Configuration.Settings.General.CurrentVideoIsSmpte = true;
            Assert.Equal(3600000, SubtitleFormat.FramesToMilliseconds(framesInOneHour, frameRate));
            Assert.Equal(framesInOneHour, SubtitleFormat.MillisecondsToFrames(3600000, frameRate));
        }
        finally
        {
            Configuration.Settings.General.CurrentVideoIsSmpte = old;
        }
    }

    [Fact]
    public void SmpteTimingLeavesWholeFrameRatesAlone()
    {
        var old = Configuration.Settings.General.CurrentVideoIsSmpte;
        try
        {
            Configuration.Settings.General.CurrentVideoIsSmpte = true;
            Assert.Equal(3600000, SubtitleFormat.FramesToMilliseconds(90000, 25));
            Assert.Equal(3600000, SubtitleFormat.FramesToMilliseconds(86400, 24));
        }
        finally
        {
            Configuration.Settings.General.CurrentVideoIsSmpte = old;
        }
    }

    [Fact]
    public void SmpteTimingFramePartOfTimeCodeStaysBelowFrameRate()
    {
        var oldSmpte = Configuration.Settings.General.CurrentVideoIsSmpte;
        var oldRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentVideoIsSmpte = true;
            Configuration.Settings.General.CurrentFrameRate = 23.976;
            Assert.Equal(23, SubtitleFormat.MillisecondsToFramesMaxFrameRate(999));
            Assert.Equal(958, SubtitleFormat.FramesToMillisecondsMax999(23));
        }
        finally
        {
            Configuration.Settings.General.CurrentVideoIsSmpte = oldSmpte;
            Configuration.Settings.General.CurrentFrameRate = oldRate;
        }
    }
}
