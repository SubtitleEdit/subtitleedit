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

        if (selectedIndices != null && selectedIndices.Count > 0)
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
                // Extending blindly put this image on screen while the next one was already
                // showing - two overlapping images is not a rendering nuisance in Blu-ray SUP or
                // VobSub, it is invalid. The Apply duration limits dialog clips to the next start
                // (minus the minimum gap) and reports the cue as only partially fixed; do the same
                // here, minus the reporting this window has no room for.
                var next = index + 1 < subtitles.Count ? subtitles[index + 1] : null;
                var wantedMs = (double)MinimumDurationMs;
                if (next != null)
                {
                    var allowedMs = (next.StartTime - item.StartTime).TotalMilliseconds -
                                    Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
                    if (allowedMs < wantedMs)
                    {
                        wantedMs = allowedMs;
                    }
                }

                if (wantedMs > durationMs)
                {
                    item.Duration = System.TimeSpan.FromMilliseconds(wantedMs);
                }
            }
            else if (durationMs > MaximumDurationMs)
            {
                item.Duration = System.TimeSpan.FromMilliseconds(MaximumDurationMs);
            }
        }
    }
}

