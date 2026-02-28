using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public partial class VoiceSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _voiceTestText;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public bool RefreshVoices { get; private set; }

    public VoiceSettingsViewModel()
    {
        VoiceTestText = Se.Settings.Video.TextToSpeech.VoiceTestText;
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Video.TextToSpeech.VoiceTestText = VoiceTestText;
        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void RefreshVoiceList()
    {
        Se.Settings.Video.TextToSpeech.VoiceTestText = VoiceTestText;
        Se.SaveSettings();
        RefreshVoices = true;
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