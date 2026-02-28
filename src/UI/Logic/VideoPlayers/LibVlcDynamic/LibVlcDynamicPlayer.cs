using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// Cross-platform LibVLC video player wrapper that dynamically loads the VLC library at runtime.
/// Provides video playback functionality including play/pause control, seeking, volume adjustment,
/// playback speed control, and audio track switching. Supports Windows, Linux, and macOS platforms.
/// Implements the <see cref="IVideoPlayerInstance"/> interface for use in Subtitle Edit.
/// </summary>
/// <remarks>
/// This player uses P/Invoke to dynamically load LibVLC functions, allowing it to work without
/// requiring VLC to be installed at compile time. The library search paths can be customized
/// via the <see cref="LibVlcPath"/> property. Supports volume boost up to 130% when enabled.
/// VLC states: Playing=3, Paused=4, Ended=6.
/// </remarks>
public sealed class LibVlcDynamicPlayer : IDisposable, IVideoPlayerInstance
{
    /// <summary>
    /// Set this path (directory only) to override the default search paths.
    /// </summary>
    public static string LibVlcPath = string.Empty;

    public string PlayerSubName { get; set; } = string.Empty;
    public static int MaxVolume { get; set; } = 130;

    private IntPtr _library = IntPtr.Zero;
    private IntPtr _libVlc = IntPtr.Zero;
    private IntPtr _mediaPlayer = IntPtr.Zero;
    private string _fileName = string.Empty;
    private bool _disposed;
    private double? _pausePosition;
    private int _volume = -1;

