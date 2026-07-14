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
/// The T3 + S3Gen GGUFs are downloaded into CrispASR/models/ (shared with the CrispASR
/// speech-to-text models) — `-m auto` is avoided because its codec auto-discovery only
/// finds *-s3gen-f16.gguf while it actually downloads the q8_0 variants.
/// Chatterbox has one baked default voice; "voices" listed beyond Default come from
/// WAVs imported via <see cref="ImportVoice"/>. The full reference-WAV path is sent per-request
/// as the `voice` field — runtime WAV cloning is wired upstream in CrispASR's chatterbox backend.
/// </summary>
public class ChatterboxTtsCpp : ITtsEngine
{
    public string Name => "Chatterbox TTS (CrispASR)";
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
    private static string? _serverLaunchCommand;
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
    /// Returns the update status of the CrispASR engine Chatterbox runs on. Because Chatterbox
    /// shares the speech-to-text CrispASR install, this reflects that engine's
    /// <c>.installed.sha256</c> sidecar; returns <see cref="DownloadHashManager.UpdateStatus.Unknown"/>
    /// when CrispASR is not installed or the sidecar is missing.
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
        // Chatterbox is driven by the CrispASR binary, so its GGUFs live alongside
        // the CrispASR speech-to-text models in CrispASR/models/ rather than under
        // TextToSpeech/Chatterbox/. The voices folder and synth output WAVs still
        // live under TextToSpeech/Chatterbox/ since those are TTS-engine state, not
        // models.
        var modelsFolder = Path.Combine(Se.CrispAsrFolder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        MigrateLegacyModels(modelsFolder);

        return modelsFolder;
    }

    private static bool _legacyMigrationDone;

