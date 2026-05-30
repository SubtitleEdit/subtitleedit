using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class FfmpegGeneratorExtractAudioTests
{
    [Fact]
    public void ExtractAudio_Defaults_ReproduceSttClipCommand()
    {
        // The STT audio-clip callers rely on the historical 16 kHz / 32k behavior;
        // the defaults must keep producing exactly that so transcription is unaffected.
        var args = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            "video.mp4", 1.0, 2.0, useCenterChannelOnly: false, "clip.wav");

        Assert.Contains("-vn -ar 16000 -b:a 32k", args);
        Assert.Contains("\"clip.wav\"", args);
    }

    [Fact]
    public void ExtractAudio_SampleRateZero_OmitsArSoSourceRateIsKept()
    {
        // Issue #11235: passing 0 must NOT force a sample rate (no -ar), so ffmpeg
        // keeps the source rate instead of downsampling to 16 kHz.
        var args = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            "video.mp4", 0.0, 1.5, useCenterChannelOnly: false, "clip.wav",
            audioTrackFfIndex: -1, sampleRate: 0, audioBitRate: "");

        Assert.DoesNotContain("-ar", args);
        Assert.DoesNotContain("-b:a", args);
        Assert.Contains("-vn", args);
    }

    [Fact]
    public void ExtractAudio_Mp3WithBitrate_KeepsSourceRateAndSetsBitrate()
    {
        // MP3 export at source sample rate with a chosen bitrate (issue #11237).
        var args = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            "video.mp4", 0.0, 1.5, useCenterChannelOnly: false, "clip.mp3",
            audioTrackFfIndex: -1, sampleRate: 0, audioBitRate: "192k");

        Assert.DoesNotContain("-ar", args);
        Assert.Contains("-b:a 192k", args);
        Assert.Contains("\"clip.mp3\"", args);
    }

    [Fact]
    public void ExtractAudio_ExplicitSampleRate_EmitsAr()
    {
        var args = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            "video.mp4", 0.0, 1.5, useCenterChannelOnly: false, "clip.flac",
            audioTrackFfIndex: -1, sampleRate: 48000, audioBitRate: "");

        Assert.Contains("-ar 48000", args);
        Assert.DoesNotContain("-b:a", args);
    }
}
