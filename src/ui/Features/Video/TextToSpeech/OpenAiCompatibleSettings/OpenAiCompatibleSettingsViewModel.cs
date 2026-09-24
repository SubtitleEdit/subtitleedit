using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.OpenAiCompatibleSettings;

public partial class OpenAiCompatibleSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _customUrl;
    [ObservableProperty] private string _customModels;
    [ObservableProperty] private string _customVoices;
    [ObservableProperty] private string _instructions;
    [ObservableProperty] private double _speed;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public OpenAiCompatibleSettingsViewModel()
    {
        var s = Se.Settings.Video.TextToSpeech;
        CustomUrl = s.OpenAiCompatibleCustomUrl ?? string.Empty;
        CustomModels = s.OpenAiCompatibleCustomModels ?? string.Empty;
        CustomVoices = s.OpenAiCompatibleCustomVoices ?? string.Empty;
        Instructions = s.OpenAiCompatibleInstructions ?? string.Empty;
        Speed = s.OpenAiCompatibleSpeed > 0 ? s.OpenAiCompatibleSpeed : 1.0;
    }

    [RelayCommand]
    private async Task ShowMoreOnWeb()
    {
        await Window!.Launcher.LaunchUriAsync(new Uri("https://openrouter.ai/models?fmt=cards&output_modalities=speech"));
    }

    [RelayCommand]
    private void Reset()
    {
        var defaults = new SeVideoTextToSpeech();
        CustomUrl = defaults.OpenAiCompatibleCustomUrl;
        CustomModels = defaults.OpenAiCompatibleCustomModels;
        CustomVoices = defaults.OpenAiCompatibleCustomVoices;
        Instructions = defaults.OpenAiCompatibleInstructions;
        Speed = defaults.OpenAiCompatibleSpeed;
    }

    [RelayCommand]
    private void Ok()
    {
        var s = Se.Settings.Video.TextToSpeech;
        s.OpenAiCompatibleCustomUrl = CustomUrl?.Trim() ?? string.Empty;
        s.OpenAiCompatibleCustomModels = CustomModels?.Trim() ?? string.Empty;
        s.OpenAiCompatibleCustomVoices = CustomVoices?.Trim() ?? string.Empty;
        s.OpenAiCompatibleInstructions = Instructions?.Trim() ?? string.Empty;
        s.OpenAiCompatibleSpeed = Math.Clamp(Speed, 0.25, 4.0);
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
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/text-to-speech", "engine-settings");
        }
    }
}
