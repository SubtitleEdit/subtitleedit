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

    [Theory]
    [InlineData(-1.5)] // end before start: ffmpeg 9.0.2 rejects a negative -t, older ones fail in atrim
    [InlineData(0.0)]  // "-t 0.000" means "no limit" - the clip would be the whole rest of the file
    [InlineData(0.0004)] // rounds to "0.000"
    [InlineData(double.NaN)]
    public void ExtractAudio_NoDuration_NeverReachesFfmpegAsZeroOrNegative(double durationSeconds)
    {
        Assert.False(FfmpegGenerator.HasClipDuration(durationSeconds));

        var args = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            "video.mp4", 10.0, durationSeconds, useCenterChannelOnly: false, "clip.wav");

        Assert.Contains("-ss 10.000 -t 0.001 -i", args);
    }

    [Fact]
    public void ExtractAudio_ShortButRealDuration_IsKept()
    {
        Assert.True(FfmpegGenerator.HasClipDuration(0.04));

        var args = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            "video.mp4", 10.0, 0.04, useCenterChannelOnly: false, "clip.wav");

        Assert.Contains("-t 0.040 -i", args);
    }

    [Fact]
    public void OutputTail_KeepsOnlyTheLastLines()
    {
        var tail = new FfmpegOutputTail();
        for (var i = 1; i <= 40; i++)
        {
            tail.Add("line " + i);
        }

        var lines = tail.ToString().Split(Environment.NewLine);

        Assert.Equal(15, lines.Length);
        Assert.Equal("line 26", lines[0]);
        Assert.Equal("line 40", lines[^1]);
    }
}
