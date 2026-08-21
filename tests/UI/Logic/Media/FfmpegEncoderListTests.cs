using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class FfmpegEncoderListTests
{
    // Trimmed "ffmpeg -encoders" output: the legend block, the separator, then real entries.
    // This is the shape of what the Flatpak's own ffmpeg prints - no x265, no NVENC/AMF/QSV.
    private const string FlatpakEncodersOutput = """
        Encoders:
         V..... = Video
         A..... = Audio
         S..... = Subtitle
         .F.... = Frame-level multithreading
         ..S... = Slice-level multithreading
         ...X.. = Codec is experimental
         ....B. = Supports draw_horiz_band
         .....D = Supports direct rendering method 1
         ------
         V....D libx264              libx264 H.264 / AVC / MPEG-4 AVC (codec h264)
         V....D libvpx-vp9           libvpx VP9 (codec vp9)
         V..... prores_ks            Apple ProRes (iCodec Pro) (codec prores)
         V..... h264_vaapi           H.264/AVC (VAAPI) (codec h264)
         V..... hevc_vaapi           H.265/HEVC (VAAPI) (codec hevc)
         V..... png                  PNG (Portable Network Graphics) image
         A....D aac                  AAC (Advanced Audio Coding)
         S..... srt                  SubRip subtitle (codec subrip)
        """;

    [Fact]
    public void ParsesEncoderNames()
    {
        var names = FfmpegHelper.ParseEncoderNames(FlatpakEncodersOutput);

        Assert.Contains("libx264", names);
        Assert.Contains("libvpx-vp9", names);
        Assert.Contains("prores_ks", names);
        Assert.Contains("hevc_vaapi", names);
        Assert.Contains("aac", names);
        Assert.Contains("srt", names);
        Assert.DoesNotContain("libx265", names);
    }

    [Fact]
    public void SkipsTheLegendLines()
    {
        var names = FfmpegHelper.ParseEncoderNames(FlatpakEncodersOutput);

        // " V..... = Video" and friends match the flag columns but have "=" where a name goes.
        Assert.DoesNotContain("=", names);
        Assert.DoesNotContain("Video", names);
        Assert.DoesNotContain("Encoders:", names);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("ffmpeg: command not found")]
    public void ReturnsNothingForUnusableOutput(string? output)
    {
        Assert.Empty(FfmpegHelper.ParseEncoderNames(output!));
    }

    [Fact]
    public void HidesTheCodecsTheBundledFfmpegLacks()
    {
        var offered = new List<VideoEncodingItem>
        {
            new("libx264", "H.264/AVC (CPU)"),
            new("libx265", "H.265/HEVC (CPU)"),
            new("libvpx-vp9", "VP9 (CPU)"),
            new("prores_ks", "ProRes (CPU)"),
            new("h264_nvenc", "H.264/AVC (NVIDIA GPU)"),
            new("hevc_qsv", "H.265/HEVC (Intel QSV)"),
        };

        var unsupported = VideoEncodingItem.GetUnsupported(offered, FfmpegHelper.ParseEncoderNames(FlatpakEncodersOutput));

        Assert.Equal(new[] { "libx265", "h264_nvenc", "hevc_qsv" }, unsupported.Select(p => p.Codec));
    }

    [Fact]
    public void HidesNothingWhenTheProbeFailed()
    {
        var offered = new List<VideoEncodingItem> { new("libx264", "H.264/AVC (CPU)") };

        // An empty set means "we could not ask ffmpeg", not "ffmpeg has no encoders".
        Assert.Empty(VideoEncodingItem.GetUnsupported(offered, new HashSet<string>()));
    }

    [Fact]
    public void HidesNothingWhenEveryCodecWouldGo()
    {
        var offered = new List<VideoEncodingItem>
        {
            new("libx264", "H.264/AVC (CPU)"),
            new("libx265", "H.265/HEVC (CPU)"),
        };

        // Rather than leave the user an empty combo box, keep the old behaviour - something is
        // wrong with the probe, not with every single codec.
        Assert.Empty(VideoEncodingItem.GetUnsupported(offered, new HashSet<string> { "aac" }));
    }
}
