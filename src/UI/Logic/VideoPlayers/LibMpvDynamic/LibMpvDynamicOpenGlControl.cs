using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

public class LibMpvDynamicOpenGlControl : OpenGlControlBase
{
    public LibMpvDynamicPlayer? _mpvPlayer;
    private bool _isInitialized;

    // OpenGL delegates for clearing framebuffer
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlClearDelegate(uint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlClearColorDelegate(float r, float g, float b, float a);

    private GlClearDelegate? _glClear;
    private GlClearColorDelegate? _glClearColor;
    private const uint GL_COLOR_BUFFER_BIT = 0x00004000;

    public LibMpvDynamicOpenGlControl(LibMpvDynamicPlayer videoPlayerInstanceMpv)
    {
        _mpvPlayer = videoPlayerInstanceMpv;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    public LibMpvDynamicPlayer? Player => _mpvPlayer;

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        // Resolve OpenGL functions
        ResolveGlFunctions(gl);

        if (!_isInitialized)
        {
            // Set up the GetProcAddress delegate for OpenGL
            _mpvPlayer?.InitializeWithOpenGL((ctx, name) =>
            {
                try
                {
                    return gl.GetProcAddress(name);
                }
                catch
                {
                    return IntPtr.Zero;
                }
            });

            // Subscribe to render requests
            if (_mpvPlayer != null)
            {
                _mpvPlayer.RequestRender += OnMpvRequestRender;
                _mpvPlayer.PlayerSubName = "OpenGL";
            }

            _isInitialized = true;
        }
    }

    private void ResolveGlFunctions(GlInterface gl)
    {
        var clearPtr = gl.GetProcAddress("glClear");
        var clearColorPtr = gl.GetProcAddress("glClearColor");

        if (clearPtr != IntPtr.Zero)
        {
            _glClear = Marshal.GetDelegateForFunctionPointer<GlClearDelegate>(clearPtr);
        }

        if (clearColorPtr != IntPtr.Zero)
        {
            _glClearColor = Marshal.GetDelegateForFunctionPointer<GlClearColorDelegate>(clearColorPtr);
        }
    }

    private void OnMpvRequestRender()
    {
        // Request a redraw on the UI thread
        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Render);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        // Always clear the framebuffer first to remove stale video frames
        _glClearColor?.Invoke(0f, 0f, 0f, 0f);
        _glClear?.Invoke(GL_COLOR_BUFFER_BIT);

        if (!_isInitialized || _mpvPlayer == null)
        {
            System.Diagnostics.Debug.WriteLine("Render: Not initialized or no player");
            return;
        }

        // If no file is loaded, keep the black screen and don't render
        if (string.IsNullOrEmpty(_mpvPlayer.FileName))
        {
            System.Diagnostics.Debug.WriteLine("Render: No file loaded - showing black");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"Render: Rendering file {_mpvPlayer.FileName}");


        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var size = Bounds.Size * scaling;
        var width = (int)size.Width;
        var height = (int)size.Height;

        if (width <= 0 || height <= 0)
        {
            return;
        }

        try
        {
            // Render mpv content to the framebuffer
            _mpvPlayer.RenderToFramebuffer(fb, width, height, flipY: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
        }
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        //if (_mpvPlayer != null)
        //{
        //    _mpvPlayer.RequestRender -= OnMpvRequestRender;
        //    _mpvPlayer.Dispose();
        //    _mpvPlayer = null;
        //}

        base.OnOpenGlDeinit(gl);
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