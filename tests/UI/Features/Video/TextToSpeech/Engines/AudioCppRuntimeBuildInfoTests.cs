using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The audio.cpp runtime is one binary shared by four engines, and each support-files build
/// only compiles the families it was asked for. The installer reads the archive's
/// BUILD-INFO.txt to see whether the installed build can serve the engine at all — without
/// that check an August build (index_tts2 only) looked "installed" to FireRedTTS3 and failed
/// at synthesis with "unsupported model family hint" until the user re-downloaded by hand.
/// </summary>
public class AudioCppRuntimeBuildInfoTests
{
    private const string BuildInfo2026_09_06 =
        "audio.cpp build for SubtitleEdit's IndexTTS-2.5 engine\n" +
        "source      : https://github.com/0xShug0/audio.cpp\n" +
        "ref         : b0757573c90bf3ada5cf8ffbc69f3ab80a7a6947\n" +
        "models      : index_tts2,higgs_audio_tts,fish_audio,fireredtts3\n" +
        "backend     : metal (arm64)\n" +
        "built       : 2026-09-06T07:33:34Z by Build audio.cpp IndexTTS release archives\n";

    [Fact]
    public void ParseBuiltModelFamilies_ReadsTheModelsLine()
    {
        var families = AudioCppRuntime.ParseBuiltModelFamilies(BuildInfo2026_09_06);

        Assert.NotNull(families);
        Assert.Equal(new[] { "index_tts2", "higgs_audio_tts", "fish_audio", "fireredtts3" }, families);
    }

    [Fact]
    public void ParseBuiltModelFamilies_AugustBuildLacksTheNewerFamilies()
    {
        var families = AudioCppRuntime.ParseBuiltModelFamilies(
            "audio.cpp build for SubtitleEdit's IndexTTS-2.5 engine\r\nmodels      : index_tts2\r\nbackend     : cpu\r\n");

        Assert.NotNull(families);
        Assert.Contains(IndexTts25AudioCpp.FamilyName, families);
        Assert.DoesNotContain(FireRedTts3AudioCpp.FamilyName, families);
        Assert.DoesNotContain(HiggsTtsAudioCpp.FamilyName, families);
    }

    [Fact]
    public void ParseBuiltModelFamilies_NoModelsLine_IsUnknown()
    {
        Assert.Null(AudioCppRuntime.ParseBuiltModelFamilies("source : x\nref : y\n"));
        Assert.Null(AudioCppRuntime.ParseBuiltModelFamilies(string.Empty));
    }

    [Fact]
    public void EveryAudioCppEngineFamily_IsInThePinnedBuild()
    {
        var families = AudioCppRuntime.ParseBuiltModelFamilies(BuildInfo2026_09_06)!;

        Assert.Contains(IndexTts25AudioCpp.FamilyName, families);
        Assert.Contains(HiggsTtsAudioCpp.FamilyName, families);
        Assert.Contains(FishTtsAudioCpp.FamilyName, families);
        Assert.Contains(FireRedTts3AudioCpp.FamilyName, families);
    }
}
