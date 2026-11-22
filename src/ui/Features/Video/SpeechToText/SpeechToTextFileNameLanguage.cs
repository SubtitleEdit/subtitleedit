using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// Picks the language code for a speech-to-text file name ("video.ja.srt") from the configured
/// language (selected language or online engine hint) and the language detected from the
/// transcript text (#17098). The text wins only when it clearly contradicts the configuration:
/// it is in another script, or it has strong word evidence for a language not easily confused
/// with the configured one - so a deliberate "Norwegian" is never turned into "da" by a guess.
/// </summary>
public static class SpeechToTextFileNameLanguage
{
    /// <summary>Minimum keyword hits before same-script text detection may overrule the configuration.</summary>
    public const int MinimumWordScore = 10;

    private static readonly Dictionary<string, string> NonLatinScripts = BuildNonLatinScripts();

    // Languages the text detector can mix up with each other (same script, shared words).
    private static readonly string[][] ConfusableGroups =
    {
        new[] { "da", "no", "nb", "nn", "sv" },
        new[] { "id", "ms", "jw", "jv", "su", "tl" },
        new[] { "es", "pt", "gl", "ca" },
        new[] { "sr", "hr", "bs", "sl", "mk" },
        new[] { "cs", "sk" },
        new[] { "nl", "af" },
        new[] { "ru", "uk", "be", "bg" },
        new[] { "ar", "fa", "ur", "ps", "ckb" },
        new[] { "hi", "mr", "ne", "sa" },
        new[] { "zh", "yue" },
        new[] { "he", "iw", "yi" },
    };

    /// <param name="translateToEnglish">Whisper "translate to English" is on - the output is English.</param>
    /// <param name="configuredCode">Normalized selected language or online hint, or null.</param>
    /// <param name="detectedCode">Normalized language detected from the transcript text, or null.</param>
    /// <param name="detectedWordScore">Keyword hits backing <paramref name="detectedCode"/> (0 when it came from script/letters only).</param>
    public static string? Resolve(bool translateToEnglish, string? configuredCode, string? detectedCode, int detectedWordScore)
    {
        if (translateToEnglish)
        {
            return "en";
        }

        if (string.IsNullOrEmpty(configuredCode))
        {
            return detectedCode;
        }

        if (string.IsNullOrEmpty(detectedCode) || BaseLanguage(configuredCode) == BaseLanguage(detectedCode))
        {
            // Same language: keep the configured spelling (a regional "pt-br" stays).
            return configuredCode;
        }

        var configuredScript = GetScript(configuredCode);
        var detectedScript = GetScript(detectedCode);
        if (configuredScript == null || detectedScript == null)
        {
            // Unknown code, or Serbian (written in both scripts) - can't tell, trust the configuration.
            return configuredCode;
        }

        if (configuredScript != detectedScript)
        {
            return detectedCode;
        }

        if (detectedWordScore >= MinimumWordScore && !AreConfusable(configuredCode, detectedCode))
        {
            return detectedCode;
        }

        return configuredCode;
    }

    /// <summary>
    /// "Latin", "Cyrillic", "Han", ... for a language code, or null when unknown or ambiguous.
    /// </summary>
    internal static string? GetScript(string languageCode)
    {
        var code = languageCode.ToLowerInvariant();
        if (code == "sr-latn")
        {
            return "Latin";
        }

        if (code == "sr-cyrl")
        {
            return "Cyrillic";
        }

        if (code.StartsWith("zh", StringComparison.Ordinal))
        {
            return "Han";
        }

        var baseCode = BaseLanguage(code);
        if (baseCode == "sr")
        {
            return null;
        }

        if (NonLatinScripts.TryGetValue(baseCode, out var script))
        {
            return script;
        }

        var isKnown = baseCode is "nb" or "jv" or "ckb" ||
                      WhisperLanguage.Languages.Any(p => p.Code.Equals(baseCode, StringComparison.OrdinalIgnoreCase));
        return isKnown ? "Latin" : null;
    }

    private static bool AreConfusable(string a, string b)
    {
        var baseA = BaseLanguage(a);
        var baseB = BaseLanguage(b);
        return ConfusableGroups.Any(g => g.Contains(baseA) && g.Contains(baseB));
    }

    private static string BaseLanguage(string code)
    {
        var lower = code.ToLowerInvariant();
        var dash = lower.IndexOf('-');
        return dash > 0 ? lower.Substring(0, dash) : lower;
    }

    private static Dictionary<string, string> BuildNonLatinScripts()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        void Add(string script, params string[] codes)
        {
            foreach (var code in codes)
            {
                map[code] = script;
            }
        }

        Add("Cyrillic", "ru", "uk", "bg", "mk", "be", "kk", "ky", "tg", "mn", "ba", "tt");
        Add("Arabic", "ar", "fa", "ur", "ps", "ckb", "sd", "ug");
        Add("Hebrew", "he", "iw", "yi");
        Add("Greek", "el");
        Add("Thai", "th");
        Add("Lao", "lo");
        Add("Khmer", "km");
        Add("Myanmar", "my");
        Add("Japanese", "ja");
        Add("Korean", "ko");
        Add("Han", "zh", "yue");
        Add("Devanagari", "hi", "mr", "ne", "sa");
        Add("Bengali", "bn", "as");
        Add("Gurmukhi", "pa");
        Add("Gujarati", "gu");
        Add("Tamil", "ta");
        Add("Telugu", "te");
        Add("Kannada", "kn");
        Add("Malayalam", "ml");
        Add("Sinhala", "si");
        Add("Georgian", "ka");
        Add("Armenian", "hy");
        Add("Ethiopic", "am");
        Add("Tibetan", "bo");
        return map;
    }
}
