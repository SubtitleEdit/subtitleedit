using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VideoPlayerUndocked : PositionAndSizeForm
    {
        private readonly Main _mainForm;
        private readonly Controls.VideoPlayerContainer _videoPlayerContainer;
        private readonly Keys _redockKeys;
        private readonly Keys _mainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        private readonly Keys _mainGeneralGoToNextSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitlePlayTranslate);
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
        private readonly Keys _mainGeneralGoToPrevSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitlePlayTranslate);
        private bool _autoSized;

        private readonly Dictionary<DateTime, int> _mouseMoveDiff;
        private int _mouseLastX = -1;
        private int _mouseLastY = -1;

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

            _mouseMoveDiff = new Dictionary<DateTime, int>();
            videoPlayerContainer.TextBox.MouseMove += VideoPlayerUndocked_MouseMove;
            videoPlayerContainer.PanelPlayer.MouseMove += VideoPlayerUndocked_MouseMove;
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
            ShowControlsAfterMouseMove();

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
                if (_videoPlayerContainer.VideoWidth > 0 && _videoPlayerContainer.VideoHeight > 0 && !_autoSized)
                {
                    int wDiff = _videoPlayerContainer.VideoWidth - _videoPlayerContainer.PanelPlayer.Width;
                    int hDiff = _videoPlayerContainer.VideoHeight - _videoPlayerContainer.PanelPlayer.Height;
                    Width += wDiff;
                    Height += hDiff;
                    _autoSized = true;
                }
                else
                {
                    var parts = Configuration.Settings.General.UndockedVideoPosition.Split(';');
                    if (parts.Length == 4)
                    {
                        if (int.TryParse(parts[2], out var width)
                            && int.TryParse(parts[3], out var height))
                        {
                            Width = width;
                            Height = height;
                        }
                    }

                    _autoSized = false;
                }

                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData || _mainGeneralGoToPrevSubtitlePlayTranslate == e.KeyData)
            {
                _mainForm.GotoPrevSubPosFromvideoPos();
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToNextSubtitle == e.KeyData || _mainGeneralGoToNextSubtitlePlayTranslate == e.KeyData)
            {
                _mainForm.GotoNextSubPosFromVideoPos();
                e.SuppressKeyPress = true;
            }
            else if (_redockKeys == e.KeyData)
            {
                _mainForm.RedockVideoControlsToolStripMenuItemClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Right)
            {
                _videoPlayerContainer.CurrentPosition += 2;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Left)
            {
                _videoPlayerContainer.CurrentPosition -= 2;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Up)
            {
                _videoPlayerContainer.Volume += 2;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Down)
            {
                _videoPlayerContainer.Volume -= 2;
                e.SuppressKeyPress = true;
            }
            else
            {
                _mainForm.MainKeyDown(sender, e);
            }
        }

        private void VideoPlayerUndocked_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsFullscreen)
            {
                _mouseLastX = -1;
                _mouseLastY = -1;
                return;
            }

            if (_mouseLastX == -1 && _mouseLastY == -1)
            {
                _mouseLastX = MousePosition.X;
                _mouseLastY = MousePosition.Y;
            }

            var diff = Math.Abs(MousePosition.X - _mouseLastX) + Math.Abs(MousePosition.Y - _mouseLastY);
            var dateTime = DateTime.UtcNow;

            if (_mouseMoveDiff.Count > 100)
            {
                _mouseMoveDiff.Remove(_mouseMoveDiff.First().Key);
            }

            if (!_mouseMoveDiff.ContainsKey(dateTime) && diff > 0)
            {
                _mouseMoveDiff.Add(dateTime, diff);
            }

            var totalDiff = 0;
            var fromTime = dateTime.AddSeconds(-1);
            foreach (var kvp in _mouseMoveDiff)
            {
                if (kvp.Key > fromTime)
                {
                    totalDiff += kvp.Value;
                }
            }

            if (totalDiff > 75)
            {
                ShowControlsAfterMouseMove();
            }

            _mouseLastX = MousePosition.X;
            _mouseLastY = MousePosition.Y;
        }

        private void ShowControlsAfterMouseMove()
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
            _videoPlayerContainer.SetFullFixed();
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_mainForm.ProcessCmdKeyFromChildForm(ref msg, keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
