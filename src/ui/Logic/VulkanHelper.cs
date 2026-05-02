using System;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public static class VulkanHelper
{
    private const string VulkanDll = "vulkan-1.dll";

    public static bool IsInstalled()
    {
        var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.System);
        if (File.Exists(Path.Combine(systemRoot, VulkanDll)))
        {
            return true;
        }

        var sysWow64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64");
        if (File.Exists(Path.Combine(sysWow64, VulkanDll)))
        {
            return true;
        }

        var pathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var folder in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                if (File.Exists(Path.Combine(folder, VulkanDll)))
                {
                    return true;
                }
            }
            catch
            {
                // ignore invalid/inaccessible path entries
            }
        }

        return false;
    }

    public static string? TryFindBinFolder()
    {
        var sdkRoot = Environment.GetEnvironmentVariable("VULKAN_SDK");
        if (!string.IsNullOrEmpty(sdkRoot))
        {
            var binFolder = Path.Combine(sdkRoot, "Bin");
            if (File.Exists(Path.Combine(binFolder, VulkanDll)))
            {
                return binFolder;
            }
        }

        const string defaultRoot = @"C:\VulkanSDK";
        if (Directory.Exists(defaultRoot))
        {
            try
            {
                var versionedFolders = Directory.GetDirectories(defaultRoot)
                    .Select(d => new { Path = d, Version = TryParseVersion(Path.GetFileName(d)) })
                    .Where(x => x.Version != null)
                    .OrderByDescending(x => x.Version);

                foreach (var entry in versionedFolders)
                {
                    var binFolder = Path.Combine(entry.Path, "Bin");
                    if (File.Exists(Path.Combine(binFolder, VulkanDll)))
                    {
                        return binFolder;
                    }
                }
            }
            catch
            {
                // ignore inaccessible install root
            }
        }

        return null;
    }

    private static Version? TryParseVersion(string name)
    {
        return Version.TryParse(name, out var v) ? v : null;
    }
}
