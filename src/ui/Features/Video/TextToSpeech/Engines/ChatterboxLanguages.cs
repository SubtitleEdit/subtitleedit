using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The 23 languages of the multilingual Chatterbox model, plus an "Auto" entry that sends no
/// language and keeps the pre-selection behaviour (the model guesses, usually with an English
/// accent on non-English text).
///
/// The codes are ISO 639-1 two-letter IDs. CrispASR's chatterbox backend turns the request's
/// <c>language</c> field into the <c>[xx]</c> language token it prepends to the T3 prompt —
/// the same mechanism as its CLI <c>-l</c> flag. This only has an effect on the multilingual
/// Base GGUFs (2454-token text vocab); see
/// <see cref="Logic.Download.ChatterboxTtsCppDownloadService.IsLegacyEnglishOnlyModel"/> for how
/// pre-multilingual files are detected and re-downloaded. Chatterbox Turbo is an English-only
/// distillation, so <see cref="ChatterboxTtsCpp.GetLanguages"/> offers it "Auto" alone.
///
/// The multilingual tokenizer actually carries a few more tags (bg, cs, hu, ro, sk, ta, vi and
/// special tags like [ipa]); only the 23 languages ResembleAI ships and documents are listed
/// here — the extra tags exist in the vocab but are not advertised as trained.
/// </summary>
internal static class ChatterboxLanguages
{
    private static readonly TtsLanguage[] Catalog = new TtsLanguage[]
    {
        new("Arabic", "ar"),
        new("Chinese", "zh"),
        new("Danish", "da"),
        new("Dutch", "nl"),
        new("English", "en"),
        new("Finnish", "fi"),
        new("French", "fr"),
        new("German", "de"),
        new("Greek", "el"),
        new("Hebrew", "he"),
        new("Hindi", "hi"),
        new("Italian", "it"),
        new("Japanese", "ja"),
        new("Korean", "ko"),
        new("Malay", "ms"),
        new("Norwegian", "no"),
        new("Polish", "pl"),
        new("Portuguese", "pt"),
        new("Russian", "ru"),
        new("Spanish", "es"),
        new("Swahili", "sw"),
        new("Swedish", "sv"),
        new("Turkish", "tr"),
    };

    /// <summary>
    /// Code for the "Auto" entry — an empty code means no language field is sent.
    /// </summary>
    public const string AutoCode = "";

    public static readonly TtsLanguage Auto = new("Auto", AutoCode);

    /// <summary>
    /// "Auto" first, then the 23 languages alphabetically. Auto leads so a combo that falls
    /// back to its first entry preserves the old no-language behaviour.
    ///
    /// Declared after <see cref="Catalog"/> on purpose: static field initializers run in textual
    /// order, so moving this above the catalog would copy a null array.
    /// </summary>
    public static readonly TtsLanguage[] All = BuildAll();

    private static TtsLanguage[] BuildAll()
    {
        var result = new TtsLanguage[Catalog.Length + 1];
        result[0] = Auto;
        Array.Copy(Catalog, 0, result, 1, Catalog.Length);
        return result;
    }

    /// <summary>
    /// The value to send as the request's <c>language</c> field for <paramref name="language"/>,
    /// or an empty string when no field should be sent (Auto, an unknown entry, or nothing
    /// selected).
    /// </summary>
    public static string ResolveLanguageArg(TtsLanguage? language)
    {
        // A null language means the CALLER had none to hand over, which is not the same as the
        // user picking "Auto". The cast dialog's voice-test button and every cross-engine cast
        // row pass null on purpose ("engines fall back to their own saved defaults"), so without
        // this fallback those paths would silently ignore the language the user did pick — the
        // same hole CosyVoice3 had in #13272.
        if (language == null)
        {
            return ResolveSavedLanguageArg();
        }

        var code = language.Code;
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        // Guard against a language object left over from another engine (the view model can hold
        // one while switching engines): only codes this engine actually advertises are passed on.
        return IsSupported(code) ? code : string.Empty;
    }

    /// <summary>
    /// The language code behind the pick saved by the main TTS window for this engine, or an
    /// empty string when nothing is saved / the saved entry is "Auto". The setting stores the
    /// DISPLAY NAME, which is how <c>TextToSpeechViewModel</c> writes and restores it.
    /// </summary>
    public static string ResolveSavedLanguageArg()
    {
        var savedName = Se.Settings.Video.TextToSpeech.ChatterboxCrispAsrLanguage;
        if (string.IsNullOrWhiteSpace(savedName))
        {
            return string.Empty;
        }

        return All.FirstOrDefault(l => string.Equals(l.Name, savedName, StringComparison.OrdinalIgnoreCase))?.Code
               ?? string.Empty;
    }

    /// <summary>True when <paramref name="code"/> is one of the 23 supported language codes.</summary>
    public static bool IsSupported(string? code) =>
        !string.IsNullOrWhiteSpace(code)
        && Catalog.Any(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
}
