using Avalonia.Controls;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.EngineSettings;
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
            var variant = await CrispAsrDownloadHelper.PromptVariantAsync(owner, engine.Name);
            if (variant == null)
            {
                return null;
            }

            var engineVm = await windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
                owner, vm =>
                {
                    vm.Engine = engine;
                    vm.CrispAsrWindowsVariant = variant;
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
    /// Forces a re-download of the CrispASR engine binary. The MADLAD models are left untouched -
    /// only the engine executable (and its sidecar hash) is refreshed.
    /// </summary>
    /// <param name="owner">The window that owns the prompts and the download dialog.</param>
    /// <param name="windowService">Opens the download dialog.</param>
    /// <param name="askForBuild">
    /// True to let the user switch builds. Otherwise the installed build is kept, because
    /// speech-to-text runs the same binary and an update must not swap its GPU build for another.
    /// </param>
    /// <returns>True if the engine was re-installed.</returns>
    public static async Task<bool> UpdateEngineAsync(Window owner, IWindowService windowService, bool askForBuild = false)
    {
        var engine = new CrispAsrMadlad();

        var variant = await ResolveBuildAsync(owner, engine.Name, engine.GetAndCreateWhisperFolder(), engine.IsEngineInstalled(), askForBuild);
        if (variant == null)
        {
            return false;
        }

        var engineVm = await windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
            owner, vm =>
            {
                vm.Engine = engine;
                vm.CrispAsrWindowsVariant = variant;
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
    /// The build for a re-download of the engine in <paramref name="folder"/>: the installed one
    /// when it is known and the user did not ask to switch, otherwise whatever the user picks.
    /// Returns null when the user cancels.
    /// </summary>
    public static async Task<string?> ResolveBuildAsync(Window owner, string engineName, string folder, bool isInstalled, bool askForBuild)
    {
        if (!isInstalled)
        {
            return await CrispAsrDownloadHelper.PromptVariantAsync(owner, engineName);
        }

        var installed = askForBuild ? null : CrispAsrDownloadHelper.GetInstalledVariant(folder);
        return installed ?? await CrispAsrDownloadHelper.PromptVariantAsync(
            owner,
            engineName,
            string.Format(Se.Language.Translate.CrispAsrSwitchBuildInfo, engineName));
    }

    /// <summary>
    /// Opens the engine settings dialog, whose download button is how the user switches builds.
    /// </summary>
    public static Task ShowEngineSettingsAsync(Window owner, IWindowService windowService)
    {
        return windowService.ShowDialogAsync<SpeechToTextEngineSettingsWindow, SpeechToTextEngineSettingsViewModel>(
            owner,
            vm => vm.Initialize(new CrispAsrMadlad(), () => UpdateEngineAsync(vm.Window!, windowService, askForBuild: true)));
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

        // The build prompt has its own Cancel, so asking first would be a second question in a row.
        if (!engineInstalled && CrispAsrDownloadHelper.HasVariantChoice)
        {
            await DownloadAsync(owner, windowService, preselectModelName, autoStartModelDownload: !string.IsNullOrEmpty(preselectModelName));
            return IsReady(preselectModelName);
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
