using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Input;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

public class LibMpvDynamicSoftwareControl : Control
{
    private LibMpvDynamicPlayer? _mpvPlayer;
    private WriteableBitmap? _renderTarget;
    private bool _isInitialized;

    public LibMpvDynamicPlayer? Player => _mpvPlayer;

    public LibMpvDynamicSoftwareControl(LibMpvDynamicPlayer mpvPlayer)
    {
        _mpvPlayer = mpvPlayer;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (_mpvPlayer == null)
        {
            throw new InvalidOperationException("MpvPlayer is not initialized");
        }

        System.Diagnostics.Debug.WriteLine("Initializing MpvPlayer with software rendering");

        try
        {
            _mpvPlayer.InitializeWithSoftwareRendering();
            _mpvPlayer.PlayerSubName = "sw";
            _mpvPlayer.RequestRender += OnMpvRequestRender;
            _isInitialized = true;
            System.Diagnostics.Debug.WriteLine("MpvPlayer initialized successfully with software rendering!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize MpvPlayer: {ex.Message}");
        }
    }

    private void OnMpvRequestRender()
    {
        // Request a redraw on the UI thread
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!_isInitialized || _mpvPlayer == null || VisualRoot == null)
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
        if (string.IsNullOrEmpty(_mpvPlayer.FileName))
        {
            context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));
            return;
        }

        try
        {
            using (var lockedBitmap = _renderTarget.Lock())
            {
#if ANDROID
        var pixelFormat = "rgba";
#else
                var pixelFormat = "bgra";
#endif
                _mpvPlayer.SoftwareRender(
                    lockedBitmap.Size.Width,
                    lockedBitmap.Size.Height,
                    lockedBitmap.Address,
                    pixelFormat);
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
        // Don't apply scaling - use bounds directly as pixel size
        // This matches the working LibMpv.Avalonia implementation
        return new PixelSize(
            (int)Bounds.Width,
            (int)Bounds.Height);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_mpvPlayer != null)
        {
            _mpvPlayer.RequestRender -= OnMpvRequestRender;
            _mpvPlayer.Dispose();
            _mpvPlayer = null;
        }

        _renderTarget?.Dispose();
        _renderTarget = null;

        _isInitialized = false;
    }

    public void LoadFile(string path)
    {
        _mpvPlayer?.LoadFile(path);
        // Trigger initial render
        InvalidateVisual();
    }

    public void TogglePlayPause()
    {
        _mpvPlayer?.PlayOrPause();
    }

    public void Unload()
    {
        _mpvPlayer?.CloseFile();
    }
}