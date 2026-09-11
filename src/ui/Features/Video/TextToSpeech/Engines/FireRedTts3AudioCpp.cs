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
/// FireRedTTS3-Base (FireRed Team / Xiaohongshu) run through the audio.cpp runtime — the same
/// pure C++/ggml install <see cref="IndexTts25AudioCpp"/>, <see cref="HiggsTtsAudioCpp"/> and
/// <see cref="FishTtsAudioCpp"/> use (see <see cref="AudioCppRuntime"/>), so the binaries are
/// downloaded once and shared. Zero-shot voice cloning across 24 languages, with the best
/// published speaker similarity of the open cloning models (Seed-TTS-eval avg 78.8, MiniMax
/// MLS-Test avg 84.8; arXiv 2608.17492). 24 kHz output.
///
/// Like the other audio.cpp engines, the server honours a per-request <c>voice_ref</c>, so
/// switching voice does NOT restart the server — only a model or backend change does — which
/// is also what makes per-line cloning ("Clone from video") free here. Unlike Higgs, the model
/// does NOT detect the language of the input text: the request carries an explicit language
/// tag (<see cref="FireRedTts3Languages"/>) and audio.cpp's own default is Chinese, so the
/// language combo is mandatory here and English is pre-selected. The <c>.txt</c> sidecar next to
/// the reference WAV (the transcript convention the cloning engines share) is passed as
/// <c>reference_text</c> and is REQUIRED: the model is a pure in-context continuation - the
/// prompt is <c>&lt;|Lang|&gt;&lt;|sot|&gt;{reference_text}{text}&lt;|eot|&gt;</c> followed by the
/// reference audio - so without the transcript nothing pairs the reference audio with any text
/// and the output is a second or two of noise on every seed (#14480). That is why a per-line
/// clip with no transcript (no original-language subtitle loaded) is not cloned by this engine.
///
/// Cross-lingual cloning (reference in one language, text in another) is the model's weak spot:
/// the single language tag covers both the reference transcript and the text, and measured
/// against an English reference the tag that reads best depends on the target language
/// (Italian and Spanish want the reference's tag, French and German the target's). The tag
/// therefore follows the text as documented, and the docs steer cross-language dubbing to the
/// engines that handle it.
///
/// Licence note: the audio.cpp binaries and the FireRedTTS3 weights are both Apache-2.0, so
/// there is no first-run licence gate — unlike Higgs and Fish on the same runtime.
/// </summary>
public class FireRedTts3AudioCpp : ITtsEngine, IPerLineCloneEngine
{
    public string Name => "FireRedTTS3 (audio.cpp)";
    public string Description => "FireRedTTS3 (FireRed Team) voice cloning in 24 languages, via audio.cpp";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => true;

    // Q8_0 is the default: same 24 kHz output as the original-precision file at 7.6 GB less on
    // disk. audio.cpp names the unquantized package "orig" rather than bf16/f16 because the
    // checkpoint mixes precisions per stage (Qwen3 AR, FireRed DiT flow, RedAE decoder).
    public const string ModelKeyQ8_0 = "Q8_0 (~3.9 GB)";
    public const string ModelKeyOrig = "Original (~11.5 GB)";
    public const string DefaultModelKey = ModelKeyQ8_0;

    public const string ModelQ8_0FileName = "fireredtts3-base-q8_0.gguf";
    public const string ModelOrigFileName = "fireredtts3-base-orig.gguf";

    /// <summary>Family name audio.cpp registers FireRedTTS3 (Base and Instruct) under.</summary>
    public const string FamilyName = "fireredtts3";

    /// <summary>Model id used in the generated server config and in each request body.</summary>
    private const string ServerModelId = "fireredtts3";

