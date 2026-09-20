using Nikse.SubtitleEdit.Features.Video.BackgroundMusic;
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
    private const string BuildInfo2026_09_17 =
        "audio.cpp build for SubtitleEdit's IndexTTS-2.5 engine\n" +
        "source      : https://github.com/0xShug0/audio.cpp\n" +
        "ref         : 4af143229384fb6da3f373dc87de145ae954609b\n" +
        "models      : index_tts2,higgs_audio_tts,fish_audio,fireredtts3\n" +
        "backend     : metal (arm64)\n" +
        "built       : 2026-09-17T04:28:06Z by Build audio.cpp IndexTTS release archives\n";

    private const string BuildInfo2026_09_17b =
        "audio.cpp build for SubtitleEdit's IndexTTS-2.5 engine\n" +
        "source      : https://github.com/0xShug0/audio.cpp\n" +
        "ref         : 4af143229384fb6da3f373dc87de145ae954609b\n" +
        "models      : index_tts2,higgs_audio_tts,fish_audio,fireredtts3,ace_step\n" +
        "backend     : metal (arm64)\n";

    [Fact]
    public void ParseBuiltModelFamilies_ReadsTheModelsLine()
    {
        var families = AudioCppRuntime.ParseBuiltModelFamilies(BuildInfo2026_09_17);

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
        var families = AudioCppRuntime.ParseBuiltModelFamilies(BuildInfo2026_09_17b)!;

        Assert.Contains(IndexTts25AudioCpp.FamilyName, families);
        Assert.Contains(HiggsTtsAudioCpp.FamilyName, families);
        Assert.Contains(FishTtsAudioCpp.FamilyName, families);
        Assert.Contains(FireRedTts3AudioCpp.FamilyName, families);
        // Video > Generate background music and the TTS window's background music.
        Assert.Contains(AceStepAudioCpp.FamilyName, families);
    }

    [Fact]
    public void PreviousBuild_LacksAceStep_SoMusicAsksForTheUpdate()
    {
        Assert.False(AudioCppRuntime.ParseBuiltModelFamilies(BuildInfo2026_09_17)!.Contains(AceStepAudioCpp.FamilyName));
    }
}
