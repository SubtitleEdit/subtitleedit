using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;


namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class LibMpvMono : VideoPlayer, IDisposable
    {

        private IntPtr _libMpvDll;
        private IntPtr _mpvHandle;
        private Timer _videoLoadedTimer;
        //        private Timer _videoEndedTimer;

        public override event EventHandler OnVideoLoaded;
        public override event EventHandler OnVideoEnded;

        private const int MpvFormatString = 1;


        private static byte[] GetUtf8Bytes(string s)
        {
            return Encoding.UTF8.GetBytes(s + "\0");
        }

        public static IntPtr AllocateUtf8IntPtrArrayWithSentinel(string[] arr, out IntPtr[] byteArrayPointers)
        {
            int numberOfStrings = arr.Length + 1; // add extra element for extra null pointer last (sentinel)
            byteArrayPointers = new IntPtr[numberOfStrings];
            IntPtr rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
            for (int index = 0; index < arr.Length; index++)
            {
                var bytes = GetUtf8Bytes(arr[index]);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
                byteArrayPointers[index] = unmanagedPointer;
            }
            Marshal.Copy(byteArrayPointers, 0, rootPointer, numberOfStrings);
            return rootPointer;
        }

        private void DoMpvCommand(params string[] args)
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            var mainPtr = AllocateUtf8IntPtrArrayWithSentinel(args, out var byteArrayPointers);
            NativeMethods.mpv_command(_mpvHandle, mainPtr);
            foreach (var ptr in byteArrayPointers)
            {
                Marshal.FreeHGlobal(ptr);
            }
            Marshal.FreeHGlobal(mainPtr);
        }

        public override string PlayerName => "MPV Player";

        private int _volume = 75;
        public override int Volume
        {
            get => _volume;
            set
            {
                DoMpvCommand("set", "volume", value.ToString());
                _volume = value;
            }
        }

        public override double Duration
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return 0;
                }

                int mpvFormatDouble = 5;
                double d = 0;
                NativeMethods.mpv_get_property(_mpvHandle, GetUtf8Bytes("duration"), mpvFormatDouble, ref d);
                return d;
            }
        }

        public override double CurrentPosition
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return 0;
                }

                int mpvFormatDouble = 5;
                double d = 0;
                NativeMethods.mpv_get_property(_mpvHandle, GetUtf8Bytes("time-pos"), mpvFormatDouble, ref d);
                return d;
            }
            set
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute");
            }
        }

        private double _playRate = 2.0;
        public override double PlayRate
        {
            get => _playRate;
            set
            {
                DoMpvCommand("set", "speed", value.ToString(CultureInfo.InvariantCulture));
                _playRate = value;
            }
        }

        public void GetNextFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            DoMpvCommand("frame-step");
        }

        public void GetPreviousFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            DoMpvCommand("frame-back-step");
        }

        public override void Play()
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            var bytes = GetUtf8Bytes("no");
            NativeMethods.mpv_set_property(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
        }

        public override void Pause()
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            var bytes = GetUtf8Bytes("yes");
            NativeMethods.mpv_set_property(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
        }

        public override void Stop()
        {
            Pause();
            CurrentPosition = 0;
        }

        public override bool IsPaused
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return true;
                }

                var lpBuffer = NativeMethods.mpv_get_property_string(_mpvHandle, GetUtf8Bytes("pause"));
                string s = Marshal.PtrToStringAnsi(lpBuffer);
                bool isPaused = s == "yes";
                NativeMethods.mpv_free(lpBuffer);
                return isPaused;
            }
        }

        public override bool IsPlaying => !IsPaused;

        private List<string> _audioTrackIds;
        public int AudioTrackCount
        {
            get
            {
                if (_audioTrackIds == null)
                {
                    _audioTrackIds = new List<string>();
                    var lpBuffer = NativeMethods.mpv_get_property_string(_mpvHandle, GetUtf8Bytes("track-list"));
                    string trackListJson = Marshal.PtrToStringAnsi(lpBuffer);
                    foreach (var json in Json.ReadObjectArray(trackListJson))
                    {
                        string trackType = Json.ReadTag(json, "type");
                        string id = Json.ReadTag(json, "id");
                        if (trackType == "audio")
                        {
                            _audioTrackIds.Add(id);
                        }
                    }
                    NativeMethods.mpv_free(lpBuffer);
                }
                return _audioTrackIds.Count;
            }
        }

        public int AudioTrackNumber
        {
            get
            {
                var lpBuffer = NativeMethods.mpv_get_property_string(_mpvHandle, GetUtf8Bytes("aid"));
                string str = Marshal.PtrToStringAnsi(lpBuffer);
                int number = 0;
                if (AudioTrackCount > 1 && _audioTrackIds.Contains(str))
                {
                    number = _audioTrackIds.IndexOf(str);
                }
                NativeMethods.mpv_free(lpBuffer);
                return number;
            }
            set
            {
                string id = "1";
                if (AudioTrackCount > 1 && value >= 0 && value < _audioTrackIds.Count)
                {
                    id = _audioTrackIds[value];
                }
                DoMpvCommand("set", "aid", id);
            }
        }

        public static bool IsInstalled => true;

        public static string GetMpvPath(string fileName)
        {
            var path = Path.Combine(Configuration.DataDirectory, fileName);
            if (File.Exists(path))
            {
                return path;
            }

            return null;
        }

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            _mpvHandle = NativeMethods.mpv_create();
            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                //Mono.Unix.
                NativeMethods.mpv_initialize(_mpvHandle);

                string videoOutput = "vaapi";
                if (!string.IsNullOrWhiteSpace(Configuration.Settings.General.MpvVideoOutput))
                {
                    videoOutput = Configuration.Settings.General.MpvVideoOutput;
                }

                NativeMethods.mpv_set_option_string(_mpvHandle, GetUtf8Bytes("vo"), GetUtf8Bytes(videoOutput)); // "direct3d_shaders" is default, "direct3d" could be used for compabality with old systems

                NativeMethods.mpv_set_option_string(_mpvHandle, GetUtf8Bytes("keep-open"), GetUtf8Bytes("always")); // don't auto close video
                NativeMethods.mpv_set_option_string(_mpvHandle, GetUtf8Bytes("no-sub"), GetUtf8Bytes("")); // don't load subtitles
                if (ownerControl != null)
                {
                    int mpvFormatInt64 = 4;
                    var windowId = ownerControl.Handle.ToInt64();
                    NativeMethods.mpv_set_option(_mpvHandle, GetUtf8Bytes("wid"), mpvFormatInt64, ref windowId);
                }
                DoMpvCommand("loadfile", videoFileName);

                _videoLoadedTimer = new Timer { Interval = 50 };
                _videoLoadedTimer.Tick += VideoLoadedTimer_Tick;
                _videoLoadedTimer.Start();
            }
        }

        private void VideoLoadedTimer_Tick(object sender, EventArgs e)
        {
            _videoLoadedTimer.Stop();
            const int mpvEventFileLoaded = 8;
            int l = 0;
            while (l < 10000)
            {
                Application.DoEvents();
                try
                {
                    if (_mpvHandle != IntPtr.Zero)
                    {
                        var eventIdHandle = NativeMethods.mpv_wait_event(_mpvHandle, 0);
                        var eventId = Convert.ToInt64(Marshal.PtrToStructure(eventIdHandle, typeof(int)));
                        if (eventId == mpvEventFileLoaded)
                        {
                            break;
                        }
                    }
                    l++;
                }
                catch
                {
                    return;
                }
            }
            Application.DoEvents();
            OnVideoLoaded?.Invoke(this, null);

            Application.DoEvents();
            Pause();
        }


        public override void DisposeVideoPlayer()
        {
            Dispose();
        }

        private readonly object _lockObj = new object();

        private void ReleaseUnmanagedResources()
        {
            try
            {
                lock (_lockObj)
                {
                    if (_mpvHandle != IntPtr.Zero)
                    {
                        NativeMethods.mpv_terminate_destroy(_mpvHandle);
                        _mpvHandle = IntPtr.Zero;
                    }

                    if (_libMpvDll != IntPtr.Zero)
                    {
                        NativeMethods.FreeLibrary(_libMpvDll);
                        _libMpvDll = IntPtr.Zero;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        ~LibMpvMono()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_mpvHandle != IntPtr.Zero)
            {
                DoMpvCommand("quit");
            }

            ReleaseUnmanagedResources();
        }

    }
}
