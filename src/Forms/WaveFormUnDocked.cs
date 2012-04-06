using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class WaveFormUnDocked : Form
    {
        Main _mainForm = null;
        PositionsAndSizes _positionsAndSizes = null;
        Keys _redockKeys;

        public Panel PanelContainer
        {
            get
            {
                return panelContainer;
            }
        }

        public WaveFormUnDocked(Main mainForm, PositionsAndSizes positionsAndSizes)
        {
            InitializeComponent();
            _mainForm = mainForm;
            this.Icon = (Icon)mainForm.Icon.Clone();
            _positionsAndSizes = positionsAndSizes;
            _redockKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
        }

        public WaveFormUnDocked()
        {
        }

        private void WaveFormUnDocked_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && panelContainer.Controls.Count > 0)
            {
                var controlWaveForm = panelContainer.Controls[0];
                var controlButtons = panelContainer.Controls[1];
                var controlTrackBar = panelContainer.Controls[2];
                panelContainer.Controls.Clear();
                _mainForm.ReDockWaveForm(controlWaveForm, controlButtons, controlTrackBar);
                _mainForm.SetWaveFormToggleOff();
            }
            _positionsAndSizes.SavePositionAndSize(this);
        }

        private void WaveFormUnDocked_KeyDown(object sender, KeyEventArgs e)
        {
            if (_redockKeys == e.KeyData)
                _mainForm.RedockVideoControlsToolStripMenuItemClick(null, null);
            else
                _mainForm.MainKeyDown(sender, e);
        }
    }
}
