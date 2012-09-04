using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class WaveFormGenerateTimeCodes : Form
    {
        public bool StartFromVideoPosition { get; set; }
        public bool DeleteAll { get; set; }
        public bool DeleteForward { get; set; }
        public int BlockSize { get; set; }
        public int VolumeMinimum { get; set; }
        public int VolumeMaximum { get; set; }
        public int DefaultMilliseconds { get; set; }

        public WaveFormGenerateTimeCodes()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            StartFromVideoPosition = radioButtonStartFromPos.Checked;
            DeleteAll = radioButtonDeleteAll.Checked;
            DeleteForward = radioButtonForward.Checked;
            BlockSize = (int)numericUpDownBlockSize.Value;
            VolumeMinimum = (int)numericUpDownMinVol.Value;
            VolumeMaximum = (int)numericUpDownMaxVol.Value;
            DefaultMilliseconds = (int) numericUpDownDefaultMilliseconds.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }       
      
    }
}
