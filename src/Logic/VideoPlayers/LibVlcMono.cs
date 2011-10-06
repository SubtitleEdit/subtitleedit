using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    class LibVlcMono : VideoPlayer
    {
        private System.Windows.Forms.Timer _videoLoadedTimer;
        private System.Windows.Forms.Timer _videoEndTimer;

        private IntPtr _libVlcDLL;
        private IntPtr _libVlc;
        private IntPtr _mediaPlayer;
        private System.Windows.Forms.Control _ownerControl;
        private System.Windows.Forms.Form _parentForm;

        // LibVLC Core - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__core.html
        [DllImport("libvlc")]
        private static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray)] string[] argv);

        [DllImport("libvlc")]
        private static extern IntPtr libvlc_get_version();

        [DllImport("libvlc")]
        private static extern void libvlc_release(IntPtr libVlc);

        // LibVLC Media - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media.html
        [DllImport("libvlc")]
        private static extern IntPtr libvlc_media_new_path(IntPtr instance, byte[] input);

        [DllImport("libvlc")]
        private static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

        [DllImport("libvlc")]
        private static extern void libvlc_media_release(IntPtr media);


        // LibVLC Video Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__video.html#g8f55326b8b51aecb59d8b8a446c3f118
        [DllImport("libvlc")]
        private static extern void libvlc_video_get_size(IntPtr mediaPlayer, UInt32 number, out UInt32 x, out UInt32 y);

        [DllImport("libvlc")]
        private static extern int libvlc_audio_get_track_count(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern int libvlc_audio_get_track(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern int libvlc_audio_set_track(IntPtr mediaPlayer, int trackNumber);

        // LibVLC Audio Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__audio.html
        [DllImport("libvlc")]
        private static extern int libvlc_audio_get_volume(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern void libvlc_audio_set_volume(IntPtr mediaPlayer, int volume);


        // LibVLC Media Player - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media__player.html
        [DllImport("libvlc")]
        private static extern void libvlc_media_player_play(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern void libvlc_media_player_stop(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern void libvlc_media_player_pause(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern void libvlc_media_player_set_hwnd(IntPtr mediaPlayer, IntPtr windowsHandle);

        [DllImport("libvlc")]
        private static extern int libvlc_media_player_is_playing(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern Int64 libvlc_media_player_get_time(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern void libvlc_media_player_set_time(IntPtr mediaPlayer, Int64 position);

        [DllImport("libvlc")]
        private static extern float libvlc_media_player_get_fps(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern byte libvlc_media_player_get_state(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern Int64 libvlc_media_player_get_length(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern void libvlc_media_list_player_release(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern float libvlc_media_player_get_rate(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        private static extern int libvlc_media_player_set_rate(IntPtr mediaPlayer, float rate);

        private static byte[] StringToCharPointer(string s)
        {
            return Encoding.UTF8.GetBytes(s + "\0");
        }

        public override string PlayerName
        {
            get { return "VLC Lib Mono"; }
        }

        public override int Volume
        {
            get
            {
                return libvlc_audio_get_volume(_mediaPlayer);
            }
            set
            {
                libvlc_audio_set_volume(_mediaPlayer, value);
            }
        }

        public override double Duration
        {
            get
            {
                return libvlc_media_player_get_length(_mediaPlayer) / 1000.0;
            }
        }

        public override double CurrentPosition
        {
            get
            {
                return libvlc_media_player_get_time(_mediaPlayer) / 1000.0;
            }
            set
            {
                libvlc_media_player_set_time(_mediaPlayer, (long)(value * 1000.0));
            }
        }

        public override double PlayRate
        {
            get
            {
                return libvlc_media_player_get_rate(_mediaPlayer);
            }
            set
            {
                if (value >= 0 && value <= 2.0)
                    libvlc_media_player_set_rate(_mediaPlayer, (float)value);
            }
        }

        public override void Play()
        {
            libvlc_media_player_play(_mediaPlayer);
        }

        public override void Pause()
        {
            if (!IsPaused)
                libvlc_media_player_pause(_mediaPlayer);
        }

        public override void Stop()
        {
            libvlc_media_player_stop(_mediaPlayer);
        }

        public override bool IsPaused
        {
            get
            {
                const int Paused = 4;
                int state = libvlc_media_player_get_state(_mediaPlayer);
                return state == Paused;
            }
        }

        public override bool IsPlaying
        {
            get
            {
                const int Playing = 3;
                int state = libvlc_media_player_get_state(_mediaPlayer);
                return state == Playing;
            }
        }

        public int AudioTrackCount
        {
            get
            {
                return libvlc_audio_get_track_count(_mediaPlayer) - 1;
            }
        }


        public int AudioTrackNumber
        {
            get
            {
                return libvlc_audio_get_track(_mediaPlayer) - 1;
            }
            set
            {
                libvlc_audio_set_track(_mediaPlayer, value + 1);
            }
        }

        public LibVlcMono MakeSecondMediaPlayer(System.Windows.Forms.Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            LibVlcMono newVlc = new LibVlcMono();
            newVlc._libVlc = this._libVlc;
            newVlc._libVlcDLL = this._libVlcDLL;
            newVlc._ownerControl = ownerControl;
            if (ownerControl != null)
                newVlc._parentForm = ownerControl.FindForm();

            newVlc.OnVideoLoaded = onVideoLoaded;
            newVlc.OnVideoEnded = onVideoEnded;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                IntPtr media = libvlc_media_new_path(_libVlc, Encoding.UTF8.GetBytes(videoFileName + "\0"));
                newVlc._mediaPlayer = libvlc_media_player_new_from_media(media);
                libvlc_media_release(media);

                //  Linux: libvlc_media_player_set_xdrawable (_mediaPlayer, xdrawable);
                //  Mac: libvlc_media_player_set_nsobject (_mediaPlayer, view);
                libvlc_media_player_set_hwnd(newVlc._mediaPlayer, ownerControl.Handle); // windows

                if (onVideoEnded != null)
                {
                    newVlc._videoEndTimer = new System.Windows.Forms.Timer { Interval = 500 };
                    newVlc._videoEndTimer.Tick += VideoEndTimerTick;
                    newVlc._videoEndTimer.Start();
                }

                libvlc_media_player_play(newVlc._mediaPlayer);
                newVlc._videoLoadedTimer = new System.Windows.Forms.Timer { Interval = 500 };
                newVlc._videoLoadedTimer.Tick += new EventHandler(newVlc.VideoLoadedTimer_Tick);
                newVlc._videoLoadedTimer.Start();
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
            libvlc_media_player_pause(_mediaPlayer);
            _videoLoadedTimer.Stop();

            if (OnVideoLoaded != null)
                OnVideoLoaded.Invoke(_mediaPlayer, new EventArgs());
        }


        public override void Initialize(System.Windows.Forms.Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            _ownerControl = ownerControl;
            if (ownerControl != null)
                _parentForm = ownerControl.FindForm();

            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                string[] initParameters = new string[] { "--no-sub-autodetect-file" }; //, "--no-video-title-show" }; //TODO: Put in options/config file
                _libVlc = libvlc_new(initParameters.Length, initParameters);
                IntPtr media = libvlc_media_new_path(_libVlc, Encoding.UTF8.GetBytes(videoFileName + "\0"));
                _mediaPlayer = libvlc_media_player_new_from_media(media);
                libvlc_media_release(media);

                //  Linux: libvlc_media_player_set_xdrawable (_mediaPlayer, xdrawable);
                //  Mac: libvlc_media_player_set_nsobject (_mediaPlayer, view);
                libvlc_media_player_set_hwnd(_mediaPlayer, ownerControl.Handle); // windows

                if (onVideoEnded != null)
                {
                    _videoEndTimer = new System.Windows.Forms.Timer { Interval = 500 };
                    _videoEndTimer.Tick += VideoEndTimerTick;
                    _videoEndTimer.Start();
                }

                libvlc_media_player_play(_mediaPlayer);
                _videoLoadedTimer = new System.Windows.Forms.Timer { Interval = 500 };
                _videoLoadedTimer.Tick += new EventHandler(VideoLoadedTimer_Tick);
                _videoLoadedTimer.Start();
            }
        }



        void VideoEndTimerTick(object sender, EventArgs e)
        {
            const int Ended = 6;
            int state = libvlc_media_player_get_state(_mediaPlayer);
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

            ThreadPool.QueueUserWorkItem(DisposeVLC, this);
        }

        private void DisposeVLC(object player)
        {
            try
            {
                if (_mediaPlayer != IntPtr.Zero)
                {
                    libvlc_media_player_stop(_mediaPlayer);
                    libvlc_media_list_player_release(_mediaPlayer);
                }

                if (_libVlc != IntPtr.Zero)
                    libvlc_release(_libVlc);
            }
            catch
            {
            }
        }

        public override event EventHandler OnVideoLoaded;

        public override event EventHandler OnVideoEnded;

    }
}
