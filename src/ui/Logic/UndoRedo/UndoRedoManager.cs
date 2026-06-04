using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/// <summary>
/// Thread-safe undo/redo manager with automatic change detection.
/// </summary>
public sealed class UndoRedoManager : IUndoRedoManager
{
    private readonly List<UndoRedoItem> _undoList = new();
    private readonly List<UndoRedoItem> _redoList = new();
    private readonly Lock _lock = new();

    private Timer? _changeDetectionTimer;
    // `volatile` so the lock-free pre-check in CheckForChanges doesn't tear-read
    // the reference on weak memory architectures (ARM). Writers still assign
    // under _lock for ordering vs other field mutations.
    private volatile IUndoRedoClient? _undoRedoClient;
    private volatile bool _isChangeDetectionActive;
    // Int (instead of `volatile bool`) so Dispose() can use Interlocked.Exchange
    // to atomically check-and-set — concurrent Dispose() calls can't both pass
    // the gate and double-dispose the timer. Reads outside the lock use
    // Volatile.Read so the disposal flag is still observed promptly across
    // threads (plain int reads aren't guaranteed visible on weak memory
    // architectures).
    private int _disposed;
    // Narrowed from 1s → 250ms to reduce the window in which two distinct user
    // actions (e.g. text edit + waveform drag) get bundled into a single undo
    // entry — issue #11280. CheckForChanges is gated by IsTyping() and
    // IsAlreadyTracked() so most ticks are nearly free when nothing has changed.
    private TimeSpan _detectionInterval = TimeSpan.FromMilliseconds(250);

    private const int MaxUndoItems = 100;
    private const int MaxPreviewLength = 50;
    private const int MaxLinesToList = 5;

    // -------------------------------------------------------------------------
    // Interface: IReadOnlyList properties
    // -------------------------------------------------------------------------

    public IReadOnlyList<UndoRedoItem> UndoList
    {
        get { lock (_lock) { return [.. _undoList]; } }
    }

    public IReadOnlyList<UndoRedoItem> RedoList
    {
        get { lock (_lock) { return [.. _redoList]; } }
    }

    public bool CanUndo
    {
        get
        {
            // Read the hash outside the lock so client.GetFastHash() (which
            // enumerates the subtitle collection) can't invert lock order with
            // any caller holding a collection-side lock while waiting on _lock.
            var currentHash = _undoRedoClient?.GetFastHash() ?? 0;
            lock (_lock)
            {
                if (_undoList.Count == 0)
                {
                    return false;
                }
                return _undoList.Last().Hash != currentHash || _undoList.Count > 1;
            }
        }
    }

    public bool CanRedo
    {
        get { lock (_lock) { return _redoList.Count > 0; } }
    }

    public int UndoCount
    {
        get { lock (_lock) { return _undoList.Count; } }
    }

    public int RedoCount
    {
        get { lock (_lock) { return _redoList.Count; } }
    }

    // -------------------------------------------------------------------------
    // Change-detection setup
    // -------------------------------------------------------------------------

