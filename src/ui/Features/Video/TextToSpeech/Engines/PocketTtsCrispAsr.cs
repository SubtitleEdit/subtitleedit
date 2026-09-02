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
/// Kyutai Pocket TTS (100M) run through the CrispASR runtime. By far the smallest of the
/// voice-cloning engines wired up in SE — one 124-365 MB GGUF per language vs ~870 MB for
/// IndexTTS — and the fastest (continuous-latent AR at 12.5 Hz + one-step LSD flow head +
/// Mimi decoder; measured RTF ~0.1 on an M-class CPU). Six languages, one checkpoint each:
/// English, German, Spanish, Italian, Portuguese, and Kyutai's French 24-layer preview.
///
/// The model produces near-silence without voice conditioning, so like the other cloning
/// engines the voice combo lists imported reference WAVs and there is no built-in default
/// voice. Cloning needs no reference transcript (the Mimi encoder + speaker projection
/// condition from audio alone), which also makes per-line voice cloning a plain file copy.
///
/// License: CC-BY-4.0 plus Kyutai's gated-use conditions — see
/// https://huggingface.co/kyutai/pocket-tts (the GGUF repo itself is ungated).
/// </summary>
public class PocketTtsCrispAsr : ITtsEngine, IPerLineCloneEngine
{
    public string Name => "Pocket TTS (CrispASR)";
    public string Description => "Kyutai Pocket TTS 100M with voice cloning — 6 languages, via CrispASR";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => true;

    // One checkpoint per language (upstream's defaults: English F16, the distilled 6L
    // languages at Q8_0, and the undistilled French 24L preview at Q8_0). English also gets
    // the Q8_0 for slow machines. The label carries the language, so HasLanguageParameter
    // stays false — picking a model IS picking the language.
    public const string ModelKeyEnglishF16 = "English F16 (~219 MB)";
    public const string ModelKeyEnglishQ8_0 = "English Q8_0 (~124 MB)";
    public const string ModelKeyGermanQ8_0 = "German Q8_0 (~124 MB)";
    public const string ModelKeySpanishQ8_0 = "Spanish Q8_0 (~124 MB)";
    public const string ModelKeyItalianQ8_0 = "Italian Q8_0 (~124 MB)";
    public const string ModelKeyPortugueseQ8_0 = "Portuguese Q8_0 (~124 MB)";
    public const string ModelKeyFrenchQ8_0 = "French 24L Q8_0 (~365 MB)";
    public const string DefaultModelKey = ModelKeyEnglishF16;

    public const string EnglishF16FileName = "pocket-tts-english-f16.gguf";
    public const string EnglishQ8_0FileName = "pocket-tts-english-q8_0.gguf";
    public const string GermanQ8_0FileName = "pocket-tts-german-q8_0.gguf";
    public const string SpanishQ8_0FileName = "pocket-tts-spanish-q8_0.gguf";
    public const string ItalianQ8_0FileName = "pocket-tts-italian-q8_0.gguf";
    public const string PortugueseQ8_0FileName = "pocket-tts-portuguese-q8_0.gguf";
    public const string FrenchQ8_0FileName = "pocket-tts-french_24l-q8_0.gguf";

