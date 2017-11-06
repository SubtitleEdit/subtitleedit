﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class WaveformUndocked : PositionAndSizeForm
    {
        private Main _mainForm = null;
        private Keys _redockKeys;

        public Panel PanelContainer
        {
            get
            {
                return panelContainer;
            }
        }

        public WaveformUndocked(Main mainForm)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _mainForm = mainForm;
            this.Icon = (Icon)mainForm.Icon.Clone();
            _redockKeys = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
        }

        public WaveformUndocked()
        {
        }

        private void WaveformUndocked_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && panelContainer.Controls.Count > 0)
            {
                var controlWaveform = panelContainer.Controls[0];
                var controlButtons = panelContainer.Controls[1];
                var controlTrackBar = panelContainer.Controls[2];
                panelContainer.Controls.Clear();
                _mainForm.ReDockWaveform(controlWaveform, controlButtons, controlTrackBar);
                _mainForm.SetWaveformToggleOff();
            }
        }

        private void WaveformUndocked_KeyDown(object sender, KeyEventArgs e)
        {
            if (_redockKeys == e.KeyData)
                _mainForm.RedockVideoControlsToolStripMenuItemClick(null, null);
            else
                _mainForm.MainKeyDown(sender, e);
        }
    }
}
