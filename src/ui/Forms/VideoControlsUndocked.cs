﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VideoControlsUndocked : PositionAndSizeForm
    {
        private readonly Main _mainForm;
        private readonly Keys _redockKeys;

        public Panel PanelContainer => panelContainer;

        public VideoControlsUndocked(Main mainForm)
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            UiUtil.FixFonts(this);
            _mainForm = mainForm;
            Icon = (Icon)mainForm.Icon.Clone();
            _redockKeys = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
        }

        public VideoControlsUndocked()
        {
        }

        private void VideoControlsUndocked_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && panelContainer.Controls.Count > 0)
            {
                ReDock();
            }
        }

        private void ReDock()
        {
            var control = panelContainer.Controls[0];
            var controlCheckBox = panelContainer.Controls[1];
            panelContainer.Controls.Clear();
            _mainForm.ReDockVideoButtons(control, controlCheckBox);
        }

        private void VideoControlsUndocked_KeyDown(object sender, KeyEventArgs e)
        {
            if (_redockKeys == e.KeyData)
            {
                _mainForm.RedockVideoControlsToolStripMenuItemClick(null, null);
            }
            else
            {
                _mainForm.MainKeyDown(sender, e);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_mainForm.ProcessCmdKeyFromChildForm(ref msg, keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void VideoControlsUndocked_Shown(object sender, System.EventArgs e)
        {
            Height = 340;

            if (panelContainer.Controls.Count > 0)
            {
                panelContainer.Controls[0].Anchor = AnchorStyles.Left | AnchorStyles.Top;
                panelContainer.Controls[0].Height = 300;
            }
        }
    }
}
