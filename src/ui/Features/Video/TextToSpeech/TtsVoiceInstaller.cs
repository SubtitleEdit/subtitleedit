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
    public static Task<bool> EnsureCrispAsrForChatterbox(Window? window, IWindowService windowService, bool forceRedownload)
        => EnsureCrispAsrAsync(window, windowService, forceRedownload,
            engineDisplayName: "Chatterbox TTS",
            extraCapabilityCheck: () => ChatterboxTtsCpp.IsCrispAsrChatterboxCapable(),
            minVersionNote: "v0.6.0 or newer");

    /// <summary>
    /// Ensures the CrispASR runtime that Qwen3 TTS (CrispASR) runs on is installed.
    /// No extra capability check (any sufficiently recent CrispASR exposes the qwen3-tts
    /// backends), so the dialogs read with the right engine name and don't mention
    /// Chatterbox.
    /// </summary>
    public static Task<bool> EnsureCrispAsrForQwen3(Window? window, IWindowService windowService, bool forceRedownload)
        => EnsureCrispAsrAsync(window, windowService, forceRedownload,
            engineDisplayName: "Qwen3 TTS (CrispASR)",
            extraCapabilityCheck: null,
            minVersionNote: null);

    /// <summary>
    /// Ensures the CrispASR runtime that VibeVoice (CrispASR) runs on is installed.
    /// VibeVoice's GGUF is fetched on first server start via crispasr's own --auto-download,
    /// so this is the only install step SE drives directly.
    /// </summary>
    public static Task<bool> EnsureCrispAsrForVibeVoice(Window? window, IWindowService windowService, bool forceRedownload)
        => EnsureCrispAsrAsync(window, windowService, forceRedownload,
            engineDisplayName: "VibeVoice (CrispASR)",
            extraCapabilityCheck: null,
            minVersionNote: null);

    /// <summary>
    /// Ensures the CrispASR runtime that IndexTTS (CrispASR) runs on is installed.
    /// IndexTTS's GGUFs are fetched on first server start via crispasr's own --auto-download,
    /// so this is the only install step SE drives directly.
    /// </summary>
    public static Task<bool> EnsureCrispAsrForIndexTts(Window? window, IWindowService windowService, bool forceRedownload)
        => EnsureCrispAsrAsync(window, windowService, forceRedownload,
            engineDisplayName: "IndexTTS (CrispASR)",
            extraCapabilityCheck: null,
            minVersionNote: null);

    /// <summary>
    /// Ensures the CrispASR runtime that Zonos TTS (CrispASR) runs on is installed.
    /// The zonos-tts backend's GGUFs (transformer + DAC codec) are staged into SE's
    /// CrispAsr/models folder by
    /// <see cref="Nikse.SubtitleEdit.Logic.Download.ZonosTtsCrispAsrDownloadService"/> with
    /// hash verification before first synth.
    /// </summary>
    public static Task<bool> EnsureCrispAsrForZonos(Window? window, IWindowService windowService, bool forceRedownload)
        => EnsureCrispAsrAsync(window, windowService, forceRedownload,
            engineDisplayName: "Zonos TTS (CrispASR)",
            extraCapabilityCheck: null,
            minVersionNote: null);

    /// <summary>
    /// Ensures the CrispASR runtime that CosyVoice3 (CrispASR) runs on is installed.
    /// The cosyvoice3-tts backend ships in CrispASR v0.6.12+; every required GGUF (LLM + flow
    /// + hift + s3tok + campplus + voice-bank) is staged into SE's CrispAsr/models folder by
    /// <see cref="Nikse.SubtitleEdit.Logic.Download.CosyVoice3CrispAsrDownloadService"/> with
    /// hash verification before first synth.
    /// </summary>
    public static Task<bool> EnsureCrispAsrForCosyVoice3(Window? window, IWindowService windowService, bool forceRedownload)
        => EnsureCrispAsrAsync(window, windowService, forceRedownload,
            engineDisplayName: "CosyVoice3 (CrispASR)",
            extraCapabilityCheck: null,
            minVersionNote: "v0.6.12 or newer");

    /// <summary>
    /// Ensures the CrispASR runtime that F5-TTS (CrispASR) runs on is installed.
    /// The f5-tts backend ships in CrispASR v0.6.12+.
    /// </summary>
    public static Task<bool> EnsureCrispAsrForF5Tts(Window? window, IWindowService windowService, bool forceRedownload)
        => EnsureCrispAsrAsync(window, windowService, forceRedownload,
            engineDisplayName: "F5-TTS (CrispASR)",
            extraCapabilityCheck: null,
            minVersionNote: "v0.6.12 or newer");

    /// <summary>
    /// Ensures the CrispASR runtime that VoxCPM2 (CrispASR) runs on is installed.
    /// The voxcpm2-tts backend ships in CrispASR v0.7.0+.
    /// </summary>
    public static Task<bool> EnsureCrispAsrForVoxCPM2(Window? window, IWindowService windowService, bool forceRedownload)
        => EnsureCrispAsrAsync(window, windowService, forceRedownload,
            engineDisplayName: "VoxCPM2 (CrispASR)",
            extraCapabilityCheck: null,
            minVersionNote: "v0.7.0 or newer");

    /// <summary>
    /// Shared CrispASR install/update flow used by all TTS engines that sit on the
    /// CrispASR runtime. Prompts refer to <paramref name="engineDisplayName"/> so users
    /// see the right engine name. <paramref name="extraCapabilityCheck"/> lets the caller
    /// require a backend-specific minimum (Chatterbox needs the chatterbox backend to be
    /// present in the binary, for example); returning false there triggers a re-download
    /// even when CrispASR itself looks up to date.
    /// </summary>
    private static async Task<bool> EnsureCrispAsrAsync(
        Window? window,
        IWindowService windowService,
        bool forceRedownload,
        string engineDisplayName,
        Func<bool>? extraCapabilityCheck,
        string? minVersionNote)
    {
        if (window == null)
        {
            return false;
        }

        var isInstalled = File.Exists(ChatterboxTtsCpp.GetCrispAsrExecutable());
        var isCapable = isInstalled && (extraCapabilityCheck?.Invoke() ?? true);
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

            var versionRequirement = !string.IsNullOrEmpty(minVersionNote)
                ? $" needs CrispASR {minVersionNote}"
                : " requires a newer CrispASR runtime";

            var answer = await MessageBox.Show(
                window,
                isCapable ? "Update CrispASR" : "CrispASR update required",
                isCapable
                    ? $"{Environment.NewLine}A newer CrispASR runtime is available. Re-download it now?"
                    : $"{Environment.NewLine}\"{engineDisplayName}\"{versionRequirement}. Re-download now?",
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
                $"{Environment.NewLine}\"{engineDisplayName}\" runs through the CrispASR runtime. Select a build to download:",
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
                $"{Environment.NewLine}\"{engineDisplayName}\" runs through the CrispASR runtime. Download and install now?",
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

        return File.Exists(ChatterboxTtsCpp.GetCrispAsrExecutable())
               && (extraCapabilityCheck?.Invoke() ?? true);
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
