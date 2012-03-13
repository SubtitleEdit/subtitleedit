using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MultipleReplace : Form
    {
        public const string SearchTypeNormal = "Normal";
        public const string SearchTypeCaseSensitive = "CaseSensitive";
        public const string SearchTypeRegularExpression = "RegularExpression";

        Subtitle _subtitle;
        public Subtitle FixedSubtitle { get; private set; }
        public int FixCount { get; private set; }

        public MultipleReplace()
        {
            InitializeComponent();

            textBoxReplace.ContextMenu = FindReplaceDialogHelper.GetReplaceTextContextMenu(textBoxReplace);
            buttonUpdate.Enabled = false;

            Text = Configuration.Settings.Language.MultipleReplace.Title;
            labelFindWhat.Text = Configuration.Settings.Language.MultipleReplace.FindWhat;
            labelReplaceWith.Text = Configuration.Settings.Language.MultipleReplace.ReplaceWith;
            radioButtonNormal.Text = Configuration.Settings.Language.MultipleReplace.Normal;
            radioButtonRegEx.Text = Configuration.Settings.Language.MultipleReplace.RegularExpression;
            radioButtonCaseSensitive.Text = Configuration.Settings.Language.MultipleReplace.CaseSensitive;
            buttonAdd.Text = Configuration.Settings.Language.MultipleReplace.Add;
            buttonUpdate.Text = Configuration.Settings.Language.MultipleReplace.Update;
            listViewReplaceList.Columns[0].Text = Configuration.Settings.Language.MultipleReplace.Enabled;
            listViewReplaceList.Columns[1].Text = Configuration.Settings.Language.MultipleReplace.FindWhat;
            listViewReplaceList.Columns[2].Text = Configuration.Settings.Language.MultipleReplace.ReplaceWith;
            listViewReplaceList.Columns[3].Text = Configuration.Settings.Language.MultipleReplace.SearchType;
            groupBoxLinesFound.Text = string.Empty;
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.MultipleReplace.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.MultipleReplace.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.MultipleReplace.After;
            deleteToolStripMenuItem.Text = Configuration.Settings.Language.MultipleReplace.Delete;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonReplacesSelectAll.Text = Configuration.Settings.Language.FixCommonErrors.SelectAll;
            buttonReplacesInverseSelection.Text = Configuration.Settings.Language.FixCommonErrors.InverseSelection;
            FixLargeFonts();
            splitContainer1.Panel1MinSize = 200;
            splitContainer1.Panel2MinSize = 200;

            moveUpToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubrip.MoveUp;
            moveDownToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubrip.MoveDown;
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(Subtitle subtitle)
        {
            _subtitle = subtitle;

            foreach (var item in Configuration.Settings.MultipleSearchAndReplaceList)
            {
                AddToReplaceListView(item.Enabled, item.FindWhat, item.ReplaceWith, EnglishSearchTypeToLocal(item.SearchType));
            }

            GeneratePreview();
        }

        private void MultipleReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButtonRegEx)
                textBoxFind.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind);
            else
                textBoxFind.ContextMenuStrip = null;
        }

        private void ButtonAddClick(object sender, EventArgs e)
        {
            if (textBoxFind.Text.Length > 0)
            {
                string searchType = SearchTypeNormal;
                if (radioButtonCaseSensitive.Checked)
                    searchType = SearchTypeCaseSensitive;
                else if (radioButtonRegEx.Checked)
                {
                    searchType = SearchTypeRegularExpression;
                    if (!Utilities.IsValidRegex(textBoxFind.Text))
                    {
                        MessageBox.Show(Configuration.Settings.Language.General.RegularExpressionIsNotValid);
                        textBoxFind.Select();
                        return;
                    }
                }

                AddToReplaceListView(true, textBoxFind.Text, textBoxReplace.Text, EnglishSearchTypeToLocal(searchType));
                GeneratePreview();
                textBoxFind.Text = string.Empty;
                textBoxReplace.Text = string.Empty;
                textBoxFind.Select();
                SaveReplaceList(false);
            }
        }

        private void GeneratePreview()
        {
            Cursor = Cursors.WaitCursor;
            FixedSubtitle = new Subtitle(_subtitle);
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            FixCount = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                bool hit = false;
                string newText = p.Text;

                foreach (ListViewItem item in listViewReplaceList.Items)
                {
                    if (item.Checked && item.SubItems[1].Text.Length > 0)
                    {
                        string findWhat = item.SubItems[1].Text;
                        string replaceWith = item.SubItems[2].Text.Replace(@"\n", Environment.NewLine);
                        string searchType = item.SubItems[3].Text;
                        if (searchType == Configuration.Settings.Language.MultipleReplace.CaseSensitive)
                        {
                            if (newText.Contains(findWhat))
                            {
                                hit = true;
                                newText = newText.Replace(findWhat, replaceWith);
                            }
                        }
                        else if (searchType == Configuration.Settings.Language.MultipleReplace.RegularExpression)
                        {
                            var regex = new Regex(findWhat, RegexOptions.Multiline);
                            var match = regex.Match(newText);
                            if (match.Success)
                            {
                                hit = true;

                                string groupName = Utilities.GetRegExGroup(findWhat);
                                if (groupName != null && match.Groups[groupName] != null && match.Groups[groupName].Success)
                                {
                                    newText = newText.Remove(match.Groups[groupName].Index, match.Groups[groupName].Length);
                                    newText = newText.Insert(match.Groups[groupName].Index, replaceWith);
                                }
                                else
                                {
                                    newText = regex.Replace(newText, replaceWith);
                                }
                            }
                        }
                        else
                        {
                            int index = newText.ToLower().IndexOf(findWhat.ToLower());
                            while (index >= 0)
                            {
                                if (index < newText.Length)
                                    newText = newText.Substring(0, index) + replaceWith + newText.Substring(index + findWhat.Length);
                                else
                                    newText = newText.Substring(0, index) + replaceWith;

                                hit = true;
                                index = newText.ToLower().IndexOf(findWhat.ToLower(), index+replaceWith.Length);
                            }
                        }

                    }
                }
                if (hit)
                {
                    FixCount++;
                    AddToPreviewListView(p, newText);
                    FixedSubtitle.Paragraphs[_subtitle.GetIndex(p)].Text = newText;
                    hit = false;
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.MultipleReplace.LinesFoundX, FixCount);
            Cursor = Cursors.Default;
        }

        private void AddToReplaceListView(bool enabled, string findWhat, string replaceWith, string searchType)
        {
            var item = new ListViewItem("");
            item.Checked = enabled;

            var subItem = new ListViewItem.ListViewSubItem(item, findWhat);
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, replaceWith);
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, searchType);
            item.SubItems.Add(subItem);

            listViewReplaceList.Items.Add(item);
        }

        private void AddToPreviewListView(Paragraph p, string newText)
        {
            var item = new ListViewItem("");
            item.Tag = p;
            item.Checked = true;

            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);
        }

        private static string LocalSearchTypeToEnglish(string searchType)
        {
            if (searchType == Configuration.Settings.Language.MultipleReplace.RegularExpression)
                return SearchTypeRegularExpression;
            if (searchType == Configuration.Settings.Language.MultipleReplace.CaseSensitive)
                return SearchTypeCaseSensitive;
            return SearchTypeNormal;
        }

        private static string EnglishSearchTypeToLocal(string searchType)
        {
            if (searchType == SearchTypeRegularExpression)
                return Configuration.Settings.Language.MultipleReplace.RegularExpression;
            if (searchType == SearchTypeCaseSensitive)
                return Configuration.Settings.Language.MultipleReplace.CaseSensitive;
            return Configuration.Settings.Language.MultipleReplace.Normal;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            ResetUncheckLines();
            SaveReplaceList(true);
            DialogResult = DialogResult.OK;
        }

        private void SaveReplaceList(bool saveToDisk)
        {
            Configuration.Settings.MultipleSearchAndReplaceList = new List<MultipleSearchAndReplaceSetting>();
            foreach (ListViewItem item in listViewReplaceList.Items)
            {
                Configuration.Settings.MultipleSearchAndReplaceList.Add(new MultipleSearchAndReplaceSetting
                {
                    Enabled = item.Checked,
                    FindWhat = item.SubItems[1].Text,
                    ReplaceWith = item.SubItems[2].Text,
                    SearchType = LocalSearchTypeToEnglish(item.SubItems[3].Text)
                });
            }
            if (saveToDisk)
                Configuration.Settings.Save();
        }

        private void ResetUncheckLines()
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (!item.Checked)
                {
                    int index = _subtitle.GetIndex(item.Tag as Paragraph);
                    FixedSubtitle.Paragraphs[index].Text = _subtitle.Paragraphs[index].Text;
                }
            }
        }

        private void ListViewReplaceListItemChecked(object sender, ItemCheckedEventArgs e)
        {
            GeneratePreview();
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            for (int i = listViewReplaceList.Items.Count - 1; i >= 0; i--)
            {
                ListViewItem item = listViewReplaceList.Items[i];
                if (item.Selected)
                    item.Remove();
            }
            GeneratePreview();
            SaveReplaceList(false);
        }

        private void ListViewReplaceListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteToolStripMenuItemClick(null, null);
        }

        private void ButtonUpdateClick(object sender, EventArgs e)
        {
            if (listViewReplaceList.SelectedItems.Count != 1)
                return;

            if (textBoxFind.Text.Length > 0)
            {

                if (radioButtonRegEx.Checked && !Utilities.IsValidRegex(textBoxFind.Text))
                {
                    MessageBox.Show(Configuration.Settings.Language.General.RegularExpressionIsNotValid);
                    textBoxFind.Select();
                    return;
                }

                if (textBoxFind.Text.Length > 0)
                {
                    string searchType = SearchTypeNormal;
                    if (radioButtonCaseSensitive.Checked)
                        searchType = SearchTypeCaseSensitive;
                    else if (radioButtonRegEx.Checked)
                    {
                        searchType = SearchTypeRegularExpression;
                        if (!Utilities.IsValidRegex(textBoxFind.Text))
                        {
                            MessageBox.Show(Configuration.Settings.Language.General.RegularExpressionIsNotValid);
                            textBoxFind.Select();
                            return;
                        }
                    }

                    var item = listViewReplaceList.SelectedItems[0];
                    item.SubItems[1].Text = textBoxFind.Text;
                    item.SubItems[2].Text = textBoxReplace.Text;
                    item.SubItems[3].Text = EnglishSearchTypeToLocal(searchType);

                    GeneratePreview();
                    textBoxFind.Select();
                    SaveReplaceList(false);
                }
            }
        }

        private void ListViewReplaceListSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewReplaceList.SelectedItems.Count == 1)
            {
                buttonUpdate.Enabled = true;
                textBoxFind.Text = listViewReplaceList.SelectedItems[0].SubItems[1].Text;
                textBoxReplace.Text = listViewReplaceList.SelectedItems[0].SubItems[2].Text;
                string searchType = LocalSearchTypeToEnglish(listViewReplaceList.SelectedItems[0].SubItems[3].Text);
                if (searchType == SearchTypeRegularExpression)
                    radioButtonRegEx.Checked = true;
                else if (searchType == SearchTypeCaseSensitive)
                    radioButtonCaseSensitive.Checked = true;
                else
                    radioButtonNormal.Checked = true;
            }
            else
            {
                buttonUpdate.Enabled = false;
            }
        }

        private void TextBoxReplaceKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && textBoxFind.Text.Length > 0 && textBoxReplace.Text.Length > 0)
                ButtonAddClick(null, null);
        }

        private void buttonReplacesSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = true;
        }

        private void buttonReplacesInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = !item.Checked;
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            moveUpToolStripMenuItem.Visible = listViewReplaceList.Items.Count > 1 && listViewReplaceList.SelectedItems.Count == 1;
            moveDownToolStripMenuItem.Visible = listViewReplaceList.Items.Count > 1 && listViewReplaceList.SelectedItems.Count == 1;
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listViewReplaceList.SelectedIndices[0];
            if (index == 0)
                return;

            SwapReplaceList(index, index - 1);
        }

        private void SwapReplaceList(int index, int index2)
        {
            bool enabled = listViewReplaceList.Items[index].Checked;
            string findWhat = listViewReplaceList.Items[index].SubItems[1].Text;
            string replaceWith = listViewReplaceList.Items[index].SubItems[2].Text;
            string searchType = listViewReplaceList.Items[index].SubItems[3].Text;

            listViewReplaceList.Items[index].Checked = listViewReplaceList.Items[index2].Checked;
            listViewReplaceList.Items[index].SubItems[1].Text = listViewReplaceList.Items[index2].SubItems[1].Text;
            listViewReplaceList.Items[index].SubItems[2].Text = listViewReplaceList.Items[index2].SubItems[2].Text;
            listViewReplaceList.Items[index].SubItems[3].Text = listViewReplaceList.Items[index2].SubItems[3].Text;

            listViewReplaceList.Items[index2].Checked = enabled;
            listViewReplaceList.Items[index2].SubItems[1].Text = findWhat;
            listViewReplaceList.Items[index2].SubItems[2].Text = replaceWith;
            listViewReplaceList.Items[index2].SubItems[3].Text = searchType;

            listViewReplaceList.Items[index].Selected = false;
            listViewReplaceList.Items[index2].Selected = true;
            SaveReplaceList(false);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listViewReplaceList.SelectedIndices[0];
            if (index == listViewReplaceList.Items.Count - 1)
                return;

            SwapReplaceList(index, index + 1);
        }

    }
}
