using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public partial class TranslationAutoSuffix : Form
    {
        public List<string> Suffixes { get; set; }

        public TranslationAutoSuffix(List<string> suffixes)
        {
            InitializeComponent();

            foreach (var suffix in suffixes)
            {
                listViewNames.Items.Add(suffix);
            }
        }

        private void buttonRemoveNameEtc_Click(object sender, System.EventArgs e)
        {
            if (listViewNames.SelectedItems.Count == 0)
            {
                return;
            }

            var index = listViewNames.SelectedItems[0].Index;
            var text = listViewNames.Items[index].Text;
            var itemsToRemoveCount = listViewNames.SelectedIndices.Count;
            if (index >= 0)
            {
                DialogResult result;
                if (itemsToRemoveCount == 1)
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.Settings.RemoveX, text), "Subtitle Edit", MessageBoxButtons.YesNo);
                }
                else
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, itemsToRemoveCount), "Subtitle Edit", MessageBoxButtons.YesNo);
                }

                if (result != DialogResult.Yes)
                {
                    return;
                }

                for (int idx = listViewNames.SelectedIndices.Count - 1; idx >= 0; idx--)
                {
                    index = listViewNames.SelectedIndices[idx];
                    text = listViewNames.Items[index].Text;
                    listViewNames.Items.RemoveAt(index);
                }
            }
        }

        private void buttonAddNames_Click(object sender, System.EventArgs e)
        {
            var text = textBoxNameEtc.Text.RemoveControlCharacters().Trim();
            if (text.Length == 0)
            {
                return;
            }

            var illegalChars = new List<char>
            {
                '#', '%', '&','{','}','\\','<','>','*','?','/','!','\'','"',':','@'
            };

            foreach (var illegalChar in illegalChars)
            {
                if (text.Contains(illegalChar))
                {
                    return;
                }
            }

            var suffixes = GetSuffixes();
            if (!suffixes.Contains(text))
            {
                suffixes.Add(text);
                suffixes.Sort();
                listViewNames.Items.Clear();
                listViewNames.BeginUpdate();
                foreach (var suffix in suffixes)
                {
                    listViewNames.Items.Add(suffix);
                }
                listViewNames.EndUpdate();

                textBoxNameEtc.Text = string.Empty;
                textBoxNameEtc.Focus();
                for (int i = 0; i < listViewNames.Items.Count; i++)
                {
                    if (listViewNames.Items[i].ToString() == text)
                    {
                        listViewNames.Items[i].Selected = true;
                        listViewNames.Items[i].Focused = true;
                        var top = i - 5;
                        if (top < 0)
                        {
                            top = 0;
                        }

                        listViewNames.EnsureVisible(top);
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show(LanguageSettings.Current.Settings.WordAlreadyExists);
            }
        }

        private List<string> GetSuffixes()
        {
            var suffixes = new List<string>();
            foreach (ListViewItem listViewItem in listViewNames.Items)
            {
                suffixes.Add(listViewItem.Text);
            }

            return suffixes;
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {

            Suffixes = GetSuffixes();
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
