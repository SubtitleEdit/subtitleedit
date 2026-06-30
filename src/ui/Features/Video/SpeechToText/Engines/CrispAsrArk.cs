using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrArk : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR ARK";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrArk;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";
    public override string BackendName => "ark-asr";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => true;

    // ARK-ASR-3B = Whisper-large-v3 encoder (partial RoPE) + Qwen2.5-3B LM decoder.
    // 19 languages per the CrispASR model card.
    public override List<WhisperLanguage> Languages =>
        new()
        {
            new WhisperLanguage("zh", "chinese"),
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("ja", "japanese"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("ko", "korean"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("pl", "polish"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("ro", "romanian"),
            new WhisperLanguage("hu", "hungarian"),
            new WhisperLanguage("cs", "czech"),
            new WhisperLanguage("nl", "dutch"),
            new WhisperLanguage("fi", "finnish"),
            new WhisperLanguage("hr", "croatian"),
            new WhisperLanguage("sk", "slovak"),
            new WhisperLanguage("sl", "slovenian"),
            new WhisperLanguage("et", "estonian"),
            new WhisperLanguage("lt", "lithuanian"),
        };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "ark-asr-3b-q4_k.gguf",
                Size = "3.52 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/ark-asr-3b-GGUF/resolve/main/ark-asr-3b-q4_k.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "ark-asr-3b-q8_0.gguf",
                Size = "4.29 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/ark-asr-3b-GGUF/resolve/main/ark-asr-3b-q8_0.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "ark-asr-3b-f16.gguf",
                Size = "7.51 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/ark-asr-3b-GGUF/resolve/main/ark-asr-3b-f16.gguf"
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrArk;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrArk = value;
    }
}
