using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class WebVttNewVoice : Form
    {
        public string VoiceName { get; set; }

        public WebVttNewVoice()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.WebVttNewVoice.Title;
            labelVoiceName.Text = Configuration.Settings.Language.WebVttNewVoice.VoiceName;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        private void WebVttNewVoice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                VoiceName = textBox1.Text;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            VoiceName = textBox1.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
