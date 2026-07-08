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
    /// Makes sure the Paddle OCR engine (standalone only) and models are installed,
    /// prompting the user to download what is missing. Returns false if the user cancelled.
    /// </summary>
    public static async Task<bool> EnsureInstalled(Window window, IWindowService windowService, OcrEngineType engineType)
    {
        if (engineType == OcrEngineType.PaddleOcrStandalone)
        {
            if (Configuration.IsRunningOnWindows && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")))
            {
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
                        var engine = PaddleOcrDownloadType.EngineCpu;
                        if (answer == MessageBoxResult.Custom2)
                        {
                            engine = PaddleOcrDownloadType.EngineGpu11;
                        }
                        else if (answer == MessageBoxResult.Custom3)
                        {
                            engine = PaddleOcrDownloadType.EngineGpu12;
                        }

                        vm.Initialize(engine);
                    });

                if (!result.OkPressed)
                {
                    return false;
                }
            }
            else if (Configuration.IsRunningOnLinux && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin")))
            {
                var answer = await MessageBox.Show(
                    window,
                    "Download Paddle OCR?",
                    $"{Environment.NewLine}\"Paddle OCR\" requires downloading Paddle OCR.{Environment.NewLine}{Environment.NewLine}Download and use Paddle OCR?",
                    MessageBoxButtons.Cancel,
                    MessageBoxIcon.Question,
                    "CPU",
                    "GPU CUDA");

                if (answer == MessageBoxResult.Cancel)
                {
                    return false;
                }

                var result = await windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(window,
                    vm =>
                    {
                        vm.Initialize(answer == MessageBoxResult.Custom1
                            ? PaddleOcrDownloadType.EngineCpuLinux
                            : PaddleOcrDownloadType.EngineGpuLinux);
                    });

                if (!result.OkPressed)
                {
                    return false;
                }
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
}
