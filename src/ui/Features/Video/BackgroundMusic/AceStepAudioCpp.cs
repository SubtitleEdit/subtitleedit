using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.BackgroundMusic;

/// <summary>What to generate: one text2music request.</summary>
public sealed class MusicGenerationRequest
{
    public required string Prompt { get; init; }
    public required int Bpm { get; init; }
    public required int DurationSeconds { get; init; }
    public required long Seed { get; init; }
    public required string OutputFileName { get; init; }
}

public enum MusicGenerationPhase
{
    LoadingModel,
    Composing,
    Generating,
    Decoding,
    Done,
}

public sealed class MusicGenerationProgress
{
    public MusicGenerationPhase Phase { get; init; }

    /// <summary>0..100, estimated from the phase weights measured on an M4 (see <see cref="AceStepProgressParser"/>).</summary>
    public double Percent { get; init; }
}

/// <summary>
/// ACE-Step 1.5 Turbo (MIT) music generation through the shared audio.cpp runtime
/// (<see cref="AudioCppRuntime"/>, family <c>ace_step</c>). Runs <c>audiocpp_cli --task gen</c>
/// once per request rather than a long-lived server: a generation is a one-off that takes a
/// minute or two, and the ~20 s model load is small next to it.
/// </summary>
public static class AceStepAudioCpp
{
    public const string FamilyName = "ace_step";
    public const string DisplayName = "ACE-Step 1.5";
    public const string ModelFileName = "ace-step-1.5-turbo-q8_0.gguf";
    public const long ModelFileSize = 6_185_460_032;
    public const string ModelSizeText = "5.8 GB";
    public const string ModelUrl = "https://huggingface.co/audio-cpp/audio.cpp-gguf/resolve/main/ACE-Step1.5-GGUF/turbo/" + ModelFileName;

    /// <summary>
    /// The model ignores lyrics-less requests unless told they are instrumental, and a talking-head
    /// video needs music that stays out of the way, so every preset shares these.
    /// </summary>
    public const string Lyrics = "[Instrumental]";
    public const string NegativePrompt = "vocals, singing, choir, rap, aggressive, distorted guitar, heavy drums, dark, horror";

    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "models", "ACE-Step1.5-GGUF");
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

    private static string? _outFormatSupportStamp;
    private static bool _outFormatSupported;

    /// <summary>
    /// Whether the installed CLI can write float WAV (<c>--out-format float32</c>). ACE-Step decodes
    /// above ±1.0, so 16-bit output hard-clips before we get to lower the gain. Older runtimes
    /// don't have the option and reject unknown flags, hence the check (cached per binary).
    /// </summary>
    public static bool SupportsFloatOutput()
    {
        var stamp = AudioCppRuntime.GetServerExecutableStamp();
        if (_outFormatSupportStamp == stamp)
        {
            return _outFormatSupported;
        }

        var supported = false;
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = GetCliExecutable(),
                WorkingDirectory = AudioCppRuntime.GetSetEngineFolder(),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("--help");
            using var process = Process.Start(psi);
            if (process != null)
            {
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(10_000))
                {
                    process.Kill(true);
                }

                supported = (stdout.Result + stderr.Result).Contains("--out-format", StringComparison.Ordinal);
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "ACE-Step (audio.cpp): --help probe failed");
        }

        _outFormatSupportStamp = stamp;
        _outFormatSupported = supported;
        return supported;
    }

    public static List<string> BuildArguments(MusicGenerationRequest request, string modelPath, string backend, bool floatOutput)
    {
        var args = new List<string>
        {
            "--task", "gen",
            "--family", FamilyName,
            "--model", modelPath,
            "--backend", backend,
            "--task-route", "text2music",
            "--text", request.Prompt,
            "--lyrics", Lyrics,
            "--duration-seconds", request.DurationSeconds.ToString(CultureInfo.InvariantCulture),
            "--seed", request.Seed.ToString(CultureInfo.InvariantCulture),
            "--request-option", "bpm=" + request.Bpm.ToString(CultureInfo.InvariantCulture),
            "--request-option", "timesignature=4",
            "--request-option", "negative_prompt=" + NegativePrompt,
            "--log",
            "--out", request.OutputFileName,
        };

        if (floatOutput)
        {
            args.Add("--out-format");
            args.Add("float32");
        }

        return args;
    }

    /// <summary>
    /// Runs one generation. Throws <see cref="OperationCanceledException"/> when cancelled (the
    /// process tree is killed) and <see cref="InvalidOperationException"/> with the tail of the log
    /// when the CLI fails or writes no audio.
    /// </summary>
    public static async Task RunAsync(MusicGenerationRequest request, IProgress<MusicGenerationProgress>? progress, CancellationToken cancellationToken)
    {
        var exe = GetCliExecutable();
        var backend = AudioCppRuntime.GetBackend();
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

        foreach (var arg in BuildArguments(request, GetModelPath(), backend, SupportsFloatOutput()))
        {
            psi.ArgumentList.Add(arg);
        }

        TryDelete(request.OutputFileName);

        var parser = new AceStepProgressParser(request.DurationSeconds);
        var log = new StringBuilder();
        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
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

            var update = parser.Parse(line);
            if (update != null)
            {
                progress?.Report(update);
            }
        }

        process.OutputDataReceived += (_, e) => OnLine(e.Data);
        process.ErrorDataReceived += (_, e) => OnLine(e.Data);

        Se.WriteToolsLog("ACE-Step (audio.cpp) music generation: " + exe + " " + string.Join(" ", psi.ArgumentList.Select(QuoteForLog)));
        progress?.Report(new MusicGenerationProgress { Phase = MusicGenerationPhase.LoadingModel, Percent = 0 });

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start " + exe);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
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

            TryDelete(request.OutputFileName);
            throw;
        }

        // Flush the async readers before looking at the log.
        process.WaitForExit();

        if (process.ExitCode != 0 || !File.Exists(request.OutputFileName))
        {
            string tail;
            lock (log)
            {
                tail = string.Join(Environment.NewLine, log.ToString().Split('\n')
                    .Select(l => l.TrimEnd('\r'))
                    .Where(l => l.Length > 0 && !l.StartsWith("[TIMING", StringComparison.Ordinal) && !l.StartsWith("[TRACE", StringComparison.Ordinal))
                    .TakeLast(12));
            }

            Se.WriteToolsLog($"ACE-Step (audio.cpp) failed, exit code {process.ExitCode}: {tail}");
            var startupHint = AudioCppRuntime.DescribeStartupExit(process.ExitCode, backend);
            throw new InvalidOperationException(
                $"Music generation failed (exit code {process.ExitCode}).{Environment.NewLine}{startupHint}{Environment.NewLine}{tail}".Trim());
        }

        progress?.Report(new MusicGenerationProgress { Phase = MusicGenerationPhase.Done, Percent = 100 });
    }

    private static string QuoteForLog(string arg) => arg.Contains(' ') ? "\"" + arg + "\"" : arg;

    private static void TryDelete(string fileName)
    {
        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        catch
        {
            // best effort
        }
    }
}

