using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrCanary : ICrispAsrEngine
{
    public static string StaticName => "Crisp ASR Canary";
    public string Name => StaticName;
    public string Choice => WhisperChoice.CrispAsrCanary;
    public string Url => "https://github.com/CrispStrobe/CrispASR";
    public string BackendName => "canary";
    public string DefaultLanguage => "en";
    public bool IncludeLanguage => true;

    public List<WhisperLanguage> Languages =>
       new()
       {
            new WhisperLanguage("bg", "bulgarian"),
            new WhisperLanguage("hr", "croatian"),
            new WhisperLanguage("cs", "czech"),
            new WhisperLanguage("da", "danish"),
            new WhisperLanguage("nl", "dutch"),
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("et", "estonian"),
            new WhisperLanguage("fi", "finnish"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("el", "greek"),
            new WhisperLanguage("hu", "hungarian"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("lv", "latvian"),
            new WhisperLanguage("lt", "lithuanian"),
            new WhisperLanguage("mt", "maltese"),
            new WhisperLanguage("pl", "polish"),
            new WhisperLanguage("pt", "portuguese"),
            new WhisperLanguage("ro", "romanian"),
            new WhisperLanguage("sk", "slovak"),
            new WhisperLanguage("sl", "slovenian"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("sv", "swedish"),
            new WhisperLanguage("ru", "russian"),
            new WhisperLanguage("uk", "ukrainian"),
       };

    public List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "canary-1b-v2-q4_k.gguf",
                Size = "705 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/canary-1b-v2-GGUF/resolve/main/canary-1b-v2-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "canary-1b-v2-q5_0.gguf",
                Size = "815 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/canary-1b-v2-GGUF/resolve/main/canary-1b-v2-q5_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "canary-1b-v2-q8_0.gguf",
                Size = "1.14 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/canary-1b-v2-GGUF/resolve/main/canary-1b-v2-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "canary-1b-v2.gguf",
                Size = "1.97 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/canary-1b-v2-GGUF/resolve/main/canary-1b-v2.gguf",
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

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrCanary;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrCanary = value;
    }
}