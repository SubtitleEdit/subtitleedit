using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>A build a download prompt offers.</summary>
public enum GpuBuild
{
    Cpu,
    Vulkan,

    /// <summary>"CUDA", where the CUDA version is asked in a follow-up prompt.</summary>
    Cuda,
    Cuda11,
    Cuda12,
    Cuda13,
    Rocm,
}

/// <summary>What a build prompt shows: the hint under the message and the button to make the default.</summary>
/// <param name="Hint">"Detected: ..." and "Recommended for this computer: ..." lines.</param>
/// <param name="DefaultButton">The button of the recommended build.</param>
/// <param name="Recommended">The recommended build.</param>
public sealed record GpuBuildPrompt(string Hint, MessageBoxResult DefaultButton, GpuBuild Recommended);

/// <summary>
/// The hint every CPU / Vulkan / CUDA download prompt shows (#16944): what was found on this
/// computer and which of the offered builds fits it, with that build's button as the default.
/// It recommends, it never decides - the buttons and what they do are unchanged.
/// </summary>
public static class GpuBuildHint
{
    private static readonly Version Cuda13MinCompute = new(7, 5); // Turing; CUDA 13 dropped Maxwell, Pascal and Volta
    private static readonly Version Cuda12MinCompute = new(5, 0); // Maxwell
    private static readonly Version Cuda11MinCompute = new(3, 5); // Kepler

    // Most capable first; the first offered build this computer can run is recommended.
    private static readonly GpuBuild[] Preference =
    {
        GpuBuild.Cuda13,
        GpuBuild.Cuda,
        GpuBuild.Cuda12,
        GpuBuild.Cuda11,
        GpuBuild.Vulkan,
        GpuBuild.Cpu,
    };

    /// <summary>
    /// Detects this computer (off the UI thread for the driver part) and returns the hint and the
    /// default button for a prompt whose custom buttons 1, 2, 3 ... are <paramref name="buttons"/>.
    /// </summary>
    public static async Task<GpuBuildPrompt> GetAsync(params GpuBuild[] buttons)
    {
        return Create(await GpuCapabilityHelper.DetectWithCudaAsync(), buttons);
    }

    public static GpuBuildPrompt Create(GpuCapability capability, IReadOnlyList<GpuBuild> buttons)
    {
        var recommended = Recommend(capability, buttons.ToArray());
        var index = Math.Max(0, IndexOf(buttons, recommended));
        var defaultButton = index switch
        {
            0 => MessageBoxResult.Custom1,
            1 => MessageBoxResult.Custom2,
            2 => MessageBoxResult.Custom3,
            _ => MessageBoxResult.Custom4,
        };

        return new GpuBuildPrompt(GetHintText(capability, recommended, buttons), defaultButton, recommended);
    }

    /// <summary>The first of <paramref name="offered"/>, most capable first, that this computer can run.</summary>
    public static GpuBuild Recommend(GpuCapability capability, params GpuBuild[] offered)
    {
        foreach (var build in Preference)
        {
            if (offered.Contains(build) && CanRun(capability, build))
            {
                return build;
            }
        }

        return offered.Length > 0 ? offered[0] : GpuBuild.Cpu;
    }

    public static bool CanRun(GpuCapability capability, GpuBuild build)
    {
        var nvidia = capability.CudaDriverInstalled && capability.Gpus.Any(g => g.Vendor == GpuVendor.Nvidia);
        var cudaVersion = GetCudaVersion(capability);
        var compute = GetCudaCompute(capability);

        bool Cuda(int minCudaVersion, Version minCompute, bool unknownVersionOk) =>
            nvidia &&
            (cudaVersion >= minCudaVersion || (cudaVersion == 0 && unknownVersionOk)) &&
            (compute == null || compute >= minCompute);

        return build switch
        {
            GpuBuild.Cuda13 => Cuda(13000, Cuda13MinCompute, false),
            GpuBuild.Cuda12 => Cuda(12000, Cuda12MinCompute, true),
            GpuBuild.Cuda => Cuda(12000, Cuda12MinCompute, true),
            GpuBuild.Cuda11 => Cuda(11000, Cuda11MinCompute, true),
            GpuBuild.Vulkan => capability.VulkanAvailable && capability.Gpus.Any(g => g.Vendor is GpuVendor.Nvidia or GpuVendor.Amd or GpuVendor.Intel),
            GpuBuild.Rocm => false, // needs ROCm installed, which is not detected - never recommended
            _ => true,
        };
    }

