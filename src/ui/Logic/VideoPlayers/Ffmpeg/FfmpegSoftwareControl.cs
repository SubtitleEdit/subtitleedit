using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Nikse.SubtitleEdit.UiLogic.Export;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// Shows <see cref="FfmpegPlayer"/> pictures through a WriteableBitmap the size of the decoded
/// picture, letterboxed to the control's bounds, and draws the subtitle preview
/// (<see cref="FfmpegPlayer.PreviewSubtitle"/>) on top with the mpv preview style settings.
/// Works everywhere Avalonia draws (no native window, so overlays on top of it work too - unlike
/// mpv-wid / VLC embedding).
/// </summary>
public class FfmpegSoftwareControl : Control
{
    private FfmpegPlayer? _player;
    private WriteableBitmap? _bitmap;
    private long _copiedVersion = -1;
    private int _invalidatePending;

    // The preview overlay follows the position, not the pictures: while paused nothing new is
    // decoded, yet an edited line must show up. A slow tick checks whether the set of visible
    // lines changed and only then redraws.
    private UiTickPump? _overlayTick;
    private List<FfmpegPreviewSubtitle.Line> _overlayLines = [];

    // Shaping the text and building its outline geometry is far more expensive than drawing
    // it, and the same line usually stays up for dozens of frames - so the prepared blocks are
    // kept until the lines, the video rectangle or the preview style change.
    private OverlayCache? _overlayCache;

    public FfmpegPlayer? Player => _player;

    public FfmpegSoftwareControl(FfmpegPlayer player)
    {
        _player = player;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
        player.FrameReady += OnFrameReady;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _overlayTick = new UiTickPump(TimeSpan.FromMilliseconds(100), CheckOverlay, DispatcherPriority.Background);
        _overlayTick.Start();
    }

