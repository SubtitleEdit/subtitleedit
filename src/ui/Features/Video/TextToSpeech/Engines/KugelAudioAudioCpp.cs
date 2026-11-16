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
/// KugelAudio-0-Open run through the audio.cpp runtime — the same pure C++/ggml install the other
/// audio.cpp engines use (see <see cref="AudioCppRuntime"/>), so the binaries are downloaded once
/// and shared. A 7B VibeVoice-based model trained for European languages (23, including Danish,
/// Swedish, Norwegian, Finnish and Polish), 24 kHz output.
///
/// Two kinds of voice. The open release ships four preset voices embedded in the GGUF (two German
/// female, one British female, one British male), picked per request with the <c>voice_id</c>
/// option. Since audio.cpp v0.9.5 (#1275) it also clones from a reference WAV (up to 30 s, no
/// transcript needed), sent per request as <c>voice_ref</c> - so neither kind of voice switch
/// restarts the server, and per-line cloning ("Clone from video") is free. Cloning needs the
/// checkpoint's acoustic encoder, which the published GGUFs omit; SE downloads it beside the GGUF
/// (see <see cref="KugelAudioAudioCppDownloadService"/>). There is no language option — the model
/// reads the language from the text. A preset speaks every language, but the voices keep their
/// native accent; on Danish the British female preset came out anglicised to the point of being
/// unintelligible, while the other three round-tripped through speech-to-text almost word for word.
///
/// Slow and heavy: about 3-4x slower than real time on an M4 (Metal, Q4_K) and ~9 GB resident.
///
/// Licence note: the audio.cpp binaries are Apache-2.0 and the KugelAudio weights MIT, so there is
/// no first-run licence gate.
/// </summary>
public class KugelAudioAudioCpp : ITtsEngine, IPerLineCloneEngine
{
    public string Name => "KugelAudio (audio.cpp)";
    public string Description => "KugelAudio preset voices and voice cloning in 23 European languages, via audio.cpp";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => true;

    // Q4_K is the default: on Danish it round-tripped through speech-to-text as well as Q8_0 is
    // expected to, at 3.8 GB less to download and hold in memory.
    public const string ModelKeyQ4_K = "Q4_K (~5.3 GB)";
    public const string ModelKeyQ8_0 = "Q8_0 (~9.1 GB)";
    public const string DefaultModelKey = ModelKeyQ4_K;

    public const string ModelQ4_KFileName = "kugelaudio-0-open-q4_k.gguf";
    public const string ModelQ8_0FileName = "kugelaudio-0-open-q8_0.gguf";

    /// <summary>Family name audio.cpp registers KugelAudio under.</summary>
    public const string FamilyName = "kugelaudio";

    /// <summary>Model id used in the generated server config and in each request body.</summary>
    private const string ServerModelId = "kugelaudio";

    /// <summary>The preset ids the GGUF embeds, in display order.</summary>
    public static readonly string[] PresetVoices = ["default", "clear", "english_male", "english_female"];

    public const string DefaultVoice = "default";

    /// <summary>
    /// The checkpoint's acoustic encoder, which turns a reference WAV into voice features. The
    /// published GGUFs leave it out, so it is a file of its own, passed to the server as the
    /// <c>kugelaudio.reference_encoder_path</c> session option.
    /// </summary>
    public const string ReferenceEncoderFileName = "kugelaudio-0-open-acoustic-encoder.safetensors";

    /// <summary>
    /// audio.cpp's clone-aware KugelAudio model spec. The published GGUFs embed the older
    /// preset-only contract, which refuses a voice reference, so the server is pointed at this one
    /// with <c>model_spec_override</c>. Preset output is byte-identical either way.
    /// </summary>
    public const string ModelSpecFileName = "kugelaudio-model-spec.json";

    /// <summary>audio.cpp rejects a longer reference rather than truncating it, so imports are cut to this.</summary>
    public const int MaxReferenceSeconds = 30;

