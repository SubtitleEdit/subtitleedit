using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class WhisperEngineOpenAi : ISpeechToTextEngine
{
    public static string StaticName => "Whisper Open AI";
    public string Name => StaticName;
    public string Choice => WhisperChoice.OpenAi;
    public string Url => "https://github.com/openai/whisper";

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();

    public List<WhisperModel> Models
    {
        get
        {
            var models = new WhisperModel().Models;
            return models.ToList();
        }
    }

    public string Extension => ".pt";
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled()
    {
        return true;
    }

    public override string ToString()
    {
        return Name;
    }

    public string GetAndCreateWhisperFolder()
    {
        var folder = WhisperHelper.GetWhisperFolder(WhisperChoice.OpenAi);
        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var folder = new WhisperModel().ModelFolder;
        return folder;
    }

    public string GetExecutable()
    {
        var fullPath = WhisperHelper.GetExecutableFileName(WhisperChoice.OpenAi);
        return fullPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        var modelFileName = Path.Combine(GetAndCreateWhisperModelFolder(model), model.Name);
        if (Extension.Length > 0 && !modelFileName.EndsWith(Extension))
        {
            modelFileName += Extension;
        }

        var fileExists = File.Exists(modelFileName);

        return fileExists;
    }

    public string GetModelForCmdLine(string modelName)
    {
        var modelFileName = Path.Combine(GetAndCreateWhisperModelFolder(null), modelName);
        if (Extension.Length > 0 && !modelFileName.EndsWith(Extension))
        {
            modelFileName += Extension;
        }

        return modelFileName;
    }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/Whisper/{assetName}");

        await using var stream = AssetLoader.Open(uri); using var reader = new StreamReader(stream);

        var contents = await reader.ReadToEndAsync();
        return contents;
    }

    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileName = Path.Combine(folder, whisperModel.Name + Extension);
        return fileName;
    }

    public bool CanBeDownloaded()
    {
        return false;
    }
}
