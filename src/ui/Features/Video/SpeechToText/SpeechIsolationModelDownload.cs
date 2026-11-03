using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
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
    public static Task<bool> EnsureDownloadedAsync(Window window, IWindowService windowService, ISpeechToTextEngine crispAsrEngine, string featureName)
    {
        return EnsureModelDownloadedAsync(
            window,
            windowService,
            crispAsrEngine,
            SpeechIsolationModel.ToWhisperModel(),
            SpeechIsolationModel.DisplayName,
            featureName,
            Se.Language.Video.AudioToText.ASourceSeparationModel);
    }

    /// <summary>
    /// Makes sure a model that a Crisp ASR option needs on top of the transcription model is in
    /// the Crisp ASR models folder, prompting for the download when it is missing.
    /// </summary>
    /// <param name="modelKind">What the model is, as the user reads it: "a source separation model" (localized).</param>
    /// <returns>False when the user declines or cancels - the run must not start in that case.</returns>
    public static async Task<bool> EnsureModelDownloadedAsync(
        Window window,
        IWindowService windowService,
        ISpeechToTextEngine crispAsrEngine,
        WhisperModel model,
        string displayName,
        string featureName,
        string modelKind)
    {
        if (File.Exists(crispAsrEngine.GetModelForCmdLine(model.Name)))
        {
            return true;
        }

        var l = Se.Language.Video.AudioToText;
        var answer = await MessageBox.Show(
            window,
            string.Format(l.DownloadXQuestion, displayName),
            string.Format(l.FeatureXRequiresYDownloadZ, featureName, modelKind, model.Name, model.Size),
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return false;
        }

        return await DownloadModelAsync(window, windowService, crispAsrEngine, model, displayName);
    }

    /// <summary>
    /// Downloads a model into the engine's models folder without asking first - for callers that
    /// already got the user's go-ahead, e.g. one prompt covering several models.
    /// </summary>
    /// <returns>False when the download was cancelled or failed.</returns>
    public static async Task<bool> DownloadModelAsync(
        Window window,
        IWindowService windowService,
        ISpeechToTextEngine crispAsrEngine,
        WhisperModel model,
        string displayName)
    {
        var displayModel = new SpeechToTextModelDisplay
        {
            Model = model,
            Display = $"{displayName} ({model.Size})",
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
        return vm.OkPressed && File.Exists(crispAsrEngine.GetModelForCmdLine(model.Name));
    }
}
