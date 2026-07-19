using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrMossDiarize : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR MOSS Diarize";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrMossDiarize;
    public override string BackendName => "moss-diarize";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => false;
    public override bool HasNativeTimestamps => true;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";

    public override List<WhisperLanguage> Languages =>
       new()
       {
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("zh", "chinese"),
       };

    // q4_k is intentionally absent: it produces ~1.7x compressed timestamps (verified against
    // the jfk.wav ground truth in the model card with CrispASR v0.8.10) - same sub-8-bit
    // encoder drift that broke qwen3-asr q4_k. q8_0 and f16 both match the ground truth.
    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "moss-transcribe-diarize-0.9b-q8_0.gguf",
                Size = "1.41 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/MOSS-Transcribe-Diarize-GGUF/resolve/main/moss-transcribe-diarize-0.9b-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "moss-transcribe-diarize-0.9b-f16.gguf",
                Size = "1.82 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/MOSS-Transcribe-Diarize-GGUF/resolve/main/moss-transcribe-diarize-0.9b-f16.gguf",
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrMossDiarize;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrMossDiarize = value;
    }
}
