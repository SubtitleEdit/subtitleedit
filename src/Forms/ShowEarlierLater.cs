using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.IO;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ShowEarlierLater : Form
    {
        Subtitle _subtitle;
        TimeSpan _totalAdjustment = TimeSpan.FromMilliseconds(0);
        VideoInfo _videoInfo;

        public List<Paragraph> Paragraphs
        {
            get
            {
                return _subtitle.Paragraphs;
            }
        }

        public string VideoFileName { get; set; }

        public ShowEarlierLater()
        {
            InitializeComponent();
            labelTotalAdjustment.Text = string.Empty;
            timeUpDownAdjust.MaskedTextBox.Text = "000000000";
            labelSubtitle.Text = string.Empty;

            Text = Configuration.Settings.Language.ShowEarlierLater.Title;
            labelHoursMinSecsMilliSecs.Text = Configuration.Settings.Language.General.HourMinutesSecondsMilliseconds;
            buttonShowEarlier.Text = Configuration.Settings.Language.ShowEarlierLater.ShowEarlier;
            buttonShowLater.Text = Configuration.Settings.Language.ShowEarlierLater.ShowLater;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
        }

        private void ShowEarlierLater_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Right && e.Modifiers == Keys.Control)
            {
                mediaPlayer.CurrentPosition += 0.10;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Left && e.Modifiers == Keys.Control)
            {
                mediaPlayer.CurrentPosition -= 0.10;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Right && e.Modifiers == Keys.Alt)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.CurrentPosition += 0.5;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Left && e.Modifiers == Keys.Alt)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.CurrentPosition -= 0.5;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Space)
            {
                mediaPlayer.TooglePlayPause();
                e.SuppressKeyPress = true;
            }
        }

        internal void Initialize(Subtitle subtitle, string videoFileName)
        {
            _subtitle = subtitle;
            subtitleListView1.Fill(subtitle);

            timeUpDownAdjust.TimeCode = new TimeCode(TimeSpan.FromMilliseconds(Configuration.Settings.General.DefaultAdjustMilliseconds));

            VideoFileName = videoFileName;
            OpenVideo(videoFileName);
        }

        private void OpenVideo(string fileName)
        {
            if (File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Length < 1000)
                    return;

                VideoFileName = fileName;
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.Pause();
                    mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                }
                VideoInfo videoInfo = ShowVideoInfo(fileName);
                Utilities.InitializeVideoPlayerAndContainer(fileName, videoInfo, mediaPlayer, VideoLoaded, VideoEnded);
            }
            else
            {
                Height = buttonShowLater.Top + buttonShowLater.Height + 40;
                mediaPlayer.Visible = false;
                labelSubtitle.Visible = false;
            }
        }

        private VideoInfo ShowVideoInfo(string fileName)
        {
            _videoInfo = Utilities.GetVideoInfo(fileName, delegate { Application.DoEvents(); });
            var info = new FileInfo(fileName);
            long fileSizeInBytes = info.Length;
            return _videoInfo;
        }


        void VideoLoaded(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
            timer1.Start();
        }

        void VideoEnded(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void ButtonShowEarlierClick(object sender, EventArgs e)
        {
            TimeCode tc = timeUpDownAdjust.TimeCode;
            if (tc != null && tc.TotalMilliseconds > 0)
            {
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds - tc.TotalMilliseconds);
                ShowTotalAdjustMent();
                _subtitle.AddTimeToAllParagraphs(-tc.TimeSpan);
                subtitleListView1.Fill(_subtitle);
            }
        }

        private void ShowTotalAdjustMent()
        {
            TimeCode tc = new TimeCode(_totalAdjustment);
            labelTotalAdjustment.Text = string.Format(Configuration.Settings.Language.ShowEarlierLater.TotalAdjustmentX, tc.ToShortString());
        }

        private void ButtonShowLaterClick(object sender, EventArgs e)
        {
            TimeCode tc = timeUpDownAdjust.TimeCode;
            if (tc != null && tc.TotalMilliseconds > 0)
            {
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds + tc.TotalMilliseconds);
                ShowTotalAdjustMent();
                _subtitle.AddTimeToAllParagraphs(tc.TimeSpan);
                subtitleListView1.Fill(_subtitle);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer != null && mediaPlayer.VideoPlayer != null)
            {
                if (!mediaPlayer.IsPaused)
                {
                    mediaPlayer.RefreshProgressBar();
                    int index = Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);
                }
            }
        }

        private void subtitleListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count > 0 && VideoFileName != null)
            {
                int index = subtitleListView1.SelectedItems[0].Index;

                mediaPlayer.Pause();
                double pos = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
                if (pos > 1)
                    mediaPlayer.CurrentPosition = (_subtitle.Paragraphs[index].StartTime.TotalSeconds) - 0.5;
                else
                    mediaPlayer.CurrentPosition = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
                mediaPlayer.Play();
            }
        }

        private void ShowEarlierLater_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                mediaPlayer.VideoPlayer.Pause();
                mediaPlayer.VideoPlayer.DisposeVideoPlayer();
            }
        }

    }
}
