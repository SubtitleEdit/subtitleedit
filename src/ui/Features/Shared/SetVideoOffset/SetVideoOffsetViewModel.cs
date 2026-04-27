using System;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;

public partial class SetVideoOffsetViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan? _timeOffset;
    [ObservableProperty] private bool _relativeToCurrentVideoPosition;
    [ObservableProperty] private bool _keepTimeCodes;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public bool ResetPressed { get; private set; }

    public SetVideoOffsetViewModel()
    {
        TimeOffset = TimeSpan.FromMilliseconds(Se.Settings.General.CurrentVideoOffsetInMs);
    }

    public void Initialize()
    {
    }

    [RelayCommand]
    private void SetTenHours()
    {
        TimeOffset = TimeSpan.FromHours(10);
    }

    [RelayCommand]
    private void Ok()
    {
        if (TimeOffset == null)
        {
            Cancel();
            return;
        }

        Se.Settings.General.CurrentVideoOffsetInMs = (int)Math.Round(TimeOffset.Value.TotalMilliseconds, MidpointRounding.AwayFromZero);
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Reset()
    {
        ResetPressed = true;
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
        else if (e.Key == Key.Enter)
        {
            Ok();
        }
    }
}