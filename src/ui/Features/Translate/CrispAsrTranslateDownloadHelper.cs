using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// Shared download/availability logic for the <see cref="CrispAsrMadladTranslate"/> auto-translate engine,
/// used by both the auto-translate window and batch convert.
/// </summary>
public static class CrispAsrTranslateDownloadHelper
{
    /// <summary>
    /// True when the CrispASR engine is installed and a MADLAD model is available. When
    /// <paramref name="modelName"/> is given, that specific model must be installed; otherwise
    /// any installed model counts.
    /// </summary>
    public static bool IsReady(string? modelName = null)
    {
        var engine = new CrispAsrMadlad();
        if (!engine.IsEngineInstalled())
        {
            return false;
        }

        if (!string.IsNullOrEmpty(modelName))
        {
            return File.Exists(engine.GetModelForCmdLine(modelName));
        }

        return GetInstalledModelPath() != null;
    }

    /// <summary>
    /// True when the CrispASR engine binary is installed but an updated build is available - the
    /// install sidecar hash no longer matches the latest known build. Mirrors the amber status dot
    /// shown next to the engine in the auto-translate combo.
    /// </summary>
    public static bool IsEngineUpdateAvailable()
    {
        var engine = new CrispAsrMadlad();
        return engine.IsEngineInstalled()
            && DownloadHashManager.GetSidecarStatus(engine.GetAndCreateWhisperFolder()) == DownloadHashManager.UpdateStatus.UpdateAvailable;
    }

    /// <summary>
    /// Returns the path of an installed MADLAD model - the one from settings if it still exists,
    /// otherwise any known model file found in the CrispASR models folder.
    /// </summary>
    public static string? GetInstalledModelPath()
    {
        var configured = Se.Settings.AutoTranslate.CrispAsrModel;
        if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
        {
            return configured;
        }

        var engine = new CrispAsrMadlad();
        foreach (var model in engine.Models)
        {
            var path = engine.GetModelForCmdLine(model.Name);
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    /// <summary>
    /// Fills <paramref name="target"/> with the available MADLAD models (with download status) and
    /// returns the one matching <paramref name="selectModelName"/>, or the first model otherwise.
    /// </summary>
    public static SpeechToTextModelDisplay? PopulateModels(ObservableCollection<SpeechToTextModelDisplay> target, string? selectModelName)
    {
        target.Clear();
        var engine = new CrispAsrMadlad();
        SpeechToTextModelDisplay? toSelect = null;
        foreach (var model in engine.Models)
        {
            var display = new SpeechToTextModelDisplay { Model = model, Engine = engine };
            display.RefreshDownloadStatus();
            target.Add(display);
            if (model.Name == selectModelName)
            {
                toSelect = display;
            }
        }

        return toSelect ?? target.FirstOrDefault();
    }

    /// <summary>
    /// Opens the download window(s) for the CrispASR engine binary and a MADLAD model and persists the result.
    /// </summary>
    /// <returns>The name of the downloaded model, or null if nothing was downloaded.</returns>
    public static async Task<string?> DownloadAsync(Window owner, IWindowService windowService, string? preselectModelName, bool autoStartModelDownload = false)
    {
        var engine = new CrispAsrMadlad();

        if (!engine.IsEngineInstalled())
        {
            // Madlad is a CPU-bound translation model — the small CPU build is the right default
            // here, no need to make the user pick between Vulkan/CUDA/CPU. Other CrispASR flows
            // (speech-to-text, Chatterbox TTS) still prompt because those benefit from a GPU build.
            var engineVm = await windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
                owner, vm =>
                {
                    vm.Engine = engine;
                    vm.CrispAsrWindowsVariant = "cpu";
                    vm.StartDownload();
                });

            if (!engineVm.OkPressed || !engine.IsEngineInstalled())
            {
                return null;
            }
        }

        var models = new ObservableCollection<SpeechToTextModelDisplay>();
        SpeechToTextModelDisplay? selectedModel = null;
        foreach (var model in engine.Models)
        {
            var display = new SpeechToTextModelDisplay
            {
                Model = model,
                Engine = engine,
            };
            models.Add(display);
            if (model.Name == preselectModelName)
            {
                selectedModel = display;
            }
        }

        var modelsVm = await windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
            owner, vm =>
            {
                vm.SetModels(models, engine, selectedModel);
                if (autoStartModelDownload && selectedModel != null)
                {
                    vm.StartDownload();
                }
            });

        if (modelsVm is { OkPressed: true, SelectedModel: not null })
        {
            Se.Settings.AutoTranslate.CrispAsrExe = engine.GetExecutable();
            Configuration.Settings.Tools.AutoTranslateCrispAsrExe = Se.Settings.AutoTranslate.CrispAsrExe;
            Se.Settings.AutoTranslate.CrispAsrModel = engine.GetModelForCmdLine(modelsVm.SelectedModel.Model.Name);
            Configuration.Settings.Tools.AutoTranslateCrispAsrModel = Se.Settings.AutoTranslate.CrispAsrModel;
            Se.SaveSettings();
            return modelsVm.SelectedModel.Model.Name;
        }

        return null;
    }

