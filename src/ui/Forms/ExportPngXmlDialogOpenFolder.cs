using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportPngXmlDialogOpenFolder : Form
    {
        private readonly string _folder;
        private readonly string _fileName;

        public ExportPngXmlDialogOpenFolder(string text, string folder, string fileName = null)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Rectangle screenRectangle = RectangleToScreen(ClientRectangle);
            int titleBarHeight = screenRectangle.Top - Top;

            linkLabelOpenFolder.Text = LanguageSettings.Current.Main.Menu.File.OpenContainingFolder;

            Text = "Subtitle Edit";
            labelText.Text = text;
            UiUtil.FixLargeFonts(this, buttonOK);

            int width = Math.Max(linkLabelOpenFolder.Width, labelText.Width);
            Width = width + buttonOK.Width + 75;
            Height = labelText.Top + labelText.Height + buttonOK.Height + titleBarHeight + 40;
            _folder = folder;
            _fileName = fileName;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void ExportPngXmlDialogOpenFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void linkLabelOpenFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(_fileName) || !File.Exists(_fileName))
            {
                UiUtil.OpenFolder(_folder);
            }
            else
            {
                UiUtil.OpenFolderFromFileName(_fileName);
            }
        }
    }
}
