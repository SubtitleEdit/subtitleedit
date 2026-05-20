using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Shared voice-install gate for TTS engines: ensures the selected voice is present,
/// prompting a download when it is not.
/// </summary>
public static class TtsVoiceInstaller
{
    /// <summary>
    /// Ensures the given voice is installed, prompting a download if needed.
    /// Returns false only when the user cancels the download.
    /// </summary>
    public static async Task<bool> EnsureVoiceInstalled(ITtsEngine engine, Voice voice, Window? window, IWindowService windowService)
    {
        if (window == null || engine.IsVoiceInstalled(voice))
        {
            return true;
        }

        // Only Piper has per-voice downloads; every other engine's IsVoiceInstalled is always true.
        if (voice.EngineVoice is PiperVoice piperVoice)
        {
            var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(
                window, vm => vm.StartDownloadPiperVoice(piperVoice));
            if (!dlResult.OkPressed)
            {
                SafeDelete(Path.Combine(Piper.GetSetPiperFolder(), piperVoice.ModelShort));
                SafeDelete(Path.Combine(Piper.GetSetPiperFolder(), piperVoice.ConfigShort));
                return false;
            }
        }

        return true;
    }

    private static void SafeDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // ignore
        }
    }
}
