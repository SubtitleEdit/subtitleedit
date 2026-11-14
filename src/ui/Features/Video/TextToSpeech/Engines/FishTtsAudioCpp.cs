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
/// Fish Audio S2 Pro run through the audio.cpp runtime — the same pure C++/ggml install
/// <see cref="IndexTts25AudioCpp"/> uses (see <see cref="AudioCppRuntime"/>), so the binaries
/// are downloaded once and shared. Zero-shot voice cloning from a 10-30 s reference across
/// 80+ languages, with inline prosody and emotion control through the text itself. 44.1 kHz
/// output.
///
/// Like the other audio.cpp engines, the server honours a per-request <c>voice_ref</c>, so
/// switching voice does NOT restart the server — only a model or backend change does — which
/// is also what makes per-line cloning ("Clone from video") free here. The
/// model auto-handles the language of the input text, so there is no language combo. A
/// <c>.txt</c> sidecar next to the reference WAV (the transcript convention the other cloning
/// engines share) is passed as <c>reference_text</c>, which improves clone fidelity but is
/// optional.
///
/// Licence note: the audio.cpp binaries are Apache-2.0, but the S2 Pro *weights* are under
/// the Fish Audio Research License (research and non-commercial), which has to be accepted by
/// the user before the first download — see <see cref="IsLicenseAccepted"/>.
/// </summary>
public class FishTtsAudioCpp : ITtsEngine, IPerLineCloneEngine
{
    public string Name => "Fish Audio S2 Pro (audio.cpp)";
    public string Description => "Fish Audio S2 Pro voice cloning in 80+ languages, via audio.cpp";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => true;

    // Q8_0 is the default: same 44.1 kHz output as BF16 at 3.6 GB less on disk.
    public const string ModelKeyQ8_0 = "Q8_0 (~5.9 GB)";
    public const string ModelKeyBf16 = "BF16 (~9.5 GB)";
    public const string DefaultModelKey = ModelKeyQ8_0;

    public const string ModelQ8_0FileName = "fish-audio-s2-pro-q8_0.gguf";
    public const string ModelBf16FileName = "fish-audio-s2-pro-bf16.gguf";

    /// <summary>Family name audio.cpp registers Fish Audio S2 Pro under.</summary>
    public const string FamilyName = "fish_audio";

    /// <summary>Model id used in the generated server config and in each request body.</summary>
    private const string ServerModelId = "fishs2pro";

