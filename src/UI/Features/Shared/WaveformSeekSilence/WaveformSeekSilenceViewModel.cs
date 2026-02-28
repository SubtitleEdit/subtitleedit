using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Shared.WaveformSeekSilence;

public partial class WaveformSeekSilenceViewModel : ObservableObject
{
    [ObservableProperty] private bool _seekForward;
    [ObservableProperty] private bool _seekBackward;
    [ObservableProperty] private double? _silenceMinDuration;
    [ObservableProperty] private double? _silenceMaxVolume;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public WaveformSeekSilenceViewModel()
    {
        SeekForward = true;
        SeekBackward = false;

        SilenceMinDuration = 0.3;
        SilenceMaxVolume = 0.1;
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        var s = Se.Settings.Waveform;

        if (s.SeekSilenceSeekForward)
        {
            SeekForward = true;
            SeekBackward = false;
        }
        else
        {
            SeekForward = false;
            SeekBackward = true;
        }   

        SilenceMinDuration = s.SeekSilenceMinDurationSeconds;
        SilenceMaxVolume = s.SeekSilenceMaxVolume;
    }

    private void SaveSettings()
    {
        var s = Se.Settings.Waveform;
        s.SeekSilenceSeekForward = SeekForward;
        s.SeekSilenceMinDurationSeconds = SilenceMinDuration ?? 0.3;
        s.SeekSilenceMaxVolume = SilenceMaxVolume ?? 0.1;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(WavePeakData2 wavePeaks)
    {
    }
}