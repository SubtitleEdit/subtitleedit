using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// OmniVoice TTS via the omnivoice-tts CLI. One process per utterance (no server mode).
/// "Default" voice runs with no reference; imported WAVs use --ref-wav for cloning,
/// expecting a sibling &lt;name&gt;.txt with the reference transcript (omnivoice-tts requires
/// --ref-text whenever --ref-wav is set).
/// </summary>
public class OmniVoiceTtsCpp : ITtsEngine
{
    public string Name => "OmniVoice TTS";
    public string Description => "646 languages, voice cloning, runs on CPU";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;

    // Voice-design attribute keywords accepted by omnivoice-tts' --instruct flag (English set).
    // The CLI rejects free text - only these values, comma+space separated, are valid. Grouped
    // because each group is mutually exclusive (one gender, one age, one pitch, one accent).
    public static readonly string[] InstructionGenders =
    {
        "female",
        "male",
    };

    public static readonly string[] InstructionAges =
    {
        "child",
        "teenager",
        "young adult",
        "middle-aged",
        "elderly",
    };

    public static readonly string[] InstructionPitches =
    {
        "very low pitch",
        "low pitch",
        "moderate pitch",
        "high pitch",
        "very high pitch",
    };

    public static readonly string[] InstructionAccents =
    {
        "american accent",
        "australian accent",
        "british accent",
        "canadian accent",
        "chinese accent",
        "indian accent",
        "japanese accent",
        "korean accent",
        "portuguese accent",
        "russian accent",
    };

