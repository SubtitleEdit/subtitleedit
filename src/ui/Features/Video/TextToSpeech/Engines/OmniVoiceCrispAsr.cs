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
/// OmniVoice run through the CrispASR runtime (the <c>omnivoice</c> backend) instead of the
/// standalone <c>omnivoice-tts</c> CLI that <see cref="OmniVoiceTtsCpp"/> drives. Same model
/// family, but it reuses the CrispASR runtime a user has already installed for speech-to-text
/// and runs as a persistent server, so the model is loaded once instead of once per line.
///
/// Two GGUFs are needed and both live in SE's CrispASR/models folder:
///   - Main model : omnivoice-{q4_k,q8_0,f16}.gguf     (the quant the user picks)
///   - Tokenizer  : omnivoice-tokenizer-f16.gguf       (passed via --codec-model)
///
/// ⚠ The tokenizer must be the F16 build. cstr/omnivoice-GGUF also publishes
/// <c>omnivoice-tokenizer-q8_0.gguf</c>, and it loads and synthesises fine — but its encoder
/// tensors cannot be read back as f32 (crispasr logs <c>read_tensor_f32: unsupported type 8</c>
/// per quantizer), so encoding a *reference* voice yields garbage codes and the clone comes out
/// as noise. Worse, crispasr caches the encoded reference by audio content, so once a q8_0 run
/// has poisoned the cache a later F16 run reuses the bad codes. SE therefore only ever ships
/// and passes the F16 tokenizer. Verified against the pinned v0.8.25 binary on Apple M4 / Metal
/// by median-F0 comparison of reference vs clone:
///   female ref 191.5 Hz → clone 190.6 Hz, male ref 110.2 Hz → clone 111.8 Hz (both intelligible)
///   same run with the q8_0 tokenizer → 481 Hz, transcribes to "I." (noise)
///
/// Unlike VoxCPM2/MOSS-TTS this backend has a usable built-in voice, so the combo offers
/// "Default" (no reference) alongside any imported reference WAVs — matching what the
/// standalone OmniVoice TTS engine offers.
///
/// Server usage:
///   crispasr --server --backend omnivoice -m omnivoice-q4_k.gguf \
///       --codec-model omnivoice-tokenizer-f16.gguf --host 127.0.0.1 --port N \
///       [--voice reference.wav --ref-text "transcript"]
/// then POST /v1/audio/speech with {"input": ..., "response_format": "wav", "speed": x,
/// "language": id}.
/// Confirmed on crispasr 0.8.25: the reference comes from the startup <c>--voice</c> flag (the
/// per-request log reports <c>voice='&lt;startup&gt;'</c>), so the server is torn down and
/// restarted when the selected voice or ref-text changes — the same shape the other cloning
/// CrispASR engines settled on after #12757. The target language is the opposite case: a
/// per-request field, applied without a restart.
///
/// ⚠ Honouring <c>language</c> per request needs a crispasr build newer than the pinned v0.8.25.
/// On v0.8.25 the CLI adapter applies the language once at startup only, so the field is parsed
/// and ignored and every line stays language-agnostic (#13273). Sending it is harmless there —
/// the IDs SE offers are the model's own — and it starts working the moment
/// <see cref="Logic.Download.CrispAsrDownloadService"/> is bumped past v0.8.25.
/// </summary>
public class OmniVoiceCrispAsr : ITtsEngine
{
    public string Name => "OmniVoice TTS (CrispASR)";
    public string Description => "646 languages, built-in voice + zero-shot cloning, via CrispASR";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    // Three quants of the LLM half. The label total includes the F16 tokenizer companion
    // (~403 MB), which every quant shares.
    public const string ModelKeyQ4K = "Q4_K (~1 GB)";
    public const string ModelKeyQ8_0 = "Q8_0 (~1.2 GB)";
    public const string ModelKeyF16 = "F16 (~1.6 GB)";
    public const string DefaultModelKey = ModelKeyQ4K;

    public const string ModelQ4KFileName = "omnivoice-q4_k.gguf";
    public const string ModelQ8_0FileName = "omnivoice-q8_0.gguf";
    public const string ModelF16FileName = "omnivoice-f16.gguf";

    /// <summary>
    /// The F16 tokenizer/codec companion. Deliberately not user-selectable — see the class
    /// remarks for why the q8_0 build silently breaks voice cloning.
    /// </summary>
    public const string TokenizerFileName = "omnivoice-tokenizer-f16.gguf";

    public const string BackendName = "omnivoice";

    /// <summary>
    /// Name shown for the model's own voice, i.e. synthesis with no reference audio. Matches the
    /// standalone OmniVoice TTS engine so the two behave the same in the voice combo.
    /// </summary>
    public const string DefaultVoiceName = "Default";