    /// <summary>
    /// One-time best-effort move of chatterbox-*.gguf files from the old
    /// TextToSpeech/Chatterbox/models/ location into CrispASR/models/, so users
    /// don't have to re-download ~1 GB after the layout change. Safe to call
    /// repeatedly; bails out after the first call per process.
    /// </summary>
    private static void MigrateLegacyModels(string modelsFolder)
    {
        if (_legacyMigrationDone)
        {
            return;
        }
        _legacyMigrationDone = true;

        var legacyFolder = Path.Combine(Se.TextToSpeechFolder, "Chatterbox", "models");
        if (!Directory.Exists(legacyFolder))
        {
            return;
        }

        try
        {
            foreach (var src in Directory.GetFiles(legacyFolder, "chatterbox-*.gguf"))
            {
                var dest = Path.Combine(modelsFolder, Path.GetFileName(src));
                try
                {
                    if (File.Exists(dest))
                    {
                        File.Delete(src);
                    }
                    else
                    {
                        File.Move(src, dest);
                    }
                }
                catch (Exception ex)
                {
                    Se.LogError(ex, $"Chatterbox: failed to migrate legacy model '{src}' to '{dest}'");
                }
            }

            if (Directory.GetFileSystemEntries(legacyFolder).Length == 0)
            {
                Directory.Delete(legacyFolder);
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Chatterbox: legacy models migration failed");
        }
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
        var inputText = text;

        // Send the bare file name as the `voice` field, not the path: the crispasr server rejects
        // anything with a path separator ("'voice' must not contain '..', a leading '/' or '~', or
        // path separators" - HTTP 400 invalid_voice), which used to fail every imported voice.
        // The chatterbox backend then opens that name relative to its working directory, which is
        // why the server is started in the voices folder (see EnsureServerRunningAsync). The name
        // must keep its .wav extension - the backend does not append one.
        // Empty string falls back to the model's baked default voice.
        var payload = new Dictionary<string, object>
        {
            ["input"] = inputText,
            ["response_format"] = "wav",
        };
        if (!string.IsNullOrEmpty(chatterboxVoice.FilePath))
        {
            payload["voice"] = Path.GetFileName(chatterboxVoice.FilePath);

            // Chatterbox gates voice cloning behind a consent attestation (CrispASR
            // returns HTTP 400 consent_required without it). The user supplies their
            // own reference voice by importing a WAV into SE, which is the act being
            // attested here. The baked default voice is not cloning, so no attestation
            // is sent for it.
            payload["consent_attestation"] = "I have the speaker's consent, or it is my own voice.";

            // Skip the audible AI-disclosure prefix CrispASR otherwise prepends to cloned
            // audio; SE surfaces the AI-generated nature in its UI. The inaudible watermark
            // + C2PA provenance metadata stay embedded regardless (defaults to true server-side).
            payload["spoken_disclaimer"] = false;
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
            var launchCommand = _serverLaunchCommand;
            var died = _serverProcess?.HasExited == true;
            if (died)
            {
                StopServerInternal();
            }
            var failMsg = $"Chatterbox TTS request failed - Voice: {chatterboxVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
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
                prefix
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
                var errMsg = $"Chatterbox TTS server error {(int)response.StatusCode} {response.StatusCode} - "
                    + $"Voice: {chatterboxVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Chatterbox TTS synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LaunchCmdSuffix(launchCommand));
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
                // Run in the voices folder: the chatterbox backend opens the `voice` of a
                // /v1/audio/speech request relative to its working directory. It does not resolve it
                // against --voice-dir (that only backs the /v1/voices endpoints), and the server
                // rejects a `voice` containing path separators, so this is what lets an imported
                // reference WAV be found at all. Every path passed below is absolute, so nothing
                // else depends on the working directory.
                WorkingDirectory = GetSetVoicesFolder(),
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
            // requests with a 'voice' field"). Pointing --voice-dir at our voices folder
            // satisfies the gate and also makes /v1/voices reflect the imported WAVs. It
            // does not make the chatterbox backend look the voice up in there though -
            // that is what the working directory above is for.
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (chatterbox)");

            // Record the exact launch command in the tools log so failures later in this
            // session can be reproduced from a shell. Also cache it on the static so the
            // runtime/startup error paths can surface it inline with the error dialog.
            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("Chatterbox TTS server starting - "
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
            HookProcessExitOnce();

            // First-run model auto-download (~880 MB) needs a generous timeout.
            var deadline = DateTime.UtcNow.AddMinutes(15);
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
                    _serverModelKey = null;
                    _serverLaunchCommand = null;
                    if (LooksLikeOutdatedCrispAsr(tail))
                    {
                        throw new InvalidOperationException(
                            "Chatterbox requires CrispASR v0.6.0 or newer. Re-download CrispASR via "
                            + "Video → Audio to text → Engine settings → Re-download, then try again."
                            + LaunchCmdSuffix(exitedLaunchCommand));
                    }
                    if (LooksLikeStaleModelCache(tail))
                    {
                        throw new InvalidOperationException(
                            "Chatterbox failed to load its model — the GGUFs in "
                            + GetSetModelsFolder() + " are likely stale or partially downloaded. "
                            + "Delete them and try again so they re-download. Original output: " + tail
                            + LaunchCmdSuffix(exitedLaunchCommand));
                    }
                    if (LooksLikeChatterboxTurboTokenizerMismatch(tail))
                    {
                        throw new InvalidOperationException(
                            "Chatterbox TTS \"Turbo\" does not load with CrispASR 0.8.0. The turbo model "
                            + "is fine — 0.8.0's tokenizer/vocab check was overly strict and rejected its "
                            + "benign embedding superset (50257-token tokenizer, text vocab size 50276). "
                            + "This is fixed upstream (CrispStrobe/CrispASR#181): a newer CrispASR loads "
                            + "Turbo normally, with no re-download. Until then, switch to the \"Base\" "
                            + "Chatterbox model, which works."
                            + Environment.NewLine + Environment.NewLine + tail
                            + LaunchCmdSuffix(exitedLaunchCommand));
                    }
                    if (LooksLikeChatterboxTurboStartupCrash(modelKey, tail))
                    {
                        throw new InvalidOperationException(
                            "Chatterbox TTS \"Turbo\" model crashed CrispASR during startup. This is a known "
                            + "upstream issue in the chatterbox-turbo backend (especially on macOS/CPU). "
                            + "Try the \"Base\" model instead, or file an issue at "
                            + "https://github.com/CrispStrobe/CrispASR/issues with the log below."
                            + Environment.NewLine + Environment.NewLine + tail
                            + LaunchCmdSuffix(exitedLaunchCommand));
                    }
                    throw new InvalidOperationException(
                        $"crispasr (chatterbox) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (chatterbox) did not report healthy within 15 minutes. Last output: {lastOutput}"
                + LaunchCmdSuffix(timeoutLaunchCommand));
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

    private static bool LooksLikeChatterboxTurboTokenizerMismatch(string output)
    {
        // CrispASR 0.8.0 added a tokenizer/vocab consistency check that was overly strict —
        // it rejected the upstream chatterbox-turbo GGUF (cstr/chatterbox-turbo-GGUF), which
        // embeds a 50257-token GPT-2 tokenizer but declares text_vocab_size=50276:
        //   chatterbox: tokenizer/model vocab mismatch: tokenizer has 50257 tokens,
        //               T3 text_vocab_size=50276. Re-convert with the tokenizer paired ...
        //   crispasr[chatterbox]: failed to load T3 model '...-turbo-t3-...gguf'
        // The mismatch is benign (embedding superset — the extra rows are reserved/unused), so
        // upstream made the check directional (CrispStrobe/CrispASR#181): tokenizer < vocab now
        // warns and loads. A CrispASR newer than 0.8.0 loads Turbo with no re-download. This
        // detector therefore only fires on 0.8.0, where the model can't load at all and the Base
        // model (internally consistent, 704 == 704) is the workaround. 0.7.x never validated this.
        return output.Contains("tokenizer/model vocab mismatch", StringComparison.Ordinal)
            || (output.Contains("text_vocab_size", StringComparison.Ordinal)
                && output.Contains("Re-convert with the tokenizer", StringComparison.Ordinal));
    }

    private static bool LooksLikeChatterboxTurboStartupCrash(string modelKey, string output)
    {
        // Known CrispASR 0.6.6 (current) bug: chatterbox-turbo backend segfaults during
        // s3gen init, right after the auto-fallback-to-CPU notice and before
        // "precomputed conds loaded". Reproduces deterministically on macOS / Apple
        // Silicon. The chatterbox (Base) backend on the same binary loads fine.
        if (!string.Equals(modelKey, ModelKeyTurbo, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return output.Contains("arch=chatterbox_turbo", StringComparison.Ordinal)
            && !output.Contains("precomputed conds loaded", StringComparison.Ordinal);
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
    /// Stop the running crispasr (chatterbox) server if any, releasing GPU memory. Called by
    /// <c>TextToSpeechViewModel</c> when starting synthesis on a different engine or when the
    /// TTS window closes, so the four CrispASR-based TTS engines don't pile up in VRAM.
    /// </summary>
    public static void StopServer() => StopServerInternal();

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverModelKey = null;
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
