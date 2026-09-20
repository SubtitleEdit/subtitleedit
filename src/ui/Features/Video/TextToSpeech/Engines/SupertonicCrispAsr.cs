using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
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
/// Supertone's Supertonic-3 (~99M parameters) run through the CrispASR runtime: a
/// non-autoregressive flow-matching model at 44.1 kHz that speaks 31 languages from one 200 MB
/// GGUF. It is the fastest local engine in SE by a wide margin - measured RTF ~0.09 on an M4
/// (a 5.4 s sentence in 0.46 s) - because there is no token-by-token decode at all.
///
/// Unlike every other CrispASR-backed engine here, it does NOT clone: the open release ships
/// ten fixed preset voices (F1-F5, M1-M5) baked into the GGUF, and the backend rejects anything
/// else. So the voice combo lists those presets, there is no voice import, and no cloning
/// consent is owed.
///
/// The server takes voice, language and speed per request (verified against v0.8.34), so one
/// server instance serves every line and nothing restarts when the user changes any of them.
/// Both voice and language are sticky server-side - a request without the field keeps the
/// previous request's value - so <see cref="Speak"/> always sends both.
///
/// License: OpenRAIL-M (commercial use allowed, with use restrictions and an attribution
/// requirement) - see https://huggingface.co/Supertone/supertonic-3.
/// </summary>
public class SupertonicCrispAsr : ITtsEngine
{
    public string Name => "Supertonic (CrispASR)";
    public string Description => "Supertone Supertonic-3 — very fast, 31 languages, 10 preset voices, via CrispASR";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => false;
    public bool SupportsPerLineVoiceCloning => false;

    public const string BackendName = "supertonic";
    public const string ModelFileName = "supertonic3-f16.gguf";

    /// <summary>The preset the backend itself starts on.</summary>
    public const string DefaultVoice = "M1";

    /// <summary>
    /// The ten presets baked into the GGUF, in the order the voice combo shows them. The
    /// backend accepts exactly these names and keeps the previous voice for anything else.
    /// </summary>
    public static readonly string[] PresetVoices =
    {
        "F1", "F2", "F3", "F4", "F5",
        "M1", "M2", "M3", "M4", "M5",
    };

    // Exact byte size on cstr/supertonic-3-GGUF (HF LFS metadata). Used to reject a truncated
    // file that crispasr's --auto-download may have left behind — same guard as the other
    // CrispASR-backed engines.
    private const long ExpectedModelFileSize = 200332512L;

