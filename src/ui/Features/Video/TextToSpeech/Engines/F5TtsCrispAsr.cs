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
/// F5-TTS v1 base (E2 TTS / SWivT family) run through the CrispASR runtime. DiT flow-matching
/// transformer + Vocos vocoder, MIT-licensed, strong zero-shot voice cloning. Single ~953 MB
/// F16 GGUF (cstr/f5-tts-GGUF on Hugging Face) — no separate codec file. 24 kHz mono reference
/// WAV; cloning quality is best with 3-10 s of clean speech and an accurate transcription.
///
/// CLI usage from the upstream README:
///   crispasr --backend f5-tts -m auto \
///       --voice reference.wav \
///       --ref-text "Transcript of the reference audio" \
///       --tts "Hello, how are you today?" --tts-output out.wav
///
/// Server-mode flag layout is unconfirmed in CrispASR 0.6.12's README. This implementation
/// assumes the same per-request payload shape Qwen3 CustomVoice uses (voice = WAV path,
/// ref_text = transcription) and falls back to a startup --voice / --ref-text if needed
/// (toggle <see cref="UseStartupVoiceFlags"/> below). Verify against an actual 0.6.12 binary.
/// </summary>
public class F5TtsCrispAsr : ITtsEngine
{
    public string Name => "F5-TTS (CrispASR)";
    public string Description => "F5-TTS v1 with zero-shot voice cloning, via CrispASR";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    // Only one quant on the HF repo at time of integration. ModelKey is still exposed so the
    // engine settings dialog can grow new quants without breaking the saved-settings string.
    public const string ModelKeyF16 = "F16 (~953 MB)";
    public const string DefaultModelKey = ModelKeyF16;

    public const string TalkerF16FileName = "f5-tts-v1-base-f16.gguf";

    public const string BackendName = "f5-tts";

    // Confirmed against CrispASR 0.6.12: the f5-tts backend ignores the per-request `voice`
    // field and only reads the reference audio from the startup --voice flag (the server log
    // shows `ref_T=0 duration=0` and an "empty audio" 500 otherwise). Same upstream behaviour
    // as IndexTTS 0.6.11 — server gets torn down and restarted when the voice or ref-text
    // changes, keyed by (model, voice, ref-text) below.
    private static readonly bool UseStartupVoiceFlags = true;

