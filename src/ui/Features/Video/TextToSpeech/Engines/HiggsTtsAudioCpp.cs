using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Higgs Audio v3 TTS 4B (Boson AI) run through the audio.cpp runtime — the same pure C++/ggml
/// install <see cref="IndexTts25AudioCpp"/> uses (see <see cref="AudioCppRuntime"/>), so the
/// binaries are downloaded once and shared. Zero-shot voice cloning across 100+ languages —
/// by far the broadest language coverage of SE's cloning engines — with inline control over
/// emotion, style and prosody through the text itself. 24 kHz output.
///
/// Like the IndexTTS 2.5 engine, the server honours a per-request <c>voice_ref</c>, so
/// switching voice does NOT restart the server — only a model or backend change does — which
/// is also what makes per-line cloning ("Clone from video") free here. The
/// model auto-detects the language of the input text, so there is no language combo. A
/// <c>.txt</c> sidecar next to the reference WAV (the transcript convention the other cloning
/// engines share) is passed as <c>reference_text</c>, which improves clone fidelity but is
/// optional.
///
/// Licence note: the audio.cpp binaries are Apache-2.0, but the Higgs Audio v3 *weights* are
/// under Boson AI's research and non-commercial licence, which has to be accepted by the user
/// before the first download — see <see cref="IsLicenseAccepted"/>.
/// </summary>
public class HiggsTtsAudioCpp : ITtsEngine, IPerLineCloneEngine
{
    public string Name => "Higgs Audio v3 (audio.cpp)";
    public string Description => "Higgs Audio v3 (Boson AI) voice cloning in 100+ languages, via audio.cpp";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => true;

    // Q8_0 is the default: same 24 kHz output as BF16 at 3.2 GB less on disk.
    public const string ModelKeyQ8_0 = "Q8_0 (~4.7 GB)";
    public const string ModelKeyBf16 = "BF16 (~7.9 GB)";
    public const string DefaultModelKey = ModelKeyQ8_0;

    public const string ModelQ8_0FileName = "higgs-audio-v3-tts-4b-q8_0.gguf";
    public const string ModelBf16FileName = "higgs-audio-v3-tts-4b-bf16.gguf";

    /// <summary>Family name audio.cpp registers Higgs Audio v3 TTS under.</summary>
    public const string FamilyName = "higgs_audio_tts";

    /// <summary>Model id used in the generated server config and in each request body.</summary>
    private const string ServerModelId = "higgstts3";

