//using System;
//using System.ComponentModel;
//using System.Drawing;
//using System.Threading;
//using System.Windows.Forms;
//using Microsoft.DirectX.AudioVideoPlayback;

//namespace Nikse.SubtitleEdit.Logic.VideoPlayers
//{
//    class ManagedDirectXPlayer : VideoPlayer
//    {
//        public override event EventHandler OnVideoLoaded;
//        public override event EventHandler OnVideoEnded;

//        private System.Windows.Forms.Timer _videoEndTimer;
//        private BackgroundWorker _videoLoader;
//        private Video _managedDirectXVideo;

//        private Audio _audio; // each time Video.Audio is used a new instance is returned :(  ...so we save it in a property :)
//        // For more info see: http://blogs.msdn.com/toub/archive/2004/04/16/114630.aspx

//        public override string PlayerName { get { return "Managed DirectX"; } }

//        /// <summary>
//        /// In DirectX -10000 is silent and 0 is full volume.
//        /// Also, -3500 to 0 seems to be all you can hear! Not much use for -3500 to -9999...
//        /// </summary>
//        public override int Volume
//        {
//            get
//            {
//                if (_audio != null)
//                {
//                    return (_audio.Volume / 35) + 100;
//                }
//                return 0;
//            }
//            set
//            {
//                if (_audio != null)
//                {
//                    if (value == 0)
//                        _audio.Volume = -10000;
//                    else
//                        _audio.Volume = (value - 100) * 35;
//                }
//            }
//        }

//        public override double Duration
//        {
//            get { return _managedDirectXVideo.Duration; }
//        }

//        public override double CurrentPosition
//        {
//            get
//            {
//                return _managedDirectXVideo.CurrentPosition;
//            }
//            set
//            {
//                _managedDirectXVideo.CurrentPosition = value;
//            }
//        }

//        public override void Play()
//        {
//            _managedDirectXVideo.Play();
//        }

//        public override void Pause()
//        {
//            _managedDirectXVideo.Pause();
//        }

//        public override void Stop()
//        {
//            _managedDirectXVideo.Stop();
//        }

//        public override bool IsPaused
//        {
//            get { return _managedDirectXVideo.State == StateFlags.Paused || _managedDirectXVideo.State == StateFlags.Stopped; }
//        }

//        public override bool IsPlaying
//        {
//            get { return _managedDirectXVideo.State == StateFlags.Running; }
//        }

//        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
//        {
//            OnVideoLoaded = onVideoLoaded;
//            OnVideoEnded = onVideoEnded;
//            VideoFileName = videoFileName;
//            _managedDirectXVideo = new Video(VideoFileName);
//            _audio = _managedDirectXVideo.Audio;
//            Size s = ownerControl.Size;
//            _managedDirectXVideo.Owner = ownerControl;
//            ownerControl.Size = s;
//            _managedDirectXVideo.Play();

//            if (OnVideoLoaded != null)
//            {
//                _videoLoader = new BackgroundWorker();
//                _videoLoader.RunWorkerCompleted += VideoLoaderRunWorkerCompleted;
//                _videoLoader.DoWork += VideoLoaderDoWork;
//                _videoLoader.RunWorkerAsync();
//            }

//            _videoEndTimer = new System.Windows.Forms.Timer { Interval = 500 };
//            _videoEndTimer.Tick += VideoEndTimerTick;
//            _videoEndTimer.Start();
//        }

//        void VideoLoaderDoWork(object sender, DoWorkEventArgs e)
//        {
//            int i = 0;
//            while (CurrentPosition < 1 && i < 100)
//            {
//                Application.DoEvents();
//                Thread.Sleep(5);
//                i++;
//            }
//        }

//        void VideoLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
//        {
//            try
//            {
//                if (OnVideoLoaded != null)
//                    OnVideoLoaded.Invoke(_managedDirectXVideo, new EventArgs());
//            }
//            catch
//            {
//            }
//            _videoEndTimer = null;
//        }

//        void VideoEndTimerTick(object sender, EventArgs e)
//        {
//            if (_managedDirectXVideo != null && !_managedDirectXVideo.Paused && CurrentPosition >= Duration)
//            {
//                if (OnVideoEnded != null)
//                    OnVideoEnded.Invoke(_managedDirectXVideo, new EventArgs());
//            }
//        }

//        public override void DisposeVideoPlayer()
//        {
//            ThreadPool.QueueUserWorkItem(DisposeManagedDirectXVideo, _managedDirectXVideo);
//        }

//        private void DisposeManagedDirectXVideo(object player)
//        {
//            if (_audio != null)
//                _audio.Dispose();
//            if (player != null)
//                ((IDisposable)player).Dispose();
//        }
//    }
//}
