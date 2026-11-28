namespace SeConv.Core;

/// <summary>
/// Ctrl+C for a conversion run. The first Ctrl+C asks the run to stop at the next safe point
/// (between files, tracks, OCR images or translation batches) so nothing half-written replaces an
/// existing output and the summary/--json result still lists what was done; a second Ctrl+C
/// ends the process at once, like before.
/// </summary>
internal static class CliCancellation
{
    /// <summary>The conventional exit code of a command stopped by Ctrl+C (128 + SIGINT).</summary>
    public const int ExitCode = 130;

    private static CancellationTokenSource _source = new();

    public static CancellationToken Token => _source.Token;

    public static bool IsCancellationRequested => _source.IsCancellationRequested;

    public static void ThrowIfCancellationRequested() => _source.Token.ThrowIfCancellationRequested();

    /// <summary>
    /// Listens for Ctrl+C until disposed. Only the convert command does this: the other commands
    /// are short and keep the default "Ctrl+C ends the process".
    /// </summary>
    public static IDisposable Listen()
    {
        _source = new CancellationTokenSource();

        void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            if (_source.IsCancellationRequested)
            {
                // Second Ctrl+C: let the runtime end the process now.
                return;
            }

            e.Cancel = true;
            Console.Error.WriteLine("Cancelling... (press Ctrl+C again to stop at once)");
            _source.Cancel();
        }

        Console.CancelKeyPress += OnCancelKeyPress;
        return new Subscription(() => Console.CancelKeyPress -= OnCancelKeyPress);
    }

    private sealed class Subscription(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