    /// <summary>
    /// Exact byte sizes on the audio-cpp/audio.cpp-gguf HuggingFace repo. A truncated GGUF is
    /// the single most common failure here — a download that dies partway leaves a file the
    /// loader rejects with "GGUF tensor data range is out of bounds", so size is checked
    /// before the server is ever started. Same guard as <see cref="IndexTts25AudioCpp"/>.
    /// </summary>
    private static readonly Dictionary<string, long[]> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ8_0FileName] = [5095359136L, 5095354048L],
        [ModelBf16FileName] = [8501592736L, 8501587648L],
    };

    /// <summary>
    /// Bumping this re-prompts every user with the licence window. Keep it in step with the
    /// summary text in <see cref="LicenseDefinition"/>.
    /// </summary>
    public const string LicenseVersion = "boson-higgs-tts3-nc-license-2026-09";

    public static bool IsLicenseAccepted() =>
        string.Equals(
            Se.Settings.Video.TextToSpeech.HiggsTtsAudioCppLicenseAccepted,
            LicenseVersion,
            StringComparison.Ordinal);

    public static void AcceptLicense() =>
        Se.Settings.Video.TextToSpeech.HiggsTtsAudioCppLicenseAccepted = LicenseVersion;

    /// <summary>
    /// What the first-run licence gate shows. The weights are research / non-commercial with a
    /// carve-out for monetized creator content — meaningfully different terms from open source,
    /// which is exactly why the gate exists.
    /// </summary>
    public static ModelLicenseDefinition LicenseDefinition { get; } = new(
        DialogTitle: "Higgs Audio v3 - model license",
        Header: "The Higgs Audio v3 model has its own license",
        Intro: "The audio.cpp engine is Apache-2.0, but the Higgs Audio v3 model weights are "
            + "licensed by Boson AI for research and non-commercial use. Please read the main "
            + "points before downloading.",
        SummaryPoints: new[]
        {
            "The model weights are licensed for research and non-commercial use. Production deployment, hosted APIs, embedding in a product, or reselling the model or fine-tunes of it needs a separate commercial license from Boson AI.",
            "Digital creators may use the model in monetized content, provided Boson AI's Higgs Audio is credited prominently in the audio or in the accompanying text.",
            "Voice cloning without the speaker's consent, impersonation, fraud, election deception, biometric surveillance, and any unlawful use are prohibited.",
            "The model is provided as is, with no warranty, and Boson AI accepts no liability for what is generated.",
            "You are responsible for having the right to clone any voice you use as a reference. The model does not check consent.",
        },
        LicenseUrl: "https://huggingface.co/bosonai/higgs-tts-3-4b/blob/main/LICENSE",
        ModelPageUrl: "https://huggingface.co/bosonai/higgs-tts-3-4b",
        AcceptCheckBoxText: "I have read and accept the Boson Higgs TTS 3 Research and Non-Commercial License",
        Accept: AcceptLicense);

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.HiggsTtsAudioCppModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey == ModelKeyBf16 ? ModelKeyBf16 : ModelKeyQ8_0;
    }

    public static string GetModelFileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyBf16 ? ModelBf16FileName : ModelQ8_0FileName;

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(10),
    };

    private static readonly AudioCppServer Server = new("Higgs Audio v3", "Higgs Audio v3", "higgs_audio_tts", HttpClient);

    public Task<bool> IsInstalled(string? region) => Task.FromResult(File.Exists(AudioCppRuntime.GetServerExecutable()));

    public override string ToString() => Name;

    /// <summary>
    /// Per-engine working folder under TextToSpeech (voices, synthesis output). The audio.cpp
    /// binaries are shared (<see cref="AudioCppRuntime"/>); this is not.
    /// </summary>
    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "HiggsTtsAudioCpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// audio.cpp is pointed at a directory, not a file, so the GGUF gets its own folder under
    /// the shared models root: <c>&lt;data&gt;/audio.cpp/models/Higgs-Audio-v3-TTS-4B-GGUF/</c>.
    /// </summary>
    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "models", "Higgs-Audio-v3-TTS-4B-GGUF");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetVoicesFolder()
    {
        var folder = Path.Combine(GetSetFolder(), "voices");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        SeedVoicesFromQwen3TtsCppIfEmpty(folder);
        NormalizeVoiceTranscriptsOnce(folder);
        return folder;
    }

    private static bool _voiceSeedAttempted;
    private static bool _voicesNormalized;

    /// <summary>
    /// One-time per session: drop unusable ref-text sidecars (the shared pack ships Wikimedia
    /// attribution blurbs, not transcripts) and backfill missing transcriptions from the
    /// sibling OmniVoice pack. This engine passes the .txt sidecar as reference_text, so a
    /// blurb there would condition the clone on text nobody spoke — same cleanup CosyVoice3
    /// and MOSS-TTS run.
    /// </summary>
    private static void NormalizeVoiceTranscriptsOnce(string voicesFolder)
    {
        if (_voicesNormalized)
        {
            return;
        }
        _voicesNormalized = true;

        Qwen3TtsCrispAsr.NormalizeVoiceTranscripts(voicesFolder);
    }

    /// <summary>
    /// One-time best-effort seed of reference WAVs (plus their transcript sidecars) from the
    /// shared Qwen3 voice pack, so the voice combo is not empty on first run — this engine
    /// clones only and has no built-in voices. The pack ships at 16 kHz; Higgs clones from
    /// 24 kHz, so resample on seed rather than letting the server upsample per request.
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
                VoiceSeedHelper.CopyOrResample(src, dest, 24000, "Higgs Audio v3 (audio.cpp)");

                // Bring the transcript along; NormalizeVoiceTranscriptsOnce then drops the
                // attribution blurbs and backfills real transcripts where a sibling has them.
                var sidecar = Path.ChangeExtension(src, ".txt");
                var sidecarDest = Path.ChangeExtension(dest, ".txt");
                if (File.Exists(sidecar) && !File.Exists(sidecarDest) && File.Exists(dest))
                {
                    File.Copy(sidecar, sidecarDest);
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Higgs Audio v3 (audio.cpp): voice seeding from the shared voice pack failed");
        }
    }

    public static string GetModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetModelFileName(modelKey));

    public static bool IsValidLocalModelFile(string path, string fileName) =>
        AudioCppRuntime.IsValidModelFile(path, ExpectedFileSizes.GetValueOrDefault(fileName));

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetModelPath(modelKey), GetModelFileName(modelKey));

    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus() => AudioCppRuntime.GetEngineUpdateStatus();

    public async Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>();

        // Voice cloning only — the combo stays empty until the user imports a reference WAV.
        var voicesFolder = await Task.Run(GetSetVoicesFolder);
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new IndexTtsVoice(name, file)));
            }
        }

        return result.ToArray();
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ8_0, ModelKeyBf16 });

    // Higgs auto-detects the language of the input text (100+ languages), so there is no
    // language pick — unlike IndexTTS 2.5, where auto only separates Han script from English.
    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) =>
        Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    /// <summary>
    /// <see cref="IPerLineCloneEngine"/>: the server takes <c>voice_ref</c> as a path per request,
    /// so the voice simply points at the cut clip - nothing is staged into this engine's own
    /// folders. audio.cpp resamples the reference itself, so the 24 kHz clip is used as cut.
    /// </summary>
    public Voice? MakePerLineCloneVoice(string clipFileName, string voiceName) =>
        new Voice(new IndexTtsVoice(voiceName, clipFileName));

    /// <summary>The clip's own path, which is exactly what the voice carries.</summary>
    public string? GetPerLineReferenceClip(Voice voice) =>
        voice.EngineVoice is IndexTtsVoice indexVoice && !string.IsNullOrEmpty(indexVoice.FilePath)
            ? indexVoice.FilePath
            : null;

    /// <summary>
    /// <see cref="IPerLineCloneEngine"/>: nothing is ever staged (the voice points straight at
    /// the clip), so there is nothing to clear between runs.
    /// </summary>
    public void ResetStagedPerLineReferences()
    {
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
        if (voice.EngineVoice is not IndexTtsVoice indexVoice)
        {
            throw new ArgumentException("Voice is not an IndexTtsVoice");
        }

        if (string.IsNullOrEmpty(indexVoice.FilePath))
        {
            throw new InvalidOperationException(
                "Higgs Audio v3 (audio.cpp) requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "3-10 s of clean speech works best.");
        }

        if (!IsLicenseAccepted())
        {
            throw new InvalidOperationException(
                "The Higgs Audio v3 model licence (Boson research and non-commercial license) has not been accepted.");
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        // audio.cpp's OpenAI-style speech payload. Voice cloning goes through `voice_ref`,
        // honoured per request — no server restart when the user switches voice.
        var options = new Dictionary<string, object>();

        // The transcript of the reference WAV, when the shared .txt sidecar convention has
        // one. Optional — Higgs clones from the audio alone — but a transcript improves clone
        // fidelity, so pass it when it is there.
        var referenceText = ChatterboxTtsCpp.TryReadReferenceTranscript(indexVoice.FilePath);
        if (!string.IsNullOrEmpty(referenceText))
        {
            options["reference_text"] = referenceText;
        }

        // The clone ends the way the reference ends (see CloneReferenceTail), so condition on a
        // copy whose tail is trimmed, faded and padded with silence. Falls back to the file as is.
        var referencePath = await CloneReferenceTail.PrepareAsync(indexVoice.FilePath, Name, cancellationToken);
        var referencePrepared = !string.Equals(referencePath, indexVoice.FilePath, StringComparison.Ordinal);

        var payload = new Dictionary<string, object>
        {
            ["model"] = ServerModelId,
            ["input"] = text,
            ["response_format"] = "wav",
            ["voice_ref"] = new Dictionary<string, object>
            {
                ["type"] = "path",
                ["path"] = referencePath,
            },
        };

        if (options.Count > 0)
        {
            payload["options"] = options;
        }

        var body = JsonSerializer.Serialize(payload);
        Se.WriteToolsLog($"Higgs Audio v3 (audio.cpp): POST {Server.BaseUrl}/v1/audio/speech (voice={indexVoice}, textLen={text.Length}, preparedReference={referencePrepared})");

        for (var attempt = 1; ; attempt++)
        {
            // HttpClient disposes the request content with the request, so a retry needs a new one.
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.PostAsync($"{Server.BaseUrl}/v1/audio/speech", content, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                var serverLog = Server.SnapshotLog();
                var launchCommand = Server.LaunchCommand;
                var died = Server.HasExited;
                if (died)
                {
                    Server.Stop();
                }

                var failMsg = $"Higgs Audio v3 (audio.cpp) request failed — Voice: {indexVoice}, Text: {text}, "
                    + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand);
                Se.LogError(ex, failMsg);
                Se.WriteToolsLog(failMsg);

                throw new InvalidOperationException(
                    (died
                        ? "Higgs Audio v3 (audio.cpp) — the audiocpp_server process crashed during synthesis."
                        : "Higgs Audio v3 (audio.cpp) request failed — the connection to audiocpp_server was dropped.")
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand),
                    ex);
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await LocalServerHelper.SafeReadErrorAsync(response, cancellationToken);
                    if (attempt < MaxSynthesisAttempts && IsRunawayGenerationError(errorBody))
                    {
                        // The model never emitted its end-of-audio token and ran to max_tokens - a
                        // rare sampling accident (1 in ~370 requests measured), not a property of
                        // the line. No seed is sent, so a retry samples afresh and normally succeeds.
                        Se.WriteToolsLog($"Higgs Audio v3 (audio.cpp): generation ran to max_tokens without an end-of-audio token (attempt {attempt} of {MaxSynthesisAttempts}) — retrying. Voice: {indexVoice}, Text: {text}");
                        continue;
                    }

                    var serverLog = Server.SnapshotLog();
                    var launchCommand = Server.LaunchCommand;
                    var errMsg = $"Higgs Audio v3 (audio.cpp) server error {(int)response.StatusCode} {response.StatusCode} — "
                        + $"Voice: {indexVoice}, Text: {text}, RequestJson: {body}, "
                        + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                        + LocalServerHelper.LaunchCmdSuffix(launchCommand);
                    Se.LogError(errMsg);
                    Se.WriteToolsLog(errMsg);
                    throw new InvalidOperationException(
                        $"Higgs Audio v3 (audio.cpp) synthesis failed ({(int)response.StatusCode}): {errorBody}"
                        + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                        + LocalServerHelper.LaunchCmdSuffix(launchCommand));
                }

                await using var fileStream = File.Create(outputFileName);
                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await contentStream.CopyToAsync(fileStream, cancellationToken);
            }

            break;
        }

        return new TtsResult(outputFileName, text);
    }

    /// <summary>One retry for a runaway generation; a second failure is reported.</summary>
    private const int MaxSynthesisAttempts = 2;

    /// <summary>
    /// audio.cpp's "Higgs TTS generation reached max_tokens before EOC": the sampler never drew
    /// the end-of-audio token. Matched loosely so a rewording upstream still counts.
    /// </summary>
    internal static bool IsRunawayGenerationError(string? errorBody) =>
        !string.IsNullOrEmpty(errorBody)
        && errorBody.Contains("max_tokens", StringComparison.OrdinalIgnoreCase)
        && errorBody.Contains("EOC", StringComparison.Ordinal);

    private static Task EnsureServerRunningAsync(string modelKey, CancellationToken ct) =>
        Server.EnsureRunningAsync(
            modelKey,
            () => (GetModelPath(modelKey), IsValidLocalModelFile(GetModelPath(modelKey), GetModelFileName(modelKey))),
            (port, backend) => WriteServerConfig(port, backend, modelKey),
            ct);

    /// <summary>
    /// Writes the audio.cpp server config next to the binary. lazy_load keeps startup instant;
    /// the model is read on first use and then stays resident until the server is stopped.
    /// </summary>
    /// <remarks>
    /// The model entry names the GGUF file, not its folder. Given a folder, audio.cpp picks
    /// model.gguf or the sole *.gguf and refuses a folder holding several - so a user who had
    /// downloaded both quantizations could not start the server until one was moved out of
    /// sight (#14480). The file path is unambiguous, and auxiliary paths still resolve against
    /// its parent.
    /// </remarks>
    private static string WriteServerConfig(int port, string backend, string modelKey)
    {
        var config = new Dictionary<string, object>
        {
            ["host"] = "127.0.0.1",
            ["port"] = port,
            ["backend"] = backend,
            ["threads"] = Math.Max(1, Math.Min(8, Environment.ProcessorCount / 2)),
            ["lazy_load"] = true,
            ["models"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["id"] = ServerModelId,
                    ["family"] = FamilyName,
                    ["path"] = GetModelPath(modelKey),
                    ["task"] = "tts",
                    ["mode"] = "offline",
                },
            },
        };

        var configPath = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "higgs-tts-server.json");
        File.WriteAllText(
            configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Se.WriteToolsLog($"Higgs Audio v3 (audio.cpp): server config written to {configPath} (model={modelKey}, backend={backend})");
        return configPath;
    }

    /// <summary>
    /// Stop the audio.cpp server if running, releasing the loaded model's working set.
    /// audio.cpp never unloads a model on its own once loaded, so this is the only way the
    /// memory comes back.
    /// </summary>
    public static void StopServer() => Server.Stop();

    public bool ImportVoice(string fileName) => ImportVoice(fileName, string.Empty);

    /// <summary>
    /// Import with the reference transcript — the overload <see cref="VoiceCloneImporter"/>
    /// routes to. Higgs clones from the audio alone, but a transcript improves clone fidelity,
    /// so it is kept as the .txt sidecar Speak passes as reference_text.
    /// </summary>
    public bool ImportVoice(string fileName, string transcript)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        var voicesFolder = GetSetVoicesFolder();
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var destinationFileName = VoiceCloneImporter.GetUniqueDestinationFileName(voicesFolder, baseName);

        // audio.cpp resamples the reference itself, but importing at 24 kHz mono keeps the
        // reference clean and matches the model's own output rate.
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
            Se.LogError(ex, "Higgs Audio v3 (audio.cpp) voice import failed (ffmpeg conversion).");
            return false;
        }

        if (!File.Exists(destinationFileName))
        {
            return false;
        }

        // Caller-supplied transcript wins; otherwise fall back to a sibling .txt next to the
        // source WAV.
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
            Se.LogError(ex, "Higgs Audio v3 (audio.cpp) voice import: failed to write .txt sidecar");
        }

        return true;
    }
}
