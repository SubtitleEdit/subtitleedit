using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

[Collection("NonParallelTests")]
public class SubtitleFormatTest
{
    [Fact]
    
    public void MillisecondsToFrames()
    {
        Configuration.Settings.General.CurrentFrameRate = 23.976;
        var fr = SubtitleFormat.MillisecondsToFrames(100);
        Assert.Equal(2, fr);
        fr = SubtitleFormat.MillisecondsToFrames(999);
        Assert.Equal(24, fr);

        Configuration.Settings.General.CurrentFrameRate = 30;
        fr = SubtitleFormat.MillisecondsToFrames(100);
        Assert.Equal(3, fr);
        fr = SubtitleFormat.MillisecondsToFrames(999);
        Assert.Equal(30, fr);

        fr = SubtitleFormat.MillisecondsToFrames(2000);
        Assert.Equal(60, fr);
    }

    [Fact]
    
    public void MillisecondsToFramesMaxFrameRate()
    {
        Configuration.Settings.General.CurrentFrameRate = 30;
        var fr = SubtitleFormat.MillisecondsToFramesMaxFrameRate(100);
        Assert.Equal(3, fr);
        fr = SubtitleFormat.MillisecondsToFramesMaxFrameRate(1000);
        Assert.Equal(29, fr);

        fr = SubtitleFormat.MillisecondsToFramesMaxFrameRate(2000);
        Assert.Equal(29, fr);
    }

    [Fact]
    public void FramesToMilliseconds()
    {
        Configuration.Settings.General.CurrentFrameRate = 30;
        var fr = SubtitleFormat.FramesToMilliseconds(1);
        Assert.Equal(33, fr);
        fr = SubtitleFormat.FramesToMilliseconds(30);
        Assert.Equal(1000, fr);

        fr = SubtitleFormat.FramesToMillisecondsMax999(30);
        Assert.Equal(999, fr);
    }
}