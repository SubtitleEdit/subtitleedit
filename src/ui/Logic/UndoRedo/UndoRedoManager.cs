using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class UndoRedoManager : IUndoRedoManager
{
    private readonly List<UndoRedoItem> _undoList = new();
    private readonly List<UndoRedoItem> _redoList = new();
    private readonly object _lock = new();
    private Timer? _changeDetectionTimer;
    private IUndoRedoClient? _undoRedoClient;
    private bool _isChangeDetectionActive;
    private bool _disposed;
    private TimeSpan _detectionInterval = TimeSpan.FromSeconds(1);

    // Configuration constants
    private const int MaxUndoItems = 100; // Prevent memory issues
    private const int MaxPreviewLength = 50;
    private const int MaxLinesToList = 5;

    public List<UndoRedoItem> UndoList
    {
        get
        {
            lock (_lock)
            {
                return [.. _undoList];
            }
        }
    }

    public List<UndoRedoItem> RedoList
    {
        get
        {
            lock (_lock)
            {
                return [.. _redoList];
            }
        }
    }

    public bool CanUndo
    {
        get
        {
            lock (_lock)
            {
                if (_undoList.Count == 0)
                {
                    return false;
                }

                var currentHash = _undoRedoClient?.GetFastHash() ?? 0;
                if (_undoList.Count == 1 && _undoList[0].Hash == currentHash)
                {
                    return false;
                }

                return true;
            }
        }
    }

    public bool CanRedo
    {
        get
        {
            lock (_lock)
            {
                return _redoList.Count > 0;
            }
        }
    }

    public int UndoCount
    {
        get
        {
            lock (_lock)
            {
                return _undoList.Count;
            }
        }
    }

    public int RedoCount
    {
        get
        {
            lock (_lock)
            {
                return _redoList.Count;
            }
        }
    }

    public void SetupChangeDetection(IUndoRedoClient undoRedoClient, TimeSpan? interval = null)
    {
        lock (_lock)
        {
            _undoRedoClient = undoRedoClient;
            _detectionInterval = interval ?? TimeSpan.FromSeconds(1);
            _changeDetectionTimer = new Timer(CheckForChanges, null, Timeout.InfiniteTimeSpan, _detectionInterval);
        }
    }

    public void StartChangeDetection()
    {
        if (_changeDetectionTimer == null)
        {
            throw new InvalidOperationException("Change detection requires setup via SetupChangeDetection() first");
        }

        lock (_lock)
        {
            if (!_isChangeDetectionActive)
            {
                _isChangeDetectionActive = true;
                _changeDetectionTimer.Change(_detectionInterval, _detectionInterval);
            }
        }
    }

    public void StopChangeDetection()
    {
        lock (_lock)
        {
            if (_isChangeDetectionActive)
            {
                _isChangeDetectionActive = false;
                _changeDetectionTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
        }
    }

    public void Do(UndoRedoItem action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        lock (_lock)
        {
            _undoList.Add(action);
            _redoList.Clear();

            // Limit undo history to prevent memory issues
            if (_undoList.Count > MaxUndoItems)
            {
                _undoList.RemoveAt(0);
            }
        }
    }

    public UndoRedoItem? Undo()
    {
        lock (_lock)
        {
            if (_undoList.Count == 0)
            {
                return null;
            }

            var currentHash = _undoRedoClient?.GetFastHash() ?? 0;
            var undoItem = GetUndoItem(currentHash);

            // Always add the returned undo item to redo list (unless it's already there)
            if (undoItem != null && _undoRedoClient != null && !_redoList.Contains(undoItem))
            {
                _redoList.Add(UndoRedoItem.Clone(undoItem)!);
            }

            return undoItem;
        }
    }

    private UndoRedoItem? GetUndoItem(long currentHash)
    {
        var undoItem = _undoList.Last();
        if (_undoList.Count > 1) // always keep last undo item (initial starting state)
        {
            _undoList.RemoveAt(_undoList.Count - 1);
        }

        // If current state matches the undo item, go to the next one
        if (undoItem.Hash == currentHash && _undoList.Count > 0)
        {
            // Add the current item to redo list before getting the next one
            if (_undoRedoClient != null && !_redoList.Contains(undoItem))
            {
                _redoList.Add(UndoRedoItem.Clone(undoItem)!);
            }

            undoItem = _undoList.Last();
            if (_undoList.Count > 1)
            {
                _undoList.RemoveAt(_undoList.Count - 1);
            }
        }

        return UndoRedoItem.Clone(undoItem);
    }

    public UndoRedoItem? Redo()
    {
        lock (_lock)
        {
            if (_redoList.Count == 0)
            {
                return null;
            }

            var currentHash = _undoRedoClient?.GetFastHash() ?? 0;
            var item = GetRedoItem(currentHash);

            if (item != null)
            {
                _undoList.Add(item);
            }

            return UndoRedoItem.Clone(item);
        }
    }

    private UndoRedoItem? GetRedoItem(long currentHash)
    {
        var item = _redoList.Last();
        _redoList.RemoveAt(_redoList.Count - 1);

        // If current state matches the redo item, go to the next one
        if (item.Hash == currentHash && _redoList.Count > 0)
        {
            item = _redoList.Last();
            _redoList.RemoveAt(_redoList.Count - 1);
        }

        return item;
    }

    public void CheckForChanges(object? state)
    {
        if (_undoRedoClient == null || !_isChangeDetectionActive)
        {
            return;
        }

        if (_undoRedoClient.IsTyping())
        {
            return; // Skip change detection while typing
        }

        try
        {
            lock (_lock)
            {
                var currentHash = _undoRedoClient.GetFastHash();

                // Skip if no changes detected
                if (IsCurrentStateAlreadyTracked(currentHash))
                {
                    return;
                }

                var undoRedoItem = _undoRedoClient.MakeUndoRedoObject("Changes detected");
                if (undoRedoItem == null)
                {
                    return;
                }

                var lastUndoItem = _undoList.LastOrDefault();
                if (lastUndoItem != null)
                {
                    var hasChanges = HasChangesAndSetDescription(lastUndoItem, undoRedoItem);
                    if (!hasChanges)
                    {
                        return;
                    }
                }

                Do(undoRedoItem);
            }
        }
        catch
        {
            // Ignore exceptions in change detection
        }
    }

    private bool IsCurrentStateAlreadyTracked(long currentHash)
    {
        var lastUndoItem = _undoList.LastOrDefault();
        if (lastUndoItem?.Hash == currentHash)
        {
            return true;
        }

        var lastRedoItem = _redoList.LastOrDefault();
        if (lastRedoItem?.Hash == currentHash)
        {
            return true;
        }

        return false;
    }

    private bool HasChangesAndSetDescription(UndoRedoItem lastUndoItem, UndoRedoItem undoRedoItem)
    {
        var oldCount = lastUndoItem.Subtitles.Length;
        var newCount = undoRedoItem.Subtitles.Length;

        // Handle line count changes
        if (oldCount != newCount)
        {
            if (oldCount > newCount)
            {
                undoRedoItem.Description = string.Format(Se.Language.General.LinesDeletedX, oldCount - newCount);
            }
            else
            {
                undoRedoItem.Description = string.Format(Se.Language.General.LinesAddedX, newCount - oldCount);
            }
            return true;
        }

        // Handle content changes for same line count
        return DetectAndDescribeContentChanges(lastUndoItem, undoRedoItem);
    }

    private bool DetectAndDescribeContentChanges(UndoRedoItem lastUndoItem, UndoRedoItem undoRedoItem)
    {
        var changedLines = new List<int>();
        var textChanges = 0;
        var timingChanges = 0;

        // Analyze changes
        for (var i = 0; i < lastUndoItem.Subtitles.Length; i++)
        {
            var oldSubtitle = lastUndoItem.Subtitles[i];
            var newSubtitle = undoRedoItem.Subtitles[i];

            var hasTextChange = oldSubtitle.Text != newSubtitle.Text;
            var hasTimingChange = oldSubtitle.StartTime.TotalMilliseconds != newSubtitle.StartTime.TotalMilliseconds ||
                                oldSubtitle.EndTime.TotalMilliseconds != newSubtitle.EndTime.TotalMilliseconds;

            if (hasTextChange || hasTimingChange)
            {
                changedLines.Add(i + 1); // 1-based line numbering

                if (hasTextChange)
                {
                    textChanges++;
                }

                if (hasTimingChange)
                {
                    timingChanges++;
                }
            }
        }

        if (changedLines.Count == 0)
        {
            return false;
        }

        // Generate description based on change scope
        undoRedoItem.Description = GenerateChangeDescription(changedLines, textChanges, timingChanges, lastUndoItem, undoRedoItem);
        return true;
    }

    private string GenerateChangeDescription(List<int> changedLines, int textChanges, int timingChanges,
        UndoRedoItem lastUndoItem, UndoRedoItem undoRedoItem)
    {
        if (changedLines.Count == 1)
        {
            return GenerateSingleLineDescription(changedLines[0], lastUndoItem, undoRedoItem);
        }

        if (changedLines.Count <= MaxLinesToList)
        {
            return GenerateMultipleLineDescription(changedLines, textChanges, timingChanges);
        }

        return GenerateSummaryDescription(changedLines.Count, textChanges, timingChanges);
    }

    private string GenerateSingleLineDescription(int lineNum, UndoRedoItem lastUndoItem, UndoRedoItem undoRedoItem)
    {
        var oldSubtitle = lastUndoItem.Subtitles[lineNum - 1];
        var newSubtitle = undoRedoItem.Subtitles[lineNum - 1];

        var hasTextChange = oldSubtitle.Text != newSubtitle.Text;
        var hasTimingChange = Math.Abs(oldSubtitle.StartTime.TotalMilliseconds - newSubtitle.StartTime.TotalMilliseconds) > 0.001 ||
                              Math.Abs(oldSubtitle.EndTime.TotalMilliseconds - newSubtitle.EndTime.TotalMilliseconds) > 0.001;

        if (hasTextChange && hasTimingChange)
        {
            return string.Format(Se.Language.Main.LineXTextAndTimingChanged, lineNum);
        }

        if (hasTextChange)
        {
            var oldText = TruncateText(oldSubtitle.Text);
            var newText = TruncateText(newSubtitle.Text);
            return string.Format(Se.Language.Main.LineXTextChangedFromYToZ, lineNum, oldText, newText);
        }

        if (hasTimingChange)
        {
            return string.Format(Se.Language.Main.LineXTimingChanged, lineNum);
        }

        return $"Line {lineNum}: modified"; // fallback
    }

    private string GenerateMultipleLineDescription(List<int> changedLines, int textChanges, int timingChanges)
    {
        var linesList = string.Join(", ", changedLines);
        var changeTypes = new List<string>();

        if (textChanges > 0)
        {
            changeTypes.Add($"{textChanges} text");
        }

        if (timingChanges > 0)
        {
            changeTypes.Add($"{timingChanges} timing");
        }

        return $"Lines {linesList}: {string.Join(" and ", changeTypes)} changes";
    }

    private string GenerateSummaryDescription(int lineCount, int textChanges, int timingChanges)
    {
        var changeTypes = new List<string>();
        if (textChanges > 0)
        {
            changeTypes.Add($"{textChanges} text");
        }

        if (timingChanges > 0)
        {
            changeTypes.Add($"{timingChanges} timing");
        }

        return $"{lineCount} lines modified: {string.Join(" and ", changeTypes)} changes";
    }

    private static string TruncateText(string text)
    {
        return text.Length > MaxPreviewLength ? text.Substring(0, MaxPreviewLength) + "..." : text;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopChangeDetection();
        _changeDetectionTimer?.Dispose();
        _disposed = true;
    }

    public void Reset()
    {
        lock (_lock)
        {
            StopChangeDetection();

            _undoList.Clear();
            _redoList.Clear();

            StartChangeDetection();
        }
    }

}