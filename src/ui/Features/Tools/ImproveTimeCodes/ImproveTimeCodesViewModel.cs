using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.EngineSettings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

public partial class ImproveTimeCodesViewModel : ObservableObject, IDisposable
{
    [ObservableProperty] private ObservableCollection<ForcedAlignerOption> _aligners;
    [ObservableProperty] private ForcedAlignerOption? _selectedAligner;
    [ObservableProperty] private string _engineStatus;
    [ObservableProperty] private IBrush? _engineDotBrush;
    [ObservableProperty] private bool _isEngineInstalled;
    [ObservableProperty] private decimal _maxShiftSeconds;
    [ObservableProperty] private bool _adjustStart;
    [ObservableProperty] private bool _adjustEnd;
    [ObservableProperty] private bool _isAligning;
    [ObservableProperty] private bool _isIdle;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _summaryLine;
    [ObservableProperty] private ObservableCollection<ImproveTimeCodesRow> _rows;
    [ObservableProperty] private ImproveTimeCodesRow? _selectedRow;
    [ObservableProperty] private string _changePositionLabel;
    [ObservableProperty] private bool _canGoPrevious;
    [ObservableProperty] private bool _canGoNext;
    [ObservableProperty] private bool _hasResult;
    [ObservableProperty] private AudioVisualizer? _audioVisualizerOriginal;
    [ObservableProperty] private AudioVisualizer? _audioVisualizerAligned;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>Asks the view to bring a row into view; the grid is the view's to scroll.</summary>
    public Action<ImproveTimeCodesRow>? ScrollRowIntoView { get; set; }

    private readonly IWindowService _windowService;
    private readonly CrispAsrParakeet _engine = new();
    private readonly List<SubtitleLineViewModel> _originalSubtitles = new();
    private readonly List<SubtitleLineViewModel> _alignedSubtitles = new();
    private string _videoFileName = string.Empty;
    private int _audioTrackNumber = -1;
    private CancellationTokenSource? _cancellation;
    private UiTickPump? _positionTimer;
    private bool _disposed;

    public ImproveTimeCodesViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _aligners = new ObservableCollection<ForcedAlignerOption>();
        _rows = new ObservableCollection<ImproveTimeCodesRow>();
        _engineStatus = string.Empty;
        _statusText = Se.Language.Tools.ImproveTimeCodes.Intro;
        _summaryLine = string.Empty;
        _changePositionLabel = string.Empty;
        _isIdle = true;

