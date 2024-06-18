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
            buttonStandard.Text = LanguageSettings.Current.WhisperAdvanced.Standard;
            buttonStandardAsia.Text = LanguageSettings.Current.WhisperAdvanced.StandardAsia;
            buttonHighlightCurrentWord.Text = LanguageSettings.Current.WhisperAdvanced.HighlightCurrentWord;
            buttonSingleWords.Text = LanguageSettings.Current.WhisperAdvanced.SingleWords;
            buttonSentence.Text = LanguageSettings.Current.WhisperAdvanced.Sentence;
            comboBoxWhisperExtra.Text = Configuration.Settings.Tools.WhisperExtraSettings;

            if (whisperEngine == WhisperChoice.Cpp || whisperEngine == WhisperChoice.CppCuBlas)
            {
                tabControlCommandLineHelp.SelectedTab = TabPageCPP;
            }
            else if (whisperEngine == WhisperChoice.ConstMe)
            {
                tabControlCommandLineHelp.SelectedTab = tabPageConstMe;
            }
            else if (whisperEngine == WhisperChoice.CTranslate2 || whisperEngine == WhisperChoice.PurfviewFasterWhisper)
            {
                tabControlCommandLineHelp.SelectedTab = tabPageFasterWhisper;
            }
            else if (whisperEngine == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                tabControlCommandLineHelp.SelectedTab = tabPageFasterWhisperXxl;
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

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                if (Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd != comboBoxWhisperExtra.Text)
                {
                    Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd = string.Empty;
                }
            }

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

        private void buttonSingleWords_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = "--one_word 2";
        }

        private void buttonSentence_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = "--sentence";
        }

        private void buttonStandard_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = "--standard";
        }

        private void buttonHighlightWord_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = $"--highlight_words true --max_line_width {Configuration.Settings.General.SubtitleLineMaximumLength} --max_line_count {Configuration.Settings.General.MaxNumberOfLines}";
        }

        private void buttonStandardAsia_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = "--standard_asia";
        }

        private void buttonXxlStandard_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = "--standard";
        }

        private void buttonXxlStandardAsia_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = "--standard_asia";
        }

        private void WhisperAdvanced_Load(object sender, EventArgs e)
        {

        }

        private void buttonXxlSentence_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = "--sentence";
        }

        private void buttonXxlSingleWord_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = "--one_word 2";
        }

        private void buttonXxlHighlightWord_Click(object sender, EventArgs e)
        {
            comboBoxWhisperExtra.Text = $"--highlight_words true --max_line_width {Configuration.Settings.General.SubtitleLineMaximumLength} --max_line_count {Configuration.Settings.General.MaxNumberOfLines}";
        }
    }
}
