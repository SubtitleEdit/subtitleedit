using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAlpha;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;

public partial class AssaSetPositionViewModel : ObservableObject
{
    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public Image? ScreenshotImage { get; set; }
    public Image? ScreenshotOverlayImage { get; set; }
    public Grid? VideoGrid { get; set; }

    [ObservableProperty] private int _sourceWidth = 1920;
    [ObservableProperty] private int _sourceHeight = 1080;
    [ObservableProperty] private int _targetWidth = 1920;
    [ObservableProperty] private int _targetHeight = 1080;
    [ObservableProperty] private int _screenshotX;
    [ObservableProperty] private int _screenshotY;
    [ObservableProperty] private Bitmap _screenshotOverlayText;
    [ObservableProperty] private Bitmap _screenshot;
    [ObservableProperty] private string _screenshotOverlayPosiion;

    private Subtitle _subtitle = new();
    private bool _isLeftAligned = false;
    private bool _isHorizontalCentered = true;
    private bool _isRightAligned = false;

    private bool _isTopAligned = false;
    private bool _isVerticalCentered = false;
    private bool _isBottomAligned = true;

    public Subtitle ResultSubtitle => _subtitle;

    public AssaSetPositionViewModel()
    {
        Screenshot = new SKBitmap(1, 1).ToAvaloniaBitmap();
        ScreenshotOverlayText = new SKBitmap(1, 1).ToAvaloniaBitmap();
        ScreenshotOverlayPosiion = string.Empty;
    }

    public int ResultX
    {
        get
        {
            if (_isHorizontalCentered)
            {
                return (int)Math.Round(ScreenshotX + ScreenshotOverlayText.Size.Width / 2.0, MidpointRounding.AwayFromZero);
            }

            if (_isRightAligned)
            {
                return (int)Math.Round(ScreenshotX + ScreenshotOverlayText.Size.Width, MidpointRounding.AwayFromZero);
            }

            return ScreenshotX;
        }
    }


    public int ResultY
    {
        get
        {
            if (_isBottomAligned)
            {
                return (int)Math.Round(ScreenshotY + ScreenshotOverlayText.Size.Height, MidpointRounding.AwayFromZero);
            }

            if (_isVerticalCentered)
            {
                return (int)Math.Round(ScreenshotY + ScreenshotOverlayText.Size.Height / 2.0, MidpointRounding.AwayFromZero);
            }

            return ScreenshotY;
        }
    }

    partial void OnScreenshotXChanged(int value)
    {
        // Only update if UI elements are initialized
        if (VideoGrid != null && ScreenshotOverlayImage != null && ScreenshotImage != null)
        {
            UpdateOverlayPosition();
        }
    }

    partial void OnScreenshotYChanged(int value)
    {
        // Only update if UI elements are initialized
        if (VideoGrid != null && ScreenshotOverlayImage != null && ScreenshotImage != null)
        {
            UpdateOverlayPosition();
        }
    }

    public void Initialize(Subtitle subtitle, SubtitleLineViewModel line, string? videoFileName, int? videoWidth, int? videoHeight)
    {
        _subtitle = new Subtitle(subtitle, false);

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);
        var style = styles.FirstOrDefault(s => s.Name.Equals(line.Style, StringComparison.OrdinalIgnoreCase));
        if (style != null)
        {
            _isLeftAligned = style.Alignment == "1" || style.Alignment == "4" || style.Alignment == "7";
            _isHorizontalCentered = style.Alignment == "2" || style.Alignment == "5" || style.Alignment == "8";
            _isRightAligned = style.Alignment == "3" || style.Alignment == "6" || style.Alignment == "9";

            _isTopAligned = style.Alignment == "7" || style.Alignment == "8" || style.Alignment == "9";
            _isVerticalCentered = style.Alignment == "4" || style.Alignment == "5" || style.Alignment == "6";
            _isBottomAligned = style.Alignment == "1" || style.Alignment == "2" || style.Alignment == "3";
        }

        var previewSubtitle = new Subtitle(subtitle);
        previewSubtitle.Paragraphs.Clear();
        var previewParagraph = line.ToParagraph();
        previewParagraph.StartTime.TotalSeconds = 0;
        previewParagraph.EndTime.TotalSeconds = 10;
        previewSubtitle.Paragraphs.Add(previewParagraph);
        var previewScreenshotFileName = FfmpegGenerator.GetScreenShotWithSubtitle(previewSubtitle, videoWidth ?? 1920, videoHeight ?? 1080);
        var skBitmap = SKBitmap.Decode(previewScreenshotFileName);
        var trimResult = skBitmap.TrimTransparentPixels();

