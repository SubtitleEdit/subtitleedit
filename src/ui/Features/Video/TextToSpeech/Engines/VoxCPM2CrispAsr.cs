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
/// VoxCPM2 (MiniCPM-family, Dec 2025) run through the CrispASR runtime. Tokenizer-free CFM
/// diffusion AR pipeline (TSLM + RALM + LocDiT) at 48 kHz native, ~30 languages, Apache-2.0,
/// with zero-shot voice cloning from a user-supplied reference WAV. CrispASR ships it as the
/// <c>voxcpm2-tts</c> backend (cstr/voxcpm2-GGUF on Hugging Face).
///
/// The repo ships two quants of the main model plus a small shared companion:
///   - Main model : voxcpm2-{q4_k,f16}.gguf   (we stage the one the user picks)
///   - Companion  : voxcpm2-ref.gguf          (~430 KB; shared by both quants)
/// crispasr discovers <c>voxcpm2-ref.gguf</c> as a sibling of the main model at server startup,
/// so both files must sit together in <see cref="GetSetModelsFolder"/>. <see
/// cref="Nikse.SubtitleEdit.Logic.Download.VoxCPM2CrispAsrDownloadService"/> stages both with
/// size + SHA-256 verification before first synth; <c>-m auto --auto-download</c> is the slower
/// fallback used only when a required file is missing at startup.
///
/// CLI usage (upstream README):
///   crispasr --backend voxcpm2-tts -m auto --auto-download \
///       --voice reference.wav \
///       --tts "Hello, how are you today?" --tts-output out.wav
///
/// Cloning is zero-shot from audio alone; an adjacent .txt transcription (ref-text) is optional
/// but improves quality, so we pass <c>--ref-text</c> when a sidecar is present (same layout as
/// F5-TTS / Qwen3 CustomVoice). Server-mode payload shape mirrors the other CrispASR TTS engines
/// and should be verified against an actual v0.7.0 binary.
/// </summary>
public class VoxCPM2CrispAsr : ITtsEngine
{
    public string Name => "VoxCPM2 (CrispASR)";
    public string Description => "VoxCPM2 48 kHz multilingual TTS with zero-shot voice cloning, via CrispASR";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    // Two quants — Q4_K is the lightweight default (~1.7 GB), F16 the reference (~5 GB). The
    // label total includes the tiny shared voxcpm2-ref.gguf companion.
    public const string ModelKeyQ4K = "Q4_K (~1.7 GB)";
    public const string ModelKeyF16 = "F16 (~5 GB)";
    public const string DefaultModelKey = ModelKeyQ4K;

    public const string ModelQ4KFileName = "voxcpm2-q4_k.gguf";
    public const string ModelF16FileName = "voxcpm2-f16.gguf";
    public const string RefFileName = "voxcpm2-ref.gguf";

    public const string BackendName = "voxcpm2-tts";

    // Confirmed-by-analogy with F5-TTS/IndexTTS: the cloning backends read the reference audio
    // from the startup --voice flag, so the server is torn down and restarted when the selected
    // voice or ref-text changes (keyed by (model, voice, ref-text) below). Verify against a real
    // v0.7.0 binary; if voxcpm2-tts honours a per-request voice field this can be set to false.
    private static readonly bool UseStartupVoiceFlags = true;

    /// <summary>
    /// All filenames that must be present in <see cref="GetSetModelsFolder"/> for the chosen
    /// quant: the picked main model plus the shared voxcpm2-ref.gguf companion. Listed main-first
    /// so the download dialog reports the big file before the small one.
    /// </summary>
    public static string[] GetRequiredFileNames(string? modelKey) => new[]
    {
        GetModelFileName(modelKey),
        RefFileName,
    };

