using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
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

    [Fact]
    public void BuildSpeakPayload_WithLanguage_SendsLanguageField()
    {
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("bonjour", string.Empty, "fr");

        Assert.Equal("fr", payload["language"]);
    }

    [Fact]
    public void BuildSpeakPayload_WithoutLanguage_SendsNoLanguageField()
    {
        // No language (Auto, or the Turbo model) must keep the payload identical to the
        // pre-multilingual behaviour — the server treats a missing field as language-agnostic.
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", string.Empty);

        Assert.False(payload.ContainsKey("language"));
    }

    [Fact]
    public void Languages_LeadWithAutoThenTheTwentyThreeSupportedLanguages()
    {
        // "Auto" first so a combo falling back to its first entry reproduces the
        // pre-language-selection behaviour (no field sent).
        var all = ChatterboxLanguages.All;

        Assert.Equal(24, all.Length);
        Assert.Equal("Auto", all[0].Name);
        Assert.Equal(string.Empty, all[0].Code);
    }

    [Theory]
    [InlineData("French", "fr")]
    [InlineData("German", "de")]
    [InlineData("Chinese", "zh")]
    public void Languages_ResolveToIsoCode(string displayName, string expectedArg)
    {
        var language = ChatterboxLanguages.All.Single(l => l.Name == displayName);

        Assert.Equal(expectedArg, ChatterboxLanguages.ResolveLanguageArg(language));
    }

    [Fact]
    public void Languages_AutoResolvesToEmpty()
    {
        Assert.Equal(string.Empty, ChatterboxLanguages.ResolveLanguageArg(ChatterboxLanguages.Auto));
    }

    [Fact]
    public void Languages_ForeignEngineCodeIsDropped()
    {
        // A language object left over from another engine (e.g. OmniVoice's ISO 639-3 ids)
        // must not leak onto the wire.
        var foreign = new TtsLanguage("Standard Arabic", "arb");

        Assert.Equal(string.Empty, ChatterboxLanguages.ResolveLanguageArg(foreign));
    }

    [Fact]
    public void LegacyEnglishOnlyGguf_IsDetectedBySize()
    {
        // cstr/chatterbox-GGUF was rebuilt in place with multilingual weights; the legacy
        // English-only files are recognised by exact byte size so they get re-downloaded.
        // SetLength is metadata-only, so no 630 MB is actually written.
        var path = Path.Combine(Path.GetTempPath(), $"chatterbox-legacy-test-{Guid.NewGuid():N}.gguf");
        try
        {
            using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
                fs.SetLength(630_177_120);
            }

            Assert.True(Nikse.SubtitleEdit.Logic.Download.ChatterboxTtsCppDownloadService.IsLegacyEnglishOnlyModel(path));

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.SetLength(639_285_952); // the current multilingual T3 — must NOT be flagged
            }

            Assert.False(Nikse.SubtitleEdit.Logic.Download.ChatterboxTtsCppDownloadService.IsLegacyEnglishOnlyModel(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
