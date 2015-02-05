using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ColumnPaste : Form
    {
        public bool PasteAll { get; set; }
        public bool PasteTimeCodesOnly { get; set; }
        public bool PasteTextOnly { get; set; }
        public bool PasteOriginalTextOnly { get; set; }

        public bool PasteOverwrite { get; set; }

        public ColumnPaste(bool isOriginalAvailable, bool onlyText)
        {
            InitializeComponent();
            FixLargeFonts();

            radioButtonAll.Enabled = !onlyText;
            radioButtonTimeCodes.Enabled = !onlyText;
            radioButtonOriginalText.Visible = isOriginalAvailable;

            Text = Configuration.Settings.Language.ColumnPaste.Title;
            groupBoxChooseColumn.Text = Configuration.Settings.Language.ColumnPaste.ChooseColumn;
            groupBoxOverwriteOrInsert.Text = Configuration.Settings.Language.ColumnPaste.OverwriteShiftCellsDown;
            radioButtonOverwrite.Text = Configuration.Settings.Language.ColumnPaste.Overwrite;
            radioButtonShiftCellsDown.Text = Configuration.Settings.Language.ColumnPaste.ShiftCellsDown;
            radioButtonAll.Text = Configuration.Settings.Language.General.All;
            radioButtonTimeCodes.Text = Configuration.Settings.Language.ColumnPaste.TimeCodesOnly;
            radioButtonTextOnly.Text = Configuration.Settings.Language.ColumnPaste.TextOnly;
            radioButtonOriginalText.Text = Configuration.Settings.Language.ColumnPaste.OriginalTextOnly;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
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

        private void PasteSpecial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
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

    }
}
