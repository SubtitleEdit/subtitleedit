using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class Qwen3AsrCppEngine : ISpeechToTextEngine
{
    public static string StaticName => "Qwen3 ASR CPP";
    public string Name => StaticName;
    public string Choice => WhisperChoice.Qwen3AsrCpp;
    public string Url => "https://github.com/niksedk/qwen3-asr.cpp";

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
            var models = new[]
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
                    // Danish fine-tune of Qwen3-ASR-0.6B (LoRA on 399 h CoRal/NST/FTSpeech, merged):
                    // 18.8 vs 43.1 WER for the stock 0.6B on 600 held-out Danish clips. Needs
                    // qwen3-asr.cpp v0.1.9+ (mel filterbank fix). Apache-2.0 with CoRal's
                    // OpenRAIL-D restrictions (no speech synthesis, no biometric identification).
                    Name = "qwen3-asr-0.6b-da-q8_0.gguf",
                    Size = "1.0 GB",
                    Urls =
                    [
                        "https://huggingface.co/niksedk/qwen3-asr-0.6b-danish/resolve/main/qwen3-asr-0.6b-da-q8_0.gguf"
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

            AddCustomModels(models);
            return models;
        }
    }

    /// <summary>
    /// Lists any extra .gguf files the user dropped into the models folder (or imported via
    /// "Add custom model...") that are not in the predefined list and are not the forced
    /// aligner, so they show up in the model dropdown like whisper.cpp custom models do.
    /// </summary>
    private void AddCustomModels(List<WhisperModel> models)
    {
        var modelsFolder = GetAndCreateWhisperModelFolder(null);
        if (!Directory.Exists(modelsFolder))
        {
            return;
        }

        var known = new HashSet<string>(models.Select(m => m.Name), StringComparer.OrdinalIgnoreCase)
        {
            ForcedAlignerModel.Name,
        };

        string[] files;
        try
        {
            files = Directory.GetFiles(modelsFolder, "*.gguf", SearchOption.TopDirectoryOnly);
        }
        catch
        {
            return;
        }

        foreach (var file in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(file);
            if (known.Contains(name) || name.StartsWith("qwen3-forced-aligner", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (new FileInfo(file).Length < MinimumModelFileSize)
            {
                continue;
            }

            models.Add(new WhisperModel
            {
                Name = name,
                Size = "custom",
                Urls = [],
            });
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

    private const long MinimumModelFileSize = 10_000_000;

    public bool SupportsCustomModels => true;

    public bool CustomModelIsFolder => false;

    public string CustomModelFileTypeName => "Qwen3 ASR GGUF model (*.gguf)";

    public IReadOnlyList<string> CustomModelFilePatterns => ["*.gguf"];

    public string CustomModelHelpText => Se.Language.Video.AudioToText.CustomModelHelpGguf;

    /// <summary>
    /// Copies a user-supplied qwen3-asr.cpp GGUF into the models folder. Rejects the other
    /// "Qwen3-ASR GGUF" layouts (llama.cpp mmproj, transcribe.cpp, CapsWriter decoder-only)
    /// up front, since the engine itself would just crash on them (issue #16232).
    /// </summary>
    public string ImportCustomModel(string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Model file not found.", sourcePath);
        }

        if (!sourcePath.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("A Qwen3 ASR CPP model must be a '.gguf' file.");
        }

        if (new FileInfo(sourcePath).Length < MinimumModelFileSize)
        {
            throw new Exception("The model file is too small (must be at least 10 MB) - is it really a Qwen3 ASR model?");
        }

        var reason = Qwen3AsrCppModelValidator.GetIncompatibilityReason(sourcePath);
        if (reason != null)
        {
            throw new Exception(reason);
        }

        var fileName = Path.GetFileName(sourcePath);
        if (fileName.StartsWith("qwen3-forced-aligner", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("This is the forced aligner model, not a speech recognition model. Subtitle Edit downloads the aligner itself.");
        }

        var destination = Path.Combine(GetAndCreateWhisperModelFolder(null), fileName);
        if (!string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(sourcePath, destination, true);
        }

        return fileName;
    }

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
        if (OperatingSystem.IsWindows())
        {
            return "qwen3-asr-cli.exe";
        }

        return "qwen3-asr-cli";
    }

    public bool CanBeDownloaded()
    {
        return true;
    }

    public string DownloadSizeText => "~1 MB"; // tiny stub binary; the heavy model is fetched separately as a forced aligner.

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterQwen3AsrCpp;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterQwen3AsrCpp = value;
    }
}
