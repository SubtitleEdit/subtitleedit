using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MultipleReplaceNewGroup : Form
    {
        public MultipleReplaceNewGroup(string name)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            textBox1.Text = name;
            GroupName = name;
            label1.Text = LanguageSettings.Current.MultipleReplace.GroupName;
            if (name == string.Empty)
            {
                Text = LanguageSettings.Current.MultipleReplace.NewGroup;
            }
            else
            {
                Text = LanguageSettings.Current.MultipleReplace.RenameGroup;
            }
        }

        public string GroupName { get; set; }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void MultipleReplaceNewGroup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            GroupName = textBox1.Text.Trim();
            DialogResult = DialogResult.OK;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, null);
            }
        }

        private void MultipleReplaceNewGroup_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

    }
}
