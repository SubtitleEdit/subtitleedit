using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class PreviewVideo : Form
    {
        private string _videoFileName;
        private readonly string _subtitleFileName;
        private readonly Subtitle _subtitle;
        private readonly bool _fullScreen;

        public PreviewVideo(string videoFileName, string subtitleFileName, Subtitle subtitle, bool goFullScreen = false)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _videoFileName = videoFileName;
            _subtitleFileName = subtitleFileName;
            _subtitle = subtitle;
            _fullScreen = goFullScreen;
            videoPlayerContainer1.TryLoadGfx();
            videoPlayerContainer1.HidePlayerName();
            if (goFullScreen)
            {
                GoFullScreen();
            }
        }

        private void PreviewVideo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            if (e.Modifiers == Keys.None && e.KeyCode == Keys.Space)
            {
                videoPlayerContainer1.TogglePlayPause();
                e.SuppressKeyPress = true;
            }
            //else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Enter)
            //{
            //    if (IsFullscreen)
            //    {
            //        e.SuppressKeyPress = true;
            //        NoFullscreen();
            //    }
            //    else if (WindowState == FormWindowState.Normal)
            //    {
            //        GoFullscreen();
            //    }
            //    e.SuppressKeyPress = true;
            //}
            else if (e.KeyData == Keys.Right)
            {
                videoPlayerContainer1.CurrentPosition += 2;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Left)
            {
                videoPlayerContainer1.CurrentPosition -= 2;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Up)
            {
                videoPlayerContainer1.Volume += 2;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Down)
            {
                videoPlayerContainer1.Volume -= 2;
                e.SuppressKeyPress = true;
            }
        }

        private void PreviewVideo_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseVideo();
        }

        private void CloseVideo()
        {
            timerSubtitleOnVideo.Stop();

            Application.DoEvents();
            if (videoPlayerContainer1.VideoPlayer != null)
            {
                videoPlayerContainer1.Pause();
                videoPlayerContainer1.VideoPlayer.DisposeVideoPlayer();
                videoPlayerContainer1.VideoPlayer = null;
            }

            Application.DoEvents();
            _videoFileName = null;
            videoPlayerContainer1.Visible = false;
        }

        private void PreviewVideo_Shown(object sender, EventArgs e)
        {
            var videoInfo = UiUtil.GetVideoInfo(_videoFileName);
            UiUtil.InitializeVideoPlayerAndContainer(_videoFileName, videoInfo, videoPlayerContainer1, VideoStartLoaded, VideoStartEnded);
            Text = $"{LanguageSettings.Current.General.Preview} - {videoInfo.Width}x{videoInfo.Height}, {Path.GetFileName(_videoFileName)}";
        }

        private void VideoStartEnded(object sender, EventArgs e)
        {
            videoPlayerContainer1.Pause();
            if (videoPlayerContainer1.VideoPlayer is LibMpvDynamic libmpv)
            {
                libmpv.RemoveSubtitle();
            }
        }

        private void VideoStartLoaded(object sender, EventArgs e)
        {
            videoPlayerContainer1.Pause();
            timerSubtitleOnVideo.Start();
            if (videoPlayerContainer1.VideoPlayer is LibMpvDynamic libmpv)
            {
                libmpv.LoadSubtitle(_subtitleFileName);
            }

            if (_subtitle?.Paragraphs.Count > 0)
            {
                videoPlayerContainer1.CurrentPosition = _subtitle.Paragraphs[0].StartTime.TotalSeconds + 0.1;
            }

            if (_fullScreen)
            {
                GoFullScreen();
                videoPlayerContainer1.OnButtonClicked += (o, args) =>
                {
                    if (o is PictureBox pb)
                    {
                        if (pb.Name == "_pictureBoxFullscreenOver")
                        {
                            DialogResult = DialogResult.Cancel;
                            Close();
                        }
                    }
                };
            }
        }

        private void timerSubtitleOnVideo_Tick(object sender, EventArgs e)
        {
            if (videoPlayerContainer1 == null || videoPlayerContainer1.IsPaused)
            {
                return;
            }

            if (!videoPlayerContainer1.IsPaused)
            {
                videoPlayerContainer1.RefreshProgressBar();
            }
        }

        private void GoFullScreen()
        {
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            videoPlayerContainer1.ShowFullscreenButton = true;
            videoPlayerContainer1.ShowFullScreenControls();
        }
    }
}