    /// <summary>
    /// Exact byte sizes on the audio-cpp/audio.cpp-gguf HuggingFace repo. A truncated GGUF is
    /// the single most common failure here — a download that dies partway leaves a file the
    /// loader rejects with "GGUF tensor data range is out of bounds", so size is checked
    /// before the server is ever started. Same guard as <see cref="IndexTts25AudioCpp"/>.
    /// </summary>
    private static readonly Dictionary<string, long[]> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ8_0FileName] = [6317915552L, 6317911232L],
        [ModelBf16FileName] = [10229282400L, 10229278080L],
    };

    /// <summary>
    /// Bumping this re-prompts every user with the licence window. Keep it in step with the
    /// summary text in <see cref="LicenseDefinition"/>.
    /// </summary>
    public const string LicenseVersion = "fish-audio-research-license-2026-09";

    public static bool IsLicenseAccepted() =>
        string.Equals(
            Se.Settings.Video.TextToSpeech.FishTtsAudioCppLicenseAccepted,
            LicenseVersion,
            StringComparison.Ordinal);

    public static void AcceptLicense() =>
        Se.Settings.Video.TextToSpeech.FishTtsAudioCppLicenseAccepted = LicenseVersion;

    /// <summary>
    /// What the first-run licence gate shows. The weights are research / non-commercial —
    /// meaningfully different terms from open source, which is exactly why the gate exists.
    /// </summary>
    public static ModelLicenseDefinition LicenseDefinition { get; } = new(
        DialogTitle: "Fish Audio S2 Pro - model license",
        Header: "The Fish Audio S2 Pro model has its own license",
        Intro: "The audio.cpp engine is Apache-2.0, but the Fish Audio S2 Pro model weights are "
            + "licensed under the Fish Audio Research License, which is not open source. Please "
            + "read the main points before downloading.",
        SummaryPoints: new[]
        {
            "The model weights are free of charge for research and non-commercial use. Any commercial use needs a separate written license from Fish Audio (business@fish.audio).",
            "If you pass the model or anything derived from it to someone else, the license notice must travel with it: \"This model is licensed under the Fish Audio Research License, Copyright © 39 AI, INC. All Rights Reserved.\"",
            "The model is provided as is, with no warranty, and Fish Audio accepts no liability for what is generated.",
            "You are responsible for having the right to clone any voice you use as a reference. The model does not check consent.",
        },
        LicenseUrl: "https://huggingface.co/fishaudio/s2-pro/blob/main/LICENSE.md",
        ModelPageUrl: "https://huggingface.co/fishaudio/s2-pro",
        AcceptCheckBoxText: "I have read and accept the Fish Audio Research License",
        Accept: AcceptLicense);

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.FishTtsAudioCppModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey == ModelKeyBf16 ? ModelKeyBf16 : ModelKeyQ8_0;
    }

    public static string GetModelFileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyBf16 ? ModelBf16FileName : ModelQ8_0FileName;

    /// <summary>
    /// What goes in <c>reference_text</c> when the reference clip's transcript is unknown: the
    /// server refuses a missing or empty transcript, accepts this, and clones from it well.
    /// </summary>
    internal const string UnknownReferenceTextPlaceholder = " ";

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(10),
    };

    private static readonly AudioCppServer Server = new("Fish Audio S2 Pro", "Fish Audio S2 Pro", "fish_audio", HttpClient);

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

        var folder = Path.Combine(Se.TextToSpeechFolder, "FishTtsAudioCpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// audio.cpp is pointed at a directory, not a file, so the GGUF gets its own folder under
    /// the shared models root: <c>&lt;data&gt;/audio.cpp/models/Fish-Audio-S2-Pro-GGUF/</c>.
    /// </summary>
    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "models", "Fish-Audio-S2-Pro-GGUF");
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
    /// clones only and has no built-in voices. Transcripts matter more here than anywhere:
    /// S2 Pro refuses to clone without reference_text. The pack ships at 16 kHz; the S2 Pro
    /// codec runs at 44.1 kHz, so resample on seed.
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
                VoiceSeedHelper.CopyOrResample(src, dest, 44100, "Fish Audio S2 Pro (audio.cpp)");

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
            Se.LogError(ex, "Fish Audio S2 Pro (audio.cpp): voice seeding from the shared voice pack failed");
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

    // S2 Pro auto-handles the language of the input text (80+ languages), so there is no
    // language pick.
    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) =>
        Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    /// <summary>
    /// <see cref="IPerLineCloneEngine"/>: the server takes <c>voice_ref</c> as a path per request,
    /// so the voice simply points at the cut clip - nothing is staged into this engine's own
    /// folders. audio.cpp resamples the reference itself, so the 24 kHz clip is used as cut.
    /// A clip with no transcript beside it (no original-language subtitle loaded, so what the
    /// video says is unknown) is still a usable reference: <see cref="Speak"/> sends the blank
    /// placeholder transcript for it, which clones fine.
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
                "Fish Audio S2 Pro (audio.cpp) requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "10-30 s of clean speech works best.");
        }

        if (!IsLicenseAccepted())
        {
            throw new InvalidOperationException(
                "The Fish Audio S2 Pro model licence (Fish Audio Research License) has not been accepted.");
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        // audio.cpp's OpenAI-style speech payload. Voice cloning goes through `voice_ref`,
        // honoured per request — no server restart when the user switches voice.
        var options = new Dictionary<string, object>();

        // The transcript of the reference WAV, from the shared .txt sidecar convention. The
        // option itself is NOT optional, whatever upstream's docs say: audio.cpp answers a
        // voice_ref without reference_text, or with an empty string, with HTTP 500 "Fish Audio
        // prepare with inline reference audio requires reference_text option" (verified
        // against the real server). A single space passes that check and the clone comes out
        // fine, so an unknown transcript - a per-line clip cut from the video with no
        // original-language subtitle loaded - is sent as that placeholder. What must NEVER be
        // sent in its place is the text being spoken: the transcript and the target text share
        // one prompt, and when they are the same line the model concludes the clip already
        // says it and replays the clip instead of speaking the line (#14480, Polish subtitles
        // dubbed as the video's own language).
        var referenceText = ChatterboxTtsCpp.TryReadReferenceTranscript(indexVoice.FilePath);
        if (string.IsNullOrWhiteSpace(referenceText))
        {
            referenceText = UnknownReferenceTextPlaceholder;
            Se.WriteToolsLog($"Fish Audio S2 Pro (audio.cpp): no transcript beside reference '{indexVoice.FilePath}', cloning with a blank reference_text");
        }

        options["reference_text"] = referenceText;

        var payload = new Dictionary<string, object>
        {
            ["model"] = ServerModelId,
            ["input"] = text,
            ["response_format"] = "wav",
            ["voice_ref"] = new Dictionary<string, object>
            {
                ["type"] = "path",
                ["path"] = indexVoice.FilePath,
            },
        };

        if (options.Count > 0)
        {
            payload["options"] = options;
        }

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Fish Audio S2 Pro (audio.cpp): POST {Server.BaseUrl}/v1/audio/speech (voice={indexVoice}, textLen={text.Length})");

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

            var failMsg = $"Fish Audio S2 Pro (audio.cpp) request failed — Voice: {indexVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LocalServerHelper.LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Fish Audio S2 Pro (audio.cpp) — the audiocpp_server process crashed during synthesis."
                    : "Fish Audio S2 Pro (audio.cpp) request failed — the connection to audiocpp_server was dropped.")
                + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                + LocalServerHelper.LaunchCmdSuffix(launchCommand),
                ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await LocalServerHelper.SafeReadErrorAsync(response, cancellationToken);
                var serverLog = Server.SnapshotLog();
                var launchCommand = Server.LaunchCommand;
                var errMsg = $"Fish Audio S2 Pro (audio.cpp) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {indexVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Fish Audio S2 Pro (audio.cpp) synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand));
            }

            await using var fileStream = File.Create(outputFileName);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
    }

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

        var configPath = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "fish-s2-server.json");
        File.WriteAllText(
            configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Se.WriteToolsLog($"Fish Audio S2 Pro (audio.cpp): server config written to {configPath} (model={modelKey}, backend={backend})");
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
    /// routes to. S2 Pro cannot clone without the transcript, so getting it written as the
    /// .txt sidecar at import time is what makes the voice usable at all.
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

        // audio.cpp resamples the reference itself, but importing at 44.1 kHz mono matches
        // the S2 Pro codec's native rate, so the reference is only resampled once, here.
        try
        {
            var process = FfmpegGenerator.ConvertToMono44kHzWav(fileName, destinationFileName);
            if (!process.Start())
            {
                return false;
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Fish Audio S2 Pro (audio.cpp) voice import failed (ffmpeg conversion).");
            return false;
        }

        if (!File.Exists(destinationFileName))
        {
            return false;
        }

        // Caller-supplied transcript wins; otherwise fall back to a sibling .txt next to the
        // source WAV. Either way the sidecar lands next to the imported voice, where Speak
        // reads it back as reference_text.
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
            Se.LogError(ex, "Fish Audio S2 Pro (audio.cpp) voice import: failed to write .txt sidecar");
        }

        return true;
    }
}
