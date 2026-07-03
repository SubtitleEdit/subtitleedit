using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;
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
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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

    // Voice-design controls mirrored from the main TTS window. Visibility is engine+model+voice
    // driven so the picker doesn't appear for engines that don't use it.
    [ObservableProperty] private bool _hasInstruction;
    [ObservableProperty] private bool _isInstructionTextVisible;
    [ObservableProperty] private bool _isInstructionPickerVisible;
    [ObservableProperty] private bool _isInstructionPickerEnabled;
    [ObservableProperty] private bool _isInstructionVoiceHintVisible;
    [ObservableProperty] private string _instruction = string.Empty;
    [ObservableProperty] private string _selectedOmniVoiceGender = OmniVoiceAny;
    [ObservableProperty] private string _selectedOmniVoiceAge = OmniVoiceAny;
    [ObservableProperty] private string _selectedOmniVoicePitch = OmniVoiceAny;
    [ObservableProperty] private string _selectedOmniVoiceAccent = OmniVoiceAny;
    [ObservableProperty] private bool _omniVoiceWhisper;

    public ObservableCollection<string> OmniVoiceGenders { get; } = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionGenders);
    public ObservableCollection<string> OmniVoiceAges { get; } = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionAges);
    public ObservableCollection<string> OmniVoicePitches { get; } = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionPitches);
    public ObservableCollection<string> OmniVoiceAccents { get; } = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionAccents);

    private const string OmniVoiceAny = "(any)";
    private bool _suppressKeywordSync;

    public Window? Window { get; set; }
    public DataGrid LineGrid { get; internal set; }
    public AudioVisualizer? AudioVisualizer { get; set; }
    public TtsStepResult[] StepResults { get; set; }

    // Source-of-truth peaks (of the original video audio) for the waveform shown next to the
    // review grid. May be null when no video is loaded — the visualizer then sits idle.
    [ObservableProperty] private WavePeakData2? _wavePeakData;

    // Each ReviewRow's paragraph projected as a SubtitleLineViewModel so AudioVisualizer drag
    // logic (which writes to SubtitleLineViewModel.StartTime/EndTime) works unchanged. The
    // canonical link is ReviewRow.WaveformParagraph (set in Initialize); this list is the
    // sorted-by-start-time view the visualizer needs, and the dictionary is the reverse lookup
    // used by OnWaveformParagraphChanged to find the row that owns a mirror VM.
    public List<SubtitleLineViewModel> WaveformParagraphs { get; } = new();
    private readonly Dictionary<SubtitleLineViewModel, ReviewRow> _waveformParagraphToRow = new();

    // Cast travelling with the audio. Populated by the main TTS VM via SetActorVoiceMappings;
    // round-tripped through SubtitleEditTts.json so a future Import re-applies the same voices.
    public List<ActorVoiceMapping> ActorVoiceMappings { get; private set; } = new();

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
    private readonly List<string> _tempAudioFiles = new();

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

    private async void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        try
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
                await Dispatcher.UIThread.InvokeAsync(async () =>
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
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Error in ReviewSpeech playback timer.");
        }
    }

    private async Task PlayAudio(string fileName)
    {
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();

            _mpvContext = new LibMpvDynamicPlayer();
            _mpvContext.LoadLib(); 
            var err = _mpvContext.Initialize();
            if (err < 0)
            {
                throw new InvalidOperationException($"Failed to initialize mpv: {_mpvContext.GetErrorString(err)}");
            }
        }

        await _mpvContext.LoadAudio(fileName);

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

            // Mirror this row's paragraph as a SubtitleLineViewModel so the AudioVisualizer can
            // draw + drag it. The visualizer's drag handlers mutate StartTime/EndTime on the VM,
            // so OnWaveformParagraphChanged below forwards those edits back to row.StepResult.Paragraph.
            var waveformParagraph = new SubtitleLineViewModel
            {
                Number = p.Paragraph.Number,
                Text = p.Text,
                StartTime = TimeSpan.FromMilliseconds(p.Paragraph.StartTime.TotalMilliseconds),
                EndTime = TimeSpan.FromMilliseconds(p.Paragraph.EndTime.TotalMilliseconds),
            };
            waveformParagraph.UpdateDuration();
            waveformParagraph.PropertyChanged += OnWaveformParagraphChanged;
            row.WaveformParagraph = waveformParagraph;
            WaveformParagraphs.Add(waveformParagraph);
            _waveformParagraphToRow[waveformParagraph] = row;
        }

        // The caller passes either real peaks of the source video, an empty placeholder (when
        // there's no video yet), or null. The visualizer renders nothing when peaks are missing;
        // background generation in TextToSpeechViewModel.Import pushes real peaks into this
        // property once ffmpeg finishes, at which point the binding refreshes the visualizer.
        WavePeakData = wavePeakData;

        // Shared with the Cast dialog (see ActorVoiceDetector.FilterUsableEngines) so the two
        // windows always show the same set of usable engines. Add new engine availability rules
        // there, not here.
        foreach (var engineItem in ActorVoiceDetector.FilterUsableEngines(engines))
        {
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

    // Drag/edit done on the waveform mutates the SubtitleLineViewModel mirror; this writes the
    // new times back to the underlying TtsStepResult.Paragraph so OK/Export see the change.
    private void OnWaveformParagraphChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not SubtitleLineViewModel waveformParagraph)
        {
            return;
        }

        if (e.PropertyName != nameof(SubtitleLineViewModel.StartTime) &&
            e.PropertyName != nameof(SubtitleLineViewModel.EndTime))
        {
            return;
        }

        if (!_waveformParagraphToRow.TryGetValue(waveformParagraph, out var row))
        {
            return;
        }

        var paragraph = row.StepResult.Paragraph;
        paragraph.StartTime = new TimeCode(waveformParagraph.StartTime);
        paragraph.EndTime = new TimeCode(waveformParagraph.EndTime);

        // Cps depends on duration; refresh so the grid stays consistent with the dragged times.
        row.Cps = Math.Round(paragraph.GetCharactersPerSecond(), 2).ToString(CultureInfo.CurrentCulture);
    }

    // Centers the visualizer on the currently selected paragraph and marks it as selected so the
    // user can grab its start/end handles. Safe to call before AudioVisualizer is attached.
    public void RefreshWaveformPosition()
    {
        var av = AudioVisualizer;
        if (av == null || WavePeakData == null)
        {
            return;
        }

        var row = SelectedLine;
        var waveformParagraph = row?.WaveformParagraph;
        if (waveformParagraph == null || WaveformParagraphs.Count == 0)
        {
            return;
        }

        // SetPosition still wants an index into the list it's given — we know the mirror is in
        // WaveformParagraphs because Initialize put it there.
        var index = WaveformParagraphs.IndexOf(waveformParagraph);
        if (index < 0)
        {
            return;
        }

        var startSeconds = Math.Max(0, waveformParagraph.StartTime.TotalSeconds - 2.0);

        av.SetPosition(
            startSeconds,
            WaveformParagraphs,
            waveformParagraph.StartTime.TotalSeconds,
            index,
            new List<SubtitleLineViewModel> { waveformParagraph });
        av.InvalidateVisual();
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
        var exportFormat = new TtsImportExport
        {
            VideoFileName = _videoFileName,
            ActorVoiceMappings = ActorVoiceMappings.ToList(),
        };
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
                // Per-line engine snapshot, not the global SelectedEngine — different rows can
                // come from different engines via the cast workflow, and on re-import we want
                // each line to remember which engine produced it.
                EngineName = string.IsNullOrEmpty(line.StepResult.EngineName)
                    ? (SelectedEngine?.Name ?? string.Empty)
                    : line.StepResult.EngineName,
                Model = line.StepResult.Model,
                Instruction = line.StepResult.Instruction,
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

        var picked = result.SelectedHistoryItem;
        line.Voice = picked.Voice?.Name ?? string.Empty;
        line.StepResult.CurrentFileName = picked.FileName ?? string.Empty;
        line.StepResult.Voice = picked.Voice;
        // Restore the full engine snapshot stored with the history row so the line knows which
        // engine/model/instruction produced this audio. Without this, clicking the row would
        // sync the left panel to the live (last-set) values rather than what's actually playing.
        line.StepResult.EngineName = picked.EngineName ?? string.Empty;
        line.StepResult.Model = picked.Model ?? string.Empty;
        line.StepResult.Instruction = picked.Instruction ?? string.Empty;
        line.Speed = Math.Round(picked.Speed, 2).ToString(CultureInfo.CurrentCulture);

        // Mirror the click-to-sync behaviour so the left panel immediately reflects the picked
        // history entry's engine/model/voice/instruction.
        await ApplyLineToLeftPanelAsync(line);
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

    // Replace _cancellationTokenSource, disposing the previous instance. The
    // RegenerateAudio path swaps in a CTS owned by GeneratingAudioViewModel,
    // so disposing the old one here just frees the per-window CTS created in
    // the constructor (or a previous Play*/RegenerateAudio call we own).
    private void ReplaceCts(CancellationTokenSource next)
    {
        var old = _cancellationTokenSource;
        _cancellationTokenSource = next;
        _cancellationToken = next.Token;
        if (!ReferenceEquals(old, next))
        {
            try { old.Dispose(); } catch (ObjectDisposedException) { }
        }
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

        if (string.IsNullOrWhiteSpace(line.Text))
        {
            if (Window != null)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.General.Warning,
                    "Cannot regenerate audio with empty text",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return;
        }

        var isEngineInstalled = await engine.IsInstalled(SelectedRegion);
        if (!isEngineInstalled)
        {
            return;
        }

        if (!await TtsVoiceInstaller.EnsureVoiceInstalled(engine, voice, Window, _windowService))
        {
            return;
        }

        IsRegenerateEnabled = false;

        var oldStyle = SelectedStyle;
        if (engine is Murf && !string.IsNullOrEmpty(SelectedStyle))
        {
            Se.Settings.Video.TextToSpeech.MurfStyle = SelectedStyle;
        }

        var generatingAudioVm = _windowService.ShowWindow<GeneratingAudioWindow, GeneratingAudioViewModel>(Window!);
        ReplaceCts(generatingAudioVm.CancellationTokenSource);

        TtsResult speakResult;
        try
        {
            speakResult = await TtsInstructionSwap.RunAsync(engine, Instruction, () =>
                engine.Speak(Utilities.UnbreakLine(line.Text), _waveFolder, voice, SelectedLanguage, SelectedRegion, SelectedModel, _cancellationToken));

            line.StepResult.CurrentFileName = speakResult.FileName;
            line.StepResult.Voice = voice;

            var adjustSpeedStepResult = await TrimAndAdjustSpeed(line);
            var postProcessedFileName = await TtsPostProcessor.ApplyPostProcessing(adjustSpeedStepResult.CurrentFileName, _waveFolder, _cancellationToken);

            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            adjustSpeedStepResult.CurrentFileName = postProcessedFileName;
            // Record which engine/model/instruction this regenerate used so the row's "click to
            // sync left panel" feature can restore them later.
            adjustSpeedStepResult.EngineName = engine.Name;
            adjustSpeedStepResult.Model = SelectedModel ?? string.Empty;
            adjustSpeedStepResult.Instruction = Instruction ?? string.Empty;
            line.Speed = Math.Round(adjustSpeedStepResult.SpeedFactor, 2).ToString(CultureInfo.CurrentCulture);
            line.Cps = Math.Round(adjustSpeedStepResult.Paragraph.GetCharactersPerSecond(), 2).ToString(CultureInfo.CurrentCulture);
            line.StepResult = adjustSpeedStepResult;
            line.Voice = voice.ToString();

            line.AddHistory(voice, line.StepResult.CurrentFileName, engine.Name, SelectedModel ?? string.Empty, Instruction ?? string.Empty);
        }
        catch (HttpRequestException ex)
        {
            SeLogger.Error(ex, "TTS server error during regeneration.");
            if (Window != null)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    "TTS server error: " + ex.Message,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return;
        }
        finally
        {
            generatingAudioVm.Close();
            IsRegenerateEnabled = true;
            if (engine is Murf && oldStyle != null)
            {
                Se.Settings.Video.TextToSpeech.MurfStyle = oldStyle;
            }
        }

        _skipAutoContinue = true;
        await PlayAudio(line.StepResult.CurrentFileName);
    }

    [RelayCommand]
    private async Task PlayRow(ReviewRow? line)
    {
        if (line == null)
        {
            return;
        }

        ReplaceCts(new CancellationTokenSource());
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

        ReplaceCts(new CancellationTokenSource());
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
        // Push any edits the user made to row.Text back into the step results so
        // the caller sees them, then publish the included rows as StepResults.
        foreach (var row in Lines)
        {
            row.StepResult.Text = row.Text;
        }

        StepResults = Lines.Where(p => p.Include).Select(p => p.StepResult).ToArray();

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

        var doVad = Se.Settings.Video.TextToSpeech.VadSilenceCompressionEnabled;
        var vadMaxSilence = Se.Settings.Video.TextToSpeech.VadMaxSilenceSeconds;
        var doHighQualityStretch = Se.Settings.Video.TextToSpeech.HighQualityTimeStretchEnabled;

        // Step 1: Trim silence from start and end
        var outputFileNameTrim = Path.Combine(_waveFolder, Guid.NewGuid() + ".wav");
        _tempAudioFiles.Add(outputFileNameTrim);
        var trimProcess = FfmpegGenerator.TrimSilenceStartAndEnd(item.CurrentFileName, outputFileNameTrim);
        await trimProcess.StartAndWaitAsync(_cancellationToken);

        var currentFile = outputFileNameTrim;

        // Step 2: VAD-based internal silence compression
        if (doVad)
        {
            var vadOutput = Path.Combine(_waveFolder, $"vad_{Guid.NewGuid()}.wav");
            _tempAudioFiles.Add(vadOutput);
            var vadProcess = FfmpegGenerator.CompressInternalSilence(currentFile, vadOutput, vadMaxSilence);
            await vadProcess.StartAndWaitAsync(_cancellationToken);

            if (File.Exists(vadOutput) && new FileInfo(vadOutput).Length > 0)
            {
                currentFile = vadOutput;
            }
        }

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

        var mediaInfo = FfmpegMediaInfo.Parse(currentFile);
        if (mediaInfo.Duration.TotalMilliseconds <= p.DurationTotalMilliseconds + addDuration)
        {
            return new TtsStepResult
            {
                Paragraph = p,
                Text = item.Text,
                CurrentFileName = currentFile,
                SpeedFactor = 1.0f,
                Voice = item.Voice,
                EngineName = item.EngineName,
                Model = item.Model,
                Instruction = item.Instruction,
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
                EngineName = item.EngineName,
                Model = item.Model,
                Instruction = item.Instruction,
            };
        }

        // Step 3: Time-stretching
        var ext = ".wav";
        var factor = (decimal)mediaInfo.Duration.TotalMilliseconds / divisor;
        var outputFileName2 = Path.Combine(_waveFolder, $"{index}_{Guid.NewGuid()}{ext}");
        var overrideFileName = string.Empty;
        if (!string.IsNullOrEmpty(overrideFileName) && File.Exists(Path.Combine(_waveFolder, overrideFileName)))
        {
            outputFileName2 = Path.Combine(_waveFolder, $"{Path.GetFileNameWithoutExtension(overrideFileName)}_{Guid.NewGuid()}{ext}");
        }
        _tempAudioFiles.Add(outputFileName2);

        // Use rubberband (WSOLA) for high-quality stretch, or atempo as fallback
        Process speedProcess;
        if (doHighQualityStretch)
        {
            speedProcess = FfmpegGenerator.ChangeSpeedHighQuality(currentFile, outputFileName2, (float)factor);
        }
        else
        {
            speedProcess = FfmpegGenerator.ChangeSpeed(currentFile, outputFileName2, (float)factor);
        }
        await speedProcess.StartAndWaitAsync(_cancellationToken);

        // Fallback: if rubberband failed, retry with atempo
        if (doHighQualityStretch && (!File.Exists(outputFileName2) || new FileInfo(outputFileName2).Length == 0))
        {
            var fallbackProcess = FfmpegGenerator.ChangeSpeed(currentFile, outputFileName2, (float)factor);
            await fallbackProcess.StartAndWaitAsync(_cancellationToken);
        }

        return new TtsStepResult
        {
            Paragraph = p,
            Text = item.Text,
            CurrentFileName = outputFileName2,
            SpeedFactor = (float)factor,
            Voice = item.Voice,
            EngineName = item.EngineName,
            Model = item.Model,
            Instruction = item.Instruction,
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

            if (RegenerateAudioCommand.CanExecute(line))
            {
                RegenerateAudioCommand.Execute(line);
            }
        }
        else if (MatchesPlayPauseShortcut(e))
        {
            // A modifier-less binding (the default bare Space) must still type into a focused text
            // box; with modifiers held the shortcut works everywhere. Buttons are unaffected either
            // way - they handle Space themselves before this window-level handler runs.
            if (e.KeyModifiers == KeyModifiers.None && Window?.FocusManager?.GetFocusedElement() is TextBox)
            {
                return;
            }

            e.Handled = true;
            TogglePlayPauseSelectedRow();
        }
    }

    /// <summary>
    /// True when the pressed keys match the user's main-window play/pause bindings
    /// (TogglePlayPause / TogglePlayPause2; defaults Space and Ctrl/Cmd+Space), so the review
    /// window plays with the same keys as the rest of the app (#12093).
    /// </summary>
    private static bool MatchesPlayPauseShortcut(KeyEventArgs e)
    {
        var cmdOrWin = OperatingSystem.IsMacOS() ? "Win" : "Ctrl";
        var toggleKeys = Se.Settings.Shortcuts.FirstOrDefault(s => s.ActionName == nameof(MainViewModel.TogglePlayPauseCommand))?.Keys
                         ?? [nameof(Key.Space)];
        var toggle2Keys = Se.Settings.Shortcuts.FirstOrDefault(s => s.ActionName == nameof(MainViewModel.TogglePlayPause2Command))?.Keys
                          ?? [cmdOrWin, nameof(Key.Space)];
        return MatchesKeys(e, toggleKeys) || MatchesKeys(e, toggle2Keys);
    }

    // Matches a stored shortcut key list (modifier tokens + one main key) against a key event.
    // Multi-key non-modifier chords are not supported here - the full ShortcutManager handles
    // those in the main window; a dialog only needs the simple form.
    private static bool MatchesKeys(KeyEventArgs e, List<string> keys)
    {
        var modifiers = KeyModifiers.None;
        Key? mainKey = null;
        foreach (var token in keys)
        {
            if (token is "Ctrl" or "Control" or "LeftCtrl" or "RightCtrl")
            {
                modifiers |= KeyModifiers.Control;
            }
            else if (token is "Alt" or "LeftAlt" or "RightAlt")
            {
                modifiers |= KeyModifiers.Alt;
            }
            else if (token is "Shift" or "LeftShift" or "RightShift")
            {
                modifiers |= KeyModifiers.Shift;
            }
            else if (token is "Win" or "Meta" or "LWin" or "RWin" or "Cmd" or "Command")
            {
                modifiers |= KeyModifiers.Meta;
            }
            else if (Enum.TryParse<Key>(token, out var key) && mainKey == null)
            {
                mainKey = key;
            }
            else
            {
                return false;
            }
        }

        return mainKey != null && mainKey == e.Key && e.KeyModifiers == modifiers;
    }

    private void TogglePlayPauseSelectedRow()
    {
        if (IsStopVisible || Lines.Any(l => l.IsPlaying))
        {
            Stop();
            return;
        }

        var line = SelectedLine;
        if (line is { IsPlayingEnabled: true } && PlayRowCommand.CanExecute(line))
        {
            PlayRowCommand.Execute(line);
        }
    }

    internal void SelectedEngineChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedEngineChanged();
    }

    // True while ApplyLineToLeftPanelAsync is in the middle of switching the engine itself. We
    // skip the fire-and-forget refresh that the ComboBox's SelectionChanged event triggers in
    // that case, otherwise it races our own awaited refresh and wins — overwriting the row's
    // voice/model/instruction with the engine's defaults.
    private bool _suppressEngineRefreshDispatch;

    public void SelectedEngineChanged()
    {
        if (_suppressEngineRefreshDispatch)
        {
            return;
        }

        // Fire-and-forget wrapper for the SelectionChanged event. Callers that need to wait for
        // the refresh (e.g. ApplyLineToLeftPanelAsync) should await SelectedEngineChangedAsync()
        // directly so they see the new Voices/Models populated before applying further changes.
        Dispatcher.UIThread.PostSafe(async () => await SelectedEngineChangedAsync());
    }

    public async Task SelectedEngineChangedAsync()
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        var voices = await engine.GetVoices(SelectedLanguage?.Code ?? string.Empty);
        Voices.Clear();
        foreach (var vo in voices)
        {
            Voices.Add(vo);
        }

        var lastVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
        if (lastVoice == null)
        {
            lastVoice = Voices.FirstOrDefault(p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                  p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
        }

        SelectedVoice = lastVoice ?? Voices.First();

        if (engine.HasLanguageParameter)
        {
            var languages = await engine.GetLanguages(SelectedVoice, null); // SelectedModel);
            Languages.Clear();
            foreach (var language in languages)
            {
                Languages.Add(language);
            }

            SelectedLanguage = Languages.FirstOrDefault();
        }

        if (engine.HasRegion)
        {
            var regions = await engine.GetRegions();
            Regions.Clear();
            foreach (var region in regions)
            {
                Regions.Add(region);
            }

            SelectedRegion = Regions.FirstOrDefault();
        }

        if (engine.HasModel)
        {
            var models = await engine.GetModels();
            Models.Clear();
            foreach (var model in models)
            {
                Models.Add(model);
            }

            SelectedModel = Models.FirstOrDefault();
        }

        IsElevenLabsControlsVisible = false;
        UpdateInstructionVisibility();
        LoadInstructionForEngine();
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
                SelectedModel = Models.First();
            }
        }
        else if (engine is Qwen3TtsCpp)
        {
            SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.Qwen3TtsCppModel);
            if (string.IsNullOrEmpty(SelectedModel))
            {
                SelectedModel = Models.FirstOrDefault();
            }
        }
        else if (engine is ChatterboxTtsCpp)
        {
            SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.ChatterboxModel);
            if (string.IsNullOrEmpty(SelectedModel))
            {
                SelectedModel = Models.FirstOrDefault();
            }
        }
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
            Dispatcher.UIThread.PostSafe(async () =>
            {
                var voices = await murf.GetVoices(SelectedLanguage?.Code ?? string.Empty);
                Voices.Clear();
                ObservableCollectionExtensions.AddRange(Voices, voices);

                var lastVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
                if (lastVoice == null)
                {
                    lastVoice = Voices.FirstOrDefault(p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                          p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
                }

                SelectedVoice = lastVoice ?? Voices.First();
            });
        }
    }

    internal void SelectedModelChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;
        var voice = SelectedVoice;
        var model = SelectedModel;
        IsElevenLabsEngineV3Selected = false;
        UpdateInstructionVisibility();
        if (engine == null || voice == null || model == null)
        {
            return;
        }

        Dispatcher.UIThread.PostSafe(async () =>
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

                SelectedLanguage = Languages.FirstOrDefault(p => p.Name == Se.Settings.Video.TextToSpeech.ElevenLabsLanguage);
                if (SelectedLanguage == null)
                {
                    SelectedLanguage = Languages.FirstOrDefault(p => p.Code == "en");
                }
            }
        });
    }

    // ---- voice-design / instruction helpers -----------------------------------------------

    private static ObservableCollection<string> BuildKeywordOptions(string[] keywords)
    {
        var options = new ObservableCollection<string> { OmniVoiceAny };
        foreach (var k in keywords)
        {
            options.Add(k);
        }
        return options;
    }

    private static bool IsReal(string? value) => !string.IsNullOrEmpty(value) && value != OmniVoiceAny;

    partial void OnSelectedVoiceChanged(Voice? value) => UpdateInstructionVisibility();

    // When the user clicks a row, push that row's recorded engine/voice/model/instruction into
    // the left-side combos so they reflect what produced the selected line. Without this the
    // panel always shows the *current* (last-set) values, which is confusing when each line was
    // generated from a different per-actor mapping.
    //
    // Guard with _suppressSelectedLineSync so the chain of engine/model writes doesn't loop
    // back through SelectedEngineChanged (which would reset Voices and clobber our intended
    // voice).
    partial void OnSelectedLineChanged(ReviewRow? value)
    {
        // Re-center the waveform on the newly selected row regardless of the left-panel sync
        // state — the visual cue should follow the user's click immediately.
        RefreshWaveformPosition();

        if (value == null)
        {
            return;
        }

        // If another apply is in flight, queue this selection — we'll re-apply at the end of the
        // current cycle so the panel ends up on the row the user actually clicked last (not
        // wherever the in-flight apply ends).
        if (_suppressSelectedLineSync)
        {
            _pendingApplyRow = value;
            return;
        }

        _ = ApplyLineWithFollowUpAsync(value);
    }

    private async Task ApplyLineWithFollowUpAsync(ReviewRow row)
    {
        await ApplyLineToLeftPanelAsync(row);

        // Drain any selection changes that landed during the await. Loop until we catch up so
        // multiple rapid clicks all resolve to the latest.
        while (_pendingApplyRow != null && !ReferenceEquals(_pendingApplyRow, row))
        {
            var next = _pendingApplyRow;
            _pendingApplyRow = null;
            row = next;
            await ApplyLineToLeftPanelAsync(row);
        }
        _pendingApplyRow = null;
    }

    private bool _suppressSelectedLineSync;
    private ReviewRow? _pendingApplyRow;

    private async Task ApplyLineToLeftPanelAsync(ReviewRow row)
    {
        var step = row.StepResult;
        if (step == null)
        {
            return;
        }

        _suppressSelectedLineSync = true;
        try
        {
            ITtsEngine? targetEngine = null;
            if (!string.IsNullOrEmpty(step.EngineName))
            {
                targetEngine = Engines.FirstOrDefault(e => string.Equals(e.Name, step.EngineName, StringComparison.OrdinalIgnoreCase));
            }
            targetEngine ??= step.Voice != null
                ? Engines.FirstOrDefault(e => string.Equals(e.Name, SelectedEngine?.Name, StringComparison.OrdinalIgnoreCase))
                : SelectedEngine;

            // Refresh Voices / Models / Languages / Regions when:
            //   - The row's engine differs from the current engine, OR
            //   - We haven't loaded for this engine yet. After Initialize (Lines.Count > 0) the
            //     Loaded-time refresh is skipped, and if the first row's engine matches the
            //     caller-supplied SelectedEngine the engine-change branch above also skips —
            //     leaving Models empty. We detect that with `engine.HasModel && Models.Count==0`.
            //
            // Setting SelectedEngine also fires the ComboBox's SelectionChanged → which would
            // dispatch its own fire-and-forget refresh. We suppress that with the flag so the
            // two don't race; the awaited call below does the same work and lets us apply the
            // row's voice/model on top of it without it being immediately overwritten.
            var needsRefresh = targetEngine != null
                && (!ReferenceEquals(targetEngine, SelectedEngine)
                    || (targetEngine.HasModel && Models.Count == 0));
            if (needsRefresh)
            {
                _suppressEngineRefreshDispatch = true;
                try
                {
                    SelectedEngine = targetEngine;
                    await SelectedEngineChangedAsync();
                }
                finally
                {
                    _suppressEngineRefreshDispatch = false;
                }
            }

            // Voice: prefer the recorded Voice instance, otherwise match by name.
            if (step.Voice != null)
            {
                var match = Voices.FirstOrDefault(v => string.Equals(v.Name, step.Voice.Name, StringComparison.OrdinalIgnoreCase));
                SelectedVoice = match ?? step.Voice;
            }

            if (!string.IsNullOrEmpty(step.Model))
            {
                var matchModel = Models.FirstOrDefault(m => string.Equals(m, step.Model, StringComparison.OrdinalIgnoreCase));
                if (matchModel != null)
                {
                    SelectedModel = matchModel;
                }
            }

            Instruction = step.Instruction ?? string.Empty;
            UpdateInstructionVisibility();
            if (IsInstructionPickerVisible)
            {
                SyncOmniVoicePickerFromInstruction();
            }
        }
        finally
        {
            _suppressSelectedLineSync = false;
        }
    }

    partial void OnSelectedOmniVoiceGenderChanged(string value) => RebuildOmniVoiceInstruction();
    partial void OnSelectedOmniVoiceAgeChanged(string value) => RebuildOmniVoiceInstruction();
    partial void OnSelectedOmniVoicePitchChanged(string value) => RebuildOmniVoiceInstruction();
    partial void OnSelectedOmniVoiceAccentChanged(string value) => RebuildOmniVoiceInstruction();
    partial void OnOmniVoiceWhisperChanged(bool value) => RebuildOmniVoiceInstruction();

    private void RebuildOmniVoiceInstruction()
    {
        if (_suppressKeywordSync || !IsInstructionPickerVisible)
        {
            return;
        }

        var parts = new List<string>();
        if (IsReal(SelectedOmniVoiceGender))
        {
            parts.Add(SelectedOmniVoiceGender);
        }
        if (IsReal(SelectedOmniVoiceAge))
        {
            parts.Add(SelectedOmniVoiceAge);
        }
        if (IsReal(SelectedOmniVoicePitch))
        {
            parts.Add(SelectedOmniVoicePitch);
        }
        if (IsReal(SelectedOmniVoiceAccent))
        {
            parts.Add(SelectedOmniVoiceAccent);
        }
        if (OmniVoiceWhisper)
        {
            parts.Add(OmniVoiceTtsCpp.InstructionWhisper);
        }
        Instruction = string.Join(", ", parts);
    }

    private void SyncOmniVoicePickerFromInstruction()
    {
        var present = (Instruction ?? string.Empty)
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _suppressKeywordSync = true;
        SelectedOmniVoiceGender = MatchGroup(OmniVoiceTtsCpp.InstructionGenders, present);
        SelectedOmniVoiceAge = MatchGroup(OmniVoiceTtsCpp.InstructionAges, present);
        SelectedOmniVoicePitch = MatchGroup(OmniVoiceTtsCpp.InstructionPitches, present);
        SelectedOmniVoiceAccent = MatchGroup(OmniVoiceTtsCpp.InstructionAccents, present);
        OmniVoiceWhisper = present.Contains(OmniVoiceTtsCpp.InstructionWhisper);
        _suppressKeywordSync = false;

        RebuildOmniVoiceInstruction();
    }

    private static string MatchGroup(string[] group, HashSet<string> present)
        => Array.Find(group, present.Contains) ?? OmniVoiceAny;

    // Recomputes the three visibility/enabled flags from the current engine, model, and voice.
    // Call after any of those change. Mirrors RefreshInstructionVisibility +
    // UpdateOmniVoicePickerState in the main TTS VM.
    private void UpdateInstructionVisibility()
    {
        var engine = SelectedEngine;
        var model = SelectedModel;

        IsInstructionTextVisible =
            (engine is Qwen3TtsCpp && Qwen3TtsCpp.IsVoiceDesignModel(model))
            || (engine is Qwen3TtsCrispAsr && Qwen3TtsCrispAsr.IsVoiceDesignModel(model));
        IsInstructionPickerVisible = engine is OmniVoiceTtsCpp;
        HasInstruction = IsInstructionTextVisible || IsInstructionPickerVisible;

        var isDefaultOmniVoice = SelectedVoice?.EngineVoice is OmniVoice ov && string.IsNullOrEmpty(ov.FilePath);
        IsInstructionPickerEnabled = IsInstructionPickerVisible && isDefaultOmniVoice;
        IsInstructionVoiceHintVisible = IsInstructionPickerVisible && !isDefaultOmniVoice;
    }

    private void LoadInstructionForEngine()
    {
        Instruction = SelectedEngine switch
        {
            Qwen3TtsCpp => Se.Settings.Video.TextToSpeech.Qwen3TtsCppInstruction ?? string.Empty,
            Qwen3TtsCrispAsr => Se.Settings.Video.TextToSpeech.Qwen3TtsCppInstruction ?? string.Empty,
            OmniVoiceTtsCpp => Se.Settings.Video.TextToSpeech.OmniVoiceTtsCppInstruction ?? string.Empty,
            _ => string.Empty,
        };

        if (IsInstructionPickerVisible)
        {
            SyncOmniVoicePickerFromInstruction();
        }
    }

    internal void OnClosing(WindowClosingEventArgs e)
    {
        _skipAutoContinue = true;
        _timer.Stop();
        _timer.Elapsed -= OnTimerOnElapsed;
        _timer.Dispose();
        try { _cancellationTokenSource.Cancel(); } catch (ObjectDisposedException) { }
        try { _cancellationTokenSource.Dispose(); } catch (ObjectDisposedException) { }
        lock (_playLock)
        {
            _mpvContext?.Dispose();
            _mpvContext = null;
        }

        // The waveform mirrors subscribe to PropertyChanged in Initialize so drags
        // on the visualizer write back into the per-row TtsStepResult. Detach now
        // so the mirror VMs (and the rows they reference) become collectible.
        foreach (var wp in WaveformParagraphs)
        {
            wp.PropertyChanged -= OnWaveformParagraphChanged;
        }
        WaveformParagraphs.Clear();
        _waveformParagraphToRow.Clear();

        // Intermediate WAVs from TrimAndAdjustSpeed land in _waveFolder and would
        // otherwise pile up across regenerate cycles. Best-effort — _waveFolder is
        // usually a session-scoped temp dir but a stray locked file shouldn't tank
        // window close. A regenerated row's *final* audio file is tracked here too,
        // and on OK it was just published via StepResults for the merge step —
        // deleting it would silently break that line in the final audio — so keep
        // anything a published result still references.
        var publishedFiles = OkPressed
            ? new HashSet<string>(StepResults.Select(r => r.CurrentFileName), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in _tempAudioFiles)
        {
            if (publishedFiles.Contains(f))
            {
                continue;
            }

            try { if (File.Exists(f))
            {
                File.Delete(f);
            } } catch { /* ignore */ }
        }
        _tempAudioFiles.Clear();

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
        RefreshWaveformPosition();
    }
}