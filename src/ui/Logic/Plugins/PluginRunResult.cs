namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Outcome of launching a plugin process. <see cref="Succeeded"/> means the process ran
/// and produced a parseable response - it does not mean the response status was "ok".
/// </summary>
public class PluginRunResult
{
    public bool Succeeded { get; init; }

    /// <summary>Set when the process ran and wrote a valid response file.</summary>
    public PluginResponse? Response { get; init; }

    /// <summary>Failure reason when <see cref="Succeeded"/> is false (process failed, timed out, bad response, ...).</summary>
    public string? ErrorMessage { get; init; }

    public bool WasCancelled { get; init; }

    public static PluginRunResult Ok(PluginResponse response) => new() { Succeeded = true, Response = response };

    public static PluginRunResult Cancelled() => new() { Succeeded = false, WasCancelled = true };

    public static PluginRunResult Error(string message) => new() { Succeeded = false, ErrorMessage = message };
}