        var settings = Se.Settings.Tools.ImproveTimeCodes;
        _maxShiftSeconds = (decimal)Math.Clamp(settings.MaxShiftSeconds, 0.2, 10.0);
        _adjustStart = settings.AdjustStart;
        _adjustEnd = settings.AdjustEnd;
    }

    /// <param name="subtitles">The lines to re-time, sorted by start time.</param>
    /// <param name="audioTrackNumber">The ffmpeg stream index of the audio track in use, or -1 for ffmpeg's own pick.</param>
    public void Initialize(
        List<SubtitleLineViewModel> subtitles,
        AudioVisualizer audioVisualizer,
        string videoFileName,
        int audioTrackNumber,
        string? languageCode)
    {
        _videoFileName = videoFileName;
        _audioTrackNumber = audioTrackNumber;

        _originalSubtitles.Clear();
        _originalSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        ResetAligned();

        Rows.Clear();
        for (var i = 0; i < _originalSubtitles.Count; i++)
        {
            Rows.Add(new ImproveTimeCodesRow(i, _originalSubtitles[i], OnRowApplyChanged));
        }

        foreach (var option in ImproveTimeCodesAligners.Rank(languageCode))
        {
            option.IsInstalled = File.Exists(_engine.GetModelForCmdLine(option.FileName));
            Aligners.Add(option);
        }

        RefreshAlignerDisplay();

        // The list is ranked for this subtitle's language, so its head is the right default -
        // unless the user already settled on something that is installed.
        var configured = Se.Settings.Tools.ImproveTimeCodes.Aligner;
        SelectedAligner = Aligners.FirstOrDefault(a => a.Choice == configured && a.IsInstalled)
                          ?? Aligners.FirstOrDefault();

        RefreshEngineStatus();

        Dispatcher.UIThread.Post(() =>
        {
            if (AudioVisualizerOriginal == null || AudioVisualizerAligned == null || _disposed)
            {
                return;
            }

            foreach (var av in new[] { AudioVisualizerOriginal, AudioVisualizerAligned })
            {
                av.WavePeaks = audioVisualizer.WavePeaks;
                av.ShotChanges = new List<double>(audioVisualizer.ShotChanges ?? new List<double>());
                av.StartPositionSeconds = audioVisualizer.StartPositionSeconds;
                av.ZoomFactor = audioVisualizer.ZoomFactor;
                av.VerticalZoomFactor = audioVisualizer.VerticalZoomFactor;
                av.UpdateTheme();
            }

            // The click events only fire while something listens for position changes.
            AudioVisualizerOriginal.OnVideoPositionChanged += (_, _) => { };
            AudioVisualizerAligned.OnVideoPositionChanged += (_, _) => { };
            AudioVisualizerOriginal.OnPrimarySingleClicked += (_, e) => SelectRowAt(e.Seconds, _originalSubtitles);
            AudioVisualizerAligned.OnPrimarySingleClicked += (_, e) => SelectRowAt(e.Seconds, _alignedSubtitles);

            PushParagraphsToVisualizers();
            StartPositionTimer();
        });
    }

    private void ResetAligned()
    {
        _alignedSubtitles.Clear();
        _alignedSubtitles.AddRange(_originalSubtitles.Select(p => new SubtitleLineViewModel(p)));
    }

    private void RefreshAlignerDisplay()
    {
        var l = Se.Language.Tools.ImproveTimeCodes;
        foreach (var option in Aligners)
        {
            option.Display = option.IsInstalled
                ? option.BaseDisplay
                : $"{option.BaseDisplay}  -  {string.Format(l.ModelWillBeDownloaded, option.Size)}";
        }
    }

    private void RefreshEngineStatus()
    {
        IsEngineInstalled = File.Exists(_engine.GetExecutable());
        var status = IsEngineInstalled ? DownloadDotStatus.UpToDate : DownloadDotStatus.NotInstalled;
        EngineDotBrush = StatusDots.BrushFor(status);
        EngineStatus = IsEngineInstalled
            ? $"Crisp ASR {CrispAsrVersion.TryGet(_engine.GetExecutable()) ?? string.Empty}".TrimEnd()
            : $"Crisp ASR - {StatusDots.StatusText(status)}";
    }

    [RelayCommand]
    private async Task ShowEngineSettings()
    {
        if (Window == null)
        {
            return;
        }

        if (!File.Exists(_engine.GetExecutable()))
        {
            await DownloadEngineAsync();
            return;
        }

        await _windowService.ShowDialogAsync<SpeechToTextEngineSettingsWindow, SpeechToTextEngineSettingsViewModel>(
            Window,
            vm => vm.Initialize(_engine, DownloadEngineAsync));

        RefreshEngineStatus();
    }

    private async Task DownloadEngineAsync()
    {
        if (Window == null)
        {
            return;
        }

        await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
            Window, viewModel =>
            {
                viewModel.Engine = _engine;
                viewModel.StartDownload();
            });

        RefreshEngineStatus();
    }

    private async Task<string?> EnsureAlignerModelAsync(ForcedAlignerOption aligner)
    {
        var modelPath = _engine.GetModelForCmdLine(aligner.FileName);
        if (File.Exists(modelPath))
        {
            return modelPath;
        }

        var display = new SpeechToTextModelDisplay
        {
            Model = aligner.ToWhisperModel(),
            Engine = _engine,
        };

        var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
            Window!, viewModel =>
            {
                viewModel.SetModels(new ObservableCollection<SpeechToTextModelDisplay> { display }, _engine, display);
                viewModel.StartDownload();
            });

        if (!vm.OkPressed || !File.Exists(modelPath))
        {
            return null;
        }

        aligner.IsInstalled = true;
        RefreshAlignerDisplay();

        // Display is a plain property, so the combo only re-reads it when the item is replaced.
        var index = Aligners.IndexOf(aligner);
        if (index >= 0)
        {
            Aligners.RemoveAt(index);
            Aligners.Insert(index, aligner);
            SelectedAligner = aligner;
        }

        return modelPath;
    }

    [RelayCommand]
    private async Task Align()
    {
        if (Window == null || SelectedAligner == null || IsAligning)
        {
            return;
        }

        var l = Se.Language.Tools.ImproveTimeCodes;

        if (!File.Exists(_engine.GetExecutable()))
        {
            await DownloadEngineAsync();
            if (!File.Exists(_engine.GetExecutable()))
            {
                return;
            }
        }

        var aligner = SelectedAligner;
        var modelPath = await EnsureAlignerModelAsync(aligner);
        if (modelPath == null)
        {
            return;
        }

        SaveSettings();

        IsAligning = true;
        IsIdle = false;
        HasResult = false;
        ProgressValue = 0;
        StatusText = l.ExtractingAudio;
        _cancellation = new CancellationTokenSource();
        var cancellationToken = _cancellation.Token;
        var workFolder = Path.Combine(Path.GetTempPath(), "se-improve-time-codes-" + Guid.NewGuid());

        try
        {
            Directory.CreateDirectory(workFolder);
            var audioFileName = Path.Combine(workFolder, "audio.wav");
            if (!await ExtractAudioAsync(audioFileName, cancellationToken))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, l.ExtractAudioFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
                StatusText = l.Intro;
                return;
            }

            // We wrote this file ourselves: 16 kHz, mono, 16-bit, behind a 44 byte header.
            var totalSeconds = Math.Max(0, new FileInfo(audioFileName).Length - 44) / (16000.0 * 2);

            var general = Se.Settings.General;
            var options = new SubtitleRetimer.Options
            {
                MaxShiftSeconds = (double)MaxShiftSeconds,
                AdjustStart = AdjustStart,
                AdjustEnd = AdjustEnd,
                MinDurationSeconds = general.SubtitleMinimumDisplayMilliseconds / 1000.0,
                ReadingCharsPerSecond = general.SubtitleOptimalCharactersPerSeconds,
                MinGapSeconds = general.MinimumBetweenLines.GetMilliseconds() / 1000.0,
            };

            var lines = _originalSubtitles
                .Select(p => new SubtitleRetimer.Line(p.Text ?? string.Empty, p.StartTime.TotalSeconds, p.EndTime.TotalSeconds))
                .ToList();

            var progress = new Progress<SubtitleRetimer.Progress>(p =>
            {
                ProgressValue = p.Percent;
                StatusText = string.Format(l.AligningBatchXOfY, p.BatchIndex, p.BatchCount);
            });

            using var audio = new FfmpegWindowAudioSource(GetFfmpegPath(), audioFileName, totalSeconds, workFolder);
            var runner = new CrispAsrAlignOnlyRunner(_engine.GetExecutable(), modelPath, Se.WriteToolsLog);
            var results = await Task.Run(
                () => new SubtitleRetimer(runner, audio, options).RetimeAsync(lines, progress, cancellationToken),
                cancellationToken);

            ShowResults(results);
        }
        catch (OperationCanceledException)
        {
            StatusText = l.Intro;
        }
        catch (ForcedAlignerException exception)
        {
            Se.WriteToolsLog(exception.Detail);
            StatusText = exception.Message;
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Improve time codes failed");
            StatusText = exception.Message;
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            IsAligning = false;
            IsIdle = true;
            _cancellation?.Dispose();
            _cancellation = null;
            TryDeleteFolder(workFolder);
        }
    }

    private void ShowResults(IReadOnlyList<SubtitleRetimer.LineResult> results)
    {
        for (var i = 0; i < Rows.Count && i < results.Count; i++)
        {
            Rows[i].SetResult(results[i]);
        }

        RebuildAligned();

        var l = Se.Language.Tools.ImproveTimeCodes;
        var retimed = Rows.Where(r => r.IsChanged).ToList();
        var noSpeech = Rows.Count(r => r.Status == SubtitleRetimer.LineStatus.NoSpeech);
        var meanShiftMs = retimed.Count == 0
            ? 0
            : retimed.Average(r => (Math.Abs(r.StartShiftMs) + Math.Abs(r.EndShiftMs)) / 2.0);

        SummaryLine = string.Format(l.SummaryXRetimedYKeptZSkipped, retimed.Count, Rows.Count - retimed.Count - noSpeech, noSpeech) +
                      "   ·   " + string.Format(l.MeanShiftX, meanShiftMs.ToString("0", CultureInfo.InvariantCulture));
        StatusText = string.Empty;
        ProgressValue = 100;
        HasResult = retimed.Count > 0;

        SelectedRow = retimed.FirstOrDefault();
        if (SelectedRow == null)
        {
            UpdateChangeNavigation();
        }
    }

    /// <summary>The aligned list is the original with every ticked, re-timed row's new times laid over it.</summary>
    private void RebuildAligned()
    {
        for (var i = 0; i < Rows.Count; i++)
        {
            var row = Rows[i];
            var target = _alignedSubtitles[i];
            var source = _originalSubtitles[i];
            if (row.IsChanged && row.Apply)
            {
                target.StartTime = TimeSpan.FromSeconds(row.NewStartSeconds);
                target.EndTime = TimeSpan.FromSeconds(row.NewEndSeconds);
            }
            else
            {
                target.StartTime = source.StartTime;
                target.EndTime = source.EndTime;
            }

            target.UpdateDuration();
        }

        PushParagraphsToVisualizers();
    }

    private void OnRowApplyChanged(ImproveTimeCodesRow row)
    {
        RebuildAligned();
        HasResult = Rows.Any(r => r.IsChanged && r.Apply);
    }

    partial void OnSelectedRowChanged(ImproveTimeCodesRow? value)
    {
        UpdateChangeNavigation();
        if (value == null)
        {
            PushParagraphsToVisualizers();
            return;
        }

        var aligned = _alignedSubtitles[value.Index];
        var midSeconds = (aligned.StartTime.TotalSeconds + aligned.EndTime.TotalSeconds) / 2.0;
        CenterVisualizerOn(AudioVisualizerOriginal, midSeconds);
        CenterVisualizerOn(AudioVisualizerAligned, midSeconds);
        PushParagraphsToVisualizers();
    }

    private void UpdateChangeNavigation()
    {
        var changed = Rows.Where(r => r.IsChanged).ToList();
        var index = SelectedRow == null ? -1 : SelectedRow.Index;
        CanGoPrevious = changed.Any(r => r.Index < index);
        CanGoNext = changed.Any(r => r.Index > index);

        var position = SelectedRow != null ? changed.IndexOf(SelectedRow) : -1;
        ChangePositionLabel = position >= 0
            ? string.Format(Se.Language.Tools.ImproveTimeCodes.ChangeXOfY, position + 1, changed.Count)
            : string.Empty;
    }

    [RelayCommand]
    private void PreviousChange()
    {
        var index = SelectedRow?.Index ?? Rows.Count;
        SelectAndScroll(Rows.LastOrDefault(r => r.IsChanged && r.Index < index));
    }

    [RelayCommand]
    private void NextChange()
    {
        var index = SelectedRow?.Index ?? -1;
        SelectAndScroll(Rows.FirstOrDefault(r => r.IsChanged && r.Index > index));
    }

    private void SelectAndScroll(ImproveTimeCodesRow? row)
    {
        if (row == null)
        {
            return;
        }

        SelectedRow = row;
        ScrollRowIntoView?.Invoke(row);
    }

    /// <summary>
    /// A click in either waveform selects the line under it. The visualizer hands out clones
    /// of its paragraphs, so the line is found by time rather than by reference.
    /// </summary>
    private void SelectRowAt(double seconds, List<SubtitleLineViewModel> subtitles)
    {
        var index = subtitles.FindIndex(p => p.StartTime.TotalSeconds <= seconds && seconds <= p.EndTime.TotalSeconds);
        if (index >= 0 && index < Rows.Count)
        {
            SelectAndScroll(Rows[index]);
        }
    }

    private void PushParagraphsToVisualizers()
    {
        var index = SelectedRow?.Index ?? -1;
        Push(AudioVisualizerOriginal, _originalSubtitles, index);
        Push(AudioVisualizerAligned, _alignedSubtitles, index);
    }

    private static void Push(AudioVisualizer? av, List<SubtitleLineViewModel> subtitles, int selectedIndex)
    {
        if (av == null)
        {
            return;
        }

        av.SetPosition(av.StartPositionSeconds, subtitles, av.CurrentVideoPositionSeconds, selectedIndex, new List<SubtitleLineViewModel>());
        av.InvalidateVisual();
    }

    private static void CenterVisualizerOn(AudioVisualizer? av, double seconds)
    {
        var peaks = av?.WavePeaks;
        if (av == null || peaks == null || av.Bounds.Width <= 0 || av.ZoomFactor <= 0)
        {
            return;
        }

        var visibleSeconds = av.Bounds.Width / (av.ZoomFactor * peaks.SampleRate);
        av.StartPositionSeconds = Math.Max(0, seconds - (visibleSeconds / 2.0));
    }

    private void StartPositionTimer()
    {
        _positionTimer = new UiTickPump(TimeSpan.FromMilliseconds(100));
        _positionTimer.Tick += (_, _) =>
        {
            if (AudioVisualizerOriginal != null && AudioVisualizerAligned != null)
            {
                AudioVisualizerAligned.CurrentVideoPositionSeconds = AudioVisualizerOriginal.CurrentVideoPositionSeconds;
                AudioVisualizerOriginal.InvalidateVisual();
                AudioVisualizerAligned.InvalidateVisual();
            }
        };
        _positionTimer.Start();
    }

    private async Task<bool> ExtractAudioAsync(string audioFileName, CancellationToken cancellationToken)
    {
        // 16 kHz mono PCM - what every CTC aligner expects.
        var map = SpeechToTextViewModel.BuildAudioMapParameter(_videoFileName, _audioTrackNumber, _videoFileName);
        var arguments =
            $"-hide_banner -nostats -loglevel error -y -i \"{_videoFileName}\" -vn {map} -ar 16000 -ac 1 -acodec pcm_s16le \"{audioFileName}\"";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(GetFfmpegPath(), arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            },
        };

        try
        {
            process.Start();
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Could not start ffmpeg");
            return false;
        }

        // Drain both pipes, or a chatty ffmpeg blocks on a full buffer and never exits.
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
                // It exited on its own in the meantime.
            }

            throw;
        }

        return process.ExitCode == 0 && File.Exists(audioFileName);
    }

    private static string GetFfmpegPath()
    {
        var path = Se.Settings.General.FfmpegPath;
        return File.Exists(path) ? path : "ffmpeg";
    }

    private static void TryDeleteFolder(string folder)
    {
        try
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
        catch
        {
            // Temp cleanup is best effort.
        }
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Tools.ImproveTimeCodes;
        settings.Aligner = SelectedAligner?.Choice ?? string.Empty;
        settings.MaxShiftSeconds = (double)MaxShiftSeconds;
        settings.AdjustStart = AdjustStart;
        settings.AdjustEnd = AdjustEnd;
    }

    /// <summary>The input lines, in input order, carrying the accepted new times.</summary>
    public List<SubtitleLineViewModel> GetAlignedSubtitles() => new(_alignedSubtitles);

    [RelayCommand]
    private void Ok()
    {
        if (IsAligning || !HasResult)
        {
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsAligning)
        {
            // First Cancel stops the run and leaves the window open on whatever it had.
            _cancellation?.Cancel();
            return;
        }

        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/improve-time-codes");
        }
        else if (e.Key == Key.F8 || (e.Key == Key.Down && e.KeyModifiers == KeyModifiers.Alt))
        {
            e.Handled = true;
            NextChange();
        }
        else if (e.Key == Key.F7 || (e.Key == Key.Up && e.KeyModifiers == KeyModifiers.Alt))
        {
            e.Handled = true;
            PreviousChange();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _positionTimer?.Stop();
        _positionTimer = null;

        try
        {
            _cancellation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The run finished between the null check and Cancel().
        }
    }
}
