using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Speech-to-text engine for Russian driving the GigaAM models (Sber's "Giga Acoustic Model",
/// https://huggingface.co/ai-sage/GigaAM-v3) through the pip-installed "onnx-asr" Python package
/// (https://github.com/istupakov/onnx-asr). GigaAM v2/v3 substantially outperform Whisper on
/// Russian; onnx-asr runs the ONNX exports on CPU via onnxruntime on all platforms. Because the
/// package is a library, transcription runs a small bundled helper script
/// (<c>gigaam_transcribe.py</c>) via python, which segments the audio with Silero VAD and writes
/// an SRT next to the input audio. Installation is detected by importing the package
/// (<c>python3 -c "import onnx_asr"</c>); models are downloaded automatically from Hugging Face
/// on first use.
/// </summary>
public class GigaAmEngine : ISpeechToTextEngine
{
    public static string StaticName => "GigaAM";
    public string Name => StaticName;
    public string Choice => WhisperChoice.GigaAm;
    public string Url => "https://github.com/istupakov/onnx-asr";

    private const string TranscribeScriptName = "gigaam_transcribe.py";

    // The console-script shim pip/pipx installs for the package (onnx-asr --help).
    private const string CliShimName = "onnx-asr";

    private static bool? _isOnnxAsrInstalled;

    // The python interpreter that can actually "import onnx_asr". Resolved during the install
    // check and reused for transcription so both use the same Python (see GetExecutable).
    private static string? _resolvedPython;

    // GigaAM is a Russian model family (the default engine args request int8 weights;
    // clear them to use the full-precision ONNX exports instead).
    public List<WhisperLanguage> Languages => new() { new WhisperLanguage("ru", "russian") };

    // Model names are onnx-asr model ids; the package downloads the ONNX exports from
    // Hugging Face (istupakov/gigaam-*-onnx) automatically on first use. Sizes are the
    // int8 download sizes requested by the default "--quantization int8" engine argument
    // (full-precision exports are ~0.9 GB). The "e2e" v3 variants output punctuation and
    // capitalization; the plain models output normalized lowercase text.
    public List<WhisperModel> Models => new List<WhisperModel>
    {
        new WhisperModel { Name = "gigaam-v3-e2e-rnnt", Size = "233 MB" },
        new WhisperModel { Name = "gigaam-v3-e2e-ctc", Size = "225 MB" },
        new WhisperModel { Name = "gigaam-v3-rnnt", Size = "229 MB" },
        new WhisperModel { Name = "gigaam-v3-ctc", Size = "225 MB" },
        new WhisperModel { Name = "gigaam-v2-rnnt", Size = "240 MB" },
        new WhisperModel { Name = "gigaam-v2-ctc", Size = "236 MB" },
    };

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterGigaAm;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterGigaAm = value;
    }

    public bool IsEngineInstalled()
    {
        if (_isOnnxAsrInstalled.HasValue)
        {
            return _isOnnxAsrInstalled.Value;
        }

        // onnx-asr is often installed into a user/venv/pipx Python rather than the first
        // interpreter on PATH, so checking a single fixed interpreter wrongly reports
        // "not installed". Probe each candidate and pick the first one that can import
        // the package; remember it for transcription.
        foreach (var python in GetPythonCandidates())
        {
            if (CanImportOnnxAsr(python))
            {
                _resolvedPython = python;
                _isOnnxAsrInstalled = true;
                return true;
            }
        }

        _isOnnxAsrInstalled = false;
        return false;
    }

    private static bool CanImportOnnxAsr(string python)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(python, "-c \"import onnx_asr\"")
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
    /// The python interpreters to probe for onnx-asr, in priority order: the interpreter behind
    /// an installed <c>onnx-asr</c> CLI shim (covers pipx / venv / conda on Mac/Linux), then the
    /// per-platform standard installs, and finally PATH resolution.
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

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (OperatingSystem.IsWindows())
        {
            // Windows python.org installs: %LocalAppData%\Programs\Python\Python3XX\python.exe
            var pythonRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs", "Python");
            if (Directory.Exists(pythonRoot))
            {
                foreach (var versionDir in Directory.GetDirectories(pythonRoot).OrderByDescending(p => p))
                {
                    Add(Path.Combine(versionDir, "python.exe"));
                }
            }

            Add("python"); // resolved via PATH
            return candidates.Where(p => p == "python" || File.Exists(p));
        }

        // Highest priority: the exact interpreter behind an installed onnx-asr CLI shim. pipx,
        // a hand-made venv, and conda all install the package into an isolated environment that
        // none of the shared interpreters below can import, so probing them alone reports "not
        // found" even though the user did install it. The shim's shebang names the one
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

        if (!string.IsNullOrEmpty(home))
        {
            Add(Path.Combine(home, ".pyenv", "shims", "python3"));
        }

        Add("/usr/bin/python3");
        Add("python3"); // resolved via PATH

        return candidates.Where(p => p == "python3" || File.Exists(p));
    }

    /// <summary>
    /// Interpreters discovered by reading the shebang of an installed <c>onnx-asr</c> CLI shim.
    /// A pip/pipx/venv/conda console script starts with <c>#!/abs/path/to/that/env/bin/python</c>,
    /// which points at the exact interpreter that can <c>import onnx_asr</c> - the one case the
    /// fixed interpreter list cannot cover, because an isolated environment's package is not
    /// importable from Homebrew/system Python.
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
    /// Locations an installed <c>onnx-asr</c> console-script shim commonly lives: pipx and
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

        var folder = Path.Combine(baseFolder, "GigaAm");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        // onnx-asr downloads models itself into the Hugging Face cache.
        return GetAndCreateWhisperFolder();
    }

    public string GetExecutable()
    {
        // The engine drives the onnx-asr *library* through python (see the class summary), so the
        // executable is the Python interpreter. Prefer the interpreter that actually has onnx-asr
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

        // Not installed in any probed interpreter; return a sensible default so the "pip install
        // onnx-asr" guidance points at a real interpreter.
        return GetPythonCandidates().FirstOrDefault() ?? (OperatingSystem.IsWindows() ? "python" : "python3");
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
        // onnx-asr resolves models by id and downloads them from Hugging Face on first use,
        // so any of the supported models can be used right away.
        return true;
    }

    public string GetModelForCmdLine(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return "gigaam-v3-e2e-rnnt";
        }

        return modelName;
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
            return "GigaAM transcribes Russian speech via the \"onnx-asr\" Python library.\n\n" +
                   "Install it with: pip3 install \"onnx-asr[cpu,hub]\"\n\n" +
                   "Models (GigaAM v2/v3 ONNX exports) are downloaded from Hugging Face on first use.\n" +
                   "Tip: the v3 \"e2e\" models output punctuation and capitalization.";
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
