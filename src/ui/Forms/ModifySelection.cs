using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ModifySelection : PositionAndSizeForm
    {
        private readonly SubtitleListView _subtitleListView;
        private readonly Subtitle _subtitle;
        private readonly SubtitleFormat _format;
        private bool _loading;

        private const int FunctionContains = 0;
        private const int FunctionStartsWith = 1;
        private const int FunctionEndsWith = 2;
        private const int FunctionNotContains = 3;
        private const int FunctionAllUppercase = 4;
        private const int FunctionRegEx = 5;
        private const int FunctionOdd = 6;
        private const int FunctionEven = 7;
        private const int FunctionDurationLessThan = 8;
        private const int FunctionDurationGreaterThan = 9;
        private const int FunctionMoreThanTwoLines = 10;
        private const int FunctionBookmarked = 11;
        private const int FunctionStyle = 12;
        private const int FunctionActor = 13;

        private const string ContainsString = "Contains";
        private const string StartsWith = "Starts with";
        private const string EndsWith = "Ends with";
        private const string NotContains = "Not contains";
        private const string AllUppercase = "AllUppercase";
        private const string RegEx = "RegEx";
        private const string Odd = "Odd";
        private const string Even = "Even";
        private const string DurationLessThan = "Duration <";
        private const string DurationGreaterThan = "Duration >";
        private const string MoreThanTwoLines = "More than two lines";
        private const string Bookmarked = "Bookmarked";
        private const string Style = "Style";
        private const string Actor = "Actor";

        public ModifySelection(Subtitle subtitle, SubtitleFormat format, SubtitleListView subtitleListView)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _loading = true;
            _subtitle = subtitle;
            _format = format;
            _subtitleListView = subtitleListView;
            labelInfo.Text = string.Empty;
            comboBoxRule.SelectedIndex = 0;
            Text = LanguageSettings.Current.ModifySelection.Title;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonApply.Text = LanguageSettings.Current.General.Apply;
            groupBoxRule.Text = LanguageSettings.Current.ModifySelection.Rule;
            groupBoxWhatToDo.Text = LanguageSettings.Current.ModifySelection.DoWithMatches;
            checkBoxCaseSensitive.Text = LanguageSettings.Current.ModifySelection.CaseSensitive;
            radioButtonNewSelection.Text = LanguageSettings.Current.ModifySelection.MakeNewSelection;
            radioButtonAddToSelection.Text = LanguageSettings.Current.ModifySelection.AddToCurrentSelection;
            radioButtonSubtractFromSelection.Text = LanguageSettings.Current.ModifySelection.SubtractFromCurrentSelection;
            radioButtonIntersect.Text = LanguageSettings.Current.ModifySelection.IntersectWithCurrentSelection;
            columnHeaderApply.Text = LanguageSettings.Current.General.Apply;
            columnHeaderLine.Text = LanguageSettings.Current.General.LineNumber;
            columnHeaderText.Text = LanguageSettings.Current.General.Text;
            toolStripMenuItemInverseSelection.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;

            listViewStyles.Visible = false;

            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxRule.Items.Clear();
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.Contains);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.StartsWith);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.EndsWith);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.NoContains);
            comboBoxRule.Items.Add(LanguageSettings.Current.ChangeCasing.AllUppercase);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.RegEx);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.OddLines);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.EvenLines);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.DurationLessThan);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.DurationGreaterThan);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.MoreThanTwoLines);
            comboBoxRule.Items.Add(LanguageSettings.Current.ModifySelection.Bookmarked);
            if (_format.HasStyleSupport)
            {
                comboBoxRule.Items.Add(LanguageSettings.Current.General.Style);
            }
            if (_format.HasStyleSupport && (_format.GetType() == typeof(AdvancedSubStationAlpha) || _format.GetType() == typeof(SubStationAlpha)))
            {
                comboBoxRule.Items.Add(LanguageSettings.Current.General.Actor);
            }

            checkBoxCaseSensitive.Checked = Configuration.Settings.Tools.ModifySelectionCaseSensitive;
            textBoxText.Text = Configuration.Settings.Tools.ModifySelectionText;
            switch (Configuration.Settings.Tools.ModifySelectionRule)
            {
                case StartsWith:
                    comboBoxRule.SelectedIndex = FunctionStartsWith;
                    break;
                case EndsWith:
                    comboBoxRule.SelectedIndex = FunctionEndsWith;
                    break;
                case NotContains:
                    comboBoxRule.SelectedIndex = FunctionNotContains;
                    break;
                case AllUppercase:
                    comboBoxRule.SelectedIndex = FunctionAllUppercase;
                    break;
                case RegEx:
                    comboBoxRule.SelectedIndex = FunctionRegEx;
                    break;
                case Odd:
                    comboBoxRule.SelectedIndex = FunctionOdd;
                    break;
                case Even:
                    comboBoxRule.SelectedIndex = FunctionEven;
                    break;
                case DurationLessThan:
                    comboBoxRule.SelectedIndex = FunctionDurationLessThan;
                    break;
                case DurationGreaterThan:
                    comboBoxRule.SelectedIndex = FunctionDurationGreaterThan;
                    break;
                case MoreThanTwoLines:
                    comboBoxRule.SelectedIndex = FunctionMoreThanTwoLines;
                    break;
                case Bookmarked:
                    comboBoxRule.SelectedIndex = FunctionBookmarked;
                    break;
                case Style when _format.HasStyleSupport:
                    comboBoxRule.SelectedIndex = FunctionStyle;
                    break;
                case Actor when _format.GetType() == typeof(AdvancedSubStationAlpha) || _format.GetType() == typeof(SubStationAlpha):
                    comboBoxRule.SelectedIndex = FunctionActor;
                    break;
                default:
                    comboBoxRule.SelectedIndex = FunctionContains;
                    break;
            }

            if (!_format.HasStyleSupport)
            {
                listViewFixes.Columns.Remove(columnHeaderStyle);
            }

            _loading = false;
            Preview();
        }

        private void ModifySelection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            ApplySelection();
            DialogResult = DialogResult.OK;

            Configuration.Settings.Tools.ModifySelectionCaseSensitive = checkBoxCaseSensitive.Checked;
            Configuration.Settings.Tools.ModifySelectionText = textBoxText.Text;
            switch (comboBoxRule.SelectedIndex)
            {
                case FunctionStartsWith:
                    Configuration.Settings.Tools.ModifySelectionRule = StartsWith;
                    break;
                case FunctionEndsWith:
                    Configuration.Settings.Tools.ModifySelectionRule = EndsWith;
                    break;
                case FunctionNotContains:
                    Configuration.Settings.Tools.ModifySelectionRule = NotContains;
                    break;
                case FunctionAllUppercase:
                    Configuration.Settings.Tools.ModifySelectionRule = AllUppercase;
                    break;
                case FunctionRegEx:
                    Configuration.Settings.Tools.ModifySelectionRule = RegEx;
                    break;
                case FunctionOdd:
                    Configuration.Settings.Tools.ModifySelectionRule = Odd;
                    break;
                case FunctionEven:
                    Configuration.Settings.Tools.ModifySelectionRule = Even;
                    break;
                case FunctionDurationLessThan:
                    Configuration.Settings.Tools.ModifySelectionRule = DurationLessThan;
                    break;
                case FunctionDurationGreaterThan:
                    Configuration.Settings.Tools.ModifySelectionRule = DurationGreaterThan;
                    break;
                case FunctionMoreThanTwoLines:
                    Configuration.Settings.Tools.ModifySelectionRule = MoreThanTwoLines;
                    break;
                case FunctionBookmarked:
                    Configuration.Settings.Tools.ModifySelectionRule = Bookmarked;
                    break;
                case FunctionStyle:
                    Configuration.Settings.Tools.ModifySelectionRule = Style;
                    break;
                case FunctionActor:
                    Configuration.Settings.Tools.ModifySelectionRule = Actor;
                    break;
                default:
                    Configuration.Settings.Tools.ModifySelectionRule = ContainsString;
                    break;
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            ApplySelection();
        }

        private ListViewItem MakeListViewItem(Paragraph p, int index)
        {
            var item = new ListViewItem(string.Empty) { Tag = index, Checked = true };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(UiUtil.GetListViewTextFromString(p.Text));
            if (_format.HasStyleSupport)
            {
                item.SubItems.Add(string.IsNullOrEmpty(p.Style) ? p.Extra : p.Style);
            }

            return item;
        }

        private void Preview()
        {
            if (_loading)
            {
                return;
            }

            Regex regEx = null;
            var listViewItems = new List<ListViewItem>();
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            var text = textBoxText.Text;
            if (comboBoxRule.SelectedIndex != FunctionRegEx)
            {
                text = text.Replace("\\r\\n", Environment.NewLine);
            }

            var styles = new List<string>();
            var actors = new List<string>();
            if (comboBoxRule.SelectedIndex == FunctionStyle) // Select styles
            {
                foreach (ListViewItem item in listViewStyles.Items)
                {
                    if (item.Checked)
                    {
                        styles.Add(item.Text);
                    }
                }
            }
            else if (comboBoxRule.SelectedIndex == FunctionActor) // Select actors
            {
                foreach (ListViewItem item in listViewStyles.Items)
                {
                    if (item.Checked)
                    {
                        actors.Add(item.Text);
                    }
                }
            }

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                if ((radioButtonSubtractFromSelection.Checked || radioButtonIntersect.Checked) && _subtitleListView.Items[i].Selected ||
                    !radioButtonSubtractFromSelection.Checked && !radioButtonIntersect.Checked)
                {
                    var p = _subtitle.Paragraphs[i];
                    if (text.Length > 0)
                    {
                        if (comboBoxRule.SelectedIndex == FunctionContains) // Contains
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.Contains(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && p.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                            {
                                listViewItems.Add(MakeListViewItem(p, i));
                            }
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionStartsWith) // Starts with
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.StartsWith(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && p.Text.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                            {
                                listViewItems.Add(MakeListViewItem(p, i));
                            }
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionEndsWith) // Ends with
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.EndsWith(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && p.Text.EndsWith(text, StringComparison.OrdinalIgnoreCase))
                            {
                                listViewItems.Add(MakeListViewItem(p, i));
                            }
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionNotContains) // Not contains
                        {
                            if (checkBoxCaseSensitive.Checked && !p.Text.Contains(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && !p.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                            {
                                listViewItems.Add(MakeListViewItem(p, i));
                            }
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionRegEx) // RegEx
                        {
                            labelInfo.Text = string.Empty;
                            if (regEx == null)
                            {
                                try
                                {
                                    regEx = new Regex(text, RegexOptions.Compiled);
                                }
                                catch (Exception e)
                                {
                                    labelInfo.Text = e.Message;
                                    break;
                                }
                            }
                            if (regEx.IsMatch(p.Text))
                            {
                                listViewItems.Add(MakeListViewItem(p, i));
                            }
                        }
                    }

                    if (comboBoxRule.SelectedIndex == FunctionOdd) // Select odd lines
                    {
                        if (i % 2 == 0)
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionEven) // Select even lines
                    {
                        if (i % 2 == 1)
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionDurationLessThan) // Duration less than
                    {
                        if (p.Duration.TotalMilliseconds < (double)numericUpDownDuration.Value)
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionDurationGreaterThan) // Duration greater than
                    {
                        if (p.Duration.TotalMilliseconds > (double)numericUpDownDuration.Value)
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionMoreThanTwoLines)
                    {
                        if (p.Text.SplitToLines().Count > 2)
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionAllUppercase) // All uppercase
                    {
                        if (HtmlUtil.RemoveHtmlTags(p.Text, true) == HtmlUtil.RemoveHtmlTags(p.Text, true).ToUpperInvariant() && p.Text != p.Text.ToLowerInvariant())
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionBookmarked) // Bookmarked
                    {
                        if (p.Bookmark != null)
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionStyle) // Select styles
                    {
                        if (styles.Contains(string.IsNullOrEmpty(p.Style) ? p.Extra : p.Style))
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionActor) // Select actors
                    {
                        if (actors.Contains(p.Actor))
                        {
                            listViewItems.Add(MakeListViewItem(p, i));
                        }
                    }
                }
            }

            listViewFixes.Items.AddRange(listViewItems.ToArray());
            listViewFixes.EndUpdate();
            groupBoxPreview.Text = string.Format(LanguageSettings.Current.ModifySelection.MatchingLinesX, listViewFixes.Items.Count);
            listViewFixes.AutoSizeLastColumn();
        }

        private void ApplySelection()
        {
            _subtitleListView.BeginUpdate();
            if (radioButtonNewSelection.Checked || radioButtonIntersect.Checked)
            {
                _subtitleListView.SelectNone();
            }

            if (radioButtonNewSelection.Checked || radioButtonAddToSelection.Checked || radioButtonIntersect.Checked)
            {
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (item.Checked)
                    {
                        var index = Convert.ToInt32(item.Tag);
                        _subtitleListView.Items[index].Selected = true;
                    }
                }
            }
            else if (radioButtonSubtractFromSelection.Checked)
            {
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (item.Checked)
                    {
                        var index = Convert.ToInt32(item.Tag);
                        _subtitleListView.Items[index].Selected = false;
                    }
                }
            }
            _subtitleListView.EndUpdate();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Preview();
        }

        private void comboBoxRule_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxText.Visible = true;
            listViewStyles.Visible = false;
            numericUpDownDuration.Visible = comboBoxRule.SelectedIndex == FunctionDurationLessThan || comboBoxRule.SelectedIndex == FunctionDurationGreaterThan;
            if (comboBoxRule.SelectedIndex == FunctionRegEx) // RegEx
            {
                textBoxText.ContextMenuStrip = FindReplaceDialogHelper.GetRegExContextMenu(textBoxText);
                checkBoxCaseSensitive.Enabled = false;
            }
            else if (comboBoxRule.SelectedIndex == FunctionOdd || comboBoxRule.SelectedIndex == FunctionEven || comboBoxRule.SelectedIndex == FunctionMoreThanTwoLines || comboBoxRule.SelectedIndex == FunctionBookmarked)
            {
                checkBoxCaseSensitive.Enabled = false;
                textBoxText.ContextMenuStrip = null;
                textBoxText.Visible = false;
            }
            else if (comboBoxRule.SelectedIndex == FunctionDurationLessThan || comboBoxRule.SelectedIndex == FunctionDurationGreaterThan || comboBoxRule.SelectedIndex == FunctionAllUppercase)
            {
                checkBoxCaseSensitive.Enabled = false;
                listViewStyles.Visible = false;
                textBoxText.Visible = false;
                if (comboBoxRule.SelectedIndex == FunctionDurationLessThan)
                {
                    if (numericUpDownDuration.Value == 0 &&
                        Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds >= numericUpDownDuration.Minimum &&
                        Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds <= numericUpDownDuration.Maximum)
                    {
                        numericUpDownDuration.Value = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
                    }
                }
                else
                {
                    if (numericUpDownDuration.Value == 0 &&
                        Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds >= numericUpDownDuration.Minimum &&
                        Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds <= numericUpDownDuration.Maximum)
                    {
                        numericUpDownDuration.Value = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                    }
                }
            }
            else if (comboBoxRule.SelectedIndex == FunctionStyle)
            {
                checkBoxCaseSensitive.Enabled = false;
                listViewStyles.Visible = true;
                listViewStyles.BringToFront();
                FillStyles();
            }
            else if (comboBoxRule.SelectedIndex == FunctionActor)
            {
                checkBoxCaseSensitive.Enabled = false;
                listViewStyles.Visible = true;
                listViewStyles.BringToFront();
                FillActors();
            }
            else
            {
                textBoxText.ContextMenuStrip = null;
                checkBoxCaseSensitive.Enabled = true;
            }

            Preview();
        }

        private void FillStyles()
        {
            listViewStyles.AutoSizeLastColumn();
            var styles = new List<string>();
            var formatType = _format.GetType();
            if (formatType == typeof(AdvancedSubStationAlpha) || formatType == typeof(SubStationAlpha))
            {
                styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            }
            else if (formatType == typeof(TimedText10) || formatType == typeof(ItunesTimedText))
            {
                styles = TimedText10.GetStylesFromHeader(_subtitle.Header);
            }
            else if (formatType == typeof(Sami) || formatType == typeof(SamiModern))
            {
                styles = _subtitle.Header == null ? Sami.GetStylesFromSubtitle(_subtitle) : Sami.GetStylesFromHeader(_subtitle.Header);
            }

            _loading = true;
            listViewStyles.Items.Clear();
            listViewStyles.Items.AddRange(styles.OrderBy(p => p).Select(p => new ListViewItem { Text = p }).ToArray());
            _loading = false;
        }

        private void FillActors()
        {
            listViewStyles.AutoSizeLastColumn();
            var actors = new List<string>();
            foreach (var paragraph in _subtitle.Paragraphs)
            {
                if (!string.IsNullOrEmpty(paragraph.Actor) && !actors.Contains(paragraph.Actor))
                {
                    actors.Add(paragraph.Actor);
                }
            }

            _loading = true;
            listViewStyles.Items.Clear();
            listViewStyles.Items.AddRange(actors.OrderBy(p => p).Select(p => new ListViewItem { Text = p }).ToArray());
            _loading = false;
        }

        private void checkBoxCaseSensitive_CheckedChanged(object sender, EventArgs e)
        {
            Preview();
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            Preview();
        }

        private void ModifySelection_Shown(object sender, EventArgs e)
        {
            ModifySelection_Resize(sender, e);
        }

        private void listViewStyles_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Preview();
        }

        private void numericUpDownDuration_ValueChanged(object sender, EventArgs e)
        {
            Preview();
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

        private void ModifySelection_Resize(object sender, EventArgs e)
        {
            ModifySelection_ResizeEnd(null, null);
        }

        private void ModifySelection_ResizeEnd(object sender, EventArgs e)
        {
            listViewFixes.Columns[0].Width = 50;
            listViewFixes.Columns[1].Width = 80;

            var remainingWidth = listViewFixes.ClientSize.Width - listViewFixes.Columns[0].Width - listViewFixes.Columns[1].Width;
            if (_format.HasStyleSupport)
            {
                listViewFixes.Columns[2].Width = 4 * remainingWidth / 5;
                remainingWidth -= listViewFixes.Columns[2].Width;
            }

            listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = remainingWidth;
        }
    }
}
