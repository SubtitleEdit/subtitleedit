using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

public enum GpuVendor
{
    Unknown,
    Nvidia,
    Amd,
    Intel,
    Apple,
}

/// <summary>One graphics adapter found on this computer.</summary>
/// <param name="Name">The adapter name as the driver reports it, e.g. "NVIDIA GeForce RTX 4070".</param>
/// <param name="Vendor">Who made it.</param>
/// <param name="DriverVersion">The vendor's driver version (for NVIDIA e.g. "581.57"), or null when not known.</param>
/// <param name="MemoryBytes">Dedicated video memory, or null when not known.</param>
/// <param name="CudaIndex">The device number CUDA gives it (the N of "cuda:N"), or null when not known or not a CUDA device.</param>
/// <param name="ComputeCapability">The CUDA compute capability (e.g. 8.9), or null when not known or not a CUDA device.</param>
public sealed record GpuInfo(string Name, GpuVendor Vendor, string? DriverVersion = null, long? MemoryBytes = null, int? CudaIndex = null, Version? ComputeCapability = null);

/// <summary>What this computer can run a GPU build on.</summary>
/// <param name="Gpus">The hardware adapters found (software and remote-display adapters are left out).</param>
/// <param name="CudaDriverInstalled">True when an NVIDIA driver with CUDA support is installed.</param>
/// <param name="VulkanAvailable">True when a Vulkan loader is installed.</param>
/// <param name="CudaVersion">The newest CUDA version the NVIDIA driver supports (13000 = 13.0), or 0 when not known.</param>
public sealed record GpuCapability(IReadOnlyList<GpuInfo> Gpus, bool CudaDriverInstalled, bool VulkanAvailable, int CudaVersion = 0)
{
    public static readonly GpuCapability None = new(Array.Empty<GpuInfo>(), false, false);
}

/// <summary>
/// Looks at the installed graphics adapters and drivers, so a build prompt can say which of
/// CPU / Vulkan / CUDA fits this computer instead of leaving the user to guess (#16944).
/// Detection only reads the registry (Windows) or /proc and /sys (Linux) - no process is started,
/// so it is quick enough to call right before a prompt is shown. It recommends, it never decides:
/// what an adapter can run is not a promise that a given engine build works on it.
/// </summary>
public static class GpuCapabilityHelper
{
    public const string VariantCpu = "cpu";
    public const string VariantVulkan = "vulkan";
    public const string VariantCuda = "cuda";

    private const string WindowsDisplayClassKey = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";

    private static GpuCapability? _cached;
    private static Task<GpuCapability>? _withCudaTask;

    /// <summary>The capability of this computer. Detected once per run; never throws.</summary>
    public static GpuCapability Detect()
    {
        return _cached ??= DetectNow();
    }

    /// <summary>
    /// <see cref="Detect"/> plus what the NVIDIA driver says (CUDA version, device numbers and
    /// compute capability). Asking the driver loads it, which can be slow or - with a broken
    /// driver - hang, so it runs on a worker thread and is given up on after a few seconds; the
    /// registry / /proc result is returned then. Never throws.
    /// </summary>
    public static async Task<GpuCapability> DetectWithCudaAsync()
    {
        var basic = Detect();
        if (!basic.CudaDriverInstalled)
        {
            return basic;
        }

        _withCudaTask ??= Task.Run(() => WithCudaDeviceNumbers(basic));
        try
        {
            return await _withCudaTask.WaitAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(true);
        }
        catch
        {
            return basic;
        }
    }

