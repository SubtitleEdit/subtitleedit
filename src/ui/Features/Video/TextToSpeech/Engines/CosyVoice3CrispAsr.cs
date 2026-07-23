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
/// Alibaba CosyVoice3 0.5B (Dec 2025 release) run through the CrispASR runtime. LLM + flow-matching DiT
/// + HiFT vocoder architecture with 9 languages and 18 Mandarin dialects. Supports BOTH baked-in voice
/// presets (8 entries in <c>cosyvoice3-voices.gguf</c>) AND zero-shot cloning from a user-supplied 16 kHz
/// mono reference WAV (the s3tok speech tokenizer + CAMPPlus speaker encoder do the cloning).
///
/// The backend needs ALL of LLM + flow + hift + s3tok + campplus + voices co-located as siblings of
/// the LLM. <see cref="Nikse.SubtitleEdit.Logic.Download.CosyVoice3CrispAsrDownloadService"/> stages
/// every required GGUF into <c>CrispAsr/models/</c> with size + SHA-256 verification before first
/// synth — crispasr's own <c>--auto-download</c> only fires when <c>-m auto</c> is passed, which is
/// the slower fallback we keep for the case where any required file is missing at startup.
///
/// Sibling discovery in CrispASR 0.6.12 looks for the F16 companion filenames specifically
/// (<c>cosyvoice3-flow-f16.gguf</c>, <c>cosyvoice3-s3tok-f16.gguf</c>) — passing the Q8_0 / Q4_K
/// variants instead trips a "no flow GGUF found" startup error even with --codec-model. Both quants
/// therefore share the F16 companions; only the LLM differs (Q4_K ~1.6 GB total, F16 ~2.5 GB).
///
/// When the user picks a baked preset, <c>--voice &lt;preset_name&gt;</c> is passed at server startup.
/// When the user picks an imported WAV, <c>--voice /path/to/ref.wav --ref-text "transcription"</c>
/// is passed — both must be in place for cloning to work (no transcription → noisy output).
///
/// The repo on Hugging Face (cstr/cosyvoice3-0.5b-2512-GGUF) ships ~5 companion files:
///   - LLM           : cosyvoice3-llm-{q4_k,f16}.gguf  (we manage this)
///   - Flow          : cosyvoice3-flow-{q8_0,f16}.gguf  (crispasr --auto-download)
///   - HiFT vocoder  : cosyvoice3-hift-f16.gguf         (crispasr --auto-download)
///   - S3 tokenizer  : cosyvoice3-s3tok-{q4_k,f16}.gguf (crispasr --auto-download)
///   - CAMPPlus      : cosyvoice3-campplus-f16.gguf     (crispasr --auto-download)
///   - Voice bank    : cosyvoice3-voices.gguf           (crispasr --auto-download)
///
/// First-pass integration only stages the LLM file in SE's CrispASR/models folder; the rest are
/// fetched lazily by <c>--auto-download</c> into <c>~/.cache/crispasr/</c> on first synth. Keeping
/// a single primary file mirrors VibeVoice's layout and avoids a 6-file download-progress UI.
/// </summary>
public class CosyVoice3CrispAsr : ITtsEngine
{
    public string Name => "CosyVoice3 (CrispASR)";
    public string Description => "Alibaba CosyVoice3 0.5B with 9 languages + 18 zh dialects, via CrispASR";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    // Two LLM quants — Q4_K is the lightweight default (~1.6 GB total), F16 the reference (~2.5 GB).
    // The label total covers every required companion (flow / hift / s3tok / campplus / voices).
    public const string ModelKeyQ4K = "Q4_K (~1.6 GB total)";
    public const string ModelKeyF16 = "F16 (~2.5 GB total)";
    public const string DefaultModelKey = ModelKeyQ4K;

    public const string LlmQ4KFileName = "cosyvoice3-llm-q4_k.gguf";
    public const string LlmF16FileName = "cosyvoice3-llm-f16.gguf";
    public const string FlowF16FileName = "cosyvoice3-flow-f16.gguf";
    public const string HiftF16FileName = "cosyvoice3-hift-f16.gguf";
    public const string S3TokF16FileName = "cosyvoice3-s3tok-f16.gguf";
    public const string CampPlusF16FileName = "cosyvoice3-campplus-f16.gguf";
    public const string VoicesGgufFileName = "cosyvoice3-voices.gguf";

