using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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

        private void VideoPlayerUnDocked_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && panelContainer.Controls.Count > 0)
            { 
                var control = panelContainer.Controls[0];
                panelContainer.Controls.Clear();
                _mainForm.ReDockVideoPlayer(control);
                _mainForm.SetVideoPlayerToogleOff();
            }
            _positionsAndSizes.SavePositionAndSize(this);            
        }

        private void VideoPlayerUnDocked_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Enter)
            {
                if (WindowState == FormWindowState.Maximized)
                    WindowState = FormWindowState.Normal;
                else if (WindowState == FormWindowState.Normal)
                    WindowState = FormWindowState.Maximized;
                e.SuppressKeyPress = true;
            }
            else if (_videoPlayerContainer.VideoPlayer != null)
            {
                if (e.Modifiers == Keys.None && e.KeyCode == Keys.Space)
                {
                    if (_videoPlayerContainer.VideoPlayer.IsPlaying)
                        _videoPlayerContainer.VideoPlayer.Pause();
                    else
                        _videoPlayerContainer.VideoPlayer.Play();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Right && e.Modifiers == Keys.Control)
                {
                    _videoPlayerContainer.CurrentPosition += 0.10;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Left && e.Modifiers == Keys.Control)
                {
                    _videoPlayerContainer.CurrentPosition -= 0.10;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Right && e.Modifiers == Keys.Alt)
                {
                    _videoPlayerContainer.CurrentPosition += 0.5;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Left && e.Modifiers == Keys.Alt)
                {
                    _videoPlayerContainer.CurrentPosition -= 0.5;
                    e.SuppressKeyPress = true;
                }
            }
        }
    }
}
