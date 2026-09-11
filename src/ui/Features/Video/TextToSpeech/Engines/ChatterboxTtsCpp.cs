using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic;
using System;
using System.Collections.Concurrent;
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
/// Chatterbox TTS via the existing CrispASR install (shared with the speech-to-text feature).
/// Spawns `crispasr --server --backend chatterbox -m &lt;t3&gt; --codec-model &lt;s3gen&gt;` and POSTs
/// to the OpenAI-compatible /v1/audio/speech endpoint. Requires CrispASR v0.6.0 or newer.
/// The T3 + S3Gen GGUFs are downloaded into CrispASR/models/ (shared with the CrispASR
/// speech-to-text models) — `-m auto` is avoided because its codec auto-discovery only
/// finds *-s3gen-f16.gguf while it actually downloads the q8_0 variants.
/// Chatterbox has one baked default voice; "voices" listed beyond Default come from
/// WAVs imported via <see cref="ImportVoice"/>. The full reference-WAV path is sent per-request
/// as the `voice` field — runtime WAV cloning is wired upstream in CrispASR's chatterbox backend.
/// The Base model is the multilingual build (23 languages via <see cref="ChatterboxLanguages"/>,
/// sent as the per-request `language` field); Turbo is English-only.
/// </summary>
public class ChatterboxTtsCpp : ITtsEngine
{
    public string Name => "Chatterbox TTS (CrispASR)";
    public string Description => "via CrispASR (Base or Turbo model + voice cloning)";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => false;

    /// <summary>
    /// The only sample rate the chatterbox backend clones from without losing anything. 16 kHz
    /// keeps a partial path upstream, but SE always targets the full one.
    /// </summary>
    internal const int CloneReferenceSampleRate = 24000;

    /// <summary>WAVE_FORMAT_IEEE_FLOAT — the other reference format the backend accepts.</summary>
    private const int AudioFormatIeeeFloat = 3;

    /// <summary>A 44-byte canonical WAV header with no samples after it. Anything this small is not audio.</summary>
    private const long MinimumUsableWavLength = 44;

    public const string ModelKeyBase = ChatterboxTtsCppDownloadService.ModelKeyBase;
    public const string ModelKeyBaseF16 = ChatterboxTtsCppDownloadService.ModelKeyBaseF16;
    public const string ModelKeyBaseQ4K = ChatterboxTtsCppDownloadService.ModelKeyBaseQ4K;
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
    // The `voice` the running server last served, i.e. whose clone conditionals are installed in
    // it (bare file name, empty for the model's baked default voice). See NeedsRestartForVoice.
    private static string _serverVoiceKey = string.Empty;
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
        var variant = OperatingSystem.IsWindows() && folder != null
            ? DownloadHashManager.DetectCrispAsrWindowsVariant(folder)
            : null;
        var key = DownloadHashManager.ResolveCrispAsrExecutableKey(variant);
        if (key == null)
        {
            return true;
        }

        var hash = Sha256Util.ComputeSha256(exe);
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
        var modelsFolder = Se.CrispAsrModelsFolder;
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

    // A plain existence check is enough now that the Base pair is the versioned chatterbox-v3-*
    // artifact: the file name pins which weights it is, so nothing on disk can quietly become
    // something else. The byte-size probe this replaced existed only because upstream rebuilt
    // the unversioned names in place (English-only -> multilingual) and left the old files
    // looking installed; users still holding those get them deleted after the V3 download.
    public static bool AreModelsInstalled(string? modelKey = null)
    {
        return File.Exists(GetT3ModelPath(modelKey)) && File.Exists(GetS3GenModelPath(modelKey));
    }

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