    public void SetupChangeDetection(IUndoRedoClient client, TimeSpan? interval = null)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        lock (_lock)
        {
            _undoRedoClient = client;
            // Keep the field-initializer default (250ms) when caller passes null
            // — see the comment on _detectionInterval for the rationale.
            _detectionInterval = interval ?? _detectionInterval;

            _changeDetectionTimer?.Dispose();
            _changeDetectionTimer = new Timer(
                CheckForChanges, null,
                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    public void StartChangeDetection()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        lock (_lock)
        {
            if (_changeDetectionTimer is null)
                throw new InvalidOperationException(
                    "Call SetupChangeDetection() before StartChangeDetection().");

            if (_isChangeDetectionActive)
            {
                return;
            }
            _isChangeDetectionActive = true;
            _changeDetectionTimer.Change(_detectionInterval, _detectionInterval);
        }
    }

    public void StopChangeDetection()
    {
        lock (_lock)
        {
            if (!_isChangeDetectionActive)
            {
                return;
            }
            _isChangeDetectionActive = false;
            _changeDetectionTimer?.Change(
                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    // -------------------------------------------------------------------------
    // Core Do / Undo / Redo
    // -------------------------------------------------------------------------

    public void Do(UndoRedoItem action)
    {
        ArgumentNullException.ThrowIfNull(action);
        lock (_lock) { DoCore(action); }
    }

    private void DoCore(UndoRedoItem action)
    {
        _undoList.Add(action);
        _redoList.Clear();

        if (_undoList.Count > MaxUndoItems)
        {
            _undoList.RemoveAt(0);
        }
    }

    public UndoRedoItem? Undo()
    {
        // Capture the client + read the live hash outside the lock — see the
        // CanUndo / CheckForChanges comments for why.
        var client = _undoRedoClient;
        var currentHash = client?.GetFastHash() ?? 0;

        // First lock: decide which path to take. If the top of the undo stack
        // matches the live state, the resolution is purely internal (pop/push)
        // and we can return immediately without calling back into the client.
        lock (_lock)
        {
            if (_undoList.Count == 0)
            {
                return null;
            }

            if (_undoList.Last().Hash == currentHash)
            {
                if (_undoList.Count == 1)
                {
                    return null;
                }
                PushIfNew(_redoList, PopLast(_undoList));
                return UndoRedoItem.Clone(_undoList.Last());
            }
        }

        // Else: live state has unrecorded changes. Snapshot it OUTSIDE the lock
        // because MakeUndoRedoObject enumerates the subtitle collection — see
        // issue #11280 for why we need the snapshot in the first place. The
        // unrecorded edit is a divergence from any prior redo timeline (same
        // semantic as DoCore clearing redo on a new explicit action).
        var live = client?.MakeUndoRedoObject("Unrecorded changes");

        lock (_lock)
        {
            // Re-check: another thread may have cleared the undo list (Reset)
            // between the two locks. Bail safely instead of returning a stale ref.
            if (_undoList.Count == 0)
            {
                return null;
            }

            if (live is not null)
            {
                _redoList.Clear();
                _redoList.Add(live);
            }

            return UndoRedoItem.Clone(_undoList.Last());
        }
    }

    public UndoRedoItem? Redo()
    {
        lock (_lock)
        {
            if (_redoList.Count == 0)
            {
                return null;
            }

            var item = PopLast(_redoList);

            // Add to undo without clearing redo (unlike DoCore which clears
            // redo for new user actions, redo should preserve remaining entries).
            _undoList.Add(UndoRedoItem.Clone(item)!);
            if (_undoList.Count > MaxUndoItems)
            {
                _undoList.RemoveAt(0);
            }

            return UndoRedoItem.Clone(item);
        }
    }

    // -------------------------------------------------------------------------
    // Automatic change detection (TimerCallback signature required by interface)
    // -------------------------------------------------------------------------

    public void CheckForChanges(object? state)
    {
        // Capture the client once so subsequent calls see a consistent reference
        // even if SetupChangeDetection races with us. `_undoRedoClient` is
        // volatile so this read isn't torn.
        var client = _undoRedoClient;
        if (client is null || !_isChangeDetectionActive || Volatile.Read(ref _disposed) != 0)
        {
            return;
        }

        if (client.IsTyping())
        {
            return;
        }

        try
        {
            // Both GetFastHash and MakeUndoRedoObject enumerate the subtitle
            // collection; calling them under _lock would invert lock order with
            // anything on the UI thread that holds a collection lock and is
            // waiting on _lock. Do all client calls outside the lock.
            var currentHash = client.GetFastHash();

            bool alreadyTracked;
            lock (_lock)
            {
                alreadyTracked = IsAlreadyTracked(currentHash);
            }
            if (alreadyTracked)
            {
                return;
            }

            var snapshot = client.MakeUndoRedoObject("Changes detected");
            if (snapshot is null)
            {
                return;
            }

            lock (_lock)
            {
                // Re-validate after the lock release/reacquire window — another
                // tick or an explicit Do() may have captured this hash already.
                if (IsAlreadyTracked(snapshot.Hash))
                {
                    return;
                }

                var last = _undoList.LastOrDefault();
                if (last is not null && !HasChanges(last, snapshot))
                {
                    return;
                }

                DoCore(snapshot);
            }
        }
        catch (Exception ex)
        {
            // Change detection must never crash the application, but swallowing
            // silently used to hide real bugs in MakeUndoRedoObject and friends.
            // Log so they're at least diagnosable.
            Se.LogError(ex, "UndoRedoManager.CheckForChanges");
        }
    }

    // Hash on UndoRedoItem and the value returned by GetFastHash are both int;
    // accepting `long` here silently widened both sides on every call.
    private bool IsAlreadyTracked(int hash) =>
        _undoList.LastOrDefault()?.Hash == hash ||
        _redoList.LastOrDefault()?.Hash == hash;

    // -------------------------------------------------------------------------
    // Change description helpers
    // -------------------------------------------------------------------------

    private static bool HasChanges(UndoRedoItem prev, UndoRedoItem next)
    {
        var oldCount = prev.Subtitles.Length;
        var newCount = next.Subtitles.Length;

        if (oldCount != newCount)
        {
            next.Description = oldCount > newCount
                ? string.Format(Se.Language.General.LinesDeletedX, oldCount - newCount)
                : string.Format(Se.Language.General.LinesAddedX, newCount - oldCount);
            return true;
        }

        return DetectContentChanges(prev, next);
    }

    private static bool DetectContentChanges(UndoRedoItem prev, UndoRedoItem next)
    {
        var changedLines = new List<int>();
        var textChanges = 0;
        var timingChanges = 0;

        for (var i = 0; i < prev.Subtitles.Length; i++)
        {
            var o = prev.Subtitles[i];
            var n = next.Subtitles[i];

            var textChanged = o.Text != n.Text;
            var timingChanged =
                Math.Abs(o.StartTime.TotalMilliseconds - n.StartTime.TotalMilliseconds) > 0.5 ||
                Math.Abs(o.EndTime.TotalMilliseconds - n.EndTime.TotalMilliseconds) > 0.5;
            var bookmarkChanged = o.Bookmark != n.Bookmark;

            if (!textChanged && !timingChanged && !bookmarkChanged)
            {
                continue;
            }

            changedLines.Add(i + 1);
            if (textChanged)
            {
                textChanges++;
            }
            if (timingChanged)
            {
                timingChanges++;
            }
        }

        if (changedLines.Count == 0)
        {
            return false;
        }

        next.Description = changedLines.Count switch
        {
            1 => SingleLineDescription(changedLines[0], prev, next),
            <= MaxLinesToList => MultiLineDescription(changedLines, textChanges, timingChanges),
            _ => SummaryDescription(changedLines.Count, textChanges, timingChanges)
        };

        return true;
    }

    private static string SingleLineDescription(int line, UndoRedoItem prev, UndoRedoItem next)
    {
        var o = prev.Subtitles[line - 1];
        var n = next.Subtitles[line - 1];

        var textChanged = o.Text != n.Text;
        var timingChanged =
            Math.Abs(o.StartTime.TotalMilliseconds - n.StartTime.TotalMilliseconds) > 0.5 ||
            Math.Abs(o.EndTime.TotalMilliseconds - n.EndTime.TotalMilliseconds) > 0.5;

        return (textChanged, timingChanged) switch
        {
            (true, true) => string.Format(Se.Language.Main.LineXTextAndTimingChanged, line),
            (true, false) => string.Format(Se.Language.Main.LineXTextChangedFromYToZ,
                                 line, Truncate(o.Text), Truncate(n.Text)),
            (false, true) => string.Format(Se.Language.Main.LineXTimingChanged, line),
            _ => $"Line {line}: modified"
        };
    }

    private static string MultiLineDescription(
        List<int> lines, int textChanges, int timingChanges) =>
        $"Lines {string.Join(", ", lines)}: {FormatChangeTypes(textChanges, timingChanges)} changes";

    private static string SummaryDescription(
        int lineCount, int textChanges, int timingChanges) =>
        $"{lineCount} lines modified: {FormatChangeTypes(textChanges, timingChanges)} changes";

    private static string FormatChangeTypes(int textChanges, int timingChanges)
    {
        var parts = new List<string>(2);
        if (textChanges > 0)
        {
            parts.Add($"{textChanges} text");
        }
        if (timingChanges > 0)
        {
            parts.Add($"{timingChanges} timing");
        }
        return string.Join(" and ", parts);
    }

    private static string Truncate(string text) =>
        text.Length > MaxPreviewLength
            ? string.Concat(text.AsSpan(0, MaxPreviewLength), "...")
            : text;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    public void Reset()
    {
        bool wasActive;
        lock (_lock)
        {
            wasActive = _isChangeDetectionActive;
            if (wasActive)
            {
                _isChangeDetectionActive = false;
                _changeDetectionTimer?.Change(
                    Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
            _undoList.Clear();
            _redoList.Clear();
        }

        if (wasActive)
        {
            StartChangeDetection();
        }
    }

    public void Dispose()
    {
        // Interlocked.Exchange is atomic — concurrent Dispose() calls can't
        // both pass the check-and-set and double-dispose the timer (Timer.Dispose
        // is idempotent today, but the previous `if (_disposed) return; _disposed = true;`
        // pattern was a TOCTOU race regardless).
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        StopChangeDetection();
        _changeDetectionTimer?.Dispose();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Adds a clone of <paramref name="item"/> only if the stack top differs by hash.</summary>
    private static void PushIfNew(List<UndoRedoItem> stack, UndoRedoItem item)
    {
        if (stack.LastOrDefault()?.Hash != item.Hash)
        {
            stack.Add(UndoRedoItem.Clone(item)!);
        }
    }

    private static UndoRedoItem PopLast(List<UndoRedoItem> list)
    {
        var item = list[^1];
        list.RemoveAt(list.Count - 1);
        return item;
    }
}