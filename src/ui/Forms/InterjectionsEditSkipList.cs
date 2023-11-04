using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class InterjectionsEditSkipList : Form
    {
        public List<string> SkipList { get; set; }

        public InterjectionsEditSkipList(List<string> skipList)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            TextBoxInterjections.Text = string.Join(Environment.NewLine, skipList);
            Text = LanguageSettings.Current.Interjections.EditSkipList.Trim('.');
            labelInfo.Text = LanguageSettings.Current.Interjections.EditSkipListInfo;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void Interjections_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#remove_text_for_hi");
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var lines = TextBoxInterjections.Lines.ToList();
            SkipList = lines.Select(p => p.Trim().ToLowerInvariant().CapitalizeFirstLetter()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            DialogResult = DialogResult.OK;
        }

        private void InterjectionsEditList_Shown(object sender, EventArgs e)
        {
            TextBoxInterjections.SelectionStart = 0;
            TextBoxInterjections.SelectionLength = 0;
            TextBoxInterjections.Focus();
        }
    }
}