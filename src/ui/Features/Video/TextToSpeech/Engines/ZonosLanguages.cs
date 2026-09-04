using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The languages Zonos-v0.1 can be asked to speak, as the eSpeak-NG voice codes the model was
/// conditioned with (Zonos has an integer language conditioner indexed by these codes, and
/// CrispASR's <c>zonos-tts</c> backend also hands the same code to eSpeak for G2P). The codes
/// are read verbatim from the GGUF's <c>zonos.language_codes</c> table, so every entry here is
/// one the backend accepts; an unknown code is logged and silently falls back to en-us.
///
/// Zyphra only advertises English, Japanese, Chinese, French and German as trained languages —
/// the rest of the table exists in the model but with little or no training data behind it.
/// They are still offered: eSpeak phonemising Czech AS Czech is always better than the English
/// G2P the backend falls back to when no language is sent, which was what every non-English
/// line got before SE sent one at all.
///
/// The list leads with English (US) because that is the backend's default when the request has
/// no <c>language</c> field — so a combo that falls back to its first entry reproduces the old
/// behaviour, the same role "Auto" plays for <see cref="ChatterboxLanguages"/>. Zonos cannot
/// detect the language itself, so there is no honest "Auto" entry to offer.
///
/// Left out of the model's table on purpose: the constructed and reconstructed entries
/// (Interlingua, Lojban, Lingua Franca Nova, Pyash, Ancient Greek, Classical Nahuatl).
/// </summary>
internal static class ZonosLanguages
{
    /// <summary>The backend's own default (sent as nothing = en-us); leads the list.</summary>
    public static readonly TtsLanguage Default = new("English (US)", "en-us");

    private static readonly TtsLanguage[] Catalog = new TtsLanguage[]
    {
        new("Afrikaans", "af"),
        new("Albanian", "sq"),
        new("Amharic", "am"),
        new("Arabic", "ar"),
        new("Aragonese", "an"),
        new("Armenian", "hy"),
        new("Armenian (Western)", "hyw"),
        new("Assamese", "as"),
        new("Azerbaijani", "az"),
        new("Bashkir", "ba"),
        new("Basque", "eu"),
        new("Bengali", "bn"),
        new("Bishnupriya Manipuri", "bpy"),
        new("Bosnian", "bs"),
        new("Bulgarian", "bg"),
        new("Burmese", "my"),
        new("Cantonese", "yue"),
        new("Catalan", "ca"),
        new("Chinese (Mandarin)", "cmn"),
        new("Chinese (Hakka)", "hak"),
        new("Croatian", "hr"),
        new("Czech", "cs"),
        new("Danish", "da"),
        new("Dutch", "nl"),
        new("English (Caribbean)", "en-029"),
        new("English (UK)", "en-gb"),
        new("English (UK, Lancashire)", "en-gb-x-gbclan"),
        new("English (UK, Received Pronunciation)", "en-gb-x-rp"),
        new("English (UK, West Midlands)", "en-gb-x-gbcwmd"),
        new("English (Scotland)", "en-gb-scotland"),
        new("Esperanto", "eo"),
        new("Estonian", "et"),
        new("Finnish", "fi"),
        new("French", "fr-fr"),
        new("French (Belgium)", "fr-be"),
        new("French (Switzerland)", "fr-ch"),
        new("Georgian", "ka"),
        new("German", "de"),
        new("Greek", "el"),
        new("Greenlandic", "kl"),
        new("Guarani", "gn"),
        new("Gujarati", "gu"),
        new("Haitian Creole", "ht"),
        new("Hindi", "hi"),
        new("Hungarian", "hu"),
        new("Icelandic", "is"),
        new("Indonesian", "id"),
        new("Irish", "ga"),
        new("Italian", "it"),
        new("Japanese", "ja"),
        new("Kannada", "kn"),
        new("Kazakh", "kk"),
        new("K'iche'", "quc"),
        new("Konkani", "kok"),
        new("Korean", "ko"),
        new("Kurdish", "ku"),
        new("Kyrgyz", "ky"),
        new("Latin", "la"),
        new("Latvian", "lv"),
        new("Lithuanian", "lt"),
        new("Macedonian", "mk"),
        new("Malay", "ms"),
        new("Malayalam", "ml"),
        new("Maltese", "mt"),
        new("Māori", "mi"),
        new("Marathi", "mr"),
        new("Nepali", "ne"),
        new("Norwegian (Bokmål)", "nb"),
        new("Odia", "or"),
        new("Oromo", "om"),
        new("Papiamento", "pap"),
        new("Persian", "fa"),
        new("Persian (Latin script)", "fa-latn"),
        new("Polish", "pl"),
        new("Portuguese", "pt"),
        new("Portuguese (Brazil)", "pt-br"),
        new("Punjabi", "pa"),
        new("Romanian", "ro"),
        new("Russian", "ru"),
        new("Russian (Latvia)", "ru-lv"),
        new("Scottish Gaelic", "gd"),
        new("Serbian", "sr"),
        new("Setswana", "tn"),
        new("Shan", "shn"),
        new("Sindhi", "sd"),
        new("Sinhala", "si"),
        new("Slovak", "sk"),
        new("Slovenian", "sl"),
        new("Spanish", "es"),
        new("Spanish (Latin America)", "es-419"),
        new("Swahili", "sw"),
        new("Swedish", "sv"),
        new("Tamil", "ta"),
        new("Tatar", "tt"),
        new("Telugu", "te"),
        new("Turkish", "tr"),
        new("Urdu", "ur"),
        new("Uzbek", "uz"),
        new("Vietnamese", "vi"),
        new("Vietnamese (Central)", "vi-vn-x-central"),
        new("Vietnamese (Southern)", "vi-vn-x-south"),
        new("Welsh", "cy"),
    };

