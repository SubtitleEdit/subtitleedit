using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class SeekSilence : Form
    {
        public bool SeekForward { get; set; }
        public double SecondsDuration { get; set; }
        public int VolumeBelow { get; set; }

        public SeekSilence()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.SeekSilence.Title;
            groupBoxSearchDirection.Text = Configuration.Settings.Language.SeekSilence.SearchDirection;
            radioButtonForward.Text = Configuration.Settings.Language.SeekSilence.Forward;
            radioButtonBack.Text = Configuration.Settings.Language.SeekSilence.Back;
            labelDuration.Text = Configuration.Settings.Language.SeekSilence.LengthInSeconds;
            labelVolumeBelow.Text = Configuration.Settings.Language.SeekSilence.MaxVolume;
            numericUpDownSeconds.Value = (decimal)Configuration.Settings.VideoControls.WaveformSeeksSilenceDurationSeconds;
            numericUpDownVolume.Value = Configuration.Settings.VideoControls.WaveformSeeksSilenceMaxVolume;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void SeekSilence_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SeekForward = radioButtonForward.Checked;
            SecondsDuration = (double)numericUpDownSeconds.Value;
            VolumeBelow = (int)numericUpDownVolume.Value;
            Configuration.Settings.VideoControls.WaveformSeeksSilenceDurationSeconds = SecondsDuration;
            numericUpDownVolume.Value = VolumeBelow;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
