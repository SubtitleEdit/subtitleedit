using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic;

public static class VulkanHelper
{
    public static bool IsInstalled()
    {
        const string vulkanDll = "vulkan-1.dll";

        var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.System);
        if (File.Exists(Path.Combine(systemRoot, vulkanDll)))
        {
            return true;
        }

        var sysWow64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64");
        if (File.Exists(Path.Combine(sysWow64, vulkanDll)))
        {
            return true;
        }

        var pathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var folder in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                if (File.Exists(Path.Combine(folder, vulkanDll)))
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
}
