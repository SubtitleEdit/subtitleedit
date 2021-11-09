using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MultipleReplace : PositionAndSizeForm
    {
        internal const string Group = "Group";
        internal const string GroupName = "Name";
        internal const string GroupEnabled = "Enabled";
        internal const string MultipleSearchAndReplaceItem = "MultipleSearchAndReplaceItem";
        internal const string RuleEnabled = "Enabled";
        internal const string FindWhat = "FindWhat";
        internal const string ReplaceWith = "ReplaceWith";
        internal const string SearchType = "SearchType";
        internal const string Description = "Description";

        private const string TemplateFilterExtension = "|*.template";

        private readonly List<MultipleSearchAndReplaceGroup> _oldMultipleSearchAndReplaceGroups = new List<MultipleSearchAndReplaceGroup>();
        private readonly Dictionary<string, Regex> _compiledRegExList = new Dictionary<string, Regex>();
        private Subtitle _subtitle;
        private Subtitle _original;
        public Subtitle FixedSubtitle { get; private set; }
        public int FixCount { get; private set; }
        public List<int> DeleteIndices { get; }

        public void SetDeleteIndices()
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Checked && item.SubItems[3].Text == string.Empty)
                {
                    var index = _subtitle.GetIndex(item.Tag as Paragraph);
                    if (!DeleteIndices.Contains(index))
                    {
                        DeleteIndices.Add(index);
                    }
                }
            }
        }

        private MultipleSearchAndReplaceGroup _currentGroup;

        public MultipleReplace()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            DeleteIndices = new List<int>();
            openFileDialog1.FileName = string.Empty;
            saveFileDialog1.FileName = string.Empty;

            textBoxReplace.ContextMenuStrip = FindReplaceDialogHelper.GetReplaceTextContextMenu(textBoxReplace);
            buttonUpdate.Enabled = false;

            Text = LanguageSettings.Current.MultipleReplace.Title;
            labelFindWhat.Text = LanguageSettings.Current.MultipleReplace.FindWhat;
            labelReplaceWith.Text = LanguageSettings.Current.MultipleReplace.ReplaceWith;
            labelDescription.Text = LanguageSettings.Current.MultipleReplace.Description;
            radioButtonNormal.Text = LanguageSettings.Current.MultipleReplace.Normal;
            radioButtonRegEx.Text = LanguageSettings.Current.MultipleReplace.RegularExpression;
            radioButtonCaseSensitive.Text = LanguageSettings.Current.MultipleReplace.CaseSensitive;
            buttonAdd.Text = LanguageSettings.Current.MultipleReplace.Add;
            buttonUpdate.Text = LanguageSettings.Current.MultipleReplace.Update;
            listViewRules.Columns[0].Text = LanguageSettings.Current.MultipleReplace.Enabled;
            listViewRules.Columns[1].Text = LanguageSettings.Current.MultipleReplace.FindWhat;
            listViewRules.Columns[2].Text = LanguageSettings.Current.MultipleReplace.ReplaceWith;
            listViewRules.Columns[3].Text = LanguageSettings.Current.MultipleReplace.SearchType;
            listViewRules.Columns[4].Text = LanguageSettings.Current.MultipleReplace.Description;
            groupBoxGroups.Text = LanguageSettings.Current.MultipleReplace.Groups;
            groupBoxLinesFound.Text = string.Empty;
            listViewFixes.Columns[0].Text = LanguageSettings.Current.General.Apply;
            listViewFixes.Columns[1].Text = LanguageSettings.Current.General.LineNumber;
            listViewFixes.Columns[2].Text = LanguageSettings.Current.General.Before;
            listViewFixes.Columns[3].Text = LanguageSettings.Current.General.After;
            deleteToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Remove;
            toolStripMenuItemRemoveAll.Text = LanguageSettings.Current.MultipleReplace.RemoveAll;
            toolStripMenuItemImport.Text = LanguageSettings.Current.MultipleReplace.Import;
            toolStripMenuItemExport.Text = LanguageSettings.Current.MultipleReplace.Export;
            importToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Import;
            exportToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Export;
            buttonImportGroups.Text = LanguageSettings.Current.MultipleReplace.Import;
            buttonExportGroups.Text = LanguageSettings.Current.MultipleReplace.Export;
            buttonNewGroup.Text = LanguageSettings.Current.MultipleReplace.NewGroup;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonApply.Text = LanguageSettings.Current.General.Apply;
            buttonReplacesSelectAll.Text = LanguageSettings.Current.FixCommonErrors.SelectAll;
            buttonReplacesInverseSelection.Text = LanguageSettings.Current.FixCommonErrors.InverseSelection;
            UiUtil.FixLargeFonts(this, buttonOK);
            splitContainer1.Panel1MinSize = 200;
            splitContainer1.Panel2MinSize = 200;

            moveUpToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            moveBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            newToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.NewGroup;

            toolStripMenuItemMoveRulesToGroup.Text = LanguageSettings.Current.MultipleReplace.MoveSelectedRulesToGroup;
            moveUpToolStripMenuItem1.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem1.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveToTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            moveToBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            toolStripMenuItemRename.Text = LanguageSettings.Current.MultipleReplace.RenameGroup;
            deleteToolStripMenuItem1.Text = LanguageSettings.Current.MultipleReplace.Remove;
            selectAllToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            inverseSelectionToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;

            radioButtonCaseSensitive.Left = radioButtonNormal.Left + radioButtonNormal.Width + 40;
            radioButtonRegEx.Left = radioButtonCaseSensitive.Left + radioButtonCaseSensitive.Width + 40;
        }

        public void Initialize(Subtitle subtitle)
        {
            _subtitle = subtitle ?? throw new ArgumentNullException(nameof(subtitle));
            _original = new Subtitle(_subtitle);
            _oldMultipleSearchAndReplaceGroups.Clear();

            if (Configuration.Settings.MultipleSearchAndReplaceGroups.Count == 0)
            {
                Configuration.Settings.MultipleSearchAndReplaceGroups.Add(new MultipleSearchAndReplaceGroup { Enabled = true, Name = "Default", Rules = new List<MultipleSearchAndReplaceSetting>() });
            }

            foreach (var group in Configuration.Settings.MultipleSearchAndReplaceGroups)
            {
                var oldGroup = new MultipleSearchAndReplaceGroup { Name = group.Name, Enabled = group.Enabled };
                oldGroup.Rules = group.Rules.ConvertAll(rule => new MultipleSearchAndReplaceSetting()
                {
                    Enabled = rule.Enabled,
                    FindWhat = rule.FindWhat,
                    ReplaceWith = rule.ReplaceWith,
                    SearchType = rule.SearchType,
                    Description = rule.Description
                });
                _oldMultipleSearchAndReplaceGroups.Add(oldGroup);
            }

            if (subtitle.Paragraphs == null || subtitle.Paragraphs.Count == 0)
            {
                groupBoxLinesFound.Enabled = false;
            }

            UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, Configuration.Settings.MultipleSearchAndReplaceGroups[0]);
        }

        internal void RunFromBatch(Subtitle subtitle)
        {
            Initialize(subtitle);
            GeneratePreview();
            SetDeleteIndices();
        }

        internal void RunFromBatch(Subtitle subtitle, IEnumerable<string> importFileNames)
        {
            var savedList = Configuration.Settings.MultipleSearchAndReplaceGroups;
            try
            {
                Configuration.Settings.MultipleSearchAndReplaceGroups = new List<MultipleSearchAndReplaceGroup>();
                foreach (var fileName in importFileNames)
                {
                    if (fileName.Equals("."))
                    {
                        Configuration.Settings.MultipleSearchAndReplaceGroups.AddRange(savedList);
                    }
                    else
                    {
                        Configuration.Settings.MultipleSearchAndReplaceGroups.AddRange(ImportGroupsFile(fileName));
                    }
                }
                RunFromBatch(subtitle);
            }
            finally
            {
                Configuration.Settings.MultipleSearchAndReplaceGroups = savedList;
            }
        }

        private void MultipleReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                buttonCancel_Click(null, null);
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#multiple_replace");
            }
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            textBoxFind.ContextMenuStrip = sender == radioButtonRegEx ? FindReplaceDialogHelper.GetRegExContextMenu(textBoxFind) : null;
        }

        private void ButtonAddClick(object sender, EventArgs e)
        {
            if (_currentGroup == null)
            {
                return;
            }

            string findText = textBoxFind.Text.RemoveControlCharacters();
            if (findText.Length > 0)
            {
                string searchType = ReplaceExpression.SearchTypeNormal;
                if (radioButtonCaseSensitive.Checked)
                {
                    searchType = ReplaceExpression.SearchTypeCaseSensitive;
                }
                else if (radioButtonRegEx.Checked)
                {
                    searchType = ReplaceExpression.SearchTypeRegularExpression;
                    if (!RegexUtils.IsValidRegex(findText))
                    {
                        MessageBox.Show(LanguageSettings.Current.General.RegularExpressionIsNotValid);
                        textBoxFind.Select();
                        return;
                    }
                }

                var rule = new MultipleSearchAndReplaceSetting { Enabled = true, FindWhat = findText, ReplaceWith = textBoxReplace.Text, SearchType = searchType, Description = textBoxDescription.Text };
                _currentGroup.Rules.Add(rule);
                AddToRulesListView(rule);
                textBoxFind.Text = string.Empty;
                textBoxReplace.Text = string.Empty;
                textBoxDescription.Text = string.Empty;
                GeneratePreview();
                textBoxFind.Select();
            }
        }

        private void GeneratePreview()
        {
            Cursor = Cursors.WaitCursor;
            FixedSubtitle = new Subtitle(_subtitle);
            int fixedLines = 0;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            var replaceExpressions = new HashSet<ReplaceExpression>();
            foreach (var group in Configuration.Settings.MultipleSearchAndReplaceGroups)
            {
                if (group.Enabled)
                {
                    foreach (var rule in group.Rules)
                    {
                        if (rule.Enabled)
                        {
                            string findWhat = rule.FindWhat;
                            if (!string.IsNullOrEmpty(findWhat)) // allow space or spaces
                            {
                                string replaceWith = RegexUtils.FixNewLine(rule.ReplaceWith);
                                findWhat = RegexUtils.FixNewLine(findWhat);
                                string searchType = rule.SearchType;
                                var mpi = new ReplaceExpression(findWhat, replaceWith, searchType);
                                replaceExpressions.Add(mpi);
                                if (mpi.SearchType == ReplaceExpression.SearchRegEx && !_compiledRegExList.ContainsKey(findWhat))
                                {
                                    _compiledRegExList.Add(findWhat, new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline));
                                }
                            }
                        }
                    }
                }
            }

            var fixes = new List<ListViewItem>();
            for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
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
                            newText = RegexUtils.ReplaceNewLineSafe(r, newText, item.ReplaceWith);
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
                            } while (index >= 0);
                        }
                    }
                }

                if (hit && newText != p.Text)
                {
                    fixedLines++;
                    fixes.Add(MakePreviewListItem(p, newText));
                    FixedSubtitle.Paragraphs[i].Text = newText;
                }
            }

            listViewFixes.Items.AddRange(fixes.ToArray());
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MultipleReplace.LinesFoundX, fixedLines);
            Cursor = Cursors.Default;
        }

        private void AddToRulesListView(MultipleSearchAndReplaceSetting rule)
        {
            var item = new ListViewItem(string.Empty) { Checked = rule.Enabled, Tag = rule };
            item.SubItems.Add(rule.FindWhat);
            item.SubItems.Add(rule.ReplaceWith);
            item.SubItems.Add(EnglishSearchTypeToLocal(rule.SearchType));
            item.SubItems.Add(rule.Description);
            listViewRules.Items.Add(item);
        }

        private ListViewItem MakePreviewListItem(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(p.Text));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(newText));
            return item;
        }

        private static string LocalSearchTypeToEnglish(string searchType)
        {
            if (searchType == LanguageSettings.Current.MultipleReplace.RegularExpression)
            {
                return ReplaceExpression.SearchTypeRegularExpression;
            }

            if (searchType == LanguageSettings.Current.MultipleReplace.CaseSensitive)
            {
                return ReplaceExpression.SearchTypeCaseSensitive;
            }

            return ReplaceExpression.SearchTypeNormal;
        }

        private static string EnglishSearchTypeToLocal(string searchType)
        {
            if (searchType == ReplaceExpression.SearchTypeRegularExpression)
            {
                return LanguageSettings.Current.MultipleReplace.RegularExpression;
            }

            if (searchType == ReplaceExpression.SearchTypeCaseSensitive)
            {
                return LanguageSettings.Current.MultipleReplace.CaseSensitive;
            }

            return LanguageSettings.Current.MultipleReplace.Normal;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SetDeleteIndices();
            ResetUncheckLines();
            SetFixCount();
            SaveReplaceList(true);
            DialogResult = DialogResult.OK;
        }

        public void SetFixCount()
        {
            for (var index = 0; index < _original.Paragraphs.Count; index++)
            {
                var p = _original.Paragraphs[index];
                var f = FixedSubtitle.GetParagraphOrDefault(index);
                if (f != null && f.Text != p.Text)
                {
                    FixCount++;
                }
            }
        }

        private void SaveReplaceList(bool saveToDisk)
        {
            if (saveToDisk)
            {
                Configuration.Settings.Save();
            }
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

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (listViewRules.Items.Count < 1 || listViewRules.SelectedItems.Count < 1 || _currentGroup == null)
            {
                return;
            }

            listViewRules.BeginUpdate();
            for (int i = listViewRules.Items.Count - 1; i >= 0; i--)
            {
                ListViewItem item = listViewRules.Items[i];
                if (item.Selected)
                {
                    item.Remove();
                    _currentGroup.Rules.Remove(item.Tag as MultipleSearchAndReplaceSetting);
                }
            }
            listViewRules.EndUpdate();
            GeneratePreview();
        }

        private void ListViewRulesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewRules.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewRules.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift)) //InverseSelection
            {
                listViewRules.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        private void ButtonUpdateClick(object sender, EventArgs e)
        {
            if (listViewRules.SelectedItems.Count != 1 || _currentGroup == null)
            {
                return;
            }

            string findText = textBoxFind.Text.RemoveControlCharacters();
            textBoxFind.Text = findText;

            string replaceText = textBoxReplace.Text.RemoveControlCharacters();
            textBoxReplace.Text = replaceText;

            if (findText.Length > 0)
            {
                string searchType = ReplaceExpression.SearchTypeNormal;
                if (radioButtonCaseSensitive.Checked)
                {
                    searchType = ReplaceExpression.SearchTypeCaseSensitive;
                }
                else if (radioButtonRegEx.Checked)
                {
                    searchType = ReplaceExpression.SearchTypeRegularExpression;
                    if (!RegexUtils.IsValidRegex(findText))
                    {
                        MessageBox.Show(LanguageSettings.Current.General.RegularExpressionIsNotValid);
                        textBoxFind.Select();
                        return;
                    }
                }

                var item = listViewRules.SelectedItems[0];
                item.SubItems[1].Text = findText;
                item.SubItems[2].Text = replaceText;
                item.SubItems[3].Text = EnglishSearchTypeToLocal(searchType);
                item.SubItems[4].Text = textBoxDescription.Text;

                _currentGroup.Rules[item.Index].FindWhat = findText;
                _currentGroup.Rules[item.Index].ReplaceWith = replaceText;
                _currentGroup.Rules[item.Index].SearchType = searchType;
                _currentGroup.Rules[item.Index].Description = textBoxDescription.Text;

                GeneratePreview();
                textBoxFind.Select();
            }
        }

        private void ListViewReplaceListSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewRules.SelectedItems.Count == 1)
            {
                buttonUpdate.Enabled = true;
                textBoxFind.Text = listViewRules.SelectedItems[0].SubItems[1].Text;
                textBoxReplace.Text = listViewRules.SelectedItems[0].SubItems[2].Text;
                string searchType = LocalSearchTypeToEnglish(listViewRules.SelectedItems[0].SubItems[3].Text);
                if (searchType == ReplaceExpression.SearchTypeRegularExpression)
                {
                    radioButtonRegEx.Checked = true;
                }
                else if (searchType == ReplaceExpression.SearchTypeCaseSensitive)
                {
                    radioButtonCaseSensitive.Checked = true;
                }
                else
                {
                    radioButtonNormal.Checked = true;
                }

                textBoxDescription.Text = listViewRules.SelectedItems[0].SubItems[4].Text;
            }
            else
            {
                buttonUpdate.Enabled = false;
            }
        }

        private void TextBoxReplaceKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonAddClick(null, null);
            }
        }

        private void buttonReplacesSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = true;
            }
        }

        private void buttonReplacesInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = !item.Checked;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool moreThanOneGroup = Configuration.Settings.MultipleSearchAndReplaceGroups.Count > 1;
            bool isVisible = listViewRules.Items.Count > 1 && listViewRules.SelectedItems.Count == 1;
            toolStripSeparator3.Visible = isVisible;
            moveUpToolStripMenuItem.Visible = isVisible;
            moveDownToolStripMenuItem.Visible = isVisible;
            moveTopToolStripMenuItem.Visible = isVisible;
            moveBottomToolStripMenuItem.Visible = isVisible;

            toolStripMenuItemExport.Visible = listViewRules.Items.Count > 0;
            toolStripMenuItemRemoveAll.Visible = listViewRules.Items.Count > 0;
            toolStripSeparator2.Visible = listViewRules.Items.Count > 0;

            deleteToolStripMenuItem.Visible = listViewRules.SelectedItems.Count >= 1;

            if (moreThanOneGroup && listViewRules.SelectedItems.Count >= 1)
            {
                toolStripMenuItemMoveRulesToGroup.Visible = true;
                toolStripSeparator4.Visible = true;
                toolStripMenuItemMoveRulesToGroup.DropDownItems.Clear();
                foreach (var g in Configuration.Settings.MultipleSearchAndReplaceGroups)
                {
                    if (g != _currentGroup)
                    {
                        var menuItem = new ToolStripMenuItem(g.Name) { Tag = g };
                        menuItem.Click += (o, args) => { MoveRulesToGroup(g); };
                        toolStripMenuItemMoveRulesToGroup.DropDownItems.Add(menuItem);
                    }
                }

                UiUtil.FixFonts(toolStripMenuItemMoveRulesToGroup);
            }
            else
            {
                toolStripMenuItemMoveRulesToGroup.Visible = false;
                toolStripSeparator4.Visible = false;
            }
        }

        private void MoveRulesToGroup(MultipleSearchAndReplaceGroup newGroup)
        {
            foreach (ListViewItem item in listViewRules.SelectedItems)
            {
                var rule = (MultipleSearchAndReplaceSetting)item.Tag;
                _currentGroup.Rules.Remove(rule);
                newGroup.Rules.Add(rule);
            }
            UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, _currentGroup);
            GeneratePreview();
        }

        private void SwapRules(int index, int index2)
        {
            if (_currentGroup == null)
            {
                return;
            }

            listViewRules.ItemChecked -= ListViewRulesItemChecked;

            var temp = _currentGroup.Rules[index];
            _currentGroup.Rules[index] = _currentGroup.Rules[index2];
            _currentGroup.Rules[index2] = temp;

            bool enabled = listViewRules.Items[index].Checked;
            string findWhat = listViewRules.Items[index].SubItems[1].Text;
            string replaceWith = listViewRules.Items[index].SubItems[2].Text;
            string searchType = listViewRules.Items[index].SubItems[3].Text;
            string description = listViewRules.Items[index].SubItems[4].Text;

            listViewRules.Items[index].Checked = listViewRules.Items[index2].Checked;
            listViewRules.Items[index].SubItems[1].Text = listViewRules.Items[index2].SubItems[1].Text;
            listViewRules.Items[index].SubItems[2].Text = listViewRules.Items[index2].SubItems[2].Text;
            listViewRules.Items[index].SubItems[3].Text = listViewRules.Items[index2].SubItems[3].Text;
            listViewRules.Items[index].SubItems[4].Text = listViewRules.Items[index2].SubItems[4].Text;
            listViewRules.Items[index].Tag = _currentGroup.Rules[index];

            listViewRules.Items[index2].Checked = enabled;
            listViewRules.Items[index2].SubItems[1].Text = findWhat;
            listViewRules.Items[index2].SubItems[2].Text = replaceWith;
            listViewRules.Items[index2].SubItems[3].Text = searchType;
            listViewRules.Items[index2].SubItems[4].Text = description;
            listViewRules.Items[index2].Tag = _currentGroup.Rules[index2];

            listViewRules.Items[index].Selected = false;
            listViewRules.Items[index2].Selected = true;
            listViewRules.Items[index2].Focused = true;
            listViewRules.EnsureVisible(index2);
            GeneratePreview();
            listViewRules.ItemChecked += ListViewRulesItemChecked;
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewRules.SelectedItems.Count != 1 || listViewRules.Items.Count < 2)
            {
                return;
            }

            int index = listViewRules.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            SwapRules(index, index - 1);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewRules.SelectedItems.Count != 1 || listViewRules.Items.Count < 2)
            {
                return;
            }

            int index = listViewRules.SelectedIndices[0];
            if (index == listViewRules.Items.Count - 1)
            {
                return;
            }

            SwapRules(index, index + 1);
        }

        private void moveTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewRules.SelectedItems.Count != 1 || listViewRules.Items.Count < 2)
            {
                return;
            }

            int index = listViewRules.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            if (_currentGroup == null)
            {
                return;
            }

            var temp = _currentGroup.Rules[index];
            _currentGroup.Rules.Remove(temp);
            _currentGroup.Rules.Insert(0, temp);
            listViewGroups_SelectedIndexChanged(null, null);
            GeneratePreview();
        }

        private void moveBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewRules.SelectedItems.Count != 1 || listViewRules.Items.Count < 2)
            {
                return;
            }

            int index = listViewRules.SelectedIndices[0];
            int bottomIndex = listViewRules.Items.Count - 1;
            if (index == bottomIndex)
            {
                return;
            }

            var temp = _currentGroup.Rules[index];
            _currentGroup.Rules.Remove(temp);
            _currentGroup.Rules.Add(temp);
            listViewGroups_SelectedIndexChanged(null, null);
            listViewRules.Items[0].Selected = false;
            listViewRules.Items[bottomIndex].Selected = true;
            listViewRules.Items[bottomIndex].Focused = true;
            listViewRules.EnsureVisible(bottomIndex);
            GeneratePreview();
        }

        private void ImportRulesFile(string fileName)
        {
            if (_currentGroup == null)
            {
                return;
            }

            var doc = new XmlDocument { XmlResolver = null };
            doc.Load(fileName);

            var replaceNodes = doc.DocumentElement?.SelectNodes("//MultipleSearchAndReplaceItem");
            if (replaceNodes == null)
            {
                return;
            }

            foreach (XmlNode listNode in replaceNodes)
            {
                var item = MakeMultipleSearchAndReplaceSetting(listNode);
                AddToRulesListView(item);
                _currentGroup.Rules.Add(item);
            }
        }

        private List<MultipleSearchAndReplaceGroup> ImportGroupsFile(string fileName)
        {
            var list = new List<MultipleSearchAndReplaceGroup>();
            var doc = new XmlDocument { XmlResolver = null };
            doc.Load(fileName);
            var groups = doc.DocumentElement?.SelectNodes("//Group");
            if (groups != null)
            {
                foreach (XmlNode groupNode in groups)
                {
                    var group = new MultipleSearchAndReplaceGroup();
                    var nameNode = groupNode.SelectSingleNode(GroupName);
                    var enabledNode = groupNode.SelectSingleNode(GroupEnabled);

                    group.Name = nameNode != null ? nameNode.InnerText : "Untitled";
                    group.Enabled = enabledNode != null ? Convert.ToBoolean(enabledNode.InnerText) : false;

                    group.Rules = new List<MultipleSearchAndReplaceSetting>();
                    list.Add(group);

                    var replaceItems = groupNode.SelectNodes("MultipleSearchAndReplaceItem");
                    if (replaceItems != null)
                    {
                        foreach (XmlNode listNode in replaceItems)
                        {
                            var item = MakeMultipleSearchAndReplaceSetting(listNode);
                            group.Rules.Add(item);
                        }
                    }
                }
            }

            if (list.Count == 0)
            {
                // import into "untitled" group if only rules
                var group = new MultipleSearchAndReplaceGroup
                {
                    Name = "untitled",
                    Rules = new List<MultipleSearchAndReplaceSetting>()
                };
                var replaceItems = doc.DocumentElement?.SelectNodes("//MultipleSearchAndReplaceItem");
                if (replaceItems != null)
                {
                    foreach (XmlNode listNode in replaceItems)
                    {
                        var item = MakeMultipleSearchAndReplaceSetting(listNode);
                        group.Rules.Add(item);
                    }
                }

                if (group.Rules.Count > 0)
                {
                    list.Add(group);
                }
            }

            return list;
        }

        private static MultipleSearchAndReplaceSetting MakeMultipleSearchAndReplaceSetting(XmlNode listNode)
        {
            var item = new MultipleSearchAndReplaceSetting();
            var subNode = listNode.SelectSingleNode(RuleEnabled);
            if (subNode != null)
            {
                item.Enabled = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = listNode.SelectSingleNode(FindWhat);
            if (subNode != null)
            {
                item.FindWhat = subNode.InnerText;
            }

            subNode = listNode.SelectSingleNode(ReplaceWith);
            if (subNode != null)
            {
                item.ReplaceWith = subNode.InnerText;
            }

            subNode = listNode.SelectSingleNode(SearchType);
            if (subNode != null)
            {
                item.SearchType = subNode.InnerText;
            }

            subNode = listNode.SelectSingleNode(SearchType);
            if (subNode != null)
            {
                item.SearchType = subNode.InnerText;
            }

            subNode = listNode.SelectSingleNode(Description);
            if (subNode != null)
            {
                item.Description = subNode.InnerText;
            }

            return item;
        }

        private void MultipleReplace_Shown(object sender, EventArgs e)
        {
            GeneratePreview();
            listViewRules.ItemChecked += ListViewRulesItemChecked;
            listViewGroups.ItemChecked += listViewGroups_ItemChecked;
            listViewGroups.SelectedIndexChanged += listViewGroups_SelectedIndexChanged;
            MultipleReplace_ResizeEnd(sender, null);
            listViewGroups_SelectedIndexChanged(null, null);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Configuration.Settings.MultipleSearchAndReplaceGroups = _oldMultipleSearchAndReplaceGroups;
            DialogResult = DialogResult.Cancel;
        }

        private void buttonNewGroup_Click(object sender, EventArgs e)
        {
            newToolStripMenuItem_Click(sender, null);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new MultipleReplaceNewGroup(string.Empty))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.GroupName.Length > 0)
                {
                    var newGroup = new MultipleSearchAndReplaceGroup { Enabled = true, Name = form.GroupName, Rules = new List<MultipleSearchAndReplaceSetting>() };
                    Configuration.Settings.MultipleSearchAndReplaceGroups.Add(newGroup);
                    UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, newGroup);
                }
            }
        }

        private void UpdateViewFromModel(List<MultipleSearchAndReplaceGroup> groups, MultipleSearchAndReplaceGroup focusGroup)
        {
            listViewGroups.BeginUpdate();
            listViewGroups.Items.Clear();
            for (int index = 0; index < groups.Count; index++)
            {
                var group = groups[index];
                var lvi = new ListViewItem(group.Name) { Checked = group.Enabled, Tag = group };
                listViewGroups.Items.Add(lvi);
                if (group == focusGroup)
                {
                    listViewGroups.Items[index].Selected = true;
                    listViewGroups.Items[index].Focused = true;
                    listViewGroups.EnsureVisible(index);
                }
            }
            listViewGroups.EndUpdate();
            if (groups.Count == 0)
            {
                groupBoxReplaces.Text = string.Empty;
                textBoxFind.Text = string.Empty;
                textBoxReplace.Text = string.Empty;
                textBoxDescription.Text = string.Empty;
                textBoxDescription.Text = string.Empty;
                radioButtonNormal.Checked = true;
            }
        }

        private void listViewGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewGroups.SelectedItems.Count < 1)
            {
                return;
            }

            _currentGroup = listViewGroups.SelectedItems[0].Tag as MultipleSearchAndReplaceGroup;
            if (_currentGroup == null)
            {
                return;
            }

            listViewRules.ItemChecked -= ListViewRulesItemChecked;
            listViewGroups.ItemChecked -= listViewGroups_ItemChecked;
            listViewRules.BeginUpdate();
            groupBoxReplaces.Text = string.Format(LanguageSettings.Current.MultipleReplace.RulesForGroupX, _currentGroup.Name);
            listViewRules.Items.Clear();
            foreach (var rule in _currentGroup.Rules)
            {
                AddToRulesListView(rule);
            }
            if (_currentGroup.Rules.Count > 0)
            {
                listViewRules.Items[0].Selected = true;
            }
            else
            {
                textBoxFind.Text = string.Empty;
                textBoxReplace.Text = string.Empty;
                textBoxDescription.Text = string.Empty;
            }
            listViewRules.EndUpdate();
            listViewRules.ItemChecked += ListViewRulesItemChecked;
            listViewGroups.ItemChecked += listViewGroups_ItemChecked;
        }

        private void listViewGroups_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var group = e.Item.Tag as MultipleSearchAndReplaceGroup;
            if (group == null)
            {
                return;
            }

            group.Enabled = e.Item.Checked;
            GeneratePreview();
        }

        private void ListViewRulesItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!(e.Item.Tag is MultipleSearchAndReplaceSetting rule))
            {
                return;
            }

            rule.Enabled = e.Item.Checked;
            GeneratePreview();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (_currentGroup == null)
            {
                return;
            }

            _currentGroup.Rules.Clear();
            listViewRules.Items.Clear();
            GeneratePreview();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (_currentGroup == null || _currentGroup.Rules.Count == 0)
            {
                return;
            }

            saveFileDialog1.Title = LanguageSettings.Current.MultipleReplace.ExportRulesTitle;
            saveFileDialog1.Filter = LanguageSettings.Current.MultipleReplace.Rules + TemplateFilterExtension;
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var textWriter = new XmlTextWriter(saveFileDialog1.FileName, null) { Formatting = Formatting.Indented };
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("Settings", string.Empty);
                textWriter.WriteStartElement("MultipleSearchAndReplaceList", string.Empty);
                textWriter.WriteStartElement(Group, string.Empty);
                textWriter.WriteElementString(GroupName, _currentGroup.Name);
                foreach (var item in _currentGroup.Rules)
                {
                    textWriter.WriteStartElement(MultipleSearchAndReplaceItem, string.Empty);
                    textWriter.WriteElementString(RuleEnabled, item.Enabled.ToString());
                    textWriter.WriteElementString(FindWhat, item.FindWhat);
                    textWriter.WriteElementString(ReplaceWith, item.ReplaceWith);
                    textWriter.WriteElementString(SearchType, item.SearchType);
                    textWriter.WriteElementString(Description, item.Description);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
                textWriter.Close();
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (_currentGroup == null)
            {
                return;
            }

            openFileDialog1.Title = LanguageSettings.Current.MultipleReplace.ImportRulesTitle;
            openFileDialog1.Filter = LanguageSettings.Current.MultipleReplace.Rules + TemplateFilterExtension;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    ImportRulesFile(openFileDialog1.FileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
                GeneratePreview();
            }
        }

        private void ToolStripMenuItemRenameClick(object sender, EventArgs e)
        {
            if (_currentGroup == null)
            {
                return;
            }

            using (var form = new MultipleReplaceNewGroup(_currentGroup.Name))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.GroupName.Length > 0)
                {
                    _currentGroup.Name = form.GroupName;
                    UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, _currentGroup);
                }
            }
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_currentGroup == null)
            {
                return;
            }

            var idx = Configuration.Settings.MultipleSearchAndReplaceGroups.IndexOf(_currentGroup);
            Configuration.Settings.MultipleSearchAndReplaceGroups.RemoveAt(idx);
            if (Configuration.Settings.MultipleSearchAndReplaceGroups.Count > 0)
            {
                if (idx >= Configuration.Settings.MultipleSearchAndReplaceGroups.Count)
                {
                    idx--;
                }

                _currentGroup = Configuration.Settings.MultipleSearchAndReplaceGroups[idx];
            }
            else
            {
                _currentGroup = null;
                listViewRules.Items.Clear();
                listViewFixes.Items.Clear();
            }
            UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, _currentGroup);
        }

        private void SwapGroups(int index, int index2)
        {
            var temp = Configuration.Settings.MultipleSearchAndReplaceGroups[index];
            Configuration.Settings.MultipleSearchAndReplaceGroups[index] = Configuration.Settings.MultipleSearchAndReplaceGroups[index2];
            Configuration.Settings.MultipleSearchAndReplaceGroups[index2] = temp;

            UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, _currentGroup);
            GeneratePreview();
        }

        private void moveUpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listViewGroups.Items.Count < 2 || listViewGroups.SelectedItems.Count == 0)
            {
                return;
            }

            int index = listViewGroups.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            SwapGroups(index, index - 1);
        }

        private void moveDownToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listViewGroups.Items.Count < 2 || listViewGroups.SelectedItems.Count == 0)
            {
                return;
            }

            int index = listViewGroups.SelectedIndices[0];
            if (index == listViewGroups.Items.Count - 1)
            {
                return;
            }

            SwapGroups(index, index + 1);
        }

        private void ResizeListViewLastColumn()
        {
            listViewRules.AutoSizeLastColumn();
            listViewFixes.AutoSizeLastColumn();
        }

        private void MultipleReplace_ResizeEnd(object sender, EventArgs e)
        {
            ResizeListViewLastColumn();
        }

        private void moveToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewGroups.Items.Count < 2 || listViewGroups.SelectedItems.Count == 0)
            {
                return;
            }

            int index = listViewGroups.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            var temp = Configuration.Settings.MultipleSearchAndReplaceGroups[index];
            Configuration.Settings.MultipleSearchAndReplaceGroups.Remove(temp);
            Configuration.Settings.MultipleSearchAndReplaceGroups.Insert(0, temp);
            UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, _currentGroup);
            GeneratePreview();
        }

        private void moveToBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewGroups.Items.Count < 2 || listViewGroups.SelectedItems.Count == 0)
            {
                return;
            }

            int index = listViewGroups.SelectedIndices[0];
            int bottomIndex = listViewGroups.Items.Count - 1;
            if (index == bottomIndex)
            {
                return;
            }

            var temp = Configuration.Settings.MultipleSearchAndReplaceGroups[index];
            Configuration.Settings.MultipleSearchAndReplaceGroups.Remove(temp);
            Configuration.Settings.MultipleSearchAndReplaceGroups.Insert(bottomIndex, temp);
            UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, temp);
            GeneratePreview();
        }

        private void contextMenuStripGroups_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool doShow = listViewGroups.SelectedItems.Count > 0;
            foreach (ToolStripItem item in contextMenuStripGroups.Items)
            {
                if (item != newToolStripMenuItem &&
                    item != toolStripSeparatorGroupImportExport &&
                    item != importToolStripMenuItem)
                {
                    item.Visible = doShow;
                }
                if (item == exportToolStripMenuItem)
                {
                    item.Visible = listViewGroups.Items.Count > 0;
                }
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            SetDeleteIndices();
            ResetUncheckLines();
            _subtitle = new Subtitle(FixedSubtitle);
            GeneratePreview();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewGroups.Items.Count == 0)
            {
                return;
            }

            using (var form = new MultipleReplaceExportImport(Configuration.Settings.MultipleSearchAndReplaceGroups, true))
            {
                form.ShowDialog(this);
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.MultipleReplace.ImportRulesTitle;
            openFileDialog1.Filter = LanguageSettings.Current.MultipleReplace.Rules + TemplateFilterExtension;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    var importGroups = ImportGroupsFile(openFileDialog1.FileName);
                    if (importGroups.Count == 0)
                    {
                        MessageBox.Show(LanguageSettings.Current.MultipleReplace.NothingToImport);
                        return;
                    }

                    using (var form = new MultipleReplaceExportImport(importGroups, false))
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            var oldGroupsSelectedIndex = listViewGroups.SelectedIndices.Count == 0 ? -1 : listViewGroups.SelectedIndices[0];
                            foreach (var importGroup in importGroups.Where(g => form.ChosenGroups.Contains(g.Name)))
                            {
                                importGroup.Name = FixDuplicateName(importGroup.Name, Configuration.Settings.MultipleSearchAndReplaceGroups);
                                Configuration.Settings.MultipleSearchAndReplaceGroups.Add(importGroup);
                            }
                            UpdateViewFromModel(Configuration.Settings.MultipleSearchAndReplaceGroups, _currentGroup);
                            if (oldGroupsSelectedIndex == -1 && listViewGroups.Items.Count > 0)
                            {
                                listViewGroups.Items[0].Selected = true;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
                GeneratePreview();
            }
        }

        private static string FixDuplicateName(string newGroupName, List<MultipleSearchAndReplaceGroup> existingGroups)
        {
            if (existingGroups.All(p => p.Name != newGroupName))
            {
                return newGroupName;
            }

            for (int i = 1; i < int.MaxValue; i++)
            {
                var name = $"{newGroupName}_{i}";
                if (existingGroups.All(p => p.Name != name))
                {
                    return name;
                }
            }

            return Guid.NewGuid().ToString();
        }

        private void buttonImportGroups_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(sender, e);
        }

        private void buttonExportGroups_Click(object sender, EventArgs e)
        {
            exportToolStripMenuItem_Click(sender, e);
        }

        private void listViewFixes_ClientSizeChanged(object sender, EventArgs e)
        {
            ResizeListViewLastColumn();
        }

        private void listViewFixes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewFixes.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewFixes.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift)) //InverseSelection
            {
                listViewFixes.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewRules.Items)
            {
                item.Checked = true;
            }
        }

        private void inverseSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewRules.Items)
            {
                item.Checked = !item.Checked;
            }
        }
    }
}
