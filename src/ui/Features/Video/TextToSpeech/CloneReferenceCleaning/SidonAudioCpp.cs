using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.CloneReferenceCleaning;

/// <summary>
/// Sidon v0.1 (MIT) speech restoration through the shared audio.cpp runtime
/// (<see cref="AudioCppRuntime"/>, family <c>sidon</c>): takes single-speaker speech with music,
/// noise or reverb under it and returns the speech alone, mono at 48 kHz, trimmed to the input's
/// length. Runs <c>audiocpp_cli --task s2s</c> - one call for one file, one batch call for a
/// folder - since the model loads in about a second and a server would only add a port to manage.
/// </summary>
/// <remarks>
/// Measured on an M4 (Metal) against the Matrix Reloaded trailer: music-only stretches drop from
/// about -14 dB to about -62 dB while the speech keeps its level, a 2 s clip takes about 1 s
/// including the model load, a 30 s clip about 6 s, and a batch of 13 short clips about 10 s.
/// Peak RSS is about 2 GB.
/// </remarks>
public static class SidonAudioCpp
{
    public const string FamilyName = "sidon";
    public const string DisplayName = "Sidon";
    public const string ModelFileName = "sidon-v0.1-f32.gguf";
    public const long ModelFileSize = 984_276_544;
    public const string ModelSizeText = "940 MB";
    public const string ModelUrl = "https://huggingface.co/audio-cpp/audio.cpp-gguf/resolve/main/Sidon-GGUF/" + ModelFileName;

    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "models", "Sidon-GGUF");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetModelPath() => Path.Combine(GetSetModelsFolder(), ModelFileName);

    public static string GetCliExecutable() =>
        Path.Combine(AudioCppRuntime.GetSetEngineFolder(), OperatingSystem.IsWindows() ? "audiocpp_cli.exe" : "audiocpp_cli");

    public static bool IsModelInstalled() => IsValidLocalModelFile(GetModelPath());

    public static bool IsValidLocalModelFile(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists)
            {
                return false;
            }

            // A symlinked GGUF reports the link's own length; measure the target.
            var length = info.ResolveLinkTarget(returnFinalTarget: true) is FileInfo target ? target.Length : info.Length;
            return length == ModelFileSize;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Arguments for one file: <paramref name="inputFileName"/> restored into <paramref name="outputFileName"/>.</summary>
    internal static List<string> BuildFileArguments(string inputFileName, string outputFileName, string modelPath, string backend) =>
        new()
        {
            "--task", "s2s",
            "--family", FamilyName,
            "--model", modelPath,
            "--backend", backend,
            "--audio", inputFileName,
            "--out", outputFileName,
        };

    /// <summary>
    /// Arguments for every .wav in <paramref name="inputFolder"/>, restored into
    /// <paramref name="outputFolder"/> under the same name. The CLI replaces characters it does
    /// not like in output names, so callers should hand it ASCII names.
    /// </summary>
    internal static List<string> BuildFolderArguments(string inputFolder, string outputFolder, string modelPath, string backend) =>
        new()
        {
            "--task", "s2s",
            "--family", FamilyName,
            "--model", modelPath,
            "--backend", backend,
            "--batch-audio-dir", inputFolder,
            "--out-dir", outputFolder,
        };

    /// <summary>Restores one file. Throws <see cref="InvalidOperationException"/> when the CLI fails or writes no audio.</summary>
    public static async Task RestoreFileAsync(string inputFileName, string outputFileName, CancellationToken cancellationToken)
    {
        var backend = AudioCppRuntime.GetBackend();
        await RunCliAsync(BuildFileArguments(inputFileName, outputFileName, GetModelPath(), backend), backend, null, cancellationToken);
        if (!File.Exists(outputFileName))
        {
            throw new InvalidOperationException("Sidon wrote no audio for " + inputFileName);
        }
    }

    /// <summary>
    /// Restores every .wav in <paramref name="inputFolder"/> into <paramref name="outputFolder"/> in
    /// one model load. <paramref name="progress"/> gets (done, total) as output files appear.
    /// </summary>
    public static async Task RestoreFolderAsync(string inputFolder, string outputFolder, Action<int, int>? progress, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputFolder);
        var total = Directory.GetFiles(inputFolder, "*.wav").Length;
        var backend = AudioCppRuntime.GetBackend();
        Action? poll = progress == null
            ? null
            : () => progress(Math.Min(total, Directory.GetFiles(outputFolder, "*.wav").Length), total);
        await RunCliAsync(BuildFolderArguments(inputFolder, outputFolder, GetModelPath(), backend), backend, poll, cancellationToken);
    }

    private static async Task RunCliAsync(List<string> arguments, string backend, Action? poll, CancellationToken cancellationToken)
    {
        var exe = GetCliExecutable();
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            WorkingDirectory = Path.GetDirectoryName(exe) ?? AudioCppRuntime.GetSetEngineFolder(),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        foreach (var argument in arguments)
        {
            psi.ArgumentList.Add(argument);
        }

        var log = new StringBuilder();
        using var process = new Process { StartInfo = psi };
        void OnLine(string? line)
        {
            if (line == null)
            {
                return;
            }

            lock (log)
            {
                log.AppendLine(line);
            }
        }

        process.OutputDataReceived += (_, e) => OnLine(e.Data);
        process.ErrorDataReceived += (_, e) => OnLine(e.Data);

        Se.WriteToolsLog("Sidon (audio.cpp): " + exe + " " + string.Join(" ", psi.ArgumentList.Select(QuoteForLog)));
        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start " + exe);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            while (!process.HasExited)
            {
                poll?.Invoke();
                try
                {
                    await process.WaitForExitAsync(cancellationToken).WaitAsync(TimeSpan.FromMilliseconds(500), cancellationToken);
                }
                catch (TimeoutException)
                {
                    // still running - report progress and wait again
                }
            }
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(true);
            }
            catch
            {
                // it may have exited in between
            }

            throw;
        }

        // Flush the async readers before looking at the log.
        process.WaitForExit();
        poll?.Invoke();

        if (process.ExitCode != 0)
        {
            string tail;
            lock (log)
            {
                tail = string.Join(Environment.NewLine, log.ToString().Split('\n')
                    .Select(l => l.TrimEnd('\r'))
                    .Where(l => l.Length > 0 && !l.StartsWith("[TIMING", StringComparison.Ordinal) && !l.StartsWith("[TRACE", StringComparison.Ordinal))
                    .TakeLast(12));
            }

            Se.WriteToolsLog($"Sidon (audio.cpp) failed, exit code {process.ExitCode}: {tail}");
            var startupHint = AudioCppRuntime.DescribeStartupExit(process.ExitCode, backend);
            throw new InvalidOperationException(
                $"Sidon speech restoration failed (exit code {process.ExitCode}).{Environment.NewLine}{startupHint}{Environment.NewLine}{tail}".Trim());
        }
    }

    private static string QuoteForLog(string arg) => arg.Contains(' ') ? "\"" + arg + "\"" : arg;
}
