using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.UiLogic.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Zyphra ZONOS3-Mini run through CrispASR's <c>zonos3</c> backend (v0.8.55+). Hybrid
/// Gated-DeltaNet/attention MoE backbone (800M total, ~100M active per token) emitting 15 Hz
/// codec frames, rendered to 44.1 kHz by the ZAC iSTFT codec. 43 languages, Apache-2.0.
/// Replaced Zonos-v0.1 in this engine on 2026-11-22: in an ASR round-trip on 27 subtitle lines
/// it scored 0-5% WER in en/fr/es/it/de where v0.1 scored 12-42%, at ~1 s per line on Metal
/// against ~20 s.
///
/// Zyphra's own GGUFs from <c>Zyphra/ZONOS3-Mini</c> (pinned to <see cref="RepoRevision"/>),
/// in their own subfolder of CrispASR/models because the upstream names are generic:
///  - backbone-ptq.gguf : the language model
///  - zac-keep2.gguf    : the pruned codec, ~3x faster than the full zac.gguf
///  - voices/*.embedding.npy : the preset speakers, see <see cref="ZonosVoicePresets"/>
///
/// NOT a cloning engine: the zonos3 backend takes a speaker only as a precomputed 192-d
/// embedding, and cloning from a WAV (spk-encoder.gguf) is not wired upstream yet. So it
/// offers Zyphra's presets plus <see cref="DefaultVoiceName"/>, the unconditioned prompt for
/// languages without a preset (Danish came out right on two of four lines that way).
///
/// No language parameter: the model reads the language from the text, and the backend has no
/// <c>-l</c>. Upstream's NeMo text normalizer is not linked, so numbers are read as written.
/// </summary>
public class ZonosTtsCrispAsr : ITtsEngine
{
    public string Name => "Zonos TTS (CrispASR)";
    public string Description => "Zyphra ZONOS3-Mini at 44.1 kHz, 27 preset voices in 10 languages, via CrispASR";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => false;
    public bool SupportsPerLineVoiceCloning => false;

    public const string TalkerFileName = "backbone-ptq.gguf";
    public const string CodecFileName = "zac-keep2.gguf";

    public const string BackendName = "zonos3";

    /// <summary>The CrispASR release that added the zonos3 backend.</summary>
    public static readonly Version MinimumCrispAsrVersion = new(0, 8, 55);

    /// <summary>Zyphra/ZONOS3-Mini commit the URLs and hashes are pinned to.</summary>
    public const string RepoRevision = "2921b8821772b0bd10a04456863d21abf9d2f283";

    /// <summary>The unconditioned voice: no speaker embedding is sent.</summary>
    public const string DefaultVoiceName = "Default (no preset)";

