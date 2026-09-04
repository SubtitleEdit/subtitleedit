using Nikse.SubtitleEdit.Core.Common;
using System.IO;
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

    /// <summary>
    /// Without "-y" ffmpeg refuses to touch an existing output file ("File ... already exists.
    /// Exiting.") - and with the old file still in place the burn-in reported success while
    /// nothing had been re-encoded (issue #14210).
    /// </summary>
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "500k")]
    [InlineData("2", "500k")]
    public void Overwrite_IsAlwaysAllowed(string pass, string twoPassBitRate)
    {
        var parameters = Generate("libx264", "aac", "output.mp4", pass, twoPassBitRate);

        Assert.StartsWith("-y ", parameters);
    }

    /// <summary>
    /// A Blu-ray sup (from the image-based editor, or a batch item's subtitle) is a second input
    /// laid over the frames, scaled to the output size like the video - libass' "ass" filter
    /// renders text only. Overlapping lines are shown together this way (issue #14456).
    /// </summary>
    private static string GenerateImage(string cutStart = "", bool inputIsAudioOnly = false, Nikse.SubtitleEdit.Features.Video.BurnIn.BurnInLogo? logo = null)
    {
        return FfmpegGenerator.GenerateHardcodedVideoFile(
            "input.mp4",
            "/tmp/subs.sup",
            "output.mp4",
            320,
            240,
            "libx264",
            string.Empty,
            "yuv420p",
            string.Empty,
            "aac",
            false,
            "48000",
            string.Empty,
            "128k",
            string.Empty,
            string.Empty,
            cutStart,
            string.Empty,
            string.Empty,
            logo,
            inputIsAudioOnly,
            subtitleIsImage: true);
    }

    [Fact]
    public void ImageSubtitle_IsASecondInputOverlaidAfterScaling()
    {
        var parameters = GenerateImage();

        Assert.Contains("-y -i \"input.mp4\" -i \"/tmp/subs.sup\"", parameters);
        Assert.Contains("-filter_complex \"[0:v]scale=320:240[video];[1:s]scale=320:240[subs];[video][subs]overlay\"", parameters);
        Assert.DoesNotContain("ass=", parameters);
        Assert.DoesNotContain("-vf", parameters);
    }

    /// <summary>
    /// "-ss" before the video input restarts its timestamps at zero, and the sup demuxer cannot
    /// seek, so the subtitle input is shifted back by the same time to stay in step.
    /// </summary>
    [Fact]
    public void ImageSubtitle_WithCut_ShiftsTheSubtitleInputBackByTheCut()
    {
        var parameters = GenerateImage(cutStart: "-ss 00:01:02.500");

        Assert.Contains("-y -ss 00:01:02.500 -i \"input.mp4\" -itsoffset -00:01:02.500 -i \"/tmp/subs.sup\"", parameters);
    }

    [Fact]
    public void ImageSubtitle_WithoutCut_HasNoOffset()
    {
        Assert.DoesNotContain("-itsoffset", GenerateImage());
    }

    [Fact]
    public void ImageSubtitle_AudioOnlyInput_OverlaysOnTheCanvasAndNumbersTheInputsAfterIt()
    {
        var parameters = GenerateImage(inputIsAudioOnly: true);

        Assert.Contains("-i \"input.mp4\" -f lavfi -i color=c=black:s=320x240:r=25 -i \"/tmp/subs.sup\"", parameters);
        Assert.Contains("[1:v]scale=320:240[video];[2:s]scale=320:240[subs];[video][subs]overlay", parameters);
    }

    [Fact]
    public void ImageSubtitle_WithLogo_PutsTheLogoOverTheSubtitledFrames()
    {
        var logoFileName = Path.GetTempFileName();
        try
        {
            var logo = new Nikse.SubtitleEdit.Features.Video.BurnIn.BurnInLogo
            {
                LogoFileName = logoFileName,
                X = 10,
                Y = 20,
                Size = 100,
                Alpha = 100,
            };

            var parameters = GenerateImage(logo: logo);

            Assert.Contains($"-i \"input.mp4\" -i \"/tmp/subs.sup\" -i \"{logoFileName}\"", parameters);
            Assert.Contains("[0:v]scale=320:240[video];[1:s]scale=320:240[subs];[video][subs]overlay[withsubs];[2:v]scale=", parameters);
            Assert.Contains("[withsubs][logo]overlay=10:20", parameters);
        }
        finally
        {
            File.Delete(logoFileName);
        }
    }

    /// <summary>
    /// The text path is what every existing user runs; the image switch must not touch it.
    /// </summary>
    [Fact]
    public void TextSubtitle_StillUsesTheAssFilter()
    {
        var parameters = Generate("libx264", "aac", "output.mp4");

        Assert.Contains("-y -i \"input.mp4\" ", parameters);
        Assert.Contains("-vf \"scale=320:240,ass=subtitle.ass\"", parameters);
        Assert.DoesNotContain("-filter_complex", parameters);
        Assert.DoesNotContain("overlay", parameters);
    }
}
