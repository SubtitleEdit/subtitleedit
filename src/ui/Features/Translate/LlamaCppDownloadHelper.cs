using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// A curated llama.cpp translate model wrapped for display in a combobox (name + size + install status).
/// </summary>
public class LlamaCppModelDisplay
{
    public LlamaCppModel Model { get; }

    public LlamaCppModelDisplay(LlamaCppModel model)
    {
        Model = model;
    }

    public override string ToString()
    {
        // Custom entries (no Url) come from a *.gguf the user dropped into the models folder -
        // by definition already on disk, so just flag them visually instead of saying "installed".
        if (string.IsNullOrEmpty(Model.Url))
        {
            return $"{Model.DisplayName} (custom, {Model.Size})";
        }

        var installed = LlamaCppServerManager.IsModelInstalled(Model);
        return installed
            ? Model.DisplayName
            : $"{Model.DisplayName} ({Model.Size}, not installed)";
    }
}

/// <summary>
/// Shared download/availability logic for the llama.cpp auto-translate engine - mirrors
/// <see cref="CrispAsrTranslateDownloadHelper"/>.
/// </summary>
public static class LlamaCppDownloadHelper
{
    /// <summary>
    /// True when the llama-server binary is installed and a model is available. When
    /// <paramref name="modelFileName"/> is given, that specific model must be installed.
    /// </summary>
    public static bool IsReady(string? modelFileName = null)
    {
        if (!LlamaCppServerManager.IsEngineInstalled())
        {
            return false;
        }

        if (!string.IsNullOrEmpty(modelFileName))
        {
            return LlamaCppServerManager.IsModelInstalled(modelFileName);
        }

        return GetInstalledModelPath() != null;
    }

    /// <summary>
    /// Path of an installed model - the one from settings if it still exists, otherwise any
    /// known model file found in the llama.cpp models folder. "Known" includes both curated
    /// entries and custom <c>*.gguf</c> files the user has dropped into the folder.
    /// </summary>
    public static string? GetInstalledModelPath()
    {
        var configured = Se.Settings.AutoTranslate.LlamaCppModel;
        if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
        {
            return configured;
        }

        foreach (var model in LlamaCppServerManager.GetAllTranslateModels())
        {
            var path = LlamaCppServerManager.GetModelPath(model.FileName);
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    /// <summary>
    /// Fills <paramref name="target"/> with the given curated models and returns the one matching
    /// <paramref name="selectFileName"/>, or the first model otherwise.
    /// </summary>
    public static LlamaCppModelDisplay? PopulateModels(
        ObservableCollection<LlamaCppModelDisplay> target,
        System.Collections.Generic.IReadOnlyList<LlamaCppModel> models,
        string? selectFileName)
    {
        target.Clear();
        LlamaCppModelDisplay? toSelect = null;
        foreach (var model in models)
        {
            var display = new LlamaCppModelDisplay(model);
            target.Add(display);
            if (model.FileName == selectFileName)
            {
                toSelect = display;
            }
        }

        return toSelect ?? target.FirstOrDefault();
    }

    private static LlamaCppModel? ResolveModel(string? modelFileName)
    {
        var all = LlamaCppServerManager.GetAllTranslateModels();
        if (!string.IsNullOrEmpty(modelFileName))
        {
            var match = all.FirstOrDefault(m => m.FileName == modelFileName);
            if (match != null)
            {
                return match;
            }
        }

        return all.FirstOrDefault(m => LlamaCppServerManager.IsModelInstalled(m.FileName))
               ?? all.FirstOrDefault();
    }

    /// <summary>
    /// Opens the llama.cpp download window (engine binary + selected model) and persists the result.
    /// When <paramref name="forceVariant"/> is given (e.g. re-downloading an update), that build is
    /// used and the Windows build picker is skipped. When <paramref name="forceEngineDownload"/> is
    /// set, the engine binary is re-downloaded even though it is already installed. When
    /// <paramref name="forceModelDownload"/> is set, the model file is re-downloaded even though it
    /// is already installed.
    /// </summary>
    /// <returns>The downloaded model file name, or null if nothing was downloaded.</returns>
    public static async Task<string?> DownloadAsync(Window owner, IWindowService windowService, LlamaCppModel? model, string? forceVariant = null, bool forceEngineDownload = false, bool forceModelDownload = false)
    {
        var variant = forceVariant ?? Logic.Download.LlamaCppDownloadService.VariantCpu;
        if (forceVariant == null && !LlamaCppServerManager.IsEngineInstalled() && OperatingSystem.IsWindows())
        {
            var buildAnswer = await MessageBox.Show(
                owner,
                "Download llama.cpp?",
                "Select which llama.cpp build to download:",
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU",
                "Vulkan (GPU)",
                "CUDA (NVIDIA GPU)");

            variant = buildAnswer switch
            {
                MessageBoxResult.Custom1 => Logic.Download.LlamaCppDownloadService.VariantCpu,
                MessageBoxResult.Custom2 => Logic.Download.LlamaCppDownloadService.VariantVulkan,
                MessageBoxResult.Custom3 => Logic.Download.LlamaCppDownloadService.VariantCuda,
                _ => null,
            } ?? string.Empty;

            if (string.IsNullOrEmpty(variant))
            {
                return null;
            }
        }

        var dlVm = await windowService.ShowDialogAsync<DownloadLlamaCppWindow, DownloadLlamaCppViewModel>(
            owner, vm =>
            {
                vm.Variant = variant;
                vm.Model = model;
                vm.ForceEngineDownload = forceEngineDownload;
                vm.ForceModelDownload = forceModelDownload;
                vm.StartDownload();
            });

        if (!dlVm.OkPressed)
        {
            return null;
        }

        return model?.FileName ?? string.Empty;
    }

    /// <summary>
    /// Ensures the llama-server binary and the requested model are installed, prompting the user
    /// to download them if not.
    /// </summary>
    public static async Task<bool> EnsureReadyAsync(Window owner, IWindowService windowService, string? modelFileName)
    {
        var engineInstalled = LlamaCppServerManager.IsEngineInstalled();
        var model = ResolveModel(modelFileName);
        var modelInstalled = model != null && LlamaCppServerManager.IsModelInstalled(model);

        if (engineInstalled && modelInstalled)
        {
            var modelPath = LlamaCppServerManager.GetModelPath(model!.FileName);
            if (Se.Settings.AutoTranslate.LlamaCppModel != modelPath)
            {
                Se.Settings.AutoTranslate.LlamaCppModel = modelPath;
                Configuration.Settings.Tools.LlamaCppModel = modelPath;
                Se.SaveSettings();
            }

            return true;
        }

        // Custom (user-supplied) model whose .gguf has vanished between discovery and now -
        // there's no Url to download from, so prompting to download would just fail. Tell the
        // user the file is gone and bail.
        if (model != null && string.IsNullOrEmpty(model.Url) && !modelInstalled)
        {
            await MessageBox.Show(
                owner,
                Se.Language.General.Error,
                $"The custom model file '{model.FileName}' was not found in the llama.cpp models folder.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        string message;
        if (!engineInstalled && !modelInstalled)
        {
            message = "llama.cpp requires the llama-server engine and a translation model to be downloaded. Download now?";
        }
        else if (!engineInstalled)
        {
            message = "llama.cpp requires the llama-server engine to be downloaded. Download now?";
        }
        else
        {
            message = "llama.cpp requires the selected translation model to be downloaded. Download now?";
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

        await DownloadAsync(owner, windowService, model);

        return LlamaCppServerManager.IsEngineInstalled()
               && model != null
               && LlamaCppServerManager.IsModelInstalled(model);
    }
}
