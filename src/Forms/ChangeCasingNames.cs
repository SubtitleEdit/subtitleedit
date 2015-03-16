using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeCasingNames : Form
    {
        private readonly List<string> _usedNames = new List<string>();
        private int _noOfLinesChanged;
        private Subtitle _subtitle;

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
            FixLargeFonts();
        }

        public int LinesChanged
        {
            get { return _noOfLinesChanged; }
        }

        private void FixLargeFonts()
        {
            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                var newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void ChangeCasingNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void AddToListViewNames(string name)
        {
            var item = new ListViewItem(string.Empty) { Checked = true };

            var subItem = new ListViewItem.ListViewSubItem(item, name);
            item.SubItems.Add(subItem);

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
            if (listViewNames.Items == null || listViewNames.Items.Count == 0)
                return;
            Cursor = Cursors.WaitCursor;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string text = p.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    foreach (ListViewItem item in listViewNames.Items)
                    {
                        string name = item.SubItems[1].Text;
                        if (item.Checked && name.Length > 1 && name.ToLower() != name && text.Contains(name, StringComparison.OrdinalIgnoreCase))
                        {
                            string textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
                            if (textNoTags != textNoTags.ToUpper())
                            {
                                var st = new StripableText(text);
                                st.FixCasing(new List<string> { name }, true, false, false, string.Empty);
                                text = st.MergedString;
                            }
                        }
                    }
                    if (text != p.Text)
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

            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);
        }

        private void FindAllNames()
        {
            string language = Utilities.AutoDetectLanguageName("en_US", _subtitle);
            if (string.IsNullOrEmpty(language))
                language = "en_US";

            var namesList = new NamesList(Configuration.DictionariesFolder, language, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);

            // Will contains both one word names and multi names
            var namesEtcList = namesList.GetAllNames();

            if (language.StartsWith("en", StringComparison.Ordinal))
            {
                namesEtcList.Remove("Black");
                namesEtcList.Remove("Bill");
                namesEtcList.Remove("Long");
                namesEtcList.Remove("Don");
            }

            var sb = new StringBuilder();
            foreach (Paragraph p in _subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = HtmlUtil.RemoveHtmlTags(sb.ToString());
            listViewNames.BeginUpdate();
            foreach (string name in namesEtcList)
            {
                if (name.Length > 1 && name != name.ToLower())
                {
                    int startIndex = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                    while (startIndex >= 0)
                    {
                        if (text.Substring(startIndex, name.Length) != name && !_usedNames.Contains(name) && Utilities.StartEndOkay(text, name, startIndex)) // do not add names where casing already is correct
                        {
                            _usedNames.Add(name);
                            AddToListViewNames(name);
                            break; // break while
                        }
                        startIndex = text.IndexOf(name, startIndex + name.Length, StringComparison.OrdinalIgnoreCase);
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
                return;

            string name = listViewNames.SelectedItems[0].SubItems[1].Text;
            listViewFixes.BeginUpdate();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Selected = false;

                string text = item.SubItems[2].Text.Replace(Configuration.Settings.General.ListViewLineSeparatorString, Environment.NewLine);

                string lower = text.ToLower();
                if (lower.Contains(name.ToLower()) && name.Length > 1 && name != name.ToLower())
                {
                    int start = lower.IndexOf(name.ToLower(), StringComparison.Ordinal);
                    if (start >= 0)
                    {
                        bool startOk = (start == 0) || (lower[start - 1] == ' ') || (lower[start - 1] == '-') || (lower[start - 1] == '"') ||
                                       lower[start - 1] == '\'' || lower[start - 1] == '>' || Environment.NewLine.EndsWith(lower[start - 1]);

                        if (startOk)
                        {
                            int end = start + name.Length;
                            bool endOk = end <= lower.Length;
                            if (endOk)
                                endOk = end == lower.Length || (@" ,.!?:;')<-""" + Environment.NewLine).Contains(lower[end]);

                            item.Selected = endOk;
                        }
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
            listViewNames.ItemChecked -= ListViewNamesItemChecked;
            listViewNames.BeginUpdate();
            foreach (ListViewItem item in listViewNames.Items)
                item.Checked = true;
            listViewNames.EndUpdate();
            listViewNames.ItemChecked += ListViewNamesItemChecked;
            GeneratePreview();
        }

        private void buttonInverseSelection_Click(object sender, EventArgs e)
        {
            listViewNames.ItemChecked -= ListViewNamesItemChecked;
            listViewNames.BeginUpdate();
            foreach (ListViewItem item in listViewNames.Items)
                item.Checked = !item.Checked;
            listViewNames.EndUpdate();
            listViewNames.ItemChecked += ListViewNamesItemChecked;
            GeneratePreview();
        }

    }
}