using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class TextPrompt : Form
    {
        public string InputText { get; set; }

        public TextPrompt(string title, string label, string initialText)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = title;
            labelDescription.Text = label;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            textBox1.Text = initialText;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void FormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                InputText = textBox1.Text;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            InputText = textBox1.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
