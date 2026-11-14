using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;
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
/// Mistral's Voxtral 4B TTS (2603) run through the audio.cpp runtime — the same pure C++/ggml
/// install the other audio.cpp engines use (see <see cref="AudioCppRuntime"/>), so the binaries
/// are downloaded once and shared. 24 kHz output.
///
/// No cloning: the open checkpoint lacks the codec encoder, so only the 20 preset voices embedded
/// in the GGUF are available, picked per request with the <c>voice_id</c> option (switching voice
/// does not restart the server). Five general presets are English; the rest are one or two native
/// voices each for French, German, Spanish, Italian, Portuguese, Dutch, Hindi and Arabic. There is
/// no language option — the model reads the language from the text, and the preset sets the accent.
///
/// On an M4 (Metal, Q8_0) it ran about 1.35x slower than real time with ~9.5 GB resident, and
/// English, French, German, Spanish, Italian, Portuguese and Dutch round-tripped through
/// speech-to-text word for word. Languages outside the nine come out as accented gibberish.
///
/// Licence note: the audio.cpp binaries are Apache-2.0, but the weights and the preset voices are
/// CC BY-NC 4.0, so the engine sits behind the first-run licence gate like Fish and Breeze.
/// </summary>
public class VoxtralTtsAudioCpp : ITtsEngine
{
    public string Name => "Voxtral TTS (audio.cpp)";
    public string Description => "Mistral Voxtral preset voices in 9 languages, via audio.cpp";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => false;
    public bool SupportsPerLineVoiceCloning => false;

    // Q8_0 is the default: upstream validated it with the embedded presets (which stay BF16 in
    // both files), and it round-tripped word for word in seven languages at 3.3 GB less than BF16.
    public const string ModelKeyQ8_0 = "Q8_0 (~4.7 GB)";
    public const string ModelKeyBf16 = "BF16 (~8.0 GB)";
    public const string DefaultModelKey = ModelKeyQ8_0;

    public const string ModelQ8_0FileName = "voxtral-4b-tts-2603-q8_0.gguf";
    public const string ModelBf16FileName = "voxtral-4b-tts-2603-bf16.gguf";

    /// <summary>Family name audio.cpp registers Voxtral TTS under.</summary>
    public const string FamilyName = "voxtral_tts";

    /// <summary>Model id used in the generated server config and in each request body.</summary>
    private const string ServerModelId = "voxtraltts";

    /// <summary>The preset ids the GGUF embeds, in display order (English first).</summary>
    public static readonly string[] PresetVoices =
    [
        "casual_female", "casual_male", "cheerful_female", "neutral_female", "neutral_male",
        "fr_female", "fr_male",
        "de_female", "de_male",
        "es_female", "es_male",
        "it_female", "it_male",
        "pt_female", "pt_male",
        "nl_female", "nl_male",
        "hi_female", "hi_male",
        "ar_male",
    ];

    public const string DefaultVoice = "casual_male";

    /// <summary>
    /// Exact byte sizes on the audio-cpp/audio.cpp-gguf HuggingFace repo. A truncated GGUF is
    /// the single most common failure here — a download that dies partway leaves a file the
    /// loader rejects with "GGUF tensor data range is out of bounds", so size is checked
    /// before the server is ever started. Same guard as <see cref="IndexTts25AudioCpp"/>.
    /// </summary>
    private static readonly Dictionary<string, long[]> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ8_0FileName] = [4722918208L],
        [ModelBf16FileName] = [8030847808L],
    };

    /// <summary>
    /// Bumping this re-prompts every user with the licence window. Keep it in step with the
    /// summary text in <see cref="LicenseDefinition"/>.
    /// </summary>
    public const string LicenseVersion = "cc-by-nc-4.0";

    public static bool IsLicenseAccepted() =>
        string.Equals(
            Se.Settings.Video.TextToSpeech.VoxtralTtsAudioCppLicenseAccepted,
            LicenseVersion,
            StringComparison.Ordinal);

    public static void AcceptLicense() =>
        Se.Settings.Video.TextToSpeech.VoxtralTtsAudioCppLicenseAccepted = LicenseVersion;

    /// <summary>What the first-run licence gate shows.</summary>
    public static ModelLicenseDefinition LicenseDefinition { get; } = new(
        DialogTitle: "Voxtral TTS - model license",
        Header: "The Voxtral TTS model has its own license",
        Intro: "The audio.cpp engine is Apache-2.0, but the Voxtral 4B TTS model weights and its "
            + "preset voices are licensed by Mistral AI under Creative Commons "
            + "Attribution-NonCommercial 4.0 (CC BY-NC 4.0). Please read the main points before downloading.",
        SummaryPoints: new[]
        {
            "The model and its preset voices are free to use for non-commercial purposes only.",
            "Commercial use needs a separate license from Mistral AI. That includes dubbing or voice-over for a client or for a fee, and audio for monetized videos, ads or sponsored content.",
            "If you share the model or adapted versions of it, you must give credit to Mistral AI, link to the license and say if you changed it.",
            "Do not use the voices to impersonate a real person or to mislead listeners.",
        },
        LicenseUrl: "https://creativecommons.org/licenses/by-nc/4.0/",
        ModelPageUrl: "https://huggingface.co/mistralai/Voxtral-4B-TTS-2603",
        AcceptCheckBoxText: "I have read and accept the CC BY-NC 4.0 license",
        Accept: AcceptLicense);

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.VoxtralTtsAudioCppModel;
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

    private static readonly AudioCppServer Server = new("Voxtral TTS", "Voxtral TTS", "voxtral_tts", HttpClient);

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

        var folder = Path.Combine(Se.TextToSpeechFolder, "VoxtralTtsAudioCpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// The GGUF gets its own folder under the shared models root:
    /// <c>&lt;data&gt;/audio.cpp/models/Voxtral-4B-TTS-2603-GGUF/</c>.
    /// </summary>
    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "models", "Voxtral-4B-TTS-2603-GGUF");
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
        // No language parameter, so all presets are listed; each one names its language.
        return Task.FromResult(PresetVoices.Select(v => new Voice(new VoxtralTtsVoice(v))).ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ8_0, ModelKeyBf16 });

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
        var id = (voice?.EngineVoice as VoxtralTtsVoice)?.Voice;
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
        Se.WriteToolsLog($"Voxtral TTS (audio.cpp): POST {Server.BaseUrl}/v1/audio/speech (voice={voiceId}, textLen={text.Length})");

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

            var failMsg = $"Voxtral TTS (audio.cpp) request failed — Voice: {voiceId}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LocalServerHelper.LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Voxtral TTS (audio.cpp) — the audiocpp_server process crashed during synthesis."
                    : "Voxtral TTS (audio.cpp) request failed — the connection to audiocpp_server was dropped.")
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
                var errMsg = $"Voxtral TTS (audio.cpp) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {voiceId}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Voxtral TTS (audio.cpp) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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
                    // Preset voices only: the open checkpoint has no codec encoder, so no clone task.
                    ["task"] = "tts",
                    ["mode"] = "offline",
                },
            },
        };

        var configPath = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "voxtral-tts-server.json");
        File.WriteAllText(
            configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Se.WriteToolsLog($"Voxtral TTS (audio.cpp): server config written to {configPath} (model={modelKey}, backend={backend})");
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
