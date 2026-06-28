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

    // Snapshot of the subtitle times when the dialog opened. Both "Apply" and "Change" (OK)
    // recompute from this snapshot (original x factor), so applying repeatedly - e.g. Apply
    // then Change - is idempotent and never compounds the factor.
    private List<(double Start, double End)>? _originalTimes;

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
        Apply(); // apply once; idempotent (recomputed from the original snapshot)
        Window?.Close();
    }

    [RelayCommand]
    private void Apply()
    {
        if (SpeedPercent <= 0)
        {
            return;
        }

        if (_subtitles != null)
        {
            ApplyToSubtitles();
            return;
        }

        _binaryApplyCallback?.Invoke(SpeedPercent, AdjustAll, AdjustSelectedLines, AdjustSelectedLinesAndForward);
    }

    private void ApplyToSubtitles()
    {
        if (_subtitles == null || _originalTimes == null)
        {
            return;
        }

        var factor = 100.0 / SpeedPercent;

        IEnumerable<int> indices;
        if (AdjustSelectedLinesAndForward && _selectedIndices?.Count > 0)
            indices = Enumerable.Range(_selectedIndices[0], _subtitles.Count - _selectedIndices[0]);
        else if (AdjustSelectedLines && _selectedIndices?.Count > 0)
            indices = _selectedIndices;
        else
            indices = Enumerable.Range(0, _subtitles.Count);

        foreach (var i in indices)
        {
            if (i < 0 || i >= _subtitles.Count || i >= _originalTimes.Count)
            {
                continue;
            }

            _subtitles[i].SetTimes(
                TimeSpan.FromMilliseconds(_originalTimes[i].Start * factor),
                TimeSpan.FromMilliseconds(_originalTimes[i].End * factor));
        }
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
        _originalTimes = subtitles
            .Select(s => (s.StartTime.TotalMilliseconds, s.EndTime.TotalMilliseconds))
            .ToList();
        IsSelectionAvailable = selectedIndices.Count > 0;
        SetDefaultScope(IsSelectionAvailable);
    }

    internal void Initialize(bool isSelectionAvailable, Action<double, bool, bool, bool> binaryApplyCallback)
    {
        IsSelectionAvailable = isSelectionAvailable;
        SetDefaultScope(isSelectionAvailable);
        _binaryApplyCallback = binaryApplyCallback;
    }

    // Default to "selected lines" when a selection is available (mirrors the Adjust all times
    // dialog), otherwise "all lines".
    private void SetDefaultScope(bool isSelectionAvailable)
    {
        if (isSelectionAvailable)
        {
            AdjustSelectedLines = true;
            AdjustAll = false;
            AdjustSelectedLinesAndForward = false;
        }
        else
        {
            AdjustAll = true;
            AdjustSelectedLines = false;
            AdjustSelectedLinesAndForward = false;
        }
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
