using System.Linq;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

public class KokoroTtsCppTests
{
    [Fact]
    public async Task GetVoices_Returns_BakedVoiceList()
    {
        var engine = new KokoroTtsCpp();
        var voices = await engine.GetVoices(string.Empty);

        Assert.NotEmpty(voices);
        // The baked list ships with all 103 voices that live in voices-v1.1-zh.bin.
        Assert.Equal(103, voices.Length);

        // Spot-check the default + one each of American Female, British Female,
        // Mandarin Female, Mandarin Male.
        var voiceNames = voices.Select(v => ((KokoroTtsVoice)v.EngineVoice!).Voice).ToList();
        Assert.Contains(KokoroTtsCpp.DefaultVoice, voiceNames);
        Assert.Contains("af_maple", voiceNames);
        Assert.Contains("bf_vale", voiceNames);
        Assert.Contains("zf_001", voiceNames);
        Assert.Contains("zm_009", voiceNames);
    }

    [Fact]
    public void Engine_HasNoApiKey_NoRegion_NoModelDropdown()
    {
        var engine = new KokoroTtsCpp();
        Assert.False(engine.HasApiKey);
        Assert.False(engine.HasRegion);
        Assert.False(engine.HasModel);
        Assert.False(engine.HasLanguageParameter);
        Assert.False(engine.HasKeyFile);
    }

    [Fact]
    public void ImportVoice_NotSupported()
    {
        // Kokoro uses fixed pre-trained voice style vectors and cannot clone.
        var engine = new KokoroTtsCpp();
        Assert.False(engine.ImportVoice("anything.wav"));
    }
}
