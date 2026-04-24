using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrGlm : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR GLM";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrGlm;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";
    public override string BackendName => "glm-asr";
    public override string DefaultLanguage => "zh";
    public override bool IncludeLanguage => true;

    public override List<WhisperLanguage> Languages =>
        new()
        {
            new WhisperLanguage("zh", "chinese"),
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("yue", "cantonese"),
            new WhisperLanguage("ja", "japanese"),
            new WhisperLanguage("ko", "korean"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("ru", "russian"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("pt", "portuguese"),
            new WhisperLanguage("vi", "vietnamese"),
            new WhisperLanguage("th", "thai"),
            new WhisperLanguage("id", "indonesian"),
            new WhisperLanguage("ar", "arabic"),
            new WhisperLanguage("hi", "hindi"),
            new WhisperLanguage("tr", "turkish"),
        };


    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "glm-asr-nano-q4_k.gguf",
                Size = "1.3 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/glm-asr-nano-GGUF/resolve/main/glm-asr-nano-q4_k.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "glm-asr-nano-q8_0.gguf",
                Size = "2.4 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/glm-asr-nano-GGUF/resolve/main/glm-asr-nano-q8_0.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "glm-asr-nano.gguf",
                Size = "4.5 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/glm-asr-nano-GGUF/resolve/main/glm-asr-nano.gguf"
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
        return Name;
    }

    public override string GetAndCreateWhisperFolder()
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrGlm;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrGlm = value;
    }
}