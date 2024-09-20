using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class LibMpvDynamic : VideoPlayer, IDisposable
    {

        #region LibMpv methods - see https://github.com/mpv-player/mpv/blob/master/libmpv/client.h

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
        private delegate int MpvSetOption(IntPtr mpvHandle, byte[] name, int format, ref ulong data);
        private MpvSetOption _mpvSetOption;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOptionString(IntPtr mpvHandle, byte[] name, byte[] value);
        private MpvSetOptionString _mpvSetOptionString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvGetPropertyString(IntPtr mpvHandle, byte[] name, int format, ref IntPtr data);
        private MpvGetPropertyString _mpvGetPropertyString;

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
        private string _secondSubtitleFileName;
        private int _brightness; // -100 to 100
        private int _contrast; // -100 to 100

        public override event EventHandler OnVideoLoaded;
        public override event EventHandler OnVideoEnded;

        private const int MpvFormatString = 1;

        private object GetDllType(Type type, string name)
        {
            var address = NativeMethods.CrossGetProcAddress(_libMpvDll, name);
            return address != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer(address, type) : null;
        }

        private void LoadLibMpvDynamic()
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
            var numberOfStrings = arr.Length + 1; // add extra element for extra null pointer last (sentinel)
            byteArrayPointers = new IntPtr[numberOfStrings];
            IntPtr rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
            for (var index = 0; index < arr.Length; index++)
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

        public override string PlayerName => $"libmpv {VersionNumber}";

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

                    var mpvFormatDouble = 5;
                    double d = 0;
                    _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("duration"), mpvFormatDouble, ref d);
                    return d;
                }
            }
        }

        public double VideoBitrate
        {
            get
            {
                lock (_lockObj)
                {
                    if (_mpvHandle == IntPtr.Zero)
                    {
                        return 0;
                    }

                    var mpvFormatDouble = 5;
                    double d = 0;
                    _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("video-bitrate"), mpvFormatDouble, ref d);
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

                    var mpvFormatDouble = 5;
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

                    DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute", "exact");
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

        public void SetAudioChannelFrontCenter()
        {
            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("af"), GetUtf8Bytes("lavfi=[pan=mono|c0=FC]"));
        }

        public void SetAudioChannelFrontReset()
        {
            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("af"), GetUtf8Bytes(""));
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

        public int VideoWidth
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return 0;
                }
                var mpvFormatDouble = 5;
                double d = 0;
                _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("width"), mpvFormatDouble, ref d);
                return (int)d;
            }
        }

        public int VideoHeight
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return 0;
                }
                var mpvFormatDouble = 5;
                double d = 0;
                _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("height"), mpvFormatDouble, ref d);
                return (int)d;
            }
        }

        public int VideoTotalFrames
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return 0;
                }
                var mpvFormatDouble = 5;
                double d = 0;
                _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("estimated-frame-count"), mpvFormatDouble, ref d);
                return (int)d;
            }
        }

        public double VideoFps
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return 0;
                }
                var mpvFormatDouble = 5;
                double d = 0;
                _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("container-fps"), mpvFormatDouble, ref d);
                return d;
            }
        }

        public string VideoCodec
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return string.Empty;
                }

                var lpBuffer = IntPtr.Zero;
                _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("video-codec"), MpvFormatString, ref lpBuffer);
                var codec = Marshal.PtrToStringAnsi(lpBuffer);
                _mpvFree(lpBuffer);
                return codec;
            }
        }

        public override bool IsPlaying => !IsPaused;

        private List<KeyValuePair<int, string>> _audioTrackIds;
        public List<KeyValuePair<int, string>> AudioTracks
        {
            get
            {
                if (_audioTrackIds != null)
                {
                    return _audioTrackIds;
                }

                if (_mpvHandle == IntPtr.Zero)
                {
                    return new List<KeyValuePair<int, string>>();
                }

                _audioTrackIds = new List<KeyValuePair<int, string>>();
                var lpBuffer = IntPtr.Zero;
                _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("track-list"), MpvFormatString, ref lpBuffer);
                string trackListJson = Marshal.PtrToStringAnsi(lpBuffer);
                foreach (var json in Json.ReadObjectArray(trackListJson))
                {
                    var trackType = Json.ReadTag(json, "type");
                    if (trackType == "audio")
                    {
                        var lang = Json.ReadTag(json, "lang");
                        if (int.TryParse(Json.ReadTag(json, "id"), out var id))
                        {
                            _audioTrackIds.Add(new KeyValuePair<int, string>(id, lang));
                        }
                    }
                }
                _mpvFree(lpBuffer);
                return _audioTrackIds;
            }
        }

        public int AudioTrackNumber
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return 0;
                }

                var lpBuffer = IntPtr.Zero;
                _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("aid"), MpvFormatString, ref lpBuffer);
                var numberString = Marshal.PtrToStringAnsi(lpBuffer);
                if (string.IsNullOrEmpty(numberString))
                {
                    return 0;
                }

                if (!int.TryParse(numberString, out var id))
                {
                    return 0;
                }

                var idx = _audioTrackIds.FindIndex(x => x.Key == id);
                var number = AudioTracks.Count > 1 && idx != -1 ? idx : 0;
                _mpvFree(lpBuffer);
                return number;
            }
            set
            {
                string id = "1";
                if (AudioTracks.Count > 1 && value >= 0 && value < _audioTrackIds.Count)
                {
                    id = _audioTrackIds[value].Key.ToString();
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

        public void LoadSecondSubtitle(string fileName)
        {
            _secondSubtitleFileName = fileName;
            DoMpvCommand("sub-add", fileName, "select");
        }

        public void RemoveSubtitle()
        {
            if (!string.IsNullOrEmpty(_secondSubtitleFileName))
            {
                return;
            }

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
                        return LoadLib();
                    }

                    var dllFile = GetMpvPath("libmpv-2.dll");
                    if (File.Exists(dllFile))
                    {
                        return File.Exists(dllFile);
                    }

                    dllFile = GetMpvPath("mpv-2.dll");
                    if (File.Exists(dllFile))
                    {
                        return File.Exists(dllFile);
                    }

                    dllFile = GetMpvPath("mpv-1.dll");
                    return File.Exists(dllFile);
                }
                catch (Exception ex)
                {
                    SeLogger.Error(ex, "LibMpvDynamic.IsInstalled failed");
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
                if (Configuration.IsRunningOnWindows)
                {
                    var mpvPath = GetMpvPath("libmpv-2.dll");
                    if (!File.Exists(mpvPath))
                    {
                        mpvPath = GetMpvPath("mpv-2.dll");
                    }

                    if (!File.Exists(mpvPath))
                    {
                        mpvPath = GetMpvPath("mpv-1.dll");
                    }

                    _libMpvDll = NativeMethods.CrossLoadLibrary(mpvPath);

                    if (_libMpvDll == IntPtr.Zero)
                    {
                        // to work with e.g. cyrillic characters!
                        Directory.SetCurrentDirectory(Configuration.DataDirectory);
                        if (File.Exists("mpv-2.dll"))
                        {
                            _libMpvDll = NativeMethods.CrossLoadLibrary("mpv-2.dll");
                        }
                        else
                        {
                            _libMpvDll = NativeMethods.CrossLoadLibrary("mpv-1.dll");
                        }
                    }
                }
                else
                {
                    _libMpvDll = NativeMethods.CrossLoadLibrary("libmpv.so");
                    if (_libMpvDll == IntPtr.Zero)
                    {
                        _libMpvDll = NativeMethods.CrossLoadLibrary("libmpv.so.1");
                    }
                    if (_libMpvDll == IntPtr.Zero)
                    {
                        _libMpvDll = NativeMethods.CrossLoadLibrary("libmpv.so.2");
                    }

                    int i = 107;
                    while (_libMpvDll == IntPtr.Zero && i < 120)
                    {
                        _libMpvDll = NativeMethods.CrossLoadLibrary($"libmpv.so.1.{i}.0");
                        i++;
                    }

                    i = 107;
                    while (_libMpvDll == IntPtr.Zero && i < 120)
                    {
                        _libMpvDll = NativeMethods.CrossLoadLibrary($"libmpv.so.2.{i}.0");
                        i++;
                    }
                }
            }

            return _libMpvDll != IntPtr.Zero;
        }

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            _secondSubtitleFileName = null;
            if (LoadLib())
            {
                LoadLibMpvDynamic();
                if (!IsAllMethodsLoaded())
                {
                    throw new Exception("MPV - not all needed methods found in dll file");
                }
                _mpvHandle = _mpvCreate.Invoke();

                if (Configuration.Settings.General.MpvLogging)
                {
                    var logFileName = Path.Combine(Configuration.DataDirectory, "mpv-log-" + Guid.NewGuid() + ".txt");
                    _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("log-file"), GetUtf8Bytes(logFileName));
                }

                
                if (_mpvSetOptionString(_mpvHandle, GetUtf8Bytes("input-cursor-passthrough"), GetUtf8Bytes("yes")) != 0)
                {
                    // if --input-cursor-passthrough=yes is not avaliable, use --input-cursor=no
                    _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("input-cursor"), GetUtf8Bytes("no"));
                }
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

                string videoOutput = string.Empty;
                if (Configuration.IsRunningOnLinux)
                {
                    videoOutput = Configuration.Settings.General.MpvVideoOutputLinux;
                }
                else if (!string.IsNullOrWhiteSpace(Configuration.Settings.General.MpvVideoOutputWindows))
                {
                    videoOutput = Configuration.Settings.General.MpvVideoOutputWindows;
                }

                if (!string.IsNullOrEmpty(videoOutput))
                {
                    _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("vo"), GetUtf8Bytes(videoOutput));
                }

                if (!string.IsNullOrEmpty(Configuration.Settings.General.MpvVideoVf))
                {
                    _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("vf"), GetUtf8Bytes(Configuration.Settings.General.MpvVideoVf));
                }

                if (!string.IsNullOrEmpty(Configuration.Settings.General.MpvVideoAf))
                {
                    _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("af"), GetUtf8Bytes(Configuration.Settings.General.MpvVideoAf));
                }

                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("keep-open"), GetUtf8Bytes("always")); // don't auto close video
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("no-sub"), GetUtf8Bytes(string.Empty)); // don't load subtitles (does not seem to work anymore)
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("sid"), GetUtf8Bytes("no")); // don't load subtitles
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("hr-seek"), GetUtf8Bytes("yes")); // don't load subtitles

                if (videoFileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    videoFileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("ytdl"), GetUtf8Bytes("yes"));
                }

                if (!string.IsNullOrEmpty(Configuration.Settings.General.MpvExtraOptions))
                {
                    var options = Configuration.Settings.General.MpvExtraOptions.Split(' ');
                    if (options?.Length > 0)
                    {
                        foreach (var option in options)
                        {
                            var parts = option.TrimStart('-').Split('=');
                            if (parts.Length == 2)
                            {
                                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes(parts[0]), GetUtf8Bytes(parts[1]));
                            }
                            else
                            {
                                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes(option), GetUtf8Bytes(string.Empty));
                            }
                        }
                    }
                }

                DoMpvCommand("loadfile", videoFileName);

                System.Threading.Thread.Sleep(100);
                SetVideoOwner(ownerControl);

                _videoLoadedTimer = new Timer { Interval = 50 };
                _videoLoadedTimer.Tick += VideoLoadedTimer_Tick;
                _videoLoadedTimer.Start();

                SetVideoOwner(ownerControl);

                VideoFileName = videoFileName;
            }
        }

        private void SetVideoOwner(Control ownerControl)
        {
            if (ownerControl != null)
            {
                var iterations = 25;
                var returnCode = -1;
                var mpvFormatInt64 = 4;
                if (ownerControl.IsDisposed)
                {
                    return;
                }

                var windowId = (ulong)ownerControl.Handle;
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

            Pause();
        }

        private void VideoLoadedTimer_Tick(object sender, EventArgs e)
        {
            _videoLoadedTimer.Stop();
            const int mpvEventFileLoaded = 8;
            var l = 0;
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
            Pause();
            Application.DoEvents();
            OnVideoLoaded?.Invoke(this, null);
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

        public int ToggleBrightness()
        {
            _brightness += 25;
            if (_brightness > 100)
            {
                _brightness = -100;
            }

            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("brightness"), GetUtf8Bytes(_brightness.ToString(CultureInfo.InvariantCulture)));
            return _brightness;
        }

        public int ToggleContrast()
        {
            _contrast += 25;
            if (_contrast > 100)
            {
                _contrast = -100;
            }

            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("contrast"), GetUtf8Bytes(_contrast.ToString(CultureInfo.InvariantCulture)));
            return _contrast;
        }

        internal static VideoInfo GetVideoInfo(string fileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                var libmpv = new LibMpvDynamic();
                libmpv.Initialize(null, fileName, null, null);

                for (int i = 0; i < 10; i++)
                {
                    System.Threading.Thread.Sleep(10);
                    Application.DoEvents();
                }

                info.Width = libmpv.VideoWidth;
                info.Height = libmpv.VideoHeight;
                info.TotalSeconds = libmpv.Duration;
                info.TotalMilliseconds = info.TotalSeconds * 1000.0;
                info.TotalFrames = libmpv.VideoTotalFrames;
                info.VideoCodec = libmpv.VideoCodec;
                info.FramesPerSecond = libmpv.VideoFps;
                info.FileType = Path.GetExtension(fileName).TrimStart('.');
                info.Success = true;
                libmpv.HardDispose();
            }
            catch
            {
                // ignored
            }

            return info;
        }
    }
}