    public const string BackendName = "cosyvoice3-tts";

    /// <summary>
    /// All filenames that must be present in <see cref="GetSetModelsFolder"/> for the chosen
    /// quant. Both quants share the F16 companion set — only the LLM differs. Listed in this
    /// order so the download dialog progresses through them deterministically.
    /// </summary>
    public static string[] GetRequiredFileNames(string? modelKey) => new[]
    {
        ResolveModelKey(modelKey) == ModelKeyF16 ? LlmF16FileName : LlmQ4KFileName,
        FlowF16FileName,
        HiftF16FileName,
        S3TokF16FileName,
        CampPlusF16FileName,
        VoicesGgufFileName,
    };

    // Preset voice names baked into cosyvoice3-voices.gguf. Reading these dynamically would require
    // either a running server (/v1/voices) or parsing the voices GGUF, neither of which is worth the
    // complexity for a static list that only changes when cstr re-uploads the voice bank.
    public static readonly (string Display, string Preset)[] Presets =
    {
        ("Zero-shot (Mandarin)",   "zero_shot"),
        ("FLEURS English",         "fleurs-en"),
        ("FLEURS German",          "fleurs-de"),
        ("FLEURS Mandarin",        "fleurs-zh"),
        ("FLEURS Japanese",        "fleurs-ja"),
        ("FLEURS French",          "fleurs-fr"),
        ("FLEURS Spanish",         "fleurs-es"),
        ("FLEURS Korean",          "fleurs-ko"),
    };

