using System;
using System.Diagnostics;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class MPlayer : VideoPlayer
    {
        private Process _mplayer;
        private System.Windows.Forms.Timer timer;
        private TimeSpan _lengthInSeconds;
        private TimeSpan _lastLengthInSeconds = TimeSpan.FromDays(0);
        private bool _paused;
        private bool _loaded = false;
        private bool _ended = false;
        private bool _waitForChange = false;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float FramesPerSecond { get; private set; }
        public string VideoFormat { get; private set; }
        public string VideoCodec { get; private set; }

        public override string PlayerName
        {
            get { return "MPlayer"; }
        }

        float _volume;
        public override int Volume
        {
            get
            {
                return (int)_volume;
            }
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _volume = value;
                    SetProperty("volume", value.ToString(), true);
                }
            }
        }

        public override double Duration
        {
            get { return _lengthInSeconds.TotalSeconds; }
        }

        double _timePosition;
        public override double CurrentPosition
        {
            get
            {
                return _timePosition;
            }
            set
            {
                // NOTE: FOR ACCURATE SEARCH USE MPlayer2 - http://www.mplayer2.org/)
                _timePosition = value;
                _mplayer.StandardInput.WriteLine(string.Format("pausing_keep seek {0:0.0} 2", value));
                _mplayer.StandardInput.Flush();
            }
        }

        public override void Play()
        {
//            SetProperty("pause", "1", false);
            _mplayer.StandardInput.WriteLine("pause");
            _mplayer.StandardInput.Flush();
            _paused = false;
        }

        public override void Pause()
        {
            //_mplayer.StandardInput.WriteLine("pausing set_property pause 1");
            _mplayer.StandardInput.WriteLine("pause");
            _mplayer.StandardInput.Flush();
            _paused = true;
        }

        public override void Stop()
        {
            _mplayer.StandardInput.WriteLine("stop");
            _mplayer.StandardInput.Flush();
            _paused = true;
            _lastLengthInSeconds = _lengthInSeconds;
        }

        public override bool IsPaused
        {
            get { return _paused; }
        }

        public override bool IsPlaying
        {
            get { return !_paused; }
        }

        public override void Initialize(System.Windows.Forms.Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            _loaded = false;
            string mplayerExeName = GetMPlayerFileName;
            if (!string.IsNullOrEmpty(mplayerExeName))
            {
                _mplayer = new Process();
                _mplayer.StartInfo.FileName = mplayerExeName;
                //vo options: gl, gl2, directx:noaccel
                if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
                    _mplayer.StartInfo.Arguments = "-slave -idle -quiet -osdlevel 0 -vsync -wid " + ownerControl.Handle.ToInt32() + " \"" + videoFileName + "\" ";
                else
                    _mplayer.StartInfo.Arguments = "-slave -idle -quiet -osdlevel 0 -vsync -vo directx:noaccel -wid " + ownerControl.Handle.ToInt32() + " \"" + videoFileName + "\" ";
                _mplayer.StartInfo.UseShellExecute = false;
                _mplayer.StartInfo.RedirectStandardInput = true;
                _mplayer.StartInfo.RedirectStandardOutput = true;
                _mplayer.StartInfo.CreateNoWindow = true;
                _mplayer.OutputDataReceived += new DataReceivedEventHandler(MPlayerOutputDataReceived);
                _mplayer.Start();
                _mplayer.StandardInput.NewLine = "\n";
                _mplayer.BeginOutputReadLine(); // Async reading of output to prevent deadlock

                // static properties
                GetProperty("width", true);
                GetProperty("height", true);
                GetProperty("fps", true);
                GetProperty("video_format", true);
                GetProperty("video_codec", true);
                GetProperty("length", true);

                // semi static variable
                GetProperty("volume", true);

                // start timer to collect variable properties
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 1000;
                timer.Tick += new EventHandler(timer_Tick);
                timer.Start();

                OnVideoLoaded = onVideoLoaded;
                OnVideoEnded = onVideoEnded;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            // variable properties
            _mplayer.StandardInput.WriteLine("pausing_keep_force get_property time_pos");
            _mplayer.StandardInput.WriteLine("pausing_keep_force get_property pause");
            _mplayer.StandardInput.Flush();

            if (!_ended && OnVideoEnded != null && _lengthInSeconds.TotalSeconds == Duration)
            {
                _ended = true;
                OnVideoEnded.Invoke(this, null);
            }
            else if (_lengthInSeconds.TotalSeconds < Duration)
            {
                _ended = false;
            }

            if (OnVideoLoaded != null && _loaded == true)
            {
                timer.Stop();
                _loaded = false;
                OnVideoLoaded.Invoke(this, null);
                timer.Interval = 200;
                timer.Start();
            }

            if (_lengthInSeconds != _lastLengthInSeconds)
                _paused = false;
            _lastLengthInSeconds = _lengthInSeconds;
        }

        void MPlayerOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            if (e.Data.StartsWith("Playing "))
                _loaded = true;

            int indexOfEqual = e.Data.IndexOf("=");
            if (indexOfEqual > 0 && indexOfEqual + 1 < e.Data.Length && e.Data.StartsWith("ANS_"))
            {
                string code = e.Data.Substring(0, indexOfEqual);
                string value = e.Data.Substring(indexOfEqual + 1);

                switch (code)
                {
                    // Examples:
                    //  ANS_time_pos=8.299958, ANS_width=624, ANS_height=352, ANS_fps=23.976025, ANS_video_format=1145656920, ANS_video_format=1145656920, ANS_video_codec=ffodivx,
                    //  ANS_length=1351.600213, ANS_volume=100.000000
                    case "ANS_time_pos":
                        _timePosition = Convert.ToDouble(value);
                        break;
                    case "ANS_width":
                        Width = Convert.ToInt32(value);
                        break;
                    case "ANS_height":
                        Height = Convert.ToInt32(value);
                        break;
                    case "ANS_fps":
                        FramesPerSecond = (float)Convert.ToDouble(value);
                        break;
                    case "ANS_video_format":
                        VideoFormat = value;
                        break;
                    case "ANS_video_codec":
                        VideoCodec = value;
                        break;
                    case "ANS_length":
                        _lengthInSeconds = TimeSpan.FromSeconds(Convert.ToDouble(value));
                        break;
                    case "ANS_volume":
                        _volume = (float)Convert.ToDouble(value);
                        break;
                    case "ANS_pause":
                        _paused = value == "yes" || value == "1";
                        break;
                }
                _waitForChange = false;
            }
        }

        public static string GetMPlayerFileName
        {
            get
            {
                if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
                    return "mplayer";

                string fileName = Path.Combine(Configuration.BaseDirectory, "mplayer.exe");
                if (File.Exists(fileName))
                    return fileName;

                fileName = @"C:\Program Files (x86)\SMPlayer\mplayer\mplayer.exe";
                if (File.Exists(fileName))
                    return fileName;

                fileName = @"C:\Program Files (x86)\mplayer\mplayer.exe";
                if (File.Exists(fileName))
                    return fileName;

                fileName = @"C:\Program Files\mplayer\mplayer.exe";
                if (File.Exists(fileName))
                    return fileName;

                fileName = @"C:\Program Files\SMPlayer\mplayer\mplayer.exe";
                if (File.Exists(fileName))
                    return fileName;

                return null;
            }
        }

        public static bool IsInstalled
        {
            get
            {
                return GetMPlayerFileName != null;
            }
        }

        private void GetProperty(string propertyName, bool keepPause)
        {
            if (keepPause)
                _mplayer.StandardInput.WriteLine("pausing_keep get_property " + propertyName);
            else
                _mplayer.StandardInput.WriteLine("get_property " + propertyName);
            _mplayer.StandardInput.Flush();
        }

        private void SetProperty(string propertyName, string value, bool keepPause)
        {
            if (keepPause)
                _mplayer.StandardInput.WriteLine("pausing_keep set_property " + propertyName + " " + value);
            else
                _mplayer.StandardInput.WriteLine("set_property " + propertyName + " " + value);
            _mplayer.StandardInput.Flush();

            UglySleep();
        }

        private void UglySleep()
        {
            _waitForChange = true;
            int i = 0;

            while (i < 100 && _waitForChange)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(2);
                i++;
            }
            _waitForChange = false;
        }

        public override void DisposeVideoPlayer()
        {
            timer.Stop();
            if (_mplayer != null)
            {
                _mplayer.OutputDataReceived -= MPlayerOutputDataReceived;
                _mplayer.StandardInput.WriteLine("quit");
                _mplayer.StandardInput.Flush();
            }
        }

        public override event EventHandler OnVideoLoaded;

        public override event EventHandler OnVideoEnded;
    }
}
