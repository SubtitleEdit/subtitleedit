using System.Text.Json;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// KugelAudio-0-Open: four preset voices embedded in the GGUF, picked per request with
/// <c>voice_id</c>, plus reference-audio cloning (<c>voice_ref</c>, audio.cpp v0.9.5) through a
/// separately downloaded acoustic encoder. No language option (the model reads the language from
/// the text). audio.cpp answers an unknown voice_id with HTTP 500, so these pin that SE only ever
/// sends one of the four.
/// </summary>
public class KugelAudioAudioCppTests
{
    [Fact]
    public void Engine_ClonesButHasNoLanguagePick()
    {
        var engine = new KugelAudioAudioCpp();

        Assert.False(engine.HasLanguageParameter);
        Assert.True(engine.HasModel);
        Assert.True(engine.SupportsVoiceCloning);
        Assert.True(engine.SupportsPerLineVoiceCloning);
        Assert.False(engine.ImportVoice("/voices/does-not-exist.wav"));
    }

    [Fact]
    public void Catalog_OffersItAsACloningEngine()
    {
        Assert.Contains(TtsEngineCatalog.CreateAll(null!), e => e is KugelAudioAudioCpp);
        Assert.Contains(TtsEngineCatalog.CreateVoiceCloningEngines(), e => e is KugelAudioAudioCpp);
    }

    [Fact]
    public async Task Voices_StartWithTheFourPresetsWhateverTheLanguage()
    {
        var voices = await new KugelAudioAudioCpp().GetVoices("da");

        Assert.Equal(
            new[] { "default", "clear", "english_male", "english_female" },
            voices.Take(4).Select(v => ((KugelAudioVoice)v.EngineVoice!).Voice).ToArray());
        Assert.Equal("German female", voices[0].Name);
        Assert.All(voices.Skip(4), v => Assert.IsType<IndexTtsVoice>(v.EngineVoice));
    }

    [Fact]
    public void PerLineCloneVoice_PointsStraightAtTheClip()
    {
        var engine = new KugelAudioAudioCpp();
        var voice = engine.MakePerLineCloneVoice("/clips/line-12.wav", "Line 12")!;

        Assert.Equal("/clips/line-12.wav", engine.GetPerLineReferenceClip(voice));
        Assert.Null(engine.GetPerLineReferenceClip(new Voice(new KugelAudioVoice("clear"))));
        Assert.Null(KugelAudioAudioCpp.GetReferencePath(null));
    }

    [Fact]
    public void BuildCloneSpeechPayload_SendsTheReferenceAsAPathAndNoPreset()
    {
        var json = JsonSerializer.Serialize(KugelAudioAudioCpp.BuildCloneSpeechPayload("Hej med dig.", "/voices/ada.wav"));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("kugelaudio", root.GetProperty("model").GetString());
        Assert.Equal("path", root.GetProperty("voice_ref").GetProperty("type").GetString());
        Assert.Equal("/voices/ada.wav", root.GetProperty("voice_ref").GetProperty("path").GetString());
        Assert.False(root.TryGetProperty("options", out _));
    }

    [Fact]
    public void BuildServerConfig_AddsEncoderAndSpecOnlyWhenCloningIsInstalled()
    {
        var withCloning = JsonSerializer.Serialize(KugelAudioAudioCpp.BuildServerConfig(1234, "metal", "/m/k.gguf", "/m/enc.safetensors", "/m/spec.json", true));
        using (var doc = JsonDocument.Parse(withCloning))
        {
            var model = doc.RootElement.GetProperty("models")[0];
            Assert.Equal("kugelaudio", model.GetProperty("family").GetString());
            Assert.Equal("tts", model.GetProperty("task").GetString());
            Assert.Equal("/m/spec.json", model.GetProperty("model_spec_override").GetString());
            Assert.Equal("/m/enc.safetensors", model.GetProperty("session_options").GetProperty("kugelaudio.reference_encoder_path").GetString());
        }

        var presetOnly = JsonSerializer.Serialize(KugelAudioAudioCpp.BuildServerConfig(1234, "metal", "/m/k.gguf", "/m/enc.safetensors", "/m/spec.json", false));
        using (var doc = JsonDocument.Parse(presetOnly))
        {
            var model = doc.RootElement.GetProperty("models")[0];
            Assert.False(model.TryGetProperty("model_spec_override", out _));
            Assert.False(model.TryGetProperty("session_options", out _));
        }
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