    // Exact byte sizes from the HF tree API (cstr/voxcpm2-GGUF). Same truncation guard as the
    // other CrispASR TTS engines — a partial file crashes the loader at server startup.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ4KFileName] = 1689498432L,
        [ModelF16FileName] = 4972550208L,
        [RefFileName] = 433600L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.VoxCPM2CrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey switch
        {
            ModelKeyF16 => ModelKeyF16,
            _ => ModelKeyQ4K,
        };
    }

    public static string GetModelFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyF16 => ModelF16FileName,
        _ => ModelQ4KFileName,
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

    // VoxCPM2 has Metal/CUDA backends in CrispASR v0.7.0, but on CPU a single utterance can still
    // take minutes through the CFM diffusion ODE + LocDiT. 30 minutes gives generous headroom and
    // matches the other CrispASR TTS engines; users can always cancel from the GeneratingAudioWindow.
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(30),
    };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverLaunchCommand;
    private static string? _serverModelKey;
    // Only used when UseStartupVoiceFlags is true. Changing the reference WAV means tear-down +
    // restart so the new --voice path takes effect.
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "VoxCPM2CrispAsr");
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
    /// One-time best-effort seed of WAV reference voices from qwen3-tts.cpp's voices folder so
    /// the voice combo isn't empty on first run. Copies the .wav (resampled to 24 kHz mono) and
    /// any .txt transcription sidecar.
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
                        // qwen3-tts.cpp voice pack ships at 16 kHz; resample to 24 kHz mono on
                        // seed (crispasr upsamples to VoxCPM2's 48 kHz internally).
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
                        Se.LogError(ex, $"VoxCPM2 (CrispASR): resample seed '{src}' failed; falling back to plain copy");
                        try { if (!File.Exists(dest))
                        {
                            File.Copy(src, dest);
                        } } catch { }
                    }
                }

                var sidecar = Path.ChangeExtension(src, ".txt");
                if (File.Exists(sidecar))
                {
                    var sidecarDest = Path.ChangeExtension(dest, ".txt");
                    if (!File.Exists(sidecarDest))
                    {
                        // The qwen3-tts.cpp voice pack's .txt sidecars are Wikimedia attribution
                        // blurbs, not spoken transcriptions - used as ref-text they produce
                        // runaway, off-voice output, and being non-empty they also defeat the
                        // missing-transcription prompt. Same filter Qwen3 (CrispASR) applies.
                        try
                        {
                            if (!Qwen3TtsCrispAsr.LooksLikeAttributionBlurb(File.ReadAllText(sidecar)))
                            {
                                File.Copy(sidecar, sidecarDest);
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "VoxCPM2 (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    public static string GetModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetModelFileName(modelKey));

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
                Se.LogError($"VoxCPM2 (CrispASR): removing wrong-sized local model file {destinationPath}");
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
            Se.LogError(ex, $"VoxCPM2 (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public static bool AreModelsInstalled(string? modelKey = null)
    {
        var modelsFolder = GetSetModelsFolder();
        foreach (var name in GetRequiredFileNames(modelKey))
        {
            if (!IsValidLocalModelFile(Path.Combine(modelsFolder, name), name))
            {
                return false;
            }
        }

        return true;
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
                result.Add(new Voice(new VoxCPM2Voice(name, file)));
            }
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
        if (voice.EngineVoice is not VoxCPM2Voice voxVoice)
        {
            throw new ArgumentException("Voice is not a VoxCPM2Voice");
        }

        if (string.IsNullOrEmpty(voxVoice.FilePath))
        {
            throw new InvalidOperationException(
                "VoxCPM2 (CrispASR) requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "Reference WAV should be mono (3-10 s of clean speech); an adjacent .txt file "
                + "with the spoken transcription is optional but improves cloning quality.");
        }

        var refText = TryReadRefText(voxVoice.FilePath);
        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, voxVoice.FilePath, refText, cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");

        var speed = Math.Clamp(Se.Settings.Video.TextToSpeech.VoxCPM2CrispAsrSpeed, 0.25, 4.0);
        var payload = new Dictionary<string, object>
        {
            ["input"] = text,
            ["response_format"] = "wav",
            // The OpenAI-compat server resolves `voice` as a NAME listed by /v1/voices (the file
            // stem inside --voice-dir). Passing a full path makes the server hang indefinitely,
            // so send the reference WAV's base name — it lives in the voices folder = --voice-dir.
            ["voice"] = Path.GetFileNameWithoutExtension(voxVoice.FilePath),
            ["speed"] = speed,
            // VoxCPM2 gates voice cloning behind a consent attestation (CrispASR v0.7.0 returns
            // HTTP 400 consent_required without it). The user supplies their own reference voice
            // by importing a WAV into SE, which is the act being attested here.
            ["consent_attestation"] = "I have the speaker's consent, or it is my own voice.",
            // Skip the audible AI-disclosure prefix CrispASR otherwise prepends to cloned audio;
            // SE surfaces the AI-generated nature in its UI. The inaudible watermark + C2PA
            // provenance metadata stay embedded regardless (defaults to true server-side).
            ["spoken_disclaimer"] = false,
        };
        if (!string.IsNullOrEmpty(refText))
        {
            // TODO: verify field name against a real v0.7.0 binary — the OpenAI-compat server
            // may expose this as ref_text / ref-text / instructions.
            payload["ref_text"] = refText;
        }

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"VoxCPM2 (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={voxVoice}, refTextLen={refText.Length}, textLen={text.Length})");

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

            var failMsg = $"VoxCPM2 (CrispASR) request failed — Voice: {voxVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "VoxCPM2 (CrispASR) — the crispasr server crashed during synthesis."
                    : "VoxCPM2 (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"VoxCPM2 (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {voxVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"VoxCPM2 (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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

            var modelFileName = GetModelFileName(modelKey);
            var modelPath = GetModelPath(modelKey);
            var hasLocalModel = AreModelsInstalled(modelKey);

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
            psi.ArgumentList.Add(hasLocalModel ? modelPath : "auto");
            if (!hasLocalModel)
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
                ?? throw new InvalidOperationException("Failed to start crispasr (voxcpm2-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("VoxCPM2 (CrispASR) server starting — "
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

            // First-run auto-download (the main GGUF is up to ~5 GB) needs a generous timeout.
            var deadline = DateTime.UtcNow.AddMinutes(hasLocalModel ? 5 : 30);
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
                        $"crispasr (voxcpm2-tts) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (voxcpm2-tts) did not report healthy within {(hasLocalModel ? 5 : 30)} minutes. Last output: {lastOutput}"
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

    public bool ImportVoice(string fileName) => ImportVoice(fileName, string.Empty);

    /// <summary>
    /// Imports <paramref name="fileName"/> as a VoxCPM2 reference voice. When
    /// <paramref name="transcript"/> is non-empty it's written to the destination .txt sidecar
    /// (improves cloning quality); when empty, falls back to copying any .txt sidecar that
    /// already lives next to the source WAV.
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

        // Resample to 24 kHz mono on import via ffmpeg regardless of source format (crispasr
        // upsamples to VoxCPM2's 48 kHz internally).
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
            Se.LogError(ex, "VoxCPM2 (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        if (!File.Exists(destinationFileName))
        {
            return false;
        }

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
            Se.LogError(ex, "VoxCPM2 (CrispASR) voice import: failed to write .txt sidecar");
        }

        return true;
    }

    /// <summary>
    /// Writes <paramref name="transcript"/> to <c>&lt;voice&gt;.txt</c> next to the supplied
    /// voice WAV inside the voices folder. Used to retro-fit a transcription onto a voice that
    /// was already imported (or seeded) without a full re-import.
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
            Se.LogError(ex, $"VoxCPM2 (CrispASR): failed to write ref-text sidecar for '{voiceWavPath}'");
            return false;
        }
    }
}
