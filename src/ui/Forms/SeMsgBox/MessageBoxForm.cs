using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.SeMsgBox
{
    public sealed partial class MessageBoxForm : Form
    {
        private string _text;

        public MessageBoxForm(string text, string caption, MessageBoxButtons buttons)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            InitializeText(text);
            InitializeButtons(buttons);

            Text = caption;
            _text = text;
            labelText.Font = new Font(Font.FontFamily, Font.Size + 2);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonYes.Text = LanguageSettings.Current.General.Yes;
            buttonNo.Text = LanguageSettings.Current.General.No;
            buttonAbort.Text = LanguageSettings.Current.DvdSubRip.Abort;

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void InitializeText(string text)
        {
            using (var g = CreateGraphics())
            {
                var textSize = g.MeasureString(text, Font);
                Height = (int) textSize.Height + 80 + (Height - buttonOK.Top);
                Width = (int)textSize.Width + 200;
            }

            labelText.Text = text;
        }

        private void InitializeButtons(MessageBoxButtons buttons)
        {
            var buttonControls = new List<Button>();
            switch (buttons)
            {
                case MessageBoxButtons.OK: 
                    buttonControls.AddRange(new List<Button> { buttonOK});
                    break;
                case MessageBoxButtons.OKCancel:
                    buttonControls.AddRange(new List<Button> { buttonOK, buttonCancel });
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    buttonControls.AddRange(new List<Button> { buttonAbort, buttonRetry, buttonIgnore });
                    break;
                case MessageBoxButtons.YesNoCancel:
                    buttonControls.AddRange(new List<Button> { buttonYes, buttonNo, buttonCancel });
                    break;
                case MessageBoxButtons.YesNo:
                    buttonControls.AddRange(new List<Button> { buttonYes, buttonNo });
                    break;
                case MessageBoxButtons.RetryCancel:
                    buttonControls.AddRange(new List<Button> { buttonRetry, buttonCancel });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttons), buttons, null);
            }

            const int buttonWidth = 80;
            var start = (Width - (buttonControls.Count * (buttonWidth + 10)) - 10) / 2;
            foreach (var buttonControl in buttonControls)
            {
                buttonControl.Width = buttonWidth;
                buttonControl.Visible = true;
                buttonControl.Left = start;
                buttonControl.TabIndex = start;
                start += buttonWidth + 10;
            }
        }

        private void buttonYes_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void buttonNo_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }

        private void buttonRetry_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
        }

        private void buttonIgnore_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Ignore;
        }

        private void MessageBoxForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                DialogResult = DialogResult.Cancel;
            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
            {
                e.SuppressKeyPress = true;
                if (!string.IsNullOrEmpty(_text))
                {
                    Clipboard.SetText(_text);
                }
            }
        }
    }
}
