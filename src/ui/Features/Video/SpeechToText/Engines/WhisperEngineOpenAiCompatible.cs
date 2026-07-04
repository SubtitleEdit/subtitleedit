using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class OpenAiCompatibleSttEngine : IOnlineSttEngine
{
    public static string StaticName => "OpenAI Compatible Server";
    public string Name => StaticName;
    public string Choice => WhisperChoice.OpenAiCompatible;
    public string Url => "https://platform.openai.com/docs/guides/speech-to-text";

    public ISttTranscriber? CreateTranscriber(out string? configErrorMessage)
    {
        var settings = OpenAiSttService.GetSettingsFromConfiguration();
        if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
        {
            configErrorMessage = Se.Language.General.OpenAiCompatibleSttUrlMissing;
            return null;
        }

        configErrorMessage = null;
        return new OpenAiSttService(settings);
    }

    public string ProbeUrl => Se.Settings.Tools.OpenAiCompatibleSttUrl;
    public long UploadThresholdBytes => OpenAiSttChunker.DefaultThresholdBytes;
    public long ChunkSizeBytes => OpenAiSttChunker.DefaultChunkSizeBytes;

    public override string ToString()
    {
        return Name;
    }

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();

    public List<WhisperModel> Models
    {
        get
        {
            // Return empty list as model is configured in settings
            return new List<WhisperModel>();
        }
    }

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled()
    {
        // Always returns true as this is a cloud service
        return true;
    }

    public bool CanBeDownloaded()
    {
        return false;
    }

    public string GetAndCreateWhisperFolder()
    {
        return WhisperHelper.GetWhisperFolder(WhisperChoice.OpenAiCompatible);
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        return new WhisperModel().ModelFolder;
    }

    public string GetExecutable()
    {
        return string.Empty;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        // Model is on server side
        return true;
    }

    public string GetModelForCmdLine(string modelName)
    {
        return modelName;
    }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{assetName}");

        try
        {
            await using var stream = AssetLoader.Open(uri);
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            return contents;
        }
        catch
        {
            return "OpenAI Compatible Speech-to-Text service.\n\n" +
                   "Configure the endpoint URL and API key in Settings > Tools > Audio to Text.\n" +
                   "Supports any OpenAI-compatible STT API endpoint.";
        }
    }

    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        return string.Empty;
    }

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.OpenAiCompatibleSttPrompt;
        set => Se.Settings.Tools.OpenAiCompatibleSttPrompt = value;
    }
}
