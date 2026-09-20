using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The 24 languages FireRedTTS3 is trained on (technical report arXiv 2608.17492): Arabic,
/// Cantonese, Chinese, Czech, Dutch, English, Finnish, French, German, Greek, Hindi,
/// Indonesian, Italian, Japanese, Korean, Polish, Portuguese, Romanian, Russian, Spanish, Thai,
/// Turkish, Ukrainian and Vietnamese. The 21 Chinese dialect tags (<c>ZH_Sichuan</c>, ...) are
/// deliberately left out: they need a dialect reference clip to have any effect (issue #3
/// upstream) and are not subtitle languages.
///
/// audio.cpp's <c>language</c> request option takes the model's own tag, which is the English
/// name of the language ("English", "Cantonese", ...), so <see cref="TtsLanguage.Code"/> holds
/// that tag verbatim rather than an ISO code. There is no "Auto": the model has no language
/// detection and audio.cpp's default for an unset tag is Chinese, so English leads the list as
/// the pick when nothing is saved.
///
/// Text normalization caveat, from audio.cpp's docs/models/fireredtts3.md: numbers, dates and
/// units are only spelled out for Chinese, English and Cantonese; the other tags get whitespace
/// cleanup and read digits literally.
/// </summary>
internal static class FireRedTts3Languages
{
    /// <summary>FireRedTTS3 language tags, passed to audio.cpp verbatim. English first.</summary>
    private static readonly string[] Supported =
    {
        "English", "Arabic", "Cantonese", "Chinese", "Czech", "Dutch", "Finnish", "French", "German",
        "Greek", "Hindi", "Indonesian", "Italian", "Japanese", "Korean", "Polish", "Portuguese",
        "Romanian", "Russian", "Spanish", "Thai", "Turkish", "Ukrainian", "Vietnamese",
    };

    public const string DefaultTag = "English";

    /// <summary>English first (the default pick), then the other 23 sorted by name.</summary>
    public static readonly TtsLanguage[] All = Build();

    private static TtsLanguage[] Build()
    {
        var languages = Supported
            .Skip(1)
            .Select(tag => new TtsLanguage(tag, tag))
            .OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        languages.Insert(0, new TtsLanguage(DefaultTag, DefaultTag));
        return languages.ToArray();
    }

    /// <summary>
    /// The tag to send for <paramref name="language"/>. Never empty: an unset tag makes
    /// audio.cpp fall back to Chinese, so unknown or missing picks resolve to the saved pick
    /// and then to English.
    /// </summary>
    public static string ResolveLanguageTag(TtsLanguage? language)
    {
        // A null language means the CALLER had none to hand over, not a user choice - the cast
        // dialog's voice-test button and cross-engine cast rows both pass null and rely on the
        // engine falling back to its own saved default (#13272).
        if (language == null)
        {
            return ResolveSavedLanguageTag();
        }

        // Guard against a language object left over from another engine (the view model can hold
        // one while switching engines): only tags this engine actually advertises are passed on.
        var match = All.FirstOrDefault(l =>
            string.Equals(l.Code, language.Code, StringComparison.OrdinalIgnoreCase)
            || string.Equals(l.Name, language.Name, StringComparison.OrdinalIgnoreCase));
        return match?.Code ?? ResolveSavedLanguageTag();
    }

    /// <summary>
    /// The tag behind the pick saved by the main TTS window, or English when nothing is saved.
    /// The setting stores the DISPLAY NAME, which is how <c>TextToSpeechViewModel</c> writes
    /// and restores it (and here name and tag are the same string).
    /// </summary>
    public static string ResolveSavedLanguageTag()
    {
        var savedName = Se.Settings.Video.TextToSpeech.FireRedTts3AudioCppLanguage;
        if (string.IsNullOrWhiteSpace(savedName))
        {
            return DefaultTag;
        }

        return All.FirstOrDefault(l => string.Equals(l.Name, savedName, StringComparison.OrdinalIgnoreCase))?.Code
               ?? DefaultTag;
    }
}
