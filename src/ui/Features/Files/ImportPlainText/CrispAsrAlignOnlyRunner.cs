using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
                    "The forced aligner ran out of memory on this window. Try a shorter window length.", detail);
            }

            throw new ForcedAlignerException($"The forced aligner failed (exit code {process.ExitCode}).", detail);
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
    public string Detail { get; }

    public ForcedAlignerException(string message, string detail = "") : base(message)
    {
        Detail = detail;
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
