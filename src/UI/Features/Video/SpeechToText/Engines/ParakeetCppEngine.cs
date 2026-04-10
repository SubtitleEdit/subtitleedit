using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class ParakeetCppEngine : ISpeechToTextEngine
{
    public static string StaticName => "Parakeet.cpp";
    public string Name => StaticName;
    public string Choice => WhisperChoice.ParakeetCpp;
    public string Url => "https://github.com/Frikallo/parakeet.cpp";

    // The 110M model is English-only; the 600M TDT model is multilingual.
    // Languages here reflect what the 600M model supports.
    public List<WhisperLanguage> Languages =>
        new()
        {
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("pt", "portuguese"),
            new WhisperLanguage("zh", "chinese"),
            new WhisperLanguage("ja", "japanese"),
            new WhisperLanguage("ko", "korean"),
            new WhisperLanguage("ru", "russian"),
            new WhisperLanguage("pl", "polish"),
            new WhisperLanguage("tr", "turkish"),
            new WhisperLanguage("nl", "dutch"),
        };

    // Each model requires both a .safetensors weights file and a vocab.txt file.
    // The Urls list contains both downloads in order: [weights, vocab].
    public List<WhisperModel> Models =>
        new()
        {
            new WhisperModel
            {
                Name = "parakeet-tdt_ctc-110m",
                Size = "450 MB",
                Urls =
                [
                    "https://huggingface.co/nvidia/parakeet-tdt_ctc-110m/resolve/main/model.safetensors",
                    "https://huggingface.co/nvidia/parakeet-tdt_ctc-110m/resolve/main/vocab.txt",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-600m",
                Size = "2.4 GB",
                Urls =
                [
                    "https://huggingface.co/nvidia/parakeet-tdt-600m/resolve/main/model.safetensors",
                    "https://huggingface.co/nvidia/parakeet-tdt-600m/resolve/main/vocab.txt",
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

    public string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.DataFolder;
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        var folder = Path.Combine(baseFolder, "parakeet.cpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    // Each model gets its own sub-folder to keep weights + vocab together.
    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var baseFolder = GetAndCreateWhisperFolder();
        if (whisperModel == null)
        {
            return baseFolder;
        }

        var modelFolder = Path.Combine(baseFolder, whisperModel.Name);
        if (!Directory.Exists(modelFolder))
        {
            Directory.CreateDirectory(modelFolder);
        }

        return modelFolder;
    }

    public string GetExecutable()
    {
        return Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
    }

    // A model is considered installed when both model.safetensors and vocab.txt
    // are present in the model sub-folder and the weights file is non-trivially sized.
    public bool IsModelInstalled(WhisperModel model)
    {
        var modelFolder = GetAndCreateWhisperModelFolder(model);

        var weightsFile = Path.Combine(modelFolder, "model.safetensors");
        if (!File.Exists(weightsFile) || new FileInfo(weightsFile).Length < 10_000_000)
        {
            return false;
        }

        var vocabFile = Path.Combine(modelFolder, "vocab.txt");
        return File.Exists(vocabFile);
    }

    // Returns the path to the model.safetensors file used on the CLI:
    //   parakeet <model.safetensors> <audio.wav> --vocab <vocab.txt>
    public string GetModelForCmdLine(string modelName)
    {
        var modelFolder = Path.Combine(GetAndCreateWhisperFolder(), modelName);
        return Path.Combine(modelFolder, "model.safetensors");
    }

    // Convenience helper so callers can build the --vocab argument.
    public string GetVocabForCmdLine(string modelName)
    {
        var modelFolder = Path.Combine(GetAndCreateWhisperFolder(), modelName);
        return Path.Combine(modelFolder, "vocab.txt");
    }

    // Returns the download destination for a given URL inside the model's folder.
    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileNameOnly = Path.GetFileName(url);
        return Path.Combine(folder, fileNameOnly);
    }

    public string GetExecutableFileName() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "parakeet.exe" : "parakeet";

    public bool CanBeDownloaded() => true;

    public override string ToString() => Name;

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{assetName}");

        await using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}