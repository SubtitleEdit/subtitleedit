using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// Shared download/availability logic for the <see cref="CrispAsrMadladTranslate"/> auto-translate engine,
/// used by both the auto-translate window and batch convert.
/// </summary>
public static class CrispAsrTranslateDownloadHelper
{
    public static bool IsReady()
    {
        return new CrispAsrMadlad().IsEngineInstalled() && GetInstalledModelPath() != null;
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
    /// Opens the download window(s) for the CrispASR engine binary and a MADLAD model and persists the result.
    /// </summary>
    /// <returns>The name of the downloaded model, or null if nothing was downloaded.</returns>
    public static async Task<string?> DownloadAsync(Window owner, IWindowService windowService, string? preselectModelName)
    {
        var engine = new CrispAsrMadlad();

        if (!engine.IsEngineInstalled())
        {
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
    /// Ensures the CrispASR engine and a MADLAD model are installed, prompting the user to download them if not.
    /// </summary>
    /// <returns>True if the engine is ready to use.</returns>
    public static async Task<bool> EnsureReadyAsync(Window owner, IWindowService windowService, string? preselectModelName)
    {
        var engine = new CrispAsrMadlad();
        var engineInstalled = engine.IsEngineInstalled();
        if (engineInstalled)
        {
            var installedModel = GetInstalledModelPath();
            if (installedModel != null)
            {
                // self-heal: point settings at the binary/model that are actually on disk
                if (Se.Settings.AutoTranslate.CrispAsrExe != engine.GetExecutable() ||
                    Se.Settings.AutoTranslate.CrispAsrModel != installedModel)
                {
                    Se.Settings.AutoTranslate.CrispAsrExe = engine.GetExecutable();
                    Se.Settings.AutoTranslate.CrispAsrModel = installedModel;
                    Configuration.Settings.Tools.AutoTranslateCrispAsrExe = Se.Settings.AutoTranslate.CrispAsrExe;
                    Configuration.Settings.Tools.AutoTranslateCrispAsrModel = installedModel;
                    Se.SaveSettings();
                }

                return true;
            }
        }

        var message = engineInstalled
            ? $"{CrispAsrMadladTranslate.StaticName} requires a MADLAD model to be downloaded. Download now?"
            : $"{CrispAsrMadladTranslate.StaticName} requires the CrispASR engine and a MADLAD model to be downloaded. Download now?";

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

        await DownloadAsync(owner, windowService, preselectModelName);

        return IsReady();
    }
}
