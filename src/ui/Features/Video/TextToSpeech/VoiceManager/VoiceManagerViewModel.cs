using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceCloneConsent;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;

/// <summary>
/// One place to see and tend every voice of every engine: listen to a reference recording on
/// its waveform, fix its transcript, rename/delete it, copy it to another cloning engine, import
/// new recordings and fetch voice packs. Only file-backed clones are editable; presets, Piper
/// models and online voices are listed for orientation.
/// </summary>
public partial class VoiceManagerViewModel : ObservableObject
{
    public ObservableCollection<VoiceManagerEngineItem> Engines { get; } = new();
    public ObservableCollection<VoiceManagerRow> Voices { get; } = new();

    [ObservableProperty] private VoiceManagerEngineItem? _selectedEngine;
    [ObservableProperty] private VoiceManagerRow? _selectedVoice;
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private WavePeakData2? _wavePeakData;
    [ObservableProperty] private string _transcript = string.Empty;
    [ObservableProperty] private string _transcriptHint = string.Empty;
    [ObservableProperty] private bool _isTranscriptVisible;
    [ObservableProperty] private bool _isTranscriptDirty;
    [ObservableProperty] private bool _isFileVoiceSelected;
    [ObservableProperty] private bool _isPlayEnabled;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isImportVisible;
    [ObservableProperty] private bool _isOpenFolderEnabled;
    [ObservableProperty] private bool _isVoicePacksVisible;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private string _voiceInfoText = string.Empty;
    [ObservableProperty] private string _voiceCountText = string.Empty;
    [ObservableProperty] private string _emptyText = string.Empty;
    [ObservableProperty] private bool _isEmptyTextVisible;

    public Window? Window { get; set; }
    public AudioVisualizer? AudioVisualizer { get; set; }

    /// <summary>The listing started by the last engine pick - awaited by tests, fire-and-forget otherwise.</summary>
    internal Task VoicesLoaded { get; private set; } = Task.CompletedTask;

    /// <summary>The waveform/transcript load of the last voice pick; see <see cref="VoicesLoaded"/>.</summary>
    internal Task DetailsLoaded { get; private set; } = Task.CompletedTask;

    /// <summary>Engines whose voices folder was changed here; the caller re-lists those.</summary>
    public HashSet<string> ChangedEngineNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Renames done here, in order, so the caller can re-point what referenced the old name (a
    /// cast row, the saved voice pick) instead of letting it fall through to another voice.
    /// </summary>
    public List<VoiceRename> Renames { get; } = new();

    /// <summary>Deletes done here, for the same reason as <see cref="Renames"/>.</summary>
    public List<VoiceRename> Deletes { get; } = new();

    public record VoiceRename(string EngineName, string OldName, string NewName);

    /// <summary>The cloning engines a selected voice can be copied to (everything but its own).</summary>
    public List<ITtsEngine> CopyTargets =>
        Engines.Where(e => e.IsCloning && !ReferenceEquals(e.Engine, SelectedEngine?.Engine)).Select(e => e.Engine).ToList();

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;

    private readonly List<VoiceManagerRow> _allRows = new();
    private string _voicesFolder = string.Empty;
    private int _loadGeneration;
    private int _detailsGeneration;
    private bool _suppressTranscriptDirty;

    private LibMpvDynamicPlayer? _player;
    private readonly object _playLock = new();
    private readonly DispatcherTimer _playbackTimer;
    private readonly Stopwatch _playbackStopwatch = new();
    private double _playingDuration;

    public VoiceManagerViewModel(IWindowService windowService, IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _playbackTimer.Tick += OnPlaybackTick;
    }

    internal void Initialize(IEnumerable<ITtsEngine> engines, ITtsEngine? selectedEngine)
    {
        // Only the cloning engines: they are the ones with voices to tend (reference recordings
        // to play, transcribe, rename, copy). Presets, Piper models and online voice lists have
        // nothing to manage here. The order stays the TTS window's.
        foreach (var engine in engines.Where(e => e.SupportsVoiceCloning))
        {
            Engines.Add(new VoiceManagerEngineItem(engine));
        }

        SelectedEngine = Engines.FirstOrDefault(e => ReferenceEquals(e.Engine, selectedEngine))
                         ?? Engines.FirstOrDefault();

        _ = CheckInstalledAsync(Engines.ToList());
    }

    /// <summary>
    /// Fills in each engine's installed dot after the window is up - the checks look for
    /// binaries and model files, which is too slow to do on the UI thread before showing.
    /// </summary>
    private static async Task CheckInstalledAsync(List<VoiceManagerEngineItem> items)
    {
        foreach (var item in items)
        {
            bool installed;
            try
            {
                installed = await Task.Run(() => item.Engine.IsInstalled(null));
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, $"Voice manager: install check for {item.Name} failed");
                installed = false;
            }

            item.IsInstalled = installed;
        }
    }

