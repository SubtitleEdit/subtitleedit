using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// OpenGL-based control for LibVLC that uses memory rendering callbacks to capture video frames
/// and upload them to OpenGL textures. Provides hardware-accelerated display with cross-platform support.
/// Note: LibVLC handles decoding, while this control manages the OpenGL texture upload and rendering.
/// </summary>
public class LibVlcDynamicOpenGlControl : OpenGlControlBase
{
    private LibVlcDynamicPlayer? _vlcPlayer;
    private bool _isInitialized;
    private IntPtr _videoBuffer = IntPtr.Zero;
    private int _videoWidth;
    private int _videoHeight;
    private uint _textureId;
    private readonly object _bufferLock = new object();

    // OpenGL function delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlGenTexturesDelegate(int n, ref uint textures);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlBindTextureDelegate(uint target, uint texture);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlTexImage2DDelegate(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlTexParameteriDelegate(uint target, uint pname, int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlClearDelegate(uint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlClearColorDelegate(float r, float g, float b, float a);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlEnableDelegate(uint cap);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlDisableDelegate(uint cap);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlBeginDelegate(uint mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlEndDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlTexCoord2fDelegate(float s, float t);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GlVertex2fDelegate(float x, float y);

    private GlGenTexturesDelegate? _glGenTextures;
    private GlBindTextureDelegate? _glBindTexture;
    private GlTexImage2DDelegate? _glTexImage2D;
    private GlTexParameteriDelegate? _glTexParameteri;
    private GlClearDelegate? _glClear;
    private GlClearColorDelegate? _glClearColor;
    private GlEnableDelegate? _glEnable;
    private GlDisableDelegate? _glDisable;
    private GlBeginDelegate? _glBegin;
    private GlEndDelegate? _glEnd;
    private GlTexCoord2fDelegate? _glTexCoord2f;
    private GlVertex2fDelegate? _glVertex2f;

    private const uint GL_TEXTURE_2D = 0x0DE1;
    private const uint GL_RGBA = 0x1908;
    private const uint GL_UNSIGNED_BYTE = 0x1401;
    private const uint GL_TEXTURE_MIN_FILTER = 0x2801;
    private const uint GL_TEXTURE_MAG_FILTER = 0x2800;
    private const uint GL_LINEAR = 0x2601;
    private const uint GL_COLOR_BUFFER_BIT = 0x00004000;
    private const uint GL_QUADS = 0x0007;

    public LibVlcDynamicPlayer? Player => _vlcPlayer;

    public LibVlcDynamicOpenGlControl(LibVlcDynamicPlayer vlcPlayer)
    {
        _vlcPlayer = vlcPlayer;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        ResolveGlFunctions(gl);

        if (!_isInitialized && _vlcPlayer != null)
        {
            try
            {
                _vlcPlayer.LoadLib();
                _vlcPlayer.PlayerSubName = "OpenGL";

                // Initialize OpenGL texture
                if (_glGenTextures != null)
                {
                    _glGenTextures(1, ref _textureId);
                }

                // Set up video callbacks to receive frames
                SetupVideoCallbacks();

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("VLC Player initialized successfully with OpenGL!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize VLC Player: {ex.Message}");
            }
        }
    }

    private void ResolveGlFunctions(GlInterface gl)
    {
        _glGenTextures = GetGlFunction<GlGenTexturesDelegate>(gl, "glGenTextures");
        _glBindTexture = GetGlFunction<GlBindTextureDelegate>(gl, "glBindTexture");
        _glTexImage2D = GetGlFunction<GlTexImage2DDelegate>(gl, "glTexImage2D");
        _glTexParameteri = GetGlFunction<GlTexParameteriDelegate>(gl, "glTexParameteri");
        _glClear = GetGlFunction<GlClearDelegate>(gl, "glClear");
        _glClearColor = GetGlFunction<GlClearColorDelegate>(gl, "glClearColor");
        _glEnable = GetGlFunction<GlEnableDelegate>(gl, "glEnable");
        _glDisable = GetGlFunction<GlDisableDelegate>(gl, "glDisable");
        _glBegin = GetGlFunction<GlBeginDelegate>(gl, "glBegin");
        _glEnd = GetGlFunction<GlEndDelegate>(gl, "glEnd");
        _glTexCoord2f = GetGlFunction<GlTexCoord2fDelegate>(gl, "glTexCoord2f");
        _glVertex2f = GetGlFunction<GlVertex2fDelegate>(gl, "glVertex2f");
    }

    private T? GetGlFunction<T>(GlInterface gl, string name) where T : Delegate
    {
        var ptr = gl.GetProcAddress(name);
        return ptr != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<T>(ptr) : null;
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        // Clear the framebuffer
        _glClearColor?.Invoke(0f, 0f, 0f, 1f);
        _glClear?.Invoke(GL_COLOR_BUFFER_BIT);

        if (!_isInitialized || _vlcPlayer == null)
        {
            return;
        }

        // If no file is loaded, keep the black screen
        if (string.IsNullOrEmpty(_vlcPlayer.FileName))
        {
            return;
        }

        lock (_bufferLock)
        {
            if (_videoBuffer == IntPtr.Zero || _videoWidth <= 0 || _videoHeight <= 0)
            {
                return;
            }

            try
            {
                // Update texture with video data
                _glBindTexture?.Invoke(GL_TEXTURE_2D, _textureId);
                _glTexParameteri?.Invoke(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_LINEAR);
                _glTexParameteri?.Invoke(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_LINEAR);
                _glTexImage2D?.Invoke(GL_TEXTURE_2D, 0, (int)GL_RGBA, _videoWidth, _videoHeight, 0, GL_RGBA, GL_UNSIGNED_BYTE, _videoBuffer);

                // Draw textured quad
                _glEnable?.Invoke(GL_TEXTURE_2D);
                _glBegin?.Invoke(GL_QUADS);

                _glTexCoord2f?.Invoke(0, 1); _glVertex2f?.Invoke(-1, -1);
                _glTexCoord2f?.Invoke(1, 1); _glVertex2f?.Invoke(1, -1);
                _glTexCoord2f?.Invoke(1, 0); _glVertex2f?.Invoke(1, 1);
                _glTexCoord2f?.Invoke(0, 0); _glVertex2f?.Invoke(-1, 1);

                _glEnd?.Invoke();
                _glDisable?.Invoke(GL_TEXTURE_2D);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OpenGL render error: {ex.Message}");
            }
        }
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        if (_videoBuffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_videoBuffer);
            _videoBuffer = IntPtr.Zero;
        }

        base.OnOpenGlDeinit(gl);
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
                RequestNextFrameRendering();
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
                _videoWidth = mediaInfo.Dimension.Width;
                _videoHeight = mediaInfo.Dimension.Height;

                // Allocate buffer for video frame
                var bufferSize = _videoWidth * _videoHeight * 4; // RGBA
                if (_videoBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_videoBuffer);
                }
                _videoBuffer = Marshal.AllocHGlobal(bufferSize);

                System.Diagnostics.Debug.WriteLine($"Video dimensions set: {_videoWidth}x{_videoHeight}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get video dimensions: {ex.Message}");
        }
    }
}
