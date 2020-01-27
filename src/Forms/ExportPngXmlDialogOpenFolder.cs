using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportPngXmlDialogOpenFolder : Form
    {
        private readonly string _folder;
        public ExportPngXmlDialogOpenFolder(string text, string folder)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Rectangle screenRectangle = RectangleToScreen(ClientRectangle);
            int titleBarHeight = screenRectangle.Top - Top;

            linkLabelOpenFolder.Text = Configuration.Settings.Language.Main.Menu.File.OpenContainingFolder;

            Text = string.Empty;
            labelText.Text = text;
            UiUtil.FixLargeFonts(this, buttonOK);

            int width = Math.Max(linkLabelOpenFolder.Width, labelText.Width);
            Width = width + buttonOK.Width + 75;
            Height = labelText.Top + labelText.Height + buttonOK.Height + titleBarHeight + 40;
            _folder = folder;
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
            UiUtil.OpenFolder(_folder);
        }
    }
}
