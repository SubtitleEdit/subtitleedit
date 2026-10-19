using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;
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
/// BreezeBlue's Breeze TTS 2 run through the audio.cpp runtime — the same pure C++/ggml install
/// <see cref="IndexTts25AudioCpp"/> uses (see <see cref="AudioCppRuntime"/>), so the binaries
/// are downloaded once and shared. English and Chinese only, 24 kHz output.
///
/// Two ways to pick a voice, both steered by the free-text instruction:
/// <list type="bullet">
/// <item>The "Default" voice has no reference WAV: the instruction describes the voice and
/// the model designs it ("voice design").</item>
/// <item>A reference WAV clones that voice, and the instruction then directs the delivery
/// (tone, emotion, pace). Cloning needs the clip's transcript as <c>reference_text</c>.</item>
/// </list>
///
/// Like the other audio.cpp engines, the server honours a per-request <c>voice_ref</c>, so
/// switching voice does NOT restart the server — only a model or backend change does — which
/// is also what makes per-line cloning ("Clone from video") free here.
///
/// Licence note: the audio.cpp binaries are Apache-2.0, but the Breeze TTS 2 *weights* are
/// under the BreezeBlue Research and Non-Commercial License, which also covers the generated
/// audio, so it has to be accepted by the user before the first download — see
/// <see cref="IsLicenseAccepted"/>.
/// </summary>
public class BreezeTtsAudioCpp : ITtsEngine, IPerLineCloneEngine
{
    public string Name => "Breeze TTS 2 (audio.cpp)";
    public string Description => "Breeze TTS 2 voice design and voice cloning in English and Chinese, via audio.cpp";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => true;

    // Q8_0 is the default: upstream measured no audible loss against BF16, and it is faster.
    // There is no F32 option on purpose - upstream warns the model mispronounces and repeats
    // itself in f32, as it was trained in bf16.
    public const string ModelKeyQ8_0 = "Q8_0 (~5.1 GB)";
    public const string ModelKeyBf16 = "BF16 (~7.3 GB)";
    public const string DefaultModelKey = ModelKeyQ8_0;

    public const string ModelQ8_0FileName = "breeze-tts-2-q8_0.gguf";
    public const string ModelBf16FileName = "breeze-tts-2-bf16.gguf";

    /// <summary>Family name audio.cpp registers Breeze TTS 2 under.</summary>
    public const string FamilyName = "breeze_tts";

    /// <summary>Model id used in the generated server config and in each request body.</summary>
    private const string ServerModelId = "breezetts2";

    /// <summary>The voice with no reference WAV: designed from the instruction alone.</summary>
    public const string DefaultVoiceName = "Default";

