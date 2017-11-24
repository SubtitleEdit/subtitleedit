using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VideoControlsUndocked : PositionAndSizeForm
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

        public VideoControlsUndocked(Main mainForm)
        {            
            AutoScaleMode = AutoScaleMode.Dpi; // UiUtil.PreInitialize(this);
            InitializeComponent();
            //UiUtil.FixFonts(this);
            _mainForm = mainForm;
            Icon = (Icon)mainForm.Icon.Clone();
            _redockKeys = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
        }

        public VideoControlsUndocked()
        {
            // TODO: Complete member initialization
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
                _mainForm.RedockVideoControlsToolStripMenuItemClick(null, null);
            else
                _mainForm.MainKeyDown(sender, e);
        }
    }
}
