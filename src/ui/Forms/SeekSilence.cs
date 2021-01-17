using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SeekSilence : Form
    {
        public bool SeekForward { get; set; }
        public double SecondsDuration { get; set; }
        public double VolumeBelow { get; set; }

        public SeekSilence()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.SeekSilence.Title;
            groupBoxSearchDirection.Text = LanguageSettings.Current.SeekSilence.SearchDirection;
            radioButtonForward.Text = LanguageSettings.Current.SeekSilence.Forward;
            radioButtonBack.Text = LanguageSettings.Current.SeekSilence.Back;
            labelDuration.Text = LanguageSettings.Current.SeekSilence.LengthInSeconds;
            labelVolumeBelow.Text = LanguageSettings.Current.SeekSilence.MaxVolume;
            numericUpDownSeconds.Value = (decimal)Configuration.Settings.VideoControls.WaveformSeeksSilenceDurationSeconds;
            numericUpDownVolume.Value = (decimal)Configuration.Settings.VideoControls.WaveformSeeksSilenceMaxVolume;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void SeekSilence_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SeekForward = radioButtonForward.Checked;
            SecondsDuration = (double)numericUpDownSeconds.Value;
            VolumeBelow = (double)numericUpDownVolume.Value;
            Configuration.Settings.VideoControls.WaveformSeeksSilenceDurationSeconds = SecondsDuration;
            Configuration.Settings.VideoControls.WaveformSeeksSilenceMaxVolume = VolumeBelow;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
