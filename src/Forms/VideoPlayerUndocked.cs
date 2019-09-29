using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VideoPlayerUndocked : PositionAndSizeForm
    {
        private readonly Main _mainForm;
        private readonly Controls.VideoPlayerContainer _videoPlayerContainer;
        private readonly Keys _redockKeys;

        public bool RedockOnFullscreenEnd { get; set; }

        public Panel PanelContainer => panelContainer;

        public VideoPlayerUndocked(Main main, Controls.VideoPlayerContainer videoPlayerContainer)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _mainForm = main;
            Icon = (Icon)_mainForm.Icon.Clone();
            _videoPlayerContainer = videoPlayerContainer;
            _redockKeys = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
            RedockOnFullscreenEnd = false;
            videoPlayerContainer.TextBox.MouseMove += VideoPlayerUndocked_MouseMove;
        }

        public VideoPlayerUndocked()
        {
        }

        private void VideoPlayerUndocked_FormClosing(object sender, FormClosingEventArgs e)
        {
            _videoPlayerContainer.ShowCursor();
            if (RedockOnFullscreenEnd)
            {
                _mainForm.RedockVideoControlsToolStripMenuItemClick(null, null);
            }
            else if (e.CloseReason == CloseReason.UserClosing && panelContainer.Controls.Count > 0)
            {
                var control = panelContainer.Controls[0];
                if (control is Panel)
                {
                    panelContainer.Controls.Clear();
                    _mainForm.ReDockVideoPlayer(control);
                    _mainForm.SetVideoPlayerToggleOff();
                }
            }
        }

        private void VideoPlayerUndocked_KeyDown(object sender, KeyEventArgs e)
        {
            VideoPlayerUndocked_MouseMove(null, null);

            if (e.Modifiers == Keys.None && e.KeyCode == Keys.Space)
            {
                _videoPlayerContainer.TogglePlayPause();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Enter)
            {
                if (IsFullscreen)
                {
                    e.SuppressKeyPress = true;
                    NoFullscreen();
                }
                else if (WindowState == FormWindowState.Normal)
                {
                    GoFullscreen();
                }
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Escape && IsFullscreen)
            {
                e.SuppressKeyPress = true;
                NoFullscreen();
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.A)
            {
                if (_videoPlayerContainer.VideoWidth > 0 && _videoPlayerContainer.VideoHeight > 0)
                {
                    int wDiff = _videoPlayerContainer.VideoWidth - _videoPlayerContainer.PanelPlayer.Width;
                    int hDiff = _videoPlayerContainer.VideoHeight - _videoPlayerContainer.PanelPlayer.Height;
                    Width += wDiff;
                    Height += hDiff;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt && IsFullscreen)
            {
                _mainForm.GotoPrevSubPosFromvideoPos();
                e.Handled = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Down && IsFullscreen)
            {
                _mainForm.GotoNextSubPosFromVideoPos();
                e.Handled = true;
            }
            else if (_redockKeys == e.KeyData)
            {
                _mainForm.RedockVideoControlsToolStripMenuItemClick(null, null);
                e.SuppressKeyPress = true;
            }
            else
            {
                _mainForm.MainKeyDown(sender, e);
            }
        }

        private void VideoPlayerUndocked_MouseMove(object sender, MouseEventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
            }

            _videoPlayerContainer.ShowControls();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (IsFullscreen)
            {
                var videoScreen = Screen.FromControl(_videoPlayerContainer);
                var mainScreen = Screen.FromControl(_mainForm);
                _videoPlayerContainer.HideControls(videoScreen.DeviceName == mainScreen.DeviceName);
            }
        }

        internal void GoFullscreen()
        {
            IsFullscreen = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            _videoPlayerContainer.FontSizeFactor = 1.5F;
            _videoPlayerContainer.SetSubtitleFont();
            _videoPlayerContainer.SubtitleText = string.Empty;
            _videoPlayerContainer.ShowFullScreenControls();
            timer1.Start();
        }

        internal bool IsFullscreen { get; set; }

        internal void NoFullscreen()
        {
            IsFullscreen = false;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            WindowState = FormWindowState.Normal;
            _videoPlayerContainer.FontSizeFactor = 1.0F;
            _videoPlayerContainer.SetSubtitleFont();
            _videoPlayerContainer.SubtitleText = string.Empty;
            _videoPlayerContainer.ShowFullscreenButton = Configuration.Settings.General.VideoPlayerShowFullscreenButton;
            _videoPlayerContainer.ShowNonFullScreenControls();
            if (RedockOnFullscreenEnd)
            {
                Close();
            }
        }

        private void VideoPlayerUndocked_Shown(object sender, EventArgs e)
        {
            Refresh();
        }

    }
}
