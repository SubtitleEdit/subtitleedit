using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The ten output languages in the Qwen3-TTS 1.7B talker's <c>codec_language_id</c> table
/// (Chinese, English, German, Italian, Portuguese, Spanish, Japanese, Korean, French, Russian —
/// from the HF config; emitted into the GGUF as <c>qwen3tts.codec_language_names</c>), plus an
/// "Auto" entry that leaves the choice to the model.
///
/// The pick is sent as the per-request <c>language</c> field of <c>/v1/audio/speech</c>.
/// crispasr's qwen3-tts backend maps the ISO code to the English name the table is keyed by
/// (core_lang::iso_to_english covers all ten) and sets the talker's explicit
/// <c>codec_language_id</c> in the codec prefill — applies to VoiceDesign, CustomVoice and
/// cloned voices alike (CrispASR #329, SE #13110). Without it the prefill takes the "nothink"
/// path and the model infers the language from the text, which for a cloned reference in
/// another language surfaces as a strong accent. Auto (no field) keeps that pre-existing
/// behaviour; a code the loaded GGUF doesn't know logs a server-side warning and falls back to
/// auto.
///
/// Display names are taken from <see cref="OmniVoiceLanguages"/> so sibling engines label the
/// same language identically in the UI.
/// </summary>
internal static class Qwen3TtsCrispAsrLanguages
{
    /// <summary>
    /// Code for the "Auto" entry — an empty code means no <c>language</c> field is sent and the
    /// model infers the language from the text itself (the pre-existing behaviour).
    /// </summary>
    public const string AutoCode = "";

    public static readonly TtsLanguage Auto = new("Auto", AutoCode);

    private static readonly string[] IsoCodes = { "zh", "en", "de", "it", "pt", "es", "ja", "ko", "fr", "ru" };

    /// <summary>
    /// "Auto" first, then the ten supported languages sorted by display name. Auto stays first
    /// so the combo's default selection preserves the old no-language behaviour.
    /// </summary>
    public static readonly TtsLanguage[] All = Build();

    private static TtsLanguage[] Build()
    {
        var omniNames = OmniVoiceLanguages.All.ToDictionary(l => l.Code, l => l.Name, StringComparer.OrdinalIgnoreCase);

        var languages = IsoCodes
            .Select(iso => new TtsLanguage(omniNames.TryGetValue(iso, out var name) ? name : iso, iso))
            .OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        languages.Insert(0, Auto);
        return languages.ToArray();
    }

    /// <summary>
    /// The value to send as the request's <c>language</c> field for <paramref name="language"/>,
    /// or an empty string when no field should be sent (Auto, an unknown entry, or nothing
    /// selected).
    /// </summary>
    public static string ResolveLanguageArg(TtsLanguage? language)
    {
        // A null language means the CALLER had none to hand over, not that the user picked
        // "Auto" - the cast dialog's voice-test button and cross-engine cast rows both pass null
        // and rely on the engine falling back to its own saved default (#13272).
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
        // one while switching engines): only values this engine actually advertises are passed on.
        return All.Any(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase))
            ? code
            : string.Empty;
    }

    /// <summary>
    /// The language code behind the pick saved by the main TTS window, or an empty string when
    /// nothing is saved / the saved entry is "Auto". The setting stores the DISPLAY NAME, which
    /// is how <c>TextToSpeechViewModel</c> writes and restores it.
    /// </summary>
    public static string ResolveSavedLanguageArg()
    {
        var savedName = Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrLanguage;
        if (string.IsNullOrWhiteSpace(savedName))
        {
            return string.Empty;
        }

        return All.FirstOrDefault(l => string.Equals(l.Name, savedName, StringComparison.OrdinalIgnoreCase))?.Code
               ?? string.Empty;
    }
}
