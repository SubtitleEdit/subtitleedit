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
    private IUndoRedoClient? _undoRedoClient;
    private volatile bool _isChangeDetectionActive;
    private volatile bool _disposed;
    private TimeSpan _detectionInterval = TimeSpan.FromSeconds(1);

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
            lock (_lock)
            {
                if (_undoList.Count == 0) return false;
                var currentHash = _undoRedoClient?.GetFastHash() ?? 0;
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
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            _undoRedoClient = client;
            _detectionInterval = interval ?? TimeSpan.FromSeconds(1);

            _changeDetectionTimer?.Dispose();
            _changeDetectionTimer = new Timer(
                CheckForChanges, null,
                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    public void StartChangeDetection()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_changeDetectionTimer is null)
            throw new InvalidOperationException(
                "Call SetupChangeDetection() before StartChangeDetection().");

        if (!_isChangeDetectionActive)
        {
            _isChangeDetectionActive = true;
            _changeDetectionTimer.Change(_detectionInterval, _detectionInterval);
        }
    }

    public void StopChangeDetection()
    {
        if (_isChangeDetectionActive)
        {
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
            _undoList.RemoveAt(0);
    }

    public UndoRedoItem? Undo()
    {
        lock (_lock)
        {
            if (_undoList.Count == 0) return null;

            var currentHash = _undoRedoClient?.GetFastHash() ?? 0;

            // Top of stack matches live state — save it to redo and go deeper.
            if (_undoList.Last().Hash == currentHash)
            {
                if (_undoList.Count == 1) return null;

                PushIfNew(_redoList, PopLast(_undoList));
            }

            // Always keep at least one item in the undo stack (baseline state).
            var target = _undoList.Last();
            if (_undoList.Count > 1)
                _undoList.RemoveAt(_undoList.Count - 1);

            PushIfNew(_redoList, target);
            return UndoRedoItem.Clone(target);
        }
    }

    public UndoRedoItem? Redo()
    {
        lock (_lock)
        {
            if (_redoList.Count == 0) return null;

            var currentHash = _undoRedoClient?.GetFastHash() ?? 0;

            // Skip an entry that already matches the current live state.
            if (_redoList.Last().Hash == currentHash)
            {
                if (_redoList.Count == 1) return null;
                _redoList.RemoveAt(_redoList.Count - 1);
            }

            var item = PopLast(_redoList);

            // Add to undo without clearing redo (unlike DoCore which clears
            // redo for new user actions, redo should preserve remaining entries).
            _undoList.Add(UndoRedoItem.Clone(item)!);
            if (_undoList.Count > MaxUndoItems)
                _undoList.RemoveAt(0);

            return UndoRedoItem.Clone(item);
        }
    }

    // -------------------------------------------------------------------------
    // Automatic change detection (TimerCallback signature required by interface)
    // -------------------------------------------------------------------------

    public void CheckForChanges(object? state)
    {
        if (_undoRedoClient is null || !_isChangeDetectionActive || _disposed)
            return;

        if (_undoRedoClient.IsTyping())
            return;

        try
        {
            // Read hash before acquiring lock to keep critical section short.
            var currentHash = _undoRedoClient.GetFastHash();

            lock (_lock)
            {
                if (IsAlreadyTracked(currentHash)) return;

                var snapshot = _undoRedoClient.MakeUndoRedoObject("Changes detected");
                if (snapshot is null) return;

                var last = _undoList.LastOrDefault();
                if (last is not null && !HasChanges(last, snapshot)) return;

                DoCore(snapshot);
            }
        }
        catch
        {
            // Swallow — change detection must never crash the application.
        }
    }

    private bool IsAlreadyTracked(long hash) =>
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

            if (!textChanged && !timingChanged) continue;

            changedLines.Add(i + 1);
            if (textChanged) textChanges++;
            if (timingChanged) timingChanges++;
        }

        if (changedLines.Count == 0) return false;

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
        if (textChanges > 0) parts.Add($"{textChanges} text");
        if (timingChanges > 0) parts.Add($"{timingChanges} timing");
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
        // Stop *outside* the lock — StopChangeDetection touches volatile fields
        // and the timer only; no _lock needed, and calling it inside would deadlock.
        StopChangeDetection();

        lock (_lock)
        {
            _undoList.Clear();
            _redoList.Clear();
        }

        StartChangeDetection();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

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
            stack.Add(UndoRedoItem.Clone(item)!);
    }

    private static UndoRedoItem PopLast(List<UndoRedoItem> list)
    {
        var item = list[^1];
        list.RemoveAt(list.Count - 1);
        return item;
    }
}