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
    [ObservableProperty] private bool _isPlayWithSpeech;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _hint;
    [ObservableProperty] private bool _lowerVideoVolume;
    [ObservableProperty] private bool _pauseVideoWhenLate;

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
        _title = Se.Language.Video.TextToSpeech.SpeakFromCurrentLineTitle;
        _hint = Se.Language.Video.TextToSpeech.SpeakFromCurrentLineHint;

        var settings = Se.Settings.Video.TextToSpeech;
        _lowerVideoVolume = settings.PlayWithSpeechLowerVideoVolume;
        _pauseVideoWhenLate = settings.PlayWithSpeechPauseVideoWhenLate;
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

    /// <summary>Turns the dialog into the "Play with speech" prompt, which adds its playback options.</summary>
    public void InitializePlayWithSpeech()
    {
        IsPlayWithSpeech = true;
        Title = Se.Language.Video.TextToSpeech.PlayWithSpeechTitle;
        Hint = Se.Language.Video.TextToSpeech.PlayWithSpeechHint;
    }

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

    /// <param name="preferredVoiceName">The voice to keep selected (by name) when the list is
    /// reloaded after an engine download; null picks the saved voice.</param>
    private async Task LoadEngine(ITtsEngine engine, string? preferredVoiceName = null)
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

        SelectedVoice = PickVoice(Voices, preferredVoiceName ?? Se.Settings.Video.TextToSpeech.SpeakFromLineVoice);
    }

    /// <summary>The voice named <paramref name="name"/>, else an English voice, else the first.</summary>
    internal static Voice? PickVoice(Collection<Voice> voices, string? name)
    {
        return voices.FirstOrDefault(v => v.Name == name)
               ?? voices.FirstOrDefault(v => v.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                             v.Name.Contains("English", StringComparison.OrdinalIgnoreCase))
               ?? voices.FirstOrDefault();
    }

    /// <param name="preferredLanguageName">The language to keep selected (by name); null keeps
    /// the current pick, or the saved one.</param>
    private async Task LoadLanguages(ITtsEngine engine, Voice voice, string? preferredLanguageName = null)
    {
        var version = _engineLoadVersion;
        var languages = await engine.GetLanguages(voice, null);
        if (version != _engineLoadVersion || !ReferenceEquals(SelectedVoice, voice))
        {
            return;
        }

        var previous = preferredLanguageName ?? SelectedLanguage?.Name ?? Se.Settings.Video.TextToSpeech.SpeakFromLineLanguage;
        Languages.Clear();
        foreach (var language in languages)
        {
            Languages.Add(language);
        }

        SelectedLanguage = Languages.FirstOrDefault(l => l.Name == previous) ?? Languages.FirstOrDefault();
    }

    /// <summary>
    /// Reloads the voices after an engine download, keeping the voice and language the user
    /// picked before clicking OK - a plain reload would fall back to the saved voice.
    /// </summary>
    private async Task ReloadEngineKeepingSelection(ITtsEngine engine, string? voiceName, string? languageName)
    {
        await LoadEngine(engine, voiceName);
        await EnsureLanguageLoaded(engine, languageName);
    }

    /// <summary>
    /// Loads the language list now when the engine needs one and it is not there yet - the list
    /// otherwise loads in a posted callback that may run after the dialog has closed.
    /// </summary>
    private async Task EnsureLanguageLoaded(ITtsEngine engine, string? languageName)
    {
        var voice = SelectedVoice;
        if (!engine.HasLanguageParameter || voice == null)
        {
            return;
        }

        if (SelectedLanguage == null || (languageName != null && SelectedLanguage.Name != languageName))
        {
            await LoadLanguages(engine, voice, languageName);
        }
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
            // half-way for an install prompt. The voice list is reloaded after a download, so
            // remember the user's pick by name.
            var voiceName = voice.Name;
            var languageName = SelectedLanguage?.Name;
            if (!await TtsEngineInstaller.EnsureEngineInstalled(engine, Window, _windowService, null, null, null, null,
                    () => ReloadEngineKeepingSelection(engine, voiceName, languageName)))
            {
                return;
            }

            voice = SelectedVoice ?? voice;
            if (!await TtsVoiceInstaller.EnsureVoiceInstalled(engine, voice, Window, _windowService))
            {
                return;
            }

            await EnsureLanguageLoaded(engine, languageName);

            var settings = Se.Settings.Video.TextToSpeech;
            settings.SpeakFromLineEngine = engine.Name;
            settings.SpeakFromLineVoice = voice.Name;
            if (SelectedLanguage != null)
            {
                settings.SpeakFromLineLanguage = SelectedLanguage.Name;
            }

            if (IsPlayWithSpeech)
            {
                settings.PlayWithSpeechLowerVideoVolume = LowerVideoVolume;
                settings.PlayWithSpeechPauseVideoWhenLate = PauseVideoWhenLate;
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
