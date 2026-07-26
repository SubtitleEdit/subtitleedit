using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

[Collection(TtsSettingsCollection.Name)]
public class MossTtsCrispAsrTests
{
    [Fact]
    public void BuildSpeakPayload_DoesNotSendVoiceField()
    {
        // #12757: a per-request `voice` (bare name) makes the moss-tts backend fail to load the
        // reference and fall back to zero-shot (random/male-female-flipping voice). The clone must
        // come from the server's startup --voice flag, so the payload must NOT carry `voice`.
        var payload = MossTtsCrispAsr.BuildSpeakPayload("hello", 1.0);

        Assert.False(payload.ContainsKey("voice"));
    }

    [Fact]
    public void BuildSpeakPayload_DoesNotSendRefTextField()
    {
        // ref-text is likewise supplied via the startup --ref-text flag, not per request.
        var payload = MossTtsCrispAsr.BuildSpeakPayload("hello", 1.0);

        Assert.False(payload.ContainsKey("ref_text"));
    }

    [Fact]
    public void BuildSpeakPayload_DoesNotSendSeed()
    {
        // No seed: the reference conditioning keeps the speaker consistent, and letting the server
        // re-roll each call means a bad render of a short line comes out clean on regenerate rather
        // than being locked in permanently by a fixed seed (#12757 follow-up).
        var payload = MossTtsCrispAsr.BuildSpeakPayload("hello", 1.0);

        Assert.False(payload.ContainsKey("seed"));
    }

    [Fact]
    public void BuildSpeakPayload_CarriesCoreFields()
    {
        using var _ = new AcceptVoiceCloningScope(true);

        var payload = MossTtsCrispAsr.BuildSpeakPayload("hello world", 1.25);

        Assert.Equal("hello world", payload["input"]);
        Assert.Equal("wav", payload["response_format"]);
        Assert.Equal(1.25, payload["speed"]);
        Assert.Equal(false, payload["spoken_disclaimer"]);
        Assert.True(payload.ContainsKey("consent_attestation"));
    }

    [Fact]
    public void BuildSpeakPayload_WhenVoiceCloningAccepted_SendsMarkingAttestation()
    {
        // CrispASR v0.8.22+ rejects "spoken_disclaimer": false on a clone without this field.
        using var _ = new AcceptVoiceCloningScope(true);

        var payload = MossTtsCrispAsr.BuildSpeakPayload("hello", 1.0);

        Assert.True(payload.ContainsKey("marking_attestation"));
    }

    [Fact]
    public void BuildSpeakPayload_WhenVoiceCloningNotAccepted_SendsNoAttestations()
    {
        using var _ = new AcceptVoiceCloningScope(false);

        var payload = MossTtsCrispAsr.BuildSpeakPayload("hello", 1.0);

        Assert.False(payload.ContainsKey("consent_attestation"));
        Assert.False(payload.ContainsKey("marking_attestation"));
        Assert.False(payload.ContainsKey("spoken_disclaimer"));
        Assert.Equal("hello", payload["input"]);
    }
}
