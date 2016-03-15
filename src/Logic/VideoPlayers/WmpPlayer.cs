//using System;
//using System.Threading;
//using System.Windows.Forms;
//using AxWMPLib;

//namespace Nikse.SubtitleEdit.Logic.VideoPlayers
//{
//    public class WmpPlayer : VideoPlayer
//    {
//        public override event EventHandler OnVideoLoaded;
//        public override event EventHandler OnVideoEnded;

//        private AxWindowsMediaPlayer _axWindowsMediaPlayer;

//        public override string PlayerName { get { return "Windows Media Player"; } }

//        public override int Volume
//        {
//            get { return _axWindowsMediaPlayer.settings.volume; }
//            set { _axWindowsMediaPlayer.settings.volume = value; }
//        }

//        public override double Duration
//        {
//            get { return _axWindowsMediaPlayer.currentMedia.duration; }
//        }

//        public override double CurrentPosition
//        {
//            get { return _axWindowsMediaPlayer.Ctlcontrols.currentPosition; }
//            set { _axWindowsMediaPlayer.Ctlcontrols.currentPosition = value; }
//        }

//        public override void Play()
//        {
//            _axWindowsMediaPlayer.Ctlcontrols.play();
//        }

//        public override void Pause()
//        {
//            _axWindowsMediaPlayer.Ctlcontrols.pause();
//        }

//        public override void Stop()
//        {
//            _axWindowsMediaPlayer.Ctlcontrols.stop();
//        }

//        public override bool IsPaused
//        {
//            get { return _axWindowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsPaused; }
//        }

//        public override bool IsPlaying
//        {
//            get { return _axWindowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying; }
//        }

//        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
//        {
//            OnVideoLoaded = onVideoLoaded;
//            OnVideoEnded = onVideoEnded;
//            VideoFileName = videoFileName;
//            _axWindowsMediaPlayer = new AxWindowsMediaPlayer();
//            ownerControl.Controls.Add(_axWindowsMediaPlayer);
//            _axWindowsMediaPlayer.Dock = DockStyle.Fill;
//            _axWindowsMediaPlayer.uiMode = "none";
//            _axWindowsMediaPlayer.enableContextMenu = false;
//            _axWindowsMediaPlayer.URL = videoFileName;
//            _axWindowsMediaPlayer.PlayStateChange += AxWindowsMediaPlayerStatusChange;
//            _axWindowsMediaPlayer.PlayStateChange += AxWindowsMediaPlayerEnded;
//        }

//        private void AxWindowsMediaPlayerStatusChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
//        {
//            try
//            {
//                if (e.newState == (int)WMPLib.WMPPlayState.wmppsPlaying ||
//                    e.newState == (int)WMPLib.WMPPlayState.wmppsMediaEnded)
//                {
//                    _axWindowsMediaPlayer.PlayStateChange -= AxWindowsMediaPlayerStatusChange;
//                    if (OnVideoLoaded != null)
//                        OnVideoLoaded.Invoke(_axWindowsMediaPlayer, new EventArgs());
//                }
//            }
//            catch
//            {
//            }
//        }

//        private void AxWindowsMediaPlayerEnded(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
//        {
//            try
//            {
//                if (e.newState == (int)WMPLib.WMPPlayState.wmppsMediaEnded && OnVideoEnded != null)
//                    OnVideoEnded.Invoke(_axWindowsMediaPlayer, new EventArgs());
//            }
//            catch
//            {
//            }
//        }

//        public override void DisposeVideoPlayer()
//        {
//            ThreadPool.QueueUserWorkItem(DisposeAxWindowsMediaPlayer, _axWindowsMediaPlayer);
//        }

//        private static void DisposeAxWindowsMediaPlayer(object player)
//        {
//            if (player != null)
//                ((IDisposable)player).Dispose();
//        }
//    }
//}
