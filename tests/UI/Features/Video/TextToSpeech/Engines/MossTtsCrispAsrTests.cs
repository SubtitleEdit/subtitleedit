using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
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

    [Fact]
    public void Languages_LeadWithAutoThenTheTwentySupportedLanguages()
    {
        // "Auto" must stay first: the view model falls back to the first entry, and Auto is the
        // pre-existing behaviour (no -l flag, prompt says "- Language: None").
        var all = MossTtsLanguages.All;

        Assert.Equal("Auto", all[0].Name);
        Assert.Equal(string.Empty, all[0].Code);
        Assert.Equal(21, all.Length);
    }

    [Fact]
    public void Languages_ReuseOmniVoiceDisplayNames()
    {
        // The display strings come from the shared OmniVoice table so both engines label the same
        // language identically; Arabic is the one entry that table lacks.
        var names = MossTtsLanguages.All.Select(l => l.Name).ToList();

        Assert.Contains("German", names);
        Assert.Contains("Chinese", names);
        Assert.Contains("Persian", names);
        Assert.Contains("Arabic", names);
        Assert.DoesNotContain(names, n => n.Length <= 2);
    }

    [Theory]
    [InlineData("German", "de")]
    [InlineData("Chinese", "zh")]
    [InlineData("Arabic", "ar")]
    public void Languages_SendIsoCodeWhenCrispAsrCanSpellItOut(string displayName, string expectedArg)
    {
        // crispasr maps these codes to an English name itself (src/core/lang_names.h), so the code
        // is what reaches the prompt as "German" / "Chinese" / "Arabic".
        var language = MossTtsLanguages.All.Single(l => l.Name == displayName);

        Assert.Equal(expectedArg, language.Code);
    }

    [Theory]
    [InlineData("Hebrew", "hebrew")]
    [InlineData("Hungarian", "hungarian")]
    [InlineData("Persian", "persian")]
    [InlineData("Czech", "czech")]
    [InlineData("Danish", "danish")]
    [InlineData("Swedish", "swedish")]
    [InlineData("Greek", "greek")]
    public void Languages_SpellOutTheCodesCrispAsrCannotMap(string displayName, string expectedArg)
    {
        // he/hu/fa/cs/da/sv/el are missing from crispasr's ISO map, so a code would land in the prompt
        // verbatim as a bare two letters - unreliable in an LLM prompt. Send the name instead.
        var language = MossTtsLanguages.All.Single(l => l.Name == displayName);

        Assert.Equal(expectedArg, language.Code);
    }

    [Fact]
    public void ResolveLanguageArg_AutoOrNull_PassesNoFlag()
    {
        Assert.Equal(string.Empty, MossTtsLanguages.ResolveLanguageArg(null));
        Assert.Equal(string.Empty, MossTtsLanguages.ResolveLanguageArg(MossTtsLanguages.Auto));
    }

    [Fact]
    public void ResolveLanguageArg_ForeignLanguage_PassesNoFlag()
    {
        // Switching engines can leave a language from another engine's list selected (OmniVoice has
        // 646 of them); only codes MOSS-TTS actually supports may reach the server.
        Assert.Equal(string.Empty, MossTtsLanguages.ResolveLanguageArg(new TtsLanguage("Abadi", "kbt")));
    }

    [Fact]
    public void ResolveLanguageArg_SupportedLanguage_PassesItThrough()
    {
        var german = MossTtsLanguages.All.Single(l => l.Name == "German");

        Assert.Equal("de", MossTtsLanguages.ResolveLanguageArg(german));
    }
}
