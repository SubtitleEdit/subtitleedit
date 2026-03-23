using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AdvancedTtsSettings;

public partial class AdvancedTtsSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _doProAudioChain;
    [ObservableProperty] private bool _doAudioDucking;
    [ObservableProperty] private string _audioDuckingVolume;
    [ObservableProperty] private bool _doVadSilenceCompression;
    [ObservableProperty] private string _vadMaxSilenceMs;
    [ObservableProperty] private bool _doHighQualityTimeStretch;
    [ObservableProperty] private string _silencePaddingMs;
    [ObservableProperty] private string _outputSampleRate;
    [ObservableProperty] private string _edgeTtsRate;
    [ObservableProperty] private string _edgeTtsPitch;
    [ObservableProperty] private string _edgeTtsVolume;
    [ObservableProperty] private bool _isEdgeTtsEngine;
    [ObservableProperty] private string _rubberbandStatus;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public AdvancedTtsSettingsViewModel()
    {
        RubberbandStatus = FfmpegGenerator.IsRubberbandAvailable() ? "(installed)" : "(not found in FFmpeg)";
        var s = Se.Settings.Video.TextToSpeech;
        DoProAudioChain = s.ProAudioChainEnabled;
        DoAudioDucking = s.AudioDuckingEnabled;
        AudioDuckingVolume = s.AudioDuckingOriginalVolume.ToString();
        DoVadSilenceCompression = s.VadSilenceCompressionEnabled;
        VadMaxSilenceMs = ((int)(s.VadMaxSilenceSeconds * 1000)).ToString();
        DoHighQualityTimeStretch = s.HighQualityTimeStretchEnabled;
        SilencePaddingMs = s.SilencePaddingMs.ToString();
        OutputSampleRate = s.OutputSampleRate.ToString();
        EdgeTtsRate = s.EdgeTtsRate;
        EdgeTtsPitch = s.EdgeTtsPitch;
        EdgeTtsVolume = s.EdgeTtsVolume;
    }

    [RelayCommand]
    private void Ok()
    {
        var s = Se.Settings.Video.TextToSpeech;
        s.ProAudioChainEnabled = DoProAudioChain;
        s.AudioDuckingEnabled = DoAudioDucking;
        s.AudioDuckingOriginalVolume = int.TryParse(AudioDuckingVolume, out var dv) ? dv : 15;
        s.VadSilenceCompressionEnabled = DoVadSilenceCompression;
        s.VadMaxSilenceSeconds = int.TryParse(VadMaxSilenceMs, out var vadMs) ? vadMs / 1000.0 : 0.15;
        s.HighQualityTimeStretchEnabled = DoHighQualityTimeStretch;
        s.SilencePaddingMs = int.TryParse(SilencePaddingMs, out var sp) ? sp : 0;
        s.OutputSampleRate = int.TryParse(OutputSampleRate, out var sr) ? sr : 0;
        s.EdgeTtsRate = EdgeTtsRate;
        s.EdgeTtsPitch = EdgeTtsPitch;
        s.EdgeTtsVolume = EdgeTtsVolume;
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
    }
}