    partial void OnSelectedEngineChanged(VoiceManagerEngineItem? value)
    {
        StopPlayback();
        OnPropertyChanged(nameof(CopyTargets));
        IsImportVisible = value != null;
        IsVoicePacksVisible = value != null;
        VoicesLoaded = LoadVoicesAsync(value, null);
    }

    partial void OnFilterTextChanged(string value)
    {
        ApplyFilter(SelectedVoice?.Name);
    }

    partial void OnSelectedVoiceChanged(VoiceManagerRow? value)
    {
        StopPlayback();
        IsFileVoiceSelected = value?.IsFileVoice == true;
        IsPlayEnabled = value?.IsFileVoice == true && File.Exists(value.FilePath);
        DetailsLoaded = LoadVoiceDetailsAsync(value);
    }

    partial void OnTranscriptChanged(string value)
    {
        if (!_suppressTranscriptDirty)
        {
            IsTranscriptDirty = true;
        }
    }

    private async Task LoadVoicesAsync(VoiceManagerEngineItem? engineItem, string? selectName)
    {
        var generation = ++_loadGeneration;
        _allRows.Clear();
        Voices.Clear();
        SelectedVoice = null;
        WavePeakData = null;
        IsEmptyTextVisible = false;
        VoiceCountText = string.Empty;
        _voicesFolder = string.Empty;
        IsOpenFolderEnabled = false;

        if (engineItem == null)
        {
            return;
        }

        IsLoading = true;
        StatusText = Se.Language.Video.TextToSpeech.LoadingVoicesDotDotDot;
        List<VoiceManagerRow> rows;
        try
        {
            var engine = engineItem.Engine;
            var voices = await engine.GetVoices(string.Empty);
            rows = await Task.Run(() => voices
                .Where(v => !PerLineVoiceClone.IsSelected(v))
                .Select(v => MakeRow(engine, v))
                .ToList());
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Voice manager: listing voices for {engineItem.Name} failed");
            rows = new List<VoiceManagerRow>();
            StatusText = ex.Message;
        }

        if (generation != _loadGeneration)
        {
            return; // another engine was picked while this one was listing
        }

        _allRows.AddRange(rows);
        engineItem.SetCount(rows.Count);
        _voicesFolder = ResolveVoicesFolder(engineItem.Engine, rows);
        IsOpenFolderEnabled = !string.IsNullOrEmpty(_voicesFolder) && Directory.Exists(_voicesFolder);
        IsLoading = false;
        StatusText = string.IsNullOrEmpty(_voicesFolder) ? string.Empty : _voicesFolder;
        ApplyFilter(selectName);
    }

    private static VoiceManagerRow MakeRow(ITtsEngine engine, Voice voice)
    {
        var filePath = VoiceFileRename.GetReferenceFilePath(voice);
        var lang = Se.Language.Video.TextToSpeech;
        if (filePath != null)
        {
            var (seconds, format) = ReadWaveInfo(filePath);
            var hasTranscript = VoiceReferenceTranscript.Read(filePath) != null;
            return new VoiceManagerRow(voice, VoiceKind.Clone, lang.VoiceKindClone, IconNames.AccountVoice, filePath, seconds, format, hasTranscript);
        }

        if (engine is Piper)
        {
            var installed = engine.IsVoiceInstalled(voice);
            return new VoiceManagerRow(voice, VoiceKind.Model, lang.VoiceKindModel, installed ? IconNames.CheckCircle : IconNames.CloudDownload, null, 0,
                installed ? Se.Language.General.Installed : Se.Language.General.NotInstalled, false);
        }

        if (engine.HasApiKey)
        {
            return new VoiceManagerRow(voice, VoiceKind.Online, lang.VoiceKindOnline, IconNames.Web, null, 0, string.Empty, false);
        }

        return new VoiceManagerRow(voice, VoiceKind.Preset, lang.VoiceKindPreset, IconNames.Robot, null, 0, string.Empty, false);
    }

