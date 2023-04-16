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
        private readonly IFindAndReplace _findAndReplaceMethods;
        private readonly Keys _findNextShortcut;
        private Regex _regEx;
        private readonly Subtitle _subtitle;

        public FindDialog(Subtitle subtitle, IFindAndReplace findAndReplaceMethods)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.FindDialog.Title;
            labelFindWhat.Text = LanguageSettings.Current.ReplaceDialog.FindWhat;
            buttonFind.Text = LanguageSettings.Current.FindDialog.FindNext;
            buttonFindPrev.Text = LanguageSettings.Current.FindDialog.FindPrevious;
            radioButtonNormal.Text = LanguageSettings.Current.FindDialog.Normal;
            radioButtonCaseSensitive.Text = LanguageSettings.Current.FindDialog.CaseSensitive;
            radioButtonRegEx.Text = LanguageSettings.Current.FindDialog.RegularExpression;
            checkBoxWholeWord.Text = LanguageSettings.Current.FindDialog.WholeWord;
            buttonCount.Text = LanguageSettings.Current.FindDialog.Count;
            labelCount.Text = string.Empty;
            _subtitle = subtitle;
            _findAndReplaceMethods = findAndReplaceMethods;

            if (Width < radioButtonRegEx.Right + 5)
            {
                Width = radioButtonRegEx.Right + 5;
            }

            UiUtil.FixLargeFonts(this, buttonFind);
            _findNextShortcut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditFindNext);
        }

        private ReplaceType FindReplaceType
        {
            get
            {
                var result = new ReplaceType
                {
                    SearchOriginal = true,
                    SearchTranslation = true
                };

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
                e.Handled = true;
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else if (e.KeyData == _findNextShortcut)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                var ctrl = ActiveControl;
                FindNext();
                Focus();
                ctrl?.Focus();
            }
        }

        private void ButtonFind_Click(object sender, EventArgs e)
        {
            FindNext();
            buttonFind.Focus();
        }

        private void FindNext()
        {
            var searchText = FindText;
            textBoxFind.Text = searchText;

            if (searchText.Length == 0)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (radioButtonNormal.Checked)
            {
                DialogResult = DialogResult.OK;
                _findAndReplaceMethods.FindDialogFind(FindText, FindReplaceType);
            }
            else if (radioButtonCaseSensitive.Checked)
            {
                DialogResult = DialogResult.OK;
                _findAndReplaceMethods.FindDialogFind(FindText, FindReplaceType);
            }
            else if (radioButtonRegEx.Checked)
            {
                try
                {
                    _regEx = new Regex(RegexUtils.FixNewLine(searchText), RegexOptions.Compiled, TimeSpan.FromSeconds(5));
                    DialogResult = DialogResult.OK;
                    _findAndReplaceMethods.FindDialogFind(FindText, FindReplaceType);
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
                e.SuppressKeyPress = true;
                e.Handled = true;

                FindNext();
                Focus();
                textBoxFind.Focus();
            }
        }

        private void ComboBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                FindNext();
                Focus();
                comboBoxFind.Focus();
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

        private void FindDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _findAndReplaceMethods.FindDialogClose();
        }

        private void buttonFindPrev_Click(object sender, EventArgs e)
        {
            var searchText = FindText;
            textBoxFind.Text = searchText;

            if (searchText.Length == 0)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (radioButtonNormal.Checked)
            {
                DialogResult = DialogResult.OK;
                _findAndReplaceMethods.FindDialogFindPrevious(FindText);
            }
            else if (radioButtonCaseSensitive.Checked)
            {
                DialogResult = DialogResult.OK;
                _findAndReplaceMethods.FindDialogFindPrevious(FindText);
            }
            else if (radioButtonRegEx.Checked)
            {
                try
                {
                    _regEx = new Regex(RegexUtils.FixNewLine(searchText), RegexOptions.Compiled, TimeSpan.FromSeconds(5));
                    DialogResult = DialogResult.OK;
                    _findAndReplaceMethods.FindDialogFindPrevious(FindText);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }

            buttonFindPrev.Focus();
        }
    }
}
