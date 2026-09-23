using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrSenseVoice : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR SenseVoice";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrSenseVoice;
    public override string BackendName => "sensevoice";
    public override string DefaultLanguage => "auto";
    public override bool IncludeLanguage => true;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";

    // SenseVoice-Small (FunAudioLLM) officially covers these five languages plus
    // native language identification ("auto"), emotion and audio-event tags.
    public override List<WhisperLanguage> Languages =>
        new()
        {
            new WhisperLanguage("auto", "Auto detect"),
            new WhisperLanguage("zh", "chinese"),
            new WhisperLanguage("yue", "cantonese"),
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("ja", "japanese"),
            new WhisperLanguage("ko", "korean"),
        };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "sensevoice-small-q4_k.gguf",
                Size = "136 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/sensevoice-small-GGUF/resolve/main/sensevoice-small-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "sensevoice-small-q8_0.gguf",
                Size = "252 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/sensevoice-small-GGUF/resolve/main/sensevoice-small-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "sensevoice-small-f16.gguf",
                Size = "469 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/sensevoice-small-GGUF/resolve/main/sensevoice-small-f16.gguf",
                ],
            },
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

        var length = new FileInfo(modelFile).Length;
        return length > 10_000_000 && !PreCmvnModelSizes.Contains(length);
    }

    /// <summary>
    /// Byte sizes of the GGUFs published before 2026-09-23, when cstr/sensevoice-small-GGUF
    /// was re-converted with the am.mvn CMVN tensors (sensevoice.cmvn_shift/scale) that
    /// CrispASR v0.8.36 applies. The old files still transcribe but crispasr warns on every run
    /// and their emotion/event tags are wrong, so they count as not installed: the model dot
    /// goes grey and Transcribe offers the download, which replaces the file. The re-converted
    /// files are exactly 4,608 bytes larger, so the size alone tells them apart.
    /// </summary>
    private static readonly HashSet<long> PreCmvnModelSizes =
    [
        135_549_024, // sensevoice-small-q4_k.gguf
        251_670_368, // sensevoice-small-q8_0.gguf
        469_397_824, // sensevoice-small-f16.gguf
    ];

    /// <summary>
    /// True when the model file exists but is one of the pre-CMVN conversions.
    /// </summary>
    public bool IsModelOutdated(WhisperModel model)
    {
        var modelFile = GetModelForCmdLine(model.Name);
        return File.Exists(modelFile) && PreCmvnModelSizes.Contains(new FileInfo(modelFile).Length);
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrSenseVoice;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrSenseVoice = value;
    }
}
