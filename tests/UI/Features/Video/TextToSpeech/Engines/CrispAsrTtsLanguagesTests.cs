using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The CosyVoice3 / Qwen3-TTS (CrispASR) target-language lists added for #13110. Both are sent
/// as the per-request "language" field of /v1/audio/speech; CosyVoice3 additionally engages
/// cross-lingual cloning when it differs from the reference voice's language.
/// </summary>
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
    public void ResolveLanguageArg_AutoOrNull_SendsNoField()
    {
        Assert.Equal(string.Empty, CosyVoice3Languages.ResolveLanguageArg(null));
        Assert.Equal(string.Empty, CosyVoice3Languages.ResolveLanguageArg(CosyVoice3Languages.Auto));
        Assert.Equal(string.Empty, Qwen3TtsCrispAsrLanguages.ResolveLanguageArg(null));
        Assert.Equal(string.Empty, Qwen3TtsCrispAsrLanguages.ResolveLanguageArg(Qwen3TtsCrispAsrLanguages.Auto));
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
