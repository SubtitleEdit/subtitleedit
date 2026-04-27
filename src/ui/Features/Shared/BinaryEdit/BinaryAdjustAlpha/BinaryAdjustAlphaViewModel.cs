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

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAlpha;

public partial class BinaryAdjustAlphaViewModel : ObservableObject
{
    [ObservableProperty] private double _alphaAdjustment;
    [ObservableProperty] private double _transparencyThreshold;
    [ObservableProperty] private Bitmap? _previewBitmap;
    [ObservableProperty] private Bitmap? _checkeredBackgroundBitmap;

    public Window? Window { get; set; }
    public Image? PreviewImage { get; set; }
    public Image? BackgroundImage { get; set; }
    public bool OkPressed { get; private set; }

    public string AlphaAdjustmentDisplay => $"{AlphaAdjustment:F0}";
    public string TransparencyThresholdDisplay => $"{TransparencyThreshold:F0}";

    private List<BinarySubtitleItem> _subtitles = new();
    private DispatcherTimer? _previewUpdateTimer;
    private bool _isDirty;

    public BinaryAdjustAlphaViewModel()
    {
        _alphaAdjustment = 0;
        _transparencyThreshold = 0;
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

    partial void OnAlphaAdjustmentChanged(double value)
    {
        OnPropertyChanged(nameof(AlphaAdjustmentDisplay));
        SchedulePreviewUpdate();
    }

    partial void OnTransparencyThresholdChanged(double value)
    {
        OnPropertyChanged(nameof(TransparencyThresholdDisplay));
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
        AlphaAdjustment = 0;
        TransparencyThreshold = 0;
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
        
        // Create checkered background
        CheckeredBackgroundBitmap = CreateCheckeredBackground(originalBitmap.Width, originalBitmap.Height);
        
        // Apply alpha adjustments
        var adjustedBitmap = AdjustAlpha(originalBitmap, (float)AlphaAdjustment, (byte)TransparencyThreshold);
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
            var adjustedBitmap = AdjustAlpha(originalBitmap, (float)AlphaAdjustment, (byte)TransparencyThreshold);
            subtitle.Bitmap = adjustedBitmap.ToAvaloniaBitmap();
        }
    }

    private static SKBitmap AdjustAlpha(SKBitmap originalBitmap, float alphaAdjustment, byte transparencyThreshold)
    {
        var adjustedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);

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
                    
                    // Skip if already fully transparent
                    if (a == 0)
                    {
                        ((uint*)adjustedPixels)[index] = pixel;
                        continue;
                    }
                    
                    // Apply alpha adjustment (additive)
                    float newAlpha = a + alphaAdjustment;
                    
                    // Clamp to valid range
                    newAlpha = Math.Clamp(newAlpha, 0, 255);
                    
                    // Apply transparency threshold
                    if (newAlpha < transparencyThreshold)
                    {
                        newAlpha = 0;
                    }
                    
                    a = (byte)newAlpha;
                    
                    // Reconstruct pixel
                    var adjustedPixel = (uint)((a << 24) | (r << 16) | (g << 8) | b);
                    ((uint*)adjustedPixels)[index] = adjustedPixel;
                }
            }
        }

        return adjustedBitmap;
    }

    public static Bitmap CreateCheckeredBackground(int width, int height)
    {
        const int checkSize = 10;
        var bitmap = new SKBitmap(width, height);
        
        using (var canvas = new SKCanvas(bitmap))
        {
            // Draw checkered pattern (light and dark gray)
            var lightGray = new SKColor(200, 200, 200);
            var darkGray = new SKColor(150, 150, 150);
            
            for (int y = 0; y < height; y += checkSize)
            {
                for (int x = 0; x < width; x += checkSize)
                {
                    var color = ((x / checkSize + y / checkSize) % 2 == 0) ? lightGray : darkGray;
                    using var paint = new SKPaint { Color = color };
                    canvas.DrawRect(x, y, checkSize, checkSize, paint);
                }
            }
        }

        return bitmap.ToAvaloniaBitmap();
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
