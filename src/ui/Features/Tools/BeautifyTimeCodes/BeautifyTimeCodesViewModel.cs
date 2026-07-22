using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes.Profile;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

public partial class BeautifyTimeCodesViewModel : ObservableObject, IDisposable
{
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private Avalonia.Threading.DispatcherTimer? _positionTimer;
    private volatile bool _dirty;
    private volatile bool _updateInProgress;
    private readonly Lock _timerLock = new Lock();
    private readonly List<SubtitleLineViewModel> _allSubtitles;
    private readonly List<SubtitleLineViewModel> _originalSubtitles;
    private readonly List<SubtitleLineViewModel> _beautifiedSubtitles;
    private List<double> _shotChanges;
    private double _frameRate = 25.0;
    private volatile bool _disposed;

    private readonly IWindowService _windowService;

    /// <summary>Indices into _beautifiedSubtitles whose start or end differs from the original.</summary>
    private readonly List<int> _changedIndices = new();
    private int _currentChangeIndex = -1;

    [ObservableProperty] private Controls.AudioVisualizerControl.AudioVisualizer? _audioVisualizerOriginal;
    [ObservableProperty] private Controls.AudioVisualizerControl.AudioVisualizer? _audioVisualizerBeautified;

    [ObservableProperty] private string _statsLine = string.Empty;
    [ObservableProperty] private string _changePositionLabel = string.Empty;
    [ObservableProperty] private string _changeDetail = string.Empty;
    [ObservableProperty] private string _changeNotes = string.Empty;
    [ObservableProperty] private bool _hasChanges;
    [ObservableProperty] private bool _canGoPrevious;
    [ObservableProperty] private bool _canGoNext;

