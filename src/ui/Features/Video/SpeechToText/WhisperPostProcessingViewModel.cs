using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public partial class WhisperPostProcessingViewModel : ObservableObject
{
    [ObservableProperty] private bool _adjustTimings;
    [ObservableProperty] private bool _mergeShortLines;
    [ObservableProperty] private bool _breakSplitLongLines;
    [ObservableProperty] private bool _fixShortDuration;
    [ObservableProperty] private bool _fixCasing;
    [ObservableProperty] private bool _addPeriods;
    [ObservableProperty] private bool _changeUnderlineToColor;
    [ObservableProperty] private Color _changeUnderlineToColorColor;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public WhisperPostProcessingViewModel(IWindowService windowService)
    {
    }

    [RelayCommand]
    private void OK()
    {
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
            Window?.Close();
        }
    }
}