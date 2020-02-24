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
    public class LibMpvDynamic : VideoPlayer, IDisposable
    {

        #region mpv dll methods - see https://github.com/mpv-player/mpv/blob/master/libmpv/client.h
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvCreate();
        private MpvCreate _mpvCreate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvInitialize(IntPtr mpvHandle);
        private MpvInitialize _mpvInitialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvCommand(IntPtr mpvHandle, IntPtr utf8Strings);
        private MpvCommand _mpvCommand;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvWaitEvent(IntPtr mpvHandle, double wait);
        private MpvWaitEvent _mpvWaitEvent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOption(IntPtr mpvHandle, byte[] name, int format, ref long data);
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MpvFree(IntPtr data);
        private MpvFree _mpvFree;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ulong MpvClientApiVersion();
        private MpvClientApiVersion _mpvClientApiVersion;

        #endregion

        private static IntPtr _libMpvDll = IntPtr.Zero;
        private IntPtr _mpvHandle;
        private Timer _videoLoadedTimer;
        private double? _pausePosition; // Hack to hold precise seeking when paused

        public override event EventHandler OnVideoLoaded;
        public override event EventHandler OnVideoEnded;

        private const int MpvFormatString = 1;

        private object GetDllType(Type type, string name)
        {
            var address = NativeMethods.CrossGetProcAddress(_libMpvDll, name);
            if (address != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer(address, type);
            }
            return null;
        }

        private void LoadLibMpvDynamic()
        {
            _mpvCreate = (MpvCreate)GetDllType(typeof(MpvCreate), "mpv_create");
            _mpvInitialize = (MpvInitialize)GetDllType(typeof(MpvInitialize), "mpv_initialize");
            _mpvWaitEvent = (MpvWaitEvent)GetDllType(typeof(MpvWaitEvent), "mpv_wait_event");
            _mpvCommand = (MpvCommand)GetDllType(typeof(MpvCommand), "mpv_command");
            _mpvSetOption = (MpvSetOption)GetDllType(typeof(MpvSetOption), "mpv_set_option");
            _mpvSetOptionString = (MpvSetOptionString)GetDllType(typeof(MpvSetOptionString), "mpv_set_option_string");
            _mpvGetPropertyString = (MpvGetPropertystring)GetDllType(typeof(MpvGetPropertystring), "mpv_get_property");
            _mpvGetPropertyDouble = (MpvGetPropertyDouble)GetDllType(typeof(MpvGetPropertyDouble), "mpv_get_property");
            _mpvSetProperty = (MpvSetProperty)GetDllType(typeof(MpvSetProperty), "mpv_set_property");
            _mpvFree = (MpvFree)GetDllType(typeof(MpvFree), "mpv_free");
            _mpvClientApiVersion = (MpvClientApiVersion)GetDllType(typeof(MpvClientApiVersion), "mpv_client_api_version");
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
                   _mpvSetProperty != null &&
                   _mpvFree != null;
        }

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
            _mpvCommand(_mpvHandle, mainPtr);
            foreach (var ptr in byteArrayPointers)
            {
                Marshal.FreeHGlobal(ptr);
            }
            Marshal.FreeHGlobal(mainPtr);
        }

        public override string PlayerName => "libmpv " + VersionNumber;

        private int _volume = 75;
        public override int Volume
        {
            get => _volume;
            set
            {
                var v = Configuration.Settings.General.AllowVolumeBoost ? (int)Math.Round(value * 1.5) : value;
                DoMpvCommand("set", "volume", v.ToString(CultureInfo.InvariantCulture));
                _volume = value;
            }
        }

        public override double Duration
        {
            get
            {
                lock (_lockObj)
                {
                    if (_mpvHandle == IntPtr.Zero)
                    {
                        return 0;
                    }

                    int mpvFormatDouble = 5;
                    double d = 0;
                    _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("duration"), mpvFormatDouble, ref d);
                    return d;
                }
            }
        }

        public override double CurrentPosition
        {
            get
            {
                lock (_lockObj)
                {
                    if (_mpvHandle == IntPtr.Zero)
                    {
                        return 0;
                    }

                    if (_pausePosition != null)
                    {
                        if (_pausePosition < 0)
                        {
                            return 0;
                        }

                        return _pausePosition.Value;
                    }

                    int mpvFormatDouble = 5;
                    double d = 0;
                    _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("time-pos"), mpvFormatDouble, ref d);
                    return d;
                }
            }
            set
            {
                lock (_lockObj)
                {

                    if (_mpvHandle == IntPtr.Zero)
                    {
                        return;
                    }

                    if (IsPaused && value <= Duration)
                    {
                        _pausePosition = value;
                    }

                    DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute");
                }
            }
        }

        private double _playRate = 1.0;
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
            _pausePosition = null;
        }

        public void GetPreviousFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            DoMpvCommand("frame-back-step");
            _pausePosition = null;
        }

        public override void Play()
        {
            lock (_lockObj)
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                _pausePosition = null;

                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                var bytes = GetUtf8Bytes("no");
                _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
            }
        }

        public override void Pause()
        {
            lock (_lockObj)
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                var bytes = GetUtf8Bytes("yes");
                _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
            }
        }

        public override void Stop()
        {
            lock (_lockObj)
            {

                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                Pause();
                _pausePosition = null;
                CurrentPosition = 0;
            }
        }

        public override bool IsPaused
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return true;
                }

                var lpBuffer = IntPtr.Zero;
                _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref lpBuffer);
                var isPaused = Marshal.PtrToStringAnsi(lpBuffer) == "yes";
                _mpvFree(lpBuffer);
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
                    var lpBuffer = IntPtr.Zero;
                    _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("track-list"), MpvFormatString, ref lpBuffer);
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
                    _mpvFree(lpBuffer);
                }
                return _audioTrackIds.Count;
            }
        }

        public int AudioTrackNumber
        {
            get
            {
                var lpBuffer = IntPtr.Zero;
                _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("aid"), MpvFormatString, ref lpBuffer);
                string str = Marshal.PtrToStringAnsi(lpBuffer);
                int number = 0;
                if (AudioTrackCount > 1 && _audioTrackIds.Contains(str))
                {
                    number = _audioTrackIds.IndexOf(str);
                }
                _mpvFree(lpBuffer);
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

        public string VersionNumber
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return string.Empty;
                }

                var version = _mpvClientApiVersion();
                var high = version >> 16;
                var low = version & 0xff;
                return high + "." + low;
            }
        }

        public void LoadSubtitle(string fileName)
        {
            DoMpvCommand("sub-add", fileName, "select");
        }

        public void RemoveSubtitle()
        {
            DoMpvCommand("sub-remove");
        }

        public void ReloadSubtitle()
        {
            DoMpvCommand("sub-reload");
        }

        public void HideCursor()
        {
            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("cursor-autohide"), GetUtf8Bytes("always"));
        }

        public void ShowCursor()
        {
            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("cursor-autohide"), GetUtf8Bytes("no"));
        }

        public static bool IsInstalled
        {
            get
            {
                try
                {
                    if (Configuration.IsRunningOnLinux)
                    {
                        var lib = NativeMethods.CrossLoadLibrary("libmpv.so");
                        if (lib != IntPtr.Zero)
                        {
                            NativeMethods.CrossFreeLibrary(lib);
                            return true;
                        }
                        return false;
                    }
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
            if (Configuration.IsRunningOnWindows)
            {
                var path = Path.Combine(Configuration.DataDirectory, fileName);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static bool LoadLib()
        {
            if (_libMpvDll == IntPtr.Zero)
            {
                var fileName = Configuration.IsRunningOnWindows ? GetMpvPath("mpv-1.dll") : "libmpv.so";
                _libMpvDll = NativeMethods.CrossLoadLibrary(fileName);
            }
            return _libMpvDll != IntPtr.Zero;
        }

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            if (LoadLib())
            {
                LoadLibMpvDynamic();
                if (!IsAllMethodsLoaded())
                {
                    throw new Exception("MPV - not all needed methods found in dll file");
                }
                _mpvHandle = _mpvCreate.Invoke();

                //var logFileName = Path.Combine(Configuration.DataDirectory, "mpv-log-" + Guid.NewGuid() + ".txt");
                //_mpvSetOptionString(_mpvHandle, GetUtf8Bytes("log-file"), GetUtf8Bytes(logFileName));
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
                SetVideoOwner(ownerControl);

                string videoOutput = "direct3d";
                if (Configuration.IsRunningOnLinux)
                {
                    videoOutput = Configuration.Settings.General.MpvVideoOutputLinux;
                }
                else if (!string.IsNullOrWhiteSpace(Configuration.Settings.General.MpvVideoOutput))
                {
                    videoOutput = Configuration.Settings.General.MpvVideoOutput;
                }

                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("vo"), GetUtf8Bytes(videoOutput)); // use "direct3d" or "opengl"
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("keep-open"), GetUtf8Bytes("always")); // don't auto close video
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("no-sub"), GetUtf8Bytes("")); // don't load subtitles
                if (videoFileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    videoFileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("ytdl"), GetUtf8Bytes("yes"));
                }

                DoMpvCommand("loadfile", videoFileName);

                System.Threading.Thread.Sleep(100);
                SetVideoOwner(ownerControl);

                _videoLoadedTimer = new Timer { Interval = 50 };
                _videoLoadedTimer.Tick += VideoLoadedTimer_Tick;
                _videoLoadedTimer.Start();

                SetVideoOwner(ownerControl);
            }
        }

        private void SetVideoOwner(Control ownerControl)
        {
            if (ownerControl != null)
            {
                int iterations = 25;
                int returnCode = -1;
                int mpvFormatInt64 = 4;
                if (ownerControl.IsDisposed)
                {
                    return;
                }
                var windowId = ownerControl.Handle.ToInt64();
                while (returnCode != 0 && iterations > 0)
                {
                    Application.DoEvents();

                    lock (_lockObj)
                    {
                        if (_mpvHandle == IntPtr.Zero)
                        {
                            return;
                        }
                        returnCode = _mpvSetOption(_mpvHandle, GetUtf8Bytes("wid"), mpvFormatInt64, ref windowId);
                        if (returnCode != 0)
                        {
                            iterations--;
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
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
                    lock (_lockObj)
                    {
                        if (_mpvHandle == IntPtr.Zero)
                        {
                            return;
                        }

                        var eventIdHandle = _mpvWaitEvent(_mpvHandle, 0);
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
                        HardDispose();
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
        }

        public void HardDispose()
        {
            DoMpvCommand("stop");
            System.Threading.Thread.Sleep(150);
            Application.DoEvents();
            DoMpvCommand("quit");
            _mpvHandle = IntPtr.Zero;
            System.Threading.Thread.Sleep(150);
            Application.DoEvents();
            System.Threading.Thread.Sleep(50);
            Application.DoEvents();
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
        }
    }
}
