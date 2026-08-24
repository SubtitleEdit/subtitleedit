using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
/// dots.tts SOAR (dots studio, Apache-2.0) run through the CrispASR runtime. A 2B continuous
/// autoregressive TTS model: a Qwen2.5-1.5B backbone drives an 18-layer flow-matching DiT that
/// predicts acoustic latents directly, and a BigVGAN vocoder renders them at <b>48 kHz</b> — the
/// highest output rate of any TTS engine SE ships (the others are 24 kHz). There is no discrete
/// audio codec anywhere in the stack, and the backbone reads BPE text rather than phonemes, so no
/// G2P dictionary is involved.
///
/// SOAR is the post-trained variant (Self-corrective Alignment): upstream reports the best
/// zero-shot speaker similarity of the three dots.tts checkpoints, which is why it is the one
/// CrispASR ships.
///
/// Three GGUFs are needed:
///  - dots-tts-soar-{q4_k,q8_0,f16}.gguf : the core model (LLM + PatchEncoder + DiT)
///  - dots-tts-soar-vocoder-f16.gguf     : BigVGAN 48 kHz vocoder (required companion)
///  - dots-tts-soar-spk-f16.gguf         : CAM++ speaker encoder (required for voice cloning)
///
/// The quants are deliberately <i>mixed</i>: cstr keeps the flow-matching DiT at F16 in every
/// quant, because it runs in a CFG Euler ODE loop (~16 steps x 18 layers x 2 CFG passes) where
/// per-step quantization noise compounds and derails generation — a fully-Q8 DiT produces no-EOS
/// runaway. Only the LLM and PatchEncoder are quantized, so Q4_K is a legitimate choice here
/// rather than the usual sub-8-bit gamble. Do not re-quantize these files.
///
/// Verified against the pinned CrispASR v0.8.29 on macOS/Metal (2026-08-24): CLI and server mode
/// both work with the exact flags and payload below, output is 48 kHz mono, and the clone tracks
/// the reference speaker (median f0: reference 190.3 Hz, clone 195.8 Hz, preset voice 124.8 Hz).
/// Note that v0.8.29's <c>--list-backends-json</c> does <i>not</i> advertise the
/// <c>voice-cloning</c> capability for this backend even though cloning demonstrably works — the
/// capability bitmask is stale, not the feature.
///
/// <b>It is slow.</b> Measured RTF 7.64 on an M4 with the F16 core at 16 ODE steps (3.5 s of audio
/// in 26.9 s), against ~1.6 for IndexTTS 2.5 on the same machine. The cost is the CFG Euler loop
/// in the DiT, which stays F16 in every quant, so a smaller quant saves memory rather than time —
/// the ODE step count is the lever, and it scales close to linearly (4/16/32 steps took 25/38/68 s
/// wall including model load).
///
/// CLI shape:
///   crispasr --backend dots-tts -m dots-tts-soar-q8_0.gguf \
///       --voice reference.wav --i-have-rights \
///       --tts "Hello, how are you today?" --tts-output out.wav
///
/// Unlike F5-TTS / CosyVoice3, dots.tts conditions on a CAM++ speaker embedding (a 512-d x-vector
/// projected to a 1024-d g_cond) rather than by continuing a transcribed reference, so there is no
/// reference-transcript parameter at all — an imported WAV needs no .txt sidecar.
/// </summary>
public class DotsTtsCrispAsr : ITtsEngine
{
    public string Name => "dots.tts (CrispASR)";
    public string Description => "dots.tts SOAR 2B, 48 kHz with voice cloning, via CrispASR";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => false;

    // Labels carry the total download (core + vocoder + speaker encoder), not just the core file,
    // so the dropdown does not understate what the user is about to fetch.
    public const string ModelKeyQ4K = "Q4_K (~2.4 GB)";
    public const string ModelKeyQ8_0 = "Q8_0 (~3.5 GB)";
    public const string ModelKeyF16 = "F16 (~5.0 GB)";
    public const string DefaultModelKey = ModelKeyQ8_0;

    public const string CoreQ4KFileName = "dots-tts-soar-q4_k.gguf";
    public const string CoreQ8_0FileName = "dots-tts-soar-q8_0.gguf";
    public const string CoreF16FileName = "dots-tts-soar-f16.gguf";

    /// <summary>
    /// BigVGAN vocoder. Kept at F16 for every core quant — it is only ~345 MB, and it is the last
    /// stage before the waveform, so this is the one place where saving 25 MB is not worth it.
    /// </summary>
    public const string VocoderFileName = "dots-tts-soar-vocoder-f16.gguf";

    /// <summary>
    /// CAM++ speaker encoder (~14 MB). CrispASR auto-discovers it as a sibling of the core GGUF
    /// and only loads it when --voice is passed, so there is no flag for it — it just has to be
    /// in the same folder. Without it the backend cannot clone.
    /// </summary>
    public const string SpeakerEncoderFileName = "dots-tts-soar-spk-f16.gguf";

