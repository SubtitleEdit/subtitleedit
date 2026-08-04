using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Chatterbox is the one SE engine the crispasr server classifies as voice cloning, because its
/// <c>voice</c> field keeps the <c>.wav</c> extension — the server's literal clone test. That makes
/// it the engine that hard-requires both attestations, so its payload is worth pinning down.
/// </summary>
[Collection(TtsSettingsCollection.Name)]
public class ChatterboxTtsCppTests
{
    [Fact]
    public void BuildSpeakPayload_KeepsWavExtensionOnVoiceName()
    {
        // The chatterbox backend does not append an extension, and the server needs the .wav to
        // recognise the request as cloning at all.
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", "/voices/Arnold.wav");

        Assert.Equal("Arnold.wav", payload["voice"]);
    }

    [Fact]
    public void BuildSpeakPayload_SendsBareFileNameNotPath()
    {
        // A path separator is rejected outright with HTTP 400 invalid_voice.
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", Path.Combine("some", "dir", "Arnold.wav"));

        Assert.Equal("Arnold.wav", payload["voice"]);
    }

    [Fact]
    public void BuildSpeakPayload_WhenCloningAccepted_SendsBothAttestations()
    {
        using var _ = new AcceptVoiceCloningScope(true);

        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", "/voices/Arnold.wav");

        Assert.True(payload.ContainsKey("consent_attestation"));
        Assert.True(payload.ContainsKey("marking_attestation"));
        Assert.Equal(false, payload["spoken_disclaimer"]);
    }

    [Fact]
    public void BuildSpeakPayload_WhenCloningNotAccepted_SendsNoAttestations()
    {
        using var _ = new AcceptVoiceCloningScope(false);

        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", "/voices/Arnold.wav");

        Assert.Equal("Arnold.wav", payload["voice"]);
        Assert.False(payload.ContainsKey("consent_attestation"));
        Assert.False(payload.ContainsKey("marking_attestation"));
        Assert.False(payload.ContainsKey("spoken_disclaimer"));
    }

    [Fact]
    public void BuildSpeakPayload_WithBakedDefaultVoice_IsNotCloning()
    {
        // No reference WAV means no cloning, so neither a `voice` field nor an attestation.
        using var _ = new AcceptVoiceCloningScope(true);

        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", string.Empty);

        Assert.False(payload.ContainsKey("voice"));
        Assert.False(payload.ContainsKey("consent_attestation"));
        Assert.False(payload.ContainsKey("marking_attestation"));
        Assert.Equal("hello", payload["input"]);
    }
}
