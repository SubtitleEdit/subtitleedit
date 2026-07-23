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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Zonos-v0.1 (Zyphra) run through the CrispASR runtime. 26-layer GQA autoregressive
/// transformer (2B) emitting 9 DAC codebooks @ 44.1 kHz, CFG-guided, with voice cloning
/// from a reference WAV. Apache-2.0.
///
/// Two GGUFs are needed (same talker + companion split as the other CrispASR TTS engines):
///  - zonos-v0.1-transformer-q8_0.gguf : the AR transformer (the actual TTS model)
///  - dac-44khz-f16.gguf               : the Descript DAC 44 kHz codec (vocoder)
///
/// Voice cloning only — no built-in voice bank. Zonos transcribes the reference internally
/// (eSpeak phonemisation), so unlike Qwen3 CustomVoice there is no .txt ref-text sidecar.
/// This is a minimal engine: a single Q8_0 quant, no model dropdown, no per-engine settings
/// dialog. Mirrors <see cref="IndexTtsCrispAsr"/>.
/// </summary>
public class ZonosTtsCrispAsr : ITtsEngine
{
    public string Name => "Zonos TTS (CrispASR)";
    public string Description => "Zyphra Zonos-v0.1 with voice cloning, via CrispASR";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;

    public const string TalkerFileName = "zonos-v0.1-transformer-q8_0.gguf";
    public const string CodecFileName = "dac-44khz-f16.gguf";

    public const string BackendName = "zonos-tts";

