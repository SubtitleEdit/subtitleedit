using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public static class SpeechIsolationModelDownload
{
    /// <summary>
    /// Makes sure the source separation model is in the Crisp ASR models folder, prompting for
    /// the download when it is missing. Speech-to-text and text-to-speech share the one file.
    /// </summary>
    /// <param name="featureName">The option that needs the model, as the user sees it.</param>
    /// <returns>False when the user declines or cancels - the run must not start in that case.</returns>
    public static async Task<bool> EnsureDownloadedAsync(Window window, IWindowService windowService, ISpeechToTextEngine crispAsrEngine, string featureName)
    {
        var isolationModel = SpeechIsolationModel.ToWhisperModel();
        if (File.Exists(crispAsrEngine.GetModelForCmdLine(isolationModel.Name)))
        {
            return true;
        }

        var answer = await MessageBox.Show(
            window,
            $"Download {SpeechIsolationModel.DisplayName}?",
            $"'{featureName}' requires a source separation model.\nDownload and use {isolationModel.Name} ({isolationModel.Size})?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return false;
        }

        var displayModel = new SpeechToTextModelDisplay
        {
            Model = isolationModel,
            Display = $"{SpeechIsolationModel.DisplayName} ({isolationModel.Size})",
            Engine = crispAsrEngine,
        };
        var models = new ObservableCollection<SpeechToTextModelDisplay> { displayModel };
        var vm = await windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
            window, viewModel =>
            {
                viewModel.SetModels(models, crispAsrEngine, displayModel);
                viewModel.StartDownload();
            });

        // A download that was cancelled or failed must not start a run that then cannot find the model.
        return vm.OkPressed && File.Exists(crispAsrEngine.GetModelForCmdLine(isolationModel.Name));
    }
}
