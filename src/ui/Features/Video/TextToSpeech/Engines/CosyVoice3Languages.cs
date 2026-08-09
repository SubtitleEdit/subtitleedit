using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
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
        // A null language means the CALLER had none to hand over, which is not the same as the
        // user picking "Auto". The cast dialog's voice-test button and every cross-engine cast
        // row pass null on purpose ("engines fall back to their own saved defaults"), so without
        // this fallback the request went out with no `language` field and the clone kept the
        // reference's accent on exactly the dubbing path the target language exists for (#13272).
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
        return IsSupported(code) ? code : string.Empty;
    }

    /// <summary>
    /// The language code behind the pick saved by the main TTS window, or an empty string when
    /// nothing is saved / the saved entry is "Auto". The setting stores the DISPLAY NAME, which
    /// is how <c>TextToSpeechViewModel</c> writes and restores it.
    /// </summary>
    public static string ResolveSavedLanguageArg()
    {
        var savedName = Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrLanguage;
        if (string.IsNullOrWhiteSpace(savedName))
        {
            return string.Empty;
        }

        return All.FirstOrDefault(l => string.Equals(l.Name, savedName, StringComparison.OrdinalIgnoreCase))?.Code
               ?? string.Empty;
    }

    /// <summary>True when <paramref name="code"/> is one of the nine languages CosyVoice3 knows.</summary>
    public static bool IsSupported(string? code) =>
        !string.IsNullOrWhiteSpace(code)
        && All.Any(l => !string.IsNullOrEmpty(l.Code) && string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The language of a cloning reference, resolved the way the backend documents it: the
    /// explicit "Reference language" pick from the settings window first, then a best-effort
    /// detection over the reference transcript.
    /// </summary>
    /// <remarks>
    /// The backend only goes cross-lingual once it knows BOTH the target and the reference
    /// language, and its own reference detection declines on short or Latin-script transcripts —
    /// which is every en→de / en→fr / en→ru pair a dubbing workflow actually asks for. Detecting
    /// SE-side means a user who imported an English reference and asked for German gets
    /// cross-lingual synthesis without first finding a combo box in an engine settings window.
    /// The explicit pick still wins, and a detection outside CosyVoice3's nine languages is
    /// discarded rather than sent (the backend would reject it).
    /// </remarks>
    public static string ResolveSourceLanguageArg(string? refText)
    {
        var configured = (Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrSourceLanguage ?? string.Empty).Trim();
        if (IsSupported(configured))
        {
            return configured;
        }

        return DetectSourceLanguage(refText);
    }

    /// <summary>
    /// Best-effort language of <paramref name="refText"/>, or an empty string when it cannot be
    /// determined or is not one of CosyVoice3's nine languages. Uses the same detector the rest
    /// of SE uses for subtitle language (function words first, then writing system).
    /// </summary>
    public static string DetectSourceLanguage(string? refText)
    {
        if (string.IsNullOrWhiteSpace(refText))
        {
            return string.Empty;
        }

        try
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(refText, 0, 0));
            var detected = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle);
            return IsSupported(detected) ? detected! : string.Empty;
        }
        catch
        {
            // Detection is an optimization - a failure just means we send no source_lang and the
            // backend falls back to its own (declining) detection.
            return string.Empty;
        }
    }
}