    // Exact byte size from the HF tree API (cstr/f5-tts-GGUF). Same truncation guard as the
    // other CrispASR TTS engines — a partial file would crash the loader on startup.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [TalkerF16FileName] = 999097152L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.F5TtsCrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return ModelKeyF16;
    }

    public static string GetTalkerFileName(string? modelKey) => TalkerF16FileName;

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
    // Only used when UseStartupVoiceFlags is true. Matches the IndexTTS layout: changing the
    // selected reference WAV means tear-down + restart so the new --voice path takes effect.
    private static string? _serverVoicePath;
    private static string? _serverRefText;
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "F5TtsCrispAsr");
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
    /// F5-TTS requires a transcription (ref-text) alongside the reference WAV, same as
    /// Qwen3 CustomVoice — so we copy both the .wav and the .txt sidecar.
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

                if (!File.Exists(dest))
                {
                    try
                    {
                        // qwen3-tts.cpp voice pack ships at 16 kHz; F5-TTS expects 24 kHz.
                        // Resample on seed via ffmpeg so cloning sees 24 kHz directly.
                        var ffmpeg = FfmpegGenerator.ConvertToMono24kHzWav(src, dest);
                        if (!ffmpeg.Start())
                        {
                            File.Copy(src, dest);
                        }
                        else
                        {
                            ffmpeg.WaitForExit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Se.LogError(ex, $"F5-TTS (CrispASR): resample seed '{src}' failed; falling back to plain copy");
                        try { if (!File.Exists(dest)) File.Copy(src, dest); } catch { }
                    }
                }

                var sidecar = Path.ChangeExtension(src, ".txt");
                if (File.Exists(sidecar))
                {
                    var sidecarDest = Path.ChangeExtension(dest, ".txt");
                    if (!File.Exists(sidecarDest))
                    {
                        try { File.Copy(sidecar, sidecarDest); } catch { }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "F5-TTS (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    public static string GetTalkerPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetTalkerFileName(modelKey));

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
                Se.LogError($"F5-TTS (CrispASR): removing wrong-sized local model file {destinationPath}");
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
            Se.LogError(ex, $"F5-TTS (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetTalkerPath(modelKey), GetTalkerFileName(modelKey));

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
                result.Add(new Voice(new F5TtsVoice(name, file)));
            }
        }

        return Task.FromResult(result.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyF16 });

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
        if (voice.EngineVoice is not F5TtsVoice f5Voice)
        {
            throw new ArgumentException("Voice is not an F5TtsVoice");
        }

        if (string.IsNullOrEmpty(f5Voice.FilePath))
        {
            throw new InvalidOperationException(
                "F5-TTS (CrispASR) requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "Reference WAV should be 24 kHz mono (3-10 s of clean speech) with an "
                + "adjacent .txt file holding the spoken transcription.");
        }

        var refText = TryReadRefText(f5Voice.FilePath);
        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, f5Voice.FilePath, refText, cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");

        var speed = Math.Clamp(Se.Settings.Video.TextToSpeech.F5TtsCrispAsrSpeed, 0.25, 4.0);
        var payload = new Dictionary<string, object>
        {
            ["input"] = text,
            ["response_format"] = "wav",
            ["voice"] = f5Voice.FilePath,
            ["speed"] = speed,
        };
        if (!string.IsNullOrEmpty(refText))
        {
            // TODO: verify field name. CrispASR's CLI uses `--ref-text`; the OpenAI-compat
            // server may expose this as `ref_text`, `ref-text`, or `instructions`. Adjust
            // once a real synth round-trip confirms which key the backend reads.
            payload["ref_text"] = refText;
        }

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"F5-TTS (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={f5Voice}, refTextLen={refText.Length}, textLen={text.Length})");

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

            var failMsg = $"F5-TTS (CrispASR) request failed — Voice: {f5Voice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "F5-TTS (CrispASR) — the crispasr server crashed during synthesis."
                    : "F5-TTS (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"F5-TTS (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {f5Voice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"F5-TTS (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LaunchCmdSuffix(launchCommand));
            }

            await using var fileStream = File.Create(outputFileName);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
    }

    private static string TryReadRefText(string wavPath)
    {
        try
        {
            var sidecar = Path.ChangeExtension(wavPath, ".txt");
            return File.Exists(sidecar) ? File.ReadAllText(sidecar).Trim() : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
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

    private static async Task EnsureServerRunningAsync(string modelKey, string voicePath, string refText, CancellationToken ct)
    {
        // When UseStartupVoiceFlags is true the server is keyed by (model, voice, ref-text) since
        // those are baked in at process start. When false the server is keyed only by model and
        // voice/ref-text are sent per-request, so the cache key collapses to (model).
        bool MatchesCurrent() =>
            _serverProcess is { HasExited: false } && _serverPort != 0
            && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
            && (!UseStartupVoiceFlags
                || (string.Equals(_serverVoicePath, voicePath, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(_serverRefText, refText, StringComparison.Ordinal)));

        if (MatchesCurrent())
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (MatchesCurrent())
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

            var talkerFileName = GetTalkerFileName(modelKey);
            var talker = GetTalkerPath(modelKey);
            var hasLocalTalker = IsValidLocalModelFile(talker, talkerFileName);

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
            if (!hasLocalTalker)
            {
                psi.ArgumentList.Add("--auto-download");
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());

            if (UseStartupVoiceFlags)
            {
                psi.ArgumentList.Add("--voice");
                psi.ArgumentList.Add(voicePath);
                if (!string.IsNullOrEmpty(refText))
                {
                    psi.ArgumentList.Add("--ref-text");
                    psi.ArgumentList.Add(refText);
                }
            }

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (f5-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("F5-TTS (CrispASR) server starting — "
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
            _serverVoicePath = voicePath;
            _serverRefText = refText;
            HookProcessExitOnce();

            // First-run auto-download (~953 MB talker) needs a generous timeout.
            var deadline = DateTime.UtcNow.AddMinutes(hasLocalTalker ? 5 : 20);
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
                    _serverVoicePath = null;
                    _serverRefText = null;
                    throw new InvalidOperationException(
                        $"crispasr (f5-tts) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (f5-tts) did not report healthy within {(hasLocalTalker ? 5 : 20)} minutes. Last output: {lastOutput}"
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
        _serverVoicePath = null;
        _serverRefText = null;
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

    public bool ImportVoice(string fileName) => ImportVoice(fileName, string.Empty);

    /// <summary>
    /// Imports <paramref name="fileName"/> as an F5-TTS reference voice. When
    /// <paramref name="transcript"/> is non-empty it's written to the destination .txt sidecar
    /// directly (improves cloning quality significantly). When empty, falls back to copying
    /// any .txt sidecar that already lives next to the source WAV.
    /// </summary>
    public bool ImportVoice(string fileName, string transcript)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        var voicesFolder = GetSetVoicesFolder();
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var destinationFileName = GetUniqueDestinationFileName(voicesFolder, baseName);

        // F5-TTS expects 24 kHz mono. Resample on import via ffmpeg regardless of source format.
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
            Se.LogError(ex, "F5-TTS (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        if (!File.Exists(destinationFileName))
        {
            return false;
        }

        // Voice cloning quality depends on an accurate transcription. Caller-supplied
        // transcript wins; otherwise fall back to a sibling .txt next to the source WAV.
        try
        {
            var destSidecar = Path.ChangeExtension(destinationFileName, ".txt");
            if (!string.IsNullOrWhiteSpace(transcript))
            {
                File.WriteAllText(destSidecar, transcript.Trim());
            }
            else
            {
                var sourceSidecar = Path.ChangeExtension(fileName, ".txt");
                if (File.Exists(sourceSidecar) && !File.Exists(destSidecar))
                {
                    File.Copy(sourceSidecar, destSidecar);
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "F5-TTS (CrispASR) voice import: failed to write .txt sidecar");
        }

        return true;
    }

    /// <summary>
    /// Writes <paramref name="transcript"/> to <c>&lt;voice&gt;.txt</c> next to the supplied
    /// voice WAV inside the voices folder. Used to retro-fit a transcription onto a voice
    /// that was already imported (or seeded from the qwen3-tts.cpp pack) without going through
    /// a full re-import.
    /// </summary>
    public static bool TryWriteRefTextSidecar(string voiceWavPath, string transcript)
    {
        if (string.IsNullOrEmpty(voiceWavPath) || string.IsNullOrWhiteSpace(transcript))
        {
            return false;
        }

        try
        {
            var sidecar = Path.ChangeExtension(voiceWavPath, ".txt");
            File.WriteAllText(sidecar, transcript.Trim());
            return true;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"F5-TTS (CrispASR): failed to write ref-text sidecar for '{voiceWavPath}'");
            return false;
        }
    }
}
