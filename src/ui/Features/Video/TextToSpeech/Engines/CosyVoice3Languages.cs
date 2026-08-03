using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The nine languages CosyVoice3 0.5B is trained on (Chinese, English, Japanese, Korean, German,
/// Spanish, French, Italian, Russian — the model card's "9 languages"; its 18 Mandarin dialects
/// are voice-level variants, not output languages), plus an "Auto" entry that leaves the choice
/// to the model.
///
/// The pick is sent as the per-request <c>language</c> field of <c>/v1/audio/speech</c> —
/// crispasr's cosyvoice3-tts backend compares it to the reference voice's language
/// (src/core/tts_lang.h) and mirrors upstream <c>frontend_cross_lingual</c> when they differ:
/// the reference transcript is dropped from the LM prompt so the clone speaks the target
/// language instead of imitating the reference's (CrispASR #329, SE #13110). Same language (or
/// Auto) keeps plain zero-shot cloning — the pre-existing behaviour. Requires a CrispASR build
/// with the #329 fix for Latin-script reference voices; older builds ignore the field for those
/// (their reference-language detection only answered for Hangul/Kana/Han/Cyrillic).
///
/// ISO-639-1 codes are sent as-is: the backend normalizes them via core_tts_lang::norm, which
/// covers all nine. Display names are taken from <see cref="OmniVoiceLanguages"/> so sibling
/// engines label the same language identically in the UI.
/// </summary>
internal static class CosyVoice3Languages
{
    /// <summary>
    /// Code for the "Auto" entry — an empty code means no <c>language</c> field is sent and the
    /// clone stays zero-shot in the reference's language (the pre-existing behaviour).
    /// </summary>
    public const string AutoCode = "";

    public static readonly TtsLanguage Auto = new("Auto", AutoCode);

    private static readonly string[] IsoCodes = { "zh", "en", "ja", "ko", "de", "es", "fr", "it", "ru" };

    /// <summary>
    /// "Auto" first, then the nine supported languages sorted by display name. Auto stays first
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
        var code = language?.Code;
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
}
