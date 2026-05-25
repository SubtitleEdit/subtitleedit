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
/// Qwen3-TTS run through the CrispASR runtime instead of qwen3-tts.cpp. Useful as an A/B
/// comparison engine since CrispASR's qwen3-tts backends emit EOS reliably on short prompts
/// where qwen3-tts.cpp 0.6B doesn't (see PR investigation notes from May 2026).
///
/// Two CrispASR sub-backends are wired up:
///  - qwen3-tts-1.7b-voicedesign — VoiceDesign model, requires an `instructions` field
///    per request (free-text voice description, e.g. "a calm female voice").
///  - qwen3-tts-1.7b-customvoice — voice cloning, takes a reference WAV + transcription.
///
/// The 1.7B VoiceDesign talker GGUF is the same file the qwen3-tts.cpp engine uses (cstr's
/// upload, new tensor names). The CrispASR-style codec/tokenizer GGUF is a different file
/// (~986 MB) and lives in this engine's own folder — see <see cref="Qwen3TtsCrispAsrDownloadService"/>
/// for the URLs.
/// </summary>
public class Qwen3TtsCrispAsr : ITtsEngine
{
    public string Name => "Qwen3 TTS (CrispASR)";
    public string Description => "via CrispASR (VoiceDesign or CustomVoice 1.7B)";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    public const string ModelKeyVoiceDesign = "1.7B VoiceDesign";
    public const string ModelKeyCustomVoice = "1.7B CustomVoice";
    public const string DefaultModelKey = ModelKeyVoiceDesign;

