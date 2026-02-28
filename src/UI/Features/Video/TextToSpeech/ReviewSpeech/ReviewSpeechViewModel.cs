using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ElevenLabsSettingsViewModel = Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings.ElevenLabsSettingsViewModel;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

public partial class ReviewSpeechViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ITtsEngine> _engines;
    [ObservableProperty] private ITtsEngine? _selectedEngine;
    [ObservableProperty] private ObservableCollection<Voice> _voices;
    [ObservableProperty] private Voice? _selectedVoice;
    [ObservableProperty] private ObservableCollection<TtsLanguage> _languages;
    [ObservableProperty] private TtsLanguage? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<string> _regions;
    [ObservableProperty] private string? _selectedRegion;
    [ObservableProperty] private ObservableCollection<string> _models;
    [ObservableProperty] private string? _selectedModel;
    [ObservableProperty] private ObservableCollection<string> _styles;
    [ObservableProperty] private string? _selectedStyle;
    [ObservableProperty] private ObservableCollection<ReviewRow> _lines;
    [ObservableProperty] private ReviewRow? _selectedLine;
    [ObservableProperty] private bool _isRegenerateEnabled;
    [ObservableProperty] private bool _isElevenLabsControlsVisible;
    [ObservableProperty] private bool _autoContinue;
    [ObservableProperty] private bool _isPlayVisible;
    [ObservableProperty] private bool _isStopVisible;
    [ObservableProperty] private bool _isElevenLabsEngineV3Selected;
    [ObservableProperty] private double _stability;
    [ObservableProperty] private double _similarity;
    [ObservableProperty] private double _speakerBoost;
    [ObservableProperty] private double _speed;
    [ObservableProperty] private double _styleExaggeration;

    public Window? Window { get; set; }
    public DataGrid LineGrid { get; internal set; }
    public TtsStepResult[] StepResults { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;

    private LibMpvDynamicPlayer? _mpvContext;
    private Lock _playLock;
    private readonly Timer _timer;
    private string _videoFileName;
    private string _waveFolder;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    private bool _skipAutoContinue;
    private long _startPlayTicks;

    public ReviewSpeechViewModel(IFolderHelper folderHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _windowService = windowService;

        LineGrid = new DataGrid();
        Lines = new ObservableCollection<ReviewRow>();
        Engines = new ObservableCollection<ITtsEngine>();
        Voices = new ObservableCollection<Voice>();
        Languages = new ObservableCollection<TtsLanguage>();
        Regions = new ObservableCollection<string>();
        Models = new ObservableCollection<string>();
        Styles = new ObservableCollection<string>();
        StepResults = [];

        Stability = Se.Settings.Video.TextToSpeech.ElevenLabsStability;
        Similarity = Se.Settings.Video.TextToSpeech.ElevenLabsSimilarity;
        SpeakerBoost = Se.Settings.Video.TextToSpeech.ElevenLabsSpeakerBoost;
        Speed = Se.Settings.Video.TextToSpeech.ElevenLabsSpeed;
        StyleExaggeration = Se.Settings.Video.TextToSpeech.ElevenLabsStyleeExaggeration;

        IsPlayVisible = true;
        _videoFileName = string.Empty;
        _waveFolder = string.Empty;
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _playLock = new Lock();
        _timer = new Timer(200);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        _timer.Stop();

        if (_cancellationTokenSource.IsCancellationRequested || _mpvContext == null)
        {
            IsPlayVisible = true;
            IsStopVisible = false;
            foreach (var l in Lines)
            {
                l.IsPlaying = false;
                l.IsPlayingEnabled = true;
            }

            return;
        }

        var paused = _mpvContext.IsPaused;

        var line = SelectedLine;
        var timeSinceStart = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _startPlayTicks);
        if (paused && AutoContinue && !_skipAutoContinue && line != null && timeSinceStart.TotalMilliseconds > 500)
        {
            Dispatcher.UIThread.Invoke<Task>(async () =>
            {
                line.IsPlaying = false;
                var index = Lines.IndexOf(line);
                if (index < Lines.Count - 1)
                {
                    var nextLine = Lines[index + 1];
                    nextLine.IsPlaying = true;
                    SelectedLine = nextLine;
                    LineGrid.ScrollIntoView(nextLine, null);
                    await PlayAudio(nextLine.StepResult.CurrentFileName);
                }
                else
                {
                    _skipAutoContinue = true; // no more lines to play

                    IsPlayVisible = true;
                    IsStopVisible = false;
                    foreach (var l in Lines)
                    {
                        l.IsPlaying = false;
                        l.IsPlayingEnabled = true;
                    }
                }
            });

            return;
        }

        IsPlayVisible = paused;
        IsStopVisible = !paused;

        if (paused)
        {
            foreach (var l in Lines)
            {
                l.IsPlaying = false;
                l.IsPlayingEnabled = true;
            }
            return;
        }

        _timer.Start();
    }

    private async Task PlayAudio(string fileName)
    {
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();
            _mpvContext = new LibMpvDynamicPlayer();
        }

        await _mpvContext.LoadFile(fileName);

        _timer.Start();
    }

    internal void Initialize(
        TtsStepResult[] stepResults,
        ITtsEngine[] engines,
        ITtsEngine engine,
        Voice[] voices,
        Voice voice,
        TtsLanguage[] languages,
        TtsLanguage? language,
        string videoFileName,
        string waveFolder,
        WavePeakData2? wavePeakData)
    {
        foreach (var p in stepResults)
        {
            var row = new ReviewRow
            {
                Include = true,
                Number = p.Paragraph.Number,
                Text = p.Text,
                Voice = p.Voice == null ? string.Empty : p.Voice.ToString(),
                Speed = Math.Round(p.SpeedFactor, 2).ToString(CultureInfo.CurrentCulture),
                Cps = Math.Round(p.Paragraph.GetCharactersPerSecond(), 2).ToString(CultureInfo.CurrentCulture),
                StepResult = p,
            };
            row.StartHistory();
            Lines.Add(row);
        }

        foreach (var engineItem in engines)
        {
            if (engineItem is ElevenLabs && string.IsNullOrWhiteSpace(Se.Settings.Video.TextToSpeech.ElevenLabsApiKey))
            {
                continue;
            }

            if (engineItem is Murf && string.IsNullOrWhiteSpace(Se.Settings.Video.TextToSpeech.MurfApiKey))
            {
                continue;
            }

            if (engineItem is AzureSpeech && string.IsNullOrWhiteSpace(Se.Settings.Video.TextToSpeech.AzureApiKey))
            {
                continue;
            }

            if (engineItem is GoogleSpeech && string.IsNullOrWhiteSpace(Se.Settings.Video.TextToSpeech.GoogleKeyFile))
            {
                continue;
            }

            Engines.Add(engineItem);
        }

        SelectedEngine = engine;

        ObservableCollectionExtensions.AddRange(Voices, voices);
        SelectedVoice = voice;

        ObservableCollectionExtensions.AddRange(Languages, languages);
        SelectedLanguage = language;

        _videoFileName = videoFileName;
        _waveFolder = waveFolder;

        if (Lines.Count > 0)
        {
            SelectedLine = Lines[0];
            LineGrid.SelectedIndex = 0;
            LineGrid.ScrollIntoView(LineGrid.SelectedItem, null);
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var folder = await _folderHelper.PickFolderAsync(Window!, Se.Language.General.SelectSaveFolder);
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        var jsonFileName = Path.Combine(folder, "SubtitleEditTts.json");

        // ask if overwrite if jsonFileName exists
        if (File.Exists(jsonFileName))
        {
            var answer = await MessageBox.Show(
                Window,
                Se.Language.General.OverwriteQuestion,
                string.Format(Se.Language.General.OverwriteFilesInFolderX, folder),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                File.Delete(jsonFileName);
            }
            catch (Exception e)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    $"Could not overwrite the file \"{jsonFileName}" + Environment.NewLine + e.Message,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        }

        // Copy files
        var index = 0;
        var exportFormat = new TtsImportExport { VideoFileName = _videoFileName };
        foreach (var line in Lines)
        {
            index++;
            var sourceFileName = line.StepResult.CurrentFileName;
            var targetFileName = Path.Combine(folder, index.ToString().PadLeft(4, '0') + Path.GetExtension((string?)sourceFileName));

            if (File.Exists(targetFileName))
            {
                try
                {
                    File.Delete(targetFileName);
                }
                catch (Exception e)
                {
                    await MessageBox.Show(
                        Window,
                        Se.Language.General.Error,
                        $"Could not overwrite the file \"{targetFileName}" + Environment.NewLine + e.Message,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            File.Copy(sourceFileName, targetFileName, true);

            exportFormat.Items.Add(new TtsImportExportItem
            {
                AudioFileName = targetFileName,
                StartMs = (long)Math.Round((double)line.StepResult.Paragraph.StartTime.TotalMilliseconds, MidpointRounding.AwayFromZero),
                EndMs = (long)Math.Round((double)line.StepResult.Paragraph.EndTime.TotalMilliseconds, MidpointRounding.AwayFromZero),
                VoiceName = line.StepResult.Voice?.Name ?? string.Empty,
                EngineName = SelectedEngine != null ? SelectedEngine.ToString() : string.Empty,
                SpeedFactor = line.StepResult.SpeedFactor,
                Text = line.Text,
                Include = line.Include,
            });
        }

        // Export json
        var json = JsonSerializer.Serialize(exportFormat, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonFileName, json);

        await _folderHelper.OpenFolder(Window!, folder);
    }

    [RelayCommand]
    private void ElevenLabsReset()
    {
        var settings = new SeVideoTextToSpeech();
        Stability = settings.ElevenLabsStability;
        Similarity = settings.ElevenLabsSimilarity;
        SpeakerBoost = settings.ElevenLabsSpeakerBoost;
        Speed = settings.ElevenLabsSpeed;
        StyleExaggeration = settings.ElevenLabsStyleeExaggeration;
    }

    [RelayCommand]
    private async Task ShowHistory(ReviewRow? line)
    {
        if (Window == null || line == null)
        {
            return;
        }

        var result =
            await _windowService.ShowDialogAsync<ReviewSpeechHistoryWindow, ReviewSpeechHistoryViewModel>(Window!,
                vm => vm.Initialize(line));
        if (!result.OkPressed || result.SelectedHistoryItem == null)
        {
            return;
        }

        line.Voice = result.SelectedHistoryItem?.Voice?.Name ?? string.Empty;
        line.StepResult.CurrentFileName = result.SelectedHistoryItem?.FileName ?? string.Empty;
        line.StepResult.Voice = result.SelectedHistoryItem?.Voice;
        line.Speed = Math.Round(result.SelectedHistoryItem?.Speed ?? 1.0f, 2).ToString(CultureInfo.CurrentCulture);
    }

    [RelayCommand]
    private async Task ShowElevenLabsEngineV3Help()
    {
        if (Window == null)
        {
            return;
        }

        await Window.Launcher.LaunchUriAsync(new Uri("https://elevenlabs.io/blog/eleven-v3-audio-tags-expressing-emotional-context-in-speech"));
    }

    [RelayCommand]
    private async Task ShowStabilityHelp()
    {
        await ElevenLabsSettingsViewModel.ShowStabilityHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowSimilarityHelp()
    {
        await ElevenLabsSettingsViewModel.ShowSimilarityHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowSpeakerBoostHelp()
    {
        await ElevenLabsSettingsViewModel.ShowSpeakerBoostHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowSpeedHelp()
    {
        await ElevenLabsSettingsViewModel.ShowSpeedHelp(Window!);
    }

    [RelayCommand]
    private async Task ShowStyleExaggerationHelp()
    {
        await ElevenLabsSettingsViewModel.ShowStyleExaggerationHelp(Window!);
    }

    [RelayCommand]
    private async Task RegenerateAudio(ReviewRow? row)
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        if (engine is ElevenLabs)
        {
            var settings = Se.Settings.Video.TextToSpeech;

            settings.ElevenLabsStability = Stability;
            settings.ElevenLabsSimilarity = Similarity;
            settings.ElevenLabsSpeakerBoost = SpeakerBoost;
            settings.ElevenLabsSpeed = Speed;
            settings.ElevenLabsStyleeExaggeration = StyleExaggeration;
        }

        var voice = SelectedVoice;
        var line = row ?? SelectedLine;
        if (engine == null || voice == null || line == null)
        {
            return;
        }

        var isEngineInstalled = await engine.IsInstalled(SelectedRegion);
        if (!isEngineInstalled)
        {
            return;
        }

        IsRegenerateEnabled = false;
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        if (!engine.IsVoiceInstalled(voice) && voice.EngineVoice is PiperVoice piperVoice)
        {
            var modelFileName = Path.Combine(Piper.GetSetPiperFolder(), piperVoice.ModelShort);
            var configFileName = Path.Combine(Piper.GetSetPiperFolder(), piperVoice.ConfigShort);
            if (!File.Exists(modelFileName) || !File.Exists(configFileName))
            {
                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadPiperVoice(piperVoice));
                if (!dlResult.OkPressed)
                {
                    SafeDelete(modelFileName);
                    SafeDelete(configFileName);
                    return;
                }
            }
        }

        var oldStyle = SelectedStyle;
        if (engine is Murf && !string.IsNullOrEmpty(SelectedStyle))
        {
            Se.Settings.Video.TextToSpeech.MurfStyle = SelectedStyle;
        }

        var speakResult = await engine.Speak(line.Text, _waveFolder, voice, SelectedLanguage, SelectedRegion, SelectedModel, _cancellationToken);

        line.AddHistory(voice, speakResult);

        line.StepResult.CurrentFileName = speakResult.FileName;
        line.StepResult.Voice = voice;

        var adjustSpeedStepResult = await TrimAndAdjustSpeed(line);
        line.Speed = Math.Round(adjustSpeedStepResult.SpeedFactor, 2).ToString(CultureInfo.CurrentCulture);
        line.Cps = Math.Round(adjustSpeedStepResult.Paragraph.GetCharactersPerSecond(), 2).ToString(CultureInfo.CurrentCulture);
        line.StepResult = adjustSpeedStepResult;
        line.Voice = voice.ToString();

        _skipAutoContinue = true;
        await PlayAudio(line.StepResult.CurrentFileName);

        IsRegenerateEnabled = true;

        if (engine is Murf && oldStyle != null)
        {
            Se.Settings.Video.TextToSpeech.MurfStyle = oldStyle;
        }
    }

    private static void SafeDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // ignore
        }
    }

    [RelayCommand]
    private async Task PlayRow(ReviewRow? line)
    {
        if (line == null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        _skipAutoContinue = false;
        _startPlayTicks = DateTime.UtcNow.Ticks;

        line.IsPlaying = true;
        foreach (var l in Lines)
        {
            l.IsPlayingEnabled = false;
        }

        await PlayAudio(line.StepResult.CurrentFileName);
    }


    [RelayCommand]
    private async Task Play()
    {
        var line = SelectedLine;
        if (line == null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        _skipAutoContinue = false;
        _startPlayTicks = DateTime.UtcNow.Ticks;
        await PlayAudio(line.StepResult.CurrentFileName);
    }

    [RelayCommand]
    private void Stop()
    {
        _skipAutoContinue = true;
        _cancellationTokenSource.Cancel();
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();
            _mpvContext = null;
        }

        IsPlayVisible = true;
        IsStopVisible = false;
    }

    [RelayCommand]
    private void Ok()
    {
        // set StepResults with the current lines
        var stepResults = new List<TtsStepResult>();
        foreach (var row in Lines)
        {
            var stepResult = row.StepResult;
            stepResult.Text = row.Text;
            stepResults.Add(stepResult);
        }

        StepResults = stepResults.ToArray();

        foreach (var p in stepResults)
        {
            Lines.Add(new ReviewRow
            {
                Include = true,
                Number = p.Paragraph.Number,
                Text = p.Text,
                Voice = p.Voice == null ? string.Empty : p.Voice.ToString(),
                Speed = Math.Round(p.SpeedFactor, 2).ToString(CultureInfo.CurrentCulture),
                Cps = Math.Round(p.Paragraph.GetCharactersPerSecond(), 2).ToString(CultureInfo.CurrentCulture),
                StepResult = p
            });
        }

        StepResults = Enumerable.Where<ReviewRow>(Lines, p => p.Include).Select(p => p.StepResult).ToArray();


        Se.SaveSettings();
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Invoke(() => { Window?.Close(); });
    }

    private async Task<TtsStepResult> TrimAndAdjustSpeed(ReviewRow row)
    {
        var item = row.StepResult;
        var p = item.Paragraph;
        var index = Lines.IndexOf(row);
        var next = index + 1 < Lines.Count ? Lines[index + 1] : null;
        var outputFileNameTrim = Path.Combine(_waveFolder, Guid.NewGuid() + ".wav");
        var trimProcess = FfmpegGenerator.TrimSilenceStartAndEnd(item.CurrentFileName, outputFileNameTrim);
#pragma warning disable CA1416 // Validate platform compatibility
        _ = trimProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        await trimProcess.WaitForExitAsync(_cancellationToken);

        var addDuration = 0d;
        if (next != null && p.EndTime.TotalMilliseconds < next.StepResult.Paragraph.StartTime.TotalMilliseconds)
        {
            var diff = next.StepResult.Paragraph.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
            addDuration = Math.Min(1000, diff);
            if (addDuration < 0)
            {
                addDuration = 0;
            }
        }

        var mediaInfo = FfmpegMediaInfo.Parse(outputFileNameTrim);
        if (mediaInfo.Duration.TotalMilliseconds <= p.DurationTotalMilliseconds + addDuration)
        {
            return new TtsStepResult
            {
                Paragraph = p,
                Text = item.Text,
                CurrentFileName = outputFileNameTrim,
                SpeedFactor = 1.0f,
                Voice = item.Voice,
            };
        }

        var divisor = (decimal)(p.DurationTotalMilliseconds + addDuration);
        if (divisor <= 0)
        {
            return new TtsStepResult
            {
                Paragraph = p,
                Text = item.Text,
                CurrentFileName = item.CurrentFileName,
                SpeedFactor = 1.0f,
                Voice = item.Voice,
            };
        }

        var ext = ".wav";
        var factor = (decimal)mediaInfo.Duration.TotalMilliseconds / divisor;
        var outputFileName2 = Path.Combine(_waveFolder, $"{index}_{Guid.NewGuid()}{ext}");
        var overrideFileName = string.Empty;
        if (!string.IsNullOrEmpty(overrideFileName) && File.Exists(Path.Combine(_waveFolder, overrideFileName)))
        {
            outputFileName2 = Path.Combine(_waveFolder, $"{Path.GetFileNameWithoutExtension(overrideFileName)}_{Guid.NewGuid()}{ext}");
        }

        var mergeProcess = FfmpegGenerator.ChangeSpeed(outputFileNameTrim, outputFileName2, (float)factor);
#pragma warning disable CA1416 // Validate platform compatibility
        _ = mergeProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        await mergeProcess.WaitForExitAsync(_cancellationToken);

        return new TtsStepResult
        {
            Paragraph = p,
            Text = item.Text,
            CurrentFileName = outputFileName2,
            SpeedFactor = (float)factor,
            Voice = item.Voice,
        };
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
            UiUtil.ShowHelp("features/text-to-speech");
        }
        else if (e.Key == Key.R && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            var line = SelectedLine;
            if (line == null || line.IsPlaying || !line.IsPlayingEnabled)
            {
                return;
            }

            RegenerateAudio(SelectedLine).ConfigureAwait(false);
        }
    }

    internal void SelectedEngineChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedEngineChanged();
    }

    public void SelectedEngineChanged()
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var voices = await engine.GetVoices(SelectedLanguage?.Code ?? string.Empty);
            Voices.Clear();
            foreach (var vo in voices)
            {
                Voices.Add(vo);
            }

            var lastVoice = Enumerable.FirstOrDefault<Voice>(Voices, v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
            if (lastVoice == null)
            {
                lastVoice = Enumerable.FirstOrDefault<Voice>(Voices, p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                                          p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            }

            SelectedVoice = lastVoice ?? Enumerable.First<Voice>(Voices);

            if (engine.HasLanguageParameter)
            {
                var languages = await engine.GetLanguages(SelectedVoice, null); // SelectedModel);
                Languages.Clear();
                foreach (var language in languages)
                {
                    Languages.Add(language);
                }

                SelectedLanguage = Enumerable.FirstOrDefault<TtsLanguage>(Languages);
            }

            if (engine.HasRegion)
            {
                var regions = await engine.GetRegions();
                Regions.Clear();
                foreach (var region in regions)
                {
                    Regions.Add(region);
                }

                SelectedRegion = Enumerable.FirstOrDefault<string>(Regions);
            }

            if (engine.HasModel)
            {
                var models = await engine.GetModels();
                Models.Clear();
                foreach (var model in models)
                {
                    Models.Add(model);
                }

                SelectedModel = Enumerable.FirstOrDefault<string>(Models);
            }

            IsElevenLabsControlsVisible = false;
            if (engine is AzureSpeech)
            {
                SelectedRegion = Se.Settings.Video.TextToSpeech.AzureRegion;
                if (string.IsNullOrEmpty(SelectedRegion))
                {
                    SelectedRegion = "westeurope";
                }
            }
            else if (engine is ElevenLabs)
            {
                IsElevenLabsControlsVisible = true;
                SelectedModel = Se.Settings.Video.TextToSpeech.ElevenLabsModel;
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Enumerable.First<string>(Models);
                }
            }
        });
    }

    internal void SelectedLanguageChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        if (engine is Murf murf)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var voices = await murf.GetVoices(SelectedLanguage?.Code ?? string.Empty);
                Voices.Clear();
                ObservableCollectionExtensions.AddRange(Voices, voices);

                var lastVoice = Enumerable.FirstOrDefault<Voice>(Voices, v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
                if (lastVoice == null)
                {
                    lastVoice = Enumerable.FirstOrDefault<Voice>(Voices, p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                                              p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
                }

                SelectedVoice = lastVoice ?? Enumerable.First<Voice>(Voices);
            });
        }
    }

    internal void SelectedModelChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;
        var voice = SelectedVoice;
        var model = SelectedModel;
        IsElevenLabsEngineV3Selected = false;
        if (engine == null || voice == null || model == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            if (engine is ElevenLabs && model == "eleven_v3")
            {
                IsElevenLabsEngineV3Selected = true;
            }

            if (engine.HasLanguageParameter)
            {
                var languages = await engine.GetLanguages(voice, model);
                Languages.Clear();
                foreach (var language in languages)
                {
                    Languages.Add(language);
                }

                SelectedLanguage = Enumerable.FirstOrDefault<TtsLanguage>(Languages, p => p.Name == Se.Settings.Video.TextToSpeech.ElevenLabsLanguage);
                if (SelectedLanguage == null)
                {
                    SelectedLanguage = Enumerable.FirstOrDefault<TtsLanguage>(Languages, p => p.Code == "en");
                }
            }
        });
    }

    internal void OnClosing(WindowClosingEventArgs e)
    {
        _skipAutoContinue = true;
        _timer.Stop();
        _cancellationTokenSource.Cancel();
        lock (_playLock)
        {
            _mpvContext?.Dispose();
            _mpvContext = null;
        }

        UiUtil.SaveWindowPosition(Window);
    }

    internal void DataGridDoubleClicked()
    {
        var line = SelectedLine;
        if (line == null || line.IsPlaying || !line.IsPlayingEnabled)
        {
            return;
        }

        _ = PlayRow(line);
    }

    internal void Loaded()
    {
        UiUtil.RestoreWindowPosition(Window);
    }
}