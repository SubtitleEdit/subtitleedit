using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The CosyVoice3 / Qwen3-TTS (CrispASR) target-language lists added for #13110. Both are sent
/// as the per-request "language" field of /v1/audio/speech; CosyVoice3 additionally engages
/// cross-lingual cloning when it differs from the reference voice's language.
///
/// Several cases read or write the saved picks in <see cref="Se.Settings"/>, hence the shared
/// TTS-settings collection.
/// </summary>
[Collection(TtsSettingsCollection.Name)]
public class CrispAsrTtsLanguagesTests
{
    [Fact]
    public void CosyVoice3Languages_LeadWithAutoThenTheNineSupportedLanguages()
    {
        // "Auto" must stay first: the view model falls back to the first entry, and Auto is the
        // pre-existing behaviour (no language field, plain zero-shot cloning).
        var all = CosyVoice3Languages.All;

        Assert.Equal("Auto", all[0].Name);
        Assert.Equal(string.Empty, all[0].Code);
        Assert.Equal(10, all.Length);
    }

    [Fact]
    public void Qwen3TtsCrispAsrLanguages_LeadWithAutoThenTheTenSupportedLanguages()
    {
        // The ten entries mirror the talker's codec_language_id table (HF config); Auto keeps
        // the "nothink" prefill path where the model infers the language from the text.
        var all = Qwen3TtsCrispAsrLanguages.All;

        Assert.Equal("Auto", all[0].Name);
        Assert.Equal(string.Empty, all[0].Code);
        Assert.Equal(11, all.Length);
    }

    /// <summary>
    /// #13273: the OmniVoice (CrispASR) combo listed all 646 languages and changed nothing —
    /// Speak() took the pick and never put it on the request. Wiring it up needed "Auto" in front
    /// of the catalog too: the view model falls back to the first entry, and without Auto that is
    /// "Abadi", a real ISO 639-3 id the model would happily condition on.
    /// </summary>
    [Fact]
    public void OmniVoiceLanguages_LeadWithAutoThenTheWholeCatalog()
    {
        var all = OmniVoiceLanguages.All;

        Assert.Equal("Auto", all[0].Name);
        Assert.Equal(string.Empty, all[0].Code);
        Assert.Equal(647, all.Length);

        // The entry that made a bare first-entry fallback dangerous.
        Assert.Equal("Abadi", all[1].Name);
    }

    [Theory]
    [InlineData("German", "de")]
    [InlineData("Standard Arabic", "arb")]
    [InlineData("Egyptian Arabic", "arz")]
    public void OmniVoiceLanguages_SendTheModelsOwnIds(string displayName, string expectedCode)
    {
        var language = OmniVoiceLanguages.All.Single(l => l.Name == displayName);

        Assert.Equal(expectedCode, language.Code);
        Assert.Equal(expectedCode, OmniVoiceLanguages.ResolveLanguageArg(language));
    }

    /// <summary>
    /// The ids are ISO 639-3 *individual* languages, so the macrolanguage code "ar" is absent by
    /// design — Arabic is arb/arz/ary and friends. Asserted so a future "missing Arabic" report
    /// does not get 'fixed' by inventing an id the model has never seen.
    /// </summary>
    [Fact]
    public void OmniVoiceLanguages_HaveNoMacrolanguageArabic()
    {
        Assert.False(OmniVoiceLanguages.IsSupported("ar"));
        Assert.True(OmniVoiceLanguages.IsSupported("arb"));
        Assert.True(OmniVoiceLanguages.IsSupported("ary"));
    }

    [Fact]
    public void OmniVoiceLanguages_ResolveLanguageArg_AutoAndUnknownSendNoField()
    {
        using var _ = new SavedTtsLanguageScope("German", "German", "German", "German");

        // An explicit "Auto" pick must win over whatever is saved - unlike a null argument.
        Assert.Equal(string.Empty, OmniVoiceLanguages.ResolveLanguageArg(OmniVoiceLanguages.Auto));

        // A language object left over from another engine, and a locale-shaped code the model
        // has no token for. crispasr falls back to language-agnostic on both, but the request
        // should not carry them in the first place.
        Assert.Equal(string.Empty, OmniVoiceLanguages.ResolveLanguageArg(new TtsLanguage("German", "de-DE")));
        Assert.Equal(string.Empty, OmniVoiceLanguages.ResolveLanguageArg(new TtsLanguage("Klingon", "tlh")));
    }

    [Fact]
    public void OmniVoiceLanguages_ResolveLanguageArg_Null_FallsBackToTheSavedPick()
    {
        using (new SavedTtsLanguageScope("German", "German", "German", "German"))
        {
            Assert.Equal("de", OmniVoiceLanguages.ResolveLanguageArg(null));
        }

        using (new SavedTtsLanguageScope(string.Empty, string.Empty, string.Empty, string.Empty))
        {
            Assert.Equal(string.Empty, OmniVoiceLanguages.ResolveLanguageArg(null));
        }

        using (new SavedTtsLanguageScope("Auto", "Auto", "Auto", "Auto"))
        {
            Assert.Equal(string.Empty, OmniVoiceLanguages.ResolveLanguageArg(null));
        }
    }

