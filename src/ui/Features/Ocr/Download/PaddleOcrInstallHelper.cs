using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

/// <summary>
/// Prompts for and runs the Paddle OCR engine/models download when missing.
/// Shared by the features that run Paddle OCR (image OCR, batch convert, video OCR).
/// </summary>
public static class PaddleOcrInstallHelper
{
    /// <summary>
    /// True when the standalone Paddle OCR engine binary and the OCR models are already on disk,
    /// so no download is needed. Used to show an install-status dot next to the engine in a picker.
    /// The Python engine is installed via pip (outside our control), so it is not covered here.
    /// </summary>
    public static bool IsStandaloneInstalled()
    {
        if (Configuration.IsRunningOnWindows && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")))
        {
            return false;
        }

        if (Configuration.IsRunningOnLinux && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin")))
        {
            return false;
        }

        return Directory.Exists(Se.PaddleOcrModelsFolder);
    }

    /// <summary>
    /// Makes sure the Paddle OCR engine (standalone only) and models are installed,
    /// prompting the user to download what is missing. Returns false if the user cancelled.
    /// </summary>
    public static async Task<bool> EnsureInstalled(Window window, IWindowService windowService, OcrEngineType engineType)
    {
        // Only Windows and Linux have a standalone build; on macOS there is nothing to fetch and
        // the engine check is skipped as before.
        if (engineType == OcrEngineType.PaddleOcrStandalone
            && (Configuration.IsRunningOnWindows || Configuration.IsRunningOnLinux)
            && !IsStandaloneEngineInstalled())
        {
            if (!await DownloadEngineAsync(window, windowService))
            {
                return false;
            }
        }

        if (!Directory.Exists(Se.PaddleOcrModelsFolder))
        {
            var result = await windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(window,
                vm => { vm.Initialize(PaddleOcrDownloadType.Models); });

            if (!result.OkPressed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// True when the standalone engine binary is on disk (models are checked separately).
    /// Always false on macOS, where there is no standalone build.
    /// </summary>
    public static bool IsStandaloneEngineInstalled()
    {
        if (Configuration.IsRunningOnWindows)
        {
            return File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe"));
        }

        if (Configuration.IsRunningOnLinux)
        {
            return File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin"));
        }

        return false;
    }

    /// <summary>
    /// Asks CPU / CUDA 11 / CUDA 12 and downloads the standalone engine, whether or not one is
    /// already installed - the engine settings dialog uses it to re-download or switch build.
    /// Returns false if the user cancelled.
    /// </summary>
    public static async Task<bool> DownloadEngineAsync(Window window, IWindowService windowService)
    {
        if (!Configuration.IsRunningOnWindows && !Configuration.IsRunningOnLinux)
        {
            return false;
        }

        var answer = await MessageBox.Show(
            window,
            "Download Paddle OCR?",
            $"{Environment.NewLine}\"Paddle OCR\" requires downloading Paddle OCR.{Environment.NewLine}{Environment.NewLine}Download and use Paddle OCR?",
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "CPU",
            "GPU CUDA 11",
            "GPU CUDA 12");

        if (answer == MessageBoxResult.Cancel)
        {
            return false;
        }

        var result = await windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(window,
            vm =>
            {
                var isLinux = Configuration.IsRunningOnLinux;
                var engine = isLinux ? PaddleOcrDownloadType.EngineCpuLinux : PaddleOcrDownloadType.EngineCpu;
                if (answer == MessageBoxResult.Custom2)
                {
                    engine = isLinux ? PaddleOcrDownloadType.EngineGpu11Linux : PaddleOcrDownloadType.EngineGpu11;
                }
                else if (answer == MessageBoxResult.Custom3)
                {
                    engine = isLinux ? PaddleOcrDownloadType.EngineGpu12Linux : PaddleOcrDownloadType.EngineGpu12;
                }

                vm.Initialize(engine);
            });

        return result.OkPressed;
    }
}
