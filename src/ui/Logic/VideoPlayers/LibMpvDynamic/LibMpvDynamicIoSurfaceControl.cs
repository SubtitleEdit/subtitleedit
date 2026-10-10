using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// mpv on macOS without making Avalonia render through OpenGL.
/// <para>
/// libmpv can only render with OpenGL (or in software), and <see cref="LibMpvDynamicOpenGlControl"/>
/// needs Avalonia's compositor to run on OpenGL too - Apple's deprecated path. Here mpv renders
/// in its own offscreen CGL context, on its own thread, into IOSurface-backed textures, and the
/// IOSurfaces are handed to Avalonia's Metal compositor through its GPU interop. A frame is
/// finished (glFinish) before it is offered, the compositor copies it on a timeline wait/signal
/// of two MTLSharedEvents, and a buffer is only drawn into again once its frame was released -
/// three buffers, so mpv rarely waits.
/// </para>
/// <para>
/// Only works when Avalonia picked its Metal renderer, which Program.cs puts first when this
/// player is selected (so switching to it takes a restart).
/// </para>
/// </summary>
[SupportedOSPlatform("macos")]
public class LibMpvDynamicIoSurfaceControl : Control
{
    private const int BufferCount = 3;
    private const int ReleaseWaitTimeoutMs = 100;

    private sealed class Buffer
    {
        public IntPtr Surface;
        public uint Texture;
        public uint Framebuffer;
        public int Width;
        public int Height;
        public ulong LastFrame;
    }

    private LibMpvDynamicPlayer? _mpvPlayer;

    // Render thread
    private Thread? _renderThread;
    private readonly AutoResetEvent _wake = new(false);
    private IntPtr _glContext;
    private readonly Buffer?[] _buffers = new Buffer?[BufferCount];
    private int _nextBuffer;
    private ulong _frame;
    private int _targetWidth;
    private int _targetHeight;

    // Shared with the compositor; created once, released when the render thread ends.
    private IntPtr _device;
    private IntPtr _readyEvent;
    private IntPtr _releasedEvent;

    // UI thread
    private ICompositionGpuInterop? _interop;
    private CompositionDrawingSurface? _surface;
    private CompositionSurfaceVisual? _visual;
    private ICompositionImportedGpuSemaphore? _readyImported;
    private ICompositionImportedGpuSemaphore? _releasedImported;
    private readonly Dictionary<Buffer, ICompositionImportedGpuImage> _imported = new();
    private volatile bool _presenting;
    private bool _loggedUnsupported;
    private bool _loggedPresentError;

    public LibMpvDynamicIoSurfaceControl(LibMpvDynamicPlayer player)
    {
        _mpvPlayer = player;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    public LibMpvDynamicPlayer? Player => _mpvPlayer;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _ = AttachCompositionAsync();
    }

