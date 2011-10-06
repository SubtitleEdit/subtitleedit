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
        }

        public VideoPlayerUnDocked()
        {
        }

        private void VideoPlayerUnDocked_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && panelContainer.Controls.Count > 0)
            {
                var control = panelContainer.Controls[0];
                panelContainer.Controls.Clear();
                _mainForm.ReDockVideoPlayer(control);
                _mainForm.SetVideoPlayerToggleOff();
            }
            _positionsAndSizes.SavePositionAndSize(this);
        }

        private void VideoPlayerUnDocked_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Enter)
            {
                if (WindowState == FormWindowState.Maximized)
                    WindowState = FormWindowState.Normal;
                else if (WindowState == FormWindowState.Normal)
                    WindowState = FormWindowState.Maximized;
                e.SuppressKeyPress = true;
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

    }
}
