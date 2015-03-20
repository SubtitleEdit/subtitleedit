using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MultipleReplace : PositionAndSizeForm
    {

        private class PreviewWorker : BackgroundWorker
        {
            public bool Restart { get; set; }
        }

        private class PreviewFix
        {
            public Paragraph Original { get; set; }
            public string NewText { get; set; }
        }

        private class FindAndReplaceRule
        {
            public const int SearchNormal = 0;
            public const int SearchCaseSensitive = 1;
            public const int SearchRegularExpression = 2;
            public int SearchType { get; private set; }
            public string FindWhat { get; private set; }
            public string ReplaceWith { get; private set; }

            public FindAndReplaceRule(string findWhat, string replaceWith, string searchType)
            {
                FindWhat = findWhat;
                ReplaceWith = replaceWith.Replace(@"\n", Environment.NewLine);
                if (searchType == Configuration.Settings.Language.MultipleReplace.CaseSensitive)
                    SearchType = SearchCaseSensitive;
                else if (searchType == Configuration.Settings.Language.MultipleReplace.RegularExpression)
                    SearchType = SearchRegularExpression;
            }
        }

        public const string SearchTypeNormal = "Normal";
        public const string SearchTypeCaseSensitive = "CaseSensitive";
        public const string SearchTypeRegularExpression = "RegularExpression";
        private readonly List<MultipleSearchAndReplaceSetting> _oldMultipleSearchAndReplaceList = new List<MultipleSearchAndReplaceSetting>();
        private readonly Dictionary<string, Regex> _compiledRegexList = new Dictionary<string, Regex>();
        private readonly PreviewWorker _previewer;
        private Subtitle _subtitle;
        public List<int> DeleteIndices { get; private set; }
        public Subtitle FixedSubtitle { get; private set; }
        public int FixCount { get; private set; }

        public MultipleReplace()
        {
            InitializeComponent();

            _previewer = new PreviewWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = true };
            _previewer.RunWorkerCompleted += PreviewRunWorkerCompleted;
            _previewer.ProgressChanged += PreviewProgressChanged;
            _previewer.DoWork += PreviewDoWork;
            components.Add(_previewer);

            openFileDialog1.FileName = string.Empty;
            saveFileDialog1.FileName = string.Empty;

            textBoxReplace.ContextMenu = FindReplaceDialogHelper.GetReplaceTextContextMenu(textBoxReplace);
            buttonUpdate.Enabled = false;

            Text = Configuration.Settings.Language.MultipleReplace.Title;
            labelFindWhat.Text = Configuration.Settings.Language.MultipleReplace.FindWhat;
            labelReplaceWith.Text = Configuration.Settings.Language.MultipleReplace.ReplaceWith;
            radioButtonNormal.Text = Configuration.Settings.Language.MultipleReplace.Normal;
            radioButtonRegex.Text = Configuration.Settings.Language.MultipleReplace.RegularExpression;
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
            FixLargeFonts();
            splitContainer1.Panel1MinSize = 200;
            splitContainer1.Panel2MinSize = 200;

            moveUpToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubRip.MoveDown;
            moveTopToolStripMenuItem.Text = Configuration.Settings.Language.MultipleReplace.MoveToTop;
            moveBottomToolStripMenuItem.Text = Configuration.Settings.Language.MultipleReplace.MoveToBottom;

            radioButtonCaseSensitive.Left = radioButtonNormal.Right + 40;
            radioButtonRegex.Left = radioButtonCaseSensitive.Right + 40;
        }

        private void FixLargeFonts()
        {
            var graphics = CreateGraphics();
            var textSize = graphics.MeasureString(buttonOK.Text, Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                var newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        internal void RunFromBatch(Subtitle subtitle)
        {
            Initialize(subtitle);
            GeneratePreview();
        }

        public void Initialize(Subtitle subtitle)
        {
            if (subtitle == null)
                throw new ArgumentNullException("subtitle");

            _subtitle = subtitle;
            _compiledRegexList.Clear();
            _oldMultipleSearchAndReplaceList.Clear();
            listViewReplaceList.Items.Clear();
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
                ButtonCancel_Click(null, null);
            if (e.KeyCode == Keys.F1)
                Utilities.ShowHelp("#multiple_replace");
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButtonRegex)
                textBoxFind.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind);
            else
                textBoxFind.ContextMenu = null;
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            if (textBoxFind.Text.Length > 0)
            {
                var searchType = SearchTypeNormal;
                if (radioButtonCaseSensitive.Checked)
                    searchType = SearchTypeCaseSensitive;
                else if (radioButtonRegex.Checked)
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
            }
        }

        private void GeneratePreview()
        {
            if (_previewer.IsBusy)
            {
                _previewer.Restart = true;
                _previewer.CancelAsync();
            }
            else
            {
                buttonOK.Enabled = false;
                _previewer.RunWorkerAsync();
                groupBoxLinesFound.Text = string.Empty;
            }
        }

        private void PreviewDoWork(object sender, DoWorkEventArgs e)
        {
            FixedSubtitle = new Subtitle(_subtitle);
            DeleteIndices = new List<int>();
            FixCount = 0;

            var rules = new List<FindAndReplaceRule>();
            var self = sender as PreviewWorker;
            using (var ruleQueue = new BlockingCollection<FindAndReplaceRule>(13))
            {
                self.ReportProgress(0, ruleQueue);
                for (;;)
                {
                    if (self.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    var rule = ruleQueue.Take();
                    if (rule == null)
                        break;
                    rules.Add(rule);
                    if (rule.SearchType == FindAndReplaceRule.SearchRegularExpression && !_compiledRegexList.ContainsKey(rule.FindWhat))
                    {
                        var regex = new Regex(rule.FindWhat, RegexOptions.Compiled | RegexOptions.Multiline);
                        _compiledRegexList.Add(rule.FindWhat, regex);
                    }
                }
            }
            if (rules.Count == 0)
                return;

            var fixQueue = new BlockingCollection<PreviewFix>(37);
            var hitCount = 0;
            try
            {
                foreach (var p in _subtitle.Paragraphs)
                {
                    var hit = false;
                    var newText = p.Text;

                    foreach (var rule in rules)
                    {
                        if (self.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        if (rule.SearchType == FindAndReplaceRule.SearchCaseSensitive)
                        {
                            if (newText.Contains(rule.FindWhat))
                            {
                                hit = true;
                                newText = newText.Replace(rule.FindWhat, rule.ReplaceWith);
                            }
                        }
                        else if (rule.SearchType == FindAndReplaceRule.SearchRegularExpression)
                        {
                            var regex = _compiledRegexList[rule.FindWhat];
                            if (regex.IsMatch(newText))
                            {
                                hit = true;
                                newText = regex.Replace(newText, rule.ReplaceWith);
                            }
                        }
                        else
                        {
                            var index = newText.IndexOf(rule.FindWhat, StringComparison.OrdinalIgnoreCase);
                            if (index >= 0)
                            {
                                hit = true;
                                do
                                {
                                    newText = newText.Remove(index, rule.FindWhat.Length).Insert(index, rule.ReplaceWith);
                                    index = newText.IndexOf(rule.FindWhat, index + rule.ReplaceWith.Length, StringComparison.OrdinalIgnoreCase);
                                }
                                while (index >= 0);
                            }
                        }
                    }
                    if (hit && newText != p.Text)
                    {
                        if (++hitCount > 18)
                        {
                            self.ReportProgress(hitCount, fixQueue);
                            FixCount += hitCount;
                            hitCount = 0;
                        }
                        fixQueue.Add(new PreviewFix { Original = p, NewText = newText });
                        var index = _subtitle.GetIndex(p);
                        FixedSubtitle.Paragraphs[index].Text = newText;
                        if (!string.IsNullOrWhiteSpace(p.Text) && (string.IsNullOrWhiteSpace(newText) || string.IsNullOrWhiteSpace(HtmlUtil.RemoveHtmlTags(newText, true))))
                        {
                            DeleteIndices.Add(index);
                        }
                    }
                }
                DeleteIndices.Reverse();
            }
            finally
            {
                self.ReportProgress(hitCount + 1, fixQueue);
                FixCount += hitCount;
                fixQueue.Add(null);
            }
        }

        private void PreviewProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                // e.ProgressPercentage: 0; e.UserState: replace-rule queue
                var ruleQueue = e.UserState as BlockingCollection<FindAndReplaceRule>;
                try
                {
                    for (var index = 0; index < listViewReplaceList.Items.Count; index++)
                    {
                        var item = listViewReplaceList.Items[index];
                        if (item.Checked)
                        {
                            var findWhat = item.SubItems[1].Text;
                            if (findWhat.Length > 0)
                            {
                                var rule = new FindAndReplaceRule(findWhat, item.SubItems[2].Text, item.SubItems[3].Text);
                                do
                                    Application.DoEvents();
                                while (!ruleQueue.TryAdd(rule));
                            }
                        }
                    }
                    ruleQueue.Add(null);
                }
                catch (ObjectDisposedException)
                {
                    return; // preview worker has been cancelled
                }
                listViewFixes.Items.Clear();
            }
            else
            {
                // e.ProgressPercentage: preview-fix count; e.UserState: preview-fix queue
                var fixQueue = e.UserState as BlockingCollection<PreviewFix>;
                var c = e.ProgressPercentage;
                listViewFixes.BeginUpdate();
                do
                {
                    var fix = fixQueue.Take();
                    if (fix == null)
                        fixQueue.Dispose();
                    else
                        AddToPreviewListView(fix.Original, fix.NewText);
                }
                while (--c > 0);
                listViewFixes.EndUpdate();
            }
        }

        private void PreviewRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var self = sender as PreviewWorker;

            if (e.Error != null)
            {
                var dr = MessageBox.Show(this, e.Error.Message, "Generate Preview Exception", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (dr == DialogResult.Cancel)
                    ButtonCancel_Click(null, null);
                self.Restart = true;
            }

            if (self.Restart)
            {
                self.Restart = false;
                self.RunWorkerAsync();
            }
            else
            {
                groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.MultipleReplace.LinesFoundX, FixCount);
                buttonOK.Enabled = true;
            }
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

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            ResetUncheckedLines();
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

        private void ResetUncheckedLines()
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (!item.Checked)
                {
                    var index = _subtitle.GetIndex(item.Tag as Paragraph);
                    FixedSubtitle.Paragraphs[index].Text = _subtitle.Paragraphs[index].Text;
                }
            }
        }

        private void ListViewReplaceList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            GeneratePreview();
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewReplaceList.Items.Count > 0 && listViewReplaceList.SelectedItems.Count > 0)
            {
                for (var index = listViewReplaceList.Items.Count - 1; index >= 0; index--)
                {
                    if (listViewReplaceList.Items[index].Selected)
                        listViewReplaceList.Items.RemoveAt(index);
                }
                GeneratePreview();
            }
        }

        private void ListViewReplaceList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteToolStripMenuItem_Click(null, null);
            if (listViewReplaceList.SelectedItems.Count == 1)
            {
                if (e.KeyCode == Keys.Up && e.Control && !e.Alt && !e.Shift)
                    MoveUpToolStripMenuItem_Click(sender, e);
                if (e.KeyCode == Keys.Down && e.Control && !e.Alt && !e.Shift)
                    MoveDownToolStripMenuItem_Click(sender, e);

                if (e.KeyData == (Keys.Control | Keys.Home))
                    MoveTopToolStripMenuItem_Click(sender, e);
                else if (e.KeyData == (Keys.Control | Keys.End))
                    MoveBottomToolStripMenuItem_Click(sender, e);
            }
        }

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            if (listViewReplaceList.SelectedItems.Count != 1)
                return;

            if (textBoxFind.Text.Length > 0)
            {
                var searchType = SearchTypeNormal;
                if (radioButtonCaseSensitive.Checked)
                {
                    searchType = SearchTypeCaseSensitive;
                }
                else if (radioButtonRegex.Checked)
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
            }
        }

        private void ListViewReplaceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewReplaceList.SelectedItems.Count == 1)
            {
                buttonUpdate.Enabled = true;
                textBoxFind.Text = listViewReplaceList.SelectedItems[0].SubItems[1].Text;
                textBoxReplace.Text = listViewReplaceList.SelectedItems[0].SubItems[2].Text;
                var searchType = LocalSearchTypeToEnglish(listViewReplaceList.SelectedItems[0].SubItems[3].Text);
                if (searchType == SearchTypeRegularExpression)
                    radioButtonRegex.Checked = true;
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

        private void TextBoxReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ButtonAdd_Click(null, null);
        }

        private void ButtonReplacesSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = true;
        }

        private void ButtonReplacesInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = !item.Checked;
        }

        private void ContextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            moveUpToolStripMenuItem.Visible = listViewReplaceList.Items.Count > 1 && listViewReplaceList.SelectedItems.Count == 1;
            moveDownToolStripMenuItem.Visible = listViewReplaceList.Items.Count > 1 && listViewReplaceList.SelectedItems.Count == 1;
            moveTopToolStripMenuItem.Visible = listViewReplaceList.Items.Count > 1 && listViewReplaceList.SelectedItems.Count == 1;
            moveBottomToolStripMenuItem.Visible = listViewReplaceList.Items.Count > 1 && listViewReplaceList.SelectedItems.Count == 1;
        }

        private void MoveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listViewReplaceList.SelectedIndices[0];
            if (index > 0)
            {
                SwapReplaceList(index, index - 1);
            }
        }

        private void SwapReplaceList(int index, int index2)
        {
            var enabled = listViewReplaceList.Items[index].Checked;
            var findWhat = listViewReplaceList.Items[index].SubItems[1].Text;
            var replaceWith = listViewReplaceList.Items[index].SubItems[2].Text;
            var searchType = listViewReplaceList.Items[index].SubItems[3].Text;

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
            GeneratePreview();
        }

        private void MoveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listViewReplaceList.SelectedIndices[0] + 1;
            if (index < listViewReplaceList.Items.Count)
            {
                SwapReplaceList(index - 1, index);
            }
        }

        private void MoveTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listViewReplaceList.SelectedIndices[0];
            if (index > 0)
            {
                var item = listViewReplaceList.Items[index];
                listViewReplaceList.Items.RemoveAt(index);
                listViewReplaceList.Items.Insert(0, item);
                GeneratePreview();
            }
        }

        private void MoveBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int bottomIndex = listViewReplaceList.Items.Count - 1;
            int index = listViewReplaceList.SelectedIndices[0];
            if (index < bottomIndex)
            {
                var item = listViewReplaceList.Items[index];
                listViewReplaceList.Items.RemoveAt(index);
                listViewReplaceList.Items.Add(item);
                GeneratePreview();
            }
        }

        private void ButtonExport_Click(object sender, EventArgs e)
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

        private void ButtonImport_Click(object sender, EventArgs e)
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

                SaveReplaceList(false);
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

                listViewReplaceList.ItemChecked -= ListViewReplaceList_ItemChecked;
                listViewReplaceList.BeginUpdate();
                listViewReplaceList.Items.Clear();
                foreach (var item in Configuration.Settings.MultipleSearchAndReplaceList)
                    AddToReplaceListView(item.Enabled, item.FindWhat, item.ReplaceWith, EnglishSearchTypeToLocal(item.SearchType));
                listViewReplaceList.ItemChecked += ListViewReplaceList_ItemChecked;
                listViewReplaceList.EndUpdate();
                GeneratePreview();
            }
        }

        private void ButtonRemoveAll_Click(object sender, EventArgs e)
        {
            listViewReplaceList.Items.Clear();
            GeneratePreview();
        }

        private void MultipleReplace_Shown(object sender, EventArgs e)
        {
            listViewReplaceList.ItemChecked += ListViewReplaceList_ItemChecked;
            GeneratePreview();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Configuration.Settings.MultipleSearchAndReplaceList.Clear();
            foreach (var item in _oldMultipleSearchAndReplaceList)
            {
                Configuration.Settings.MultipleSearchAndReplaceList.Add(item);
            }
            DialogResult = DialogResult.Cancel;
        }

        private void MultipleReplace_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (_previewer.IsBusy)
            {
                _previewer.Restart = false;
                _previewer.CancelAsync();
            }
            if(e.CloseReason == CloseReason.UserClosing)
            {
                SaveReplaceList(false);
            }
        }

    }
}
