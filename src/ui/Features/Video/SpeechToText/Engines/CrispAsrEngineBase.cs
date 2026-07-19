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
    public virtual bool HasNativeTimestamps => false;
    public abstract List<WhisperLanguage> Languages { get; }
    public abstract List<WhisperModel> Models { get; }
    public abstract string Extension { get; }
    public abstract string UnpackSkipFolder { get; }
    public abstract bool IsEngineInstalled();
    public abstract bool CanBeDownloaded();

    /// <summary>
    /// CrispASR ships several Windows variants of very different sizes (CPU ~3 MB, Vulkan
    /// ~25 MB, CUDA ~684 MB), and the variant is picked at download time rather than now.
    /// We surface the smallest reasonable number and note that it varies so the user has a
    /// floor without being misled by the CUDA bundle.
    /// </summary>
    public virtual string DownloadSizeText
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return "~3 MB – 684 MB"; // CPU – CUDA depending on variant chosen at download time
            }
            if (OperatingSystem.IsLinux())
            {
                return "~5 MB";
            }
            if (OperatingSystem.IsMacOS())
            {
                return "~4 MB";
            }
            return string.Empty;
        }
    }
    public abstract string GetAndCreateWhisperFolder();
    public abstract string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel);
    public abstract string GetExecutable();
    public abstract bool IsModelInstalled(WhisperModel model);
    public abstract string GetModelForCmdLine(string modelName);
    public abstract string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url);
    public abstract string CommandLineParameter { get; set; }

    public virtual async Task<string> GetHelpText()
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