    public const string VoiceDesignTalkerFileName = "qwen3-tts-12hz-1.7b-voicedesign-q8_0.gguf";
    public const string CustomVoiceTalkerFileName = "qwen3-tts-12hz-1.7b-customvoice-q8_0.gguf";
    public const string CodecFileName = "qwen3-tts-tokenizer-12hz.gguf";

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : saved;
        }

        return modelKey == ModelKeyCustomVoice ? ModelKeyCustomVoice : ModelKeyVoiceDesign;
    }

    public static string GetTalkerFileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyCustomVoice ? CustomVoiceTalkerFileName : VoiceDesignTalkerFileName;

    /// <summary>
    /// CrispASR sub-backend name matching <paramref name="modelKey"/>.
    /// VoiceDesign → qwen3-tts-1.7b-voicedesign, CustomVoice → qwen3-tts-1.7b-customvoice.
    /// </summary>
    public static string GetBackendName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyCustomVoice
            ? "qwen3-tts-1.7b-customvoice"
            : "qwen3-tts-1.7b-voicedesign";

    /// <summary>
    /// True when the resolved model is the instruction-tuned VoiceDesign variant. Only this
    /// model honours the voice instruction; CustomVoice ignores it and uses voice cloning.
    /// </summary>
    public static bool IsVoiceDesignModel(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyVoiceDesign;

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
    private static readonly StringBuilder _serverLog = new();

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetCrispAsrExecutable()));
    }

    public override string ToString() => Name;

    /// <summary>
    /// Path to the crispasr executable installed by the speech-to-text feature.
    /// Shared with Chatterbox TTS and all CrispASR ASR engines.
    /// </summary>
    public static string GetCrispAsrExecutable()
    {
        return new CrispAsrCohere().GetExecutable();
    }

    /// <summary>
    /// Returns the update status of the CrispASR runtime this engine sits on top of.
    /// Reads the speech-to-text CrispASR install's <c>.installed.sha256</c> sidecar;
    /// returns <see cref="DownloadHashManager.UpdateStatus.Unknown"/> when CrispASR
    /// is not installed or the sidecar is missing. Mirrors
    /// <see cref="ChatterboxTtsCpp.GetEngineUpdateStatus"/>.
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "Qwen3TtsCrispAsr");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetModelsFolder()
    {
        // Qwen3 TTS (CrispASR) is driven by the CrispASR binary, so its GGUFs live
        // alongside the CrispASR speech-to-text models in CrispASR/models/ rather
        // than under TextToSpeech/Qwen3TtsCrispAsr/. The voices folder and synth
        // output WAVs still live under TextToSpeech/Qwen3TtsCrispAsr/ since those
        // are TTS-engine state, not models. Mirrors ChatterboxTtsCpp's layout.
        var modelsFolder = Path.Combine(Se.CrispAsrFolder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        MigrateLegacyModels(modelsFolder);

        return modelsFolder;
    }

    private static bool _legacyModelsMigrationDone;

    /// <summary>
    /// One-time best-effort move of qwen3-tts-*.gguf files from the old
    /// TextToSpeech/Qwen3TtsCrispAsr/models/ location into CrispASR/models/, so
    /// users don't have to re-download up to ~5 GB after the layout change. Safe
    /// to call repeatedly; bails out after the first call per process. Mirrors
    /// <see cref="ChatterboxTtsCpp"/>'s MigrateLegacyModels.
    /// </summary>
    private static void MigrateLegacyModels(string modelsFolder)
    {
        if (_legacyModelsMigrationDone)
        {
            return;
        }
        _legacyModelsMigrationDone = true;

        var legacyFolder = Path.Combine(Se.TextToSpeechFolder, "Qwen3TtsCrispAsr", "models");
        if (!Directory.Exists(legacyFolder))
        {
            return;
        }

        var fileNames = new[]
        {
            VoiceDesignTalkerFileName,
            CustomVoiceTalkerFileName,
            CodecFileName,
        };

        try
        {
            foreach (var fileName in fileNames)
            {
                var src = Path.Combine(legacyFolder, fileName);
                if (!File.Exists(src))
                {
                    continue;
                }

                var dest = Path.Combine(modelsFolder, fileName);
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
                    Se.LogError(ex, $"Qwen3 TTS (CrispASR): failed to migrate legacy model '{src}' to '{dest}'");
                }
            }

            if (Directory.GetFileSystemEntries(legacyFolder).Length == 0)
            {
                Directory.Delete(legacyFolder);
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Qwen3 TTS (CrispASR): legacy models migration failed");
        }
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
    /// One-time best-effort copy of WAV reference voices from the existing qwen3-tts.cpp
    /// engine's voices folder. The same files work for both engines (just 24 kHz mono
    /// reference audio with optional .txt sidecars), so users who already downloaded the
    /// qwen3-tts.cpp voice pack get them for free here without a second ~10 MB download.
    /// Users who never installed qwen3-tts.cpp get voices.zip pulled by the
    /// DownloadTtsViewModel flow that chains after model download.
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
                    File.Copy(src, dest);
                }

                // Copy the matching .txt sidecar too (CrispASR uses it as the reference
                // transcription for CustomVoice cloning).
                var sidecar = Path.ChangeExtension(src, ".txt");
                if (File.Exists(sidecar))
                {
                    var sidecarDest = Path.ChangeExtension(dest, ".txt");
                    if (!File.Exists(sidecarDest))
                    {
                        File.Copy(sidecar, sidecarDest);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Qwen3 TTS (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    public static string GetTalkerPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetTalkerFileName(modelKey));

    public static string GetCodecPath() =>
        Path.Combine(GetSetModelsFolder(), CodecFileName);

    public static bool AreModelsInstalled(string? modelKey = null) =>
        File.Exists(GetTalkerPath(modelKey)) && File.Exists(GetCodecPath());

    /// <summary>
    /// Path crispasr's --auto-download writes GGUFs to before SE took over model management.
    /// Exposed so the SE downloader can copy already-cached files instead of re-pulling 2 GB,
    /// and so settings UI can reference the same location.
    /// </summary>
    public static string GetCrispAsrCacheFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "crispasr");

    /// <summary>
    /// Best-effort copy of <paramref name="fileName"/> from <see cref="GetCrispAsrCacheFolder"/>
    /// into SE's models folder. Used by the downloader to avoid re-pulling files crispasr's
    /// own --auto-download has already fetched. Returns true if the destination ends up present
    /// (whether seeded just now or already there); false on any IO failure.
    /// </summary>
    public static bool TrySeedModelFromCrispAsrCache(string fileName, string destinationPath)
    {
        if (File.Exists(destinationPath))
        {
            return true;
        }

        try
        {
            var cachePath = Path.Combine(GetCrispAsrCacheFolder(), fileName);
            if (!File.Exists(cachePath))
            {
                return false;
            }

            File.Copy(cachePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Qwen3 TTS (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>();

        // VoiceDesign has a baked default voice (the instruction string drives the rest).
        // CustomVoice is pure voice cloning and refuses requests without a reference WAV,
        // so don't offer a "Default" entry there — the user must import a voice first.
        var modelKey = ResolveModelKey(Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel);
        if (modelKey == ModelKeyVoiceDesign)
        {
            result.Add(new Voice(new Qwen3TtsVoice("Default", string.Empty)));
        }

        var voicesFolder = GetSetVoicesFolder();
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new Qwen3TtsVoice(name, file)));
            }
        }

        return Task.FromResult(result.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyVoiceDesign, ModelKeyCustomVoice });

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
        if (voice.EngineVoice is not Qwen3TtsVoice qwen3Voice)
        {
            throw new ArgumentException("Voice is not a Qwen3TtsVoice");
        }

        var modelKey = ResolveModelKey(model);

        // CustomVoice does voice cloning and rejects requests without a `voice` field.
        // Surface a clear up-front error instead of letting the backend return 500.
        if (modelKey == ModelKeyCustomVoice && string.IsNullOrEmpty(qwen3Voice.FilePath))
        {
            throw new InvalidOperationException(
                "Qwen3 TTS (CrispASR) CustomVoice requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "Reference WAV should be 24 kHz mono; an adjacent .txt file with the "
                + "spoken transcription is required for best cloning quality.");
        }

        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");
        var inputText = Utilities.UnbreakLine(text);
        // Share the qwen3-tts.cpp instruction setting so users get the same voice description
        // regardless of which Qwen3 engine they're testing with.
        var instruction = Se.Settings.Video.TextToSpeech.Qwen3TtsCppInstruction ?? string.Empty;

        // OpenAI-compatible /v1/audio/speech payload. CrispASR's qwen3-tts backends look at:
        //   - `input`             — the text to synthesise
        //   - `response_format`   — "wav"
        //   - `voice`             — absolute WAV path or filename in --voice-dir
        //                           (required for CustomVoice; ignored by VoiceDesign)
        //   - `instructions`      — free-text style/voice description
        //                           (required for VoiceDesign; ignored by CustomVoice)
        var payload = new Dictionary<string, object>
        {
            ["input"] = inputText,
            ["response_format"] = "wav",
        };
        if (!string.IsNullOrEmpty(qwen3Voice.FilePath))
        {
            payload["voice"] = qwen3Voice.FilePath;
        }
        if (!string.IsNullOrEmpty(instruction))
        {
            payload["instructions"] = instruction;
        }
        else if (modelKey == ModelKeyVoiceDesign)
        {
            // VoiceDesign refuses requests without an instruction. Send a neutral default
            // so the first run doesn't surface a confusing 500 from the server.
            payload["instructions"] = "a calm female voice";
        }

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Qwen3 TTS (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={qwen3Voice}, model={modelKey}, textLen={text.Length}, instructionLen={instruction.Length})");

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

            var failMsg = $"Qwen3 TTS (CrispASR) request failed — Voice: {qwen3Voice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Qwen3 TTS (CrispASR) — the crispasr server crashed during synthesis."
                    : "Qwen3 TTS (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"Qwen3 TTS (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {qwen3Voice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Qwen3 TTS (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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

            // Talker and codec GGUFs: if locally staged, use directly; otherwise hand off to
            // crispasr's --auto-download to fetch them into ~/.cache/crispasr/ on first run.
            // Slower the first time, instant after. A proper SE-side download service is a
            // follow-up; this keeps the engine usable without manual file placement.
            var talker = GetTalkerPath(modelKey);
            var hasLocalTalker = File.Exists(talker);
            var codec = GetCodecPath();
            var hasLocalCodec = File.Exists(codec);

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
            // The crispasr server gates /v1/audio/speech requests with a `voice` field behind
            // --voice-dir being set. Pointing at our voices folder satisfies the gate and also
            // makes /v1/voices reflect any imported reference WAVs.
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (qwen3-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("Qwen3 TTS (CrispASR) server starting — "
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

            // First-run auto-download (~2 GB talker + ~986 MB codec) needs a generous timeout.
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
                    _serverModelKey = null;
                    _serverLaunchCommand = null;
                    throw new InvalidOperationException(
                        $"crispasr (qwen3-tts) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (qwen3-tts) did not report healthy within {(hasLocalTalker && hasLocalCodec ? 5 : 30)} minutes. Last output: {lastOutput}"
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

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverModelKey = null;
        _serverLaunchCommand = null;
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

        // CrispASR's qwen3-tts CustomVoice backend expects a 24 kHz mono reference WAV.
        // Always resample on import via ffmpeg so the saved file is in the right shape
        // regardless of what the user picked.
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
            Se.LogError(ex, "Qwen3 TTS (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        if (!File.Exists(destinationFileName))
        {
            return false;
        }

        // CustomVoice voice cloning expects a matching .txt sidecar holding the spoken
        // transcription of the reference WAV. If the source had one (same basename, .txt),
        // copy it alongside the imported WAV under the de-duplicated destination name.
        try
        {
            var sourceSidecar = Path.ChangeExtension(fileName, ".txt");
            if (File.Exists(sourceSidecar))
            {
                var destSidecar = Path.ChangeExtension(destinationFileName, ".txt");
                if (!File.Exists(destSidecar))
                {
                    File.Copy(sourceSidecar, destSidecar);
                }
            }
        }
        catch (Exception ex)
        {
            // Sidecar copy is best-effort — log and continue. The imported WAV alone is
            // still usable; CustomVoice quality drops without the transcription but it
            // doesn't fail outright.
            Se.LogError(ex, "Qwen3 TTS (CrispASR) voice import: failed to copy .txt sidecar");
        }

        return true;
    }
}