    private static (double seconds, string format) ReadWaveInfo(string fileName)
    {
        try
        {
            using var stream = File.OpenRead(fileName);
            var header = new WaveHeader2(stream);
            if (header.ChunkId != "RIFF" || header.Format != "WAVE" || header.BytesPerSecond <= 0)
            {
                return (0, string.Empty);
            }

            var channels = header.NumberOfChannels == 1 ? "mono" : header.NumberOfChannels == 2 ? "stereo" : $"{header.NumberOfChannels} ch";
            var format = $"{header.SampleRate / 1000.0:0.#} kHz · {channels} · {header.BitsPerSample}-bit";
            return (header.LengthInSeconds, format);
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Voice manager: cannot read wave header of \"{fileName}\"");
            return (0, string.Empty);
        }
    }

    /// <summary>
    /// The folder the engine lists its clones from: taken from the first file-backed voice, or -
    /// for an engine with no clones yet, where "open folder" is the way to see where to put one -
    /// from the engine's own static <c>GetSetVoicesFolder</c>, which every cloning engine has but
    /// <see cref="ITtsEngine"/> does not declare.
    /// </summary>
    private static string ResolveVoicesFolder(ITtsEngine engine, List<VoiceManagerRow> rows)
    {
        var fromRow = rows.FirstOrDefault(r => r.FilePath != null)?.FilePath;
        if (fromRow != null)
        {
            return Path.GetDirectoryName(fromRow) ?? string.Empty;
        }

        if (!engine.SupportsVoiceCloning)
        {
            return string.Empty;
        }

        try
        {
            var method = engine.GetType().GetMethod("GetSetVoicesFolder", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes);
            return method?.Invoke(null, null) as string ?? string.Empty;
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Voice manager: resolving voices folder for {engine.Name} failed");
            return string.Empty;
        }
    }

    private void ApplyFilter(string? selectName)
    {
        var filter = FilterText.Trim();
        var rows = string.IsNullOrEmpty(filter)
            ? _allRows
            : _allRows.Where(r => r.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                  r.KindText.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

        Voices.Clear();
        foreach (var row in rows)
        {
            Voices.Add(row);
        }

        VoiceCountText = string.Format(Se.Language.Video.TextToSpeech.XVoices, Voices.Count);
        IsEmptyTextVisible = Voices.Count == 0 && !IsLoading;
        EmptyText = _allRows.Count == 0
            ? Se.Language.Video.TextToSpeech.NoVoicesForEngine
            : Se.Language.Video.TextToSpeech.NoVoicesMatchFilter;

        SelectedVoice = Voices.FirstOrDefault(v => v.Name == selectName) ?? Voices.FirstOrDefault();
    }

    private async Task LoadVoiceDetailsAsync(VoiceManagerRow? row)
    {
        // Its own counter: the engine listing selects a row while it is still in flight, and
        // sharing one counter made that selection cancel the listing - IsLoading then never
        // cleared and the window sat on an empty list with the progress bar running.
        var generation = ++_detailsGeneration;
        WavePeakData = null;
        _suppressTranscriptDirty = true;
        Transcript = string.Empty;
        _suppressTranscriptDirty = false;
        IsTranscriptDirty = false;

        var engine = SelectedEngine?.Engine;
        var requirement = VoiceReferenceTranscript.GetRequirement(engine);
        IsTranscriptVisible = row?.IsFileVoice == true && requirement != TranscriptRequirement.NotUsed;
        TranscriptHint = requirement switch
        {
            TranscriptRequirement.Required => Se.Language.Video.TextToSpeech.TranscriptRequiredHint,
            TranscriptRequirement.Optional => Se.Language.Video.TextToSpeech.TranscriptOptionalHint,
            _ => string.Empty,
        };

        if (row == null)
        {
            VoiceInfoText = string.Empty;
            return;
        }

        VoiceInfoText = row.IsFileVoice
            ? string.Join("  ·  ", new[] { row.Format, row.Duration, Path.GetFileName(row.FilePath) }.Where(s => !string.IsNullOrEmpty(s)))
            : row.KindText + (string.IsNullOrEmpty(row.Format) ? string.Empty : "  ·  " + row.Format);

        if (!row.IsFileVoice || !File.Exists(row.FilePath))
        {
            return;
        }

        var filePath = row.FilePath!;
        _suppressTranscriptDirty = true;
        Transcript = VoiceReferenceTranscript.Read(filePath) ?? string.Empty;
        _suppressTranscriptDirty = false;
        IsTranscriptDirty = false;

        var peaks = await Task.Run(() =>
        {
            try
            {
                using var generator = new WavePeakGenerator2(filePath);
                // No peak file: a reference clip is seconds long and the peaks are regenerated
                // on every selection - cheaper than littering the voices folder with .peak files.
                return generator.IsSupported ? generator.GeneratePeaks(0, string.Empty) : null;
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, $"Voice manager: generating peaks for \"{filePath}\" failed");
                return null;
            }
        });

        if (generation != _detailsGeneration)
        {
            return;
        }

        WavePeakData = peaks;
        FitWaveformToClip();
    }

    /// <summary>
    /// Zooms the waveform so the whole clip fills the control: the visualizer shows
    /// <c>width / (zoom · peaksPerSecond)</c> seconds, so solving for the clip length gives the
    /// zoom. Re-run when the control is resized.
    /// </summary>
    public void FitWaveformToClip()
    {
        var av = AudioVisualizer;
        var peaks = WavePeakData;
        if (av == null || peaks == null || peaks.LengthInSeconds <= 0 || av.Bounds.Width <= 0)
        {
            return;
        }

        var zoom = av.Bounds.Width / (peaks.LengthInSeconds * peaks.SampleRate);
        av.ZoomFactor = Math.Clamp(zoom, AudioVisualizer.MinZoomFactor, AudioVisualizer.MaxZoomFactor);
        av.StartPositionSeconds = 0;
        av.CurrentVideoPositionSeconds = 0;
        av.InvalidateVisual();
    }

    #region Playback

    [RelayCommand]
    private async Task PlayOrStop()
    {
        if (IsPlaying)
        {
            StopPlayback();
            return;
        }

        var row = SelectedVoice;
        if (row?.FilePath == null || !File.Exists(row.FilePath))
        {
            return;
        }

        try
        {
            DisposePlayerOffThread();
            LibMpvDynamicPlayer player;
            lock (_playLock)
            {
                player = new LibMpvDynamicPlayer();
                player.LoadLib();
                var err = player.Initialize();
                if (err < 0)
                {
                    throw new InvalidOperationException($"Failed to initialize mpv: {player.GetErrorString(err)}");
                }

                _player = player;
            }

            _playingDuration = row.DurationSeconds;
            IsPlaying = true;
            _playbackStopwatch.Restart();
            await player.LoadAudio(row.FilePath);
            _playbackTimer.Start();
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Voice manager: unable to play \"{row.FilePath}\"");
            StopPlayback();
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, "Unable to play audio: " + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnPlaybackTick(object? sender, EventArgs e)
    {
        LibMpvDynamicPlayer? player;
        lock (_playLock)
        {
            player = _player;
        }

        if (player == null)
        {
            StopPlayback();
            return;
        }

        var position = player.Position;
        var av = AudioVisualizer;
        if (av != null)
        {
            av.CurrentVideoPositionSeconds = position;
            av.InvalidateVisual();
        }

        // mpv reports "not playing" for the first moments while the file loads, hence the grace
        // period; after that, idle or past the end means the clip is done.
        var duration = _playingDuration > 0 ? _playingDuration : player.Duration;
        var settled = _playbackStopwatch.ElapsedMilliseconds > 700;
        if ((settled && !player.IsPlaying) || (duration > 0 && position >= duration - 0.03))
        {
            StopPlayback();
        }
    }

    /// <summary>Seeks a running playback; a click on an idle waveform only moves the cursor.</summary>
    public void SeekTo(double seconds)
    {
        LibMpvDynamicPlayer? player;
        lock (_playLock)
        {
            player = _player;
        }

        if (player != null && IsPlaying)
        {
            player.Position = Math.Max(0, seconds);
        }

        var av = AudioVisualizer;
        if (av != null)
        {
            av.CurrentVideoPositionSeconds = Math.Max(0, seconds);
            av.InvalidateVisual();
        }
    }

    private void StopPlayback()
    {
        _playbackTimer.Stop();
        IsPlaying = false;
        DisposePlayerOffThread();
        var av = AudioVisualizer;
        if (av != null)
        {
            av.CurrentVideoPositionSeconds = 0;
            av.InvalidateVisual();
        }
    }

    /// <summary>
    /// Tears the mpv core down on a worker thread: destroying it inline on the UI thread corrupts
    /// state that later blows up in the window's input pane teardown (#13376, #13567).
    /// </summary>
    private void DisposePlayerOffThread()
    {
        LibMpvDynamicPlayer? player;
        lock (_playLock)
        {
            player = _player;
            _player = null;
        }

        if (player == null)
        {
            return;
        }

        Task.Run(() =>
        {
            try
            {
                player.Stop();
                player.Dispose();
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "Voice manager: disposing the preview player failed");
            }
        });
    }

    #endregion

    #region Transcript

    [RelayCommand]
    private async Task SaveTranscript()
    {
        var row = SelectedVoice;
        if (row?.FilePath == null)
        {
            return;
        }

        if (!VoiceReferenceTranscript.Write(row.FilePath, Transcript, out var error))
        {
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return;
        }

        row.HasTranscript = !string.IsNullOrWhiteSpace(Transcript);
        IsTranscriptDirty = false;
        MarkChanged();
    }

    [RelayCommand]
    private async Task TranscribeWithSpeechToText()
    {
        var row = SelectedVoice;
        if (row?.FilePath == null)
        {
            return;
        }

        StopPlayback();
        var text = await RunSpeechToTextAsync(row.FilePath);
        if (!string.IsNullOrWhiteSpace(text))
        {
            Transcript = text;
        }
    }

    private async Task<string?> RunSpeechToTextAsync(string audioFileName)
    {
        if (Window == null)
        {
            return null;
        }

        var sttResult = await _windowService.ShowDialogAsync<SpeechToTextWindow, SpeechToTextViewModel>(Window, vm =>
        {
            vm.Initialize(audioFileName, -1);
        });

        if (!sttResult.OkPressed || sttResult.TranscribedSubtitle == null || sttResult.TranscribedSubtitle.Paragraphs.Count == 0)
        {
            return null;
        }

        return string.Join(' ', sttResult.TranscribedSubtitle.Paragraphs
            .Select(p => p.Text?.Replace('\n', ' ').Replace('\r', ' ').Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t)));
    }

    /// <summary>
    /// Asks for the transcript of <paramref name="audioFileName"/> for <paramref name="engine"/>,
    /// pre-filled with what is already known. Null means the user cancelled; an empty string is a
    /// deliberate "none" and is only offered when the engine can do without.
    /// </summary>
    private async Task<string?> PromptTranscriptAsync(ITtsEngine engine, string audioFileName, string? known)
    {
        var requirement = VoiceReferenceTranscript.GetRequirement(engine);
        if (requirement == TranscriptRequirement.NotUsed)
        {
            return known ?? string.Empty;
        }

        var initial = known ?? string.Empty;
        if (requirement == TranscriptRequirement.Required && string.IsNullOrWhiteSpace(initial))
        {
            // Auto-transcribe first so the user mostly just reviews; the prompt keeps the
            // speech-to-text button for a re-run.
            initial = await RunSpeechToTextAsync(audioFileName) ?? string.Empty;
        }

        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle, initial, 500, 150);
            vm.ConfigureExtraButton(Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot, () => RunSpeechToTextAsync(audioFileName));
        });

        if (!result.OkPressed)
        {
            return null;
        }

        var text = (result.Text ?? string.Empty).Trim();
        if (requirement == TranscriptRequirement.Required && text.Length == 0)
        {
            return null;
        }

        return text;
    }

    #endregion

    #region Rename / delete / copy / import

    [RelayCommand]
    private async Task RenameVoice()
    {
        var row = SelectedVoice;
        if (Window == null || row == null || !VoiceFileRename.CanRename(row.Voice))
        {
            return;
        }

        StopPlayback();
        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.Video.TextToSpeech.RenameVoiceTitle, row.Name, 400, 30, returnSubmits: true);
        });

        if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text) || result.Text.Trim() == row.Name)
        {
            return;
        }

        var newFileName = VoiceFileRename.Rename(row.Voice, result.Text, out var error);
        if (newFileName == null)
        {
            await MessageBox.Show(Window, Se.Language.Video.TextToSpeech.RenameVoiceTitle,
                string.Format(Se.Language.Video.TextToSpeech.VoiceXCouldNotBeRenamedX, row.Name, error), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MarkChanged();
        var newName = Path.GetFileNameWithoutExtension(newFileName).Replace('_', ' ');
        Renames.Add(new VoiceRename(SelectedEngine!.Name, row.Name, newName));
        await LoadVoicesAsync(SelectedEngine, newName);
    }

    [RelayCommand]
    private async Task DeleteVoice()
    {
        var row = SelectedVoice;
        if (Window == null || row == null || !VoiceFileRename.CanRename(row.Voice))
        {
            return;
        }

        StopPlayback();
        var answer = await MessageBox.Show(Window, Se.Language.Video.TextToSpeech.DeleteVoiceTitle,
            string.Format(Se.Language.Video.TextToSpeech.DeleteVoiceXQuestion, row.Name), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        var index = Voices.IndexOf(row);
        if (!VoiceFileRename.Delete(row.Voice, out var error))
        {
            await MessageBox.Show(Window, Se.Language.Video.TextToSpeech.DeleteVoiceTitle,
                string.Format(Se.Language.Video.TextToSpeech.VoiceXCouldNotBeDeletedX, row.Name, error), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MarkChanged();
        Deletes.Add(new VoiceRename(SelectedEngine!.Name, row.Name, string.Empty));
        var next = Voices.ElementAtOrDefault(index + 1) ?? Voices.ElementAtOrDefault(index - 1);
        await LoadVoicesAsync(SelectedEngine, next?.Name);
    }

    [RelayCommand]
    private async Task CopyVoiceTo(ITtsEngine? target)
    {
        var row = SelectedVoice;
        if (Window == null || target == null || row?.FilePath == null || !File.Exists(row.FilePath))
        {
            return;
        }

        StopPlayback();
        if (!await EnsureConsentAsync(target))
        {
            return;
        }

        // The target lists by file name, so a same-named voice there would be silently shadowed
        // or suffixed by the engine's unique-name logic; either way tell the user first.
        var existing = await SafeGetVoiceNamesAsync(target);
        if (existing.Contains(row.Name))
        {
            var overwrite = await MessageBox.Show(Window, Se.Language.Video.TextToSpeech.CopyVoiceTo,
                string.Format(Se.Language.Video.TextToSpeech.VoiceXAlreadyExistsInYContinue, row.Name, target.Name), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (overwrite != MessageBoxResult.Yes)
            {
                return;
            }
        }

        // The transcript travels with the voice: what is being edited if unsaved, else the sidecar.
        var transcript = IsTranscriptDirty ? Transcript.Trim() : VoiceReferenceTranscript.Read(row.FilePath) ?? string.Empty;
        if (VoiceReferenceTranscript.GetRequirement(target) != TranscriptRequirement.NotUsed && string.IsNullOrWhiteSpace(transcript))
        {
            var prompted = await PromptTranscriptAsync(target, row.FilePath, transcript);
            if (prompted == null)
            {
                return;
            }

            transcript = prompted;
        }

        var sourceFile = row.FilePath;
        StatusText = string.Format(Se.Language.Video.TextToSpeech.CopyingVoiceXToYDotDotDot, row.Name, target.Name);
        var ok = await Task.Run(() => VoiceCloneImporter.Import(target, sourceFile, transcript));
        if (!ok)
        {
            StatusText = string.Empty;
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.Video.TextToSpeech.VoiceXCouldNotBeCopiedToY, row.Name, target.Name), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ChangedEngineNames.Add(target.Name);
        var targetItem = Engines.FirstOrDefault(e => ReferenceEquals(e.Engine, target));
        if (targetItem != null && targetItem.HasCount)
        {
            targetItem.SetCount(existing.Count + (existing.Contains(row.Name) ? 0 : 1));
        }

        StatusText = string.Format(Se.Language.Video.TextToSpeech.VoiceXCopiedToY, row.Name, target.Name);
    }

    private static async Task<HashSet<string>> SafeGetVoiceNamesAsync(ITtsEngine engine)
    {
        try
        {
            var voices = await engine.GetVoices(string.Empty);
            return voices.Select(v => v.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Voice manager: listing voices for {engine.Name} failed");
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    [RelayCommand]
    private async Task ImportVoice()
    {
        var engine = SelectedEngine?.Engine;
        if (Window == null || engine == null)
        {
            return;
        }

        StopPlayback();
        if (!await EnsureConsentAsync(engine))
        {
            return;
        }

        string fileName;
        if (engine is Piper)
        {
            fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Video.TextToSpeech.ImportPiperVoiceTitle, "Piper voice model", "*.onnx");
        }
        else
        {
            fileName = await _fileHelper.PickOpenFile(Window, "Open audio file (for clone)", Se.Language.General.AudioFiles, "*.wav;*.mp3");
        }

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        await ImportFileAsync(engine, fileName);
    }

    internal async Task ImportFileAsync(ITtsEngine engine, string fileName)
    {
        if (Window == null)
        {
            return;
        }

        bool ok;
        if (engine is Piper piper)
        {
            ok = piper.ImportVoice(fileName);
            if (!ok)
            {
                await MessageBox.Show(Window, Se.Language.Video.TextToSpeech.PiperVoiceConfigMissingTitle,
                    string.Format(Se.Language.Video.TextToSpeech.PiperVoiceConfigMissingMessage, Path.GetFileName(fileName) + ".json"));
                return;
            }
        }
        else
        {
            if (!await EnsureConsentAsync(engine))
            {
                return;
            }

            var transcript = await PromptTranscriptAsync(engine, fileName, VoiceReferenceTranscript.Read(fileName));
            if (transcript == null)
            {
                return;
            }

            ok = await Task.Run(() => VoiceCloneImporter.Import(engine, fileName, transcript));
        }

        if (!ok)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.Video.TextToSpeech.VoiceXCouldNotBeImported, Path.GetFileName(fileName)), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MarkChanged();
        var baseName = Path.GetFileNameWithoutExtension(fileName).Replace('_', ' ');
        await LoadVoicesAsync(SelectedEngine, baseName);
        StatusText = string.Format(Se.Language.Video.TextToSpeech.VoiceXImported, Path.GetFileName(fileName));
    }

    private Task<bool> EnsureConsentAsync(ITtsEngine engine) =>
        VoiceCloneConsentPrompt.EnsureAsync(engine, Window!,
            () => _windowService.ShowDialogAsync<VoiceCloneConsentWindow, VoiceCloneConsentViewModel>(Window!, _ => { }));

    private void MarkChanged()
    {
        if (SelectedEngine != null)
        {
            ChangedEngineNames.Add(SelectedEngine.Name);
        }
    }

    #endregion

    [RelayCommand]
    private async Task OpenVoicesFolder()
    {
        if (Window != null && !string.IsNullOrEmpty(_voicesFolder) && Directory.Exists(_voicesFolder))
        {
            await _folderHelper.OpenFolder(Window, _voicesFolder);
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        var engine = SelectedEngine?.Engine;
        if (engine != null)
        {
            try
            {
                await engine.RefreshVoices(string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, $"Voice manager: refreshing voices for {engine.Name} failed");
            }
        }

        await LoadVoicesAsync(SelectedEngine, SelectedVoice?.Name);
    }

    [RelayCommand]
    private async Task DownloadVoicePacks()
    {
        var engine = SelectedEngine?.Engine;
        if (Window == null || engine == null)
        {
            return;
        }

        StopPlayback();
        var result = await _windowService.ShowDialogAsync<DownloadVoicePacksWindow, DownloadVoicePacksViewModel>(Window, vm =>
        {
            vm.Initialize(Engines.Where(e => e.IsCloning).Select(e => e.Engine).ToList(), engine);
        });

        if (result.InstalledCount > 0 && result.TargetEngine != null)
        {
            ChangedEngineNames.Add(result.TargetEngine.Name);
            var targetItem = Engines.FirstOrDefault(e => ReferenceEquals(e.Engine, result.TargetEngine));
            if (targetItem != null && !ReferenceEquals(targetItem, SelectedEngine))
            {
                SelectedEngine = targetItem;
            }
            else
            {
                await LoadVoicesAsync(SelectedEngine, SelectedVoice?.Name);
            }

            StatusText = string.Format(Se.Language.Video.TextToSpeech.XVoicesInstalledYSkipped, result.InstalledCount, result.SkippedCount);
        }
    }

    [RelayCommand]
    private void Close()
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
        else if (e.Key == Key.Space && IsPlayEnabled && e.Source is not TextBox)
        {
            e.Handled = true;
            _ = PlayOrStop();
        }
        else if (e.Key == Key.F2)
        {
            e.Handled = true;
            _ = RenameVoice();
        }
        else if (e.Key == Key.Delete && e.Source is not TextBox)
        {
            e.Handled = true;
            _ = DeleteVoice();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/text-to-speech", "voice-manager");
        }
    }

    internal void OnClosing()
    {
        _playbackTimer.Stop();
        DisposePlayerOffThread();
    }
}
