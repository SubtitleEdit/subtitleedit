using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// The CrispASR build prompts and installed-build lookup shared by speech-to-text and the MADLAD
/// auto-translate engine. All CrispASR engines live in one folder, so a build picked in one
/// feature is the build the others run as well.
/// </summary>
public static class CrispAsrDownloadHelper
{
    /// <summary>
    /// True where upstream ships several builds to choose from. macOS and Linux ARM64 get one
    /// build per architecture, so <see cref="PromptVariantAsync"/> has nothing to ask there.
    /// </summary>
    public static bool HasVariantChoice =>
        OperatingSystem.IsWindows()
        || (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture != Architecture.Arm64);

    /// <summary>
    /// Asks which build to download on platforms that have a choice. Returns the variant string
    /// for the download dialog, or null when the user cancels. Without a choice it returns an
    /// empty string, which selects the platform's only build.
    /// </summary>
    /// <param name="owner">The window that owns the prompts.</param>
    /// <param name="engineName">The engine shown in the prompt title.</param>
    /// <param name="message">Overrides the default "requires downloading" text.</param>
    public static Task<string?> PromptVariantAsync(Window owner, string engineName, string? message = null)
    {
        if (OperatingSystem.IsWindows())
        {
            return PromptWindowsVariantAsync(owner, engineName, message);
        }

        return HasVariantChoice
            ? PromptLinuxVariantAsync(owner, engineName, message)
            : Task.FromResult<string?>(string.Empty);
    }

    /// <summary>
    /// The variant string of the build installed in <paramref name="folder"/>, or null when it is
    /// not known. The sidecar written at install time is the direct answer, including the empty
    /// string of the default Linux build. Detection only covers installs that predate it.
    /// </summary>
    public static string? GetInstalledVariant(string folder)
    {
        var sidecar = DownloadHashManager.TryReadSidecar(folder);
        var variant = sidecar == null ? null : DownloadHashManager.GetCrispAsrVariant(sidecar.Value.Key);
        if (variant != null)
        {
            return variant;
        }

        if (OperatingSystem.IsWindows())
        {
            return DownloadHashManager.DetectCrispAsrWindowsVariant(folder);
        }

        return HasVariantChoice ? DownloadHashManager.DetectCrispAsrLinuxVariant(folder) : null;
    }

    /// <summary>
    /// Windows build prompt: CPU (with a standard/legacy follow-up), Vulkan (with a
    /// Vulkan-SDK warning when none is detected) or CUDA. Returns the variant string
    /// for the download dialog, or null when the user cancels.
    /// </summary>
    public static async Task<string?> PromptWindowsVariantAsync(Window owner, string engineName, string? message = null)
    {
        var answer = await MessageBox.Show(
            owner,
            string.Format(Se.Language.General.DownloadXQuestion, engineName),
            Environment.NewLine + (message ?? string.Format(Se.Language.Video.AudioToText.XRequiresCrispAsrSelectVersion, engineName)),
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "CPU",
            "Vulkan",
            "CUDA");

        if (answer == MessageBoxResult.None || answer == MessageBoxResult.Cancel)
        {
            return null;
        }

        var crispVariant = answer switch
        {
            MessageBoxResult.Custom1 => "cpu",
            MessageBoxResult.Custom3 => "cuda",
            _ => "vulkan",
        };

        if (crispVariant == "cuda")
        {
            // Upstream added a Windows CUDA 13 build alongside the CUDA 12 one in v0.8.31, so
            // Windows now gets the same follow-up Linux has had since v0.8.30 (#14343).
            return await PromptCudaVersionAsync(owner);
        }

        if (crispVariant == "cpu")
        {
            return await PromptCpuFlavorAsync(owner);
        }

        if (crispVariant == "vulkan" && !VulkanHelper.IsInstalled())
        {
            var vulkanAnswer = await MessageBox.Show(
                owner,
                Se.Language.General.VulkanSdkMayBeRequired,
                Se.Language.General.VulkanSdkRequiredContinue,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (vulkanAnswer == MessageBoxResult.No)
            {
                UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                return null;
            }

            if (vulkanAnswer != MessageBoxResult.Yes)
            {
                return null;
            }
        }

        return crispVariant;
    }

    /// <summary>
    /// Linux x86_64 build prompt: CPU, Vulkan (any GPU), CUDA (NVIDIA) or ROCm (AMD).
    /// Returns "vulkan" / "cuda" / "cuda13" / "hip", empty string for the default CPU build,
    /// or null when the user cancels.
    /// </summary>
    public static async Task<string?> PromptLinuxVariantAsync(Window owner, string engineName, string? message = null)
    {
        var answer = await MessageBox.Show(
            owner,
            string.Format(Se.Language.General.DownloadXQuestion, engineName),
            Environment.NewLine + (message ?? string.Format(Se.Language.Video.AudioToText.XRequiresCrispAsrSelectVersion, engineName)),
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "CPU",
            "Vulkan",
            "CUDA",
            "ROCm");

        if (answer == MessageBoxResult.Custom3)
        {
            return await PromptCudaVersionAsync(owner);
        }

        return answer switch
        {
            MessageBoxResult.Custom1 => string.Empty,
            MessageBoxResult.Custom2 => "vulkan",
            MessageBoxResult.Custom4 => "hip",
            _ => null,
        };
    }

    /// <summary>
    /// Follow-up prompt after the user picks "CUDA" in the CrispASR variant selector, on both
    /// Windows and Linux - upstream ships a CUDA 12 and a CUDA 13 build for each.
    /// Returns "cuda" (CUDA 12 build) or "cuda13", or null when the user cancels.
    /// </summary>
    private static async Task<string?> PromptCudaVersionAsync(Window owner)
    {
        var answer = await MessageBox.Show(
            owner,
            Se.Language.General.CrispAsrCudaBuild,
            Environment.NewLine + Se.Language.General.CrispAsrCudaBuildInfo,
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "CUDA 12",
            "CUDA 13");

        return answer switch
        {
            MessageBoxResult.Custom1 => "cuda",
            MessageBoxResult.Custom2 => "cuda13",
            _ => null,
        };
    }

    /// <summary>
    /// Follow-up prompt after the user picks "CPU" in the CrispASR variant selector.
    /// Returns "cpu" (modern, recommended), "cpu-legacy" (compatibility build for CPUs without AVX2),
    /// or null when the user cancels.
    /// </summary>
    private static async Task<string?> PromptCpuFlavorAsync(Window owner)
    {
        var cpuAnswer = await MessageBox.Show(
            owner,
            Se.Language.General.CrispAsrCpuBuild,
            Environment.NewLine + Se.Language.General.CrispAsrCpuBuildInfo,
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
}
