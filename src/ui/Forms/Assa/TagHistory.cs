using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class TagHistory : Form
    {
        public string HistoryStyle { get; private set; }

        public TagHistory()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.AssaOverrideTags.History;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            FillHistory();
        }

        private void FillHistory()
        {
            listBoxHistory.BeginUpdate();
            listBoxHistory.Items.Clear();
            foreach (var tag in Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory)
            {
                listBoxHistory.Items.Add(tag);
            }

            listBoxHistory.EndUpdate();

            if (Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory.Count > 0)
            {
                listBoxHistory.SelectedIndex = 0;
            }
        }

        private void listBoxHistory_SelectedIndexChanged(object sender, System.EventArgs e)
        {
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            var idx = listBoxHistory.SelectedIndex;
            if (idx < 0)
            {
                return;
            }

            HistoryStyle = Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory[idx];
            DialogResult = DialogResult.OK;
        }

        private void TagHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.OpenUrl("https://www.nikse.dk/SubtitleEdit/AssaOverrideTags");
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Enter)
            {
                buttonOK_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void listBoxHistory_DoubleClick(object sender, System.EventArgs e)
        {
            if (listBoxHistory.SelectedIndex != ListBox.NoMatches)
            {
                buttonOK_Click(null, null);
            }
        }

        private void removeToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var indices = new List<int>();
            foreach (int idx in listBoxHistory.SelectedIndices)
            {
                indices.Add(idx);
            }

            foreach (var idx in indices.OrderByDescending(p => p))
            {
                Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory.RemoveAt(idx);
            }

            FillHistory();
        }
    }
}
