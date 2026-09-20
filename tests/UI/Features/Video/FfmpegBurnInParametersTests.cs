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
    private static string Generate(string videoEncoding, string audioEncoding, string outputFileName, string pass = "", string twoPassBitRate = "", string preset = "", string crf = "", string tune = "")
    {
        return FfmpegGenerator.GenerateHardcodedVideoFile(
            "input.mp4",
            "subtitle.ass",
            outputFileName,
            320,
            240,
            videoEncoding,
            preset,
            "yuv420p",
            crf,
            audioEncoding,
            false,
            "48000",
            tune,
            "128k",
            pass,
            twoPassBitRate);
    }

    [Fact]
    public void Nvenc_Tune_IsWrittenNextToThePreset()
    {
        var parameters = Generate("h264_nvenc", "copy", "output.mp4", preset: "p7", crf: "25", tune: "ll");

        Assert.Contains("-preset p7 -tune ll", parameters);
        Assert.Contains("-cq 25", parameters);
    }

    [Fact]
    public void Nvenc_LosslessTune_LeavesOutTheCqValue()
    {
        // nvenc pins constant QP 0 for a lossless tune and drops -cq, so writing one would only
        // put a quality in the command line that the encode never uses.
        var parameters = Generate("h264_nvenc", "copy", "output.mp4", preset: "p4", crf: "25", tune: "lossless");

        Assert.Contains("-preset p4 -tune lossless", parameters);
        Assert.DoesNotContain("-cq", parameters);
    }

    [Fact]
    public void NoTune_WritesNoTuneArgument()
    {
        var parameters = Generate("libx264", "aac", "output.mkv", preset: "medium", crf: "23");

        Assert.DoesNotContain("-tune", parameters);
        Assert.Contains("-crf 23", parameters);
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
    private static string GenerateImage(string cutStart = "", bool inputIsAudioOnly = false, Nikse.SubtitleEdit.Features.Video.BurnIn.BurnInLogo? logo = null, string subtitleFileName = "/tmp/subs.sup")
    {
        return FfmpegGenerator.GenerateHardcodedVideoFile(
            "input.mp4",
            subtitleFileName,
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
        Assert.Contains("-filter_complex \"[0:v]scale=320:240[video];[1:s]scale=320:240[subs];[video][subs]overlay=eof_action=pass\"", parameters);
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

        Assert.Contains("-y -ss 00:01:02.500 -i \"input.mp4\" -itsoffset -62.5 -i \"/tmp/subs.sup\"", parameters);
    }

    /// <summary>
    /// ffmpeg restarts the sup input at zero as well - at its first segment. A sup whose first
    /// subtitle is at 11:19 showed it on the first frame of the video, and every later one that
    /// much too early, so the input is moved forward by the time of that first segment.
    /// </summary>
    [Theory]
    [InlineData("", " -itsoffset 679.762 -i ")]
    [InlineData("-ss 00:10:00.000", " -itsoffset 79.762 -i ")]
    [InlineData("-ss 00:11:19.762", "\"input.mp4\" -i ")]
    public void ImageSubtitle_IsMovedToTheTimeOfItsFirstSegment(string cutStart, string expected)
    {
        var supFileName = Path.Combine(Path.GetTempPath(), $"se-test-{System.Guid.NewGuid()}.sup");
        try
        {
            // "PG", a 90 kHz presentation time stamp (679.762 s), a decoding time stamp, and an
            // empty END segment.
            var pts = (uint)(679.762 * 90000);
            File.WriteAllBytes(supFileName, new byte[]
            {
                (byte)'P', (byte)'G',
                (byte)(pts >> 24), (byte)(pts >> 16), (byte)(pts >> 8), (byte)pts,
                0, 0, 0, 0,
                0x80, 0, 0,
            });

            var parameters = GenerateImage(cutStart: cutStart, subtitleFileName: supFileName);

            Assert.Contains(expected, parameters);
        }
        finally
        {
            File.Delete(supFileName);
        }
    }

    /// <summary>
    /// When the sup ran out before the video, the overlay's default end-of-stream action made
    /// ffmpeg write one last frame stamped ~4294967 s: .mp4 output aborted with exit code 176
    /// and .mkv output claimed a duration of 1193 hours.
    /// </summary>
    [Fact]
    public void ImageSubtitle_OverlayLetsTheVideoThroughWhenTheSubtitlesEnd()
    {
        Assert.Contains("[video][subs]overlay=eof_action=pass", GenerateImage());
    }

    /// <summary>
    /// The audio bit rate box is always shown, but "-b:a" was only written for a target file size.
    /// </summary>
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "500k")]
    [InlineData("2", "500k")]
    public void AudioBitRate_IsWrittenWheneverAudioIsEncoded(string pass, string twoPassBitRate)
    {
        var parameters = Generate("libx264", "aac", "output.mp4", pass, twoPassBitRate);

        Assert.Single(System.Text.RegularExpressions.Regex.Matches(parameters, "-b:a 128k"));
    }

    [Fact]
    public void AudioBitRate_IsLeftOutWhenAudioIsCopied()
    {
        Assert.DoesNotContain("-b:a", Generate("libx264", "copy", "output.mp4"));
    }

    /// <summary>
    /// ffmpeg aborts on a sample rate the encoder cannot do: libopus takes 48000 only (of the
    /// rates offered), ac3 and mp3 stop at 48000, aac at 96000.
    /// </summary>
    [Theory]
    [InlineData("libopus", "44100", "48000")]
    [InlineData("libopus", "96000", "48000")]
    [InlineData("ac3", "96000", "48000")]
    [InlineData("mp3", "192000", "48000")]
    [InlineData("aac", "192000", "96000")]
    [InlineData("aac", "44100", "44100")]
    [InlineData("libvorbis", "96000", "96000")]
    public void SampleRate_IsLimitedToWhatTheEncoderSupports(string audioEncoding, string sampleRate, string expected)
    {
        Assert.Equal(expected, FfmpegGenerator.GetSupportedSampleRate(audioEncoding, sampleRate));
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
            Assert.Contains("[0:v]scale=320:240[video];[1:s]scale=320:240[subs];[video][subs]overlay=eof_action=pass[withsubs];[2:v]scale=", parameters);
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

    /// <summary>
    /// No subtitle lines (issue #14777): the ass filter is left out entirely - "ass=" with no
    /// file name makes ffmpeg fail with "Invalid argument" before writing a frame.
    /// </summary>
    [Fact]
    public void NoSubtitle_OnlyScales()
    {
        var parameters = FfmpegGenerator.GenerateHardcodedVideoFile(
            "input.mp4", string.Empty, "output.mp4", 320, 240, "h264_nvenc", string.Empty, "yuv420p",
            string.Empty, "aac", false, "48000", string.Empty, "128k", string.Empty, string.Empty);

        Assert.Contains("-vf \"scale=320:240\"", parameters);
        Assert.DoesNotContain("ass=", parameters);
        Assert.DoesNotContain("-filter_complex", parameters);
    }

    [Fact]
    public void NoSubtitle_WithLogo_OverlaysTheLogoOnTheScaledVideo()
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

            var parameters = FfmpegGenerator.GenerateHardcodedVideoFile(
                "input.mp4", string.Empty, "output.mp4", 320, 240, "libx264", string.Empty, "yuv420p",
                string.Empty, "aac", false, "48000", string.Empty, "128k", string.Empty, string.Empty,
                burnInLogo: logo);

            Assert.Contains("-filter_complex \"[0:v]scale=320:240[withsubs];[1:v]scale=", parameters);
            Assert.DoesNotContain("ass=", parameters);
        }
        finally
        {
            File.Delete(logoFileName);
        }
    }
    private static string Generate3D(Nikse.SubtitleEdit.UiLogic.Export.Export3DMode mode, int depth, string subtitleFileName = "subtitle.ass",
        bool subtitleIsImage = false, Nikse.SubtitleEdit.Features.Video.BurnIn.BurnInLogo? logo = null)
    {
        return FfmpegGenerator.GenerateHardcodedVideoFile(
            "input.mp4", subtitleFileName, "output.mp4", 1920, 1080, "libx264", string.Empty, "yuv420p",
            string.Empty, "aac", false, "48000", string.Empty, "128k", string.Empty, string.Empty,
            burnInLogo: logo, subtitleIsImage: subtitleIsImage, mode3D: mode, depth3D: depth);
    }

    [Fact]
    public void Text3D_HalfSideBySide_RendersOnATransparentCopyAndOverlaysEachEye()
    {
        // Run through ffmpeg 4.4 by hand: each eye gets a half-width copy of the subtitle, the
        // left one 7 pixels to the right and the right one 7 to the left.
        var parameters = Generate3D(Nikse.SubtitleEdit.UiLogic.Export.Export3DMode.HalfSideBySide, 7);

        Assert.Contains(
            "-filter_complex \"[0:v]scale=1920:1080,split=3[v3d1][v3d2][v3d0];" +
            "[v3d0]format=rgba,colorchannelmixer=rr=0:gg=0:bb=0:aa=0,ass=subtitle.ass:alpha=1,split[s3d1][s3d2];" +
            "[s3d1]scale=960:1080[s3d1h];[s3d2]scale=960:1080[s3d2h];" +
            "[v3d1]crop=960:1080:0:0[e3d1];[v3d2]crop=960:1080:960:0[e3d2];" +
            "[e3d1][s3d1h]overlay=x=7:y=0:alpha=premultiplied[o3d1];[e3d2][s3d2h]overlay=x=-7:y=0:alpha=premultiplied[o3d2];" +
            "[o3d1][o3d2]hstack\"",
            parameters);
        Assert.DoesNotContain("-vf", parameters);
    }

    [Fact]
    public void Image3D_HalfTopBottom_SplitsTheSupStreamIntoBothEyes()
    {
        var parameters = Generate3D(Nikse.SubtitleEdit.UiLogic.Export.Export3DMode.HalfTopBottom, -4, "/tmp/subs.sup", subtitleIsImage: true);

        // Bitmap subtitles are not premultiplied, so the overlays keep the default alpha.
        Assert.Contains(
            "-filter_complex \"[0:v]scale=1920:1080,split[v3d1][v3d2];[1:s]scale=1920:1080,split[s3d1][s3d2];" +
            "[s3d1]scale=1920:540[s3d1h];[s3d2]scale=1920:540[s3d2h];" +
            "[v3d1]crop=1920:540:0:0[e3d1];[v3d2]crop=1920:540:0:540[e3d2];" +
            "[e3d1][s3d1h]overlay=x=-4:y=0:eof_action=pass[o3d1];[e3d2][s3d2h]overlay=x=4:y=0:eof_action=pass[o3d2];" +
            "[o3d1][o3d2]vstack\"",
            parameters);
    }

    [Fact]
    public void Text3D_WithoutSubtitleLines_IsJustTheScale()
    {
        var parameters = Generate3D(Nikse.SubtitleEdit.UiLogic.Export.Export3DMode.HalfSideBySide, 5, string.Empty);

        Assert.Contains("-vf \"scale=1920:1080\"", parameters);
        Assert.DoesNotContain("stack", parameters);
    }

    [Fact]
    public void Text3D_WithLogo_PutsTheLogoOverTheStackedEyes()
    {
        var logoFileName = Path.GetTempFileName();
        try
        {
            var logo = new Nikse.SubtitleEdit.Features.Video.BurnIn.BurnInLogo { LogoFileName = logoFileName, X = 10, Y = 20, Size = 100, Alpha = 100 };

            var parameters = Generate3D(Nikse.SubtitleEdit.UiLogic.Export.Export3DMode.HalfSideBySide, 0, logo: logo);

            Assert.Contains("[o3d1][o3d2]hstack[withsubs];[1:v]scale=", parameters);
        }
        finally
        {
            File.Delete(logoFileName);
        }
    }
}
