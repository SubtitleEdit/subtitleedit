﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpcHC
{
    public class MpcHc : VideoPlayer, IDisposable
    {
        private const string ModePlay = "0";
        private const string ModePause = "1";
        private string _playMode = string.Empty;
        private int _loaded;
        private IntPtr _mpcHandle = IntPtr.Zero;
        private IntPtr _videoHandle = IntPtr.Zero;
        private IntPtr _videoPanelHandle = IntPtr.Zero;
        private ProcessStartInfo _startInfo;
        private Process _process;
        private IntPtr _messageHandlerHandle = IntPtr.Zero;
        private string _videoFileName;
        private Timer _positionTimer;
        private double _positionInSeconds;
        private double _durationInSeconds;
        private MessageHandlerWindow _form;
        private int _initialWidth;
        private int _initialHeight;
        private Timer _hideMpcTimer = new Timer();
        private int _hideMpcTimerCount;

        public override string PlayerName
        {
            get { return "MPC-HC"; }
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
                // MPC-HC moves from 0-100 in steps of 5
                for (int i = 0; i < 100; i += 5)
                    SendMpcMessage(MpcHcCommand.DecreaseVolume);
                for (_volume = 0; _volume < value; _volume += 5)
                    SendMpcMessage(MpcHcCommand.IncreaseVolume);
            }
        }

        public override double Duration
        {
            get
            {
                return _durationInSeconds;
            }
        }

        public override double CurrentPosition
        {
            get
            {
                return _positionInSeconds;
            }
            set
            {
                SendMpcMessage(MpcHcCommand.SetPosition, string.Format(CultureInfo.InvariantCulture, "{0:0.000}", value));
            }
        }

        public override void Play()
        {
            _playMode = ModePlay;
            SendMpcMessage(MpcHcCommand.Play);
        }

        public override void Pause()
        {
            _playMode = ModePause;
            SendMpcMessage(MpcHcCommand.Pause);
        }

        public override void Stop()
        {
            SendMpcMessage(MpcHcCommand.Stop);
        }

        public override bool IsPaused
        {
            get { return _playMode == ModePause; }
        }

        public override bool IsPlaying
        {
            get { return _playMode == ModePlay; }
        }

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            if (ownerControl == null)
                return;

            VideoFileName = videoFileName;
            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            _initialWidth = ownerControl.Width;
            _initialHeight = ownerControl.Height;
            _form = new MessageHandlerWindow();
            _form.OnCopyData += OnCopyData;
            _form.Show();
            _form.Hide();
            _videoPanelHandle = ownerControl.Handle;
            _messageHandlerHandle = _form.Handle;
            _videoFileName = videoFileName;
            _startInfo = new ProcessStartInfo();
            _startInfo.FileName = GetMpcHcFileName();
            _startInfo.Arguments = "/new /minimized /slave " + _messageHandlerHandle;
            _process = Process.Start(_startInfo);
            if (_process != null)
                _process.WaitForInputIdle();
            _positionTimer = new Timer { Interval = 100 };
            _positionTimer.Tick += PositionTimerTick;
        }

        private void PositionTimerTick(object sender, EventArgs e)
        {
            SendMpcMessage(MpcHcCommand.GetCurrentPosition);
        }

        private void OnCopyData(object sender, EventArgs e)
        {
            var message = (Message)sender;
            var cds = (NativeMethods.CopyDataStruct)Marshal.PtrToStructure(message.LParam, typeof(NativeMethods.CopyDataStruct));
            var command = cds.dwData.ToUInt32();
            var param = Marshal.PtrToStringAuto(cds.lpData);
            var multiParam = new string[0];
            if (param != null)
                multiParam = param.Split('|');

            switch (command)
            {
                case MpcHcCommand.Connect:
                    _positionTimer.Stop();
                    _mpcHandle = (IntPtr)Convert.ToInt64(Marshal.PtrToStringAuto(cds.lpData));
                    SendMpcMessage(MpcHcCommand.OpenFile, _videoFileName);
                    _positionTimer.Start();
                    break;
                case MpcHcCommand.PlayMode:
                    _playMode = param;
                    if (param == ModePlay && _loaded == 0)
                    {
                        _loaded = 1;
                        if (!HijackMpcHc())
                        {
                            Application.DoEvents();
                            HijackMpcHc();
                        }
                    }
                    Application.DoEvents();
                    HideMpcPlayerWindow();
                    break;
                case MpcHcCommand.NowPlaying:
                    if (_loaded == 1)
                    {
                        _loaded = 2;

                        _durationInSeconds = 5000;
                        double d;
                        if (multiParam.Length >= 5 && double.TryParse(multiParam[4].Replace(",", ".").Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                            _durationInSeconds = d;
                        else if (multiParam.Length >= 1 && double.TryParse(multiParam[multiParam.Length - 1].Replace(",", ".").Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                            _durationInSeconds = d;

                        Pause();
                        Resize(_initialWidth, _initialHeight);
                        if (OnVideoLoaded != null)
                            OnVideoLoaded.Invoke(this, new EventArgs());
                        SendMpcMessage(MpcHcCommand.SetSubtitleTrack, "-1");

                        // be sure to hide MPC
                        _hideMpcTimerCount = 20;
                        _hideMpcTimer.Interval = 100;
                        _hideMpcTimer.Tick += (o, args) =>
                        {
                            _hideMpcTimer.Stop();
                            if (_hideMpcTimerCount > 0)
                            {
                                Application.DoEvents();
                                HideMpcPlayerWindow();
                                _hideMpcTimerCount--;
                                _hideMpcTimer.Start();
                            }
                        };
                        _hideMpcTimer.Start();
                    }
                    break;
                case MpcHcCommand.NotifyEndOfStream:
                    if (OnVideoEnded != null)
                        OnVideoEnded.Invoke(this, new EventArgs());
                    break;
                case MpcHcCommand.CurrentPosition:
                    if (!string.IsNullOrWhiteSpace(param))
                    {
                        double d;
                        if (double.TryParse(param.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                            _positionInSeconds = d;
                    }
                    break;
            }
        }

        private void HideMpcPlayerWindow()
        {
            NativeMethods.ShowWindow(_process.MainWindowHandle, NativeMethods.ShowWindowCommands.Hide);
            NativeMethods.SetWindowPos(_process.MainWindowHandle, (IntPtr)NativeMethods.SpecialWindowHandles.HWND_TOP, -9999, -9999, 0, 0, NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        internal static bool GetWindowHandle(IntPtr windowHandle, ArrayList windowHandles)
        {
            windowHandles.Add(windowHandle);
            return true;
        }

        private ArrayList GetChildWindows()
        {
            var windowHandles = new ArrayList();
            NativeMethods.EnumedWindow callBackPtr = GetWindowHandle;
            NativeMethods.EnumChildWindows(_process.MainWindowHandle, callBackPtr, windowHandles);
            return windowHandles;
        }

        private static bool IsWindowMpcHcVideo(IntPtr hWnd)
        {
            var className = new StringBuilder(256);
            int returnCode = NativeMethods.GetClassName(hWnd, className, className.Capacity); // Get the window class name
            if (returnCode != 0)
                return className.ToString().EndsWith(":b:0000000000010003:0000000000000006:0000000000000000") || // MPC-HC 64-bit video class???
                       className.ToString().EndsWith(":b:00010003:00000006:00000000");                           // MPC-HC 32-bit video class???
            return false;
        }

        private bool HijackMpcHc()
        {
            IntPtr handle = _process.MainWindowHandle;
            var handles = GetChildWindows();
            foreach (var h in handles)
            {
                if (IsWindowMpcHcVideo((IntPtr)h))
                {
                    _videoHandle = (IntPtr)h;
                    NativeMethods.SetParent((IntPtr)h, _videoPanelHandle);
                    NativeMethods.SetWindowPos(handle, (IntPtr)NativeMethods.SpecialWindowHandles.HWND_TOP, -9999, -9999, 0, 0, NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE);
                    return true;
                }
            }
            return false;
        }

        public override void Resize(int width, int height)
        {
            NativeMethods.ShowWindow(_process.MainWindowHandle, NativeMethods.ShowWindowCommands.ShowNoActivate);
            NativeMethods.SetWindowPos(_videoHandle, (IntPtr)NativeMethods.SpecialWindowHandles.HWND_TOP, 0, 0, width, height, NativeMethods.SetWindowPosFlags.SWP_NOREPOSITION);
            HideMpcPlayerWindow();
        }

        public static string GetMpcHcFileName()
        {
            string path;

            if (IntPtr.Size == 8) // 64-bit
            {
                path = Path.Combine(Configuration.BaseDirectory, @"MPC-HC\mpc-hc64.exe");
                if (File.Exists(path))
                    return path;

                if (!string.IsNullOrEmpty(Configuration.Settings.General.MpcHcLocation))
                {
                    path = Path.GetDirectoryName(Configuration.Settings.General.MpcHcLocation);
                    if (path != null && (File.Exists(path) && path.EndsWith("mpc-hc64.exe", StringComparison.OrdinalIgnoreCase)))
                        return path;
                    if (Directory.Exists(Configuration.Settings.General.MpcHcLocation))
                    {
                        path = Path.Combine(Configuration.Settings.General.MpcHcLocation, @"MPC-HC\mpc-hc64.exe");
                        if (File.Exists(path))
                            return path;
                    }
                }

                path = RegistryUtil.GetValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{2ACBF1FA-F5C3-4B19-A774-B22A31F231B9}_is1", "InstallLocation");
                if (path != null)
                {
                    path = Path.Combine(path, "mpc-hc64.exe");
                    if (File.Exists(path))
                        return path;
                }

                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"MPC-HC\mpc-hc64.exe");
                if (File.Exists(path))
                    return path;

                path = @"C:\Program Files\MPC-HC\mpc-hc64.exe";
                if (File.Exists(path))
                    return path;

                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"K-Lite Codec Pack\MPC-HC\mpc-hc64.exe");
                if (File.Exists(path))
                    return path;

                path = @"C:\Program Files (x86)\K-Lite Codec Pack\MPC-HC64\mpc-hc64.exe";
                if (File.Exists(path))
                    return path;

                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"K-Lite Codec Pack\MPC-HC64\mpc-hc64.exe");
                if (File.Exists(path))
                    return path;

                path = @"C:\Program Files (x86)\MPC-HC\mpc-hc64.exe";
                if (File.Exists(path))
                    return path;
            }
            else
            {
                path = Path.Combine(Configuration.BaseDirectory, @"MPC-HC\mpc-hc.exe");
                if (File.Exists(path))
                    return path;

                if (!string.IsNullOrEmpty(Configuration.Settings.General.MpcHcLocation))
                {
                    path = Path.GetDirectoryName(Configuration.Settings.General.MpcHcLocation);
                    if (path != null && File.Exists(path) && path.EndsWith("mpc-hc.exe", StringComparison.OrdinalIgnoreCase))
                        return path;
                    if (Directory.Exists(Configuration.Settings.General.MpcHcLocation))
                    {
                        path = Path.Combine(Configuration.Settings.General.MpcHcLocation, @"MPC-HC\mpc-hc.exe");
                        if (File.Exists(path))
                            return path;
                    }
                }

                path = RegistryUtil.GetValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{2624B969-7135-4EB1-B0F6-2D8C397B45F7}_is1", "InstallLocation");
                if (path != null)
                {
                    path = Path.Combine(path, "mpc-hc.exe");
                    if (File.Exists(path))
                        return path;
                }

                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"MPC-HC\mpc-hc.exe");
                if (File.Exists(path))
                    return path;

                path = @"C:\Program Files (x86)\MPC-HC\mpc-hc.exe";
                if (File.Exists(path))
                    return path;

                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"K-Lite Codec Pack\MPC-HC\mpc-hc.exe");
                if (File.Exists(path))
                    return path;

                path = @"C:\Program Files\MPC-HC\mpc-hc.exe";
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        public static bool IsInstalled
        {
            get { return true; }
        }

        public override void DisposeVideoPlayer()
        {
            Dispose();
        }

        public override event EventHandler OnVideoLoaded;

        public override event EventHandler OnVideoEnded;

        private void ReleaseUnmangedResources()
        {
            try
            {
                lock (this)
                {
                    if (_mpcHandle != IntPtr.Zero)
                    {
                        SendMpcMessage(MpcHcCommand.CloseApplication);
                        _mpcHandle = IntPtr.Zero;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        ~MpcHc()
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
            try
            {
                if (disposing)
                {
                    // release managed resources
                    if (_positionTimer != null)
                    {
                        _positionTimer.Stop();
                        _positionTimer.Dispose();
                        _positionTimer = null;
                    }
                    if (_hideMpcTimer != null)
                    {
                        _hideMpcTimer.Stop();
                        _hideMpcTimer.Dispose();
                        _hideMpcTimer = null;
                    }

                    if (_form != null)
                    {
                        _form.OnCopyData -= OnCopyData;
                        //_form.Dispose(); this gives an error when doing File -> Exit...
                        _form = null;
                    }

                    if (_process != null)
                    {
                        _process.Dispose();
                        _process = null;
                    }
                    _startInfo = null;
                }
                ReleaseUnmangedResources();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void SendMpcMessage(uint command)
        {
            SendMpcMessage(command, string.Empty);
        }

        private void SendMpcMessage(uint command, string parameter)
        {
            if (_mpcHandle == IntPtr.Zero || _messageHandlerHandle == IntPtr.Zero)
                return;

            parameter += (char)0;
            NativeMethods.CopyDataStruct cds;
            cds.dwData = (UIntPtr)command;
            cds.cbData = parameter.Length * Marshal.SystemDefaultCharSize;
            cds.lpData = Marshal.StringToCoTaskMemAuto(parameter);
            NativeMethods.SendMessage(_mpcHandle, NativeMethods.WindowsMessageCopyData, _messageHandlerHandle, ref cds);
        }

    }
}
