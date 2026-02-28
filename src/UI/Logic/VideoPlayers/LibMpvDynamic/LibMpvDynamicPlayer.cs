using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

public sealed class LibMpvDynamicPlayer : IDisposable, IVideoPlayerInstance
{
    /// <summary>
    /// Set this path (directory only) to override the default search paths.
    /// </summary>
    public static string MpvPath = string.Empty;

    public string PlayerSubName { get; set; } = string.Empty;
    public static int MaxVolume { get; set; } = 130;

    private IntPtr _library = IntPtr.Zero;
    private IntPtr _mpv = IntPtr.Zero;
    private IntPtr _renderContext = IntPtr.Zero;
    private bool _disposed;
    private string _fileName = string.Empty;

    [StructLayout(LayoutKind.Sequential)]
    private struct MpvOpenGlInitParams
    {
        public IntPtr get_proc_address;
        public IntPtr get_proc_address_ctx;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MpvRenderParam
    {
        public int type;
        public IntPtr data;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MpvOpenGLFBO
    {
        public int fbo;
        public int w;
        public int h;
        public int internal_format;
    }

    // Basic mpv functions
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr MpvCreate();

    private MpvCreate? _mpvCreate;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvInitialize(IntPtr mpvHandle);

    private MpvInitialize? _mpvInitialize;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvCommand(IntPtr mpvHandle, IntPtr utf8Strings);

    private MpvCommand? _mpvCommand;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr MpvWaitEvent(IntPtr mpvHandle, double wait);

    private MpvWaitEvent? _mpvWaitEvent;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvSetOption(IntPtr mpvHandle, byte[] name, int format, ref ulong data);

    private MpvSetOption? _mpvSetOption;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvSetOptionString(IntPtr mpvHandle, byte[] name, byte[] value);

    private MpvSetOptionString? _mpvSetOptionString;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvGetPropertyString(IntPtr mpvHandle, byte[] name, int format, ref IntPtr data);

    private MpvGetPropertyString? _mpvGetPropertyString;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvGetPropertyDouble(IntPtr mpvHandle, byte[] name, int format, ref double data);

    private MpvGetPropertyDouble? _mpvGetPropertyDouble;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvSetProperty(IntPtr mpvHandle, byte[] name, int format, ref byte[] data);

    private MpvSetProperty? _mpvSetProperty;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MpvFree(IntPtr data);

    private MpvFree? _mpvFree;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ulong MpvClientApiVersion();

    private MpvClientApiVersion? _mpvClientApiVersion;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr MpvErrorString(int error);

    private MpvErrorString? _mpvErrorString;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr MpvTerminateDestroy(IntPtr mpvHandle);

    private MpvTerminateDestroy? _mpvTerminateDestroy;

    // Render API functions
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvRenderContextCreate(out IntPtr res, IntPtr mpvHandle, IntPtr parameters);

    private MpvRenderContextCreate? _mpvRenderContextCreate;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvRenderContextRender(IntPtr ctx, IntPtr parameters);

    private MpvRenderContextRender? _mpvRenderContextRender;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MpvRenderContextFree(IntPtr ctx);

    private MpvRenderContextFree? _mpvRenderContextFree;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MpvRenderContextSetUpdateCallback(IntPtr ctx, IntPtr callback, IntPtr callbackCtx);

    private MpvRenderContextSetUpdateCallback? _mpvRenderContextSetUpdateCallback;

    // OpenGL proc address callback - public delegate for external use
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr GetProcAddress(IntPtr ctx, string name);

    // Internal mpv callback wrapper
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr MpvGetProcAddressFunc(IntPtr ctx, string name);

    // Render callback
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MpvRenderUpdateFunc(IntPtr ctx);

    private GetProcAddress? _getProcAddress;
    private MpvRenderUpdateFunc? _renderUpdateCallback;

    // Render API constants
    private const int MPV_RENDER_PARAM_INVALID = 0;
    private const int MPV_RENDER_PARAM_API_TYPE = 1;
    private const int MPV_RENDER_PARAM_OPENGL_INIT_PARAMS = 2;
    private const int MPV_RENDER_PARAM_OPENGL_FBO = 3;
    private const int MPV_RENDER_PARAM_FLIP_Y = 4;
    private const int MPV_RENDER_PARAM_DEPTH = 5;
    private const int MPV_RENDER_PARAM_SW_SIZE = 17;
    private const int MPV_RENDER_PARAM_SW_FORMAT = 18;
    private const int MPV_RENDER_PARAM_SW_STRIDE = 19;
    private const int MPV_RENDER_PARAM_SW_POINTER = 20;

    private const string MPV_RENDER_API_TYPE_OPENGL = "opengl";
    private const string MPV_RENDER_API_TYPE_SW = "sw";

    private const int MPV_FORMAT_STRING = 1;
    private const int MPV_FORMAT_FLAG = 3;
    private const int MPV_FORMAT_INT64 = 4;
    private const int MPV_FORMAT_DOUBLE = 5;

    public event Action? RequestRender;

    public LibMpvDynamicPlayer()
    {

    }

    private static string[] GetLibraryNames()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ["libmpv-2.dll"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return ["libmpv.so.2", "libmpv.so"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return ["libmpv.dylib", "libmpv.2.dylib"];
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported OS platform.");
        }
    }

    private static string[] GetLibraryPaths()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return
            [
                MpvPath,
                Directory.GetCurrentDirectory(),
                string.Empty,
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return
            [
                MpvPath,
                Directory.GetCurrentDirectory(),
                "/usr/local/lib",
                "/usr/lib",
                "/lib",
                "/usr/lib64",
                "/lib64",
                "/usr/lib/x86_64-linux-gnu",
                "/lib/x86_64-linux-gnu",
                "/usr/lib/aarch64-linux-gnu",
                "/lib/aarch64-linux-gnu",
                "/usr/lib/arm-linux-gnueabihf",
                "/lib/arm-linux-gnueabihf",
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return
            [
                MpvPath,
                Directory.GetCurrentDirectory(),
                "/Applications/Subtitle Edit.app/Contents/Frameworks",
                "/opt/local/lib",
                "/usr/local/lib",
                "/opt/homebrew/lib",
                "/opt/lib",
            ];
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported OS platform.");
        }
    }

    private void LoadLibMpvMethods()
    {
        _mpvCreate = (MpvCreate)GetDllType(typeof(MpvCreate), "mpv_create");
        _mpvInitialize = (MpvInitialize)GetDllType(typeof(MpvInitialize), "mpv_initialize");
        _mpvWaitEvent = (MpvWaitEvent)GetDllType(typeof(MpvWaitEvent), "mpv_wait_event");
        _mpvCommand = (MpvCommand)GetDllType(typeof(MpvCommand), "mpv_command");
        _mpvSetOption = (MpvSetOption)GetDllType(typeof(MpvSetOption), "mpv_set_option");
        _mpvSetOptionString = (MpvSetOptionString)GetDllType(typeof(MpvSetOptionString), "mpv_set_option_string");
        _mpvGetPropertyString = (MpvGetPropertyString)GetDllType(typeof(MpvGetPropertyString), "mpv_get_property");
        _mpvGetPropertyDouble = (MpvGetPropertyDouble)GetDllType(typeof(MpvGetPropertyDouble), "mpv_get_property");
        _mpvSetProperty = (MpvSetProperty)GetDllType(typeof(MpvSetProperty), "mpv_set_property");
        _mpvFree = (MpvFree)GetDllType(typeof(MpvFree), "mpv_free");
        _mpvClientApiVersion = (MpvClientApiVersion)GetDllType(typeof(MpvClientApiVersion), "mpv_client_api_version");
        _mpvErrorString = (MpvErrorString)GetDllType(typeof(MpvErrorString), "mpv_error_string");
        _mpvTerminateDestroy = (MpvTerminateDestroy)GetDllType(typeof(MpvTerminateDestroy), "mpv_terminate_destroy");

        // Load render API functions
        _mpvRenderContextCreate = (MpvRenderContextCreate)GetDllType(typeof(MpvRenderContextCreate), "mpv_render_context_create");
        _mpvRenderContextRender = (MpvRenderContextRender)GetDllType(typeof(MpvRenderContextRender), "mpv_render_context_render");
        _mpvRenderContextFree = (MpvRenderContextFree)GetDllType(typeof(MpvRenderContextFree), "mpv_render_context_free");
        _mpvRenderContextSetUpdateCallback = (MpvRenderContextSetUpdateCallback)GetDllType(typeof(MpvRenderContextSetUpdateCallback), "mpv_render_context_set_update_callback");
    }

    private object GetDllType(Type type, string name)
    {
        var address = NativeMethods.CrossGetProcAddress(_library, name);
        return address != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer(address, type) : IntPtr.Zero;
    }

    private bool LoadLibraryInternal()
    {
        foreach (var libName in GetLibraryNames())
        {
            foreach (var libPath in GetLibraryPaths())
            {
                var fullPath = Path.Combine(libPath, libName);
                if (File.Exists(fullPath))
                {
                    var libHandle = NativeMethods.CrossLoadLibrary(fullPath);
                    if (libHandle != IntPtr.Zero)
                    {
                        _library = libHandle;
                        LoadLibMpvMethods();
                        _mpv = _mpvCreate!.Invoke();
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void LoadLib()
    {
        if (_library == IntPtr.Zero)
        {
            LoadLibraryInternal();
        }
    }

    public bool CanLoad()
    {
        if (_library != IntPtr.Zero)
        {
            return true;
        }

        return LoadLibraryInternal();
    }

    public int Initialize()
    {
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero || _mpvInitialize == null)
        {
            return -1;
        }

        return _mpvInitialize(_mpv);
    }

    private static byte[] GetUtf8Bytes(string s)
    {
        return Encoding.UTF8.GetBytes(s + "\0");
    }

    public string GetErrorString(int error)
    {
        if (_mpvErrorString == null)
        {
            return $"mpv error {error}";
        }

        var ptr = _mpvErrorString(error);
        return ptr == IntPtr.Zero ? $"mpv error {error}" : Marshal.PtrToStringUTF8(ptr) ?? $"mpv error {error}";
    }

    public int SetOptionString(string name, string value)
    {
        if (_mpvSetOptionString == null)
        {
            return -1;
        }

        var nameBytes = GetUtf8Bytes(name);
        var valueBytes = GetUtf8Bytes(value);
        return _mpvSetOptionString(_mpv, nameBytes, valueBytes);
    }

    public static IntPtr AllocateUtf8IntPtrArrayWithSentinel(string[] arr, out IntPtr[] byteArrayPointers)
    {
        var numberOfStrings = arr.Length + 1;
        byteArrayPointers = new IntPtr[numberOfStrings];
        var rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
        for (var index = 0; index < arr.Length; index++)
        {
            var bytes = GetUtf8Bytes(arr[index]);
            var unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            byteArrayPointers[index] = unmanagedPointer;
        }

        Marshal.Copy(byteArrayPointers, 0, rootPointer, numberOfStrings);
        return rootPointer;
    }

    private int DoMpvCommand(params string[] args)
    {
        if (_mpv == IntPtr.Zero || _mpvCommand == null)
        {
            return 0;
        }

        var mainPtr = AllocateUtf8IntPtrArrayWithSentinel(args, out var byteArrayPointers);
        var result = _mpvCommand(_mpv, mainPtr);
        foreach (var ptr in byteArrayPointers)
        {
            Marshal.FreeHGlobal(ptr);
        }

        Marshal.FreeHGlobal(mainPtr);
        return result;
    }

    private void OnRenderUpdate(IntPtr ctx)
    {
        // Request a redraw from the UI thread
        RequestRender?.Invoke();
    }

    public void InitializeWithOpenGL(GetProcAddress getProcAddress)
    {
        LoadLibraryInternal();
        EnsureNotDisposed();

        if (_mpvInitialize == null || _mpvRenderContextCreate == null || _mpvRenderContextSetUpdateCallback == null)
        {
            Se.LogError(new InvalidOperationException("MPV delegates not loaded"), "LibMpvDynamicPlayer InitializeWithOpenGL");
            return;
        }

        _getProcAddress = getProcAddress;

        // Set mpv to use OpenGL render API for all platforms
        SetOptionString("vo", "libmpv");
        SetOptionString("gpu-api", "opengl");

        // Platform-specific GPU context configuration
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // On Linux, configure gpu-context based on display server
            try
            {
                var sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")?.ToLowerInvariant();
                var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
                var x11Display = Environment.GetEnvironmentVariable("DISPLAY");

                if (sessionType == "wayland" || (!string.IsNullOrEmpty(waylandDisplay) && sessionType == null))
                {
                    SetOptionString("gpu-context", "wayland");
                }
                else if (sessionType == "x11" || (!string.IsNullOrEmpty(x11Display) && sessionType == null))
                {
                    SetOptionString("gpu-context", "x11");
                }
                // else: don't force gpu-context, mpv will autodetect
            }
            catch
            {
                // Ignore detection errors; fallback to mpv defaults
            }
        }

        // Initialize mpv first
        var err = _mpvInitialize(_mpv);
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer InitializeWithOpenGL mpv_initialize");
        }

        // Create OpenGL init params
        var initParams = new MpvOpenGlInitParams
        {
            get_proc_address = Marshal.GetFunctionPointerForDelegate<MpvGetProcAddressFunc>(
                new MpvGetProcAddressFunc((ctx, name) => getProcAddress(ctx, name))
            ),
            get_proc_address_ctx = IntPtr.Zero
        };

        var initParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvOpenGlInitParams>());
        Marshal.StructureToPtr(initParams, initParamsPtr, false);

        try
        {
            // Build render context params
            var apiTypeBytes = Encoding.UTF8.GetBytes(MPV_RENDER_API_TYPE_OPENGL + "\0");
            var apiTypePtr = Marshal.AllocHGlobal(apiTypeBytes.Length);
            Marshal.Copy(apiTypeBytes, 0, apiTypePtr, apiTypeBytes.Length);

            var renderParams = new[]
            {
                new MpvRenderParam { type = MPV_RENDER_PARAM_API_TYPE, data = apiTypePtr },
                new MpvRenderParam { type = MPV_RENDER_PARAM_OPENGL_INIT_PARAMS, data = initParamsPtr },
                new MpvRenderParam { type = MPV_RENDER_PARAM_INVALID, data = IntPtr.Zero }
            };

            var renderParamsSize = Marshal.SizeOf<MpvRenderParam>() * renderParams.Length;
            var renderParamsPtr = Marshal.AllocHGlobal(renderParamsSize);

            for (var i = 0; i < renderParams.Length; i++)
            {
                var offset = renderParamsPtr + (i * Marshal.SizeOf<MpvRenderParam>());
                Marshal.StructureToPtr(renderParams[i], offset, false);
            }

            // Create render context
            err = _mpvRenderContextCreate(out _renderContext, _mpv, renderParamsPtr);
            if (err < 0)
            {
                Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer InitializeWithOpenGL mpv_render_context_create");
            }

            // Set update callback
            _renderUpdateCallback = OnRenderUpdate;
            var callbackPtr = Marshal.GetFunctionPointerForDelegate(_renderUpdateCallback);
            _mpvRenderContextSetUpdateCallback(_renderContext, callbackPtr, IntPtr.Zero);

            // Cleanup
            Marshal.FreeHGlobal(renderParamsPtr);
            Marshal.FreeHGlobal(apiTypePtr);
        }
        finally
        {
            Marshal.FreeHGlobal(initParamsPtr);
        }
    }

    public void RenderToFramebuffer(int fbo, int width, int height, bool flipY = true)
    {
        if (_renderContext == IntPtr.Zero || _mpvRenderContextRender == null)
        {
            return;
        }

        var fboData = new MpvOpenGLFBO
        {
            fbo = fbo,
            w = width,
            h = height,
            internal_format = 0 // 0 = auto-detect
        };

        var fboPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvOpenGLFBO>());
        Marshal.StructureToPtr(fboData, fboPtr, false);

        try
        {
            var flipYValue = flipY ? 1 : 0;
            var flipYPtr = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(flipYPtr, flipYValue);

            try
            {
                var renderParams = new[]
                {
                    new MpvRenderParam { type = MPV_RENDER_PARAM_OPENGL_FBO, data = fboPtr },
                    new MpvRenderParam { type = MPV_RENDER_PARAM_FLIP_Y, data = flipYPtr },
                    new MpvRenderParam { type = MPV_RENDER_PARAM_INVALID, data = IntPtr.Zero }
                };

                var renderParamsSize = Marshal.SizeOf<MpvRenderParam>() * renderParams.Length;
                var renderParamsPtr = Marshal.AllocHGlobal(renderParamsSize);

                try
                {
                    for (var i = 0; i < renderParams.Length; i++)
                    {
                        var offset = renderParamsPtr + (i * Marshal.SizeOf<MpvRenderParam>());
                        Marshal.StructureToPtr(renderParams[i], offset, false);
                    }

                    var err = _mpvRenderContextRender(_renderContext, renderParamsPtr);
                    if (err < 0 && err != -2) // -2 = nothing to render
                    {
                        Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer RenderToFramebuffer mpv_render_context_render");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(renderParamsPtr);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(flipYPtr);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(fboPtr);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_renderContext != IntPtr.Zero && _mpvRenderContextFree != null)
        {
            _mpvRenderContextFree(_renderContext);
            _renderContext = IntPtr.Zero;
        }

        if (_mpv != IntPtr.Zero && _mpvTerminateDestroy != null)
        {
            _mpvTerminateDestroy.Invoke(_mpv);
            _mpv = IntPtr.Zero;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            Se.LogError(new ObjectDisposedException(nameof(LibMpvDynamicPlayer)), "LibMpvDynamicPlayer method called after disposal");
        }
    }

    // public media player properties/methods

    public string Name => $"libmpv {VersionNumber} " + PlayerSubName;

    public string FileName => _fileName;

    public async Task LoadFile(string path)
    {
        EnsureNotDisposed();

        var err = await Task.Run(() => DoMpvCommand("loadfile", path));
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer LoadFile");
        }

        SetOptionString("keep-open", "always");
        SetOptionString("sid", "no");

        SetOptionString("hr-seek", "yes");
        SetOptionString("rebase-start-time", "no");

        _fileName = path;
    }

    public void PlayOrPause() // toggle play/pause
    {
        _pausedValue = null;
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return;
        }

        var err = DoMpvCommand("cycle", "pause");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer PlayOrPause");
        }
    }

    public void CloseFile()
    {
        _fileName = string.Empty;
        _pausedValue = null;

        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return;
        }

        // Stop playback and clear the current file/playlist, returning to idle
        var err = DoMpvCommand("stop");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer CloseFile");
        }

        // Ask UI to repaint so any previously rendered frame can be cleared
        RequestRender?.Invoke();
    }

