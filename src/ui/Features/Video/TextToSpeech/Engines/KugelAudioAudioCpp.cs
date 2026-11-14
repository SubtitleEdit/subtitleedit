using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
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
/// No cloning: the open release ships four preset voices embedded in the GGUF (two German female,
/// one British female, one British male), picked per request with the <c>voice_id</c> option, so
/// switching voice does not restart the server. There is no language option either — the model
/// reads the language from the text. A preset speaks every language, but the voices keep their
/// native accent; on Danish the British female preset came out anglicised to the point of being
/// unintelligible, while the other three round-tripped through speech-to-text almost word for word.
///
/// Slow and heavy: about 3-4x slower than real time on an M4 (Metal, Q4_K) and ~9 GB resident.
///
/// Licence note: the audio.cpp binaries are Apache-2.0 and the KugelAudio weights MIT, so there is
/// no first-run licence gate.
/// </summary>
public class KugelAudioAudioCpp : ITtsEngine
{
    public string Name => "KugelAudio (audio.cpp)";
    public string Description => "KugelAudio preset voices in 23 European languages, via audio.cpp";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => false;
    public bool SupportsPerLineVoiceCloning => false;

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
    /// Exact byte sizes on the audio-cpp/audio.cpp-gguf HuggingFace repo. A truncated GGUF is
    /// the single most common failure here — a download that dies partway leaves a file the
    /// loader rejects with "GGUF tensor data range is out of bounds", so size is checked
    /// before the server is ever started. Same guard as <see cref="IndexTts25AudioCpp"/>.
    /// </summary>
    private static readonly Dictionary<string, long[]> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ4_KFileName] = [5732997442L],
        [ModelQ8_0FileName] = [9752398658L],
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

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetModelPath(modelKey), GetModelFileName(modelKey));

    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus() => AudioCppRuntime.GetEngineUpdateStatus();

    public Task<Voice[]> GetVoices(string language)
    {
        // Every preset speaks every language, so the list does not depend on the language.
        return Task.FromResult(PresetVoices.Select(v => new Voice(new KugelAudioVoice(v))).ToArray());
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

    public async Task<TtsResult> Speak(
        string text,
        string outputFolder,
        Voice voice,
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken)
    {
        var voiceId = ResolveVoiceId(voice);
        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        var body = JsonSerializer.Serialize(BuildSpeechPayload(text, voiceId));
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
                    // Preset voices only: the open model has no clone task.
                    ["task"] = "tts",
                    ["mode"] = "offline",
                },
            },
        };

        var configPath = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "kugelaudio-server.json");
        File.WriteAllText(
            configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Se.WriteToolsLog($"KugelAudio (audio.cpp): server config written to {configPath} (model={modelKey}, backend={backend})");
        return configPath;
    }

    /// <summary>
    /// Stop the audio.cpp server if running, releasing the loaded model's working set.
    /// audio.cpp never unloads a model on its own once loaded, so this is the only way the
    /// memory comes back.
    /// </summary>
    public static void StopServer() => Server.Stop();

    public bool ImportVoice(string fileName) => false;
}
