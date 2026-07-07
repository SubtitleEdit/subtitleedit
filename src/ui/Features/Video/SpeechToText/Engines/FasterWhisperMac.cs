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
/// Speech-to-text engine for macOS that drives the pip-installed "faster-whisper" Python package
/// (SYSTRAN's CTranslate2 implementation of Whisper). CTranslate2 has no Apple GPU backend, so this
/// engine is CPU-only and slower than <see cref="MlxWhisperMac"/>; it exists because faster-whisper's
/// decoding gives noticeably better results in some languages (e.g. Arabic). Because the pip package
/// ships a library rather than a console tool, transcription runs a small bundled helper script
/// (<c>faster_whisper_transcribe.py</c>) via <c>python3</c>, which uses batched inference (2-4x
/// faster than sequential decoding, identical weights) and writes an SRT next to the input audio.
/// Installation is detected by importing the package (<c>python3 -c "import faster_whisper"</c>);
/// models are downloaded automatically from Hugging Face on first use.
/// </summary>
public class FasterWhisperMac : ISpeechToTextEngine
{
    public static string StaticName => "Faster Whisper Mac";
    public string Name => StaticName;
    public string Choice => WhisperChoice.FasterWhisperMac;
    public string Url => "https://github.com/SYSTRAN/faster-whisper";

    private const string TranscribeScriptName = "faster_whisper_transcribe.py";

    private static bool? _isFasterWhisperInstalled;

    // The python3 interpreter that can actually "import faster_whisper". Resolved during the install
    // check and reused for transcription so both use the same Python (see GetExecutable).
    private static string? _resolvedPython;

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();

    // Built explicitly (not filtered from the CTranslate2 list, which has no turbo entry).
    // faster-whisper resolves these names itself and downloads the CTranslate2-converted weights
    // from Hugging Face on first use; sizes are the approximate downloads. large-v3 is the
    // accuracy pick (e.g. for Arabic); large-v3-turbo trades a little accuracy for speed.
    public List<WhisperModel> Models => new List<WhisperModel>
    {
        new WhisperModel { Name = "large-v3", Size = "2.9 GB" },
        new WhisperModel { Name = "large-v3-turbo", Size = "1.6 GB" },
        new WhisperModel { Name = "large-v2", Size = "2.9 GB" },
        new WhisperModel { Name = "medium", Size = "1.5 GB" },
        new WhisperModel { Name = "small", Size = "484 MB" },
        new WhisperModel { Name = "base", Size = "145 MB" },
        new WhisperModel { Name = "tiny", Size = "75 MB" },
    };

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterFasterWhisperMac;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterFasterWhisperMac = value;
    }

    public bool IsEngineInstalled()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return false;
        }

        if (_isFasterWhisperInstalled.HasValue)
        {
            return _isFasterWhisperInstalled.Value;
        }

        // faster-whisper is almost never installed into Homebrew's Python (it is "externally
        // managed", PEP 668), so checking a single fixed interpreter wrongly reports "not
        // installed". Probe each candidate and pick the first one that can import the package;
        // remember it for transcription.
        foreach (var python in GetPythonCandidates())
        {
            if (CanImportFasterWhisper(python))
            {
                _resolvedPython = python;
                _isFasterWhisperInstalled = true;
                return true;
            }
        }

        _isFasterWhisperInstalled = false;
        return false;
    }

    private static bool CanImportFasterWhisper(string python)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(python, "-c \"import faster_whisper\"")
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
    /// The python3 interpreters to probe for faster-whisper, in priority order: Homebrew,
    /// python.org framework builds (newest first), pyenv, the system Python, and finally PATH
    /// resolution. Mirrors <see cref="MlxWhisperMac"/>.
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

        var folder = Path.Combine(baseFolder, "FasterWhisperMac");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        // faster-whisper downloads models itself into the Hugging Face cache.
        return new WhisperCTranslate2Model().ModelFolder;
    }

    public string GetExecutable()
    {
        // The engine drives the faster-whisper *library* through python3 (see the class summary),
        // so the executable is the Python interpreter. Prefer the interpreter that actually has
        // faster-whisper importable (resolved during the install check) so detection and
        // transcription stay in sync.
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
        // faster-whisper" guidance points at a real interpreter.
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
        // faster-whisper resolves models by name and downloads them on first use,
        // so any of the supported models can be used right away.
        return true;
    }

    public string GetModelForCmdLine(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return "large-v3";
        }

        // Names (tiny ... large-v3-turbo), local paths and Hugging Face repo ids all pass
        // straight through; faster-whisper resolves them itself.
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
            return "Faster Whisper Mac runs SYSTRAN's \"faster-whisper\" Python library via python3.\n\n" +
                   "Install it with: pip3 install faster-whisper\n\n" +
                   "CTranslate2 has no Apple GPU backend, so this engine is CPU-only and slower than\n" +
                   "MLX Whisper - but its decoding gives better results in some languages (e.g. Arabic).\n" +
                   "Batched inference (2-4x speedup, same accuracy) is used automatically.\n\n" +
                   "Models (tiny, base, small, medium, large-v2, large-v3, large-v3-turbo) are downloaded\n" +
                   "from Hugging Face on first use.\n" +
                   "Tip: add --compute-type int8 in advanced settings for ~2x more speed with\n" +
                   "near-identical accuracy.";
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
