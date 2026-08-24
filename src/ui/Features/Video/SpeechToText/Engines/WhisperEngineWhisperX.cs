using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// WhisperX, packaged as a standalone PyInstaller build (see
/// https://github.com/muaz978/subtitleedit-whisperx-standalone) so it downloads and runs like
/// every other engine here, with no separate Python install or managed virtual environment.
/// Requiring Python was the reason an earlier version of this engine (PR #14031) was not
/// accepted upstream.
/// </summary>
public class WhisperEngineWhisperX : ISpeechToTextEngine
{
    public static string StaticName => "WhisperX";
    public string Name => StaticName;
    public string Choice => WhisperChoice.WhisperX;
    public string Url => "https://github.com/m-bain/whisperX";

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();
    public List<WhisperModel> Models => new WhisperModel().Models.ToList();
    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled()
    {
        return File.Exists(GetExecutable());
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

        var folder = Path.Combine(baseFolder, "WhisperX");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var folder = new WhisperModel().ModelFolder;
        Directory.CreateDirectory(folder);
        return folder;
    }

    // The archive is unpacked flat into GetAndCreateWhisperFolder(), so the executable and its
    // "_internal" PyInstaller payload folder land directly there - same layout as CTranslate2.
    public string GetExecutable()
    {
        return Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
    }

    internal static string GetExecutableFileName()
    {
        return OperatingSystem.IsWindows() ? "whisperx-standalone.exe" : "whisperx-standalone";
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        // WhisperX owns the Hugging Face cache and downloads the selected Whisper/alignment
        // model on first use. Do not route its model name through SE's .pt/.bin downloader.
        return IsEngineInstalled();
    }

    public string GetModelForCmdLine(string modelName) => modelName;

    public async Task<string> GetHelpText()
    {
        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{StaticName}.txt");
        await using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
        => Path.Combine(GetAndCreateWhisperModelFolder(whisperModel), Path.GetFileName(url));

    public bool CanBeDownloaded() => true;

    public string DownloadSizeText
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return "~750 MB";
            }

            if (OperatingSystem.IsMacOS())
            {
                return "~300 MB";
            }

            if (OperatingSystem.IsLinux())
            {
                return "~750 MB";
            }

            return string.Empty;
        }
    }

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterWhisperX;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterWhisperX = value;
    }
}
