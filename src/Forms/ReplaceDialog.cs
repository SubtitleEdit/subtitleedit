using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Enums;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ReplaceDialog : Form
    {
        private Regex _regEx;
        private int _left;
        private int _top;
        private bool _userAction = false;

        public ReplaceDialog()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.ReplaceDialog.Title;
            labelFindWhat.Text = Configuration.Settings.Language.ReplaceDialog.FindWhat;
            radioButtonNormal.Text = Configuration.Settings.Language.ReplaceDialog.Normal;
            radioButtonCaseSensitive.Text = Configuration.Settings.Language.ReplaceDialog.CaseSensitive;
            radioButtonRegEx.Text = Configuration.Settings.Language.ReplaceDialog.RegularExpression;
            labelReplaceWith.Text = Configuration.Settings.Language.ReplaceDialog.ReplaceWith;
            buttonFind.Text = Configuration.Settings.Language.ReplaceDialog.Find;
            buttonReplace.Text = Configuration.Settings.Language.ReplaceDialog.Replace;
            buttonReplaceAll.Text = Configuration.Settings.Language.ReplaceDialog.ReplaceAll;

            if (Width < radioButtonRegEx.Right + 5)
                Width = radioButtonRegEx.Right + 5;

            Utilities.FixLargeFonts(this, buttonReplace);
        }

        public bool ReplaceAll { get; set; }
        public bool FindOnly { get; set; }

        public FindType GetFindType()
        {
            if (radioButtonNormal.Checked)
                return FindType.Normal;
            if (radioButtonCaseSensitive.Checked)
                return FindType.CaseSensitive;
            return FindType.RegEx;
        }

        public FindReplaceDialogHelper GetFindDialogHelper(int startLineIndex)
        {
            return new FindReplaceDialogHelper(GetFindType(), textBoxFind.Text, _regEx, textBoxReplace.Text, _left, _top, startLineIndex);
        }

        private void FormReplaceDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        internal void Initialize(string selectedText, FindReplaceDialogHelper findHelper)
        {
            textBoxFind.Text = selectedText;
            //if we are searching for the same thing, then keep the replace text the same.
            if (selectedText == findHelper.FindText)
                textBoxReplace.Text = findHelper.ReplaceText;
            textBoxFind.SelectAll();
            _left = findHelper.WindowPositionLeft;
            _top = findHelper.WindowPositionTop;

            if (findHelper.FindType == FindType.RegEx)
                radioButtonRegEx.Checked = true;
            else if (findHelper.FindType == FindType.CaseSensitive)
                radioButtonCaseSensitive.Checked = true;
            else
                radioButtonNormal.Checked = true;
        }

        private void ButtonReplaceClick(object sender, EventArgs e)
        {
            ReplaceAll = false;
            FindOnly = false;

            Validate(textBoxFind.Text);
        }

        private void ButtonReplaceAllClick(object sender, EventArgs e)
        {
            ReplaceAll = true;
            FindOnly = false;

            Validate(textBoxFind.Text);
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
                    _regEx = new Regex(textBoxFind.Text, RegexOptions.Compiled);
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
            ReplaceAll = false;
            FindOnly = true;

            Validate(textBoxFind.Text);
        }

        private void FormReplaceDialog_Shown(object sender, EventArgs e)
        {
            if (_left > 0 && _top > 0)
            {
                Left = _left;
                Top = _top;
            }
            else
            {
                _left = Left;
                _top = Top;
            }
        }

        private void FormReplaceDialog_Move(object sender, EventArgs e)
        {
            if (Left > 0 && Top > 0)
            {
                _left = Left;
                _top = Top;
            }
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButtonRegEx)
                textBoxFind.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind);
            else
                textBoxFind.ContextMenu = null;
        }

        private void TextBoxFindKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonFindClick(null, null);
                e.SuppressKeyPress = true;
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

        private void ReplaceDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_userAction)
                DialogResult = DialogResult.Cancel;
        }
    }
}
