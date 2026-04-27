using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;

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
    private IntPtr _ownedChildHandle;

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_CLIPSIBLINGS = 0x04000000;
    private const uint WS_CLIPCHILDREN = 0x02000000;

    // Static Control Styles
    private const uint SS_BLACKRECT = 0x00000004;

    // Extended Window Styles
    private const uint WS_EX_TRANSPARENT = 0x00000020;

    private static bool ShouldUseOwnedChildHandle => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

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
        _nativeHandle = CreateRenderTargetHandle(parent.Handle);
        var handleDescriptor = _ownedChildHandle != IntPtr.Zero ? "HWND" : parent.HandleDescriptor;

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

        return new PlatformHandle(_nativeHandle, handleDescriptor);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_ownedChildHandle != IntPtr.Zero)
        {
            DestroyWindow(_ownedChildHandle);
            _ownedChildHandle = IntPtr.Zero;
        }

        _nativeHandle = IntPtr.Zero;
        _isInitialized = false;
        base.DestroyNativeControlCore(control);
    }

    private IntPtr CreateRenderTargetHandle(IntPtr parentHandle)
    {
        if (!ShouldUseOwnedChildHandle)
        {
            return parentHandle;
        }

        // --- CONFIGURATION ---
        // To make it BLACK: Use SS_BLACKRECT and exStyle = 0
        // To make it TRANSPARENT: Remove SS_BLACKRECT and use WS_EX_TRANSPARENT

        uint style = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | SS_BLACKRECT;
        uint exStyle = 0;

        // Uncomment these two lines for Transparency instead of Black:
        // style = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
        // exStyle = WS_EX_TRANSPARENT;

        _ownedChildHandle = CreateWindowExW(
            exStyle,
            "STATIC",
            string.Empty,
            style,
            0,
            0,
            1,
            1,
            parentHandle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (_ownedChildHandle == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.WriteLine($"Failed to create VLC child host window: {error}. Falling back to parent handle.");
            return parentHandle;
        }

        return _ownedChildHandle;
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

        _vlcPlayer.SetWindowHandle(windowHandle);

        Dispatcher.UIThread.Post(() =>
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            PlatformCursorManager.ForceArrowCursor();
        }, DispatcherPriority.Background);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowExW(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hWnd);

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