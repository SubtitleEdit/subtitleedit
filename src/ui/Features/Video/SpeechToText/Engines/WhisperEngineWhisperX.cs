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
    //
    // Minus the NbAiLab "*.nb" entries: their names only work for engines SE downloads models
    // for (SE resolves the name to a local folder), while whisperx gets the bare name on the
    // command line, and "tiny.nb" is neither a faster-whisper size nor a Hugging Face repo id -
    // the run would fail after the model picker accepted it.
    public List<WhisperModel> Models => new WhisperPurfviewFasterWhisperModel().Models
        .Where(m => !m.Name.EndsWith(".nb", StringComparison.Ordinal))
        .ToList();

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

    // WhisperX owns its models: the bare model name goes on the command line and the
    // faster-whisper backend fetches it into the Hugging Face hub cache on first use, showing
    // its progress in the console log. SE's own model downloader must never run for this
    // engine - it lands in Purfview's model folder, which whisperx never reads, so the user
    // paid for gigabytes twice and the prompt came back on every run (#15170 history).
    public bool DownloadsOwnModels => true;

    public bool IsModelInstalled(WhisperModel model)
    {
        // Purely informational (the model dot): SE never downloads for this engine, see
        // DownloadsOwnModels. The models come from the same Hugging Face repos as
        // WhisperEngineCTranslate2/Purfview (same backend), so a complete snapshot for that
        // repo id in the hub cache is what "downloaded" means here (#15170 - reporting every
        // model as installed showed a green dot for models never downloaded).
        return IsModelInHubCache(model, GetHuggingFaceHubCacheDir());
    }

    internal static bool IsModelInHubCache(WhisperModel model, string hubCacheDir)
    {
        var repoId = GetHuggingFaceRepoId(model);
        if (string.IsNullOrEmpty(repoId))
        {
            return false;
        }

        // huggingface_hub layout: <cache>/models--<org>--<name>/snapshots/<revision>/<files>.
        // An interrupted download leaves the repo folder with ".incomplete" blobs and no
        // model.bin in any snapshot, so require the weights file rather than the folder.
        var snapshots = Path.Combine(hubCacheDir, "models--" + repoId.Replace("/", "--"), "snapshots");
        if (!Directory.Exists(snapshots))
        {
            return false;
        }

        try
        {
            return Directory.EnumerateDirectories(snapshots)
                .Any(snapshot => File.Exists(Path.Combine(snapshot, "model.bin")));
        }
        catch (Exception)
        {
            return false;
        }
    }

    // faster-whisper's own model names (faster_whisper/utils.py _MODELS). whisperx hands the
    // --model value to faster-whisper, which accepts these names or a Hugging Face repo id
    // (anything with a "/") and rejects everything else with "Invalid model size".
    internal static readonly HashSet<string> FasterWhisperModelNames = new(StringComparer.Ordinal)
    {
        "tiny.en", "tiny", "base.en", "base", "small.en", "small", "medium.en", "medium",
        "large-v1", "large-v2", "large-v3", "large", "distil-large-v2", "distil-medium.en",
        "distil-small.en", "distil-large-v3", "distil-large-v3.5", "large-v3-turbo", "turbo",
    };

    // Where faster-whisper's name map differs from the URL SE downloads from for the
    // CTranslate2/Purfview engines: the name resolves to another repo, so the hub cache
    // holds it under that repo's folder.
    private static readonly Dictionary<string, string> FasterWhisperRepoOverrides = new(StringComparer.Ordinal)
    {
        ["distil-large-v3.5"] = "distil-whisper/distil-large-v3.5-ct2",
    };

    // e.g. "https://huggingface.co/Systran/faster-whisper-tiny/resolve/main/model.bin"
    //   -> "Systran/faster-whisper-tiny". Custom models found in Purfview's model folder have
    // no URL, so they get no repo id and no green dot.
    internal static string? GetHuggingFaceRepoId(WhisperModel model)
    {
        if (FasterWhisperRepoOverrides.TryGetValue(model.Name, out var overrideRepoId))
        {
            return overrideRepoId;
        }

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
        return parts.Length >= 2 && parts[0].Length > 0 && parts[1].Length > 0
            ? $"{parts[0]}/{parts[1]}"
            : null;
    }

    internal static string GetHuggingFaceHubCacheDir()
    {
        return GetHuggingFaceHubCacheDir(Environment.GetEnvironmentVariable);
    }

    // Mirrors huggingface_hub's constants.py: HF_HUB_CACHE wins, then the legacy
    // HUGGINGFACE_HUB_CACHE, then HF_HOME/hub, then XDG_CACHE_HOME/huggingface/hub, and finally
    // ~/.cache/huggingface/hub (on Windows "~" is the user profile folder, the same as Python's
    // expanduser). Like the library, a leading "~" in the result is expanded.
    internal static string GetHuggingFaceHubCacheDir(Func<string, string?> getEnvironmentVariable)
    {
        var hubCache = getEnvironmentVariable("HF_HUB_CACHE");
        if (string.IsNullOrWhiteSpace(hubCache))
        {
            hubCache = getEnvironmentVariable("HUGGINGFACE_HUB_CACHE");
        }

        if (string.IsNullOrWhiteSpace(hubCache))
        {
            var hfHome = getEnvironmentVariable("HF_HOME");
            if (string.IsNullOrWhiteSpace(hfHome))
            {
                var xdgCacheHome = getEnvironmentVariable("XDG_CACHE_HOME");
                var cacheHome = !string.IsNullOrWhiteSpace(xdgCacheHome)
                    ? xdgCacheHome
                    : Path.Combine("~", ".cache");
                hfHome = Path.Combine(cacheHome, "huggingface");
            }

            hubCache = Path.Combine(hfHome, "hub");
        }

        return ExpandUser(hubCache);
    }

    private static string ExpandUser(string path)
    {
        if (path == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        if (path.StartsWith("~/", StringComparison.Ordinal) || path.StartsWith("~\\", StringComparison.Ordinal))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[2..]);
        }

        return path;
    }

    // A model faster-whisper has no name for (e.g. "anime.ja") goes on the command line as its
    // repo id, which faster-whisper downloads into the same hub cache the model dot checks.
    public string GetModelForCmdLine(string modelName)
    {
        if (FasterWhisperModelNames.Contains(modelName))
        {
            return modelName;
        }

        var model = Models.FirstOrDefault(m => m.Name == modelName);
        return (model != null ? GetHuggingFaceRepoId(model) : null) ?? modelName;
    }

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

    // Measured from the whisperx-standalone-101 support-files .7z assets (compressed download
    // size); unpacks to roughly 950 MB (Windows), 1.1 GB (macOS), 1.7 GB (Linux) on disk, plus
    // another ~2 GB of Whisper/alignment/diarization models that download from Hugging Face the
    // first time each is used.
    public string DownloadSizeText
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return "~240 MB";
            }

            if (OperatingSystem.IsMacOS())
            {
                return "~220 MB";
            }

            if (OperatingSystem.IsLinux())
            {
                return "~355 MB";
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
