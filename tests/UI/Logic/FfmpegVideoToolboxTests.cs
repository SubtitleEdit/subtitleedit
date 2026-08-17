using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic;

// VideoToolbox (issue #13382) takes a different set of flags than the x264/NVENC/AMF encoders:
// no -preset, and constant quality is "-q:v 1-100" rather than "-crf". Emitting the wrong flag
// is not a hard error - ffmpeg just warns and silently ignores it - so lock the mapping down.
// Also covers the hvc1 tag, which every HEVC encoder needs for Apple playback.
public class FfmpegVideoToolboxTests
{
    private static string Generate(string videoEncoding, string preset, string crf)
    {
        return FfmpegGenerator.GenerateHardcodedVideoFile(
            "in.mp4",
            "sub.ass",
            "out.mp4",
            1920,
            1080,
            videoEncoding,
            preset,
            "yuv420p",
            crf,
            "aac",
            false,
            "48000",
            string.Empty,
            "128k",
            string.Empty,
            string.Empty);
    }

    [Theory]
    [InlineData("h264_videotoolbox")]
    [InlineData("hevc_videotoolbox")]
    public void VideoToolbox_UsesQualityInsteadOfCrf(string videoEncoding)
    {
        var args = Generate(videoEncoding, string.Empty, "70");

        Assert.Contains($"-c:v {videoEncoding}", args);
        Assert.Contains("-q:v 70", args);
        Assert.DoesNotContain("-crf", args);
    }

    [Theory]
    [InlineData("h264_videotoolbox")]
    [InlineData("hevc_videotoolbox")]
    public void VideoToolbox_NeverEmitsPreset(string videoEncoding)
    {
        // A preset can survive in the saved settings from a previously selected encoder.
        var args = Generate(videoEncoding, "medium", string.Empty);

        Assert.DoesNotContain("-preset", args);
    }

    // QuickTime and the rest of the Apple stack reject the hev1 tag the mov muxer writes by
    // default, so every HEVC encoder needs the tag - not just libx265, which was the only one
    // getting it before.
    [Theory]
    [InlineData("libx265")]
    [InlineData("hevc_videotoolbox")]
    [InlineData("hevc_nvenc")]
    [InlineData("hevc_amf")]
    [InlineData("hevc_qsv")]
    public void HevcEncoders_AreTaggedHvc1(string videoEncoding)
    {
        Assert.Contains("-tag:v hvc1", Generate(videoEncoding, string.Empty, string.Empty));
    }

    [Theory]
    [InlineData("libx264")]
    [InlineData("h264_videotoolbox")]
    [InlineData("h264_nvenc")]
    [InlineData("prores_videotoolbox")]
    public void NonHevcEncoders_AreNotTaggedHvc1(string videoEncoding)
    {
        Assert.DoesNotContain("-tag:v hvc1", Generate(videoEncoding, string.Empty, string.Empty));
    }

    // prores_videotoolbox takes the same named profiles as prores_ks, mapped to their indexes.
    [Theory]
    [InlineData("proxy", "0")]
    [InlineData("lt", "1")]
    [InlineData("standard", "2")]
    [InlineData("hq", "3")]
    [InlineData("4444", "4")]
    [InlineData("4444xq", "5")]
    public void ProResVideoToolbox_MapsProfileNameToIndex(string preset, string expectedProfile)
    {
        var args = Generate("prores_videotoolbox", preset, string.Empty);

        Assert.Contains($"-profile:v {expectedProfile}", args);
        Assert.DoesNotContain("-preset", args);
    }
}