    private async Task AttachCompositionAsync()
    {
        try
        {
            var compositor = ElementComposition.GetElementVisual(this)?.Compositor;
            if (compositor == null)
            {
                return;
            }

            var interop = await compositor.TryGetCompositionGpuInterop();
            if (interop == null ||
                !interop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.IOSurfaceRef) ||
                !interop.SupportedSemaphoreTypes.Contains(KnownPlatformGraphicsExternalSemaphoreHandleTypes.MetalSharedEvent))
            {
                if (!_loggedUnsupported)
                {
                    _loggedUnsupported = true;
                    Se.LogError(new InvalidOperationException(
                        "Avalonia is not rendering with Metal (IOSurface import unavailable: " +
                        string.Join(",", interop?.SupportedImageHandleTypes ?? []) +
                        ") - restart Subtitle Edit after choosing the mpv Metal player"),
                        "LibMpvDynamicIoSurfaceControl");
                }

                return;
            }

            if (VisualRoot == null)
            {
                return; // detached while awaiting
            }

            if (_device == IntPtr.Zero)
            {
                _device = MacIoSurfaceInterop.CreateDefaultMetalDevice();
                if (interop.DeviceLuid is { } luid && !luid.SequenceEqual(MacIoSurfaceInterop.GetDeviceLuid(_device)))
                {
                    Se.LogError(new InvalidOperationException("Default MTLDevice differs from the compositor's device"), "LibMpvDynamicIoSurfaceControl");
                }

                _readyEvent = MacIoSurfaceInterop.NewSharedEvent(_device);
                _releasedEvent = MacIoSurfaceInterop.NewSharedEvent(_device);
            }

            _interop = interop;
            _readyImported = interop.ImportSemaphore(new PlatformHandle(_readyEvent, KnownPlatformGraphicsExternalSemaphoreHandleTypes.MetalSharedEvent));
            _releasedImported = interop.ImportSemaphore(new PlatformHandle(_releasedEvent, KnownPlatformGraphicsExternalSemaphoreHandleTypes.MetalSharedEvent));
            await Task.WhenAll(_readyImported.ImportCompleted, _releasedImported.ImportCompleted);

            _surface = compositor.CreateDrawingSurface();
            _visual = compositor.CreateSurfaceVisual();
            _visual.Surface = _surface;
            _visual.Size = new Vector(Bounds.Width, Bounds.Height);
            ElementComposition.SetElementChildVisual(this, _visual);

            UpdateTargetSize();
            _presenting = true;
            StartRenderThread();
            _wake.Set();
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "LibMpvDynamicIoSurfaceControl attach");
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        // A detach is not a dispose (layout 12 reparents the live control), so the render
        // thread and mpv stay; only what belongs to this compositor goes.
        _presenting = false;
        ElementComposition.SetElementChildVisual(this, null);
        _surface?.Dispose();
        _surface = null;
        _visual = null;
        foreach (var image in _imported.Values)
        {
            _ = image.DisposeAsync();
        }

