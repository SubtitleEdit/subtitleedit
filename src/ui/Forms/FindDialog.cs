using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FindDialog : PositionAndSizeForm
    {
        private Regex _regEx;
        private readonly Subtitle _subtitle;
        public FindDialog(Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.FindDialog.Title;
            buttonFind.Text = LanguageSettings.Current.FindDialog.Find;
            radioButtonNormal.Text = LanguageSettings.Current.FindDialog.Normal;
            radioButtonCaseSensitive.Text = LanguageSettings.Current.FindDialog.CaseSensitive;
            radioButtonRegEx.Text = LanguageSettings.Current.FindDialog.RegularExpression;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            checkBoxWholeWord.Text = LanguageSettings.Current.FindDialog.WholeWord;
            buttonCount.Text = LanguageSettings.Current.FindDialog.Count;
            labelCount.Text = string.Empty;
            _subtitle = subtitle;

            if (Width < radioButtonRegEx.Right + 5)
            {
                Width = radioButtonRegEx.Right + 5;
            }

            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        private ReplaceType FindReplaceType
        {
            get
            {
                var result = new ReplaceType();
                if (radioButtonNormal.Checked)
                {
                    result.FindType = FindType.Normal;
                }
                else if (radioButtonCaseSensitive.Checked)
                {
                    result.FindType = FindType.CaseSensitive;
                }
                else
                {
                    result.FindType = FindType.RegEx;
                }

                result.WholeWord = checkBoxWholeWord.Checked;
                return result;
            }
            set
            {
                switch (value.FindType)
                {
                    case FindType.Normal:
                        radioButtonNormal.Checked = true;
                        break;
                    case FindType.CaseSensitive:
                        radioButtonCaseSensitive.Checked = true;
                        break;
                    case FindType.RegEx:
                        radioButtonRegEx.Checked = true;
                        break;
                }
            }
        }

        private string FindText
        {
            get
            {
                if (Configuration.Settings.Tools.FindHistory.Count == 0)
                {
                    return textBoxFind.Text;
                }

                return comboBoxFind.Text;
            }
        }

        public FindReplaceDialogHelper GetFindDialogHelper(int startLineIndex)
        {
            return new FindReplaceDialogHelper(FindReplaceType, FindText, _regEx, string.Empty, startLineIndex);
        }

        private void FormFindDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ButtonFind_Click(object sender, EventArgs e)
        {
            string searchText = FindText;
            textBoxFind.Text = searchText;

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
                    _regEx = new Regex(RegexUtils.FixNewLine(searchText), RegexOptions.Compiled);
                    DialogResult = DialogResult.OK;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonFind_Click(null, null);
            }
        }

        private void ComboBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonFind_Click(null, null);
            }
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButtonRegEx)
            {
                if (textBoxFind.Visible)
                {
                    comboBoxFind.ContextMenuStrip = null;
                    textBoxFind.ContextMenuStrip = FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind);
                }
                else
                {
                    textBoxFind.ContextMenuStrip = null;
                    comboBoxFind.ContextMenuStrip = FindReplaceDialogHelper.GetRegExContextMenu(comboBoxFind);
                }
            }
            else
            {
                textBoxFind.ContextMenuStrip = null;
                comboBoxFind.ContextMenuStrip = null;
            }
            checkBoxWholeWord.Enabled = !radioButtonRegEx.Checked;
            labelCount.Text = string.Empty;
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
                FindReplaceType = findHelper.FindReplaceType;
                checkBoxWholeWord.Checked = findHelper.FindReplaceType.WholeWord;
                checkBoxWholeWord.Enabled = FindReplaceType.FindType != FindType.RegEx;
            }
        }

        internal void SetIcon(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                Icon = Icon.FromHandle(bitmap.GetHicon());
            }
        }

        private void buttonCount_Click(object sender, EventArgs e)
        {
            if (FindText.Length == 0)
            {
                labelCount.Text = string.Empty;
                return;
            }
            var count = GetFindDialogHelper(0).FindCount(_subtitle, checkBoxWholeWord.Checked);
            var colorIfFound = Configuration.Settings.General.UseDarkTheme ? Color.FromArgb(9, 128, 204) : Color.Blue;
            labelCount.ForeColor = count > 0 ? colorIfFound : Color.Red;
            labelCount.Text = count == 1 ? LanguageSettings.Current.FindDialog.OneMatch : string.Format(LanguageSettings.Current.FindDialog.XNumberOfMatches, count);
        }

        private void comboBoxFind_TextChanged(object sender, EventArgs e)
        {
            labelCount.Text = string.Empty;
        }

        private void textBoxFind_TextChanged(object sender, EventArgs e)
        {
            labelCount.Text = string.Empty;
        }

        private void checkBoxWholeWord_CheckedChanged(object sender, EventArgs e)
        {
            labelCount.Text = string.Empty;
        }

    }
}
