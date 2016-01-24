using Nikse.SubtitleEdit.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class MPlayer : VideoPlayer, IDisposable
    {
        private Process _mplayer;
        private Timer _timer;
        private TimeSpan _lengthInSeconds;
        private TimeSpan _lastLengthInSeconds = TimeSpan.FromDays(0);
        private bool _paused;
        private bool _loaded = false;
        private bool _ended = false;
        private string _videoFileName;
        private bool _waitForChange = false;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float FramesPerSecond { get; private set; }
        public string VideoFormat { get; private set; }
        public string VideoCodec { get; private set; }
        private double? _pausePosition = null; // Hack to hold precise seeking when paused
        private int _pauseCounts = 0;
        private double _speed = 1.0;

        public override string PlayerName
        {
            get { return "MPlayer"; }
        }

        private float _volume;

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

        private double _timePosition;

        public override double CurrentPosition
        {
            get
            {
                if (_paused && _pausePosition != null)
                {
                    if (_pausePosition < 0)
                        return 0;
                    return _pausePosition.Value;
                }
                return _timePosition;
            }
            set
            {
                // NOTE: FOR ACCURATE SEARCH USE MPlayer2 - http://www.mplayer2.org/)
                _timePosition = value;
                if (IsPaused && value <= Duration)
                    _pausePosition = value;
                _mplayer.StandardInput.WriteLine(string.Format("pausing_keep seek {0:0.0} 2", value));
            }
        }

        public override double PlayRate
        {
            get
            {
                return _speed;
            }
            set
            {
                if (value >= 0 && value <= 2.0)
                {
                    _speed = value;
                    SetProperty("speed", value.ToString(CultureInfo.InvariantCulture), true);
                }
            }
        }

        public override void Play()
        {
            _mplayer.StandardInput.WriteLine("pause");
            _pauseCounts = 0;
            _paused = false;
            _pausePosition = null;
        }

        public override void Pause()
        {
            if (!_paused)
                _mplayer.StandardInput.WriteLine("pause");
            _pauseCounts = 0;
            _paused = true;
        }

        public override void Stop()
        {
            CurrentPosition = 0;
            Pause();
            _mplayer.StandardInput.WriteLine("pausing_keep_force seek 0 2");
            _pauseCounts = 0;
            _paused = true;
            _lastLengthInSeconds = _lengthInSeconds;
            _pausePosition = null;
        }

        public override bool IsPaused
        {
            get { return _paused; }
        }

        public override bool IsPlaying
        {
            get { return !_paused; }
        }

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            _loaded = false;
            _videoFileName = videoFileName;
            string mplayerExeName = GetMPlayerFileName;
            if (!string.IsNullOrEmpty(mplayerExeName))
            {
                _mplayer = new Process();
                _mplayer.StartInfo.FileName = mplayerExeName;
                //vo options: gl, gl2, directx:noaccel
                if (Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac())
                    _mplayer.StartInfo.Arguments = "-nofs -quiet -slave -idle -nosub -noautosub -loop 0 -osdlevel 0 -vsync -wid " + ownerControl.Handle.ToInt32() + " \"" + videoFileName + "\" ";
                else
                    _mplayer.StartInfo.Arguments = "-nofs -quiet -slave -idle -nosub -noautosub -loop 0 -osdlevel 0 -vo direct3d -wid " + (int)ownerControl.Handle + " \"" + videoFileName + "\" ";

                _mplayer.StartInfo.UseShellExecute = false;
                _mplayer.StartInfo.RedirectStandardInput = true;
                _mplayer.StartInfo.RedirectStandardOutput = true;
                _mplayer.StartInfo.CreateNoWindow = true;
                _mplayer.OutputDataReceived += MPlayerOutputDataReceived;

                try
                {
                    _mplayer.Start();
                }
                catch
                {
                    MessageBox.Show("Unable to start MPlayer - make sure MPlayer is installed!");
                    throw;
                }

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
                _timer = new Timer();
                _timer.Interval = 1000;
                _timer.Tick += timer_Tick;
                _timer.Start();

                OnVideoLoaded = onVideoLoaded;
                OnVideoEnded = onVideoEnded;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // variable properties
            _mplayer.StandardInput.WriteLine("pausing_keep_force get_property time_pos");
            _mplayer.StandardInput.WriteLine("pausing_keep_force get_property pause");

            if (!_ended && OnVideoEnded != null && _lengthInSeconds.TotalSeconds == Duration)
            {
                //  _ended = true;
                //  OnVideoEnded.Invoke(this, null);
            }
            else if (_lengthInSeconds.TotalSeconds < Duration)
            {
                _ended = false;
            }

            if (OnVideoLoaded != null && _loaded)
            {
                _timer.Stop();
                _loaded = false;
                OnVideoLoaded.Invoke(this, null);
                _timer.Interval = 100;
                _timer.Start();
            }

            if (_lengthInSeconds != _lastLengthInSeconds)
                _paused = false;
            _lastLengthInSeconds = _lengthInSeconds;
        }

        private void MPlayerOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            System.Diagnostics.Debug.WriteLine("MPlayer: " + e.Data);

            if (e.Data.StartsWith("Playing "))
            {
                _loaded = true;
                return;
            }

            if (e.Data.StartsWith("Exiting..."))
            {
                _ended = true;
                if (_loaded)
                {
                    _mplayer.StandardInput.WriteLine("loadfile " + _videoFileName);
                    if (OnVideoEnded != null)
                        OnVideoEnded.Invoke(this, null);
                }
                return;
            }

            int indexOfEqual = e.Data.IndexOf('=');
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
                        _timePosition = Convert.ToDouble(value.Replace(",", "."), CultureInfo.InvariantCulture);
                        break;
                    case "ANS_width":
                        Width = Convert.ToInt32(value);
                        break;
                    case "ANS_height":
                        Height = Convert.ToInt32(value);
                        break;
                    case "ANS_fps":
                        double d;
                        if (double.TryParse(value, out d))
                            FramesPerSecond = (float)Convert.ToDouble(value.Replace(",", "."), CultureInfo.InvariantCulture);
                        else
                            FramesPerSecond = 25.0f;
                        break;
                    case "ANS_video_format":
                        VideoFormat = value;
                        break;
                    case "ANS_video_codec":
                        VideoCodec = value;
                        break;
                    case "ANS_length":
                        _lengthInSeconds = TimeSpan.FromSeconds(Convert.ToDouble(value.Replace(",", "."), CultureInfo.InvariantCulture));
                        break;
                    case "ANS_volume":
                        _volume = (float)Convert.ToDouble(value.Replace(",", "."), CultureInfo.InvariantCulture);
                        break;
                    case "ANS_pause":
                        if (value == "yes" || value == "1")
                            _pauseCounts++;
                        else
                            _pauseCounts--;
                        if (_pauseCounts > 3)
                            _paused = true;
                        else if (_pauseCounts < -3)
                        {
                            _paused = false;
                            _pausePosition = null;
                        }
                        else if (Math.Abs(_pauseCounts) > 10)
                            _pauseCounts = 0;
                        break;
                }
                _waitForChange = false;
            }
        }

        public static string GetMPlayerFileName
        {
            get
            {
                if (Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac())
                    return "mplayer";

                string fileName = Path.Combine(Configuration.BaseDirectory, "mplayer2.exe");
                if (File.Exists(fileName))
                    return fileName;

                fileName = Path.Combine(Configuration.BaseDirectory, "mplayer.exe");
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
        }

        private void SetProperty(string propertyName, string value, bool keepPause)
        {
            if (keepPause)
                _mplayer.StandardInput.WriteLine("pausing_keep set_property " + propertyName + " " + value);
            else
                _mplayer.StandardInput.WriteLine("set_property " + propertyName + " " + value);

            UglySleep();
        }

        private void UglySleep()
        {
            _waitForChange = true;
            int i = 0;

            while (i < 100 && _waitForChange)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(2);
                i++;
            }
            _waitForChange = false;
        }

        public override void DisposeVideoPlayer()
        {
            _timer.Stop();
            if (_mplayer != null)
            {
                _mplayer.OutputDataReceived -= MPlayerOutputDataReceived;
                _mplayer.StandardInput.WriteLine("quit");
            }
        }

        public override event EventHandler OnVideoLoaded;

        public override event EventHandler OnVideoEnded;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_mplayer != null)
                {
                    _mplayer.Dispose();
                    _mplayer = null;
                }

                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

    }
}
