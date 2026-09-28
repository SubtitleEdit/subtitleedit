using System;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Main.GridTimeAdjust;

public partial class GridTimeAdjustViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public bool OkPressed { get; set; }
    public bool FocusEndTime { get; set; }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private TimeSpan _startTime;

    [ObservableProperty]
    private TimeSpan _endTime;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private decimal _stepMs = 500;

    private bool _isUpdating;

    public void Initialize(string title, TimeSpan startTime, TimeSpan endTime, int initialStep, bool focusEndTime)
    {
        Title = title;
        _isUpdating = true;
        StartTime = startTime;
        EndTime = endTime;
        Duration = endTime - startTime;
        StepMs = initialStep > 0 ? initialStep : 500;
        FocusEndTime = focusEndTime;
        _isUpdating = false;
        OkPressed = false;
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        if (_isUpdating)
        {
            return;
        }

        _isUpdating = true;
        Duration = EndTime - value;
        _isUpdating = false;
    }

    partial void OnEndTimeChanged(TimeSpan value)
    {
        if (_isUpdating)
        {
            return;
        }

        _isUpdating = true;
        Duration = value - StartTime;
        _isUpdating = false;
    }

    partial void OnDurationChanged(TimeSpan value)
    {
        if (_isUpdating)
        {
            return;
        }

        _isUpdating = true;
        EndTime = StartTime + value;
        _isUpdating = false;
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
        OkPressed = false;
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Ok();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Cancel();
            e.Handled = true;
        }
    }
}
