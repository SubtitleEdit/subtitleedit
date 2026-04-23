using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public abstract class CrispAsrEngineBase : ICrispAsrEngine
{
    public abstract string Name { get; }
    public abstract string Choice { get; }
    public abstract string Url { get; }
    public abstract string BackendName { get; }
    public abstract string DefaultLanguage { get; }
    public abstract bool IncludeLanguage { get; }
    public abstract List<WhisperLanguage> Languages { get; }
    public abstract List<WhisperModel> Models { get; }
    public abstract string Extension { get; }
    public abstract string UnpackSkipFolder { get; }
    public abstract bool IsEngineInstalled();
    public abstract bool CanBeDownloaded();
    public abstract string GetAndCreateWhisperFolder();
    public abstract string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel);
    public abstract string GetExecutable();
    public abstract bool IsModelInstalled(WhisperModel model);
    public abstract string GetModelForCmdLine(string modelName);
    public abstract string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url);
    public abstract string CommandLineParameter { get; set; }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{Name.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{assetName}");
        var commonUri = new Uri("avares://SubtitleEdit/Assets/SpeechToText/CrispASRCommon.txt");

        await using var headerStream = AssetLoader.Open(uri);
        using var headerReader = new StreamReader(headerStream);
        var header = await headerReader.ReadToEndAsync();

        await using var commonStream = AssetLoader.Open(commonUri);
        using var commonReader = new StreamReader(commonStream);
        var common = await commonReader.ReadToEndAsync();

        return header.TrimEnd() + Environment.NewLine + common;
    }
}
