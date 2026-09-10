using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// Platform-specific cursor management for forcing cursor updates.
/// Used to work around cursor display issues with native window embedding (libmpv with wid).
/// </summary>
public static partial class PlatformCursorManager
{
    #region Windows API

    [LibraryImport("user32.dll")]
    private static partial IntPtr SetCursor(IntPtr hCursor);

    [LibraryImport("user32.dll", EntryPoint = "LoadCursorW")]
    private static partial IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    private const int IDC_ARROW = 32512;

    #endregion

    #region Linux X11 API

    [LibraryImport("libX11.so.6", EntryPoint = "XDefineCursor")]
    private static partial int XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor);

    [LibraryImport("libX11.so.6", EntryPoint = "XCreateFontCursor")]
    private static partial IntPtr XCreateFontCursor(IntPtr display, uint shape);

    [LibraryImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    private static partial IntPtr XOpenDisplay(IntPtr display);

    [LibraryImport("libX11.so.6", EntryPoint = "XDefaultRootWindow")]
    private static partial IntPtr XDefaultRootWindow(IntPtr display);

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
            if (OperatingSystem.IsWindows())
            {
                ForceArrowCursorWindows();
            }
            else if (OperatingSystem.IsLinux())
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

    /// <summary>
    /// Forces the OS to hide the cursor by clearing it directly via OS APIs.
    /// Used together with WM_SETCURSOR handling so the pointer stays hidden over the
    /// embedded video window (libmpv with wid) while the full-screen overlay is hidden.
    /// </summary>
    public static void HideCursor()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                SetCursor(IntPtr.Zero);
            }
            // Linux/macOS: rely on Avalonia's cursor management (StandardCursorType.None)
        }
        catch
        {
            // Silently ignore errors - cursor hiding is a best-effort operation
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
