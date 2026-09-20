using Nikse.SubtitleEdit.Features.Video.BurnIn;

namespace UITests.Features.Video;

/// <summary>
/// The nvenc "-preset" and "-tune" values. ffmpeg 9.0 removed the deprecated aliases, so offering one of
/// them now aborts the burn-in with "exit code -22" (issue #14927) - these assert that only
/// names ffmpeg still has are offered, and that a settings file written before that keeps the
/// speed/quality the user picked instead of quietly falling back to the default.
/// </summary>
public class VideoPresetOptionsTests
{
    [Theory]
    [InlineData("h264_nvenc")]
    [InlineData("hevc_nvenc")]
    public void Nvenc_OffersOnlyPresetsFfmpegStillHas(string codec)
    {
        Assert.True(VideoPresetOptions.IsNvenc(codec));

        var presets = VideoPresetOptions.GetNvencPresets();

        Assert.Equal(new[] { "slow", "medium", "fast", "p1", "p2", "p3", "p4", "p5", "p6", "p7" }, presets);
    }

    [Theory]
    [InlineData("libx264")]
    [InlineData("h264_qsv")]
    [InlineData("h264_videotoolbox")]
    [InlineData("prores_ks")]
    public void NonNvencCodecs_AreNotTreatedAsNvenc(string codec)
    {
        Assert.False(VideoPresetOptions.IsNvenc(codec));
    }

    [Theory]
    [InlineData("default", "p4")]
    [InlineData("hp", "p1")]
    [InlineData("hq", "p7")]
    [InlineData("bd", "p5")]
    [InlineData("ll", "p4")]
    [InlineData("llhq", "p7")]
    [InlineData("llhp", "p1")]
    [InlineData("lossless", "p4")]
    [InlineData("losslesshp", "p1")]
    public void RemovedNvencPresets_AreMigratedToTheirReplacement(string stored, string expected)
    {
        Assert.Equal(expected, VideoPresetOptions.Migrate("h264_nvenc", stored));
        Assert.Equal(expected, VideoPresetOptions.Migrate("hevc_nvenc", stored));
    }

    [Theory]
    [InlineData("slow")]
    [InlineData("medium")]
    [InlineData("fast")]
    [InlineData("p7")]
    public void PresetsFfmpegKept_AreLeftAlone(string stored)
    {
        Assert.Equal(stored, VideoPresetOptions.Migrate("h264_nvenc", stored));
    }

    [Theory]
    [InlineData("libx264", "veryslow")]
    [InlineData("prores_ks", "hq")]
    [InlineData("h264_qsv", "veryfast")]
    public void OtherEncoders_KeepTheirOwnPresetNames(string codec, string stored)
    {
        Assert.Equal(stored, VideoPresetOptions.Migrate(codec, stored));
    }

    [Theory]
    [InlineData("h264_nvenc")]
    [InlineData("hevc_nvenc")]
    public void Nvenc_OffersTheTuningModes(string codec)
    {
        var tunes = VideoPresetOptions.GetTunes(codec);

        Assert.Equal(new[] { VideoPresetOptions.BlankTune, "hq", "ll", "ull", "lossless" }, tunes);
    }

    [Theory]
    [InlineData("libx264")]
    [InlineData("h264_amf")]
    [InlineData("h264_videotoolbox")]
    public void OtherEncoders_HaveNoTuningModes(string codec)
    {
        Assert.Equal(new[] { VideoPresetOptions.BlankTune }, VideoPresetOptions.GetTunes(codec));
    }

    [Theory]
    [InlineData("lossless", "lossless")]
    [InlineData("losslesshp", "lossless")]
    [InlineData("ll", "ll")]
    [InlineData("llhq", "ll")]
    [InlineData("llhp", "ll")]
    public void RemovedAliasesThatSetATuningMode_KeepIt(string stored, string expected)
    {
        Assert.Equal(expected, VideoPresetOptions.MigrateTune("h264_nvenc", stored));
        Assert.Equal(expected, VideoPresetOptions.MigrateTune("hevc_nvenc", stored));
    }

    [Theory]
    [InlineData("hq")]
    [InlineData("hp")]
    [InlineData("bd")]
    [InlineData("default")]
    [InlineData("medium")]
    [InlineData("p7")]
    public void PresetsWithoutATuningMode_ImplyNone(string stored)
    {
        Assert.Equal(string.Empty, VideoPresetOptions.MigrateTune("h264_nvenc", stored));
    }

    [Fact]
    public void OtherEncoders_NeverGetAnImpliedTuningMode()
    {
        // ProRes has its own "lossless"-ish profiles; nothing there implies an nvenc tune.
        Assert.Equal(string.Empty, VideoPresetOptions.MigrateTune("prores_ks", "lossless"));
        Assert.Equal(string.Empty, VideoPresetOptions.MigrateTune("libx264", "ll"));
    }

    [Fact]
    public void NoPreset_StaysEmpty()
    {
        Assert.Null(VideoPresetOptions.Migrate("h264_nvenc", null));
        Assert.Equal(string.Empty, VideoPresetOptions.Migrate("h264_nvenc", string.Empty));
        Assert.Equal(string.Empty, VideoPresetOptions.MigrateTune("h264_nvenc", null));
        Assert.Equal(string.Empty, VideoPresetOptions.MigrateTune("h264_nvenc", string.Empty));
    }
}
