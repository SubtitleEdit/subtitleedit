using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Shared voice-install gate for TTS engines: ensures the selected voice is present,
/// prompting a download when it is not.
/// </summary>
public static class TtsVoiceInstaller
{
    /// <summary>
    /// Ensures the given voice is installed, prompting a download if needed.
    /// Returns false only when the user cancels the download.
    /// </summary>
    public static async Task<bool> EnsureVoiceInstalled(ITtsEngine engine, Voice voice, Window? window, IWindowService windowService)
    {
        if (window == null || engine.IsVoiceInstalled(voice))
        {
            return true;
        }

        // Only Piper has per-voice downloads; every other engine's IsVoiceInstalled is always true.
        if (voice.EngineVoice is PiperVoice piperVoice)
        {
            var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(
                window, vm => vm.StartDownloadPiperVoice(piperVoice));
            if (!dlResult.OkPressed)
            {
                SafeDelete(Path.Combine(Piper.GetSetPiperFolder(), piperVoice.ModelShort));
                SafeDelete(Path.Combine(Piper.GetSetPiperFolder(), piperVoice.ConfigShort));
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Ensures the CrispASR runtime that Chatterbox TTS runs on is installed and new enough.
    /// When <paramref name="forceRedownload"/> is true the download runs even if CrispASR is
    /// already Chatterbox-capable - used by the settings "Update CrispASR" button when a newer
    /// release exists. Returns true when CrispASR ends up installed and Chatterbox-capable.
    /// </summary>
    public static async Task<bool> EnsureCrispAsrForChatterbox(Window? window, IWindowService windowService, bool forceRedownload)
    {
        if (window == null)
        {
            return false;
        }

        var isInstalled = File.Exists(ChatterboxTtsCpp.GetCrispAsrExecutable());
        var isCapable = isInstalled && ChatterboxTtsCpp.IsCrispAsrChatterboxCapable();
        if (!forceRedownload && isInstalled && isCapable)
        {
            return true;
        }

        var crispAsrEngine = (ISpeechToTextEngine)new CrispAsrCohere();
        string crispVariant;

        if (isInstalled)
        {
            // Already installed - re-download with the variant the user originally picked.
            var folder = crispAsrEngine.GetAndCreateWhisperFolder();
            crispVariant = (Configuration.IsRunningOnWindows
                ? DownloadHashManager.DetectCrispAsrWindowsVariant(folder)
                : null) ?? "vulkan";

            var answer = await MessageBox.Show(
                window,
                isCapable ? "Update CrispASR" : "CrispASR update required",
                isCapable
                    ? $"{Environment.NewLine}A newer CrispASR runtime is available. Re-download it now?"
                    : $"{Environment.NewLine}\"Chatterbox TTS\" needs CrispASR v0.6.0 or newer. Re-download now?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }
        }
        else if (Configuration.IsRunningOnWindows)
        {
            var variantAnswer = await MessageBox.Show(
                window,
                "Download CrispASR?",
                $"{Environment.NewLine}\"Chatterbox TTS\" runs through the CrispASR runtime. Select a build to download:",
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU",
                "Vulkan",
                "CUDA");

            if (variantAnswer == MessageBoxResult.None || variantAnswer == MessageBoxResult.Cancel)
            {
                return false;
            }

            crispVariant = variantAnswer switch
            {
                MessageBoxResult.Custom1 => "cpu",
                MessageBoxResult.Custom3 => "cuda",
                _ => "vulkan",
            };

            if (crispVariant == "cpu")
            {
                var cpuAnswer = await PromptCrispAsrCpuFlavorAsync(window);
                if (cpuAnswer == null)
                {
                    return false;
                }
                crispVariant = cpuAnswer;
            }

            if (crispVariant == "vulkan" && !VulkanHelper.IsInstalled())
            {
                var vulkanAnswer = await MessageBox.Show(
                    window,
                    "Vulkan SDK may be required",
                    $"The Vulkan version requires the Vulkan SDK to be installed.{Environment.NewLine}{Environment.NewLine}You can download it from:{Environment.NewLine}https://vulkan.lunarg.com/sdk/home{Environment.NewLine}{Environment.NewLine}Continue with Vulkan download?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (vulkanAnswer == MessageBoxResult.No)
                {
                    UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                    return false;
                }

                if (vulkanAnswer != MessageBoxResult.Yes)
                {
                    return false;
                }
            }
        }
        else
        {
            var answer = await MessageBox.Show(
                window,
                "Download CrispASR?",
                $"{Environment.NewLine}\"Chatterbox TTS\" runs through the CrispASR runtime. Download and install now?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            crispVariant = "vulkan"; // ignored on non-Windows
        }

        var dlVm = await windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
            window, viewModel =>
            {
                viewModel.Engine = crispAsrEngine;
                viewModel.CrispAsrWindowsVariant = crispVariant;
                viewModel.StartDownload();
            });

        if (!dlVm.OkPressed)
        {
            return false;
        }

        return File.Exists(ChatterboxTtsCpp.GetCrispAsrExecutable()) && ChatterboxTtsCpp.IsCrispAsrChatterboxCapable();
    }

    /// <summary>
    /// Follow-up prompt after the user picks "CPU" in the CrispASR variant selector.
    /// Returns "cpu" (modern, recommended), "cpu-legacy" (compatibility build for CPUs without AVX2),
    /// or null when the user cancels.
    /// </summary>
    private static async Task<string?> PromptCrispAsrCpuFlavorAsync(Window window)
    {
        var cpuAnswer = await MessageBox.Show(
            window,
            "CrispASR CPU build",
            $"{Environment.NewLine}Standard is recommended for most machines.{Environment.NewLine}{Environment.NewLine}Legacy is a fallback for older CPUs without AVX2 support.",
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "Standard",
            "Legacy");

        return cpuAnswer switch
        {
            MessageBoxResult.Custom1 => "cpu",
            MessageBoxResult.Custom2 => "cpu-legacy",
            _ => null,
        };
    }

    private static void SafeDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // ignore
        }
    }
}
