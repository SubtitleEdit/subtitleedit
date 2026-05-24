using Nikse.SubtitleEdit.Core.AudioToText;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class ForcedAlignerOption
{
    public const string BuiltInChoice = "built-in";
    public const string CanaryCtcChoice = "canary-ctc";
    public const string Qwen3Choice = "qwen3-forced";

    // wav2vec2 forced aligner choices. The choice string mirrors CrispASR's
    // `-am wav2vec2-aligner-<code>` alias so the persisted setting is self-
    // documenting and lines up with upstream docs.
    public const string Wav2Vec2EnChoice = "wav2vec2-aligner-en";
    public const string Wav2Vec2DeChoice = "wav2vec2-aligner-de";
    public const string Wav2Vec2FrChoice = "wav2vec2-aligner-fr";
    public const string Wav2Vec2EsChoice = "wav2vec2-aligner-es";
    public const string Wav2Vec2ItChoice = "wav2vec2-aligner-it";
    public const string Wav2Vec2JaChoice = "wav2vec2-aligner-ja";
    public const string Wav2Vec2ZhChoice = "wav2vec2-aligner-zh";
    public const string Wav2Vec2NlChoice = "wav2vec2-aligner-nl";
    public const string Wav2Vec2PtChoice = "wav2vec2-aligner-pt";
    public const string Wav2Vec2ArChoice = "wav2vec2-aligner-ar";
    public const string Wav2Vec2UkChoice = "wav2vec2-aligner-uk";
    public const string Wav2Vec2CsChoice = "wav2vec2-aligner-cs";

    public string BaseDisplay { get; }
    public string Display { get; set; }
    public string Choice { get; }
    public string FileName { get; }
    public string Url { get; }
    public string Size { get; }

    public bool IsBuiltIn => Choice == BuiltInChoice;

    public ForcedAlignerOption(string display, string choice, string fileName, string url, string size)
    {
        BaseDisplay = display;
        Display = display;
        Choice = choice;
        FileName = fileName;
        Url = url;
        Size = size;
    }

    public override string ToString() => Display;

    public WhisperModel ToWhisperModel() => new()
    {
        Name = FileName,
        Size = Size,
        Urls = string.IsNullOrEmpty(Url) ? Array.Empty<string>() : new[] { Url },
    };

    public static ForcedAlignerOption BuiltIn() =>
        new("Built-in aligner", BuiltInChoice, string.Empty, string.Empty, string.Empty);

    public static ForcedAlignerOption CanaryCtc() =>
        new("Canary CTC aligner",
            CanaryCtcChoice,
            "canary-ctc-aligner-q8_0.gguf",
            "https://huggingface.co/cstr/canary-ctc-aligner-GGUF/resolve/main/canary-ctc-aligner-q8_0.gguf",
            "650 MB");

    public static ForcedAlignerOption Qwen3() =>
        new("Qwen3 forced aligner",
            Qwen3Choice,
            "qwen3-forced-aligner-0.6b-q8_0.gguf",
            "https://huggingface.co/cstr/qwen3-forced-aligner-0.6b-GGUF/resolve/main/qwen3-forced-aligner-0.6b-q8_0.gguf",
            "986 MB");

    /// <summary>
    /// 12 language-specific wav2vec2 CTC forced aligners shipped by CrispASR
    /// ("WhisperX aligner zoo"). Same Q4_K-quantized GGUFs that WhisperX itself
    /// uses for word-level alignment, hosted on cstr's HuggingFace namespace.
    /// Works on top of any Crisp ASR backend via `-am &lt;path&gt;`.
    /// </summary>
    public static IReadOnlyList<ForcedAlignerOption> Wav2Vec2All() => new[]
    {
        Wav2Vec2(Wav2Vec2EnChoice),
        Wav2Vec2(Wav2Vec2DeChoice),
        Wav2Vec2(Wav2Vec2FrChoice),
        Wav2Vec2(Wav2Vec2EsChoice),
        Wav2Vec2(Wav2Vec2ItChoice),
        Wav2Vec2(Wav2Vec2JaChoice),
        Wav2Vec2(Wav2Vec2ZhChoice),
        Wav2Vec2(Wav2Vec2NlChoice),
        Wav2Vec2(Wav2Vec2PtChoice),
        Wav2Vec2(Wav2Vec2ArChoice),
        Wav2Vec2(Wav2Vec2UkChoice),
        Wav2Vec2(Wav2Vec2CsChoice),
    };

    /// <summary>
    /// Returns the wav2vec2 aligner choice for the given ISO-639-1 language
    /// code (e.g. "ja"), or null when no aligner is available for that
    /// language. Used by the speech-to-text UI to auto-suggest a matching
    /// aligner when the user picks an ASR language.
    /// </summary>
    public static string? Wav2Vec2ChoiceForLanguage(string? languageCode)
    {
        if (string.IsNullOrEmpty(languageCode))
        {
            return null;
        }

        // Normalise common variants. Upstream zh-cn maps to plain "zh".
        var code = languageCode.ToLowerInvariant();
        if (code.StartsWith("zh", StringComparison.Ordinal))
        {
            code = "zh";
        }

        return Wav2Vec2MetaByCode.TryGetValue(code, out var meta) ? meta.Choice : null;
    }

    public static ForcedAlignerOption Wav2Vec2(string choice)
    {
        var code = ExtractLanguageCode(choice);
        if (code != null && Wav2Vec2MetaByCode.TryGetValue(code, out var meta))
        {
            return new ForcedAlignerOption(meta.Display, meta.Choice, meta.FileName, meta.Url, meta.Size);
        }

        // Defensive fallback. Should not happen for any constant we ship.
        return BuiltIn();
    }

    private static string? ExtractLanguageCode(string choice)
    {
        const string prefix = "wav2vec2-aligner-";
        return choice != null && choice.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? choice.Substring(prefix.Length).ToLowerInvariant()
            : null;
    }

    private readonly record struct Wav2Vec2Meta(string Choice, string Display, string FileName, string Url, string Size);

    private const string HfBase = "https://huggingface.co/cstr/";

    // The 12 aliases registered by crispasr_model_registry.cpp. All Q4_K-
    // quantized; sizes from upstream registry. The display strings use the
    // English language name so the combo box is readable regardless of UI
    // language; the wav2vec2 prefix disambiguates from Canary/Qwen3.
    private static readonly IReadOnlyDictionary<string, Wav2Vec2Meta> Wav2Vec2MetaByCode =
        new Dictionary<string, Wav2Vec2Meta>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new(Wav2Vec2EnChoice, "wav2vec2 aligner (English)",
                "wav2vec2-xlsr-en-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-english-GGUF/resolve/main/wav2vec2-xlsr-en-q4_k.gguf",
                "~212 MB"),
            ["de"] = new(Wav2Vec2DeChoice, "wav2vec2 aligner (German)",
                "wav2vec2-large-xlsr-53-german-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-german-GGUF/resolve/main/wav2vec2-large-xlsr-53-german-q4_k.gguf",
                "~300 MB"),
            ["fr"] = new(Wav2Vec2FrChoice, "wav2vec2 aligner (French)",
                "wav2vec2-large-xlsr-53-french-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-french-GGUF/resolve/main/wav2vec2-large-xlsr-53-french-q4_k.gguf",
                "~300 MB"),
            ["es"] = new(Wav2Vec2EsChoice, "wav2vec2 aligner (Spanish)",
                "wav2vec2-large-xlsr-53-spanish-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-spanish-GGUF/resolve/main/wav2vec2-large-xlsr-53-spanish-q4_k.gguf",
                "~300 MB"),
            ["it"] = new(Wav2Vec2ItChoice, "wav2vec2 aligner (Italian)",
                "wav2vec2-large-xlsr-53-italian-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-italian-GGUF/resolve/main/wav2vec2-large-xlsr-53-italian-q4_k.gguf",
                "~300 MB"),
            ["ja"] = new(Wav2Vec2JaChoice, "wav2vec2 aligner (Japanese)",
                "wav2vec2-large-xlsr-53-japanese-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-japanese-GGUF/resolve/main/wav2vec2-large-xlsr-53-japanese-q4_k.gguf",
                "~300 MB"),
            ["zh"] = new(Wav2Vec2ZhChoice, "wav2vec2 aligner (Chinese)",
                "wav2vec2-large-xlsr-53-chinese-zh-cn-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-chinese-zh-cn-GGUF/resolve/main/wav2vec2-large-xlsr-53-chinese-zh-cn-q4_k.gguf",
                "~300 MB"),
            ["nl"] = new(Wav2Vec2NlChoice, "wav2vec2 aligner (Dutch)",
                "wav2vec2-large-xlsr-53-dutch-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-dutch-GGUF/resolve/main/wav2vec2-large-xlsr-53-dutch-q4_k.gguf",
                "~300 MB"),
            ["pt"] = new(Wav2Vec2PtChoice, "wav2vec2 aligner (Portuguese)",
                "wav2vec2-large-xlsr-53-portuguese-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-portuguese-GGUF/resolve/main/wav2vec2-large-xlsr-53-portuguese-q4_k.gguf",
                "~300 MB"),
            ["ar"] = new(Wav2Vec2ArChoice, "wav2vec2 aligner (Arabic)",
                "wav2vec2-large-xlsr-53-arabic-q4_k.gguf",
                HfBase + "wav2vec2-large-xlsr-53-arabic-GGUF/resolve/main/wav2vec2-large-xlsr-53-arabic-q4_k.gguf",
                "~300 MB"),
            ["uk"] = new(Wav2Vec2UkChoice, "wav2vec2 aligner (Ukrainian)",
                "wav2vec2-xls-r-300m-uk-with-small-lm-q4_k.gguf",
                HfBase + "wav2vec2-xls-r-300m-uk-with-small-lm-GGUF/resolve/main/wav2vec2-xls-r-300m-uk-with-small-lm-q4_k.gguf",
                "~300 MB"),
            ["cs"] = new(Wav2Vec2CsChoice, "wav2vec2 aligner (Czech)",
                "wav2vec2-xls-r-300m-cs-250-q4_k.gguf",
                HfBase + "wav2vec2-xls-r-300m-cs-250-GGUF/resolve/main/wav2vec2-xls-r-300m-cs-250-q4_k.gguf",
                "~300 MB"),
        };

    public static IReadOnlyList<ForcedAlignerOption> All()
    {
        var list = new List<ForcedAlignerOption>
        {
            BuiltIn(),
            CanaryCtc(),
            Qwen3(),
        };
        list.AddRange(Wav2Vec2All());
        return list;
    }

    public static ForcedAlignerOption FromChoice(string? choice)
    {
        if (string.IsNullOrEmpty(choice))
        {
            return BuiltIn();
        }

        if (choice.StartsWith("wav2vec2-aligner", StringComparison.OrdinalIgnoreCase))
        {
            return Wav2Vec2(choice);
        }

        return choice switch
        {
            CanaryCtcChoice => CanaryCtc(),
            Qwen3Choice => Qwen3(),
            _ => BuiltIn(),
        };
    }
}
