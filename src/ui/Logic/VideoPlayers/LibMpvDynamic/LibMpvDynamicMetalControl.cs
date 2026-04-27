using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// macOS Metal-backed libmpv video control for Avalonia 12.
/// Uses the mpv Metal render API (MPV_RENDER_API_TYPE_METAL) with a
/// <c>CAMetalLayer</c> attached to the underlying <c>NSView</c>.
/// mpv manages drawable acquisition and presentation internally once
/// the layer is passed in the init params, so no per-frame drawable
/// management is needed from this side.
/// </summary>
[SupportedOSPlatform("macos")]
public class LibMpvDynamicMetalControl : NativeControlHost
{
    private LibMpvDynamicPlayer? _mpvPlayer;
    private bool _isInitialized;
    private IntPtr _metalLayer = IntPtr.Zero;
    private IntPtr _mtlDevice = IntPtr.Zero;
    private int _lastPixelWidth;
    private int _lastPixelHeight;

    // ── Objective-C runtime ──────────────────────────────────────────────

    [DllImport("libobjc.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    // Returns id
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr msg_id(IntPtr self, IntPtr op);


    // void, one IntPtr arg
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void msg_void_id(IntPtr self, IntPtr op, IntPtr arg);

    // void, BOOL (byte) arg
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void msg_void_bool(IntPtr self, IntPtr op, byte arg);

    // void, NSUInteger arg  (pixel format enum, etc.)
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void msg_void_nuint(IntPtr self, IntPtr op, nuint arg);

    // void, one double arg  (contentsScale)
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void msg_void_double(IntPtr self, IntPtr op, double value);

    // void, two double args  (CGSize mapped as two doubles: width, height)
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void msg_void_cgsize(IntPtr self, IntPtr op, double width, double height);

    // double return  (backingScaleFactor)
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern double msg_double(IntPtr self, IntPtr op);

    // ── Metal framework ──────────────────────────────────────────────────

    [DllImport("/System/Library/Frameworks/Metal.framework/Metal")]
    private static extern IntPtr MTLCreateSystemDefaultDevice();

    // MTLPixelFormat.bgra8Unorm = 80
    private const nuint MtlPixelFormatBgra8Unorm = 80;

    // ── Public API ───────────────────────────────────────────────────────

    public LibMpvDynamicPlayer? Player => _mpvPlayer;

    public LibMpvDynamicMetalControl(LibMpvDynamicPlayer mpvPlayer)
    {
        _mpvPlayer = mpvPlayer;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    // ── NativeControlHost overrides ──────────────────────────────────────

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var handle = base.CreateNativeControlCore(parent);

        if (!_isInitialized && _mpvPlayer != null)
        {
            try
            {
                InitializeMetalOnView(handle.Handle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MetalControl] Init failed: {ex.Message}");
            }
        }

        return handle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_mpvPlayer != null)
        {
            _mpvPlayer.RequestRender -= OnMpvRequestRender;
        }

        _isInitialized = false;
        base.DestroyNativeControlCore(control);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Keep the Metal layer's drawable size in sync with the control's layout size.
        if (_isInitialized && _metalLayer != IntPtr.Zero && change.Property.Name == nameof(Bounds))
        {
            UpdateDrawableSize();
        }
    }

    // ── Setup ────────────────────────────────────────────────────────────

    private void InitializeMetalOnView(IntPtr nsView)
    {
        // 1. Get the default Metal device.
        _mtlDevice = MTLCreateSystemDefaultDevice();
        if (_mtlDevice == IntPtr.Zero)
            throw new InvalidOperationException("[MetalControl] MTLCreateSystemDefaultDevice returned NULL");

        // 2. Allocate and initialise a CAMetalLayer.
        var cls = objc_getClass("CAMetalLayer");
        var layerAlloc = msg_id(cls, sel_registerName("alloc"));
        _metalLayer = msg_id(layerAlloc, sel_registerName("init"));
        if (_metalLayer == IntPtr.Zero)
            throw new InvalidOperationException("[MetalControl] CAMetalLayer alloc/init returned NULL");

        // 3. Configure the layer.
        msg_void_id(_metalLayer, sel_registerName("setDevice:"), _mtlDevice);
        msg_void_nuint(_metalLayer, sel_registerName("setPixelFormat:"), MtlPixelFormatBgra8Unorm);
        // framebufferOnly = NO – allows mpv to sample the texture if needed.
        msg_void_bool(_metalLayer, sel_registerName("setFramebufferOnly:"), 0);

        // Sync HiDPI scale with the host window (may be 1.0 before a window exists,
        // but gets corrected on the next bounds change via UpdateDrawableSize).
        var window = msg_id(nsView, sel_registerName("window"));
        var scale = window != IntPtr.Zero
            ? msg_double(window, sel_registerName("backingScaleFactor"))
            : 1.0;
        msg_void_double(_metalLayer, sel_registerName("setContentsScale:"), scale);

        // 4. Attach the CAMetalLayer to the NSView.
        msg_void_bool(nsView, sel_registerName("setWantsLayer:"), 1);   // YES
        msg_void_id(nsView, sel_registerName("setLayer:"), _metalLayer);

        // 5. Initialise mpv with the Metal render API.
        //    Passing both device AND layer means mpv will handle drawable
        //    acquisition and presentation internally.
        if (_mpvPlayer != null)
        {
            _mpvPlayer.InitializeWithMetal(_mtlDevice, _metalLayer);
            _mpvPlayer.PlayerSubName = "Metal";
            _mpvPlayer.RequestRender += OnMpvRequestRender;
        }

        _isInitialized = true;
        System.Diagnostics.Debug.WriteLine("[MetalControl] Initialized successfully");
    }

    /// <summary>
    /// Updates <c>CAMetalLayer.drawableSize</c> to match the current control
    /// bounds scaled by the display's backing-scale factor.
    /// </summary>
    private void UpdateDrawableSize()
    {
        if (_metalLayer == IntPtr.Zero)
            return;

        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        var pixelWidth = (int)(Bounds.Width * scaling);
        var pixelHeight = (int)(Bounds.Height * scaling);

        if (pixelWidth <= 0 || pixelHeight <= 0)
            return;

        if (pixelWidth == _lastPixelWidth && pixelHeight == _lastPixelHeight)
            return;

        _lastPixelWidth = pixelWidth;
        _lastPixelHeight = pixelHeight;

        // setDrawableSize: takes a CGSize (two CGFloat / double values).
        msg_void_cgsize(_metalLayer, sel_registerName("setDrawableSize:"),
            pixelWidth, pixelHeight);

        System.Diagnostics.Debug.WriteLine($"[MetalControl] Drawable size → {pixelWidth}×{pixelHeight}");
    }

    // ── Render callback ──────────────────────────────────────────────────

    private void OnMpvRequestRender()
    {
        Dispatcher.UIThread.Post(DoRender, DispatcherPriority.Render);
    }

    private void DoRender()
    {
        if (!_isInitialized || _mpvPlayer == null || _metalLayer == IntPtr.Zero)
            return;

        // Keep drawable size up to date (guards against the first render
        // arriving before the first Bounds change notification).
        UpdateDrawableSize();

        // Nothing to show until a file is loaded.
        if (string.IsNullOrEmpty(_mpvPlayer.FileName))
            return;

        try
        {
            // Because the CAMetalLayer was supplied in the init params,
            // mpv acquires nextDrawable and presents it automatically.
            _mpvPlayer.RenderMetal();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MetalControl] Render error: {ex.Message}");
        }
    }

    // ── Public helpers (mirror the other control types) ──────────────────

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

