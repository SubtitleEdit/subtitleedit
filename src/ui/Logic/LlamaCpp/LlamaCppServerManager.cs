using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.LlamaCpp;

/// <summary>
/// A curated llama.cpp model that can be downloaded and served. Optional <paramref name="MmprojFileName"/>
/// / <paramref name="MmprojUrl"/> are set for multimodal (vision) models that need a separate vision
/// projector. <paramref name="ChatTemplate"/> and <paramref name="NoJinja"/> override the llama-server
/// launch flags when the bundled chat template needs replacing (TranslateGemma ships a non-standard
/// Jinja template).
/// </summary>
public sealed record LlamaCppModel(
    string DisplayName,
    string FileName,
    string Size,
    string Url,
    string? MmprojFileName = null,
    string? MmprojUrl = null,
    string? ChatTemplate = null,
    bool NoJinja = false);

/// <summary>
/// Manages the local <c>llama-server</c> process used by the llama.cpp auto-translate and OCR
/// engines: folder/executable paths, the curated model lists, and the server lifecycle (start,
/// health probe, stop, kill on app exit). Modeled on the CrispASR/Chatterbox server handling.
/// </summary>
public static class LlamaCppServerManager
{
    public static readonly IReadOnlyList<LlamaCppModel> TranslateModels = new[]
    {
        new LlamaCppModel("TranslateGemma 4B (Q4_K_M)", "translategemma-4b_Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF/resolve/main/translategemma-4b_Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 4B (Q5_K_M)", "translategemma-4b_Q5_K_M.gguf", "2.8 GB",
            "https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF/resolve/main/translategemma-4b_Q5_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 4B (Q8_0)", "translategemma-4b-it-q8_0.gguf", "4.1 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-4b-it-Q8_0-GGUF/resolve/main/translategemma-4b-it-q8_0.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 12B (Q4_K_M)", "translategemma-12b-it-q4_k_m.gguf", "7.3 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-12b-it-Q4_K_M-GGUF/resolve/main/translategemma-12b-it-q4_k_m.gguf",
            ChatTemplate: "gemma", NoJinja: true),

        // Alternative model family. Qwen 3 is the strongest open model for CJK
        // (Chinese/Japanese/Korean) and competitive elsewhere — useful fallback
        // when Gemma's quirks bite (occasional refusals, formatting drift, etc).
        // --no-jinja + chatml bypasses the embedded Jinja template's
        // enable_thinking logic on the hybrid Qwen3-8B so output is clean
        // translation, not <think>...</think> reasoning blocks.
        new LlamaCppModel("Qwen 3 4B Instruct (Q4_K_M)", "Qwen_Qwen3-4B-Instruct-2507-Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3-4B-Instruct-2507-GGUF/resolve/main/Qwen_Qwen3-4B-Instruct-2507-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3 8B (Q4_K_M)", "Qwen_Qwen3-8B-Q4_K_M.gguf", "4.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3-8B-GGUF/resolve/main/Qwen_Qwen3-8B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),

        // Qwen 3.5 - newer Qwen generation. Same chatml + --no-jinja handling as Qwen 3 (bypasses the
        // embedded thinking template so the output is clean translation). Kept to <= 8 GB.
        new LlamaCppModel("Qwen 3.5 4B (Q4_K_M)", "Qwen_Qwen3.5-4B-Q4_K_M.gguf", "2.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 4B (Q8_0)", "Qwen_Qwen3.5-4B-Q8_0.gguf", "4.3 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q8_0.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 9B (Q4_K_M)", "Qwen_Qwen3.5-9B-Q4_K_M.gguf", "5.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),

        // Aya Expanse 8B (Cohere) - a dedicated multilingual model (23 languages), a good translation
        // alternative to the Gemma/Qwen families. Uses its own embedded (Cohere) chat template, so we
        // leave ChatTemplate/NoJinja at their defaults instead of forcing gemma/chatml. Kept to <= 8 GB.
        new LlamaCppModel("Aya Expanse 8B (Q4_K_M)", "aya-expanse-8b-Q4_K_M.gguf", "4.7 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q4_K_M.gguf"),
        new LlamaCppModel("Aya Expanse 8B (Q5_K_M)", "aya-expanse-8b-Q5_K_M.gguf", "5.4 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q5_K_M.gguf"),
        new LlamaCppModel("Aya Expanse 8B (Q8_0)", "aya-expanse-8b-Q8_0.gguf", "7.9 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q8_0.gguf"),
    };

    // Models for the AI review tool (proofreading). Translation-tuned models (TranslateGemma,
    // Aya) are deliberately absent - proofreading needs general instruction-following and strict
    // JSON output, where the plain instruct models are much stronger. Kept to <= 8 GB.
    public static readonly IReadOnlyList<LlamaCppModel> ReviewModels = new[]
    {
        new LlamaCppModel("Qwen 3.5 4B (Q4_K_M)", "Qwen_Qwen3.5-4B-Q4_K_M.gguf", "2.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 4B (Q8_0)", "Qwen_Qwen3.5-4B-Q8_0.gguf", "4.3 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q8_0.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 9B (Q4_K_M)", "Qwen_Qwen3.5-9B-Q4_K_M.gguf", "5.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Gemma 3 4B it (Q4_K_M)", "google_gemma-3-4b-it-Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/bartowski/google_gemma-3-4b-it-GGUF/resolve/main/google_gemma-3-4b-it-Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("Gemma 3 12B it (Q4_K_M)", "google_gemma-3-12b-it-Q4_K_M.gguf", "7.3 GB",
            "https://huggingface.co/bartowski/google_gemma-3-12b-it-GGUF/resolve/main/google_gemma-3-12b-it-Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
    };

    public static readonly IReadOnlyList<LlamaCppModel> OcrModels = new[]
    {
        new LlamaCppModel("GLM-OCR 0.9B (Q8_0)", "GLM-OCR-Q8_0.gguf", "1.4 GB",
            "https://huggingface.co/ggml-org/GLM-OCR-GGUF/resolve/main/GLM-OCR-Q8_0.gguf",
            MmprojFileName: "mmproj-GLM-OCR-Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/ggml-org/GLM-OCR-GGUF/resolve/main/mmproj-GLM-OCR-Q8_0.gguf"),
        new LlamaCppModel("LightOnOCR 1B (Q8_0)", "LightOnOCR-1B-1025-Q8_0.gguf", "1.2 GB",
            "https://huggingface.co/ggml-org/LightOnOCR-1B-1025-GGUF/resolve/main/LightOnOCR-1B-1025-Q8_0.gguf",
            MmprojFileName: "mmproj-LightOnOCR-1B-1025-Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/ggml-org/LightOnOCR-1B-1025-GGUF/resolve/main/mmproj-LightOnOCR-1B-1025-Q8_0.gguf"),
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

    /// <summary>
    /// Returns the update status of the installed llama-server binary relative to the version
    /// pinned in <see cref="LlamaCppDownloadService"/>. The check hashes the executable on disk
    /// directly (no install-time sidecar needed) because <see cref="DownloadHashManager.LlamaCpp"/>
    /// tracks SHA-256s of the unpacked llama-server binary per OS/arch. Returns
    /// <see cref="DownloadHashManager.UpdateStatus.Unknown"/> when nothing is installed, the
    /// platform key cannot be resolved, or the on-disk hash does not match any known release —
    /// so callers do not false-positive an update prompt.
    /// </summary>
    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus()
    {
        var exe = GetExecutable();
        if (!File.Exists(exe))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        var key = DownloadHashManager.ResolveLlamaCppExecutableKey();
        if (string.IsNullOrEmpty(key))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        try
        {
            var hash = DownloadHashManager.ComputeSha256(exe);
            return DownloadHashManager.GetStatus(key, hash);
        }
        catch
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }
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
    /// Returns the curated <see cref="TranslateModels"/> plus any other <c>*.gguf</c> the user has
    /// dropped into the llama.cpp models folder. Custom entries are emitted with an empty <c>Url</c>
    /// (no download needed - already on disk), the file name as <c>DisplayName</c>, and a
    /// human-readable file size. <c>mmproj-*.gguf</c> sidecars are skipped because they're not
    /// standalone translation models.
    /// </summary>
    public static IReadOnlyList<LlamaCppModel> GetAllTranslateModels()
    {
        return GetCuratedPlusCustomModels(TranslateModels);
    }

    public static IReadOnlyList<LlamaCppModel> GetAllReviewModels()
    {
        return GetCuratedPlusCustomModels(ReviewModels);
    }

    private static IReadOnlyList<LlamaCppModel> GetCuratedPlusCustomModels(IReadOnlyList<LlamaCppModel> curated)
    {
        var folder = GetAndCreateModelsFolder();
        if (!Directory.Exists(folder))
        {
            return curated;
        }

        // "known" spans all curated lists so e.g. a downloaded review model does not show up
        // as a custom entry in the translate list (and vice versa).
        var knownNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in TranslateModels.Concat(ReviewModels).Concat(OcrModels))
        {
            knownNames.Add(m.FileName);
            if (!string.IsNullOrEmpty(m.MmprojFileName))
            {
                knownNames.Add(m.MmprojFileName);
            }
        }

        var custom = new List<LlamaCppModel>();
        try
        {
            foreach (var path in Directory.EnumerateFiles(folder, "*.gguf", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(path);
                if (knownNames.Contains(name))
                {
                    continue;
                }
                if (name.StartsWith("mmproj-", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var size = FormatFileSize(new FileInfo(path).Length);
                custom.Add(new LlamaCppModel(name, name, size, Url: string.Empty));
            }
        }
        catch
        {
            // ignore - if the folder can't be scanned (locked / IO error) just fall back to curated only.
            return curated;
        }

        if (custom.Count == 0)
        {
            return curated;
        }

        custom.Sort((a, b) => string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase));
        return curated.Concat(custom).ToList();
    }

    private static string FormatFileSize(long bytes)
    {
        const double gb = 1024d * 1024d * 1024d;
        const double mb = 1024d * 1024d;
        if (bytes >= gb)
        {
            return (bytes / gb).ToString("0.#", CultureInfo.InvariantCulture) + " GB";
        }
        if (bytes >= mb)
        {
            return (bytes / mb).ToString("0", CultureInfo.InvariantCulture) + " MB";
        }
        return (bytes / 1024d).ToString("0", CultureInfo.InvariantCulture) + " KB";
    }

    public static bool IsModelInstalled(LlamaCppModel model)
    {
        if (!IsModelInstalled(model.FileName))
        {
            return false;
        }

        if (model.MmprojFileName == null)
        {
            return true;
        }

        return IsModelInstalled(model.MmprojFileName);
    }

    /// <summary>
    /// Starts (or reuses) a llama-server for the given model and points
    /// <see cref="Core.Settings.ToolsSettings.LlamaCppApiUrl"/> at it. Throws on failure.
    /// </summary>
    public static async Task EnsureServerRunningAsync(LlamaCppModel model, CancellationToken cancellationToken)
    {
        var modelPath = GetModelPath(model.FileName);
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

            string? mmprojPath = null;
            if (model.MmprojFileName != null)
            {
                mmprojPath = GetModelPath(model.MmprojFileName);
                if (!File.Exists(mmprojPath))
                {
                    throw new FileNotFoundException("llama.cpp vision projector not found - please download the model first.", mmprojPath);
                }
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
            if (mmprojPath != null)
            {
                psi.ArgumentList.Add("--mmproj");
                psi.ArgumentList.Add(mmprojPath);
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString(CultureInfo.InvariantCulture));
            // Offload all layers to the GPU when a GPU build is in use; ignored by the CPU build.
            psi.ArgumentList.Add("-ngl");
            psi.ArgumentList.Add("99");
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("8192");
            if (model.NoJinja)
            {
                psi.ArgumentList.Add("--no-jinja");
            }
            if (model.ChatTemplate != null)
            {
                psi.ArgumentList.Add("--chat-template");
                psi.ArgumentList.Add(model.ChatTemplate);
            }

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