/// <summary>
/// Maps <c>audiocpp_cli --log</c> lines to a phase and an estimated percentage. The CLI prints a
/// <c>[TIMING ts=…] key value</c> line as each step finishes; only the VAE decode reports steps
/// (one line per chunk), the other phases just start and end. Weights are from a 60 s track on an
/// M4 (Metal): load ~20 s, planner ~25 s, text/DiT ~10 s, decode ~37 s.
/// </summary>
public sealed class AceStepProgressParser
{
    private const double ComposingStart = 15;
    private const double GeneratingStart = 45;
    private const double DecodingStart = 55;

    private MusicGenerationPhase _phase = MusicGenerationPhase.LoadingModel;
    private int _expectedChunks;
    private int _decodedChunks;
    private int _latentFrames;
    private int _chunkFrames;
    private int _overlapFrames;

    public AceStepProgressParser(int durationSeconds)
    {
        // 25 latent frames per second, 128-frame chunks stepping by 64 — replaced by the logged values.
        _expectedChunks = Math.Max(1, (int)Math.Ceiling(durationSeconds * 25 / 64.0));
    }

    public MusicGenerationProgress? Parse(string line)
    {
        var (key, value) = SplitLogLine(line);
        if (key == null)
        {
            return null;
        }

        switch (key)
        {
            case "ace_step.session.ensure_planner_ms":
                return Move(MusicGenerationPhase.Composing, ComposingStart);
            case "ace_step.session.ensure_pre_dit_ms":
            case "ace_step.session.ensure_diffusion_ms":
                return Move(MusicGenerationPhase.Generating, GeneratingStart);
            case "ace_step.session.ensure_vae_decoder_ms":
                return Move(MusicGenerationPhase.Decoding, DecodingStart);
            case "ace_step.diffusion.context_frames":
                _latentFrames = ParseInt(value);
                UpdateExpectedChunks();
                return null;
            case "ace_step.vae.decode.chunk_frames":
                _chunkFrames = ParseInt(value);
                UpdateExpectedChunks();
                return null;
            case "ace_step.vae.decode.overlap_frames":
                _overlapFrames = ParseInt(value);
                UpdateExpectedChunks();
                return null;
            case "ace_step.vae.decode.total_ms":
                _decodedChunks++;
                var fraction = Math.Min(1.0, (double)_decodedChunks / _expectedChunks);
                return Move(MusicGenerationPhase.Decoding, DecodingStart + fraction * (99 - DecodingStart));
            default:
                return null;
        }
    }

    private MusicGenerationProgress Move(MusicGenerationPhase phase, double percent)
    {
        if (phase > _phase)
        {
            _phase = phase;
        }

        return new MusicGenerationProgress { Phase = _phase, Percent = percent };
    }

    private void UpdateExpectedChunks()
    {
        var step = _chunkFrames - 2 * _overlapFrames;
        if (_latentFrames > 0 && _chunkFrames > 0 && step > 0)
        {
            _expectedChunks = Math.Max(1, (int)Math.Ceiling((double)_latentFrames / step));
        }
    }

    private static int ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;

    internal static (string? Key, string? Value) SplitLogLine(string line)
    {
        if (!line.StartsWith('['))
        {
            return (null, null);
        }

        var close = line.IndexOf(']');
        if (close < 0 || close + 1 >= line.Length)
        {
            return (null, null);
        }

        var parts = line[(close + 1)..].Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => (null, null),
            1 => (parts[0], null),
            _ => (parts[0], parts[1].Trim()),
        };
    }
}
