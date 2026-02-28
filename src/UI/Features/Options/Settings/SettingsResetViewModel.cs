using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class SettingsResetViewModel : ObservableObject
{
    [ObservableProperty] private bool _resetAll;
    [ObservableProperty] private bool _resetRecentFiles;
    [ObservableProperty] private bool _resetShortcuts;
    [ObservableProperty] private bool _resetMultipleReplaceRules;
    [ObservableProperty] private bool _resetRules;
    [ObservableProperty] private bool _resetAppearance;
    [ObservableProperty] private bool _resetAutoTranslate;
    [ObservableProperty] private bool _resetWaveform;
    [ObservableProperty] private bool _resetSyntaxColoring;
    [ObservableProperty] private bool _resetWindowPositionAndSize;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public SettingsResetViewModel()
    {
        ResetAll = true;
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