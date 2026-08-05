using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The 20 languages MOSS-TTS v1.5 is trained on (the <c>language:</c> list in the
/// OpenMOSS-Team/MOSS-TTS model card front matter — the card's own prose table drops Hebrew), plus
/// an "Auto" entry that leaves the choice to the model.
///
/// MOSS-TTS takes the language as a plain-text field in its prompt: crispasr's moss-tts backend
/// reads <c>params.language</c>, runs it through <c>crispasr_iso_to_english_lang()</c> and writes
/// the result into the <c>&lt;user_inst&gt;</c> block as <c>- Language:\n&lt;name&gt;</c>
/// (src/moss_tts.cpp). With no language passed the field is literally <c>None</c>, which is what
/// every SE synthesis did before this list existed — the likely cause of the heavy accent users
/// reported on cross-lingual clones (#12757), where a reference WAV in one language was asked to
/// read text in another.
///
/// <para>
/// <b>Why some codes are ISO and some are spelled out.</b> crispasr's ISO→English map
/// (src/core/lang_names.h) covers only 17 codes; of MOSS-TTS's 20 languages it misses he, hu, fa,
/// cs, da, sv and el. Unmapped values pass through verbatim, so sending "hu" would put the bare code
/// in the prompt — exactly what that header warns is unreliable ("de" reads as the English word
/// "of"). Those six therefore send the English name instead, which passes through unchanged. The
/// name arrives lowercased (crispasr lowercases the <c>-l</c> value in cli.cpp) but that is far
/// better than a two-letter code. If crispasr ever adds the missing codes, sending the names keeps
/// working, so nothing here has to change.
/// </para>
///
/// Display names are taken from <see cref="OmniVoiceLanguages"/> so the two engines label the same
/// language identically in the UI; only Arabic is absent there (it lists regional Arabics but no
/// macrolanguage entry) and is spelled out locally.
/// </summary>
internal static class MossTtsLanguages
{
    /// <summary>
    /// Code for the "Auto" entry — an empty code means no <c>-l</c> flag is passed and MOSS-TTS
    /// infers the language from the text itself (the pre-existing behaviour).
    /// </summary>
    public const string AutoCode = "";

    public static readonly TtsLanguage Auto = new("Auto", AutoCode);

    /// <summary>
    /// (ISO-639-1 code, value sent as <c>-l</c>) for each supported language. The two differ only
    /// for the six languages crispasr cannot spell out itself — see the class remarks.
    /// </summary>
    private static readonly (string Iso, string Arg)[] Supported =
    {
        ("zh", "zh"),
        ("en", "en"),
        ("de", "de"),
        ("es", "es"),
        ("fr", "fr"),
        ("ja", "ja"),
        ("it", "it"),
        ("he", "hebrew"),
        ("hu", "hungarian"),
        ("ko", "ko"),
        ("ru", "ru"),
        ("fa", "persian"),
        ("ar", "ar"),
        ("pl", "pl"),
        ("pt", "pt"),
        ("cs", "czech"),
        ("da", "danish"),
        ("sv", "swedish"),
        ("el", "greek"),
        ("tr", "tr"),
    };

    // OmniVoice's 646-entry table has no macrolanguage "ar" (only regional Arabics), so the one
    // display name that cannot be reused is supplied here.
    private static readonly Dictionary<string, string> ExtraDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ar"] = "Arabic",
    };

    /// <summary>
    /// "Auto" first, then the 20 supported languages sorted by display name. Auto stays first so
    /// the combo's default selection preserves the old no-language behaviour.
    /// </summary>
    public static readonly TtsLanguage[] All = Build();

    private static TtsLanguage[] Build()
    {
        var omniNames = OmniVoiceLanguages.All.ToDictionary(l => l.Code, l => l.Name, StringComparer.OrdinalIgnoreCase);

        var languages = Supported
            .Select(s => new TtsLanguage(ResolveDisplayName(omniNames, s.Iso), s.Arg))
            .OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        languages.Insert(0, Auto);
        return languages.ToArray();
    }

    private static string ResolveDisplayName(IReadOnlyDictionary<string, string> omniNames, string iso)
    {
        if (omniNames.TryGetValue(iso, out var name))
        {
            return name;
        }

        return ExtraDisplayNames.TryGetValue(iso, out var extra) ? extra : iso;
    }

    /// <summary>
    /// The value to pass to crispasr's <c>-l</c> for <paramref name="language"/>, or an empty
    /// string when no flag should be passed (Auto, an unknown entry, or nothing selected).
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
        var savedName = Se.Settings.Video.TextToSpeech.MossTtsCrispAsrLanguage;
        if (string.IsNullOrWhiteSpace(savedName))
        {
            return string.Empty;
        }

        return All.FirstOrDefault(l => string.Equals(l.Name, savedName, StringComparison.OrdinalIgnoreCase))?.Code
               ?? string.Empty;
    }
}
