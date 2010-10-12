using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Enums;

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
        }

        private FindType GetFindType()
        {
            if (radioButtonNormal.Checked)
                return FindType.Normal;
            if (radioButtonCaseSensitive.Checked)
                return FindType.CaseSensitive;
            return FindType.RegEx;
        }

        public FindReplaceDialogHelper GetFindDialogHelper(int startLineIndex)
        {
            return new FindReplaceDialogHelper(GetFindType(), textBoxFind.Text, _regEx, string.Empty, 200, 300, startLineIndex);
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
            string searchText = textBoxFind.Text.Trim();

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
                    _regEx = new Regex(textBoxFind.Text, RegexOptions.Compiled);
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

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButtonRegEx)
                textBoxFind.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind);
            else
                textBoxFind.ContextMenuStrip = null;
        }

        internal void Initialize(string selectedText, FindReplaceDialogHelper findHelper)
        {
            textBoxFind.Text = selectedText;
            textBoxFind.SelectAll();

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

    }
}
