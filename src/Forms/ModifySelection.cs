using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ModifySelection : PositionAndSizeForm
    {
        private readonly SubtitleListView _subtitleListView;
        private Subtitle _subtitle;
        private bool _loading;

        private const int FunctionContains = 0;
        private const int FunctionStartsWith = 1;
        private const int FunctionEndsWith = 2;
        private const int FunctionNotContains = 3;
        private const int FunctionRegEx = 4;
        private const int FunctionUnequal = 5;
        private const int FunctionEqual = 6;

        public ModifySelection(Subtitle subtitle, SubtitleListView subtitleListView)
        {
            InitializeComponent();
            _loading = true;
            _subtitle = subtitle;
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

            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxRule.Items.Clear();
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.Contains);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.StartsWith);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.EndsWith);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.NoContains);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.RegEx);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.UnequalLines);
            comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.EqualLines);

            checkBoxCaseSensitive.Checked = Configuration.Settings.Tools.ModifySelectionCaseSensitive;
            textBox1.Text = Configuration.Settings.Tools.ModifySelectionText;
            if (Configuration.Settings.Tools.ModifySelectionRule == "Starts with")
                comboBoxRule.SelectedIndex = FunctionStartsWith;
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Ends with")
                comboBoxRule.SelectedIndex = FunctionEndsWith;
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Not contains")
                comboBoxRule.SelectedIndex = FunctionNotContains;
            else if (Configuration.Settings.Tools.ModifySelectionRule == "RegEx")
                comboBoxRule.SelectedIndex = FunctionRegEx;
            else
                comboBoxRule.SelectedIndex = 0;
            _loading = false;
            Preview();
        }

        private void ModifySelection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
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
            Configuration.Settings.Tools.ModifySelectionText = textBox1.Text;
            if (comboBoxRule.SelectedIndex == FunctionContains)
                Configuration.Settings.Tools.ModifySelectionRule = "Contains";
            else if (comboBoxRule.SelectedIndex == FunctionStartsWith)
                Configuration.Settings.Tools.ModifySelectionRule = "Starts with";
            else if (comboBoxRule.SelectedIndex == FunctionEndsWith)
                Configuration.Settings.Tools.ModifySelectionRule = "Ends with";
            else if (comboBoxRule.SelectedIndex == FunctionNotContains)
                Configuration.Settings.Tools.ModifySelectionRule = "Not contains";
            else if (comboBoxRule.SelectedIndex == FunctionRegEx)
                Configuration.Settings.Tools.ModifySelectionRule = "RegEx";
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            ApplySelection();
        }

        private void AddToListView(Paragraph p, int index)
        {
            var item = new ListViewItem(string.Empty) { Tag = index, Checked = true };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void Preview()
        {
            if (_loading)
                return;

            Regex regEx = null;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            string text = textBox1.Text;
            if (comboBoxRule.SelectedIndex != FunctionRegEx)
                text = text.Replace("\\r\\n", Environment.NewLine);

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
                                AddToListView(p, i);
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionStartsWith) // Starts with
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.StartsWith(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && p.Text.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                                AddToListView(p, i);
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionEndsWith) // Ends with
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.EndsWith(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && p.Text.EndsWith(text, StringComparison.OrdinalIgnoreCase))
                                AddToListView(p, i);
                        }
                        else if (comboBoxRule.SelectedIndex == FunctionNotContains) // Not contains
                        {
                            if (checkBoxCaseSensitive.Checked && !p.Text.Contains(text, StringComparison.Ordinal) || !checkBoxCaseSensitive.Checked && !p.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                                AddToListView(p, i);
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
                                AddToListView(p, i);
                        }
                    }
                    if (comboBoxRule.SelectedIndex == FunctionUnequal) // select unequal lines
                    {
                        if (i % 2 == 0)
                            AddToListView(p, i);
                    }
                    else if (comboBoxRule.SelectedIndex == FunctionEqual) // select equal lines
                    {
                        if (i % 2 == 1)
                            AddToListView(p, i);
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
                _subtitleListView.SelectNone();

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
            if (comboBoxRule.SelectedIndex == FunctionRegEx) // regex
            {
                textBox1.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBox1);
                checkBoxCaseSensitive.Enabled = false;
            }
            else if (comboBoxRule.SelectedIndex == FunctionUnequal || comboBoxRule.SelectedIndex == FunctionEqual)
            {
                textBox1.ContextMenuStrip = null;
                checkBoxCaseSensitive.Enabled = false;
            }
            else
            {
                textBox1.ContextMenuStrip = null;
                checkBoxCaseSensitive.Enabled = true;
            }
            Preview();
        }

        private void checkBoxCaseSensitive_CheckedChanged(object sender, EventArgs e)
        {
            Preview();
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            Preview();
        }

    }
}