    /// <summary>
    /// English (US) first, then the rest sorted by name (ordinal, case-insensitive - sorted here
    /// rather than by hand so the table above can be edited without keeping it in order).
    ///
    /// Declared after <see cref="Catalog"/> on purpose: static field initializers run in textual
    /// order, so moving this above the catalog would copy a null array.
    /// </summary>
    public static readonly TtsLanguage[] All = BuildAll();

    private static TtsLanguage[] BuildAll()
    {
        var result = new TtsLanguage[Catalog.Length + 1];
        result[0] = Default;
        Array.Copy(Catalog, 0, result, 1, Catalog.Length);
        Array.Sort(result, 1, Catalog.Length, Comparer<TtsLanguage>.Create(
            (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name)));
        return result;
    }

    /// <summary>
    /// The value to send as the request's <c>language</c> field for <paramref name="language"/>,
    /// or an empty string when no field should be sent (an unknown entry, or nothing selected
    /// and nothing saved). Sending nothing leaves the backend on its en-us default.
    /// </summary>
    public static string ResolveLanguageArg(TtsLanguage? language)
    {
        // A null language means the CALLER had none to hand over, not that the user picked
        // English: the cast dialog's voice-test button and every cross-engine cast row pass null
        // on purpose ("engines fall back to their own saved defaults"), so without this fallback
        // those paths would silently ignore the language the user did pick — the hole CosyVoice3
        // had in #13272 and Chatterbox closed in #13470.
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
    /// empty string when nothing is saved. The setting stores the DISPLAY NAME, which is how
    /// <c>TextToSpeechViewModel</c> writes and restores it.
    /// </summary>
    public static string ResolveSavedLanguageArg()
    {
        var savedName = Se.Settings.Video.TextToSpeech.ZonosTtsCrispAsrLanguage;
        if (string.IsNullOrWhiteSpace(savedName))
        {
            return string.Empty;
        }

        return All.FirstOrDefault(l => string.Equals(l.Name, savedName, StringComparison.OrdinalIgnoreCase))?.Code
               ?? string.Empty;
    }

    /// <summary>True when <paramref name="code"/> is one of the codes in the model's table.</summary>
    public static bool IsSupported(string? code) =>
        !string.IsNullOrWhiteSpace(code)
        && All.Any(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
}
