using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
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

            Text = LanguageSettings.Current.MergedShortLines.Title;
            labelMaxCharacters.Text = LanguageSettings.Current.MergedShortLines.MaximumCharacters;
            labelMaxMillisecondsBetweenLines.Text = LanguageSettings.Current.MergedShortLines.MaximumMillisecondsBetween;

            checkBoxOnlyContinuationLines.Text = LanguageSettings.Current.MergedShortLines.OnlyMergeContinuationLines;

            listViewFixes.Columns[0].Text = LanguageSettings.Current.General.Apply;
            listViewFixes.Columns[1].Text = LanguageSettings.Current.General.LineNumber;
            listViewFixes.Columns[2].Text = LanguageSettings.Current.MergedShortLines.MergedText;

            toolStripMenuItemInverseSelection.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            SubtitleListview1.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
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

            var greenColor = UiUtil.GreenBackgroundColor;
            SubtitleListview1.Fill(_subtitle);
            foreach (var index in mergedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, greenColor);
            }

            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MergedShortLines.NumberOfMergesX, NumberOfMerges);
        }

        private bool IsFixAllowed(Paragraph p)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                var numbers = item.SubItems[1].Text;
                foreach (var number in numbers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
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

            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var dialogHelper = new DialogSplitMerge
            {
                DialogStyle = Configuration.Settings.General.DialogStyle, 
                TwoLetterLanguageCode = language,
            };
            numberOfMerges = 0;
            var mergedSubtitle = new Subtitle();
            var lastMerged = false;
            Paragraph p = null;
            var lineNumbers = new StringBuilder();
            var onlyContinuousLines = checkBoxOnlyContinuationLines.Checked;
            for (var i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (!lastMerged)
                {
                    p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                    mergedSubtitle.Paragraphs.Add(p);
                }

                var next = subtitle.GetParagraphOrDefault(i);
                if (next != null)
                {
                    if (Utilities.QualifiesForMerge(p, next, maxMillisecondsBetweenLines, maxCharacters, onlyContinuousLines) && IsFixAllowed(p))
                    {
                        if (MergeShortLinesUtils.GetStartTag(p.Text) == MergeShortLinesUtils.GetStartTag(next.Text) &&
                            MergeShortLinesUtils.GetEndTag(p.Text) == MergeShortLinesUtils.GetEndTag(next.Text))
                        {
                            var nextArr = next.Text.SplitToLines();
                            if (!p.Text.HasSentenceEnding(language) && p.Text.SplitToLines().Count == 1 && 
                                nextArr.Count == 2 && dialogHelper.IsDialog(nextArr))
                            {
                                if (nextArr[0].Contains('-'))
                                {
                                    nextArr[0] = nextArr[0].Remove(nextArr[0].IndexOf('-'), 1);
                                }

                                var s1 = ("- " + p.Text.Trim() + nextArr[0]).Replace("  ", " ");
                                var s2 = nextArr[1];

                                if (HtmlUtil.RemoveHtmlTags(s1, true).Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                                {
                                    continue;
                                }

                                p.Text = dialogHelper.FixDashesAndSpaces(s1 + Environment.NewLine + s2);
                            }
                            else
                            {
                                var s1 = p.Text.Trim();
                                s1 = s1.Substring(0, s1.Length - MergeShortLinesUtils.GetEndTag(s1).Length);
                                var s2 = next.Text.Trim();
                                s2 = s2.Substring(MergeShortLinesUtils.GetStartTag(s2).Length);
                                p.Text = Utilities.AutoBreakLine(s1 + Environment.NewLine + s2, language);
                            }
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
                var index = listViewFixes.SelectedIndices[0];
                var item = listViewFixes.Items[index];
                var numbers = item.SubItems[1].Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var number in numbers)
                {
                    foreach (var p in _subtitle.Paragraphs)
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
            MergedSubtitle = MergeShortLinesInSubtitle(_subtitle, mergedIndexes, out int count, (double)numericUpDownMaxMillisecondsBetweenLines.Value, (int)numericUpDownMaxCharacters.Value, false);
            NumberOfMerges = count;
            SubtitleListview1.Fill(_subtitle);
            var greenColor = UiUtil.GreenBackgroundColor;
            foreach (var index in mergedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, greenColor);
            }
            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MergedShortLines.NumberOfMergesX, NumberOfMerges);
        }

        private void MergeShortLines_ResizeEnd(object sender, EventArgs e)
        {
            listViewFixes.AutoSizeLastColumn();
        }

        private void MergeShortLines_Shown(object sender, EventArgs e)
        {
            MergeShortLines_ResizeEnd(sender, e);

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
            listViewFixes.CheckAll();
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            listViewFixes.InvertCheck();
        }
    }
}
