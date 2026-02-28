using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class WhisperEnginePurfviewFasterWhisperXxl : ISpeechToTextEngine
{
    public static string StaticName => "Purfview Faster Whisper XXL";
    public string Name => StaticName;
    public string Choice => WhisperChoice.PurfviewFasterWhisperXxl;
    public string Url => "https://github.com/Purfview/whisper-standalone-win";
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
    public string UnpackSkipFolder => "Whisper-Faster/";

    public bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.WhisperFolder;
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        var folder = Path.Combine(baseFolder, "Purfview-Whisper-Faster");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var baseFolder = GetAndCreateWhisperFolder();

        var folder = Path.Combine(baseFolder, "_models");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (whisperModel != null)
        {
            folder = Path.Combine(folder, "faster-whisper-" + whisperModel.Name);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        return folder;
    }

    public string GetExecutable()
    {
        var fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        var baseFolder = GetAndCreateWhisperFolder();
        var folder = Path.Combine(baseFolder, "_models");
        folder = Path.Combine(folder, "faster-whisper-" + model.Name);
        if (!Directory.Exists(folder))
        {
            return false;
        }

        var binFileName = Path.GetFileName(model.Urls.First(p => p.EndsWith(".bin")));
        binFileName = Path.Combine(folder, binFileName);
        if (!File.Exists(binFileName))
        {
            return false;
        }

        var fileInfo = new FileInfo(binFileName);
        return fileInfo.Length > 10_000_000;
    }

    public string GetModelForCmdLine(string modelName)
    {
        return modelName;
    }

    public override string ToString()
    {
        return Name;
    }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/Whisper/{assetName}");

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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "faster-whisper-xxl.exe";
        }

        return "faster-whisper-xxl";
    }

    public bool CanBeDownloaded()
    {
        return true;
    }
}
