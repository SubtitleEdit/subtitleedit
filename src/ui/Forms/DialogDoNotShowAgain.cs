using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class DialogDoNotShowAgain : Form
    {
        public bool DoNoDisplayAgain { get; set; }

        public DialogDoNotShowAgain(string title, string text)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Rectangle screenRectangle = RectangleToScreen(ClientRectangle);
            int titleBarHeight = screenRectangle.Top - Top;

            checkBoxDoNotDisplayAgain.Text = LanguageSettings.Current.Main.DoNotDisplayMessageAgain;

            Text = title;
            labelText.Text = text;
            UiUtil.FixLargeFonts(this, buttonOK);

            int width = Math.Max(checkBoxDoNotDisplayAgain.Width, labelText.Width);
            Width = width + buttonOK.Width + 75;
            Height = labelText.Top + labelText.Height + buttonOK.Height + titleBarHeight + 40;
        }

        private void SpellCheckCompleted_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void DialogDoNotShowAgain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DoNoDisplayAgain = checkBoxDoNotDisplayAgain.Checked;
        }

    }
}
