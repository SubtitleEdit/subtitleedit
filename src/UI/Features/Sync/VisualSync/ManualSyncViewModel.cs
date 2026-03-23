using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public partial class ManualSyncViewModel : ObservableObject
{
    [ObservableProperty] private double _offsetSeconds;
    [ObservableProperty] private double _speedFactor;

    public Window? Window { get; set; }
    
    public bool OkPressed { get; private set; }

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

    internal void Initialize(ObservableCollection<SubtitleLineViewModel> subtitles, double offsetSeconds = 0.0, double speedFactor = 1.0)
    {
        OffsetSeconds = offsetSeconds;
        SpeedFactor = Math.Abs(speedFactor) < 0.000001 ? 1.0 : speedFactor;
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

    internal static void ChangeSpeed(ObservableCollection<SubtitleLineViewModel> subtitles, double speedPercent)
    {
        foreach (var subtitle in subtitles)
        {
            subtitle.StartTime = TimeSpan.FromMilliseconds(subtitle.StartTime.TotalMilliseconds * (100.0 / speedPercent));
            subtitle.EndTime = TimeSpan.FromMilliseconds(subtitle.EndTime.TotalMilliseconds * (100.0 / speedPercent));
        }
    }
}