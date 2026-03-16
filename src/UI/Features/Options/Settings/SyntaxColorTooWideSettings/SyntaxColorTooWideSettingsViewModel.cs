using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SyntaxColorTooWideSettings;

public partial class SyntaxColorTooWideSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _fonts = [];
    [ObservableProperty] private string _selectedFont = string.Empty;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private int _maxWidthPixels;
    [ObservableProperty] private string _sampleText = "I know the quick brown fox jumps over the lazy dog!";
    [ObservableProperty] private int _sampleBoxWidth = 720;
    [ObservableProperty] private Bitmap _imagePreview;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private bool _previewDirty;
    private readonly System.Timers.Timer _updateTimer;

    public SyntaxColorTooWideSettingsViewModel()
    {
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFont = Fonts.Count > 0 ? Fonts[0] : string.Empty;
        _imagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();

        _updateTimer = new System.Timers.Timer(300);
        _updateTimer.Elapsed += (_, _) =>
        {
            _updateTimer.Stop();
            if (_previewDirty)
            {
                _previewDirty = false;
                UpdatePreview();
            }
            _updateTimer.Start();
        };
    }

    internal void Initialize(int tooWidePixels, string fontName, int fontSize)
    {
        SelectedFont = Fonts.Contains(fontName) ? fontName : (Fonts.Count > 0 ? Fonts[0] : string.Empty);
        FontSize = fontSize;
        MaxWidthPixels = tooWidePixels;

        if (MaxWidthPixels < 720)
        {
            SampleBoxWidth = 720;
        }
        else if (MaxWidthPixels < 1280)
        {
            SampleBoxWidth = 1280;
        }
        else if (MaxWidthPixels < 1920)
        {
            SampleBoxWidth = 1920;
        }
        else
        {
            SampleBoxWidth = MaxWidthPixels + 200;
        }

        _updateTimer.Start();
        _previewDirty = true;
    }

    internal void MarkPreviewDirty() => _previewDirty = true;

    private void UpdatePreview()
    {
        if (string.IsNullOrEmpty(SelectedFont) || FontSize <= 0 || SampleBoxWidth <= 0)
        {
            return;
        }

        var bitmapWidth = Math.Clamp(SampleBoxWidth, 100, 1400);
        var widthScale = (double)bitmapWidth / SampleBoxWidth;
        var greenLineX = (int)(MaxWidthPixels * widthScale);
        var previewHeight = Math.Max(60, (int)(FontSize * 2.8));

        var info = new SKImageInfo(bitmapWidth, previewHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(18, 18, 22));

        // Green safe-area tint (left of limit line)
        if (greenLineX > 0 && greenLineX < bitmapWidth)
        {
            using var p = new SKPaint { Color = new SKColor(0, 160, 0, 22) };
            canvas.DrawRect(new SKRect(0, 0, greenLineX, previewHeight), p);
        }

        // Draw text and measure width
        var typeface = SKTypeface.FromFamilyName(SelectedFont) ?? SKTypeface.Default;
        using var skFont = new SKFont(typeface, FontSize);
        var rawTextWidth = skFont.MeasureText(SampleText);
        var scaledTextWidth = (float)(rawTextWidth * widthScale);

        // Red overflow tint (text past the limit line)
        if (greenLineX > 0 && scaledTextWidth > greenLineX)
        {
            using var p = new SKPaint { Color = new SKColor(200, 30, 30, 50) };
            canvas.DrawRect(new SKRect(greenLineX, 0, Math.Min(scaledTextWidth, bitmapWidth), previewHeight), p);
        }

        // Sample text
        var metrics = skFont.Metrics;
        var textY = previewHeight / 2f - (metrics.Ascent + metrics.Descent) / 2f;
        using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText(SampleText, 0, textY, skFont, textPaint);

        // Green limit line
        if (greenLineX > 0 && greenLineX <= bitmapWidth)
        {
            using var p = new SKPaint { Color = new SKColor(50, 220, 50), StrokeWidth = 2, IsAntialias = false };
            canvas.DrawLine(greenLineX, 0, greenLineX, previewHeight, p);
        }

        // Outer box border
        using var borderPaint = new SKPaint
        {
            Color = new SKColor(90, 90, 90, 200),
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
        };
        canvas.DrawRect(new SKRect(0.5f, 0.5f, bitmapWidth - 0.5f, previewHeight - 0.5f), borderPaint);

        using var snap = surface.Snapshot();
        using var skBmp = SKBitmap.FromImage(snap);
        var avaloniaBitmap = skBmp.ToAvaloniaBitmap();

        Dispatcher.UIThread.Invoke(() =>
        {
            var old = ImagePreview;
            ImagePreview = avaloniaBitmap;
            old?.Dispose();
        });
    }

    [RelayCommand]
    private void Ok()
    {
        _updateTimer.Stop();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _updateTimer.Stop();
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            _updateTimer.Stop();
            Window?.Close();
        }
    }
}