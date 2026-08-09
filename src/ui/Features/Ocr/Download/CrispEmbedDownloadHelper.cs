using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

/// <summary>
/// Shared download/availability flow for the CrispEmbed OCR engine - the OCR-side analog of
/// <see cref="Translate.LlamaCppDownloadHelper"/>, used by both the OCR window and the batch
/// convert settings.
/// </summary>
public static class CrispEmbedDownloadHelper
{
    /// <summary>
    /// Makes sure the CrispEmbed engine binaries and the given model are on disk, offering
    /// downloads for anything missing. The optional callbacks fire after the engine/model
    /// download dialog closes (regardless of outcome) so callers can refresh install-status UI.
    /// </summary>
    public static async Task<bool> EnsureReadyAsync(
        Window owner,
        IWindowService windowService,
        CrispEmbedBackend backend,
        CrispEmbedModel model,
        bool forceModelDownload = false,
        Action? onEngineDownloadClosed = null,
        Action? onModelDownloadClosed = null)
    {
        if (!CrispEmbedEngine.IsEngineInstalled()
            && !await DownloadEngineAsync(owner, windowService, onEngineDownloadClosed))
        {
            return false;
        }

        if (forceModelDownload || !backend.IsModelInstalled(model))
        {
            if (!forceModelDownload)
            {
                var answer = await MessageBox.Show(
                    owner,
                    "Download model?",
                    $"{Environment.NewLine}Download the model \"{model.Name}\" ({model.Size})?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            var modelResult = await windowService.ShowDialogAsync<DownloadCrispEmbedWindow, DownloadCrispEmbedViewModel>(owner,
                vm => vm.InitializeModel(backend, model));

            onModelDownloadClosed?.Invoke();

            if (!modelResult.OkPressed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Asks which hardware build to fetch (where the platform has a choice) and runs the engine
    /// download. Used both for the first install and for re-downloading over an existing one -
    /// the extract step clears only the top-level binaries, so downloaded models survive a
    /// variant switch.
    /// </summary>
    /// <returns>True when the engine was downloaded; false if the user cancelled or it failed.</returns>
    public static async Task<bool> DownloadEngineAsync(
        Window owner,
        IWindowService windowService,
        Action? onEngineDownloadClosed = null)
    {
        var isReDownload = CrispEmbedEngine.IsEngineInstalled();
        var title = isReDownload
            ? string.Format(Se.Language.General.ReDownloadX, CrispEmbedEngine.StaticName)
            : "Download CrispEmbed?";
        var lead = isReDownload
            ? "\"CrispEmbed\" is already installed."
            : "\"CrispEmbed\" requires downloading the CrispEmbed engine.";

        // Where there is a build to choose, ask for it either way - re-running this prompt is
        // the only route back to Vulkan/CUDA for someone who picked CPU on first run (#13400).
        var pickMessage = $"{Environment.NewLine}{lead}{Environment.NewLine}{Environment.NewLine}Select a version to download:";

        string variant;
        if (Configuration.IsRunningOnWindows)
        {
            var answer = await MessageBox.Show(
                owner,
                title,
                pickMessage,
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU",
                "Vulkan",
                "CUDA");

            if (answer == MessageBoxResult.Cancel || answer == MessageBoxResult.None)
            {
                return false;
            }

            variant = answer switch
            {
                MessageBoxResult.Custom1 => "cpu",
                MessageBoxResult.Custom3 => "cuda",
                _ => "vulkan",
            };
        }
        else if (Configuration.IsRunningOnLinux && RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            var answer = await MessageBox.Show(
                owner,
                title,
                pickMessage,
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU (~10 MB)",
                "GPU CUDA (~718 MB)");

            if (answer == MessageBoxResult.Cancel || answer == MessageBoxResult.None)
            {
                return false;
            }

            variant = answer == MessageBoxResult.Custom2 ? "cuda" : string.Empty;
        }
        else
        {
            // macOS and Linux ARM64 ship a single build, so there is nothing to choose - but a
            // re-download is still useful to pick up a new release or repair a broken install.
            var answer = await MessageBox.Show(
                owner,
                title,
                isReDownload
                    ? $"{Environment.NewLine}{lead}{Environment.NewLine}{Environment.NewLine}Download it again?"
                    : $"{Environment.NewLine}\"CrispEmbed\" requires downloading the CrispEmbed engine ({CrispEmbedEngine.DownloadSizeText}).{Environment.NewLine}{Environment.NewLine}Download and use CrispEmbed?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            variant = string.Empty;
        }

        var engineResult = await windowService.ShowDialogAsync<DownloadCrispEmbedWindow, DownloadCrispEmbedViewModel>(owner,
            vm => vm.InitializeEngine(variant));

        onEngineDownloadClosed?.Invoke();

        return engineResult.OkPressed;
    }
}
