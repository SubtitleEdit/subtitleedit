using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The 31 languages Supertonic-3 speaks, with the codes CrispASR's <c>supertonic</c> backend
/// accepts (it conditions the model through a <c>&lt;lang&gt;</c> tag in the character stream;
/// there is no phonemizer). The list is the backend's own: it logs exactly these codes when
/// handed one it does not know.
///
/// English leads because it is the backend's default, and Supertonic cannot detect the language
/// itself, so there is no honest "Auto" entry to offer. The language matters: a Danish line
/// synthesized under the German tag came back unintelligible in a speech-to-text round trip,
/// while the same line under the Danish tag transcribed word for word.
/// </summary>
internal static class SupertonicLanguages
{
    /// <summary>The backend's own default; leads the list.</summary>
    public static readonly TtsLanguage Default = new("English", "en");

    private static readonly TtsLanguage[] Catalog = new TtsLanguage[]
    {
        new("Arabic", "ar"),
        new("Bulgarian", "bg"),
        new("Croatian", "hr"),
        new("Czech", "cs"),
        new("Danish", "da"),
        new("Dutch", "nl"),
        new("Estonian", "et"),
        new("Finnish", "fi"),
        new("French", "fr"),
        new("German", "de"),
        new("Greek", "el"),
        new("Hindi", "hi"),
        new("Hungarian", "hu"),
        new("Indonesian", "id"),
        new("Italian", "it"),
        new("Japanese", "ja"),
        new("Korean", "ko"),
        new("Latvian", "lv"),
        new("Lithuanian", "lt"),
        new("Polish", "pl"),
        new("Portuguese", "pt"),
        new("Romanian", "ro"),
        new("Russian", "ru"),
        new("Slovak", "sk"),
        new("Slovenian", "sl"),
        new("Spanish", "es"),
        new("Swedish", "sv"),
        new("Turkish", "tr"),
        new("Ukrainian", "uk"),
        new("Vietnamese", "vi"),
    };

    /// <summary>
    /// English first, then the rest sorted by name.
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

    public static bool IsSupported(string? code) =>
        !string.IsNullOrWhiteSpace(code)
        && All.Any(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The value to send as the request's <c>language</c> field. Never empty: the backend keeps
    /// the previous request's language when the field is missing, so a line sent without one
    /// would be spoken in whatever language the line before it used.
    /// </summary>
    public static string ResolveLanguageArg(TtsLanguage? language)
    {
        // A null language means the CALLER had none to hand over, not that the user picked
        // English: the cast dialog's voice-test button and every cross-engine cast row pass null
        // on purpose ("engines fall back to their own saved defaults").
        if (language == null)
        {
            return ResolveSavedLanguageArg();
        }

        // Guard against a language object left over from another engine (the view model can hold
        // one while switching engines): only codes this engine actually advertises are passed on.
        return IsSupported(language.Code) ? language.Code.ToLowerInvariant() : Default.Code;
    }

    /// <summary>
    /// The language code behind the pick saved by the main TTS window for this engine. The
    /// setting stores the DISPLAY NAME, which is how <c>TextToSpeechViewModel</c> writes and
    /// restores it.
    /// </summary>
    public static string ResolveSavedLanguageArg()
    {
        var savedName = Se.Settings.Video.TextToSpeech.SupertonicCrispAsrLanguage;
        if (string.IsNullOrWhiteSpace(savedName))
        {
            return Default.Code;
        }

        return All.FirstOrDefault(l => string.Equals(l.Name, savedName, StringComparison.OrdinalIgnoreCase))?.Code
               ?? Default.Code;
    }
}