    // Stand-alone toggle - not part of any mutually-exclusive group.
    public const string InstructionWhisper = "whisper";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetExecutableFileName()));
    }

    public static bool IsModelsInstalled()
    {
        return File.Exists(GetModelBasePath()) && File.Exists(GetModelTokenizerPath());
    }

    /// <summary>
    /// Returns the update status of the installed OmniVoice engine relative to the release
    /// pinned in <see cref="OmniVoiceDownloadService"/>. Reads the key from the
    /// <c>.installed.sha256</c> sidecar written at install time. Returns
    /// <see cref="DownloadHashManager.UpdateStatus.Unknown"/> when nothing is installed yet
    /// or the sidecar is missing (older installs predating hash tracking), so callers do
    /// not false-positive an update prompt.
    /// </summary>
    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus()
    {
        var folder = GetSetFolder();
        if (!File.Exists(GetExecutableFileName()))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        var sidecar = Path.Combine(folder, ".installed.sha256");
        if (!File.Exists(sidecar))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        try
        {
            var lines = File.ReadAllLines(sidecar);
            if (lines.Length < 2)
            {
                return DownloadHashManager.UpdateStatus.Unknown;
            }
            return DownloadHashManager.GetStatus(lines[0].Trim(), lines[1].Trim());
        }
        catch
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }
    }

    public override string ToString() => Name;

    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "OmniVoice");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetModelsFolder()
    {
        var modelsFolder = Path.Combine(GetSetFolder(), "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public static string GetSetVoicesFolder()
    {
        var voicesFolder = Path.Combine(GetSetFolder(), "voices");
        if (!Directory.Exists(voicesFolder))
        {
            Directory.CreateDirectory(voicesFolder);
        }

        return voicesFolder;
    }

    public static string GetExecutableFileName()
    {
        return Path.Combine(GetSetFolder(), OperatingSystem.IsWindows() ? "omnivoice-tts.exe" : "omnivoice-tts");
    }

    public static string GetCodecExecutableFileName()
    {
        return Path.Combine(GetSetFolder(), OperatingSystem.IsWindows() ? "omnivoice-codec.exe" : "omnivoice-codec");
    }

    public static string GetModelBasePath() =>
        Path.Combine(GetSetModelsFolder(), OmniVoiceDownloadService.ModelBaseFileName);

    public static string GetModelTokenizerPath() =>
        Path.Combine(GetSetModelsFolder(), OmniVoiceDownloadService.ModelTokenizerFileName);

    public Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>
        {
            new Voice(new OmniVoiceVoice("Default", string.Empty)),
        };

        var voicesFolder = GetSetVoicesFolder();
        foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
        {
            var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
            result.Add(new Voice(new OmniVoiceVoice(name, file)));
        }

        return Task.FromResult(result.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(Array.Empty<string>());

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(OmniVoiceLanguages.All);

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken)
    {
        return GetVoices(language);
    }

    public async Task<TtsResult> Speak(
        string text,
        string outputFolder,
        Voice voice,
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken)
    {
        if (voice.EngineVoice is not OmniVoiceVoice omniVoice)
        {
            throw new ArgumentException("Voice is not an OmniVoiceVoice");
        }

        var exe = GetExecutableFileName();
        if (!File.Exists(exe))
        {
            throw new FileNotFoundException("omnivoice-tts executable not found.", exe);
        }

        var modelPath = GetModelBasePath();
        var codecPath = GetModelTokenizerPath();
        if (!File.Exists(modelPath) || !File.Exists(codecPath))
        {
            throw new FileNotFoundException(
                $"OmniVoice TTS models not found in {GetSetModelsFolder()}. Download them via the TTS download dialog.");
        }

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");
        var inputText = Utilities.UnbreakLine(text);

        var psi = new ProcessStartInfo
        {
            WorkingDirectory = GetSetFolder(),
            FileName = exe,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = Encoding.UTF8,
        };

        // The Windows Vulkan build needs vulkan-1.dll on PATH at launch. Add the SDK bin
        // folder if we know about it, so the binary works even when the runtime isn't in
        // the global PATH.
        if (OperatingSystem.IsWindows())
        {
            var vulkanPath = Se.Settings.Video.TextToSpeech.OmniVoiceTtsCppVulkanPath;
            if (string.IsNullOrEmpty(vulkanPath))
            {
                vulkanPath = Logic.VulkanHelper.TryFindBinFolder();
            }
            if (!string.IsNullOrEmpty(vulkanPath) && psi.EnvironmentVariables["Path"] != null)
            {
                psi.EnvironmentVariables["Path"] =
                    psi.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" + vulkanPath;
            }
        }
        psi.ArgumentList.Add("--model");
        psi.ArgumentList.Add(modelPath);
        psi.ArgumentList.Add("--codec");
        psi.ArgumentList.Add(codecPath);
        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add(outputFileName);

        // omnivoice-tts treats --lang absent / "None" identically (auto). Only pass
        // a code when one is explicitly selected so a blank/null selection means
        // auto-detect, not "language not found".
        if (language != null && !string.IsNullOrEmpty(language.Code))
        {
            psi.ArgumentList.Add("--lang");
            psi.ArgumentList.Add(language.Code);
        }

        // Voice cloning: omnivoice-tts requires both --ref-wav and --ref-text.
        // Pair each voices/<name>.wav with a sibling <name>.txt holding the transcript.
        // The transcript is captured at import time; if it has been deleted out from under
        // us we fail loudly so the user knows custom voice cloning isn't happening, rather
        // than silently producing the default speaker.
        var usingReference = !string.IsNullOrEmpty(omniVoice.FilePath) && File.Exists(omniVoice.FilePath);
        if (usingReference)
        {
            var refTextPath = Path.ChangeExtension(omniVoice.FilePath, ".txt");
            if (!File.Exists(refTextPath))
            {
                throw new FileNotFoundException(
                    $"OmniVoice TTS voice cloning requires a transcript file at {refTextPath}. "
                    + "Re-import the voice to provide its transcript.",
                    refTextPath);
            }

            psi.ArgumentList.Add("--ref-wav");
            psi.ArgumentList.Add(omniVoice.FilePath);
            psi.ArgumentList.Add("--ref-text");
            psi.ArgumentList.Add(refTextPath);
        }

        // Voice-design keywords (--instruct) only shape the voice when there is no reference
        // audio - with --ref-wav omnivoice-tts takes the voice from the clone and ignores them.
        if (!usingReference)
        {
            var instruction = (Se.Settings.Video.TextToSpeech.OmniVoiceTtsCppInstruction ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(instruction))
            {
                psi.ArgumentList.Add("--instruct");
                psi.ArgumentList.Add(instruction);
            }
        }

        Se.WriteToolsLog($"OmniVoice TTS: {exe} {string.Join(' ', psi.ArgumentList)} (voice={omniVoice}, textLen={text.Length})");

        var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start omnivoice-tts");

        try
        {
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

            await process.StandardInput.WriteAsync(inputText.AsMemory(), cancellationToken);
            process.StandardInput.Close();

            await process.WaitForExitAsync(cancellationToken);
            var stderr = await stderrTask;
            var stdout = await stdoutTask;

            if (process.ExitCode != 0 || !File.Exists(outputFileName))
            {
                var msg = $"OmniVoice TTS failed (exit code {process.ExitCode}) - "
                    + $"Voice: {omniVoice}, Text: {text}, "
                    + $"Args: {string.Join(' ', psi.ArgumentList)}, "
                    + $"StdErr: {stderr.Trim()}, StdOut: {stdout.Trim()}";
                Se.LogError(msg);
                Se.WriteToolsLog(msg);
                throw new InvalidOperationException(
                    $"OmniVoice TTS failed (exit code {process.ExitCode}). {stderr.Trim()}");
            }

            return new TtsResult(outputFileName, text);
        }
        catch (OperationCanceledException)
        {
            // The omnivoice-tts process keeps running after the await is cancelled. Kill the
            // process tree and remove the partial output file before propagating the cancel,
            // so we don't leak CPU work or stale .wav files.
            TryKill(process);
            TryDelete(outputFileName);
            throw;
        }
        finally
        {
            process.Dispose();
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // best effort
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // best effort
        }
    }

    private static string GetUniqueDestinationFileName(string folder, string baseName, string extension)
    {
        var candidate = Path.Combine(folder, baseName + extension);
        if (!File.Exists(candidate))
        {
            return candidate;
        }

        var number = 1;
        do
        {
            candidate = Path.Combine(folder, $"{baseName}_{number}{extension}");
            number++;
        } while (File.Exists(candidate));

        return candidate;
    }

    public bool ImportVoice(string fileName) => ImportVoice(fileName, transcript: null);

    /// <summary>
    /// Imports a voice for cloning. If <paramref name="transcript"/> is provided it is written
    /// next to the imported WAV as the required reference transcript; otherwise we fall back
    /// to copying a sibling &lt;basename&gt;.txt from the source folder if one exists.
    /// </summary>
    public bool ImportVoice(string fileName, string? transcript)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        var voicesFolder = GetSetVoicesFolder();
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var destinationFileName = GetUniqueDestinationFileName(voicesFolder, baseName, ".wav");

        if (Path.GetExtension(fileName).Equals(".wav", StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(fileName, destinationFileName, overwrite: false);
        }
        else
        {
            var process = FfmpegGenerator.ConvertFormat(fileName, destinationFileName);
            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                _ = process.Start();
            }
            else
            {
                throw new PlatformNotSupportedException("Process.Start() is not supported on this platform.");
            }
            process.WaitForExit();
        }

        // Pair the imported WAV with a sibling .txt holding the transcript. Without it
        // omnivoice-tts will reject --ref-wav.
        var destTextFile = Path.ChangeExtension(destinationFileName, ".txt");
        if (!string.IsNullOrWhiteSpace(transcript))
        {
            File.WriteAllText(destTextFile, transcript);
        }
        else
        {
            var sourceTextFile = Path.ChangeExtension(fileName, ".txt");
            if (File.Exists(sourceTextFile) && !File.Exists(destTextFile))
            {
                File.Copy(sourceTextFile, destTextFile, overwrite: false);
            }
        }

        return File.Exists(destinationFileName);
    }
}
