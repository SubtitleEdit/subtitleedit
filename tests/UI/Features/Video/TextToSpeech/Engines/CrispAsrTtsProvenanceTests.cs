using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The consent / AI-marking attestations SE sends to the crispasr TTS server, and the
/// Video → Text to speech → "accept voice cloning" setting that gates them.
///
/// Verified against a real crispasr 0.8.23 server (f5-tts backend, POST /v1/audio/speech):
/// a request whose <c>voice</c> ends in <c>.wav</c> plus <c>"spoken_disclaimer": false</c> and no
/// <c>marking_attestation</c> comes back HTTP 400 <c>marking_attestation_required</c>; the same
/// request with the field renders normally.
/// </summary>
[Collection(TtsSettingsCollection.Name)]
public class CrispAsrTtsProvenanceTests
{
    [Fact]
    public void AddSpeechAttestations_WhenAccepted_SendsConsentMarkingAndSkipsDisclaimer()
    {
        using var _ = new AcceptVoiceCloningScope(true);
        var payload = new Dictionary<string, object>();

        CrispAsrTtsProvenance.AddSpeechAttestations(payload);

        Assert.Equal(CrispAsrTtsProvenance.ConsentAttestation, payload["consent_attestation"]);
        Assert.Equal(CrispAsrTtsProvenance.MarkingAttestation, payload["marking_attestation"]);
        Assert.Equal(false, payload["spoken_disclaimer"]);
    }

    [Fact]
    public void AddSpeechAttestations_WhenNotAccepted_SendsNothing()
    {
        // No attestation means the server keeps its own defaults: the audible AI disclaimer stays
        // on and an actual clone request is refused with HTTP 400 consent_required. That refusal
        // is the point of turning the setting off.
        using var _ = new AcceptVoiceCloningScope(false);
        var payload = new Dictionary<string, object>();

        CrispAsrTtsProvenance.AddSpeechAttestations(payload);

        Assert.Empty(payload);
    }

    [Fact]
    public void AddSpeechAttestations_LeavesExistingFieldsAlone()
    {
        using var _ = new AcceptVoiceCloningScope(true);
        var payload = new Dictionary<string, object>
        {
            ["input"] = "hello",
            ["voice"] = "Arnold.wav",
        };

        CrispAsrTtsProvenance.AddSpeechAttestations(payload);

        Assert.Equal("hello", payload["input"]);
        Assert.Equal("Arnold.wav", payload["voice"]);
    }

    [Fact]
    public void AddServerMarkingArgs_WhenNotAccepted_AddsNothing()
    {
        using var _ = new AcceptVoiceCloningScope(false);
        var args = new Collection<string> { "--server" };

        CrispAsrTtsProvenance.AddServerMarkingArgs(args, "/does/not/matter/crispasr");

        Assert.Equal(new[] { "--server" }, args);
    }

    [Fact]
    public void AddServerMarkingArgs_WithNoInstalledExecutable_AddsNothing()
    {
        // crispasr aborts on an unknown argument, so the opt-out flags must never be passed to a
        // binary we cannot confirm is new enough to understand them.
        using var _ = new AcceptVoiceCloningScope(true);
        var args = new Collection<string> { "--server" };

        CrispAsrTtsProvenance.AddServerMarkingArgs(args, Path.Combine(Path.GetTempPath(), "no-such-crispasr"));

        Assert.Equal(new[] { "--server" }, args);
    }

    [Fact]
    public void SupportsMarkingOptOut_WithMissingExecutable_IsFalse()
    {
        Assert.False(CrispAsrTtsProvenance.SupportsMarkingOptOut(
            Path.Combine(Path.GetTempPath(), "no-such-crispasr")));
    }

    [Fact]
    public void SupportsMarkingOptOut_WithUnknownBuild_IsTrue()
    {
        // An unrecognised hash is a custom local build; SE gives those the benefit of the doubt
        // rather than silently dropping the flags (same call the chatterbox capability check makes).
        var exe = Path.Combine(Path.GetTempPath(), "crispasr-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(exe, "not a real crispasr build");
        try
        {
            Assert.True(CrispAsrTtsProvenance.SupportsMarkingOptOut(exe));
        }
        finally
        {
            File.Delete(exe);
        }
    }

    [Fact]
    public void AcceptVoiceCloning_DefaultsToOn()
    {
        Assert.True(new SeVideoTextToSpeech().AcceptVoiceCloning);
    }
}

/// <summary>
/// Restores <see cref="SeVideoTextToSpeech.AcceptVoiceCloning"/> after a test flips it.
/// </summary>
internal sealed class AcceptVoiceCloningScope : IDisposable
{
    private readonly bool _original;

    public AcceptVoiceCloningScope(bool accepted)
    {
        _original = Se.Settings.Video.TextToSpeech.AcceptVoiceCloning;
        Se.Settings.Video.TextToSpeech.AcceptVoiceCloning = accepted;
    }

    public void Dispose()
    {
        Se.Settings.Video.TextToSpeech.AcceptVoiceCloning = _original;
    }
}

/// <summary>
/// Groups every test that reads or writes the shared <see cref="Se.Settings"/> TTS state so xUnit
/// runs them one at a time instead of racing on the static.
/// </summary>
[CollectionDefinition(Name)]
public class TtsSettingsCollection
{
    public const string Name = "TtsSettings";
}
