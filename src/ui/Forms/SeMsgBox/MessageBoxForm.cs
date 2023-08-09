using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.SeMsgBox
{
    public sealed partial class MessageBoxForm : Form
    {
        private readonly string _text;

        public MessageBoxForm(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            InitializeIcon(icon);
            InitializeText(text);
            InitializeButtons(buttons);

            Text = caption;
            _text = text;
            labelText.Font = new Font(Font.FontFamily, Font.Size + 2);
            copyTextToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.CopyToClipboard;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonYes.Text = LanguageSettings.Current.General.Yes;
            buttonNo.Text = LanguageSettings.Current.General.No;
            buttonAbort.Text = LanguageSettings.Current.DvdSubRip.Abort;

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void InitializeIcon(MessageBoxIcon icon)
        {
            pictureBoxIcon.SizeMode = PictureBoxSizeMode.AutoSize;
            if (icon == MessageBoxIcon.Information)
            {
                pictureBoxIcon.Image = Properties.Resources.MsgBoxInfo;
                TryLoadIcon(pictureBoxIcon, "Information");
                pictureBoxIcon.BringToFront();
            }
            else if (icon == MessageBoxIcon.Question)
            {
                pictureBoxIcon.Image = Properties.Resources.MsgBoxQuestion;
                TryLoadIcon(pictureBoxIcon, "Question");
                pictureBoxIcon.BringToFront();
            }
            else if (icon == MessageBoxIcon.Warning)
            {
                pictureBoxIcon.Image = Properties.Resources.MsgBoxWarning;
                TryLoadIcon(pictureBoxIcon, "Warning");
                pictureBoxIcon.BringToFront();
            }
            else if (icon == MessageBoxIcon.Error)
            {
                pictureBoxIcon.Image = Properties.Resources.MsgBoError;
                TryLoadIcon(pictureBoxIcon, "Error");
                pictureBoxIcon.BringToFront();
            }
            else
            {
                pictureBoxIcon.Visible = false;
            }
        }

        private static void TryLoadIcon(PictureBox pb, string iconName)
        {
            var theme = Configuration.Settings.General.UseDarkTheme ? "DarkTheme" : "DefaultTheme";
            if (!string.IsNullOrEmpty(Configuration.Settings.General.ToolbarIconTheme) && !Configuration.Settings.General.ToolbarIconTheme.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                theme = Configuration.Settings.General.ToolbarIconTheme;
            }

            var themeFullPath = Path.Combine(Configuration.IconsDirectory, theme, iconName + ".png");
            if (File.Exists(themeFullPath))
            {
                pb.Image = new Bitmap(themeFullPath);
                return;
            }

            var fullPath = Path.Combine(Configuration.IconsDirectory, "DefaultTheme", iconName + ".png");
            if (File.Exists(fullPath))
            {
                pb.Image = new Bitmap(fullPath);
            }
        }

        private void InitializeText(string text)
        {
            if (text == null)
            {
                text = string.Empty;
            }

            if (text.Length > 500)
            {
                seTextBox2.Text = text;
                labelText.Visible = false;
                seTextBox2.ContextMenuStrip = contextMenuStrip1;
                return;
            }

            seTextBox2.Visible = false;
            labelText.ContextMenuStrip = contextMenuStrip1;
            using (var g = CreateGraphics())
            {
                var textSize = g.MeasureString(text, Font);
                Height = (int) textSize.Height + 90 + (Height - buttonOK.Top);
                Width = (int)textSize.Width + 220;
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

        private void copyTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(_text);
        }
    }
}
