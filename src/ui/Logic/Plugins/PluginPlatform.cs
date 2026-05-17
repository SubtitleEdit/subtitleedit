using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Platform keys used in <see cref="PluginIndexEntry.Downloads"/> to pick the
/// right plugin zip for the current OS + architecture (e.g. "win-x64",
/// "linux-arm64", "osx-arm64"). Keys take the form "{os}-{arch}" where
/// <c>os</c> is one of <c>win</c>, <c>linux</c>, <c>osx</c> and <c>arch</c>
/// is one of <c>x64</c>, <c>arm64</c>, <c>x86</c>, <c>arm</c>.
/// </summary>
public static class PluginPlatform
{
    public static string? GetCurrentKey()
    {
        string os;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            os = "win";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            os = "osx";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            os = "linux";
        }
        else
        {
            return null;
        }

        var arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            _ => null,
        };

        return arch == null ? null : os + "-" + arch;
    }

    /// <summary>Returns the download URL matching the current platform, or null if none.</summary>
    public static string? ResolveDownloadUrl(PluginIndexEntry entry)
    {
        if (entry.Downloads == null || entry.Downloads.Count == 0)
        {
            return null;
        }

        var key = GetCurrentKey();
        if (key == null)
        {
            return null;
        }

        var match = entry.Downloads
            .FirstOrDefault(kv => string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrWhiteSpace(match.Value) ? null : match.Value;
    }

    public static bool IsSupportedByEntry(PluginIndexEntry entry)
    {
        return !string.IsNullOrEmpty(ResolveDownloadUrl(entry));
    }
}
