using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VideoPlayerUnDocked : Form
    {
        Main _mainForm = null;
        PositionsAndSizes _positionsAndSizes = null;
        Controls.VideoPlayerContainer _videoPlayerContainer;
        Keys _redockKeys;

        public bool RedockOnFullscreenEnd { get; set; }

        public Panel PanelContainer
        {
            get
            {
                return panelContainer;
            }
        }

        public VideoPlayerUnDocked(Main main, PositionsAndSizes positionsAndSizes, Controls.VideoPlayerContainer videoPlayerContainer)
        {
            InitializeComponent();
            _mainForm = main;
            this.Icon = (Icon)_mainForm.Icon.Clone();
            _positionsAndSizes = positionsAndSizes;
            _videoPlayerContainer = videoPlayerContainer;
            _redockKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
            RedockOnFullscreenEnd = false;
        }

        public VideoPlayerUnDocked()
        {
        }

        private void VideoPlayerUnDocked_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RedockOnFullscreenEnd)
            {
                _mainForm.redockVideoControlsToolStripMenuItem_Click(null, null);
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
            _positionsAndSizes.SavePositionAndSize(this);
        }

        private void VideoPlayerUnDocked_KeyDown(object sender, KeyEventArgs e)
        {
            VideoPlayerUnDocked_MouseMove(null, null);

            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Enter)
            {
                if (WindowState == FormWindowState.Maximized)
                {
                    FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
                    WindowState = FormWindowState.Normal;
                    _videoPlayerContainer.FontSizeFactor = 1.0F;
                    _videoPlayerContainer.SetSubtitleFont();
                    _videoPlayerContainer.SubtitleText = string.Empty;
                    if (RedockOnFullscreenEnd)
                        this.Close();
                }
                else if (WindowState == FormWindowState.Normal)
                {
                    GoFullscreen();
                }
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Escape && WindowState == FormWindowState.Maximized)
            {
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
                WindowState = FormWindowState.Normal;
                _videoPlayerContainer.FontSizeFactor = 1.0F;
                _videoPlayerContainer.SetSubtitleFont();
                _videoPlayerContainer.SubtitleText = string.Empty;
                e.SuppressKeyPress = true;
                if (RedockOnFullscreenEnd)
                    this.Close();
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

            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt && WindowState == FormWindowState.Maximized)
            {
                _mainForm.GotoPrevSubPosFromvideoPos();
                e.Handled = true;          
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Down && WindowState == FormWindowState.Maximized)
            {
                _mainForm.GotoNextSubPosFromvideoPos();
                e.Handled = true;
            }




            else if (_redockKeys == e.KeyData)
            {
                _mainForm.redockVideoControlsToolStripMenuItem_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else
            {
                _mainForm.Main_KeyDown(sender, e);
            }
        }

        private void VideoPlayerUnDocked_MouseMove(object sender, MouseEventArgs e)
        {
            if (timer1.Enabled)
                timer1.Stop();
            _videoPlayerContainer.ShowControls();
            timer1.Start();
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            timer1.Stop();
            if (WindowState == FormWindowState.Maximized)
            {
                _videoPlayerContainer.HideControls();
            }
        }

        internal void GoFullscreen()
        {
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            _videoPlayerContainer.FontSizeFactor = 1.5F;
            _videoPlayerContainer.SetSubtitleFont();
            _videoPlayerContainer.SubtitleText = string.Empty;
            timer1.Start();
        }
    }
}
