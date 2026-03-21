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
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ImageBasedPreviewViewModel : ObservableObject
{
    [ObservableProperty] private Bitmap _bitmapPreview;
    [ObservableProperty] private string _title;

    public Window? Window { get; set; }
    public Image ImagePreview { get; internal set; }

    private readonly Timer _timerUpdatePreview;

    public ImageBasedPreviewViewModel()
    {
        BitmapPreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        Title = Se.Language.General.Preview;
        ImagePreview = new Image();

        _timerUpdatePreview = new Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        UpdateTitle();
    }

    public void Initialize(SKBitmap bitmap, int width, int height, int x, int y)
    {
        var skBitmap = new SKBitmap(width, height, true);
        using (var canvas = new SKCanvas(skBitmap))
        {
            UiUtil.DrawCheckerboardBackground(canvas, width, height);
            canvas.DrawBitmap(bitmap, x, y);
        }

        BitmapPreview = skBitmap.ToAvaloniaBitmap();
        _timerUpdatePreview.Start();
    }    

    [RelayCommand]
    private void Ok()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private void UpdateTitle()
    {
        if (Window != null)
        {
            var windowWidth = (int)ImagePreview.Bounds.Width;
            var windowHeight = (int)ImagePreview.Bounds.Height;
            var targetWidth = BitmapPreview.PixelSize.Width;
            var targetHeight = BitmapPreview.PixelSize.Height;

            var scaleX = (double)windowWidth / targetWidth;
            var scaleY = (double)windowHeight / targetHeight;
            var scale = Math.Min(scaleX, scaleY);
            var zoomPct = (int)(scale * 100);

            Title = string.Format(Se.Language.File.Export.PreviewTitle, windowWidth, windowHeight, targetWidth, targetHeight, zoomPct);
        }
    }
}