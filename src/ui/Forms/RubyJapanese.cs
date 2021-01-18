using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class RubyJapanese : Form
    {
        public string RubyText { get; private set; }
        public bool RubyItalic { get; private set; }
        public string RubyBaseText { get; private set; }

        public RubyJapanese(string before, string text, string after)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            var rubyBaseText = text;
            if (text.Contains("ruby-container", System.StringComparison.Ordinal))
            {
                var start = text.IndexOf("<ruby-text", System.StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = text.IndexOf('>', start);
                    if (start > 0)
                    {
                        var end = text.IndexOf('<', start);
                        if (end > 0)
                        {
                            textBoxRubyText.Text = text.Substring(start + 1, end - start - 1);
                        }
                    }
                    checkBoxItalic.Checked = text.Contains("ruby-text-italic", System.StringComparison.Ordinal);
                }

                start = text.IndexOf("<ruby-base", System.StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = text.IndexOf('>', start);
                    if (start > 0)
                    {
                        var end = text.IndexOf('<', start);
                        if (end > 0)
                        {
                            rubyBaseText = text.Substring(start + 1, end - start - 1);
                        }
                    }
                }
            }
            RubyBaseText = rubyBaseText;
            label1.Text = before;
            label2.Text = rubyBaseText;
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

        private void textBoxRubyText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(null, null);
            }
        }
    }
}