        ScreenshotOverlayText = trimResult.TrimmedBitmap.ToAvaloniaBitmap();
        ScreenshotX = trimResult.Left;
        ScreenshotY = trimResult.Top;

        if (string.IsNullOrEmpty(_subtitle.Header))
        {
            _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        // Get source resolution from subtitle header
        var oldPlayResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", _subtitle.Header);
        if (int.TryParse(oldPlayResX, out var w) && w > 0)
        {
            SourceWidth = w;
        }

        var oldPlayResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", _subtitle.Header);
        if (int.TryParse(oldPlayResY, out var h) && h > 0)
        {
            SourceHeight = h;
        }

        // Set target resolution from video if available
        if (videoWidth.HasValue && videoWidth.Value > 0)
        {
            TargetWidth = videoWidth.Value;
        }
        else
        {
            TargetWidth = SourceWidth;
        }

        if (videoHeight.HasValue && videoHeight.Value > 0)
        {
            TargetHeight = videoHeight.Value;
        }
        else
        {
            TargetHeight = SourceHeight;
        }

        if (TargetWidth <= 0 || TargetHeight <= 0)
        {
            TargetWidth = 1820;
            TargetHeight = 1080;
        }

        if (string.IsNullOrEmpty(videoFileName))
        {
            Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetHeight, TargetHeight);
            return;
        }

        var fileName = FfmpegGenerator.GetScreenShot(videoFileName, new TimeCode(line.StartTime.TotalMilliseconds).ToDisplayString());
        if (System.IO.File.Exists(fileName))
        {
            try
            {
                Screenshot = new Bitmap(fileName);
            }
            catch
            {
                Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetHeight, TargetHeight);
            }
        }
        else
        {
            Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetHeight, TargetHeight);
        }
    }

    [RelayCommand]
    private async Task CenterHorizontally()
    {
        ScreenshotX = (int)Math.Round((double)(SourceWidth / 2.0) - (double)(ScreenshotOverlayText.Size.Width / 2.0), MidpointRounding.AwayFromZero);
    }

    [RelayCommand]
    private async Task CenterVertically()
    {
        ScreenshotY = (int)Math.Round((double)(SourceHeight / 2.0) - (double)(ScreenshotOverlayText.Size.Height / 2.0), MidpointRounding.AwayFromZero);
    }

    [RelayCommand]
    private async Task Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    public void UpdateOverlayPosition()
    {
        if (VideoGrid == null || ScreenshotOverlayImage == null || ScreenshotImage == null)
        {
            return;
        }

        var screenshotImageWidth = ScreenshotImage.Bounds.Width;
        var screenshotImageHeight = ScreenshotImage.Bounds.Height;

        if (screenshotImageWidth <= 0 || screenshotImageHeight <= 0)
        {
            return;
        }

        var overlayBitmap = ScreenshotOverlayText;
        if (overlayBitmap == null)
        {
            return;
        }

        // Calculate scale factor based on screenshot image size
        var scaleX = screenshotImageWidth / TargetWidth;
        var scaleY = screenshotImageHeight / TargetHeight;

        // Position and size the overlay
        var overlayWidth = overlayBitmap.Size.Width * scaleX;
        var overlayHeight = overlayBitmap.Size.Height * scaleY;
        var overlayX = ScreenshotX * scaleX;
        var overlayY = ScreenshotY * scaleY;

        // Calculate the offset to center the screenshot image in the VideoGrid
        var gridWidth = VideoGrid.Bounds.Width;
        var gridHeight = VideoGrid.Bounds.Height;
        var offsetX = (gridWidth - screenshotImageWidth) / 2;
        var offsetY = (gridHeight - screenshotImageHeight) / 2;

        ScreenshotOverlayImage.Width = overlayWidth;
        ScreenshotOverlayImage.Height = overlayHeight;
        ScreenshotOverlayImage.Margin = new Thickness(
            offsetX + overlayX,
            offsetY + overlayY,
            0,
            0);

        ScreenshotOverlayPosiion = $"X: {ScreenshotX}, Y: {ScreenshotY}";
    }
}
