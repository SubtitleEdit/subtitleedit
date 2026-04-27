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
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryResizeImages;

public partial class BinaryResizeImagesViewModel : ObservableObject
{
    [ObservableProperty] private int _percentage;
    [ObservableProperty] private Bitmap? _previewBitmap;
    [ObservableProperty] private string _imageSizeText;

    public Window? Window { get; set; }
    public Image? PreviewImage { get; set; }
    public bool OkPressed { get; private set; }

    private List<BinarySubtitleItem> _subtitles = new();
    private DispatcherTimer? _previewUpdateTimer;
    private bool _isDirty;
    private int _originalWidth;
    private int _originalHeight;

    public BinaryResizeImagesViewModel()
    {
        _percentage = 100;
        _imageSizeText = string.Empty;
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
        
        if (_subtitles.Count > 0 && _subtitles[0].Bitmap != null)
        {
            _originalWidth = (int)_subtitles[0].Bitmap!.Size.Width;
            _originalHeight = (int)_subtitles[0].Bitmap!.Size.Height;
        }
        
        UpdatePreview();
    }

    partial void OnPercentageChanged(int value)
    {
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

    private void UpdatePreview()
    {
        if (_subtitles.Count == 0 || _subtitles[0].Bitmap == null)
        {
            return;
        }

        var firstSubtitle = _subtitles[0];
        using var originalBitmap = firstSubtitle.Bitmap!.ToSkBitmap();
        
        var newWidth = (int)(originalBitmap.Width * Percentage / 100.0);
        var newHeight = (int)(originalBitmap.Height * Percentage / 100.0);

        var resizedBitmap = ResizeBitmap(originalBitmap, newWidth, newHeight);
        PreviewBitmap = resizedBitmap.ToAvaloniaBitmap();
        
        ImageSizeText = $"Original: {_originalWidth} × {_originalHeight} px\nNew: {newWidth} × {newHeight} px";
    }

    public void ApplyResize()
    {
        foreach (var subtitle in _subtitles)
        {
            if (subtitle.Bitmap == null)
            {
                continue;
            }

            using var originalBitmap = subtitle.Bitmap.ToSkBitmap();
            
            var newWidth = (int)(originalBitmap.Width * Percentage / 100.0);
            var newHeight = (int)(originalBitmap.Height * Percentage / 100.0);

            var resizedBitmap = ResizeBitmap(originalBitmap, newWidth, newHeight);
            subtitle.Bitmap = resizedBitmap.ToAvaloniaBitmap();
        }
    }

    private SKBitmap ResizeBitmap(SKBitmap originalBitmap, int width, int height)
    {
        var resizedBitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(resizedBitmap))
        {
            canvas.Clear(SKColors.Transparent);

            var scale = SKMatrix.CreateScale((float)width / originalBitmap.Width,
                                             (float)height / originalBitmap.Height);

            using (var shader = SKShader.CreateBitmap(originalBitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, scale))
            using (var paint = new SKPaint())
            {
                paint.Shader = shader;
                paint.IsAntialias = true;
                canvas.DrawRect(new SKRect(0, 0, width, height), paint);
            }
        }
        return resizedBitmap;
    }

    [RelayCommand]
    private async Task Ok()
    {
        var msg = GetValidationError();
        if (!string.IsNullOrEmpty(msg))
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ApplyResize();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private string GetValidationError()
    {
        if (Window == null)
        {
            return "Window is null";
        }

        if (Percentage <= 0)
        {
            return "Percentage must be greater than 0";
        }

        return string.Empty;
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
