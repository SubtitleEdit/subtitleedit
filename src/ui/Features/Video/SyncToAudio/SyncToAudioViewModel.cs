using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SyncToAudio;

/// <summary>
/// Video > More > Sync subtitle to audio: runs <see cref="AudioSubtitleSync"/> against the speech
/// in the loaded video's waveform (or against another subtitle that is already in sync), shows
/// the offset, frame-rate change and confidence it found, and hands the result to the main window
/// to apply on Apply.
/// </summary>
public partial class SyncToAudioViewModel : ObservableObject
{
    [ObservableProperty] private bool _useAudioReference = true;
    [ObservableProperty] private bool _useSubtitleReference;
    [ObservableProperty] private bool _isAudioReferenceAvailable;
    [ObservableProperty] private string _referenceSubtitleFileName = string.Empty;
    [ObservableProperty] private string _referenceSubtitleDisplayName = string.Empty;
    [ObservableProperty] private int _maxOffsetSeconds;
    [ObservableProperty] private bool _tryFrameRates;
    [ObservableProperty] private bool _isAnalyzing;
    // Hold off system sleep while the job runs (#16183); the display may still turn off.
    private readonly SleepInhibitorScope _sleepInhibitor = new(Se.Language.Video.SyncToAudioTitle);

    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private bool _hasResult;
    [ObservableProperty] private string _resultOffsetText = string.Empty;
    [ObservableProperty] private string _resultFrameRateText = string.Empty;
    [ObservableProperty] private string _resultConfidenceText = string.Empty;
    [ObservableProperty] private bool _isLowConfidence;
    [ObservableProperty] private string _errorText = string.Empty;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public AudioSubtitleSync.Result? Result { get; private set; }

    private readonly IFileHelper _fileHelper;
    private List<(double StartSeconds, double EndSeconds)> _lines = new();
    private WavePeakData2? _wavePeaks;
    private double[]? _speechSignal;
    private bool _speechSignalComputed;
    private double _videoFrameRate;
    private string? _lastOpenedFilePath;
    private CancellationTokenSource? _cancellationTokenSource;

