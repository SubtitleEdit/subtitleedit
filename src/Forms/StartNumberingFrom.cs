using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class StartNumberingFrom : PositionAndSizeForm
    {
        private int _startFromNumber;

        public StartNumberingFrom()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.StartNumberingFrom.Title;
            label1.Text = Configuration.Settings.Language.StartNumberingFrom.StartFromNumber;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void FormStartNumberingFrom_KeyDown(object sender, KeyEventArgs e)
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
                ButtonOkClick(null, null);
            }
            else
            {
                if (e.KeyCode != Keys.D0 && e.KeyCode != Keys.D1 && e.KeyCode != Keys.D2 && e.KeyCode != Keys.D3 && e.KeyCode != Keys.D4 && e.KeyCode != Keys.D5 && e.KeyCode != Keys.D6 && e.KeyCode != Keys.D7 && e.KeyCode != Keys.D8 && e.KeyCode != Keys.D9 && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right && e.KeyCode != Keys.Back && e.KeyCode != Keys.Home && e.KeyCode != Keys.End && (e.KeyValue < 96 || e.KeyValue > 105))
                {
                    if (e.KeyData == (Keys.Control | Keys.V) && Clipboard.GetText(TextDataFormat.UnicodeText).Length > 0)
                    {
                        var p = Clipboard.GetText(TextDataFormat.UnicodeText);
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

        public int StartFromNumber => _startFromNumber;
    }
}
