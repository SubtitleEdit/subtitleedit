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

            checkBoxUncheckInsertsAllCaps.Checked = settings.FixContinuationStyleUncheckInsertsAllCaps;
            checkBoxUncheckInsertsItalic.Checked = settings.FixContinuationStyleUncheckInsertsItalic;
            checkBoxUncheckInsertsLowercase.Checked = settings.FixContinuationStyleUncheckInsertsLowercase;
            checkBoxHideContinuationCandidatesWithoutName.Checked = settings.FixContinuationStyleHideContinuationCandidatesWithoutName;
            checkBoxIgnoreLyrics.Checked = settings.FixContinuationStyleIgnoreLyrics;

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
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.FixContinuationStyleUncheckInsertsAllCaps = checkBoxUncheckInsertsAllCaps.Checked;
            Configuration.Settings.General.FixContinuationStyleUncheckInsertsItalic = checkBoxUncheckInsertsItalic.Checked;
            Configuration.Settings.General.FixContinuationStyleUncheckInsertsLowercase = checkBoxUncheckInsertsLowercase.Checked;
            Configuration.Settings.General.FixContinuationStyleHideContinuationCandidatesWithoutName = checkBoxHideContinuationCandidatesWithoutName.Checked;
            Configuration.Settings.General.FixContinuationStyleIgnoreLyrics = checkBoxIgnoreLyrics.Checked;

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
    }
}