    /// <summary>
    /// Exact byte sizes on the audio-cpp/audio.cpp-gguf HuggingFace repo. A truncated GGUF is
    /// the single most common failure here — a download that dies partway leaves a file the
    /// loader rejects with "GGUF tensor data range is out of bounds", so size is checked
    /// before the server is ever started. Same guard as <see cref="IndexTts25AudioCpp"/>.
    /// </summary>
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ8_0FileName] = 5079668352L,
        [ModelBf16FileName] = 7342916800L,
    };

    /// <summary>
    /// Bumping this re-prompts every user with the licence window. Keep it in step with the
    /// summary text in <see cref="LicenseDefinition"/>.
    /// </summary>
    public const string LicenseVersion = "breezeblue-research-non-commercial-1.1";

    public static bool IsLicenseAccepted() =>
        string.Equals(
            Se.Settings.Video.TextToSpeech.BreezeTtsAudioCppLicenseAccepted,
            LicenseVersion,
            StringComparison.Ordinal);

    public static void AcceptLicense() =>
        Se.Settings.Video.TextToSpeech.BreezeTtsAudioCppLicenseAccepted = LicenseVersion;

    /// <summary>
    /// What the first-run licence gate shows. Stricter than the other gated models: the
    /// non-commercial terms cover the generated audio too, not only the weights.
    /// </summary>
    public static ModelLicenseDefinition LicenseDefinition { get; } = new(
        DialogTitle: "Breeze TTS 2 - model license",
        Header: "The Breeze TTS 2 model has its own license",
        Intro: "The audio.cpp engine is Apache-2.0, but the Breeze TTS 2 model weights are "
            + "licensed under the BreezeBlue Research and Non-Commercial License, which is not "
            + "open source. Please read the main points before downloading.",
        SummaryPoints: new[]
        {
            "The model is free of charge for research and personal, non-commercial use only. This also covers the audio it generates.",
            "Commercial use needs a separate written license from BreezeBlue (contact@breeze.blue). That includes dubbing or voice-over for a client or for a fee, and audio for monetized videos, ads or sponsored content. There is no exception for creators or small businesses.",
            "If you pass the model on to someone else, it must go with a complete copy of the license and the notice: \"Breeze TTS 2 is licensed under the BreezeBlue Research and Non-Commercial License Agreement. Copyright (c) 2026 RESONIA, INC. All Rights Reserved.\"",
            "You are responsible for having the right to clone any voice you use as a reference. Do not imitate a real person without their explicit consent.",
        },
        LicenseUrl: "https://huggingface.co/BreezeBlue/Breeze-TTS-2/blob/main/LICENSE",
        ModelPageUrl: "https://huggingface.co/BreezeBlue/Breeze-TTS-2",
        AcceptCheckBoxText: "I have read and accept the BreezeBlue Research and Non-Commercial License",
        Accept: AcceptLicense);

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.BreezeTtsAudioCppModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey == ModelKeyBf16 ? ModelKeyBf16 : ModelKeyQ8_0;
    }

    public static string GetModelFileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyBf16 ? ModelBf16FileName : ModelQ8_0FileName;

    /// <summary>
    /// What goes in <c>reference_text</c> when the reference clip's transcript is unknown: the
    /// server refuses a missing or empty transcript and accepts this.
    /// </summary>
    internal const string UnknownReferenceTextPlaceholder = " ";

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(10),
    };

    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverLaunchCommand;
    // Only the model and the backend are baked into the running server — voice is per request.
    private static string? _serverModelKey;
    private static string? _serverBackend;
    private static string? _serverExeStamp;
    private static bool _processExitHooked;
    private static readonly StringBuilder _serverLog = new();

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

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

        var folder = Path.Combine(Se.TextToSpeechFolder, "BreezeTtsAudioCpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// audio.cpp is pointed at a directory, not a file, so the GGUF gets its own folder under
    /// the shared models root: <c>&lt;data&gt;/audio.cpp/models/Breeze-TTS-2-GGUF/</c>.
    /// </summary>
    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "models", "Breeze-TTS-2-GGUF");
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
    /// has no built-in voices besides the instruction-designed "Default". Transcripts matter:
    /// audio.cpp refuses a Breeze clone without reference_text. The pack ships at 16 kHz; the
    /// Breeze codec runs at 24 kHz, so resample on seed.
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
                VoiceSeedHelper.CopyOrResample(src, dest, 24000, "Breeze TTS 2 (audio.cpp)");

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
            Se.LogError(ex, "Breeze TTS 2 (audio.cpp): voice seeding from the shared voice pack failed");
        }
    }

    public static string GetModelPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetModelFileName(modelKey));

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
            var info = new FileInfo(path);

            // FileInfo.Length reports the size of the *link* for a symlink, not of its target,
            // so a symlinked GGUF (a reasonable way to share a 5 GB model between apps or
            // put it on another disk) would look truncated — and the caller deletes files it
            // considers truncated. Resolve to the final target before measuring.
            var length = info.ResolveLinkTarget(returnFinalTarget: true) is FileInfo target
                ? target.Length
                : info.Length;

            return length == expected;
        }
        catch
        {
            return false;
        }
    }

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetModelPath(modelKey), GetModelFileName(modelKey));

    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus()
    {
        var exe = AudioCppRuntime.GetServerExecutable();
        if (!File.Exists(exe))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        var folder = Path.GetDirectoryName(exe);
        return string.IsNullOrEmpty(folder)
            ? DownloadHashManager.UpdateStatus.Unknown
            : DownloadHashManager.GetSidecarStatus(folder);
    }

    public async Task<Voice[]> GetVoices(string language)
    {
        // "Default" has no reference: the instruction designs the voice. Every WAV in the
        // voices folder is a clone reference.
        var result = new List<Voice>
        {
            new Voice(new IndexTtsVoice(DefaultVoiceName, string.Empty)),
        };

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

    // The model reads English or Chinese from the text itself, so there is no language pick.
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

        if (!IsLicenseAccepted())
        {
            throw new InvalidOperationException(
                "The Breeze TTS 2 model licence (BreezeBlue Research and Non-Commercial License) has not been accepted.");
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        // Voice description without a reference, delivery direction with one.
        var instruction = Se.Settings.Video.TextToSpeech.BreezeTtsAudioCppInstruction;

        string? referenceText = null;
        if (!string.IsNullOrEmpty(indexVoice.FilePath))
        {
            // The transcript of the reference WAV, from the shared .txt sidecar convention.
            referenceText = ChatterboxTtsCpp.TryReadReferenceTranscript(indexVoice.FilePath);
            if (string.IsNullOrWhiteSpace(referenceText))
            {
                Se.WriteToolsLog($"Breeze TTS 2 (audio.cpp): no transcript beside reference '{indexVoice.FilePath}', cloning with a blank reference_text");
            }
        }

        var payload = BuildSpeechPayload(text, indexVoice.FilePath, referenceText, instruction);
        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Breeze TTS 2 (audio.cpp): POST {ServerBaseUrl}/v1/audio/speech (voice={indexVoice}, textLen={text.Length})");

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

            var failMsg = $"Breeze TTS 2 (audio.cpp) request failed — Voice: {indexVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Breeze TTS 2 (audio.cpp) — the audiocpp_server process crashed during synthesis."
                    : "Breeze TTS 2 (audio.cpp) request failed — the connection to audiocpp_server was dropped.")
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
                var errMsg = $"Breeze TTS 2 (audio.cpp) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {indexVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Breeze TTS 2 (audio.cpp) synthesis failed ({(int)response.StatusCode}): {errorBody}"
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
    /// audio.cpp's OpenAI-style speech payload. Voice cloning goes through <c>voice_ref</c>,
    /// honoured per request, so switching voice needs no server restart. Without a reference
    /// the same <c>tts</c> model designs the voice from the instruction.
    /// </summary>
    internal static Dictionary<string, object> BuildSpeechPayload(string text, string? referencePath, string? referenceText, string? instruction)
    {
        var options = new Dictionary<string, object>();

        // audio.cpp falls back to "Speak clearly and naturally." when there is none. The option
        // is `instruction` in the pinned build; upstream later renamed it `instruct` but still
        // accepts this name.
        var trimmedInstruction = (instruction ?? string.Empty).Trim();
        if (trimmedInstruction.Length > 0)
        {
            options["instruction"] = trimmedInstruction;
        }

        var payload = new Dictionary<string, object>
        {
            ["model"] = ServerModelId,
            ["input"] = text,
            ["response_format"] = "wav",
        };

        if (!string.IsNullOrEmpty(referencePath))
        {
            // audio.cpp refuses a Breeze clone with a missing or empty reference_text
            // ("BreezeTTS clone requires reference_text"). A single space passes that check, so
            // an unknown transcript - a per-line clip cut from the video with no
            // original-language subtitle loaded - is sent as that placeholder, like Fish S2 Pro.
            // What must NEVER be sent in its place is the text being spoken: the transcript and
            // the target text share one prompt, and when they are the same line the model can
            // replay the clip instead of speaking the line (#14480).
            options["reference_text"] = string.IsNullOrWhiteSpace(referenceText)
                ? UnknownReferenceTextPlaceholder
                : referenceText.Trim();
            payload["voice_ref"] = new Dictionary<string, object>
            {
                ["type"] = "path",
                ["path"] = referencePath,
            };
        }

        if (options.Count > 0)
        {
            payload["options"] = options;
        }

        return payload;
    }

    private static async Task EnsureServerRunningAsync(string modelKey, CancellationToken ct)
    {
        var backend = AudioCppRuntime.GetBackend();
        var exeStamp = AudioCppRuntime.GetServerExecutableStamp();

        if (_serverProcess is { HasExited: false } && _serverPort != 0
            && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(_serverBackend, backend, StringComparison.OrdinalIgnoreCase)
            && string.Equals(_serverExeStamp, exeStamp, StringComparison.Ordinal))
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0
                && string.Equals(_serverModelKey, modelKey, StringComparison.OrdinalIgnoreCase)
                && string.Equals(_serverBackend, backend, StringComparison.OrdinalIgnoreCase)
                && string.Equals(_serverExeStamp, exeStamp, StringComparison.Ordinal))
            {
                return;
            }

            if (_serverProcess != null)
            {
                StopServerInternal();
            }

            var exe = AudioCppRuntime.GetServerExecutable();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException(
                    "audio.cpp server not found. Download the Breeze TTS 2 engine first.", exe);
            }

            var modelFileName = GetModelFileName(modelKey);
            var modelPath = GetModelPath(modelKey);
            if (!IsValidLocalModelFile(modelPath, modelFileName))
            {
                throw new FileNotFoundException(
                    File.Exists(modelPath)
                        ? $"The Breeze TTS 2 model file is incomplete: {modelPath}. Delete it and download again."
                        : $"The Breeze TTS 2 model file is missing: {modelPath}",
                    modelPath);
            }

            var port = FindFreeLoopbackPort();
            var configPath = WriteServerConfig(port, backend, modelKey);

            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? AudioCppRuntime.GetSetEngineFolder(),
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
            psi.ArgumentList.Add("--config");
            psi.ArgumentList.Add(configPath);

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start audiocpp_server (breeze_tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog($"Breeze TTS 2 (audio.cpp) server starting — PID: {process.Id}, Cmd: {launchCommand}");

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
            _serverBackend = backend;
            _serverExeStamp = AudioCppRuntime.GetServerExecutableStamp();
            HookProcessExitOnce();

            // The config uses lazy_load, so /health answers within a second or two — the 5 GB
            // model is only read on the first synthesis request. A long deadline here would just
            // hide a crash-at-startup (e.g. a Vulkan build on a box with no Vulkan driver, which
            // dies in the loader with 0xC0000135 before printing anything).
            var deadline = DateTime.UtcNow.AddMinutes(2);
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
                    _serverBackend = null;
                    _serverExeStamp = null;
                    throw new InvalidOperationException(
                        $"audiocpp_server exited during startup (code {exitCode}). "
                        + AudioCppRuntime.DescribeStartupExit(exitCode, backend)
                        + $" Output: {tail}"
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
                $"audiocpp_server did not report healthy within 2 minutes. Last output: {lastOutput}"
                + LaunchCmdSuffix(timeoutLaunchCommand));
        }
        finally
        {
            ServerLock.Release();
        }
    }

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

        var configPath = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "breeze-tts-server.json");
        File.WriteAllText(
            configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Se.WriteToolsLog($"Breeze TTS 2 (audio.cpp): server config written to {configPath} (model={modelKey}, backend={backend})");
        return configPath;
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
    /// Stop the audio.cpp server if running, releasing the loaded model's working set.
    /// audio.cpp never unloads a model on its own once loaded, so this is the only way the
    /// memory comes back.
    /// </summary>
    public static void StopServer() => StopServerInternal();

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverLaunchCommand = null;
        _serverModelKey = null;
        _serverBackend = null;
        _serverExeStamp = null;
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
    /// Import with the reference transcript — the overload <see cref="VoiceCloneImporter"/>
    /// routes to. audio.cpp cannot clone Breeze without the transcript, so getting it written
    /// as the .txt sidecar at import time is what makes the voice clone well.
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

        // audio.cpp resamples the reference itself, but importing at 24 kHz mono matches the
        // Breeze codec's native rate, so the reference is only resampled once, here.
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
            Se.LogError(ex, "Breeze TTS 2 (audio.cpp) voice import failed (ffmpeg conversion).");
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
            Se.LogError(ex, "Breeze TTS 2 (audio.cpp) voice import: failed to write .txt sidecar");
        }

        return true;
    }
}
