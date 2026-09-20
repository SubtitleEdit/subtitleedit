using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class SpeechOnlyWaveformTests
{
    [Fact]
    public void PeakFile_SitsNextToTheNormalOne_SoBothStayCached()
    {
        var normal = Path.Combine(Path.GetTempPath(), "Waveforms", "abc123-1.wav");

        var speech = SpeechOnlyWaveform.GetPeakFileName(normal);

        Assert.Equal(Path.Combine(Path.GetTempPath(), "Waveforms", "speech-abc123-1.wav"), speech);
    }

    [Fact]
    public void PeakFile_IsNotMistakenForAnotherAudioTrackOfTheSameVideo()
    {
        // GetPeakWaveFileName falls back to the first "<hash>-*.wav" when no track is given.
        var speech = Path.GetFileName(SpeechOnlyWaveform.GetPeakFileName(Path.Combine("w", "abc123-1.wav")));

        Assert.False(speech.StartsWith("abc123-", StringComparison.Ordinal));
    }

    [Fact]
    public void Extract_PicksTheSelectedAudioTrack()
    {
        var args = SpeechOnlyWaveform.BuildExtractArguments("/v/film.mkv", 2, "/tmp/audio.wav");

        Assert.Equal("-nostdin -y -i \"/v/film.mkv\" -vn -map 0:2? -ar 16000 -ac 1 \"/tmp/audio.wav\"", args);
    }

    [Fact]
    public void Extract_WithoutATrack_LetsFfmpegChoose()
    {
        var args = SpeechOnlyWaveform.BuildExtractArguments("/v/film.mkv", -1, "/tmp/audio.wav");

        Assert.DoesNotContain("-map", args);
    }
}
