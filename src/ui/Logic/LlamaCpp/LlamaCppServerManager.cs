using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.LlamaCpp;

/// <summary>
/// A curated TranslateGemma model that can be downloaded and served via llama.cpp.
/// </summary>
public sealed record LlamaCppTranslateModel(string DisplayName, string FileName, string Size, string Url);

/// <summary>
/// Manages the local <c>llama-server</c> process used by the llama.cpp auto-translate engine:
/// folder/executable paths, the curated model list, and the server lifecycle (start, health
/// probe, stop, kill on app exit). Modeled on the CrispASR/Chatterbox server handling.
/// </summary>
public static class LlamaCppServerManager
{
    public static readonly IReadOnlyList<LlamaCppTranslateModel> Models = new[]
    {
        new LlamaCppTranslateModel("TranslateGemma 4B (Q4_K_M)", "translategemma-4b_Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF/resolve/main/translategemma-4b_Q4_K_M.gguf"),
        new LlamaCppTranslateModel("TranslateGemma 4B (Q5_K_M)", "translategemma-4b_Q5_K_M.gguf", "2.8 GB",
            "https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF/resolve/main/translategemma-4b_Q5_K_M.gguf"),
        new LlamaCppTranslateModel("TranslateGemma 4B (Q8_0)", "translategemma-4b-it-q8_0.gguf", "4.1 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-4b-it-Q8_0-GGUF/resolve/main/translategemma-4b-it-q8_0.gguf"),
        new LlamaCppTranslateModel("TranslateGemma 12B (Q4_K_M)", "translategemma-12b-it-q4_k_m.gguf", "7.3 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-12b-it-Q4_K_M-GGUF/resolve/main/translategemma-12b-it-q4_k_m.gguf"),
    };

    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromMinutes(5) };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverModelPath;
    private static bool _processExitHooked;
    private static readonly StringBuilder _serverLog = new();

    public static bool IsServerRunning => _serverProcess is { HasExited: false } && _serverPort != 0;

    public static string? RunningModelPath => IsServerRunning ? _serverModelPath : null;

    public static string ApiUrl => $"http://127.0.0.1:{_serverPort}/v1/chat/completions";

    public static string GetAndCreateFolder()
    {
        var folder = Se.LlamaCppFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetAndCreateModelsFolder()
    {
        var folder = Path.Combine(GetAndCreateFolder(), "models");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetExecutable()
    {
        return Path.Combine(GetAndCreateFolder(), OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server");
    }

    public static bool IsEngineInstalled()
    {
        return File.Exists(GetExecutable());
    }

    public static string GetModelPath(string fileName)
    {
        return Path.Combine(GetAndCreateModelsFolder(), fileName);
    }

    public static bool IsModelInstalled(string fileName)
    {
        var path = GetModelPath(fileName);
        return File.Exists(path) && new FileInfo(path).Length > 10_000_000;
    }

    /// <summary>
    /// Starts (or reuses) a llama-server for the given model and points
    /// <see cref="Core.Settings.ToolsSettings.LlamaCppApiUrl"/> at it. Throws on failure.
    /// </summary>
    public static async Task EnsureServerRunningAsync(string modelPath, CancellationToken cancellationToken)
    {
        if (IsServerRunning && _serverModelPath == modelPath)
        {
            Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
            return;
        }

        await ServerLock.WaitAsync(cancellationToken);
        try
        {
            if (IsServerRunning && _serverModelPath == modelPath)
            {
                Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
                return;
            }

            // Server not running, or running with a different model - (re)start.
            if (_serverProcess != null)
            {
                StopServerInternal();
            }

            var exe = GetExecutable();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException("llama-server executable not found - please download llama.cpp first.", exe);
            }

            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("llama.cpp model not found - please download a model first.", modelPath);
            }

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? GetAndCreateFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(modelPath);
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString(CultureInfo.InvariantCulture));
            // Offload all layers to the GPU when a GPU build is in use; ignored by the CPU build.
            psi.ArgumentList.Add("-ngl");
            psi.ArgumentList.Add("99");
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("8192");

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start llama-server");

            Se.WriteToolsLog($"llama-server starting - PID: {process.Id}, Cmd: {FormatLaunchCommand(exe, psi.ArgumentList)}");

            lock (_serverLog)
            {
                _serverLog.Clear();
            }

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    lock (_serverLog) _serverLog.AppendLine(e.Data);
                }
            };
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    lock (_serverLog) _serverLog.AppendLine(e.Data);
                }
            };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            _serverProcess = process;
            _serverPort = port;
            _serverModelPath = modelPath;
            HookProcessExitOnce();

            var deadline = DateTime.UtcNow.AddMinutes(5);
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    var tail = SnapshotServerLog();
                    _serverProcess = null;
                    _serverPort = 0;
                    _serverModelPath = null;
                    throw new InvalidOperationException(
                        $"llama-server exited during startup (code {process.ExitCode}). Output: {tail}");
                }

                if (await ProbeHealthAsync(port, TimeSpan.FromSeconds(2), cancellationToken))
                {
                    Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            var lastOutput = SnapshotServerLog();
            StopServerInternal();
            throw new TimeoutException(
                $"llama-server did not report healthy within 5 minutes. Last output: {lastOutput}");
        }
        finally
        {
            ServerLock.Release();
        }
    }

    public static void StopServer()
    {
        ServerLock.Wait();
        try
        {
            StopServerInternal();
        }
        finally
        {
            ServerLock.Release();
        }
    }

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverModelPath = null;
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

    public static string SnapshotServerLog()
    {
        lock (_serverLog)
        {
            var s = _serverLog.ToString().TrimEnd();
            return s.Length > 2000 ? s[^2000..] : s;
        }
    }

    private static async Task<bool> ProbeHealthAsync(int port, TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            using var resp = await HttpClient.GetAsync($"http://127.0.0.1:{port}/health", cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static int FindFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static void HookProcessExitOnce()
    {
        if (_processExitHooked)
        {
            return;
        }

        _processExitHooked = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopServerInternal();
    }

    private static string FormatLaunchCommand(string exe, System.Collections.ObjectModel.Collection<string> args)
    {
        static string Quote(string s) =>
            !string.IsNullOrEmpty(s) && s.IndexOfAny(new[] { ' ', '\t' }) >= 0
                ? "\"" + s.Replace("\"", "\\\"") + "\""
                : s;

        var sb = new StringBuilder(Quote(exe));
        foreach (var a in args)
        {
            sb.Append(' ').Append(Quote(a));
        }

        return sb.ToString();
    }
}
