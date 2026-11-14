using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Common;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The <c>crispasr --server</c> process of one CrispASR TTS engine: started with the engine's own
/// arguments on a loopback port, its output captured for error messages, polled on /health
/// until it is ready, and killed on request or when Subtitle Edit exits. Each engine keeps one
/// static instance and decides itself when a running server can be reused, since what is baked
/// into the process (model, voice, reference text, steps) differs per engine.
/// </summary>
internal sealed class CrispAsrTtsServer
{
    private readonly string _serverName;
    private readonly string _engineName;
    private readonly HttpClient _httpClient;

    private readonly StringBuilder _log = new();
    private Process? _process;
    private int _port;
    private string? _launchCommand;
    private bool _processExitHooked;

    /// <param name="serverName">Server name in error messages, e.g. "crispasr (vibevoice)".</param>
    /// <param name="engineName">Engine name in the tools log, e.g. "VibeVoice (CrispASR)".</param>
    /// <param name="httpClient">The engine's client, used for the /health probe.</param>
    public CrispAsrTtsServer(string serverName, string engineName, HttpClient httpClient)
    {
        _serverName = serverName;
        _engineName = engineName;
        _httpClient = httpClient;
    }

    public string BaseUrl => $"http://127.0.0.1:{_port}";

    /// <summary>The command line of the running server, for error messages.</summary>
    public string? LaunchCommand => _launchCommand;

    /// <summary>True while a started server process is still running.</summary>
    public bool IsAlive => _process is { HasExited: false } && _port != 0;

    /// <summary>True when a server was started and its process has since died.</summary>
    public bool HasExited => _process?.HasExited == true;

    /// <summary>
    /// Starts the server described by <paramref name="psi"/> (which must redirect stdout and
    /// stderr) listening on <paramref name="port"/>, and starts capturing its output.
    /// </summary>
    /// <param name="logDetail">Extra settings for the tools log line, e.g. "steps: 16, ".</param>
    public void Start(ProcessStartInfo psi, int port, string logDetail = "")
    {
        var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start {_serverName}");

        var launchCommand = LocalServerHelper.FormatLaunchCommand(psi.FileName, psi.ArgumentList);
        _launchCommand = launchCommand;
        Se.WriteToolsLog($"{_engineName} server starting — PID: {process.Id}, {logDetail}Cmd: {launchCommand}");

        lock (_log) _log.Clear();
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) lock (_log) _log.AppendLine(e.Data);
        };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) lock (_log) _log.AppendLine(e.Data);
        };
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        _process = process;
        _port = port;
        HookProcessExitOnce();
    }

    /// <summary>
    /// Waits until the server started by <see cref="Start"/> answers /health. Throws when the
    /// process exits first, and stops it and throws when it is not healthy within
    /// <paramref name="timeoutMinutes"/> (first runs may be auto-downloading gigabytes).
    /// </summary>
    public async Task WaitUntilHealthyAsync(int timeoutMinutes, CancellationToken ct)
    {
        var process = _process ?? throw new InvalidOperationException($"{_serverName} was not started");
        var port = _port;
        var deadline = DateTime.UtcNow.AddMinutes(timeoutMinutes);
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            if (process.HasExited)
            {
                var tail = SnapshotLog();
                var exitCode = process.ExitCode;
                var exitedLaunchCommand = _launchCommand;
                ClearState();
                throw new InvalidOperationException(
                    $"{_serverName} exited during startup (code {exitCode}). Output: {tail}"
                    + LocalServerHelper.LaunchCmdSuffix(exitedLaunchCommand));
            }

            if (await LocalServerHelper.ProbeHealthAsync(_httpClient, port, TimeSpan.FromSeconds(2), ct))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }

        var lastOutput = SnapshotLog();
        var timeoutLaunchCommand = _launchCommand;
        Stop();
        throw new TimeoutException(
            $"{_serverName} did not report healthy within {timeoutMinutes} minutes. Last output: {lastOutput}"
            + LocalServerHelper.LaunchCmdSuffix(timeoutLaunchCommand));
    }

    /// <summary>The last 2000 characters the server printed.</summary>
    public string SnapshotLog()
    {
        lock (_log)
        {
            var s = _log.ToString().TrimEnd();
            return s.Length > 2000 ? s[^2000..] : s;
        }
    }

    /// <summary>Stops the server if running, releasing the loaded model's memory.</summary>
    public void Stop()
    {
        var p = _process;
        ClearState();
        if (p == null)
        {
            return;
        }

        try
        {
            if (!p.HasExited)
            {
                p.Kill(entireProcessTree: true);
                p.WaitForExit(2000);
            }
        }
        catch
        {
            // best effort
        }
        finally
        {
            p.Dispose();
        }
    }

    private void ClearState()
    {
        _process = null;
        _port = 0;
        _launchCommand = null;
    }

    private void HookProcessExitOnce()
    {
        if (_processExitHooked)
        {
            return;
        }

        _processExitHooked = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Stop();
    }
}
