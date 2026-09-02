using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The 14 languages Confucius4-TTS is trained on, per the NetEase Youdao technical report
/// (arXiv 2608.11650): Chinese, English, Japanese, Korean, German, French, Spanish, Indonesian,
/// Italian, Thai, Portuguese, Russian, Malay and Vietnamese.
///
/// crispasr's confucius4-tts backend maps the <c>-l</c> ISO code onto the Chinese-language
/// prompt template the model was trained with ("请用X语朗读接下来的文字",
/// src/confucius4_tts.cpp) — an invented prompt string derails the T2S stage, which is why the
/// codes are passed through verbatim rather than spelled out. The backend's map actually
/// carries ~97 codes, but everything outside these 14 is a best-effort extension the model was
/// never trained on, so only the official list is offered here.
///
/// There is deliberately no "Auto" entry: the backend has no language detection — with no
/// <c>-l</c> flag it silently reads everything as English, so an "Auto" label would just be a
/// misleading name for English. English leads the list as the default pick instead.
///
/// Display names are taken from <see cref="OmniVoiceLanguages"/> so the engines label the same
/// language identically in the UI; all 14 codes are present there.
/// </summary>
internal static class Confucius4TtsLanguages
{
    /// <summary>ISO-639-1 codes passed to crispasr's <c>-l</c> verbatim. English first — it is
    /// the combo's default when nothing is saved, and no <c>-l</c> flag also means English.</summary>
    private static readonly string[] Supported =
    {
        "en", "zh", "ja", "ko", "de", "fr", "es", "id", "it", "th", "pt", "ru", "ms", "vi",
    };

    /// <summary>
    /// English first (the backend's implicit default), then the other 13 sorted by display name.
    /// </summary>
    public static readonly TtsLanguage[] All = Build();

    private static TtsLanguage[] Build()
    {
        var omniNames = OmniVoiceLanguages.All.ToDictionary(l => l.Code, l => l.Name, StringComparer.OrdinalIgnoreCase);

        var languages = Supported
            .Skip(1)
            .Select(code => new TtsLanguage(omniNames.TryGetValue(code, out var name) ? name : code, code))
            .OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        languages.Insert(0, new TtsLanguage(omniNames.TryGetValue("en", out var en) ? en : "English", "en"));
        return languages.ToArray();
    }

    /// <summary>
    /// The value to pass to crispasr's <c>-l</c> for <paramref name="language"/>, or an empty
    /// string when no flag should be passed (which the backend reads as English).
    /// </summary>
    public static string ResolveLanguageArg(TtsLanguage? language)
    {
        // A null language means the CALLER had none to hand over, not a user choice - the cast
        // dialog's voice-test button and cross-engine cast rows both pass null and rely on the
        // engine falling back to its own saved default (#13272).
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
    /// nothing is saved. The setting stores the DISPLAY NAME, which is how
    /// <c>TextToSpeechViewModel</c> writes and restores it.
    /// </summary>
    public static string ResolveSavedLanguageArg()
    {
        var savedName = Se.Settings.Video.TextToSpeech.Confucius4TtsCrispAsrLanguage;
        if (string.IsNullOrWhiteSpace(savedName))
        {
            return string.Empty;
        }

        return All.FirstOrDefault(l => string.Equals(l.Name, savedName, StringComparison.OrdinalIgnoreCase))?.Code
               ?? string.Empty;
    }
}
