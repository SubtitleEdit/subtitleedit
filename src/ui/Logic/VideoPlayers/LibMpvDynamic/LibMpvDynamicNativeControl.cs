using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;
using System.Timers;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

public class LibMpvDynamicNativeControl : NativeControlHost
{
    private LibMpvDynamicPlayer? _mpvPlayer;
    private bool _isInitialized;
    private IntPtr _nativeHandle;
    private Timer? _resizeTimer;
    private bool _isResizing;

    public LibMpvDynamicPlayer? Player => _mpvPlayer;

    public LibMpvDynamicNativeControl(LibMpvDynamicPlayer mpvPlayer)
    {
        _mpvPlayer = mpvPlayer;
        ClipToBounds = true;

        // Force arrow cursor immediately
        Cursor = new Cursor(StandardCursorType.Arrow);

        // Initialize resize timer - wait for resize to finish before showing video again
        _resizeTimer = new Timer(10); // 10ms after resize stops
        _resizeTimer.AutoReset = false;
        _resizeTimer.Elapsed += OnResizeFinished;

        // Handle when control is loaded
        Loaded += (s, e) =>
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            PlatformCursorManager.ForceArrowCursor();
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Detect window resize
        if (change.Property.Name == nameof(Bounds) && _isInitialized && _mpvPlayer != null)
        {
            var newBounds = (Rect)change.NewValue!;
            var oldBounds = (Rect)(change.OldValue ?? new Rect());

            // Check if size actually changed
            if (Math.Abs(newBounds.Width - oldBounds.Width) > 1 || 
                Math.Abs(newBounds.Height - oldBounds.Height) > 1)
            {
                OnResizeStarted();
            }
        }
    }

    private void OnResizeStarted()
    {
        if (!_isResizing && _mpvPlayer != null)
        {
            _isResizing = true;
            
            // Hide the native control during resize to prevent flashing
            IsVisible = false;
        }

        // Reset timer - resize is ongoing
        _resizeTimer?.Stop();
        _resizeTimer?.Start();
    }

    private void OnResizeFinished(object? sender, ElapsedEventArgs e)
    {
        // Resize has stopped, show the control again
        Dispatcher.UIThread.Post(() =>
        {
            if (_isResizing && _mpvPlayer != null)
            {
                _isResizing = false;

                // Show the control again
                IsVisible = true;
            }
        });
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        // Get the native window handle from the parent
        _nativeHandle = parent.Handle;

        if (!_isInitialized && _mpvPlayer != null)
        {
            try
            {
                _mpvPlayer.PlayerSubName = "wid";
                System.Diagnostics.Debug.WriteLine($"Initializing mpv with native window handle: {_nativeHandle}");

                // Initialize mpv with native window embedding
                InitializeWithNativeWindow(_nativeHandle);

                // Subscribe to render requests
                _mpvPlayer.RequestRender += OnMpvRequestRender;

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("MpvPlayer initialized successfully with native window!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize MpvPlayer: {ex.Message}");
            }
        }

        // Return the native handle as the platform handle
        return new PlatformHandle(_nativeHandle, "HWND");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_mpvPlayer != null)
        {
            _mpvPlayer.RequestRender -= OnMpvRequestRender;
        }

        // Clean up resize timer
        if (_resizeTimer != null)
        {
            _resizeTimer.Stop();
            _resizeTimer.Elapsed -= OnResizeFinished;
            _resizeTimer.Dispose();
            _resizeTimer = null;
        }

        _isInitialized = false;
        base.DestroyNativeControlCore(control);
    }

    private void InitializeWithNativeWindow(IntPtr windowHandle)
    {
        if (_mpvPlayer == null)
        {
            return;
        }

        // Load the libmpv library
        _mpvPlayer.LoadLib();

        // Set the window ID (wid) option before initialization
        // This tells mpv to embed itself in the provided window handle
        var widString = GetWindowIdString(windowHandle);
        System.Diagnostics.Debug.WriteLine($"Setting wid to: {widString}");

        var err = _mpvPlayer.SetOptionString("wid", widString);
        if (err < 0)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set wid: {_mpvPlayer.GetErrorString(err)}");
        }

        // Set video output to use the embedded window
        _mpvPlayer.SetOptionString("vo", "gpu");

        // Keep subtitles off (we'll handle them separately)
        _mpvPlayer.SetOptionString("sid", "no");

        // Keep the video paused at the end
        _mpvPlayer.SetOptionString("keep-open", "always");

        // Platform-specific GPU context configuration
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            ConfigureLinuxGpuContext();
        }

        // Initialize mpv
        err = _mpvPlayer.Initialize();
        if (err < 0)
        {
            throw new InvalidOperationException($"Failed to initialize mpv: {_mpvPlayer.GetErrorString(err)}");
        }

        // After initialization, ensure cursor is reset
        Dispatcher.UIThread.Post(() =>
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            PlatformCursorManager.ForceArrowCursor();
        }, DispatcherPriority.Background);
    }

    private void ConfigureLinuxGpuContext()
    {
        if (_mpvPlayer == null)
        {
            return;
        }

        try
        {
            var sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")?.ToLowerInvariant();
            var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            var x11Display = Environment.GetEnvironmentVariable("DISPLAY");

            if (sessionType == "wayland" || (!string.IsNullOrEmpty(waylandDisplay) && sessionType == null))
            {
                _mpvPlayer.SetOptionString("gpu-context", "wayland");
            }
            else if (sessionType == "x11" || (!string.IsNullOrEmpty(x11Display) && sessionType == null))
            {
                _mpvPlayer.SetOptionString("gpu-context", "x11egl");
            }
        }
        catch
        {
            // Ignore detection errors; fallback to mpv defaults
        }
    }

    private static string GetWindowIdString(IntPtr handle)
    {
        // Convert the window handle to a string format that mpv expects
        // On Windows, this is a decimal string
        // On Linux (X11), this is a decimal string
        // On macOS, this needs special handling

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return handle.ToString();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // X11 window ID
            return handle.ToString();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS NSView pointer needs to be passed as-is
            return handle.ToString();
        }

        return handle.ToString();
    }

    private void OnMpvRequestRender()
    {
        // With native window embedding, mpv handles rendering automatically
        // We just need to invalidate to trigger any UI updates if needed
        Dispatcher.UIThread.Post(() => InvalidateVisual(), DispatcherPriority.Render);
    }

    public void LoadFile(string path)
    {
        _mpvPlayer?.LoadFile(path);
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