    // Exact byte sizes from the HF tree API (size field, == on-disk bytes). Same truncation
    // guard as VibeVoice/IndexTTS — a partial file would otherwise crash the loader at server
    // startup with an opaque access violation.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [LlmQ4KFileName] = 383891200L,
        [LlmF16FileName] = 1289653952L,
        [FlowF16FileName] = 665140992L,
        [HiftF16FileName] = 41601888L,
        [S3TokF16FileName] = 484406944L,
        [CampPlusF16FileName] = 14153600L,
        [VoicesGgufFileName] = 665472L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey switch
        {
            ModelKeyF16 => ModelKeyF16,
            _ => ModelKeyQ4K,
        };
    }

    public static string GetLlmFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyF16 => LlmF16FileName,
        _ => LlmQ4KFileName,
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
    private static string? _serverModelKey;
    // CosyVoice3 takes the active voice via --voice at startup (either a preset name or a
    // reference WAV path). For cloned voices --ref-text is also baked in at startup. Any
    // change to either triggers a teardown + restart — same trade-off IndexTTS made.
    private static string? _serverVoiceArg;
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "CosyVoice3CrispAsr");
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
    /// CosyVoice3 clones at 16 kHz mono and requires a transcription sidecar, same shape as
    /// Qwen3 CustomVoice — so we resample to 16 kHz on seed and copy the .txt sidecar too.
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
                        var ffmpeg = FfmpegGenerator.ConvertToMono16kHzWav(src, dest);
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
                        Se.LogError(ex, $"CosyVoice3 (CrispASR): resample seed '{src}' failed; falling back to plain copy");
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
            Se.LogError(ex, "CosyVoice3 (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    public static string GetLlmPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetLlmFileName(modelKey));

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
                Se.LogError($"CosyVoice3 (CrispASR): removing wrong-sized local model file {destinationPath}");
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
            Se.LogError(ex, $"CosyVoice3 (CrispASR): cache seed copy failed for {fileName}");
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

        // Baked-in presets come first so the combo opens on a working default. Imported WAVs
        // (zero-shot clones) follow — they need a .txt sidecar with the transcription or the
        // server returns noise.
        foreach (var (display, preset) in Presets)
        {
            result.Add(new Voice(new CosyVoice3Voice(display, preset)));
        }

        var voicesFolder = GetSetVoicesFolder();
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                var refText = TryReadRefText(file);
                result.Add(new Voice(new CosyVoice3Voice(name, file, refText)));
            }
        }

        return Task.FromResult(result.ToArray());
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

            // Sidecars written by pre-filter seeding can be Wikimedia attribution blurbs, not
            // transcriptions - treat them as "no transcript" (same read-time filter Qwen3
            // CrispASR applies) so they neither poison ref-text nor suppress the prompt.
            var text = File.ReadAllText(sidecar).Trim();
            return Qwen3TtsCrispAsr.LooksLikeAttributionBlurb(text) ? string.Empty : text;
        }
        catch
        {
            return string.Empty;
        }
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
        if (voice.EngineVoice is not CosyVoice3Voice cosyVoice)
        {
            throw new ArgumentException("Voice is not a CosyVoice3Voice");
        }

        // Either Preset OR FilePath must be set. Preset wins if both are populated (defensive
        // — shouldn't happen with the constructors but guards against future drift).
        // Error results, not throws: a throw from Speak escapes the generate loop's per-segment
        // handling and aborts the entire run - reachable from cast rows and the review window,
        // where the main window's transcript prompt never had a chance to fire.
        var voiceArg = !string.IsNullOrEmpty(cosyVoice.Preset) ? cosyVoice.Preset : cosyVoice.FilePath;
        if (string.IsNullOrEmpty(voiceArg))
        {
            var error = "CosyVoice3 (CrispASR) requires a preset or an imported reference WAV. "
                + "Pick one of the baked presets (zero_shot / fleurs-*) or import a 16 kHz mono "
                + "reference WAV with an adjacent .txt transcription sidecar.";
            Se.WriteToolsLog("CosyVoice3 (CrispASR): " + error, true);
            return new TtsResult { Text = text, FileName = string.Empty, Error = true, ErrorMessage = error };
        }

        var isClone = string.IsNullOrEmpty(cosyVoice.Preset);
        if (isClone && string.IsNullOrEmpty(cosyVoice.RefText))
        {
            var error = "CosyVoice3 (CrispASR) zero-shot cloning requires a transcription. "
                + $"Add a .txt sidecar next to '{Path.GetFileName(cosyVoice.FilePath)}' with the "
                + "spoken text of the reference WAV.";
            Se.WriteToolsLog("CosyVoice3 (CrispASR): " + error, true);
            return new TtsResult { Text = text, FileName = string.Empty, Error = true, ErrorMessage = error };
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, voiceArg, cosyVoice.RefText, cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");

        var speed = Math.Clamp(Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrSpeed, 0.25, 4.0);
        // Deliberately NO `voice` / `ref_text` field: for cloning, voiceArg is an absolute WAV
        // path, which the server rejects outright (HTTP 400, "'voice' must not contain … path
        // separators" — path-traversal guard), so every clone synthesis failed. The backend gets
        // both voice (preset name or WAV path) and ref-text from the startup --voice/--ref-text
        // flags instead, and the server restarts on (voiceArg, ref-text) change — see
        // EnsureServerRunningAsync. Same bug family as MOSS-TTS #12757.
        var payload = new Dictionary<string, object>
        {
            ["input"] = text,
            ["response_format"] = "wav",
            ["speed"] = speed,
        };
        if (isClone)
        {
            // Attests the user's own imported reference; the server logs it for cloned synthesis.
            payload["consent_attestation"] = "I have the speaker's consent, or it is my own voice.";
            // Skip the audible AI-disclosure prefix CrispASR otherwise prepends to cloned audio;
            // SE surfaces the AI-generated nature in its UI. The inaudible watermark + C2PA
            // provenance metadata stay embedded regardless (defaults to true server-side).
            payload["spoken_disclaimer"] = false;
        }

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"CosyVoice3 (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={cosyVoice}, clone={isClone}, refTextLen={cosyVoice.RefText.Length}, textLen={text.Length})");

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

            var failMsg = $"CosyVoice3 (CrispASR) request failed — Voice: {cosyVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "CosyVoice3 (CrispASR) — the crispasr server crashed during synthesis."
                    : "CosyVoice3 (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"CosyVoice3 (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {cosyVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"CosyVoice3 (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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

    private static async Task EnsureServerRunningAsync(string modelKey, string voiceArg, string refText, CancellationToken ct)
    {
        // Server is keyed by (model, voice, ref-text) since all three are baked in at process
        // start. A switch from preset → clone, clone → different clone, or refText edit all
        // require a fresh server.
        bool MatchesCurrent() =>
            _serverProcess is { HasExited: false } && _serverPort != 0
            && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(_serverVoiceArg, voiceArg, StringComparison.OrdinalIgnoreCase)
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

            // crispasr's cosyvoice3-tts backend needs LLM + flow + hift + s3tok + campplus +
            // voices co-located as siblings of the LLM. We stage every file via SE's download
            // service into CrispAsr/models/, so when all are present we don't need
            // --auto-download. If any required file is missing, fall back to -m auto +
            // --auto-download which makes crispasr fetch the whole bundle into its own cache
            // (slower, no SE progress bar — happens silently behind the synth call).
            var llmFileName = GetLlmFileName(modelKey);
            var llmPath = GetLlmPath(modelKey);
            var allLocal = AreModelsInstalled(modelKey);

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
            psi.ArgumentList.Add(allLocal ? llmPath : "auto");
            if (!allLocal)
            {
                psi.ArgumentList.Add("--auto-download");
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());

            // --voice-dir lets the server resolve relative WAV filenames against our voices
            // folder. Doesn't hurt for preset-based voices (the preset name short-circuits the
            // filesystem lookup).
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());

            // Voice arg is either a baked preset name ("zero_shot", "fleurs-en", ...) or an
            // absolute path to an imported reference WAV. For WAV clones --ref-text carries
            // the spoken transcription; required by the s3tok speech tokenizer.
            psi.ArgumentList.Add("--voice");
            psi.ArgumentList.Add(voiceArg);
            if (!string.IsNullOrEmpty(refText))
            {
                psi.ArgumentList.Add("--ref-text");
                psi.ArgumentList.Add(refText);
            }

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (cosyvoice3-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("CosyVoice3 (CrispASR) server starting — "
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
            _serverVoiceArg = voiceArg;
            _serverRefText = refText;
            HookProcessExitOnce();

            // Local startup is fast; first-run --auto-download (~921 MB minimum, ~2.5 GB for F16)
            // needs a generous timeout for the silent companion fetch behind -m auto.
            var deadline = DateTime.UtcNow.AddMinutes(allLocal ? 5 : 30);
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
                    _serverVoiceArg = null;
                    _serverRefText = null;
                    throw new InvalidOperationException(
                        $"crispasr (cosyvoice3-tts) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (cosyvoice3-tts) did not report healthy within {(allLocal ? 5 : 30)} minutes. Last output: {lastOutput}"
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
        _serverVoiceArg = null;
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
    /// Imports <paramref name="fileName"/> as a CosyVoice3 zero-shot reference voice. When
    /// <paramref name="transcript"/> is non-empty it's written to the destination .txt sidecar
    /// directly (the s3tok tokenizer requires it). When empty, falls back to copying any .txt
    /// sidecar that already lives next to the source WAV.
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

        // CosyVoice3's s3tok speech tokenizer expects 16 kHz mono — resample on import so the
        // server doesn't have to upsample / convert on every synth call.
        try
        {
            var process = FfmpegGenerator.ConvertToMono16kHzWav(fileName, destinationFileName);
            if (!process.Start())
            {
                return false;
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "CosyVoice3 (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        if (!File.Exists(destinationFileName))
        {
            return false;
        }

        // Zero-shot cloning requires an accurate transcription. Caller-supplied transcript
        // wins; otherwise fall back to a sibling .txt next to the source WAV. Either way the
        // sidecar gets written to the destination folder so the engine can read it back later.
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
            Se.LogError(ex, "CosyVoice3 (CrispASR) voice import: failed to write .txt sidecar");
        }

        return true;
    }

    /// <summary>
    /// Writes <paramref name="transcript"/> to <c>&lt;voice&gt;.txt</c> next to the supplied
    /// voice WAV inside the voices folder. Used to retro-fit a transcription onto a voice
    /// that was already imported (or seeded from the qwen3-tts.cpp pack) without going through
    /// a full re-import. Returns false on IO failure.
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
            Se.LogError(ex, $"CosyVoice3 (CrispASR): failed to write ref-text sidecar for '{voiceWavPath}'");
            return false;
        }
    }
}
