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
/// Zonos-v0.1 (Zyphra) run through the CrispASR runtime. 26-layer GQA autoregressive
/// transformer (2B) emitting 9 DAC codebooks @ 44.1 kHz, CFG-guided. Apache-2.0.
///
/// Two GGUFs are needed (same talker + companion split as the other CrispASR TTS engines):
///  - zonos-v0.1-transformer-q8_0.gguf : the AR transformer (the actual TTS model)
///  - dac-44khz-f16.gguf               : the Descript DAC 44 kHz codec (vocoder)
///
/// NOT a cloning engine, although the model is one and SE presented it as one until CrispASR
/// v0.8.34 said otherwise: the backend's <c>zonos_tts_set_voice()</c> is a stub (the ResNet293
/// speaker encoder is not ported), so <c>--voice</c> was always ignored - output with and
/// without it is sample-identical on v0.8.33. What speaks instead is a 128-d Gaussian speaker
/// embedding drawn once at model load from a constant RNG state, which makes it one fixed
/// voice: the same on every line and across server restarts. (<c>--seed N</c> at startup draws
/// a different one, of very uneven quality.) So this engine offers that single default voice,
/// passes no <c>--voice</c>, asks for no cloning consent and sends no cloning attestations.
/// Put cloning back only when upstream re-declares <c>voice-cloning</c> in
/// <c>--list-backends-json</c>.
///
/// This is a minimal engine: a single Q8_0 quant, no model dropdown, no per-engine settings
/// dialog.
///
/// Zonos is NOT language-agnostic: the input text goes through
/// eSpeak G2P in whatever language the backend is told, and the model carries a language
/// conditioner on top. CrispASR defaults both to en-us when the request has no
/// <c>language</c> field, so a Czech line came out phonemised as English (#14433). The main
/// window's language combo (<see cref="ZonosLanguages"/>) is passed as the server's startup
/// <c>-l</c> flag: the zonos backend reads the language once at init, so a language change
/// restarts the server (verified against v0.8.31: the request body's
/// <c>language</c> / <c>target_lang</c> fields leave zonos on en-us).
/// </summary>
public class ZonosTtsCrispAsr : ITtsEngine
{
    public string Name => "Zonos TTS (CrispASR)";
    public string Description => "Zyphra Zonos-v0.1 at 44.1 kHz, one default voice, via CrispASR";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => false;
    public bool SupportsPerLineVoiceCloning => false;

    public const string TalkerFileName = "zonos-v0.1-transformer-q8_0.gguf";
    public const string CodecFileName = "dac-44khz-f16.gguf";

    public const string BackendName = "zonos-tts";

    public const string DefaultVoiceName = "Default";

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
    // The -l language the running server was started with (empty = backend default en-us).
    // zonos reads it at init only, so a change restarts the server.
    private static string _serverLanguage = string.Empty;
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
        // The backend's one fixed speaker - see the class summary. Reference WAVs that earlier
        // versions imported or seeded into the engine's voices folder are left on disk but no
        // longer listed: the backend never used them.
        return Task.FromResult(new[] { new Voice(new ZonosTtsVoice(DefaultVoiceName, string.Empty)) });
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(Array.Empty<string>());

    /// <summary>
    /// The eSpeak voice codes from the model's own language table; English (US) leads as the
    /// backend default. See <see cref="ZonosLanguages"/>.
    /// </summary>
    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(ZonosLanguages.All);

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

        // The language to speak: picks the eSpeak G2P voice and the model's language
        // conditioner. Nothing = the backend's en-us default, which is what every line got
        // before #14433 - fine for English, English-phonemised gibberish for the rest.
        var languageArg = ZonosLanguages.ResolveLanguageArg(language);

        await EnsureServerRunningAsync(languageArg, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");
        var inputText = text;

        // OpenAI-compatible /v1/audio/speech payload. No `voice` field: the backend has a
        // single speaker and no way to pick another per request.
        var payload = new Dictionary<string, object>
        {
            ["input"] = inputText,
            ["response_format"] = "wav",
        };

        // Also sent per request, which is CrispASR's documented TTS API for the language to
        // speak. The v0.8.31 zonos backend does not act on it (only the startup -l above does),
        // but a server that learns to will then agree with the flag rather than fight it.
        if (!string.IsNullOrEmpty(languageArg))
        {
            payload["language"] = languageArg;
        }

        // No CrispAsrTtsProvenance.AddSpeechAttestations: nothing is cloned, so there is no
        // reference to attest consent for, and the server adds its spoken AI disclosure only to
        // clones and real-person presets - not to this synthetic speaker.

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Zonos TTS (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={zonosVoice}, textLen={text.Length}, language={(string.IsNullOrEmpty(languageArg) ? "(default en-us)" : languageArg)})");

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

    private static bool IsServerRunningWith(string languageArg) =>
        _serverProcess is { HasExited: false } && _serverPort != 0
        && string.Equals(_serverLanguage, languageArg, StringComparison.OrdinalIgnoreCase);

    private static async Task EnsureServerRunningAsync(string languageArg, CancellationToken ct)
    {
        // The zonos backend reads its language (-l) at init, so the server is restarted when
        // the selected language changes. Tracked next to _serverProcess so an already-running
        // server with the matching language is reused.
        if (IsServerRunningWith(languageArg))
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (IsServerRunningWith(languageArg))
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
            // No --voice / --voice-dir: the backend cannot clone (see the class summary), and
            // v0.8.34 answers --voice with a "NOT supported ... RANDOM speaker" warning.
            // The language to speak, as the eSpeak code from the model's own table (see
            // ZonosLanguages). Omitted = the backend's en-us default. The CLI lowercases it.
            if (!string.IsNullOrEmpty(languageArg))
            {
                psi.ArgumentList.Add("-l");
                psi.ArgumentList.Add(languageArg);
            }
            CrispAsrTtsProvenance.AddServerMarkingArgs(psi.ArgumentList, exe);

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
            _serverLanguage = languageArg;
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
                    _serverLanguage = string.Empty;
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
        _serverLanguage = string.Empty;
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
    /// Nothing to import: the backend has no speaker encoder, so a reference recording cannot
    /// become a voice. Unreachable from the UI while <see cref="SupportsVoiceCloning"/> is false.
    /// </summary>
    public bool ImportVoice(string fileName) => false;
}
