using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Platform.Windows;

public static partial class SystemMenu
{
    private const uint WM_SYSCOMMAND = 0x0112;
    private const uint TPM_LEFTALIGN = 0x0000;
    private const uint TPM_RETURNCMD = 0x0100;

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT lpPoint);

    [LibraryImport("user32.dll")]
    private static partial uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

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

    private static void ShowMenu(IntPtr hwnd)
    {
        var menu = GetSystemMenu(hwnd, false);
        if (menu == IntPtr.Zero)
        {
            return;
        }

        GetCursorPos(out var point);
        SetForegroundWindow(hwnd);
        var cmd = TrackPopupMenu(menu, TPM_LEFTALIGN | TPM_RETURNCMD, point.X, point.Y, 0, hwnd, IntPtr.Zero);
        if (cmd != 0)
        {
            PostMessage(hwnd, WM_SYSCOMMAND, (IntPtr)cmd, IntPtr.Zero);
        }
    }
}