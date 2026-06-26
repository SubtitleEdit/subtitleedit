using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;

public partial class ChangeSpeedViewModel : ObservableObject
{
    [ObservableProperty] private double _speedPercent;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;
    [ObservableProperty] private bool _isSelectionAvailable;

    private ObservableCollection<SubtitleLineViewModel>? _subtitles;
    private IList<int>? _selectedIndices;
    private Action<double, bool, bool, bool>? _binaryApplyCallback;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public ChangeSpeedViewModel()
    {
        SpeedPercent = 100.0;
        AdjustAll = true;
    }

    [RelayCommand]
    private void SetFromDropFrameValue()
    {
        SpeedPercent = 100.1001;
    }

    [RelayCommand]
    private void SetToDropFrameValue()
    {
        SpeedPercent = 99.9889;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Apply()
    {
        if (_subtitles != null)
        {
            if (AdjustSelectedLinesAndForward && _selectedIndices?.Count > 0)
                ChangeSpeed(_subtitles.Skip(_selectedIndices[0]), SpeedPercent);
            else if (AdjustSelectedLines && _selectedIndices?.Count > 0)
                ChangeSpeed(_selectedIndices.Select(i => _subtitles[i]), SpeedPercent);
            else
                ChangeSpeed(_subtitles, SpeedPercent);
            return;
        }
        _binaryApplyCallback?.Invoke(SpeedPercent, AdjustAll, AdjustSelectedLines, AdjustSelectedLinesAndForward);
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void Initialize(ObservableCollection<SubtitleLineViewModel> subtitles, IList<int> selectedIndices)
    {
        _subtitles = subtitles;
        _selectedIndices = selectedIndices;
        IsSelectionAvailable = selectedIndices.Count > 0;
        if (!IsSelectionAvailable)
            AdjustAll = true;
    }

    internal void Initialize(bool isSelectionAvailable, Action<double, bool, bool, bool> binaryApplyCallback)
    {
        IsSelectionAvailable = isSelectionAvailable;
        if (!isSelectionAvailable)
        {
            AdjustAll = true;
        }
        _binaryApplyCallback = binaryApplyCallback;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/change-speed");
        }
    }

    internal static void ChangeSpeed(IEnumerable<SubtitleLineViewModel> subtitles, double speedPercent)
    {
        var factor = 100.0 / speedPercent;
        foreach (var subtitle in subtitles)
        {
            subtitle.SetTimes(
                TimeSpan.FromMilliseconds(subtitle.StartTime.TotalMilliseconds * factor),
                TimeSpan.FromMilliseconds(subtitle.EndTime.TotalMilliseconds * factor));
        }
    }
}
