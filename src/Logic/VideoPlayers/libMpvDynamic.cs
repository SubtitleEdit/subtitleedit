using Nikse.SubtitleEdit.Core;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class LibMpvDynamic : VideoPlayer, IDisposable
    {

        #region mpv dll methods
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvCreate();
        private MpvCreate _mpvCreate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvInitialize(IntPtr mpvHandle);
        private MpvInitialize _mpvInitialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvCommand(IntPtr mpvHandle, [MarshalAs(UnmanagedType.LPArray)] string[] args);
        private MpvCommand _mpvCommand;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvTerminateDestroy(IntPtr mpvHandle);
        private MpvTerminateDestroy _mpvTerminateDestroy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvWaitEvent(IntPtr mpvHandle, double wait);
        private MpvWaitEvent _mpvWaitEvent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOption(IntPtr mpvHandle, byte[] name, int format, ref uint data);
        private MpvSetOption _mpvSetOption;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOptionString(IntPtr mpvHandle, byte[] name, byte[] value);
        private MpvSetOptionString _mpvSetOptionString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvGetPropertystring(IntPtr mpvHandle, byte[] name, int format, ref IntPtr data);
        private MpvGetPropertystring _mpvGetPropertyString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvGetPropertyDouble(IntPtr mpvHandle, byte[] name, int format, ref double data);
        private MpvGetPropertyDouble _mpvGetPropertyDouble;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetProperty(IntPtr mpvHandle, byte[] name, int format, ref byte[] data);
        private MpvSetProperty _mpvSetProperty;
        #endregion

        private IntPtr _libMpvDll;
        private IntPtr _mpvHandle;
        private Timer _videoLoadedTimer;
        private Timer _videoEndedTimer;

        public override event EventHandler OnVideoLoaded;
        public override event EventHandler OnVideoEnded;

        private object GetDllType(Type type, string name)
        {
            IntPtr address = NativeMethods.GetProcAddress(_libMpvDll, name);
            if (address != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer(address, type);
            }
            return null;
        }

        private void LoadLibVlcDynamic()
        {
            _mpvCreate = (MpvCreate)GetDllType(typeof(MpvCreate), "mpv_create");
            _mpvInitialize = (MpvInitialize)GetDllType(typeof(MpvInitialize), "mpv_initialize");
            _mpvTerminateDestroy = (MpvTerminateDestroy)GetDllType(typeof(MpvTerminateDestroy), "mpv_terminate_destroy");
            _mpvWaitEvent = (MpvWaitEvent)GetDllType(typeof(MpvWaitEvent), "mpv_wait_event");
            _mpvCommand = (MpvCommand)GetDllType(typeof(MpvCommand), "mpv_command");
            _mpvSetOption = (MpvSetOption)GetDllType(typeof(MpvSetOption), "mpv_set_option");
            _mpvSetOptionString = (MpvSetOptionString)GetDllType(typeof(MpvSetOptionString), "mpv_set_option_string");
            _mpvGetPropertyString = (MpvGetPropertystring)GetDllType(typeof(MpvGetPropertystring), "mpv_get_property");
            _mpvGetPropertyDouble = (MpvGetPropertyDouble)GetDllType(typeof(MpvGetPropertyDouble), "mpv_get_property");
            _mpvSetProperty = (MpvSetProperty)GetDllType(typeof(MpvSetProperty), "mpv_set_property");
        }

        private bool IsAllMethodsLoaded()
        {
            return _mpvCreate != null &&
                   _mpvInitialize != null &&
                   _mpvWaitEvent != null &&
                   _mpvCommand != null &&
                   _mpvSetOption != null &&
                   _mpvSetOptionString != null &&
                   _mpvGetPropertyString != null &&
                   _mpvGetPropertyDouble != null &&
                   _mpvSetProperty != null;
        }


        public override string PlayerName
        {
            get { return "MPV Lib"; }
        }

        private int _volume = 75;
        public override int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _mpvCommand(_mpvHandle, new[] { "set", "volume", value.ToString(), null });
                _volume = value;
            }
        }

        public override double Duration
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                    return 0;

                int mpvFormatDouble = 5;
                double d = 0;
                _mpvGetPropertyDouble(_mpvHandle, Encoding.UTF8.GetBytes("duration\0"), mpvFormatDouble, ref d);
                return d;
            }
        }

        public override double CurrentPosition
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                    return 0;

                int mpvFormatDouble = 5;
                double d = 0;
                _mpvGetPropertyDouble(_mpvHandle, Encoding.UTF8.GetBytes("time-pos\0"), mpvFormatDouble, ref d);
                return d;
            }
            set
            {
                if (_mpvHandle == IntPtr.Zero)
                    return;

                _mpvCommand(_mpvHandle, new[] { "seek", value.ToString(CultureInfo.InvariantCulture), "absolute", null });
            }
        }

        private double _playRate = 1.0;
        public override double PlayRate
        {
            get
            {
                return _playRate; 
            }
            set
            {
                _mpvCommand(_mpvHandle, new[] { "set", "speed", value.ToString(CultureInfo.InvariantCulture), null });
                _playRate = value;
            }
        }

        public void GetNextFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            _mpvCommand(_mpvHandle, new[] { "frame-step", null });
        }

        public void GetPreviousFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            _mpvCommand(_mpvHandle, new[] { "frame-back-step", null });
        }


        public override void Play()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            int MPV_FORMAT_STRING = 1;
            var bytes = Encoding.UTF8.GetBytes("no\0");
            _mpvSetProperty(_mpvHandle, Encoding.UTF8.GetBytes("pause\0"), MPV_FORMAT_STRING, ref bytes);
        }

        public override void Pause()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            int MPV_FORMAT_STRING = 1;
            var bytes = Encoding.UTF8.GetBytes("yes\0");
            _mpvSetProperty(_mpvHandle, Encoding.UTF8.GetBytes("pause\0"), MPV_FORMAT_STRING, ref bytes);
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
                    return true;

                int mpvFormatString = 1;
                IntPtr lpBuffer = Marshal.AllocHGlobal(10);
                try
                {
                    _mpvGetPropertyString(_mpvHandle, Encoding.UTF8.GetBytes("pause\0"), mpvFormatString, ref lpBuffer);
                }
                catch
                {
                    // ignored
                }
                string str = Marshal.PtrToStringAnsi(lpBuffer);
                return str == "yes";
            }
        }

        public override bool IsPlaying
        {
            get { return !IsPaused; }
        }

        public int AudioTrackCount
        {
            get
            {
                return 1; //TODO: Fix
            }
        }

        private int _audioTrackNumber = 0;
        public int AudioTrackNumber
        {
            get
            {
                return _audioTrackNumber;
            }
            set
            {
                //TODO: Fix
                //_mpvCommand(_mpvHandle, new[] { "set", "aid", value.ToString(CultureInfo.InvariantCulture), null });
                //_audioTrackNumber = value;
            }
        }

        public static bool IsInstalled
        {
            get
            {
                try
                {
                    string dllFile = GetMpvPath("mpv-1.dll");
                    return File.Exists(dllFile);
                }
                catch
                {
                    return false;
                }
            }
        }

        public static string GetMpvPath(string fileName)
        {
            if (Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac())
                return null;

            var path = Path.Combine(Configuration.BaseDirectory, @"MPV\" + fileName);
            if (File.Exists(path))
                return path;

            path = Path.Combine(Configuration.BaseDirectory, fileName);
            if (File.Exists(path))
                return path;

            return null;
        }

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            string dllFile = GetMpvPath("mpv-1.dll");
            if (File.Exists(dllFile))
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(dllFile));
                _libMpvDll = NativeMethods.LoadLibrary(dllFile);
                LoadLibVlcDynamic();
                if (!IsAllMethodsLoaded())
                {
                    throw new Exception("MPV - not all needed methods found in dll file");
                }
                _mpvHandle = _mpvCreate.Invoke();
            }
            else if (!Directory.Exists(videoFileName))
            {
                return;
            }

            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                _mpvInitialize.Invoke(_mpvHandle);

                string videoOutput = "direct3d_shaders"; 
                if (!string.IsNullOrWhiteSpace(Configuration.Settings.General.MpvVideoOutput))
                    videoOutput = Configuration.Settings.General.MpvVideoOutput;
                _mpvSetOptionString(_mpvHandle, Encoding.UTF8.GetBytes("vo\0"), Encoding.UTF8.GetBytes(videoOutput + "\0")); // "direct3d_shaders" is default, "direct3d" could be used for compabality with old systems                                                                                                                           

                _mpvSetOptionString(_mpvHandle, Encoding.UTF8.GetBytes("keep-open\0"), Encoding.UTF8.GetBytes("always\0")); // don't auto close video
                _mpvSetOptionString(_mpvHandle, Encoding.UTF8.GetBytes("no-sub\0"), Encoding.UTF8.GetBytes("\0")); // don't load subtitles
                if (ownerControl != null)
                {
                    int mpvFormatInt64 = 4;
                    uint windowId = (uint)ownerControl.Handle.ToInt64();
                    _mpvSetOption(_mpvHandle, Encoding.UTF8.GetBytes("wid\0"), mpvFormatInt64, ref windowId);
                }
                _mpvCommand(_mpvHandle, new[] { "loadfile", videoFileName, null });

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
                    var eventIdHandle = _mpvWaitEvent(_mpvHandle, 0);
                    var eventId = Convert.ToInt64(Marshal.PtrToStructure(eventIdHandle, typeof(int)));
                    if (eventId == mpvEventFileLoaded)
                    {
                        break;
                    }
                    l++;
                }
                catch
                {
                    return;
                }
            }
            Application.DoEvents();
            if (OnVideoLoaded != null)
                OnVideoLoaded.Invoke(this, null);
            Application.DoEvents();
            Pause();
        }

        private void VideoEndedTimer_Tick(object sender, EventArgs e)
        {
        }

        public override void DisposeVideoPlayer()
        {
            Dispose();
        }

        private void ReleaseUnmangedResources()
        {
            try
            {
                lock (this)
                {
                    if (_mpvHandle != IntPtr.Zero)
                    {
                        _mpvTerminateDestroy(_mpvHandle);
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

        ~LibMpvDynamic()
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
            _mpvCommand(_mpvHandle, new[] { "quit", null });
            ReleaseUnmangedResources();
        }

    }
}
