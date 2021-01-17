using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ColumnPaste : Form
    {
        public bool PasteAll { get; set; }
        public bool PasteTimeCodesOnly { get; set; }
        public bool PasteTextOnly { get; set; }
        public bool PasteOriginalTextOnly { get; set; }

        public bool PasteOverwrite { get; set; }

        public ColumnPaste(bool isOriginalAvailable, bool onlyText)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);

            radioButtonAll.Enabled = !onlyText;
            radioButtonTimeCodes.Enabled = !onlyText;
            radioButtonOriginalText.Visible = isOriginalAvailable;

            Text = LanguageSettings.Current.ColumnPaste.Title;
            groupBoxChooseColumn.Text = LanguageSettings.Current.ColumnPaste.ChooseColumn;
            groupBoxOverwriteOrInsert.Text = LanguageSettings.Current.ColumnPaste.OverwriteShiftCellsDown;
            radioButtonOverwrite.Text = LanguageSettings.Current.ColumnPaste.Overwrite;
            radioButtonShiftCellsDown.Text = LanguageSettings.Current.ColumnPaste.ShiftCellsDown;
            radioButtonAll.Text = LanguageSettings.Current.General.All;
            radioButtonTimeCodes.Text = LanguageSettings.Current.ColumnPaste.TimeCodesOnly;
            radioButtonTextOnly.Text = LanguageSettings.Current.ColumnPaste.TextOnly;
            radioButtonOriginalText.Text = LanguageSettings.Current.ColumnPaste.OriginalTextOnly;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            if (Configuration.Settings.Tools.ColumnPasteColumn == "timecodes")
            {
                radioButtonTimeCodes.Checked = true;
            }
            else if (Configuration.Settings.Tools.ColumnPasteColumn == "text")
            {
                radioButtonTextOnly.Checked = true;
            }
            else if (isOriginalAvailable && Configuration.Settings.Tools.ColumnPasteColumn == "originaltext")
            {
                radioButtonOriginalText.Checked = true;
            }
            else
            {
                radioButtonAll.Checked = true;
            }

            if (onlyText)
            {
                radioButtonTextOnly.Checked = true;
            }


            if (Configuration.Settings.Tools.ColumnPasteOverwriteMode == "shiftdown")
            {
                radioButtonShiftCellsDown.Checked = true;
            }
            else
            {
                radioButtonOverwrite.Checked = true;
            }
        }

        private void PasteSpecial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            PasteAll = radioButtonAll.Checked;
            PasteTimeCodesOnly = radioButtonTimeCodes.Checked;
            PasteTextOnly = radioButtonTextOnly.Checked;
            PasteOriginalTextOnly = radioButtonOriginalText.Checked;
            PasteOverwrite = radioButtonOverwrite.Checked;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ColumnPaste_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (radioButtonTimeCodes.Checked)
            {
                Configuration.Settings.Tools.ColumnPasteColumn = "timecodes";
            }
            else if (radioButtonTextOnly.Checked)
            {
                Configuration.Settings.Tools.ColumnPasteColumn = "text";
            }
            else if (radioButtonOriginalText.Checked)
            {
                Configuration.Settings.Tools.ColumnPasteColumn = "originaltext";
            }
            else
            {
                Configuration.Settings.Tools.ColumnPasteColumn = "all";
            }

            Configuration.Settings.Tools.ColumnPasteOverwriteMode = radioButtonOverwrite.Checked ? "overwrite" : "shiftdown";
        }
    }
}
