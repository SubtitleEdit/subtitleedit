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

    private void ClearOriginalTimeCodes()
    {
        foreach (var row in Subtitles)
        {
            row.SetOriginalTimeCodes(null, null);
        }
    }
}
