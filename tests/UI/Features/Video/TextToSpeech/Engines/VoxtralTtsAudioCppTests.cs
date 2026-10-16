using System.Text.Json;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Voxtral 4B TTS is a fixed-voice audio.cpp engine: 20 presets embedded in the GGUF, picked per
/// request with <c>voice_id</c>, and no language option (the model reads the language from the
/// text). The weights are CC BY-NC 4.0, so unlike KugelAudio it has a licence gate.
/// </summary>
public class VoxtralTtsAudioCppTests
{
    [Fact]
    public void Engine_IsAFixedVoiceEngineWithoutALanguagePick()
    {
        var engine = new VoxtralTtsAudioCpp();

        Assert.False(engine.HasLanguageParameter);
        Assert.True(engine.HasModel);
        Assert.False(engine.SupportsVoiceCloning);
        Assert.False(engine.SupportsPerLineVoiceCloning);
        Assert.False(engine.ImportVoice("/voices/ada.wav"));
    }

    [Fact]
    public void Catalog_OffersItButNotAsACloningEngine()
    {
        Assert.Contains(TtsEngineCatalog.CreateAll(null!), e => e is VoxtralTtsAudioCpp);
        Assert.DoesNotContain(TtsEngineCatalog.CreateVoiceCloningEngines(), e => e is VoxtralTtsAudioCpp);
    }

    [Fact]
    public async Task Voices_AreTheTwentyPresetsWithReadableUniqueNames()
    {
        var voices = await new VoxtralTtsAudioCpp().GetVoices("fr");
        var ids = voices.Select(v => ((VoxtralTtsVoice)v.EngineVoice!).Voice).ToArray();

        Assert.Equal(20, ids.Length);
        Assert.Equal(VoxtralTtsAudioCpp.PresetVoices, ids);
        Assert.Contains(VoxtralTtsAudioCpp.DefaultVoice, ids);
        Assert.Equal("English - casual female", voices[0].Name);
        Assert.Contains(voices, v => v.Name == "French - female");
        Assert.Equal(voices.Length, voices.Select(v => v.Name).Distinct().Count());
        Assert.DoesNotContain(voices, v => v.Name.Contains('_'));
    }

    [Theory]
    [InlineData("fr_male", "fr_male")]
    [InlineData("AR_MALE", "ar_male")]
    [InlineData("ar_female", VoxtralTtsAudioCpp.DefaultVoice)]
    [InlineData("", VoxtralTtsAudioCpp.DefaultVoice)]
    public void ResolveVoiceId_OnlyPassesOnPresetsTheServerKnows(string id, string expected)
    {
        Assert.Equal(expected, VoxtralTtsAudioCpp.ResolveVoiceId(new Voice(new VoxtralTtsVoice(id))));
    }

    [Fact]
    public void ResolveVoiceId_ForeignEngineVoiceFallsBackToTheDefault()
    {
        Assert.Equal(VoxtralTtsAudioCpp.DefaultVoice, VoxtralTtsAudioCpp.ResolveVoiceId(new Voice(new KugelAudioVoice("default"))));
        Assert.Equal(VoxtralTtsAudioCpp.DefaultVoice, VoxtralTtsAudioCpp.ResolveVoiceId(null));
    }

    [Fact]
    public void BuildSpeechPayload_SendsThePresetAsVoiceIdAndNoLanguage()
    {
        var json = JsonSerializer.Serialize(VoxtralTtsAudioCpp.BuildSpeechPayload("Bonjour à tous.", "fr_female"));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("voxtraltts", root.GetProperty("model").GetString());
        Assert.Equal("Bonjour à tous.", root.GetProperty("input").GetString());
        Assert.Equal("fr_female", root.GetProperty("options").GetProperty("voice_id").GetString());
        Assert.False(root.GetProperty("options").TryGetProperty("language", out _));
        Assert.False(root.TryGetProperty("voice_ref", out _));
    }

    [Theory]
    [InlineData(VoxtralTtsAudioCpp.ModelKeyQ8_0, VoxtralTtsAudioCpp.ModelQ8_0FileName)]
    [InlineData(VoxtralTtsAudioCpp.ModelKeyBf16, VoxtralTtsAudioCpp.ModelBf16FileName)]
    [InlineData("something old", VoxtralTtsAudioCpp.ModelQ8_0FileName)]
    public void GetModelFileName_DefaultsToQ8_0(string modelKey, string expected)
    {
        Assert.Equal(expected, VoxtralTtsAudioCpp.GetModelFileName(modelKey));
    }

    [Fact]
    public void License_IsGated()
    {
        Assert.Equal("Voxtral TTS - model license", VoxtralTtsAudioCpp.LicenseDefinition.DialogTitle);
        Assert.Contains("CC BY-NC 4.0", VoxtralTtsAudioCpp.LicenseDefinition.AcceptCheckBoxText);
    }
}
