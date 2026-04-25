using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

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

    public override List<WhisperLanguage> Languages =>
       new()
       {
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

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "qwen3-asr-1.7b-q4_k.gguf",
                Size = "1.33 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-1.7b-GGUF/resolve/main/qwen3-asr-1.7b-q4_k.gguf",
                    "https://huggingface.co/cstr/qwen3-forced-aligner-0.6b-GGUF/resolve/main/qwen3-forced-aligner-0.6b-q8_0.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-1.7b-q8_0.gguf",
                Size = "2.51 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-1.7b-GGUF/resolve/main/qwen3-asr-1.7b-q8_0.gguf",
                    "https://huggingface.co/cstr/qwen3-forced-aligner-0.6b-GGUF/resolve/main/qwen3-forced-aligner-0.6b-q8_0.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "qwen3-asr-1.7b-f16.gguf",
                Size = "4.7 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/qwen3-asr-1.7b-GGUF/resolve/main/qwen3-asr-1.7b-f16.gguf",
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
        return Name;
    }

    public override string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.SpeechToTextFolder;
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        var folder = Path.Combine(baseFolder, "CrispASR");
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrQwen3;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrQwen3 = value;
    }
}