using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrMega : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Mega";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrMega;
    public override string BackendName => "mega-asr";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => true;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";

    public override List<WhisperLanguage> Languages =>
       new()
       {
            new WhisperLanguage("auto", "Auto detect"),
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("zh", "chinese"),
       };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "mega-asr-1.7b-q4_k.gguf",
                Size = "1.3 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/mega-asr-GGUF/resolve/main/mega-asr-1.7b-q4_k.gguf",
                    "https://huggingface.co/cstr/qwen3-forced-aligner-0.6b-GGUF/resolve/main/qwen3-forced-aligner-0.6b-q8_0.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "mega-asr-1.7b-f16.gguf",
                Size = "4.4 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/mega-asr-GGUF/resolve/main/mega-asr-1.7b-f16.gguf",
                    "https://huggingface.co/cstr/qwen3-forced-aligner-0.6b-GGUF/resolve/main/qwen3-forced-aligner-0.6b-q8_0.gguf"
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrMega;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrMega = value;
    }
}