    [Theory]
    [InlineData("Chinese", "zh")]
    [InlineData("German", "de")]
    [InlineData("Russian", "ru")]
    public void CosyVoice3Languages_SendIsoCodes(string displayName, string expectedCode)
    {
        // The backend normalizes ISO-639-1 codes itself (core_tts_lang::norm), so the code is
        // sent as-is; display names come from the shared OmniVoice table.
        var language = CosyVoice3Languages.All.Single(l => l.Name == displayName);

        Assert.Equal(expectedCode, language.Code);
    }

    [Theory]
    [InlineData("Portuguese", "pt")]
    [InlineData("Japanese", "ja")]
    public void Qwen3TtsCrispAsrLanguages_SendIsoCodes(string displayName, string expectedCode)
    {
        // crispasr's qwen3-tts adapter maps the ISO code to the English name the model's
        // codec_language_names table is keyed by (core_lang::iso_to_english covers all ten).
        var language = Qwen3TtsCrispAsrLanguages.All.Single(l => l.Name == displayName);

        Assert.Equal(expectedCode, language.Code);
    }

    [Fact]
    public void ResolveLanguageArg_Auto_SendsNoField()
    {
        using var _ = new SavedTtsLanguageScope("German", "German", "German");

        // An explicit "Auto" pick means the user asked for no language field, and it must win
        // over whatever is saved - unlike a null argument (see below).
        Assert.Equal(string.Empty, CosyVoice3Languages.ResolveLanguageArg(CosyVoice3Languages.Auto));
        Assert.Equal(string.Empty, Qwen3TtsCrispAsrLanguages.ResolveLanguageArg(Qwen3TtsCrispAsrLanguages.Auto));
        Assert.Equal(string.Empty, MossTtsLanguages.ResolveLanguageArg(MossTtsLanguages.Auto));
    }

    [Fact]
    public void ResolveLanguageArg_Null_FallsBackToTheSavedPick()
    {
        // #13272: the cast dialog's voice-test button and every cross-engine cast row call Speak
        // with language: null on the assumption that engines fall back to their own saved
        // default. These three didn't, so the target language silently vanished on exactly the
        // dubbing path it exists for and the clone kept the reference's accent.
        using var _ = new SavedTtsLanguageScope("German", "Portuguese", "German");

        Assert.Equal("de", CosyVoice3Languages.ResolveLanguageArg(null));
        Assert.Equal("pt", Qwen3TtsCrispAsrLanguages.ResolveLanguageArg(null));
        Assert.Equal("de", MossTtsLanguages.ResolveLanguageArg(null));
    }

    [Fact]
    public void ResolveLanguageArg_Null_WithNothingSaved_SendsNoField()
    {
        using var _ = new SavedTtsLanguageScope(string.Empty, string.Empty, string.Empty);

        Assert.Equal(string.Empty, CosyVoice3Languages.ResolveLanguageArg(null));
        Assert.Equal(string.Empty, Qwen3TtsCrispAsrLanguages.ResolveLanguageArg(null));
        Assert.Equal(string.Empty, MossTtsLanguages.ResolveLanguageArg(null));
    }

    [Fact]
    public void ResolveLanguageArg_Null_WithSavedAuto_SendsNoField()
    {
        using var _ = new SavedTtsLanguageScope("Auto", "Auto", "Auto");

        Assert.Equal(string.Empty, CosyVoice3Languages.ResolveLanguageArg(null));
        Assert.Equal(string.Empty, Qwen3TtsCrispAsrLanguages.ResolveLanguageArg(null));
        Assert.Equal(string.Empty, MossTtsLanguages.ResolveLanguageArg(null));
    }

