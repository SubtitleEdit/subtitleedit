using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public static class ProcessExtensions
{
    // Centralizes the CA1416 suppression: Process.Start() is flagged as unsupported on
    // platforms (iOS/tvOS/browser) that Subtitle Edit does not target.
    public static void StartProcess(this Process process)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416 // Validate platform compatibility
    }

    public static async Task StartAndWaitAsync(this Process process, CancellationToken cancellationToken)
    {
        process.StartProcess();
        process.DrainRedirectedOutput();
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // WaitForExitAsync only stops *waiting* on cancellation - the child keeps running.
            // Cancel means stop the work, so kill it; otherwise a cancelled merge/generation
            // leaves ffmpeg burning CPU (and holding its input files open) to completion.
            KillNoError(process);
            throw;
        }
    }

    /// <summary>
    /// Starts the process and waits for it to exit, but no longer than <paramref name="timeout"/>.
    /// On overrun the process is killed and a <see cref="TimeoutException"/> naming the command is
    /// thrown, so a wedged child process (e.g. an ffmpeg waiting on a prompt) surfaces as an error
    /// instead of freezing the pipeline forever (#12093). User cancellation kills the process too
    /// and still surfaces as <see cref="OperationCanceledException"/>.
    /// </summary>
    public static async Task StartAndWaitAsync(this Process process, CancellationToken cancellationToken, TimeSpan timeout)
    {
        process.StartProcess();
        process.DrainRedirectedOutput();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            KillNoError(process);
            if (cancellationToken.IsCancellationRequested)
            {
                throw; // user cancel - propagate as cancellation, not as a timeout
            }

            throw new TimeoutException(
                $"\"{process.StartInfo.FileName} {process.StartInfo.Arguments}\" did not finish within {timeout.TotalSeconds:0} seconds and was killed.");
        }
    }

    /// <summary>
    /// Starts pumping whichever standard streams the caller redirected, so the child can keep
    /// writing to them.
    /// </summary>
    /// <remarks>
    /// A redirected stream nobody reads is a pipe that fills up, and the child then blocks forever
    /// on its next write - <see cref="Process.WaitForExitAsync"/> waits just as long. Windows gives
    /// an anonymous pipe a 4 KB buffer by default, and ffmpeg spends more than that on its banner
    /// and stream dump alone, so voice cloning hung on Windows while the same code was fine on
    /// macOS/Linux with their 64 KB pipes (#13768). Doing it here means a caller cannot forget it;
    /// the callers that read a stream themselves never use these helpers.
    /// </remarks>
    private static void DrainRedirectedOutput(this Process process)
    {
        // Each stream gets its own try: if stdout is already being read by the caller, stderr
        // still needs draining here - one shared try would skip it and bring the pipe hang back.
        try
        {
            if (process.StartInfo.RedirectStandardOutput)
            {
                process.BeginOutputReadLine();
            }
        }
        catch (InvalidOperationException)
        {
            // Already being read asynchronously - nothing to do, the pipe is being drained.
        }

        try
        {
            if (process.StartInfo.RedirectStandardError)
            {
                process.BeginErrorReadLine();
            }
        }
        catch (InvalidOperationException)
        {
            // Already being read asynchronously - nothing to do, the pipe is being drained.
        }
    }

    private static void KillNoError(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch
        {
            // best-effort - the process may have exited in the meantime
        }
    }
}