    // Exact byte sizes on cstr/pocket-tts-GGUF (HF LFS metadata). Used to reject truncated
    // files that crispasr's --auto-download may have left behind — same guard as the other
    // CrispASR-backed engines.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [EnglishF16FileName] = 219234528L,
        [EnglishQ8_0FileName] = 123632992L,
        [GermanQ8_0FileName] = 123633408L,
        [SpanishQ8_0FileName] = 123634464L,
        [ItalianQ8_0FileName] = 123633664L,
        [PortugueseQ8_0FileName] = 123634592L,
        [FrenchQ8_0FileName] = 364587520L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.PocketTtsCrispAsrModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey switch
        {
            ModelKeyEnglishQ8_0 => ModelKeyEnglishQ8_0,
            ModelKeyGermanQ8_0 => ModelKeyGermanQ8_0,
            ModelKeySpanishQ8_0 => ModelKeySpanishQ8_0,
            ModelKeyItalianQ8_0 => ModelKeyItalianQ8_0,
            ModelKeyPortugueseQ8_0 => ModelKeyPortugueseQ8_0,
            ModelKeyFrenchQ8_0 => ModelKeyFrenchQ8_0,
            _ => ModelKeyEnglishF16,
        };
    }

    public static string GetModelFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyEnglishQ8_0 => EnglishQ8_0FileName,
        ModelKeyGermanQ8_0 => GermanQ8_0FileName,
        ModelKeySpanishQ8_0 => SpanishQ8_0FileName,
        ModelKeyItalianQ8_0 => ItalianQ8_0FileName,
        ModelKeyPortugueseQ8_0 => PortugueseQ8_0FileName,
        ModelKeyFrenchQ8_0 => FrenchQ8_0FileName,
        _ => EnglishF16FileName,
    };

    /// <summary>
    /// The explicit per-language backend names added in CrispASR v0.8.31 — passing these
    /// (rather than the base "pocket-tts" plus a language flag) pins both the checkpoint's
    /// tokenizer and its auto-download route, so a mismatched -m file fails loudly.
    /// </summary>
    public static string GetBackendName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyGermanQ8_0 => "pocket-tts-de",
        ModelKeySpanishQ8_0 => "pocket-tts-es",
        ModelKeyItalianQ8_0 => "pocket-tts-it",
        ModelKeyPortugueseQ8_0 => "pocket-tts-pt",
        ModelKeyFrenchQ8_0 => "pocket-tts-fr",
        _ => "pocket-tts",
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
    // Tracks the model key (= language + quant) the running server was started with. Unlike
    // indextts, the pocket-tts backend resolves the per-request `voice` (a bare stem) against
    // --voice-dir and caches the latents per reference, so a voice change needs NO restart —
    // only a model change does. Verified against the v0.8.31 server (issue #255 upstream).
    private static string? _serverModelKey;
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "PocketTtsCrispAsr");
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
    /// One-time best-effort seed of WAV reference voices from qwen3-tts.cpp's voices folder.
    /// Pocket TTS conditions through the 24 kHz Mimi encoder, and the qwen3-tts.cpp voice pack
    /// ships at 16 kHz mono — resample on seed via ffmpeg, same path ImportVoice uses, so the
    /// reference isn't upsampled on every synth call. Same rationale as IndexTTS (CrispASR).
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

                // When ffmpeg cannot do it, seeding at 16 kHz beats skipping the voice.
                VoiceSeedHelper.CopyOrResample(src, dest, 24000, "Pocket TTS (CrispASR)");
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Pocket TTS (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    public static string GetModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetModelFileName(modelKey));

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetModelPath(modelKey), GetModelFileName(modelKey));

    /// <summary>
    /// Path crispasr's --auto-download writes GGUFs to. Mirrors the IndexTTS (CrispASR) helper
    /// so the SE-side downloader can adopt already-cached files instead of re-pulling.
    /// </summary>
    public static string GetCrispAsrCacheFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "crispasr");

    /// <summary>
    /// Best-effort copy of <paramref name="fileName"/> from <see cref="GetCrispAsrCacheFolder"/>
    /// into SE's models folder. See <see cref="Qwen3TtsCrispAsr.TrySeedModelFromCrispAsrCache"/>
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
                Se.LogError($"Pocket TTS (CrispASR): removing wrong-sized local model file {destinationPath}");
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
            Se.LogError(ex, $"Pocket TTS (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public const string PerLineReferencePrefix = "se-per-line-";

    private static bool IsStagedPerLineReference(string fileName) =>
        Path.GetFileName(fileName).StartsWith(PerLineReferencePrefix, StringComparison.OrdinalIgnoreCase);

    public async Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>();

        // Voice cloning only — the model produces near-silence without voice conditioning, so
        // there is no built-in default voice. The combo is empty until the user imports a
        // reference WAV (or the qwen3-tts.cpp voice seed runs above).
        // Off the UI thread: GetSetVoicesFolder does one-time reference-WAV seeding through
        // ffmpeg, and this is awaited from SelectedEngineChanged on the dispatcher.
        var voicesFolder = await Task.Run(GetSetVoicesFolder);
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                // The per-line clone's own references live here too (the backend resolves bare
                // stems against --voice-dir); they belong to one line of one run, not in a combo.
                if (IsStagedPerLineReference(file))
                {
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new PocketTtsVoice(name, file)));
            }
        }

        return result.ToArray();
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[]
    {
        ModelKeyEnglishF16,
        ModelKeyEnglishQ8_0,
        ModelKeyGermanQ8_0,
        ModelKeySpanishQ8_0,
        ModelKeyItalianQ8_0,
        ModelKeyPortugueseQ8_0,
        ModelKeyFrenchQ8_0,
    });

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    /// <summary>
    /// <see cref="IPerLineCloneEngine"/>: the pocket-tts backend resolves the request's bare
    /// voice stem against its --voice-dir, so the clip is staged in there and the voice's
    /// FilePath points at the copy — that is the lookup key <see cref="Speak"/> sends. Unlike
    /// Qwen3, cloning needs no transcript (the Mimi encoder conditions from audio alone), so
    /// staging never fails on a missing sidecar and every cut clip is usable.
    /// </summary>
    public Voice? MakePerLineCloneVoice(string clipFileName, string voiceName) =>
        StagePerLineReference(clipFileName) is { } staged
            ? new Voice(new PocketTtsVoice(voiceName, staged))
            : null;

    /// <summary>
    /// The staged copy in the voices folder, which is the only reference this engine ever
    /// speaks from — exporting it is what lets an imported session be re-dubbed on a machine
    /// that no longer has the video.
    /// </summary>
    public string? GetPerLineReferenceClip(Voice voice) =>
        voice.EngineVoice is PocketTtsVoice pocketVoice && !string.IsNullOrEmpty(pocketVoice.FilePath)
            ? pocketVoice.FilePath
            : null;

    /// <summary>
    /// <see cref="IPerLineCloneEngine"/>: see <see cref="ClearStagedPerLineReferences"/>.
    /// </summary>
    public void ResetStagedPerLineReferences() => ClearStagedPerLineReferences();

    public static string? StagePerLineReference(string clipFileName)
    {
        try
        {
            if (string.IsNullOrEmpty(clipFileName) || !File.Exists(clipFileName))
            {
                return null;
            }

            // An exported session carries the staged name, so re-staging it on import would
            // otherwise pile a second prefix on top — once per round trip.
            var baseName = Path.GetFileNameWithoutExtension(clipFileName);
            if (baseName.StartsWith(PerLineReferencePrefix, StringComparison.OrdinalIgnoreCase))
            {
                baseName = baseName.Substring(PerLineReferencePrefix.Length);
            }

            var stagedFileName = Path.Combine(GetSetVoicesFolder(), PerLineReferencePrefix + baseName + ".wav");
            File.Copy(clipFileName, stagedFileName, overwrite: true);
            return stagedFileName;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Pocket TTS (CrispASR): staging the per-line reference '{clipFileName}' failed");
            return null;
        }
    }

    /// <summary>
    /// Removes every staged per-line reference, leaving the user's imported voices alone.
    /// Called when a per-line run starts, so a run over a shorter subtitle cannot leave the
    /// previous run's extra lines lying in the voices folder.
    /// </summary>
    public static void ClearStagedPerLineReferences()
    {
        try
        {
            var voicesFolder = Path.Combine(GetSetFolder(), "voices");
            if (!Directory.Exists(voicesFolder))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(voicesFolder, PerLineReferencePrefix + "*"))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // A reference still open (the server may be reading it) is left for next time.
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Pocket TTS (CrispASR): clearing the staged per-line references failed");
        }
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
        if (voice.EngineVoice is not PocketTtsVoice pocketVoice)
        {
            throw new ArgumentException("Voice is not a PocketTtsVoice");
        }

        if (string.IsNullOrEmpty(pocketVoice.FilePath))
        {
            throw new InvalidOperationException(
                "Pocket TTS (CrispASR) requires a reference voice WAV — the model produces "
                + "near-silence without voice conditioning. Import one via the voice settings, "
                + "then pick it in the voice combo (3-10 s of clean speech works best).");
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, pocketVoice.FilePath, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");
        var inputText = text;

        // OpenAI-compatible /v1/audio/speech payload. Unlike indextts, the pocket-tts backend
        // honours a per-request `voice`: a bare stem (no path separators, no .wav — the server's
        // path-traversal guard rejects paths) is resolved against the startup --voice-dir and
        // the reference latents are cached per stem, so switching voices needs no server
        // restart. Verified against the v0.8.31 server; upstream issue #255.
        var speed = Math.Clamp(Se.Settings.Video.TextToSpeech.PocketTtsCrispAsrSpeed, 0.25, 4.0);
        var payload = new Dictionary<string, object>
        {
            ["input"] = inputText,
            ["response_format"] = "wav",
            ["speed"] = speed,
            ["voice"] = Path.GetFileNameWithoutExtension(pocketVoice.FilePath),
        };

        // Attests the user's own imported reference and the AI-disclosure duty; see
        // CrispAsrTtsProvenance. Skipped when voice cloning has not been accepted in settings.
        CrispAsrTtsProvenance.AddSpeechAttestations(payload);

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Pocket TTS (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={pocketVoice}, textLen={text.Length})");

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

            var failMsg = $"Pocket TTS (CrispASR) request failed — Voice: {pocketVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Pocket TTS (CrispASR) — the crispasr server crashed during synthesis."
                    : "Pocket TTS (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"Pocket TTS (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {pocketVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Pocket TTS (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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
        // Voice changes ride on the per-request stem (see Speak), so only a model change —
        // a different language checkpoint, hence a different --backend AND -m — restarts the
        // server. The startup --voice still carries the currently selected reference as the
        // fallback conditioning for a request whose stem cannot be resolved.
        if (_serverProcess is { HasExited: false } && _serverPort != 0
            && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0
                && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase))
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
            var modelFileName = GetModelFileName(modelKey);
            var modelPath = GetModelPath(modelKey);
            var hasLocalModel = IsValidLocalModelFile(modelPath, modelFileName);

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
            psi.ArgumentList.Add(GetBackendName(modelKey));
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(hasLocalModel ? modelPath : "auto");
            if (!hasLocalModel)
            {
                psi.ArgumentList.Add("--auto-download");
                // The Kyutai weights are behind a restricted-licence tag in crispasr's registry,
                // so its managed download refuses without this. SE's own downloader (the normal
                // path) fetches the ungated GGUF mirror directly and never needs it.
                psi.ArgumentList.Add("--accept-license");
                psi.ArgumentList.Add("pocket-tts-terms");
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());
            // /v1/audio/speech resolves the request's bare voice stem against this folder.
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());
            // Fallback conditioning for a request without a resolvable stem — without ANY
            // reference the model produces near-silence, never a normal default voice.
            psi.ArgumentList.Add("--voice");
            psi.ArgumentList.Add(voicePath);
            CrispAsrTtsProvenance.AddServerMarkingArgs(psi.ArgumentList, exe);

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (pocket-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("Pocket TTS (CrispASR) server starting — "
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

            // First-run auto-download is at most ~365 MB (French), and the model itself loads
            // in seconds — much lighter than the other CrispASR TTS engines.
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
                    _serverModelKey = null;
                    throw new InvalidOperationException(
                        $"crispasr (pocket-tts) exited during startup (code {exitCode}). Output: {tail}"
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
                $"crispasr (pocket-tts) did not report healthy within {(hasLocalModel ? 5 : 15)} minutes. Last output: {lastOutput}"
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
    /// Stop the running crispasr (pocket-tts) server if any, releasing GPU memory. Called by
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
        _serverModelKey = null;
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

        // The Mimi encoder runs at 24 kHz. Resample on import via ffmpeg regardless of source
        // format. No .txt sidecar needed (cloning conditions from audio alone).
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
            Se.LogError(ex, "Pocket TTS (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        return File.Exists(destinationFileName);
    }
}
