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
}