    /// <summary>
    /// Exact byte sizes on the audio-cpp/audio.cpp-gguf HuggingFace repo. A truncated GGUF is
    /// the single most common failure here — a download that dies partway leaves a file the
    /// loader rejects with "GGUF tensor data range is out of bounds", so size is checked
    /// before the server is ever started. Same guard as <see cref="IndexTts25AudioCpp"/>.
    /// </summary>
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ModelQ8_0FileName] = 4180334848L,
        [ModelOrigFileName] = 12301253120L,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.FireRedTts3AudioCppModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : ResolveModelKey(saved);
        }

        return modelKey == ModelKeyOrig ? ModelKeyOrig : ModelKeyQ8_0;
    }

    public static string GetModelFileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyOrig ? ModelOrigFileName : ModelQ8_0FileName;

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

        var folder = Path.Combine(Se.TextToSpeechFolder, "FireRedTts3AudioCpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// audio.cpp is pointed at a directory, not a file, so the GGUF gets its own folder under
    /// the shared models root: <c>&lt;data&gt;/audio.cpp/models/FireRedTTS3-Base-GGUF/</c>.
    /// </summary>
    public static string GetSetModelsFolder()
    {
        var folder = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "models", "FireRedTTS3-Base-GGUF");
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
    /// clones only and has no built-in voices. The pack ships at 16 kHz; FireRedTTS3 clones from
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
                VoiceSeedHelper.CopyOrResample(src, dest, 24000, "FireRedTTS3 (audio.cpp)");

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
            Se.LogError(ex, "FireRedTTS3 (audio.cpp): voice seeding from the shared voice pack failed");
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
            // so a symlinked GGUF (a reasonable way to share a 3.9 GB model between apps or
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

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyQ8_0, ModelKeyOrig });

    // No "Auto": the model has no language detection and audio.cpp defaults an unset tag to
    // Chinese, so every request carries an explicit tag (English when nothing is picked).
    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) =>
        Task.FromResult(FireRedTts3Languages.All);

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    /// <summary>
    /// <see cref="IPerLineCloneEngine"/>: the server takes <c>voice_ref</c> as a path per request,
    /// so the voice simply points at the cut clip - nothing is staged into this engine's own
    /// folders. audio.cpp resamples the reference itself, so the 24 kHz clip is used as cut.
    /// </summary>
    public Voice? MakePerLineCloneVoice(string clipFileName, string voiceName)
    {
        // No usable transcript means no cloning for this line: the prompt pairs the reference
        // audio with its transcript, and with that missing the model returns noise rather than
        // the line (see the class remarks). Null makes the caller fall back to an ordinary
        // voice for the line, which at least speaks it.
        if (string.IsNullOrWhiteSpace(Qwen3TtsCrispAsr.TryReadUsableTranscript(clipFileName)))
        {
            Se.WriteToolsLog(
                $"FireRedTTS3 (audio.cpp): no usable transcript beside '{clipFileName}' - not cloning this line "
                + "(load the original-language subtitle so the clips get their transcripts)");
            return null;
        }

        return new Voice(new IndexTtsVoice(voiceName, clipFileName));
    }

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
                "FireRedTTS3 (audio.cpp) requires a reference voice WAV. "
                + "Import one via the voice settings, then pick it in the voice combo. "
                + "3-10 s of clean speech works best.");
        }

        var modelKey = ResolveModelKey(model);
        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        // audio.cpp's OpenAI-style speech payload. Voice cloning goes through `voice_ref`,
        // honoured per request — no server restart when the user switches voice.
        // The language tag is always sent: audio.cpp's default for this family is Chinese,
        // so an omitted tag would read every non-Chinese subtitle with a Chinese frontend.
        var languageTag = FireRedTts3Languages.ResolveLanguageTag(language);
        var options = new Dictionary<string, object>
        {
            ["language"] = languageTag,
        };

        // The transcript of the reference WAV from the shared .txt sidecar convention. Not
        // optional here: the model continues the reference in-context and needs its text to
        // pair with the audio - without it every seed produced one or two seconds of noise
        // (#14480). Failing with a message beats generating that.
        var referenceText = Qwen3TtsCrispAsr.TryReadUsableTranscript(indexVoice.FilePath);
        if (string.IsNullOrWhiteSpace(referenceText))
        {
            throw new InvalidOperationException(
                $"FireRedTTS3 (audio.cpp) needs a transcript of the reference voice '{indexVoice}' "
                + "(a .txt file with the same name next to the WAV). Without it the model produces "
                + "noise instead of speech. Pick the voice again to be asked for the transcript, "
                + "or add the .txt file by hand.");
        }

        options["reference_text"] = referenceText;

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
            ["options"] = options,
        };

        var body = JsonSerializer.Serialize(payload);
        Se.WriteToolsLog($"FireRedTTS3 (audio.cpp): POST {ServerBaseUrl}/v1/audio/speech (voice={indexVoice}, language={languageTag}, textLen={text.Length}, preparedReference={referencePrepared})");

        using var content = new StringContent(body, Encoding.UTF8, "application/json");
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

            var failMsg = $"FireRedTTS3 (audio.cpp) request failed — Voice: {indexVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "FireRedTTS3 (audio.cpp) — the audiocpp_server process crashed during synthesis."
                    : "FireRedTTS3 (audio.cpp) request failed — the connection to audiocpp_server was dropped.")
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
                var errMsg = $"FireRedTTS3 (audio.cpp) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {indexVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"FireRedTTS3 (audio.cpp) synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LaunchCmdSuffix(launchCommand));
            }

            await using var fileStream = File.Create(outputFileName);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }
        return new TtsResult(outputFileName, text);
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
                    "audio.cpp server not found. Download the FireRedTTS3 engine first.", exe);
            }

            var modelFileName = GetModelFileName(modelKey);
            var modelPath = GetModelPath(modelKey);
            if (!IsValidLocalModelFile(modelPath, modelFileName))
            {
                throw new FileNotFoundException(
                    File.Exists(modelPath)
                        ? $"The FireRedTTS3 model file is incomplete: {modelPath}. Delete it and download again."
                        : $"The FireRedTTS3 model file is missing: {modelPath}",
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
                ?? throw new InvalidOperationException("Failed to start audiocpp_server (fireredtts3)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog($"FireRedTTS3 (audio.cpp) server starting — PID: {process.Id}, Cmd: {launchCommand}");

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

            // The config uses lazy_load, so /health answers within a second or two — the 3.9 GB
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
                    // The Base package declares the clone task only (Instruct adds tts/design).
                    ["task"] = "clon",
                    ["mode"] = "offline",
                },
            },
        };

        var configPath = Path.Combine(AudioCppRuntime.GetSetEngineFolder(), "fireredtts3-server.json");
        File.WriteAllText(
            configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Se.WriteToolsLog($"FireRedTTS3 (audio.cpp): server config written to {configPath} (model={modelKey}, backend={backend})");
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
    /// routes to. FireRedTTS3 cannot clone without the transcript (see the class remarks), so it
    /// is kept as the .txt sidecar Speak passes as reference_text; a voice imported without one
    /// is asked for it when picked.
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
            Se.LogError(ex, "FireRedTTS3 (audio.cpp) voice import failed (ffmpeg conversion).");
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
            Se.LogError(ex, "FireRedTTS3 (audio.cpp) voice import: failed to write .txt sidecar");
        }

        return true;
    }
}
