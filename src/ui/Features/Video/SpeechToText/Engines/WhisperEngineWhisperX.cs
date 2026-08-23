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
/// Optional Python-based WhisperX engine. The managed installation is kept below
/// Subtitle Edit's data folder and does not modify the user's system Python packages.
/// </summary>
public sealed class WhisperEngineWhisperX : ISpeechToTextEngine
{
    public static string StaticName => "WhisperX";
    public string Name => StaticName;
    public string Choice => WhisperChoice.WhisperX;
    public string Url => "https://github.com/m-bain/whisperX";

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();
    public List<WhisperModel> Models => new WhisperModel().Models.ToList();
    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled() => File.Exists(GetExecutable());

    public override string ToString() => Name;

    public string GetAndCreateWhisperFolder()
    {
        var folder = Path.Combine(Se.SpeechToTextFolder, "WhisperX");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var folder = new WhisperModel().ModelFolder;
        Directory.CreateDirectory(folder);
        return folder;
    }

    public string GetExecutable()
    {
        var managed = GetManagedExecutable();
        if (File.Exists(managed))
        {
            return managed;
        }

        var configured = ResolveConfiguredExecutable(Se.Settings.Tools.AudioToText.WhisperXLocation);
        return configured ?? FindExecutableOnPath() ?? (OperatingSystem.IsWindows() ? "whisperx.exe" : "whisperx");
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        // WhisperX owns the Hugging Face cache and downloads the selected model on first use.
        // Do not route its model name to Subtitle Edit's .pt downloader.
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

    public string DownloadSizeText => "~1-2 GB (Python packages; models download on first use)";

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterWhisperX;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterWhisperX = value;
    }

    public string GetManagedExecutable()
    {
        var scripts = OperatingSystem.IsWindows() ? "Scripts" : "bin";
        var fileName = OperatingSystem.IsWindows() ? "whisperx.exe" : "whisperx";
        return Path.Combine(GetAndCreateWhisperFolder(), ".venv", scripts, fileName);
    }

    public string GetManagedPython()
    {
        var scripts = OperatingSystem.IsWindows() ? "Scripts" : "bin";
        var fileName = OperatingSystem.IsWindows() ? "python.exe" : "python";
        return Path.Combine(GetAndCreateWhisperFolder(), ".venv", scripts, fileName);
    }

    public static string? FindPythonExecutable()
    {
        var candidates = OperatingSystem.IsWindows()
            ? new[] { "python.exe", "py.exe" }
            : new[]
            {
                "python3", "python", "/opt/homebrew/bin/python3", "/usr/local/bin/python3",
                "/Library/Frameworks/Python.framework/Versions/Current/bin/python3",
            };

        foreach (var candidate in candidates)
        {
            var path = candidate.Contains('/') || candidate.Contains(Path.DirectorySeparatorChar)
                ? candidate
                : FindExecutableOnPath(candidate);
            if (!string.IsNullOrEmpty(path) && (File.Exists(path) || !Path.IsPathRooted(path)))
            {
                return path;
            }
        }

        return null;
    }

    private static string? ResolveConfiguredExecutable(string? configured)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            return null;
        }

        configured = configured.Trim();
        if (File.Exists(configured))
        {
            return configured;
        }

        if (Directory.Exists(configured))
        {
            var fileName = OperatingSystem.IsWindows() ? "whisperx.exe" : "whisperx";
            var candidate = Path.Combine(configured, fileName);
            return File.Exists(candidate) ? candidate : null;
        }

        return null;
    }

    private static string? FindExecutableOnPath(string name = "whisperx")
    {
        var paths = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            paths.Add("/opt/homebrew/bin");
            paths.Add("/usr/local/bin");
            paths.Add(Path.Combine(home, ".local", "bin"));
            paths.Add(Path.Combine(home, ".pyenv", "shims"));
        }

        var fileName = OperatingSystem.IsWindows() && !name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? name + ".exe"
            : name;
        foreach (var path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var candidate = Path.Combine(path, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
