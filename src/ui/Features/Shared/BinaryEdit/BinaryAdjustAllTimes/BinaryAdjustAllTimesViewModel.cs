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
    [ObservableProperty] private string _totalAdjustmentInfo;

    private double _totalAdjustment;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public BinaryAdjustAllTimesViewModel()
    {
        TotalAdjustmentInfo = string.Empty;
        LoadSettings();
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

    [RelayCommand]
    private void ShowEarlier()
    {
        _totalAdjustment -= Adjustment.TotalSeconds;
        ShowTotalAdjustmentInfo();
    }

    private void ShowTotalAdjustmentInfo()
    {
        TotalAdjustmentInfo = string.Format(Se.Language.General.TotalAdjustmentX, new TimeCode(_totalAdjustment * 1000.0).ToShortDisplayString());
    }

    [RelayCommand]
    private void ShowLater()
    {
        _totalAdjustment += Adjustment.TotalSeconds;
        ShowTotalAdjustmentInfo();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
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

    public void AdjustTimes(List<BinarySubtitleItem> subtitles, List<int>? selectedIndices = null)
    {
        var indicesToProcess = new List<int>();

        if (AdjustSelectedLinesAndForward && selectedIndices != null && selectedIndices.Count > 0)
        {
            // Get the first selected index and all indices from there forward
            var firstIndex = selectedIndices[0];
            for (int i = firstIndex; i < subtitles.Count; i++)
            {
                indicesToProcess.Add(i);
            }
        }
        else if (AdjustSelectedLines && selectedIndices != null && selectedIndices.Count > 0)
        {
            indicesToProcess.AddRange(selectedIndices);
        }
        else
        {
            // Adjust all
            for (int i = 0; i < subtitles.Count; i++)
            {
                indicesToProcess.Add(i);
            }
        }

        var adjustmentMs = _totalAdjustment * 1000.0;

        foreach (var index in indicesToProcess)
        {
            if (index < 0 || index >= subtitles.Count)
            {
                continue;
            }

            var item = subtitles[index];
            
            // Adjust start time
            var newStartTimeMs = item.StartTime.TotalMilliseconds + adjustmentMs;
            if (newStartTimeMs < 0)
            {
                newStartTimeMs = 0;
            }
            item.StartTime = TimeSpan.FromMilliseconds(newStartTimeMs);

            // Adjust end time
            var newEndTimeMs = item.EndTime.TotalMilliseconds + adjustmentMs;
            if (newEndTimeMs < 0)
            {
                newEndTimeMs = 0;
            }
            item.EndTime = TimeSpan.FromMilliseconds(newEndTimeMs);

            // Update duration
            item.Duration = item.EndTime - item.StartTime;
        }

        // Reset total adjustment after applying
        _totalAdjustment = 0;
        TotalAdjustmentInfo = string.Empty;
    }
}

