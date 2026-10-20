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
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;

public partial class AssaSetPositionViewModel : ObservableObject
{
    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public Image? ScreenshotImage { get; set; }
    /// <summary>
    /// The draggable text (with its selection box and anchor dot) - sized, placed and rotated here.
    /// </summary>
    public Control? ScreenshotOverlayImage { get; set; }

    /// <summary>
    /// Where the frame is shown inside <see cref="VideoGrid"/>, for drawing the guides over it.
    /// </summary>
    public event EventHandler<Rect>? FrameRectChanged;
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

    /// <summary>
    /// The \pos point in script (PlayRes) space - shown and edited in the X/Y fields.
    /// </summary>
    [ObservableProperty] private int _positionX;
    [ObservableProperty] private int _positionY;

    /// <summary>
    /// The anchor grid cell the line was last placed with (0 = top left ... 8 = bottom right), -1 after a drag or edit.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AnchorName))]
    private int _anchorIndex = -1;

    [ObservableProperty] private string _tagsPreview = string.Empty;
    [ObservableProperty] private string _lineInfo = string.Empty;

    /// <summary>
    /// False until the text is rendered - the position is unknown until then, so moving the text
    /// and OK wait for it.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    private bool _isPreviewReady;
    [ObservableProperty] private bool _showSafeArea = Se.Settings.Assa.SetPositionShowSafeArea;
    [ObservableProperty] private bool _showCenterLines = Se.Settings.Assa.SetPositionShowCenterLines;
    [ObservableProperty] private bool _snapToGuides = Se.Settings.Assa.SetPositionSnapToGuides;
    [ObservableProperty] private bool _clickToPlace = Se.Settings.Assa.SetPositionClickToPlace;
    [ObservableProperty] private bool _showSelection = Se.Settings.Assa.SetPositionShowSelection;

    /// <summary>
    /// The title-safe frame the anchor grid places the line inside, as a fraction of each side.
    /// </summary>
    public const double SafeAreaMargin = 0.05;

    public static IReadOnlyList<string> AnchorNames =>
    [
        Se.Language.Assa.SetPositionTopLeft, Se.Language.Assa.SetPositionTopCenter, Se.Language.Assa.SetPositionTopRight,
        Se.Language.Assa.SetPositionMiddleLeft, Se.Language.General.Center, Se.Language.Assa.SetPositionMiddleRight,
        Se.Language.Assa.SetPositionBottomLeft, Se.Language.Assa.SetPositionBottomCenter, Se.Language.Assa.SetPositionBottomRight,
    ];

    public string AnchorName => AnchorIndex >= 0 && AnchorIndex < 9 ? AnchorNames[AnchorIndex] : Se.Language.Assa.SetPositionCustom;

    private bool _updatingPositionFields;
    private int _initialScreenshotX;
    private int _initialScreenshotY;
    private decimal _initialRotation;

    private static readonly Regex FrzRegex = new(@"\\frz\(?(-?\d+(\.\d+)?)\)?", RegexOptions.Compiled);
    private static readonly Regex PosRegex = new(@"\\pos\(\s*-?\d+(\.\d+)?\s*,\s*-?\d+(\.\d+)?\s*\)", RegexOptions.Compiled);
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
    private bool _isHorizontalCentered = true;
    private bool _isRightAligned = false;

    private bool _isVerticalCentered = false;
    private bool _isBottomAligned = true;

    public Subtitle ResultSubtitle => _subtitle;

    /// <summary>
    /// The effective \an alignment (1-9) - where on the text box \pos is.
    /// </summary>
    public string Alignment => _alignment;

    /// <summary>
    /// Width of the canvas the text was rendered on (ScreenshotX/Y space).
    /// </summary>
    public int RenderWidth => _renderWidth;

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

    partial void OnPositionXChanged(int value) => MoveAnchorTo(value, null);

    partial void OnPositionYChanged(int value) => MoveAnchorTo(null, value);

