using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GoToLine : Form
    {
        private int _max;
        private int _min;
        private int _lineNumber;

        public GoToLine()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.GoToLine.Title;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public int LineNumber => _lineNumber;

        public void Initialize(int min, int max)
        {
            _min = min;
            _max = max;
            labelGoToLine.Text = string.Format(Text + " ({0} - {1})", min, max);
        }

        private void FormGoToLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void TextBox1KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Validate(textBox1.Text);
            }
            else
            {
                if (e.KeyCode == Keys.D0 ||
                    e.KeyCode == Keys.D1 ||
                    e.KeyCode == Keys.D2 ||
                    e.KeyCode == Keys.D3 ||
                    e.KeyCode == Keys.D4 ||
                    e.KeyCode == Keys.D5 ||
                    e.KeyCode == Keys.D6 ||
                    e.KeyCode == Keys.D7 ||
                    e.KeyCode == Keys.D8 ||
                    e.KeyCode == Keys.D9 ||
                    e.KeyCode == Keys.Delete ||
                    e.KeyCode == Keys.Left ||
                    e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.Back ||
                    e.KeyCode == Keys.Home ||
                    e.KeyCode == Keys.End ||
                    e.KeyValue >= 96 && e.KeyValue <= 105)
                {
                }
                else if (e.KeyData == (Keys.Control | Keys.V) && Clipboard.GetText(TextDataFormat.UnicodeText).Length > 0)
                {
                    string p = Clipboard.GetText(TextDataFormat.UnicodeText);
                    if (!int.TryParse(p, out _))
                    {
                        e.SuppressKeyPress = true;
                    }
                }
                else if (e.Modifiers != Keys.Control && e.Modifiers != Keys.Alt)
                {
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            Validate(textBox1.Text);
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void Validate(string inp)
        {
            if (int.TryParse(inp, out _lineNumber) && _lineNumber >= _min && _lineNumber <= _max)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(string.Format(LanguageSettings.Current.GoToLine.XIsNotAValidNumber, textBox1.Text));
            }
        }
    }
}
