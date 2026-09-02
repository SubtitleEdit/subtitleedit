using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Tracks who owned the Windows foreground, so an activation the user aimed at a window can be
/// told apart from one the OS handed over by itself.
///
/// The undocked foreground-steal watcher (#14168) classifies tool-window activations and hands
/// the foreground back to the main window when the OS picked the tool window on its own. Deciding
/// that from the physical pointer buttons alone misreads every keyboard-aimed switch: Alt+Tab to
/// the undocked waveform flickered into the foreground and lost it 200 ms later (#14354).
///
/// Two signals are collected, both from WinEvent hooks:
///
/// * The task switcher. The classic switch window raises EVENT_SYSTEM_SWITCHSTART when it opens
///   and EVENT_SYSTEM_SWITCHEND when it commits, right before the chosen window is activated.
///   Neither reached SE on the reporter's Windows 11, whose switcher is the XAML one the shell
///   hosts (window class MultitaskingViewFrame / XamlExplorerHostIslandWindow) - so that window
///   simply taking the foreground counts as the same signal.
///
/// * The foreground history. <see cref="PreviousForegroundWindowStillUsable"/> answers what became
///   of the window that held the foreground before the current one. This is the signal that does
///   not depend on the switcher announcing itself in any way: the OS only has to pick a new
///   foreground window when the old one went away, so a previous window that is still there means
///   the user moved on deliberately - Alt+Tab, the taskbar, Alt+Esc, Win+Tab alike - while one
///   that was closed or minimized is the handover the watcher exists to correct.
///
/// Windows only; on other platforms nothing is hooked, <see cref="TaskSwitchJustCommitted"/> stays
/// false and <see cref="PreviousForegroundWindowStillUsable"/> stays null (unknown), which leaves
/// the caller on its pointer-only rules. The hooks must be installed from a thread that pumps
/// messages (the UI thread) and live for the process lifetime.
/// </summary>
public static class ForegroundWindowTracker
{
    private const uint EventSystemForeground = 0x0003;
    private const uint EventSystemSwitchStart = 0x0014;
    private const uint EventSystemSwitchEnd = 0x0015;
    private const uint WinEventOutOfContext = 0x0000;

    // The task switcher's own window, across the shells that have hosted it. The XAML ones take
    // the foreground themselves, so they must be kept out of the history as well as recognised.
    private static readonly string[] TaskSwitcherWindowClasses =
    {
        "XamlExplorerHostIslandWindow", // Windows 11 Alt+Tab and Task view
        "MultitaskingViewFrame",        // Windows 10 Alt+Tab and Task view
        "TaskSwitcherWnd",              // Windows 7/8 switcher
        "#32771",                       // the classic switch window (AltTabSettings)
    };

    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
        IntPtr hmodWinEventProc, WinEventDelegate pfnWinEventProc, uint idProcess, uint idThread,
        uint dwFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    // Keeps the delegate alive for the lifetime of the hooks - the GC must not collect it while
    // user32 still holds the callback pointer.
    private static WinEventDelegate? _callback;
    private static bool _initialized;
    private static bool _foregroundHookInstalled;
    private static bool _switcherOpen;
    private static long _lastSwitchEndTick;
    private static long _lastSwitcherForegroundTick;
    private static IntPtr _currentForeground;
    private static IntPtr _previousForeground;

