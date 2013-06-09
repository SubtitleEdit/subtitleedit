using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ModifySelection : Form
    {
        SubtitleListView _subtitleListView;
        Subtitle _subtitle;
        bool _loading;

        public ModifySelection(Subtitle subtitle, SubtitleListView subtitleListView)
        {
            InitializeComponent();
            _loading = true;
            _subtitle = subtitle;
            _subtitleListView = subtitleListView;
            labelInfo.Text = string.Empty;
            comboBoxRule.SelectedIndex = 0;
            Text = Configuration.Settings.Language.ModifySelection.Title;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
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

            FixLargeFonts();

            if (!string.IsNullOrEmpty(Configuration.Settings.Language.ModifySelection.Contains)) //TODO: Remove in SE 3.4
            {
                comboBoxRule.Items.Clear();
                comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.Contains);
                comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.StartsWith);
                comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.EndsWith);
                comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.NoContains);
                comboBoxRule.Items.Add(Configuration.Settings.Language.ModifySelection.RegEx);
            }

            checkBoxCaseSensitive.Checked = Configuration.Settings.Tools.ModifySelectionCaseSensitive;
            textBox1.Text = Configuration.Settings.Tools.ModifySelectionText;
            if (Configuration.Settings.Tools.ModifySelectionRule == "Starts with")
                comboBoxRule.SelectedIndex = 1;
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Ends with")
                comboBoxRule.SelectedIndex = 2;
            else if (Configuration.Settings.Tools.ModifySelectionRule == "Not contains")
                comboBoxRule.SelectedIndex = 3;
            else if (Configuration.Settings.Tools.ModifySelectionRule == "RegEx")
                comboBoxRule.SelectedIndex = 4;
            else
                comboBoxRule.SelectedIndex = 0;
            _loading = false;
            Preview();
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
            if (comboBoxRule.SelectedIndex == 0)
                Configuration.Settings.Tools.ModifySelectionRule = "Contains";
            else if (comboBoxRule.SelectedIndex == 1)
                Configuration.Settings.Tools.ModifySelectionRule = "Starts with";
            else if (comboBoxRule.SelectedIndex == 2)
                Configuration.Settings.Tools.ModifySelectionRule = "Ends with";
            else if (comboBoxRule.SelectedIndex == 3)
                Configuration.Settings.Tools.ModifySelectionRule = "Not contains";
            else if (comboBoxRule.SelectedIndex == 4)
                Configuration.Settings.Tools.ModifySelectionRule = "RegEx";
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            ApplySelection();
        }

        private void AddToListView(Paragraph p, int index)
        {
            var item = new ListViewItem(string.Empty) { Tag = index, Checked = true };
            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);
        }

        private void Preview()
        {
            if (_loading)
                return;

            Regex regEx = null;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            if (textBox1.Text.Length > 0)
            {
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    if ((radioButtonSubtractFromSelection.Checked || radioButtonIntersect.Checked) && _subtitleListView.Items[i].Selected ||
                        !radioButtonSubtractFromSelection.Checked && !radioButtonIntersect.Checked)
                    {
                        Paragraph p = _subtitle.Paragraphs[i];
                        if (comboBoxRule.SelectedIndex == 0) // Contains
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.Contains(textBox1.Text) || !checkBoxCaseSensitive.Checked && p.Text.ToLower().Contains(textBox1.Text.ToLower()))
                                AddToListView(p, i);
                        }
                        else if (comboBoxRule.SelectedIndex == 1) // Starts with
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.StartsWith(textBox1.Text) || !checkBoxCaseSensitive.Checked && p.Text.ToLower().StartsWith(textBox1.Text.ToLower()))
                                AddToListView(p, i);
                        }
                        else if (comboBoxRule.SelectedIndex == 2) // Ends with
                        {
                            if (checkBoxCaseSensitive.Checked && p.Text.EndsWith(textBox1.Text) || !checkBoxCaseSensitive.Checked && p.Text.ToLower().EndsWith(textBox1.Text.ToLower()))
                                AddToListView(p, i);
                        }
                        else if (comboBoxRule.SelectedIndex == 3) // Not contains
                        {
                            if (checkBoxCaseSensitive.Checked && !p.Text.Contains(textBox1.Text) || !checkBoxCaseSensitive.Checked && !p.Text.ToLower().Contains(textBox1.Text.ToLower()))
                                AddToListView(p, i);
                        }
                        else if (comboBoxRule.SelectedIndex == 4) // RegEx
                        {
                            labelInfo.Text = string.Empty;
                            if (regEx == null)
                            {
                                try
                                {
                                    regEx = new Regex(textBox1.Text, RegexOptions.Compiled);
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
                }
            }
            listViewFixes.EndUpdate();
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.ModifySelection.MatchingLinesX))
                groupBoxPreview.Text = string.Format(Configuration.Settings.Language.ModifySelection.MatchingLinesX, listViewFixes.Items.Count);
            else
                groupBoxPreview.Text = string.Format("Matching lines: {0}", listViewFixes.Items.Count); //TODO: Remove in 3.4
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
            if (comboBoxRule.SelectedIndex == 4) // regex
            {
                textBox1.ContextMenu = FindReplaceDialogHelper.GetRegExContextMenu(textBox1);
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
