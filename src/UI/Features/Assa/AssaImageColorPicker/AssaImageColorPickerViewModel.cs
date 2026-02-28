using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAlpha;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaImageColorPicker;

public partial class AssaImageColorPickerViewModel : ObservableObject
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
    [ObservableProperty] private Bitmap _screenshot;
    [ObservableProperty] private string _screenshotOverlayPosiion;
    [ObservableProperty] private SolidColorBrush _currentMouseColor = new SolidColorBrush(Colors.Transparent);
    [ObservableProperty] private SolidColorBrush _clickedColor = new SolidColorBrush(Colors.Transparent);
    [ObservableProperty] private string _currentMouseColorHex = "#00000000";
    [ObservableProperty] private string _clickedColorHex = "#00000000";
    [ObservableProperty] private string _copyButtonContent = "Copy";

    private Subtitle _subtitle = new();

    public Subtitle ResultSubtitle => _subtitle;

    public AssaImageColorPickerViewModel()
    {
        Screenshot = new SKBitmap(1, 1).ToAvaloniaBitmap();
        ScreenshotOverlayPosiion = string.Empty;
        CopyButtonContent = Se.Language.Assa.CopyColorAsHextoClipboard;
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

    [RelayCommand]
    private async Task CopyColorToClipboard()
    {
        if (Window?.Clipboard != null)
        {
            await ClipboardHelper.SetTextAsync(Window, ClickedColorHex);

            // Show check icon temporarily
            var originalContent = CopyButtonContent;
            CopyButtonContent = "✓ Copied!";

            // Reset after 2 seconds
            await Task.Delay(2000);
            CopyButtonContent = originalContent;
        }
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

        ScreenshotOverlayPosiion = $"X: {ScreenshotX}, Y: {ScreenshotY}";
    }

    public void OnImagePointerMoved(object? sender, PointerEventArgs e)
    {
        if (ScreenshotImage == null || Screenshot == null)
        {
            return;
        }

        var position = e.GetPosition(ScreenshotImage);
        var imageWidth = ScreenshotImage.Bounds.Width;
        var imageHeight = ScreenshotImage.Bounds.Height;

        if (imageWidth <= 0 || imageHeight <= 0)
        {
            return;
        }

        var scaleX = Screenshot.PixelSize.Width / imageWidth;
        var scaleY = Screenshot.PixelSize.Height / imageHeight;

        var pixelX = (int)(position.X * scaleX);
        var pixelY = (int)(position.Y * scaleY);

        ScreenshotX = pixelX;
        ScreenshotY = pixelY;

        if (pixelX >= 0 && pixelX < Screenshot.PixelSize.Width && pixelY >= 0 && pixelY < Screenshot.PixelSize.Height)
        {
            var color = GetPixelColor(Screenshot, pixelX, pixelY);
            CurrentMouseColor = new SolidColorBrush(color);
            CurrentMouseColorHex = ColorToHex(color);
            ScreenshotOverlayPosiion = $"X: {pixelX}, Y: {pixelY} - {CurrentMouseColorHex}";
        }
    }

    public void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ScreenshotImage == null || Screenshot == null)
        {
            return;
        }

        var position = e.GetPosition(ScreenshotImage);
        var imageWidth = ScreenshotImage.Bounds.Width;
        var imageHeight = ScreenshotImage.Bounds.Height;

        if (imageWidth <= 0 || imageHeight <= 0)
        {
            return;
        }

        var scaleX = Screenshot.PixelSize.Width / imageWidth;
        var scaleY = Screenshot.PixelSize.Height / imageHeight;

        var pixelX = (int)(position.X * scaleX);
        var pixelY = (int)(position.Y * scaleY);

        if (pixelX >= 0 && pixelX < Screenshot.PixelSize.Width && pixelY >= 0 && pixelY < Screenshot.PixelSize.Height)
        {
            var color = GetPixelColor(Screenshot, pixelX, pixelY);
            ClickedColor = new SolidColorBrush(color);
            ClickedColorHex = ColorToHex(color);
        }
    }

    private static unsafe Color GetPixelColor(Bitmap bitmap, int x, int y)
    {
        try
        {
            if (bitmap.Format == Avalonia.Platform.PixelFormat.Bgra8888)
            {
                var pixel = stackalloc byte[4];
                bitmap.CopyPixels(
                    new Avalonia.PixelRect(x, y, 1, 1),
                    (nint)pixel,
                    4,
                    4);

                return Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
            }

            using var skBitmap = bitmap.ToSkBitmap();
            var skColor = skBitmap.GetPixel(x, y);
            return Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
        }
        catch
        {
            using var skBitmap = bitmap.ToSkBitmap();
            var skColor = skBitmap.GetPixel(x, y);
            return Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
        }
    }

    private static string ColorToHex(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
