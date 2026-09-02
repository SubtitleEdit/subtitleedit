using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
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

    private bool _recalculateUnavailable;

    public bool IsRecalculateBlocked =>
        _recalculateUnavailable && SelectedAdjustType?.Type == BinaryAdjustDurationType.Recalculate;

    public bool ShowRecalculateControls =>
        SelectedAdjustType?.Type == BinaryAdjustDurationType.Recalculate && !_recalculateUnavailable;

    public bool ShowAdjustNote => !IsRecalculateBlocked;

    partial void OnSelectedAdjustTypeChanged(BinaryAdjustDurationDisplay value)
    {
        OnPropertyChanged(nameof(IsRecalculateBlocked));
        OnPropertyChanged(nameof(ShowRecalculateControls));
        OnPropertyChanged(nameof(ShowAdjustNote));
        OkCommand.NotifyCanExecuteChanged();
    }

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

    public void Initialize(IEnumerable<BinarySubtitleItem> itemsInScope)
    {
        _recalculateUnavailable = itemsInScope.Any(s => string.IsNullOrWhiteSpace(s.Text));
        if (_recalculateUnavailable)
        {
            OnPropertyChanged(nameof(IsRecalculateBlocked));
            OnPropertyChanged(nameof(ShowRecalculateControls));
            OnPropertyChanged(nameof(ShowAdjustNote));
            OkCommand.NotifyCanExecuteChanged();
        }
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

            // A negative adjustment must not push the end time before the start time
            var minEndTime = subtitle.StartTime + TimeSpan.FromMilliseconds(100);
            if (AdjustSeconds < 0 && newEndTime < minEndTime)
            {
                newEndTime = minEndTime;
            }

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
                // Cap against the next cue, but never below this cue's own start: two images
                // sharing a start time capped flat to a zero-length cue, and rows out of order
                // to a negative one. The Seconds branch above already floors its result.
                subtitle.EndTime = CapEndTime(subtitle, nextSubtitle);
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

            // Set the duration TO the percentage of the original (110% = 10% longer), like the
            // main "Adjust durations" dialog and SE4 - the two share the same saved setting,
            // so they must not interpret it differently (this used to ADD the percentage).
            var originalDuration = subtitle.EndTime - subtitle.StartTime;
            var newDuration = originalDuration.TotalSeconds * (AdjustPercent / 100.0);
            var newEndTime = subtitle.StartTime + TimeSpan.FromSeconds(newDuration);

            if (nextSubtitle != null && newEndTime > nextSubtitle.StartTime)
            {
                subtitle.EndTime = CapEndTime(subtitle, nextSubtitle);
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
            // Strip tags/line breaks so the recalculated durations land at the requested CPS
            var charCount = (double)(subtitle.Text ?? string.Empty).CountCharacters(true);

            // Defence in depth: the window blocks Recalculate when any item in scope has no text
            // (image subtitles carry none until they are OCR'd), but this method is public and a
            // zero character count would otherwise collapse the cue to zero length.
            if (charCount <= 0)
            {
                continue;
            }

            // Whole milliseconds, rounded up: a fractional duration truncates to one ms short on
            // save, which puts the line just over the CPS it was computed for (#14418).
            var optimalDuration = CpsHelper.GetDurationForCps(charCount, AdjustRecalculateOptimalCharacterPerSecond);
            var maxDuration = CpsHelper.GetDurationForCps(charCount, AdjustRecalculateMaxCharacterPerSecond);

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
                subtitle.EndTime = CapEndTime(subtitle, nextSubtitle);
            }
            
            subtitle.Duration = subtitle.EndTime - subtitle.StartTime;
        }
    }

    /// <summary>
    /// The latest end time that still leaves this cue a real duration: just before the next cue,
    /// and never at or before this cue's own start.
    /// </summary>
    private static TimeSpan CapEndTime(BinarySubtitleItem subtitle, BinarySubtitleItem? nextSubtitle)
    {
        if (nextSubtitle == null)
        {
            return subtitle.EndTime;
        }

        var capped = nextSubtitle.StartTime - TimeSpan.FromMilliseconds(10);
        var minimumEndTime = subtitle.StartTime + TimeSpan.FromMilliseconds(10);
        return capped < minimumEndTime ? minimumEndTime : capped;
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

    private bool CanOk() => !IsRecalculateBlocked;

    [RelayCommand(CanExecute = nameof(CanOk))]
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
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, Se.Language.General.Percent);
            }
        }
        else if (SelectedAdjustType.Type == BinaryAdjustDurationType.Fixed)
        {
            if (AdjustFixed <= 0)
            {
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, Se.Language.General.FixedValue);
            }
        }
        else if (SelectedAdjustType.Type == BinaryAdjustDurationType.Recalculate)
        {
            if (AdjustRecalculateMaxCharacterPerSecond <= 1)
            {
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, Se.Language.General.MaxCharactersPerSecond);
            }

            if (AdjustRecalculateOptimalCharacterPerSecond <= 1)
            {
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, Se.Language.General.OptimalCharactersPerSecond);
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
