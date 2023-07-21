using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ConvertColorsToDialog : Form
    {
        public ConvertColorsToDialog()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.ConvertColorsToDialog;
            var settings = Configuration.Settings.Tools;

            Text = language.Title;
            checkBoxRemoveColorTags.Text = language.RemoveColorTags;
            checkBoxAddNewLines.Text = language.AddNewLines;
            checkBoxReBreakLines.Text = language.ReBreakLines;

            checkBoxRemoveColorTags.Checked = settings.ConvertColorsToDialogRemoveColorTags;
            checkBoxAddNewLines.Checked = settings.ConvertColorsToDialogAddNewLines;
            checkBoxReBreakLines.Checked = settings.ConvertColorsToDialogReBreakLines;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.ConvertColorsToDialogRemoveColorTags = checkBoxRemoveColorTags.Checked;
            Configuration.Settings.Tools.ConvertColorsToDialogAddNewLines = checkBoxAddNewLines.Checked;
            Configuration.Settings.Tools.ConvertColorsToDialogReBreakLines = checkBoxReBreakLines.Checked;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ConvertColorsToDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        internal void ConvertColorsToDialogInSubtitle(Subtitle subtitle)
        {
            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, checkBoxRemoveColorTags.Checked, checkBoxAddNewLines.Checked, checkBoxReBreakLines.Checked);
        }
    }
}
