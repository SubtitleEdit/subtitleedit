using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;

/// <summary>
/// Picks the engine and voice for "Speak from current line". Only engines that answer within
/// about a second per line are offered - the reader speaks line after line while the next one is
/// generated, so a cloning engine taking several seconds per line would leave long silences.
/// </summary>
public partial class SpeakFromLineViewModel : ObservableObject
{
    [ObservableProperty] private ITtsEngine? _selectedEngine;
    [ObservableProperty] private Voice? _selectedVoice;
    [ObservableProperty] private TtsLanguage? _selectedLanguage;
    [ObservableProperty] private bool _hasLanguageParameter;
    [ObservableProperty] private string _engineDescription;
    [ObservableProperty] private bool _isBusy;

    public ObservableCollection<ITtsEngine> Engines { get; }
    public ObservableCollection<Voice> Voices { get; }
    public ObservableCollection<TtsLanguage> Languages { get; }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IWindowService _windowService;
    private int _engineLoadVersion;

    public SpeakFromLineViewModel(ITtsDownloadService ttsDownloadService, IWindowService windowService)
    {
        _windowService = windowService;
        Engines = new ObservableCollection<ITtsEngine>(TtsEngineCatalog.CreateAll(ttsDownloadService).Where(IsFastEngine));
        Voices = new ObservableCollection<Voice>();
        Languages = new ObservableCollection<TtsLanguage>();
        _engineDescription = string.Empty;

        var settings = Se.Settings.Video.TextToSpeech;
        // Supertonic by default: the fastest local engine, and one download covers 31 languages.
        SelectedEngine = Engines.FirstOrDefault(e => e.Name == settings.SpeakFromLineEngine)
                         ?? Engines.FirstOrDefault(e => e is SupertonicCrispAsr)
                         ?? Engines.FirstOrDefault();
    }

    /// <summary>
    /// The engines fast enough to keep up with speech: Piper, Kokoro and Supertonic run locally
    /// with a real-time factor far below one, and Edge TTS is a free cloud service that answers
    /// in about a second.
    /// </summary>
    public static bool IsFastEngine(ITtsEngine engine) =>
        engine is Piper or EdgeTts or KokoroTtsCpp or SupertonicCrispAsr;

    partial void OnSelectedEngineChanged(ITtsEngine? value)
    {
        if (value != null)
        {
            Dispatcher.UIThread.Post(async () => await LoadEngine(value));
        }
    }

    partial void OnSelectedVoiceChanged(Voice? value)
    {
        var engine = SelectedEngine;
        if (engine != null && value != null && engine.HasLanguageParameter)
        {
            Dispatcher.UIThread.Post(async () => await LoadLanguages(engine, value));
        }
    }

    private async Task LoadEngine(ITtsEngine engine)
    {
        var version = ++_engineLoadVersion;
        HasLanguageParameter = engine.HasLanguageParameter;
        EngineDescription = engine.Description;

        Voice[] voices;
        try
        {
            voices = await engine.GetVoices(string.Empty);
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Speak from current line: loading voices for {engine.Name} failed");
            voices = [];
        }

        if (version != _engineLoadVersion)
        {
            return; // another engine was picked while this one's voices loaded
        }

        Voices.Clear();
        foreach (var voice in voices)
        {
            Voices.Add(voice);
        }

        Languages.Clear();
        SelectedLanguage = null;

        var settings = Se.Settings.Video.TextToSpeech;
        SelectedVoice = Voices.FirstOrDefault(v => v.Name == settings.SpeakFromLineVoice)
                        ?? Voices.FirstOrDefault(v => v.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                      v.Name.Contains("English", StringComparison.OrdinalIgnoreCase))
                        ?? Voices.FirstOrDefault();
    }

    private async Task LoadLanguages(ITtsEngine engine, Voice voice)
    {
        var version = _engineLoadVersion;
        var languages = await engine.GetLanguages(voice, null);
        if (version != _engineLoadVersion || !ReferenceEquals(SelectedVoice, voice))
        {
            return;
        }

        var previous = SelectedLanguage?.Name ?? Se.Settings.Video.TextToSpeech.SpeakFromLineLanguage;
        Languages.Clear();
        foreach (var language in languages)
        {
            Languages.Add(language);
        }

        SelectedLanguage = Languages.FirstOrDefault(l => l.Name == previous) ?? Languages.FirstOrDefault();
    }

    [RelayCommand]
    private async Task Ok()
    {
        var engine = SelectedEngine;
        var voice = SelectedVoice;
        if (engine == null || voice == null || Window == null || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            // Downloads happen here, with this window as owner, so the reader never has to stop
            // half-way for an install prompt.
            if (!await TtsEngineInstaller.EnsureEngineInstalled(engine, Window, _windowService, null, null, null, null, () => LoadEngine(engine)))
            {
                return;
            }

            voice = SelectedVoice ?? voice;
            if (!await TtsVoiceInstaller.EnsureVoiceInstalled(engine, voice, Window, _windowService))
            {
                return;
            }

            var settings = Se.Settings.Video.TextToSpeech;
            settings.SpeakFromLineEngine = engine.Name;
            settings.SpeakFromLineVoice = voice.Name;
            if (SelectedLanguage != null)
            {
                settings.SpeakFromLineLanguage = SelectedLanguage.Name;
            }

            OkPressed = true;
            Window.Close();
        }
        finally
        {
            IsBusy = false;
        }
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
