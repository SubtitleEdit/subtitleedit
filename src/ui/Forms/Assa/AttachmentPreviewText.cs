using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public partial class AttachmentPreviewText : Form
    {
        public string PreviewText { get; private set; }

        public AttachmentPreviewText(string previewText)
        {
            InitializeComponent();

            textBoxPreviewText.Text = previewText;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            PreviewText = textBoxPreviewText.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
