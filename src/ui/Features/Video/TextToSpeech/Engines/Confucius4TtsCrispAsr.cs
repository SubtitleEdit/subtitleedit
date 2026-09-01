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
/// Confucius4-TTS (NetEase Youdao, Apache-2.0) run through the CrispASR runtime. A two-stage
/// pipeline: a GPT-2 T2S model (24L/1280d) turns text into semantic tokens, a flow-matching
/// DiT + WaveNet S2A model renders them into mel frames conditioned on a w2v-BERT embedding plus
/// a CAM++ style vector from the reference audio, and a BigVGAN vocoder outputs <b>22.05 kHz</b>
/// mono — the only 22 kHz TTS engine SE ships. Officially trained on 14 languages
/// (<see cref="Confucius4TtsLanguages"/>), passed as the startup <c>-l</c> flag.
///
/// Four GGUFs are needed:
///  - confucius4-tts-t2s-{q8_0,f16}.gguf  : the T2S core (passed as -m)
///  - confucius4-tts-s2a-{q8_0,f16}.gguf  : flow-matching S2A (passed as --codec-model)
///  - confucius4-tts-bigvgan-22k-f16.gguf : BigVGAN vocoder (auto-discovered sibling)
///  - confucius4-tts-w2v-f16.gguf         : w2v-BERT reference encoder (auto-discovered sibling)
///
/// <b>Q4_K is deliberately not offered.</b> The registry's default quant makes the flow-matching
/// alignment break down audibly — an A/B against Q8_0 with the same text and reference confirmed
/// upstream's report (CrispASR #377): Q4_K sounds flat and robotic where Q8_0 holds the
/// reference's prosody. That earlier Q4_K test is also why this engine was first judged "too
/// robotic to ship". Since Q8_0 vs F16 only differs in the T2S/S2A core (the vocoder and w2v
/// companions are F16 in both), Q8_0 is the default.
///
/// <b>Zero-shot cloning is mandatory, not optional</b> — without <c>--voice</c> conditioning the
/// output is unintelligible by design (there is no default-voice mode at all), so the voice combo
/// only offers imported reference WAVs and synthesis refuses to run without one.
///
/// The S2A/vocoder chain works at 22.05 kHz and the w2v-BERT/CAM++ encoders at 16 kHz; crispasr
/// resamples the reference internally to both, so references are staged at 22.05 kHz — the mel
/// path reads them untouched and the encoder path downsamples cleanly.
///
/// Like indextts/dots-tts, the backend applies the reference only at init from the startup
/// <c>--voice</c> flag (crispasr_backend_confucius4_tts.cpp), and <c>/v1/audio/speech</c> has no
/// per-request language field, so a change of voice, quant, language or step count restarts the
/// server. No <c>ref-text</c> either: conditioning is from audio alone, so imported WAVs need no
/// .txt sidecar.
///
/// CLI shape (verified on v0.8.31, macOS/Metal, 2026-09-01):
///   crispasr --backend confucius4-tts -m confucius4-tts-t2s-q8_0.gguf \
///       --codec-model confucius4-tts-s2a-q8_0.gguf \
///       --voice reference.wav --i-have-rights -l en \
///       --tts "Hello, how are you today?" --tts-output out.wav
/// </summary>
public class Confucius4TtsCrispAsr : ITtsEngine
{
    public string Name => "Confucius4-TTS (CrispASR)";
    public string Description => "Confucius4-TTS (NetEase Youdao), 14 languages with voice cloning, via CrispASR";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => false;

    // Labels carry the total download (T2S + S2A + vocoder + w2v-BERT companion), not just the
    // quantized pair, so the dropdown does not understate what the user is about to fetch.
    public const string ModelKeyQ8_0 = "Q8_0 (~1.9 GB)";
    public const string ModelKeyF16 = "F16 (~2.6 GB)";
    public const string DefaultModelKey = ModelKeyQ8_0;

    public const string T2sQ8_0FileName = "confucius4-tts-t2s-q8_0.gguf";
    public const string T2sF16FileName = "confucius4-tts-t2s-f16.gguf";
    public const string S2aQ8_0FileName = "confucius4-tts-s2a-q8_0.gguf";
    public const string S2aF16FileName = "confucius4-tts-s2a-f16.gguf";

    /// <summary>
    /// BigVGAN 22.05 kHz vocoder. F16 for both quants — the Q8_0 variant is the same size to
    /// within 4 KB, so there is nothing to save.
    /// </summary>
    public const string VocoderFileName = "confucius4-tts-bigvgan-22k-f16.gguf";

    /// <summary>
    /// w2v-BERT reference encoder (~824 MB, F16 only — no other quant exists in the repo).
    /// CrispASR auto-discovers it as a sibling of the T2S GGUF; without it the T2S stage loses
    /// its native condition_emb and clone similarity drops.
    /// </summary>
    public const string W2vFileName = "confucius4-tts-w2v-f16.gguf";

    public const string BackendName = "confucius4-tts";

