using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Platform.Windows;

/// <summary>
/// Sets the Win32 "immersive dark mode" flag on a window so the OS-drawn title bar follows
/// Subtitle Edit's dark/light theme instead of staying light (reported by OmrSi, #10766).
/// No-op on non-Windows platforms.
/// </summary>
public static class WindowsDarkMode
{
    // Windows 10 1903+ and Windows 11 use attribute 20; Windows 10 1809 (build 17763) used 19.
    // Try the modern attribute first and fall back so older Windows 10 still themes the caption.
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

    public static void Apply(IntPtr hwnd, bool dark)
    {
        if (hwnd == IntPtr.Zero || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var value = dark ? 1 : 0;
        if (DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref value, sizeof(int)) != 0)
        {
            DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkModeBefore20H1, ref value, sizeof(int));
        }
    }
}
