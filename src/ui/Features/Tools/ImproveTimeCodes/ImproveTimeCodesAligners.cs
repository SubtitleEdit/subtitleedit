using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

/// <summary>
/// Orders the forced aligners for re-timing, best first for the subtitle's language.
///
/// Re-timing feeds the aligner short windows cut out of running dialogue, which is where
/// the models differ most. Measured on such windows:
/// - a wav2vec2 character aligner for the language is the most precise - a 20 ms grid, and
///   line ends that stop where the speech stops instead of where the next line starts;
/// - the Canary CTC aligner is steady to a frame or two (80 ms) across 25 languages;
/// - the Qwen3 aligner drifts by several hundred ms on line ends, so it is the fallback
///   for the languages only it covers.
/// </summary>
public static class ImproveTimeCodesAligners
{
    // The 25 European languages of NVIDIA Canary v2, which the CTC aligner is cut from.
    private static readonly HashSet<string> CanaryLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "bg", "hr", "cs", "da", "nl", "en", "et", "fi", "fr", "de", "el", "hu", "it",
        "lv", "lt", "mt", "pl", "pt", "ro", "sk", "sl", "es", "sv", "ru", "uk",
    };

    private static readonly HashSet<string> Qwen3Languages = new(StringComparer.OrdinalIgnoreCase)
    {
        "zh", "en", "fr", "de", "it", "ja", "ko", "pt", "ru", "es",
    };

    public static List<ForcedAlignerOption> Rank(string? twoLetterLanguageCode)
    {
        var language = (twoLetterLanguageCode ?? string.Empty).Trim().ToLowerInvariant();
        var all = ForcedAlignerOption.All().Where(o => !o.IsBuiltIn).ToList();

        int Score(ForcedAlignerOption option)
        {
            if (option.Choice == ForcedAlignerOption.CanaryCtcChoice)
            {
                return CanaryLanguages.Contains(language) || language.Length == 0 ? 1 : 3;
            }

            if (option.Choice == ForcedAlignerOption.Qwen3Choice)
            {
                return Qwen3Languages.Contains(language) ? 2 : 4;
            }

            // wav2vec2-aligner-<code>: only any use for its own language.
            return language.Length > 0 && option.Choice.EndsWith("-" + language, StringComparison.OrdinalIgnoreCase) ? 0 : 5;
        }

        // OrderBy is stable, so aligners that tie keep the order of ForcedAlignerOption.All().
        return all.OrderBy(Score).ToList();
    }
}
