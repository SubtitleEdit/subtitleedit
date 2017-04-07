using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeCasingNames : Form
    {
        // Used to make sure we don't add name twice in listview
        private readonly HashSet<string> _usedNames = new HashSet<string>();
        private int _noOfLinesChanged;
        private Subtitle _subtitle;
        private const string ExpectedEndChars = " ,.!?:;')]<-\"\r\n";
        public ChangeCasingNames()
        {
            InitializeComponent();
            labelXLinesSelected.Text = string.Empty;
            Text = Configuration.Settings.Language.ChangeCasingNames.Title;
            groupBoxNames.Text = string.Empty;
            listViewNames.Columns[0].Text = Configuration.Settings.Language.ChangeCasingNames.Enabled;
            listViewNames.Columns[1].Text = Configuration.Settings.Language.ChangeCasingNames.Name;
            groupBoxLinesFound.Text = string.Empty;
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.After;

            buttonSelectAll.Text = Configuration.Settings.Language.FixCommonErrors.SelectAll;
            buttonInverseSelection.Text = Configuration.Settings.Language.FixCommonErrors.InverseSelection;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            listViewFixes.Resize += delegate
            {
                var width = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width)) / 2;
                listViewFixes.Columns[2].Width = width;
                listViewFixes.Columns[3].Width = width;
            };
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public int LinesChanged
        {
            get { return _noOfLinesChanged; }
        }

        private void ChangeCasingNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void AddToListViewNames(string name)
        {
            var item = new ListViewItem(string.Empty) { Checked = true };
            item.SubItems.Add(name);
            listViewNames.Items.Add(item);
        }

        public void Initialize(Subtitle subtitle)
        {
            _subtitle = subtitle;

            FindAllNames();
            GeneratePreview();
        }

        private void GeneratePreview()
        {
            Cursor = Cursors.WaitCursor;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string text = p.Text;
                if (string.IsNullOrEmpty(text) || text.Length == 1)
                {
                    continue;
                }
                foreach (ListViewItem item in listViewNames.Items)
                {
                    if (!item.Checked)
                    {
                        continue;
                    }
                    string name = item.SubItems[1].Text;
                    string textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
                    if (textNoTags != textNoTags.ToUpper())
                    {
                        if (text.Contains(name, StringComparison.OrdinalIgnoreCase))
                        {
                            var st = new StrippableText(text);
                            st.FixCasing(new List<string> { name }, true, false, false, string.Empty);
                            text = st.MergedString;
                        }
                    }
                }
                if (!text.Equals(p.Text, StringComparison.Ordinal))
                {
                    AddToPreviewListView(p, text);
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.ChangeCasingNames.LinesFoundX, listViewFixes.Items.Count);
            Cursor = Cursors.Default;
        }

        private void AddToPreviewListView(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void FindAllNames()
        {
            string language = LanguageAutoDetect.AutoDetectLanguageName("en_US", _subtitle);
            if (string.IsNullOrEmpty(language))
                language = "en_US";

            var namesList = new NamesList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);

            // Will contains both one word names and multi names
            var namesEtcList = namesList.GetAllNames();

            if (language.StartsWith("en", StringComparison.Ordinal))
            {
                namesEtcList.Remove("Black");
                namesEtcList.Remove("Bill");
                namesEtcList.Remove("Long");
                namesEtcList.Remove("Don");
            }
            string textNoTags = HtmlUtil.RemoveHtmlTags(_subtitle.GetAllTexts(), true);
            listViewNames.BeginUpdate();
            foreach (string name in namesEtcList)
            {
                if (name.Length <= 1 || name.ToLower().Equals(name, StringComparison.Ordinal))
                {
                    continue;
                }

                int startIndex = textNoTags.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                while (startIndex >= 0)
                {
                    // [\-" '>\n\r]
                    bool startOk = (startIndex == 0) || (textNoTags[startIndex - 1] == ' ') || (textNoTags[startIndex - 1] == '-') ||
                                   (textNoTags[startIndex - 1] == '"') || (textNoTags[startIndex - 1] == '\'') || (textNoTags[startIndex - 1] == '>') ||
                                   (textNoTags[startIndex - 1] == '\n') || (textNoTags[startIndex - 1] == '\r');

                    if (startOk)
                    {
                        int endIdx = startIndex + name.Length;
                        bool endOk = endIdx == textNoTags.Length || ExpectedEndChars.Contains(textNoTags[endIdx]);
                        string nameInText = textNoTags.Substring(startIndex, name.Length);
                        // do not add names where casing is already correct
                        if (endOk && !nameInText.Equals(name, StringComparison.Ordinal))
                        {
                            if (!_usedNames.Contains(name))
                            {
                                _usedNames.Add(name);
                                AddToListViewNames(name);
                                break; // break while
                            }
                        }
                    }
                    // keep looking for unbalenced casing
                    startIndex = textNoTags.IndexOf(name, startIndex + name.Length, StringComparison.OrdinalIgnoreCase);
                }
            }
            listViewNames.EndUpdate();
            groupBoxNames.Text = string.Format(Configuration.Settings.Language.ChangeCasingNames.NamesFoundInSubtitleX, listViewNames.Items.Count);
        }

        private void ListViewNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            labelXLinesSelected.Text = string.Empty;
            if (listViewNames.SelectedItems.Count != 1)
                return;

            string name = listViewNames.SelectedItems[0].SubItems[1].Text;
            // invalid name
            if (name.Length <= 1 || name.ToLower() == name)
            {
                return;
            }

            listViewFixes.BeginUpdate();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Selected = false;
                string text = item.SubItems[2].Text.Replace(Configuration.Settings.General.ListViewLineSeparatorString, Environment.NewLine);
                int start = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    bool startOk = (start == 0) || (text[start - 1] == ' ') || (text[start - 1] == '-') || (text[start - 1] == '"') ||
                                   (text[start - 1] == '\'') || (text[start - 1] == '>') || (text[start - 1] == '\n') || (text[start - 1] == '\r');

                    if (startOk)
                    {
                        int end = start + name.Length;
                        // ends ok
                        item.Selected = end == text.Length || ExpectedEndChars.Contains(text[end]);
                    }
                }
            }

            listViewFixes.EndUpdate();

            if (listViewFixes.SelectedItems.Count > 0)
                listViewFixes.EnsureVisible(listViewFixes.SelectedItems[0].Index);
        }

        private void ListViewNamesItemChecked(object sender, ItemCheckedEventArgs e)
        {
            GeneratePreview();
        }

        private void ChangeCasingNames_Shown(object sender, EventArgs e)
        {
            listViewNames.ItemChecked += ListViewNamesItemChecked;
        }

        internal void FixCasing()
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Checked)
                {
                    _noOfLinesChanged++;
                    var p = item.Tag as Paragraph;
                    if (p != null)
                        p.Text = item.SubItems[3].Text.Replace(Configuration.Settings.General.ListViewLineSeparatorString, Environment.NewLine);
                }
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedItems.Count > 1)
                labelXLinesSelected.Text = string.Format(Configuration.Settings.Language.Main.XLinesSelected, listViewFixes.SelectedItems.Count);
            else
                labelXLinesSelected.Text = string.Empty;
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            DoSelection(true);
        }

        private void buttonInverseSelection_Click(object sender, EventArgs e)
        {
            DoSelection(false);
        }

        private void DoSelection(bool selectAll)
        {
            listViewNames.ItemChecked -= ListViewNamesItemChecked;
            listViewNames.BeginUpdate();
            foreach (ListViewItem item in listViewNames.Items)
            {
                if (selectAll)
                    item.Checked = true;
                else
                    item.Checked = !item.Checked;
            }
            listViewNames.EndUpdate();
            listViewNames.ItemChecked += ListViewNamesItemChecked;
            GeneratePreview();
        }
    }
}