    /// <summary>
    /// Forces a re-download of the CrispASR engine binary to apply an available update. The MADLAD
    /// models are left untouched - only the engine executable (and its sidecar hash) is refreshed.
    /// </summary>
    /// <returns>True if the engine was re-installed.</returns>
    public static async Task<bool> UpdateEngineAsync(Window owner, IWindowService windowService)
    {
        var engine = new CrispAsrMadlad();

        // Same small CPU build the regular CrispASR/MADLAD flow installs - this is a CPU-bound
        // translation model, so there is no GPU variant to choose between.
        var engineVm = await windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
            owner, vm =>
            {
                vm.Engine = engine;
                vm.CrispAsrWindowsVariant = "cpu";
                vm.StartDownload();
            });

        if (!engineVm.OkPressed || !engine.IsEngineInstalled())
        {
            return false;
        }

        Se.Settings.AutoTranslate.CrispAsrExe = engine.GetExecutable();
        Configuration.Settings.Tools.AutoTranslateCrispAsrExe = Se.Settings.AutoTranslate.CrispAsrExe;
        Se.SaveSettings();
        return true;
    }

    /// <summary>
    /// Ensures the CrispASR engine and a MADLAD model are installed, prompting the user to download them if not.
    /// </summary>
    /// <returns>True if the engine is ready to use.</returns>
    public static async Task<bool> EnsureReadyAsync(Window owner, IWindowService windowService, string? preselectModelName)
    {
        var engine = new CrispAsrMadlad();
        var engineInstalled = engine.IsEngineInstalled();

        // The model we need on disk: the specific one the caller picked, or any installed one as a fallback.
        var wantedModelPath = !string.IsNullOrEmpty(preselectModelName)
            ? engine.GetModelForCmdLine(preselectModelName)
            : GetInstalledModelPath();
        var modelInstalled = wantedModelPath != null && File.Exists(wantedModelPath);

        if (engineInstalled && modelInstalled)
        {
            // self-heal: point settings at the binary/model that are actually on disk
            if (Se.Settings.AutoTranslate.CrispAsrExe != engine.GetExecutable() ||
                Se.Settings.AutoTranslate.CrispAsrModel != wantedModelPath)
            {
                Se.Settings.AutoTranslate.CrispAsrExe = engine.GetExecutable();
                Se.Settings.AutoTranslate.CrispAsrModel = wantedModelPath!;
                Configuration.Settings.Tools.AutoTranslateCrispAsrExe = Se.Settings.AutoTranslate.CrispAsrExe;
                Configuration.Settings.Tools.AutoTranslateCrispAsrModel = wantedModelPath!;
                Se.SaveSettings();
            }

            return true;
        }

        string message;
        if (!engineInstalled && !modelInstalled)
        {
            message = $"{CrispAsrMadladTranslate.StaticName} requires the CrispASR engine and a MADLAD model to be downloaded. Download now?";
        }
        else if (!engineInstalled)
        {
            message = $"{CrispAsrMadladTranslate.StaticName} requires the CrispASR engine to be downloaded. Download now?";
        }
        else
        {
            message = $"{CrispAsrMadladTranslate.StaticName} requires the selected MADLAD model to be downloaded. Download now?";
        }

        var answer = await MessageBox.Show(
            owner,
            Se.Language.General.Download,
            message,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return false;
        }

        await DownloadAsync(owner, windowService, preselectModelName, autoStartModelDownload: !string.IsNullOrEmpty(preselectModelName));

        return IsReady(preselectModelName);
    }
}
