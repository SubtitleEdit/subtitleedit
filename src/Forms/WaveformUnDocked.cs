using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class WaveformUnDocked : Form
    {
        private Main _mainForm = null;
        private PositionsAndSizes _positionsAndSizes = null;
        private Keys _redockKeys;

        public Panel PanelContainer
        {
            get
            {
                return panelContainer;
            }
        }

        public WaveformUnDocked(Main mainForm, PositionsAndSizes positionsAndSizes)
        {
            InitializeComponent();
            _mainForm = mainForm;
            this.Icon = (Icon)mainForm.Icon.Clone();
            _positionsAndSizes = positionsAndSizes;
            _redockKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
        }

        public WaveformUnDocked()
        {
        }

        private void WaveformUnDocked_FormClosing(object sender, FormClosingEventArgs e)
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
            _positionsAndSizes.SavePositionAndSize(this);
        }

        private void WaveformUnDocked_KeyDown(object sender, KeyEventArgs e)
        {
            if (_redockKeys == e.KeyData)
                _mainForm.RedockVideoControlsToolStripMenuItemClick(null, null);
            else
                _mainForm.MainKeyDown(sender, e);
        }
    }
}
