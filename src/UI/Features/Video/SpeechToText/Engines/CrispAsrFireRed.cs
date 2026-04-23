using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrFireRed : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Fire Red";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrFireRed;
    public override string BackendName => "firered";
    public override string DefaultLanguage => "zh";
    public override bool IncludeLanguage => true;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";

  public override List<WhisperLanguage> Languages =>
    new()
    {
        // --- Core & Dialects ---
        new WhisperLanguage("zh", "chinese"),
        new WhisperLanguage("en", "english"),
        new WhisperLanguage("yue", "cantonese"),
        new WhisperLanguage("wuu", "shanghainese"),
        new WhisperLanguage("nan", "minnan"),
        new WhisperLanguage("gan", "gan"),
        new WhisperLanguage("hak", "hakka"),
        new WhisperLanguage("hsn", "xiang"),
        
        // --- Major Multilingual (Commonly used in 100+ set) ---
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
        new WhisperLanguage("nl", "dutch"),
        new WhisperLanguage("sv", "swedish"),
        new WhisperLanguage("pl", "polish"),
        new WhisperLanguage("ms", "malay"),
        new WhisperLanguage("fa", "persian"),
        new WhisperLanguage("el", "greek"),
        new WhisperLanguage("cs", "czech"),
        new WhisperLanguage("hu", "hungarian"),
        new WhisperLanguage("ro", "romanian"),
        new WhisperLanguage("da", "danish"),
        new WhisperLanguage("fi", "finnish"),
        new WhisperLanguage("he", "hebrew"),
        new WhisperLanguage("uk", "ukrainian"),
        new WhisperLanguage("sk", "slovak"),
        new WhisperLanguage("hr", "croatian"),
        new WhisperLanguage("bg", "bulgarian"),
        new WhisperLanguage("bn", "bengali"),
        new WhisperLanguage("ta", "tamil"),
        new WhisperLanguage("te", "telugu"),
        new WhisperLanguage("kn", "kannada"),
        new WhisperLanguage("ml", "malayalam"),
        new WhisperLanguage("mr", "marathi"),
        new WhisperLanguage("gu", "gujarati"),
        new WhisperLanguage("pa", "punjabi"),
        new WhisperLanguage("sw", "swahili"),
        new WhisperLanguage("yo", "yoruba"),
        new WhisperLanguage("ha", "hausa"),
    };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "firered-asr2-aed-q4_k.gguf",
                Size = "1 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/firered-asr2-aed-GGUF/resolve/main/firered-asr2-aed-q4_k.gguf",
                    "https://huggingface.co/cstr/firered-vad-GGUF/resolve/main/firered-vad.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "firered-asr2-aed-q8_0.gguf",
                Size = "1.5 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/firered-asr2-aed-GGUF/resolve/main/firered-asr2-aed-q8_0.gguf",
                    "https://huggingface.co/cstr/firered-vad-GGUF/resolve/main/firered-vad.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "firered-asr2-aed.gguf",
                Size = "2.4 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/firered-asr2-aed-GGUF/resolve/main/firered-asr2-aed.gguf",
                    "https://huggingface.co/cstr/firered-vad-GGUF/resolve/main/firered-vad.gguf"
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrFireRed;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrFireRed = value;
    }
}