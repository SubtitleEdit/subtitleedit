using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.AudioToText;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class WhisperAdvanced : Form
    {
        public WhisperAdvanced(string whisperEngine)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            comboBoxWhisperExtra.Items.Clear();
            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperExtraSettingsHistory))
            {
                foreach (var line in Configuration.Settings.Tools.WhisperExtraSettingsHistory.SplitToLines())
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        comboBoxWhisperExtra.Items.Add(line);
                    }
                }
            }

            Text = LanguageSettings.Current.WhisperAdvanced.Title;
            labelWhisperExtraCmdLine.Text = LanguageSettings.Current.WhisperAdvanced.CommandLineArguments;
            labelNote.Text = LanguageSettings.Current.WhisperAdvanced.Info;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            comboBoxWhisperExtra.Text = Configuration.Settings.Tools.WhisperExtraSettings;

            if (whisperEngine == WhisperChoice.Cpp)
            {
                tabControlCommandLineHelp.SelectedTab = TabPageCPP;
            }
            else if (whisperEngine == WhisperChoice.ConstMe)
            {
                tabControlCommandLineHelp.SelectedTab = tabPageConstMe;
            }
            else if (whisperEngine == WhisperChoice.CTranslate2 ||
                     whisperEngine == WhisperChoice.PurfviewFasterWhisper)
            {
                tabControlCommandLineHelp.SelectedTab = tabPageFasterWhisper;
            }
            else
            {
                tabControlCommandLineHelp.SelectedTab = tabPageOpenAI;
            }

            try
            {
                textBoxCpp.Font = new Font("Consolas", textBoxCpp.Font.Size);
                textBoxConstMe.Font = new Font("Consolas", textBoxCpp.Font.Size);
                textBoxOpenAI.Font = new Font("Consolas", textBoxCpp.Font.Size);
            }
            catch
            {
                // ignore
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var param = comboBoxWhisperExtra.Text.Trim();
            if (!string.IsNullOrWhiteSpace(param) && !Configuration.Settings.Tools.WhisperExtraSettings.Contains(param))
            {
                Configuration.Settings.Tools.WhisperExtraSettingsHistory = param + Environment.NewLine +
                                                                           Configuration.Settings.Tools.WhisperExtraSettingsHistory;
            }

            Configuration.Settings.Tools.WhisperExtraSettings = comboBoxWhisperExtra.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void comboBoxWhisperExtra_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(null, null);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void WhisperAdvanced_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void WhisperAdvanced_Shown(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Focus();
            comboBoxWhisperExtra.SelectAll();
        }
    }
}
