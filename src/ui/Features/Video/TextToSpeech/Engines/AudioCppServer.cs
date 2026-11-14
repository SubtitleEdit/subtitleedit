using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The audiocpp_server process of one audio.cpp engine: started on a free loopback port with the
/// engine's own config, reused while the model, backend and server binary stay the same, and
/// stopped when any of them changes, on request, or when Subtitle Edit exits. Each engine keeps
/// one static instance, so engines never share a server.
/// </summary>
internal sealed class AudioCppServer
{
    private readonly string _engineName;
    private readonly string _modelName;
    private readonly string _familyName;
    private readonly HttpClient _httpClient;

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly StringBuilder _log = new();
    private Process? _process;
    private int _port;
    private string? _launchCommand;

    // Only the model and the backend are baked into the running server — voice is per request.
    private string? _modelKey;
    private string? _backend;
    private string? _exeStamp;
    private bool _processExitHooked;

    /// <param name="engineName">Engine name in messages, e.g. "Fish Audio S2 Pro".</param>
    /// <param name="modelName">Model name in the missing/incomplete model file messages.</param>
    /// <param name="familyName">Family audio.cpp registers the model under, e.g. "fish_audio".</param>
    /// <param name="httpClient">The engine's client, used for the /health probe.</param>
    public AudioCppServer(string engineName, string modelName, string familyName, HttpClient httpClient)
    {
        _engineName = engineName;
        _modelName = modelName;
        _familyName = familyName;
        _httpClient = httpClient;
    }

    public string BaseUrl => $"http://127.0.0.1:{_port}";

    /// <summary>The command line of the running server, for error messages.</summary>
    public string? LaunchCommand => _launchCommand;

    /// <summary>True when a server was started and its process has since died.</summary>
    public bool HasExited => _process?.HasExited == true;

    /// <summary>
    /// Starts the server for <paramref name="modelKey"/> unless one is already running with the
    /// same model, backend and server binary.
    /// </summary>
    /// <param name="modelKey">The selected model; a different one restarts the server.</param>
    /// <param name="checkModel">Path of the model file and whether it is complete.</param>
    /// <param name="writeServerConfig">Writes the config for (port, backend) and returns its path.</param>
    public async Task EnsureRunningAsync(
        string modelKey,
        Func<(string Path, bool IsValid)> checkModel,
        Func<int, string, string> writeServerConfig,
        CancellationToken ct)
    {
        var backend = AudioCppRuntime.GetBackend();
        var exeStamp = AudioCppRuntime.GetServerExecutableStamp();

        if (IsRunning(modelKey, backend, exeStamp))
        {
            return;
        }

        await _lock.WaitAsync(ct);
        try
        {
            if (IsRunning(modelKey, backend, exeStamp))
            {
                return;
            }

            if (_process != null)
            {
                Stop();
            }

            var exe = AudioCppRuntime.GetServerExecutable();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException(
                    $"audio.cpp server not found. Download the {_engineName} engine first.", exe);
            }

            var (modelPath, modelIsValid) = checkModel();
            if (!modelIsValid)
            {
                throw new FileNotFoundException(
                    File.Exists(modelPath)
                        ? $"The {_modelName} model file is incomplete: {modelPath}. Delete it and download again."
                        : $"The {_modelName} model file is missing: {modelPath}",
                    modelPath);
            }

            var port = LocalServerHelper.FindFreeLoopbackPort();
            var configPath = writeServerConfig(port, backend);

            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? AudioCppRuntime.GetSetEngineFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                // The server writes UTF-8. Without these the reader decodes it in the OS default
                // codepage, and non-ASCII text in the captured log - the line being synthesised,
                // upstream's em dashes - reaches bug reports as mojibake (#13572).
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            psi.ArgumentList.Add("--config");
            psi.ArgumentList.Add(configPath);

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException($"Failed to start audiocpp_server ({_familyName})");

            var launchCommand = LocalServerHelper.FormatLaunchCommand(exe, psi.ArgumentList);
            _launchCommand = launchCommand;
            Se.WriteToolsLog($"{_engineName} (audio.cpp) server starting — PID: {process.Id}, Cmd: {launchCommand}");

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
            _modelKey = modelKey;
            _backend = backend;
            _exeStamp = AudioCppRuntime.GetServerExecutableStamp();
            HookProcessExitOnce();

            // The config uses lazy_load, so /health answers within a second or two — the multi-GB
            // model is only read on the first synthesis request. A long deadline here would just
            // hide a crash-at-startup (e.g. a Vulkan build on a box with no Vulkan driver, which
            // dies in the loader with 0xC0000135 before printing anything).
            var deadline = DateTime.UtcNow.AddMinutes(2);
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
                        $"audiocpp_server exited during startup (code {exitCode}). "
                        + AudioCppRuntime.DescribeStartupExit(exitCode, backend)
                        + $" Output: {tail}"
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
                $"audiocpp_server did not report healthy within 2 minutes. Last output: {lastOutput}"
                + LocalServerHelper.LaunchCmdSuffix(timeoutLaunchCommand));
        }
        finally
        {
            _lock.Release();
        }
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

    /// <summary>
    /// Stop the server if running, releasing the loaded model's working set. audio.cpp never
    /// unloads a model on its own once loaded, so this is the only way the memory comes back.
    /// </summary>
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

    private bool IsRunning(string modelKey, string? backend, string? exeStamp) =>
        _process is { HasExited: false } && _port != 0
        && string.Equals(_modelKey, modelKey, StringComparison.OrdinalIgnoreCase)
        && string.Equals(_backend, backend, StringComparison.OrdinalIgnoreCase)
        && string.Equals(_exeStamp, exeStamp, StringComparison.Ordinal);

    private void ClearState()
    {
        _process = null;
        _port = 0;
        _launchCommand = null;
        _modelKey = null;
        _backend = null;
        _exeStamp = null;
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
