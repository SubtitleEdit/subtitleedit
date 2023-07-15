using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.ShotChanges
{
    public partial class AdjustTimingViaShotChanges : Form
    {
        private readonly Subtitle _subtitle;

        public AdjustTimingViaShotChanges(Subtitle subtitle, string videoFileName, WavePeakData wavePeaks, List<double> shotChanges)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;

            SetAudioVisualizerSettings();
            audioVisualizer.WavePeaks = wavePeaks;
            audioVisualizer.ShotChanges = shotChanges;

            OpenVideo(videoFileName);
            videoPlayerContainer1.TryLoadGfx();
            videoPlayerContainer1.HidePlayerName();
        }

        private void SetAudioVisualizerSettings()
        {
            audioVisualizer.ShowWaveform = Configuration.Settings.General.ShowWaveform;
            audioVisualizer.ShowSpectrogram = Configuration.Settings.General.ShowSpectrogram;
            audioVisualizer.ShowGridLines = Configuration.Settings.VideoControls.WaveformDrawGrid;
            audioVisualizer.GridColor = Configuration.Settings.VideoControls.WaveformGridColor;
            audioVisualizer.SelectedColor = Configuration.Settings.VideoControls.WaveformSelectedColor;
            audioVisualizer.Color = Configuration.Settings.VideoControls.WaveformColor;
            audioVisualizer.BackgroundColor = Configuration.Settings.VideoControls.WaveformBackgroundColor;
            audioVisualizer.TextColor = Configuration.Settings.VideoControls.WaveformTextColor;
            audioVisualizer.CursorColor = Configuration.Settings.VideoControls.WaveformCursorColor;
            audioVisualizer.ChaptersColor = Configuration.Settings.VideoControls.WaveformChaptersColor;
            audioVisualizer.TextSize = Configuration.Settings.VideoControls.WaveformTextSize;
            audioVisualizer.TextBold = Configuration.Settings.VideoControls.WaveformTextBold;
            audioVisualizer.MouseWheelScrollUpIsForward = Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward;
            audioVisualizer.AllowOverlap = Configuration.Settings.VideoControls.WaveformAllowOverlap;
            audioVisualizer.ClosenessForBorderSelection = Configuration.Settings.VideoControls.WaveformBorderHitMs;
            audioVisualizer.AllowNewSelection = false;
        }

        private void OpenVideo(string videoFileName)
        {
            var videoInfo = UiUtil.GetVideoInfo(videoFileName);
            UiUtil.InitializeVideoPlayerAndContainer(videoFileName, videoInfo, videoPlayerContainer1, VideoStartLoaded, VideoStartEnded);
        }

        private void VideoStartEnded(object sender, EventArgs e)
        {
            videoPlayerContainer1.Pause();
        }

        private void VideoStartLoaded(object sender, EventArgs e)
        {
            videoPlayerContainer1.Pause();
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            var startPositionSeconds = 0;
            var currentVideoPositionSeconds = 0;
            var subtitleIndex = 0;
            var selectedIndices = listView1.SelectedIndices;

            audioVisualizer.SetPosition(startPositionSeconds, _subtitle, currentVideoPositionSeconds, subtitleIndex, selectedIndices);
        }

        private void AdjustTimingViaShotChanges_ResizeEnd(object sender, System.EventArgs e)
        {
            audioVisualizer.Invalidate();
        }

        private void AdjustTimingViaShotChanges_Resize(object sender, System.EventArgs e)
        {
            audioVisualizer.Invalidate();
        }

        private void AdjustTimingViaShotChanges_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void AdjustTimingViaShotChanges_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseVideo();
        }

        private void CloseVideo()
        {
            if (videoPlayerContainer1.VideoPlayer != null)
            {
                videoPlayerContainer1.Pause();
                videoPlayerContainer1.VideoPlayer.DisposeVideoPlayer();
                videoPlayerContainer1.VideoPlayer = null;
            }

            Application.DoEvents();
            videoPlayerContainer1.Visible = false;
        }
    }
}