    public SyncToAudioViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        ReferenceSubtitleDisplayName = Se.Language.Video.SyncToAudioNoReferenceSubtitle;
        MaxOffsetSeconds = Math.Clamp(Se.Settings.Synchronization.SyncToAudioMaxOffsetSeconds, 1, 36000);
        TryFrameRates = Se.Settings.Synchronization.SyncToAudioTryFrameRates;
    }

    public void Initialize(IEnumerable<SubtitleLineViewModel> lines, WavePeakData2? wavePeaks, double videoFrameRate, string? lastOpenedFilePath)
    {
        _lines = lines
            .Where(l => l.EndTime > l.StartTime)
            .Select(l => (l.StartTime.TotalSeconds, l.EndTime.TotalSeconds))
            .ToList();
        _wavePeaks = wavePeaks;
        _videoFrameRate = videoFrameRate;
        _lastOpenedFilePath = lastOpenedFilePath;
        IsAudioReferenceAvailable = wavePeaks != null;
        if (!IsAudioReferenceAvailable)
        {
            UseAudioReference = false;
            UseSubtitleReference = true;
            ErrorText = Se.Language.Video.SyncToAudioNoWaveform;
        }
    }

    public void OnLoaded()
    {
        if (UseAudioReference && IsAudioReferenceAvailable)
        {
            _ = Analyze();
        }
    }

    partial void OnIsAnalyzingChanged(bool value) => _sleepInhibitor.SetActive(value);

    public void OnClosing()
    {
        _cancellationTokenSource?.Cancel();
        _sleepInhibitor.Dispose();
    }

    partial void OnUseAudioReferenceChanged(bool value)
    {
        if (value)
        {
            HasResult = false;
            Result = null;
        }
    }

    partial void OnUseSubtitleReferenceChanged(bool value)
    {
        if (value)
        {
            HasResult = false;
            Result = null;
        }
    }

    [RelayCommand]
    private async Task BrowseReferenceSubtitle()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, includeVideoFiles: false, lastOpenedFilePath: _lastOpenedFilePath);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        ReferenceSubtitleFileName = fileName;
        ReferenceSubtitleDisplayName = Path.GetFileName(fileName);
        UseSubtitleReference = true;
        UseAudioReference = false;
        await Analyze();
    }

    [RelayCommand]
    private async Task Analyze()
    {
        if (IsAnalyzing)
        {
            return;
        }

        ErrorText = string.Empty;
        HasResult = false;
        Result = null;
        if (_lines.Count == 0)
        {
            ErrorText = Se.Language.Video.SyncToAudioNoLines;
            return;
        }

        var options = new AudioSubtitleSync.Options(MaxOffsetSeconds, TryFrameRates, _videoFrameRate);
        var useAudio = UseAudioReference;
        var referenceFileName = ReferenceSubtitleFileName;
        if (!useAudio && string.IsNullOrEmpty(referenceFileName))
        {
            ErrorText = Se.Language.Video.SyncToAudioPickReferenceSubtitle;
            return;
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        IsAnalyzing = true;
        ProgressValue = 0;
        ProgressText = Se.Language.Video.SyncToAudioAnalyzing;
        var progress = new Progress<double>(p => ProgressValue = p * 100);
        try
        {
            var lines = _lines;
            var result = await Task.Run(() =>
            {
                var reference = useAudio ? GetSpeechSignal() : GetSubtitleSignal(referenceFileName);
                if (reference == null)
                {
                    return null;
                }

                return AudioSubtitleSync.Find(reference, lines, options, progress, cancellationToken);
            }, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (result == null)
            {
                ErrorText = useAudio ? Se.Language.Video.SyncToAudioNoSpeechFound : Se.Language.Video.SyncToAudioReferenceSubtitleEmpty;
                return;
            }

            ShowResult(result);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Sync subtitle to audio failed");
            ErrorText = exception.Message;
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested || _cancellationTokenSource?.Token == cancellationToken)
            {
                IsAnalyzing = false;
            }
        }
    }

    private double[]? GetSpeechSignal()
    {
        if (!_speechSignalComputed)
        {
            _speechSignal = _wavePeaks == null ? null : AudioSubtitleSync.SpeechFromPeaks(_wavePeaks);
            _speechSignalComputed = true;
        }

        return _speechSignal;
    }

    private static double[]? GetSubtitleSignal(string fileName)
    {
        var subtitle = Subtitle.Parse(fileName);
        var lines = subtitle.Paragraphs
            .Where(p => p.EndTime.TotalSeconds > p.StartTime.TotalSeconds)
            .Select(p => (p.StartTime.TotalSeconds, p.EndTime.TotalSeconds))
            .ToList();
        return lines.Count == 0 ? null : AudioSubtitleSync.SignalFromLines(lines);
    }

    private void ShowResult(AudioSubtitleSync.Result result)
    {
        Result = result;
        ResultOffsetText = string.Format(Se.Language.Video.SyncToAudioResultOffsetX, FormatOffset(result.OffsetSeconds));
        ResultFrameRateText = result.IsFrameRateChanged
            ? string.Format(Se.Language.Video.SyncToAudioResultFrameRateXToY, FormatFrameRate(result.FromFrameRate), FormatFrameRate(result.ToFrameRate))
            : Se.Language.Video.SyncToAudioResultFrameRateUnchanged;
        ResultConfidenceText = string.Format(Se.Language.Video.SyncToAudioResultConfidenceX, Math.Round(result.Confidence * 100).ToString(CultureInfo.CurrentCulture));
        IsLowConfidence = result.Confidence < AudioSubtitleSync.LowConfidence;
        HasResult = true;
    }

    internal static string FormatOffset(double seconds)
    {
        var sign = seconds < 0 ? "-" : "+";
        var abs = Math.Abs(seconds);
        return sign + abs.ToString("0.00", CultureInfo.CurrentCulture) + " " + Se.Language.General.Seconds.ToLowerInvariant();
    }

    private static string FormatFrameRate(double frameRate)
    {
        return frameRate.ToString("0.###", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// One line for the status bar / undo history: "+1.24 seconds" or "+1.24 seconds, 25 → 23.976".
    /// </summary>
    internal static string Describe(AudioSubtitleSync.Result result)
    {
        var text = FormatOffset(result.OffsetSeconds);
        if (result.IsFrameRateChanged)
        {
            text += ", " + FormatFrameRate(result.FromFrameRate) + " → " + FormatFrameRate(result.ToFrameRate);
        }

        return text;
    }

    /// <summary>
    /// Moves every line by the result: times scaled by the ratio (start plus scaled duration, so
    /// equal durations stay equal after rounding, as in Change frame rate), then shifted by the
    /// offset.
    /// </summary>
    internal static void Apply(ObservableCollection<SubtitleLineViewModel> subtitles, AudioSubtitleSync.Result result)
    {
        var offsetMs = result.OffsetSeconds * 1000.0;
        foreach (var line in subtitles)
        {
            var originalStartMs = line.StartTime.TotalMilliseconds;
            var originalEndMs = line.EndTime.TotalMilliseconds;
            var newStart = TimeSpanExtensions.FromMillisecondsWholeMilliseconds(originalStartMs * result.Ratio + offsetMs);
            var newDuration = TimeSpanExtensions.FromMillisecondsWholeMilliseconds((originalEndMs - originalStartMs) * result.Ratio);
            line.SetTimes(newStart, newStart + newDuration);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        if (Result == null || IsAnalyzing)
        {
            return;
        }

        Se.Settings.Synchronization.SyncToAudioMaxOffsetSeconds = MaxOffsetSeconds;
        Se.Settings.Synchronization.SyncToAudioTryFrameRates = TryFrameRates;
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
