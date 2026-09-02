using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
    [ObservableProperty] private decimal _rotation;

    private static readonly Regex FrzRegex = new(@"\\frz\(?(-?\d+(\.\d+)?)\)?", RegexOptions.Compiled);
    private static readonly Regex InlineAlignmentRegex = new(@"\\an([1-9])", RegexOptions.Compiled);

    public decimal ResultRotation => Rotation;

    /// <summary>
    /// The override tags OK writes in front of the line: \pos plus, when the text is rotated, \frz.
    /// </summary>
    public string ResultTags => BuildPositionTags(ResultX, ResultY, Rotation, _styleAngle);

    // Angle of the line's style. \frz overrides it, so the spinner starts from it when the text has
    // no \frz of its own and OK pins \frz whenever it is non-zero (a spinner at 0 must give 0°).
    private decimal _styleAngle;

    private Subtitle _subtitle = new();

    // Size of the canvas the overlay text was rendered on. ScreenshotX/Y and the overlay bitmap
    // live in this pixel space; \pos() is PlayRes (SourceWidth/Height) space - see ToScriptSpace.
    private int _renderWidth = 1920;
    private int _renderHeight = 1080;

    private string _alignment = "2";
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
            var renderX = (double)ScreenshotX;
            if (_isHorizontalCentered)
            {
                renderX += ScreenshotOverlayText.Size.Width / 2.0;
            }
            else if (_isRightAligned)
            {
                renderX += ScreenshotOverlayText.Size.Width;
            }

            return ToScriptSpace(renderX, SourceWidth, _renderWidth);
        }
    }


    public int ResultY
    {
        get
        {
            var renderY = (double)ScreenshotY;
            if (_isBottomAligned)
            {
                renderY += ScreenshotOverlayText.Size.Height;
            }
            else if (_isVerticalCentered)
            {
                renderY += ScreenshotOverlayText.Size.Height / 2.0;
            }

            return ToScriptSpace(renderY, SourceHeight, _renderHeight);
        }
    }

    /// <summary>
    /// The overlay is measured in render (video pixel) space, but libass interprets \pos() in
    /// PlayRes space - e.g. \pos(320,180) with PlayRes 640x360 is the center of a 1080p frame.
    /// Convert at this boundary; writing render pixels into the tag put the text off by
    /// renderSize/playRes whenever the script resolution differs from the video's (#13350).
    /// </summary>
    internal static int ToScriptSpace(double renderValue, int playRes, int renderSize)
    {
        if (playRes <= 0 || renderSize <= 0)
        {
            return (int)Math.Round(renderValue, MidpointRounding.AwayFromZero);
        }

        return (int)Math.Round(renderValue * playRes / renderSize, MidpointRounding.AwayFromZero);
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

    partial void OnRotationChanged(decimal value)
    {
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

        // Resolve the script resolution before rendering the preview. A header without PlayRes
        // would make libass assume 384x288 while this dialog measured in a different space, so
        // stamp one in that case - ResultSubtitle carries it back so the saved \pos matches.
        (_subtitle.Header, var playResX, var playResY) =
            EnsurePlayRes(_subtitle.Header, videoWidth ?? 1920, videoHeight ?? 1080);
        SourceWidth = playResX;
        SourceHeight = playResY;

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(_subtitle.Header);
        var style = styles.FirstOrDefault(s => s.Name.Equals(line.Style, StringComparison.OrdinalIgnoreCase));
        _styleAngle = style?.Angle ?? 0;
        ApplyAlignment(ResolveAlignment(style?.Alignment, line.Text));

        var frzMatch = FrzRegex.Match(line.Text ?? string.Empty);
        Rotation = frzMatch.Success && decimal.TryParse(frzMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var frz)
            ? frz
            : _styleAngle;

        // Without a video, render at the script's own resolution so PlayRes aspect is honored and
        // render space equals script space. GetScreenShotWithSubtitle bumps odd sizes to even for
        // the encoder - mirror that here so the stored dimensions match the actual canvas.
        _renderWidth = videoWidth is > 0 ? videoWidth.Value : SourceWidth;
        _renderHeight = videoHeight is > 0 ? videoHeight.Value : SourceHeight;
        _renderWidth += _renderWidth % 2;
        _renderHeight += _renderHeight % 2;

        var previewSubtitle = MakePreviewSubtitle(_subtitle, line);
        var previewScreenshotFileName = FfmpegGenerator.GetScreenShotWithSubtitle(previewSubtitle, _renderWidth, _renderHeight);

        // ffmpeg can fail to produce the frame (missing/blocked binary, odd filter input): decoding
        // a null file name threw and took the whole dialog down. The png and both bitmaps are ours
        // to clean up.
        if (!string.IsNullOrEmpty(previewScreenshotFileName))
        {
            using var skBitmap = SKBitmap.Decode(previewScreenshotFileName);
            try
            {
                File.Delete(previewScreenshotFileName);
            }
            catch
            {
                // ignore cleanup errors
            }

            if (skBitmap != null)
            {
                using var trimResult = skBitmap.TrimTransparentPixels();
                ScreenshotOverlayText = trimResult.TrimmedBitmap.ToAvaloniaBitmap();
                ScreenshotX = trimResult.Left;
                ScreenshotY = trimResult.Top;
            }
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
            TargetWidth = 1920;
            TargetHeight = 1080;
        }

        if (string.IsNullOrEmpty(videoFileName))
        {
            Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetWidth, TargetHeight);
            return;
        }

        // Pass exact seconds (not ToDisplayString, which honors the HH:MM:SS:FF time-code format
        // setting and would feed ffmpeg an unparseable "-ss 00:01:23:15", blanking the preview - #12182).
        var fileName = FfmpegGenerator.GetScreenShot(videoFileName, (line.StartTime.TotalMilliseconds / 1000.0).ToString("0.###", CultureInfo.InvariantCulture));
        if (System.IO.File.Exists(fileName))
        {
            try
            {
                Screenshot = new Bitmap(fileName);
            }
            catch
            {
                Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetWidth, TargetHeight);
            }
        }
        else
        {
            Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetWidth, TargetHeight);
        }
    }

    /// <summary>
    /// Builds the one-line subtitle that ffmpeg renders into the draggable text overlay.
    /// </summary>
    internal static Subtitle MakePreviewSubtitle(Subtitle subtitle, SubtitleLineViewModel line)
    {
        var previewSubtitle = new Subtitle(subtitle);
        previewSubtitle.Paragraphs.Clear();

        // The ASSA writer takes the style name from Paragraph.Extra, which ToParagraph only fills
        // in when it knows the format. Without it every preview fell back to the first style in the
        // header, so lines using any other style were positioned against the wrong font/size (#13350).
        var previewParagraph = line.ToParagraph(new AdvancedSubStationAlpha());
        previewParagraph.StartTime.TotalSeconds = 0;
        previewParagraph.EndTime.TotalSeconds = 10;

        // The overlay must be the unrotated text: the dialog rotates it itself around the anchor
        // by the spinner value. Rendering the line's own \frz (or the style's Angle) into the bitmap
        // rotated it twice - \frz25 previewed at 50° and its trimmed box no longer matched the
        // anchor libass places \pos at (#14440). \pos is kept so the overlay starts where the line is.
        previewParagraph.Text = "{\\frz0}" + FrzRegex.Replace(previewParagraph.Text, string.Empty).Replace("{}", string.Empty);
        previewSubtitle.Paragraphs.Add(previewParagraph);

        return previewSubtitle;
    }

    /// <summary>
    /// The effective \an alignment (1-9) of the line: an inline \an tag wins over the style, and
    /// bottom-center is the ASS default.
    /// </summary>
    internal static string ResolveAlignment(string? styleAlignment, string? text)
    {
        var inline = InlineAlignmentRegex.Match(text ?? string.Empty);
        if (inline.Success)
        {
            return inline.Groups[1].Value;
        }

        return styleAlignment is { Length: 1 } && styleAlignment[0] is >= '1' and <= '9' ? styleAlignment : "2";
    }

    private void ApplyAlignment(string alignment)
    {
        _alignment = alignment;
        _isLeftAligned = alignment == "1" || alignment == "4" || alignment == "7";
        _isHorizontalCentered = alignment == "2" || alignment == "5" || alignment == "8";
        _isRightAligned = alignment == "3" || alignment == "6" || alignment == "9";

        _isTopAligned = alignment == "7" || alignment == "8" || alignment == "9";
        _isVerticalCentered = alignment == "4" || alignment == "5" || alignment == "6";
        _isBottomAligned = alignment == "1" || alignment == "2" || alignment == "3";
    }

    /// <summary>
    /// Where libass rotates the text: \org defaults to the \pos point, which is the alignment
    /// anchor of the text box - not its center.
    /// </summary>
    internal static RelativePoint GetRotationOrigin(string alignment)
    {
        var x = alignment is "1" or "4" or "7" ? 0.0 : alignment is "3" or "6" or "9" ? 1.0 : 0.5;
        var y = alignment is "7" or "8" or "9" ? 0.0 : alignment is "4" or "5" or "6" ? 0.5 : 1.0;
        return new RelativePoint(x, y, RelativeUnit.Relative);
    }

    internal static string BuildPositionTags(int x, int y, decimal rotation, decimal styleAngle)
    {
        var tags = $"\\pos({x},{y})";
        if (rotation != 0 || styleAngle != 0)
        {
            tags += "\\frz" + rotation.ToString("0.##", CultureInfo.InvariantCulture);
        }

        return tags;
    }

    /// <summary>
    /// Returns the header with a guaranteed PlayResX/PlayResY (stamping the fallback when either
    /// is missing or invalid) together with the effective values.
    /// </summary>
    internal static (string Header, int PlayResX, int PlayResY) EnsurePlayRes(string header, int fallbackWidth, int fallbackHeight)
    {
        var hasX = int.TryParse(AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", header), out var w) && w > 0;
        var hasY = int.TryParse(AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", header), out var h) && h > 0;
        if (hasX && hasY)
        {
            return (header, w, h);
        }

        return (AdvancedSubStationAlpha.SetResolution(header, fallbackWidth, fallbackHeight), fallbackWidth, fallbackHeight);
    }

    [RelayCommand]
    private async Task CenterHorizontally()
    {
        // ScreenshotX/Y are render-space, so center on the render canvas - centering on
        // SourceWidth (PlayRes) was off whenever the two resolutions differed.
        ScreenshotX = (int)Math.Round(_renderWidth / 2.0 - ScreenshotOverlayText.Size.Width / 2.0, MidpointRounding.AwayFromZero);
    }

    [RelayCommand]
    private async Task CenterVertically()
    {
        ScreenshotY = (int)Math.Round(_renderHeight / 2.0 - ScreenshotOverlayText.Size.Height / 2.0, MidpointRounding.AwayFromZero);
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/assa-set-position");
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

        // Overlay coordinates are render-canvas pixels; the background screenshot always shows the
        // same full frame, so map render space to the displayed image size. (TargetWidth is the
        // PlayRes fallback when no video is loaded, which is the wrong space for the overlay.)
        var scaleX = screenshotImageWidth / _renderWidth;
        var scaleY = screenshotImageHeight / _renderHeight;

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

        // ASSA \frz rotates counter-clockwise around the anchor (\org defaults to \pos);
        // Avalonia RotateTransform is clockwise, so negate.
        ScreenshotOverlayImage.RenderTransformOrigin = GetRotationOrigin(_alignment);
        ScreenshotOverlayImage.RenderTransform = new RotateTransform(-(double)Rotation);

        // Show the script-space anchor that OK writes into \pos, not the render-space top-left.
        ScreenshotOverlayPosiion = Rotation == 0
            ? $"X: {ResultX}, Y: {ResultY}"
            : $"X: {ResultX}, Y: {ResultY}, {Rotation}°";
    }
}
