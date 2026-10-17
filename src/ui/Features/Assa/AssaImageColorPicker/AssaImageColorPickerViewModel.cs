using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAlpha;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaImageColorPicker;

public partial class AssaImageColorPickerViewModel : ObservableObject
{
    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public bool IsColorSelection { get; private set; }
    public Color? SelectedColor => HasPickedColor ? ClickedColor.Color : null;
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

    /// <summary>
    /// The pixels around the pointer, enlarged - the pointer pixel outlined in the middle.
    /// </summary>
    [ObservableProperty] private Bitmap? _magnifier;

    [ObservableProperty] private string _currentMouseColorRgbHex = string.Empty;
    [ObservableProperty] private string _clickedColorRgbHex = string.Empty;

    /// <summary>
    /// The picked color the way ASSA writes it: &amp;HBBGGRR&amp;.
    /// </summary>
    [ObservableProperty] private string _clickedColorAssa = string.Empty;
    [ObservableProperty] private bool _hasPickedColor;
    [ObservableProperty] private bool _isLoadingFrame;
    [ObservableProperty] private string _copiedText = string.Empty;

    /// <summary>
    /// Colors picked in this session, newest first - click one to pick it again.
    /// </summary>
    public ObservableCollection<PickedColorItem> History { get; } = [];

    private const int MagnifierCells = 11;
    private const int MagnifierCellSize = 8;
    private const int MaxHistory = 14;
    private SKBitmap? _screenshotPixels;
    private bool _isClosed;

    private Subtitle _subtitle = new();

    public Subtitle ResultSubtitle => _subtitle;

    public AssaImageColorPickerViewModel()
    {
        Screenshot = new SKBitmap(1, 1).ToAvaloniaBitmap();
        ScreenshotOverlayPosiion = string.Empty;
    }

    partial void OnScreenshotChanged(Bitmap value)
    {
        _screenshotPixels?.Dispose();
        _screenshotPixels = null;
    }

    /// <summary>
    /// The Skia copy of the frame and the magnifier bitmap are only freed when replaced - free
    /// them when the window closes too.
    /// </summary>
    public void OnClosed()
    {
        _isClosed = true;
        _screenshotPixels?.Dispose();
        _screenshotPixels = null;
        var magnifier = Magnifier;
        Magnifier = null;
        magnifier?.Dispose();
    }

    /// <summary>
    /// A Skia copy of the frame for the magnifier (made once, not on every pointer move).
    /// </summary>
    private SKBitmap Pixels => _screenshotPixels ??= Screenshot.ToSkBitmap();

    internal static string ToRgbHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    internal static string ToAssaColor(Color color) => $"&H{color.B:X2}{color.G:X2}{color.R:X2}&";

    /// <summary>
    /// Makes a color the picked one (click on the frame or on a history swatch) and puts it first in the history.
    /// </summary>
    public void Pick(Color color)
    {
        ClickedColor = new SolidColorBrush(color);
        ClickedColorHex = ColorToHex(color);
        ClickedColorRgbHex = ToRgbHex(color);
        ClickedColorAssa = ToAssaColor(color);
        HasPickedColor = true;

        var existing = History.FirstOrDefault(h => h.Color == color);
        if (existing != null)
        {
            History.Remove(existing);
        }

        History.Insert(0, new PickedColorItem(color));
        while (History.Count > MaxHistory)
        {
            History.RemoveAt(History.Count - 1);
        }
    }

    [RelayCommand]
    private void PickFromHistory(PickedColorItem? item)
    {
        if (item != null)
        {
            Pick(item.Color);
        }
    }

    [RelayCommand]
    private Task CopyHex() => Copy(ClickedColorRgbHex);

    [RelayCommand]
    private Task CopyAssa() => Copy(ClickedColorAssa);

    [RelayCommand]
    private Task CopyAssaTag() => Copy("\\c" + ClickedColorAssa);