    /// <summary>
    /// X/Y fields: put the \pos point there (script space), keeping the text's size.
    /// </summary>
    private void MoveAnchorTo(int? x, int? y)
    {
        if (_updatingPositionFields || !IsPreviewReady)
        {
            return;
        }

        AnchorIndex = -1;
        if (x.HasValue)
        {
            ScreenshotX += (int)Math.Round((x.Value - ResultX) * (double)_renderWidth / Math.Max(1, SourceWidth));
        }

        if (y.HasValue)
        {
            ScreenshotY += (int)Math.Round((y.Value - ResultY) * (double)_renderHeight / Math.Max(1, SourceHeight));
        }

        // The text box moves in whole render pixels, so a typed script-space value can round to no
        // move at all (PlayRes larger than the video) - the field then showed 961 while OK wrote
        // 960. Show the value that will be written.
        UpdatePositionFields();
    }

    partial void OnShowSafeAreaChanged(bool value) => Se.Settings.Assa.SetPositionShowSafeArea = value;

    partial void OnShowCenterLinesChanged(bool value) => Se.Settings.Assa.SetPositionShowCenterLines = value;

    partial void OnSnapToGuidesChanged(bool value) => Se.Settings.Assa.SetPositionSnapToGuides = value;

    partial void OnClickToPlaceChanged(bool value) => Se.Settings.Assa.SetPositionClickToPlace = value;
    partial void OnShowSelectionChanged(bool value) => Se.Settings.Assa.SetPositionShowSelection = value;

    /// <summary>
    /// Anchor grid: places the text box inside the safe area - left/center/right, top/middle/bottom.
    /// </summary>
    [RelayCommand]
    private void SetAnchor(int index)
    {
        if (index < 0 || index > 8 || !IsPreviewReady)
        {
            return;
        }

        var width = ScreenshotOverlayText.Size.Width;
        var height = ScreenshotOverlayText.Size.Height;
        var marginX = _renderWidth * SafeAreaMargin;
        var marginY = _renderHeight * SafeAreaMargin;
        var column = index % 3;
        var row = index / 3;
        var x = column switch { 0 => marginX, 1 => (_renderWidth - width) / 2, _ => _renderWidth - marginX - width };
        var y = row switch { 0 => marginY, 1 => (_renderHeight - height) / 2, _ => _renderHeight - marginY - height };
        ScreenshotX = (int)Math.Round(x, MidpointRounding.AwayFromZero);
        ScreenshotY = (int)Math.Round(y, MidpointRounding.AwayFromZero);
        AnchorIndex = index;
    }

    /// <summary>
    /// Back to where the line was when the dialog opened.
    /// </summary>
    [RelayCommand]
    private void ResetPosition()
    {
        if (!IsPreviewReady)
        {
            return;
        }

        ScreenshotX = _initialScreenshotX;
        ScreenshotY = _initialScreenshotY;
        Rotation = _initialRotation;
        AnchorIndex = -1;
    }

    [RelayCommand]
    private void ResetRotation() => Rotation = 0;

    /// <summary>
    /// A drag moved the text box to this render-space top-left. With snapping on, an edge or the
    /// center that comes close to a guide (center lines, safe area) sticks to it.
    /// </summary>
    public void DragTo(int x, int y)
    {
        if (!IsPreviewReady)
        {
            return;
        }

        AnchorIndex = -1;
        if (SnapToGuides)
        {
            var width = ScreenshotOverlayText.Size.Width;
            var height = ScreenshotOverlayText.Size.Height;
            x = (int)Math.Round(Snap(x, width, _renderWidth));
            y = (int)Math.Round(Snap(y, height, _renderHeight));
        }

        ScreenshotX = x;
        ScreenshotY = y;
    }

