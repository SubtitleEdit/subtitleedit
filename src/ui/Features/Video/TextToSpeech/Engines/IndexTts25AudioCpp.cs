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
/// IndexTTS-2.5 (IndexTeam / Bilibili) run through the audio.cpp runtime — a pure C++/ggml
/// engine, no Python. Zero-shot voice cloning in Chinese, English, Japanese, Spanish and
/// Arabic, with emotion control and speaking-rate control.
///
/// Unlike <see cref="IndexTtsCrispAsr"/> (IndexTTS-1.5 via CrispASR), audio.cpp's server
/// honours a per-request <c>voice_ref</c>, so switching voice does NOT restart the server —
/// only a model or backend change does — which is also what makes per-line cloning ("Clone
/// from video") free here. Everything else follows the same server-mode shape:
/// one persistent process on a loopback port, OpenAI-style POST /v1/audio/speech.
///
/// The model is a single self-describing GGUF (the package spec is embedded), so no sidecar
/// config/tokenizer files are needed next to it.
///
/// Licence note: the audio.cpp binaries are Apache-2.0, but the IndexTTS-2.5 *weights* are
/// under the bilibili Model Use License, which is not OSI-approved and has to be accepted by
/// the user before the first download — see <see cref="IsLicenseAccepted"/>.
/// </summary>
public class IndexTts25AudioCpp : ITtsEngine, IPerLineCloneEngine
{
    public string Name => "IndexTTS 2.5 (audio.cpp)";
    public string Description => "IndexTTS-2.5 (Bilibili / IndexTeam) voice cloning in 5 languages, via audio.cpp";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => true;

    // The Q8_0 GGUF is the default: same 22.05 kHz output as F16 at 1 GB less on disk. The
    // "orig" dtype build (7.3 GB) is deliberately not offered — it is a debugging artifact.
    public const string ModelKeyQ8_0 = "Q8_0 (~3.3 GB)";
    public const string ModelKeyF16 = "F16 (~4.2 GB)";
    public const string DefaultModelKey = ModelKeyQ8_0;

    public const string ModelQ8_0FileName = "index-tts2_5-q8_0.gguf";
    public const string ModelF16FileName = "index-tts2_5-f16.gguf";

    /// <summary>Family name audio.cpp registers IndexTTS2/2.5 under; 2.5 is a variant of it.</summary>
    public const string FamilyName = "index_tts2";

    /// <summary>Model id used in the generated server config and in each request body.</summary>
    private const string ServerModelId = "indextts25";