    /// <summary>
    /// Exact byte sizes on the audio-cpp/audio.cpp-gguf HuggingFace repo. A truncated GGUF is
    /// the single most common failure here — a download that dies partway leaves a file the
    /// loader rejects with "GGUF tensor data range is out of bounds", so size is checked
    /// before the server is ever started. Same guard as <see cref="IndexTts25AudioCpp"/>.
    /// </summary>
    private static readonly Dictionary<string, long[]> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ4_KFileName] = [5732997442L],
        [ModelQ8_0FileName] = [9752398658L],
        [ReferenceEncoderFileName] = [687428480L],
        [ModelSpecFileName] = [6274L],
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.KugelAudioAudioCppModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey == ModelKeyQ8_0 ? ModelKeyQ8_0 : ModelKeyQ4_K;
    }

    public static string GetModelFileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyQ8_0 ? ModelQ8_0FileName : ModelQ4_KFileName;

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(10),
    };

    private static readonly AudioCppServer Server = new("KugelAudio", "KugelAudio", "kugelaudio", HttpClient);

    public Task<bool> IsInstalled(string? region) => Task.FromResult(File.Exists(AudioCppRuntime.GetServerExecutable()));

    public override string ToString() => Name;

    /// <summary>
    /// Per-engine working folder under TextToSpeech (synthesis output). The audio.cpp binaries
    /// are shared (<see cref="AudioCppRuntime"/>); this is not.
    /// </summary>
    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "KugelAudioAudioCpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// The GGUF gets its own folder under the shared models root:
    /// <c>&lt;data&gt;/audio.cpp/models/KugelAudio-0-Open-GGUF/</c>.
    /// </summary>
    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "models", "KugelAudio-0-Open-GGUF");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetModelFileName(modelKey));

    public static bool IsValidLocalModelFile(string path, string fileName) =>
        AudioCppRuntime.IsValidModelFile(path, ExpectedFileSizes.GetValueOrDefault(fileName));

    public static string GetReferenceEncoderPath() => Path.Combine(GetSetModelsFolder(), ReferenceEncoderFileName);

    public static string GetModelSpecPath() => Path.Combine(GetSetModelsFolder(), ModelSpecFileName);

    /// <summary>
    /// The GGUF plus the two cloning files. An install from before cloning has only the GGUF, so
    /// it reads as not installed and the download dialog fetches just the missing encoder.
    /// </summary>
    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetModelPath(modelKey), GetModelFileName(modelKey))
        && IsCloningInstalled();

    public static bool IsCloningInstalled() =>
        IsValidLocalModelFile(GetReferenceEncoderPath(), ReferenceEncoderFileName)
        && IsValidLocalModelFile(GetModelSpecPath(), ModelSpecFileName);

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
    /// One-time best-effort seed of reference WAVs from the shared Qwen3 voice pack, so cloning
    /// has something to show beside the presets on first run. Resampled to 24 kHz, the model's
    /// own rate, on seed. No transcripts: KugelAudio clones from the audio alone.
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
            if (!Directory.Exists(sourceFolder))
            {
                return;
            }

            foreach (var src in Directory.GetFiles(sourceFolder, "*.wav"))
            {
                VoiceSeedHelper.CopyOrResample(src, Path.Combine(voicesFolder, Path.GetFileName(src)), 24000, "KugelAudio (audio.cpp)");
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "KugelAudio (audio.cpp): voice seeding from the shared voice pack failed");
        }
    }

    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus() => AudioCppRuntime.GetEngineUpdateStatus();

    public async Task<Voice[]> GetVoices(string language)
    {
        // Every voice speaks every language, so the list does not depend on the language:
        // the four presets first, then the reference WAVs to clone from.
        var result = PresetVoices.Select(v => new Voice(new KugelAudioVoice(v))).ToList();

        var voicesFolder = await Task.Run(GetSetVoicesFolder);
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new IndexTtsVoice(name, file)));
            }
        }

        return result.ToArray();
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ4_K, ModelKeyQ8_0 });

    // No language option: the model infers the language from the text.
    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) =>
        Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    /// <summary>
    /// The preset to send for <paramref name="voice"/>. A voice object left over from another
    /// engine, or an unknown id, falls back to <see cref="DefaultVoice"/> rather than being
    /// passed on: the server answers an unknown voice_id with HTTP 500.
    /// </summary>
    internal static string ResolveVoiceId(Voice? voice)
    {
        var id = (voice?.EngineVoice as KugelAudioVoice)?.Voice;
        return PresetVoices.FirstOrDefault(p => string.Equals(p, id, StringComparison.OrdinalIgnoreCase))
               ?? DefaultVoice;
    }

    /// <summary>
    /// <see cref="IPerLineCloneEngine"/>: the server takes <c>voice_ref</c> as a path per request,
    /// so the voice simply points at the cut clip - nothing is staged.
    /// </summary>
    public Voice? MakePerLineCloneVoice(string clipFileName, string voiceName) =>
        new Voice(new IndexTtsVoice(voiceName, clipFileName));

    /// <summary>The clip's own path, which is exactly what the voice carries.</summary>
    public string? GetPerLineReferenceClip(Voice voice) => GetReferencePath(voice);

    /// <summary>Nothing is ever staged, so there is nothing to clear between runs.</summary>
    public void ResetStagedPerLineReferences()
    {
    }

    /// <summary>The reference WAV a clone voice points at; null for a preset (or foreign) voice.</summary>
    internal static string? GetReferencePath(Voice? voice) =>
        voice?.EngineVoice is IndexTtsVoice indexVoice && !string.IsNullOrEmpty(indexVoice.FilePath)
            ? indexVoice.FilePath
            : null;

    /// <summary>audio.cpp's OpenAI-style speech payload; the preset goes in as <c>voice_id</c>.</summary>
    internal static Dictionary<string, object> BuildSpeechPayload(string text, string voiceId) => new()
    {
        ["model"] = ServerModelId,
        ["input"] = text,
        ["response_format"] = "wav",
        ["options"] = new Dictionary<string, object>
        {
            ["voice_id"] = voiceId,
        },
    };

    /// <summary>
    /// The cloning payload: the reference goes in as <c>voice_ref</c> (which takes precedence over
    /// any <c>voice_id</c>, so none is sent).
    /// </summary>
    internal static Dictionary<string, object> BuildCloneSpeechPayload(string text, string referencePath) => new()
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

    public async Task<TtsResult> Speak(
        string text,
        string outputFolder,
        Voice voice,
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken)
    {
        var referencePath = GetReferencePath(voice);
        if (referencePath != null && !File.Exists(referencePath))
        {
            throw new InvalidOperationException($"KugelAudio (audio.cpp): reference voice WAV not found: {referencePath}");
        }

        // For the logs and error messages: the preset id, or the reference file cloned from.
        var voiceId = referencePath == null ? ResolveVoiceId(voice) : Path.GetFileName(referencePath);
        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        var body = JsonSerializer.Serialize(referencePath == null
            ? BuildSpeechPayload(text, voiceId)
            : BuildCloneSpeechPayload(text, referencePath));
        Se.WriteToolsLog($"KugelAudio (audio.cpp): POST {Server.BaseUrl}/v1/audio/speech (voice={voiceId}, textLen={text.Length})");

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

            var failMsg = $"KugelAudio (audio.cpp) request failed — Voice: {voiceId}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LocalServerHelper.LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "KugelAudio (audio.cpp) — the audiocpp_server process crashed during synthesis."
                    : "KugelAudio (audio.cpp) request failed — the connection to audiocpp_server was dropped.")
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
                var errMsg = $"KugelAudio (audio.cpp) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {voiceId}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"KugelAudio (audio.cpp) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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
        var config = BuildServerConfig(port, backend, GetModelPath(modelKey), GetReferenceEncoderPath(), GetModelSpecPath(), IsCloningInstalled());

        var configPath = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "kugelaudio-server.json");
        File.WriteAllText(
            configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Se.WriteToolsLog($"KugelAudio (audio.cpp): server config written to {configPath} (model={modelKey}, backend={backend}, cloning={IsCloningInstalled()})");
        return configPath;
    }

    /// <summary>
    /// The server config. With the cloning files present the model entry also gets the encoder
    /// (session option) and the clone-aware spec (override); without them it stays the plain
    /// preset-only entry, so presets keep working on an install that has not fetched them yet.
    /// </summary>
    internal static Dictionary<string, object> BuildServerConfig(int port, string backend, string modelPath, string encoderPath, string specPath, bool cloning)
    {
        var model = new Dictionary<string, object>
        {
            ["id"] = ServerModelId,
            ["family"] = FamilyName,
            ["path"] = modelPath,
            // "tts" serves both: a request with voice_ref clones, one without uses the preset.
            ["task"] = "tts",
            ["mode"] = "offline",
        };

        if (cloning)
        {
            model["model_spec_override"] = specPath;
            model["session_options"] = new Dictionary<string, object>
            {
                [FamilyName + ".reference_encoder_path"] = encoderPath,
            };
        }

        return new Dictionary<string, object>
        {
            ["host"] = "127.0.0.1",
            ["port"] = port,
            ["backend"] = backend,
            ["threads"] = Math.Max(1, Math.Min(8, Environment.ProcessorCount / 2)),
            ["lazy_load"] = true,
            ["models"] = new[] { model },
        };
    }

    /// <summary>
    /// Stop the audio.cpp server if running, releasing the loaded model's working set.
    /// audio.cpp never unloads a model on its own once loaded, so this is the only way the
    /// memory comes back.
    /// </summary>
    public static void StopServer() => Server.Stop();

    /// <summary>
    /// Copies a reference into the voices folder as 24 kHz mono, cut to
    /// <see cref="MaxReferenceSeconds"/> (the server refuses a longer one). No transcript needed.
    /// </summary>
    public bool ImportVoice(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        var voicesFolder = GetSetVoicesFolder();
        var destinationFileName = VoiceCloneImporter.GetUniqueDestinationFileName(voicesFolder, Path.GetFileNameWithoutExtension(fileName));

        try
        {
            using var process = FfmpegGenerator.ConvertToMono24kHzWav(fileName, destinationFileName, MaxReferenceSeconds);
            if (!process.Start())
            {
                return false;
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "KugelAudio (audio.cpp) voice import failed (ffmpeg conversion).");
            return false;
        }

        return File.Exists(destinationFileName);
    }
}
