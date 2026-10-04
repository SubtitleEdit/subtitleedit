using System.Text.Json;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// KugelAudio-0-Open is the audio.cpp engine that does not clone: four preset voices embedded in
/// the GGUF, picked per request with <c>voice_id</c>, and no language option (the model reads the
/// language from the text). audio.cpp answers an unknown voice_id with HTTP 500, so these pin that
/// SE only ever sends one of the four.
/// </summary>
public class KugelAudioAudioCppTests
{
    [Fact]
    public void Engine_IsAFixedVoiceEngineWithoutALanguagePick()
    {
        var engine = new KugelAudioAudioCpp();

        Assert.False(engine.HasLanguageParameter);
        Assert.True(engine.HasModel);
        Assert.False(engine.SupportsVoiceCloning);
        Assert.False(engine.SupportsPerLineVoiceCloning);
        Assert.False(engine.ImportVoice("/voices/ada.wav"));
    }

    [Fact]
    public void Catalog_OffersItButNotAsACloningEngine()
    {
        Assert.Contains(TtsEngineCatalog.CreateAll(null!), e => e is KugelAudioAudioCpp);
        Assert.DoesNotContain(TtsEngineCatalog.CreateVoiceCloningEngines(), e => e is KugelAudioAudioCpp);
    }

    [Fact]
    public async Task Voices_AreTheFourPresetsWhateverTheLanguage()
    {
        var voices = await new KugelAudioAudioCpp().GetVoices("da");

        Assert.Equal(
            new[] { "default", "clear", "english_male", "english_female" },
            voices.Select(v => ((KugelAudioVoice)v.EngineVoice!).Voice).ToArray());
        Assert.Equal("German female", voices[0].Name);
        Assert.Equal(voices.Length, voices.Select(v => v.Name).Distinct().Count());
    }

    [Theory]
    [InlineData("english_male", "english_male")]
    [InlineData("Clear", "clear")]
    [InlineData("nope", KugelAudioAudioCpp.DefaultVoice)]
    [InlineData("", KugelAudioAudioCpp.DefaultVoice)]
    public void ResolveVoiceId_OnlyPassesOnPresetsTheServerKnows(string id, string expected)
    {
        Assert.Equal(expected, KugelAudioAudioCpp.ResolveVoiceId(new Voice(new KugelAudioVoice(id))));
    }

    [Fact]
    public void ResolveVoiceId_ForeignEngineVoiceFallsBackToTheDefault()
    {
        Assert.Equal(KugelAudioAudioCpp.DefaultVoice, KugelAudioAudioCpp.ResolveVoiceId(new Voice(new KokoroVoice("af_maple"))));
        Assert.Equal(KugelAudioAudioCpp.DefaultVoice, KugelAudioAudioCpp.ResolveVoiceId(null));
    }

    [Fact]
    public void BuildSpeechPayload_SendsThePresetAsVoiceIdAndNoLanguage()
    {
        var json = JsonSerializer.Serialize(KugelAudioAudioCpp.BuildSpeechPayload("Hej med dig.", "clear"));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("kugelaudio", root.GetProperty("model").GetString());
        Assert.Equal("Hej med dig.", root.GetProperty("input").GetString());
        Assert.Equal("clear", root.GetProperty("options").GetProperty("voice_id").GetString());
        Assert.False(root.GetProperty("options").TryGetProperty("language", out _));
        Assert.False(root.TryGetProperty("voice_ref", out _));
    }

    [Theory]
    [InlineData(null, KugelAudioAudioCpp.ModelQ4_KFileName)]
    [InlineData(KugelAudioAudioCpp.ModelKeyQ4_K, KugelAudioAudioCpp.ModelQ4_KFileName)]
    [InlineData(KugelAudioAudioCpp.ModelKeyQ8_0, KugelAudioAudioCpp.ModelQ8_0FileName)]
    [InlineData("something old", KugelAudioAudioCpp.ModelQ4_KFileName)]
    public void GetModelFileName_DefaultsToQ4_K(string? modelKey, string expected)
    {
        Assert.Equal(expected, KugelAudioAudioCpp.GetModelFileName(modelKey));
    }
}
