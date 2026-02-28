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
        var baseFolder = Se.WhisperFolder;
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

        if (!File.Exists(fullPath) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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
        var modelFileName = Path.Combine(GetAndCreateWhisperModelFolder(null), "faster-whisper-" + modelName);
        return modelFileName;
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

    internal static string GetExecutableFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "whisper-ctranslate2.exe";
        }

        return "whisper-ctranslate2";
    }

    public bool CanBeDownloaded()
    {
        return true;
    }
}