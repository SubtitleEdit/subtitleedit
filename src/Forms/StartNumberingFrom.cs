using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class StartNumberingFrom : PositionAndSizeForm
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
                if ((e.KeyValue >= 48 && e.KeyValue <= 57) || (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Left ||
                   e.KeyCode == Keys.Right || e.KeyCode == Keys.Back || e.KeyCode == Keys.Home || e.KeyCode == Keys.End) ||
                   (e.KeyValue >= 96 && e.KeyValue <= 105))
                {
                    return;
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
