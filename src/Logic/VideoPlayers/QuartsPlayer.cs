using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using QuartzTypeLib;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class QuartsPlayer : VideoPlayer
    {
        public override event EventHandler OnVideoLoaded;
        public override event EventHandler OnVideoEnded;

        private QuartzTypeLib.IVideoWindow _quartzVideo;
        private FilgraphManagerClass _quartzFilgraphManager;
        private bool _isPaused;
        private Control _owner;
        private System.Windows.Forms.Timer _videoEndTimer;
        private BackgroundWorker _videoLoader;
        int _sourceWidth;
        int _sourceHeight;          

        public override string PlayerName { get { return "DirectShow"; } }

        /// <summary>
        /// In DirectX -10000 is silent and 0 is full volume.
        /// Also, -3500 to 0 seems to be all you can hear! Not much use for -3500 to -9999...
        /// </summary>
        public override int Volume
        {
            get
            {
                try
                {
                    return (_quartzFilgraphManager.Volume / 35) + 100;
                }
                catch 
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    if (value == 0)
                        _quartzFilgraphManager.Volume = -10000;
                    else
                        _quartzFilgraphManager.Volume = (value - 100) * 35;
                }
                catch
                {
                }
            }
        }

        public override double Duration
        {
            get { return _quartzFilgraphManager.Duration; }
        }

        public override double CurrentPosition
        {
            get { return _quartzFilgraphManager.CurrentPosition; }
            set
            {
                if (value >= 0 && value <= Duration)
                    _quartzFilgraphManager.CurrentPosition = value;
            }
        }

        public override void Play()
        {
            _quartzFilgraphManager.Run();
            _isPaused = false;
        }

        public override void Pause()
        {
            _quartzFilgraphManager.Pause();
            _isPaused = true;
        }

        public override void Stop()
        {
            _quartzFilgraphManager.Stop();
            _isPaused = true;
        }

        public override bool IsPaused
        {
            get
            {
                return _isPaused;
            }
        }

        public override bool IsPlaying
        {
            get 
            {
                return !IsPaused;
            }
        }

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            const int wsChild = 0x40000000;

            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            VideoFileName = videoFileName;
            _owner = ownerControl;
            _quartzFilgraphManager = new FilgraphManagerClass();
            _quartzFilgraphManager.RenderFile(VideoFileName);
            _quartzVideo = _quartzFilgraphManager;
            _quartzVideo.Owner = (int)ownerControl.Handle;
            _quartzVideo.SetWindowPosition(0, 0, ownerControl.Width, ownerControl.Height);
            _quartzVideo.WindowStyle = wsChild;
            _quartzFilgraphManager.Run();
            _quartzFilgraphManager.GetVideoSize(out _sourceWidth, out _sourceHeight);
            _owner.Resize += OwnerControlResize;

            if (OnVideoLoaded != null)
            {
                _videoLoader = new BackgroundWorker();
                _videoLoader.RunWorkerCompleted += VideoLoaderRunWorkerCompleted;
                _videoLoader.DoWork += VideoLoaderDoWork; 
                _videoLoader.RunWorkerAsync();               
            }

            OwnerControlResize(this, null);
            _videoEndTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _videoEndTimer.Tick += VideoEndTimerTick;
            _videoEndTimer.Start();

           _quartzVideo.MessageDrain =  (int)ownerControl.Handle;
        }

        public static VideoInfo GetVideoInfo(string videoFileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                var quartzFilgraphManager = new FilgraphManagerClass();
                quartzFilgraphManager.RenderFile(videoFileName);
                int width;
                int height;
                quartzFilgraphManager.GetVideoSize(out width, out height);

                info.Width = width;
                info.Height = height;      
                if (quartzFilgraphManager.AvgTimePerFrame > 0)
                    info.FramesPerSecond = 1 / quartzFilgraphManager.AvgTimePerFrame;
                info.Success = true;
                info.TotalMilliseconds = quartzFilgraphManager.Duration * 1000;
                info.TotalSeconds = quartzFilgraphManager.Duration;
                info.TotalFrames = info.TotalSeconds * info.FramesPerSecond;
                info.VideoCodec = string.Empty; // TODO... get real codec names from quartzFilgraphManager.FilterCollection;

                Marshal.ReleaseComObject(quartzFilgraphManager);
            }
            catch
            {

            }
            return info;
        }

        void VideoLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            while (CurrentPosition < 1 && i < 100)
            {
                Application.DoEvents();
                Thread.Sleep(5);
                i++;
            }
        }

        void VideoLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (OnVideoLoaded != null)
            {
                try
                {
                    OnVideoLoaded.Invoke(_quartzFilgraphManager, new EventArgs());
                }
                catch
                { 
                }
            }
            _videoEndTimer = null;
        }

        void VideoEndTimerTick(object sender, EventArgs e)
        {
            if (_isPaused == false && _quartzFilgraphManager != null && CurrentPosition >= Duration)
            {
                _isPaused = true;
                if (OnVideoEnded != null)
                    OnVideoEnded.Invoke(_quartzFilgraphManager, new EventArgs());
            }
        }

        void OwnerControlResize(object sender, EventArgs e)
        {
            if (_quartzVideo == null)
                return;

            // calc new scaled size with correct aspect ratio
            float factorX = _owner.Width / (float)_sourceWidth;
            float factorY = _owner.Height / (float)_sourceHeight;

            if (factorX > factorY)
            {
                _quartzVideo.Width = (int)(_sourceWidth * factorY);
                _quartzVideo.Height = (int)(_sourceHeight * factorY);
            }
            else
            {
                _quartzVideo.Width = (int)(_sourceWidth * factorX);
                _quartzVideo.Height = (int)(_sourceHeight * factorX);
            }

            _quartzVideo.Left = (_owner.Width - _quartzVideo.Width) / 2;
            _quartzVideo.Top = (_owner.Height - _quartzVideo.Height) / 2;
        }

        public override void DisposeVideoPlayer()
        {
            ThreadPool.QueueUserWorkItem(DisposeQuarts, _quartzFilgraphManager);
        }

        private void DisposeQuarts(object player)
        {
            try
            {
                if (_quartzVideo != null)
                    _quartzVideo.Owner = -1;
            }
            catch
            {
            }

            if (_quartzFilgraphManager != null)
            {
                _quartzFilgraphManager.Stop();
                Marshal.ReleaseComObject(_quartzFilgraphManager);
            }

            _quartzFilgraphManager = null;
            _quartzVideo = null;
        }
    }
}
