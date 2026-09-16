using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// The per-encoder quality and pixel format settings offered by burn-in, which were built for
/// libx264 and did not survive the hardware encoders:
/// <list type="bullet">
/// <item>AMF "-quality" is a three-way preference whose integers differ per codec - h264_amf
/// takes 0-2, hevc_amf takes 0/5/10 - so the shared 0-10 list aborted every AMD H.264 encode
/// above 2 with "Value 5.000000 for parameter 'quality' out of range [0 - 2]".</item>
/// <item>QSV has no "crf" option at all: ffmpeg warned the option went unused and encoded at its
/// default CQP, so the quality picked did nothing.</item>
/// <item>An unsupported "-pix_fmt" is not an error either - ffmpeg auto-selects another format,
/// so a 10-bit pick for nvenc silently wrote an 8-bit file.</item>
/// </list>
/// </summary>
public class BurnInEncoderQualityTests
{
    private static string Generate(string videoEncoding, string crf, string pixelFormat)
    {
        return FfmpegGenerator.GenerateHardcodedVideoFile(
            "in.mkv", "sub.ass", "out.mkv", 1920, 1080, videoEncoding, string.Empty, pixelFormat,
            crf, "aac", false, "48000", string.Empty, string.Empty, string.Empty, string.Empty);
    }

    [Theory]
    [InlineData("h264_amf")]
    [InlineData("hevc_amf")]
    public void Amf_OffersOnlyNamedQualities(string codec)
    {
        Assert.True(VideoPresetOptions.IsAmf(codec));

        var qualities = VideoPresetOptions.GetAmfQualities();

        Assert.Equal(new[] { VideoPresetOptions.BlankTune, "quality", "balanced", "speed" }, qualities);
        Assert.DoesNotContain(qualities, p => int.TryParse(p, out _));
    }

    [Theory]
    [InlineData("libx264")]
    [InlineData("h264_nvenc")]
    [InlineData("h264_qsv")]
    [InlineData("prores_ks")]
    public void NonAmfCodecs_AreNotTreatedAsAmf(string codec)
    {
        Assert.False(VideoPresetOptions.IsAmf(codec));
    }

    [Theory]
    [InlineData("h264_amf", "0", "balanced")]
    [InlineData("h264_amf", "1", "speed")]
    [InlineData("h264_amf", "2", "quality")]
    [InlineData("hevc_amf", "0", "quality")]
    [InlineData("hevc_amf", "5", "balanced")]
    [InlineData("hevc_amf", "10", "speed")]
    public void StoredAmfNumber_MigratesToTheNameThatMeantTheSame(string codec, string stored, string expected)
    {
        Assert.Equal(expected, VideoPresetOptions.MigrateAmfQuality(codec, stored));
    }

    [Theory]
    [InlineData("3")]
    [InlineData("5")]
    [InlineData("10")]
    public void StoredH264AmfNumberFfmpegRejected_HasNoEquivalent(string stored)
    {
        // These never worked, so there is no picked quality to preserve - blank lets ffmpeg decide.
        Assert.Null(VideoPresetOptions.MigrateAmfQuality("h264_amf", stored));
    }

    [Theory]
    [InlineData("balanced")]
    [InlineData(" ")]
    [InlineData("")]
    public void AlreadyValidAmfQuality_IsLeftAlone(string stored)
    {
        Assert.Equal(stored, VideoPresetOptions.MigrateAmfQuality("h264_amf", stored));
    }

    [Fact]
    public void NonAmfQuality_IsLeftAlone()
    {
        Assert.Equal("23", VideoPresetOptions.MigrateAmfQuality("libx264", "23"));
    }

    [Theory]
    [InlineData("h264_amf")]
    [InlineData("hevc_amf")]
    public void Amf_QualityIsWrittenAsName(string videoEncoding)
    {
        var args = Generate(videoEncoding, "balanced", string.Empty);

        Assert.Contains("-quality balanced", args);
        Assert.DoesNotContain("-crf", args);
    }

    [Theory]
    [InlineData("h264_qsv")]
    [InlineData("hevc_qsv")]
    public void Qsv_UsesGlobalQualityNotCrf(string videoEncoding)
    {
        var args = Generate(videoEncoding, "23", string.Empty);

        Assert.Contains("-global_quality 23", args);
        Assert.DoesNotContain("-crf", args);
    }

    [Theory]
    [InlineData("libx264")]
    [InlineData("libx265")]
    [InlineData("libvpx-vp9")]
    public void SoftwareEncoders_StillUseCrf(string videoEncoding)
    {
        var args = Generate(videoEncoding, "23", string.Empty);

        Assert.Contains("-crf 23", args);
        Assert.DoesNotContain("-global_quality", args);
    }

    [Theory]
    [InlineData("h264_nvenc", "yuv420p", "yuv444p")]
    [InlineData("hevc_nvenc", "yuv420p", "yuv444p", "p010le")]
    [InlineData("h264_amf", "nv12")]
    [InlineData("hevc_amf", "nv12")]
    [InlineData("h264_qsv", "nv12")]
    [InlineData("hevc_qsv", "nv12", "p010le")]
    [InlineData("prores_ks", "yuv422p10le", "yuv444p10le")]
    public void HardwareEncoders_OfferOnlyFormatsTheySupport(string codec, params string[] expected)
    {
        var items = PixelFormatItem.GetPixelFormats(codec);

        Assert.Equal(PixelFormatItem.Blank, items[0].Codec);
        Assert.Equal(expected, items.Skip(1).Select(p => p.Codec));
    }

    [Theory]
    [InlineData("libx264")]
    [InlineData("libx265")]
    [InlineData("libvpx-vp9")]
    [InlineData("h264_videotoolbox")]
    [InlineData(null)]
    public void UnrestrictedEncoders_KeepTheFullPixelFormatList(string? codec)
    {
        var items = PixelFormatItem.GetPixelFormats(codec);

        Assert.Equal(
            new[] { PixelFormatItem.Blank, "yuv420p", "yuv422p", "yuv444p", "yuv420p10le", "yuv422p10le", "yuv444p10le" },
            items.Select(p => p.Codec));
    }

    [Fact]
    public void EveryOfferedPixelFormatHasAName()
    {
        var codecs = VideoEncodingItem.VideoEncodings.Select(p => p.Codec).Append(null);

        foreach (var codec in codecs)
        {
            Assert.All(PixelFormatItem.GetPixelFormats(codec), p => Assert.False(string.IsNullOrEmpty(p.Name)));
        }
    }

    [Theory]
    [InlineData("h264_qsv", "yuv420p", "nv12")]
    [InlineData("hevc_qsv", "yuv420p10le", "p010le")]
    [InlineData("h264_nvenc", "nv12", "yuv420p")]
    [InlineData("libx264", "yuv420p", "yuv420p")]
    public void StoredPixelFormat_MigratesToTheEncodersEquivalent(string codec, string stored, string expected)
    {
        Assert.Equal(expected, PixelFormatItem.Migrate(codec, stored));
    }

    [Theory]
    [InlineData("h264_nvenc", "yuv422p10le")]
    [InlineData("h264_amf", "yuv444p")]
    [InlineData("prores_ks", "yuv420p")]
    public void StoredPixelFormatTheEncoderCannotUse_HasNoEquivalent(string codec, string stored)
    {
        Assert.Null(PixelFormatItem.Migrate(codec, stored));
    }
}
