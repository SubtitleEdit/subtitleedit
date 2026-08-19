using Nikse.SubtitleEdit.Core.Common;
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
/// Base GGUFs (2454-token text vocab), which is what the versioned <c>chatterbox-v3-*</c> pair
/// in <see cref="Logic.Download.ChatterboxTtsCppDownloadService"/> guarantees. Chatterbox Turbo
/// is an English-only distillation, so <see cref="ChatterboxTtsCpp.GetLanguages"/> offers it
/// "Auto" alone.
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

    /// <summary>
    /// The language of a cloning reference, sent as the request's <c>source_lang</c> field: a
    /// best-effort detection over <paramref name="refText"/> (the transcript sidecar beside the
    /// reference WAV) first, then the explicit "Reference language" pick from the engine
    /// settings window. The sidecar holds the text actually spoken in THIS reference - per-line
    /// clone-from-video writes one per clip - so it outranks the global pick, which describes
    /// the user's own imported reference and goes stale the moment references come from a video
    /// in another language.
    /// </summary>
    /// <remarks>
    /// Chatterbox V3 follows upstream's cross-lingual recommendation once it knows what language
    /// the reference is spoken in: it drops to CFG weight 0, which keeps the speaker's timbre
    /// while shedding the reference language's accent. Without it an English reference asked for
    /// German keeps the English accent — the same hole CosyVoice3 had in #13272, and the reason
    /// the server grew a per-request <c>source_lang</c> in CrispASR v0.8.29 (#329).
    ///
    /// Unlike CosyVoice3, Chatterbox clones from the WAV alone and never asks for a transcript,
    /// so the settings pick is the path most users will take; the sidecar is only read when one
    /// happens to exist (which is what a reference cut by another SE feature leaves behind).
    /// </remarks>
    public static string ResolveSourceLanguageArg(string? refText)
    {
        var detected = DetectSourceLanguage(refText);
        if (!string.IsNullOrEmpty(detected))
        {
            return detected;
        }

        var configured = (Se.Settings.Video.TextToSpeech.ChatterboxCrispAsrSourceLanguage ?? string.Empty).Trim();
        return IsSupported(configured) ? configured : string.Empty;
    }

    /// <summary>
    /// Best-effort language of <paramref name="refText"/>, or an empty string when it cannot be
    /// determined or is not one of Chatterbox's 23 languages. Uses the same detector the rest of
    /// SE uses for subtitle language (function words first, then writing system).
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
            // clone stays plain zero-shot.
            return string.Empty;
        }
    }
}
