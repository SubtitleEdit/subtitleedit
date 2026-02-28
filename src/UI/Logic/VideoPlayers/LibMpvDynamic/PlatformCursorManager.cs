using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// Platform-specific cursor management for forcing cursor updates.
/// Used to work around cursor display issues with native window embedding (libmpv with wid).
/// </summary>
public static class PlatformCursorManager
{
    #region Windows API

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr hCursor);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    private const int IDC_ARROW = 32512;

    #endregion

    #region Linux X11 API

    [DllImport("libX11.so.6", EntryPoint = "XDefineCursor")]
    private static extern int XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor);

    [DllImport("libX11.so.6", EntryPoint = "XCreateFontCursor")]
    private static extern IntPtr XCreateFontCursor(IntPtr display, uint shape);

    [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6", EntryPoint = "XDefaultRootWindow")]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    private const uint XC_LEFT_PTR = 68; // Standard arrow cursor in X11

    #endregion

    #region macOS API

    // macOS cursor management would require Objective-C interop
    // For now, we'll rely on Avalonia's cursor management on macOS

    #endregion

    /// <summary>
    /// Forces the OS to refresh the cursor to the standard arrow cursor.
    /// This bypasses framework cursor management and directly calls OS APIs.
    /// </summary>
    public static void ForceArrowCursor()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ForceArrowCursorWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ForceArrowCursorLinux();
            }
            // macOS: rely on Avalonia's cursor management
        }
        catch
        {
            // Silently ignore errors - cursor refresh is a best-effort operation
            // The application should continue working even if cursor refresh fails
        }
    }

    private static void ForceArrowCursorWindows()
    {
        var arrowCursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
        if (arrowCursor != IntPtr.Zero)
        {
            SetCursor(arrowCursor);
        }
    }

    private static void ForceArrowCursorLinux()
    {
        // Note: This is a simplified implementation
        // A full implementation would need the actual window handle
        // For now, this serves as a placeholder for X11 cursor management
        var display = XOpenDisplay(IntPtr.Zero);
        if (display != IntPtr.Zero)
        {
            var cursor = XCreateFontCursor(display, XC_LEFT_PTR);
            if (cursor != IntPtr.Zero)
            {
                var rootWindow = XDefaultRootWindow(display);
                XDefineCursor(display, rootWindow, cursor);
            }
        }
    }
}
