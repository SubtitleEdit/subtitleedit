using Avalonia.Threading;
using System;
using System.Diagnostics;
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
/// </summary>
public sealed class UiTickPump : IDisposable
{
    private readonly TimeSpan _interval;
    private Action _tick;
    private readonly DispatcherPriority _priority;
    private readonly Action _postedTick;
    private Thread? _thread;
    private volatile bool _running;
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
        _thread = new Thread(Loop) { IsBackground = true, Name = "ui-tick-pump" };
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

    private void Loop()
    {
        var intervalTicks = (long)(_interval.TotalSeconds * Stopwatch.Frequency);
        var next = Stopwatch.GetTimestamp() + intervalTicks;
        while (_running)
        {
            var now = Stopwatch.GetTimestamp();
            var waitMs = (next - now) * 1000.0 / Stopwatch.Frequency;
            if (waitMs > 0)
            {
                Thread.Sleep((int)Math.Ceiling(waitMs));
                continue;
            }

            // Schedule the next tick from now rather than from the missed due time, so a UI
            // thread that was busy is not hit with a burst of catch-up ticks.
            next = Stopwatch.GetTimestamp() + intervalTicks;
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
}