    /// <summary>
    /// Base is the multilingual v3 pair in three quantizations — q8_0 (default), f16 and q4_k,
    /// all the same weights at different precision — plus the separate Turbo distillation.
    /// Measured 2026-08-12 on crispasr v0.8.28 (Apple M4), 27 runs over en/de/fr × 2 seeds ×
    /// built-in and cloned voice: every quantization returned the prompt verbatim through a
    /// parakeet-v3 ASR roundtrip and synthesis time was within noise of the others, so the
    /// only real trade-off is download size and peak RSS (~2.2 GB f16 / ~1.6 GB q8_0 /
    /// ~1.25 GB q4_k). q8_0 stays the default; f16 is offered for parity with upstream's
    /// reference tier, not because it measured better.
    /// </summary>
    public Task<string[]> GetModels() => Task.FromResult(ChatterboxTtsCppDownloadService.GetAllModelKeys());

    /// <summary>
    /// The multilingual Base models take a per-request language (23 languages, "[xx]" prompt
    /// token server-side); Turbo is an English-only distillation, so it gets "Auto" alone
    /// rather than an empty combo.
    /// </summary>
    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) =>
        Task.FromResult(ResolveModelKey(model) == ModelKeyTurbo
            ? new[] { ChatterboxLanguages.Auto }
            : ChatterboxLanguages.All);

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

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, ResolveVoiceKey(chatterboxVoice.FilePath), cancellationToken);

        // Off the calling thread because the repair shells out to ffmpeg. Deliberately not a hard
        // failure either: the check is stricter than the backend's (which still has a partial
        // 16 kHz path), so a reference that could not be re-encoded - no ffmpeg, say - is still
        // worth sending. If the backend does reject it, the 500 handler below turns "backend
        // returned empty audio" into an explanation.
        await Task.Run(() => EnsureCloneReferenceIsUsable(chatterboxVoice.FilePath), cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");
        var inputText = text;

        // Multilingual language selection (#13273-adjacent): a per-request field the server
        // turns into the [xx] prompt token. Only the Base model is multilingual — Turbo is an
        // English-only distillation, so no field is sent for it regardless of the pick.
        var languageArg = modelKey == ModelKeyTurbo
            ? string.Empty
            : ChatterboxLanguages.ResolveLanguageArg(language);

        // Language the reference recording is spoken in (CrispASR v0.8.29 / #329). The backend
        // needs BOTH sides before it goes cross-lingual, so without this an English reference
        // asked for German keeps the English accent. Only meaningful when actually cloning and
        // when a target language was asked for at all.
        var sourceLanguageArg = !string.IsNullOrEmpty(chatterboxVoice.FilePath) && !string.IsNullOrEmpty(languageArg)
            ? ChatterboxLanguages.ResolveSourceLanguageArg(TryReadReferenceTranscript(chatterboxVoice.FilePath))
            : string.Empty;

        if (string.IsNullOrEmpty(sourceLanguageArg)
            && !string.IsNullOrEmpty(chatterboxVoice.FilePath)
            && !string.IsNullOrEmpty(languageArg))
        {
            // Target language asked for, reference language unknowable: the clone keeps the
            // reference's accent and nothing in the 200 response says so. Same wording as the
            // CosyVoice3 path, which hit this first.
            Se.WriteToolsLog(
                $"Chatterbox TTS: target language '{languageArg}' requested for cloned voice "
                + $"'{chatterboxVoice}', but the language of its reference WAV is unknown. Synthesis "
                + "stays zero-shot and keeps the reference's accent - set \"Reference language\" in "
                + "the Chatterbox settings window to enable cross-lingual synthesis.",
                true);
        }

        var payload = BuildSpeakPayload(inputText, chatterboxVoice.FilePath, languageArg, sourceLanguageArg);

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Chatterbox TTS: POST {ServerBaseUrl}/v1/audio/speech (voice={chatterboxVoice}, model={modelKey}, textLen={text.Length}, language={(string.IsNullOrEmpty(languageArg) ? "(auto)" : languageArg)}, sourceLanguage={(string.IsNullOrEmpty(sourceLanguageArg) ? "(none)" : sourceLanguageArg)})");
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
            //
            // Let the process finish dying first: the fatal lines are still travelling through the
            // async stderr reader when the socket drops, so snapshotting straight away captured a
            // log that stopped at the last normal line and said nothing about the crash (#13572).
            var died = await WaitForServerExitAsync(TimeSpan.FromSeconds(2));
            var serverLog = SnapshotServerLog();
            var launchCommand = _serverLaunchCommand;
            if (died)
            {
                StopServerInternal();
            }
            var failMsg = $"Chatterbox TTS request failed - Voice: {chatterboxVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            string prefix;
            if (LooksLikeCudaBackendCrash(serverLog))
            {
                prefix = "Chatterbox TTS hit a CrispASR bug during synthesis (CUDA error in ggml_cuda_cpy). "
                         + "This is not specific to the CUDA build - the Vulkan build crashes at the identical "
                         + "point, so re-downloading a different build does not help. The known trigger is the "
                         + "first step after the cloned voice changes; Subtitle Edit restarts the server on a "
                         + "voice change to avoid it, so please report this at "
                         + "https://github.com/CrispStrobe/CrispASR/issues with the server log below.";
            }
            else if (LooksLikeUpstreamChatterboxCrash(serverLog))
            {
                prefix = "Chatterbox TTS hit a CrispASR runtime bug during synthesis (ggml tensor read out of bounds). "
                         + "This is an upstream issue — please file it at https://github.com/CrispStrobe/CrispASR/issues with the server log below. "
                         + "The crash reproduces on the CPU and Vulkan builds (chatterbox's T3 step runs on CPU regardless of the build).";
            }
            else
            {
                prefix = "Chatterbox TTS request failed — "
                         + (died ? "the crispasr server crashed during synthesis." : "the connection to the crispasr server was dropped.");
            }

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

                // The HTTP body only ever says "backend returned empty audio" - the reason the
                // backend produced none is in the server log, so name it (#13508).
                var prefix = LooksLikeCloneReferenceRejected(serverLog)
                    ? $"Chatterbox TTS could not use the reference voice \"{Path.GetFileName(chatterboxVoice.FilePath)}\" for cloning: "
                      + $"CrispASR needs a {CloneReferenceSampleRate / 1000} kHz mono WAV and re-encoding this one did not produce that. "
                      + "Re-import the voice, or convert it yourself with "
                      + $"`ffmpeg -i <input> -ar {CloneReferenceSampleRate} -ac 1 -c:a pcm_s16le <output>.wav`. "
                    : string.Empty;

                throw new InvalidOperationException(
                    prefix
                    + $"Chatterbox TTS synthesis failed ({(int)response.StatusCode}): {errorBody}"
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
    /// Builds the <c>/v1/audio/speech</c> JSON payload. Extracted so the cloning attestations are
    /// unit-testable without a running crispasr server.
    /// </summary>
    /// <remarks>
    /// Sends the bare file name as the <c>voice</c> field, not the path: the crispasr server
    /// rejects anything with a path separator ("'voice' must not contain '..', a leading '/' or
    /// '~', or path separators" — HTTP 400 invalid_voice), which used to fail every imported voice.
    /// The chatterbox backend then opens that name relative to its working directory, which is why
    /// the server is started in the voices folder (see <see cref="EnsureServerRunningAsync"/>). The
    /// name must keep its <c>.wav</c> extension — the backend does not append one, and it is also
    /// the exact test the server uses to decide a request is voice cloning, which makes Chatterbox
    /// the one SE engine that hard-requires the attestations. An empty path falls back to the
    /// model's baked default voice, which is not cloning and needs no attestation.
    /// </remarks>
    internal static Dictionary<string, object> BuildSpeakPayload(
        string inputText,
        string? voiceFilePath,
        string? languageCode = null,
        string? sourceLanguageCode = null)
    {
        var payload = new Dictionary<string, object>
        {
            ["input"] = inputText,
            ["response_format"] = "wav",
        };

        if (!string.IsNullOrEmpty(languageCode))
        {
            payload["language"] = languageCode;
        }

        // Only alongside a reference and a target language: on its own the field says what a
        // recording that is not being cloned from is spoken in, which is nothing.
        if (!string.IsNullOrEmpty(sourceLanguageCode)
            && !string.IsNullOrEmpty(voiceFilePath)
            && !string.IsNullOrEmpty(languageCode))
        {
            payload["source_lang"] = sourceLanguageCode;
        }

        var voiceKey = ResolveVoiceKey(voiceFilePath);
        if (!string.IsNullOrEmpty(voiceKey))
        {
            payload["voice"] = voiceKey;
            CrispAsrTtsProvenance.AddSpeechAttestations(payload);
        }

        return payload;
    }

    /// <summary>
    /// The <c>voice</c> field for a reference WAV path — the bare file name, empty for the model's
    /// baked default voice. Also identifies which clone the running server has conditionals
    /// installed for (see <see cref="EnsureServerRunningAsync"/>).
    /// </summary>
    internal static string ResolveVoiceKey(string? voiceFilePath)
    {
        return string.IsNullOrEmpty(voiceFilePath) ? string.Empty : Path.GetFileName(voiceFilePath);
    }

    /// <summary>
    /// The spoken text of a reference WAV, read from the <c>.txt</c> sidecar the other cloning
    /// engines use, or null when there is none. Chatterbox itself never needs the transcript —
    /// it clones from the audio alone — so this exists only to detect what language the
    /// reference is in for <c>source_lang</c>.
    /// </summary>
    internal static string? TryReadReferenceTranscript(string? referenceWavPath)
    {
        if (string.IsNullOrEmpty(referenceWavPath))
        {
            return null;
        }

        try
        {
            var sidecar = Path.ChangeExtension(referenceWavPath, ".txt");
            if (!File.Exists(sidecar))
            {
                return null;
            }

            var text = File.ReadAllText(sidecar).Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }
        catch
        {
            // A sidecar we cannot read is the same as no sidecar - detection just declines.
            return null;
        }
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

    /// <summary>
    /// True when the running server has already served a cloned voice and a different one is now
    /// asked for.
    /// </summary>
    /// <remarks>
    /// Installing a second voice's conditionals into a live chatterbox server crashes it at the
    /// first autoregressive step of that request (#13572). Both reported crashes have the same
    /// shape - previous line fine with voice A, <c>atomic native WAV clone (B.wav ...) - all 5 conds
    /// installed</c>, then death at <c>chatterbox[ar]: step=0</c> - and the CUDA and Vulkan builds
    /// die at the identical token, so this is the shared voice-load path and not a GPU backend bug.
    /// A server that loads the voice at startup is fine, so the fix is to make every voice the first
    /// one its server sees. Costs a model reload per voice change, which an actor-voice cast pays on
    /// every actor change; that is still cheaper than the crash it replaces, since #13795's retry
    /// already restarts the server after throwing away the line's synthesis.
    /// <para>
    /// Switching to the baked default voice restarts too: that swaps the conditionals back through
    /// the same upstream code. Going the other way - default → clone - does not, because the clone
    /// is then the first one this server installs. Narrow this once upstream fixes it.
    /// </para>
    /// </remarks>
    private static bool NeedsRestartForVoice(string voiceKey) => NeedsRestartForVoice(_serverVoiceKey, voiceKey);

    /// <inheritdoc cref="NeedsRestartForVoice(string)"/>
    internal static bool NeedsRestartForVoice(string loadedVoiceKey, string requestedVoiceKey)
    {
        return !string.IsNullOrEmpty(loadedVoiceKey)
               && !string.Equals(loadedVoiceKey, requestedVoiceKey, StringComparison.Ordinal);
    }

    /// <summary>
    /// Starts the crispasr server for <paramref name="modelKey"/>, or reuses the running one.
    /// Restarts it when <paramref name="voiceKey"/> differs from the voice it last served — see
    /// <see cref="NeedsRestartForVoice"/>.
    /// </summary>
    private static async Task EnsureServerRunningAsync(string modelKey, string voiceKey, CancellationToken ct)
    {
        if (_serverProcess is { HasExited: false }
            && _serverPort != 0
            && _serverModelKey == modelKey
            && !NeedsRestartForVoice(voiceKey))
        {
            _serverVoiceKey = voiceKey;
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false }
                && _serverPort != 0
                && _serverModelKey == modelKey
                && !NeedsRestartForVoice(voiceKey))
            {
                _serverVoiceKey = voiceKey;
                return;
            }

            // Server not running — or running with a different model variant, or with another
            // voice's clone conditionals installed. (Re)start.
            if (_serverProcess != null)
            {
                if (_serverProcess is { HasExited: false } && NeedsRestartForVoice(voiceKey))
                {
                    Se.WriteToolsLog(
                        "Chatterbox TTS: restarting the crispasr server before switching cloned voice "
                        + $"'{_serverVoiceKey}' -> '{(string.IsNullOrEmpty(voiceKey) ? "(default)" : voiceKey)}' "
                        + "- installing a second voice's conditionals into a live server crashes the "
                        + "chatterbox backend at the first autoregressive step (#13572).");
                }

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
            CrispAsrTtsProvenance.AddServerMarkingArgs(psi.ArgumentList, exe);

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
            _serverVoiceKey = voiceKey;
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
                    _serverVoiceKey = string.Empty;
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

    /// <summary>
    /// A CUDA-backend fault during chatterbox synth (#13572):
    /// <code>
    /// chatterbox[ar]: step=0 tok=3704
    /// CUDA error: invalid argument
    ///   current device: 0, in function ggml_cuda_cpy at ggml/src/ggml-cuda/cpy.cu:474
    ///   cudaMemcpyAsync(src1_ddc, src0_ddc, ggml_nbytes(src0), cudaMemcpyDeviceToDevice, main_stream)
    /// </code>
    /// Every report on that issue crashes at the first autoregressive step of the request that
    /// switches to a different cloned voice, right after the new conditionals are installed.
    /// <b>This is not a CUDA-specific bug</b>: the Vulkan build dies at the identical point - same
    /// voice, same conditionals, same <c>prefill 163</c>, same <c>step=0 tok=3704</c> - so the fault
    /// is in the shared voice-load path and only its symptom is backend-shaped (a device-to-device
    /// copy failing with "invalid argument" is what a buffer that is not device memory looks like).
    /// <see cref="EnsureServerRunningAsync"/> now restarts the server on a clone-voice change to
    /// stay off that path. Matched on the ggml function name plus the generic "CUDA error" banner so
    /// a fault in another op is still recognised as a CUDA one.
    /// </summary>
    internal static bool LooksLikeCudaBackendCrash(string output)
    {
        return output.Contains("CUDA error", StringComparison.Ordinal)
               || output.Contains("ggml_cuda_cpy", StringComparison.Ordinal);
    }

    internal static bool LooksLikeCloneReferenceRejected(string output)
    {
        // The chatterbox backend clones from the reference WAV natively and only accepts
        // 24 kHz (full path) or 16 kHz (partial M2+M3 path) mono. Anything else used to
        // degrade to the baked default voice; since v0.8.x it produces no audio at all and
        // the server answers HTTP 500 "synthesis failed (backend returned empty audio)":
        //   chatterbox: native WAV cloning failed.
        //     Tried 24 kHz: sample rate 48000 not supported (need 24000); pre-convert ...
        //     Tried 16 kHz: sample rate 48000 not supported (need 16000); pre-convert ...
        return output.Contains("native WAV cloning failed", StringComparison.Ordinal)
            || output.Contains("not supported (need 24000)", StringComparison.Ordinal);
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

    /// <summary>
    /// Waits up to <paramref name="timeout"/> for a server believed to have crashed to actually
    /// exit, and for the last of its output to reach <c>_serverLog</c>. Returns whether it exited.
    /// </summary>
    /// <remarks>
    /// <see cref="Process.WaitForExitAsync"/> waits for the redirected stdout/stderr readers to
    /// reach EOF as well as for the process itself - that drain is the whole point here. The crash
    /// report in #13572 lost the "CUDA error: invalid argument" line that named the fault, because
    /// the socket dropped a moment before the reader delivered it. Bounded by the token so a
    /// reader held open by an inherited handle cannot stall a generation run.
    /// </remarks>
    private static async Task<bool> WaitForServerExitAsync(TimeSpan timeout)
    {
        var p = _serverProcess;
        if (p == null)
        {
            return false;
        }

        try
        {
            using var cts = new CancellationTokenSource(timeout);
            await p.WaitForExitAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            // Timed out. The process may still have exited with only the drain outstanding, in
            // which case this is a crash with a possibly-truncated log rather than a live server.
            return SafeHasExited(p);
        }
        catch
        {
            // Disposed or never started - nothing to drain.
            return false;
        }
    }

    private static bool SafeHasExited(Process p)
    {
        try
        {
            return p.HasExited;
        }
        catch
        {
            return false;
        }
    }

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverModelKey = null;
        _serverVoiceKey = string.Empty;
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
        // reference WAV is 24 kHz mono PCM16/F32 — anything else used to silently fall
        // back to the default voice and now fails synthesis outright. Always resample on
        // import via ffmpeg so the saved WAV is in the right shape regardless of what the
        // user picked.
        return ConvertToCloneReferenceWav(fileName, destinationFileName);
    }

    /// <summary>
    /// Re-encodes the reference WAV in place when it is not in the shape the chatterbox backend
    /// can clone from. <see cref="ImportVoice"/> already converts on the way in, but that is not
    /// the only way a WAV reaches the voices folder: the folder is documented and users copy
    /// files into it directly, and voices imported by older versions predate the conversion.
    /// Such a file fails every synthesis with an opaque HTTP 500 (#13508), so the shape is
    /// re-checked before each request — a header read, so the cost is one open of ~44 bytes and
    /// the conversion only ever runs once per file.
    /// </summary>
    /// <returns>
    /// false only when there is a reference that needs converting and the conversion failed -
    /// on this call, or on an earlier one against the same file contents. No reference at all
    /// (the baked default voice) and a reference that has gone missing are both the server's
    /// business, not a conversion failure.
    /// </returns>
    internal static bool EnsureCloneReferenceIsUsable(string? voiceFilePath)
    {
        // No reference means the baked default voice, which is not cloning and needs no WAV.
        if (string.IsNullOrEmpty(voiceFilePath))
        {
            return true;
        }

        FileStamp stamp;
        try
        {
            var info = new FileInfo(voiceFilePath);
            if (!info.Exists)
            {
                // A reference that has gone missing is the server's business, not a conversion
                // failure.
                return true;
            }

            stamp = new FileStamp(info.LastWriteTimeUtc.Ticks, info.Length);
        }
        catch (Exception exception)
        {
            Se.WriteToolsLog($"Chatterbox TTS: could not stat the reference voice \"{voiceFilePath}\" ({exception.Message}) - sending it as it is");
            return true;
        }

        // This runs per line, so a repair that cannot succeed - no ffmpeg on the machine, a WAV
        // ffmpeg will not decode - must be attempted once, not once for every line of the
        // subtitle. Checked ahead of the header read as well: that read logs when it fails, and
        // a tools-log entry per line is its own kind of runaway. A repair that succeeds needs no
        // guard at all - the file then passes the header check below and never comes back here.
        //
        // Keyed on the file's stamp, not its path alone: the voices folder is documented and the
        // user may well fix the WAV in place while the session is running, and a path-only guard
        // would keep refusing the repaired file until restart.
        if (FailedCloneReferenceRepairs.TryGetValue(voiceFilePath, out var failedStamp))
        {
            if (failedStamp == stamp)
            {
                return false;
            }

            FailedCloneReferenceRepairs.TryRemove(voiceFilePath, out _);
        }

        if (IsCloneReadyReferenceWav(voiceFilePath))
        {
            return true;
        }

        Se.WriteToolsLog($"Chatterbox TTS: reference voice \"{Path.GetFileName(voiceFilePath)}\" is not "
            + $"{CloneReferenceSampleRate / 1000} kHz mono - re-encoding it in place before synthesis");

        if (ConvertToCloneReferenceWav(voiceFilePath, voiceFilePath))
        {
            return true;
        }

        // The conversion writes a temp file and only moves it on success, so a failure leaves the
        // reference exactly as it was - the stamp read above still describes it.
        FailedCloneReferenceRepairs[voiceFilePath] = stamp;
        return false;
    }

    /// <summary>Last write time and length, enough to tell a replaced reference WAV from the old one.</summary>
    private readonly record struct FileStamp(long Ticks, long Length);

    /// <summary>
    /// Reference WAVs whose in-place repair has already been tried and failed, against the file
    /// contents that failed, so it is not retried for every remaining line - but a file the user
    /// replaces or repairs mid-session gets a fresh attempt.
    /// </summary>
    private static readonly ConcurrentDictionary<string, FileStamp> FailedCloneReferenceRepairs =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// True when the WAV is exactly what the chatterbox backend clones from: 24 kHz mono,
    /// PCM16 or 32-bit float. Everything else — including a file that is not a RIFF/WAVE at
    /// all, or one whose header cannot be parsed — counts as needing a conversion, which is
    /// the safe direction: converting an acceptable file again costs one ffmpeg run, while
    /// letting an unacceptable one through fails the synthesis.
    /// </summary>
    internal static bool IsCloneReadyReferenceWav(string fileName)
    {
        try
        {
            using var stream = File.OpenRead(fileName);
            var header = new WaveHeader2(stream);
            if (header.ChunkId != "RIFF" || header.Format != "WAVE")
            {
                return false;
            }

            if (header.SampleRate != CloneReferenceSampleRate || header.NumberOfChannels != 1)
            {
                return false;
            }

            // An extensible header carries the right samples but not the plain fmt-16 header a
            // strict WAV reader expects - normalize it like any other mismatch (the safe
            // direction; common recorders emit extensible even for mono 16-bit).
            if (header.IsExtensibleFormat)
            {
                return false;
            }

            return (header.AudioFormat == WaveHeader2.AudioFormatPcm && header.BitsPerSample == 16)
                   || (header.AudioFormat == AudioFormatIeeeFloat && header.BitsPerSample == 32);
        }
        catch (Exception ex)
        {
            Se.WriteToolsLog($"Chatterbox TTS: could not read the WAV header of \"{fileName}\" ({ex.Message}) - treating it as needing a conversion");
            return false;
        }
    }

    /// <summary>
    /// Resamples to 24 kHz mono PCM16 via <see cref="VoiceSeedHelper"/> (which owns the ffmpeg
    /// traps: a run that never exits, and an exit code nobody checked). Always converts into a
    /// temp file first — ffmpeg cannot read and write the same path, which is exactly what the
    /// in-place repair asks for, and a half-written file must never end up in the voices folder
    /// where it would be listed as a voice.
    /// </summary>
    /// <returns>false when nothing usable was produced; the destination is then left untouched.</returns>
    private static bool ConvertToCloneReferenceWav(string sourceFileName, string destinationFileName)
    {
        var tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
        try
        {
            // copyOnFailure: false — the whole point here is that a verbatim copy seeds a voice
            // the backend refuses to clone from. Better no voice than a broken one.
            VoiceSeedHelper.CopyOrResample(sourceFileName, tempFileName, CloneReferenceSampleRate, "Chatterbox TTS", copyOnFailure: false);

            // A header with no samples after it is not audio, however cleanly ffmpeg exited.
            if (!File.Exists(tempFileName) || new FileInfo(tempFileName).Length <= MinimumUsableWavLength)
            {
                Se.LogError($"Chatterbox TTS: no usable {CloneReferenceSampleRate / 1000} kHz mono audio came out of converting \"{sourceFileName}\".");
                return false;
            }

            File.Move(tempFileName, destinationFileName, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Chatterbox TTS voice conversion failed (ffmpeg).");
            return false;
        }
        finally
        {
            try
            {
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
            catch
            {
                // best effort
            }
        }
    }
}