    /// <summary>
    /// All filenames that must sit in <see cref="GetSetModelsFolder"/> for the chosen quant:
    /// the picked main model plus the shared F16 tokenizer. Main first so the download dialog
    /// reports the big file before the small one.
    /// </summary>
    public static string[] GetRequiredFileNames(string? modelKey) => new[]
    {
        GetModelFileName(modelKey),
        TokenizerFileName,
    };

    // Exact byte sizes from the HF tree API (cstr/omnivoice-GGUF). A truncated GGUF crashes the
    // loader at server startup, so size is checked before the file is considered installed.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ4KFileName] = 597547584L,
        [ModelQ8_0FileName] = 817748544L,
        [ModelF16FileName] = 1230625280L,
        [TokenizerFileName] = 403183616L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.OmniVoiceCrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey switch
        {
            ModelKeyF16 => ModelKeyF16,
            ModelKeyQ8_0 => ModelKeyQ8_0,
            _ => ModelKeyQ4K,
        };
    }

    public static string GetModelFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyF16 => ModelF16FileName,
        ModelKeyQ8_0 => ModelQ8_0FileName,
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

    // Measured RTF is ~1.2-1.8 on Apple M4 / Metal (the codec half runs on CPU), so a long
    // subtitle line is seconds rather than minutes. 30 minutes still matches the other CrispASR
    // TTS engines and leaves room for CPU-only machines; users can cancel from the
    // GeneratingAudioWindow.
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(30),
    };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverLaunchCommand;
    private static string? _serverModelKey;
    // The reference is a startup flag, so changing the voice or its transcript means tear-down
    // and restart.
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "OmniVoiceCrispAsr");
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

        SeedVoicesFromOmniVoiceTtsCppIfEmpty(voicesFolder);
        return voicesFolder;
    }

    private static bool _voiceSeedAttempted;

    /// <summary>
    /// One-time best-effort seed from the standalone OmniVoice TTS engine's voices folder, so a
    /// user who already imported reference WAVs there does not have to import them again here.
    /// Copies the WAV (resampled to 24 kHz mono, OmniVoice's native rate) and any .txt sidecar.
    /// </summary>
    private static void SeedVoicesFromOmniVoiceTtsCppIfEmpty(string voicesFolder)
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

            var sourceFolder = OmniVoiceTtsCpp.GetSetVoicesFolder();
            if (!Directory.Exists(sourceFolder) || !Directory.EnumerateFiles(sourceFolder, "*.wav").Any())
            {
                return;
            }

            foreach (var src in Directory.GetFiles(sourceFolder, "*.wav"))
            {
                var dest = Path.Combine(voicesFolder, Path.GetFileName(src));
                VoiceSeedHelper.CopyOrResample(src, dest, 24000, "OmniVoice (CrispASR)");

                var sidecar = Path.ChangeExtension(src, ".txt");
                if (File.Exists(sidecar))
                {
                    var sidecarDest = Path.ChangeExtension(dest, ".txt");
                    if (!File.Exists(sidecarDest))
                    {
                        // These sidecars were written by OmniVoice TTS' own import, which stores
                        // the spoken transcription, so they are usable as ref-text as-is.
                        try { File.Copy(sidecar, sidecarDest); } catch { }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "OmniVoice (CrispASR): voice seeding from OmniVoice TTS folder failed");
        }
    }

    public static string GetModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetModelFileName(modelKey));

    public static string GetTokenizerPath() =>
        Path.Combine(GetSetModelsFolder(), TokenizerFileName);

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
                Se.LogError($"OmniVoice (CrispASR): removing wrong-sized local model file {destinationPath}");
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
            Se.LogError(ex, $"OmniVoice (CrispASR): cache seed copy failed for {fileName}");
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

    public async Task<Voice[]> GetVoices(string language)
    {
        // The backend has a usable built-in voice, so "Default" leads and cloning is opt-in.
        var result = new List<Voice>
        {
            new Voice(new OmniVoiceCrispAsrVoice(DefaultVoiceName, string.Empty)),
        };

        // Off the UI thread: GetSetVoicesFolder does one-time reference-WAV seeding through
        // ffmpeg, and this is awaited from SelectedEngineChanged on the dispatcher.
        var voicesFolder = await Task.Run(GetSetVoicesFolder);
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new OmniVoiceCrispAsrVoice(name, file)));
            }
        }

        return result.ToArray();
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ4K, ModelKeyQ8_0, ModelKeyF16 });

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(OmniVoiceLanguages.All);

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
        if (voice.EngineVoice is not OmniVoiceCrispAsrVoice omniVoice)
        {
            throw new ArgumentException("Voice is not an OmniVoiceCrispAsrVoice");
        }

        var voicePath = omniVoice.FilePath ?? string.Empty;
        var refText = string.IsNullOrEmpty(voicePath) ? string.Empty : TryReadRefText(voicePath);
        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, voicePath, refText, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        var speed = Math.Clamp(Se.Settings.Video.TextToSpeech.OmniVoiceCrispAsrSpeed, 0.25, 4.0);

        // No `voice` field on purpose: like the other cloning CrispASR backends, omnivoice reads
        // the reference from the startup --voice flag and treats a per-request `voice` as a file
        // path, so sending a bare stem would drop us to the built-in voice without saying so
        // (#12757). The server restarts on (voice, ref-text) change instead — see
        // EnsureServerRunningAsync.
        var payload = new Dictionary<string, object>
        {
            ["input"] = text,
            ["response_format"] = "wav",
            ["speed"] = speed,
        };

        // Only meaningful for cloned voices; the built-in voice impersonates no one.
        if (!string.IsNullOrEmpty(voicePath))
        {
            CrispAsrTtsProvenance.AddSpeechAttestations(payload);
        }

        // Target language (#13273). Unlike voice/ref-text this is a per-request field, so it needs
        // no server restart — which is exactly why it has to travel on the payload: SE keeps one
        // server for the whole session, so a startup flag could never follow a combo change after
        // the first line. Auto (empty) sends no field and leaves the model language-agnostic.
        var languageArg = OmniVoiceLanguages.ResolveLanguageArg(language);
        if (!string.IsNullOrEmpty(languageArg))
        {
            payload["language"] = languageArg;
        }

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"OmniVoice (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={omniVoice}, refTextLen={refText.Length}, textLen={text.Length}, language={(string.IsNullOrEmpty(languageArg) ? "(auto)" : languageArg)})");

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

            var failMsg = $"OmniVoice (CrispASR) request failed — Voice: {omniVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "OmniVoice (CrispASR) — the crispasr server crashed during synthesis."
                    : "OmniVoice (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"OmniVoice (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {omniVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"OmniVoice (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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
            if (!File.Exists(sidecar))
            {
                return string.Empty;
            }

            var text = File.ReadAllText(sidecar).Trim();
            return Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(text) ? string.Empty : text;
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
            && string.Equals(_serverVoicePath, voicePath, StringComparison.OrdinalIgnoreCase)
            && string.Equals(_serverRefText, refText, StringComparison.Ordinal);

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
            if (hasLocalModel)
            {
                // Pin the F16 tokenizer explicitly. Left to sibling discovery, crispasr would be
                // free to pick up a q8_0 tokenizer sitting in the same folder, which silently
                // breaks cloning — see the class remarks.
                psi.ArgumentList.Add("--codec-model");
                psi.ArgumentList.Add(GetTokenizerPath());
            }
            else
            {
                psi.ArgumentList.Add("--auto-download");
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());
            CrispAsrTtsProvenance.AddServerMarkingArgs(psi.ArgumentList, exe);

            if (!string.IsNullOrEmpty(voicePath))
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
                ?? throw new InvalidOperationException("Failed to start crispasr (omnivoice)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("OmniVoice (CrispASR) server starting — "
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

            // First-run auto-download pulls up to ~1.6 GB, so it gets a generous deadline.
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
                        $"crispasr (omnivoice) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (omnivoice) did not report healthy within {(hasLocalModel ? 5 : 30)} minutes. Last output: {lastOutput}"
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
    /// Imports <paramref name="fileName"/> as an OmniVoice reference voice. When
    /// <paramref name="transcript"/> is non-empty it is written to the destination .txt sidecar
    /// (passed as --ref-text and improves cloning quality); when empty, any .txt sidecar already
    /// next to the source WAV is copied instead.
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

        // Resample to 24 kHz mono on import — OmniVoice's native rate, so crispasr does no
        // further conversion of the reference.
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
            Se.LogError(ex, "OmniVoice (CrispASR) voice import failed (ffmpeg conversion).");
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
            Se.LogError(ex, "OmniVoice (CrispASR) voice import: failed to write .txt sidecar");
        }

        return true;
    }

    /// <summary>
    /// Writes <paramref name="transcript"/> to <c>&lt;voice&gt;.txt</c> next to the supplied voice
    /// WAV, so a transcription can be retro-fitted onto an imported or seeded voice without a
    /// full re-import.
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
            Se.LogError(ex, $"OmniVoice (CrispASR): failed to write ref-text sidecar for '{voiceWavPath}'");
            return false;
        }
    }
}
