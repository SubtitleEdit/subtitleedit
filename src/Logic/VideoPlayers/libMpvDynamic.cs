﻿using Nikse.SubtitleEdit.Core;
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
        private delegate int MpvTerminateDestroy(IntPtr mpvHandle);
        private MpvTerminateDestroy _mpvTerminateDestroy;

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
        private delegate UInt32 MpvClientApiVersion();
        private MpvClientApiVersion _mpvClientApiVersion;

        #endregion

        private IntPtr _libMpvDll;
        private IntPtr _mpvHandle;
        private Timer _videoLoadedTimer;
        private double? _pausePosition = null; // Hack to hold precise seeking when paused
        //        private Timer _videoEndedTimer;

        public override event EventHandler OnVideoLoaded;
        public override event EventHandler OnVideoEnded;

        private const int MpvFormatString = 1;

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
                return;

            IntPtr[] byteArrayPointers;
            var mainPtr = AllocateUtf8IntPtrArrayWithSentinel(args, out byteArrayPointers);
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
            get
            {
                return _volume;
            }
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
                    return 0;

                int mpvFormatDouble = 5;
                double d = 0;
                _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("duration"), mpvFormatDouble, ref d);
                return d;
            }
        }

        public override double CurrentPosition
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                    return 0;

                if (_pausePosition != null)
                {
                    if (_pausePosition < 0)
                        return 0;
                    return _pausePosition.Value;
                }

                int mpvFormatDouble = 5;
                double d = 0;
                _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("time-pos"), mpvFormatDouble, ref d);
                return d;
            }
            set
            {
                if (_mpvHandle == IntPtr.Zero)
                    return;

                if (IsPaused && value <= Duration)
                    _pausePosition = value;

                DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute");
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
                DoMpvCommand("set", "speed", value.ToString(CultureInfo.InvariantCulture));
                _playRate = value;
            }
        }

        public void GetNextFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            DoMpvCommand("frame-step");
        }

        public void GetPreviousFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            DoMpvCommand("frame-back-step");
        }

        public override void Play()
        {
            _pausePosition = null;

            if (_mpvHandle == IntPtr.Zero)
                return;

            var bytes = GetUtf8Bytes("no");
            _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
        }

        public override void Pause()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            var bytes = GetUtf8Bytes("yes");
            _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
        }

        public override void Stop()
        {
            Pause();
            _pausePosition = null;
            CurrentPosition = 0;
        }

        public override bool IsPaused
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                    return true;

                var lpBuffer = IntPtr.Zero;
                _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref lpBuffer);
                var isPaused = Marshal.PtrToStringAnsi(lpBuffer) == "yes";
                _mpvFree(lpBuffer);
                return isPaused;
            }
        }

        public override bool IsPlaying
        {
            get { return !IsPaused; }
        }

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
                    return string.Empty;

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

            var path = Path.Combine(Configuration.DataDirectory, fileName);
            if (File.Exists(path))
                return path;

            return null;
        }

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            string dllFile = GetMpvPath("mpv-1.dll");
            if (File.Exists(dllFile))
            {
                var path = Path.GetDirectoryName(dllFile);
                if (path != null)
                    Directory.SetCurrentDirectory(path);
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

                string videoOutput = "direct3d";
                if (!string.IsNullOrWhiteSpace(Configuration.Settings.General.MpvVideoOutput))
                    videoOutput = Configuration.Settings.General.MpvVideoOutput;
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("vo"), GetUtf8Bytes(videoOutput)); // use "direct3d" or "opengl"

                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("keep-open"), GetUtf8Bytes("always")); // don't auto close video
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("no-sub"), GetUtf8Bytes("")); // don't load subtitles
                if (ownerControl != null)
                {
                    int mpvFormatInt64 = 4;
                    var windowId = ownerControl.Handle.ToInt64();
                    _mpvSetOption(_mpvHandle, GetUtf8Bytes("wid"), mpvFormatInt64, ref windowId);
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

        //private void VideoEndedTimer_Tick(object sender, EventArgs e)
        //{
        //}

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

                    //if (_libMpvDll != IntPtr.Zero) - hm, will make video hang on second video...
                    //{
                    //    NativeMethods.FreeLibrary(_libMpvDll);
                    //    _libMpvDll = IntPtr.Zero;
                    //}
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

        public void HardDispose()
        {
            DoMpvCommand("stop");
            Application.DoEvents();
            DoMpvCommand("quit");
            Application.DoEvents();
            Dispose();
            NativeMethods.FreeLibrary(_libMpvDll);
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmangedResources();
        }

    }
}