    public BeautifyTimeCodesViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _allSubtitles = new List<SubtitleLineViewModel>();
        _originalSubtitles = new List<SubtitleLineViewModel>();
        _beautifiedSubtitles = new List<SubtitleLineViewModel>();
        _shotChanges = new List<double>();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.AutoReset = false;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
    }

    private void TimerUpdatePreviewElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (!_dirty || _updateInProgress)
        {
            _timerUpdatePreview.Start();
            return;
        }

        lock (_timerLock)
        {
            if (!_dirty || _updateInProgress)
            {
                _timerUpdatePreview.Start();
                return;
            }

            _dirty = false;
            _updateInProgress = true;
        }

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (AudioVisualizerBeautified == null || _allSubtitles.Count == 0)
        {
            _updateInProgress = false;
            if (!_disposed)
            {
                _timerUpdatePreview.Start();
            }
            return;
        }

        // Build a Subtitle from the current row values and run the full libse
        // beautifier — it reads the profile directly from Configuration.Settings.BeautifyTimeCodes.
        var subtitle = BuildSubtitleFromRows();

        var beautifier = new Core.Forms.TimeCodesBeautifier(subtitle, _frameRate, new List<double>(), _shotChanges);
        beautifier.Beautify();

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (!_disposed && AudioVisualizerBeautified != null)
            {
                _beautifiedSubtitles.Clear();
                var subRipFormat = new SubRip();
                for (int i = 0; i < subtitle.Paragraphs.Count; i++)
                {
                    var p = subtitle.Paragraphs[i];
                    var vm = new SubtitleLineViewModel(p, subRipFormat) { Number = i + 1 };
                    _beautifiedSubtitles.Add(vm);
                }

                // Push the paragraphs into the visualizer's _displayableParagraphs via SetPosition.
                PushParagraphsToVisualizers();
                RecomputeChanges();
            }

            _updateInProgress = false;
            if (!_disposed)
            {
                _timerUpdatePreview.Start();
            }
        });
    }

    private void PushParagraphsToVisualizers()
    {
        if (AudioVisualizerOriginal != null)
        {
            AudioVisualizerOriginal.SetPosition(
                AudioVisualizerOriginal.StartPositionSeconds,
                _originalSubtitles,
                AudioVisualizerOriginal.CurrentVideoPositionSeconds,
                -1,
                new List<SubtitleLineViewModel>());
        }

        if (AudioVisualizerBeautified != null)
        {
            AudioVisualizerBeautified.SetPosition(
                AudioVisualizerBeautified.StartPositionSeconds,
                _beautifiedSubtitles,
                AudioVisualizerBeautified.CurrentVideoPositionSeconds,
                -1,
                new List<SubtitleLineViewModel>());
        }
    }

    /// <summary>Walk original vs beautified and rebuild the navigable list of differences.</summary>
    private void RecomputeChanges()
    {
        _changedIndices.Clear();

        var n = Math.Min(_originalSubtitles.Count, _beautifiedSubtitles.Count);
        for (var i = 0; i < n; i++)
        {
            var o = _originalSubtitles[i];
            var b = _beautifiedSubtitles[i];
            if (Math.Abs(o.StartTime.TotalMilliseconds - b.StartTime.TotalMilliseconds) > 0.5 ||
                Math.Abs(o.EndTime.TotalMilliseconds - b.EndTime.TotalMilliseconds) > 0.5)
            {
                _changedIndices.Add(i);
            }
        }

        HasChanges = _changedIndices.Count > 0;
        if (_currentChangeIndex < 0 || _currentChangeIndex >= _changedIndices.Count)
        {
            _currentChangeIndex = _changedIndices.Count > 0 ? 0 : -1;
        }

        UpdateStatsLine();
        UpdateChangeView();
    }

    private void UpdateStatsLine()
    {
        var lang = Se.Language.Tools.BeautifyTimeCodes;
        var fps = _frameRate.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        StatsLine = $"{lang.SubtitlesCount}: {_originalSubtitles.Count}   ·   " +
                    $"{lang.ChangedCount}: {_changedIndices.Count}   ·   " +
                    $"{Se.Language.General.FrameRate}: {fps}   ·   " +
                    $"{lang.ShotChangesCount}: {_shotChanges.Count}";
    }

    private void UpdateChangeView()
    {
        if (_currentChangeIndex < 0 || _changedIndices.Count == 0)
        {
            ChangePositionLabel = string.Empty;
            ChangeDetail = Se.Language.Tools.BeautifyTimeCodes.NoChanges;
            ChangeNotes = string.Empty;
            CanGoPrevious = false;
            CanGoNext = false;
            if (AudioVisualizerOriginal != null)
            {
                AudioVisualizerOriginal.AllSelectedParagraphs = new List<SubtitleLineViewModel>();
            }
            if (AudioVisualizerBeautified != null)
            {
                AudioVisualizerBeautified.AllSelectedParagraphs = new List<SubtitleLineViewModel>();
            }
            return;
        }

        CanGoPrevious = _currentChangeIndex > 0;
        CanGoNext = _currentChangeIndex < _changedIndices.Count - 1;
        ChangePositionLabel = string.Format(
            Se.Language.Tools.BeautifyTimeCodes.ChangeXOfY,
            _currentChangeIndex + 1, _changedIndices.Count);

        var idx = _changedIndices[_currentChangeIndex];
        var o = _originalSubtitles[idx];
        var b = _beautifiedSubtitles[idx];

        ChangeDetail = BuildChangeDetail(o, b);
        ChangeNotes = BuildChangeNotes(o, b);

        // Center both visualizers on the *midpoint* of the (beautified) paragraph
        var midSeconds = (b.StartTime.TotalSeconds + b.EndTime.TotalSeconds) / 2.0;
        CenterVisualizerOn(AudioVisualizerOriginal, midSeconds);
        CenterVisualizerOn(AudioVisualizerBeautified, midSeconds);

        if (AudioVisualizerOriginal != null)
        {
            AudioVisualizerOriginal.SetPosition(
                AudioVisualizerOriginal.StartPositionSeconds,
                _originalSubtitles,
                AudioVisualizerOriginal.CurrentVideoPositionSeconds,
                idx,
                new List<SubtitleLineViewModel>());
            AudioVisualizerOriginal.InvalidateVisual();
        }

        if (AudioVisualizerBeautified != null)
        {
            AudioVisualizerBeautified.SetPosition(
                AudioVisualizerBeautified.StartPositionSeconds,
                _beautifiedSubtitles,
                AudioVisualizerBeautified.CurrentVideoPositionSeconds,
                idx,
                new List<SubtitleLineViewModel>());
            AudioVisualizerBeautified.InvalidateVisual();
        }
    }

    private string BuildChangeDetail(SubtitleLineViewModel original, SubtitleLineViewModel beautified)
    {
        var sb = new System.Text.StringBuilder();

        sb.Append('#').Append(beautified.Number).Append("   ");

        var startDeltaMs = beautified.StartTime.TotalMilliseconds - original.StartTime.TotalMilliseconds;
        if (Math.Abs(startDeltaMs) > 0.5)
        {
            sb.Append(Se.Language.General.StartTime).Append(": ")
              .Append(FormatTime(original.StartTime))
              .Append(" → ")
              .Append(FormatTime(beautified.StartTime))
              .Append("  ")
              .Append(FormatDelta(startDeltaMs));
        }

        var endDeltaMs = beautified.EndTime.TotalMilliseconds - original.EndTime.TotalMilliseconds;
        if (Math.Abs(endDeltaMs) > 0.5)
        {
            if (sb.Length > 6)
            {
                sb.Append("    ");
            }
            sb.Append(Se.Language.General.EndTime).Append(": ")
              .Append(FormatTime(original.EndTime))
              .Append(" → ")
              .Append(FormatTime(beautified.EndTime))
              .Append("  ")
              .Append(FormatDelta(endDeltaMs));
        }

        return sb.ToString();
    }

    private string BuildChangeNotes(SubtitleLineViewModel original, SubtitleLineViewModel beautified)
    {
        var lang = Se.Language.Tools.BeautifyTimeCodes;
        var idx = _beautifiedSubtitles.IndexOf(beautified);
        var prevB = idx > 0 ? _beautifiedSubtitles[idx - 1] : null;
        var nextB = (idx >= 0 && idx < _beautifiedSubtitles.Count - 1) ? _beautifiedSubtitles[idx + 1] : null;

        var parts = new System.Collections.Generic.List<string>();

        var startChanged = Math.Abs(beautified.StartTime.TotalMilliseconds - original.StartTime.TotalMilliseconds) > 0.5;
        if (startChanged)
        {
            var reason = DetectStartReason(original, beautified, prevB) ?? lang.NoReasonNote;
            parts.Add(Se.Language.General.StartTime + ": " + reason);
        }

        var endChanged = Math.Abs(beautified.EndTime.TotalMilliseconds - original.EndTime.TotalMilliseconds) > 0.5;
        if (endChanged)
        {
            var reason = DetectEndReason(original, beautified, nextB) ?? lang.NoReasonNote;
            parts.Add(Se.Language.General.EndTime + ": " + reason);
        }

        // Duration reason — applies regardless of which side moved
        var durationReason = DetectDurationReason(original, beautified);
        if (durationReason != null)
        {
            parts.Add(Se.Language.General.Duration + ": " + durationReason);
        }

        return string.Join("    ·    ", parts);
    }

    private string? DetectStartReason(SubtitleLineViewModel original, SubtitleLineViewModel beautified, SubtitleLineViewModel? prev)
    {
        if (_frameRate <= 0)
        {
            return null;
        }

        var snap = DetectShotChangeSnap(beautified.StartTime, isOutCue: false);
        if (snap != null)
        {
            return snap;
        }

        // Min-gap-to-previous: new start equals previous end + min-gap
        if (prev != null)
        {
            var minGapMs = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            var expected = prev.EndTime.TotalMilliseconds + minGapMs;
            var halfFrame = (1.0 / _frameRate) * 500.0;
            if (Math.Abs(beautified.StartTime.TotalMilliseconds - expected) <= halfFrame)
            {
                return Se.Language.Tools.BeautifyTimeCodes.MinGapEnforced;
            }
        }

        return DetectFrameSnap(original.StartTime, beautified.StartTime);
    }

    private string? DetectEndReason(SubtitleLineViewModel original, SubtitleLineViewModel beautified, SubtitleLineViewModel? next)
    {
        if (_frameRate <= 0)
        {
            return null;
        }

        var snap = DetectShotChangeSnap(beautified.EndTime, isOutCue: true);
        if (snap != null)
        {
            return snap;
        }

        if (next != null)
        {
            var minGapMs = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            var expected = next.StartTime.TotalMilliseconds - minGapMs;
            var halfFrame = (1.0 / _frameRate) * 500.0;
            if (Math.Abs(beautified.EndTime.TotalMilliseconds - expected) <= halfFrame)
            {
                return Se.Language.Tools.BeautifyTimeCodes.MinGapEnforced;
            }
        }

        return DetectFrameSnap(original.EndTime, beautified.EndTime);
    }

    private string? DetectShotChangeSnap(TimeSpan beautifiedT, bool isOutCue)
    {
        if (_shotChanges.Count == 0 || _frameRate <= 0)
        {
            return null;
        }

        var oneFrame = 1.0 / _frameRate;
        var refSec = isOutCue ? beautifiedT.TotalSeconds + oneFrame : beautifiedT.TotalSeconds;
        foreach (var sc in _shotChanges)
        {
            if (Math.Abs(sc - refSec) <= oneFrame)
            {
                return Se.Language.Tools.BeautifyTimeCodes.SnappedToShotChange;
            }
        }
        return null;
    }

    private string? DetectFrameSnap(TimeSpan originalT, TimeSpan beautifiedT)
    {
        var origFrames = originalT.TotalSeconds * _frameRate;
        var newFrames = beautifiedT.TotalSeconds * _frameRate;
        var origAligned = Math.Abs(origFrames - Math.Round(origFrames)) < 0.05;
        var newAligned = Math.Abs(newFrames - Math.Round(newFrames)) < 0.05;
        if (newAligned && !origAligned)
        {
            return Se.Language.Tools.BeautifyTimeCodes.SnappedToFrame;
        }
        return null;
    }

    private string? DetectDurationReason(SubtitleLineViewModel original, SubtitleLineViewModel beautified)
    {
        var newDuration = beautified.EndTime.TotalMilliseconds - beautified.StartTime.TotalMilliseconds;
        var oldDuration = original.EndTime.TotalMilliseconds - original.StartTime.TotalMilliseconds;
        if (Math.Abs(newDuration - oldDuration) < 1.0)
        {
            return null; // duration didn't change meaningfully
        }

        var minMs = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var maxMs = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
        var tol = _frameRate > 0 ? 1000.0 / _frameRate * 0.5 : 5.0; // half-frame tolerance

        if (oldDuration < minMs && Math.Abs(newDuration - minMs) <= tol)
        {
            return Se.Language.Tools.BeautifyTimeCodes.MinDurationEnforced;
        }
        if (oldDuration > maxMs && Math.Abs(newDuration - maxMs) <= tol)
        {
            return Se.Language.Tools.BeautifyTimeCodes.MaxDurationEnforced;
        }

        return null;
    }

    private string FormatTime(TimeSpan t) =>
        $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2},{t.Milliseconds:D3}";

    private string FormatDelta(double ms)
    {
        var sign = ms >= 0 ? "+" : "−"; // U+2212 minus for nicer look
        var absMs = Math.Abs(ms);
        if (_frameRate > 0)
        {
            var frames = absMs * _frameRate / 1000.0;
            return $"({sign}{absMs:0} ms / {sign}{frames:0.#} f)";
        }
        return $"({sign}{absMs:0} ms)";
    }

    private void CenterVisualizerOn(Controls.AudioVisualizerControl.AudioVisualizer? av, double seconds)
    {
        if (av == null)
        {
            return;
        }

        var peaks = av.WavePeaks;
        if (peaks == null || av.Bounds.Width <= 0 || av.ZoomFactor <= 0)
        {
            return;
        }

        var visibleSeconds = av.Bounds.Width / (av.ZoomFactor * peaks.SampleRate);
        av.StartPositionSeconds = Math.Max(0, seconds - visibleSeconds / 2.0);
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles, Controls.AudioVisualizerControl.AudioVisualizer audioVisualizer, string videoFileName)
    {
        _allSubtitles.Clear();
        _allSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));

        _originalSubtitles.Clear();
        _originalSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));

        _shotChanges = audioVisualizer.ShotChanges ?? new List<double>();

        _frameRate = 25.0;
        if (!string.IsNullOrEmpty(videoFileName) && System.IO.File.Exists(videoFileName))
        {
            try
            {
                var mediaInfo = Logic.Media.FfmpegMediaInfo2.Parse(videoFileName);
                if (mediaInfo.FramesRate > 0)
                {
                    _frameRate = (double)mediaInfo.FramesRate;
                }
            }
            catch
            {
                // fall back to 25 fps
            }
        }

        UpdateStatsLine();

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (AudioVisualizerOriginal == null || AudioVisualizerBeautified == null)
            {
                return;
            }

            AudioVisualizerOriginal.WavePeaks = audioVisualizer.WavePeaks;
            AudioVisualizerOriginal.ShotChanges = new List<double>(_shotChanges);
            AudioVisualizerOriginal.StartPositionSeconds = audioVisualizer.StartPositionSeconds;
            AudioVisualizerOriginal.ZoomFactor = audioVisualizer.ZoomFactor;
            AudioVisualizerOriginal.VerticalZoomFactor = audioVisualizer.VerticalZoomFactor;
            AudioVisualizerOriginal.UpdateTheme();

            AudioVisualizerBeautified.WavePeaks = audioVisualizer.WavePeaks;
            AudioVisualizerBeautified.ShotChanges = new List<double>(_shotChanges);
            AudioVisualizerBeautified.StartPositionSeconds = audioVisualizer.StartPositionSeconds;
            AudioVisualizerBeautified.ZoomFactor = audioVisualizer.ZoomFactor;
            AudioVisualizerBeautified.VerticalZoomFactor = audioVisualizer.VerticalZoomFactor;
            AudioVisualizerBeautified.UpdateTheme();

            // Push original paragraphs immediately so the user sees them while the
            // first beautify pass runs in the background.
            AudioVisualizerOriginal.SetPosition(
                AudioVisualizerOriginal.StartPositionSeconds,
                _originalSubtitles,
                AudioVisualizerOriginal.CurrentVideoPositionSeconds,
                -1,
                new List<SubtitleLineViewModel>());

            _dirty = true;
            AudioVisualizerOriginal.InvalidateVisual();
            AudioVisualizerBeautified.InvalidateVisual();

            _timerUpdatePreview.Start();
            StartPositionTimer();
        });
    }

    private void StartPositionTimer()
    {
        _positionTimer = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _positionTimer.Tick += (s, e) =>
        {
            if (AudioVisualizerOriginal != null && AudioVisualizerBeautified != null)
            {
                AudioVisualizerBeautified.CurrentVideoPositionSeconds = AudioVisualizerOriginal.CurrentVideoPositionSeconds;
                AudioVisualizerOriginal.InvalidateVisual();
                AudioVisualizerBeautified.InvalidateVisual();
            }
        };
        _positionTimer.Start();
    }

    private void StopPositionTimer()
    {
        if (_positionTimer != null)
        {
            _positionTimer.Stop();
            _positionTimer = null;
        }
    }

    [RelayCommand]
    private async Task EditProfile()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BeautifyTimeCodesProfileWindow, BeautifyTimeCodesProfileViewModel>(Window, vm =>
        {
            vm.Initialize();
        });

        if (result.OkPressed)
        {
            _dirty = true; // re-run beautify with the new profile
        }
    }

    [RelayCommand]
    private void PreviousChange()
    {
        if (_currentChangeIndex > 0)
        {
            _currentChangeIndex--;
            UpdateChangeView();
        }
    }

    [RelayCommand]
    private void NextChange()
    {
        if (_currentChangeIndex < _changedIndices.Count - 1)
        {
            _currentChangeIndex++;
            UpdateChangeView();
        }
    }

    [RelayCommand]
    private void Ok()
    {
        StopPositionTimer();
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);

        // Apply final beautification to commit the result back to _allSubtitles
        var subtitle = BuildSubtitleFromRows();

        var beautifier = new Core.Forms.TimeCodesBeautifier(subtitle, _frameRate, new List<double>(), _shotChanges);
        beautifier.Beautify();

        _allSubtitles.Clear();
        var subRipFormat = new SubRip();
        foreach (var p in subtitle.Paragraphs)
        {
            _allSubtitles.Add(new SubtitleLineViewModel(p, subRipFormat));
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        StopPositionTimer();
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
        Window?.Close();
    }

    public List<SubtitleLineViewModel> GetBeautifiedSubtitles()
    {
        return new List<SubtitleLineViewModel>(_allSubtitles);
    }

    private Subtitle BuildSubtitleFromRows()
    {
        var subtitle = new Subtitle();
        foreach (var p in _allSubtitles.Select(p => p.ToParagraph()).OrderBy(p => p.StartTime.TotalMilliseconds))
        {
            subtitle.Paragraphs.Add(p);
        }

        return subtitle;
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
            UiUtil.ShowHelp("features/beautify-time-codes");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopPositionTimer();
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
    }
}
