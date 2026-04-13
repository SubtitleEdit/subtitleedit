using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrCohere : ISpeechToTextEngine
{
    public static string StaticName => "Crisp ASR Cohere";
    public string Name => StaticName;
    public string Choice => WhisperChoice.CrispAsrCohere;
    public string Url => "https://github.com/CrispStrobe/CrispASR";

    public List<WhisperLanguage> Languages =>
       new()
       {
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("pt", "portuguese"),
            new WhisperLanguage("el", "greek"),
            new WhisperLanguage("nl", "dutch"),
            new WhisperLanguage("pl", "polish"),

            new WhisperLanguage("zh", "chinese"),
            new WhisperLanguage("ja", "japanese"),
            new WhisperLanguage("ko", "korean"),
            new WhisperLanguage("vi", "vietnamese"),

            new WhisperLanguage("ar", "arabic"),
       };

    public List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "cohere-transcribe-q4_k.gguf",
                Size = "1.51 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/cohere-transcribe-03-2026-GGUF/resolve/main/cohere-transcribe-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "cohere-transcribe-q5_0.gguf",
                Size = "1.74 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/cohere-transcribe-03-2026-GGUF/resolve/main/cohere-transcribe-q5_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "cohere-transcribe-q5_1.gguf",
                Size = "1.85 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/cohere-transcribe-03-2026-GGUF/resolve/main/cohere-transcribe-q5_1.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "cohere-transcribe-q6_k.gguf",
                Size = "1.98 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/cohere-transcribe-03-2026-GGUF/resolve/main/cohere-transcribe-q6_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "cohere-transcribe-q8_0.gguf",
                Size = "2.42 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/cohere-transcribe-03-2026-GGUF/resolve/main/cohere-transcribe-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "cohere-transcribe.gguf",
                Size = "4.14 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/cohere-transcribe-03-2026-GGUF/resolve/main/cohere-transcribe.gguf",
                ],
            },

       };

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public override string ToString()
    {
        return Name;
    }

    public string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.WhisperFolder;
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

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var folder = GetAndCreateWhisperFolder();
        var modelsFolder = Path.Combine(folder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public string GetExecutable()
    {
        string fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        var modelFile = GetModelForCmdLine(model.Name);
        if (!File.Exists(modelFile))
        {
            return false;
        }

        return new FileInfo(modelFile).Length > 10_000_000;
    }

    public string GetModelForCmdLine(string modelName)
    {
        var modelFileName = Path.Combine(GetAndCreateWhisperModelFolder(null), modelName);
        return modelFileName;
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

    internal static string GetExecutableFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "crispasr.exe";
        }

        return "crispasr";
    }

    public bool CanBeDownloaded()
    {
        return true;
    }
}