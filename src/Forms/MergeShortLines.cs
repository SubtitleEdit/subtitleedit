using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class MergeShortLines : PositionAndSizeForm
    {
        public int NumberOfMerges { get; private set; }
        public Subtitle MergedSubtitle { get; private set; }

        private Subtitle _subtitle;

        public MergeShortLines()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);
            SubtitleListview1.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            SubtitleListview1.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
        }

        private void MergeShortLines_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        public void Initialize(Subtitle subtitle)
        {
            if (subtitle.Paragraphs.Count > 0)
            {
                subtitle.Renumber();
            }

            Text = Configuration.Settings.Language.MergedShortLines.Title;
            labelMaxCharacters.Text = Configuration.Settings.Language.MergedShortLines.MaximumCharacters;
            labelMaxMillisecondsBetweenLines.Text = Configuration.Settings.Language.MergedShortLines.MaximumMillisecondsBetween;

            checkBoxOnlyContinuationLines.Text = Configuration.Settings.Language.MergedShortLines.OnlyMergeContinuationLines;

            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.MergedShortLines.MergedText;

            toolStripMenuItemInverseSelection.Text = Configuration.Settings.Language.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.SelectAll;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            NumberOfMerges = 0;
            if (Configuration.Settings.Tools.MergeShortLinesMaxChars > numericUpDownMaxCharacters.Maximum)
            {
                numericUpDownMaxCharacters.Value = numericUpDownMaxCharacters.Maximum;
            }
            else if (Configuration.Settings.Tools.MergeShortLinesMaxChars < numericUpDownMaxCharacters.Minimum)
            {
                numericUpDownMaxCharacters.Value = numericUpDownMaxCharacters.Minimum;
            }
            else
            {
                numericUpDownMaxCharacters.Value = Configuration.Settings.Tools.MergeShortLinesMaxChars;
            }
            numericUpDownMaxMillisecondsBetweenLines.Value = Configuration.Settings.Tools.MergeShortLinesMaxGap;
            checkBoxOnlyContinuationLines.Checked = Configuration.Settings.Tools.MergeShortLinesOnlyContinuous;
            _subtitle = subtitle;
        }

        private void AddToListView(Paragraph p, string lineNumbers, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            item.SubItems.Add(lineNumbers.TrimEnd(','));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(newText));
            listViewFixes.Items.Add(item);
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
            {
                return;
            }

            var mergedIndexes = new List<int>();

            NumberOfMerges = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            MergedSubtitle = MergeShortLinesInSubtitle(_subtitle, mergedIndexes, out var count, (double)numericUpDownMaxMillisecondsBetweenLines.Value, (int)numericUpDownMaxCharacters.Value, true);
            NumberOfMerges = count;

            SubtitleListview1.Fill(_subtitle);

            foreach (var index in mergedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, Color.Green);
            }

            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.MergedShortLines.NumberOfMergesX, NumberOfMerges);
        }

        private bool IsFixAllowed(Paragraph p)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                string numbers = item.SubItems[1].Text;
                foreach (string number in numbers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (number == p.Number.ToString(CultureInfo.InvariantCulture))
                    {
                        return item.Checked;
                    }
                }
            }
            return true;
        }

        public Subtitle MergeShortLinesInSubtitle(Subtitle subtitle, List<int> mergedIndexes, out int numberOfMerges, double maxMillisecondsBetweenLines, int maxCharacters, bool clearFixes)
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            if (clearFixes)
            {
                listViewFixes.Items.Clear();
            }

            numberOfMerges = 0;
            string language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var mergedSubtitle = new Subtitle();
            bool lastMerged = false;
            Paragraph p = null;
            var lineNumbers = new StringBuilder();
            bool onlyContinuousLines = checkBoxOnlyContinuationLines.Checked;
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (!lastMerged)
                {
                    p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                    mergedSubtitle.Paragraphs.Add(p);
                }
                Paragraph next = subtitle.GetParagraphOrDefault(i);
                if (next != null)
                {
                    if (Utilities.QualifiesForMerge(p, next, maxMillisecondsBetweenLines, maxCharacters, onlyContinuousLines) && IsFixAllowed(p))
                    {
                        if (MergeShortLinesUtils.GetStartTag(p.Text) == MergeShortLinesUtils.GetStartTag(next.Text) &&
                            MergeShortLinesUtils.GetEndTag(p.Text) == MergeShortLinesUtils.GetEndTag(next.Text))
                        {
                            string s1 = p.Text.Trim();
                            s1 = s1.Substring(0, s1.Length - MergeShortLinesUtils.GetEndTag(s1).Length);
                            string s2 = next.Text.Trim();
                            s2 = s2.Substring(MergeShortLinesUtils.GetStartTag(s2).Length);
                            p.Text = Utilities.AutoBreakLine(s1 + Environment.NewLine + s2, language);
                        }
                        else
                        {
                            p.Text = Utilities.AutoBreakLine(p.Text + Environment.NewLine + next.Text, language);
                        }
                        p.EndTime = next.EndTime;

                        if (lastMerged)
                        {
                            lineNumbers.Append(next.Number);
                            lineNumbers.Append(',');
                        }
                        else
                        {
                            lineNumbers.Append(p.Number);
                            lineNumbers.Append(',');
                            lineNumbers.Append(next.Number);
                            lineNumbers.Append(',');
                        }

                        lastMerged = true;
                        numberOfMerges++;
                        if (!mergedIndexes.Contains(i))
                        {
                            mergedIndexes.Add(i);
                        }

                        if (!mergedIndexes.Contains(i - 1))
                        {
                            mergedIndexes.Add(i - 1);
                        }
                    }
                    else
                    {
                        lastMerged = false;
                    }
                }
                else
                {
                    lastMerged = false;
                }
                if (!lastMerged && lineNumbers.Length > 0 && clearFixes)
                {
                    AddToListView(p, lineNumbers.ToString(), p.Text);
                    lineNumbers.Clear();
                }
            }
            if (!lastMerged)
            {
                mergedSubtitle.Paragraphs.Add(new Paragraph(subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1)));
            }

            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            return mergedSubtitle;
        }

        private void NumericUpDownMaxCharactersValueChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void NumericUpDownMaxMillisecondsBetweenLinesValueChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.MergeShortLinesMaxGap = (int)numericUpDownMaxMillisecondsBetweenLines.Value;
            Configuration.Settings.Tools.MergeShortLinesOnlyContinuous = checkBoxOnlyContinuationLines.Checked;
            DialogResult = DialogResult.OK;
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedIndices.Count > 0)
            {
                int index = listViewFixes.SelectedIndices[0];
                ListViewItem item = listViewFixes.Items[index];
                string[] numbers = item.SubItems[1].Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string number in numbers)
                {
                    foreach (Paragraph p in _subtitle.Paragraphs)
                    {
                        if (p.Number.ToString(CultureInfo.InvariantCulture) == number)
                        {
                            index = _subtitle.GetIndex(p);
                            SubtitleListview1.EnsureVisible(index);
                        }
                    }
                }
            }
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var mergedIndexes = new List<int>();

            NumberOfMerges = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            int count;
            MergedSubtitle = MergeShortLinesInSubtitle(_subtitle, mergedIndexes, out count, (double)numericUpDownMaxMillisecondsBetweenLines.Value, (int)numericUpDownMaxCharacters.Value, false);
            NumberOfMerges = count;
            SubtitleListview1.Fill(_subtitle);
            foreach (var index in mergedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, Color.Green);
            }
            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.MergedShortLines.NumberOfMergesX, NumberOfMerges);
        }

        private void MergeShortLines_Shown(object sender, EventArgs e)
        {
            GeneratePreview();
            listViewFixes.Focus();
            if (listViewFixes.Items.Count > 0)
            {
                listViewFixes.Items[0].Selected = true;
            }
        }

        private void checkBoxOnlyContinuationLines_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void MergeShortLines_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.MergeShortLinesMaxChars = (int)numericUpDownMaxCharacters.Value;
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
