﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class TranslationAutoSuffix : Form
    {
        public List<string> Suffixes { get; set; }

        public TranslationAutoSuffix(List<string> suffixes)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            foreach (var suffix in suffixes)
            {
                listViewNames.Items.Add(suffix);
            }

            Text = LanguageSettings.Current.Settings.TranslationAutoSuffix;

            buttonRemoveNameEtc.Text = LanguageSettings.Current.DvdSubRip.Remove;
            buttonAddNames.Text = LanguageSettings.Current.DvdSubRip.Add;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
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
            if (suffixes.Contains(text))
            {
                MessageBox.Show(LanguageSettings.Current.Settings.WordAlreadyExists);
                return;
            }

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
            for (var i = 0; i < listViewNames.Items.Count; i++)
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

        private void TranslationAutoSuffix_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
