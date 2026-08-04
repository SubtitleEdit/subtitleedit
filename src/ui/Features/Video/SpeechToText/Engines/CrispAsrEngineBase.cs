using Avalonia.Platform;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
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
    /// CrispASR ships several builds per platform whose sizes differ by two orders of magnitude,
    /// and the variant is picked at download time rather than now — so Windows and Linux get a
    /// range rather than a single number that would either understate the GPU bundles or scare
    /// users off the CPU one. Figures are the v0.8.25 release assets
    /// (<see cref="Nikse.SubtitleEdit.Logic.Download.CrispAsrDownloadService"/> holds the pin);
    /// they drift with every release, so treat them as indicative.
    /// </summary>
    public virtual string DownloadSizeText
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                // CPU ~7 MB, Vulkan ~32 MB, CUDA ~688 MB.
                return "~7 MB – 688 MB";
            }
            if (OperatingSystem.IsLinux())
            {
                // CPU ~25 MB (arm64 ~23 MB), Vulkan ~55 MB, ROCm ~97 MB, CUDA 13 ~190 MB, CUDA 12 ~259 MB.
                return "~25 MB – 259 MB";
            }
            if (OperatingSystem.IsMacOS())
            {
                return "~15 MB";
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
        // The per-backend asset name is derived from Name, so a backend added without its
        // matching .txt would throw straight into the caller's RelayCommand. Fall back to the
        // shared text instead - missing help is not worth taking the dialog down for.
        var assetName = $"{Name.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{assetName}");
        var commonUri = new Uri("avares://SubtitleEdit/Assets/SpeechToText/CrispASRCommon.txt");

        var header = string.Empty;
        try
        {
            await using var headerStream = AssetLoader.Open(uri);
            using var headerReader = new StreamReader(headerStream);
            header = (await headerReader.ReadToEndAsync()).TrimEnd();
        }
        catch
        {
            Se.LogError($"No help text asset \"{assetName}\" for speech-to-text backend \"{Name}\".");
        }

        await using var commonStream = AssetLoader.Open(commonUri);
        using var commonReader = new StreamReader(commonStream);
        var common = await commonReader.ReadToEndAsync();

        return header.Length == 0 ? common : header + Environment.NewLine + common;
    }
}
