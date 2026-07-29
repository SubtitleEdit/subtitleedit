using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustBrightness;

public partial class BinaryAdjustBrightnessViewModel : ObservableObject, IDisposable
{
    [ObservableProperty] private double _brightness;
    [ObservableProperty] private double _contrast;
    [ObservableProperty] private double _gamma;
    [ObservableProperty] private Bitmap? _previewBitmap;

    public Window? Window { get; set; }
    public Image? PreviewImage { get; set; }
    public bool OkPressed { get; private set; }

    public string BrightnessDisplay => $"{Brightness:F0}";
    public string ContrastDisplay => $"{Contrast:F0}";
    public string GammaDisplay => $"{Gamma / 100.0:F2}";

    private List<BinarySubtitleItem> _subtitles = new();
    private DispatcherTimer? _previewUpdateTimer;
    private bool _isDirty;

    public BinaryAdjustBrightnessViewModel()
    {
        _brightness = 0;
        _contrast = 0;
        _gamma = 100; // 1.0 gamma
        InitializeTimer();
    }

    private void InitializeTimer()
    {
        _previewUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _previewUpdateTimer.Tick += (_, _) =>
        {
            _previewUpdateTimer?.Stop();
            if (_isDirty)
            {
                _isDirty = false;
                UpdatePreview();
            }
        };
    }

    public void Initialize(List<BinarySubtitleItem> subtitles)
    {
        _subtitles = subtitles;
        UpdatePreview();
    }

    partial void OnBrightnessChanged(double value)
    {
        OnPropertyChanged(nameof(BrightnessDisplay));
        SchedulePreviewUpdate();
    }

    partial void OnContrastChanged(double value)
    {
        OnPropertyChanged(nameof(ContrastDisplay));
        SchedulePreviewUpdate();
    }

    partial void OnGammaChanged(double value)
    {
        OnPropertyChanged(nameof(GammaDisplay));
        SchedulePreviewUpdate();
    }

    private void SchedulePreviewUpdate()
    {
        if (_previewUpdateTimer == null)
        {
            return;
        }

        _isDirty = true;
        _previewUpdateTimer.Stop();
        _previewUpdateTimer.Start();
    }

    [RelayCommand]
    private void Reset()
    {
        Brightness = 0;
        Contrast = 0;
        Gamma = 100;
        UpdatePreview();
    }

    [RelayCommand]
    private void UpdatePreview()
    {
        if (_subtitles.Count == 0 || _subtitles[0].Bitmap == null)
        {
            return;
        }

        var firstSubtitle = _subtitles[0];
        using var originalBitmap = firstSubtitle.Bitmap!.ToSkBitmap();
        using var adjustedBitmap = SubtitleImageAdjuster.AdjustBrightness(originalBitmap, (float)Brightness, (float)Contrast, (float)(Gamma / 100.0));
        var old = PreviewBitmap;
        PreviewBitmap = adjustedBitmap.ToAvaloniaBitmap();
        old?.Dispose();
    }

    public void ApplyAdjustments()
    {
        foreach (var subtitle in _subtitles)
        {
            if (subtitle.Bitmap == null)
            {
                continue;
            }

            using var originalBitmap = subtitle.Bitmap.ToSkBitmap();
            using var adjustedBitmap = SubtitleImageAdjuster.AdjustBrightness(originalBitmap, (float)Brightness, (float)Contrast, (float)(Gamma / 100.0));
            var old = subtitle.Bitmap;
            subtitle.Bitmap = adjustedBitmap.ToAvaloniaBitmap();
            old?.Dispose();
        }
    }


    [RelayCommand]
    private void Ok()
    {
        if (Window == null)
        {
            return;
        }

        ApplyAdjustments();
        OkPressed = true;
        Window.Close();
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

    public void Dispose()
    {
        _isDirty = false;
        _previewUpdateTimer?.Stop();
        _previewUpdateTimer = null;
        var old = PreviewBitmap;
        PreviewBitmap = null;
        old?.Dispose();
    }
}
