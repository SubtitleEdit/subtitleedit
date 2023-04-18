using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ReplaceDialog : PositionAndSizeForm
    {
        private readonly IFindAndReplace _findAndReplaceMethods;
        private readonly Keys _findNextShortcut;
        private Regex _regEx;
        private bool _userAction;
        private bool _findNext;
        private FindReplaceDialogHelper _findHelper;

        public ReplaceDialog(IFindAndReplace findAndReplaceMethods)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _findAndReplaceMethods = findAndReplaceMethods;

            Text = LanguageSettings.Current.ReplaceDialog.Title;
            labelFindWhat.Text = LanguageSettings.Current.ReplaceDialog.FindWhat;
            radioButtonNormal.Text = LanguageSettings.Current.ReplaceDialog.Normal;
            radioButtonCaseSensitive.Text = LanguageSettings.Current.ReplaceDialog.CaseSensitive;
            radioButtonRegEx.Text = LanguageSettings.Current.ReplaceDialog.RegularExpression;
            labelReplaceWith.Text = LanguageSettings.Current.ReplaceDialog.ReplaceWith;
            checkBoxWholeWord.Text = LanguageSettings.Current.FindDialog.WholeWord;
            buttonFind.Text = LanguageSettings.Current.ReplaceDialog.Find;
            buttonReplace.Text = LanguageSettings.Current.ReplaceDialog.Replace;
            buttonReplaceAll.Text = LanguageSettings.Current.ReplaceDialog.ReplaceAll;

            if (Width < radioButtonRegEx.Right + 5)
            {
                Width = radioButtonRegEx.Right + 5;
            }

            UiUtil.FixLargeFonts(this, buttonReplace);
            _findNextShortcut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditFindNext);

            comboBoxFindReplaceIn.Items.Clear();
            comboBoxFindReplaceIn.Items.Add(LanguageSettings.Current.ReplaceDialog.TranslationAndOriginal);
            comboBoxFindReplaceIn.Items.Add(LanguageSettings.Current.ReplaceDialog.TranslationOnly);
            comboBoxFindReplaceIn.Items.Add(LanguageSettings.Current.ReplaceDialog.OriginalOnly);
            comboBoxFindReplaceIn.SelectedIndex = 0;
        }

        public bool ReplaceAll { get; set; }
        public bool FindOnly { get; set; }

        public ReplaceType GetFindType()
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

            result.SearchOriginal = !comboBoxFindReplaceIn.Enabled ||
                                    comboBoxFindReplaceIn.SelectedIndex == 0 ||
                                    comboBoxFindReplaceIn.SelectedIndex == 2;

            result.SearchTranslation = !comboBoxFindReplaceIn.Enabled ||
                                       comboBoxFindReplaceIn.SelectedIndex == 0 ||
                                       comboBoxFindReplaceIn.SelectedIndex == 1;

            result.WholeWord = checkBoxWholeWord.Checked;

            return result;
        }

        public FindReplaceDialogHelper GetFindDialogHelper(int startLineIndex)
        {
            return new FindReplaceDialogHelper(GetFindType(), textBoxFind.Text, _regEx, textBoxReplace.Text, startLineIndex);
        }

        private void FormReplaceDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else if (e.KeyData == _findNextShortcut)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                var ctrl = ActiveControl;
                Find();
                Focus();
                ctrl?.Focus();
            }
        }

        internal void Initialize(string selectedText, FindReplaceDialogHelper findHelper, bool replaceInOriginal)
        {
            _findHelper = findHelper;
            textBoxFind.Text = selectedText;
            if (FindOnly && !string.IsNullOrEmpty(selectedText))
            {
                _findNext = true;
            }

            //if we are searching for the same thing, then keep the replace text the same.
            if (selectedText == findHelper.FindText)
            {
                textBoxReplace.Text = findHelper.ReplaceText.Replace(Environment.NewLine, "\\n");
            }
            textBoxFind.SelectAll();
            if (findHelper.FindReplaceType.FindType == FindType.RegEx)
            {
                radioButtonRegEx.Checked = true;
            }
            else if (findHelper.FindReplaceType.FindType == FindType.CaseSensitive)
            {
                radioButtonCaseSensitive.Checked = true;
            }
            else
            {
                radioButtonNormal.Checked = true;
            }

            if (findHelper.FindReplaceType.FindType != FindType.RegEx)
            {
                checkBoxWholeWord.Checked = findHelper.FindReplaceType.WholeWord;
            }

            labelFindReplaceIn.Enabled = replaceInOriginal;
            comboBoxFindReplaceIn.Enabled = replaceInOriginal;
        }

        private void ButtonReplaceClick(object sender, EventArgs e)
        {
            ReplaceAll = false;
            FindOnly = false;

            Validate(textBoxFind.Text);
            if (DialogResult == DialogResult.OK)
            {
                UpdateFindHelper();
                _findAndReplaceMethods.ReplaceDialogReplace(_findHelper);
            }

            buttonReplace.Focus();
        }

        private void ButtonReplaceAllClick(object sender, EventArgs e)
        {
            ReplaceAll = true;
            FindOnly = false;

            Validate(textBoxFind.Text);
            if (DialogResult == DialogResult.OK)
            {
                UpdateFindHelper();
                _findAndReplaceMethods.ReplaceDialogReplaceAll(_findHelper);
            }

            buttonReplaceAll.Focus();
        }

        private void Validate(string searchText)
        {
            if (searchText.Length == 0)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (radioButtonNormal.Checked)
            {
                DialogResult = DialogResult.OK;
                _userAction = true;
            }
            else if (radioButtonCaseSensitive.Checked)
            {
                DialogResult = DialogResult.OK;
                _userAction = true;
            }
            else if (radioButtonRegEx.Checked)
            {
                try
                {
                    _regEx = new Regex(RegexUtils.FixNewLine(textBoxFind.Text), RegexOptions.Compiled);
                    DialogResult = DialogResult.OK;
                    _userAction = true;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void ButtonFindClick(object sender, EventArgs e)
        {
            Find();
            buttonFind.Focus();
        }

        private void Find()
        {
            if (_findNext && _findHelper != null)
            {
                _findHelper.ReplaceFromPosition++;
            }

            UpdateFindHelper();

            ReplaceAll = false;
            FindOnly = true;

            Validate(textBoxFind.Text);
            if (DialogResult == DialogResult.OK)
            {
                _findAndReplaceMethods.ReplaceDialogFind(_findHelper);
            }
        }

        private void UpdateFindHelper()
        {
            if (_findHelper == null)
            {
                return;
            }

            _findHelper.FindReplaceType = GetFindType();
            _findHelper.FindText = textBoxFind.Text;
            _findHelper.FindTextLength = textBoxFind.Text.Length;
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            textBoxFind.ContextMenuStrip = sender == radioButtonRegEx 
                ? FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind) 
                : null;

            checkBoxWholeWord.Enabled = !radioButtonRegEx.Checked;
        }

        private void TextBoxFindKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                Find();
                Focus();
                textBoxFind.Focus();
            }
        }

        internal void SetIcon(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                Icon = Icon.FromHandle(bitmap.GetHicon());
            }
        }

        private void ReplaceDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _findAndReplaceMethods.ReplaceDialogClose();

            if (!_userAction)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void textBoxFind_TextChanged(object sender, EventArgs e)
        {
            _findNext = false;
        }

        private void ReplaceDialog_Activated(object sender, EventArgs e)
        {
            var allowReplaceInOriginal = _findAndReplaceMethods.GetAllowReplaceInOriginal();
            labelFindReplaceIn.Enabled = allowReplaceInOriginal;
            comboBoxFindReplaceIn.Enabled = allowReplaceInOriginal;
        }
    }
}
