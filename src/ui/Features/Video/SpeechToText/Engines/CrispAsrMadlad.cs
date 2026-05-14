using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// CrispASR MADLAD-400 text translation backend. This is not a speech-to-text engine - it is
/// only used to drive the shared CrispASR binary/model download windows for the auto-translate
/// feature, so it is intentionally not registered in <see cref="CrispAsrEngine"/>.
/// </summary>
public class CrispAsrMadlad : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR MADLAD";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrMadlad;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";
    public override string BackendName => "madlad";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => false;
    public override bool HasNativeTimestamps => false;

    public override List<WhisperLanguage> Languages => new();

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "madlad400-3b-mt-q4_k.gguf",
                Size = "1.8 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/madlad400-3b-mt-GGUF/resolve/main/madlad400-3b-mt-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "madlad400-3b-mt-q8_0.gguf",
                Size = "3.1 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/madlad400-3b-mt-GGUF/resolve/main/madlad400-3b-mt-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "madlad400-3b-mt-f16.gguf",
                Size = "5.7 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/madlad400-3b-mt-GGUF/resolve/main/madlad400-3b-mt-f16.gguf",
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
        return StaticName;
    }

    public override string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.SpeechToTextFolder;
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
        return Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
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
        return Path.Combine(GetAndCreateWhisperModelFolder(null), modelName);
    }

    public override string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileNameOnly = Path.GetFileName(url);
        return Path.Combine(folder, fileNameOnly);
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

    public override string CommandLineParameter { get; set; } = string.Empty;
}
