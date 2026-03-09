using Nikse.SubtitleEdit.Core.AudioToText;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class WhisperEngineHttpApi : ISpeechToTextEngine
{
    public static string StaticName => "Whisper HTTP API";
    public string Name => StaticName;
    public string Choice => WhisperChoice.HttpApi;
    public string Url => "https://platform.openai.com/docs/guides/speech-to-text";

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();

    public List<WhisperModel> Models => new List<WhisperModel>
    {
        new WhisperModel { Name = "whisper-1", Size = "cloud" },
        new WhisperModel { Name = "gpt-4o-transcribe", Size = "cloud" },
        new WhisperModel { Name = "gpt-4o-mini-transcribe", Size = "cloud" },
    };

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled() => true;
    public bool CanBeDownloaded() => false;

    public string GetAndCreateWhisperFolder() => string.Empty;
    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel) => string.Empty;
    public string GetExecutable() => string.Empty;
    public bool IsModelInstalled(WhisperModel model) => true;
    public string GetModelForCmdLine(string modelName) => modelName;

    public Task<string> GetHelpText() => Task.FromResult(
        "Transcribes audio via HTTP API (e.g. OpenAI Whisper API).\n\n" +
        "Configure the API URL and API key in the fields below.\n" +
        "Default URL: https://api.openai.com/v1/audio/transcriptions\n\n" +
        "Compatible with any service that implements the OpenAI audio transcription API.");

    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url) => string.Empty;

    public override string ToString() => Name;
}
