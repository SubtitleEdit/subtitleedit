using System;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VobSubOcrNewFolder : Form
    {
        public string FolderName { get; set; }

        public VobSubOcrNewFolder()
        {
            InitializeComponent();
            FolderName = null;

            Text = Configuration.Settings.Language.VobSubOcrNewFolder.Title;
            label1.Text = Configuration.Settings.Language.VobSubOcrNewFolder.Message;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void FormVobSubOcrNewFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            string folderName = textBoxFolder.Text.Trim();
            if (folderName.Length >= 0)
            {
                try
                {
                    string fullName = Configuration.VobSubCompareFolder + folderName;
                    Directory.CreateDirectory(fullName);
                    FolderName = folderName;
                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void TextBoxFolderKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ButtonOkClick(null, null);
        }
    }
}
