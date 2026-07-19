using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Speech-to-text engine for Apple Silicon Macs that drives the pip-installed "mlx-whisper"
/// Python package. Unlike CTranslate2 / faster-whisper (CPU-only on macOS), MLX runs Whisper on
/// the Apple GPU and Neural Engine via Apple's MLX framework, so on M-series Macs it is typically
/// much faster. Because the pip package ships a library rather than a console tool, transcription
/// runs a small bundled helper script (<c>mlx_whisper_transcribe.py</c>) via <c>python3</c>, which
/// writes an SRT next to the input audio. Installation is detected by importing the package
/// (<c>python3 -c "import mlx_whisper"</c>); MLX-format models are downloaded automatically from
/// Hugging Face (the <c>mlx-community</c> org) on first use.
/// </summary>
public class MlxWhisperMac : ISpeechToTextEngine
{
    public static string StaticName => "MLX Whisper";
    public string Name => StaticName;
    public string Choice => WhisperChoice.MlxWhisperMac;
    public string Url => "https://github.com/ml-explore/mlx-examples/tree/main/whisper";

    private const string TranscribeScriptName = "mlx_whisper_transcribe.py";

    // The console-script shim pip/pipx installs for the package (mlx_whisper --help).
    private const string CliShimName = "mlx_whisper";

