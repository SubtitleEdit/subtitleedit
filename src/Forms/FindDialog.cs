using Nikse.SubtitleEdit.Core;
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

            Text = Configuration.Settings.Language.FindDialog.Title;
            buttonFind.Text = Configuration.Settings.Language.FindDialog.Find;
            radioButtonNormal.Text = Configuration.Settings.Language.FindDialog.Normal;
            radioButtonCaseSensitive.Text = Configuration.Settings.Language.FindDialog.CaseSensitive;
            radioButtonRegEx.Text = Configuration.Settings.Language.FindDialog.RegularExpression;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            checkBoxWholeWord.Text = Configuration.Settings.Language.FindDialog.WholeWord;
            buttonCount.Text = Configuration.Settings.Language.FindDialog.Count;
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
            labelCount.ForeColor = count > 0 ? Color.Blue : Color.Red;
            labelCount.Text = count == 1 ? Configuration.Settings.Language.FindDialog.OneMatch : string.Format(Configuration.Settings.Language.FindDialog.XNumberOfMatches, count);
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
