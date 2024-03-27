using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FindDialog : PositionAndSizeForm
    {
        private readonly IFindAndReplace _findAndReplaceMethods;
        private readonly Keys _findNextShortcut;
        private readonly Keys _findShortcut;
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
            _findShortcut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditFind);
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
            else if (e.KeyData == _findShortcut)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (comboBoxFind.Visible)
                {
                    comboBoxFind.Focus();
                    comboBoxFind.SelectAll();
                }
                else
                {
                    textBoxFind.Focus();
                    textBoxFind.SelectAll();
                }
            }
        }

        private void ButtonFind_Click(object sender, EventArgs e)
        {
            SetRegEx();
            FindNext();
            buttonFind.Focus();
        }

        private void SetRegEx()
        {
            if (radioButtonRegEx.Checked)
            {
                try
                {
                    _regEx = new Regex(RegexUtils.FixNewLine(FindText), RegexOptions.Compiled, TimeSpan.FromSeconds(5));
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void FindNext()
        {
            var searchText = FindText;
            textBoxFind.Text = searchText;

            if (searchText.Length == 0)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
                _findAndReplaceMethods.FindDialogFind(FindText, FindReplaceType, _regEx);
            }
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(25), () =>
                {
                    FindNext();
                    Focus();
                    textBoxFind.Focus();
                });
            }
        }

        private void ComboBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(25), () =>
                {
                    FindNext();
                    Focus();
                    comboBoxFind.Focus();
                });
            }
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            // remove any existing context menu
            textBoxFind.ContextMenuStrip = null;
            comboBoxFind.ContextMenuStrip = null;
            
            // only hook context menu if regex radio button is checked
            if (sender == radioButtonRegEx && radioButtonRegEx.Checked)
            {
                if (textBoxFind.Visible)
                {
                    textBoxFind.ContextMenuStrip = FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind);
                }
                else if (radioButtonRegEx.Checked)
                {
                    comboBoxFind.ContextMenuStrip = FindReplaceDialogHelper.GetRegExContextMenu(comboBoxFind);
                }
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
                comboBoxFind.Items.Clear();
                for (var index = 0; index < Configuration.Settings.Tools.FindHistory.Count; index++)
                {
                    var s = Configuration.Settings.Tools.FindHistory[index];
                    comboBoxFind.Items.Add(s);
                }
                comboBoxFind.Text = selectedText;
                comboBoxFind.SelectAll();
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

            SetRegEx();
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
                SetRegEx();
                _findAndReplaceMethods.FindDialogFindPrevious(FindText);
            }
            else if (radioButtonCaseSensitive.Checked)
            {
                DialogResult = DialogResult.OK;
                SetRegEx();
                _findAndReplaceMethods.FindDialogFindPrevious(FindText);
            }
            else
            {
                SetRegEx();
            }

            buttonFindPrev.Focus();
        }

        private void FindDialog_Shown(object sender, EventArgs e)
        {
            if (comboBoxFind.Visible)
            {
                comboBoxFind.Focus();
            }
            else
            {
                textBoxFind.Focus();
            }
        }
    }
}
