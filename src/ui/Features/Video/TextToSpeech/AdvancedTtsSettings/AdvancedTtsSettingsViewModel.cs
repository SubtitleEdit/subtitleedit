using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AdvancedTtsSettings;

public partial class AdvancedTtsSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _doProAudioChain;
    [ObservableProperty] private bool _doAudioDucking;
    [ObservableProperty] private int _audioDuckingVolume;
    [ObservableProperty] private bool _doVadSilenceCompression;
    [ObservableProperty] private int _vadMaxSilenceMs;
    [ObservableProperty] private bool _doHighQualityTimeStretch;
    [ObservableProperty] private int _silencePaddingMs;
    [ObservableProperty] private int _outputSampleRate;
    [ObservableProperty] private string _generationFolder;
    [ObservableProperty] private bool _doDeleteTempFiles;
    [ObservableProperty] private string _edgeTtsRate;
    [ObservableProperty] private string _edgeTtsPitch;
    [ObservableProperty] private string _edgeTtsVolume;
    [ObservableProperty] private bool _isEdgeTtsEngine;
    [ObservableProperty] private string _rubberbandStatus;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;

    public AdvancedTtsSettingsViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
        RubberbandStatus = FfmpegGenerator.IsRubberbandAvailable()
            ? Se.Language.Video.TextToSpeech.RubberbandInstalled
            : Se.Language.Video.TextToSpeech.RubberbandNotFound;
        var s = Se.Settings.Video.TextToSpeech;
        DoProAudioChain = s.ProAudioChainEnabled;
        DoAudioDucking = s.AudioDuckingEnabled;
        AudioDuckingVolume = s.AudioDuckingOriginalVolume;
        DoVadSilenceCompression = s.VadSilenceCompressionEnabled;
        VadMaxSilenceMs = (int)Math.Round(s.VadMaxSilenceSeconds * 1000);
        DoHighQualityTimeStretch = s.HighQualityTimeStretchEnabled;
        SilencePaddingMs = s.SilencePaddingMs;
        OutputSampleRate = s.OutputSampleRate;
        EdgeTtsRate = s.EdgeTtsRate;
        EdgeTtsPitch = s.EdgeTtsPitch;
        EdgeTtsVolume = s.EdgeTtsVolume;
        GenerationFolder = s.GenerationFolder ?? string.Empty;
        DoDeleteTempFiles = s.DeleteTempFiles;
    }

    [RelayCommand]
    private async Task BrowseGenerationFolder()
    {
        var folder = await _folderHelper.PickFolderAsync(Window!, Se.Language.Video.TextToSpeech.GenerationFolder);
        if (!string.IsNullOrEmpty(folder))
        {
            GenerationFolder = folder;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        var s = Se.Settings.Video.TextToSpeech;
        s.ProAudioChainEnabled = DoProAudioChain;
        s.AudioDuckingEnabled = DoAudioDucking;
        s.AudioDuckingOriginalVolume = AudioDuckingVolume;
        s.VadSilenceCompressionEnabled = DoVadSilenceCompression;
        s.VadMaxSilenceSeconds = VadMaxSilenceMs / 1000.0;
        s.HighQualityTimeStretchEnabled = DoHighQualityTimeStretch;
        s.SilencePaddingMs = SilencePaddingMs;
        s.OutputSampleRate = OutputSampleRate;
        s.EdgeTtsRate = EdgeTts.NormalizeProsodyValue(EdgeTtsRate, "%");
        s.EdgeTtsPitch = EdgeTts.NormalizeProsodyValue(EdgeTtsPitch, "Hz");
        s.EdgeTtsVolume = EdgeTts.NormalizeProsodyValue(EdgeTtsVolume, "%");
        s.GenerationFolder = GenerationFolder?.Trim() ?? string.Empty;
        s.DeleteTempFiles = DoDeleteTempFiles;
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
