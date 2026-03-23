using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustBrightness;

public partial class BinaryAdjustBrightnessViewModel : ObservableObject
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
            _previewUpdateTimer.Stop();
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
        
        var adjustedBitmap = AdjustBrightness(originalBitmap, (float)Brightness, (float)Contrast, (float)(Gamma / 100.0));
        PreviewBitmap = adjustedBitmap.ToAvaloniaBitmap();
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
            var adjustedBitmap = AdjustBrightness(originalBitmap, (float)Brightness, (float)Contrast, (float)(Gamma / 100.0));
            subtitle.Bitmap = adjustedBitmap.ToAvaloniaBitmap();
        }
    }

    private static SKBitmap AdjustBrightness(SKBitmap originalBitmap, float brightness, float contrast, float gamma)
    {
        var adjustedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
        
        // Normalize values for calculations
        var brightnessAdjust = brightness; // -100 to 100
        var contrastAdjust = (contrast + 100) / 100.0f; // Convert -100 to 100 range to 0 to 2 multiplier
        
        // Build lookup table for gamma correction
        var gammaLookup = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            gammaLookup[i] = (byte)Math.Clamp(Math.Pow(i / 255.0, 1.0 / gamma) * 255.0, 0, 255);
        }

        unsafe
        {
            var originalPixels = originalBitmap.GetPixels();
            var adjustedPixels = adjustedBitmap.GetPixels();
            
            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    var index = y * originalBitmap.Width + x;
                    var pixel = ((uint*)originalPixels)[index];
                    
                    var a = (byte)((pixel >> 24) & 0xFF);
                    var r = (byte)((pixel >> 16) & 0xFF);
                    var g = (byte)((pixel >> 8) & 0xFF);
                    var b = (byte)(pixel & 0xFF);
                    
                    // Skip transparent pixels
                    if (a == 0)
                    {
                        ((uint*)adjustedPixels)[index] = pixel;
                        continue;
                    }
                    
                    // Apply brightness
                    r = (byte)Math.Clamp(r + brightnessAdjust, 0, 255);
                    g = (byte)Math.Clamp(g + brightnessAdjust, 0, 255);
                    b = (byte)Math.Clamp(b + brightnessAdjust, 0, 255);
                    
                    // Apply contrast
                    r = (byte)Math.Clamp(((r - 128) * contrastAdjust) + 128, 0, 255);
                    g = (byte)Math.Clamp(((g - 128) * contrastAdjust) + 128, 0, 255);
                    b = (byte)Math.Clamp(((b - 128) * contrastAdjust) + 128, 0, 255);
                    
                    // Apply gamma
                    r = gammaLookup[r];
                    g = gammaLookup[g];
                    b = gammaLookup[b];
                    
                    // Reconstruct pixel
                    var adjustedPixel = (uint)((a << 24) | (r << 16) | (g << 8) | b);
                    ((uint*)adjustedPixels)[index] = adjustedPixel;
                }
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
}
