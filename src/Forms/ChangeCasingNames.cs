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
            Cursor = Cursors.WaitCursor;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string text = p.Text;
                foreach (ListViewItem item in listViewNames.Items)
                {
                    string name = item.SubItems[1].Text;

                    string textNoTags = Utilities.RemoveHtmlTags(text);
                    if (textNoTags != textNoTags.ToUpper())
                    {
                        if (item.Checked && text != null && text.Contains(name, StringComparison.OrdinalIgnoreCase) && name.Length > 1 && name != name.ToLower())
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
            var namesEtcList = new List<string>();
            // Will contains both one word names and multi names

            //string languageCode = Utilities.AutoDetectGoogleLanguage(_subtitle);
            string languageCode = Utilities.AutoDetectLanguageName("en_US", _subtitle);
            if (string.IsNullOrEmpty(languageCode))
                languageCode = "en_US";

            NamesList.LoadNamesEtcWordLists(namesEtcList, namesEtcList, languageCode);
            if (languageCode.StartsWith("en"))
            {
                if (namesEtcList.Contains("Black"))
                    namesEtcList.Remove("Black");
                if (namesEtcList.Contains("Bill"))
                    namesEtcList.Remove("Bill");
                if (namesEtcList.Contains("Long"))
                    namesEtcList.Remove("Long");
                if (namesEtcList.Contains("Don"))
                    namesEtcList.Remove("Don");
            }

            var sb = new StringBuilder();
            foreach (Paragraph p in _subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = Utilities.RemoveHtmlTags(sb.ToString());
            string textToLower = text.ToLower();
            foreach (string name in namesEtcList)
            {
                int startIndex = textToLower.IndexOf(name.ToLower(), StringComparison.Ordinal);
                if (startIndex >= 0)
                {
                    while (startIndex >= 0 && startIndex < text.Length &&
                           textToLower.Substring(startIndex).Contains(name.ToLower()) && name.Length > 1 && name != name.ToLower())
                    {
                        bool startOk = (startIndex == 0) || (text[startIndex - 1] == ' ') || (text[startIndex - 1] == '-') ||
                                       (text[startIndex - 1] == '"') || (text[startIndex - 1] == '\'') || (text[startIndex - 1] == '>') ||
                                       (Environment.NewLine.EndsWith(text[startIndex - 1].ToString(CultureInfo.InvariantCulture)));

                        if (startOk)
                        {
                            int end = startIndex + name.Length;
                            bool endOk = end <= text.Length;
                            if (endOk)
                                endOk = end == text.Length || (@" ,.!?:;')-<""" + Environment.NewLine).Contains(text[end]);

                            if (endOk && text.Substring(startIndex, name.Length) != name) // do not add names where casing already is correct
                            {
                                if (!_usedNames.Contains(name))
                                {
                                    _usedNames.Add(name);
                                    AddToListViewNames(name);
                                    break; // break while
                                }
                            }
                        }

                        startIndex = textToLower.IndexOf(name.ToLower(), startIndex + 2, StringComparison.Ordinal);
                    }
                }
            }
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