    private static GpuCapability DetectNow()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return DetectWindows();
            }

            if (OperatingSystem.IsLinux())
            {
                return DetectLinux();
            }

            if (OperatingSystem.IsMacOS())
            {
                // One build per architecture on macOS; Apple Silicon always runs on Metal.
                return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                    ? new GpuCapability(new[] { new GpuInfo("Apple Silicon", GpuVendor.Apple) }, false, false)
                    : GpuCapability.None;
            }
        }
        catch
        {
            // A locked-down registry or an unreadable /proc must not break the download prompt.
        }

        return GpuCapability.None;
    }

    /// <summary>
    /// The build to suggest: "cuda" for an NVIDIA adapter with a CUDA driver, "vulkan" for any
    /// other hardware adapter when a Vulkan loader is installed, else "cpu".
    /// </summary>
    public static string RecommendVariant(GpuCapability capability)
    {
        return GpuBuildHint.Recommend(capability, GpuBuild.Cpu, GpuBuild.Vulkan, GpuBuild.Cuda) switch
        {
            GpuBuild.Cuda => VariantCuda,
            GpuBuild.Vulkan => VariantVulkan,
            _ => VariantCpu,
        };
    }

    /// <summary>
    /// The adapters in the order to show them: by CUDA device number where known, then NVIDIA,
    /// then AMD, then by memory. Listing all of them matters on a computer with several - which
    /// one an engine runs on is the engine's choice (for CUDA normally device 0), not something
    /// this check decides.
    /// </summary>
    public static IReadOnlyList<GpuInfo> GetGpusForDisplay(GpuCapability capability)
    {
        return capability.Gpus
            .OrderBy(g => g.CudaIndex ?? int.MaxValue)
            .ThenByDescending(g => g.Vendor == GpuVendor.Nvidia)
            .ThenByDescending(g => g.Vendor == GpuVendor.Amd)
            .ThenByDescending(g => g.MemoryBytes ?? 0)
            .ToList();
    }

    /// <summary>
    /// Adds the CUDA device numbers to the NVIDIA adapters, so a prompt can say which card is
    /// "cuda:0" and which is "cuda:1". Kept apart from <see cref="Detect"/> because it asks the
    /// driver, which is slower than reading the registry - call it where the numbers are shown.
    /// </summary>
    public static GpuCapability WithCudaDeviceNumbers(GpuCapability capability)
    {
        if (!capability.CudaDriverInstalled)
        {
            return capability;
        }

        var driver = CudaDeviceEnumerator.GetDriverInfo();
        var numbered = WithCudaDeviceNumbers(capability, driver.Devices);
        return driver.CudaVersion > 0 ? numbered with { CudaVersion = driver.CudaVersion } : numbered;
    }

    /// <summary>
    /// Matches CUDA's devices to the adapters by name. Two cards of the same model are
    /// interchangeable, so each CUDA device takes the first unnumbered adapter with its name.
    /// A CUDA device no adapter was found for is added, so the list is never short of one.
    /// </summary>
    public static GpuCapability WithCudaDeviceNumbers(GpuCapability capability, IReadOnlyList<CudaDevice> cudaDevices)
    {
        if (cudaDevices.Count == 0)
        {
            return capability;
        }

        var gpus = capability.Gpus.ToList();
        var driverVersion = gpus.FirstOrDefault(g => g.Vendor == GpuVendor.Nvidia)?.DriverVersion;
        foreach (var device in cudaDevices.OrderBy(d => d.Index))
        {
            var match = gpus.FindIndex(g =>
                g.Vendor == GpuVendor.Nvidia &&
                g.CudaIndex == null &&
                string.Equals(g.Name, device.Name, StringComparison.OrdinalIgnoreCase));
            if (match < 0)
            {
                // Linux lists a card whose model /proc did not give as "NVIDIA GPU".
                match = gpus.FindIndex(g => g.Vendor == GpuVendor.Nvidia && g.CudaIndex == null && g.Name == "NVIDIA GPU");
            }

            var memory = device.MemoryBytes > 0 ? device.MemoryBytes : (long?)null;
            if (match >= 0)
            {
                gpus[match] = gpus[match] with
                {
                    Name = string.IsNullOrEmpty(device.Name) ? gpus[match].Name : device.Name,
                    CudaIndex = device.Index,
                    MemoryBytes = gpus[match].MemoryBytes ?? memory,
                    ComputeCapability = device.ComputeCapability,
                };
            }
            else
            {
                gpus.Add(new GpuInfo(string.IsNullOrEmpty(device.Name) ? "NVIDIA GPU" : device.Name, GpuVendor.Nvidia, driverVersion, memory, device.Index, device.ComputeCapability));
            }
        }

        return capability with { Gpus = gpus };
    }

    /// <summary>The first adapter of <see cref="GetGpusForDisplay"/>, or null when there is none.</summary>
    public static GpuInfo? GetPrimaryGpu(GpuCapability capability)
    {
        return GetGpusForDisplay(capability).FirstOrDefault();
    }

    /// <summary>"NVIDIA GeForce RTX 4070 (CUDA device 0, driver 581.57, 12 GB)" - the details that are known, in that order.</summary>
    public static string Describe(GpuInfo gpu)
    {
        var details = new List<string>();
        if (gpu.CudaIndex != null)
        {
            details.Add("CUDA device " + gpu.CudaIndex.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrEmpty(gpu.DriverVersion))
        {
            details.Add("driver " + gpu.DriverVersion);
        }

        if (gpu.MemoryBytes is > 0)
        {
            // Drivers report a little less than the size on the box (a 32 GB card as 31.4 GB),
            // never more, so a part-used gigabyte is rounded up to the size the user knows it by.
            var gb = gpu.MemoryBytes.Value / (1024.0 * 1024.0 * 1024.0);
            details.Add(gb >= 1
                ? Math.Ceiling(gb - 0.05).ToString("0", CultureInfo.InvariantCulture) + " GB"
                : Math.Round(gb * 1024).ToString("0", CultureInfo.InvariantCulture) + " MB");
        }

        return details.Count == 0 ? gpu.Name : $"{gpu.Name} ({string.Join(", ", details)})";
    }

    public static GpuVendor GetVendor(string? name, string? vendorId = null)
    {
        var id = (vendorId ?? string.Empty).Trim().ToLowerInvariant();
        if (id.StartsWith("0x", StringComparison.Ordinal))
        {
            id = id.Substring(2);
        }

        switch (id)
        {
            case "10de": return GpuVendor.Nvidia;
            case "1002": return GpuVendor.Amd;
            case "8086": return GpuVendor.Intel;
        }

        var n = name ?? string.Empty;
        if (n.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("GeForce", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("Quadro", StringComparison.OrdinalIgnoreCase))
        {
            return GpuVendor.Nvidia;
        }

        if (n.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
        {
            return GpuVendor.Amd;
        }

        if (n.Contains("Intel", StringComparison.OrdinalIgnoreCase))
        {
            return GpuVendor.Intel;
        }

        return GpuVendor.Unknown;
    }

    /// <summary>
    /// The NVIDIA driver version from the Windows one: the last five digits are the NVIDIA version,
    /// so "32.0.15.8157" is "581.57". Returns null when the text is not a Windows driver version.
    /// </summary>
    public static string? NvidiaVersionFromWindowsDriverVersion(string? windowsDriverVersion)
    {
        var parts = (windowsDriverVersion ?? string.Empty).Trim().Split('.');
        if (parts.Length != 4 || parts.Any(p => p.Length == 0 || !p.All(char.IsAsciiDigit)))
        {
            return null;
        }

        var digits = parts[2] + parts[3].PadLeft(4, '0');
        if (digits.Length < 5)
        {
            return null;
        }

        var lastFive = digits.Substring(digits.Length - 5);
        return lastFive.Substring(0, 3).TrimStart('0').PadLeft(1, '0') + "." + lastFive.Substring(3);
    }

    /// <summary>The driver version in /proc/driver/nvidia/version, e.g. "580.95.05", or null.</summary>
    public static string? ParseLinuxNvidiaDriverVersion(string? procVersionText)
    {
        // "NVRM version: NVIDIA UNIX x86_64 Kernel Module  580.95.05  Tue Sep ..." and the
        // open module's "NVIDIA UNIX Open Kernel Module for x86_64  580.95.05  Release Build".
        var match = Regex.Match(procVersionText ?? string.Empty, @"Kernel Module(?:\s+for\s+\S+)?\s+(\d+(?:\.\d+)+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>The "Model:" line of /proc/driver/nvidia/gpus/*/information, or null.</summary>
    public static string? ParseLinuxNvidiaModel(string? informationText)
    {
        var match = Regex.Match(informationText ?? string.Empty, @"^Model:\s*(.+?)\s*$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value : null;
    }

    [SupportedOSPlatform("windows")]
    private static GpuCapability DetectWindows()
    {
        var gpus = new List<GpuInfo>();
        using (var classKey = Registry.LocalMachine.OpenSubKey(WindowsDisplayClassKey))
        {
            if (classKey != null)
            {
                foreach (var subKeyName in classKey.GetSubKeyNames())
                {
                    // Adapters are the four-digit keys ("0000", "0001"); "Properties" and
                    // "Configuration" sit beside them and are not readable without elevation.
                    if (subKeyName.Length != 4 || !subKeyName.All(char.IsAsciiDigit))
                    {
                        continue;
                    }

                    try
                    {
                        using var key = classKey.OpenSubKey(subKeyName);
                        var name = key?.GetValue("DriverDesc") as string;
                        if (key == null || string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        var matchingDeviceId = key.GetValue("MatchingDeviceId") as string ?? string.Empty;
                        var vendorMatch = Regex.Match(matchingDeviceId, @"VEN_([0-9A-Fa-f]{4})");
                        var vendor = GetVendor(name, vendorMatch.Success ? vendorMatch.Groups[1].Value : null);
                        if (vendor == GpuVendor.Unknown)
                        {
                            continue; // Microsoft Basic Display Adapter, remote-desktop and virtual displays
                        }

                        var driverVersion = key.GetValue("DriverVersion") as string;
                        if (vendor == GpuVendor.Nvidia)
                        {
                            driverVersion = NvidiaVersionFromWindowsDriverVersion(driverVersion) ?? driverVersion;
                        }

                        gpus.Add(new GpuInfo(name.Trim(), vendor, driverVersion, ReadWindowsMemorySize(key)));
                    }
                    catch
                    {
                        // skip an adapter key that cannot be read
                    }
                }
            }
        }

        // The same adapter can be listed more than once (a stale entry after a driver update).
        gpus = gpus
            .GroupBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(x => x.MemoryBytes ?? 0).First())
            .ToList();

        var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var cudaDriver = File.Exists(Path.Combine(system32, "nvcuda.dll"));
        return new GpuCapability(gpus, cudaDriver, VulkanHelper.IsInstalled());
    }

    [SupportedOSPlatform("windows")]
    private static long? ReadWindowsMemorySize(RegistryKey key)
    {
        // qwMemorySize is the 64-bit value; MemorySize is 32-bit and wraps on adapters over 4 GB.
        switch (key.GetValue("HardwareInformation.qwMemorySize"))
        {
            case long l when l > 0: return l;
            case byte[] { Length: >= 8 } bytes: return BitConverter.ToInt64(bytes, 0);
        }

        switch (key.GetValue("HardwareInformation.MemorySize"))
        {
            case int i when i > 0: return i;
            case byte[] { Length: >= 4 } bytes: return BitConverter.ToUInt32(bytes, 0);
        }

        return null;
    }

    private static GpuCapability DetectLinux()
    {
        var gpus = new List<GpuInfo>();

        // The NVIDIA proprietary/open kernel module publishes itself here; nouveau does not, and
        // nouveau cannot run CUDA, so this file is the "CUDA driver installed" test.
        const string nvidiaProc = "/proc/driver/nvidia";
        var nvidiaVersion = File.Exists(nvidiaProc + "/version")
            ? ParseLinuxNvidiaDriverVersion(File.ReadAllText(nvidiaProc + "/version"))
            : null;
        var cudaDriver = nvidiaVersion != null;
        if (cudaDriver && Directory.Exists(nvidiaProc + "/gpus"))
        {
            foreach (var gpuFolder in Directory.GetDirectories(nvidiaProc + "/gpus"))
            {
                var informationFile = Path.Combine(gpuFolder, "information");
                var model = File.Exists(informationFile) ? ParseLinuxNvidiaModel(File.ReadAllText(informationFile)) : null;
                gpus.Add(new GpuInfo(string.IsNullOrEmpty(model) ? "NVIDIA GPU" : model, GpuVendor.Nvidia, nvidiaVersion));
            }
        }

        const string drm = "/sys/class/drm";
        if (Directory.Exists(drm))
        {
            foreach (var card in Directory.GetDirectories(drm, "card*"))
            {
                // "card0" is an adapter; "card0-DP-1" is one of its connectors.
                if (Path.GetFileName(card).Contains('-'))
                {
                    continue;
                }

                var vendorFile = Path.Combine(card, "device", "vendor");
                if (!File.Exists(vendorFile))
                {
                    continue;
                }

                var vendor = GetVendor(null, File.ReadAllText(vendorFile));
                if (vendor == GpuVendor.Unknown || (vendor == GpuVendor.Nvidia && gpus.Any(g => g.Vendor == GpuVendor.Nvidia)))
                {
                    continue;
                }

                var name = vendor switch
                {
                    GpuVendor.Nvidia => "NVIDIA GPU",
                    GpuVendor.Amd => "AMD GPU",
                    _ => "Intel GPU",
                };
                if (!gpus.Any(g => g.Vendor == vendor))
                {
                    gpus.Add(new GpuInfo(name, vendor));
                }
            }
        }

        return new GpuCapability(gpus, cudaDriver, IsLinuxVulkanLoaderInstalled());
    }

    private static bool IsLinuxVulkanLoaderInstalled()
    {
        var folders = new[]
        {
            "/usr/lib/x86_64-linux-gnu",
            "/usr/lib/aarch64-linux-gnu",
            "/usr/lib64",
            "/usr/lib",
            "/lib/x86_64-linux-gnu",
            "/lib64",
        };

        return folders.Any(f => File.Exists(Path.Combine(f, "libvulkan.so.1")));
    }
}