    // Exact byte sizes in the HF tree at RepoRevision. Used to reject truncated files.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [TalkerFileName] = 663150624L,
        [CodecFileName] = 452325440L,
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
    private static readonly CrispAsrTtsServer Server = new("crispasr (zonos3)", "Zonos TTS (CrispASR)", HttpClient);
    private static readonly SemaphoreSlim ServerLock = new(1, 1);

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetCrispAsrExecutable()));
    }

    public override string ToString() => Name;

    /// <summary>
    /// Path to the crispasr executable installed by the speech-to-text feature. Shared with
    /// Chatterbox / Qwen3 TTS / IndexTTS (CrispASR) / all CrispASR ASR engines.
    /// </summary>
    public static string GetCrispAsrExecutable()
    {
        return new CrispAsrCohere().GetExecutable();
    }

    /// <summary>
    /// Mirrors <see cref="IndexTtsCrispAsr.GetEngineUpdateStatus"/> — reads the speech-to-text
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

        var folder = Path.Combine(Se.TextToSpeechFolder, "ZonosTtsCrispAsr");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetModelsFolder()
    {
        // Like the other CrispASR-backed engines, the GGUFs live under CrispASR/models/ rather
        // than TextToSpeech/ - in a subfolder, as Zyphra's file names are generic. The
        // Zonos-v0.1 files an older SE put directly in CrispASR/models/ are no longer used.
        var modelsFolder = Path.Combine(Se.CrispAsrFolder, "models", "zonos3-mini");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public static string GetSetVoicesFolder()
    {
        var folder = Path.Combine(GetSetModelsFolder(), "voices");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetTalkerPath() =>
        Path.Combine(GetSetModelsFolder(), TalkerFileName);

    public static string GetCodecPath() =>
        Path.Combine(GetSetModelsFolder(), CodecFileName);

    internal static string GetVoicePath(ZonosVoicePresets.Preset preset) =>
        Path.Combine(GetSetVoicesFolder(), preset.FileName);

    public static bool IsValidVoiceFile(string path)
    {
        try
        {
            return File.Exists(path) && new FileInfo(path).Length == ZonosVoicePresets.FileSize;
        }
        catch
        {
            return false;
        }
    }

    public static bool AreModelsInstalled() =>
        IsValidLocalModelFile(GetTalkerPath(), TalkerFileName)
        && IsValidLocalModelFile(GetCodecPath(), CodecFileName)
        && ZonosVoicePresets.All.All(p => IsValidVoiceFile(GetVoicePath(p)));

    /// <summary>True when the installed crispasr has the zonos3 backend.</summary>
    public static bool IsCrispAsrZonos3Capable() =>
        CrispAsrVersion.IsAtLeast(CrispAsrVersion.TryGet(GetCrispAsrExecutable()), MinimumCrispAsrVersion);

    public Task<Voice[]> GetVoices(string language)
    {
        // The presets in Zyphra's order within each language, then the unconditioned prompt for
        // every other language. Listed whether or not the files are downloaded yet: the engine
        // installer fetches them all together with the models.
        var voices = new List<Voice> { new(new ZonosTtsVoice(DefaultVoiceName, string.Empty)) };
        foreach (var preset in ZonosVoicePresets.All)
        {
            voices.Add(new Voice(new ZonosTtsVoice(preset.DisplayName, preset.FileStem)));
        }

        return Task.FromResult(voices.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(Array.Empty<string>());

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    public async Task<TtsResult> Speak(
        string text,
        string outputFolder,
        Voice voice,
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken)
    {
        if (voice.EngineVoice is not ZonosTtsVoice zonosVoice)
        {
            throw new ArgumentException("Voice is not a ZonosTtsVoice");
        }

        await EnsureServerRunningAsync(cancellationToken);

        var outputFileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");

        // OpenAI-compatible /v1/audio/speech payload. "voice" is the preset's file stem, which
        // the server resolves to <stem>.embedding.npy under --voice-dir; no "voice" = the
        // unconditioned prompt. The zonos3 backend switches speaker per request, so one server
        // serves every voice.
        var payload = new Dictionary<string, object>
        {
            ["input"] = text,
            ["response_format"] = "wav",
        };
        var preset = ZonosVoicePresets.FindByStem(zonosVoice.PresetStem);
        if (preset != null)
        {
            payload["voice"] = preset.FileStem;
        }

        // No CrispAsrTtsProvenance.AddSpeechAttestations: nothing is cloned, so there is no
        // reference to attest consent for. Zyphra does not say whether its presets are real
        // people, so no --speaker-identity is claimed either way (the server logs a warning).

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Zonos TTS (CrispASR): POST {Server.BaseUrl}/v1/audio/speech (voice={zonosVoice}, textLen={text.Length})");

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

            var failMsg = $"Zonos TTS (CrispASR) request failed — Voice: {zonosVoice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LocalServerHelper.LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Zonos TTS (CrispASR) — the crispasr server crashed during synthesis."
                    : "Zonos TTS (CrispASR) request failed — the connection to the crispasr server was dropped.")
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
                var errMsg = $"Zonos TTS (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {zonosVoice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Zonos TTS (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LocalServerHelper.LaunchCmdSuffix(launchCommand));
            }

            await using var fileStream = File.Create(outputFileName);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
    }

    private static async Task EnsureServerRunningAsync(CancellationToken ct)
    {
        if (Server.IsAlive)
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (Server.IsAlive)
            {
                return;
            }

            Server.Stop();

            var exe = GetCrispAsrExecutable();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException(
                    "CrispASR executable not found. Install CrispASR via Video → Audio to text first.", exe);
            }

            // crispasr has no --auto-download for ZONOS3, so the files must be staged by
            // ZonosTtsCrispAsrDownloadService (the engine installer offers it first).
            if (!AreModelsInstalled())
            {
                throw new FileNotFoundException(
                    "Zonos TTS (CrispASR) models are missing. Use the model download button to fetch them.", GetTalkerPath());
            }

            var port = LocalServerHelper.FindFreeLoopbackPort();
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
            psi.ArgumentList.Add(BackendName);
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(GetTalkerPath());
            psi.ArgumentList.Add("--codec-model");
            psi.ArgumentList.Add(GetCodecPath());
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());
            CrispAsrTtsProvenance.AddServerMarkingArgs(psi.ArgumentList, exe);

            Server.Start(psi, port);
            await Server.WaitUntilHealthyAsync(5, ct);
        }
        finally
        {
            ServerLock.Release();
        }
    }

    /// <summary>
    /// Stop the running crispasr (zonos3) server if any, releasing GPU memory. Called by
    /// <c>TextToSpeechViewModel</c> when starting synthesis on a different engine or when the
    /// TTS window closes, so the CrispASR-based TTS engines don't pile up in VRAM.
    /// </summary>
    public static void StopServer() => Server.Stop();

    /// <summary>
    /// Nothing to import: the zonos3 backend cannot turn a reference recording into a speaker
    /// embedding yet. Unreachable from the UI while <see cref="SupportsVoiceCloning"/> is false.
    /// </summary>
    public bool ImportVoice(string fileName) => false;
}