    [Theory]
    [InlineData("The quick brown fox is not what it seems, and there is nothing we can do about it.", "en")]
    [InlineData("Ich habe das Buch nicht gelesen, aber es ist mir egal, was du davon hältst.", "de")]
    [InlineData("Это моя тестовая запись, и я не знаю, что ещё сказать об этом.", "ru")]
    public void DetectSourceLanguage_ReadsTheReferenceTranscript(string refText, string expected)
    {
        // The reference language is half of the cross-lingual gate, and the backend's own
        // detector declines on Latin scripts. Reading it off the transcript SE already keeps in
        // the voice's .txt sidecar is what makes en->de work without any manual setup (#13272).
        Assert.Equal(expected, CosyVoice3Languages.DetectSourceLanguage(refText));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Jeg har ikke læst bogen, men jeg er ligeglad med hvad du mener om den.")]
    public void DetectSourceLanguage_UnknownOrUnsupported_ReturnsEmpty(string refText)
    {
        // Danish is a language the detector knows but CosyVoice3 does not - sending it would be
        // rejected by the backend, so an unsupported hit counts as "could not determine".
        Assert.Equal(string.Empty, CosyVoice3Languages.DetectSourceLanguage(refText));
    }

    [Fact]
    public void ResolveSourceLanguageArg_ExplicitPickWinsOverDetection()
    {
        using var _ = new SavedReferenceLanguageScope("fr");

        Assert.Equal("fr", CosyVoice3Languages.ResolveSourceLanguageArg(
            "The quick brown fox is not what it seems, and there is nothing we can do about it."));
    }

    [Fact]
    public void ResolveSourceLanguageArg_NoPick_DetectsPerVoice()
    {
        // One global setting cannot be right for a user with reference WAVs in two languages,
        // so with nothing picked each voice answers from its own transcript.
        using var _ = new SavedReferenceLanguageScope(string.Empty);

        Assert.Equal("en", CosyVoice3Languages.ResolveSourceLanguageArg(
            "The quick brown fox is not what it seems, and there is nothing we can do about it."));
        Assert.Equal("de", CosyVoice3Languages.ResolveSourceLanguageArg(
            "Ich habe das Buch nicht gelesen, aber es ist mir egal, was du davon hältst."));
        Assert.Equal(string.Empty, CosyVoice3Languages.ResolveSourceLanguageArg(string.Empty));
    }

    [Fact]
    public void ResolveLanguageArg_ForeignLanguage_SendsNoField()
    {
        // Switching engines can leave a language from another engine's list selected (OmniVoice
        // has 646 of them); only codes the engine actually supports may reach the server.
        Assert.Equal(string.Empty, CosyVoice3Languages.ResolveLanguageArg(new TtsLanguage("Abadi", "kbt")));
        Assert.Equal(string.Empty, Qwen3TtsCrispAsrLanguages.ResolveLanguageArg(new TtsLanguage("Abadi", "kbt")));
    }

    [Fact]
    public void ResolveLanguageArg_SupportedLanguage_PassesItThrough()
    {
        Assert.Equal("de", CosyVoice3Languages.ResolveLanguageArg(
            CosyVoice3Languages.All.Single(l => l.Name == "German")));
        Assert.Equal("pt", Qwen3TtsCrispAsrLanguages.ResolveLanguageArg(
            Qwen3TtsCrispAsrLanguages.All.Single(l => l.Name == "Portuguese")));
    }

    [Fact]
    public void CosyVoice3Languages_HasNoPortuguese_Qwen3HasNoKoreanGap()
    {
        // Guard the per-model capability split: CosyVoice3's nine languages include Korean but
        // not Portuguese; Qwen3-TTS's ten include Portuguese. A refactor that merges the two
        // lists would silently offer languages a model cannot speak.
        Assert.DoesNotContain(CosyVoice3Languages.All, l => l.Code == "pt");
        Assert.Contains(CosyVoice3Languages.All, l => l.Code == "ko");
        Assert.Contains(Qwen3TtsCrispAsrLanguages.All, l => l.Code == "pt");
    }
}

/// <summary>
/// Sets the three saved target-language picks (stored as DISPLAY NAMES, which is how
/// TextToSpeechViewModel writes them) and restores them afterwards.
/// </summary>
internal sealed class SavedTtsLanguageScope : IDisposable
{
    private readonly string _cosyVoice3;
    private readonly string _qwen3;
    private readonly string _moss;
    private readonly string _omniVoice;

    public SavedTtsLanguageScope(string cosyVoice3, string qwen3, string moss, string omniVoice = "")
    {
        var settings = Se.Settings.Video.TextToSpeech;
        _cosyVoice3 = settings.CosyVoice3CrispAsrLanguage;
        _qwen3 = settings.Qwen3TtsCrispAsrLanguage;
        _moss = settings.MossTtsCrispAsrLanguage;
        _omniVoice = settings.OmniVoiceCrispAsrLanguage;

        settings.CosyVoice3CrispAsrLanguage = cosyVoice3;
        settings.Qwen3TtsCrispAsrLanguage = qwen3;
        settings.MossTtsCrispAsrLanguage = moss;
        settings.OmniVoiceCrispAsrLanguage = omniVoice;
    }

    public void Dispose()
    {
        var settings = Se.Settings.Video.TextToSpeech;
        settings.CosyVoice3CrispAsrLanguage = _cosyVoice3;
        settings.Qwen3TtsCrispAsrLanguage = _qwen3;
        settings.MossTtsCrispAsrLanguage = _moss;
        settings.OmniVoiceCrispAsrLanguage = _omniVoice;
    }
}

/// <summary>
/// Sets CosyVoice3's "Reference language" pick (an ISO code, unlike the target-language setting)
/// and restores it afterwards.
/// </summary>
internal sealed class SavedReferenceLanguageScope : IDisposable
{
    private readonly string _original;

    public SavedReferenceLanguageScope(string code)
    {
        _original = Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrSourceLanguage;
        Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrSourceLanguage = code;
    }

    public void Dispose()
    {
        Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrSourceLanguage = _original;
    }
}
