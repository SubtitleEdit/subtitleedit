using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// A fixed-interval UI tick driven by a dedicated thread that posts each tick to the Avalonia
/// dispatcher, instead of a <see cref="DispatcherTimer"/>.
/// <para>
/// A DispatcherTimer rides on the platform's own timer message (a Win32 SetTimer/WM_TIMER on
/// Windows). With mpv embedded through its own native window (the Windows default player,
/// "mpv-wid"), that timer message stopped waking the UI thread's message loop for 100-1000 ms
/// around play/pause - the thread sat idle in GetMessage with the tick overdue, while a posted
/// message woke it at once (measured in the #14523 follow-up: the waveform cursor jumped and
/// the time display froze at every play, only with the native window). A posted tick does not
/// depend on the timer message, and executing it also promotes the dispatcher's other due
/// timers (the 50 ms position/display timers), so those stop freezing too.
/// </para>
/// <para>
/// The wait itself must not be a plain <see cref="Thread.Sleep(int)"/> on Windows: a sleep is
/// only as fine as the process's timer resolution, which defaults to 15.6 ms, so "sleep 16 ms"
/// wakes on the second timer interrupt at a steady 31 ms and the 16 ms cursor tick lands on
/// every other display frame. Nothing in Subtitle Edit or Avalonia raises the resolution, and
/// libmpv raises it only for the duration of its own short sleeps (NtSetTimerResolution in
/// mpv's timer-win32.c), so the tick rate flickered between 60 and 32 Hz with what mpv's
/// threads happened to be doing, and stayed at 32 Hz with the FFmpeg player (#14909: waveform
/// "shaking" that varied with nothing changed and was worse with FFmpeg). The pump therefore
/// waits on a high-resolution waitable timer (Windows 10 1803+, sub-millisecond without touching
/// the system timer resolution), and on older Windows raises the resolution to 1 ms for as long
/// as the loop runs.
/// </para>
/// </summary>
public sealed partial class UiTickPump : IDisposable
{
    private readonly TimeSpan _interval;
    private Action _tick;
    private readonly DispatcherPriority _priority;
    private readonly Action _postedTick;
    private Thread? _thread;
    private volatile bool _running;
    private int _generation; // bumped per Start, so a thread from before a quick Stop/Start exits
    private int _tickPending; // 1 while a posted tick has not run yet: never queue a backlog

    public UiTickPump(TimeSpan interval, Action tick, DispatcherPriority priority)
    {
        _interval = interval < TimeSpan.FromMilliseconds(1) ? TimeSpan.FromMilliseconds(1) : interval;
        _tick = tick;
        _priority = priority;
        _postedTick = RunTick;
    }

    /// <summary>
    /// DispatcherTimer-shaped constructor: subscribe <see cref="Tick"/>, then <see cref="Start"/>.
    /// Lets the position timers of the waveform dialogs swap in without restructuring.
    /// </summary>
    public UiTickPump(TimeSpan interval)
        : this(interval, () => { }, DispatcherPriority.Normal)
    {
        _tick = () => Tick?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raised on the UI thread once per interval while running.</summary>
    public event EventHandler? Tick;

    public bool IsRunning => _running;

    /// <summary>DispatcherTimer-style alias for <see cref="IsRunning"/>.</summary>
    public bool IsEnabled => _running;

    public void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        var generation = Interlocked.Increment(ref _generation);
        _thread = new Thread(() => Loop(generation)) { IsBackground = true, Name = "ui-tick-pump" };
        _thread.Start();
    }

    public void Stop()
    {
        _running = false;
    }

    public void Dispose()
    {
        Stop();
    }

    private void Loop(int generation)
    {
        using var waiter = TickWaiter.Create();
        var intervalTicks = (long)(_interval.TotalSeconds * Stopwatch.Frequency);
        var next = Stopwatch.GetTimestamp() + intervalTicks;
        while (_running && generation == Volatile.Read(ref _generation))
        {
            var now = Stopwatch.GetTimestamp();
            var waitMs = (next - now) * 1000.0 / Stopwatch.Frequency;
            if (waitMs > 0)
            {
                waiter.Wait(waitMs);
                continue;
            }

            // Keep the cadence anchored to the due time, so the wake-up latency and the cost of
            // the post do not accumulate into the period (scheduling from "now" made a 16 ms
            // pump run at ~58 Hz). Only when the loop is a whole interval late - the UI thread
            // was busy, or the wait overslept - re-anchor to now, so the pump does not fire a
            // burst of catch-up ticks.
            next += intervalTicks;
            if (next <= now)
            {
                next = now + intervalTicks;
            }

            if (Interlocked.CompareExchange(ref _tickPending, 1, 0) == 0)
            {
                Dispatcher.UIThread.Post(_postedTick, _priority);
            }
        }
    }