    public bool IsPlaying
    {
        get
        {
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return false;
            }

            try
            {
                double pauseValue = 0;
                var nameBytes = GetUtf8Bytes("pause");
                var err = _mpvGetPropertyDouble(_mpv, nameBytes, MPV_FORMAT_FLAG, ref pauseValue);

                if (err < 0)
                {
                    return false;
                }

                return pauseValue == 0; // pause=0 means playing
            }
            catch
            {
                return false;
            }
        }
    }

    public bool IsPaused
    {
        get
        {
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return false;
            }

            try
            {
                double pauseValue = 0;
                var nameBytes = GetUtf8Bytes("pause");
                var err = _mpvGetPropertyDouble(_mpv, nameBytes, MPV_FORMAT_FLAG, ref pauseValue);

                if (err < 0)
                {
                    return false;
                }

                return pauseValue != 0;
            }
            catch
            {
                return false;
            }
        }
    }


    private double? _pausedValue;

    public double Position
    {
        get
        {
            if (_pausedValue.HasValue && IsPaused && !Se.Settings.General.UseFrameMode)
            {
                return _pausedValue.Value;
            }

            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return 0;
            }

            try
            {
                double position = 0;
                var nameBytes = GetUtf8Bytes("time-pos");
                var err = _mpvGetPropertyDouble(_mpv, nameBytes, MPV_FORMAT_DOUBLE, ref position);

                if (err < 0)
                {
                    return 0;
                }

                return position;
            }
            catch
            {
                return 0;
            }
        }
        set
        {
            _pausedValue = value;
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero)
            {
                return;
            }

            var err = DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute");
            //if (err < 0)
            //{
            //    Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer Position set");
            //}
        }
    }

    public double Duration
    {
        get
        {
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return 0;
            }

            try
            {
                double duration = 0;
                var nameBytes = GetUtf8Bytes("duration");
                var err = _mpvGetPropertyDouble(_mpv, nameBytes, MPV_FORMAT_DOUBLE, ref duration);

                if (err < 0)
                {
                    return 0;
                }

                return duration;
            }
            catch
            {
                return 0;
            }
        }
    }

    public int VolumeMaximum => 130;

    public double Volume
    {
        get
        {
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return 100;
            }

            try
            {
                double volume = 100;
                var nameBytes = GetUtf8Bytes("volume");
                var err = _mpvGetPropertyDouble(_mpv, nameBytes, MPV_FORMAT_DOUBLE, ref volume);

                if (err < 0)
                {
                    return 100;
                }

                return volume;
            }
            catch
            {
                return 100;
            }
        }
        set
        {
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero)
            {
                return;
            }

            // Clamp volume between 0 and 100
            var clampedVolume = Math.Max(0, Math.Min(100, value));
            var err = DoMpvCommand("set", "volume", clampedVolume.ToString(CultureInfo.InvariantCulture));
            //if (err < 0)
            //{
            //    Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer Volume set");
            //}
        }
    }

    public double Speed
    {
        get
        {
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return 1.0;
            }

            try
            {
                var speed = 1.0;
                var nameBytes = GetUtf8Bytes("speed");
                var err = _mpvGetPropertyDouble(_mpv, nameBytes, MPV_FORMAT_DOUBLE, ref speed);

                if (err < 0)
                {
                    return 1.0;
                }

                return speed;
            }
            catch
            {
                return 1.0;
            }
        }
        set
        {
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero)
            {
                return;
            }

            // Clamp speed to reasonable values (0.25x to 4x)
            var clampedSpeed = Math.Max(0.25, Math.Min(4.0, value));
            var err = DoMpvCommand("set", "speed", clampedSpeed.ToString(CultureInfo.InvariantCulture));
            //if (err < 0)
            //{
            //    Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer Speed set");
            //}
        }
    }

    public void Stop()
    {
        _pausedValue = null;
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return;
        }

        // Pause playback first
        var err = DoMpvCommand("set", "pause", "yes");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer Stop pause");
        }

        // Seek back to position 0
        err = DoMpvCommand("seek", "0", "absolute");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer Stop seek");
        }

        // Request render to show the first frame
        RequestRender?.Invoke();
    }

    public void Play()
    {
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return;
        }

        var err = DoMpvCommand("set", "pause", "no");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer play");
        }
    }

    public void Pause()
    {
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return;
        }

        var err = DoMpvCommand("set", "pause", "yes");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer pause");
        }
    }

    public AudioTrackInfo? ToggleAudioTrack()
    {
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            var audioTracks = GetAudioTracks();

            if (audioTracks.Count == 0)
            {
                return null;
            }

            // Find current track and select next one
            var currentIdx = audioTracks.FindIndex(t => t.IsSelected);
            var nextIdx = currentIdx >= 0 ? (currentIdx + 1) % audioTracks.Count : 0;
            var nextTrack = audioTracks[nextIdx];

            // Switch to the next audio track by ID
            var err = DoMpvCommand("set", "aid", nextTrack.Id.ToString(CultureInfo.InvariantCulture));
            if (err < 0)
            {
                Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer ToggleAudioTrack set aid");
                return null;
            }

            return nextTrack;
        }
        catch
        {
            return null;
        }
    }

    public List<AudioTrackInfo> GetAudioTracks()
    {
        var audioTracks = new List<AudioTrackInfo>();

        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null || _mpvGetPropertyString == null || _mpvFree == null)
        {
            return audioTracks;
        }

        try
        {
            // Get track list count
            double trackCount = 0;
            var trackCountBytes = GetUtf8Bytes("track-list/count");
            var err = _mpvGetPropertyDouble(_mpv, trackCountBytes, MPV_FORMAT_DOUBLE, ref trackCount);

            if (err < 0 || trackCount <= 0)
            {
                return audioTracks;
            }

            // Iterate through tracks to find audio tracks
            for (var i = 0; i < (int)trackCount; i++)
            {
                // Get track type
                var typePtr = IntPtr.Zero;
                var typeBytes = GetUtf8Bytes($"track-list/{i}/type");
                err = _mpvGetPropertyString(_mpv, typeBytes, MPV_FORMAT_STRING, ref typePtr);

                if (err < 0 || typePtr == IntPtr.Zero)
                {
                    continue;
                }

                var type = Marshal.PtrToStringUTF8(typePtr);
                _mpvFree(typePtr);

                if (!string.Equals(type, "audio", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Get track ID
                double trackId = -1;
                var idBytes = GetUtf8Bytes($"track-list/{i}/id");
                err = _mpvGetPropertyDouble(_mpv, idBytes, MPV_FORMAT_DOUBLE, ref trackId);

                if (err < 0 || trackId < 0)
                {
                    continue;
                }

                var trackInfo = new AudioTrackInfo
                {
                    Id = (int)trackId
                };

                // Get track language (optional)
                var langPtr = IntPtr.Zero;
                var langBytes = GetUtf8Bytes($"track-list/{i}/lang");
                err = _mpvGetPropertyString(_mpv, langBytes, MPV_FORMAT_STRING, ref langPtr);

                if (err >= 0 && langPtr != IntPtr.Zero)
                {
                    trackInfo.Language = Marshal.PtrToStringUTF8(langPtr);
                    _mpvFree(langPtr);
                }

                // Get track title (optional)
                var titlePtr = IntPtr.Zero;
                var titleBytes = GetUtf8Bytes($"track-list/{i}/title");
                err = _mpvGetPropertyString(_mpv, titleBytes, MPV_FORMAT_STRING, ref titlePtr);

                if (err >= 0 && titlePtr != IntPtr.Zero)
                {
                    trackInfo.Title = Marshal.PtrToStringUTF8(titlePtr);
                    _mpvFree(titlePtr);
                }

                // Get track ff-index (optional)
                double ffIndex = -1;
                var ffIndexBytes = GetUtf8Bytes($"track-list/{i}/ff-index");
                err = _mpvGetPropertyDouble(_mpv, ffIndexBytes, MPV_FORMAT_DOUBLE, ref ffIndex);

                if (err >= 0 && ffIndex >= 0)
                {
                    trackInfo.FfIndex = (int)ffIndex;
                }

                // Get track selected status
                double selectedValue = 0;
                var selectedBytes = GetUtf8Bytes($"track-list/{i}/selected");
                err = _mpvGetPropertyDouble(_mpv, selectedBytes, MPV_FORMAT_FLAG, ref selectedValue);

                trackInfo.IsSelected = err >= 0 && selectedValue == 1;

                audioTracks.Add(trackInfo);
            }

            return audioTracks;
        }
        catch
        {
            return audioTracks;
        }
    }

    public void SetAudioTrack(int trackId)
    {
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return;
        }

        var err = DoMpvCommand("set", "aid", trackId.ToString(CultureInfo.InvariantCulture));
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer SetAudioTrack");
        }
    }

    public void InitializeWithSoftwareRendering()
    {
        LoadLibraryInternal();
        EnsureNotDisposed();

        // Set mpv to use software rendering
        SetOptionString("vo", "libmpv");

        if (_mpvInitialize == null || _mpvRenderContextCreate == null || _mpvRenderContextSetUpdateCallback == null)
        {
            throw new InvalidOperationException("MPV delegates not loaded for software rendering.");
        }

        // Initialize mpv
        var err = _mpvInitialize(_mpv);
        if (err < 0)
        {
            throw new InvalidOperationException(GetErrorString(err));
        }

        // Build render context params for software rendering
        var apiTypeBytes = Encoding.UTF8.GetBytes(MPV_RENDER_API_TYPE_SW + "\0");
        var apiTypePtr = Marshal.AllocHGlobal(apiTypeBytes.Length);
        Marshal.Copy(apiTypeBytes, 0, apiTypePtr, apiTypeBytes.Length);

        try
        {
            var renderParams = new[]
            {
                new MpvRenderParam { type = MPV_RENDER_PARAM_API_TYPE, data = apiTypePtr },
                new MpvRenderParam { type = MPV_RENDER_PARAM_INVALID, data = IntPtr.Zero }
            };

            var renderParamsSize = Marshal.SizeOf<MpvRenderParam>() * renderParams.Length;
            var renderParamsPtr = Marshal.AllocHGlobal(renderParamsSize);

            try
            {
                for (var i = 0; i < renderParams.Length; i++)
                {
                    var offset = renderParamsPtr + (i * Marshal.SizeOf<MpvRenderParam>());
                    Marshal.StructureToPtr(renderParams[i], offset, false);
                }

                // Create render context
                err = _mpvRenderContextCreate(out _renderContext, _mpv, renderParamsPtr);
                if (err < 0)
                {
                    throw new InvalidOperationException($"Failed to create software render context: {GetErrorString(err)}");
                }

                // Set update callback
                _renderUpdateCallback = OnRenderUpdate;
                var callbackPtr = Marshal.GetFunctionPointerForDelegate(_renderUpdateCallback);
                _mpvRenderContextSetUpdateCallback(_renderContext, callbackPtr, IntPtr.Zero);
            }
            finally
            {
                Marshal.FreeHGlobal(renderParamsPtr);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(apiTypePtr);
        }
    }

    public void SoftwareRender(int width, int height, IntPtr surfaceAddress, string format)
    {
        if (_disposed)
        {
            return;
        }

        if (_renderContext == IntPtr.Zero || _mpvRenderContextRender == null)
        {
            return;
        }

        System.Diagnostics.Debug.WriteLine($"SoftwareRender: width={width}, height={height}, format={format}, address={surfaceAddress}");

        unsafe
        {
            var size = new[] { width, height };
            // MPV_RENDER_PARAM_SW_STRIDE expects a pointer to size_t
            // size_t is platform-specific: 4 bytes on 32-bit, 8 bytes on 64-bit
            // nuint (native uint) is the C# equivalent of size_t
            nuint stride = (nuint)(width * 4);

            fixed (int* sizePtr = size)
            {
                var formatBytes = Encoding.UTF8.GetBytes(format + "\0");
                var formatPtr = Marshal.AllocHGlobal(formatBytes.Length);
                Marshal.Copy(formatBytes, 0, formatPtr, formatBytes.Length);

                // Allocate and write the stride value (size_t)
                var stridePtr = Marshal.AllocHGlobal(IntPtr.Size);
                if (IntPtr.Size == 8) // 64-bit
                {
                    *(ulong*)stridePtr = (ulong)stride;
                }
                else // 32-bit
                {
                    *(uint*)stridePtr = (uint)stride;
                }

                try
                {
                    var renderParams = new[]
                    {
                        new MpvRenderParam { type = MPV_RENDER_PARAM_SW_SIZE, data = (IntPtr)sizePtr },
                        new MpvRenderParam { type = MPV_RENDER_PARAM_SW_FORMAT, data = formatPtr },
                        new MpvRenderParam { type = MPV_RENDER_PARAM_SW_STRIDE, data = stridePtr },
                        new MpvRenderParam { type = MPV_RENDER_PARAM_SW_POINTER, data = surfaceAddress },
                        new MpvRenderParam { type = MPV_RENDER_PARAM_INVALID, data = IntPtr.Zero }
                    };

                    var renderParamsSize = Marshal.SizeOf<MpvRenderParam>() * renderParams.Length;
                    var renderParamsPtr = Marshal.AllocHGlobal(renderParamsSize);

                    try
                    {
                        for (var i = 0; i < renderParams.Length; i++)
                        {
                            var offset = renderParamsPtr + (i * Marshal.SizeOf<MpvRenderParam>());
                            Marshal.StructureToPtr(renderParams[i], offset, false);
                        }

                        var err = _mpvRenderContextRender(_renderContext, renderParamsPtr);
                        if (err < 0 && err != -2) // -2 = nothing to render
                        {
                            System.Diagnostics.Debug.WriteLine($"Software render failed: {GetErrorString(err)} (code: {err})");
                        }
                        else if (err == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Software render SUCCESS");
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(renderParamsPtr);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(formatPtr);
                    Marshal.FreeHGlobal(stridePtr);
                }
            }
        }
    }

    public void SubRemove()
    {
        DoMpvCommand("sub-remove");
    }

    public void SubReload()
    {
        DoMpvCommand("sub-reload");
    }

    public void SubAdd(string fileName)
    {
        DoMpvCommand("sub-add", fileName, "select");
    }

    public string VersionNumber
    {
        get
        {
            if (_mpv == IntPtr.Zero || _mpvClientApiVersion == null)
            {
                return string.Empty;
            }

            var version = _mpvClientApiVersion();
            var high = version >> 16;
            var low = version & 0xff;
            return high + "." + low;
        }
    }

}