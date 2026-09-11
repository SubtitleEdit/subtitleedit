using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;

public partial class SetVideoOffsetViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan? _timeOffset;
    [ObservableProperty] private bool _relativeToCurrentVideoPosition;
    [ObservableProperty] private bool _keepTimeCodes;
    [ObservableProperty] private ObservableCollection<VideoOffsetHistoryItem> _offsetHistory;
    [ObservableProperty] private VideoOffsetHistoryItem? _selectedOffsetHistoryItem;

    private const int MaxHistoryItems = 10;

    // "Apply" and "OK" hand the offset to the caller instead of the caller reading it off a
    // closed dialog, so the window can stay open and be applied again with a new value.
    private Action<TimeSpan, bool, bool>? _applyCallback;
    private Action? _resetCallback;

    // Whether the current inputs have already been applied, so OK after Apply just closes. In
    // "relative to current video position" mode a re-apply is not idempotent while the video
    // plays: it would recompute against the moved position and silently replace the offset (and
    // shift baked time codes) the user just verified.
    private bool _appliedWithCurrentInput;

    public Window? Window { get; set; }

    public SetVideoOffsetViewModel()
    {
        OffsetHistory = new ObservableCollection<VideoOffsetHistoryItem>();
        TimeOffset = TimeSpan.FromMilliseconds(Se.Settings.General.CurrentVideoOffsetInMs);
    }

    public void Initialize(Action<TimeSpan, bool, bool> applyCallback, Action resetCallback)
    {
        _applyCallback = applyCallback;
        _resetCallback = resetCallback;
        LoadHistory();
    }

    [RelayCommand]
    private void SetTenHours()
    {
        TimeOffset = TimeSpan.FromHours(10);
    }

    [RelayCommand]
    private void Apply()
    {
        ApplyCurrentOffset();
    }

    [RelayCommand]
    private void Ok()
    {
        if (TimeOffset == null)
        {
            Cancel();
            return;
        }

        if (!_appliedWithCurrentInput)
        {
            ApplyCurrentOffset();
        }

        Window?.Close();
    }

    [RelayCommand]
    private void Reset()
    {
        TimeOffset = TimeSpan.Zero;
        _resetCallback?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true; // the OK button is IsDefault and would run OK again on the same Enter
            Ok();
        }
    }

    private void ApplyCurrentOffset()
    {
        if (TimeOffset == null || _applyCallback == null)
        {
            return;
        }

        var offset = TimeOffset.Value;
        _applyCallback(offset, RelativeToCurrentVideoPosition, KeepTimeCodes);

        // The typed offset is remembered, not the one "relative to current video position"
        // computed from it - the typed one is what the user would want to pick again.
        AddToHistory(offset);

        // Last: the history sync above can echo a rounded value back into TimeOffset, which
        // would clear the flag again right after it was set.
        _appliedWithCurrentInput = true;
    }

    private void LoadHistory()
    {
        var msList = (Se.Settings.General.VideoOffsetHistoryInMs ?? new List<long>())
            .Where(p => p != 0)
            .Distinct()
            .Take(MaxHistoryItems)
            .ToList();

        if (msList.Count == 0)
        {
            // SE 4 parity: the two offsets a broadcast time code almost always needs.
            msList.Add((long)TimeSpan.FromHours(1).TotalMilliseconds);
            msList.Add((long)TimeSpan.FromHours(10).TotalMilliseconds);
        }

        OffsetHistory.Clear();
        foreach (var ms in msList)
        {
            OffsetHistory.Add(new VideoOffsetHistoryItem(ms));
        }

        SyncSelectedHistoryItem();
    }

    private void AddToHistory(TimeSpan offset)
    {
        var ms = ToMilliseconds(offset);
        if (ms == 0)
        {
            return; // "no offset" is what Reset is for - it would only be noise in the list
        }

        // A repeat of an offset already in the list moves to the top instead of doubling up.
        var existing = OffsetHistory.FirstOrDefault(p => p.TotalMilliseconds == ms);
        if (existing != null)
        {
            OffsetHistory.Remove(existing);
        }

        OffsetHistory.Insert(0, existing ?? new VideoOffsetHistoryItem(ms));

        while (OffsetHistory.Count > MaxHistoryItems)
        {
            OffsetHistory.RemoveAt(OffsetHistory.Count - 1);
        }

        Se.Settings.General.VideoOffsetHistoryInMs = OffsetHistory.Select(p => p.TotalMilliseconds).ToList();

        // Removing the selected item cleared the drop-down selection - put it back on the entry
        // the offset field now holds.
        SyncSelectedHistoryItem();
    }

    /// <summary>
    /// The drop-down shows the entry matching the offset field, or nothing when the field holds
    /// something else - so picking an entry the user has just typed over selects it again.
    /// </summary>
    private void SyncSelectedHistoryItem()
    {
        var ms = TimeOffset.HasValue ? ToMilliseconds(TimeOffset.Value) : 0;
        SelectedOffsetHistoryItem = OffsetHistory.FirstOrDefault(p => p.TotalMilliseconds == ms);
    }

    private static long ToMilliseconds(TimeSpan offset)
    {
        return (long)Math.Round(offset.TotalMilliseconds, MidpointRounding.AwayFromZero);
    }

    partial void OnTimeOffsetChanged(TimeSpan? value)
    {
        _appliedWithCurrentInput = false;
        SyncSelectedHistoryItem();
    }

    partial void OnRelativeToCurrentVideoPositionChanged(bool value)
    {
        _appliedWithCurrentInput = false;
    }

    partial void OnKeepTimeCodesChanged(bool value)
    {
        _appliedWithCurrentInput = false;
    }

    partial void OnSelectedOffsetHistoryItemChanged(VideoOffsetHistoryItem? value)
    {
        if (value != null)
        {
            TimeOffset = value.Offset;
        }
    }
}
