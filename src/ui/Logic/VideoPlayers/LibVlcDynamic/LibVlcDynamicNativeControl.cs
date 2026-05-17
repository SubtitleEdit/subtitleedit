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
    private IntPtr _x11Display;

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
    private static bool ShouldUseOwnedX11Child => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

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
        var handleDescriptor = parent.HandleDescriptor;
        if (_ownedChildHandle != IntPtr.Zero && ShouldUseOwnedChildHandle)
        {
            handleDescriptor = "HWND";
        }

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
            if (ShouldUseOwnedX11Child && _x11Display != IntPtr.Zero)
            {
                XDestroyWindow(_x11Display, _ownedChildHandle);
                XSync(_x11Display, false);
                XCloseDisplay(_x11Display);
                _x11Display = IntPtr.Zero;
            }
            else
            {
                DestroyWindow(_ownedChildHandle);
            }
            _ownedChildHandle = IntPtr.Zero;
        }

        _nativeHandle = IntPtr.Zero;
        _isInitialized = false;
        base.DestroyNativeControlCore(control);
    }

    private IntPtr CreateRenderTargetHandle(IntPtr parentHandle)
    {
        if (ShouldUseOwnedX11Child)
        {
            return CreateX11ChildWindow(parentHandle);
        }

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

    // Avalonia's NativeControlHost on X11 hands us the host window's XID. libVLC then renders
    // into that window via libvlc_media_player_set_xwindow, but the host window has no opaque
    // background, so VLC's output composites over whatever sits behind the Avalonia window —
    // visible as translucent video (see issue #10977). Mirror the Windows path: create an
    // X11 child window with BlackPixel as its background and give that XID to VLC.
    private IntPtr CreateX11ChildWindow(IntPtr parentHandle)
    {
        try
        {
            _x11Display = XOpenDisplay(IntPtr.Zero);
            if (_x11Display == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine("Failed to open X11 display for VLC child window. Falling back to parent handle.");
                return parentHandle;
            }

            var screen = XDefaultScreen(_x11Display);
            var black = XBlackPixel(_x11Display, screen);

            _ownedChildHandle = XCreateSimpleWindow(
                _x11Display,
                parentHandle,
                0, 0,
                1, 1,
                0,
                black,
                black);

            if (_ownedChildHandle == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine("Failed to create X11 child window for VLC. Falling back to parent handle.");
                XCloseDisplay(_x11Display);
                _x11Display = IntPtr.Zero;
                return parentHandle;
            }

            XMapWindow(_x11Display, _ownedChildHandle);
            XSync(_x11Display, false);
            return _ownedChildHandle;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"X11 child window creation threw: {ex.Message}. Falling back to parent handle.");
            if (_x11Display != IntPtr.Zero)
            {
                XCloseDisplay(_x11Display);
                _x11Display = IntPtr.Zero;
            }
            _ownedChildHandle = IntPtr.Zero;
            return parentHandle;
        }
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

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XDefaultScreen(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern UIntPtr XBlackPixel(IntPtr display, int screenNumber);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XCreateSimpleWindow(
        IntPtr display,
        IntPtr parent,
        int x,
        int y,
        uint width,
        uint height,
        uint borderWidth,
        UIntPtr border,
        UIntPtr background);

    [DllImport("libX11.so.6")]
    private static extern int XMapWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XDestroyWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XSync(IntPtr display, [MarshalAs(UnmanagedType.Bool)] bool discard);

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