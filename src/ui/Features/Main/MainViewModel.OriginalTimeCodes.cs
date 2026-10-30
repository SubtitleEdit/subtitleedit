using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Main;

// The original's own time codes (SubtitleLineViewModel.OriginalStartTime/EndTime). With
// "Lock original to translation" on, retiming a translation line moves its original line along;
// off, the timeline's original row drags the original's time codes on their own.
public partial class MainViewModel
{
    public MenuItem MenuItemAudioVisualizerLockOriginalToTranslation { get; set; } = new();

    public bool CanEditOriginalTimeCodes => CanEditOriginal && _subtitleOriginal is { Paragraphs.Count: > 0 };

    public bool IsOriginalLockedToTranslation => Se.Settings.General.LockOriginalToTranslation;

    [RelayCommand]
    private void ToggleLockOriginalToTranslation()
    {
        Se.Settings.General.LockOriginalToTranslation = !Se.Settings.General.LockOriginalToTranslation;
        OnPropertyChanged(nameof(IsOriginalLockedToTranslation));
        _shortcutManager.ClearKeys();
    }

    internal bool HasOriginalLine(SubtitleLineViewModel row)
    {
        return row.IsReferenceOnly || !IsShowingOriginalNonMatchingLines || !string.IsNullOrEmpty(row.OriginalText);
    }

    /// <summary>
    /// New time codes for the original line a row stands for. A display-only reference row is
    /// that line itself, so its own times move.
    /// </summary>
    internal void SetRowOriginalTimeCodes(SubtitleLineViewModel row, TimeSpan start, TimeSpan end)
    {
        if (row.IsReferenceOnly)
        {
            row.SetTimes(start, end);
        }
        else
        {
            row.SetOriginalTimeCodes(start, end);
        }

        _updateAudioVisualizer = true;
    }

    /// <summary>
    /// Gives each row the time codes of the original line it displays, where those differ from the
    /// row's own - so an original whose timing is not the translation's keeps it.
    /// </summary>
    private void SyncOriginalTimeCodesFromLinkedLines()
    {
        if (_subtitleOriginal == null || _subtitleOriginal.Paragraphs.Count == 0)
        {
            return;
        }

        var linesById = new Dictionary<Guid, Paragraph>(_subtitleOriginal.Paragraphs.Count);
        foreach (var p in _subtitleOriginal.Paragraphs)
        {
            if (p.Id is { } id)
            {
                linesById[id] = p;
            }
        }

        foreach (var row in Subtitles)
        {
            if (row.IsReferenceOnly ||
                row.ReferenceParagraphId is not { } id ||
                !linesById.TryGetValue(id, out var line))
            {
                continue;
            }

            var start = TimeSpan.FromMilliseconds(line.StartTime.TotalMilliseconds);
            var end = TimeSpan.FromMilliseconds(line.EndTime.TotalMilliseconds);
            if (start == row.StartTime && end == row.EndTime)
            {
                row.SetOriginalTimeCodes(null, null);
            }
            else
            {
                row.SetOriginalTimeCodes(start, end);
            }
        }
    }

    // Same reasoning as the 500 ms look at the rows in TimelineTracks: nothing announces a change
    // to the original, so it is looked at again now and then instead of per frame.
    private const int ReadOnlyOriginalTimeCodesMaxAgeMs = 250;

    private readonly Dictionary<Guid, (TimeSpan Start, TimeSpan End)> _readOnlyOriginalTimeCodes = new();
    private Subtitle? _readOnlyOriginalTimeCodesSource;
    private int _readOnlyOriginalTimeCodesCount;
    private long _readOnlyOriginalTimeCodesBuiltMs;

    /// <summary>
    /// The read-only original's own time codes by line id, or null when the original is editable.
    /// The rows' original time codes follow the translation as it is retimed (with "Lock original
    /// to translation" on, or when a line shares its row's times), but a read-only original never
    /// moves. Kept in one reused dictionary: the timeline asks on every frame, while the original's
    /// lines are rarely replaced - a pass over them per frame was thousands of inserts 60 times a
    /// second. A new original or line count rebuilds at once, anything else within the max age.
    /// </summary>
    internal IReadOnlyDictionary<Guid, (TimeSpan Start, TimeSpan End)>? GetReadOnlyOriginalTimeCodes()
    {
        if (!IsOriginalReadOnly || _subtitleOriginal == null)
        {
            return null;
        }

        var now = Environment.TickCount64;
        if (ReferenceEquals(_readOnlyOriginalTimeCodesSource, _subtitleOriginal) &&
            _readOnlyOriginalTimeCodesCount == _subtitleOriginal.Paragraphs.Count &&
            now - _readOnlyOriginalTimeCodesBuiltMs < ReadOnlyOriginalTimeCodesMaxAgeMs)
        {
            return _readOnlyOriginalTimeCodes;
        }

        _readOnlyOriginalTimeCodesSource = _subtitleOriginal;
        _readOnlyOriginalTimeCodesCount = _subtitleOriginal.Paragraphs.Count;
        _readOnlyOriginalTimeCodesBuiltMs = now;
        _readOnlyOriginalTimeCodes.Clear();
        foreach (var p in _subtitleOriginal.Paragraphs)
        {
            if (p.Id is { } id)
            {
                _readOnlyOriginalTimeCodes[id] = (TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds), TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds));
            }
        }

        return _readOnlyOriginalTimeCodes;
    }

    private void ClearOriginalTimeCodes()
    {
        foreach (var row in Subtitles)
        {
            row.SetOriginalTimeCodes(null, null);
        }
    }
}
