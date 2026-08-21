using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Coalesces high-frequency per-line UI feedback from a background work loop (OCR,
/// auto-translate) into batched dispatcher jobs (#13885). Fast engines finish a line in
/// milliseconds, and per-line selection/scroll/text dispatches outrank or block input
/// processing, so Pause/Cancel clicks and even the window's minimize are starved until the
/// run completes.
///
/// The flush is leading-edge throttled: an update arriving after a quiet period is applied
/// immediately (so slower engines feel live), and only while updates keep streaming are they
/// batched to at most one flush per flushIntervalMs. Ordered updates (line text, fix results,
/// result rows) are all applied in order; selection and progress are latest-wins - a newly
/// processed line replaces any still-pending selection. Flushes run at Background dispatcher
/// priority, below input, so a flood of updates can never starve input.
///
/// Enqueue methods may be called from any thread; the apply callbacks and queued update
/// actions always run on the UI thread.
/// </summary>
public sealed class CoalescedUiUpdateQueue
{
    private readonly int _flushIntervalMs;
    private readonly Action<int> _applySelect;
    private readonly Action<double, string> _applyProgress;

    private readonly Lock _lock = new Lock();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private List<Action> _pendingUpdates = new List<Action>();
    private int _pendingSelectIndex = -1;
    private double _pendingProgressValue;
    private string? _pendingProgressText;
    private bool _flushScheduled;
    private long _lastFlushElapsedMs;

    public CoalescedUiUpdateQueue(Action<int> applySelect, Action<double, string> applyProgress, int flushIntervalMs = 100)
    {
        _applySelect = applySelect;
        _applyProgress = applyProgress;
        _flushIntervalMs = flushIntervalMs;
        _lastFlushElapsedMs = -flushIntervalMs; // the very first update flushes immediately
    }

    public void EnqueueUpdate(Action update)
    {
        lock (_lock)
        {
            _pendingUpdates.Add(update);
        }

        ScheduleFlush();
    }

    public void EnqueueSelect(int index)
    {
        lock (_lock)
        {
            _pendingSelectIndex = index;
        }

        ScheduleFlush();
    }

    public void EnqueueProgress(double value, string text)
    {
        lock (_lock)
        {
            _pendingProgressValue = value;
            _pendingProgressText = text;
        }

        ScheduleFlush();
    }

    private void ScheduleFlush()
    {
        long delayMs;
        lock (_lock)
        {
            if (_flushScheduled)
            {
                return;
            }

            _flushScheduled = true;
            var sinceLastFlushMs = _clock.ElapsedMilliseconds - _lastFlushElapsedMs;
            delayMs = _flushIntervalMs - sinceLastFlushMs;
        }

        // DispatcherTimer may only be touched from the UI thread, and the enqueue often happens
        // on a worker thread. Background priority keeps both the scheduling hop and the flush
        // itself below input processing.
        if (delayMs <= 0)
        {
            // Quiet period since the last flush - apply right away so the grid feels live.
            Dispatcher.UIThread.Post(Flush, DispatcherPriority.Background);
        }
        else
        {
            var delay = TimeSpan.FromMilliseconds(delayMs);
            Dispatcher.UIThread.Post(
                () => DispatcherTimer.RunOnce(Flush, delay, DispatcherPriority.Background),
                DispatcherPriority.Background);
        }
    }

    /// <summary>
    /// Applies all pending updates. Normally fired by the flush scheduling on the UI thread;
    /// call it directly (from the UI thread) before opening a modal or building the final
    /// result, so nothing reads a half-flushed queue.
    /// </summary>
    public void Flush()
    {
        List<Action> updates;
        int selectIndex;
        double progressValue;
        string? progressText;
        lock (_lock)
        {
            _flushScheduled = false;
            _lastFlushElapsedMs = _clock.ElapsedMilliseconds;
            updates = _pendingUpdates;
            _pendingUpdates = new List<Action>();
            selectIndex = _pendingSelectIndex;
            _pendingSelectIndex = -1;
            progressValue = _pendingProgressValue;
            progressText = _pendingProgressText;
            _pendingProgressText = null;
        }

        foreach (var update in updates)
        {
            update();
        }

        if (progressText != null)
        {
            _applyProgress(progressValue, progressText);
        }

        if (selectIndex >= 0)
        {
            _applySelect(selectIndex);
        }
    }
}
