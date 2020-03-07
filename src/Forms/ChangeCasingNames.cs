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
        private readonly HashSet<string> _usedNames = new HashSet<string>();
        private Subtitle _subtitle;
        private const string ExpectedEndChars = " ,.!?:;')]<-\"\r\n";
        private NameList _nameList;
        private List<string> _nameListInclMulti;
        private string _language;

        public ChangeCasingNames()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
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
            toolStripMenuItem1SelectAll.Text = Configuration.Settings.Language.FixCommonErrors.SelectAll;
            toolStripMenuItem2InverseSelection.Text = Configuration.Settings.Language.FixCommonErrors.InverseSelection;
            labelExtraNames.Text = Configuration.Settings.Language.ChangeCasingNames.ExtraNames;
            buttonAddCustomNames.Text = Configuration.Settings.Language.DvdSubRip.Add;
            toolStripMenuItemInverseSelection.Text = Configuration.Settings.Language.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.SelectAll;
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

        public int LinesChanged { get; private set; }

        private void ChangeCasingNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
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

            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
            if (string.IsNullOrEmpty(_language))
            {
                _language = "en_US";
            }

            _nameList = new NameList(Configuration.DictionariesDirectory, _language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            _nameListInclMulti = _nameList.GetAllNames(); // Will contains both one word names and multi names

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
                foreach (ListViewItem item in listViewNames.Items)
                {
                    string name = item.SubItems[1].Text;

                    string textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
                    if (textNoTags != textNoTags.ToUpperInvariant())
                    {
                        if (item.Checked && text != null && text.Contains(name, StringComparison.OrdinalIgnoreCase) && name.Length > 1 && name != name.ToLowerInvariant())
                        {
                            var st = new StrippableText(text);
                            st.FixCasing(new List<string> { name }, true, false, false, string.Empty);
                            text = st.MergedString;
                        }
                    }
                }
                if (text != p.Text)
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
            item.SubItems.Add(UiUtil.GetListViewTextFromString(p.Text));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(newText));
            listViewFixes.Items.Add(item);
        }

        private void AddCustomNames()
        {
            foreach (string s in textBoxExtraNames.Text.Split(','))
            {
                var name = s.Trim();
                if (name.Length > 1 && !_nameListInclMulti.Contains(name))
                {
                    _nameListInclMulti.Add(name);
                }
            }
        }

        private void FindAllNames()
        {
            string text = HtmlUtil.RemoveHtmlTags(_subtitle.GetAllTexts());
            string textToLower = text.ToLowerInvariant();
            listViewNames.BeginUpdate();
            foreach (string name in _nameListInclMulti)
            {
                int startIndex = textToLower.IndexOf(name.ToLowerInvariant(), StringComparison.Ordinal);
                if (startIndex >= 0)
                {
                    while (startIndex >= 0 && startIndex < text.Length &&
                           textToLower.Substring(startIndex).Contains(name.ToLowerInvariant()) && name.Length > 1 && name != name.ToLowerInvariant())
                    {
                        bool startOk = startIndex == 0 || "([ --'>\r\n¿¡\"”“„".Contains(text[startIndex - 1]);
                        if (startOk)
                        {
                            int end = startIndex + name.Length;
                            bool endOk = end <= text.Length;
                            if (endOk)
                            {
                                endOk = end == text.Length || ExpectedEndChars.Contains(text[end]);
                            }

                            if (endOk && text.Substring(startIndex, name.Length) != name) // do not add names where casing already is correct
                            {
                                if (!_usedNames.Contains(name))
                                {
                                    var isDont = _language.StartsWith("en", StringComparison.OrdinalIgnoreCase) && text.Substring(startIndex).StartsWith("don't", StringComparison.InvariantCultureIgnoreCase);
                                    if (!isDont)
                                    {
                                        _usedNames.Add(name);
                                        AddToListViewNames(name);
                                        break; // break while
                                    }
                                }
                            }
                        }

                        startIndex = textToLower.IndexOf(name.ToLowerInvariant(), startIndex + 2, StringComparison.Ordinal);
                    }
                }
            }
            listViewNames.EndUpdate();
            groupBoxNames.Text = string.Format(Configuration.Settings.Language.ChangeCasingNames.NamesFoundInSubtitleX, listViewNames.Items.Count);
        }

        private void ListViewNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            labelXLinesSelected.Text = string.Empty;
            if (listViewNames.SelectedItems.Count != 1)
            {
                return;
            }

            string name = listViewNames.SelectedItems[0].SubItems[1].Text;
            listViewFixes.BeginUpdate();

            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Selected = false;

                string text = UiUtil.GetStringFromListViewText(item.SubItems[2].Text);

                string lower = text.ToLowerInvariant();
                if (lower.Contains(name.ToLowerInvariant()) && name.Length > 1 && name != name.ToLowerInvariant())
                {
                    int start = lower.IndexOf(name.ToLowerInvariant(), StringComparison.Ordinal);
                    if (start >= 0)
                    {
                        bool startOk = start == 0 || lower[start - 1] == ' ' || lower[start - 1] == '-' || lower[start - 1] == '"' ||
                                       lower[start - 1] == '\'' || lower[start - 1] == '>' || Environment.NewLine.EndsWith(lower[start - 1]);

                        if (startOk)
                        {
                            int end = start + name.Length;
                            bool endOk = end <= lower.Length;
                            if (endOk)
                            {
                                endOk = end == lower.Length || ExpectedEndChars.Contains(lower[end]);
                            }

                            item.Selected = endOk;
                        }
                    }
                }
            }

            listViewFixes.EndUpdate();

            if (listViewFixes.SelectedItems.Count > 0)
            {
                listViewFixes.EnsureVisible(listViewFixes.SelectedItems[0].Index);
            }
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
                    LinesChanged++;
                    var p = item.Tag as Paragraph;
                    if (p != null)
                    {
                        p.Text = UiUtil.GetStringFromListViewText(item.SubItems[3].Text);
                    }
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
            {
                labelXLinesSelected.Text = string.Format(Configuration.Settings.Language.Main.XLinesSelected, listViewFixes.SelectedItems.Count);
            }
            else
            {
                labelXLinesSelected.Text = string.Empty;
            }
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
                {
                    item.Checked = true;
                }
                else
                {
                    item.Checked = !item.Checked;
                }
            }
            listViewNames.EndUpdate();
            listViewNames.ItemChecked += ListViewNamesItemChecked;
            GeneratePreview();
        }

        private void buttonAddCustomNames_Click(object sender, EventArgs e)
        {
            AddCustomNames();
            textBoxExtraNames.Text = string.Empty;
            FindAllNames();
        }

        private void toolStripMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = true;
            }
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = !item.Checked;
            }
        }

        private void toolStripMenuItem1SelectAll_Click(object sender, EventArgs e)
        {
            DoSelection(true);
        }

        private void toolStripMenuItem2InverseSelection_Click(object sender, EventArgs e)
        {
            DoSelection(false);
        }
    }
}
