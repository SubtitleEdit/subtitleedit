using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class StartNumberingFrom : Form
    {
        public StartNumberingFrom()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.StartNumberingFrom.Title;
            label1.Text = Configuration.Settings.Language.StartNumberingFrom.StartFromNumber;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
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
            else if (e.KeyCode == Keys.Enter)
            {
                ButtonOkClick(null, null);
            }
            else
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            string s = textBox1.Text;
            if (Utilities.IsInteger(s))
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
                int number;
                if (int.TryParse(textBox1.Text, out number))
                    return number;
                return 1;
            }
        }

    }
}
