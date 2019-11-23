using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class RubyJapanese : Form
    {
        public string RubyText { get; private set; }
        public bool RubyItalic { get; private set; }

        public RubyJapanese(string before, string text, string after)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            label1.Text = before;
            label2.Text = text;
            label3.Text = after;
            label2.Left = label1.Left + label1.Width + 5;
            label3.Left = label2.Left + label2.Width + 5;
            textBoxRubyText.Left = label2.Left;
            checkBoxItalic.Left = textBoxRubyText.Left + textBoxRubyText.Width + 5;
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            RubyText = textBoxRubyText.Text.Trim();
            RubyItalic = checkBoxItalic.Checked;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void RubyJapanese_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void RubyJapanese_Shown(object sender, System.EventArgs e)
        {
            textBoxRubyText.Focus();
        }
    }
}
