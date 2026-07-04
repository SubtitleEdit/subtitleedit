using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenRouter;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Online speech-to-text via OpenRouter's audio transcription API. OpenRouter
/// routes to Whisper / GPT-4o-transcribe / Groq / Chirp behind one endpoint and
/// one API key, and returns the same verbose_json segment/word shape we already
/// parse. See <see cref="OpenRouterSttService"/> for the request encoding.
/// </summary>
public class OpenRouterSttEngine : IOnlineSttEngine
{
    public static string StaticName => "OpenRouter";
    public string Name => StaticName;
    public string Choice => WhisperChoice.OpenRouter;
    public string Url => "https://openrouter.ai/docs/guides/overview/multimodal/stt";

    // OpenRouter proxies Whisper-class models (25 MB caps) but the audio is
    // base64-inflated into a JSON body, so chunk more conservatively.
    private const long UploadThreshold = 18L * 1024 * 1024;
    private const long ChunkSize = 16L * 1024 * 1024;

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
    public long UploadThresholdBytes => UploadThreshold;
    public long ChunkSizeBytes => ChunkSize;

    public string GetAndCreateWhisperFolder() => WhisperHelper.GetWhisperFolder(WhisperChoice.OpenRouter);
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