    /// <summary>
    /// The newest CUDA version the driver supports (13000 = 13.0): from the driver when it was
    /// asked, else from the NVIDIA driver version (CUDA 13 needs 580, CUDA 12 525, CUDA 11 450).
    /// 0 when not known.
    /// </summary>
    public static int GetCudaVersion(GpuCapability capability)
    {
        if (capability.CudaVersion > 0)
        {
            return capability.CudaVersion;
        }

        var driver = capability.Gpus.FirstOrDefault(g => g.Vendor == GpuVendor.Nvidia && !string.IsNullOrEmpty(g.DriverVersion))?.DriverVersion;
        var majorText = (driver ?? string.Empty).Split('.')[0];
        if (!int.TryParse(majorText, NumberStyles.None, CultureInfo.InvariantCulture, out var major))
        {
            return 0;
        }

        return major switch
        {
            >= 580 => 13000,
            >= 525 => 12000,
            >= 450 => 11000,
            _ => 10000,
        };
    }

    /// <summary>
    /// The compute capability of the card an engine runs on by default - CUDA device 0 - or of
    /// the weakest NVIDIA card when the numbering is not known. Null when not known.
    /// </summary>
    private static Version? GetCudaCompute(GpuCapability capability)
    {
        var nvidia = capability.Gpus.Where(g => g.Vendor == GpuVendor.Nvidia && g.ComputeCapability != null).ToList();
        var device0 = nvidia.FirstOrDefault(g => g.CudaIndex == 0);
        return device0?.ComputeCapability ?? nvidia.Select(g => g.ComputeCapability).Min();
    }

    /// <summary>
    /// "Detected: NVIDIA GeForce RTX 4070 (driver 581.57, 12 GB)" and "Recommended for this
    /// computer: CUDA" on two lines, plus why CUDA 13 / CUDA is not recommended when an NVIDIA
    /// card is there. With several adapters each gets its own "- " line under "Detected:".
    /// </summary>
    public static string GetHintText(GpuCapability capability, GpuBuild recommended, IReadOnlyList<GpuBuild> buttons)
    {
        var l = Se.Language.General;
        var gpus = GpuCapabilityHelper.GetGpusForDisplay(capability);
        var detected = gpus.Count switch
        {
            0 => l.GpuNoneDetected,
            1 => string.Format(l.GpuDetectedX, GpuCapabilityHelper.Describe(gpus[0])),
            _ => string.Format(l.GpuDetectedX, string.Empty).TrimEnd() +
                 string.Concat(gpus.Select(g => Environment.NewLine + "- " + GpuCapabilityHelper.Describe(g))),
        };

        var lines = new List<string> { detected, string.Format(l.GpuRecommendedBuildX, GetDisplayName(recommended)) };

        var hasNvidia = capability.Gpus.Any(g => g.Vendor == GpuVendor.Nvidia);
        if (hasNvidia && buttons.Contains(GpuBuild.Cuda13) && recommended != GpuBuild.Cuda13 && CanRun(capability, GpuBuild.Cuda12))
        {
            var compute = GetCudaCompute(capability);
            lines.Add(compute != null && compute < Cuda13MinCompute
                ? string.Format(l.GpuCuda13NotForComputeX, compute.ToString(2))
                : l.GpuCuda13NeedsNewerDriver);
        }
        else if (hasNvidia && capability.CudaDriverInstalled &&
                 buttons.Any(b => b is GpuBuild.Cuda or GpuBuild.Cuda12 or GpuBuild.Cuda13 or GpuBuild.Cuda11) &&
                 recommended is not (GpuBuild.Cuda or GpuBuild.Cuda12 or GpuBuild.Cuda13 or GpuBuild.Cuda11))
        {
            lines.Add(l.GpuCudaTooOld);
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string GetDisplayName(GpuBuild build) => build switch
    {
        GpuBuild.Vulkan => "Vulkan",
        GpuBuild.Cuda => "CUDA",
        GpuBuild.Cuda11 => "CUDA 11",
        GpuBuild.Cuda12 => "CUDA 12",
        GpuBuild.Cuda13 => "CUDA 13",
        GpuBuild.Rocm => "ROCm",
        _ => "CPU",
    };

    private static int IndexOf(IReadOnlyList<GpuBuild> buttons, GpuBuild build)
    {
        for (var i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == build)
            {
                return i;
            }
        }

        return -1;
    }
}