    private void RunTick()
    {
        Interlocked.Exchange(ref _tickPending, 0);
        if (_running)
        {
            _tick();
        }
    }

    /// <summary>
    /// The platform wait behind the loop. Windows gets a high-resolution waitable timer, or a
    /// 1 ms timer resolution for the loop's lifetime when that timer is unavailable; other
    /// platforms sleep, which is already millisecond-accurate there.
    /// </summary>
    internal abstract partial class TickWaiter : IDisposable
    {
        public static TickWaiter Create()
        {
            if (OperatingSystem.IsWindows())
            {
                var timer = WindowsHighResolutionTimerWaiter.TryCreate();
                if (timer != null)
                {
                    return timer;
                }

                return new WindowsTimerResolutionWaiter();
            }

            return new SleepWaiter();
        }

        /// <summary>Which wait is in use - for tests and diagnostics.</summary>
        public abstract string Kind { get; }

        public abstract void Wait(double milliseconds);

        public virtual void Dispose()
        {
        }

        private sealed class SleepWaiter : TickWaiter
        {
            public override string Kind => "sleep";

            public override void Wait(double milliseconds)
            {
                Thread.Sleep((int)Math.Ceiling(milliseconds));
            }
        }

        private sealed partial class WindowsHighResolutionTimerWaiter : TickWaiter
        {
            private const uint CreateWaitableTimerHighResolution = 0x00000002;
            private const uint TimerAllAccess = 0x001F0003;
            private const uint WaitObject0 = 0;

            private readonly IntPtr _handle;

            private WindowsHighResolutionTimerWaiter(IntPtr handle)
            {
                _handle = handle;
            }

            public override string Kind => "high-resolution timer";

            public static WindowsHighResolutionTimerWaiter? TryCreate()
            {
                // CREATE_WAITABLE_TIMER_HIGH_RESOLUTION needs Windows 10 1803; older builds fail
                // the call and get the timer-resolution fallback instead.
                var handle = CreateWaitableTimerExW(IntPtr.Zero, IntPtr.Zero, CreateWaitableTimerHighResolution, TimerAllAccess);
                return handle == IntPtr.Zero ? null : new WindowsHighResolutionTimerWaiter(handle);
            }

            public override void Wait(double milliseconds)
            {
                // Due time in 100 ns units; negative = relative to now.
                var dueTime = -(long)Math.Ceiling(milliseconds * 10000);
                if (!SetWaitableTimer(_handle, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false))
                {
                    Thread.Sleep((int)Math.Ceiling(milliseconds));
                    return;
                }

                // The timeout only guards against a timer that never fires; the timer itself is
                // what paces the wait.
                var timeoutMs = (uint)Math.Ceiling(milliseconds) + 100;
                if (WaitForSingleObject(_handle, timeoutMs) != WaitObject0)
                {
                    Thread.Sleep((int)Math.Ceiling(milliseconds));
                }
            }

            public override void Dispose()
            {
                CloseHandle(_handle);
            }

            [LibraryImport("kernel32.dll", SetLastError = true)]
            private static partial IntPtr CreateWaitableTimerExW(IntPtr timerAttributes, IntPtr timerName, uint flags, uint desiredAccess);

            [LibraryImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static partial bool SetWaitableTimer(IntPtr timer, ref long dueTime, int period, IntPtr completionRoutine, IntPtr argToCompletionRoutine, [MarshalAs(UnmanagedType.Bool)] bool resume);

            [LibraryImport("kernel32.dll", SetLastError = true)]
            private static partial uint WaitForSingleObject(IntPtr handle, uint milliseconds);

            [LibraryImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static partial bool CloseHandle(IntPtr handle);
        }

        private sealed partial class WindowsTimerResolutionWaiter : TickWaiter
        {
            private const uint PeriodMs = 1;
            private readonly bool _raised;

            public WindowsTimerResolutionWaiter()
            {
                _raised = timeBeginPeriod(PeriodMs) == 0;
            }

            public override string Kind => _raised ? "sleep at 1 ms timer resolution" : "sleep";

            public override void Wait(double milliseconds)
            {
                Thread.Sleep((int)Math.Ceiling(milliseconds));
            }

            public override void Dispose()
            {
                if (_raised)
                {
                    timeEndPeriod(PeriodMs);
                }
            }

            [LibraryImport("winmm.dll")]
            private static partial uint timeBeginPeriod(uint period);

            [LibraryImport("winmm.dll")]
            private static partial uint timeEndPeriod(uint period);
        }
    }
}
