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

public class ChatLlmCppEngine : ISpeechToTextEngine
{
    public static string StaticName => "Chat LLM.cpp";
    public string Name => StaticName;
    public string Choice => WhisperChoice.ChatLlm;
    public string Url => "https://github.com/foldl/chatllm.cpp";

    public List<WhisperLanguage> Languages
    {
        get
        {
            return new List<WhisperLanguage>
            {
                new WhisperLanguage("en", "english"),
                new WhisperLanguage("zh", "chinese"),
                new WhisperLanguage("yue", "cantonese"),
                new WhisperLanguage("fr", "french"),
                new WhisperLanguage("de", "german"),
                new WhisperLanguage("it", "italian"),
                new WhisperLanguage("ja", "japanese"),
                new WhisperLanguage("ko", "korean"),
                new WhisperLanguage("pt", "portuguese"),
                new WhisperLanguage("ru", "russian"),
                new WhisperLanguage("es", "spanish"),
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
                    Name = "qwen3-asr-0.6b.bin",
                    Size = "1 GB",
                    Urls =
                    [
                        "https://modelscope.cn/models/judd2024/chatllm_quantized_qwen3/resolve/master/qwen3-asr-0.6b.bin"
                    ]
                },
                new WhisperModel
                {
                    Name = "qwen3-asr-1.7b.bin",
                    Size = "2.5 GB",
                    Urls =
                    [
                        "https://modelscope.cn/models/judd2024/chatllm_quantized_qwen3/resolve/master/qwen3-asr-1.7b.bin"
                    ]
                },
            }.ToList();
        }
    }

    public WhisperModel ForcedAlignerModel => new WhisperModel
    {
        Name = "qwen3-focedaligner-0.6b.bin",
        Size = "1 GB",
        Urls = [ "https://modelscope.cn/models/judd2024/chatllm_quantized_qwen3/resolve/master/qwen3-focedaligner-0.6b.bin" ],
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

        var folder = Path.Combine(baseFolder, "ChatLLM.cpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var baseFolder = GetAndCreateWhisperFolder();
        return baseFolder;
    }

    public string GetExecutable()
    {
        var fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        var baseFolder = GetAndCreateWhisperFolder();
        var modelFileName = Path.Combine(baseFolder, model.Name);
        if (!File.Exists(modelFileName))
        {
            return false;
        }

        var fileInfo = new FileInfo(modelFileName);
        return fileInfo.Length > 10_000_000;
    }

    public string GetModelForCmdLine(string modelName)
    {
        var baseFolder = GetAndCreateWhisperFolder();
        var modelFileName = Path.Combine(baseFolder, modelName);
        return modelFileName;
    }

    public override string ToString()
    {
        return Name;
    }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/Whisper/{assetName}");

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
            return "main.exe";
        }

        return "main";
    }

    public bool CanBeDownloaded()
    {
        return true;
    }
}
