using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.WaveformGuessTimeCodes;

public partial class WaveformGuessTimeCodesViewModel : ObservableObject
{
    [ObservableProperty] private bool _startFromVideoPosition;
    [ObservableProperty] private bool _startFromBeginning;

    [ObservableProperty] private bool _deleteLinesAll;
    [ObservableProperty] private bool _deleteLinesNone;
    [ObservableProperty] private bool _deleteLinesFromVideoPosition;

    [ObservableProperty] private int? _scanBlockSize;
    [ObservableProperty] private int? _scanBlockAverageMin;
    [ObservableProperty] private int? _scanBlockAverageMax;

    [ObservableProperty] private int? _splitLongSubtitlesAtMs;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public WaveformGuessTimeCodesViewModel()
    {
        StartFromVideoPosition = true;
        StartFromBeginning = false;

        DeleteLinesAll = false;
        DeleteLinesNone = false;
        DeleteLinesFromVideoPosition = true;

        ScanBlockSize = 100;
        ScanBlockAverageMin = 35;
        ScanBlockAverageMax = 70;
        SplitLongSubtitlesAtMs = 3500;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var s = Se.Settings.Waveform;
        if (s.GuessTimeCodeStartFromBeginning)
        {
            StartFromVideoPosition = false;
            StartFromBeginning = true;

            DeleteLinesNone = false;
            DeleteLinesFromVideoPosition = false;
            DeleteLinesAll = true;
        }
        else
        {
            StartFromVideoPosition = true;
            StartFromBeginning = false;

            DeleteLinesNone = false;
            DeleteLinesAll = false;
            DeleteLinesFromVideoPosition = true;
        }

        ScanBlockSize = s.GuessTimeCodeScanBlockSize;
        ScanBlockAverageMin = s.GuessTimeCodeScanBlockAverageMin;
        ScanBlockAverageMax = s.GuessTimeCodeScanBlockAverageMax;
        SplitLongSubtitlesAtMs = s.GuessTimeCodeSplitLongSubtitlesAtMs;
    }

    private void SaveSettings()
    {
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
}