    private void OnFrameReady()
    {
        // Coalesce: many frames can land between two UI ticks, one invalidate is enough.
        if (Interlocked.Exchange(ref _invalidatePending, 1) == 0)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Interlocked.Exchange(ref _invalidatePending, 0);
                InvalidateVisual();
            }, DispatcherPriority.Render);
        }
    }

    private void CheckOverlay()
    {
        var player = _player;
        if (player == null)
        {
            return;
        }

        if (!SameLines(ActiveLines(player), _overlayLines))
        {
            InvalidateVisual();
        }
    }

    private static List<FfmpegPreviewSubtitle.Line> ActiveLines(FfmpegPlayer player)
    {
        return player.PreviewSubtitlesVisible && !string.IsNullOrEmpty(player.FileName)
            ? player.PreviewSubtitle.GetActive(player.Position)
            : [];
    }

    private static bool SameLines(List<FfmpegPreviewSubtitle.Line> a, List<FfmpegPreviewSubtitle.Line> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        for (var i = 0; i < a.Count; i++)
        {
            if (!a[i].Equals(b[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.FillRectangle(Brushes.Black, bounds);

        var player = _player;
        if (player == null || string.IsNullOrEmpty(player.FileName))
        {
            // Nothing is drawn, so nothing is shown: without this, CheckOverlay keeps seeing the
            // lines from before the video was closed and repaints every tick.
            _overlayLines = [];
            _overlayCache = null;
            return;
        }

        var videoRect = bounds;
        try
        {
            var (width, height) = player.CurrentFrameSize;
            if (width > 0 && height > 0)
            {
                if (_bitmap == null || _bitmap.PixelSize.Width != width || _bitmap.PixelSize.Height != height)
                {
                    _bitmap?.Dispose();
                    _bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
                    _copiedVersion = -1;
                }

                var version = player.FrameVersion;
                if (version != _copiedVersion)
                {
                    using (var locked = _bitmap.Lock())
                    {
                        if (player.CopyCurrentFrame(locked.Address, locked.RowBytes, width, height))
                        {
                            _copiedVersion = version;
                        }
                    }
                }

                videoRect = FitRect(bounds, player.DisplayAspectRatio > 0 ? player.DisplayAspectRatio : width / (double)height);
                context.DrawImage(_bitmap, new Rect(0, 0, width, height), videoRect);
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"ffmpeg render error: {exception.Message}");
        }

        try
        {
            RenderOverlay(context, player, videoRect);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"ffmpeg overlay error: {exception.Message}");
        }
    }

    /// <summary>
    /// Draws the lines active at the player position. Sizes follow mpv's convention: the
    /// preview font size and margins are given for a 720 px tall picture and scale with the
    /// actual video rectangle, so the preview looks the same at any window size.
    /// </summary>
    private void RenderOverlay(DrawingContext context, FfmpegPlayer player, Rect videoRect)
    {
        var active = ActiveLines(player);
        _overlayLines = active;
        if (active.Count == 0 || videoRect.Height <= 0)
        {
            return;
        }

        var settings = Se.Settings.Video;
        var style = FfmpegPreviewSubtitle.TextStyle.FromSettings(settings);
        var secondaryStyle = player.PreviewSubtitle.SecondaryStyle;
        // As mpv's sub-use-margins: the black bars are part of the area the text may use (#13934).
        var area = settings.MpvPreviewMarginIsPartOfSubtitleArea ? new Rect(Bounds.Size) : videoRect;
        var cache = _overlayCache;
        if (cache == null || cache.VideoRect != videoRect || cache.Area != area || cache.Style != style ||
            cache.SecondaryStyle != secondaryStyle || !SameLines(cache.Lines, active))
        {
            cache = BuildOverlay(active, videoRect, area, style, secondaryStyle);
            _overlayCache = cache;
        }

        DrawBlocks(context, cache);
    }

    internal static void DrawBlocks(DrawingContext context, OverlayCache cache)
    {
        foreach (var block in cache.Blocks)
        {
            var paint = block.Paint;
            if (block.Box != null)
            {
                context.DrawGeometry(paint.BoxBrush, block.BoxPen, block.Box);
            }

            if (block.Geometry == null)
            {
                context.DrawText(block.Text, block.Origin);
                continue;
            }

            if (paint.ShadowWidth > 0 && paint.BoxBrush == null)
            {
                using (context.PushTransform(Matrix.CreateTranslation(paint.ShadowWidth, paint.ShadowWidth)))
                {
                    context.DrawGeometry(paint.Shadow, paint.ShadowPen, block.Geometry);
                }
            }

            if (paint.OutlinePen != null)
            {
                context.DrawGeometry(null, paint.OutlinePen, block.Geometry);
            }

            // One geometry takes one brush, so text with colors of its own is drawn as text.
            if (block.HasColors)
            {
                context.DrawText(block.Text, block.Origin);
            }
            else
            {
                context.DrawGeometry(paint.Fill, null, block.Geometry);
            }
        }
    }

    internal static OverlayCache BuildOverlay(
        List<FfmpegPreviewSubtitle.Line> active,
        Rect videoRect,
        Rect area,
        FfmpegPreviewSubtitle.TextStyle style,
        FfmpegPreviewSubtitle.TextStyle? secondaryStyle)
    {
        var scale = videoRect.Height / 720.0;
        var paint = MakePaint(style, scale);
        var secondaryPaint = secondaryStyle != null ? MakePaint(secondaryStyle, scale) : paint;

        // Text placed at the same spot stacks: the primary at the edge, the rest further in.
        var stacked = new Dictionary<int, double>();
        var blocks = new List<OverlayBlock>(2);
        var groups = active
            .GroupBy(l => (l.Secondary, Alignment: l.Alignment ?? DefaultAlignment(l.Secondary)))
            .OrderBy(g => g.Key.Secondary);
        foreach (var group in groups)
        {
            blocks.Add(MakeBlock(group.ToList(), group.Key.Secondary ? secondaryPaint : paint, group.Key.Alignment));
        }

        return new OverlayCache(active, videoRect, area, style, secondaryStyle, blocks);

        int DefaultAlignment(bool secondary)
        {
            if (!secondary)
            {
                return style.Alignment;
            }

            if (secondaryStyle != null)
            {
                return secondaryStyle.Alignment;
            }

            // No style of its own: the top, or opposite the primary when that is at the top.
            var opposite = style.Alignment is 7 or 8 or 9 ? style.Alignment - 6 : style.Alignment + 6;
            return opposite is < 1 or > 9 ? 8 : opposite;
        }

        OverlayBlock MakeBlock(List<FfmpegPreviewSubtitle.Line> lines, OverlayPaint blockPaint, int numpadAlignment)
        {
            var text = string.Join(Environment.NewLine, lines.Select(l => l.Text));
            var margin = blockPaint.Margin;
            var left = area.X + margin;
            var available = Math.Max(1, area.Width - 2 * margin);
            var horizontal = numpadAlignment is 1 or 4 or 7 ? TextAlignment.Left : numpadAlignment is 3 or 6 or 9 ? TextAlignment.Right : TextAlignment.Center;
            var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, TextToFlowDirectionConverter.GetFlowDirection(text), blockPaint.Typeface, blockPaint.FontSize, blockPaint.Fill)
            {
                TextAlignment = horizontal,
                MaxTextWidth = available,
            };

            var hasColors = false;
            var lineStart = 0;
            foreach (var line in lines)
            {
                foreach (var run in line.Runs)
                {
                    if (run.Italic)
                    {
                        formatted.SetFontStyle(FontStyle.Italic, lineStart + run.Start, run.Length);
                    }

                    if (run.Bold)
                    {
                        formatted.SetFontWeight(FontWeight.Bold, lineStart + run.Start, run.Length);
                    }

                    if (run.Color.HasValue)
                    {
                        formatted.SetForegroundBrush(new SolidColorBrush(run.Color.Value), lineStart + run.Start, run.Length);
                        hasColors = true;
                    }
                }

                lineStart += line.Text.Length + Environment.NewLine.Length;
            }

            // Horizontal placement is TextAlignment's job: the text box spans the video width
            // minus margins, so the origin is always the left margin. With justify the box is
            // as wide as the text and goes where the alignment says; the justify then only
            // moves the shorter lines inside it.
            var x = left;
            var justify = GetJustify(blockPaint.Justify);
            if (justify.HasValue)
            {
                var width = Math.Min(available, Math.Ceiling(formatted.WidthIncludingTrailingWhitespace) + 1);
                formatted.MaxTextWidth = width;
                formatted.TextAlignment = justify.Value;
                x = horizontal switch
                {
                    TextAlignment.Left => left,
                    TextAlignment.Right => left + available - width,
                    _ => left + (available - width) / 2,
                };
            }

            stacked.TryGetValue(numpadAlignment, out var stack);
            stacked[numpadAlignment] = stack + formatted.Height;
            var y = numpadAlignment switch
            {
                7 or 8 or 9 => area.Y + margin + stack,
                4 or 5 or 6 => area.Y + (area.Height - formatted.Height) / 2 + stack,
                _ => area.Bottom - margin - formatted.Height - stack,
            };

            var origin = new Point(x, y);
            Geometry? box = null;
            IPen? boxPen = null;
            if (blockPaint.BoxBrush != null)
            {
                var highlight = formatted.BuildHighlightGeometry(origin, 0, text.Length);
                if (blockPaint.OneBox)
                {
                    box = new RectangleGeometry(highlight.Bounds.Inflate(blockPaint.BoxPadding));
                }
                else
                {
                    box = highlight;
                    boxPen = new Pen(blockPaint.BoxBrush, blockPaint.BoxPadding * 2);
                }
            }

            return new OverlayBlock(formatted, origin, formatted.BuildGeometry(origin), blockPaint, hasColors, box, boxPen);
        }
    }

    private static TextAlignment? GetJustify(string? justify)
    {
        return justify switch
        {
            "left" => TextAlignment.Left,
            "center" => TextAlignment.Center,
            "right" => TextAlignment.Right,
            _ => null,
        };
    }

    private static OverlayPaint MakePaint(FfmpegPreviewSubtitle.TextStyle style, double scale)
    {
        var outlineWidth = (double)style.OutlineWidth * scale;
        var shadowWidth = (double)style.ShadowWidth * scale;
        var fill = new SolidColorBrush(style.Primary);
        var shadow = new SolidColorBrush(style.Shadow);
        var isBox = style.BorderStyle is 3 or 4;

        // The font name is a libass face name ("Arial Light"); Avalonia matches families, so
        // draw the face's family at the face's own weight (issue #15682).
        FontFaces.TryGetFamilyAndWeight(style.FontName, out var fontFamily, out var faceWeight);
        var weight = (FontWeight)(style.Bold ? Math.Max(faceWeight, (int)FontWeight.Bold) : faceWeight);

        return new OverlayPaint(
            new Typeface(fontFamily, FontStyle.Normal, weight),
            Math.Max(6, (double)style.FontSize * 1.6 * scale),
            (double)style.Margin * scale,
            style.Justify,
            fill,
            !isBox && outlineWidth > 0 ? new Pen(new SolidColorBrush(style.Outline), outlineWidth * 2, lineJoin: PenLineJoin.Round) : null,
            shadow,
            outlineWidth > 0 ? new Pen(shadow, outlineWidth * 2, lineJoin: PenLineJoin.Round) : null,
            shadowWidth,
            // As libass: a box per line is in the outline color, one box in the shadow color.
            isBox ? style.BorderStyle == 4 ? shadow : new SolidColorBrush(style.Outline) : null,
            style.BorderStyle == 4,
            Math.Max(1, Math.Max(outlineWidth, shadowWidth)));
    }

    /// <summary>What a style turns into at the current video size.</summary>
    internal sealed record OverlayPaint(
        Typeface Typeface,
        double FontSize,
        double Margin,
        string? Justify,
        IBrush Fill,
        IPen? OutlinePen,
        IBrush Shadow,
        IPen? ShadowPen,
        double ShadowWidth,
        IBrush? BoxBrush,
        bool OneBox,
        double BoxPadding);

    internal sealed record OverlayBlock(FormattedText Text, Point Origin, Geometry? Geometry, OverlayPaint Paint, bool HasColors, Geometry? Box, IPen? BoxPen);

    internal sealed record OverlayCache(
        List<FfmpegPreviewSubtitle.Line> Lines,
        Rect VideoRect,
        Rect Area,
        FfmpegPreviewSubtitle.TextStyle Style,
        FfmpegPreviewSubtitle.TextStyle? SecondaryStyle,
        List<OverlayBlock> Blocks);

    /// <summary>The largest rectangle of the given aspect ratio centered in <paramref name="bounds"/>.</summary>
    internal static Rect FitRect(Rect bounds, double aspectRatio)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0 || aspectRatio <= 0)
        {
            return bounds;
        }

        var width = bounds.Width;
        var height = width / aspectRatio;
        if (height > bounds.Height)
        {
            height = bounds.Height;
            width = height * aspectRatio;
        }

        return new Rect((bounds.Width - width) / 2, (bounds.Height - height) / 2, width, height);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _overlayTick?.Dispose();
        _overlayTick = null;

        var player = _player;
        _player = null;
        if (player != null)
        {
            player.FrameReady -= OnFrameReady;

            // Stopping the decode threads can wait on a stuck demuxer (network mounts and the
            // like); keep that off the UI thread, as the mpv software control does (#11176).
            _ = Task.Run(() =>
            {
                try
                {
                    player.Dispose();
                }
                catch (Exception exception)
                {
                    Se.LogError(exception, "FfmpegSoftwareControl background dispose");
                }
            });
        }

        _bitmap?.Dispose();
        _bitmap = null;
        _overlayCache = null;
    }
}