    public static bool IsValidLocalModelFile(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            return new FileInfo(path).Length == ExpectedModelFileSize;
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
    /// the other CrispASR TTS engines and all CrispASR ASR engines.
    /// </summary>
    public static string GetCrispAsrExecutable()
    {
        return new CrispAsrCohere().GetExecutable();
    }

    /// <summary>
    /// Mirrors <see cref="Qwen3TtsCrispAsr.GetEngineUpdateStatus"/> — reads the speech-to-text
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "SupertonicCrispAsr");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetModelsFolder()
    {
        // Like the other CrispASR-backed engines, the GGUF lives alongside CrispASR's
        // speech-to-text models in CrispASR/models/ rather than under TextToSpeech/.
        var modelsFolder = Path.Combine(Se.CrispAsrFolder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public static string GetModelPath() => Path.Combine(GetSetModelsFolder(), ModelFileName);

    public static bool IsModelInstalled() => IsValidLocalModelFile(GetModelPath());

    /// <summary>
    /// Path crispasr's --auto-download writes GGUFs to. Mirrors the IndexTTS (CrispASR) helper
    /// so the SE-side downloader can adopt an already-cached file instead of re-pulling.
    /// </summary>
    public static string GetCrispAsrCacheFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "crispasr");

    /// <summary>
    /// Best-effort copy of the model from <see cref="GetCrispAsrCacheFolder"/> into SE's models
    /// folder. See <see cref="Qwen3TtsCrispAsr.TrySeedModelFromCrispAsrCache"/> for rationale —
    /// same truncation-guard semantics.
    /// </summary>
    public static bool TrySeedModelFromCrispAsrCache(string destinationPath)
    {
        if (IsValidLocalModelFile(destinationPath))
        {
            return true;
        }

        try
        {
            if (File.Exists(destinationPath))
            {
                Se.LogError($"Supertonic (CrispASR): removing wrong-sized local model file {destinationPath}");
                File.Delete(destinationPath);
            }

            var cachePath = Path.Combine(GetCrispAsrCacheFolder(), ModelFileName);
            if (!IsValidLocalModelFile(cachePath))
            {
                return false;
            }

            File.Copy(cachePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Supertonic (CrispASR): cache seed copy failed for {ModelFileName}");
            return false;
        }
    }

    public Task<Voice[]> GetVoices(string language)
    {
        // Every preset speaks every language, so the list does not depend on the language.
        return Task.FromResult(PresetVoices.Select(v => new Voice(new SupertonicVoice(v))).ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(Array.Empty<string>());

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(SupertonicLanguages.All);

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    /// <summary>
    /// The preset to send for <paramref name="voice"/>. A voice object left over from another
    /// engine, or an id the backend would not know, falls back to <see cref="DefaultVoice"/>
    /// rather than being passed on: the backend answers an unknown name by silently keeping
    /// the previous request's voice, which would make the line's speaker depend on line order.
    /// </summary>
    internal static string ResolveVoiceId(Voice? voice)
    {
        var id = (voice?.EngineVoice as SupertonicVoice)?.Voice;
        return PresetVoices.FirstOrDefault(p => string.Equals(p, id, StringComparison.OrdinalIgnoreCase))
               ?? DefaultVoice;
    }

    /// <summary>
    /// The JSON body <see cref="Speak"/> posts to <c>/v1/audio/speech</c>.
    /// </summary>
    internal static Dictionary<string, object> BuildSpeechPayload(string text, string voiceId, string languageCode, double speed)
    {
        var payload = new Dictionary<string, object>
        {
            ["input"] = text,
            ["response_format"] = "wav",
            ["speed"] = Math.Clamp(speed, 0.25, 4.0),

            // Always both: the backend keeps the previous request's voice and language when a
            // field is missing, so leaving either out would make this line inherit them.
            ["voice"] = voiceId,
            ["language"] = languageCode,
        };

        // Not a clone, so no consent is at stake - but the same call also declines the spoken
        // AI-disclosure prefix and carries the marking attestation the server wants alongside
        // the launch-time watermark opt-out; see CrispAsrTtsProvenance.
        CrispAsrTtsProvenance.AddSpeechAttestations(payload);
        return payload;
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
        if (voice.EngineVoice is not SupertonicVoice)
        {
            throw new ArgumentException("Voice is not a SupertonicVoice");
        }

        var voiceId = ResolveVoiceId(voice);
        var languageCode = SupertonicLanguages.ResolveLanguageArg(language);

        await EnsureServerRunningAsync(cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        var payload = BuildSpeechPayload(text, voiceId, languageCode, Se.Settings.Video.TextToSpeech.SupertonicCrispAsrSpeed);
        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Supertonic (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={voiceId}, language={languageCode}, textLen={text.Length})");

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

            var failMsg = $"Supertonic (CrispASR) request failed — Voice: {voiceId}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Supertonic (CrispASR) — the crispasr server crashed during synthesis."
                    : "Supertonic (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"Supertonic (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {voiceId}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Supertonic (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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

    private static async Task EnsureServerRunningAsync(CancellationToken ct)
    {
        // Voice, language and speed all ride on the request (see Speak), and there is one model,
        // so a running server is always the right server.
        if (_serverProcess is { HasExited: false } && _serverPort != 0)
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0)
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

            // Model GGUF: use the locally staged copy when present; otherwise fall back to
            // crispasr's own --auto-download (fetches into ~/.cache/crispasr/ on first run).
            var modelPath = GetModelPath();
            var hasLocalModel = IsValidLocalModelFile(modelPath);

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? GetSetFolder(),
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
            // The presets live in the GGUF, not in this folder, and v0.8.34 honours the request's
            // `voice` without it - but the server warns at startup that a request carrying
            // `voice` "will be rejected" when --voice-dir is unset, so set it rather than lean
            // on the gate staying open.
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetFolder());
            CrispAsrTtsProvenance.AddServerMarkingArgs(psi.ArgumentList, exe);

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (supertonic)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("Supertonic (CrispASR) server starting — "
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
            HookProcessExitOnce();

            // First-run auto-download is ~200 MB, and the model itself loads in a second or two.
            var deadline = DateTime.UtcNow.AddMinutes(hasLocalModel ? 5 : 15);
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
                    throw new InvalidOperationException(
                        $"crispasr (supertonic) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (supertonic) did not report healthy within {(hasLocalModel ? 5 : 15)} minutes. Last output: {lastOutput}"
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
    /// Stop the running crispasr (supertonic) server if any. Called by
    /// <c>TextToSpeechViewModel</c> when starting synthesis on a different engine or when the
    /// TTS window closes, so the CrispASR-based TTS engines don't pile up in memory.
    /// </summary>
    public static void StopServer() => StopServerInternal();

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverLaunchCommand = null;
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

    /// <summary>
    /// Supertonic-3's open release has fixed preset voices only, so there is nothing to import.
    /// </summary>
    public bool ImportVoice(string fileName) => false;
}
