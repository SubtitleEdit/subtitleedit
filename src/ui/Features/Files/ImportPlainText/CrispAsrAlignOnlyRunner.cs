using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
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

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

/// <summary>
/// Drives <c>crispasr --align-only</c>, the standalone CTC forced alignment mode: it
/// takes audio plus the text that is spoken in it and returns corrected time codes,
/// with no transcription step in between.
/// </summary>
public sealed class CrispAsrAlignOnlyRunner : ForcedAligner.IRunner
{
    private readonly string _executable;
    private readonly string _alignerModel;
    private readonly Action<string>? _log;

    public CrispAsrAlignOnlyRunner(string executable, string alignerModel, Action<string>? log = null)
    {
        _executable = executable;
        _alignerModel = alignerModel;
        _log = log;
    }

    public async Task<string> AlignAsync(string audioFileName, string textFileName, CancellationToken cancellationToken)
    {
        var outputFileName = Path.ChangeExtension(audioFileName, ".aligned.srt");

        // --align-granularity segment gives one cue per input line, which is what lets the
        // caller map cues back onto script lines positionally.
        var arguments =
            $"--align-only -am \"{_alignerModel}\" -f \"{audioFileName}\" " +
            $"--text-file \"{textFileName}\" --align-granularity segment " +
            $"--align-output \"{outputFileName}\" --no-prints";

        Se.WriteToolsLog($"{_executable} {arguments}");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(_executable, arguments)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Path.GetDirectoryName(_executable),
            },
        };

        var stdErr = new StringBuilder();
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null)
            {
                return;
            }

            lock (stdErr)
            {
                stdErr.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        if (process.ExitCode != 0 || !File.Exists(outputFileName))
        {
            var detail = stdErr.ToString().Trim();
            _log?.Invoke(detail);

            // The aligner allocates its encoder activations up front, so a window that is
            // too long for the available memory fails here rather than degrading.
            if (detail.Contains("failed to allocate", StringComparison.OrdinalIgnoreCase))
            {
                throw new ForcedAlignerException(
                    "The forced aligner ran out of memory on this window. Try a shorter window length.", detail, process.ExitCode);
            }

            throw ForcedAlignerException.FromProcessExit("The forced aligner failed", process.ExitCode, detail);
        }

        return await File.ReadAllTextAsync(outputFileName, cancellationToken).ConfigureAwait(false);
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(true);
            }
        }
        catch
        {
            // Killing a process that just exited on its own is not an error.
        }
    }
}

public sealed class ForcedAlignerException : Exception
{
    private const int MaxDetailLines = 4;
    private const int MaxDetailLineLength = 200;

    public string Detail { get; }

    /// <summary>The engine's exit code when the engine process itself failed, else null.</summary>
    public int? ExitCode { get; }

    public ForcedAlignerException(string message, string detail = "", int? exitCode = null) : base(message)
    {
        Detail = detail;
        ExitCode = exitCode;
    }

    /// <summary>
    /// For an engine run that failed or wrote no output file. Only a non-zero exit code is an
    /// engine failure (<see cref="ExitCode"/> set, counted towards stopping early); an engine that
    /// exited normally without output gets its own message instead of "exit code 0".
    /// </summary>
    public static ForcedAlignerException FromProcessExit(string what, int exitCode, string detail)
    {
        if (exitCode == 0)
        {
            var tail = GetDetailTail(detail);
            var message = $"{what} (the engine produced no output).";
            return new ForcedAlignerException(
                tail.Length > 0 ? message + Environment.NewLine + Environment.NewLine + tail : message, detail);
        }

        return new ForcedAlignerException(FormatExitFailure(what, exitCode, detail), detail, exitCode);
    }

    /// <summary>
    /// "&lt;what&gt; (exit code 0xC0000409 - the engine crashed)." followed by the last lines the
    /// engine wrote to stderr - the exit code alone rarely says what went wrong.
    /// </summary>
    public static string FormatExitFailure(string what, int exitCode, string detail)
    {
        // Windows reports a native crash as an NTSTATUS, which only means something in hex.
        var code = exitCode < 0
            ? "0x" + exitCode.ToString("X8", CultureInfo.InvariantCulture)
            : exitCode.ToString(CultureInfo.InvariantCulture);
        var hint = DescribeExitCode(exitCode);
        var message = $"{what} (exit code {code}{(hint.Length > 0 ? " - " + hint : string.Empty)}).";

        var tail = GetDetailTail(detail);
        return tail.Length > 0 ? message + Environment.NewLine + Environment.NewLine + tail : message;
    }

    internal static string DescribeExitCode(int exitCode) => unchecked((uint)exitCode) switch
    {
        0xC0000409 => "the engine crashed",
        0xC0000005 => "the engine crashed (access violation)",
        0xC000001D => "this CPU lacks an instruction the engine needs, try the CPU legacy build",
        0xC0000135 => "a DLL the engine needs is missing",
        0xC0000017 or 0xC000012D => "out of memory",
        _ => string.Empty,
    };

    internal static string GetDetailTail(string detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
        {
            return string.Empty;
        }

        var lines = detail
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();

        return string.Join(
            Environment.NewLine,
            lines.Skip(Math.Max(0, lines.Count - MaxDetailLines))
                .Select(line => line.Length > MaxDetailLineLength ? line[..MaxDetailLineLength] + "…" : line));
    }
}

/// <summary>
/// Serves windows of a single extracted 16 kHz mono WAV. The extraction happens once;
/// each window is a stream copy out of it, which keeps per-window cost negligible.
/// </summary>
public sealed class FfmpegWindowAudioSource : ForcedAligner.IAudioSource, IDisposable
{
    private readonly string _ffmpegPath;
    private readonly string _audioFileName;
    private readonly string _workFolder;
    private readonly List<string> _windowFiles = new();
    private int _windowIndex;

    public double TotalSeconds { get; }

    public FfmpegWindowAudioSource(string ffmpegPath, string audioFileName, double totalSeconds, string workFolder)
    {
        _ffmpegPath = ffmpegPath;
        _audioFileName = audioFileName;
        _workFolder = workFolder;
        TotalSeconds = totalSeconds;
    }

    public Task<IReadOnlyList<OpenAiSttChunker.SilenceInterval>> DetectSilenceAsync(CancellationToken cancellationToken)
        => OpenAiSttChunker.DetectSilenceIntervalsAsync(_ffmpegPath, _audioFileName, cancellationToken: cancellationToken);

    public async Task<string> ExtractWindowAsync(double startSeconds, double durationSeconds, CancellationToken cancellationToken)
    {
        var target = Path.Combine(
            _workFolder,
            "se-align-" + _windowIndex.ToString(CultureInfo.InvariantCulture) + ".wav");
        _windowIndex++;
        _windowFiles.Add(target);

        var ok = await OpenAiSttChunker
            .ExtractChunkAsync(_ffmpegPath, _audioFileName, target, startSeconds, durationSeconds, cancellationToken)
            .ConfigureAwait(false);

        if (!ok || !File.Exists(target))
        {
            throw new ForcedAlignerException("Could not cut the audio window with ffmpeg.");
        }

        return target;
    }

    public void Dispose()
    {
        foreach (var file in _windowFiles)
        {
            TryDelete(file);
            TryDelete(Path.ChangeExtension(file, ".txt"));
            TryDelete(Path.ChangeExtension(file, ".aligned.srt"));
        }

        _windowFiles.Clear();
    }

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
            // Temp cleanup is best effort.
        }
    }
}
