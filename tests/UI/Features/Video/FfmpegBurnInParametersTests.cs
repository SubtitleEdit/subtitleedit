using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// The ffmpeg command line built for a burn-in job. These assert the string, not the encode:
/// the generated parameters were run through ffmpeg by hand for every container/codec pair
/// offered by <see cref="Nikse.SubtitleEdit.Features.Video.BurnIn.OutputContainer"/>, and the
/// muxer name below is the part of it that no unit test could otherwise catch - a wrong "-f"
/// only shows up when the two-pass analysis pass actually runs.
/// </summary>
public class FfmpegBurnInParametersTests
{
    private static string Generate(string videoEncoding, string audioEncoding, string outputFileName, string pass = "", string twoPassBitRate = "")
    {
        return FfmpegGenerator.GenerateHardcodedVideoFile(
            "input.mp4",
            "subtitle.ass",
            outputFileName,
            320,
            240,
            videoEncoding,
            string.Empty,
            "yuv420p",
            string.Empty,
            audioEncoding,
            false,
            "48000",
            string.Empty,
            "128k",
            pass,
            twoPassBitRate);
    }

    [Theory]
    [InlineData("output.mkv", "matroska")]
    [InlineData("output.ts", "mpegts")]
    [InlineData("output.webm", "webm")]
    [InlineData("output.mov", "mov")]
    [InlineData("output.mp4", "mp4")]
    public void TwoPass_FirstPass_WritesToTheNullDeviceWithTheRealMuxer(string outputFileName, string expectedMuxer)
    {
        var parameters = Generate("libx264", "aac", outputFileName, "1", "500k");

        var nullDevice = Configuration.IsRunningOnWindows ? "NUL" : "/dev/null";
        Assert.Contains($"-f {expectedMuxer} {nullDevice}", parameters);
        Assert.DoesNotContain(outputFileName, parameters);
    }

    [Fact]
    public void TwoPass_SecondPass_WritesTheRealFile()
    {
        var parameters = Generate("libx264", "aac", "output.ts", "2", "500k");

        Assert.Contains("\"output.ts\"", parameters);
        Assert.Contains("-pass 2", parameters);
        Assert.DoesNotContain("-f mpegts", parameters);
    }

    [Fact]
    public void OnePass_HasNoPassOrNullDevice()
    {
        var parameters = Generate("libvpx-vp9", "libopus", "output.webm");

        Assert.Contains("-c:v libvpx-vp9", parameters);
        Assert.Contains("-c:a libopus", parameters);
        Assert.Contains("\"output.webm\"", parameters);
        Assert.DoesNotContain("-pass ", parameters);
    }
}
