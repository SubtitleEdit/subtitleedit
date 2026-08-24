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

    // WhisperX runs on the same faster-whisper/ctranslate2 backend as WhisperEngineCTranslate2,
    // so it takes the same model list (correct names like large-v3-turbo/distil-*, and the
    // actual Hugging Face repos this backend downloads from) rather than whisper.cpp's ggml
    // model list, which does not match what this backend understands.
    public List<WhisperModel> Models => new WhisperPurfviewFasterWhisperModel().Models.ToList();

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
        // Shares Purfview's folder, matching WhisperEngineCTranslate2 - unused for actual model
        // storage (WhisperX's models live in the Hugging Face cache, see IsModelInstalled), but
        // this keeps the interface's expectation of "a real, existing folder" honest rather than
        // pointing at whisper.cpp's unrelated ggml model folder.
        return new WhisperEnginePurfviewFasterWhisperXxl().GetAndCreateWhisperModelFolder(whisperModel);
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
        // model on first use - do not route its model name through SE's .pt/.bin downloader.
        // Still worth checking honestly (rather than always reporting "installed"): these models
        // come from the same Hugging Face repos as WhisperEngineCTranslate2/Purfview (both run
        // the same backend), so a snapshot already sitting in the Hugging Face hub cache for
        // that repo id is a reliable signal the model does not need downloading again.
        var repoId = GetHuggingFaceRepoId(model);
        if (string.IsNullOrEmpty(repoId))
        {
            return false;
        }

        var cacheFolder = Path.Combine(GetHuggingFaceHubCacheDir(), "models--" + repoId.Replace("/", "--"));
        return Directory.Exists(cacheFolder);
    }

    private static string? GetHuggingFaceRepoId(WhisperModel model)
    {
        // e.g. "https://huggingface.co/Systran/faster-whisper-tiny/resolve/main/model.bin" -> "Systran/faster-whisper-tiny"
        var url = model.Urls?.FirstOrDefault();
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        const string marker = "huggingface.co/";
        var idx = url.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return null;
        }

        var parts = url[(idx + marker.Length)..].Split('/');
        return parts.Length >= 2 ? $"{parts[0]}/{parts[1]}" : null;
    }

    // Mirrors huggingface_hub's own cache directory resolution order: HF_HOME, then the legacy
    // HUGGINGFACE_HUB_CACHE, then the default ~/.cache/huggingface/hub.
    private static string GetHuggingFaceHubCacheDir()
    {
        var hfHome = Environment.GetEnvironmentVariable("HF_HOME");
        if (!string.IsNullOrEmpty(hfHome))
        {
            return Path.Combine(hfHome, "hub");
        }

        var hubCache = Environment.GetEnvironmentVariable("HUGGINGFACE_HUB_CACHE");
        if (!string.IsNullOrEmpty(hubCache))
        {
            return hubCache;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".cache", "huggingface", "hub");
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

    // Measured from the v1.0.1 release zips (compressed download size); unpacks to roughly
    // 950 MB (Windows), 1.1 GB (macOS), 1.7 GB (Linux) on disk, plus another ~2 GB of Whisper/
    // alignment/diarization models that download from Hugging Face the first time each is used.
    public string DownloadSizeText
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return "~350 MB";
            }

            if (OperatingSystem.IsMacOS())
            {
                return "~365 MB";
            }

            if (OperatingSystem.IsLinux())
            {
                return "~575 MB";
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
