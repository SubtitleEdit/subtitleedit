using System;
using System.Drawing;
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

            if (Width < radioButtonRegEx.Right + 5)
                Width = radioButtonRegEx.Right + 5;

            Utilities.FixLargeFonts(this, buttonCancel);
        }

        private FindType FindType
        {
            get
            {
                if (radioButtonNormal.Checked)
                    return FindType.Normal;
                if (radioButtonCaseSensitive.Checked)
                    return FindType.CaseSensitive;
                return FindType.RegEx;
            }
            set
            {
                if (value == FindType.CaseSensitive)
                    radioButtonCaseSensitive.Checked = true;
                if (value == FindType.Normal)
                    radioButtonNormal.Checked = true;
                if (value == FindType.RegEx)
                    radioButtonRegEx.Checked = true;
            }
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
            return new FindReplaceDialogHelper(FindType, FindText, _regEx, string.Empty, 200, 300, startLineIndex);
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

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ButtonFind_Click(null, null);
        }

        private void ComboBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ButtonFind_Click(null, null);
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
                FindType = findHelper.FindType;
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