using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ModifySelection : PositionAndSizeForm
    {
        private readonly SubtitleListView _subtitleListView;
        private readonly Subtitle _subtitle;
        private readonly SubtitleFormat _format;
        private readonly bool _loading;

        private const int FunctionContains = 0;
        private const int FunctionStartsWith = 1;
        private const int FunctionEndsWith = 2;
        private const int FunctionNotContains = 3;
        private const int FunctionAlUppercase = 4;
        private const int FunctionRegEx = 5;
        private const int FunctionUnequal = 6;
        private const int FunctionEqual = 7;
        private const int FunctionDurationLessThan = 8;
        private const int FunctionDurationGreaterThan = 9;
        private const int FunctionStyle = 10;

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
            Text = Configuration.Settings.Language.ModifySelection.Title;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonApply.Text = Configuration.Settings.Language.General.Apply;
            groupBoxRule.Text = Configuration.Settings.Language.ModifySelection.Rule;
            groupBoxWhatToDo.Text = Configuration.Settings.Language.ModifySelection.DoWithMatches;
            checkBoxCaseSensitive.Text = Configuration.Settings.Language.ModifySelection.CaseSensitive;
            radioButtonNewSelection.Text = Configuration.Settings.Language.ModifySelection.MakeNewSelection;
            radioButtonAddToSelection.Text = Configuration.Settings.Language.ModifySelection.AddToCurrentSelection;
            radioButtonSubtractFromSelection.Text = Configuration.Settings.Language.ModifySelection.SubtractFromCurrentSelection;
            radioButtonIntersect.Text = Configuration.Settings.Language.ModifySelection.IntersectWithCurrentSelection;
            columnHeaderApply.Text = Configuration.Settings.Language.General.Apply;
            columnHeaderLine.Text = Configuration.Settings.Language.General.LineNumber;
            columnHeaderText.Text = Configuration.Settings.Language.General.Text;
            toolStripMenuItemInverseSelection.Text = Configuration.Settings.Language.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.SelectAll;

            listViewStyles.Visible = false;

            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxRule.Items.Clear();
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.Contains);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.StartsWith);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.EndsWith);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.NoContains);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ChangeCasing.AllUppercase);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.RegEx);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.UnequalLines);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.EqualLines);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.DurationLessThan);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.DurationGreaterThan);
            if (_format.HasStyleSupport)
            {
                comboBoxRule.Items.Add(Configuration.Settings.Language.General.Style);
            }

            checkBoxCaseSensitive.Checked = Configuration.Settings.Tools.ModifySelectionCaseSensitive;
            textBoxText.Text = Configuration.Settings.Tools.ModifySelectionText;
            if (Configuration.Settings.Tools.ModifySelectionRule == "Starts with")
            {
                comboBoxRule.SelectedIndex = FunctionStartsWith;
            }
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Ends with")
            {
                comboBoxRule.SelectedIndex = FunctionEndsWith;
            }
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Not contains")
            {
                comboBoxRule.SelectedIndex = FunctionNotContains;
            }
            else if (Configuration.Settings.Tools.ModifySelectionRule == "AllUppercase")
            {
                comboBoxRule.SelectedIndex = FunctionAlUppercase;
            }
            else if (Configuration.Settings.Tools.ModifySelectionRule == "RegEx")
            {
                comboBoxRule.SelectedIndex = FunctionRegEx;
            }
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Duration <")
            {
                comboBoxRule.SelectedIndex = FunctionDurationLessThan;
            }
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Duration >")
            {
                comboBoxRule.SelectedIndex = FunctionDurationGreaterThan;
            }
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Style" && _format.HasStyleSupport)
            {
                comboBoxRule.SelectedIndex = FunctionStyle;
            }
            else
            {
                comboBoxRule.SelectedIndex = 0;
            }

            if (!_format.HasStyleSupport)
            {
                listViewFixes.Columns.Remove(columnHeaderStyle);
            }
            ModifySelection_Resize(null, null);

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
            if (comboBoxRule.SelectedIndex == FunctionContains)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "Contains";
            }
            else if (comboBoxRule.SelectedIndex == FunctionStartsWith)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "Starts with";
            }
            else if (comboBoxRule.SelectedIndex == FunctionEndsWith)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "Ends with";
            }
            else if (comboBoxRule.SelectedIndex == FunctionNotContains)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "Not contains";
            }
            else if (comboBoxRule.SelectedIndex == FunctionAlUppercase)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "AllUppercase";
            }
            else if (comboBoxRule.SelectedIndex == FunctionRegEx)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "RegEx";
            }
            else if (comboBoxRule.SelectedIndex == FunctionDurationLessThan)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "Duration <";
            }
            else if (comboBoxRule.SelectedIndex == FunctionDurationGreaterThan)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "Duration >";
            }
            else if (comboBoxRule.SelectedIndex == FunctionStyle)
            {
                Configuration.Settings.Tools.ModifySelectionRule = "Style";
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            ApplySelection();
        }

        private void AddToListView(Paragraph p, int index)
        {
            var item = new ListViewItem(string.Empty) { Tag = index, Checked = true };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(UiUtil.GetListViewTextFromString(p.Text));
            if (_format.HasStyleSupport)
            {
                item.SubItems.Add(string.IsNullOrEmpty(p.Style) ? p.Extra : p.Style);
            }
            listViewFixes.Items.Add(item);
        }

        private void Preview()
        {
            if (_loading)
            {
                return;
            }

            Regex regEx = null;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            string text = textBoxText.Text;
            if (comboBoxRule.SelectedIndex != FunctionRegEx)
            {
                text = text.Replace("\\r\\n", Environment.NewLine);
            }

            var styles = new List<string>();
            if (comboBoxRule.SelectedIndex == FunctionStyle) // select styles
            {
                foreach (ListViewItem item in listViewStyles.Items)
                {
                    if (item.Checked)
                    {
                        styles.Add(item.Text);
                    }
                }
            }

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                if ((radioButtonSubtractFromSelection.Checked || radioButtonIntersect.Checked) && _subtitleListView.Items[i].Selected ||
                    !radioButtonSubtractFromSelection.Checked && !radioButtonIntersect.Checked)
                {
                    Paragraph p = _subtitle.Paragraphs[i];
                    if (text.Length > 0)
                    {
                        if (comboBoxRule.SelectedIndex == FunctionContains) // Contains
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.Contains(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && p.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                            {
                                AddToListView(p, i);
                            }
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionStartsWith) // Starts with
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.StartsWith(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && p.Text.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                            {
                                AddToListView(p, i);
                            }
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionEndsWith) // Ends with
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.EndsWith(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && p.Text.EndsWith(text, StringComparison.OrdinalIgnoreCase))
                            {
                                AddToListView(p, i);
                            }
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionNotContains) // Not contains
                        {
                            if (checkBoxCaseSensitive.Checked && !p.Text.Contains(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && !p.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                            {
                                AddToListView(p, i);
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
                                AddToListView(p, i);
                            }
                        }
                    }
                    if (comboBoxRule.SelectedIndex == FunctionUnequal) // select unequal lines
                    {
                        if (i % 2 == 0)
                        {
                            AddToListView(p, i);
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionEqual) // select equal lines
                    {
                        if (i % 2 == 1)
                        {
                            AddToListView(p, i);
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionDurationLessThan) // duration less than
                    {
                        if (p.Duration.TotalMilliseconds < (double)numericUpDownDuration.Value)
                        {
                            AddToListView(p, i);
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionDurationGreaterThan) // duration greater than
                    {
                        if (p.Duration.TotalMilliseconds > (double)numericUpDownDuration.Value)
                        {
                            AddToListView(p, i);
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionAlUppercase) // all uppercase
                    {
                        if (p.Text == p.Text.ToUpperInvariant() && p.Text != p.Text.ToLowerInvariant())
                        {
                            AddToListView(p, i);
                        }
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionStyle) // select styles
                    {
                        if (styles.Contains(string.IsNullOrEmpty(p.Style) ? p.Extra : p.Style))
                        {
                            AddToListView(p, i);
                        }
                    }
                }
            }

            listViewFixes.EndUpdate();
            groupBoxPreview.Text = string.Format(Configuration.Settings.Language.ModifySelection.MatchingLinesX, listViewFixes.Items.Count);
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
                        int index = Convert.ToInt32(item.Tag);
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
                        int index = Convert.ToInt32(item.Tag);
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
            if (comboBoxRule.SelectedIndex == FunctionRegEx) // regex
            {
                textBoxText.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBoxText);
                checkBoxCaseSensitive.Enabled = false;
            }
            else if (comboBoxRule.SelectedIndex == FunctionUnequal || comboBoxRule.SelectedIndex == FunctionEqual)
            {
                textBoxText.ContextMenuStrip = null;
                checkBoxCaseSensitive.Enabled = false;
            }
            else if (comboBoxRule.SelectedIndex == FunctionStyle)
            {
                checkBoxCaseSensitive.Enabled = false;
                listViewStyles.Visible = true;
                listViewStyles.BringToFront();
                if (listViewStyles.Items.Count == 0)
                {
                    FillStyles();
                }
            }
            else if (comboBoxRule.SelectedIndex == FunctionDurationLessThan || comboBoxRule.SelectedIndex == FunctionDurationGreaterThan || comboBoxRule.SelectedIndex == FunctionAlUppercase)
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
            else
            {
                textBoxText.ContextMenuStrip = null;
                checkBoxCaseSensitive.Enabled = true;
            }

            Preview();
        }

        private void FillStyles()
        {
            listViewStyles.Columns[listViewStyles.Columns.Count - 1].Width = -2;
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

            foreach (var style in styles)
            {
                listViewStyles.Items.Add(style);
            }
        }

        private void checkBoxCaseSensitive_CheckedChanged(object sender, EventArgs e)
        {
            Preview();
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            Preview();
        }

        private void ModifySelection_Resize(object sender, EventArgs e)
        {
            listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
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
    }
}