    public const string BackendName = "dots-tts";

    /// <summary>
    /// Flow-matching Euler steps. Upstream recommends 10-32 (higher = better, slower);
    /// CrispASR's default is 16. Passed to the server through CRISPASR_DOTS_ODE_STEPS —
    /// there is no CLI flag for it.
    /// </summary>
    public const int MinOdeSteps = 8;
    public const int MaxOdeSteps = 32;
    public const int DefaultOdeSteps = 16;

    private const string OdeStepsEnvironmentVariable = "CRISPASR_DOTS_ODE_STEPS";

    // Exact byte sizes from the HF tree API (cstr/dots-tts-soar-GGUF). Same truncation guard as
    // the other CrispASR TTS engines — a partial GGUF crashes the loader at server startup.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [CoreQ4KFileName] = 2322162816L,
        [CoreQ8_0FileName] = 3128255616L,
        [CoreF16FileName] = 4639679552L,
        [VocoderFileName] = 362123680L,
        [SpeakerEncoderFileName] = 14953856L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.DotsTtsCrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey switch
        {
            ModelKeyQ4K => ModelKeyQ4K,
            ModelKeyF16 => ModelKeyF16,
            _ => ModelKeyQ8_0,
        };
    }

    public static string GetCoreFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyQ4K => CoreQ4KFileName,
        ModelKeyF16 => CoreF16FileName,
        _ => CoreQ8_0FileName,
    };

    public static int ResolveOdeSteps()
    {
        var saved = Se.Settings.Video.TextToSpeech.DotsTtsCrispAsrOdeSteps;
        return saved <= 0 ? DefaultOdeSteps : Math.Clamp(saved, MinOdeSteps, MaxOdeSteps);
    }

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
        Timeout = TimeSpan.FromMinutes(10),
    };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverLaunchCommand;
    // Same startup-flag cloning as indextts/f5/voxcpm2: the reference is read from the --voice
    // path at server start, never from the request body, so a voice change means tear down and
    // restart. Tracked alongside the quant and the ODE-step count, which are equally baked in.
    private static string? _serverVoicePath;
    private static string? _serverModelKey;
    private static int _serverOdeSteps;
    private static bool _processExitHooked;
    private static readonly StringBuilder _serverLog = new();

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetCrispAsrExecutable()));
    }

    public override string ToString() => Name;

    /// <summary>
    /// Path to the crispasr executable installed by the speech-to-text feature. Shared with every
    /// other CrispASR-backed engine.
    /// </summary>
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "DotsTtsCrispAsr");
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
    /// One-time best-effort seed of WAV reference voices from qwen3-tts.cpp's voices folder, so a
    /// fresh install has something in the voice combo. Resampled to 24 kHz on the way in, matching
    /// what <see cref="ImportVoice"/> produces.
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
                VoiceSeedHelper.CopyOrResample(src, dest, 24000, "dots.tts (CrispASR)");
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "dots.tts (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    public static string GetCorePath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetCoreFileName(modelKey));

    public static string GetVocoderPath() =>
        Path.Combine(GetSetModelsFolder(), VocoderFileName);

    public static string GetSpeakerEncoderPath() =>
        Path.Combine(GetSetModelsFolder(), SpeakerEncoderFileName);

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetCorePath(modelKey), GetCoreFileName(modelKey))
        && IsValidLocalModelFile(GetVocoderPath(), VocoderFileName)
        && IsValidLocalModelFile(GetSpeakerEncoderPath(), SpeakerEncoderFileName);

    /// <summary>
    /// Path crispasr's --auto-download writes GGUFs to, so SE's downloader can adopt files a
    /// previous CLI run already fetched instead of re-pulling gigabytes.
    /// </summary>
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
                Se.LogError($"dots.tts (CrispASR): removing wrong-sized local model file {destinationPath}");
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
            Se.LogError(ex, $"dots.tts (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public async Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>();

        // Voice cloning only — no built-in preset voices are exposed, so the combo is empty until
        // the user imports a reference WAV (or the qwen3-tts.cpp seed above runs).
        // Off the UI thread: GetSetVoicesFolder does one-time seeding through ffmpeg.
        var voicesFolder = await Task.Run(GetSetVoicesFolder);
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new DotsTtsVoice(name, file)));
            }
        }

        return result.ToArray();
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ4K, ModelKeyQ8_0, ModelKeyF16 });

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
        if (voice.EngineVoice is not DotsTtsVoice dotsVoice)
        {
            throw new ArgumentException("Voice is not a DotsTtsVoice");
        }

        if (string.IsNullOrEmpty(dotsVoice.FilePath))
        {
            throw new InvalidOperationException(
                "dots.tts (CrispASR) requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "Reference WAV should be 24 kHz mono (3-10 s of clean speech).");
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, dotsVoice.FilePath, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        // Deliberately NO `voice` field: like indextts/f5/voxcpm2, the dots-tts backend takes its
        // reference from the startup --voice path and the server rejects absolute paths in the
        // request body outright (path-traversal guard). Verified on v0.8.29 — the server logs
        // `voice='<startup>'` for this payload and the CAM++ embedding is built from that file.
        // Also no `ref_text`: dots.tts conditions on a speaker embedding, not on a transcribed
        // reference, so the field has nothing to bind to. No `speed` either — dots-tts is not in
        // crispasr's --tts-speed backend list, and the quality/speed knob that does work here is
        // the ODE step count passed at server launch.
        var payload = new Dictionary<string, object>
        {
            ["input"] = text,
            ["response_format"] = "wav",
        };

        // Attests the user's own imported reference and the AI-disclosure duty; see
        // CrispAsrTtsProvenance. Skipped when voice cloning has not been accepted in settings.
        CrispAsrTtsProvenance.AddSpeechAttestations(payload);

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"dots.tts (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={dotsVoice}, textLen={text.Length})");

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

            var failMsg = $"dots.tts (CrispASR) request failed — Voice: {dotsVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "dots.tts (CrispASR) — the crispasr server crashed during synthesis."
                    : "dots.tts (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"dots.tts (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {dotsVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"dots.tts (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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

    private static async Task EnsureServerRunningAsync(string modelKey, string voicePath, CancellationToken ct)
    {
        var odeSteps = ResolveOdeSteps();

        if (_serverProcess is { HasExited: false } && _serverPort != 0
            && string.Equals(_serverVoicePath, voicePath, StringComparison.OrdinalIgnoreCase)
            && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
            && _serverOdeSteps == odeSteps)
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0
                && string.Equals(_serverVoicePath, voicePath, StringComparison.OrdinalIgnoreCase)
                && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
                && _serverOdeSteps == odeSteps)
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

            var coreFileName = GetCoreFileName(modelKey);
            var core = GetCorePath(modelKey);
            var hasLocalCore = IsValidLocalModelFile(core, coreFileName);
            var vocoder = GetVocoderPath();
            var hasLocalVocoder = IsValidLocalModelFile(vocoder, VocoderFileName);

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? GetSetFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                // The server writes UTF-8; without these the captured log reaches bug reports as
                // mojibake on Windows (#13572).
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            psi.ArgumentList.Add("--server");
            psi.ArgumentList.Add("--backend");
            psi.ArgumentList.Add(BackendName);
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(hasLocalCore ? core : "auto");
            if (hasLocalVocoder)
            {
                psi.ArgumentList.Add("--codec-model");
                psi.ArgumentList.Add(vocoder);
            }
            if (!hasLocalCore || !hasLocalVocoder)
            {
                psi.ArgumentList.Add("--auto-download");
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString(CultureInfo.InvariantCulture));
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());
            // Required: the dots-tts backend clones only from the startup --voice path. The CAM++
            // speaker encoder next to the core GGUF is picked up automatically when this is set.
            psi.ArgumentList.Add("--voice");
            psi.ArgumentList.Add(voicePath);
            CrispAsrTtsProvenance.AddServerMarkingArgs(psi.ArgumentList, exe);

            // ODE steps have no CLI flag — the backend reads them from the environment. Set it
            // explicitly rather than relying on the default so the value shown in the settings
            // dialog is the value actually used.
            psi.Environment[OdeStepsEnvironmentVariable] = odeSteps.ToString(CultureInfo.InvariantCulture);

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (dots-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("dots.tts (CrispASR) server starting — "
                + $"PID: {process.Id}, "
                + $"{OdeStepsEnvironmentVariable}: {odeSteps}, "
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
            _serverModelKey = modelKey;
            _serverOdeSteps = odeSteps;
            HookProcessExitOnce();

            // Loading the F16 core is ~4.6 GB off disk, so even the warm path is not instant;
            // first run may also be pulling those gigabytes through --auto-download.
            var timeoutMinutes = hasLocalCore && hasLocalVocoder ? 10 : 30;
            var deadline = DateTime.UtcNow.AddMinutes(timeoutMinutes);
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
                    _serverModelKey = null;
                    _serverOdeSteps = 0;
                    throw new InvalidOperationException(
                        $"crispasr (dots-tts) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (dots-tts) did not report healthy within {timeoutMinutes} minutes. Last output: {lastOutput}"
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
    /// Stop the running crispasr (dots-tts) server if any, releasing GPU memory. Called by
    /// <c>TextToSpeechViewModel</c> when switching engines or closing the TTS window so the
    /// CrispASR-based engines don't pile up in VRAM.
    /// </summary>
    public static void StopServer() => StopServerInternal();

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverLaunchCommand = null;
        _serverVoicePath = null;
        _serverModelKey = null;
        _serverOdeSteps = 0;
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

        // 24 kHz mono, matching the other CrispASR cloning engines so the voices folders stay
        // interchangeable. No .txt sidecar: dots.tts conditions on a CAM++ speaker embedding, so
        // it never needs the reference transcript.
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
            Se.LogError(ex, "dots.tts (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        return File.Exists(destinationFileName);
    }
}
