using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VideoPlayerUnDocked : Form
    {
        Main _mainForm = null;
        PositionsAndSizes _positionsAndSizes = null;
        Controls.VideoPlayerContainer _videoPlayerContainer;

        public Panel PanelContainer
        {
            get
            {
                return panelContainer;
            }
        }

        public VideoPlayerUnDocked(Main main, string title, PositionsAndSizes positionsAndSizes, Controls.VideoPlayerContainer videoPlayerContainer)
        {
            InitializeComponent();
            _mainForm = main;
            _positionsAndSizes = positionsAndSizes;
            _videoPlayerContainer = videoPlayerContainer;
            Text = title;            
        }

        public VideoPlayerUnDocked()
        {
            // TODO: Complete member initialization
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
            else 
            {
                _mainForm.Main_KeyDown(sender, e);
            }
        }

    }
}
