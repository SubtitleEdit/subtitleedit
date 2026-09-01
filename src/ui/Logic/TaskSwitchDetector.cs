using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Tracks the Windows task switcher (Alt+Tab) via a WinEvent hook so a window activation that
/// the user picked in the switcher can be told apart from one the OS handed over by itself.
///
/// The undocked foreground-steal watcher (#14168) classifies tool-window activations by the
/// physical pointer buttons - but Alt+Tab activates a window with no button down, so switching
/// to an undocked tool window was "corrected" back to the main window: the tool window
/// flickered into the foreground and lost it 200 ms later (#14354). The switcher raises
/// EVENT_SYSTEM_SWITCHSTART when it opens and EVENT_SYSTEM_SWITCHEND when it commits the
/// switch, right before the chosen window is activated - so "the switcher is open, or closed a
/// moment ago" marks the activation as user-aimed.
///
/// Windows only; on other platforms nothing is hooked and <see cref="TaskSwitchJustCommitted"/>
/// stays false. The hook must be installed from a thread that pumps messages (the UI thread) and
/// lives for the process lifetime.
/// </summary>
public static class TaskSwitchDetector
{
    private const uint EventSystemSwitchStart = 0x0014;
    private const uint EventSystemSwitchEnd = 0x0015;
    private const uint WinEventOutOfContext = 0x0000;

    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
        IntPtr hmodWinEventProc, WinEventDelegate pfnWinEventProc, uint idProcess, uint idThread,
        uint dwFlags);

    // Keeps the delegate alive for the lifetime of the hook - the GC must not collect it while
    // user32 still holds the callback pointer.
    private static WinEventDelegate? _callback;
    private static bool _initialized;
    private static bool _switcherOpen;
    private static long _lastSwitchEndTick;

    /// <summary>
    /// Installs the hook once. Call from the UI thread. No-op off Windows or when the hook
    /// could not be installed (detection then just stays inert - fail-safe).
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
            SetWinEventHook(EventSystemSwitchStart, EventSystemSwitchEnd, IntPtr.Zero, _callback,
                0, 0, WinEventOutOfContext);
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
        }
        else if (eventType == EventSystemSwitchEnd)
        {
            _switcherOpen = false;
            _lastSwitchEndTick = Environment.TickCount64;
        }
    }

    /// <summary>
    /// True while the task switcher is open, or within <paramref name="withinMs"/> of it
    /// committing a switch. The activation the switcher causes arrives on the message queue
    /// right after SWITCHEND, so a small window is plenty - it only needs to be generous enough
    /// to survive dispatcher latency, not user-scale time.
    /// </summary>
    public static bool TaskSwitchJustCommitted(int withinMs = 1000)
    {
        return _switcherOpen ||
               (_lastSwitchEndTick > 0 && Environment.TickCount64 - _lastSwitchEndTick <= withinMs);
    }
}
