using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryApplyDurationLimits;

public partial class BinaryApplyDurationLimitsViewModel : ObservableObject
{
    [ObservableProperty] private int _minimumDurationMs;
    [ObservableProperty] private int _maximumDurationMs;
    [ObservableProperty] private bool _applyToAll;
    [ObservableProperty] private bool _applyToSelectedLines;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public BinaryApplyDurationLimitsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        MinimumDurationMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        MaximumDurationMs = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        ApplyToAll = true;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
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
        else if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            Ok();
        }
    }

    public void ApplyLimits(List<BinarySubtitleItem> subtitles, List<int>? selectedIndices = null)
    {
        var indicesToProcess = new List<int>();

        if (ApplyToSelectedLines && selectedIndices != null && selectedIndices.Count > 0)
        {
            indicesToProcess.AddRange(selectedIndices);
        }
        else
        {
            for (int i = 0; i < subtitles.Count; i++)
            {
                indicesToProcess.Add(i);
            }
        }

        foreach (var index in indicesToProcess)
        {
            if (index < 0 || index >= subtitles.Count)
            {
                continue;
            }

            var item = subtitles[index];
            var durationMs = item.Duration.TotalMilliseconds;

            if (durationMs < MinimumDurationMs)
            {
                item.Duration = System.TimeSpan.FromMilliseconds(MinimumDurationMs);
            }
            else if (durationMs > MaximumDurationMs)
            {
                item.Duration = System.TimeSpan.FromMilliseconds(MaximumDurationMs);
            }
        }
    }
}

