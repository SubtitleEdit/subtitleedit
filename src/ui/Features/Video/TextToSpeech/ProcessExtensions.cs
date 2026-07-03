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
        await process.WaitForExitAsync(cancellationToken);
    }

    /// <summary>
    /// Starts the process and waits for it to exit, but no longer than <paramref name="timeout"/>.
    /// On overrun the process is killed and a <see cref="TimeoutException"/> naming the command is
    /// thrown, so a wedged child process (e.g. an ffmpeg waiting on a prompt) surfaces as an error
    /// instead of freezing the pipeline forever (#12093). User cancellation still surfaces as
    /// <see cref="OperationCanceledException"/>.
    /// </summary>
    public static async Task StartAndWaitAsync(this Process process, CancellationToken cancellationToken, TimeSpan timeout)
    {
        process.StartProcess();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // best-effort - the process may have exited in the meantime
            }

            throw new TimeoutException(
                $"\"{process.StartInfo.FileName} {process.StartInfo.Arguments}\" did not finish within {timeout.TotalSeconds:0} seconds and was killed.");
        }
    }
}
