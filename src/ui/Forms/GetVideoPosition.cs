using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GetVideoPosition : Form
    {
        private double _lastPosition;
        private TimeSpan _guess;
        private double _goBackPosition;
        private double _stopPosition = -1.0;
        private readonly Subtitle _subtitle;
        private readonly string _videoFileName;
        private VideoInfo _videoInfo;
        public string VideoFileName { get; private set; }
        public TimeSpan VideoPosition { get; private set; }

        public GetVideoPosition(Subtitle subtitle, string videoFileName, VideoInfo videoInfo, TimeSpan timeSpan, string title)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            buttonThreeSecondsBack.Text = LanguageSettings.Current.SetSyncPoint.ThreeSecondsBack;
            buttonHalfASecondBack.Text = LanguageSettings.Current.SetSyncPoint.HalfASecondBack;
            buttonVerify.Text = string.Format(LanguageSettings.Current.VisualSync.PlayXSecondsAndBack, Configuration.Settings.Tools.VerifyPlaySeconds);
            buttonHalfASecondAhead.Text = LanguageSettings.Current.SetSyncPoint.HalfASecondForward;
            buttonThreeSecondsAhead.Text = LanguageSettings.Current.SetSyncPoint.ThreeSecondsForward;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;
            VideoPosition = timeSpan;
            labelSubtitle.Text = string.Empty;
            Text = title;

            AutoSizeWindowRelativeToVideo();
            videoPlayerContainer1.TryLoadGfx();
            videoPlayerContainer1.HidePlayerName();
        }

        private void AutoSizeWindowRelativeToVideo()
        {
            var aspectRatio = (double)_videoInfo.Height / _videoInfo.Width;
            var newHeight = Width * aspectRatio + (Height - videoPlayerContainer1.Bottom + videoPlayerContainer1.Top);
            Height = (int)newHeight + videoPlayerContainer1.ControlsHeight - 15;
        }

        private void OpenVideo(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            var fi = new FileInfo(fileName);
            if (fi.Length < 1000)
            {
                return;
            }

            Text += $" - {fileName}";
            VideoFileName = fileName;
            if (videoPlayerContainer1.VideoPlayer != null)
            {
                videoPlayerContainer1.Pause();
                videoPlayerContainer1.VideoPlayer.DisposeVideoPlayer();
            }

            var videoInfo = UiUtil.GetVideoInfo(fileName);
            UiUtil.InitializeVideoPlayerAndContainer(fileName, videoInfo, videoPlayerContainer1, VideoLoaded, VideoStartEnded);
        }

        private void VideoStartEnded(object sender, EventArgs e)
        {
            videoPlayerContainer1.Pause();
        }

        private void VideoLoaded(object sender, EventArgs e)
        {

            videoPlayerContainer1.Pause();

            if (_guess.TotalMilliseconds > 0 && _guess.TotalMilliseconds / TimeCode.BaseUnit < videoPlayerContainer1.VideoPlayer.Duration)
            {
                videoPlayerContainer1.VideoPlayer.CurrentPosition = _guess.TotalMilliseconds / TimeCode.BaseUnit;
                videoPlayerContainer1.RefreshProgressBar();
            }

            videoPlayerContainer1.VideoPlayer.CurrentPosition = VideoPosition.TotalSeconds;

            videoPlayerContainer1.UpdatePlayerName();

            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (videoPlayerContainer1 == null)
            {
                return;
            }

            double pos;

            videoPlayerContainer1.RefreshProgressBar();
            if (_stopPosition >= 0 && videoPlayerContainer1.CurrentPosition > _stopPosition)
            {
                videoPlayerContainer1.Pause();
                videoPlayerContainer1.CurrentPosition = _goBackPosition;
                _stopPosition = -1;
            }

            if (!videoPlayerContainer1.IsPaused)
            {
                videoPlayerContainer1.RefreshProgressBar();
                pos = videoPlayerContainer1.CurrentPosition;
            }
            else
            {
                pos = videoPlayerContainer1.CurrentPosition;
            }

            if (Math.Abs(pos - _lastPosition) > 0.01)
            {
                UiUtil.ShowSubtitle(_subtitle, videoPlayerContainer1, new SubRip());
                _lastPosition = pos;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            VideoPosition = TimeSpan.FromSeconds(videoPlayerContainer1.VideoPlayer.CurrentPosition);
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void GetTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.S && e.Modifiers == Keys.Control)
            {
                videoPlayerContainer1.Pause();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.P && e.Control)
            {
                videoPlayerContainer1.VideoPlayer.Pause();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Left)
            {
                GoBackSeconds(0.5, videoPlayerContainer1.VideoPlayer);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Right)
            {
                GoBackSeconds(-0.5, videoPlayerContainer1.VideoPlayer);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Left)
            {
                GoBackSeconds(0.1, videoPlayerContainer1.VideoPlayer);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Right)
            {
                GoBackSeconds(-0.1, videoPlayerContainer1.VideoPlayer);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp(null);
                e.SuppressKeyPress = true;
            }
        }

        private void GetTime_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            videoPlayerContainer1?.Pause();
        }

        private void GetTime_FormClosed(object sender, FormClosedEventArgs e)
        {
            videoPlayerContainer1.VideoPlayer?.DisposeVideoPlayer();
        }

        private void GoBackSeconds(double seconds, VideoPlayer mediaPlayer)
        {
            if (mediaPlayer != null)
            {
                if (mediaPlayer.CurrentPosition > seconds)
                {
                    mediaPlayer.CurrentPosition -= seconds;
                }
                else
                {
                    mediaPlayer.CurrentPosition = 0;
                }

                videoPlayerContainer1.RefreshProgressBar();
            }
        }

        private void buttonStartHalfASecondBack_Click(object sender, EventArgs e)
        {
            GoBackSeconds(0.5, videoPlayerContainer1.VideoPlayer);
        }

        private void buttonStartThreeSecondsBack_Click(object sender, EventArgs e)
        {
            GoBackSeconds(3, videoPlayerContainer1.VideoPlayer);
        }

        private void buttonStartThreeSecondsAhead_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-3.0, videoPlayerContainer1.VideoPlayer);
        }

        private void buttonStartHalfASecondAhead_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-0.5, videoPlayerContainer1.VideoPlayer);
        }

        private void buttonStartVerify_Click(object sender, EventArgs e)
        {
            if (videoPlayerContainer1?.VideoPlayer != null)
            {
                _goBackPosition = videoPlayerContainer1.CurrentPosition;
                _stopPosition = _goBackPosition + Configuration.Settings.Tools.VerifyPlaySeconds;
                videoPlayerContainer1.Play();
            }
        }

        private void GetVideoPosition_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_videoFileName) && File.Exists(_videoFileName))
            {
                OpenVideo(_videoFileName);
                videoPlayerContainer1.ShowFullscreenButton = false;
                AutoSizeWindowRelativeToVideo();
                videoPlayerContainer1.SetVolumeAndPlayerNameFont();
            }
        }
    }
}
