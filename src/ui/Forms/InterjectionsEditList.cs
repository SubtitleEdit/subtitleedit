using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class InterjectionsEditList : Form
    {
        public List<string> Interjections { get; set; }
        public List<string> SkipList { get; set; }

        public InterjectionsEditList(List<string> interjections, List<string> skipList)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            SkipList = skipList;
            TextBoxInterjections.Text = string.Join(Environment.NewLine, interjections);
            Text = LanguageSettings.Current.Interjections.Title;
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

        private void buttonSort_Click(object sender, EventArgs e)
        {
            var lines = TextBoxInterjections.Lines.ToList();
            var sb = new StringBuilder();
            foreach (var line in lines.Select(p => p.Trim().ToLowerInvariant().CapitalizeFirstLetter()).OrderBy(f => f, StringComparer.CurrentCulture))
            {
                if (line.Length > 0)
                {
                    sb.AppendLine(line.Replace(" ", "-"));
                }
            }

            TextBoxInterjections.Text = sb.ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var lines = TextBoxInterjections.Lines.ToList();
            Interjections = lines.Select(p => p.Trim().ToLowerInvariant().CapitalizeFirstLetter()).Where(p => !string.IsNullOrEmpty(p)).ToList();
            DialogResult = DialogResult.OK;
        }

        private void InterjectionsEditList_Shown(object sender, EventArgs e)
        {
            TextBoxInterjections.SelectionStart = 0;
            TextBoxInterjections.SelectionLength = 0;
            TextBoxInterjections.Focus();
        }

        private void EditSkipListStartsWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new InterjectionsEditSkipList(SkipList))
            {
                var dr = form.ShowDialog(this);
                if (dr != DialogResult.OK)
                {
                    return;
                }

                SkipList = form.SkipList;
            }
        }
    }
}