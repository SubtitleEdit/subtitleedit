using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class LibVlc11xDynamic : VideoPlayer
    {
        private System.Windows.Forms.Timer _videoLoadedTimer;
        private System.Windows.Forms.Timer _videoEndTimer;
        private System.Windows.Forms.Timer _mouseTimer;

        private IntPtr _libVlcDLL;
        private IntPtr _libVlc;
        private IntPtr _mediaPlayer;
        private System.Windows.Forms.Control _ownerControl;
        private System.Windows.Forms.Form _parentForm;
        private double? _pausePosition = null; // Hack to hold precise seeking when paused

        [DllImport("user32")]
        private static extern short GetKeyState(int vKey);

        // Win32 API functions for loading dlls dynamic
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);


        // LibVLC Core - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__core.html
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray)] string[] argv);
        libvlc_new _libvlc_new;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr libvlc_get_version();
        libvlc_get_version _libvlc_get_version;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_release(IntPtr libVlc);
        libvlc_release _libvlc_release;


        // LibVLC Media - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media.html
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr libvlc_media_new_path(IntPtr instance, byte[] input);
        libvlc_media_new_path _libvlc_media_new_path;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr libvlc_media_player_new_from_media(IntPtr media);
        libvlc_media_player_new_from_media _libvlc_media_player_new_from_media;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_media_release(IntPtr media);
        libvlc_media_release _libvlc_media_release;


        // LibVLC Video Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__video.html#g8f55326b8b51aecb59d8b8a446c3f118
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_video_get_size(IntPtr mediaPlayer, UInt32 number, out UInt32 x, out UInt32 y);
        libvlc_video_get_size _libvlc_video_get_size;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_audio_get_track_count(IntPtr mediaPlayer);
        libvlc_audio_get_track_count _libvlc_audio_get_track_count;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_audio_get_track(IntPtr mediaPlayer);
        libvlc_audio_get_track _libvlc_audio_get_track;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_audio_set_track(IntPtr mediaPlayer, int trackNumber);
        libvlc_audio_set_track _libvlc_audio_set_track;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_video_take_snapshot(IntPtr mediaPlayer, byte num, byte[] filePath, UInt32 width, UInt32 height);
        libvlc_video_take_snapshot _libvlc_video_take_snapshot;


        // LibVLC Audio Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__audio.html
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_audio_get_volume(IntPtr mediaPlayer);
        libvlc_audio_get_volume _libvlc_audio_get_volume;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_audio_set_volume(IntPtr mediaPlayer, int volume);
        libvlc_audio_set_volume _libvlc_audio_set_volume;


        // LibVLC Media Player - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media__player.html
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_media_player_play(IntPtr mediaPlayer);
        libvlc_media_player_play _libvlc_media_player_play;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_media_player_stop(IntPtr mediaPlayer);
        libvlc_media_player_stop _libvlc_media_player_stop;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_media_player_pause(IntPtr mediaPlayer);
        libvlc_media_player_pause _libvlc_media_player_pause;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_media_player_set_hwnd(IntPtr mediaPlayer, IntPtr windowsHandle);
        libvlc_media_player_set_hwnd _libvlc_media_player_set_hwnd;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_media_player_is_playing(IntPtr mediaPlayer);
        libvlc_media_player_is_playing _libvlc_media_player_is_playing;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate Int64 libvlc_media_player_get_time(IntPtr mediaPlayer);
        libvlc_media_player_get_time _libvlc_media_player_get_time;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_media_player_set_time(IntPtr mediaPlayer, Int64 position);
        libvlc_media_player_set_time _libvlc_media_player_set_time;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float libvlc_media_player_get_fps(IntPtr mediaPlayer);
        libvlc_media_player_get_fps _libvlc_media_player_get_fps;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte libvlc_media_player_get_state(IntPtr mediaPlayer);
        libvlc_media_player_get_state _libvlc_media_player_get_state;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate Int64 libvlc_media_player_get_length(IntPtr mediaPlayer);
        libvlc_media_player_get_length _libvlc_media_player_get_length;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_media_list_player_release(IntPtr mediaPlayer);
        libvlc_media_list_player_release _libvlc_media_list_player_release;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float libvlc_media_player_get_rate(IntPtr mediaPlayer);
        libvlc_media_player_get_rate _libvlc_media_player_get_rate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_media_player_set_rate(IntPtr mediaPlayer, float rate);
        libvlc_media_player_set_rate _libvlc_media_player_set_rate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_media_player_next_frame(IntPtr mediaPlayer);
        libvlc_media_player_next_frame _libvlc_media_player_next_frame;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void libvlc_video_set_callbacks(IntPtr playerInstance, LockCallbackDelegate @lock, UnlockCallbackDelegate unlock, DisplayCallbackDelegate display, IntPtr opaque);
        libvlc_video_set_callbacks _libvlc_video_set_callbacks;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int libvlc_video_set_format(IntPtr mediaPlayer, string chroma, UInt32 width, UInt32 height, UInt32 pitch);
        libvlc_video_set_format _libvlc_video_set_format;



        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public unsafe delegate void* LockEventHandler(void* opaque, void** plane);

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public unsafe delegate void UnlockEventHandler(void* opaque, void* picture, void** plane);


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


        private object GetDllType(Type type, string name)
        {
            IntPtr address = GetProcAddress(_libVlcDLL, name);
            if (address != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer(address, type);
            }
            return null;
        }

        private void LoadLibVlcDynamic()
        {
            _libvlc_new = (libvlc_new)GetDllType(typeof(libvlc_new), "libvlc_new");
            _libvlc_get_version = (libvlc_get_version)GetDllType(typeof(libvlc_get_version), "libvlc_get_version");
            _libvlc_release = (libvlc_release)GetDllType(typeof(libvlc_release), "libvlc_release");

            _libvlc_media_new_path = (libvlc_media_new_path)GetDllType(typeof(libvlc_media_new_path), "libvlc_media_new_path");
            _libvlc_media_player_new_from_media = (libvlc_media_player_new_from_media)GetDllType(typeof(libvlc_media_player_new_from_media), "libvlc_media_player_new_from_media");
            _libvlc_media_release = (libvlc_media_release)GetDllType(typeof(libvlc_media_release), "libvlc_media_release");

            _libvlc_video_get_size = (libvlc_video_get_size)GetDllType(typeof(libvlc_video_get_size), "libvlc_video_get_size");
            _libvlc_audio_get_track_count = (libvlc_audio_get_track_count)GetDllType(typeof(libvlc_audio_get_track_count), "libvlc_audio_get_track_count");
            _libvlc_audio_get_track = (libvlc_audio_get_track)GetDllType(typeof(libvlc_audio_get_track), "libvlc_audio_get_track");
            _libvlc_audio_set_track = (libvlc_audio_set_track)GetDllType(typeof(libvlc_audio_set_track), "libvlc_audio_set_track");
            _libvlc_video_take_snapshot = (libvlc_video_take_snapshot)GetDllType(typeof(libvlc_video_take_snapshot), "libvlc_video_take_snapshot");

            _libvlc_audio_get_volume = (libvlc_audio_get_volume)GetDllType(typeof(libvlc_audio_get_volume), "libvlc_audio_get_volume");
            _libvlc_audio_set_volume = (libvlc_audio_set_volume)GetDllType(typeof(libvlc_audio_set_volume), "libvlc_audio_set_volume");

            _libvlc_media_player_play = (libvlc_media_player_play)GetDllType(typeof(libvlc_media_player_play), "libvlc_media_player_play");
            _libvlc_media_player_stop = (libvlc_media_player_stop)GetDllType(typeof(libvlc_media_player_stop), "libvlc_media_player_stop");
            _libvlc_media_player_pause = (libvlc_media_player_pause)GetDllType(typeof(libvlc_media_player_pause), "libvlc_media_player_pause");
            _libvlc_media_player_set_hwnd = (libvlc_media_player_set_hwnd)GetDllType(typeof(libvlc_media_player_set_hwnd), "libvlc_media_player_set_hwnd");
            _libvlc_media_player_is_playing = (libvlc_media_player_is_playing)GetDllType(typeof(libvlc_media_player_is_playing), "libvlc_media_player_is_playing");
            _libvlc_media_player_get_time = (libvlc_media_player_get_time)GetDllType(typeof(libvlc_media_player_get_time), "libvlc_media_player_get_time");
            _libvlc_media_player_set_time = (libvlc_media_player_set_time)GetDllType(typeof(libvlc_media_player_set_time), "libvlc_media_player_set_time");
            _libvlc_media_player_get_fps = (libvlc_media_player_get_fps)GetDllType(typeof(libvlc_media_player_get_fps), "libvlc_media_player_get_fps");
            _libvlc_media_player_get_state = (libvlc_media_player_get_state)GetDllType(typeof(libvlc_media_player_get_state), "libvlc_media_player_get_state");
            _libvlc_media_player_get_length = (libvlc_media_player_get_length)GetDllType(typeof(libvlc_media_player_get_length), "libvlc_media_player_get_length");
            _libvlc_media_list_player_release = (libvlc_media_list_player_release)GetDllType(typeof(libvlc_media_list_player_release), "libvlc_media_list_player_release");
            _libvlc_media_player_get_rate = (libvlc_media_player_get_rate)GetDllType(typeof(libvlc_media_player_get_rate), "libvlc_media_player_get_rate");
            _libvlc_media_player_set_rate = (libvlc_media_player_set_rate)GetDllType(typeof(libvlc_media_player_set_rate), "libvlc_media_player_set_rate");
            _libvlc_media_player_next_frame = (libvlc_media_player_next_frame)GetDllType(typeof(libvlc_media_player_next_frame), "libvlc_media_player_next_frame");
            _libvlc_video_set_callbacks = (libvlc_video_set_callbacks)GetDllType(typeof(libvlc_video_set_callbacks), "libvlc_video_set_callbacks");
            _libvlc_video_set_format = (libvlc_video_set_format)GetDllType(typeof(libvlc_video_set_format), "libvlc_video_set_format");
        }

        private bool IsAllMethodsLoaded()
        {
            return _libvlc_new != null &&
                   _libvlc_get_version != null &&
                   _libvlc_release != null &&
                   _libvlc_media_new_path != null &&
                   _libvlc_media_player_new_from_media != null &&
                   _libvlc_media_release != null &&
                   _libvlc_video_get_size != null &&
                   _libvlc_audio_get_volume != null &&
                   _libvlc_audio_set_volume != null &&
                   _libvlc_media_player_play != null &&
                   _libvlc_media_player_stop != null &&
                   _libvlc_media_player_pause != null &&
                   _libvlc_media_player_set_hwnd != null &&
                   _libvlc_media_player_is_playing != null &&
                   _libvlc_media_player_get_time != null &&
                   _libvlc_media_player_set_time != null &&
                   _libvlc_media_player_get_fps != null &&
                   _libvlc_media_player_get_state != null &&
                   _libvlc_media_player_get_length != null &&
                   _libvlc_media_list_player_release != null &&
                   _libvlc_media_player_get_rate != null &&
                   _libvlc_media_player_set_rate != null;
        }

        public static bool IsInstalled
        {
            get
            {
                LibVlc11xDynamic vlc = new LibVlc11xDynamic();
                try
                {
                    vlc.Initialize(null, null, null, null);
                    return vlc.IsAllMethodsLoaded();
                }
                finally
                {
                    vlc.DisposeVideoPlayer();
                }
            }
        }

        public override string PlayerName
        {
            get { return "VLC Lib Dynamic"; }
        }

        public override int Volume
        {
            get
            {
                return _libvlc_audio_get_volume(_mediaPlayer);
            }
            set
            {
                _libvlc_audio_set_volume(_mediaPlayer, value);
            }
        }

        public override double Duration
        {
            get
            {
                return _libvlc_media_player_get_length(_mediaPlayer) / 1000.0;
            }
        }

        public override double CurrentPosition
        {
            get
            {
                if (_pausePosition != null)
                {
                    if (_pausePosition < 0)
                        return 0;
                    return _pausePosition.Value;
                }
                return _libvlc_media_player_get_time(_mediaPlayer) / 1000.0;
            }
            set
            {
                if (IsPaused && value <= Duration)
                    _pausePosition = value;
                _libvlc_media_player_set_time(_mediaPlayer, (long)(value * 1000.0 + 0.5));
            }
        }

        public override double PlayRate
        {
            get
            {
                return _libvlc_media_player_get_rate(_mediaPlayer);
            }
            set
            {
                if (value >= 0 && value <= 2.0)
                    _libvlc_media_player_set_rate(_mediaPlayer, (float)value);
            }
        }

        public void GetNextFrame()
        {
            _libvlc_media_player_next_frame(_mediaPlayer);
        }

        public int VlcState
        {
            get
            {
                return _libvlc_media_player_get_state(_mediaPlayer);
            }
        }


        public override void Play()
        {
            _libvlc_media_player_play(_mediaPlayer);
            _pausePosition = null;
        }

        public override void Pause()
        {
            if (!IsPaused)
                _libvlc_media_player_pause(_mediaPlayer);
        }

        public override void Stop()
        {
            _libvlc_media_player_stop(_mediaPlayer);
            _pausePosition = null;
        }

        public override bool IsPaused
        {
            get
            {
                const int Paused = 4;
                int state = _libvlc_media_player_get_state(_mediaPlayer);
                return state == Paused;
            }
        }

        public override bool IsPlaying
        {
            get
            {
                const int Playing = 3;
                int state = _libvlc_media_player_get_state(_mediaPlayer);
                return state == Playing;
            }
        }

        public int AudioTrackCount
        {
            get
            {
                return _libvlc_audio_get_track_count(_mediaPlayer)-1;
            }
        }


        public int AudioTrackNumber
        {
            get
            {
                return _libvlc_audio_get_track(_mediaPlayer)-1;
            }
            set
            {
                _libvlc_audio_set_track(_mediaPlayer, value+1);
            }
        }

        public bool TakeSnapshot(string fileName, UInt32 width, UInt32 height)
        {
            if (_libvlc_video_take_snapshot == null)
                return false;

            return _libvlc_video_take_snapshot(_mediaPlayer, 0, Encoding.UTF8.GetBytes(fileName + "\0"), width, height) == 1;
        }

        public LibVlc11xDynamic MakeSecondMediaPlayer(System.Windows.Forms.Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            LibVlc11xDynamic newVlc = new LibVlc11xDynamic();
            newVlc._libVlc = this._libVlc;
            newVlc._libVlcDLL = this._libVlcDLL;
            newVlc._ownerControl = ownerControl;
            if (ownerControl != null)
                newVlc._parentForm = ownerControl.FindForm();

            newVlc.LoadLibVlcDynamic();

            newVlc.OnVideoLoaded = onVideoLoaded;
            newVlc.OnVideoEnded = onVideoEnded;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                IntPtr media = _libvlc_media_new_path(_libVlc, Encoding.UTF8.GetBytes(videoFileName + "\0"));
                newVlc._mediaPlayer = _libvlc_media_player_new_from_media(media);
                _libvlc_media_release(media);

                //  Linux: libvlc_media_player_set_xdrawable (_mediaPlayer, xdrawable);
                //  Mac: libvlc_media_player_set_nsobject (_mediaPlayer, view);
                _libvlc_media_player_set_hwnd(newVlc._mediaPlayer, ownerControl.Handle); // windows

                if (onVideoEnded != null)
                {
                    newVlc._videoEndTimer = new System.Windows.Forms.Timer { Interval = 500 };
                    newVlc._videoEndTimer.Tick += VideoEndTimerTick;
                    newVlc._videoEndTimer.Start();
                }

                _libvlc_media_player_play(newVlc._mediaPlayer);
                newVlc._videoLoadedTimer = new System.Windows.Forms.Timer { Interval = 500 };
                newVlc._videoLoadedTimer.Tick += new EventHandler(newVlc.VideoLoadedTimer_Tick);
                newVlc._videoLoadedTimer.Start();

                newVlc._mouseTimer = new System.Windows.Forms.Timer { Interval = 25 };
                newVlc._mouseTimer.Tick += newVlc.MouseTimerTick;
                newVlc._mouseTimer.Start();
            }
            return newVlc;
        }

        void VideoLoadedTimer_Tick(object sender, EventArgs e)
        {
            int i = 0;
            while (!IsPlaying && i < 50)
            {
                System.Threading.Thread.Sleep(100);
                i++;
            }
            _libvlc_media_player_pause(_mediaPlayer);
            _videoLoadedTimer.Stop();

            if (OnVideoLoaded != null)
                OnVideoLoaded.Invoke(_mediaPlayer, new EventArgs());
        }

        public static string GetVlcPath(string fileName)
        {
            if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
                return null;

            string path;

            path = Path.Combine(Configuration.BaseDirectory, @"VLC\" + fileName);
            if (File.Exists(path))
                return path;

            if (!string.IsNullOrEmpty(Configuration.Settings.General.VlcLocation))
            {
                path = Path.Combine(Configuration.Settings.General.VlcLocation, fileName);
                if (File.Exists(path))
                    return path;
            }

            // XP via registry path
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC");
            if (key != null)
            {
                path = (string)key.GetValue("InstallDir");
                if (path != null && Directory.Exists(path))
                    path = Path.Combine(path, fileName);
                if (File.Exists(path))
                    return path;
            }

            // Winows 7 via registry path
            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\VideoLAN\VLC");
            if (key != null)
            {
                path = (string)key.GetValue("InstallDir");
                if (path != null && Directory.Exists(path))
                    path = Path.Combine(path, fileName);
                if (File.Exists(path))
                    return path;
            }

            path = Path.Combine(@"C:\Program Files (x86)\VideoLAN\VLC", fileName);
            if (File.Exists(path))
                return path;

            path = Path.Combine(@"C:\Program Files\VideoLAN\VLC", fileName);
            if (File.Exists(path))
                return path;

            path = Path.Combine(@"C:\Program Files (x86)\VLC", fileName);
            if (File.Exists(path))
                return path;

            path = Path.Combine(@"C:\Program Files\VLC", fileName);
            if (File.Exists(path))
                return path;

            path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"VideoLAN\VLC\" + fileName);
            if (File.Exists(path))
                return path;

            path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"VLC\" + fileName);
            if (File.Exists(path))
                return path;

            return null;
        }

        public bool InitializeAndStartFrameGrabbing(string videoFileName,
                                                    UInt32 width, UInt32 height,
                                                    LockCallbackDelegate @lock,
                                                    UnlockCallbackDelegate unlock,
                                                    DisplayCallbackDelegate display,
                                                    IntPtr opaque)
        {
            string dllFile = GetVlcPath("libvlc.dll");
            if (!File.Exists(dllFile) || string.IsNullOrEmpty(videoFileName))
                return false;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(dllFile));
            _libVlcDLL = LoadLibrary(dllFile);
            LoadLibVlcDynamic();
            string[] initParameters = new string[] { "--no-skip-frames" };
            _libVlc = _libvlc_new(initParameters.Length, initParameters);
            IntPtr media = _libvlc_media_new_path(_libVlc, Encoding.UTF8.GetBytes(videoFileName + "\0"));
            _mediaPlayer = _libvlc_media_player_new_from_media(media);
            _libvlc_media_release(media);

            _libvlc_video_set_format(_mediaPlayer, "RV24", width, height, 3 * width);
//            _libvlc_video_set_format(_mediaPlayer,"RV32", width, height, 4 * width);

            //_libvlc_video_set_callbacks(_mediaPlayer, @lock, unlock, display, opaque);
            _libvlc_video_set_callbacks(_mediaPlayer, @lock, unlock, display, opaque);
            _libvlc_audio_set_volume(_mediaPlayer, 0);
            _libvlc_media_player_set_rate(_mediaPlayer, 9f);
            //_libvlc_media_player_play(_mediaPlayer);
            return true;
        }

        public override void Initialize(System.Windows.Forms.Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            _ownerControl = ownerControl;
            if (ownerControl != null)
                _parentForm = ownerControl.FindForm();
            string dllFile = GetVlcPath("libvlc.dll");
            if (File.Exists(dllFile))
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(dllFile));
                _libVlcDLL = LoadLibrary(dllFile);
                LoadLibVlcDynamic();
            }
            else if (!Directory.Exists(videoFileName))
            {
                return;
            }

            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                string[] initParameters = new string[] { "--no-sub-autodetect-file" }; //, "--no-video-title-show" }; //TODO: Put in options/config file
                _libVlc = _libvlc_new(initParameters.Length, initParameters);
                IntPtr media = _libvlc_media_new_path(_libVlc, Encoding.UTF8.GetBytes(videoFileName + "\0"));
                _mediaPlayer = _libvlc_media_player_new_from_media(media);
                _libvlc_media_release(media);


                //  Linux: libvlc_media_player_set_xdrawable (_mediaPlayer, xdrawable);
                //  Mac: libvlc_media_player_set_nsobject (_mediaPlayer, view);

                _libvlc_media_player_set_hwnd(_mediaPlayer, ownerControl.Handle); // windows

                //hack: sometimes vlc opens in it's own windows - this code seems to prevent this
                for (int j = 0; j < 50; j++)
                {
                    System.Threading.Thread.Sleep(10);
                    System.Windows.Forms.Application.DoEvents();
                }
                _libvlc_media_player_set_hwnd(_mediaPlayer, ownerControl.Handle); // windows

                if (onVideoEnded != null)
                {
                    _videoEndTimer = new System.Windows.Forms.Timer { Interval = 500 };
                    _videoEndTimer.Tick += VideoEndTimerTick;
                    _videoEndTimer.Start();
                }

                _libvlc_media_player_play(_mediaPlayer);
                _videoLoadedTimer = new System.Windows.Forms.Timer { Interval = 500 };
                _videoLoadedTimer.Tick += new EventHandler(VideoLoadedTimer_Tick);
                _videoLoadedTimer.Start();

                _mouseTimer = new System.Windows.Forms.Timer { Interval = 25 };
                _mouseTimer.Tick += MouseTimerTick;
                _mouseTimer.Start();
            }
        }

        public static bool IsLeftMouseButtonDown()
        {
            const int KEY_PRESSED = 0x8000;
            const int VK_LBUTTON = 0x1;
            return Convert.ToBoolean(GetKeyState(VK_LBUTTON) & KEY_PRESSED);
        }

        void MouseTimerTick(object sender, EventArgs e)
        {
            _mouseTimer.Stop();
            if (_parentForm != null && _parentForm.ContainsFocus && IsLeftMouseButtonDown())
            {
                var p = _ownerControl.PointToClient(System.Windows.Forms.Control.MousePosition);
                if (p.X > 0 && p.X < _ownerControl.Width && p.Y > 0 && p.Y < _ownerControl.Height)
                {
                    if (IsPlaying)
                        Pause();
                    else
                        Play();
                    int i = 0;
                    while (IsLeftMouseButtonDown() && i < 200)
                    {
                        System.Threading.Thread.Sleep(2);
                        System.Windows.Forms.Application.DoEvents();
                        i++;
                    }
                }
            }
            _mouseTimer.Start();
        }

        void VideoEndTimerTick(object sender, EventArgs e)
        {
            const int Ended = 6;
            int state = _libvlc_media_player_get_state(_mediaPlayer);
            if (state == Ended)
            {
                // hack to make sure VLC is in ready state
                Stop();
                Play();
                Pause();
                OnVideoEnded.Invoke(_mediaPlayer, new EventArgs());
            }
        }

        public override void DisposeVideoPlayer()
        {
            if (_videoLoadedTimer != null)
                _videoLoadedTimer.Stop();

            if (_videoEndTimer != null)
                _videoEndTimer.Stop();

            if (_mouseTimer != null)
                _mouseTimer.Stop();

            ThreadPool.QueueUserWorkItem(DisposeVLC, this);
        }

        private void DisposeVLC(object player)
        {
            try
            {
                if (_mediaPlayer != IntPtr.Zero)
                {
                    _libvlc_media_player_stop(_mediaPlayer);
                    _libvlc_media_list_player_release(_mediaPlayer);
                }

                if (_libvlc_release != null && _libVlc != IntPtr.Zero)
                    _libvlc_release(_libVlc);

                if (_libVlcDLL != IntPtr.Zero)
                    FreeLibrary(_libVlcDLL);
            }
            catch
            {
            }
        }

        public override event EventHandler OnVideoLoaded;

        public override event EventHandler OnVideoEnded;

    }
}
