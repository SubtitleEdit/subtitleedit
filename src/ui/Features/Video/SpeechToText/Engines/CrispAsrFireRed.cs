using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrFireRed : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Fire Red";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrFireRed;
    public override string BackendName => "firered-asr";
    public override string DefaultLanguage => "zh";
    public override bool IncludeLanguage => true;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";

  public override List<WhisperLanguage> Languages =>
    new()
    {
        new WhisperLanguage("auto", "Auto detect"),

        // FireRedASR2-AED only transcribes Chinese (+ dialects), English and Cantonese.
        // The "100+ languages" in its docs is the separate FireRedLID module.
        new WhisperLanguage("zh", "chinese"),
        new WhisperLanguage("en", "english"),
        new WhisperLanguage("yue", "cantonese"),
        new WhisperLanguage("wuu", "shanghainese"),
        new WhisperLanguage("nan", "minnan"),
        new WhisperLanguage("gan", "gan"),
        new WhisperLanguage("hak", "hakka"),
        new WhisperLanguage("hsn", "xiang"),
    };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "firered-asr2-aed-q4_k.gguf",
                Size = "1 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/firered-asr2-aed-GGUF/resolve/main/firered-asr2-aed-q4_k.gguf",
                    "https://huggingface.co/cstr/firered-vad-GGUF/resolve/main/firered-vad.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "firered-asr2-aed-q8_0.gguf",
                Size = "1.5 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/firered-asr2-aed-GGUF/resolve/main/firered-asr2-aed-q8_0.gguf",
                    "https://huggingface.co/cstr/firered-vad-GGUF/resolve/main/firered-vad.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "firered-asr2-aed.gguf",
                Size = "2.4 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/firered-asr2-aed-GGUF/resolve/main/firered-asr2-aed.gguf",
                    "https://huggingface.co/cstr/firered-vad-GGUF/resolve/main/firered-vad.gguf"
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrFireRed;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrFireRed = value;
    }
}