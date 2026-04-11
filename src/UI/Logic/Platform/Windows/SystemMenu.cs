using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Platform.Windows;

public static class SystemMenu
{
    private const uint WM_SYSCOMMAND = 0x0112;
    private const uint TPM_LEFTALIGN = 0x0000;
    private const uint TPM_RETURNCMD = 0x0100;

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    public static void Show(Window window)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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