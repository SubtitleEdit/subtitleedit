using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Platform.Windows;

public static partial class SystemMenu
{
    private const uint WM_SYSCOMMAND = 0x0112;
    private const int SC_MINIMIZE = 0xF020;
    private const uint TPM_LEFTALIGN = 0x0000;
    private const uint TPM_RETURNCMD = 0x0100;
    private const uint TPM_WORKAREA = 0x10000;

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT lpPoint);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    [LibraryImport("user32.dll")]
    private static partial uint TrackPopupMenuEx(IntPtr hMenu, uint uFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    public static void Show(Window window)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var platformHandle = window.TryGetPlatformHandle();
        if (platformHandle == null)
        {
            return;
        }

        var hwnd = platformHandle.Handle;

        // Defer to after the current key event is fully processed.
        // TrackPopupMenu runs a nested Win32 message loop internally and will
        // fail immediately (return 0) if called while Avalonia's input dispatch
        // is still active or while a modifier key (e.g. Alt) is still held down.
        Dispatcher.UIThread.Post(() => ShowMenu(hwnd), DispatcherPriority.Background);
    }

    /// <summary>
    /// Minimizes the window the way the system menu's Minimize does. Setting
    /// WindowState = Minimized makes Avalonia leave full screen first, so a full screen
    /// window comes back as a normal window; a native minimize keeps full screen (#15127).
    /// </summary>
    public static bool Minimize(Window window)
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        var platformHandle = window.TryGetPlatformHandle();
        if (platformHandle == null)
        {
            return false;
        }

        return PostMessage(platformHandle.Handle, WM_SYSCOMMAND, SC_MINIMIZE, IntPtr.Zero);
    }

    private static void ShowMenu(IntPtr hwnd)
    {
        var menu = GetSystemMenu(hwnd, false);
        if (menu == IntPtr.Zero)
        {
            return;
        }

        // Anchor at the top-left of the client area, just below the caption, which is where
        // Windows itself opens the Alt+Space menu. Anchoring at the cursor put the menu on
        // whichever monitor the mouse was on, and with the mouse over the taskbar the menu
        // was clamped to the monitor instead of the work area and lost its last item behind
        // the taskbar (#15128). TPM_WORKAREA keeps it inside the work area regardless.
        var point = new POINT();
        if (!ClientToScreen(hwnd, ref point))
        {
            GetCursorPos(out point);
        }

        SetForegroundWindow(hwnd);
        var cmd = TrackPopupMenuEx(menu, TPM_LEFTALIGN | TPM_RETURNCMD | TPM_WORKAREA, point.X, point.Y, hwnd, IntPtr.Zero);
        if (cmd != 0)
        {
            PostMessage(hwnd, WM_SYSCOMMAND, (IntPtr)cmd, IntPtr.Zero);
        }
    }
}