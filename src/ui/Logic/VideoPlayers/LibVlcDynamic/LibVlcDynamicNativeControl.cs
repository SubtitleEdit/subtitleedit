using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using System;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// Native window control for LibVLC that embeds VLC's video output directly into a native window handle.
/// This approach uses platform-specific window embedding (HWND on Windows, X11 window on Linux, NSView on macOS).
/// </summary>
public class LibVlcDynamicNativeControl : NativeControlHost
{
    private LibVlcDynamicPlayer? _vlcPlayer;
    private bool _isInitialized;
    private IntPtr _nativeHandle;

    public LibVlcDynamicPlayer? Player => _vlcPlayer;

    public LibVlcDynamicNativeControl(LibVlcDynamicPlayer vlcPlayer)
    {
        _vlcPlayer = vlcPlayer;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);

        Loaded += (s, e) =>
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            PlatformCursorManager.ForceArrowCursor();
        };
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _nativeHandle = parent.Handle;

        if (!_isInitialized && _vlcPlayer != null)
        {
            try
            {
                _vlcPlayer.PlayerSubName = "native";
                System.Diagnostics.Debug.WriteLine($"Initializing VLC with native window handle: {_nativeHandle}");

                InitializeWithNativeWindow(_nativeHandle);
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("VLC Player initialized successfully with native window!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize VLC Player: {ex.Message}");
            }
        }

        return new PlatformHandle(_nativeHandle, "HWND");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        _isInitialized = false;
        base.DestroyNativeControlCore(control);
    }

    private void InitializeWithNativeWindow(IntPtr windowHandle)
    {
        if (_vlcPlayer == null)
        {
            return;
        }

        _vlcPlayer.LoadLib();

        var err = _vlcPlayer.Initialize();
        if (err < 0)
        {
            throw new InvalidOperationException("Failed to initialize VLC");
        }

        // Set the window handle for video output
        _vlcPlayer.SetWindowHandle(windowHandle);

        Dispatcher.UIThread.Post(() =>
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            PlatformCursorManager.ForceArrowCursor();
        }, DispatcherPriority.Background);
    }

    public async void LoadFile(string path)
    {
        if (_vlcPlayer == null)
        {
            return;
        }

        await _vlcPlayer.LoadFile(path);
        
        // Wait a bit for VLC to parse the media information
        // This ensures Duration and other metadata are available
        await System.Threading.Tasks.Task.Delay(300);
        
        System.Diagnostics.Debug.WriteLine($"File loaded, duration: {_vlcPlayer.Duration}s");
    }

    public void TogglePlayPause()
    {
        _vlcPlayer?.PlayOrPause();
    }

    public void Unload()
    {
        _vlcPlayer?.CloseFile();
    }
}
