using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAllTimes;

public partial class BinaryAdjustAllTimesViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan _adjustment;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;
    [ObservableProperty] private bool _isSelectionAvailable;
    [ObservableProperty] private string _totalAdjustmentInfo;

    private double _totalAdjustment;
    private IList<BinarySubtitleItem>? _subtitles;
    private IList<int>? _selectedIndices;
    private Action? _refreshGrid;

    public Window? Window { get; set; }

    public BinaryAdjustAllTimesViewModel()
    {
        TotalAdjustmentInfo = string.Empty;
        LoadSettings();
    }

    public void Initialize(IList<BinarySubtitleItem> subtitles, IList<int> selectedIndices, Action refreshGrid, bool forceSelectedLines = false)
    {
        _subtitles = subtitles;
        _selectedIndices = selectedIndices;
        _refreshGrid = refreshGrid;

        IsSelectionAvailable = selectedIndices.Count > 0;
        if (!IsSelectionAvailable)
        {
            AdjustAll = true;
        }
        else if (forceSelectedLines)
        {
            AdjustSelectedLines = true;
        }
    }

    private void LoadSettings()
    {
        Adjustment = TimeSpan.FromSeconds(Se.Settings.Synchronization.AdjustAllTimes.Seconds);
        if (Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesAndForwardSelected)
        {
            AdjustSelectedLinesAndForward = true;
        }
        else if (Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesSelected)
        {
            AdjustSelectedLines = true;
        }
        else
        {
            AdjustAll = true;
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Synchronization.AdjustAllTimes.Seconds = Adjustment.TotalSeconds;
        Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesAndForwardSelected = AdjustSelectedLinesAndForward;
        Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesSelected = AdjustSelectedLines;
        Se.Settings.Synchronization.AdjustAllTimes.IsAllSelected = AdjustAll;

        Se.SaveSettings();
    }

    private void ApplyStep(double seconds)
    {
        if (_subtitles == null) return;

        _totalAdjustment += seconds;
        ShowTotalAdjustmentInfo();

        var adjustmentMs = seconds * 1000.0;
        var indicesToProcess = BuildIndicesToProcess();

        foreach (var index in indicesToProcess)
        {
            if (index < 0 || index >= _subtitles.Count) continue;
            var item = _subtitles[index];
            var newStartTimeMs = item.StartTime.TotalMilliseconds + adjustmentMs;
            if (newStartTimeMs < 0) newStartTimeMs = 0;
            item.StartTime = TimeSpan.FromMilliseconds(newStartTimeMs);
        }

        _refreshGrid?.Invoke();
    }

    private IEnumerable<int> BuildIndicesToProcess()
    {
        if (_subtitles == null) yield break;

        if (AdjustSelectedLinesAndForward && _selectedIndices != null && _selectedIndices.Count > 0)
        {
            var firstIndex = _selectedIndices[0];
            for (var i = firstIndex; i < _subtitles.Count; i++)
                yield return i;
        }
        else if (AdjustSelectedLines && _selectedIndices != null && _selectedIndices.Count > 0)
        {
            foreach (var i in _selectedIndices)
                yield return i;
        }
        else if (AdjustAll)
        {
            for (var i = 0; i < _subtitles.Count; i++)
                yield return i;
        }
    }

    [RelayCommand]
    private void ShowEarlier()
    {
        ApplyStep(-Adjustment.TotalSeconds);
    }

    private void ShowTotalAdjustmentInfo()
    {
        TotalAdjustmentInfo = string.Format(Se.Language.General.TotalAdjustmentX, new TimeCode(_totalAdjustment * 1000.0).ToShortDisplayString());
    }

    [RelayCommand]
    private void ShowLater()
    {
        ApplyStep(Adjustment.TotalSeconds);
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
