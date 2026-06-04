using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Alibaba CosyVoice3 0.5B (Dec 2025 release) run through the CrispASR runtime. LLM + flow-matching DiT
/// + HiFT vocoder architecture with 9 languages and 18 Mandarin dialects. Unlike VibeVoice/IndexTTS which
/// clone from a user-supplied reference WAV, CosyVoice3 uses a BAKED-IN voice bank shipped in
/// <c>cosyvoice3-voices.gguf</c> (~665 KB). The user picks a preset name (e.g. "fleurs-en", "zero_shot");
/// crispasr loads the voice bank at server startup via the <c>--voice &lt;name&gt;</c> flag.
///
/// The repo on Hugging Face (cstr/cosyvoice3-0.5b-2512-GGUF) ships ~5 companion files:
///   - LLM           : cosyvoice3-llm-{q4_k,f16}.gguf  (we manage this)
///   - Flow          : cosyvoice3-flow-{q8_0,f16}.gguf  (crispasr --auto-download)
///   - HiFT vocoder  : cosyvoice3-hift-f16.gguf         (crispasr --auto-download)
///   - S3 tokenizer  : cosyvoice3-s3tok-{q4_k,f16}.gguf (crispasr --auto-download)
///   - CAMPPlus      : cosyvoice3-campplus-f16.gguf     (crispasr --auto-download)
///   - Voice bank    : cosyvoice3-voices.gguf           (crispasr --auto-download)
///
/// First-pass integration only stages the LLM file in SE's CrispASR/models folder; the rest are
/// fetched lazily by <c>--auto-download</c> into <c>~/.cache/crispasr/</c> on first synth. Keeping
/// a single primary file mirrors VibeVoice's layout and avoids a 6-file download-progress UI.
/// </summary>
public class CosyVoice3CrispAsr : ITtsEngine
{
    public string Name => "CosyVoice3 (CrispASR)";
    public string Description => "Alibaba CosyVoice3 0.5B with 9 languages + 18 zh dialects, via CrispASR";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    // Two LLM quants — Q4_K is the lightweight default (~384 MB), F16 the reference (~1.29 GB).
    // The flow/hift/s3tok/campplus/voices companion files (~360-665 MB) are fetched by crispasr
    // --auto-download on first run; we don't stage them ourselves yet.
    public const string ModelKeyQ4K = "Q4_K (~745 MB total)";
    public const string ModelKeyF16 = "F16 (~2.5 GB total)";
    public const string DefaultModelKey = ModelKeyQ4K;

    public const string LlmQ4KFileName = "cosyvoice3-llm-q4_k.gguf";
    public const string LlmF16FileName = "cosyvoice3-llm-f16.gguf";

    public const string BackendName = "cosyvoice3-tts";

    // Preset voice names baked into cosyvoice3-voices.gguf. Reading these dynamically would require
    // either a running server (/v1/voices) or parsing the voices GGUF, neither of which is worth the
    // complexity for a static list that only changes when cstr re-uploads the voice bank.
    public static readonly (string Display, string Preset)[] Presets =
    {
        ("Zero-shot (Mandarin)",   "zero_shot"),
        ("FLEURS English",         "fleurs-en"),
        ("FLEURS German",          "fleurs-de"),
        ("FLEURS Mandarin",        "fleurs-zh"),
        ("FLEURS Japanese",        "fleurs-ja"),
        ("FLEURS French",          "fleurs-fr"),
        ("FLEURS Spanish",         "fleurs-es"),
        ("FLEURS Korean",          "fleurs-ko"),
    };

