﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MultipleReplace : Form
    {
        public const string SearchTypeNormal = "Normal";
        public const string SearchTypeCaseSensitive = "CaseSensitive";
        public const string SearchTypeRegularExpression = "RegularExpression";
        Dictionary<string, Regex> _compiledRegExList = new Dictionary<string, Regex>();
        Subtitle _subtitle;
        public Subtitle FixedSubtitle { get; private set; }
        public int FixCount { get; private set; }
        private List<MultipleSearchAndReplaceSetting> oldMultipleSearchAndReplaceList = new List<MultipleSearchAndReplaceSetting>();

        public MultipleReplace()
        {
            InitializeComponent();

            openFileDialog1.FileName = string.Empty;
            saveFileDialog1.FileName = string.Empty;

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
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.After;
            deleteToolStripMenuItem.Text = Configuration.Settings.Language.MultipleReplace.Delete;
            buttonRemoveAll.Text = Configuration.Settings.Language.MultipleReplace.RemoveAll;
            buttonImport.Text = Configuration.Settings.Language.MultipleReplace.Import;
            buttonExport.Text = Configuration.Settings.Language.MultipleReplace.Export;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonReplacesSelectAll.Text = Configuration.Settings.Language.FixCommonErrors.SelectAll;
            buttonReplacesInverseSelection.Text = Configuration.Settings.Language.FixCommonErrors.InverseSelection;
            FixLargeFonts();
            splitContainer1.Panel1MinSize = 200;
            splitContainer1.Panel2MinSize = 200;

            moveUpToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubrip.MoveUp;
            moveDownToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubrip.MoveDown;

            radioButtonCaseSensitive.Left = radioButtonNormal.Left + radioButtonNormal.Width + 40;
            radioButtonRegEx.Left = radioButtonCaseSensitive.Left + radioButtonCaseSensitive.Width + 40;
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
                oldMultipleSearchAndReplaceList.Add(item);
            }

            GeneratePreview();
            if (subtitle == null || subtitle.Paragraphs == null || subtitle.Paragraphs.Count == 0)
                groupBoxLinesFound.Enabled = false;
        }

        private void MultipleReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == Keys.F1)
                Utilities.ShowHelp("#multiple_replace");
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
                            Regex r;
                            if (_compiledRegExList.ContainsKey(findWhat))
                            {
                                r = _compiledRegExList[findWhat];
                            }
                            else
                            {
                                r = new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline);
                                _compiledRegExList.Add(findWhat, r);
                            }

                            string result = r.Replace(newText, replaceWith);
                            if (result != newText)
                            {
                                hit = true;
                                newText = result;
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
            if (listViewReplaceList.SelectedItems.Count == 1)
            {
                if (e.KeyCode == Keys.Up && e.Control && !e.Alt && !e.Shift)
                    moveUpToolStripMenuItem_Click(sender, e);
                if (e.KeyCode == Keys.Down && e.Control && !e.Alt && !e.Shift)
                    moveDownToolStripMenuItem_Click(sender, e);
            }
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

        private void ExportClick(object sender, EventArgs e)
        {
            if (listViewReplaceList.Items.Count == 0)
                return;

            saveFileDialog1.Title = Configuration.Settings.Language.MultipleReplace.ImportRulesTitle;
            saveFileDialog1.Filter = Configuration.Settings.Language.MultipleReplace.Rules + "|*.template";
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                SaveReplaceList(false);

                var textWriter = new XmlTextWriter(saveFileDialog1.FileName, null) { Formatting = Formatting.Indented };
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("Settings", "");
                textWriter.WriteStartElement("MultipleSearchAndReplaceList", "");
                foreach (var item in Configuration.Settings.MultipleSearchAndReplaceList)
                {
                    textWriter.WriteStartElement("MultipleSearchAndReplaceItem", "");
                    textWriter.WriteElementString("Enabled", item.Enabled.ToString());
                    textWriter.WriteElementString("FindWhat", item.FindWhat);
                    textWriter.WriteElementString("ReplaceWith", item.ReplaceWith);
                    textWriter.WriteElementString("SearchType", item.SearchType);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
                textWriter.Close();
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = Configuration.Settings.Language.MultipleReplace.ImportRulesTitle;
            openFileDialog1.Filter = Configuration.Settings.Language.MultipleReplace.Rules + "|*.template";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var doc = new XmlDocument();
                try
                {
                    doc.Load(openFileDialog1.FileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }

                foreach (XmlNode listNode in doc.DocumentElement.SelectNodes("MultipleSearchAndReplaceList/MultipleSearchAndReplaceItem"))
                {
                    MultipleSearchAndReplaceSetting item = new MultipleSearchAndReplaceSetting();
                    XmlNode subNode = listNode.SelectSingleNode("Enabled");
                    if (subNode != null)
                        item.Enabled = Convert.ToBoolean(subNode.InnerText);
                    subNode = listNode.SelectSingleNode("FindWhat");
                    if (subNode != null)
                        item.FindWhat = subNode.InnerText;
                    subNode = listNode.SelectSingleNode("ReplaceWith");
                    if (subNode != null)
                        item.ReplaceWith = subNode.InnerText;
                    subNode = listNode.SelectSingleNode("SearchType");
                    if (subNode != null)
                        item.SearchType = subNode.InnerText;
                    Configuration.Settings.MultipleSearchAndReplaceList.Add(item);
                }

                listViewReplaceList.Items.Clear();
                foreach (var item in Configuration.Settings.MultipleSearchAndReplaceList)
                    AddToReplaceListView(item.Enabled, item.FindWhat, item.ReplaceWith, EnglishSearchTypeToLocal(item.SearchType));
                GeneratePreview();
            }
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            listViewReplaceList.Items.Clear();
            Configuration.Settings.MultipleSearchAndReplaceList.Clear();
        }

        private void MultipleReplace_Shown(object sender, EventArgs e)
        {
            listViewReplaceList.ItemChecked +=ListViewReplaceListItemChecked;
            GeneratePreview();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Configuration.Settings.MultipleSearchAndReplaceList.Clear();
            foreach (var item in oldMultipleSearchAndReplaceList)
            {
                Configuration.Settings.MultipleSearchAndReplaceList.Add(item);
            }
            DialogResult = DialogResult.Cancel;
        }


    }
}
