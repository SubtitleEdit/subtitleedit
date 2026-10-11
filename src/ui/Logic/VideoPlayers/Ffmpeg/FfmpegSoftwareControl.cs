using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
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

        var style = OverlayStyle.FromSettings(Se.Settings.Video);
        var cache = _overlayCache;
        if (cache == null || cache.VideoRect != videoRect || cache.Style != style || !SameLines(cache.Lines, active))
        {
            cache = BuildOverlay(active, videoRect, style);
            _overlayCache = cache;
        }

        foreach (var block in cache.Blocks)
        {
            if (block.Geometry == null)
            {
                context.DrawText(block.Text, block.Origin);
                continue;
            }

            if (cache.ShadowWidth > 0)
            {
                using (context.PushTransform(Matrix.CreateTranslation(cache.ShadowWidth, cache.ShadowWidth)))
                {
                    context.DrawGeometry(cache.Shadow, cache.ShadowPen, block.Geometry);
                }
            }

            if (cache.OutlinePen != null)
            {
                context.DrawGeometry(null, cache.OutlinePen, block.Geometry);
            }

            context.DrawGeometry(cache.Fill, null, block.Geometry);
        }
    }

    private static OverlayCache BuildOverlay(List<FfmpegPreviewSubtitle.Line> active, Rect videoRect, OverlayStyle style)
    {
        var scale = videoRect.Height / 720.0;
        var fontSize = Math.Max(6, style.FontSize * 1.6 * scale);
        var margin = style.Margin * scale;
        var outlineWidth = (double)style.OutlineWidth * scale;
        var shadowWidth = (double)style.ShadowWidth * scale;
        var fill = new SolidColorBrush(style.ColorPrimary.FromHexToColor());
        var outlinePen = outlineWidth > 0
            ? new Pen(new SolidColorBrush(style.ColorOutline.FromHexToColor()), outlineWidth * 2, lineJoin: PenLineJoin.Round)
            : null;
        var shadow = new SolidColorBrush(style.ColorShadow.FromHexToColor());
        var shadowPen = outlineWidth > 0 ? new Pen(shadow, outlineWidth * 2, lineJoin: PenLineJoin.Round) : null;
        // The font name is a libass face name ("Arial Light"); Avalonia matches families, so
        // draw the face's family at the face's own weight (issue #15682).
        FontFaces.TryGetFamilyAndWeight(style.FontName, out var fontFamily, out var faceWeight);
        var weight = (FontWeight)(style.Bold ? Math.Max(faceWeight, (int)FontWeight.Bold) : faceWeight);
        var alignment = int.TryParse(style.Alignment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var a) && a is >= 1 and <= 9 ? a : 2;

        var primary = string.Join(Environment.NewLine, active.Where(l => !l.Secondary).Select(l => l.Text));
        var secondary = string.Join(Environment.NewLine, active.Where(l => l.Secondary).Select(l => l.Text));
        var primaryItalic = active.Any(l => !l.Secondary) && active.Where(l => !l.Secondary).All(l => l.Italic);
        var secondaryItalic = active.Any(l => l.Secondary) && active.Where(l => l.Secondary).All(l => l.Italic);

        var blocks = new List<OverlayBlock>(2);
        if (primary.Length > 0)
        {
            blocks.Add(MakeBlock(primary, primaryItalic, alignment));
        }

        if (secondary.Length > 0)
        {
            // The secondary subtitle goes to the top; opposite the primary when that is at the top.
            var secondaryAlignment = alignment is 7 or 8 or 9 ? alignment - 6 : alignment + 6;
            if (secondaryAlignment is < 1 or > 9)
            {
                secondaryAlignment = 8;
            }

            blocks.Add(MakeBlock(secondary, secondaryItalic, secondaryAlignment));
        }

        return new OverlayCache(active, videoRect, style, blocks, fill, outlinePen, shadow, shadowPen, shadowWidth);

        OverlayBlock MakeBlock(string text, bool italic, int numpadAlignment)
        {
            var typeface = new Typeface(fontFamily, italic ? FontStyle.Italic : FontStyle.Normal, weight);
            var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontSize, fill)
            {
                TextAlignment = numpadAlignment is 1 or 4 or 7 ? TextAlignment.Left : numpadAlignment is 3 or 6 or 9 ? TextAlignment.Right : TextAlignment.Center,
                MaxTextWidth = Math.Max(1, videoRect.Width - 2 * margin),
            };

            // Horizontal placement is TextAlignment's job: the text box spans the video width
            // minus margins, so the origin is always the left margin.
            var x = videoRect.X + margin;
            var y = numpadAlignment switch
            {
                7 or 8 or 9 => videoRect.Y + margin,
                4 or 5 or 6 => videoRect.Y + (videoRect.Height - formatted.Height) / 2,
                _ => videoRect.Bottom - margin - formatted.Height,
            };

            var origin = new Point(x, y);
            return new OverlayBlock(formatted, origin, formatted.BuildGeometry(origin));
        }
    }

    /// <summary>The preview settings the overlay is drawn with - a change means a rebuild.</summary>
    private readonly record struct OverlayStyle(
        string FontName,
        int FontSize,
        bool Bold,
        int Margin,
        string ColorPrimary,
        string ColorOutline,
        string ColorShadow,
        decimal OutlineWidth,
        decimal ShadowWidth,
        string Alignment)
    {
        public static OverlayStyle FromSettings(SeVideo settings) => new(
            settings.MpvPreviewFontName,
            settings.MpvPreviewFontSize,
            settings.MpvPreviewFontBold,
            settings.MpvPreviewMargin,
            settings.MpvPreviewColorPrimary,
            settings.MpvPreviewColorOutline,
            settings.MpvPreviewColorShadow,
            settings.MpvPreviewOutlineWidth,
            settings.MpvPreviewShadowWidth,
            settings.MpvPreviewAlignment);
    }

    private sealed record OverlayBlock(FormattedText Text, Point Origin, Geometry? Geometry);

    private sealed record OverlayCache(
        List<FfmpegPreviewSubtitle.Line> Lines,
        Rect VideoRect,
        OverlayStyle Style,
        List<OverlayBlock> Blocks,
        IBrush Fill,
        IPen? OutlinePen,
        IBrush Shadow,
        IPen? ShadowPen,
        double ShadowWidth);

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
