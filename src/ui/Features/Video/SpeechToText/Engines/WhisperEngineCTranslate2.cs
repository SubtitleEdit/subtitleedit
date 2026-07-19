using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class WhisperEngineCTranslate2 : ISpeechToTextEngine
{
    public static string StaticName => "Whisper CTranslate2";
    public string Name => StaticName;
    public string Choice => WhisperChoice.CTranslate2;
    public string Url => "https://github.com/Softcatala/whisper-ctranslate2";

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();

    public List<WhisperModel> Models
    {
        get
        {
            var models = new WhisperPurfviewFasterWhisperModel().Models;
            return models.ToList();
        }
    }

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
        var baseFolder = Se.SpeechToTextFolder;
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        var folder = Path.Combine(baseFolder, "CTranslate2");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        return new WhisperEnginePurfviewFasterWhisperXxl().GetAndCreateWhisperModelFolder(whisperModel);
    }

    public string GetExecutable()
    {
        string fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());

        if (!File.Exists(fullPath) && OperatingSystem.IsLinux())
        {
            string[] paths = ["/usr/bin/whisper-cli", "usr/local/bin/"];
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        return fullPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        return new WhisperEnginePurfviewFasterWhisperXxl().IsModelInstalled(model);
    }

    public string GetModelForCmdLine(string modelName)
    {
        // Resolve via the model's real folder name so distil models (published as
        // "faster-distil-whisper-...") get the correct --model path, not "faster-whisper-<name>" (#12133).
        var model = Models.FirstOrDefault(m => m.Name == modelName);
        var folderName = model != null
            ? WhisperEnginePurfviewFasterWhisperXxl.GetModelFolderName(model)
            : "faster-whisper-" + modelName;
        return Path.Combine(GetAndCreateWhisperModelFolder(null), folderName);
    }

    public bool SupportsCustomModels => true;

    public bool CustomModelIsFolder => true;

    public string ImportCustomModel(string sourcePath)
    {
        // CTranslate2 shares Purfview's "_models" folder and faster-whisper folder layout.
        return WhisperEnginePurfviewFasterWhisperXxl.ImportFasterWhisperFolderModel(sourcePath, GetAndCreateWhisperModelFolder(null));
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
        if (OperatingSystem.IsWindows())
        {
            return "whisper-ctranslate2.exe";
        }

        return "whisper-ctranslate2";
    }

    public bool CanBeDownloaded()
    {
        return true;
    }

    public string DownloadSizeText
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return "~103 MB";
            }
            if (OperatingSystem.IsLinux())
            {
                return "~149 MB";
            }
            if (OperatingSystem.IsMacOS())
            {
                return "~76 MB";
            }
            return string.Empty;
        }
    }

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCTranslate2;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCTranslate2 = value;
    }
}