    /// <summary>
    /// Flow-matching Euler steps for the S2A stage, passed via <c>--tts-steps</c>. CrispASR's
    /// default is 20; upstream's reference implementation samples at 25.
    /// </summary>
    public const int MinOdeSteps = 10;
    public const int MaxOdeSteps = 40;
    public const int DefaultOdeSteps = 20;

    // Exact byte sizes from the HF tree API (cstr/confucius4-tts-GGUF). Same truncation guard as
    // the other CrispASR TTS engines — a partial GGUF crashes the loader at server startup.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [T2sQ8_0FileName] = 718043648L,
        [T2sF16FileName] = 1326049024L,
        [S2aQ8_0FileName] = 169737216L,
        [S2aF16FileName] = 222990272L,
        [VocoderFileName] = 224624608L,
        [W2vFileName] = 823502048L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.Confucius4TtsCrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey switch
        {
            ModelKeyF16 => ModelKeyF16,
            _ => ModelKeyQ8_0,
        };
    }

    public static string GetT2sFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyF16 => T2sF16FileName,
        _ => T2sQ8_0FileName,
    };

    /// <summary>
    /// The S2A quant follows the T2S quant. Passed explicitly as <c>--codec-model</c> because the
    /// backend's own sibling auto-discovery probes q4_k FIRST (crispasr_backend_confucius4_tts.cpp)
    /// — a stray q4_k file in the models folder would silently degrade quality.
    /// </summary>
    public static string GetS2aFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyF16 => S2aF16FileName,
        _ => S2aQ8_0FileName,
    };

    public static int ResolveOdeSteps()
    {
        var saved = Se.Settings.Video.TextToSpeech.Confucius4TtsCrispAsrOdeSteps;
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
    // Same startup-flag cloning as indextts/dots-tts: the reference is read from the --voice path
    // at server start, never from the request body, so a voice change means tear down and restart.
    // Tracked alongside the quant, language and ODE steps, which are equally baked in.
    private static string? _serverVoicePath;
    private static string? _serverModelKey;
    private static string? _serverLanguageArg;
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "Confucius4TtsCrispAsr");
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
    /// fresh install has something in the voice combo — with no reference this backend produces
    /// nothing usable at all. Resampled to 22.05 kHz on the way in, matching
    /// <see cref="ImportVoice"/>.
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
                VoiceSeedHelper.CopyOrResample(src, dest, 22050, "Confucius4-TTS (CrispASR)");
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Confucius4-TTS (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    public static string GetT2sPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetT2sFileName(modelKey));

    public static string GetS2aPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetS2aFileName(modelKey));

    public static string GetVocoderPath() =>
        Path.Combine(GetSetModelsFolder(), VocoderFileName);

    public static string GetW2vPath() =>
        Path.Combine(GetSetModelsFolder(), W2vFileName);

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetT2sPath(modelKey), GetT2sFileName(modelKey))
        && IsValidLocalModelFile(GetS2aPath(modelKey), GetS2aFileName(modelKey))
        && IsValidLocalModelFile(GetVocoderPath(), VocoderFileName)
        && IsValidLocalModelFile(GetW2vPath(), W2vFileName);

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
                Se.LogError($"Confucius4-TTS (CrispASR): removing wrong-sized local model file {destinationPath}");
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
            Se.LogError(ex, $"Confucius4-TTS (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public async Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>();

        // Voice cloning only — there are no preset voices and no default voice at all (the
        // backend's output is unintelligible without a reference), so the combo is empty until
        // the user imports a reference WAV (or the qwen3-tts.cpp seed above runs).
        // Off the UI thread: GetSetVoicesFolder does one-time seeding through ffmpeg.
        var voicesFolder = await Task.Run(GetSetVoicesFolder);
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new Confucius4TtsVoice(name, file)));
            }
        }

        return result.ToArray();
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ8_0, ModelKeyF16 });

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(Confucius4TtsLanguages.All);

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
        if (voice.EngineVoice is not Confucius4TtsVoice confuciusVoice)
        {
            throw new ArgumentException("Voice is not a Confucius4TtsVoice");
        }

        if (string.IsNullOrEmpty(confuciusVoice.FilePath))
        {
            throw new InvalidOperationException(
                "Confucius4-TTS (CrispASR) requires a reference voice WAV — without one the "
                + "output is unintelligible by design. Import one via the voice settings, then "
                + "pick it in the voice combo. Reference WAV should be 3-10 s of clean speech.");
        }

        var modelKey = ResolveModelKey(model);
        var languageArg = Confucius4TtsLanguages.ResolveLanguageArg(language);
        await EnsureServerRunningAsync(modelKey, confuciusVoice.FilePath, languageArg, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        // Deliberately NO `voice` field: like indextts/dots-tts, the confucius4-tts backend takes
        // its reference from the startup --voice path (applied only in the adapter's init), and
        // the server rejects absolute paths in the request body outright (path-traversal guard).
        // Also no `ref_text`: conditioning is w2v-BERT + CAM++ from the audio alone, so the field
        // has nothing to bind to. No `speed` either — confucius4-tts is not in crispasr's
        // --tts-speed backend list; the quality/speed knob that does work here is the ODE step
        // count passed at server launch. No temperature: the backend's own 0.8 default matters
        // (greedy T2S decode degenerates into a repeat loop that runs to the token cap).
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
        Se.WriteToolsLog($"Confucius4-TTS (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={confuciusVoice}, language={(string.IsNullOrEmpty(languageArg) ? "(en)" : languageArg)}, textLen={text.Length})");

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

            var failMsg = $"Confucius4-TTS (CrispASR) request failed — Voice: {confuciusVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Confucius4-TTS (CrispASR) — the crispasr server crashed during synthesis."
                    : "Confucius4-TTS (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"Confucius4-TTS (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {confuciusVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Confucius4-TTS (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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

    private static async Task EnsureServerRunningAsync(string modelKey, string voicePath, string languageArg, CancellationToken ct)
    {
        var odeSteps = ResolveOdeSteps();

        if (_serverProcess is { HasExited: false } && _serverPort != 0
            && string.Equals(_serverVoicePath, voicePath, StringComparison.OrdinalIgnoreCase)
            && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(_serverLanguageArg, languageArg, StringComparison.OrdinalIgnoreCase)
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
                && string.Equals(_serverLanguageArg, languageArg, StringComparison.OrdinalIgnoreCase)
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

            var t2sFileName = GetT2sFileName(modelKey);
            var t2s = GetT2sPath(modelKey);
            var hasLocalT2s = IsValidLocalModelFile(t2s, t2sFileName);
            var s2aFileName = GetS2aFileName(modelKey);
            var s2a = GetS2aPath(modelKey);
            var hasLocalS2a = IsValidLocalModelFile(s2a, s2aFileName);

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
            psi.ArgumentList.Add(hasLocalT2s ? t2s : "auto");
            if (hasLocalS2a)
            {
                // Explicit rather than sibling discovery: the backend probes q4_k FIRST when left
                // to find the S2A on its own, and q4_k is the quant with the audible flow-matching
                // degradation this engine deliberately does not ship.
                psi.ArgumentList.Add("--codec-model");
                psi.ArgumentList.Add(s2a);
            }
            if (!hasLocalT2s || !hasLocalS2a)
            {
                // Fallback when SE's own download was skipped or is incomplete. Note the registry
                // set behind `-m auto` is the q4_k pair — degraded but intelligible, and better
                // than refusing to speak; the SE downloader replaces it with Q8_0 on next install.
                psi.ArgumentList.Add("--auto-download");
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString(CultureInfo.InvariantCulture));
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());
            // Required: the confucius4-tts backend clones only from the startup --voice path, and
            // without a reference the output is unintelligible by design. The w2v-BERT companion
            // next to the T2S GGUF is picked up automatically when this is set.
            psi.ArgumentList.Add("--voice");
            psi.ArgumentList.Add(voicePath);
            // Target language for the Chinese prompt template. Empty = no flag, which the backend
            // reads as English.
            if (!string.IsNullOrEmpty(languageArg))
            {
                psi.ArgumentList.Add("-l");
                psi.ArgumentList.Add(languageArg);
            }
            psi.ArgumentList.Add("--tts-steps");
            psi.ArgumentList.Add(odeSteps.ToString(CultureInfo.InvariantCulture));
            CrispAsrTtsProvenance.AddServerMarkingArgs(psi.ArgumentList, exe);

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (confucius4-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("Confucius4-TTS (CrispASR) server starting — "
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
            _serverModelKey = modelKey;
            _serverLanguageArg = languageArg;
            _serverOdeSteps = odeSteps;
            HookProcessExitOnce();

            // ~1.9 GB across four GGUFs on the warm path; first run may also be pulling those
            // gigabytes through --auto-download.
            var timeoutMinutes = hasLocalT2s && hasLocalS2a ? 10 : 30;
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
                    _serverLanguageArg = null;
                    _serverOdeSteps = 0;
                    throw new InvalidOperationException(
                        $"crispasr (confucius4-tts) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (confucius4-tts) did not report healthy within {timeoutMinutes} minutes. Last output: {lastOutput}"
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
    /// Stop the running crispasr (confucius4-tts) server if any, releasing GPU memory. Called by
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
        _serverLanguageArg = null;
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

        // 22.05 kHz mono: crispasr resamples the reference to 22.05 kHz for the S2A mel path and
        // to 16 kHz for the w2v-BERT/CAM++ encoders, so staging at 22.05 kHz makes the mel path
        // resample-free and leaves the encoders a clean downsample. No .txt sidecar: conditioning
        // is from the audio alone, so the reference transcript is never needed.
        try
        {
            var process = FfmpegGenerator.ConvertToMono22kHzWav(fileName, destinationFileName);
            if (!process.Start())
            {
                return false;
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Confucius4-TTS (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        return File.Exists(destinationFileName);
    }
}
