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
    private IntPtr _ownedChildHandle;
    private Timer? _resizeTimer;
    private bool _isResizing;

    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_CLIPSIBLINGS = 0x04000000;
    private const uint WS_CLIPCHILDREN = 0x02000000;
    private const uint WM_LBUTTONDOWN = 0x0201;
    private const uint WM_NCHITTEST = 0x0084;
    private const int GWLP_WNDPROC = -4;
    private const int HTCLIENT = 1;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private WndProcDelegate? _customWndProc;
    private IntPtr _originalWndProc = IntPtr.Zero;

    // Linux still needs the temporary hide/show workaround during live resize.
    private static bool ShouldHideNativeControlDuringResize => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    // On Windows, use a dedicated child HWND for the embedded renderer instead of
    // the parent host handle. Reusing the parent handle makes native video jump to
    // the top-left corner during live resize.
    private static bool ShouldUseOwnedChildHandle => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public LibMpvDynamicPlayer? Player => _mpvPlayer;

    public LibMpvDynamicNativeControl(LibMpvDynamicPlayer mpvPlayer)
    {
        _mpvPlayer = mpvPlayer;
        ClipToBounds = true;

        Cursor = new Cursor(StandardCursorType.Arrow);

        if (ShouldHideNativeControlDuringResize)
        {
            _resizeTimer = new Timer(10);
            _resizeTimer.AutoReset = false;
            _resizeTimer.Elapsed += OnResizeFinished;
        }

        Loaded += (s, e) =>
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            PlatformCursorManager.ForceArrowCursor();
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (ShouldHideNativeControlDuringResize && change.Property.Name == nameof(Bounds) && _isInitialized && _mpvPlayer != null)
        {
            var newBounds = (Rect)change.NewValue!;
            var oldBounds = (Rect)(change.OldValue ?? new Rect());

            if (Math.Abs(newBounds.Width - oldBounds.Width) > 1 ||
                Math.Abs(newBounds.Height - oldBounds.Height) > 1)
            {
                OnResizeStarted();
            }
        }
    }

    private void OnResizeStarted()
    {
        if (!ShouldHideNativeControlDuringResize)
        {
            return;
        }

        if (!_isResizing && _mpvPlayer != null)
        {
            _isResizing = true;
            IsVisible = false;
        }

        _resizeTimer?.Stop();
        _resizeTimer?.Start();
    }

    private void OnResizeFinished(object? sender, ElapsedEventArgs e)
    {
        if (!ShouldHideNativeControlDuringResize)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (_isResizing && _mpvPlayer != null)
            {
                _isResizing = false;
                IsVisible = true;
            }
        });
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _nativeHandle = CreateRenderTargetHandle(parent.Handle);
        var handleDescriptor = _ownedChildHandle != IntPtr.Zero ? "HWND" : parent.HandleDescriptor;

        if (!_isInitialized && _mpvPlayer != null)
        {
            try
            {
                _mpvPlayer.PlayerSubName = "wid";
                System.Diagnostics.Debug.WriteLine($"Initializing mpv with native window handle: {_nativeHandle}");

                InitializeWithNativeWindow(_nativeHandle);

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("MpvPlayer initialized successfully with native window!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize MpvPlayer: {ex.Message}");
            }
        }

        return new PlatformHandle(_nativeHandle, handleDescriptor);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_resizeTimer != null)
        {
            _resizeTimer.Stop();
            _resizeTimer.Elapsed -= OnResizeFinished;
            _resizeTimer.Dispose();
            _resizeTimer = null;
        }

        if (_ownedChildHandle != IntPtr.Zero)
        {
            if (_originalWndProc != IntPtr.Zero)
            {
                SetWindowLongPtr(_ownedChildHandle, GWLP_WNDPROC, _originalWndProc);
                _originalWndProc = IntPtr.Zero;
                _customWndProc = null;
            }
            DestroyWindow(_ownedChildHandle);
            _ownedChildHandle = IntPtr.Zero;
        }

        _nativeHandle = IntPtr.Zero;
        _isResizing = false;
        _isInitialized = false;
        base.DestroyNativeControlCore(control);
    }

    private IntPtr CreateRenderTargetHandle(IntPtr parentHandle)
    {
        if (!ShouldUseOwnedChildHandle)
        {
            return parentHandle;
        }

        _ownedChildHandle = CreateWindowExW(
            0,
            "STATIC",
            string.Empty,
            WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
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
            System.Diagnostics.Debug.WriteLine($"Failed to create mpv child host window: {error}. Falling back to parent handle.");
            return parentHandle;
        }

        _customWndProc = CustomWndProc;
        _originalWndProc = SetWindowLongPtr(_ownedChildHandle, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_customWndProc));

        return _ownedChildHandle;
    }

    private void InitializeWithNativeWindow(IntPtr windowHandle)
    {
        if (_mpvPlayer == null)
        {
            return;
        }

        _mpvPlayer.LoadLib();

        var widString = GetWindowIdString(windowHandle);
        System.Diagnostics.Debug.WriteLine($"Setting wid to: {widString}");

        var err = _mpvPlayer.SetOptionString("wid", widString);
        if (err < 0)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set wid: {_mpvPlayer.GetErrorString(err)}");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _mpvPlayer.SetOptionString("vo", "xv,x11,gpu");
        }
        else
        {
            _mpvPlayer.SetOptionString("vo", "gpu");
        }

        _mpvPlayer.SetOptionString("sid", "no");
        _mpvPlayer.SetOptionString("keep-open", "always");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _mpvPlayer.SetOptionString("idle", "yes");
            _mpvPlayer.SetOptionString("force-window", "yes");
            _mpvPlayer.SetOptionString("background-color", "#000000");
        }

        err = _mpvPlayer.Initialize();
        if (err < 0)
        {
            throw new InvalidOperationException($"Failed to initialize mpv: {_mpvPlayer.GetErrorString(err)}");
        }

        Dispatcher.UIThread.Post(() =>
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            PlatformCursorManager.ForceArrowCursor();
        }, DispatcherPriority.Background);
    }

    private static string GetWindowIdString(IntPtr handle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return handle.ToString();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return handle.ToString();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return handle.ToString();
        }

        return handle.ToString();
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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_NCHITTEST)
        {
            // The STATIC window class returns HTTRANSPARENT by default, which forwards
            // all mouse messages to the parent HWND before they reach this WndProc.
            // Returning HTCLIENT makes the window opaque to mouse input so that
            // WM_LBUTTONDOWN is actually delivered here.
            return new IntPtr(HTCLIENT);
        }
        if (msg == WM_LBUTTONDOWN)
        {
            Dispatcher.UIThread.Post(TogglePlayPause);
        }
        return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            TogglePlayPause();
            e.Handled = true;
        }
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
