using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic;

// Targeting a file size normally runs two passes. VideoToolbox accepts -pass but writes an empty
// stats file, so its analyze pass is wasted time and those encodes go single-pass with an average
// bit rate instead (#13401). The trap that makes this worth pinning: with both -q:v and -b:v on
// the command line ffmpeg keeps the quality target and silently ignores the bit rate, so the file
// comes out at the wrong size with no warning. Verified against ffmpeg 4.4.6 / h264_videotoolbox:
// "-q:v 50 -b:v 800k" produced byte-for-byte the same output as "-q:v 50" alone.
public class FfmpegTargetFileSizeTests
{
    private static string Generate(string videoEncoding, string crf, string pass, string bitRate)
    {
        return FfmpegGenerator.GenerateHardcodedVideoFile(
            "in.mp4",
            "sub.ass",
            "out.mp4",
            1920,
            1080,
            videoEncoding,
            string.Empty,
            "yuv420p",
            crf,
            "aac",
            false,
            "48000",
            string.Empty,
            "128k",
            pass,
            bitRate);
    }

    [Theory]
    [InlineData("h264_videotoolbox")]
    [InlineData("hevc_videotoolbox")]
    [InlineData("prores_videotoolbox")]
    public void IsVideoToolboxEncoder_DetectsAllThree(string codec)
    {
        Assert.True(BurnInViewModel.IsVideoToolboxEncoder(codec));
    }

    [Theory]
    [InlineData("libx264")]
    [InlineData("libx265")]
    [InlineData("h264_nvenc")]
    [InlineData("hevc_amf")]
    [InlineData("h264_qsv")]
    [InlineData("")]
    [InlineData(null)]
    public void IsVideoToolboxEncoder_IgnoresEverythingElse(string? codec)
    {
        Assert.False(BurnInViewModel.IsVideoToolboxEncoder(codec));
    }

    [Fact]
    public void BitRateWithoutPass_EmitsSinglePassAverageBitRate()
    {
        var args = Generate("h264_videotoolbox", string.Empty, string.Empty, "2500k");

        Assert.Contains("-b:v 2500k", args);
        Assert.DoesNotContain("-pass", args);
    }

    [Fact]
    public void BitRateWithoutPass_DropsTheQualityTargetSoTheBitRateIsNotIgnored()
    {
        var args = Generate("h264_videotoolbox", "70", string.Empty, "2500k");

        Assert.Contains("-b:v 2500k", args);
        Assert.DoesNotContain("-q:v", args);
    }

    [Fact]
    public void QualityWithoutBitRate_StillEmitsTheQualityTarget()
    {
        var args = Generate("h264_videotoolbox", "70", string.Empty, string.Empty);

        Assert.Contains("-q:v 70", args);
        Assert.DoesNotContain("-b:v", args);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    public void TwoPassIsUnchangedForTheOtherEncoders(string pass)
    {
        var args = Generate("libx264", "23", pass, "2500k");

        Assert.Contains($"-b:v 2500k -pass {pass}", args);
        Assert.DoesNotContain("-crf", args);
    }
}
