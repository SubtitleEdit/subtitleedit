using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
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
///
/// English is the exception: there Canary leads. The English wav2vec2 model is quantised and
/// lost its place for the next lines after a line with words the subtitle leaves out ("Honestly,
/// I know"), and on real film dialogue Canary put every line on its speech where wav2vec2 left
/// lines for the user to check. Canary's 80 ms grid costs little next to that.
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

    /// <summary>
    /// Whether the speech-to-text check can hear this language: Parakeet v3 knows the same 25
    /// European languages as Canary. An unknown language is left to Parakeet to detect.
    /// </summary>
    public static bool CanCheckWithSpeechToText(string? twoLetterLanguageCode)
    {
        var language = (twoLetterLanguageCode ?? string.Empty).Trim();
        return language.Length == 0 || CanaryLanguages.Contains(language);
    }

    /// <summary>
    /// The Parakeet model to check with: an installed one that knows the language, best first,
    /// or - when none is - the smallest multilingual one, to be downloaded.
    /// </summary>
    public static WhisperModel PickSpeechToTextModel(CrispAsrParakeet engine, string? twoLetterLanguageCode, Func<WhisperModel, bool> isInstalled)
    {
        var isEnglish = string.Equals(twoLetterLanguageCode, "en", StringComparison.OrdinalIgnoreCase);
        var multilingual = engine.Models
            .Where(m => m.Name.StartsWith("parakeet-ultra-", StringComparison.Ordinal) ||
                        m.Name.StartsWith("parakeet-tdt-0.6b-v3", StringComparison.Ordinal))
            .ToList();

        // Ultra is v3 post-trained, with a lower error rate everywhere; the English-only models
        // are only any use for English. Phonon-2 is v3 retrained for English, so it leads those.
        var candidates = multilingual.Where(m => m.Name.StartsWith("parakeet-ultra-", StringComparison.Ordinal))
            .Concat(multilingual.Where(m => m.Name.StartsWith("parakeet-tdt-0.6b-v3", StringComparison.Ordinal)))
            .Concat(isEnglish
                ? engine.Models.Where(m => m.Name.StartsWith("phonon2-", StringComparison.Ordinal) ||
                                           m.Name.StartsWith("parakeet-tdt-1.1b", StringComparison.Ordinal) ||
                                           m.Name.StartsWith("parakeet-rnnt-", StringComparison.Ordinal))
                : Enumerable.Empty<WhisperModel>());

        return candidates.FirstOrDefault(isInstalled)
               ?? multilingual.First(m => m.Name == "parakeet-tdt-0.6b-v3-q4_k.gguf");
    }

    public static List<ForcedAlignerOption> Rank(string? twoLetterLanguageCode)
    {
        var language = (twoLetterLanguageCode ?? string.Empty).Trim().ToLowerInvariant();
        var all = ForcedAlignerOption.All().Where(o => !o.IsBuiltIn).ToList();

        double Score(ForcedAlignerOption option)
        {
            if (option.Choice == ForcedAlignerOption.CanaryCtcChoice)
            {
                return CanaryLanguages.Contains(language) || language.Length == 0 ? 1 : 3;
            }

            if (option.Choice == ForcedAlignerOption.Qwen3Choice)
            {
                return Qwen3Languages.Contains(language) ? 2 : 4;
            }

            // wav2vec2-aligner-<code>: only any use for its own language - and for English, second to Canary.
            if (language.Length > 0 && option.Choice.EndsWith("-" + language, StringComparison.OrdinalIgnoreCase))
            {
                return language == "en" ? 1.5 : 0;
            }

            return 5;
        }

        // OrderBy is stable, so aligners that tie keep the order of ForcedAlignerOption.All().
        return all.OrderBy(Score).ToList();
    }
}
