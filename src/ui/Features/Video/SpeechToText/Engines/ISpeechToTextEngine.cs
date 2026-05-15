using System.Collections.Generic;
using System.IO;
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

    /// <summary>
    /// Approximate download size as user-facing text (e.g. "~14 MB"), platform-aware where
    /// the install archive differs between Windows/Linux/macOS. Returns an empty string for
    /// engines that don't need downloading (e.g. cloud APIs) or where the size isn't known.
    /// Used to inform the user up front in the engine picker before any download starts.
    /// </summary>
    string DownloadSizeText => string.Empty;

    string GetAndCreateWhisperFolder();
    string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel);
    string GetExecutable();
    string GetExecutableFileName() => Path.GetFileName(GetExecutable());
    bool IsModelInstalled(WhisperModel model);
    string GetModelForCmdLine(string modelName);
    Task<string> GetHelpText();
    string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url);
    string CommandLineParameter { get; set; }
}
