using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
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
        if (!CrispEmbedEngine.IsEngineInstalled())
        {
            string variant;
            if (Configuration.IsRunningOnWindows)
            {
                var answer = await MessageBox.Show(
                    owner,
                    "Download CrispEmbed?",
                    $"{Environment.NewLine}\"CrispEmbed\" requires downloading the CrispEmbed engine.{Environment.NewLine}{Environment.NewLine}Download and use CrispEmbed?",
                    MessageBoxButtons.Cancel,
                    MessageBoxIcon.Question,
                    "CPU",
                    "Vulkan",
                    "CUDA");

                if (answer == MessageBoxResult.Cancel)
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
                    "Download CrispEmbed?",
                    $"{Environment.NewLine}\"CrispEmbed\" requires downloading the CrispEmbed engine.{Environment.NewLine}{Environment.NewLine}Download and use CrispEmbed?",
                    MessageBoxButtons.Cancel,
                    MessageBoxIcon.Question,
                    "CPU (~10 MB)",
                    "GPU CUDA (~718 MB)");

                if (answer == MessageBoxResult.Cancel)
                {
                    return false;
                }

                variant = answer == MessageBoxResult.Custom2 ? "cuda" : string.Empty;
            }
            else
            {
                var answer = await MessageBox.Show(
                    owner,
                    "Download CrispEmbed?",
                    $"{Environment.NewLine}\"CrispEmbed\" requires downloading the CrispEmbed engine ({CrispEmbedEngine.DownloadSizeText}).{Environment.NewLine}{Environment.NewLine}Download and use CrispEmbed?",
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

            if (!engineResult.OkPressed)
            {
                return false;
            }
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
}