    /// <summary>
    /// Click-to-place mode: centers the text box on this render-space point (snapping applies).
    /// </summary>
    public void CenterAt(double x, double y)
    {
        DragTo((int)Math.Round(x - ScreenshotOverlayText.Size.Width / 2.0, MidpointRounding.AwayFromZero),
            (int)Math.Round(y - ScreenshotOverlayText.Size.Height / 2.0, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Snaps a box start so its start, center or end lands on a guide within 1% of the frame.
    /// </summary>
    internal static double Snap(double start, double size, double frameSize)
    {
        var threshold = frameSize * 0.01;
        var margin = frameSize * SafeAreaMargin;
        var best = start;
        var bestDistance = threshold;
        foreach (var (guide, offset) in new[] { (margin, 0.0), (frameSize / 2, size / 2), (frameSize - margin, size) })
        {
            var candidate = guide - offset;
            var distance = Math.Abs(candidate - start);
            if (distance <= bestDistance)
            {
                best = candidate;
                bestDistance = distance;
            }
        }

        return best;
    }

    /// <summary>
    /// Arrow keys: move by one render pixel (ten with Shift).
    /// </summary>
    internal void Nudge(int dx, int dy)
    {
        if (!IsPreviewReady)
        {
            return;
        }

        AnchorIndex = -1;
        ScreenshotX += dx;
        ScreenshotY += dy;
    }

    partial void OnScreenshotXChanged(int value)
    {
        // Only update if UI elements are initialized
        if (VideoGrid != null && ScreenshotOverlayImage != null && ScreenshotImage != null)
        {
            UpdateOverlayPosition();
        }
        else
        {
            UpdatePositionFields();
        }
    }

    partial void OnScreenshotYChanged(int value)
    {
        // Only update if UI elements are initialized
        if (VideoGrid != null && ScreenshotOverlayImage != null && ScreenshotImage != null)
        {
            UpdateOverlayPosition();
        }
        else
        {
            UpdatePositionFields();
        }
    }

    partial void OnRotationChanged(decimal value)
    {
        if (VideoGrid != null && ScreenshotOverlayImage != null && ScreenshotImage != null)
        {
            UpdateOverlayPosition();
        }
        else
        {
            UpdatePositionFields();
        }
    }

    /// <summary>
    /// Video time for the background frame: the player position when it is paused inside the
    /// line (so a mid-line frame can be used for alignment, like SE4 - #15817), else the line start.
    /// </summary>
    public static double GetScreenshotSeconds(SubtitleLineViewModel line, double? videoPositionSeconds)
    {
        var startSeconds = line.StartTime.TotalMilliseconds / 1000.0;
        var endSeconds = line.EndTime.TotalMilliseconds / 1000.0;
        if (videoPositionSeconds is { } position && position >= startSeconds && position <= endSeconds)
        {
            return position;
        }

        return startSeconds;
    }

    public void Initialize(Subtitle subtitle, SubtitleLineViewModel line, string? videoFileName, int? videoWidth, int? videoHeight, double? videoPositionSeconds = null)
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

        Rotation = ReadRotation(line.Text, _styleAngle);

        // Without a video, render at the script's own resolution so PlayRes aspect is honored and
        // render space equals script space. GetScreenShotWithSubtitle bumps odd sizes to even for
        // the encoder - mirror that here so the stored dimensions match the actual canvas.
        _renderWidth = videoWidth is > 0 ? videoWidth.Value : SourceWidth;
        _renderHeight = videoHeight is > 0 ? videoHeight.Value : SourceHeight;
        _renderWidth += _renderWidth % 2;
        _renderHeight += _renderHeight % 2;

        LineInfo = $"#{line.Number}   {new TimeCode(line.StartTime).ToDisplayString()}   ·   {SourceWidth} × {SourceHeight}";

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

        // The frame and the text come from two ffmpeg runs, which made the dialog open with a
        // noticeable lag (#16289). Show it at once on the checkered background and fill both in
        // when they are ready.
        Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetWidth, TargetHeight);
        var previewSubtitle = MakePreviewSubtitle(_subtitle, line);
        var seconds = GetScreenshotSeconds(line, videoPositionSeconds);
        _ = LoadPreviewAsync(previewSubtitle, videoFileName, seconds);
    }

    private async Task LoadPreviewAsync(Subtitle previewSubtitle, string? videoFileName, double seconds)
    {
        var renderWidth = _renderWidth;
        var renderHeight = _renderHeight;
        var textTask = Task.Run(() => RenderText(previewSubtitle, renderWidth, renderHeight));
        var frameTask = string.IsNullOrEmpty(videoFileName)
            ? Task.FromResult<Bitmap?>(null)
            : Task.Run(() => LoadFrame(videoFileName, seconds));

        try
        {
            var text = await textTask;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (text != null)
                {
                    ScreenshotOverlayText = text.Value.Bitmap;
                    ScreenshotX = text.Value.Left;
                    ScreenshotY = text.Value.Top;
                }

                _initialScreenshotX = ScreenshotX;
                _initialScreenshotY = ScreenshotY;
                _initialRotation = Rotation;
                IsPreviewReady = true;
                UpdateOverlayPosition();
                UpdatePositionFields();
            });

            var frame = await frameTask;
            if (frame != null)
            {
                await Dispatcher.UIThread.InvokeAsync(() => Screenshot = frame);
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Set position: preview failed");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _initialScreenshotX = ScreenshotX;
                _initialScreenshotY = ScreenshotY;
                _initialRotation = Rotation;
                IsPreviewReady = true;
            });
        }
    }

    /// <summary>
    /// Renders the line with ffmpeg/libass and trims it to the text. ScreenshotX/Y start at the
    /// trimmed box's top-left in render space.
    /// </summary>
    private static (Bitmap Bitmap, int Left, int Top)? RenderText(Subtitle previewSubtitle, int renderWidth, int renderHeight)
    {
        // ffmpeg can fail to produce the frame (missing/blocked binary, odd filter input): decoding
        // a null file name threw and took the whole dialog down. The png and both bitmaps are ours
        // to clean up.
        var previewScreenshotFileName = FfmpegGenerator.GetScreenShotWithSubtitle(previewSubtitle, renderWidth, renderHeight);
        if (string.IsNullOrEmpty(previewScreenshotFileName))
        {
            return null;
        }

        using var skBitmap = SKBitmap.Decode(previewScreenshotFileName);
        try
        {
            File.Delete(previewScreenshotFileName);
        }
        catch
        {
            // ignore cleanup errors
        }

        if (skBitmap == null)
        {
            return null;
        }

        using var trimResult = skBitmap.TrimTransparentPixels();
        return (trimResult.TrimmedBitmap.ToAvaloniaBitmap(), trimResult.Left, trimResult.Top);
    }

    private static Bitmap? LoadFrame(string videoFileName, double seconds)
    {
        // Pass exact seconds (not ToDisplayString, which honors the HH:MM:SS:FF time-code format
        // setting and would feed ffmpeg an unparseable "-ss 00:01:23:15", blanking the preview - #12182).
        var fileName = FfmpegGenerator.GetScreenShot(videoFileName, seconds.ToString("0.###", CultureInfo.InvariantCulture));
        if (!File.Exists(fileName))
        {
            return null;
        }

        try
        {
            return new Bitmap(fileName);
        }
        catch
        {
            return null;
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
        _isHorizontalCentered = alignment == "2" || alignment == "5" || alignment == "8";
        _isRightAligned = alignment == "3" || alignment == "6" || alignment == "9";

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

    /// <summary>
    /// Writes the OK tags into the line, replacing any previous \pos/\frz. When the line already
    /// starts with an override block the tags join it, so "{\an8\i1}Hi" becomes
    /// "{\pos(..)\an8\i1}Hi" instead of "{\pos(..)}{\an8\i1}Hi".
    /// </summary>
    internal static string ApplyPositionTags(string text, string tags)
    {
        var rest = PosRegex.Replace(text ?? string.Empty, string.Empty);
        rest = RemoveOutsideTransforms(rest, FrzRegex).Replace("{}", string.Empty);
        if (rest.StartsWith("{\\", StringComparison.Ordinal) && rest.IndexOf('}') > 0)
        {
            return "{" + tags + rest.Substring(1);
        }

        return "{" + tags + "}" + rest;
    }

    /// <summary>
    /// The line's static rotation: its first \frz outside a \t(...) animation, else the style angle.
    /// A \frz inside \t is where the animation ends, not the rotation the line starts at.
    /// </summary>
    internal static decimal ReadRotation(string? text, decimal styleAngle)
    {
        text ??= string.Empty;
        foreach (var (start, length) in GetSegmentsOutsideTransforms(text))
        {
            var match = FrzRegex.Match(text, start, length);
            if (match.Success)
            {
                return decimal.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var frz)
                    ? frz
                    : styleAngle;
            }
        }

        return styleAngle;
    }

    /// <summary>
    /// Removes the regex matches from the text but leaves the contents of \t(...) animations alone,
    /// so "{\t(0,1000,\frz360)}" keeps its animated rotation.
    /// </summary>
    private static string RemoveOutsideTransforms(string text, Regex regex)
    {
        var sb = new StringBuilder(text.Length);
        var last = 0;
        foreach (var (start, length) in GetSegmentsOutsideTransforms(text))
        {
            sb.Append(text, last, start - last); // the \t(...) in between, untouched
            sb.Append(regex.Replace(text.Substring(start, length), string.Empty));
            last = start + length;
        }

        return sb.ToString();
    }

    private static List<(int Start, int Length)> GetSegmentsOutsideTransforms(string text)
    {
        var segments = new List<(int Start, int Length)>();
        var segmentStart = 0;
        var t = text.IndexOf("\\t(", StringComparison.Ordinal);
        while (t >= 0)
        {
            var depth = 0;
            var end = t + 2;
            for (; end < text.Length; end++)
            {
                if (text[end] == '(')
                {
                    depth++;
                }
                else if (text[end] == ')' && --depth == 0)
                {
                    break;
                }
                else if (text[end] == '}')
                {
                    end--; // unclosed \t(: it ends with its override block
                    break;
                }
            }

            end = Math.Min(end, text.Length - 1);
            segments.Add((segmentStart, t - segmentStart));
            segmentStart = end + 1;
            t = text.IndexOf("\\t(", segmentStart, StringComparison.Ordinal);
        }

        segments.Add((segmentStart, text.Length - segmentStart));
        return segments;
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
        if (!IsPreviewReady)
        {
            return;
        }

        AnchorIndex = -1;
        // ScreenshotX/Y are render-space, so center on the render canvas - centering on
        // SourceWidth (PlayRes) was off whenever the two resolutions differed.
        ScreenshotX = (int)Math.Round(_renderWidth / 2.0 - ScreenshotOverlayText.Size.Width / 2.0, MidpointRounding.AwayFromZero);
    }

    [RelayCommand]
    private async Task CenterVertically()
    {
        if (!IsPreviewReady)
        {
            return;
        }

        AnchorIndex = -1;
        ScreenshotY = (int)Math.Round(_renderHeight / 2.0 - ScreenshotOverlayText.Size.Height / 2.0, MidpointRounding.AwayFromZero);
    }

    [RelayCommand(CanExecute = nameof(IsPreviewReady))]
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
        var isTyping = e.Source is Avalonia.Visual visual &&
                       (visual is TextBox || Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<TextBox>(visual) != null);
        if (!isTyping && e.Key is Key.Left or Key.Right or Key.Up or Key.Down && (e.KeyModifiers & ~KeyModifiers.Shift) == KeyModifiers.None)
        {
            var step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1;
            Nudge(e.Key == Key.Left ? -step : e.Key == Key.Right ? step : 0, e.Key == Key.Up ? -step : e.Key == Key.Down ? step : 0);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
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
        UpdatePositionFields();
        FrameRectChanged?.Invoke(this, new Rect(offsetX, offsetY, screenshotImageWidth, screenshotImageHeight));
    }

    /// <summary>
    /// X/Y fields and the tag preview follow the text box (without moving it again).
    /// </summary>
    private void UpdatePositionFields()
    {
        if (!IsPreviewReady)
        {
            return; // nothing measured yet - the fields would show the placeholder's 0, 0
        }

        _updatingPositionFields = true;
        try
        {
            PositionX = ResultX;
            PositionY = ResultY;
        }
        finally
        {
            _updatingPositionFields = false;
        }

        TagsPreview = "{" + ResultTags + "}";
    }
}
