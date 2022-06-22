using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public partial class SettingsFixContinuationStyle : Form
    {
        public SettingsFixContinuationStyle()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.Settings;
            var settings = Configuration.Settings.General;
            Text = language.FixContinuationStyleSettings;
            checkBoxUncheckInsertsAllCaps.Text = language.UncheckInsertsAllCaps;
            checkBoxUncheckInsertsItalic.Text = language.UncheckInsertsItalic;
            checkBoxUncheckInsertsLowercase.Text = language.UncheckInsertsLowercase;
            checkBoxHideContinuationCandidatesWithoutName.Text = language.HideContinuationCandidatesWithoutName;
            checkBoxIgnoreLyrics.Text = language.IgnoreLyrics;
            labelContinuationPause.Text = language.ContinuationPause;
            labelMs.Text = language.Milliseconds;
            buttonEditCustomStyle.Text = language.EditCustomContinuationStyle;

            checkBoxUncheckInsertsAllCaps.Checked = settings.FixContinuationStyleUncheckInsertsAllCaps;
            checkBoxUncheckInsertsItalic.Checked = settings.FixContinuationStyleUncheckInsertsItalic;
            checkBoxUncheckInsertsLowercase.Checked = settings.FixContinuationStyleUncheckInsertsLowercase;
            checkBoxHideContinuationCandidatesWithoutName.Checked = settings.FixContinuationStyleHideContinuationCandidatesWithoutName;
            checkBoxIgnoreLyrics.Checked = settings.FixContinuationStyleIgnoreLyrics;
            numericUpDownContinuationPause.Value = settings.ContinuationPause;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            // resize window with if texts are too long
            if (checkBoxUncheckInsertsAllCaps.Left + checkBoxUncheckInsertsAllCaps.Width > Width)
            {
                Width = checkBoxUncheckInsertsAllCaps.Left + checkBoxUncheckInsertsAllCaps.Width + 10;
            }
            if (checkBoxHideContinuationCandidatesWithoutName.Left + checkBoxHideContinuationCandidatesWithoutName.Width > Width)
            {
                Width = checkBoxHideContinuationCandidatesWithoutName.Left + checkBoxHideContinuationCandidatesWithoutName.Width + 10;
            }

            numericUpDownContinuationPause.Left = labelContinuationPause.Left + labelContinuationPause.Width + 6;
            labelMs.Left = numericUpDownContinuationPause.Left + numericUpDownContinuationPause.Width + 6;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.FixContinuationStyleUncheckInsertsAllCaps = checkBoxUncheckInsertsAllCaps.Checked;
            Configuration.Settings.General.FixContinuationStyleUncheckInsertsItalic = checkBoxUncheckInsertsItalic.Checked;
            Configuration.Settings.General.FixContinuationStyleUncheckInsertsLowercase = checkBoxUncheckInsertsLowercase.Checked;
            Configuration.Settings.General.FixContinuationStyleHideContinuationCandidatesWithoutName = checkBoxHideContinuationCandidatesWithoutName.Checked;
            Configuration.Settings.General.FixContinuationStyleIgnoreLyrics = checkBoxIgnoreLyrics.Checked;
            Configuration.Settings.General.ContinuationPause = Convert.ToInt32(numericUpDownContinuationPause.Value);

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SettingsFixContinuationStyle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonEditCustomStyle_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsCustomContinuationStyle())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // Saving settings handled by dialog
                    // Reload pause setting in case it was changed
                    numericUpDownContinuationPause.Value = Configuration.Settings.General.ContinuationPause;
                }
            }
        }
    }
}