    // LibVLC Core - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__core.html
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray)] string[] argv);
    private libvlc_new? _libvlc_new;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr libvlc_get_version();
    private libvlc_get_version? _libvlc_get_version;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_release(IntPtr libVlc);
    private libvlc_release? _libvlc_release;

    // LibVLC Media - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media.html
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr libvlc_media_new_path(IntPtr instance, byte[] input);
    private libvlc_media_new_path? _libvlc_media_new_path;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_media_release(IntPtr media);
    private libvlc_media_release? _libvlc_media_release;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_media_parse_with_options(IntPtr media, int parse_flag, int timeout);
    private libvlc_media_parse_with_options? _libvlc_media_parse_with_options;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate long libvlc_media_get_duration(IntPtr media);
    private libvlc_media_get_duration? _libvlc_media_get_duration;

    // LibVLC Video Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__video.html#g8f55326b8b51aecb59d8b8a446c3f118
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_video_get_size(IntPtr mediaPlayer, UInt32 number, out UInt32 x, out UInt32 y);
    private libvlc_video_get_size? _libvlc_video_get_size;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_video_take_snapshot(IntPtr mediaPlayer, byte num, byte[] filePath, UInt32 width, UInt32 height);
    private libvlc_video_take_snapshot? _libvlc_video_take_snapshot;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_video_set_callbacks(IntPtr playerInstance, LockCallbackDelegate @lock, UnlockCallbackDelegate unlock, DisplayCallbackDelegate display, IntPtr opaque);
    private libvlc_video_set_callbacks? _libvlc_video_set_callbacks;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_video_set_format(IntPtr mediaPlayer, string chroma, UInt32 width, UInt32 height, UInt32 pitch);
    private libvlc_video_set_format? _libvlc_video_set_format;

    // LibVLC Audio Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__audio.html
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_audio_get_volume(IntPtr mediaPlayer);
    private libvlc_audio_get_volume? _libvlc_audio_get_volume;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_audio_set_volume(IntPtr mediaPlayer, int volume);
    private libvlc_audio_set_volume? _libvlc_audio_set_volume;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_audio_get_track_count(IntPtr mediaPlayer);
    private libvlc_audio_get_track_count? _libvlc_audio_get_track_count;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr libvlc_audio_get_track_description(IntPtr mediaPlayer);
    private libvlc_audio_get_track_description? _libvlc_audio_get_track_description;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_track_description_release(IntPtr mediaPlayer);
    private libvlc_track_description_release? _libvlc_track_description_release;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_audio_get_track(IntPtr mediaPlayer);
    private libvlc_audio_get_track? _libvlc_audio_get_track;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_audio_set_track(IntPtr mediaPlayer, int trackNumber);
    private libvlc_audio_set_track? _libvlc_audio_set_track;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Int64 libvlc_audio_get_delay(IntPtr mediaPlayer);
    private libvlc_audio_get_delay? _libvlc_audio_get_delay;

    // LibVLC media player - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media__player.html
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr libvlc_media_player_new_from_media(IntPtr media);
    private libvlc_media_player_new_from_media? _libvlc_media_player_new_from_media;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_media_player_play(IntPtr mediaPlayer);
    private libvlc_media_player_play? _libvlc_media_player_play;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_media_player_stop(IntPtr mediaPlayer);
    private libvlc_media_player_stop? _libvlc_media_player_stop;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_media_player_set_hwnd(IntPtr mediaPlayer, IntPtr windowsHandle);
    private libvlc_media_player_set_hwnd? _libvlc_media_player_set_hwnd;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_media_player_set_xwindow(IntPtr mediaPlayer, IntPtr windowsHandle);
    private libvlc_media_player_set_xwindow? _libvlc_media_player_set_xwindow;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_media_player_is_playing(IntPtr mediaPlayer);
    private libvlc_media_player_is_playing? _libvlc_media_player_is_playing;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_media_player_set_pause(IntPtr mediaPlayer, int doPause);
    private libvlc_media_player_set_pause? _libvlc_media_player_set_pause;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Int64 libvlc_media_player_get_time(IntPtr mediaPlayer);
    private libvlc_media_player_get_time? _libvlc_media_player_get_time;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_media_player_set_time(IntPtr mediaPlayer, Int64 position);
    private libvlc_media_player_set_time? _libvlc_media_player_set_time;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte libvlc_media_player_get_state(IntPtr mediaPlayer);
    private libvlc_media_player_get_state? _libvlc_media_player_get_state;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Int64 libvlc_media_player_get_length(IntPtr mediaPlayer);
    private libvlc_media_player_get_length? _libvlc_media_player_get_length;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void libvlc_media_player_release(IntPtr mediaPlayer);
    private libvlc_media_player_release? _libvlc_media_player_release;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate float libvlc_media_player_get_rate(IntPtr mediaPlayer);
    private libvlc_media_player_get_rate? _libvlc_media_player_get_rate;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_media_player_set_rate(IntPtr mediaPlayer, float rate);
    private libvlc_media_player_set_rate? _libvlc_media_player_set_rate;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_media_player_next_frame(IntPtr mediaPlayer);
    private libvlc_media_player_next_frame? _libvlc_media_player_next_frame;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_media_player_add_slave(IntPtr mediaPlayer, int type, byte[] filePath, bool select);
    private libvlc_media_player_add_slave? _libvlc_media_player_add_slave;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int libvlc_video_set_spu(IntPtr mediaPlayer, int trackNumber);
    private libvlc_video_set_spu? _libvlc_video_set_spu;

    /// <summary>
    /// Callback prototype to allocate and lock a picture buffer. Whenever a new video frame needs to be decoded, the lock callback is invoked. Depending on the video chroma, one or three pixel planes of adequate dimensions must be returned via the second parameter. Those planes must be aligned on 32-bytes boundaries.
    /// </summary>
    /// <param name="opaque">Private pointer as passed to SetCallbacks()</param>
    /// <param name="planes">Planes start address of the pixel planes (LibVLC allocates the array of void pointers, this callback must initialize the array)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LockCallbackDelegate(IntPtr opaque, ref IntPtr planes);

    /// <summary>
    /// Callback prototype to unlock a picture buffer. When the video frame decoding is complete, the unlock callback is invoked. This callback might not be needed at all. It is only an indication that the application can now read the pixel values if it needs to.
    /// </summary>
    /// <param name="opaque">Private pointer as passed to SetCallbacks()</param>
    /// <param name="picture">Private pointer returned from the LockCallback callback</param>
    /// <param name="planes">Pixel planes as defined by the @ref libvlc_video_lock_cb callback (this parameter is only for convenience)</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void UnlockCallbackDelegate(IntPtr opaque, IntPtr picture, ref IntPtr planes);

    /// <summary>
    /// Callback prototype to display a picture. When the video frame needs to be shown, as determined by the media playback clock, the display callback is invoked.
    /// </summary>
    /// <param name="opaque">Private pointer as passed to SetCallbacks()</param>
    /// <param name="picture">Private pointer returned from the LockCallback callback</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DisplayCallbackDelegate(IntPtr opaque, IntPtr picture);

    private const int VLC_STATE_PAUSED = 4;
    private const int VLC_STATE_PLAYING = 3;
    private const int VLC_STATE_ENDED = 6;

    [StructLayout(LayoutKind.Sequential)]
    private struct TrackDescription
    {
        public int Id;
        public IntPtr Name;
        public IntPtr PNext;
    }

    public LibVlcDynamicPlayer()
    {
    }

    private static string[] GetLibraryNames()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ["libvlc.dll"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return ["libvlc.so"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return ["libvlc.dylib"];
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
                LibVlcPath,
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "VLC"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VideoLAN", "VLC"),
                string.Empty,
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return
            [
                LibVlcPath,
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
                "/snap/vlc/current/usr/lib",
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return
            [
                LibVlcPath,
                Directory.GetCurrentDirectory(),
                "/Applications/Subtitle Edit.app/Contents/Frameworks",
                "/Users/nikolajolsson/Library/Application Support/Subtitle Edit",
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

    private void LoadLibVlcMethods()
    {
        _libvlc_new = (libvlc_new)GetDllType(typeof(libvlc_new), "libvlc_new");
        _libvlc_get_version = (libvlc_get_version)GetDllType(typeof(libvlc_get_version), "libvlc_get_version");
        _libvlc_release = (libvlc_release)GetDllType(typeof(libvlc_release), "libvlc_release");

        _libvlc_media_new_path = (libvlc_media_new_path)GetDllType(typeof(libvlc_media_new_path), "libvlc_media_new_path");
        _libvlc_media_player_new_from_media = (libvlc_media_player_new_from_media)GetDllType(typeof(libvlc_media_player_new_from_media), "libvlc_media_player_new_from_media");
        _libvlc_media_release = (libvlc_media_release)GetDllType(typeof(libvlc_media_release), "libvlc_media_release");
        _libvlc_media_parse_with_options = (libvlc_media_parse_with_options)GetDllType(typeof(libvlc_media_parse_with_options), "libvlc_media_parse_with_options");
        _libvlc_media_get_duration = (libvlc_media_get_duration)GetDllType(typeof(libvlc_media_get_duration), "libvlc_media_get_duration");

        _libvlc_video_get_size = (libvlc_video_get_size)GetDllType(typeof(libvlc_video_get_size), "libvlc_video_get_size");
        _libvlc_video_set_spu = (libvlc_video_set_spu)GetDllType(typeof(libvlc_video_set_spu), "libvlc_video_set_spu");
        _libvlc_video_set_callbacks = (libvlc_video_set_callbacks)GetDllType(typeof(libvlc_video_set_callbacks), "libvlc_video_set_callbacks");
        _libvlc_video_set_format = (libvlc_video_set_format)GetDllType(typeof(libvlc_video_set_format), "libvlc_video_set_format");
        _libvlc_video_take_snapshot = (libvlc_video_take_snapshot)GetDllType(typeof(libvlc_video_take_snapshot), "libvlc_video_take_snapshot");

        _libvlc_audio_get_track_count = (libvlc_audio_get_track_count)GetDllType(typeof(libvlc_audio_get_track_count), "libvlc_audio_get_track_count");
        _libvlc_audio_get_track_description = (libvlc_audio_get_track_description)GetDllType(typeof(libvlc_audio_get_track_description), "libvlc_audio_get_track_description");
        _libvlc_audio_get_track = (libvlc_audio_get_track)GetDllType(typeof(libvlc_audio_get_track), "libvlc_audio_get_track");
        _libvlc_audio_set_track = (libvlc_audio_set_track)GetDllType(typeof(libvlc_audio_set_track), "libvlc_audio_set_track");
        _libvlc_audio_get_delay = (libvlc_audio_get_delay)GetDllType(typeof(libvlc_audio_get_delay), "libvlc_audio_get_delay");
        _libvlc_audio_get_volume = (libvlc_audio_get_volume)GetDllType(typeof(libvlc_audio_get_volume), "libvlc_audio_get_volume");
        _libvlc_audio_set_volume = (libvlc_audio_set_volume)GetDllType(typeof(libvlc_audio_set_volume), "libvlc_audio_set_volume");

        _libvlc_track_description_release = (libvlc_track_description_release)GetDllType(typeof(libvlc_track_description_release), "libvlc_track_description_release");
        if (_libvlc_track_description_release == null)
        { //TODO: libvlc 4 beta... check when final version is out
            _libvlc_track_description_release = (libvlc_track_description_release)GetDllType(typeof(libvlc_track_description_release), "libvlc_track_description_list_release");
        }

        _libvlc_media_player_play = (libvlc_media_player_play)GetDllType(typeof(libvlc_media_player_play), "libvlc_media_player_play");
        _libvlc_media_player_set_hwnd = (libvlc_media_player_set_hwnd)GetDllType(typeof(libvlc_media_player_set_hwnd), "libvlc_media_player_set_hwnd");
        _libvlc_media_player_set_xwindow = (libvlc_media_player_set_xwindow)GetDllType(typeof(libvlc_media_player_set_xwindow), "libvlc_media_player_set_xwindow");
        _libvlc_media_player_is_playing = (libvlc_media_player_is_playing)GetDllType(typeof(libvlc_media_player_is_playing), "libvlc_media_player_is_playing");
        _libvlc_media_player_set_pause = (libvlc_media_player_set_pause)GetDllType(typeof(libvlc_media_player_set_pause), "libvlc_media_player_set_pause");
        _libvlc_media_player_get_time = (libvlc_media_player_get_time)GetDllType(typeof(libvlc_media_player_get_time), "libvlc_media_player_get_time");
        _libvlc_media_player_set_time = (libvlc_media_player_set_time)GetDllType(typeof(libvlc_media_player_set_time), "libvlc_media_player_set_time");
        _libvlc_media_player_get_state = (libvlc_media_player_get_state)GetDllType(typeof(libvlc_media_player_get_state), "libvlc_media_player_get_state");
        _libvlc_media_player_get_length = (libvlc_media_player_get_length)GetDllType(typeof(libvlc_media_player_get_length), "libvlc_media_player_get_length");
        _libvlc_media_player_release = (libvlc_media_player_release)GetDllType(typeof(libvlc_media_player_release), "libvlc_media_player_release");
        _libvlc_media_player_get_rate = (libvlc_media_player_get_rate)GetDllType(typeof(libvlc_media_player_get_rate), "libvlc_media_player_get_rate");
        _libvlc_media_player_set_rate = (libvlc_media_player_set_rate)GetDllType(typeof(libvlc_media_player_set_rate), "libvlc_media_player_set_rate");
        _libvlc_media_player_next_frame = (libvlc_media_player_next_frame)GetDllType(typeof(libvlc_media_player_next_frame), "libvlc_media_player_next_frame");
        _libvlc_media_player_add_slave = (libvlc_media_player_add_slave)GetDllType(typeof(libvlc_media_player_add_slave), "libvlc_media_player_add_slave");
        _libvlc_media_player_stop = (libvlc_media_player_stop)GetDllType(typeof(libvlc_media_player_stop), "libvlc_media_player_stop");
        if (_libvlc_media_player_stop == null)
        { //TODO: libvlc 4 beta... check when final version is out
            _libvlc_media_player_stop = (libvlc_media_player_stop)GetDllType(typeof(libvlc_media_player_stop), "libvlc_media_player_stop_async");
        }
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
                if (string.IsNullOrWhiteSpace(libPath))
                {
                    continue;
                }

                var fullPath = Path.Combine(libPath, libName);
                System.Diagnostics.Debug.WriteLine($"Trying to load VLC from: {fullPath}");

                if (!File.Exists(fullPath))
                {
                    System.Diagnostics.Debug.WriteLine($"File not found: {fullPath}");
                    continue;
                }

                System.Diagnostics.Debug.WriteLine($"File exists, attempting to load: {fullPath}");
                var libHandle = NativeMethods.CrossLoadLibrary(fullPath);

                if (libHandle != IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded VLC from: {fullPath}");
                    _library = libHandle;
                    LoadLibVlcMethods();
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load: {fullPath}");
                }
            }
        }

        System.Diagnostics.Debug.WriteLine("Failed to load VLC from any path");
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

    private static byte[] GetUtf8Bytes(string s)
    {
        return Encoding.UTF8.GetBytes(s + "\0");
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(LibVlcDynamicPlayer));
        }
    }

    private IntPtr _windowHandle = IntPtr.Zero;

    public int Initialize()
    {
        EnsureNotDisposed();
        if (_library == IntPtr.Zero)
        {
            LoadLibraryInternal();
        }

        if (_libvlc_new == null)
        {
            return -1;
        }

        if (_libVlc == IntPtr.Zero)
        {
            string[] initParameters = { "--no-sub-autodetect-file" };
            _libVlc = _libvlc_new(initParameters.Length, initParameters);
        }

        return _libVlc != IntPtr.Zero ? 0 : -1;
    }

    public string Name
    {
        get
        {
            if (_library == IntPtr.Zero || _libvlc_get_version == null)
            {
                return "VLC";
            }

            try
            {
                var versionPointer = _libvlc_get_version();
                var libVlcVersionIncludingCodeName = Marshal.PtrToStringAnsi(versionPointer);
                return $"VLC {libVlcVersionIncludingCodeName} {PlayerSubName}".Trim();
            }
            catch
            {
                return "VLC";
            }
        }
    }

    public string FileName => _fileName;

    public async Task LoadFile(string path)
    {
        EnsureNotDisposed();

        if (_libVlc == IntPtr.Zero)
        {
            Initialize();
        }

        await Task.Run(() =>
        {
            if (_libvlc_media_new_path == null || _libvlc_media_player_new_from_media == null)
            {
                return;
            }

            var media = _libvlc_media_new_path(_libVlc, GetUtf8Bytes(path));

            // Parse media to get duration and metadata (0x00 = parse local, timeout in ms)
            try
            {
                _libvlc_media_parse_with_options?.Invoke(media, 0x00, 5000);
            }
            catch
            {
                // Ignore parsing errors, will try to get duration from player later
            }

            if (_mediaPlayer == IntPtr.Zero)
            {
                // Create new media player from this media
                _mediaPlayer = _libvlc_media_player_new_from_media(media);

                // Apply window handle if it was set before LoadFile was called
                ApplyWindowHandle();
            }
            else
            {
                // Media player already exists, just load the new media
                // Stop current playback first
                _libvlc_media_player_stop?.Invoke(_mediaPlayer);

                // Release the old media player and create a new one
                _libvlc_media_player_release?.Invoke(_mediaPlayer);
                _mediaPlayer = _libvlc_media_player_new_from_media(media);

                // Reapply window handle to the new media player
                ApplyWindowHandle();
            }

            _libvlc_media_release?.Invoke(media);

            try
            {
                _libvlc_video_set_spu?.Invoke(_mediaPlayer, -1);
            }
            catch
            {
                // Ignore
            }

            // Start playing to parse media and get duration
            _libvlc_media_player_play?.Invoke(_mediaPlayer);

            // Wait for duration to become available
            var timeout = 5000; // 5 seconds timeout
            var elapsed = 0;
            var sleepInterval = 50;

            while (elapsed < timeout)
            {
                System.Threading.Thread.Sleep(sleepInterval);
                elapsed += sleepInterval;

                var duration = _libvlc_media_player_get_length?.Invoke(_mediaPlayer) ?? 0;
                if (duration > 0)
                {
                    break;
                }
            }

            // Pause immediately after getting duration and seek to start
            _libvlc_media_player_set_pause?.Invoke(_mediaPlayer, 1);
            _libvlc_media_player_set_time?.Invoke(_mediaPlayer, 0);

            _fileName = path;
        });
    }

    public void CloseFile()
    {
        _fileName = string.Empty;
        _pausePosition = null;

        EnsureNotDisposed();
        if (_mediaPlayer == IntPtr.Zero)
        {
            return;
        }

        _libvlc_media_player_stop?.Invoke(_mediaPlayer);
    }

    public void Play()
    {
        EnsureNotDisposed();
        if (_mediaPlayer == IntPtr.Zero)
        {
            return;
        }

        _libvlc_media_player_play?.Invoke(_mediaPlayer);
        _pausePosition = null;
    }

    public void PlayOrPause()
    {
        EnsureNotDisposed();
        if (_mediaPlayer == IntPtr.Zero)
        {
            return;
        }

        if (IsPlaying)
        {
            Pause();
        }
        else
        {
            Play();
        }
    }

    public void Pause()
    {
        EnsureNotDisposed();
        if (_mediaPlayer == IntPtr.Zero)
        {
            return;
        }

        _libvlc_media_player_set_pause?.Invoke(_mediaPlayer, 1);
    }

    public void Stop()
    {
        EnsureNotDisposed();
        if (_mediaPlayer == IntPtr.Zero)
        {
            return;
        }

        _libvlc_media_player_stop?.Invoke(_mediaPlayer);
        _pausePosition = null;
    }

    public bool IsPlaying
    {
        get
        {
            if (_mediaPlayer == IntPtr.Zero || _libvlc_media_player_get_state == null)
            {
                return false;
            }

            try
            {
                int state = _libvlc_media_player_get_state(_mediaPlayer);
                return state == VLC_STATE_PLAYING;
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
            if (_mediaPlayer == IntPtr.Zero || _libvlc_media_player_get_state == null)
            {
                return true;
            }

            try
            {
                int state = _libvlc_media_player_get_state(_mediaPlayer);
                return state == VLC_STATE_PAUSED;
            }
            catch
            {
                return true;
            }
        }
    }

    public double Position
    {
        get
        {
            if (_pausePosition.HasValue && IsPaused)
            {
                return _pausePosition.Value;
            }

            EnsureNotDisposed();
            if (_mediaPlayer == IntPtr.Zero || _libvlc_media_player_get_time == null)
            {
                return 0;
            }

            try
            {
                return _libvlc_media_player_get_time(_mediaPlayer) / 1000.0;
            }
            catch
            {
                return 0;
            }
        }
        set
        {
            if (IsPaused && value <= Duration)
            {
                _pausePosition = value;
            }

            EnsureNotDisposed();
            if (_mediaPlayer == IntPtr.Zero || _libvlc_media_player_set_time == null)
            {
                return;
            }

            try
            {
                _libvlc_media_player_set_time(_mediaPlayer, (long)(value * 1000.0));
            }
            catch
            {
                // Ignore
            }
        }
    }

    public double Duration
    {
        get
        {
            EnsureNotDisposed();
            if (_mediaPlayer == IntPtr.Zero || _libvlc_media_player_get_length == null)
            {
                return 0;
            }

            try
            {
                return _libvlc_media_player_get_length(_mediaPlayer) / 1000.0;
            }
            catch
            {
                return 0;
            }
        }
    }

    public int VolumeMaximum => MaxVolume;

    public double Volume
    {
        get
        {
            if (_volume != -1)
            {
                return _volume;
            }

            EnsureNotDisposed();
            if (_mediaPlayer == IntPtr.Zero || _libvlc_audio_get_volume == null)
            {
                return 100;
            }

            try
            {
                var v = _libvlc_audio_get_volume(_mediaPlayer);
                if (MaxVolume > 100)
                {
                    var result = (int)Math.Round(v / (MaxVolume / 100.0));
                    return result > 100 ? 100 : result;
                }
                return v > 100 ? 100 : v;
            }
            catch
            {
                return 100;
            }
        }
        set
        {
            _volume = (int)value;
            EnsureNotDisposed();
            if (_mediaPlayer == IntPtr.Zero || _libvlc_audio_set_volume == null)
            {
                return;
            }

            try
            {
                var clampedVolume = Math.Max(0, Math.Min(100, value));
                if (MaxVolume > 100)
                {
                    _libvlc_audio_set_volume(_mediaPlayer, (int)(clampedVolume * (MaxVolume / 100.0)));
                }
                else
                {
                    _libvlc_audio_set_volume(_mediaPlayer, (int)clampedVolume);
                }
            }
            catch
            {
                // Ignore
            }
        }
    }

    public double Speed
    {
        get
        {
            EnsureNotDisposed();
            if (_mediaPlayer == IntPtr.Zero || _libvlc_media_player_get_rate == null)
            {
                return 1.0;
            }

            try
            {
                return _libvlc_media_player_get_rate(_mediaPlayer);
            }
            catch
            {
                return 1.0;
            }
        }
        set
        {
            EnsureNotDisposed();
            if (_mediaPlayer == IntPtr.Zero || _libvlc_media_player_set_rate == null)
            {
                return;
            }

            try
            {
                var clampedSpeed = Math.Max(0.25, Math.Min(4.0, value));
                _libvlc_media_player_set_rate(_mediaPlayer, (float)clampedSpeed);
            }
            catch
            {
                // Ignore
            }
        }
    }

    public AudioTrackInfo? ToggleAudioTrack()
    {
        EnsureNotDisposed();
        if (_mediaPlayer == IntPtr.Zero || _libvlc_audio_get_track_count == null ||
            _libvlc_audio_get_track_description == null || _libvlc_audio_get_track == null ||
            _libvlc_audio_set_track == null || _libvlc_track_description_release == null)
        {
            return null;
        }

        try
        {
            var trackDescriptionsPointer = _libvlc_audio_get_track_description(_mediaPlayer);
            var audioTracks = new List<(int id, string name)>();
            var trackDescriptionPointer = trackDescriptionsPointer;

            while (trackDescriptionPointer != IntPtr.Zero)
            {
                var trackDescription = Marshal.PtrToStructure<TrackDescription>(trackDescriptionPointer);
                var name = Marshal.PtrToStringAnsi(trackDescription.Name) ?? string.Empty;
                if (trackDescription.Id != -1)
                {
                    audioTracks.Add((trackDescription.Id, name));
                }
                trackDescriptionPointer = trackDescription.PNext;
            }

            if (trackDescriptionsPointer != IntPtr.Zero)
            {
                _libvlc_track_description_release(trackDescriptionsPointer);
            }

            if (audioTracks.Count == 0)
            {
                return null;
            }

            var currentTrack = _libvlc_audio_get_track(_mediaPlayer);
            var currentIdx = audioTracks.FindIndex(t => t.id == currentTrack);
            var nextIdx = currentIdx >= 0 ? (currentIdx + 1) % audioTracks.Count : 0;
            var next = audioTracks[nextIdx];

            _libvlc_audio_set_track(_mediaPlayer, next.id);
            return new AudioTrackInfo()
            { 
                FfIndex = next.id,
                Id = next.id,
                Title = next.name,
            };
        }
        catch
        {
            return null;
        }
    }

    public void SetWindowHandle(IntPtr windowHandle)
    {
        EnsureNotDisposed();

        // Store the window handle
        _windowHandle = windowHandle;

        // If media player already exists, set it now
        if (_mediaPlayer != IntPtr.Zero)
        {
            ApplyWindowHandle();
        }
    }

    private void ApplyWindowHandle()
    {
        if (_windowHandle == IntPtr.Zero || _mediaPlayer == IntPtr.Zero)
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _libvlc_media_player_set_hwnd?.Invoke(_mediaPlayer, _windowHandle);
            System.Diagnostics.Debug.WriteLine($"Set HWND: {_windowHandle}");
        }
        else
        {
            _libvlc_media_player_set_xwindow?.Invoke(_mediaPlayer, _windowHandle);
            System.Diagnostics.Debug.WriteLine($"Set XWindow: {_windowHandle}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            if (_mediaPlayer != IntPtr.Zero)
            {
                _libvlc_media_player_stop?.Invoke(_mediaPlayer);
                _libvlc_media_player_release?.Invoke(_mediaPlayer);
                _mediaPlayer = IntPtr.Zero;
            }

            if (_libVlc != IntPtr.Zero && _libvlc_release != null)
            {
                _libvlc_release(_libVlc);
                _libVlc = IntPtr.Zero;
            }

            if (_library != IntPtr.Zero)
            {
                NativeMethods.CrossFreeLibrary(_library);
                _library = IntPtr.Zero;
            }
        }
        catch
        {
            // Ignore
        }
    }
}