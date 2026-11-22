using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrQwen3 : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Qwen3";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrQwen3;
    public override string BackendName => "qwen3";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => true;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";

    /// <summary>
    /// The first CrispASR that runs Rumble-2-Flash: v0.8.55 added its 8 s encoder attention
    /// windows and the Reson8 control-token system prefix to the qwen3 runtime (#602).
    /// </summary>
    public static readonly Version RumbleMinimumCrispAsrVersion = new(0, 8, 55);

    public override Version? GetMinimumCrispAsrVersion(WhisperModel model) =>
        IsRumbleModel(model.Name) ? RumbleMinimumCrispAsrVersion : null;

    internal static bool IsRumbleModel(string? modelName) =>
        modelName != null && modelName.StartsWith("rumble-", StringComparison.OrdinalIgnoreCase);

    public override List<WhisperLanguage> Languages =>
       new()
       {
            new WhisperLanguage("auto", "Auto detect"),
            new WhisperLanguage("zh", "chinese"),
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("yue", "cantonese"),
            new WhisperLanguage("ar", "arabic"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("pt", "portuguese"),
            new WhisperLanguage("id", "indonesian"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("ko", "korean"),
            new WhisperLanguage("ru", "russian"),
            new WhisperLanguage("th", "thai"),
            new WhisperLanguage("vi", "vietnamese"),
            new WhisperLanguage("ja", "japanese"),
            new WhisperLanguage("tr", "turkish"),
            new WhisperLanguage("hi", "hindi"),
            new WhisperLanguage("ms", "malay"),
            new WhisperLanguage("nl", "dutch"),
            new WhisperLanguage("sv", "swedish"),
            new WhisperLanguage("da", "danish"),
            new WhisperLanguage("fi", "finnish"),
            new WhisperLanguage("pl", "polish"),
            new WhisperLanguage("cs", "czech"),
            new WhisperLanguage("fil", "filipino"),
            new WhisperLanguage("fa", "persian"),
            new WhisperLanguage("el", "greek"),
            new WhisperLanguage("hu", "hungarian"),
            new WhisperLanguage("mk", "macedonian"),
            new WhisperLanguage("ro", "romanian"),
       };

    // qwen3-asr-1.7b-q4_k.gguf: the pre-2026-07-10 upload produced empty transcripts
    // on CrispASR v0.8.9 (sub-8-bit audio-encoder drift, CrispASR issue #240). Upstream
    // re-baked it with a Q8_0 audio-tower floor and replaced the file in place
    // (now 1.49 GB, sha256 ec197cef...; the broken one was 1f1d26ee...). Verified OK
    // against the pinned v0.8.9 binary. Users who downloaded the old file must delete
    // it from CrispASR/models and re-download - same filename, so SE can't tell them apart.
    //
    // Rumble-2-Flash (Reson8, Apache-2.0) leads: a 0.5B Qwen3-ASR fine-tune for 45 languages on
    // the same qwen3 runtime. On FLEURS dev (30 sentences each of da/de/en/sv/fr/pl, FireRedVAD
    // as SE runs it, CrispASR v0.8.55, 2026-11-22) q8_0 scored 9.0% pooled WER against 16.1%
    // for qwen3-asr-1.7b-q4_k and 23.4% for 0.6b-q4_k, and was the best in all six languages
    // (Danish 11.9% vs 24.6%). q8_0 beat Rumble's own q4_k (11.4%) clearly, so it goes first.
    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "rumble-2-flash-q8_0.gguf",
                Size = "657 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/rumble-2-flash-GGUF/resolve/main/rumble-2-flash-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "rumble-2-flash-q4_k.gguf",
                Size = "445 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/rumble-2-flash-GGUF/resolve/main/rumble-2-flash-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-1.7b-q4_k.gguf",
                Size = "1.49 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-1.7b-GGUF/resolve/main/qwen3-asr-1.7b-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-1.7b-q8_0.gguf",
                Size = "2.51 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-1.7b-GGUF/resolve/main/qwen3-asr-1.7b-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-1.7b-f16.gguf",
                Size = "4.7 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-1.7b-GGUF/resolve/main/qwen3-asr-1.7b-f16.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-1.7b-ja-anime-q4_k.gguf",
                Size = "1.49 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-1.7b-ja-anime-GGUF/resolve/main/qwen3-asr-1.7b-ja-anime-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-1.7b-ja-anime-q8_0.gguf",
                Size = "2.51 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-1.7b-ja-anime-GGUF/resolve/main/qwen3-asr-1.7b-ja-anime-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-0.6b-q4_k.gguf",
                Size = "631 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-0.6b-GGUF/resolve/main/qwen3-asr-0.6b-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-0.6b-q8_0.gguf",
                Size = "1.01 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-0.6b-GGUF/resolve/main/qwen3-asr-0.6b-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-0.6b-f16.gguf",
                Size = "1.88 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-0.6b-GGUF/resolve/main/qwen3-asr-0.6b-f16.gguf",
                ],
            },
       };

    public WhisperModel ForcedAlignerModel => new WhisperModel
    {
        Name = "qwen3-forced-aligner-0.6b-q8_0.gguf",
        Size = "986 MB",
        Urls = ["https://huggingface.co/cstr/qwen3-forced-aligner-0.6b-GGUF/resolve/main/qwen3-forced-aligner-0.6b-q8_0.gguf"],
    };

    public override string Extension => string.Empty;
    public override string UnpackSkipFolder => string.Empty;

    public override bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public override string ToString()
    {
        return CrispAsrEngine.GetBackendDisplayName(this);
    }

    public override string GetAndCreateWhisperFolder()
    {
        var folder = Se.CrispAsrFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public override string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var folder = GetAndCreateWhisperFolder();
        var modelsFolder = Path.Combine(folder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public override string GetExecutable()
    {
        string fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public override bool IsModelInstalled(WhisperModel model)
    {
        var modelFile = GetModelForCmdLine(model.Name);
        if (!File.Exists(modelFile))
        {
            return false;
        }

        return new FileInfo(modelFile).Length > 10_000_000;
    }

    public override string GetModelForCmdLine(string modelName)
    {
        var modelFileName = Path.Combine(GetAndCreateWhisperModelFolder(null), modelName);
        return modelFileName;
    }


    public override string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileNameOnly = Path.GetFileName(url);
        var fileName = Path.Combine(folder, fileNameOnly);
        return fileName;
    }

    internal static string GetExecutableFileName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "crispasr.exe";
        }

        return "crispasr";
    }

    public override bool CanBeDownloaded()
    {
        return true;
    }

    public override string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrQwen3;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrQwen3 = value;
    }
}