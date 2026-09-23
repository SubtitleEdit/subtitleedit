using Avalonia.Controls;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Keeps a native Windows file/folder picker inside the work area of its owner's monitor.
///
/// The common item dialog opens anchored to its owner's top-left corner at the size it had the
/// last time the user closed it, and Windows does not clamp it to the monitor. A picker the user
/// once made wide, opened from a dialog that sits away from the left edge (Speech to text, the
/// secondary subtitle dialog, ...), spilled over onto the next monitor (#13515). Avalonia owns
/// the IFileDialog, so there is nothing to hook on the dialog itself; instead a WinEvent hook
/// scoped to this process watches for the picker's window to be shown and moves it (and, if it
/// is larger than the work area, shrinks it) so it fits on the owner's monitor.
///
/// Windows only; everywhere else <see cref="Start"/> returns null.
/// </summary>
internal sealed partial class FileDialogPlacement : IDisposable
{
    private const uint EventObjectShow = 0x8002;
    private const uint WinEventOutOfContext = 0x0000;
    private const int ObjIdWindow = 0;
    private const uint GwOwner = 4;
    private const uint MonitorDefaultToNearest = 2;
    private const uint DwmwaExtendedFrameBounds = 9;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpAsyncWindowPos = 0x4000;

    private readonly IntPtr _ownerHandle;
    private readonly WinEventDelegate _callback;
    private IntPtr _hook;

    private FileDialogPlacement(IntPtr ownerHandle)
    {
        _ownerHandle = ownerHandle;

        // Kept in a field for the hook's lifetime - the GC must not collect the delegate while
        // user32 still holds the callback pointer.
        _callback = OnWinEvent;
    }

    /// <summary>
    /// Starts watching for a picker owned by <paramref name="topLevel"/>. Dispose the result when
    /// the picker has closed. Returns null when there is nothing to do (not Windows, no native
    /// handle, not on the UI thread, or the hook could not be installed).
    /// </summary>
    public static FileDialogPlacement? Start(TopLevel topLevel)
    {
        if (!OperatingSystem.IsWindows() || !Dispatcher.UIThread.CheckAccess())
        {
            return null;
        }

        try
        {
            var ownerHandle = topLevel.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (ownerHandle == IntPtr.Zero)
            {
                return null;
            }

            var placement = new FileDialogPlacement(ownerHandle);

            // Out-of-context callbacks are delivered on this (the UI) thread's message loop,
            // which keeps pumping while the picker is awaited.
            placement._hook = SetWinEventHook(EventObjectShow, EventObjectShow, IntPtr.Zero,
                placement._callback, (uint)Environment.ProcessId, 0, WinEventOutOfContext);
            if (placement._hook == IntPtr.Zero)
            {
                Se.LogError("SetWinEventHook(EVENT_OBJECT_SHOW) failed - file picker may open off screen");
                return null;
            }

            return placement;
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
            return null;
        }
    }

    public void Dispose()
    {
        if (_hook != IntPtr.Zero)
        {
            UnhookWinEvent(_hook);
            _hook = IntPtr.Zero;
        }
    }

    private void OnWinEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (eventType != EventObjectShow || idObject != ObjIdWindow || hwnd == IntPtr.Zero)
        {
            return;
        }

        try
        {
            // Only the picker's own top-level window: owned by the window that opened it and of
            // the dialog class. Child dialogs inside the picker have no owner.
            if (GetWindow(hwnd, GwOwner) != _ownerHandle || !IsDialogClass(hwnd))
            {
                return;
            }

            FitIntoWorkArea(hwnd);
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
        }
    }

    private void FitIntoWorkArea(IntPtr hwnd)
    {
        if (!GetWindowRect(hwnd, out var windowRect))
        {
            return;
        }

        // Windows 10+ windows have invisible resize borders that GetWindowRect includes; fit the
        // visible frame so the picker lands flush with the work area edge.
        var frame = windowRect;
        if (DwmGetWindowAttribute(hwnd, DwmwaExtendedFrameBounds, out var extendedFrame, Marshal.SizeOf<Rect>()) == 0)
        {
            frame = extendedFrame;
        }

        var monitor = MonitorFromWindow(_ownerHandle, MonitorDefaultToNearest);
        var monitorInfo = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (monitor == IntPtr.Zero || !GetMonitorInfoW(monitor, ref monitorInfo))
        {
            return;
        }

        var fitted = Fit(windowRect, frame, monitorInfo.WorkArea);
        if (fitted is not { } bounds)
        {
            return;
        }

        var flags = SwpNoZOrder | SwpNoActivate | SwpAsyncWindowPos;
        if (bounds.Right - bounds.Left == windowRect.Right - windowRect.Left &&
            bounds.Bottom - bounds.Top == windowRect.Bottom - windowRect.Top)
        {
            flags |= SwpNoSize;
        }

        SetWindowPos(hwnd, IntPtr.Zero, bounds.Left, bounds.Top,
            bounds.Right - bounds.Left, bounds.Bottom - bounds.Top, flags);
    }

    /// <summary>
    /// Returns the window rectangle that puts the visible <paramref name="frame"/> inside
    /// <paramref name="workArea"/> (shrunk if it does not fit), or null when it already fits.
    /// <paramref name="windowRect"/> is the full window rectangle, which may be larger than the
    /// frame by the invisible resize borders; those borders are kept around the fitted frame.
    /// </summary>
    internal static Rect? Fit(Rect windowRect, Rect frame, Rect workArea)
    {
        var frameWidth = Math.Min(frame.Right - frame.Left, workArea.Right - workArea.Left);
        var frameHeight = Math.Min(frame.Bottom - frame.Top, workArea.Bottom - workArea.Top);
        var frameLeft = Math.Clamp(frame.Left, workArea.Left, workArea.Right - frameWidth);
        var frameTop = Math.Clamp(frame.Top, workArea.Top, workArea.Bottom - frameHeight);

        if (frameLeft == frame.Left && frameTop == frame.Top &&
            frameWidth == frame.Right - frame.Left && frameHeight == frame.Bottom - frame.Top)
        {
            return null;
        }

        return new Rect
        {
            Left = frameLeft - (frame.Left - windowRect.Left),
            Top = frameTop - (frame.Top - windowRect.Top),
            Right = frameLeft + frameWidth + (windowRect.Right - frame.Right),
            Bottom = frameTop + frameHeight + (windowRect.Bottom - frame.Bottom),
        };
    }

    private static bool IsDialogClass(IntPtr hwnd)
    {
        var className = new char[16];
        var length = GetClassName(hwnd, className, className.Length);
        return length > 0 && new string(className, 0, length) == "#32770";
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;
    }

    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [LibraryImport("user32.dll")]
    private static partial IntPtr SetWinEventHook(uint eventMin, uint eventMax,
        IntPtr hmodWinEventProc, WinEventDelegate pfnWinEventProc, uint idProcess, uint idThread,
        uint dwFlags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnhookWinEvent(IntPtr hWinEventHook);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [LibraryImport("user32.dll", EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int GetClassName(IntPtr hWnd, [Out] char[] lpClassName, int nMaxCount);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmGetWindowAttribute(IntPtr hwnd, uint dwAttribute, out Rect pvAttribute, int cbAttribute);

    [LibraryImport("user32.dll")]
    private static partial IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMonitorInfoW(IntPtr hMonitor, ref MonitorInfo lpmi);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
}
