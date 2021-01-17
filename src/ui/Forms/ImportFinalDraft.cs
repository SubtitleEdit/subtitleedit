using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportFinalDraft : Form
    {
        public List<string> ChosenParagraphTypes { get; set; }

        public ImportFinalDraft(List<string> paragraphTypes)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            buttonOk.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOk);

            ChosenParagraphTypes = new List<string>();
            foreach (var paragraphType in paragraphTypes)
            {
                if (paragraphType != "Character")
                {
                    listView1.Items.Add(paragraphType);
                }
                if (paragraphType == "Dialogue" || paragraphType == "Parenthetical")
                {
                    listView1.Items[listView1.Items.Count - 1].Checked = true;
                }
            }
        }

        private void buttonOk_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                ChosenParagraphTypes.Add(item.Text);
            }
            DialogResult = DialogResult.OK;
        }

        private void ImportFinalDraft_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
