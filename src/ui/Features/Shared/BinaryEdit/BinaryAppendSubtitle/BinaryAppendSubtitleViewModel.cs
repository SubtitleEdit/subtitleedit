using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAppendSubtitle;

public partial class BinaryAppendSubtitleViewModel : ObservableObject
{
    [ObservableProperty] private bool _appendTimeCodes;
    [ObservableProperty] private bool _keepTimeCodes;
    [ObservableProperty] private TimeSpan _timeOffset;
    [ObservableProperty] private bool _isTimeOffsetVisible;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public BinaryAppendSubtitleViewModel()
    {
    }

    public void Initialize(TimeSpan suggestedOffset)
    {
        TimeOffset = suggestedOffset;

        if (Se.Settings.Tools.BinEditAppendKeepTimeCodes)
        {
            KeepTimeCodes = true;
        }
        else
        {
            AppendTimeCodes = true;
        }

        IsTimeOffsetVisible = AppendTimeCodes;
    }

    partial void OnAppendTimeCodesChanged(bool value)
    {
        IsTimeOffsetVisible = value;
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Tools.BinEditAppendKeepTimeCodes = KeepTimeCodes;
        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }
}
