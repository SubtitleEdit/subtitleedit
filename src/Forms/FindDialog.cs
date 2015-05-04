using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Enums;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FindDialog : Form
    {
        private Regex _regEx;

        public FindDialog()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.FindDialog.Title;
            buttonFind.Text = Configuration.Settings.Language.FindDialog.Find;
            radioButtonNormal.Text = Configuration.Settings.Language.FindDialog.Normal;
            radioButtonCaseSensitive.Text = Configuration.Settings.Language.FindDialog.CaseSensitive;
            radioButtonRegEx.Text = Configuration.Settings.Language.FindDialog.RegularExpression;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            if (radioButtonRegEx.Left + radioButtonRegEx.Width + 5 > Width)
                Width = radioButtonRegEx.Left + radioButtonRegEx.Width + 5;

            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private FindType GetFindType()
        {
            if (radioButtonNormal.Checked)
                return FindType.Normal;
            if (radioButtonCaseSensitive.Checked)
                return FindType.CaseSensitive;
            return FindType.RegEx;
        }

        private string FindText
        {
            get
            {
                if (textBoxFind.Visible)
                    return textBoxFind.Text;
                return comboBoxFind.Text;
            }
        }

        public FindReplaceDialogHelper GetFindDialogHelper(int startLineIndex)
        {
            return new FindReplaceDialogHelper(GetFindType(), FindText, _regEx, string.Empty, 200, 300, startLineIndex);
        }

        private void FormFindDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ButtonFindClick(object sender, EventArgs e)
        {
            string searchText = FindText;
            textBoxFind.Text = searchText;
            comboBoxFind.Text = searchText;

            if (searchText.Length == 0)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (radioButtonNormal.Checked)
            {
                DialogResult = DialogResult.OK;
            }
            else if (radioButtonCaseSensitive.Checked)
            {
                DialogResult = DialogResult.OK;
            }
            else if (radioButtonRegEx.Checked)
            {
                try
                {
                    _regEx = new Regex(FindText, RegexOptions.Compiled);
                    DialogResult = DialogResult.OK;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void TextBoxFindKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ButtonFindClick(null, null);
        }

        private void comboBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ButtonFindClick(null, null);
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButtonRegEx)
            {
                if (textBoxFind.Visible)
                {
                    comboBoxFind.ContextMenu = null;
                    textBoxFind.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind);
                }
                else
                {
                    textBoxFind.ContextMenu = null;
                    comboBoxFind.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(comboBoxFind);
                }
            }
            else
            {
                textBoxFind.ContextMenu = null;
                comboBoxFind.ContextMenu = null;
            }
        }

        internal void Initialize(string selectedText, FindReplaceDialogHelper findHelper)
        {
            if (Configuration.Settings.Tools.FindHistory.Count > 0)
            {
                textBoxFind.Visible = false;
                comboBoxFind.Visible = true;
                comboBoxFind.Text = selectedText;
                comboBoxFind.SelectAll();
                comboBoxFind.Items.Clear();
                for (int index = 0; index < Configuration.Settings.Tools.FindHistory.Count; index++)
                {
                    string s = Configuration.Settings.Tools.FindHistory[index];
                    comboBoxFind.Items.Add(s);
                }
            }
            else
            {
                comboBoxFind.Visible = false;
                textBoxFind.Visible = true;
                textBoxFind.Text = selectedText;
                textBoxFind.SelectAll();
            }

            if (findHelper != null)
            {
                if (findHelper.FindType == FindType.RegEx)
                    radioButtonRegEx.Checked = true;
                else if (findHelper.FindType == FindType.CaseSensitive)
                    radioButtonCaseSensitive.Checked = true;
                else
                    radioButtonNormal.Checked = true;
            }
        }

        internal void SetIcon(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                IntPtr Hicon = bitmap.GetHicon();
                this.Icon = Icon.FromHandle(Hicon);
            }
        }

    }
}
