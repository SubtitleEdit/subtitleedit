using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustDuration;

public partial class BinaryAdjustDurationViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BinaryAdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private BinaryAdjustDurationDisplay _selectedAdjustType;

    [ObservableProperty] private double _adjustSeconds;
    [ObservableProperty] private int _adjustPercent;
    [ObservableProperty] private double _adjustFixed;
    [ObservableProperty] private double _adjustRecalculateMaxCharacterPerSecond;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public BinaryAdjustDurationViewModel()
    {
        _adjustTypes = new ObservableCollection<BinaryAdjustDurationDisplay>(BinaryAdjustDurationDisplay.ListAll());
        _selectedAdjustType = _adjustTypes[0];
        LoadSettings();
    }

    public void AdjustDuration(List<BinarySubtitleItem> subtitles, List<int>? selectedIndices = null)
    {
        var itemsToAdjust = selectedIndices != null && selectedIndices.Count > 0
            ? selectedIndices.Select(i => subtitles[i]).ToList()
            : subtitles;

        if (SelectedAdjustType.Type == BinaryAdjustDurationType.Seconds)
        {
            DoAdjustViaSeconds(subtitles, itemsToAdjust);
        }
        else if (SelectedAdjustType.Type == BinaryAdjustDurationType.Fixed)
        {
            DoAdjustViaFixed(subtitles, itemsToAdjust);
        }
        else if (SelectedAdjustType.Type == BinaryAdjustDurationType.Percent)
        {
            DoAdjustViaPercent(subtitles, itemsToAdjust);
        }
        else if (SelectedAdjustType.Type == BinaryAdjustDurationType.Recalculate)
        {
            DoAdjustViaRecalculate(subtitles, itemsToAdjust);
        }
    }

    private void DoAdjustViaSeconds(List<BinarySubtitleItem> allSubtitles, List<BinarySubtitleItem> itemsToAdjust)
    {
        foreach (var subtitle in itemsToAdjust)
        {
            var index = allSubtitles.IndexOf(subtitle);
            var nextSubtitle = index + 1 < allSubtitles.Count ? allSubtitles[index + 1] : null;
            
            var newEndTime = subtitle.EndTime + TimeSpan.FromSeconds(AdjustSeconds);
            if (nextSubtitle != null && newEndTime <= nextSubtitle.StartTime || nextSubtitle == null)
            {
                subtitle.EndTime = newEndTime;
            }
            else if (nextSubtitle != null && newEndTime > nextSubtitle.StartTime)
            {
                var cappedEndTime = nextSubtitle.StartTime - TimeSpan.FromMilliseconds(10);
                if (cappedEndTime > subtitle.EndTime)
                {
                    subtitle.EndTime = cappedEndTime;
                }
            }
            
            subtitle.Duration = subtitle.EndTime - subtitle.StartTime;
        }
    }

    private void DoAdjustViaFixed(List<BinarySubtitleItem> allSubtitles, List<BinarySubtitleItem> itemsToAdjust)
    {
        foreach (var subtitle in itemsToAdjust)
        {
            var index = allSubtitles.IndexOf(subtitle);
            var nextSubtitle = index + 1 < allSubtitles.Count ? allSubtitles[index + 1] : null;
            
            var newDuration = TimeSpan.FromSeconds(AdjustFixed);
            var newEndTime = subtitle.StartTime + newDuration;

            if (nextSubtitle != null && newEndTime > nextSubtitle.StartTime)
            {
                subtitle.EndTime = nextSubtitle.StartTime;
            }
            else
            {
                subtitle.EndTime = newEndTime;
            }
            
            subtitle.Duration = subtitle.EndTime - subtitle.StartTime;
        }
    }

    private void DoAdjustViaPercent(List<BinarySubtitleItem> allSubtitles, List<BinarySubtitleItem> itemsToAdjust)
    {
        foreach (var subtitle in itemsToAdjust)
        {
            var index = allSubtitles.IndexOf(subtitle);
            var nextSubtitle = index + 1 < allSubtitles.Count ? allSubtitles[index + 1] : null;

            var originalDuration = subtitle.EndTime - subtitle.StartTime;
            var adjustment = originalDuration.TotalSeconds * (AdjustPercent / 100.0);
            var newEndTime = subtitle.EndTime + TimeSpan.FromSeconds(adjustment);

            if (nextSubtitle != null && newEndTime > nextSubtitle.StartTime)
            {
                subtitle.EndTime = nextSubtitle.StartTime;
            }
            else
            {
                subtitle.EndTime = newEndTime;
            }
            
            subtitle.Duration = subtitle.EndTime - subtitle.StartTime;
        }
    }

    private void DoAdjustViaRecalculate(List<BinarySubtitleItem> allSubtitles, List<BinarySubtitleItem> itemsToAdjust)
    {
        foreach (var subtitle in itemsToAdjust)
        {
            var index = allSubtitles.IndexOf(subtitle);
            var charCount = subtitle.Text?.Length ?? 0;

            var optimalDuration = TimeSpan.FromSeconds(charCount / AdjustRecalculateOptimalCharacterPerSecond);
            var maxDuration = TimeSpan.FromSeconds(charCount / AdjustRecalculateMaxCharacterPerSecond);

            var nextSubtitle = index + 1 < allSubtitles.Count ? allSubtitles[index + 1] : null;
            var maxEndTime = nextSubtitle?.StartTime ?? TimeSpan.MaxValue;

            var proposedEndTime = subtitle.StartTime + optimalDuration;
            var fallbackEndTime = subtitle.StartTime + maxDuration;

            if (proposedEndTime <= maxEndTime)
            {
                subtitle.EndTime = proposedEndTime;
            }
            else if (fallbackEndTime <= maxEndTime)
            {
                subtitle.EndTime = fallbackEndTime;
            }
            else
            {
                subtitle.EndTime = maxEndTime;
            }
            
            subtitle.Duration = subtitle.EndTime - subtitle.StartTime;
        }
    }

    private void LoadSettings()
    {
        AdjustSeconds = Se.Settings.Tools.AdjustDurations.AdjustDurationSeconds;
        AdjustPercent = Se.Settings.Tools.AdjustDurations.AdjustDurationPercent;
        AdjustFixed = Se.Settings.Tools.AdjustDurations.AdjustDurationFixed;
        AdjustRecalculateMaxCharacterPerSecond = Se.Settings.Tools.AdjustDurations.AdjustDurationMaximumCps;
        AdjustRecalculateOptimalCharacterPerSecond = Se.Settings.Tools.AdjustDurations.AdjustDurationOptimalCps;

        SelectedAdjustType = AdjustTypes.FirstOrDefault(p =>
                                 p.Type.ToString() == Se.Settings.Tools.AdjustDurations.AdjustDurationLast)
                             ?? AdjustTypes[0];
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.AdjustDurations.AdjustDurationSeconds = AdjustSeconds;
        Se.Settings.Tools.AdjustDurations.AdjustDurationPercent = AdjustPercent;
        Se.Settings.Tools.AdjustDurations.AdjustDurationFixed = AdjustFixed;
        Se.Settings.Tools.AdjustDurations.AdjustDurationMaximumCps = AdjustRecalculateMaxCharacterPerSecond;
        Se.Settings.Tools.AdjustDurations.AdjustDurationOptimalCps = AdjustRecalculateOptimalCharacterPerSecond;

        Se.Settings.Tools.AdjustDurations.AdjustDurationLast = SelectedAdjustType.Type.ToString();

        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Ok()
    {
        var msg = GetValidationError();
        if (!string.IsNullOrEmpty(msg))
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private string GetValidationError()
    {
        if (Window == null)
        {
            return "Window is null";
        }

        if (SelectedAdjustType.Type == BinaryAdjustDurationType.Seconds)
        {
            // No validation needed for seconds
        }
        else if (SelectedAdjustType.Type == BinaryAdjustDurationType.Percent)
        {
            if (AdjustPercent <= 0)
            {
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Percent");
            }
        }
        else if (SelectedAdjustType.Type == BinaryAdjustDurationType.Fixed)
        {
            if (AdjustFixed <= 0)
            {
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Fixed value");
            }
        }
        else if (SelectedAdjustType.Type == BinaryAdjustDurationType.Recalculate)
        {
            if (AdjustRecalculateMaxCharacterPerSecond <= 1)
            {
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Max character per second");
            }

            if (AdjustRecalculateOptimalCharacterPerSecond <= 1)
            {
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Optimal character per second");
            }
        }

        return string.Empty;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