    /// <summary>
    /// Installs the hooks once. Call from the UI thread. No-op off Windows; a hook that could not
    /// be installed just leaves its signal inert (fail-safe) and is logged, because a silent
    /// install failure is otherwise indistinguishable from an event that never fires.
    /// </summary>
    public static void EnsureStarted()
    {
        if (_initialized || !OperatingSystem.IsWindows())
        {
            return;
        }

        _initialized = true;

        try
        {
            _callback = OnWinEvent;

            // The window that holds the foreground right now is the first history entry -
            // otherwise the very first switch away from it has no "previous" to judge.
            _currentForeground = GetForegroundWindow();

            var foregroundHook = SetWinEventHook(EventSystemForeground, EventSystemForeground,
                IntPtr.Zero, _callback, 0, 0, WinEventOutOfContext);
            _foregroundHookInstalled = foregroundHook != IntPtr.Zero;
            if (!_foregroundHookInstalled)
            {
                Se.LogError("SetWinEventHook(EVENT_SYSTEM_FOREGROUND) failed - undocked windows " +
                            "fall back to pointer-only activation rules");
            }

            if (SetWinEventHook(EventSystemSwitchStart, EventSystemSwitchEnd, IntPtr.Zero,
                    _callback, 0, 0, WinEventOutOfContext) == IntPtr.Zero)
            {
                Se.LogError("SetWinEventHook(EVENT_SYSTEM_SWITCHSTART) failed");
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
        }
    }

    private static void OnWinEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (eventType == EventSystemSwitchStart)
        {
            _switcherOpen = true;
            return;
        }

        if (eventType == EventSystemSwitchEnd)
        {
            _switcherOpen = false;
            _lastSwitchEndTick = Environment.TickCount64;
            return;
        }

        if (eventType != EventSystemForeground || hwnd == IntPtr.Zero)
        {
            return;
        }

        if (IsTaskSwitcherWindow(hwnd))
        {
            // The switcher taking the foreground IS the Alt+Tab signal on the shells that raise
            // no SWITCHSTART. It must not enter the history: it is gone by the time the window
            // the user picked is activated, which would read as "the previous window went away".
            _lastSwitcherForegroundTick = Environment.TickCount64;
            return;
        }

        if (hwnd == _currentForeground)
        {
            return;
        }

        _previousForeground = _currentForeground;
        _currentForeground = hwnd;
    }

    private static bool IsTaskSwitcherWindow(IntPtr hwnd)
    {
        var className = new StringBuilder(64);
        if (GetClassName(hwnd, className, className.Capacity) == 0)
        {
            return false;
        }

        var name = className.ToString();
        foreach (var switcherClass in TaskSwitcherWindowClasses)
        {
            if (string.Equals(name, switcherClass, StringComparison.Ordinal))
            {
                return true;
            }
        }

        // The XAML host has been renamed once already across shell versions; match the family.
        return name.StartsWith("XamlExplorerHost", StringComparison.Ordinal);
    }

    /// <summary>
    /// True while the task switcher is open, or within <paramref name="withinMs"/> of it
    /// committing a switch. The activation the switcher causes arrives on the message queue right
    /// after it closes, so a small window is plenty - it only needs to be generous enough to
    /// survive dispatcher latency, not user-scale time.
    /// </summary>
    public static bool TaskSwitchJustCommitted(int withinMs = 1000)
    {
        var now = Environment.TickCount64;
        return _switcherOpen ||
               (_lastSwitchEndTick > 0 && now - _lastSwitchEndTick <= withinMs) ||
               (_lastSwitcherForegroundTick > 0 && now - _lastSwitcherForegroundTick <= withinMs);
    }

    /// <summary>
    /// Whether the window that held the foreground before the current one is still there - i.e.
    /// the foreground moved because the user moved, not because the OS had to pick somebody after
    /// the old window was closed or minimized.
    ///
    /// Null means "unknown", and the caller must fall back to its other evidence: off Windows, or
    /// with no hook, or with nothing recorded yet - and, importantly, whenever the hook has not
    /// caught up with the real foreground. The callback is delivered through the same message
    /// queue as the activation it describes, so at Activated time the history may still be one
    /// window behind; comparing it against GetForegroundWindow() is what makes reading it safe.
    ///
    /// A previous window that is alive but not visible is unknown too. Neither thing a user does
    /// looks like that - closing destroys the window, and minimizing leaves it visible and iconic
    /// - so it is some helper or island window that held the foreground briefly, and the switcher
    /// classes above are only the ones known by name. Answering "unknown" there leaves the caller
    /// on its older rules instead of asserting a handover that may not have happened.
    /// </summary>
    public static bool? PreviousForegroundWindowStillUsable()
    {
        if (!_foregroundHookInstalled)
        {
            return null;
        }

        var foreground = GetForegroundWindow();
        if (foreground == IntPtr.Zero || foreground != _currentForeground)
        {
            return null; // the history has not caught up - it would describe the wrong window
        }

        if (_previousForeground == IntPtr.Zero)
        {
            return null;
        }

        if (!IsWindow(_previousForeground))
        {
            return false; // closed: the OS had to hand the foreground to somebody
        }

        if (IsIconic(_previousForeground))
        {
            return false; // minimized - IsWindowVisible stays true for those
        }

        return IsWindowVisible(_previousForeground) ? true : null;
    }
}
