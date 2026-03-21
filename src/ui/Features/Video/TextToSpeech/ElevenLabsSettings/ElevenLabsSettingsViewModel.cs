using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;

public partial class ElevenLabsSettingsViewModel : ObservableObject
{
    [ObservableProperty] private double _stability;
    [ObservableProperty] private double _similarity;
    [ObservableProperty] private double _speakerBoost;
    [ObservableProperty] private double _speed;
    [ObservableProperty] private double _styleExaggeration;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public ElevenLabsSettingsViewModel()
    {
        Stability = 0.5;
        Similarity = 0.5;
        SpeakerBoost = 0;
        Speed = 1.0;
        StyleExaggeration = 0;

        LoadSettings();
    }

    private void LoadSettings()
    {
        Stability = Se.Settings.Video.TextToSpeech.ElevenLabsStability;
        Similarity = Se.Settings.Video.TextToSpeech.ElevenLabsSimilarity;
        SpeakerBoost = Se.Settings.Video.TextToSpeech.ElevenLabsSpeakerBoost;
        Speed = Se.Settings.Video.TextToSpeech.ElevenLabsSpeed;
        StyleExaggeration = Se.Settings.Video.TextToSpeech.ElevenLabsStyleeExaggeration;
    }

    public void SaveSettings()
    {
        Se.Settings.Video.TextToSpeech.ElevenLabsStability = Stability;
        Se.Settings.Video.TextToSpeech.ElevenLabsSimilarity = Similarity;
        Se.Settings.Video.TextToSpeech.ElevenLabsSpeakerBoost = SpeakerBoost;
        Se.Settings.Video.TextToSpeech.ElevenLabsSpeed = Speed;
        Se.Settings.Video.TextToSpeech.ElevenLabsStyleeExaggeration = StyleExaggeration;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task ShowStabilityHelp()
    {
        await ShowStabilityHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowSimilarityHelp()
    {
        await ShowSimilarityHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowSpeakerBoostHelp()
    {
        await ShowSpeakerBoostHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowSpeedHelp()
    {
        await ShowSpeedHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowStyleExaggerationHelp()
    {
        await ShowStyleExaggerationHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowMoreOnWeb()
    {
        await Window!.Launcher.LaunchUriAsync(new Uri("https://elevenlabs.io/docs/capabilities/text-to-speech"));
    }

    [RelayCommand]
    private void Reset()
    {
        var settings = new SeVideoTextToSpeech();
        Stability = settings.ElevenLabsStability;
        Similarity = settings.ElevenLabsSimilarity;
        SpeakerBoost = settings.ElevenLabsSpeakerBoost;
        Speed = settings.ElevenLabsSpeed;
        StyleExaggeration = settings.ElevenLabsStyleeExaggeration;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        SaveSettings();
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public static async Task ShowStabilityHelp(Window window)
    {
        await MessageBox.Show(window, "Info", "The stability slider determines how stable the voice is and the randomness between each generation. Lowering this slider introduces a broader emotional range for the voice. As mentioned before, this is also influenced heavily by the original voice. Setting the slider too low may result in odd performances that are overly random and cause the character to speak too quickly. On the other hand, setting it too high can lead to a monotonous voice with limited emotion.", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static async Task ShowSimilarityHelp(Window window)
    {
        await MessageBox.Show(window, "Info", "The similarity slider dictates how closely the AI should adhere to the original voice when attempting to replicate it. If the original audio is of poor quality and the similarity slider is set too high, the AI may reproduce artifacts or background noise when trying to mimic the voice if those were present in the original recording.", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static async Task ShowSpeakerBoostHelp(Window window)
    {
        await MessageBox.Show(window, "Info", "Boosts the similarity to the original speaker. However, using this setting requires a slightly higher computational load, which in turn increases latency. The differences introduced by this setting are generally rather subtle.", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    public static async Task ShowSpeedHelp(Window window)
    {
        await MessageBox.Show(window, "Info", "Adjusts the speed of the generated speech. A value of 1.0 represents the normal speed. Values greater than 1.0 will make the speech faster, while values less than 1.0 will slow it down. Values from 0.7 to 1.2 are allowed.", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    public static async Task ShowStyleExaggerationHelp(Window window)
    {
        await MessageBox.Show(window, "Info", "Determines the style exaggeration of the voice. This setting attempts to amplify the style of the original speaker. It does consume additional computational resources and might increase latency if set to anything other than 0. Values from 0 to 1 are allowed.", MessageBoxButtons.OK, MessageBoxIcon.Information);
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