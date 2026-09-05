using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenRouter;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Online speech-to-text via OpenRouter's audio transcription API. OpenRouter
/// routes to Whisper / GPT transcription / Groq / Chirp behind one endpoint and
/// one API key. Response formats are selected per model; see
/// <see cref="OpenRouterSttService"/> for the request encoding.
/// </summary>
public class OpenRouterSttEngine : IOnlineSttEngine
{
    public static string StaticName => "OpenRouter";
    public string Name => StaticName;
    public string Choice => WhisperChoice.OpenRouter;
    public string Url => "https://openrouter.ai/docs/guides/overview/multimodal/stt";

    // OpenRouter's upstream provider timeout is 60 seconds, so audio duration
    // matters more than the Whisper 25 MB cap: a provider must finish
    // transcribing the whole chunk within that window. Online engines upload
    // 32 kbps mp3 (~240 KB/min), so ~2.5 MB keeps chunks near 10 minutes.
    private const long UploadThreshold = 3L * 1024 * 1024;
    private const long ChunkSize = 2560L * 1024;

    // Chirp models need wav instead of mp3 (see OpenRouterSttService.RequiresWavAudio).
    // Chirp via OpenRouter also turns out to have a much tighter constraint than the
    // 60-second provider timeout above: it looks like the synchronous (non-long-running)
    // Google Speech-to-Text API underneath, which has its own hard ~60-second audio
    // duration cap independent of file size or timeout - confirmed empirically by sending
    // identical audio at 60s (succeeds), 65s and 180s (both fail with the same opaque
    // "Provider returned 400", regardless of language/prompt, which are not the cause).
    // Sizing has to survive silence snapping: ComputeAdjustedBoundaries moves each cut up
    // to maxOffsetSeconds (10s by default) to land on a silence, and a chunk sits between
    // two independently snapped cuts - so a chunk can run up to 20s longer than its target,
    // not 10s. 16 kHz mono 16-bit PCM is ~32 KB/s, so 1 MB targets ~33 seconds and the
    // worst case is ~53 seconds, still under the confirmed ~60s failure point. (1.4 MB
    // would target ~45s but allow ~65s, i.e. past the cap on an unlucky pair of snaps.)
    private const long WavUploadThreshold = 1152L * 1024;
    private const long WavChunkSize = 1024L * 1024;

    public override string ToString() => Name;

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();
    public List<WhisperModel> Models => new();

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled() => true;
    public bool CanBeDownloaded() => false;

    public ISttTranscriber? CreateTranscriber(out string? configErrorMessage)
    {
        var settings = OpenRouterSttService.GetSettingsFromConfiguration();
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            configErrorMessage = Se.Language.General.OnlineSttApiKeyMissing;
            return null;
        }

        configErrorMessage = null;
        return new OpenRouterSttService(settings);
    }

    public string ProbeUrl => "https://openrouter.ai";

    private static bool UsesWavAudio => OpenRouterSttService.RequiresWavAudio(Se.Settings.Tools.OpenRouterSttModel);
    public long UploadThresholdBytes => UsesWavAudio ? WavUploadThreshold : UploadThreshold;
    public long ChunkSizeBytes => UsesWavAudio ? WavChunkSize : ChunkSize;

    public string GetAndCreateWhisperFolder() => WhisperHelper.GetWhisperFolder(WhisperChoice.OpenRouter) ?? string.Empty;
    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel) => new WhisperModel().ModelFolder;
    public string GetExecutable() => string.Empty;
    public bool IsModelInstalled(WhisperModel model) => true;
    public string GetModelForCmdLine(string modelName) => modelName;
    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url) => string.Empty;

    public async Task<string> GetHelpText()
    {
        var uri = new Uri("avares://SubtitleEdit/Assets/SpeechToText/OpenRouter.txt");
        try
        {
            await using var stream = AssetLoader.Open(uri);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch
        {
            return "OpenRouter speech-to-text service.\n\n" +
                   "Create an API key at https://openrouter.ai/keys, then set it in the OpenRouter fields.\n" +
                   "Pick a transcription model such as openai/whisper-1 or openai/whisper-large-v3.";
        }
    }

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.OpenRouterSttPrompt;
        set => Se.Settings.Tools.OpenRouterSttPrompt = value;
    }
}
