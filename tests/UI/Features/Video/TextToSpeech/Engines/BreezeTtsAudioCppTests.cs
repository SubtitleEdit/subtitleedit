using System.Text.Json;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Breeze TTS 2 runs one audio.cpp <c>tts</c> model two ways: without a reference the instruction
/// designs the voice, with one it clones and the instruction directs the delivery. audio.cpp
/// refuses a clone without a non-empty <c>reference_text</c>, so these pin what SE sends.
/// </summary>
public class BreezeTtsAudioCppTests
{
    [Fact]
    public void Engine_ClonesAndHasNoLanguagePick()
    {
        var engine = new BreezeTtsAudioCpp();

        Assert.False(engine.HasLanguageParameter);
        Assert.True(engine.HasModel);
        Assert.True(engine.SupportsVoiceCloning);
        Assert.True(engine.SupportsPerLineVoiceCloning);
        Assert.Contains(TtsEngineCatalog.CreateVoiceCloningEngines(), e => e is BreezeTtsAudioCpp);
    }

    [Fact]
    public void BuildSpeechPayload_DefaultVoice_IsVoiceDesignWithoutReference()
    {
        var json = JsonSerializer.Serialize(BreezeTtsAudioCpp.BuildSpeechPayload(
            "Welcome.", string.Empty, null, "  A warm female narrator.  "));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("breezetts2", root.GetProperty("model").GetString());
        Assert.Equal("Welcome.", root.GetProperty("input").GetString());
        Assert.Equal("A warm female narrator.", root.GetProperty("options").GetProperty("instruction").GetString());
        Assert.False(root.GetProperty("options").TryGetProperty("reference_text", out _));
        Assert.False(root.TryGetProperty("voice_ref", out _));
    }

    [Fact]
    public void BuildSpeechPayload_NoInstructionAndNoReference_SendsNoOptions()
    {
        var json = JsonSerializer.Serialize(BreezeTtsAudioCpp.BuildSpeechPayload("Hi.", null, null, "   "));
        using var doc = JsonDocument.Parse(json);

        Assert.False(doc.RootElement.TryGetProperty("options", out _));
    }

    [Fact]
    public void BuildSpeechPayload_Clone_SendsReferenceTranscriptAndDirection()
    {
        var json = JsonSerializer.Serialize(BreezeTtsAudioCpp.BuildSpeechPayload(
            "Line to speak.", "/voices/ada.wav", " What the clip says. ", "Whisper it."));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var options = root.GetProperty("options");

        Assert.Equal("path", root.GetProperty("voice_ref").GetProperty("type").GetString());
        Assert.Equal("/voices/ada.wav", root.GetProperty("voice_ref").GetProperty("path").GetString());
        Assert.Equal("What the clip says.", options.GetProperty("reference_text").GetString());
        Assert.Equal("Whisper it.", options.GetProperty("instruction").GetString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void BuildSpeechPayload_CloneWithUnknownTranscript_SendsThePlaceholderNotTheLine(string? referenceText)
    {
        var json = JsonSerializer.Serialize(BreezeTtsAudioCpp.BuildSpeechPayload(
            "Line to speak.", "/voices/clip.wav", referenceText, null));
        using var doc = JsonDocument.Parse(json);

        var sent = doc.RootElement.GetProperty("options").GetProperty("reference_text").GetString();
        Assert.Equal(BreezeTtsAudioCpp.UnknownReferenceTextPlaceholder, sent);
        Assert.False(string.IsNullOrEmpty(sent));
    }

    [Theory]
    [InlineData(null, BreezeTtsAudioCpp.ModelQ8_0FileName)]
    [InlineData(BreezeTtsAudioCpp.ModelKeyQ8_0, BreezeTtsAudioCpp.ModelQ8_0FileName)]
    [InlineData(BreezeTtsAudioCpp.ModelKeyBf16, BreezeTtsAudioCpp.ModelBf16FileName)]
    [InlineData("something old", BreezeTtsAudioCpp.ModelQ8_0FileName)]
    public void GetModelFileName_DefaultsToQ8_0(string? modelKey, string expected)
    {
        Assert.Equal(expected, BreezeTtsAudioCpp.GetModelFileName(modelKey));
    }
}
