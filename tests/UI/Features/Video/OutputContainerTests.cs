using Nikse.SubtitleEdit.Features.Video.BurnIn;

namespace UITests.Features.Video;

/// <summary>
/// The burn-in codec/container allow-list. The pairs asserted here were verified against ffmpeg:
/// the "silent" ones matter most - VP9 or ProRes video and Vorbis audio mux into MPEG-TS with
/// exit code 0 but end up as an unplayable "bin_data" stream, so nothing but the encode log
/// would tell the user the file is broken.
/// </summary>
public class OutputContainerTests
{
    [Theory]
    [InlineData("libx264")]
    [InlineData("libx265")]
    [InlineData("h264_nvenc")]
    [InlineData("hevc_qsv")]
    [InlineData("h264_videotoolbox")]
    public void H264AndH265_CanUseMpegTs(string codec)
    {
        var extensions = OutputContainer.GetExtensions(codec);

        Assert.Contains(".ts", extensions);
        Assert.Contains(".mp4", extensions);
        Assert.Contains(".mov", extensions);
        Assert.DoesNotContain(".webm", extensions);
    }

    [Fact]
    public void Vp9_CanUseWebm_ButNotMpegTsOrQuickTime()
    {
        var extensions = OutputContainer.GetExtensions("libvpx-vp9");

        Assert.Contains(".webm", extensions);
        Assert.DoesNotContain(".ts", extensions);
        Assert.DoesNotContain(".mov", extensions);
    }

    [Theory]
    [InlineData("prores_ks")]
    [InlineData("prores_videotoolbox")]
    public void ProRes_IsQuickTimeOrMatroskaOnly(string codec)
    {
        var extensions = OutputContainer.GetExtensions(codec);

        Assert.Equal(new[] { ".mov", ".mkv" }, extensions);
    }

    [Theory]
    [InlineData("libx264")]
    [InlineData("libvpx-vp9")]
    [InlineData("prores_ks")]
    [InlineData("")]
    public void Matroska_IsAlwaysOffered(string codec)
    {
        // MakeOutputFileName falls back to ".mkv" when the stored extension is not in the list.
        Assert.Contains(OutputContainer.DefaultExtension, OutputContainer.GetExtensions(codec));
    }

    [Fact]
    public void Webm_TakesOpusOrVorbisOnly()
    {
        var audioEncodings = OutputContainer.GetAudioEncodings(".webm");

        Assert.Equal(new[] { "libopus", "libvorbis" }, audioEncodings);
    }

    [Fact]
    public void MpegTs_HasNoVorbis_AndQuickTimeHasNoOpus()
    {
        Assert.DoesNotContain("libvorbis", OutputContainer.GetAudioEncodings(".ts"));
        Assert.Contains("libopus", OutputContainer.GetAudioEncodings(".ts"));

        Assert.DoesNotContain("libopus", OutputContainer.GetAudioEncodings(".mov"));
        Assert.Contains("libvorbis", OutputContainer.GetAudioEncodings(".mov"));
    }

    [Theory]
    [InlineData(".mkv")]
    [InlineData(".mp4")]
    [InlineData(".mov")]
    [InlineData(".ts")]
    public void AudioCopy_IsOfferedWhereverTheContainerCanHoldTheSourceCodec(string extension)
    {
        Assert.Contains(OutputContainer.AudioEncodingCopy, OutputContainer.GetAudioEncodings(extension));
    }

    [Fact]
    public void AudioEncodingFor_FallsBackToSomethingTheContainerCanHold()
    {
        // "copy" would abort a WebM encode for anything but an Opus/Vorbis source.
        Assert.Equal("libopus", OutputContainer.GetAudioEncodingFor(".webm", "copy"));
        Assert.Equal("libvorbis", OutputContainer.GetAudioEncodingFor(".webm", "libvorbis"));
        // Vorbis has no usable MPEG-TS mapping; the container's own default ("copy") takes over.
        Assert.Equal("copy", OutputContainer.GetAudioEncodingFor(".ts", "libvorbis"));
        Assert.Equal("copy", OutputContainer.GetAudioEncodingFor(".mkv", "copy"));
    }

    [Theory]
    [InlineData("opus", "libopus")]
    [InlineData("vorbis", "libvorbis")]
    [InlineData("aac", "aac")]
    [InlineData("copy", "copy")]
    public void NativeOpusAndVorbis_AreMappedToTheirLibraryEncoders(string stored, string expected)
    {
        // ffmpeg's built-in "opus"/"vorbis" encoders are experimental and abort the encode with
        // "add '-strict -2'", so settings saved with those names have to move over.
        Assert.Equal(expected, OutputContainer.MigrateAudioEncoding(stored));
    }

    [Theory]
    [InlineData(".mkv", "matroska")]
    [InlineData(".ts", "mpegts")]
    [InlineData(".webm", "webm")]
    [InlineData(".mp4", "mp4")]
    [InlineData(".mov", "mov")]
    public void MuxerName_IsTheFfmpegFormatName(string extension, string expected)
    {
        // Used for "-f" in the two-pass analysis pass, which writes to the null device: "-f ts"
        // or "-f mkv" are not ffmpeg format names and abort the pass.
        Assert.Equal(expected, OutputContainer.GetMuxerName(extension));
    }
}