    private async Task Copy(string text)
    {
        if (Window?.Clipboard == null || !HasPickedColor)
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, text);
        CopiedText = "\u2713 " + text;
        await Task.Delay(2000);
        if (CopiedText == "\u2713 " + text)
        {
            CopiedText = string.Empty;
        }
    }

    /// <summary>
    /// The 11 x 11 pixels around (x, y), eight times larger, the center one outlined.
    /// </summary>
    internal static SKBitmap MakeMagnifier(SKBitmap source, int x, int y)
    {
        var size = MagnifierCells * MagnifierCellSize;
        var bitmap = new SKBitmap(size, size);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(27, 28, 33));
        using var paint = new SKPaint();
        var half = MagnifierCells / 2;
        for (var row = 0; row < MagnifierCells; row++)
        {
            for (var column = 0; column < MagnifierCells; column++)
            {
                var px = x - half + column;
                var py = y - half + row;
                if (px < 0 || py < 0 || px >= source.Width || py >= source.Height)
                {
                    continue;
                }

                paint.Color = source.GetPixel(px, py);
                canvas.DrawRect(column * MagnifierCellSize, row * MagnifierCellSize, MagnifierCellSize, MagnifierCellSize, paint);
            }
        }

        using var outline = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };
        canvas.DrawRect(half * MagnifierCellSize + 0.75f, half * MagnifierCellSize + 0.75f, MagnifierCellSize - 1.5f, MagnifierCellSize - 1.5f, outline);
        return bitmap;
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

    public void Initialize(Subtitle subtitle, SubtitleLineViewModel line, string? videoFileName, int? videoWidth, int? videoHeight, double? videoPositionSeconds = null)
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

        LoadFrame(videoFileName, AssaSetPositionViewModel.GetScreenshotSeconds(line, videoPositionSeconds));
    }

    public void InitializeForColorSelection(string? videoFileName, int videoWidth, int videoHeight, double positionSeconds)
    {
        IsColorSelection = true;
        TargetWidth = videoWidth;
        TargetHeight = videoHeight;
        LoadFrame(videoFileName, positionSeconds);
    }

    /// <summary>
    /// Shows a checkered placeholder at once and grabs the frame with ffmpeg in the background,
    /// so the window opens without waiting for ffmpeg; picking waits for the frame.
    /// </summary>
    private void LoadFrame(string? videoFileName, double positionSeconds)
    {
        Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetWidth, TargetHeight);
        if (string.IsNullOrEmpty(videoFileName))
        {
            return;
        }

        IsLoadingFrame = true;
        _ = LoadFrameAsync(videoFileName, positionSeconds);
    }

    private async Task LoadFrameAsync(string videoFileName, double positionSeconds)
    {
        var frame = await Task.Run(() =>
        {
            try
            {
                // Pass exact seconds (not ToDisplayString, which honors the HH:MM:SS:FF time-code format
                // setting and would feed ffmpeg an unparseable "-ss 00:01:23:15", blanking the preview - #12182).
                var fileName = FfmpegGenerator.GetScreenShot(videoFileName, positionSeconds.ToString("0.###", CultureInfo.InvariantCulture));
                return System.IO.File.Exists(fileName) ? new Bitmap(fileName) : null;
            }
            catch
            {
                return null;
            }
        });

        Dispatcher.UIThread.Post(() =>
        {
            if (_isClosed)
            {
                frame?.Dispose();
                return;
            }

            if (frame != null)
            {
                Screenshot = frame;
            }

            IsLoadingFrame = false;
        });
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (IsColorSelection && !HasPickedColor)
        {
            return;
        }

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
            e.Handled = true;
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/assa-image-color-picker");
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
        if (ScreenshotImage == null || Screenshot == null || IsLoadingFrame)
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
            CurrentMouseColorRgbHex = ToRgbHex(color);
            ScreenshotOverlayPosiion = $"X {pixelX} · Y {pixelY}";

            using var magnified = MakeMagnifier(Pixels, pixelX, pixelY);
            var old = Magnifier;
            Magnifier = magnified.ToAvaloniaBitmap();
            old?.Dispose();
        }
    }

    public void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ScreenshotImage == null || Screenshot == null || IsLoadingFrame)
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
            Pick(GetPixelColor(Screenshot, pixelX, pixelY));
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

/// <summary>
/// A color in the picker's history.
/// </summary>
public sealed class PickedColorItem
{
    public Color Color { get; }
    public IBrush Brush { get; }
    public string Hex { get; }

    public PickedColorItem(Color color)
    {
        Color = color;
        Brush = new SolidColorBrush(color);
        Hex = AssaImageColorPickerViewModel.ToRgbHex(color);
    }

    public override string ToString() => Hex;
}
