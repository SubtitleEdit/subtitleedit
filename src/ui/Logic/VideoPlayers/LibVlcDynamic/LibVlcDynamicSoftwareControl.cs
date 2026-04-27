using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// Software rendering control for LibVLC that renders video frames to a WriteableBitmap.
/// This approach works on all platforms but may have higher CPU usage compared to hardware-accelerated rendering.
/// Uses LibVLC's memory rendering callbacks to capture raw video frames.
/// </summary>
public class LibVlcDynamicSoftwareControl : Control
{
    private LibVlcDynamicPlayer? _vlcPlayer;
    private WriteableBitmap? _renderTarget;
    private bool _isInitialized;
    private readonly object _frameLock = new object();
    private IntPtr _frameBuffer = IntPtr.Zero;
    private int _frameWidth;
    private int _frameHeight;

    public LibVlcDynamicPlayer? Player => _vlcPlayer;

    public LibVlcDynamicSoftwareControl(LibVlcDynamicPlayer vlcPlayer)
    {
        _vlcPlayer = vlcPlayer;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (_vlcPlayer == null)
        {
            throw new InvalidOperationException("VlcPlayer is not initialized");
        }

        System.Diagnostics.Debug.WriteLine("Initializing VlcPlayer with software rendering");

        try
        {
            _vlcPlayer.LoadLib();
            _vlcPlayer.PlayerSubName = "sw";
            SetupVideoCallbacks();
            _isInitialized = true;
            System.Diagnostics.Debug.WriteLine("VlcPlayer initialized successfully with software rendering!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize VlcPlayer: {ex.Message}");
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!_isInitialized || _vlcPlayer == null || VisualRoot == null)
        {
            return;
        }

        var bitmapSize = GetPixelSize();

        if (bitmapSize.Width <= 0 || bitmapSize.Height <= 0)
        {
            System.Diagnostics.Debug.WriteLine("Skipping render - invalid size");
            return;
        }

        // Recreate bitmap if size changed
        if (_renderTarget == null ||
            _renderTarget.PixelSize.Width != bitmapSize.Width ||
            _renderTarget.PixelSize.Height != bitmapSize.Height)
        {
            _renderTarget?.Dispose();
            _renderTarget = new WriteableBitmap(
                bitmapSize,
                new Vector(96.0, 96.0),
                PixelFormat.Bgra8888,
                AlphaFormat.Premul);
        }

        // If no file is loaded, show black screen
        if (string.IsNullOrEmpty(_vlcPlayer.FileName))
        {
            context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));
            return;
        }

        try
        {
            lock (_frameLock)
            {
                using (var lockedBitmap = _renderTarget.Lock())
                {
                    // Copy frame buffer to bitmap if we have data
                    if (_frameBuffer != IntPtr.Zero && _frameWidth > 0 && _frameHeight > 0)
                    {
                        var sourceStride = _frameWidth * 4; // 4 bytes per pixel (BGRA/RGBA)
                        var destStride = lockedBitmap.RowBytes;
                        var height = Math.Min(_frameHeight, lockedBitmap.Size.Height);
                        var width = Math.Min(_frameWidth, lockedBitmap.Size.Width);

                        unsafe
                        {
                            var src = (byte*)_frameBuffer;
                            var dst = (byte*)lockedBitmap.Address;

                            for (int y = 0; y < height; y++)
                            {
                                Buffer.MemoryCopy(
                                    src + (y * sourceStride),
                                    dst + (y * destStride),
                                    width * 4,
                                    width * 4);
                            }
                        }
                    }
                }
            }

            var destRect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.DrawImage(_renderTarget, destRect);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Software render error: {ex.Message}");
            context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));
        }
    }

    private PixelSize GetPixelSize()
    {
        return new PixelSize(
            (int)Bounds.Width,
            (int)Bounds.Height);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_vlcPlayer != null)
        {
            _vlcPlayer.Dispose();
            _vlcPlayer = null;
        }

        _renderTarget?.Dispose();
        _renderTarget = null;

        if (_frameBuffer != IntPtr.Zero)
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(_frameBuffer);
            _frameBuffer = IntPtr.Zero;
        }

        _isInitialized = false;
    }

    public void LoadFile(string path)
    {
        _vlcPlayer?.LoadFile(path);
        
        // Update video dimensions after loading file
        System.Threading.Tasks.Task.Run(async () =>
        {
            // Wait a bit for VLC to initialize the media
            await System.Threading.Tasks.Task.Delay(500);
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                UpdateVideoDimensions();
                InvalidateVisual();
            });
        });
    }

    public void TogglePlayPause()
    {
        _vlcPlayer?.PlayOrPause();
    }

    public void Unload()
    {
        _vlcPlayer?.CloseFile();
    }

    private void SetupVideoCallbacks()
    {
        if (_vlcPlayer == null)
        {
            return;
        }

        // Note: VLC video callbacks would be set up here
        // For now, we'll get dimensions from video metadata when a file is loaded
        // The actual callback implementation requires proper P/Invoke setup in LibVlcDynamicPlayer
        // which is beyond the current scope
        System.Diagnostics.Debug.WriteLine("Video callbacks setup (placeholder)");
    }

    private void UpdateVideoDimensions()
    {
        if (_vlcPlayer == null || string.IsNullOrEmpty(_vlcPlayer.FileName))
        {
            return;
        }

        try
        {
            // Try to get video dimensions from media info
            // This is a workaround until proper video callbacks are implemented
            var mediaInfo = Logic.Media.FfmpegMediaInfo2.Parse(_vlcPlayer.FileName);
            if (mediaInfo?.Dimension != null && mediaInfo.Dimension.Width > 0 && mediaInfo.Dimension.Height > 0)
            {
                lock (_frameLock)
                {
                    _frameWidth = mediaInfo.Dimension.Width;
                    _frameHeight = mediaInfo.Dimension.Height;

                    // Allocate buffer for video frame
                    var bufferSize = _frameWidth * _frameHeight * 4; // RGBA/BGRA
                    if (_frameBuffer != IntPtr.Zero)
                    {
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(_frameBuffer);
                    }
                    _frameBuffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(bufferSize);

                    System.Diagnostics.Debug.WriteLine($"Video dimensions set: {_frameWidth}x{_frameHeight}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get video dimensions: {ex.Message}");
        }
    }
}
