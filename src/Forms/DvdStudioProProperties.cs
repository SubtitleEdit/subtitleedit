using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class DvdStudioProProperties : Form
    {
        public DvdStudioProProperties()
        {
            InitializeComponent();
            textBoxHeader.Text = Configuration.Settings.SubtitleSettings.DvdStudioProHeader;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.SubtitleSettings.DvdStudioProHeader = textBoxHeader.Text.TrimEnd() + Environment.NewLine;
            DialogResult = DialogResult.OK;
        }

        private void DvdStudioProProperties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
