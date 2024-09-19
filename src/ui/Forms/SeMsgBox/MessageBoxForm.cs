using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.SeMsgBox
{
    public sealed partial class MessageBoxForm : Form
    {
        private string _text;

        public MessageBoxForm(string text, string caption, MessageBoxButtons buttons)
        {
            var icon = AutoGuessIcon(text, buttons);
            Init(text, caption, buttons, icon, false);
        }

        public MessageBoxForm(string text, string caption, MessageBoxButtons buttons, bool noTextBox)
        {
            var icon = AutoGuessIcon(text, buttons);
            Init(text, caption, buttons, icon, noTextBox);
        }

        private static MessageBoxIcon AutoGuessIcon(string text, MessageBoxButtons buttons)
        {
            if (buttons == MessageBoxButtons.YesNoCancel || text.EndsWith("?"))
            {
                return MessageBoxIcon.Question;
            }

            if (buttons == MessageBoxButtons.AbortRetryIgnore)
            {
                return MessageBoxIcon.Error;
            }

            return MessageBoxIcon.Information;
        }

        public MessageBoxForm(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Init(text, caption, buttons, icon, false);
        }

        public MessageBoxForm(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, bool noTextBox)
        {
            Init(text, caption, buttons, icon, noTextBox);
        }

        private void Init(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, bool noTextBox)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelText.Font = new Font(Font.FontFamily, Font.Size + 2);
            labelText.TextAlign = ContentAlignment.TopLeft;
            Text = caption;
            _text = text;
            copyTextToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.CopyToClipboard;
            toolStripMenuItemCopyText.Text = LanguageSettings.Current.Main.Menu.ContextMenu.CopyToClipboard;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonYes.Text = LanguageSettings.Current.General.Yes;
            buttonNo.Text = LanguageSettings.Current.General.No;
            buttonAbort.Text = LanguageSettings.Current.DvdSubRip.Abort;

            InitializeIcon(icon);
            InitializeText(text, noTextBox);
            InitializeButtons(buttons);

            UiUtil.FixLargeFonts(this, buttonOK);
        }


        private void InitializeIcon(MessageBoxIcon icon)
        {
            pictureBoxIcon.SizeMode = PictureBoxSizeMode.AutoSize;
            if (icon == MessageBoxIcon.Information)
            {
                pictureBoxIcon.Image = Properties.Resources.Information;
                TryLoadIcon(pictureBoxIcon, "Information");
                pictureBoxIcon.BringToFront();
            }
            else if (icon == MessageBoxIcon.Question)
            {
                pictureBoxIcon.Image = Properties.Resources.Question;
                TryLoadIcon(pictureBoxIcon, "Question");
                pictureBoxIcon.BringToFront();
            }
            else if (icon == MessageBoxIcon.Warning)
            {
                pictureBoxIcon.Image = Properties.Resources.Warning;
                TryLoadIcon(pictureBoxIcon, "Warning");
                pictureBoxIcon.BringToFront();
            }
            else if (icon == MessageBoxIcon.Error)
            {
                pictureBoxIcon.Image = Properties.Resources.Error;
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

        private void InitializeText(string text, bool noTextBox)
        {
            if (text == null)
            {
                text = string.Empty;
            }

            if (!noTextBox && ShouldSwitchToTextBox(text))
            {
                seTextBox2.ReadOnly = true;
                seTextBox2.Text = text;
                labelText.Visible = false;
                seTextBox2.ContextMenuStrip = contextMenuStrip1;
                seTextBox2.SelectionLength = 0;
                return;
            }

            seTextBox2.Visible = false;
            labelText.ContextMenuStrip = contextMenuStrip1;
            using (var g = CreateGraphics())
            {
                var textSize = g.MeasureString(text, labelText.Font);
                Height = (int)textSize.Height + 90 + (Height - buttonOK.Top);
                var formWidth = (int)textSize.Width + 120;
                Width = Math.Max(formWidth, 320);
            }

            labelText.Text = text;
        }

        private static bool ShouldSwitchToTextBox(string text)
        {
            if (text.Length > 500)
            {
                return true;
            }

            var arr = text.SplitToLines();
            if (arr.Any(p => p.Length > 140))
            {
                return true;
            }

            return false;
        }

        private void InitializeButtons(MessageBoxButtons buttons)
        {
            var buttonControls = new List<Button>();
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    buttonControls.AddRange(new List<Button> { buttonOK });
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

            var accessKeyDictionary = new HashSet<char>();
            foreach (var buttonControl in buttonControls)
            {
                AutoAddAccessKey(buttonControl, accessKeyDictionary);

                buttonControl.Width = buttonWidth;
                buttonControl.Visible = true;
                buttonControl.Left = start;
                buttonControl.TabIndex = Math.Max(0, start);
                start += buttonWidth + 10;
            }

            if (buttonControls.Count > 0)
            {
                AcceptButton = buttonControls[0];
                buttonControls[0].NotifyDefault(true);

                if (buttonCancel.Visible)
                {
                    CancelButton = buttonCancel;
                }
            }
        }

        private static void AutoAddAccessKey(Button button, HashSet<char> accessKeyDictionary)
        {
            if (!button.Text.Contains('&'))
            {
                if (button.Text == "Cancel" && !accessKeyDictionary.Contains('a'))
                {
                    button.Text = "C&ancel";
                }
                else
                {
                    foreach (var ch in button.Text)
                    {
                        if (char.IsLetter(ch) && !accessKeyDictionary.Contains(char.ToLowerInvariant(ch)))
                        {
                            accessKeyDictionary.Add(char.ToLowerInvariant(ch));
                            var idx = button.Text.IndexOf(ch);
                            button.Text = button.Text.Insert(idx, "&");
                            return;
                        }
                    }
                }
            }
        }

        private void ButtonYes_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void ButtonNo_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }

        private void ButtonRetry_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
        }

        private void ButtonIgnore_Click(object sender, EventArgs e)
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
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
            {
                e.SuppressKeyPress = true;
                SetClipboardText();
            }
        }

        private void SetClipboardText()
        {
            if (seTextBox2.Visible && seTextBox2.SelectionLength > 0)
            {
                ClipboardSetText(seTextBox2.SelectedText);
            }
            else if (!string.IsNullOrEmpty(_text))
            {
                ClipboardSetText(_text);
            }
        }

        private void ClipboardSetText(string text)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Clipboard.Clear();
                    Clipboard.SetText(text);
                    return;
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void CopyTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetClipboardText();
        }

        private void MessageBoxForm_Shown(object sender, EventArgs e)
        {
            BringToFront();
            Activate();
            if (buttonOK.Visible)
            {
                buttonOK.Focus();
            }

            KeyPreview = true;
        }
    }
}
