using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Chatterbox TTS via the existing CrispASR install (shared with the speech-to-text feature).
/// Spawns `crispasr --server --backend chatterbox -m &lt;t3&gt; --codec-model &lt;s3gen&gt;` and POSTs
/// to the OpenAI-compatible /v1/audio/speech endpoint. Requires CrispASR v0.6.0 or newer.
/// The T3 + S3Gen GGUFs are downloaded explicitly into TextToSpeech/Chatterbox/models/ —
/// `-m auto` is avoided because its codec auto-discovery only finds *-s3gen-f16.gguf
/// while it actually downloads the q8_0 variants.
/// Chatterbox has one baked default voice; "voices" listed beyond Default come from
/// WAVs imported via <see cref="ImportVoice"/>. The full reference-WAV path is sent per-request
/// as the `voice` field — runtime WAV cloning is wired upstream in CrispASR's chatterbox backend.
/// </summary>
public class ChatterboxTtsCpp : ITtsEngine
{
    public string Name => "Chatterbox TTS";
    public string Description => "via CrispASR (Base or Turbo model + voice cloning)";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    public const string ModelKeyBase = ChatterboxTtsCppDownloadService.ModelKeyBase;
    public const string ModelKeyTurbo = ChatterboxTtsCppDownloadService.ModelKeyTurbo;
    public const string DefaultModelKey = ChatterboxTtsCppDownloadService.DefaultModelKey;

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.ChatterboxModel;
            return ChatterboxTtsCppDownloadService.ResolveModelKey(string.IsNullOrEmpty(saved) ? DefaultModelKey : saved);
        }
        return ChatterboxTtsCppDownloadService.ResolveModelKey(modelKey);
    }

    // Resolved per-model: Base → chatterbox, Turbo → chatterbox-turbo. See
    // ChatterboxTtsCppDownloadService.GetBackendName for why this matters.
    private static string GetBackendName(string? modelKey) =>
        ChatterboxTtsCppDownloadService.GetBackendName(modelKey);

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5),
    };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverModelKey;
    private static bool _processExitHooked;
    // Rolling buffer of the server's stdout+stderr — used to attach context to
    // /v1/audio/speech failures (the response body alone says "synthesis failed"
    // without the actual reason; the backend prints the reason to stderr).
    private static readonly StringBuilder _serverLog = new();

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetCrispAsrExecutable()));
    }

    public override string ToString() => Name;

    /// <summary>
    /// Path to the crispasr executable installed by the speech-to-text feature.
    /// Chatterbox piggy-backs on the same install so users don't download two copies.
    /// </summary>
    public static string GetCrispAsrExecutable()
    {
        return new CrispAsrCohere().GetExecutable();
    }

    /// <summary>
    /// Returns true when the installed crispasr executable matches a known
    /// chatterbox-capable release (currently v0.6.0+ — earlier builds neither
    /// recognise --backend chatterbox nor expose the /v1/audio/speech endpoint).
    /// Returns true when the hash is unknown so we don't false-positive on
    /// custom local builds.
    /// </summary>
    public static bool IsCrispAsrChatterboxCapable()
    {
        var exe = GetCrispAsrExecutable();
        if (!File.Exists(exe))
        {
            return false;
        }

        var folder = Path.GetDirectoryName(exe);
        var variant = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && folder != null
            ? DownloadHashManager.DetectCrispAsrWindowsVariant(folder)
            : null;
        var key = DownloadHashManager.ResolveCrispAsrExecutableKey(variant);
        if (key == null)
        {
            return true;
        }

        var hash = DownloadHashManager.ComputeSha256(exe);
        if (hash == null)
        {
            return true;
        }

        // UpdateAvailable means the installed hash is a known *older* release —
        // demote those to "not chatterbox-capable" so the user is prompted to
        // re-download. UpToDate and Unknown both pass through.
        return DownloadHashManager.GetStatus(key, hash) != DownloadHashManager.UpdateStatus.UpdateAvailable;
    }

    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "Chatterbox");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetVoicesFolder()
    {
        var voicesFolder = Path.Combine(GetSetFolder(), "voices");
        if (!Directory.Exists(voicesFolder))
        {
            Directory.CreateDirectory(voicesFolder);
        }

        return voicesFolder;
    }

    public static string GetSetModelsFolder()
    {
        var modelsFolder = Path.Combine(GetSetFolder(), "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public static string GetT3ModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), ChatterboxTtsCppDownloadService.GetT3FileName(ResolveModelKey(modelKey)));

    public static string GetS3GenModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), ChatterboxTtsCppDownloadService.GetS3GenFileName(ResolveModelKey(modelKey)));

    public static bool AreModelsInstalled(string? modelKey = null) =>
        File.Exists(GetT3ModelPath(modelKey)) && File.Exists(GetS3GenModelPath(modelKey));

    public Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>
        {
            new Voice(new ChatterboxVoice("Default", string.Empty)),
        };

        var voicesFolder = GetSetVoicesFolder();
        foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
        {
            var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
            result.Add(new Voice(new ChatterboxVoice(name, file)));
        }

        return Task.FromResult(result.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyBase, ModelKeyTurbo });

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken)
    {
        return GetVoices(language);
    }

    public async Task<TtsResult> Speak(
        string text,
        string outputFolder,
        Voice voice,
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken)
    {
        if (voice.EngineVoice is not ChatterboxVoice chatterboxVoice)
        {
            throw new ArgumentException("Voice is not a ChatterboxVoice");
        }

        await EnsureServerRunningAsync(ResolveModelKey(model), cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");
        var inputText = Utilities.UnbreakLine(text);

        // Per /v1/audio/speech: send full reference WAV path as the `voice` field.
        // Empty string falls back to the model's baked default voice.
        var payload = new Dictionary<string, object>
        {
            ["input"] = inputText,
            ["response_format"] = "wav",
        };
        if (!string.IsNullOrEmpty(chatterboxVoice.FilePath))
        {
            payload["voice"] = chatterboxVoice.FilePath;
        }

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Chatterbox TTS: POST {ServerBaseUrl}/v1/audio/speech (voice={chatterboxVoice}, model={ResolveModelKey(model)}, textLen={text.Length})");
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PostAsync($"{ServerBaseUrl}/v1/audio/speech", content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            // Connection dropped before a response — typically the server crashed
            // during synth (ggml fault, OOM, etc.). Attach what the server printed
            // so the user/we can see the underlying reason.
            var serverLog = SnapshotServerLog();
            var died = _serverProcess?.HasExited == true;
            if (died)
            {
                StopServerInternal();
            }
            var failMsg = $"Chatterbox TTS request failed - Voice: {chatterboxVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}";
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            var prefix = LooksLikeUpstreamChatterboxCrash(serverLog)
                ? "Chatterbox TTS hit a CrispASR runtime bug during synthesis (ggml tensor read out of bounds). "
                  + "This is an upstream issue — please file it at https://github.com/CrispStrobe/CrispASR/issues with the server log below. "
                  + "The crash reproduces on the CPU and Vulkan builds (chatterbox's T3 step runs on CPU regardless of the build); "
                  + "the CUDA build may avoid it but is unverified."
                : "Chatterbox TTS request failed — "
                  + (died ? "the crispasr server crashed during synthesis." : "the connection to the crispasr server was dropped.");

            throw new InvalidOperationException(
                prefix + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}"),
                ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await SafeReadErrorAsync(response, cancellationToken);
                var serverLog = SnapshotServerLog();
                var errMsg = $"Chatterbox TTS server error {(int)response.StatusCode} {response.StatusCode} - "
                    + $"Voice: {chatterboxVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}";
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Chatterbox TTS synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}"));
            }

            await using var fileStream = File.Create(outputFileName);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
    }

    /// <summary>
    /// Renders the server launch as a shell-quotable string (file path + each arg quoted only
    /// when it contains whitespace). Goes into the tools log so failures can be reproduced.
    /// </summary>
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

    private static async Task EnsureServerRunningAsync(string modelKey, CancellationToken ct)
    {
        if (_serverProcess is { HasExited: false } && _serverPort != 0 && _serverModelKey == modelKey)
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0 && _serverModelKey == modelKey)
            {
                return;
            }

            // Server not running — or running with a different model variant. (Re)start.
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
            psi.ArgumentList.Add(GetBackendName(modelKey));
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(GetT3ModelPath(modelKey));
            // Pass S3Gen explicitly. The chatterbox backend's auto-discovery only finds
            // *-s3gen-f16.gguf, so without this flag the q8_0 codec we ship is ignored
            // and synth returns empty audio.
            psi.ArgumentList.Add("--codec-model");
            psi.ArgumentList.Add(GetS3GenModelPath(modelKey));
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());
            // The crispasr server gates /v1/audio/speech requests with a `voice` field
            // behind --voice-dir being set ("warning: --voice-dir not set; … will reject
            // requests with a 'voice' field"). Imported reference-WAV cloning sends an
            // absolute path as `voice`, which the chatterbox backend resolves directly,
            // but the server-level gate still applies. Pointing --voice-dir at our
            // voices folder satisfies the gate and also makes /v1/voices reflect the
            // imported WAVs.
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (chatterbox)");

            // Record the exact launch command in the tools log so failures later in this
            // session can be reproduced from a shell.
            Se.WriteToolsLog("Chatterbox TTS server starting - "
                + $"PID: {process.Id}, "
                + $"Cmd: {FormatLaunchCommand(exe, psi.ArgumentList)}");

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
            HookProcessExitOnce();

            // First-run model auto-download (~880 MB) needs a generous timeout.
            var deadline = DateTime.UtcNow.AddMinutes(15);
            while (DateTime.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    var tail = SnapshotServerLog();
                    _serverProcess = null;
                    _serverPort = 0;
                    _serverModelKey = null;
                    if (LooksLikeOutdatedCrispAsr(tail))
                    {
                        throw new InvalidOperationException(
                            "Chatterbox requires CrispASR v0.6.0 or newer. Re-download CrispASR via "
                            + "Video → Audio to text → Engine settings → Re-download, then try again.");
                    }
                    if (LooksLikeStaleModelCache(tail))
                    {
                        throw new InvalidOperationException(
                            "Chatterbox failed to load its model — the GGUFs in "
                            + GetSetModelsFolder() + " are likely stale or partially downloaded. "
                            + "Delete them and try again so they re-download. Original output: " + tail);
                    }
                    throw new InvalidOperationException(
                        $"crispasr (chatterbox) exited during startup (code {process.ExitCode}). Output: {tail}");
                }
                if (await ProbeHealthAsync(port, TimeSpan.FromSeconds(2), ct))
                {
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }

            var lastOutput = SnapshotServerLog();
            StopServerInternal();
            throw new TimeoutException(
                $"crispasr (chatterbox) did not report healthy within 15 minutes. Last output: {lastOutput}");
        }
        finally
        {
            ServerLock.Release();
        }
    }

    private static bool LooksLikeOutdatedCrispAsr(string output)
    {
        // v0.5.x exits 0 and prints `error: unknown argument: ...` when it doesn't
        // recognise --voice-dir / --backend chatterbox. v0.6.x without the chatterbox
        // backend (e.g. ASR-only build) prints `unknown backend 'chatterbox'`.
        return output.Contains("unknown argument", StringComparison.Ordinal)
            || output.Contains("unknown backend", StringComparison.Ordinal);
    }

    private static bool LooksLikeUpstreamChatterboxCrash(string output)
    {
        // Known CrispASR v0.6.0 chatterbox synth crash on the CPU build:
        //   ggml-backend.cpp:349: GGML_ASSERT(offset + size <= ggml_nbytes(tensor)
        //                         && "tensor read out of bounds") failed
        // Hits during the first AR step after KV-cache allocation. Upstream fix
        // pending. Distinct from the static-init duplicate ggml assert at
        // ggml.cpp:22 (caught by LooksLikeStaleModelCache via a different match).
        return output.Contains("tensor read out of bounds", StringComparison.Ordinal);
    }

    private static bool LooksLikeStaleModelCache(string output)
    {
        // A truncated/format-mismatched GGUF surfaces as either a clean "tensor not
        // found" / "failed to bind" message or — when the format mismatch trips a C++
        // exception during ggml init — the `GGML_ASSERT(prev != ggml_uncaught_exception)`
        // abort with a Windows STATUS_STACK_BUFFER_OVERRUN exit code
        // (-1073740791 / 0xC0000409).
        return output.Contains("required tensor", StringComparison.Ordinal)
            || output.Contains("failed to bind", StringComparison.Ordinal)
            || output.Contains("ggml_uncaught_exception", StringComparison.Ordinal);
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
        _serverModelKey = null;
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

    private static string GetUniqueDestinationFileName(string folder, string baseName)
    {
        var candidate = Path.Combine(folder, baseName + ".wav");
        if (!File.Exists(candidate))
        {
            return candidate;
        }

        var number = 1;
        do
        {
            candidate = Path.Combine(folder, $"{baseName}_{number}.wav");
            number++;
        } while (File.Exists(candidate));

        return candidate;
    }

    public bool ImportVoice(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        var voicesFolder = GetSetVoicesFolder();
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var destinationFileName = GetUniqueDestinationFileName(voicesFolder, baseName);

        // CrispASR's chatterbox backend only does "atomic" voice cloning when the
        // reference WAV is 24 kHz mono PCM16/F32 — anything else silently falls back
        // to the default voice. Always resample on import via ffmpeg so the saved
        // WAV is in the right shape regardless of what the user picked.
        try
        {
            var process = FfmpegGenerator.ConvertToMono24kHzWav(fileName, destinationFileName);
            if (!process.Start())
            {
                return false;
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Chatterbox TTS voice import failed (ffmpeg conversion).");
            return false;
        }

        return File.Exists(destinationFileName);
    }
}