    // Exact byte sizes from the HF tree API (size field, == on-disk bytes). Same truncation
    // guard as VibeVoice/IndexTTS — a partial file would otherwise crash the loader at server
    // startup with an opaque access violation.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [LlmQ4KFileName] = 383891200L,
        [LlmF16FileName] = 1289653952L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey switch
        {
            ModelKeyF16 => ModelKeyF16,
            _ => ModelKeyQ4K,
        };
    }

    public static string GetLlmFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyF16 => LlmF16FileName,
        _ => LlmQ4KFileName,
    };

    public static bool IsValidLocalModelFile(string path, string fileName)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        if (!ExpectedFileSizes.TryGetValue(fileName, out var expected))
        {
            return true;
        }

        try
        {
            return new FileInfo(path).Length == expected;
        }
        catch
        {
            return false;
        }
    }

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5),
    };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverLaunchCommand;
    private static string? _serverModelKey;
    // CosyVoice3 takes the active preset via --voice at startup (like IndexTTS), so a preset
    // change requires teardown + restart. Tracked here so a no-op restart is avoided.
    private static string? _serverPreset;
    private static bool _processExitHooked;
    private static readonly StringBuilder _serverLog = new();

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetCrispAsrExecutable()));
    }

    public override string ToString() => Name;

    public static string GetCrispAsrExecutable()
    {
        return new CrispAsrCohere().GetExecutable();
    }

    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus()
    {
        var exe = GetCrispAsrExecutable();
        if (!File.Exists(exe))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        var folder = Path.GetDirectoryName(exe);
        return string.IsNullOrEmpty(folder)
            ? DownloadHashManager.UpdateStatus.Unknown
            : DownloadHashManager.GetSidecarStatus(folder);
    }

    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "CosyVoice3CrispAsr");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetModelsFolder()
    {
        var modelsFolder = Path.Combine(Se.CrispAsrFolder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public static string GetLlmPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetLlmFileName(modelKey));

    public static string GetCrispAsrCacheFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "crispasr");

    public static bool TrySeedModelFromCrispAsrCache(string fileName, string destinationPath)
    {
        if (IsValidLocalModelFile(destinationPath, fileName))
        {
            return true;
        }

        try
        {
            if (File.Exists(destinationPath))
            {
                Se.LogError($"CosyVoice3 (CrispASR): removing wrong-sized local model file {destinationPath}");
                File.Delete(destinationPath);
            }

            var cachePath = Path.Combine(GetCrispAsrCacheFolder(), fileName);
            if (!IsValidLocalModelFile(cachePath, fileName))
            {
                return false;
            }

            File.Copy(cachePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"CosyVoice3 (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetLlmPath(modelKey), GetLlmFileName(modelKey));

    public Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>();
        foreach (var (display, preset) in Presets)
        {
            result.Add(new Voice(new CosyVoice3Voice(display, preset)));
        }
        return Task.FromResult(result.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ4K, ModelKeyF16 });

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    public async Task<TtsResult> Speak(
        string text,
        string outputFolder,
        Voice voice,
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken)
    {
        if (voice.EngineVoice is not CosyVoice3Voice cosyVoice)
        {
            throw new ArgumentException("Voice is not a CosyVoice3Voice");
        }

        if (string.IsNullOrEmpty(cosyVoice.Preset))
        {
            throw new InvalidOperationException(
                "CosyVoice3 (CrispASR) requires a preset. Pick one of the FLEURS-* voices or zero_shot.");
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, cosyVoice.Preset, cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");

        var speed = Math.Clamp(Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrSpeed, 0.25, 4.0);
        var payload = new Dictionary<string, object>
        {
            ["input"] = text,
            ["response_format"] = "wav",
            ["voice"] = cosyVoice.Preset,
            ["speed"] = speed,
        };

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"CosyVoice3 (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (preset={cosyVoice.Preset}, textLen={text.Length})");

        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PostAsync($"{ServerBaseUrl}/v1/audio/speech", content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            var serverLog = SnapshotServerLog();
            var launchCommand = _serverLaunchCommand;
            var died = _serverProcess?.HasExited == true;
            if (died)
            {
                StopServerInternal();
            }

            var failMsg = $"CosyVoice3 (CrispASR) request failed — Preset: {cosyVoice.Preset}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "CosyVoice3 (CrispASR) — the crispasr server crashed during synthesis."
                    : "CosyVoice3 (CrispASR) request failed — the connection to the crispasr server was dropped.")
                + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                + LaunchCmdSuffix(launchCommand),
                ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await SafeReadErrorAsync(response, cancellationToken);
                var serverLog = SnapshotServerLog();
                var launchCommand = _serverLaunchCommand;
                var errMsg = $"CosyVoice3 (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Preset: {cosyVoice.Preset}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"CosyVoice3 (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LaunchCmdSuffix(launchCommand));
            }

            await using var fileStream = File.Create(outputFileName);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
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

    private static string LaunchCmdSuffix(string? launchCommand) =>
        string.IsNullOrEmpty(launchCommand)
            ? string.Empty
            : $"{Environment.NewLine}Launch command: {launchCommand}";

    private static async Task<string> SafeReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            return $"<failed to read error body: {ex.Message}>";
        }
    }

    private static async Task EnsureServerRunningAsync(string modelKey, string preset, CancellationToken ct)
    {
        if (_serverProcess is { HasExited: false } && _serverPort != 0
            && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(_serverPreset, preset, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0
                && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
                && string.Equals(_serverPreset, preset, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (_serverProcess != null)
            {
                StopServerInternal();
            }

            var exe = GetCrispAsrExecutable();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException(
                    "CrispASR executable not found. Install CrispASR via Video → Audio to text first.", exe);
            }

            // LLM is the only file we stage locally. Flow / HiFT / S3 tokenizer / CAMPPlus /
            // voices.gguf are fetched by crispasr --auto-download into ~/.cache/crispasr/ on
            // first run. Always pass --auto-download so a missing companion never bricks startup.
            var llmFileName = GetLlmFileName(modelKey);
            var llmPath = GetLlmPath(modelKey);
            var hasLocalLlm = IsValidLocalModelFile(llmPath, llmFileName);

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? GetSetFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            psi.ArgumentList.Add("--server");
            psi.ArgumentList.Add("--backend");
            psi.ArgumentList.Add(BackendName);
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(hasLocalLlm ? llmPath : "auto");
            // CosyVoice3 needs ~5 companion files alongside the LLM. We don't stage them, so
            // --auto-download is always required to pull whichever ones aren't already in the
            // crispasr cache. Idempotent when everything is present.
            psi.ArgumentList.Add("--auto-download");
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());
            // Preset is baked in at startup (like IndexTTS's --voice). Changing presets means
            // teardown + restart of the server — handled by the (modelKey, preset) cache key.
            psi.ArgumentList.Add("--voice");
            psi.ArgumentList.Add(preset);

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (cosyvoice3-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("CosyVoice3 (CrispASR) server starting — "
                + $"PID: {process.Id}, "
                + $"Cmd: {launchCommand}");

            lock (_serverLog) _serverLog.Clear();
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) lock (_serverLog) _serverLog.AppendLine(e.Data);
            };
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) lock (_serverLog) _serverLog.AppendLine(e.Data);
            };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            _serverProcess = process;
            _serverPort = port;
            _serverModelKey = modelKey;
            _serverPreset = preset;
            HookProcessExitOnce();

            // First-run auto-download (~745 MB minimum, ~2.5 GB for F16) needs a generous timeout.
            var deadline = DateTime.UtcNow.AddMinutes(hasLocalLlm ? 10 : 30);
            while (DateTime.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    var tail = SnapshotServerLog();
                    var exitCode = process.ExitCode;
                    var exitedLaunchCommand = _serverLaunchCommand;
                    _serverProcess = null;
                    _serverPort = 0;
                    _serverLaunchCommand = null;
                    _serverModelKey = null;
                    _serverPreset = null;
                    throw new InvalidOperationException(
                        $"crispasr (cosyvoice3-tts) exited during startup (code {exitCode}). Output: {tail}"
                        + LaunchCmdSuffix(exitedLaunchCommand));
                }
                if (await ProbeHealthAsync(port, TimeSpan.FromSeconds(2), ct))
                {
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }

            var lastOutput = SnapshotServerLog();
            var timeoutLaunchCommand = _serverLaunchCommand;
            StopServerInternal();
            throw new TimeoutException(
                $"crispasr (cosyvoice3-tts) did not report healthy within {(hasLocalLlm ? 10 : 30)} minutes. Last output: {lastOutput}"
                + LaunchCmdSuffix(timeoutLaunchCommand));
        }
        finally
        {
            ServerLock.Release();
        }
    }

    private static string SnapshotServerLog()
    {
        lock (_serverLog)
        {
            var s = _serverLog.ToString().TrimEnd();
            return s.Length > 2000 ? s[^2000..] : s;
        }
    }

    private static async Task<bool> ProbeHealthAsync(int port, TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
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
        if (_processExitHooked) return;
        _processExitHooked = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopServerInternal();
    }

    public static void StopServer() => StopServerInternal();

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverLaunchCommand = null;
        _serverModelKey = null;
        _serverPreset = null;
        if (p == null) return;
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

    // CosyVoice3 uses baked-in voice presets shipped in cosyvoice3-voices.gguf; there's no
    // user-supplied reference WAV to import. Stay false so the UI hides the "Import voice"
    // affordance for this engine.
    public bool ImportVoice(string fileName) => false;
}
