using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// ffmpeg marks its TrueHD encoder as experimental and refuses to run it without "-strict -2",
/// leaving a 0-byte output - so picking TrueHD never added the dub to the video (#15020).
/// </summary>
public class AddAudioTrackExperimentalEncoderTests
{
    [Fact]
    public void AddAudioTrack_TrueHd_EnablesExperimentalEncoders()
    {
        using var process = FfmpegGenerator.AddAudioTrack("in.mp4", "tts.wav", "out.mp4", "truehd", true);

        Assert.Contains(" -c:a truehd -strict -2 -ac 2 \"out.mp4\"", process.StartInfo.Arguments);
    }

    [Fact]
    public void AddAudioTrackWithDucking_TrueHd_EnablesExperimentalEncoders()
    {
        using var process = FfmpegGenerator.AddAudioTrackWithDucking("in.mp4", "tts.wav", "out.mp4", "truehd", null, 30);

        Assert.Contains(" -c:v copy -c:a truehd -strict -2 \"out.mp4\"", process.StartInfo.Arguments);
    }

    [Fact]
    public void AddAudioTrackWithBackground_TrueHd_EnablesExperimentalEncoders()
    {
        using var process = FfmpegGenerator.AddAudioTrackWithBackground("in.mp4", "bg.wav", "tts.wav", "out.mp4", "truehd", null, 30);

        Assert.Contains(" -c:v copy -c:a truehd -strict -2 \"out.mp4\"", process.StartInfo.Arguments);
    }

    [Theory]
    [InlineData("aac")]
    [InlineData("ac3")]
    [InlineData("eac3")]
    public void AddAudioTrackWithDucking_StableEncoder_IsLeftAlone(string encoding)
    {
        using var process = FfmpegGenerator.AddAudioTrackWithDucking("in.mp4", "tts.wav", "out.mp4", encoding, null, 30);

        Assert.Contains($" -c:v copy -c:a {encoding} \"out.mp4\"", process.StartInfo.Arguments);
        Assert.DoesNotContain("-strict", process.StartInfo.Arguments);
    }
}
