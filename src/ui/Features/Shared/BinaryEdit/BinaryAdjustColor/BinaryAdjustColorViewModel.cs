using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustColor;

public partial class BinaryAdjustColorViewModel : ObservableObject, IDisposable
{
    [ObservableProperty] private Color _selectedColor;
    [ObservableProperty] private IBrush _colorSwatchBrush;
    [ObservableProperty] private Bitmap? _previewBitmap;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IWindowService _windowService;
    private List<BinarySubtitleItem> _subtitles = new();
    private DispatcherTimer? _previewUpdateTimer;
    private bool _isDirty;

    public BinaryAdjustColorViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _selectedColor = Se.Settings.Tools.LastColorPickerColor.FromHexToColor();
        _colorSwatchBrush = new SolidColorBrush(_selectedColor);
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
            _previewUpdateTimer.Stop();
            if (_isDirty)
            {
                _isDirty = false;
                UpdatePreview();
            }
        };
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

    public void Initialize(List<BinarySubtitleItem> subtitles)
    {
        _subtitles = subtitles;
        UpdatePreview();
    }

    partial void OnSelectedColorChanged(Color value)
    {
        ColorSwatchBrush = new SolidColorBrush(value);
        SchedulePreviewUpdate();
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ChooseColor()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(
            Window, vm => vm.Initialize(SelectedColor));

        if (result.OkPressed)
        {
            SelectedColor = result.SelectedColor;
        }
    }

    private void UpdatePreview()
    {
        if (_subtitles.Count == 0 || _subtitles[0].Bitmap == null)
        {
            return;
        }

        using var originalBitmap = _subtitles[0].Bitmap!.ToSkBitmap();
        using var coloredBitmap = ColorBitmap(originalBitmap, SelectedColor.R, SelectedColor.G, SelectedColor.B);
        var old = PreviewBitmap;
        PreviewBitmap = coloredBitmap.ToAvaloniaBitmap();
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
            using var coloredBitmap = ColorBitmap(originalBitmap, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            var old = subtitle.Bitmap;
            subtitle.Bitmap = coloredBitmap.ToAvaloniaBitmap();
            old?.Dispose();
        }
    }

    private static SKBitmap ColorBitmap(SKBitmap originalBitmap, byte r, byte g, byte b)
    {
        var redPercent = r * 100.0 / 255;
        var greenPercent = g * 100.0 / 255;
        var bluePercent = b * 100.0 / 255;

        var adjustedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            var srcPixels = originalBitmap.GetPixels();
            var dstPixels = adjustedBitmap.GetPixels();

            for (int i = 0; i < originalBitmap.Width * originalBitmap.Height; i++)
            {
                var pixel = ((uint*)srcPixels)[i];

                var a = (byte)((pixel >> 24) & 0xFF);
                var pr = (byte)((pixel >> 16) & 0xFF);
                var pg = (byte)((pixel >> 8) & 0xFF);
                var pb = (byte)(pixel & 0xFF);

                int total = pr + pg + pb;
                if (total > 100 && a > 0)
                {
                    pr = (byte)Math.Min(255, redPercent * total / 100);
                    pg = (byte)Math.Min(255, greenPercent * total / 100);
                    pb = (byte)Math.Min(255, bluePercent * total / 100);
                }

                ((uint*)dstPixels)[i] = (uint)((a << 24) | (pr << 16) | (pg << 8) | pb);
            }
        }

        return adjustedBitmap;
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
        _previewUpdateTimer?.Stop();
        var old = PreviewBitmap;
        PreviewBitmap = null;
        old?.Dispose();
    }
}
