using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// Shows <see cref="FfmpegPlayer"/> pictures through a WriteableBitmap the size of the decoded
/// picture, letterboxed to the control's bounds. Works everywhere Avalonia draws (no native
/// window, so overlays on top of it work too - unlike mpv-wid / VLC embedding).
/// </summary>
public class FfmpegSoftwareControl : Control
{
    private FfmpegPlayer? _player;
    private WriteableBitmap? _bitmap;
    private long _copiedVersion = -1;
    private int _invalidatePending;

    public FfmpegPlayer? Player => _player;

    public FfmpegSoftwareControl(FfmpegPlayer player)
    {
        _player = player;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
        player.PlayerSubName = "sw";
        player.FrameReady += OnFrameReady;
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

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.FillRectangle(Brushes.Black, bounds);

        var player = _player;
        if (player == null || string.IsNullOrEmpty(player.FileName))
        {
            return;
        }

        try
        {
            var (width, height) = player.CurrentFrameSize;
            if (width <= 0 || height <= 0)
            {
                return;
            }

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

            context.DrawImage(_bitmap, new Rect(0, 0, width, height), FitRect(bounds, player.DisplayAspectRatio > 0 ? player.DisplayAspectRatio : width / (double)height));
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"ffmpeg render error: {exception.Message}");
        }
    }

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
    }
}