    // Exact byte sizes on cstr's HuggingFace repos (X-Linked-Size). Used to reject truncated
    // files that crispasr's --auto-download may have left behind — same trap that bit Qwen3
    // and IndexTTS (Windows access violation 0xC0000005 deep in the legacy GGUF loader).
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [TalkerFileName] = 1726190464L,
        [CodecFileName] = 108695872L,
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
    // Tracks the --voice path the running server was started with. CrispASR's TTS server
    // backends ignore the request body's `voice` field in server mode and only load the
    // reference audio from the startup --voice flag, so a voice change requires us to tear
    // down and restart the server. Same as IndexTTS — see PR #11210 discussion.
    private static string? _serverVoicePath;
    private static bool _processExitHooked;
    private static readonly StringBuilder _serverLog = new();

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetCrispAsrExecutable()));
    }

    public override string ToString() => Name;

    /// <summary>
    /// Path to the crispasr executable installed by the speech-to-text feature. Shared with
    /// Chatterbox / Qwen3 TTS / IndexTTS (CrispASR) / all CrispASR ASR engines.
    /// </summary>
    public static string GetCrispAsrExecutable()
    {
        return new CrispAsrCohere().GetExecutable();
    }

    /// <summary>
    /// Mirrors <see cref="IndexTtsCrispAsr.GetEngineUpdateStatus"/> — reads the speech-to-text
    /// CrispASR install's <c>.installed.sha256</c> sidecar.
    /// </summary>
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "ZonosTtsCrispAsr");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetModelsFolder()
    {
        // Like the other CrispASR-backed engines, the GGUFs live alongside CrispASR's
        // speech-to-text models in CrispASR/models/ rather than under TextToSpeech/.
        var modelsFolder = Path.Combine(Se.CrispAsrFolder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public static string GetSetVoicesFolder()
    {
        var voicesFolder = Path.Combine(GetSetFolder(), "voices");
        if (!Directory.Exists(voicesFolder))
        {
            Directory.CreateDirectory(voicesFolder);
        }

        SeedVoicesFromQwen3TtsCppIfEmpty(voicesFolder);
        return voicesFolder;
    }

    private static bool _voiceSeedAttempted;

    /// <summary>
    /// One-time best-effort seed of WAV reference voices from qwen3-tts.cpp's voices folder.
    /// The qwen3-tts.cpp voice pack ships at 16 kHz mono; resample to 24 kHz mono on seed via
    /// ffmpeg (same path ImportVoice uses) so crispasr doesn't upsample lossily on every synth.
    /// </summary>
    private static void SeedVoicesFromQwen3TtsCppIfEmpty(string voicesFolder)
    {
        if (_voiceSeedAttempted)
        {
            return;
        }
        _voiceSeedAttempted = true;

        try
        {
            if (Directory.EnumerateFiles(voicesFolder, "*.wav").Any())
            {
                return;
            }

            var sourceFolder = Qwen3TtsCpp.GetSetVoicesFolder();
            if (!Directory.Exists(sourceFolder) || !Directory.EnumerateFiles(sourceFolder, "*.wav").Any())
            {
                return;
            }

            foreach (var src in Directory.GetFiles(sourceFolder, "*.wav"))
            {
                var dest = Path.Combine(voicesFolder, Path.GetFileName(src));
                if (File.Exists(dest))
                {
                    continue;
                }

                try
                {
                    var ffmpeg = FfmpegGenerator.ConvertToMono24kHzWav(src, dest);
                    if (!ffmpeg.Start())
                    {
                        // ffmpeg unavailable — better to seed at 16 kHz than skip the voice.
                        File.Copy(src, dest);
                        continue;
                    }
                    ffmpeg.WaitForExit();
                }
                catch (Exception ex)
                {
                    Se.LogError(ex, $"Zonos TTS (CrispASR): resample seed '{src}' failed; falling back to plain copy");
                    try { if (!File.Exists(dest))
                    {
                        File.Copy(src, dest);
                    } } catch { }
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Zonos TTS (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    public static string GetTalkerPath() =>
        Path.Combine(GetSetModelsFolder(), TalkerFileName);

    public static string GetCodecPath() =>
        Path.Combine(GetSetModelsFolder(), CodecFileName);

    public static bool AreModelsInstalled() =>
        IsValidLocalModelFile(GetTalkerPath(), TalkerFileName)
        && IsValidLocalModelFile(GetCodecPath(), CodecFileName);

    /// <summary>
    /// Path crispasr's --auto-download writes GGUFs to. Mirrors the IndexTTS (CrispASR) helper so
    /// the SE-side downloader can adopt already-cached files instead of re-pulling the models.
    /// </summary>
    public static string GetCrispAsrCacheFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "crispasr");

    /// <summary>
    /// Best-effort copy of <paramref name="fileName"/> from <see cref="GetCrispAsrCacheFolder"/>
    /// into SE's models folder. See <see cref="IndexTtsCrispAsr.TrySeedModelFromCrispAsrCache"/>
    /// for rationale — same truncation-guard semantics.
    /// </summary>
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
                Se.LogError($"Zonos TTS (CrispASR): removing wrong-sized local model file {destinationPath}");
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
            Se.LogError(ex, $"Zonos TTS (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>();

        // Voice cloning only — no built-in default voice. The combo is empty until the user
        // imports a reference WAV (or the qwen3-tts.cpp voice seed runs above).
        var voicesFolder = GetSetVoicesFolder();
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new ZonosTtsVoice(name, file)));
            }
        }

        return Task.FromResult(result.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(Array.Empty<string>());

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
        if (voice.EngineVoice is not ZonosTtsVoice zonosVoice)
        {
            throw new ArgumentException("Voice is not a ZonosTtsVoice");
        }

        if (string.IsNullOrEmpty(zonosVoice.FilePath))
        {
            throw new InvalidOperationException(
                "Zonos TTS (CrispASR) requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "Reference WAV should be 24 kHz mono (a few seconds of clean speech).");
        }

        await EnsureServerRunningAsync(zonosVoice.FilePath, cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");
        var inputText = text;

        // OpenAI-compatible /v1/audio/speech payload. Zonos transcribes the reference internally
        // (eSpeak), so there is no `ref-text` parameter.
        // Deliberately NO `voice` field: the server rejects absolute paths outright (HTTP 400,
        // "'voice' must not contain … path separators" — path-traversal guard), so sending
        // zonosVoice.FilePath failed every synthesis. The zonos backend loads the reference at
        // init from the startup --voice flag, and the server restarts on voice change — see
        // EnsureServerRunningAsync. Same bug family as MOSS-TTS #12757.
        var payload = new Dictionary<string, object>
        {
            ["input"] = inputText,
            ["response_format"] = "wav",
            // Attests the user's own imported reference; the server logs it for cloned synthesis.
            ["consent_attestation"] = "I have the speaker's consent, or it is my own voice.",
            // Skip the audible AI-disclosure prefix CrispASR otherwise prepends to cloned audio;
            // SE surfaces the AI-generated nature in its UI. The inaudible watermark + C2PA
            // provenance metadata stay embedded regardless (defaults to true server-side).
            ["spoken_disclaimer"] = false,
        };

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Zonos TTS (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={zonosVoice}, textLen={text.Length})");

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

            var failMsg = $"Zonos TTS (CrispASR) request failed — Voice: {zonosVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Zonos TTS (CrispASR) — the crispasr server crashed during synthesis."
                    : "Zonos TTS (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"Zonos TTS (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {zonosVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Zonos TTS (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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

    private static async Task EnsureServerRunningAsync(string voicePath, CancellationToken ct)
    {
        // CrispASR's TTS server backends don't honour the per-request `voice` field — they
        // only read the reference audio from the --voice path passed at server startup. So we
        // restart the server every time the selected voice changes. Tracked next to
        // _serverProcess so an already-running server with the matching voice is reused.
        if (_serverProcess is { HasExited: false } && _serverPort != 0
            && string.Equals(_serverVoicePath, voicePath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0
                && string.Equals(_serverVoicePath, voicePath, StringComparison.OrdinalIgnoreCase))
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

            // Talker + codec GGUFs: use locally staged copies when present; otherwise fall back
            // to crispasr's own --auto-download (fetches into ~/.cache/crispasr/ on first run).
            var talker = GetTalkerPath();
            var hasLocalTalker = IsValidLocalModelFile(talker, TalkerFileName);
            var codec = GetCodecPath();
            var hasLocalCodec = IsValidLocalModelFile(codec, CodecFileName);

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
            psi.ArgumentList.Add(hasLocalTalker ? talker : "auto");
            if (hasLocalCodec)
            {
                psi.ArgumentList.Add("--codec-model");
                psi.ArgumentList.Add(codec);
            }
            if (!hasLocalTalker || !hasLocalCodec)
            {
                psi.ArgumentList.Add("--auto-download");
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());
            // /v1/audio/speech gates the `voice` field on --voice-dir being set. Same as IndexTTS.
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());
            // Server-mode voice cloning only respects the startup --voice path; skipping this
            // means the request's `voice` is ignored and the synth produces the wrong speaker.
            psi.ArgumentList.Add("--voice");
            psi.ArgumentList.Add(voicePath);

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (zonos-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("Zonos TTS (CrispASR) server starting — "
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
            _serverVoicePath = voicePath;
            HookProcessExitOnce();

            // First-run auto-download (~1.8 GB total) needs a generous timeout.
            var deadline = DateTime.UtcNow.AddMinutes(hasLocalTalker && hasLocalCodec ? 5 : 30);
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
                    _serverVoicePath = null;
                    throw new InvalidOperationException(
                        $"crispasr (zonos-tts) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (zonos-tts) did not report healthy within {(hasLocalTalker && hasLocalCodec ? 5 : 30)} minutes. Last output: {lastOutput}"
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
        if (_processExitHooked)
        {
            return;
        }
        _processExitHooked = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopServerInternal();
    }

    /// <summary>
    /// Stop the running crispasr (zonos-tts) server if any, releasing GPU memory. Called by
    /// <c>TextToSpeechViewModel</c> when starting synthesis on a different engine or when the
    /// TTS window closes, so the CrispASR-based TTS engines don't pile up in VRAM.
    /// </summary>
    public static void StopServer() => StopServerInternal();

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverLaunchCommand = null;
        _serverVoicePath = null;
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

        // Zonos clones from a reference WAV and transcribes it internally. Resample on import
        // via ffmpeg to 24 kHz mono regardless of source format. No .txt sidecar needed.
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
            Se.LogError(ex, "Zonos TTS (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        return File.Exists(destinationFileName);
    }
}
