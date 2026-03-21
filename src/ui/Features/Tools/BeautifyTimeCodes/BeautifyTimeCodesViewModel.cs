using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

public partial class BeautifyTimeCodesViewModel : ObservableObject, IDisposable
{
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private Avalonia.Threading.DispatcherTimer? _positionTimer;
    private bool _dirty;
    private readonly List<SubtitleLineViewModel> _allSubtitles;
    private readonly List<SubtitleLineViewModel> _originalSubtitles;
    private readonly List<SubtitleLineViewModel> _beautifiedSubtitles;
    private List<double> _shotChanges;
    private double _frameRate = 25.0;
    private bool _disposed;

    [ObservableProperty]
    private BeautifySettings _settings;

    [ObservableProperty]
    private Controls.AudioVisualizerControl.AudioVisualizer? _audioVisualizerOriginal;

    [ObservableProperty]
    private Controls.AudioVisualizerControl.AudioVisualizer? _audioVisualizerBeautified;

    public BeautifyTimeCodesViewModel()
    {
        _settings = new BeautifySettings();
        _allSubtitles = new List<SubtitleLineViewModel>();
        _originalSubtitles = new List<SubtitleLineViewModel>();
        _beautifiedSubtitles = new List<SubtitleLineViewModel>();
        _shotChanges = new List<double>();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                _dirty = false;
                UpdatePreview();
            }
            _timerUpdatePreview.Start();
        };

        // Listen to settings changes
        Settings.PropertyChanged += (s, e) => { _dirty = true; };
    }

    private void UpdatePreview()
    {
        if (AudioVisualizerBeautified == null || _allSubtitles.Count == 0)
        {
            return;
        }

        // Apply beautify and update the beautified visualizer
        var paragraphs = _allSubtitles.Select(p => p.Paragraph!).OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var beautifier = new Core.Common.TimeCodesBeautifier(paragraphs, _frameRate, _shotChanges, Settings.ToCore());
        var beautifiedParagraphs = beautifier.Beautify();

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_disposed || AudioVisualizerBeautified == null)
            {
                return;
            }

            // Reuse existing ViewModels and just update their properties
            _beautifiedSubtitles.Clear();
            var subRipFormat = new Core.SubtitleFormats.SubRip();
            for (int i = 0; i < beautifiedParagraphs.Count; i++)
            {
                var p = beautifiedParagraphs[i];
                var vm = new SubtitleLineViewModel(p, subRipFormat)
                {
                    Number = i + 1
                };
                _beautifiedSubtitles.Add(vm);
            }

            // Update the beautified visualizer's paragraphs
            AudioVisualizerBeautified.AllSelectedParagraphs = new List<SubtitleLineViewModel>(_beautifiedSubtitles);
            AudioVisualizerBeautified.InvalidateVisual();
        });
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles, Controls.AudioVisualizerControl.AudioVisualizer audioVisualizer, string videoFileName)
    {
        _allSubtitles.Clear();
        _allSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));

        _originalSubtitles.Clear();
        _originalSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));

        // Get shot changes from the existing AudioVisualizer
        _shotChanges = audioVisualizer.ShotChanges ?? new List<double>();

        // Get frame rate from video file using ffmpeg/ffprobe
        _frameRate = 25.0; // Default fallback
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
                // Fall back to default 25 fps if ffmpeg fails
            }
        }

        // Defer AudioVisualizer setup until window is loaded
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            // Ensure visualizers are created before copying properties
            if (AudioVisualizerOriginal == null || AudioVisualizerBeautified == null)
            {
                // Visualizers not initialized yet - this shouldn't happen
                return;
            }

            // Copy visualizer properties from the main window's AudioVisualizer
            AudioVisualizerOriginal.WavePeaks = audioVisualizer.WavePeaks;
            AudioVisualizerOriginal.ShotChanges = new List<double>(_shotChanges);
            AudioVisualizerOriginal.AllSelectedParagraphs = new List<SubtitleLineViewModel>(_originalSubtitles);
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

            // Trigger initial preview
            _dirty = true;

            // Force immediate render
            AudioVisualizerOriginal.InvalidateVisual();
            AudioVisualizerBeautified.InvalidateVisual();

            _timerUpdatePreview.Start();

            // Start position timer for audio visualizer updates
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
                // Keep visualizers synchronized - they share the same position
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
    private void Ok()
    {
        StopPositionTimer();

        // Apply final beautification
        var paragraphs = _allSubtitles.Select(p => p.Paragraph!).OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var beautifier = new Core.Common.TimeCodesBeautifier(paragraphs, _frameRate, _shotChanges, Settings.ToCore());
        var beautifiedParagraphs = beautifier.Beautify();

        _allSubtitles.Clear();
        var subRipFormat = new Core.SubtitleFormats.SubRip();
        foreach (var p in beautifiedParagraphs)
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
        Window?.Close();
    }

    public List<SubtitleLineViewModel> GetBeautifiedSubtitles()
    {
        return new List<SubtitleLineViewModel>(_allSubtitles);
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
            //UiUtil.ShowHelp("features/beautify-time-codes");
        }
    }

    internal void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _dirty = true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopPositionTimer();
        _timerUpdatePreview?.Stop();
        _timerUpdatePreview?.Dispose();

        _disposed = true;
    }
}