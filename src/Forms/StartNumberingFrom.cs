using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class StartNumberingFrom : Form
    {
        private int _startFromNumber;

        public StartNumberingFrom()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.StartNumberingFrom.Title;
            label1.Text = Configuration.Settings.Language.StartNumberingFrom.StartFromNumber;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void FormStartNumberingFrom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void TextBox1KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonOkClick(null, null);
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
                   (e.KeyValue >= 96 && e.KeyValue <= 105))
                {
                }
                else if (e.KeyData == (Keys.Shift | Keys.Home) || e.KeyData == (Keys.Shift | Keys.End) ||
                    e.KeyCode == (Keys.MButton | Keys.Space) || e.KeyCode == (Keys.LButton | Keys.RButton | Keys.Space))
                {
                }
                else if (e.KeyData == (Keys.Control | Keys.V) && Clipboard.GetText(TextDataFormat.UnicodeText).Length > 0)
                {
                    var p = Clipboard.GetText(TextDataFormat.UnicodeText);
                    int num;
                    if (!int.TryParse(p, out num))
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
            if (int.TryParse(textBox1.Text, out _startFromNumber))
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(Configuration.Settings.Language.StartNumberingFrom.PleaseEnterAValidNumber);
                textBox1.Focus();
                textBox1.SelectAll();
            }
        }

        public int StartFromNumber
        {
            get
            {
                return _startFromNumber;
            }
        }
    }
}
