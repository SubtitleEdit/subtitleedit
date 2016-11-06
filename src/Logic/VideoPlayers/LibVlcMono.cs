﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class LibVlcMono : IVideoPlayer
    {
        private Timer _videoLoadedTimer;
        private Timer _videoEndTimer;
        private IntPtr _libVlcDLL;
        private IntPtr _libVlc;
        private IntPtr _mediaPlayer;
        private Control _ownerControl;
        private Form _parentForm;
        private string _videoFileName;

        public string PlayerName
        {
            get { return "VLC Lib Mono"; }
        }

        public int Volume
        {
            get
            {
                return NativeMethods.libvlc_audio_get_volume(_mediaPlayer);
            }
            set
            {
                NativeMethods.libvlc_audio_set_volume(_mediaPlayer, value);
            }
        }

        public double Duration
        {
            get
            {
                return NativeMethods.libvlc_media_player_get_length(_mediaPlayer) / TimeCode.BaseUnit;
            }
        }

        public double CurrentPosition
        {
            get
            {
                return NativeMethods.libvlc_media_player_get_time(_mediaPlayer) / TimeCode.BaseUnit;
            }
            set
            {
                NativeMethods.libvlc_media_player_set_time(_mediaPlayer, (long)(value * TimeCode.BaseUnit));
            }
        }

        public double PlayRate
        {
            get
            {
                return NativeMethods.libvlc_media_player_get_rate(_mediaPlayer);
            }
            set
            {
                if (value >= 0 && value <= 2.0)
                    NativeMethods.libvlc_media_player_set_rate(_mediaPlayer, (float)value);
            }
        }

        public void Play()
        {
            NativeMethods.libvlc_media_player_play(_mediaPlayer);
        }

        public void Pause()
        {
            if (!IsPaused)
                NativeMethods.libvlc_media_player_pause(_mediaPlayer);
        }

        public void Stop()
        {
            NativeMethods.libvlc_media_player_stop(_mediaPlayer);
        }

        public bool IsPaused
        {
            get
            {
                const int Paused = 4;
                int state = NativeMethods.libvlc_media_player_get_state(_mediaPlayer);
                return state == Paused;
            }
        }

        public bool IsPlaying
        {
            get
            {
                const int Playing = 3;
                int state = NativeMethods.libvlc_media_player_get_state(_mediaPlayer);
                return state == Playing;
            }
        }

        public int AudioTrackCount
        {
            get
            {
                return NativeMethods.libvlc_audio_get_track_count(_mediaPlayer) - 1;
            }
        }

        public int AudioTrackNumber
        {
            get
            {
                return NativeMethods.libvlc_audio_get_track(_mediaPlayer) - 1;
            }
            set
            {
                NativeMethods.libvlc_audio_set_track(_mediaPlayer, value + 1);
            }
        }

        public string VideoFileName
        {
            get
            {
                return _videoFileName;
            }
        }

        public LibVlcMono MakeSecondMediaPlayer(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
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
                IntPtr media = NativeMethods.libvlc_media_new_path(_libVlc, Encoding.UTF8.GetBytes(videoFileName + "\0"));
                newVlc._mediaPlayer = NativeMethods.libvlc_media_player_new_from_media(media);
                NativeMethods.libvlc_media_release(media);

                //  Linux: libvlc_media_player_set_xdrawable (_mediaPlayer, xdrawable);
                //  Mac: libvlc_media_player_set_nsobject (_mediaPlayer, view);
                var ownerHandle = ownerControl == null ? IntPtr.Zero : ownerControl.Handle;
                NativeMethods.libvlc_media_player_set_hwnd(newVlc._mediaPlayer, ownerHandle); // windows

                if (onVideoEnded != null)
                {
                    newVlc._videoEndTimer = new Timer { Interval = 500 };
                    newVlc._videoEndTimer.Tick += VideoEndTimerTick;
                    newVlc._videoEndTimer.Start();
                }

                NativeMethods.libvlc_media_player_play(newVlc._mediaPlayer);
                newVlc._videoLoadedTimer = new Timer { Interval = 500 };
                newVlc._videoLoadedTimer.Tick += newVlc.VideoLoadedTimer_Tick;
                newVlc._videoLoadedTimer.Start();
            }
            return newVlc;
        }

        private void VideoLoadedTimer_Tick(object sender, EventArgs e)
        {
            int i = 0;
            while (!IsPlaying && i < 50)
            {
                System.Threading.Thread.Sleep(100);
                i++;
            }
            NativeMethods.libvlc_media_player_pause(_mediaPlayer);
            _videoLoadedTimer.Stop();

            if (OnVideoLoaded != null)
                OnVideoLoaded.Invoke(_mediaPlayer, new EventArgs());
        }

        public void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            _ownerControl = ownerControl;
            if (ownerControl != null)
                _parentForm = ownerControl.FindForm();

            _videoFileName = videoFileName;
            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                string[] initParameters = { "--no-sub-autodetect-file" }; //, "--no-video-title-show" }; // TODO: Put in options/config file
                _libVlc = NativeMethods.libvlc_new(initParameters.Length, initParameters);
                IntPtr media = NativeMethods.libvlc_media_new_path(_libVlc, Encoding.UTF8.GetBytes(videoFileName + "\0"));
                _mediaPlayer = NativeMethods.libvlc_media_player_new_from_media(media);
                NativeMethods.libvlc_media_release(media);

                //  Linux: libvlc_media_player_set_xdrawable (_mediaPlayer, xdrawable);
                //  Mac: libvlc_media_player_set_nsobject (_mediaPlayer, view);
                var ownerHandle = ownerControl == null ? IntPtr.Zero : ownerControl.Handle;
                NativeMethods.libvlc_media_player_set_hwnd(_mediaPlayer, ownerHandle); // windows

                if (onVideoEnded != null)
                {
                    _videoEndTimer = new Timer { Interval = 500 };
                    _videoEndTimer.Tick += VideoEndTimerTick;
                    _videoEndTimer.Start();
                }

                NativeMethods.libvlc_media_player_play(_mediaPlayer);
                _videoLoadedTimer = new Timer { Interval = 500 };
                _videoLoadedTimer.Tick += VideoLoadedTimer_Tick;
                _videoLoadedTimer.Start();
            }
        }

        private void VideoEndTimerTick(object sender, EventArgs e)
        {
            const int Ended = 6;
            int state = NativeMethods.libvlc_media_player_get_state(_mediaPlayer);
            if (state == Ended)
            {
                // hack to make sure VLC is in ready state
                Stop();
                Play();
                Pause();
                OnVideoEnded.Invoke(_mediaPlayer, new EventArgs());
            }
        }

        public void DisposeVideoPlayer()
        {
            if (_videoLoadedTimer != null)
                _videoLoadedTimer.Stop();

            if (_videoEndTimer != null)
                _videoEndTimer.Stop();

            System.Threading.ThreadPool.QueueUserWorkItem(DisposeVLC, this);
        }

        private void DisposeVLC(object player)
        {
            ReleaseUnmangedResources();
        }

        public event EventHandler OnVideoLoaded;

        public event EventHandler OnVideoEnded;

        ~LibVlcMono()
        {
            Dispose(false);
        }

        private void ReleaseUnmangedResources()
        {
            try
            {
                if (_mediaPlayer != IntPtr.Zero)
                {
                    NativeMethods.libvlc_media_player_stop(_mediaPlayer);
                    NativeMethods.libvlc_media_list_player_release(_mediaPlayer);
                    _mediaPlayer = IntPtr.Zero;
                }

                if (_libVlc != IntPtr.Zero)
                {
                    NativeMethods.libvlc_release(_libVlc);
                    _libVlc = IntPtr.Zero;
                }
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_videoLoadedTimer != null)
                {
                    _videoLoadedTimer.Dispose();
                    _videoLoadedTimer = null;
                }
                if (_videoEndTimer != null)
                {
                    _videoEndTimer.Dispose();
                    _videoEndTimer = null;
                }
            }
            ReleaseUnmangedResources();
        }

    }
}
