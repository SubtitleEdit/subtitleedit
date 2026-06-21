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
    public static string StaticName => "MLX Whisper Mac";
    public string Name => StaticName;
    public string Choice => WhisperChoice.MlxWhisperMac;
    public string Url => "https://github.com/ml-explore/mlx-examples/tree/main/whisper";

    private const string TranscribeScriptName = "mlx_whisper_transcribe.py";

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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return false;
        }

        if (_isMlxWhisperInstalled.HasValue)
        {
            return _isMlxWhisperInstalled.Value;
        }

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(GetExecutable(), "-c \"import mlx_whisper\"")
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
                _isMlxWhisperInstalled = false;
                return false;
            }
#pragma warning restore CA1416

            _isMlxWhisperInstalled = process.ExitCode == 0;
        }
        catch
        {
            _isMlxWhisperInstalled = false;
        }

        return _isMlxWhisperInstalled.Value;
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
        // The engine drives the mlx-whisper *library* through python3 (see the class summary),
        // so the executable is the Python interpreter. Probe the usual macOS install locations
        // (Homebrew on Apple Silicon, Homebrew/python.org on Intel, the system Python) and fall
        // back to PATH resolution.
        var candidates = new[]
        {
            "/opt/homebrew/bin/python3",
            "/usr/local/bin/python3",
            "/usr/bin/python3",
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return "python3"; // resolved via PATH
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
            return "MLX Whisper Mac runs Apple's \"mlx-whisper\" Python library via python3.\n\n" +
                   "Install it with: pip3 install mlx-whisper\n\n" +
                   "Requires an Apple Silicon Mac. MLX runs Whisper on the GPU / Neural Engine, so it is\n" +
                   "typically much faster than the CPU-only Faster Whisper Mac engine.\n\n" +
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