    /// <summary>
    /// Exact byte sizes on the audio-cpp/audio.cpp-gguf HuggingFace repo. A truncated GGUF is
    /// the single most common failure here — a download that dies at 71% leaves a file the
    /// loader rejects with "GGUF tensor data range is out of bounds", so size is checked
    /// before the server is ever started. Same guard as IndexTtsCrispAsr.ExpectedFileSizes.
    /// </summary>
    private static readonly Dictionary<string, long[]> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ8_0FileName] = [3502960096L, 3502955328L],
        [ModelF16FileName] = [4547359840L, 4547355072L],
    };

    /// <summary>
    /// Bumping this re-prompts every user with the licence window. Keep it in step with the
    /// licence text shipped in the accept dialog.
    /// </summary>
    public const string LicenseVersion = "bilibili-model-use-license-2026-08";

    public static bool IsLicenseAccepted() =>
        string.Equals(
            Se.Settings.Video.TextToSpeech.IndexTts25AudioCppLicenseAccepted,
            LicenseVersion,
            StringComparison.Ordinal);

    public static void AcceptLicense() =>
        Se.Settings.Video.TextToSpeech.IndexTts25AudioCppLicenseAccepted = LicenseVersion;

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.IndexTts25AudioCppModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey == ModelKeyF16 ? ModelKeyF16 : ModelKeyQ8_0;
    }

    public static string GetModelFileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyF16 ? ModelF16FileName : ModelQ8_0FileName;

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(10),
    };

    private static readonly AudioCppServer Server = new("IndexTTS 2.5", "IndexTTS-2.5", "index_tts2", HttpClient);

    public Task<bool> IsInstalled(string? region) => Task.FromResult(File.Exists(GetServerExecutable()));

    public override string ToString() => Name;

    /// <summary>
    /// The shared audio.cpp install — see <see cref="AudioCppRuntime"/>. Kept as a local alias
    /// because everything in this class reads it.
    /// </summary>
    public static string GetSetEngineFolder() => AudioCppRuntime.GetSetEngineFolder();

    /// <summary>
    /// Per-engine working folder under TextToSpeech (voices, synthesis output), matching where
    /// <see cref="IndexTtsCrispAsr"/> keeps its own — the binaries are shared, this is not.
    /// </summary>
    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "IndexTts25AudioCpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// audio.cpp is pointed at a directory, not a file, so the GGUF gets its own folder under
    /// the shared models root: <c>&lt;data&gt;/audio.cpp/models/IndexTTS2.5-GGUF/</c>.
    /// </summary>
    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(GetSetEngineFolder(), "models", "IndexTTS2.5-GGUF");
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
        return folder;
    }

    private static bool _voiceSeedAttempted;

    /// <summary>
    /// One-time best-effort seed of reference WAVs from the shared voice pack another engine
    /// already downloaded, so the voice combo is not empty on first run — this engine clones
    /// only and has no built-in voices. The pack ships at 16 kHz and IndexTTS clones from
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
                VoiceSeedHelper.CopyOrResample(src, dest, 24000, "IndexTTS 2.5 (audio.cpp)");
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "IndexTTS 2.5 (audio.cpp): voice seeding from the shared voice pack failed");
        }
    }

    public static string GetServerExecutable() => AudioCppRuntime.GetServerExecutable();

    public static string GetModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetModelFileName(modelKey));

    public static bool IsValidLocalModelFile(string path, string fileName) =>
        AudioCppRuntime.IsValidModelFile(path, ExpectedFileSizes.GetValueOrDefault(fileName));

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetModelPath(modelKey), GetModelFileName(modelKey));

    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus() => AudioCppRuntime.GetEngineUpdateStatus();

    /// <summary>
    /// ggml backend the shared audio.cpp install was built for — see
    /// <see cref="AudioCppRuntime.GetBackend"/>.
    /// </summary>
    public static string GetBackend() => AudioCppRuntime.GetBackend();

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

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ8_0, ModelKeyF16 });

    /// <summary>
    /// The five languages IndexTTS-2.5 was trained on. audio.cpp defaults to "auto", which
    /// only distinguishes Han-script (zh) from everything else (en) — so ja/es/ar have to be
    /// selected explicitly or they are synthesised with the wrong language's phonology.
    /// </summary>
    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(new[]
    {
        // TtsLanguage is (name, code) - every other engine writes it that way. Reversed here, the
        // combo listed "zh"/"en"/"ja" instead of language names and Speak sent language.Code, i.e.
        // the literal "Chinese", to audio.cpp - so no explicit pick ever worked.
        new TtsLanguage("Auto", "auto"),
        new TtsLanguage("Chinese", "zh"),
        new TtsLanguage("English", "en"),
        new TtsLanguage("Japanese", "ja"),
        new TtsLanguage("Spanish", "es"),
        new TtsLanguage("Arabic", "ar"),
    });

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
                "IndexTTS 2.5 (audio.cpp) requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "3-10 s of clean speech works best.");
        }

        if (!IsLicenseAccepted())
        {
            throw new InvalidOperationException(
                "The IndexTTS-2.5 model licence (bilibili Model Use License) has not been accepted.");
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        // audio.cpp's OpenAI-style speech payload. Voice cloning goes through `voice_ref`,
        // which accepts a server-side path object — and unlike CrispASR's indextts backend it
        // is honoured per request, so no server restart when the user switches voice.
        var options = new Dictionary<string, object>();

        var languageCode = language?.Code;
        if (!string.IsNullOrEmpty(languageCode) && !string.Equals(languageCode, "auto", StringComparison.OrdinalIgnoreCase))
        {
            options["language"] = languageCode;
        }

        // >1 slows down, <1 speeds up; the model's own duration control rather than a resample,
        // so pitch is unaffected.
        var durationFactor = Math.Clamp(Se.Settings.Video.TextToSpeech.IndexTts25AudioCppDurationFactor, 0.5, 2.0);
        if (Math.Abs(durationFactor - 1.0) > 0.001)
        {
            options["duration_factor"] = durationFactor;
        }

        var emotion = Se.Settings.Video.TextToSpeech.IndexTts25AudioCppEmotion;
        if (!string.IsNullOrEmpty(emotion) && !string.Equals(emotion, "none", StringComparison.OrdinalIgnoreCase))
        {
            var vector = GetEmotionVector(emotion);
            if (vector != null)
            {
                options["emotion_vector"] = vector;
                options["emotion_alpha"] = Math.Clamp(Se.Settings.Video.TextToSpeech.IndexTts25AudioCppEmotionAlpha, 0.0, 1.0);
            }
        }

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
        Se.WriteToolsLog($"IndexTTS 2.5 (audio.cpp): POST {Server.BaseUrl}/v1/audio/speech (voice={indexVoice}, textLen={text.Length})");

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

            var failMsg = $"IndexTTS 2.5 (audio.cpp) request failed — Voice: {indexVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LocalServerHelper.LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "IndexTTS 2.5 (audio.cpp) — the audiocpp_server process crashed during synthesis."
                    : "IndexTTS 2.5 (audio.cpp) request failed — the connection to audiocpp_server was dropped.")
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
                var errMsg = $"IndexTTS 2.5 (audio.cpp) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {indexVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"IndexTTS 2.5 (audio.cpp) synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand));
            }

            await using var fileStream = File.Create(outputFileName);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
    }

    /// <summary>
    /// The eight-slot emotion vector IndexTTS-2.5 takes, in the model's own order:
    /// happy, angry, sad, afraid, disgusted, melancholic, surprised, calm.
    /// </summary>
    public static IReadOnlyList<string> EmotionNames { get; } = new[]
    {
        "happy", "angry", "sad", "afraid", "disgusted", "melancholic", "surprised", "calm",
    };

    private static double[]? GetEmotionVector(string emotion)
    {
        var index = -1;
        for (var i = 0; i < EmotionNames.Count; i++)
        {
            if (string.Equals(EmotionNames[i], emotion, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            return null;
        }

        var vector = new double[EmotionNames.Count];
        vector[index] = 1.0;
        return vector;
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
                    ["task"] = "clon",
                    ["mode"] = "offline",
                },
            },
        };

        var configPath = Path.Combine(GetSetEngineFolder(), "index-tts25-server.json");
        File.WriteAllText(
            configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Se.WriteToolsLog($"IndexTTS 2.5 (audio.cpp): server config written to {configPath} (model={modelKey}, backend={backend})");
        return configPath;
    }

    /// <summary>
    /// Stop the audio.cpp server if running, releasing its ~4.6 GB working set. audio.cpp never
    /// unloads a model on its own once loaded, so this is the only way the memory comes back.
    /// </summary>
    public static void StopServer() => Server.Stop();

    public bool ImportVoice(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        var voicesFolder = GetSetVoicesFolder();
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var destinationFileName = VoiceCloneImporter.GetUniqueDestinationFileName(voicesFolder, baseName);

        // audio.cpp resamples the reference itself, but importing at 24 kHz mono keeps the
        // reference clean and matches what the other IndexTTS engine stores.
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
            Se.LogError(ex, "IndexTTS 2.5 (audio.cpp) voice import failed (ffmpeg conversion).");
            return false;
        }

        return File.Exists(destinationFileName);
    }
}
