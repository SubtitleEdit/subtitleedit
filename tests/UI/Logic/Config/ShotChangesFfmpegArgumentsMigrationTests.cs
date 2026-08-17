using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class ShotChangesFfmpegArgumentsMigrationTests
{
    [Fact]
    public void LegacyArgumentsLoseVsync()
    {
        var video = new SeVideo
        {
            ShowChangesFFmpegArguments = "-i \"{0}\" -vf \"select=gt(scene\\,{1}),showinfo\" -threads 0 -vsync vfr -f null -",
        };

        Se.MigrateShotChangesFfmpegArguments(video);

        Assert.Equal("-i \"{0}\" -vf \"select=gt(scene\\,{1}),showinfo\" -threads 0 -f null -", video.ShowChangesFFmpegArguments);
    }

    [Fact]
    public void DefaultArgumentsAreUnchanged()
    {
        var video = new SeVideo();
        var expected = video.ShowChangesFFmpegArguments;

        Se.MigrateShotChangesFfmpegArguments(video);

        Assert.DoesNotContain("-vsync", video.ShowChangesFFmpegArguments);
        Assert.Equal(expected, video.ShowChangesFFmpegArguments);
    }

    [Theory]
    [InlineData("-i \"{0}\" -vsync 0 -f null -", "-i \"{0}\" -f null -")]
    [InlineData("-i \"{0}\" -vsync passthrough -threads 0 -f null -", "-i \"{0}\" -threads 0 -f null -")]
    [InlineData("-i \"{0}\" -threads 0 -f null - -vsync vfr", "-i \"{0}\" -threads 0 -f null -")]
    [InlineData("-vsync vfr -i \"{0}\" -f null -", "-i \"{0}\" -f null -")]
    public void VsyncIsRemovedWithItsValue(string arguments, string expected)
    {
        var video = new SeVideo { ShowChangesFFmpegArguments = arguments };

        Se.MigrateShotChangesFfmpegArguments(video);

        Assert.Equal(expected, video.ShowChangesFFmpegArguments);
    }

    [Fact]
    public void OtherCustomArgumentsAreKept()
    {
        var video = new SeVideo
        {
            ShowChangesFFmpegArguments = "-i \"{0}\" -vf \"select=gt(scene\\,{1}),showinfo\" -threads 4 -f null -",
        };

        Se.MigrateShotChangesFfmpegArguments(video);

        Assert.Equal("-i \"{0}\" -vf \"select=gt(scene\\,{1}),showinfo\" -threads 4 -f null -", video.ShowChangesFFmpegArguments);
    }

    [Fact]
    public void EmptyArgumentsAreLeftAlone()
    {
        var video = new SeVideo { ShowChangesFFmpegArguments = string.Empty };

        Se.MigrateShotChangesFfmpegArguments(video);

        Assert.Equal(string.Empty, video.ShowChangesFFmpegArguments);
    }
}
