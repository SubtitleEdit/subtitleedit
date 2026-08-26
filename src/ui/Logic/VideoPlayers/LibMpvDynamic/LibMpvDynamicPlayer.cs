using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

public sealed class LibMpvDynamicPlayer : IDisposable, IVideoPlayer
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

    // True once the render context was created against a graphics API whose context is
    // thread-affine (OpenGL, Metal), which decides who is allowed to free it - see Dispose.
    private volatile bool _renderContextNeedsGraphicsContext;
    private volatile bool _disposePendingRenderContextFree;
    private volatile bool _disposed;
    private volatile bool _coreInitialized;
    private string _fileName = string.Empty;
    private double? _audioEndBound;

    // A LoadFile that arrived before the (lazily initialized) core was up - replayed by
    // MarkCoreInitialized as soon as the first render pass brings the core online (#14047).
    // Claimed via Interlocked.Exchange so the replay and a concurrent LoadFile can't both
    // run the same request.
    private string? _pendingLoadFileName;
    private double _pendingLoadStartPositionSeconds;

    // Observed-property caches, kept current by the mpv event thread (see StartEventLoop).
    // While _eventLoopActive the pause/speed/duration/eof getters read these instead of
    // doing a synchronous P/Invoke into the core per call. Doubles go through Interlocked
    // as long bits so a 32-bit runtime can't tear them.
    private Thread? _eventThread;
    private IntPtr _eventLoopHandle;
    private volatile bool _eventLoopStop;
    private volatile bool _eventLoopActive;
    private volatile bool _observedPause = true;
    private volatile bool _observedEofReached;
    private long _observedSpeedBits = BitConverter.DoubleToInt64Bits(1.0);
    private long _observedDurationBits;
    private long _observedTimePosBits;
    private volatile bool _observedTimePosValid; // false = property unavailable (no file) -> Position reports 0, like the live read
    private long _lastPlaybackRestartTimestamp; // Stopwatch ticks of the last MPV_EVENT_PLAYBACK_RESTART

    [StructLayout(LayoutKind.Sequential)]
    private struct MpvOpenGlInitParams
    {
        public IntPtr get_proc_address;
        public IntPtr get_proc_address_ctx;
    }

    /// <summary>
    /// Matches <c>mpv_metal_init_params</c> in render_metal.h.
    /// Both <see cref="device"/> (required) and <see cref="layer"/> (optional) must
    /// be Objective-C object pointers.  When <see cref="layer"/> is non-null mpv
    /// acquires and presents drawables from the layer automatically on every
    /// <see cref="RenderMetal"/> call.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MpvMetalInitParams
    {
        /// <summary>id&lt;MTLDevice&gt; – required.</summary>
        public IntPtr device;
        /// <summary>id&lt;CAMetalLayer&gt; – optional.  When set mpv manages drawables.</summary>
        public IntPtr layer;
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
    private delegate int MpvCommandAsync(IntPtr mpvHandle, ulong replyUserdata, IntPtr utf8Strings);

    private MpvCommandAsync? _mpvCommandAsync;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr MpvWaitEvent(IntPtr mpvHandle, double wait);

    private MpvWaitEvent? _mpvWaitEvent;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvObserveProperty(IntPtr mpvHandle, ulong replyUserdata, byte[] name, int format);

    private MpvObserveProperty? _mpvObserveProperty;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MpvWakeup(IntPtr mpvHandle);

    private MpvWakeup? _mpvWakeup;

    /// <summary>Matches <c>mpv_event</c> in client.h.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MpvEvent
    {
        public int eventId;
        public int error;
        public ulong replyUserdata;
        public IntPtr data;
    }

    /// <summary>Matches <c>mpv_event_property</c> in client.h.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MpvEventProperty
    {
        public IntPtr name;
        public int format;
        public IntPtr data;
    }

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

    /// <summary>
    /// MPV_FORMAT_FLAG makes mpv write a 4-byte int, so it must not be read through the
    /// "ref double" overload: the value would land in the low half of the 8-byte double and a
    /// flag of 1 would read back as the subnormal 5E-324 rather than 1.0.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int MpvGetPropertyFlag(IntPtr mpvHandle, byte[] name, int format, ref int data);

    private MpvGetPropertyFlag? _mpvGetPropertyFlag;

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
    private const string MPV_RENDER_API_TYPE_METAL = "metal";

    // Metal render param types (see render_metal.h)
    private const int MPV_RENDER_PARAM_METAL_INIT_PARAMS = 21;
    private const int MPV_RENDER_PARAM_METAL_DRAWABLE = 22;

    private const int MPV_FORMAT_NONE = 0;
    private const int MPV_FORMAT_STRING = 1;
    private const int MPV_FORMAT_FLAG = 3;
    private const int MPV_FORMAT_INT64 = 4;
    private const int MPV_FORMAT_DOUBLE = 5;

    private const int MPV_EVENT_SHUTDOWN = 1;
    private const int MPV_EVENT_PLAYBACK_RESTART = 21;
    private const int MPV_EVENT_PROPERTY_CHANGE = 22;

    // reply_userdata ids for the observed properties (see StartEventLoop)
    private const ulong ObserveIdPause = 1;
    private const ulong ObserveIdSpeed = 2;
    private const ulong ObserveIdDuration = 3;
    private const ulong ObserveIdEofReached = 4;
    private const ulong ObserveIdTimePos = 5;

    public event Action? RequestRender;

    public LibMpvDynamicPlayer()
    {

    }

    private static string[] GetLibraryNames()
    {
        if (OperatingSystem.IsWindows())
        {
            return ["libmpv-2.dll"];
        }
        else if (OperatingSystem.IsLinux())
        {
            return ["libmpv.so.2", "libmpv.so"];
        }
        else if (OperatingSystem.IsMacOS())
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
        if (OperatingSystem.IsWindows())
        {
            return
            [
                MpvPath,
                Directory.GetCurrentDirectory(),
                string.Empty,
            ];
        }
        else if (OperatingSystem.IsLinux())
        {
            return
            [
                MpvPath,
                Directory.GetCurrentDirectory(),
                "/app/lib",
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
        else if (OperatingSystem.IsMacOS())
        {
            return
            [
                MpvPath,
                Directory.GetCurrentDirectory(),
                // Running .app bundle's Frameworks dir, regardless of install
                // location. AppContext.BaseDirectory is Contents/MacOS/.
                Path.Combine(AppContext.BaseDirectory, "..", "Frameworks"),
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
        _mpvObserveProperty = (MpvObserveProperty)GetDllType(typeof(MpvObserveProperty), "mpv_observe_property");
        _mpvWakeup = (MpvWakeup)GetDllType(typeof(MpvWakeup), "mpv_wakeup");
        _mpvCommand = (MpvCommand)GetDllType(typeof(MpvCommand), "mpv_command");
        _mpvCommandAsync = (MpvCommandAsync)GetDllType(typeof(MpvCommandAsync), "mpv_command_async");
        _mpvSetOption = (MpvSetOption)GetDllType(typeof(MpvSetOption), "mpv_set_option");
        _mpvSetOptionString = (MpvSetOptionString)GetDllType(typeof(MpvSetOptionString), "mpv_set_option_string");
        _mpvGetPropertyString = (MpvGetPropertyString)GetDllType(typeof(MpvGetPropertyString), "mpv_get_property");
        _mpvGetPropertyDouble = (MpvGetPropertyDouble)GetDllType(typeof(MpvGetPropertyDouble), "mpv_get_property");
        _mpvGetPropertyFlag = (MpvGetPropertyFlag)GetDllType(typeof(MpvGetPropertyFlag), "mpv_get_property");
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

        SetYtDlpPathOption();
        SetAudioBufferOption();

        var err = _mpvInitialize(_mpv);
        if (err >= 0)
        {
            MarkCoreInitialized();
        }

        return err;
    }

    /// <summary>
    /// Flips the core to initialized, starts the event loop, and replays a LoadFile that
    /// arrived before the core was up. The core is created lazily by the first render pass
    /// (see WaitForCoreInitializedAsync), so a load issued before any frame was rendered -
    /// e.g. an "Open with" launch while the video window is still being built, or a window
    /// opened minimized/behind - used to fail with MPV_ERROR_UNINITIALIZED and the open was
    /// silently lost (#14047).
    /// </summary>
    private void MarkCoreInitialized()
    {
        _coreInitialized = true;
        StartEventLoop();

        var pendingFileName = Interlocked.Exchange(ref _pendingLoadFileName, null);
        if (string.IsNullOrEmpty(pendingFileName))
        {
            return;
        }

        var startPositionSeconds = _pendingLoadStartPositionSeconds;

        // Off this thread: the rendering Initialize* methods run during a render pass, and
        // loadfile has no business there (it can block on I/O).
        Task.Run(async () =>
        {
            try
            {
                if (_disposed)
                {
                    return;
                }

                await LoadFile(pendingFileName, startPositionSeconds);
            }
            catch (Exception e)
            {
                Se.LogError(e, "LibMpvDynamicPlayer deferred LoadFile replay");
            }
        });
    }

    /// <summary>
    /// Observes the state-shaped properties (pause/speed/duration/eof-reached) and runs a
    /// dedicated thread draining mpv's event queue into the caches above. Before this, every
    /// IsPlaying/Speed/Duration read was a synchronous P/Invoke into the core - the playhead
    /// cursor timer alone issued three per 16 ms tick - and Subtitle Edit had no way to know
    /// when mpv had actually finished applying a seek, which is what the playhead seek pin's
    /// arrive-tolerance/timeout heuristics guess at. MPV_EVENT_PLAYBACK_RESTART states it
    /// exactly (see <see cref="HasPlaybackRestartedSince"/>).
    /// <para>
    /// If anything here fails the getters silently keep their live P/Invoke fallback - the
    /// caches only take over once <c>_eventLoopActive</c> is set.
    /// </para>
    /// </summary>
    private void StartEventLoop()
    {
        StopEventLoop();

        var handle = _mpv;
        if (handle == IntPtr.Zero || _mpvWaitEvent == null || _mpvObserveProperty == null || _mpvWakeup == null)
        {
            return;
        }

        if (_mpvObserveProperty(handle, ObserveIdPause, PropertyNamePause, MPV_FORMAT_FLAG) < 0 ||
            _mpvObserveProperty(handle, ObserveIdSpeed, PropertyNameSpeed, MPV_FORMAT_DOUBLE) < 0 ||
            _mpvObserveProperty(handle, ObserveIdDuration, PropertyNameDuration, MPV_FORMAT_DOUBLE) < 0 ||
            _mpvObserveProperty(handle, ObserveIdEofReached, PropertyNameEofReached, MPV_FORMAT_FLAG) < 0 ||
            _mpvObserveProperty(handle, ObserveIdTimePos, PropertyNameTimePos, MPV_FORMAT_DOUBLE) < 0)
        {
            return;
        }

        // Seed the caches with live reads so the getters are right in the short gap before
        // mpv's initial change notifications arrive. Unavailable properties (duration before
        // a file is loaded) keep their defaults.
        if (_mpvGetPropertyFlag != null)
        {
            var flag = 0;
            if (_mpvGetPropertyFlag(handle, PropertyNamePause, MPV_FORMAT_FLAG, ref flag) >= 0)
            {
                _observedPause = flag != 0;
            }
        }

        if (_mpvGetPropertyDouble != null)
        {
            double value = 0;
            if (_mpvGetPropertyDouble(handle, PropertyNameSpeed, MPV_FORMAT_DOUBLE, ref value) >= 0 && value > 0)
            {
                Interlocked.Exchange(ref _observedSpeedBits, BitConverter.DoubleToInt64Bits(value));
            }

            value = 0;
            if (_mpvGetPropertyDouble(handle, PropertyNameDuration, MPV_FORMAT_DOUBLE, ref value) >= 0)
            {
                Interlocked.Exchange(ref _observedDurationBits, BitConverter.DoubleToInt64Bits(value));
            }

            value = 0;
            if (_mpvGetPropertyDouble(handle, PropertyNameTimePos, MPV_FORMAT_DOUBLE, ref value) >= 0)
            {
                Interlocked.Exchange(ref _observedTimePosBits, BitConverter.DoubleToInt64Bits(value));
                _observedTimePosValid = true;
            }
            else
            {
                _observedTimePosValid = false;
            }
        }

        _eventLoopStop = false;
        _eventLoopHandle = handle;
        _eventThread = new Thread(() => RunEventLoop(handle))
        {
            IsBackground = true,
            Name = "mpv-events",
        };
        _eventThread.Start();
        _eventLoopActive = true;
    }

    private void RunEventLoop(IntPtr handle)
    {
        var waitEvent = _mpvWaitEvent!;
        while (!_eventLoopStop)
        {
            IntPtr eventPtr;
            try
            {
                eventPtr = waitEvent(handle, 1.0);
            }
            catch
            {
                break;
            }

            if (_eventLoopStop)
            {
                break;
            }

            if (eventPtr == IntPtr.Zero)
            {
                continue;
            }

            var mpvEvent = Marshal.PtrToStructure<MpvEvent>(eventPtr);
            if (mpvEvent.eventId == MPV_EVENT_SHUTDOWN)
            {
                break;
            }

            if (mpvEvent.eventId == MPV_EVENT_PLAYBACK_RESTART)
            {
                Interlocked.Exchange(ref _lastPlaybackRestartTimestamp, System.Diagnostics.Stopwatch.GetTimestamp());
            }
            else if (mpvEvent.eventId == MPV_EVENT_PROPERTY_CHANGE && mpvEvent.data != IntPtr.Zero)
            {
                var property = Marshal.PtrToStructure<MpvEventProperty>(mpvEvent.data);
                switch (mpvEvent.replyUserdata)
                {
                    case ObserveIdPause:
                        if (property.format == MPV_FORMAT_FLAG && property.data != IntPtr.Zero)
                        {
                            _observedPause = Marshal.ReadInt32(property.data) != 0;
                        }

                        break;
                    case ObserveIdSpeed:
                        if (property.format == MPV_FORMAT_DOUBLE && property.data != IntPtr.Zero)
                        {
                            Interlocked.Exchange(ref _observedSpeedBits, Marshal.ReadInt64(property.data));
                        }

                        break;
                    case ObserveIdDuration:
                        if (property.format == MPV_FORMAT_DOUBLE && property.data != IntPtr.Zero)
                        {
                            Interlocked.Exchange(ref _observedDurationBits, Marshal.ReadInt64(property.data));
                        }
                        else if (property.format == MPV_FORMAT_NONE)
                        {
                            // No file loaded: mimic the live getter, which returns 0 then.
                            Interlocked.Exchange(ref _observedDurationBits, 0);
                        }

                        break;
                    case ObserveIdEofReached:
                        if (property.format == MPV_FORMAT_FLAG && property.data != IntPtr.Zero)
                        {
                            _observedEofReached = Marshal.ReadInt32(property.data) != 0;
                        }
                        else if (property.format == MPV_FORMAT_NONE)
                        {
                            _observedEofReached = false;
                        }

                        break;
                    case ObserveIdTimePos:
                        if (property.format == MPV_FORMAT_DOUBLE && property.data != IntPtr.Zero)
                        {
                            Interlocked.Exchange(ref _observedTimePosBits, Marshal.ReadInt64(property.data));
                            _observedTimePosValid = true;
                        }
                        else if (property.format == MPV_FORMAT_NONE)
                        {
                            // No file loaded: the live read errors and reports 0 - mirror that.
                            Interlocked.Exchange(ref _observedTimePosBits, 0);
                            _observedTimePosValid = false;
                        }

                        break;
                }
            }
        }
    }

    /// <summary>
    /// Stops the event thread and waits for it to exit. Must complete before
    /// <c>mpv_terminate_destroy</c> runs: no other thread may sit in <c>mpv_wait_event</c>
    /// while the core is being destroyed.
    /// </summary>
    private void StopEventLoop()
    {
        var thread = _eventThread;
        if (thread == null)
        {
            return;
        }

        _eventThread = null;
        _eventLoopActive = false;
        _eventLoopStop = true;

        var handle = _eventLoopHandle;
        _eventLoopHandle = IntPtr.Zero;
        if (handle != IntPtr.Zero)
        {
            try
            {
                _mpvWakeup?.Invoke(handle);
            }
            catch
            {
                // ignore - the 1 s wait_event timeout still bounds the join below
            }
        }

        if (!thread.Join(3000))
        {
            // wait_event re-checks the stop flag at least once a second, so this should never
            // happen; log and continue - leaking a wedged thread beats hanging the dispose.
            Se.LogError(new InvalidOperationException("mpv event thread did not stop"), "LibMpvDynamicPlayer StopEventLoop");
        }
    }

    /// <summary>
    /// True when the mpv event loop is live, i.e. <see cref="HasPlaybackRestartedSince"/>
    /// carries real information.
    /// </summary>
    public bool SupportsPlaybackRestartEvents => _eventLoopActive;

    /// <summary>
    /// Whether mpv has reported MPV_EVENT_PLAYBACK_RESTART - "seek finished, output resumed
    /// from the new position" (it also fires when a file starts) - since the given
    /// Stopwatch timestamp. This is the exact signal for "the seek has landed" that the
    /// playhead seek pin otherwise has to infer from position tolerances and timeouts.
    /// </summary>
    public bool HasPlaybackRestartedSince(long stopwatchTimestamp)
    {
        return Interlocked.Read(ref _lastPlaybackRestartTimestamp) > stopwatchTimestamp;
    }

    private static byte[] GetUtf8Bytes(string s)
    {
        return Encoding.UTF8.GetBytes(s + "\0");
    }

    // Cached UTF-8 names for the hot polled properties: the UI polls these up to ~60x/s
    // during playback (playhead cursor timer + position timers), and GetUtf8Bytes
    // allocated a fresh byte[] per call.
    private static readonly byte[] PropertyNameTimePos = GetUtf8Bytes("time-pos");
    private static readonly byte[] PropertyNamePause = GetUtf8Bytes("pause");
    private static readonly byte[] PropertyNameEofReached = GetUtf8Bytes("eof-reached");
    private static readonly byte[] PropertyNameSpeed = GetUtf8Bytes("speed");
    private static readonly byte[] PropertyNameDuration = GetUtf8Bytes("duration");
    private static readonly byte[] PropertyNameVolume = GetUtf8Bytes("volume");

    public string GetErrorString(int error)
    {
        if (_mpvErrorString == null)
        {
            return $"mpv error {error}";
        }

        var ptr = _mpvErrorString(error);
        return ptr == IntPtr.Zero ? $"mpv error {error}" : Marshal.PtrToStringUTF8(ptr) ?? $"mpv error {error}";
    }

    /// <summary>
    /// Whether the black bars of a letterboxed video count as part of the area the subtitle may
    /// use. With it on, the margin can move the preview off the picture and onto the bar, which
    /// keeps the translation clear of burned-in forced narrative (#13934).
    ///
    /// sub-use-margins covers plain subtitles; an ASS subtitle - which is what the preview is -
    /// stays inside the video frame unless sub-ass-force-margins is on too, and mpv defaults that
    /// to "no". Both are written on every call, so turning the setting off again restores mpv's
    /// own defaults instead of leaving the last value in place.
    /// </summary>
    public void ApplySubtitleMarginArea()
    {
        var useMargins = Se.Settings.Video.MpvPreviewMarginIsPartOfSubtitleArea;
        SetOptionString("sub-use-margins", useMargins ? "yes" : "no");
        SetOptionString("sub-ass-force-margins", useMargins ? "yes" : "no");
    }

    public int SetOptionString(string name, string value)
    {
        if (_mpvSetOptionString == null || _mpv == IntPtr.Zero)
        {
            return -1;
        }

        var nameBytes = GetUtf8Bytes(name);
        var valueBytes = GetUtf8Bytes(value);
        return _mpvSetOptionString(_mpv, nameBytes, valueBytes);
    }

    /// <summary>
    /// Tells mpv where to find yt-dlp, which it needs for streaming URLs (YouTube and friends).
    /// <para>
    /// mpv's ytdl_hook searches only mpv's own config directory and PATH; the copy Subtitle Edit
    /// downloads lives in the data folder - <c>%AppData%\Subtitle Edit</c> in an installed Windows
    /// build - which is on neither, so "open video from URL / open online" failed there. The
    /// portable build worked only by accident: its data folder IS the folder holding the
    /// executable, which Windows searches when starting a process (issue #13067). Adding the
    /// folder to PATH is not an option off Windows either - .NET keeps its own copy of the
    /// environment on Unix, so a managed PATH change is invisible to libmpv's getenv.
    /// </para>
    /// <para>
    /// Must be called before mpv_initialize: the scripts read their options when they load.
    /// </para>
    /// </summary>
    private void SetYtDlpPathOption()
    {
        // Subtitle Edit sets no other script options, so assigning the whole list is safe.
        // ("script-opts-append", which would append a single entry verbatim, is not accepted by
        // mpv_set_option_string - it answers MPV_ERROR_OPTION_NOT_FOUND.)
        var value = "ytdl_hook-ytdl_path=" + GetYtDlpPathList();
        var err = SetOptionString("script-opts", value);
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer could not set " + value);
        }
    }

    /// <summary>
    /// The path list handed to ytdl_hook, most specific first. ytdl_hook tries each entry in turn
    /// and moves on when one cannot be started, so the downloaded copy is listed whether or not it
    /// exists yet - it may well be downloaded later in the same session, after mpv came up. The
    /// well-known system locations and mpv's own "yt-dlp" default come last so a user's own
    /// install keeps working.
    /// </summary>
    internal static string GetYtDlpPathList()
    {
        // The copy Subtitle Edit downloads into the data folder and version-checks itself.
        var paths = new List<string> { YtDlpDownloadService.GetFullFileName() };

        if (OperatingSystem.IsWindows())
        {
            // Where a portable install keeps it - and where anyone who worked around this bug by
            // hand put it.
            paths.Add(Path.Combine(Se.ExePath, "yt-dlp.exe"));
        }
        else if (OperatingSystem.IsMacOS())
        {
            paths.Add("/opt/homebrew/bin/yt-dlp"); // Homebrew (Apple Silicon)
            paths.Add("/usr/local/bin/yt-dlp");    // Homebrew (Intel)
            paths.Add("/opt/local/bin/yt-dlp");    // MacPorts
            paths.Add("/usr/bin/yt-dlp");
        }
        else if (OperatingSystem.IsLinux())
        {
            paths.Add("/usr/local/bin/yt-dlp");
            paths.Add("/usr/bin/yt-dlp");
            paths.Add("/opt/yt-dlp/yt-dlp");
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", "yt-dlp"));
        }

        paths.Add("yt-dlp"); // mpv's own default: whatever is in PATH

        return JoinYtDlpPaths(paths);
    }

    /// <summary>
    /// Joins the candidates the way ytdl_hook wants them: separated by <c>;</c> on Windows and
    /// <c>:</c> elsewhere, in order, without repeats (in a portable install the data folder IS the
    /// executable folder). Candidates that cannot survive the trip are dropped - script-opts is a
    /// comma-separated key=value list with no escaping, so one path holding a comma, or the list
    /// separator itself, makes mpv reject the entire option (MPV_ERROR_OPTION_ERROR) and no path at
    /// all reaches the hook.
    /// </summary>
    internal static string JoinYtDlpPaths(IEnumerable<string> paths)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var usable = new List<string>();
        foreach (var path in paths)
        {
            if (path.Contains(',') || path.Contains(Path.PathSeparator))
            {
                continue;
            }

            if (seen.Add(path))
            {
                usable.Add(path);
            }
        }

        return string.Join(Path.PathSeparator, usable);
    }

    /// <summary>
    /// Brings the core up paused.
    /// <para>
    /// mpv's own default is <c>pause=no</c>, so a file starts playing the moment the demuxer has
    /// something - Subtitle Edit never wants that: opening a video is an editing action, not a
    /// "watch it" one, and every caller that does want playback (visual sync, the ASSA previews,
    /// cut video) asks for it explicitly. Pausing from managed code once the load has been issued
    /// is too late: that call sits behind an await continuation on the UI thread, which at
    /// start-up - restoring the last session, building the grid, laying out the waveform - can
    /// take a few hundred milliseconds, and the user gets a burst of the video at mpv's default
    /// volume before it lands (issue #13329).
    /// </para>
    /// <para>Must be called before mpv_initialize.</para>
    /// </summary>
    private void SetStartPausedOption()
    {
        var err = SetOptionString("pause", "yes");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer could not set pause=yes");
        }
    }

    /// <summary>
    /// Shrinks mpv's audio output buffer (its default is 0.2 s). That buffer is why pause,
    /// resume and seeks during playback take effect ~200 ms late: a pause only goes quiet once
    /// the buffered audio has played out, and resume/seek only sound once it has been refilled.
    /// Those latencies are the residuals the waveform playhead estimate exists to mask
    /// (#12740, discussion #10824), so keep the buffer short. A zero or negative setting
    /// leaves mpv's default alone - the escape hatch for machines where a small buffer
    /// causes audio dropouts (slow hardware, Bluetooth audio).
    /// <para>Must be called before mpv_initialize.</para>
    /// </summary>
    private void SetAudioBufferOption()
    {
        var seconds = Se.Settings.Video.MpvAudioBufferSeconds;
        if (seconds <= 0)
        {
            return;
        }

        var err = SetOptionString("audio-buffer", seconds.ToString("0.###", CultureInfo.InvariantCulture));
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer could not set audio-buffer");
        }
    }

    private int _brightness;

    public int ToggleBrightness()
    {
        _brightness += 25;
        if (_brightness > 100)
        {
            _brightness = -100;
        }

        SetOptionString("brightness", _brightness.ToString(CultureInfo.InvariantCulture));
        return _brightness;
    }

    private int _contrast;

    public int ToggleContrast()
    {
        _contrast += 25;
        if (_contrast > 100)
        {
            _contrast = -100;
        }

        SetOptionString("contrast", _contrast.ToString(CultureInfo.InvariantCulture));
        return _contrast;
    }

    public static IntPtr AllocateUtf8IntPtrArrayWithSentinel(string[] arr, out IntPtr[] byteArrayPointers)
    {
        var numberOfStrings = arr.Length + 1;
        byteArrayPointers = new IntPtr[numberOfStrings];
        // AllocHGlobal, matching the FreeHGlobal in the callers - this was AllocCoTaskMem,
        // which pairs with FreeCoTaskMem: freeing it with FreeHGlobal is a mismatched
        // allocator on Windows (a silent per-command leak; the Unix runtimes map both to
        // malloc/free, which is why it never showed there).
        var rootPointer = Marshal.AllocHGlobal(IntPtr.Size * numberOfStrings);
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

    /// <summary>
    /// Queues a command without waiting for the core to run it. mpv_command (the synchronous
    /// form) holds the caller until the core's dispatch accepts the command - during a scrub
    /// or slider-drag seek storm on a heavy file that stall lands on the UI thread, once per
    /// mouse move. mpv_command_async returns as soon as the command is copied into the queue;
    /// the core posts an MPV_EVENT_COMMAND_REPLY back, which the event loop drains (and
    /// ignores - matching the synchronous path, whose seek result was never acted on either).
    /// mpv copies the argument array before returning, so the buffers are freed right away
    /// exactly like in <see cref="DoMpvCommand"/>. Falls back to the synchronous path when
    /// the event loop is not running, so the reply events cannot pile up unread.
    /// </summary>
    private int DoMpvCommandFireAndForget(params string[] args)
    {
        if (_mpv == IntPtr.Zero)
        {
            return 0;
        }

        if (!_eventLoopActive || _mpvCommandAsync == null)
        {
            return DoMpvCommand(args);
        }

        var mainPtr = AllocateUtf8IntPtrArrayWithSentinel(args, out var byteArrayPointers);
        var result = _mpvCommandAsync(_mpv, 0, mainPtr);
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
        SetStartPausedOption();

        // On Linux, do NOT force gpu-context.  Avalonia (11.x) has no native
        // Wayland backend — it always provides an X11/XWayland OpenGL context,
        // so let mpv auto-detect from the context it receives.

        SetYtDlpPathOption();
        SetAudioBufferOption();

        // Initialize mpv first
        var err = _mpvInitialize(_mpv);
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer InitializeWithOpenGL mpv_initialize");
        }
        else
        {
            MarkCoreInitialized();
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

            // Only the GL thread may free this again - see Dispose / FreeRenderContext.
            _renderContextNeedsGraphicsContext = true;

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

    /// <summary>
    /// Initialises the mpv Metal render context.
    /// <para>
    /// Both <paramref name="mtlDevice"/> and <paramref name="metalLayer"/> must
    /// be valid Objective-C object pointers (obtained via the macOS Objective-C
    /// runtime).  Passing the <c>CAMetalLayer</c> lets mpv manage drawable
    /// acquisition and presentation internally, so callers only need to invoke
    /// <see cref="RenderMetal"/> when a new frame is ready.
    /// </para>
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    public void InitializeWithMetal(IntPtr mtlDevice, IntPtr metalLayer)
    {
        LoadLibraryInternal();
        EnsureNotDisposed();

        if (_mpvInitialize == null || _mpvRenderContextCreate == null || _mpvRenderContextSetUpdateCallback == null)
        {
            Se.LogError(new InvalidOperationException("MPV delegates not loaded"), "LibMpvDynamicPlayer InitializeWithMetal");
            return;
        }

        // Tell mpv to use the external (libmpv) renderer.
        SetOptionString("vo", "libmpv");
        SetOptionString("gpu-api", "metal");
        SetStartPausedOption();

        SetYtDlpPathOption();
        SetAudioBufferOption();

        var err = _mpvInitialize(_mpv);
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer InitializeWithMetal mpv_initialize");
        }
        else
        {
            MarkCoreInitialized();
        }

        // Build mpv_metal_init_params: device (required) + layer (optional).
        // With a layer set, mpv handles nextDrawable / presentDrawable internally.
        var initParams = new MpvMetalInitParams
        {
            device = mtlDevice,
            layer = metalLayer
        };

        var initParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MpvMetalInitParams>());
        Marshal.StructureToPtr(initParams, initParamsPtr, false);

        try
        {
            var apiTypeBytes = Encoding.UTF8.GetBytes(MPV_RENDER_API_TYPE_METAL + "\0");
            var apiTypePtr = Marshal.AllocHGlobal(apiTypeBytes.Length);
            Marshal.Copy(apiTypeBytes, 0, apiTypePtr, apiTypeBytes.Length);

            var renderParams = new[]
            {
                new MpvRenderParam { type = MPV_RENDER_PARAM_API_TYPE, data = apiTypePtr },
                new MpvRenderParam { type = MPV_RENDER_PARAM_METAL_INIT_PARAMS, data = initParamsPtr },
                new MpvRenderParam { type = MPV_RENDER_PARAM_INVALID, data = IntPtr.Zero }
            };

            var renderParamsSize = Marshal.SizeOf<MpvRenderParam>() * renderParams.Length;
            var renderParamsPtr = Marshal.AllocHGlobal(renderParamsSize);

            for (var i = 0; i < renderParams.Length; i++)
            {
                var offset = renderParamsPtr + (i * Marshal.SizeOf<MpvRenderParam>());
                Marshal.StructureToPtr(renderParams[i], offset, false);
            }

            err = _mpvRenderContextCreate(out _renderContext, _mpv, renderParamsPtr);
            if (err < 0)
            {
                Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer InitializeWithMetal mpv_render_context_create");
            }

            // Only the render thread may free this again - see Dispose / FreeRenderContext.
            _renderContextNeedsGraphicsContext = true;

            // Register the render-update callback so mpv can trigger redraws.
            _renderUpdateCallback = OnRenderUpdate;
            var callbackPtr = Marshal.GetFunctionPointerForDelegate(_renderUpdateCallback);
            _mpvRenderContextSetUpdateCallback(_renderContext, callbackPtr, IntPtr.Zero);

            Marshal.FreeHGlobal(renderParamsPtr);
            Marshal.FreeHGlobal(apiTypePtr);
        }
        finally
        {
            Marshal.FreeHGlobal(initParamsPtr);
        }
    }

    /// <summary>
    /// Asks mpv to render the next frame using the Metal backend.
    /// <para>
    /// Because a <c>CAMetalLayer</c> was supplied in
    /// <see cref="InitializeWithMetal"/>, mpv acquires and presents the
    /// drawable automatically – no drawable pointer needs to be passed here.
    /// </para>
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    public void RenderMetal()
    {
        if (_renderContext == IntPtr.Zero || _mpvRenderContextRender == null)
        {
            return;
        }

        // The layer was provided in init params; mpv manages drawables
        // internally.  An empty (terminator-only) params list is sufficient.
        var renderParams = new[]
        {
            new MpvRenderParam { type = MPV_RENDER_PARAM_INVALID, data = IntPtr.Zero }
        };

        var renderParamsSize = Marshal.SizeOf<MpvRenderParam>() * renderParams.Length;
        var renderParamsPtr = Marshal.AllocHGlobal(renderParamsSize);

        try
        {
            Marshal.StructureToPtr(renderParams[0], renderParamsPtr, false);

            var err = _mpvRenderContextRender(_renderContext, renderParamsPtr);
            if (err < 0 && err != -2) // -2 = MPV_ERROR_NOTHING_TO_RENDER
            {
                Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer RenderMetal mpv_render_context_render");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(renderParamsPtr);
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

    /// <summary>
    /// Records that this player is being thrown away. Call it before detaching the render host:
    /// dropping the host fires the graphics deinit callback, and that callback only completes a
    /// teardown it can already see (see <see cref="FreeRenderContextIfDisposePending"/>). Marking
    /// afterwards would race the callback and leave the core alive - the leak from issue #13048.
    /// Cheap and non-blocking, unlike <see cref="Dispose"/>.
    /// </summary>
    public void MarkForDispose()
    {
        if (_renderContextNeedsGraphicsContext)
        {
            _disposePendingRenderContextFree = true;
        }
    }

    /// <summary>
    /// Completes a <see cref="Dispose"/> that had to wait for the graphics thread, and does
    /// nothing at all if no dispose is pending.
    /// <para>
    /// Call this from the render control's deinit callback, where the graphics context is
    /// current: <c>mpv_render_context_free</c> deletes the GPU objects mpv allocated in that
    /// context, so for OpenGL and Metal no other thread may free it. Deinit alone is not a
    /// reason to tear anything down - Avalonia deinits on a plain reparent too - which is why
    /// this is a no-op unless the owner already asked for the player to go away.
    /// </para>
    /// </summary>
    public void FreeRenderContextIfDisposePending()
    {
        if (!_disposePendingRenderContextFree)
        {
            return;
        }

        FreeRenderContext();

        // Only the render-context free needs the graphics context - the core does not, and
        // mpv_terminate_destroy blocks until every mpv worker has exited. The deinit callback
        // runs on the UI thread, so terminating here would freeze the UI for as long as a
        // stuck load takes to unwind (the "Not Responding" kill in issue #13083) - the very
        // thing the worker-thread dispose in VideoPlayerControl.CloseAndDisposePlayer was
        // meant to prevent (issue #11176) but couldn't, because on this path the Dispose it
        // runs is reduced to setting a flag. TerminateCore is idempotent (interlocked handle
        // claim), so racing the owner's background Dispose is safe.
        Task.Run(TerminateCore);
    }

    private void FreeRenderContext()
    {
        var renderContext = Interlocked.Exchange(ref _renderContext, IntPtr.Zero);
        if (renderContext != IntPtr.Zero && _mpvRenderContextFree != null)
        {
            _mpvRenderContextFree(renderContext);
        }
    }

    /// <summary>
    /// Destroys the mpv core. Safe to call more than once and from more than one thread at
    /// a time: a control can dispose its player when it is detached from the visual tree
    /// while the owner disposes the same instance on a worker thread, and handing the same
    /// handle to <c>mpv_terminate_destroy</c> twice is a double free. Each handle is claimed
    /// with an interlocked exchange so exactly one caller ever gets a non-zero pointer.
    /// <para>
    /// When rendering goes through a graphics API the render context belongs to a thread this
    /// one is not - freeing an OpenGL render context without that context current is undefined -
    /// so there this only records the intent and returns. The render control's deinit callback
    /// then calls <see cref="FreeRenderContextIfDisposePending"/>, which frees the context and
    /// destroys the core. If that callback never arrives the core outlives us, which is exactly
    /// what happened before this handshake existed - so asking is never worse than not asking.
    /// </para>
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
        _pendingLoadFileName = null;

        if (_renderContextNeedsGraphicsContext && _renderContext != IntPtr.Zero)
        {
            _disposePendingRenderContextFree = true;
            return;
        }

        FreeRenderContext();
        TerminateCore();
    }

    private void TerminateCore()
    {
        var mpv = Interlocked.Exchange(ref _mpv, IntPtr.Zero);
        if (mpv != IntPtr.Zero && _mpvTerminateDestroy != null)
        {
            // The event thread must be out of mpv_wait_event before the core goes away.
            StopEventLoop();
            _mpvTerminateDestroy.Invoke(mpv);
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

    /// <summary>
    /// The mpv core is initialized lazily by the rendering surface - e.g. the OpenGL
    /// control calls InitializeWithOpenGL on its first render pass. Windows that open a
    /// video right away (the burn-in and visual sync previews load the file from their
    /// Loaded event) can therefore issue "loadfile" before mpv_initialize has run; the
    /// command then fails with MPV_ERROR_UNINITIALIZED and is never retried, leaving the
    /// preview black until some other action reloads the file (issue #12205). Wait
    /// (bounded) for the core to come up before sending commands.
    /// </summary>
    private async Task WaitForCoreInitializedAsync(int timeoutMs = 5000)
    {
        var end = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!_coreInitialized && !_disposed && DateTime.UtcNow < end)
        {
            await Task.Delay(25);
        }
    }

    public async Task LoadFile(string path, double startPositionSeconds = 0)
    {
        EnsureNotDisposed();

        // A fresh explicit load supersedes any older one still parked for the core (#14047).
        _pendingLoadFileName = null;

        // For audio-only files there is no video track, so mpv never fires the render
        // callback and subtitles are never drawn.  Inject a virtual black video stream
        // via lavfi so mpv has something to render subtitles on top of.
        var ext = Path.GetExtension(path);
        var isAudioOnly = Array.Exists(Utilities.AudioFileExtensions,
            e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
        SetOptionString("lavfi-complex", isAudioOnly ? "color=black:size=1280x720:rate=25[vo]" : "");

        // Reset any end-of-playback bound from a previous file (see audio-only handling below).
        SetOptionString("end", "none");
        _audioEndBound = null;
        _lastRawTimePos = -1;

        // Open at the wanted position instead of at 0:00 - a seek issued once the file is up
        // shows the start of the video for a moment and then jumps (issue #13329). "start" is
        // sticky, so it has to be cleared ("none", its own default) for opens that want the
        // beginning - a silent failure here would strand every later open at a stale position.
        var startErr = SetOptionString("start", startPositionSeconds > 0
            ? startPositionSeconds.ToString(CultureInfo.InvariantCulture)
            : "none");
        if (startErr < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(startErr)), "LibMpvDynamicPlayer LoadFile start");
        }

        await WaitForCoreInitializedAsync();

        if (!_coreInitialized)
        {
            if (_disposed)
            {
                return;
            }

            // Still not up: the core only comes online when the first render pass runs one of
            // the Initialize* methods, and that pass hasn't happened yet (video window still
            // being built, opened minimized or behind - e.g. an "Open with" launch, #14047).
            // Issuing loadfile now would fail with MPV_ERROR_UNINITIALIZED and the open would
            // be lost with only a log line to show for it. Park the request instead;
            // MarkCoreInitialized replays it the moment the core comes up. Position before
            // file name - the file name is the claim token, so it must be published last.
            _fileName = path;
            _pendingLoadStartPositionSeconds = startPositionSeconds;
            _pendingLoadFileName = path;

            // The core may have come up between the timeout above and parking the request,
            // with MarkCoreInitialized finding no pending load to replay. Re-check and take
            // the request back; if the exchange loses, MarkCoreInitialized owns the replay.
            if (!_coreInitialized || Interlocked.Exchange(ref _pendingLoadFileName, null) == null)
            {
                Se.LogError("LibMpvDynamicPlayer LoadFile: mpv core not initialized yet - load of \"" + path + "\" deferred to core initialization");
                return;
            }
        }

        // mpv's own default is pause=no, so it starts playing the instant it has decoded
        // something. The core is created paused (see the Initialize* methods) and every caller
        // that wants playback asks for it explicitly, but pause it here too: it is a user
        // property, so anything the user did to the previous file - or a play that ran while
        // this one was being picked - would otherwise carry over into this load.
        DoMpvCommand("set", "pause", "yes");
        _pausedValue = null;

        // Before loadfile, not after: mpv applies "sid" when the file loads, and setting it
        // afterwards left a window in which an external subtitle pushed by MpvReloader was added
        // and selected, only to be deselected again a moment later - added but never drawn
        // (issue #13407). Set here it does the same job (no embedded subtitle track is picked)
        // without ever undoing a sub-add that got in first.
        SetOptionString("sid", "no");

        var err = await Task.Run(() => DoMpvCommand("loadfile", path));
        if (_disposed)
        {
            return;
        }

        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer LoadFile");
        }

        // yt-dlp used to be pointed at from here by appending its folder to PATH - it never
        // reached libmpv (see SetYtDlpPathOption) and it ran after loadfile, too late for the
        // hook it was meant to help. The path is handed to ytdl_hook before mpv_initialize now.

        SetOptionString("keep-open", "always");

        SetOptionString("hr-seek", "yes");
        SetOptionString("rebase-start-time", "no");

        ApplySubtitleMarginArea();

        _fileName = path;

        if (isAudioOnly)
        {
            // The lavfi-complex color source above produces frames forever, so without
            // an explicit end mpv would keep "playing" the black video past the audio's
            // end. Bound playback to the audio duration so it pauses at EOF.
            //
            // For .mp3 we read the duration by walking the frame headers ourselves
            // because libmpv reports a bitrate-based estimate for VBR MP3s without a
            // Xing/Info header, which is often a few seconds short and would cause
            // playback to stop before the real end of the file (issue #10953).
            double? bound = null;
            if (ext.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                bound = Media.Mp3DurationReader.TryGetDurationSeconds(path);
            }

            if (bound.HasValue && bound.Value > 0)
            {
                SetOptionString("end", bound.Value.ToString(CultureInfo.InvariantCulture));
                _audioEndBound = bound.Value;
            }
            else
            {
                for (var i = 0; i < 50 && !_disposed; i++)
                {
                    var d = Duration;
                    if (d > 0 && !double.IsInfinity(d) && !double.IsNaN(d))
                    {
                        SetOptionString("end", d.ToString(CultureInfo.InvariantCulture));
                        _audioEndBound = d;
                        break;
                    }
                    await Task.Delay(50);
                }
            }
        }
    }

    public async Task LoadAudio(string path)
    {
        EnsureNotDisposed();

        await WaitForCoreInitializedAsync();

        // Unlike LoadFile this one is meant to start playing: it backs the "play this clip"
        // buttons in the text-to-speech windows, which load a file and expect to hear it.
        // Those windows build their own core via Initialize(), which - unlike the three
        // rendering Initialize* methods - deliberately leaves mpv's pause default alone.
        var err = await Task.Run(() => DoMpvCommand("loadfile", path));
        if (_disposed)
        {
            return;
        }

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

    public void PlayOrPause()
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
        _pendingLoadFileName = null; // a close discards a load still parked for the core
        _pausedValue = null;
        _audioEndBound = null;
        _lastRawTimePos = -1;

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
            if (_eventLoopActive)
            {
                return !_observedPause;
            }

            if (_mpv == IntPtr.Zero || _mpvGetPropertyFlag == null)
            {
                return false;
            }

            try
            {
                var pauseValue = 0;
                var nameBytes = PropertyNamePause;
                var err = _mpvGetPropertyFlag(_mpv, nameBytes, MPV_FORMAT_FLAG, ref pauseValue);

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
            if (_eventLoopActive)
            {
                return _observedPause;
            }

            if (_mpv == IntPtr.Zero || _mpvGetPropertyFlag == null)
            {
                return false;
            }

            try
            {
                var pauseValue = 0;
                var nameBytes = PropertyNamePause;
                var err = _mpvGetPropertyFlag(_mpv, nameBytes, MPV_FORMAT_FLAG, ref pauseValue);

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

    // Last raw time-pos seen by the Position getter/setter, used to gate the eof-reached
    // probe below. -1 = unknown (always probe).
    private double _lastRawTimePos = -1;

    /// <summary>
    /// Whether playback could plausibly be at the audio end bound, judged from the last seen
    /// position. The Position getter is polled up to ~60x/s during playback and the eof-reached
    /// probe is an extra synchronous P/Invoke into the mpv core on every one of those reads -
    /// for audio-only files (where <see cref="_audioEndBound"/> is always set) that doubled the
    /// polling load for the whole session. Position moves continuously and is re-read many times
    /// a second, so gating on "last seen position near the bound" cannot miss the real EOF.
    /// </summary>
    private bool MightBeAtAudioEnd()
    {
        return _lastRawTimePos < 0 || !_audioEndBound.HasValue || _lastRawTimePos >= _audioEndBound.Value - 2.0;
    }

    private bool IsEofReached()
    {
        if (_eventLoopActive)
        {
            return _observedEofReached;
        }

        if (_mpv == IntPtr.Zero || _mpvGetPropertyFlag == null)
        {
            return false;
        }

        try
        {
            var eofValue = 0;
            var nameBytes = PropertyNameEofReached;
            var err = _mpvGetPropertyFlag(_mpv, nameBytes, MPV_FORMAT_FLAG, ref eofValue);
            if (err < 0)
            {
                return false;
            }

            return eofValue != 0;
        }
        catch
        {
            return false;
        }
    }

    public double Position
    {
        get
        {
            // Audio-EOF pin must take priority over the paused-value cache. mpv auto-pauses
            // at EOF (keep-open=always), so IsPaused flips true; if the user did any seek
            // earlier in the session, _pausedValue still holds that stale seek target and
            // the cache below would return it, causing the position to "jump back" to the
            // last seek when playback completes. See #10835 / #10877.
            if (_audioEndBound.HasValue && MightBeAtAudioEnd() && IsEofReached())
            {
                return _audioEndBound.Value;
            }

            if (_pausedValue.HasValue && IsPaused && !Se.Settings.General.UseFrameMode)
            {
                return _pausedValue.Value;
            }

            EnsureNotDisposed();

            // Observed cache first: this getter runs ~80x/s during playback (60 fps cursor
            // timer + the 50 ms position timer), and a live mpv_get_property takes the core's
            // lock - while the core is busy (hr-seek on a heavy file, slow network open) that
            // read can block the UI thread, felt as cursor/UI hitches. The cached value is at
            // most one mpv playloop iteration (~a frame) behind a live query; the playhead
            // estimate in MainViewModel extrapolates on top of raw reads anyway, and its
            // freeze detection relies on the raw value STOPPING when mpv stalls - which the
            // cache preserves exactly (no extrapolation here, ever, for that reason).
            if (_eventLoopActive)
            {
                if (!_observedTimePosValid)
                {
                    return 0;
                }

                var observed = BitConverter.Int64BitsToDouble(Interlocked.Read(ref _observedTimePosBits));
                _lastRawTimePos = observed;
                return observed;
            }

            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return 0;
            }

            try
            {
                double position = 0;
                var nameBytes = PropertyNameTimePos;
                var err = _mpvGetPropertyDouble(_mpv, nameBytes, MPV_FORMAT_DOUBLE, ref position);

                if (err < 0)
                {
                    return 0;
                }

                _lastRawTimePos = position;
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
            _lastRawTimePos = value; // keep the eof-reached gate accurate across seeks
            EnsureNotDisposed();
            if (_mpv == IntPtr.Zero)
            {
                return;
            }

            // Fire-and-forget: seeks arrive in storms (scrubbing, slider drags, wheel steps -
            // one per input event), every caller already treats the result as asynchronous
            // (the playhead pin waits for the position to actually arrive), and the error
            // result was never acted on here.
            DoMpvCommandFireAndForget("seek", value.ToString(CultureInfo.InvariantCulture), "absolute");
        }
    }

    public double Duration
    {
        get
        {
            EnsureNotDisposed();
            if (_eventLoopActive)
            {
                return BitConverter.Int64BitsToDouble(Interlocked.Read(ref _observedDurationBits));
            }

            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return 0;
            }

            try
            {
                double duration = 0;
                var nameBytes = PropertyNameDuration;
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
                var nameBytes = PropertyNameVolume;
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

            // Clamp volume between 0 and the player maximum. mpv's default
            // volume-max is 130, matching MaxVolume, so values above 100 boost
            // (amplify) the audio. Clamping to 100 here meant the upper part of
            // the volume slider (100..130) did nothing.
            var clampedVolume = Math.Max(0, Math.Min(MaxVolume, value));
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
            if (_eventLoopActive)
            {
                return BitConverter.Int64BitsToDouble(Interlocked.Read(ref _observedSpeedBits));
            }

            if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null)
            {
                return 1.0;
            }

            try
            {
                var speed = 1.0;
                var nameBytes = PropertyNameSpeed;
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
        _pausedValue = null;
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

        // Every other state transition clears this (LoadFile/PlayOrPause/CloseFile/Stop/Play/
        // frame steps). Without it, pausing after a seek made during playback made the Position
        // getter keep returning that old seek target, so the slider, clock and playhead all
        // jumped back to it.
        _pausedValue = null;

        var err = DoMpvCommand("set", "pause", "yes");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer pause");
        }
    }

    public void StepOneFrameForward()
    {
        _pausedValue = null;
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return;
        }

        var err = DoMpvCommand("frame-step");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer StepOneFrameForward");
        }
    }

    public void StepOneFrameBack()
    {
        _pausedValue = null;
        EnsureNotDisposed();
        if (_mpv == IntPtr.Zero)
        {
            return;
        }

        var err = DoMpvCommand("frame-back-step");
        if (err < 0)
        {
            Se.LogError(new InvalidOperationException(GetErrorString(err)), "LibMpvDynamicPlayer StepOneFrameBack");
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
        if (_mpv == IntPtr.Zero || _mpvGetPropertyDouble == null || _mpvGetPropertyFlag == null ||
            _mpvGetPropertyString == null || _mpvFree == null)
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

                // Get track codec (optional) - used to estimate waveform-extraction time.
                var codecPtr = IntPtr.Zero;
                var codecBytes = GetUtf8Bytes($"track-list/{i}/codec");
                err = _mpvGetPropertyString(_mpv, codecBytes, MPV_FORMAT_STRING, ref codecPtr);

                if (err >= 0 && codecPtr != IntPtr.Zero)
                {
                    trackInfo.Codec = Marshal.PtrToStringUTF8(codecPtr);
                    _mpvFree(codecPtr);
                }

                // Get track channel count (optional) - used for a friendly "7.1"/"5.1" label.
                double channelCount = 0;
                var channelBytes = GetUtf8Bytes($"track-list/{i}/demux-channel-count");
                err = _mpvGetPropertyDouble(_mpv, channelBytes, MPV_FORMAT_DOUBLE, ref channelCount);

                if (err >= 0 && channelCount > 0)
                {
                    trackInfo.Channels = (int)channelCount;
                }

                // Get track selected status
                var selectedValue = 0;
                var selectedBytes = GetUtf8Bytes($"track-list/{i}/selected");
                err = _mpvGetPropertyFlag(_mpv, selectedBytes, MPV_FORMAT_FLAG, ref selectedValue);

                trackInfo.IsSelected = err >= 0 && selectedValue != 0;

                // Get track default flag
                var defaultValue = 0;
                var defaultBytes = GetUtf8Bytes($"track-list/{i}/default");
                err = _mpvGetPropertyFlag(_mpv, defaultBytes, MPV_FORMAT_FLAG, ref defaultValue);

                trackInfo.IsDefault = err >= 0 && defaultValue != 0;

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
        SetStartPausedOption();

        if (_mpvInitialize == null || _mpvRenderContextCreate == null || _mpvRenderContextSetUpdateCallback == null)
        {
            throw new InvalidOperationException("MPV delegates not loaded for software rendering.");
        }

        SetYtDlpPathOption();
        SetAudioBufferOption();

        // Initialize mpv
        var err = _mpvInitialize(_mpv);
        if (err < 0)
        {
            throw new InvalidOperationException(GetErrorString(err));
        }

        MarkCoreInitialized();

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
                    *(ulong*)stridePtr = stride;
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

    /// <summary>
    /// mpv's result for the sub-* commands: 0 or higher when the command was applied, negative
    /// when it was not. Callers that push a subtitle into a player they just created (fullscreen,
    /// undock, layout rebuild) must check this and retry: the core is initialized lazily by the
    /// rendering surface, and "sub-add" is one of mpv's playback-only commands, so a push that
    /// arrives before the core is up - or before "loadfile" has actually started playback - is
    /// rejected. Ignoring that left the video with no subtitles for the rest of the session,
    /// because nothing pushes again until an edit dirties the preview (issue #13407).
    /// </summary>
    public int SubRemove()
    {
        return DoSubtitleCommand("sub-remove");
    }

    public void SetSubtitleVisibility(bool visible)
    {
        DoMpvCommand("set", "sub-visibility", visible ? "yes" : "no");
    }

    /// <inheritdoc cref="SubRemove"/>
    public int SubReload()
    {
        return DoSubtitleCommand("sub-reload");
    }

    /// <inheritdoc cref="SubRemove"/>
    public int SubAdd(string fileName)
    {
        return DoSubtitleCommand("sub-add", fileName, "select");
    }

    private int DoSubtitleCommand(params string[] args)
    {
        // DoMpvCommand answers 0 - mpv's "success" - when there is no core to talk to at all,
        // which would read as an applied subtitle. Report the same "not initialized" mpv itself
        // uses so the caller retries instead.
        if (_mpv == IntPtr.Zero || !_coreInitialized)
        {
            return MpvErrorUninitialized;
        }

        return DoMpvCommand(args);
    }

    private const int MpvErrorUninitialized = -3; // MPV_ERROR_UNINITIALIZED in mpv's client.h

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