        _imported.Clear();
        _ = _readyImported?.DisposeAsync();
        _ = _releasedImported?.DisposeAsync();
        _readyImported = null;
        _releasedImported = null;
        _interop = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty)
        {
            if (_visual != null)
            {
                _visual.Size = new Vector(Bounds.Width, Bounds.Height);
            }

            UpdateTargetSize();
            _wake.Set();
        }
    }

    private void UpdateTargetSize()
    {
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        Volatile.Write(ref _targetWidth, Math.Max(0, (int)(Bounds.Width * scaling)));
        Volatile.Write(ref _targetHeight, Math.Max(0, (int)(Bounds.Height * scaling)));
    }

    private void StartRenderThread()
    {
        if (_renderThread != null || _mpvPlayer == null)
        {
            return;
        }

        _renderThread = new Thread(RenderLoop) { IsBackground = true, Name = "mpv IOSurface render" };
        _renderThread.Start();
    }

    private void RenderLoop()
    {
        var player = _mpvPlayer!;
        try
        {
            _glContext = MacIoSurfaceInterop.CreateOffscreenContext();
            MacIoSurfaceInterop.CGLSetCurrentContext(_glContext);

            player.InitializeWithOpenGL((_, name) => MacIoSurfaceInterop.GetGlProcAddress(name));
            player.PlayerSubName = "Metal";
            player.RequestRender += OnMpvRequestRender;

            while (true)
            {
                _wake.WaitOne(250);

                // The owner disposed the player: free mpv's render context here, where its GL
                // context is current, and end the thread.
                if (player.IsRenderContextFreePending)
                {
                    player.FreeRenderContextIfDisposePending();
                    break;
                }

                if (_presenting)
                {
                    RenderFrame(player);
                }
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "LibMpvDynamicIoSurfaceControl render thread");
        }
        finally
        {
            player.RequestRender -= OnMpvRequestRender;
            for (var i = 0; i < _buffers.Length; i++)
            {
                DestroyBuffer(_buffers[i]);
                _buffers[i] = null;
            }

            MacIoSurfaceInterop.CGLSetCurrentContext(IntPtr.Zero);
            if (_glContext != IntPtr.Zero)
            {
                MacIoSurfaceInterop.CGLDestroyContext(_glContext);
                _glContext = IntPtr.Zero;
            }

            // Imports hold their own references, so these can go even if the compositor still has them.
            MacIoSurfaceInterop.ReleaseObject(_readyEvent);
            MacIoSurfaceInterop.ReleaseObject(_releasedEvent);
            MacIoSurfaceInterop.ReleaseObject(_device);
            _readyEvent = _releasedEvent = _device = IntPtr.Zero;
        }
    }

    private void OnMpvRequestRender() => _wake.Set();

    private void RenderFrame(LibMpvDynamicPlayer player)
    {
        var width = Volatile.Read(ref _targetWidth);
        var height = Volatile.Read(ref _targetHeight);
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var index = _nextBuffer;
        var buffer = _buffers[index];

        // Not drawn into until the compositor has copied the frame it last carried. The timeout
        // keeps a compositor that dropped a frame (detach, hidden window) from stalling mpv.
        if (buffer != null && buffer.LastFrame > 0)
        {
            var waited = 0;
            while (MacIoSurfaceInterop.GetSignaledValue(_releasedEvent) < buffer.LastFrame && waited < ReleaseWaitTimeoutMs)
            {
                Thread.Sleep(1);
                waited++;
            }
        }

        if (buffer == null || buffer.Width != width || buffer.Height != height)
        {
            DestroyBuffer(buffer);
            buffer = CreateBuffer(width, height);
            _buffers[index] = buffer;
        }

        if (buffer == null)
        {
            return;
        }

        MacIoSurfaceInterop.glBindFramebuffer(MacIoSurfaceInterop.GL_FRAMEBUFFER, buffer.Framebuffer);
        MacIoSurfaceInterop.glViewport(0, 0, width, height);
        MacIoSurfaceInterop.glClearColor(0, 0, 0, 1);
        MacIoSurfaceInterop.glClear(MacIoSurfaceInterop.GL_COLOR_BUFFER_BIT);

        if (!string.IsNullOrEmpty(player.FileName))
        {
            // No flip: mpv's unflipped GL output already puts the top video row in IOSurface
            // row 0, which Metal shows at the top (flipY: true came out upside down).
            player.RenderToFramebuffer((int)buffer.Framebuffer, width, height, flipY: false);
        }

        // The compositor only waits on the shared event, which cannot follow GL work - so the
        // frame is complete before the event says so.
        MacIoSurfaceInterop.glFinish();
        MacIoSurfaceInterop.glBindFramebuffer(MacIoSurfaceInterop.GL_FRAMEBUFFER, 0);

        var frame = ++_frame;
        buffer.LastFrame = frame;
        MacIoSurfaceInterop.SetSignaledValue(_readyEvent, frame);
        _nextBuffer = (index + 1) % BufferCount;

        Dispatcher.UIThread.Post(() => _ = PresentAsync(buffer, frame), DispatcherPriority.Render);
    }

    private async Task PresentAsync(Buffer buffer, ulong frame)
    {
        var interop = _interop;
        var surface = _surface;
        if (!_presenting || interop == null || surface == null || _readyImported == null || _releasedImported == null)
        {
            return;
        }

        // A frame from a buffer a resize already replaced: its import may be on its way out.
        if (!_buffers.Contains(buffer))
        {
            return;
        }

        try
        {
            if (!_imported.TryGetValue(buffer, out var image))
            {
                // A resized buffer is a new object, so imports of replaced ones are dropped here.
                foreach (var stale in _imported.Keys.Where(b => !_buffers.Contains(b)).ToList())
                {
                    _ = _imported[stale].DisposeAsync();
                    _imported.Remove(stale);
                }

                image = interop.ImportImage(
                    new PlatformHandle(buffer.Surface, KnownPlatformGraphicsExternalImageHandleTypes.IOSurfaceRef),
                    new PlatformGraphicsExternalImageProperties
                    {
                        Width = buffer.Width,
                        Height = buffer.Height,
                        Format = PlatformGraphicsExternalImageFormat.B8G8R8A8UNorm,
                        TopLeftOrigin = true,
                    });
                _imported[buffer] = image;
                await image.ImportCompleted;
            }

            // Detached, or a newer frame dropped this import (resize) while it was awaited -
            // updating with a disposed image throws PlatformGraphicsContextLostException.
            if (_surface != surface || !_imported.TryGetValue(buffer, out var current) || current != image)
            {
                return;
            }

            await surface.UpdateWithTimelineSemaphoresAsync(image, _readyImported, frame, _releasedImported, frame);
        }
        catch (Exception exception)
        {
            // Retry the import next frame, but log only once - this runs at the video frame rate.
            if (_imported.Remove(buffer, out var failed))
            {
                _ = failed.DisposeAsync();
            }

            if (!_loggedPresentError)
            {
                _loggedPresentError = true;
                Se.LogError(exception, "LibMpvDynamicIoSurfaceControl present");
            }
        }
    }

    private static Buffer? CreateBuffer(int width, int height)
    {
        var surface = MacIoSurfaceInterop.CreateIoSurface(width, height);

        MacIoSurfaceInterop.glGenTextures(1, out var texture);
        MacIoSurfaceInterop.glBindTexture(MacIoSurfaceInterop.GL_TEXTURE_RECTANGLE, texture);
        var error = MacIoSurfaceInterop.CGLTexImageIOSurface2D(
            MacIoSurfaceInterop.CurrentContext(), MacIoSurfaceInterop.GL_TEXTURE_RECTANGLE, MacIoSurfaceInterop.GL_RGBA,
            width, height, MacIoSurfaceInterop.GL_BGRA, MacIoSurfaceInterop.GL_UNSIGNED_INT_8_8_8_8_REV, surface, 0);
        MacIoSurfaceInterop.glBindTexture(MacIoSurfaceInterop.GL_TEXTURE_RECTANGLE, 0);

        MacIoSurfaceInterop.glGenFramebuffers(1, out var framebuffer);
        MacIoSurfaceInterop.glBindFramebuffer(MacIoSurfaceInterop.GL_FRAMEBUFFER, framebuffer);
        MacIoSurfaceInterop.glFramebufferTexture2D(MacIoSurfaceInterop.GL_FRAMEBUFFER, MacIoSurfaceInterop.GL_COLOR_ATTACHMENT0, MacIoSurfaceInterop.GL_TEXTURE_RECTANGLE, texture, 0);
        var status = MacIoSurfaceInterop.glCheckFramebufferStatus(MacIoSurfaceInterop.GL_FRAMEBUFFER);
        MacIoSurfaceInterop.glBindFramebuffer(MacIoSurfaceInterop.GL_FRAMEBUFFER, 0);

        var buffer = new Buffer { Surface = surface, Texture = texture, Framebuffer = framebuffer, Width = width, Height = height };
        if (error != 0 || status != MacIoSurfaceInterop.GL_FRAMEBUFFER_COMPLETE)
        {
            Se.LogError(new InvalidOperationException($"IOSurface framebuffer {width}x{height} failed: CGL {error}, status 0x{status:X}"), "LibMpvDynamicIoSurfaceControl");
            DestroyBuffer(buffer);
            return null;
        }

        return buffer;
    }

    private static void DestroyBuffer(Buffer? buffer)
    {
        if (buffer == null)
        {
            return;
        }

        if (buffer.Framebuffer != 0)
        {
            MacIoSurfaceInterop.glDeleteFramebuffers(1, ref buffer.Framebuffer);
        }

        if (buffer.Texture != 0)
        {
            MacIoSurfaceInterop.glDeleteTextures(1, ref buffer.Texture);
        }

        // An import still in use holds its own reference to the IOSurface.
        if (buffer.Surface != IntPtr.Zero)
        {
            MacIoSurfaceInterop.CFRelease(buffer.Surface);
            buffer.Surface = IntPtr.Zero;
        }
    }

    public async void LoadFile(string path)
    {
        if (_mpvPlayer == null)
        {
            return;
        }

        try
        {
            await _mpvPlayer.LoadFile(path);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"mpv failed to load video file: {path}");
        }
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
