using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MultipleReplace : PositionAndSizeForm
    {

        internal class ReplaceExpression
        {
            internal const int SearchNormal = 0;
            internal const int SearchRegEx = 1;
            internal const int SearchCaseSensitive = 2;

            internal string FindWhat { get; set; }
            internal string ReplaceWith { get; set; }
            internal int SearchType { get; set; }

            internal ReplaceExpression(string findWhat, string replaceWith, string searchType)
            {
                FindWhat = findWhat;
                ReplaceWith = replaceWith;
                if (string.CompareOrdinal(searchType, Configuration.Settings.Language.MultipleReplace.RegularExpression) == 0)
                    SearchType = SearchRegEx;
                else if (string.CompareOrdinal(searchType, Configuration.Settings.Language.MultipleReplace.CaseSensitive) == 0)
                    SearchType = SearchCaseSensitive;
            }
        }

        public const string SearchTypeNormal = "Normal";
        public const string SearchTypeCaseSensitive = "CaseSensitive";
        public const string SearchTypeRegularExpression = "RegularExpression";
        private readonly List<MultipleSearchAndReplaceSetting> _oldMultipleSearchAndReplaceList = new List<MultipleSearchAndReplaceSetting>();
        private readonly Dictionary<string, Regex> _compiledRegExList = new Dictionary<string, Regex>();
        private Subtitle _subtitle;
        public List<int> DeleteIndices { get; private set; }
        public Subtitle FixedSubtitle { get; private set; }
        public int FixCount { get; private set; }

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
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonReplacesSelectAll.Text = Configuration.Settings.Language.FixCommonErrors.SelectAll;
            buttonReplacesInverseSelection.Text = Configuration.Settings.Language.FixCommonErrors.InverseSelection;
            Utilities.FixLargeFonts(this, buttonOK);
            splitContainer1.Panel1MinSize = 200;
            splitContainer1.Panel2MinSize = 200;

            moveUpToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubRip.MoveDown;
            moveTopToolStripMenuItem.Text = Configuration.Settings.Language.MultipleReplace.MoveToTop;
            moveBottomToolStripMenuItem.Text = Configuration.Settings.Language.MultipleReplace.MoveToBottom;

            radioButtonCaseSensitive.Left = radioButtonNormal.Left + radioButtonNormal.Width + 40;
            radioButtonRegEx.Left = radioButtonCaseSensitive.Left + radioButtonCaseSensitive.Width + 40;
        }

        public void Initialize(Subtitle subtitle)
        {
            if (subtitle == null)
                throw new ArgumentNullException("subtitle");

            _subtitle = subtitle;
            foreach (var item in Configuration.Settings.MultipleSearchAndReplaceList)
            {
                AddToReplaceListView(item.Enabled, item.FindWhat, item.ReplaceWith, EnglishSearchTypeToLocal(item.SearchType));
                _oldMultipleSearchAndReplaceList.Add(item);
            }

            if (subtitle.Paragraphs == null || subtitle.Paragraphs.Count == 0)
                groupBoxLinesFound.Enabled = false;
        }

        private void MultipleReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                buttonCancel_Click(null, null);
            else if (e.KeyCode == Keys.F1)
                Utilities.ShowHelp("#multiple_replace");
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButtonRegEx)
                textBoxFind.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind);
            else
                textBoxFind.ContextMenu = null;
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
                textBoxFind.Text = string.Empty;
                textBoxReplace.Text = string.Empty;
                GeneratePreview();
                textBoxFind.Select();
                SaveReplaceList(false);
            }
        }

        internal void RunFromBatch(Subtitle subtitle)
        {
            Initialize(subtitle);
            GeneratePreview();
        }

        private void GeneratePreview()
        {
            Cursor = Cursors.WaitCursor;
            FixedSubtitle = new Subtitle(_subtitle);
            DeleteIndices = new List<int>();
            FixCount = 0;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            var replaceExpressions = new HashSet<ReplaceExpression>();
            foreach (ListViewItem item in listViewReplaceList.Items)
            {
                if (item.Checked)
                {
                    string findWhat = item.SubItems[1].Text;
                    if (!string.IsNullOrWhiteSpace(findWhat))
                    {
                        string replaceWith = item.SubItems[2].Text.Replace(@"\n", Environment.NewLine);
                        string searchType = item.SubItems[3].Text;
                        var mpi = new ReplaceExpression(findWhat, replaceWith, searchType);
                        replaceExpressions.Add(mpi);
                        if (mpi.SearchType == ReplaceExpression.SearchRegEx && !_compiledRegExList.ContainsKey(findWhat))
                        {
                            _compiledRegExList.Add(findWhat, new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline));
                        }
                    }
                }
            }

            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                bool hit = false;
                string newText = p.Text;
                foreach (ReplaceExpression item in replaceExpressions)
                {
                    if (item.SearchType == ReplaceExpression.SearchCaseSensitive)
                    {
                        if (newText.Contains(item.FindWhat))
                        {
                            hit = true;
                            newText = newText.Replace(item.FindWhat, item.ReplaceWith);
                        }
                    }
                    else if (item.SearchType == ReplaceExpression.SearchRegEx)
                    {
                        Regex r = _compiledRegExList[item.FindWhat];
                        if (r.IsMatch(newText))
                        {
                            hit = true;
                            newText = r.Replace(newText, item.ReplaceWith);
                        }
                    }
                    else
                    {
                        int index = newText.IndexOf(item.FindWhat, StringComparison.OrdinalIgnoreCase);
                        if (index >= 0)
                        {
                            hit = true;
                            do
                            {
                                newText = newText.Remove(index, item.FindWhat.Length).Insert(index, item.ReplaceWith);
                                index = newText.IndexOf(item.FindWhat, index + item.ReplaceWith.Length, StringComparison.OrdinalIgnoreCase);
                            }
                            while (index >= 0);
                        }
                    }
                }
                if (hit && newText != p.Text)
                {
                    FixCount++;
                    AddToPreviewListView(p, newText);
                    int index = _subtitle.GetIndex(p);
                    FixedSubtitle.Paragraphs[index].Text = newText;
                    if (!string.IsNullOrWhiteSpace(p.Text) && (string.IsNullOrWhiteSpace(newText) || string.IsNullOrWhiteSpace(HtmlUtil.RemoveHtmlTags(newText, true))))
                    {
                        DeleteIndices.Add(index);
                    }
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.MultipleReplace.LinesFoundX, FixCount);
            Cursor = Cursors.Default;
            DeleteIndices.Reverse();
        }

        private void AddToReplaceListView(bool enabled, string findWhat, string replaceWith, string searchType)
        {
            var item = new ListViewItem("") { Checked = enabled };

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
            var item = new ListViewItem("") { Tag = p, Checked = true };

            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString(CultureInfo.InvariantCulture));
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

        private void buttonOK_Click(object sender, EventArgs e)
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
            SaveReplaceList(false);
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (listViewReplaceList.Items.Count < 1 || listViewReplaceList.SelectedItems.Count < 1)
                return;
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

                if (e.KeyData == (Keys.Control | Keys.Home))
                    moveTopToolStripMenuItem_Click(sender, e);
                else if (e.KeyData == (Keys.Control | Keys.End))
                    moveBottomToolStripMenuItem_Click(sender, e);
            }
        }

        private void ButtonUpdateClick(object sender, EventArgs e)
        {
            if (listViewReplaceList.SelectedItems.Count != 1)
                return;

            if (textBoxFind.Text.Length > 0)
            {
                string searchType = SearchTypeNormal;
                if (radioButtonCaseSensitive.Checked)
                {
                    searchType = SearchTypeCaseSensitive;
                }
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
            if (e.KeyCode == Keys.Enter)
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
            moveTopToolStripMenuItem.Visible = listViewReplaceList.Items.Count > 1 && listViewReplaceList.SelectedItems.Count == 1;
            moveBottomToolStripMenuItem.Visible = listViewReplaceList.Items.Count > 1 && listViewReplaceList.SelectedItems.Count == 1;
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
            GeneratePreview();
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listViewReplaceList.SelectedIndices[0];
            if (index == listViewReplaceList.Items.Count - 1)
                return;

            SwapReplaceList(index, index + 1);
        }

        private void moveTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listViewReplaceList.SelectedIndices[0];
            if (index == 0)
                return;

            var item = listViewReplaceList.Items[index];
            listViewReplaceList.Items.RemoveAt(index);
            listViewReplaceList.Items.Insert(0, item);
            GeneratePreview();
            SaveReplaceList(false);
        }

        private void moveBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listViewReplaceList.SelectedIndices[0];
            int bottomIndex = listViewReplaceList.Items.Count - 1;
            if (index == bottomIndex)
                return;

            var item = listViewReplaceList.Items[index];
            listViewReplaceList.Items.RemoveAt(index);
            listViewReplaceList.Items.Add(item);
            GeneratePreview();
            SaveReplaceList(false);
        }

        private void ExportClick(object sender, EventArgs e)
        {
            if (listViewReplaceList.Items.Count == 0)
                return;

            saveFileDialog1.Title = Configuration.Settings.Language.MultipleReplace.ExportRulesTitle;
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
                    var item = new MultipleSearchAndReplaceSetting();
                    var subNode = listNode.SelectSingleNode("Enabled");
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

                listViewReplaceList.ItemChecked -= ListViewReplaceListItemChecked;
                listViewReplaceList.BeginUpdate();
                listViewReplaceList.Items.Clear();
                foreach (var item in Configuration.Settings.MultipleSearchAndReplaceList)
                    AddToReplaceListView(item.Enabled, item.FindWhat, item.ReplaceWith, EnglishSearchTypeToLocal(item.SearchType));
                GeneratePreview();
                listViewReplaceList.ItemChecked += ListViewReplaceListItemChecked;
                listViewReplaceList.EndUpdate();
            }
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            listViewReplaceList.Items.Clear();
            Configuration.Settings.MultipleSearchAndReplaceList.Clear();
        }

        private void MultipleReplace_Shown(object sender, EventArgs e)
        {
            listViewReplaceList.ItemChecked += ListViewReplaceListItemChecked;
            GeneratePreview();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Configuration.Settings.MultipleSearchAndReplaceList.Clear();
            foreach (var item in _oldMultipleSearchAndReplaceList)
            {
                Configuration.Settings.MultipleSearchAndReplaceList.Add(item);
            }
            DialogResult = DialogResult.Cancel;
        }

    }
}
