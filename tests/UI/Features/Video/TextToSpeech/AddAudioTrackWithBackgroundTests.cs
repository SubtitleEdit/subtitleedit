using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.TextToSpeech;

public class AddAudioTrackWithBackgroundTests
{
    [Fact]
    public void MixesTheSpeechOverTheBackgroundTrack_NotOverTheVideosOwnAudio()
    {
        using var process = FfmpegGenerator.AddAudioTrackWithBackground("in.mp4", "bg.wav", "tts.wav", "out.mp4", string.Empty, null, 100);
        var args = process.StartInfo.Arguments;

        Assert.Contains("-i \"in.mp4\" -i \"bg.wav\" -i \"tts.wav\"", args);
        Assert.Contains("[1:a]volume=1.00[bg];[bg][2:a]amix=inputs=2:duration=longest:normalize=0[aout]", args);
        Assert.DoesNotContain("[0:a]", args);
        Assert.Contains("-map 0:v:0 -map \"[aout]\" -c:v copy", args);
    }

    [Fact]
    public void DuckingVolume_AppliesToTheBackground()
    {
        using var process = FfmpegGenerator.AddAudioTrackWithBackground("in.mp4", "bg.wav", "tts.wav", "out.mp4", string.Empty, null, 15);

        Assert.Contains("[1:a]volume=0.15[bg]", process.StartInfo.Arguments);
    }

    [Fact]
    public void CopyEncoding_FallsBackToTheContainerDefault()
    {
        using var process = FfmpegGenerator.AddAudioTrackWithBackground("in.mp4", "bg.wav", "tts.wav", "out.mp4", "copy", null, 100);

        Assert.DoesNotContain("-c:a", process.StartInfo.Arguments);
    }
}
