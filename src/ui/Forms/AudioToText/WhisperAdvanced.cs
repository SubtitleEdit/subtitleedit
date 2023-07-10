using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public partial class WhisperAdvanced : Form
    {
        public WhisperAdvanced(string whisperEngine)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            comboBoxWhisperExtra.Items.Clear();
            comboBoxWhisperExtra.Items.Add("--temperature <TEMPERATURE>");
            comboBoxWhisperExtra.Items.Add("--best_of <BEST_OF>");
            comboBoxWhisperExtra.Items.Add("--beam_size <BEAM_SIZE>");
            comboBoxWhisperExtra.Items.Add("--patience <PATIENCE>");
            comboBoxWhisperExtra.Items.Add("--condition_on_previous_text False");
            comboBoxWhisperExtra.Items.Add("--fp16 False");
            comboBoxWhisperExtra.Items.Add("--temperature_increment_on_fallback <TEMPERATURE_INCREMENT_ON_FALLBACK>");

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            comboBoxWhisperExtra.Text = Configuration.Settings.Tools.WhisperExtraSettings;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.WhisperExtraSettings = comboBoxWhisperExtra.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
