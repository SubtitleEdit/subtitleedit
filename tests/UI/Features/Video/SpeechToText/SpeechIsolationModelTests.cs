using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText;

public class SpeechIsolationModelTests
{
    [Fact]
    public void SeparateArguments_WriteOnlyTheSpeechStemIntoTheGivenFolder()
    {
        var args = SpeechIsolationModel.BuildSeparateArguments("/models/mbr.gguf", "/tmp/run 1/audio.wav", "/tmp/run 1");

        Assert.Equal("-m \"/models/mbr.gguf\" -f \"/tmp/run 1/audio.wav\" --separate --stems vocals --sep-output-dir \"/tmp/run 1\"", args);
    }

    [Fact]
    public void SpeechStem_IsNamedAfterTheInputFile()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-stt-run");

        var stem = SpeechIsolationModel.GetSpeechStemFileName(Path.Combine(folder, "0f3a.wav"), folder);

        Assert.Equal(Path.Combine(folder, "0f3a_vocals.wav"), stem);
    }

    [Fact]
    public void BackgroundStem_IsEverythingButTheSpeech()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-tts-separation");
        var input = Path.Combine(folder, "original.wav");

        var args = SpeechIsolationModel.BuildSeparateArguments("/models/mbr.gguf", input, folder, SpeechIsolationModel.BackgroundStem);
        var stem = SpeechIsolationModel.GetStemFileName(input, folder, SpeechIsolationModel.BackgroundStem);

        Assert.Contains("--separate --stems other ", args);
        Assert.Equal(Path.Combine(folder, "original_other.wav"), stem);
    }

    [Fact]
    public void Downmix_GivesTheEnginesSixteenKilohertzMono()
    {
        var args = SpeechIsolationModel.BuildDownmixArguments("/tmp/a_vocals.wav", "/tmp/b.wav");

        Assert.Contains("-ar 16000 -ac 1", args);
        Assert.StartsWith("-nostdin -y -i \"/tmp/a_vocals.wav\"", args);
        Assert.EndsWith("\"/tmp/b.wav\"", args);
    }
}
