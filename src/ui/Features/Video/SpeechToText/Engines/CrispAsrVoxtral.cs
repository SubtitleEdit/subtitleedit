using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Mistral's Voxtral Mini 3B speech-LLM via CrispASR's "voxtral" backend (issue #13724).
/// The backend reports "timestamps-ctc" but not "timestamps-native" in
/// <c>crispasr --list-backends-json</c>, so <see cref="CrispAsrEngineBase.HasNativeTimestamps"/>
/// stays false and the forced-aligner combo drops its "built-in" entry - a CTC aligner
/// is required, not optional.
/// </summary>
public class CrispAsrVoxtral : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Voxtral";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrVoxtral;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";
    public override string BackendName => "voxtral";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => true;

    // The eight languages Voxtral Mini 3B is trained on. "auto" is the model's own
    // prompt-level language detection - the backend has no "language-detect" cap, same
    // as Qwen3/Mega/GLM, which list an auto entry too.
    public override List<WhisperLanguage> Languages =>
        new()
        {
            new WhisperLanguage("auto", "Auto detect"),
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("pt", "portuguese"),
            new WhisperLanguage("nl", "dutch"),
            new WhisperLanguage("hi", "hindi"),
        };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "voxtral-mini-3b-2507-q4_k.gguf",
                Size = "2.65 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/voxtral-mini-3b-2507-GGUF/resolve/main/voxtral-mini-3b-2507-q4_k.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "voxtral-mini-3b-2507-q8_0.gguf",
                Size = "4.99 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/voxtral-mini-3b-2507-GGUF/resolve/main/voxtral-mini-3b-2507-q8_0.gguf"
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
        var modelsFolder = Se.CrispAsrModelsFolder;
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrVoxtral;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrVoxtral = value;
    }
}
