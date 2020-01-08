using Nikse.SubtitleEdit.Core;
using QuartzTypeLib;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

//https://docs.microsoft.com/en-us/windows/win32/directshow/directshow
//https://docs.microsoft.com/en-us/previous-versions//dd387928(v=vs.85)

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class QuartsPlayer : VideoPlayer, IDisposable
    {
        public override event EventHandler OnVideoLoaded;
        public override event EventHandler OnVideoEnded;

        private IVideoWindow _quartzVideo;
        private FilgraphManager _quartzFilgraphManager;
        private IMediaPosition _mediaPosition;
        private bool _isPaused;
        private Control _owner;
        private Timer _videoEndTimer;
        private BackgroundWorker _videoLoader;
        private int _sourceWidth;
        private int _sourceHeight;

        public override string PlayerName => "DirectShow";

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
                    return ((IBasicAudio)_quartzFilgraphManager).Volume / 35 + 100;
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
                    {
                        ((IBasicAudio)_quartzFilgraphManager).Volume = -10000;
                    }
                    else
                    {
                        ((IBasicAudio)_quartzFilgraphManager).Volume = (value - 100) * 35;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        public override double Duration
        {
            get
            {
                try
                {
                    return _mediaPosition.Duration;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override double CurrentPosition
        {
            get
            {
                try
                {
                    return _mediaPosition.CurrentPosition;
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                if (value >= 0 && value <= Duration)
                {
                    _mediaPosition.CurrentPosition = value;
                }
            }
        }

        public override double PlayRate
        {
            get => _mediaPosition.Rate;
            set
            {
                if (value >= 0 && value <= 2.0)
                {
                    _mediaPosition.Rate = value;
                }
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

        public override bool IsPaused => _isPaused;

        public override bool IsPlaying => !IsPaused;

        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            const int wsChild = 0x40000000;

            string ext = System.IO.Path.GetExtension(videoFileName)?.ToLowerInvariant();
            bool isAudio = ext == ".mp3" || ext == ".wav" || ext == ".wma" || ext == ".ogg" || ext == ".mpa" || ext == ".m4a" || ext == ".ape" || ext == ".aiff" || ext == ".flac" || ext == ".aac" || ext == ".ac3" || ext == ".mka";

            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            VideoFileName = videoFileName;
            _owner = ownerControl;

            try
            {
                // Hack for windows 10... ???
                if (!isAudio && Configuration.Settings.General.DirectShowDoubleLoadVideo)
                {
                    var quartzFilterGraphManager = new FilgraphManager();
                    quartzFilterGraphManager.RenderFile(VideoFileName);
                    var quartzVideo = quartzFilterGraphManager as IVideoWindow;
                    quartzVideo.Visible = 0;
                    quartzVideo.Owner = (int)IntPtr.Zero;
                }
            }
            catch
            {
                // ignore errors
            }

            _quartzFilgraphManager = new FilgraphManager();
            _quartzFilgraphManager.RenderFile(VideoFileName);

            if (!isAudio)
            {
                _quartzVideo = _quartzFilgraphManager as IVideoWindow;
                _quartzVideo.Owner = (int)ownerControl.Handle;
                _quartzVideo.SetWindowPosition(0, 0, ownerControl.Width, ownerControl.Height);
                _quartzVideo.WindowStyle = wsChild;
            }

            if (!isAudio)
            {
                ((IBasicVideo)_quartzFilgraphManager).GetVideoSize(out _sourceWidth, out _sourceHeight);
            }

            _owner.Resize += OwnerControlResize;
            _mediaPosition = (IMediaPosition)_quartzFilgraphManager;
            if (OnVideoLoaded != null)
            {
                _videoLoader = new BackgroundWorker();
                _videoLoader.RunWorkerCompleted += VideoLoaderRunWorkerCompleted;
                _videoLoader.DoWork += VideoLoaderDoWork;
                _videoLoader.RunWorkerAsync();
            }

            OwnerControlResize(this, null);
            _videoEndTimer = new Timer { Interval = 500 };
            _videoEndTimer.Tick += VideoEndTimerTick;
            _videoEndTimer.Start();

            if (!isAudio)
            {
                _quartzVideo.MessageDrain = (int)ownerControl.Handle;
            }
        }

        public static VideoInfo GetVideoInfo(string videoFileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                var quartzFilgraphManager = new FilgraphManager();
                quartzFilgraphManager.RenderFile(videoFileName);
                ((IBasicVideo)quartzFilgraphManager).GetVideoSize(out var width, out var height);

                info.Width = width;
                info.Height = height;
                var basicVideo2 = (IBasicVideo2)quartzFilgraphManager;
                if (basicVideo2.AvgTimePerFrame > 0)
                {
                    info.FramesPerSecond = 1 / basicVideo2.AvgTimePerFrame;
                }

                info.Success = true;
                var iMediaPosition = (quartzFilgraphManager as IMediaPosition);
                info.TotalMilliseconds = iMediaPosition.Duration * 1000;
                info.TotalSeconds = iMediaPosition.Duration;
                info.TotalFrames = info.TotalSeconds * info.FramesPerSecond;
                info.VideoCodec = string.Empty; // TODO: Get real codec names from quartzFilgraphManager.FilterCollection;

                Marshal.ReleaseComObject(quartzFilgraphManager);
            }
            catch
            {
            }
            return info;
        }

        private void VideoLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            Application.DoEvents();
        }

        private void VideoLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (OnVideoLoaded != null)
            {
                try
                {
                    OnVideoLoaded.Invoke(_quartzFilgraphManager, new EventArgs());
                }
                catch
                {
                    // ignored
                }
            }
            _videoEndTimer = null;
        }

        private void VideoEndTimerTick(object sender, EventArgs e)
        {
            if (_isPaused == false && _quartzFilgraphManager != null && CurrentPosition >= Duration)
            {
                _isPaused = true;
                if (OnVideoEnded != null && _quartzFilgraphManager != null)
                {
                    OnVideoEnded.Invoke(_quartzFilgraphManager, new EventArgs());
                }
            }
        }

        private void OwnerControlResize(object sender, EventArgs e)
        {
            if (_quartzVideo == null)
            {
                return;
            }

            try
            {
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
            catch
            {
                // ignored
            }
        }

        public override void DisposeVideoPlayer()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(DisposeQuarts, _quartzFilgraphManager);
        }

        private void DisposeQuarts(object player)
        {
            Dispose();
        }

        private void ReleaseUnmanagedResources()
        {
            try
            {
                if (_quartzVideo != null)
                {
                    _quartzVideo.Owner = -1;
                }
            }
            catch
            {
                // ignored
            }

            if (_quartzFilgraphManager != null)
            {
                try
                {
                    _quartzFilgraphManager.Stop();
                    Marshal.ReleaseComObject(_quartzFilgraphManager);
                    _quartzFilgraphManager = null;
                }
                catch
                {
                    // ignored
                }
            }
            _quartzVideo = null;
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
                if (_videoEndTimer != null)
                {
                    _videoEndTimer.Dispose();
                    _videoEndTimer = null;
                }
                if (_videoLoader != null)
                {
                    _videoLoader.Dispose();
                    _videoLoader = null;
                }
            }
            ReleaseUnmanagedResources();
        }

    }
}