    // Friendly model name -> MLX-format Hugging Face repo (mlx-community).
    private static readonly Dictionary<string, string> ModelRepos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tiny"] = "mlx-community/whisper-tiny-mlx",
        ["base"] = "mlx-community/whisper-base-mlx",
        ["small"] = "mlx-community/whisper-small-mlx",
        ["medium"] = "mlx-community/whisper-medium-mlx",
        ["large-v2"] = "mlx-community/whisper-large-v2-mlx",
        ["large-v3"] = "mlx-community/whisper-large-v3-mlx",
        ["large-v3-turbo"] = "mlx-community/whisper-large-v3-turbo",
    };

    private static bool? _isMlxWhisperInstalled;

    // The python3 interpreter that can actually "import mlx_whisper". Resolved during the install
    // check and reused for transcription so both use the same Python (see GetExecutable).
    private static string? _resolvedPython;

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();

    // Built explicitly (not filtered from the CTranslate2 list, which has no turbo entry). large-v3-turbo
    // is multilingual and the best speed/quality pick; on the Apple GPU it is very fast. Sizes are the
    // approximate MLX download sizes. Models download from Hugging Face automatically on first use.
    public List<WhisperModel> Models => new List<WhisperModel>
    {
        new WhisperModel { Name = "large-v3-turbo", Size = "1.6 GB" },
        new WhisperModel { Name = "large-v3", Size = "2.9 GB" },
        new WhisperModel { Name = "large-v2", Size = "2.9 GB" },
        new WhisperModel { Name = "medium", Size = "1.5 GB" },
        new WhisperModel { Name = "small", Size = "483 MB" },
        new WhisperModel { Name = "base", Size = "145 MB" },
        new WhisperModel { Name = "tiny", Size = "75 MB" },
    };

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterMlxWhisperMac;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterMlxWhisperMac = value;
    }

    public bool IsEngineInstalled()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return false;
        }

        if (_isMlxWhisperInstalled.HasValue)
        {
            return _isMlxWhisperInstalled.Value;
        }

        // mlx-whisper is almost never installed into Homebrew's Python (it is "externally managed",
        // PEP 668), so checking a single fixed interpreter wrongly reports "not installed". Probe each
        // candidate and pick the first one that can import the package; remember it for transcription.
        foreach (var python in GetPythonCandidates())
        {
            if (CanImportMlxWhisper(python))
            {
                _resolvedPython = python;
                _isMlxWhisperInstalled = true;
                return true;
            }
        }

        _isMlxWhisperInstalled = false;
        return false;
    }

    private static bool CanImportMlxWhisper(string python)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(python, "-c \"import mlx_whisper\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

#pragma warning disable CA1416
            process.Start();
            if (!process.WaitForExit(10_000))
            {
                process.Kill(true);
                return false;
            }
#pragma warning restore CA1416

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// The python3 interpreters to probe for mlx-whisper, in priority order: the interpreter behind
    /// an installed <c>mlx_whisper</c> CLI shim (covers pipx / venv / conda), then Homebrew,
    /// python.org framework builds (newest first), pyenv, the system Python, and finally PATH
    /// resolution.
    /// </summary>
    private static IEnumerable<string> GetPythonCandidates()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var candidates = new List<string>();

        void Add(string path)
        {
            if (!string.IsNullOrEmpty(path) && seen.Add(path))
            {
                candidates.Add(path);
            }
        }

        // Highest priority: the exact interpreter behind an installed mlx_whisper CLI shim. pipx,
        // a hand-made venv, and conda all install the package into an isolated environment that
        // none of the shared interpreters below can import, so probing them alone reports "not
        // found" even though the user did install it (#12209). The shim's shebang names the one
        // interpreter that can import the package.
        foreach (var interpreter in GetShebangInterpreters())
        {
            Add(interpreter);
        }

        Add("/opt/homebrew/bin/python3");
        Add("/usr/local/bin/python3");

        const string frameworkDir = "/Library/Frameworks/Python.framework/Versions";
        if (Directory.Exists(frameworkDir))
        {
            foreach (var versionDir in Directory.GetDirectories(frameworkDir).OrderByDescending(p => p))
            {
                Add(Path.Combine(versionDir, "bin", "python3"));
            }
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(home))
        {
            Add(Path.Combine(home, ".pyenv", "shims", "python3"));
        }

        Add("/usr/bin/python3");
        Add("python3"); // resolved via PATH

        return candidates.Where(p => p == "python3" || File.Exists(p));
    }

    /// <summary>
    /// Interpreters discovered by reading the shebang of an installed <c>mlx_whisper</c> CLI shim.
    /// A pip/pipx/venv/conda console script starts with <c>#!/abs/path/to/that/env/bin/python</c>,
    /// which points at the exact interpreter that can <c>import mlx_whisper</c> - the one case the
    /// fixed interpreter list cannot cover, because an isolated environment's package is not
    /// importable from Homebrew/system Python (#12209).
    /// </summary>
    private static IEnumerable<string> GetShebangInterpreters()
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var shim in GetCliShimCandidates())
        {
            try
            {
                if (!File.Exists(shim))
                {
                    continue;
                }

                // Read only the first line - a console script is tiny, but a same-named non-script
                // file on one of these paths shouldn't be slurped whole.
                using var reader = new StreamReader(shim);
                var firstLine = reader.ReadLine();
                if (firstLine == null || !firstLine.StartsWith("#!", StringComparison.Ordinal))
                {
                    continue;
                }

                // "#!/path/to/python" or "#!/path/to/python -E ...": take the interpreter token.
                // The "#!/usr/bin/env python3" form names no specific environment, so its first
                // token ("/usr/bin/env") is filtered out by the python-name check below - the bare
                // "python3" PATH candidate already covers that case.
                var interpreterPath = firstLine.Substring(2).Trim().Split(' ', '\t')[0];
                if (Path.IsPathRooted(interpreterPath) &&
                    Path.GetFileName(interpreterPath).StartsWith("python", StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(interpreterPath) &&
                    seen.Add(interpreterPath))
                {
                    result.Add(interpreterPath);
                }
            }
            catch
            {
                // Unreadable / permission-denied shim - skip and try the next candidate.
            }
        }

        return result;
    }

    /// <summary>
    /// Locations an installed <c>mlx_whisper</c> console-script shim commonly lives: pipx and
    /// "pip install --user" write it to <c>~/.local/bin</c>; a Homebrew-managed Python writes it
    /// into its own bin. The PATH is also scanned so a shim on the user's PATH is found when
    /// Subtitle Edit is launched from a shell.
    /// </summary>
    private static IEnumerable<string> GetCliShimCandidates()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(home))
        {
            var localBin = Path.Combine(home, ".local", "bin", CliShimName);
            if (seen.Add(localBin))
            {
                yield return localBin;
            }
        }

        foreach (var dir in new[] { "/opt/homebrew/bin", "/usr/local/bin" })
        {
            var shim = Path.Combine(dir, CliShimName);
            if (seen.Add(shim))
            {
                yield return shim;
            }
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
        {
            yield break;
        }

        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(dir))
            {
                continue;
            }

            string shim;
            try
            {
                shim = Path.Combine(dir, CliShimName);
            }
            catch
            {
                continue; // malformed PATH entry (illegal characters)
            }

            if (seen.Add(shim))
            {
                yield return shim;
            }
        }
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

        var folder = Path.Combine(baseFolder, "MlxWhisperMac");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        // mlx-whisper downloads models itself into the Hugging Face cache.
        return new WhisperCTranslate2Model().ModelFolder;
    }

    public string GetExecutable()
    {
        // The engine drives the mlx-whisper *library* through python3 (see the class summary), so the
        // executable is the Python interpreter. Prefer the interpreter that actually has mlx-whisper
        // importable (resolved during the install check) so detection and transcription stay in sync.
        if (_resolvedPython != null)
        {
            return _resolvedPython;
        }

        IsEngineInstalled();
        if (_resolvedPython != null)
        {
            return _resolvedPython;
        }

        // Not installed in any probed interpreter; return a sensible default so the "pip3 install
        // mlx-whisper" guidance points at a real interpreter.
        return GetPythonCandidates().FirstOrDefault() ?? "python3";
    }

    /// <summary>
    /// Extracts the bundled transcription helper script to the engine folder and returns its path.
    /// The script is rewritten on every call so it always matches the running build.
    /// </summary>
    public string GetTranscribeScript()
    {
        var scriptPath = Path.Combine(GetAndCreateWhisperFolder(), TranscribeScriptName);

        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{TranscribeScriptName}");
        using var stream = AssetLoader.Open(uri);
        using var fileStream = File.Create(scriptPath);
        stream.CopyTo(fileStream);

        return scriptPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        // mlx-whisper resolves models by Hugging Face repo and downloads them on first use,
        // so any of the supported models can be used right away.
        return true;
    }

    public string GetModelForCmdLine(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return ModelRepos["large-v3-turbo"];
        }

        // Allow passing a full Hugging Face repo id directly.
        if (modelName.Contains('/'))
        {
            return modelName;
        }

        return ModelRepos.TryGetValue(modelName, out var repo) ? repo : modelName;
    }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{assetName}");

        try
        {
            await using var stream = AssetLoader.Open(uri);
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            return contents;
        }
        catch
        {
            return "MLX Whisper runs Apple's \"mlx-whisper\" Python library via python3.\n\n" +
                   "Install it with: pip3 install mlx-whisper\n\n" +
                   "Requires an Apple Silicon Mac. MLX runs Whisper on the GPU / Neural Engine, so it is\n" +
                   "typically much faster than CPU-only Whisper engines on Apple Silicon.\n\n" +
                   "Models (tiny, base, small, medium, large-v2, large-v3, large-v3-turbo) are MLX-format\n" +
                   "weights downloaded from Hugging Face (mlx-community) on first use.\n" +
                   "Tip: large-v3-turbo is multilingual and the best speed/quality pick.";
        }
    }

    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        return string.Empty;
    }

    public bool CanBeDownloaded()
    {
        return false;
    }
}
