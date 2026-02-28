using System.Collections.Generic;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.AudioToText;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public interface ISpeechToTextEngine
{
    string Name { get; }
    string Choice { get; }
    string Url { get; }
    List<WhisperLanguage> Languages { get; }
    List<WhisperModel> Models { get; }
    string Extension { get;  }
    string UnpackSkipFolder { get; }
    bool IsEngineInstalled();
    bool CanBeDownloaded();
    string GetAndCreateWhisperFolder();
    string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel);
    string GetExecutable();
    bool IsModelInstalled(WhisperModel model);
    string GetModelForCmdLine(string modelName);
    Task<string> GetHelpText();
    string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url);
}
