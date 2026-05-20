using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrFunAsrMltNano : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Fun-ASR MLT Nano";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrFunAsrMltNano;
    public override string BackendName => "fun-asr-mlt-nano";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => true;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";

    public override List<WhisperLanguage> Languages =>
        new()
        {
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("zh", "chinese"),
            new WhisperLanguage("yue", "cantonese"),
            new WhisperLanguage("ja", "japanese"),
            new WhisperLanguage("ko", "korean"),
            new WhisperLanguage("vi", "vietnamese"),
            new WhisperLanguage("th", "thai"),
            new WhisperLanguage("id", "indonesian"),
            new WhisperLanguage("ms", "malay"),
            new WhisperLanguage("tl", "tagalog"),
            new WhisperLanguage("ar", "arabic"),
            new WhisperLanguage("hi", "hindi"),
            new WhisperLanguage("bg", "bulgarian"),
            new WhisperLanguage("ru", "russian"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("pt", "portuguese"),
            new WhisperLanguage("nl", "dutch"),
            new WhisperLanguage("pl", "polish"),
            new WhisperLanguage("cs", "czech"),
            new WhisperLanguage("ro", "romanian"),
            new WhisperLanguage("el", "greek"),
            new WhisperLanguage("fi", "finnish"),
            new WhisperLanguage("sv", "swedish"),
            new WhisperLanguage("tr", "turkish"),
            new WhisperLanguage("fa", "persian"),
            new WhisperLanguage("da", "danish"),
            new WhisperLanguage("hu", "hungarian"),
            new WhisperLanguage("mk", "macedonian"),
        };

    public override List<WhisperModel> Models =>
        new()
        {
            new WhisperModel
            {
                Name = "funasr-mlt-nano-2512-f16.gguf",
                Size = "1.84 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/funasr-mlt-nano-GGUF/resolve/main/funasr-mlt-nano-2512-f16.gguf",
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrFunAsrMltNano;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrFunAsrMltNano = value;
    }
}
