using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class PreviewVideo : Form
    {
        private string _videoFileName;
        private readonly string _subtitleFileName;
        private readonly Subtitle _subtitle;

        public PreviewVideo(string videoFileName, string subtitleFileName, Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _videoFileName = videoFileName;
            _subtitleFileName = subtitleFileName;
            _subtitle = subtitle;
        }

        private void PreviewVideo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
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
    }
}
