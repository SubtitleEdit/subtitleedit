using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class Qwen3AsrCppEngine : ISpeechToTextEngine
{
    public static string StaticName => "Qwen3 ASR CPP";
    public string Name => StaticName;
    public string Choice => WhisperChoice.Qwen3AsrCpp;
    public string Url => "https://github.com/woct0rdho/qwen3-asr.cpp";

    public List<WhisperLanguage> Languages
    {
        get
        {
            return new List<WhisperLanguage>
            {
                new WhisperLanguage("zh", "chinese"),
                new WhisperLanguage("en", "english"),
                new WhisperLanguage("yue", "cantonese"),
                new WhisperLanguage("ja", "japanese"),
                new WhisperLanguage("ko", "korean"),
                new WhisperLanguage("de", "german"),
                new WhisperLanguage("fr", "french"),
                new WhisperLanguage("es", "spanish"),
                new WhisperLanguage("it", "italian"),
                new WhisperLanguage("pt", "portuguese"),
                new WhisperLanguage("ru", "russian"),
                new WhisperLanguage("ar", "arabic"),
                new WhisperLanguage("hi", "hindi"),
                new WhisperLanguage("th", "thai"),
                new WhisperLanguage("vi", "vietnamese"),
                new WhisperLanguage("id", "indonesian"),
                new WhisperLanguage("ms", "malay"),
                new WhisperLanguage("tr", "turkish"),
                new WhisperLanguage("pl", "polish"),
                new WhisperLanguage("nl", "dutch"),
                new WhisperLanguage("sv", "swedish"),
                new WhisperLanguage("no", "norwegian"),
                new WhisperLanguage("da", "danish"),
                new WhisperLanguage("fi", "finnish"),
                new WhisperLanguage("el", "greek"),
                new WhisperLanguage("cs", "czech"),
                new WhisperLanguage("hu", "hungarian"),
                new WhisperLanguage("ro", "romanian"),
                new WhisperLanguage("uk", "ukrainian"),
                new WhisperLanguage("he", "hebrew"),
            };
        }
    }

    public List<WhisperModel> Models
    {
        get
        {
            return new[]
            {
                new WhisperModel
                {
                    Name = "qwen3-asr-0.6b-f16.gguf",
                    Size = "1.9 GB",
                    Urls =
                    [
                        "https://huggingface.co/OpenVoiceOS/qwen3-asr-0.6b-f16/resolve/main/qwen3-asr-0.6b-f16.gguf"
                    ]
                },
                new WhisperModel
                {
                    Name = "qwen3-asr-0.6b-q8_0.gguf",
                    Size = "1.3 GB",
                    Urls =
                    [
                        "https://huggingface.co/OpenVoiceOS/qwen3-asr-0.6b-q8-0/resolve/main/qwen3-asr-0.6b-q8_0.gguf"
                    ]
                },
                new WhisperModel
                {
                    Name = "qwen3-asr-1.7b-f16.gguf",
                    Size = "4.4 GB",
                    Urls =
                    [
                        "https://huggingface.co/FlippyDora/qwen3-asr-1.7b-GGUF/resolve/main/qwen3-asr-1.7b-f16.gguf"
                    ]
                },
                new WhisperModel
                {
                    Name = "qwen3-asr-1.7b-q8_0.gguf",
                    Size = "3.0 GB",
                    Urls =
                    [
                        "https://huggingface.co/FlippyDora/qwen3-asr-1.7b-GGUF/resolve/main/qwen3-asr-1.7b-q8_0.gguf"
                    ]
                },
            }.ToList();
        }
    }


    public WhisperModel ForcedAlignerModel => new WhisperModel
    {
        Name = "qwen3-forced-aligner-0.6b-q4_k_m.gguf",
        Size = "616 MB",
        Urls = ["https://huggingface.co/OpenVoiceOS/qwen3-forced-aligner-0.6b-q4-k-m/resolve/main/qwen3-forced-aligner-0.6b-q4_k_m.gguf"],
    };

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.DataFolder;
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        var folder = Path.Combine(baseFolder, "Qwen3ASR");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var baseFolder = GetAndCreateWhisperFolder();
        var modelsFolder = Path.Combine(baseFolder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public string GetExecutable()
    {
        var fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        var modelsFolder = GetAndCreateWhisperModelFolder(null);
        var modelFileName = Path.Combine(modelsFolder, model.Name);
        if (!File.Exists(modelFileName))
        {
            return false;
        }

        var fileInfo = new FileInfo(modelFileName);
        return fileInfo.Length > 10_000_000;
    }

    public string GetModelForCmdLine(string modelName)
    {
        var modelsFolder = GetAndCreateWhisperModelFolder(null);
        var modelFileName = Path.Combine(modelsFolder, modelName);
        return modelFileName;
    }

    public override string ToString()
    {
        return Name;
    }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{assetName}");

        await using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);

        var contents = await reader.ReadToEndAsync();
        return contents;
    }

    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileNameOnly = Path.GetFileName(url);
        var fileName = Path.Combine(folder, fileNameOnly);
        return fileName;
    }

    public string GetExecutableFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "qwen3-asr-cli.exe";
        }

        return "qwen3-asr-cli";
    }

    public bool CanBeDownloaded()
    {
        return true;
    }
}
