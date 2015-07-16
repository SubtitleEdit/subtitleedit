﻿using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GoToLine : Form
    {
        private int _max;
        private int _min;
        private int _lineNumber;

        public GoToLine()
        {
            InitializeComponent();
            Icon = SubtitleEdit.Properties.Resources.SubtitleEditFormIcon;
            Text = Configuration.Settings.Language.GoToLine.Title;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        public int LineNumber
        {
            get
            {
                return _lineNumber;
            }
        }

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
                if (int.TryParse(textBox1.Text, out _lineNumber))
                {
                    if (_lineNumber >= _min && _lineNumber <= _max)
                        DialogResult = DialogResult.OK;
                }
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
                    (e.KeyValue >= 96 && e.KeyValue <= 105))
                {
                    return;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    ButtonOkClick(null, null);
                }
                else if (e.KeyData == (Keys.Control | Keys.V) && Clipboard.GetText(TextDataFormat.UnicodeText).Length > 0)
                {
                    string p = Clipboard.GetText(TextDataFormat.UnicodeText);
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
            if (int.TryParse(textBox1.Text, out _lineNumber))
            {
                if (_lineNumber >= _min && _lineNumber <= _max)
                {
                    DialogResult = DialogResult.OK;
                    return;
                }
            }
            MessageBox.Show(string.Format(Configuration.Settings.Language.GoToLine.XIsNotAValidNumber, textBox1.Text));
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}