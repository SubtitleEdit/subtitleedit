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
    public partial class MergeDoubleLines : PositionAndSizeForm
    {
        private Subtitle _subtitle;
        private Subtitle _mergedSubtitle;
        private bool _loading = true;
        private readonly Timer _previewTimer = new Timer();

        public int NumberOfMerges { get; private set; }

        public MergeDoubleLines()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _previewTimer.Tick += previewTimer_Tick;
            _previewTimer.Interval = 250;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public Subtitle MergedSubtitle => _mergedSubtitle;

        public void Initialize(Subtitle subtitle)
        {
            if (subtitle.Paragraphs.Count > 0)
            {
                subtitle.Renumber(subtitle.Paragraphs[0].Number);
            }

            Text = Configuration.Settings.Language.MergeDoubleLines.Title;
            labelMaxMillisecondsBetweenLines.Text = Configuration.Settings.Language.MergeDoubleLines.MaxMillisecondsBetweenLines;
            checkBoxIncludeIncrementing.Text = Configuration.Settings.Language.MergeDoubleLines.IncludeIncrementing;
            numericUpDownMaxMillisecondsBetweenLines.Left = labelMaxMillisecondsBetweenLines.Left + labelMaxMillisecondsBetweenLines.Width + 3;
            checkBoxIncludeIncrementing.Left = numericUpDownMaxMillisecondsBetweenLines.Left + numericUpDownMaxMillisecondsBetweenLines.Width + 10;

            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.MergedShortLines.MergedText;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            NumberOfMerges = 0;
            _subtitle = subtitle;
            MergeDoubleLines_ResizeEnd(null, null);
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
            _mergedSubtitle = MergeLinesWithSameTextInSubtitle(_subtitle, mergedIndexes, out var count, true, checkBoxIncludeIncrementing.Checked, true, (int)numericUpDownMaxMillisecondsBetweenLines.Value);
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

        public Subtitle MergeLinesWithSameTextInSubtitle(Subtitle subtitle, List<int> mergedIndexes, out int numberOfMerges, bool clearFixes, bool fixIncrementing, bool lineAfterNext, int maxMsBetween)
        {
            var removed = new List<int>();
            if (!_loading)
            {
                listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            }

            if (clearFixes)
            {
                listViewFixes.Items.Clear();
            }

            numberOfMerges = 0;
            var mergedSubtitle = new Subtitle();
            bool lastMerged = false;
            Paragraph p = null;
            var lineNumbers = new StringBuilder();
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (!lastMerged)
                {
                    p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                    mergedSubtitle.Paragraphs.Add(p);
                }
                Paragraph next = subtitle.GetParagraphOrDefault(i);
                Paragraph afterNext = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null)
                {
                    if ((MergeLinesSameTextUtils.QualifiesForMerge(p, next, maxMsBetween) || (fixIncrementing && MergeLinesSameTextUtils.QualifiesForMergeIncrement(p, next, maxMsBetween))) && IsFixAllowed(p))
                    {
                        p.Text = next.Text;
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
                        removed.Add(i);
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
                    else if (lineAfterNext && MergeLinesSameTextUtils.QualifiesForMerge(p, afterNext, maxMsBetween) && p.Duration.TotalMilliseconds > afterNext.Duration.TotalMilliseconds && IsFixAllowed(p))
                    {
                        removed.Add(i + 2);
                        numberOfMerges++;
                        if (lastMerged)
                        {
                            lineNumbers.Append(afterNext.Number);
                            lineNumbers.Append(',');
                        }
                        else
                        {
                            lineNumbers.Append(p.Number);
                            lineNumbers.Append(',');
                            lineNumbers.Append(afterNext.Number);
                            lineNumbers.Append(',');
                        }
                        lastMerged = true;
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

                if (!removed.Contains(i) && lineNumbers.Length > 0 && clearFixes)
                {
                    AddToListView(p, lineNumbers.ToString(), p.Text);
                    lineNumbers.Clear();
                }
            }
            if (lineNumbers.Length > 0 && clearFixes && p != null)
            {
                AddToListView(p, lineNumbers.ToString(), p.Text);
            }
            if (!lastMerged)
            {
                mergedSubtitle.Paragraphs.Add(new Paragraph(subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1)));
            }

            if (!_loading)
            {
                listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            }

            mergedSubtitle.Renumber();
            return mergedSubtitle;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
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
            if (_loading)
            {
                return;
            }

            var mergedIndexes = new List<int>();

            NumberOfMerges = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            _mergedSubtitle = MergeLinesWithSameTextInSubtitle(_subtitle, mergedIndexes, out var count, false, checkBoxIncludeIncrementing.Checked, true, (int)numericUpDownMaxMillisecondsBetweenLines.Value);
            NumberOfMerges = count;
            SubtitleListview1.Fill(_subtitle);
            foreach (var index in mergedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, Color.Green);
            }
            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.MergedShortLines.NumberOfMergesX, NumberOfMerges);
        }

        private void MergeDoubleLines_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void MergeDoubleLines_Shown(object sender, EventArgs e)
        {
            GeneratePreview();
            listViewFixes.Focus();
            if (listViewFixes.Items.Count > 0)
            {
                listViewFixes.Items[0].Selected = true;
            }

            _loading = false;
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
        }

        private void checkBoxFixIncrementing_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void numericUpDownMaxMillisecondsBetweenLines_ValueChanged(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            _previewTimer.Start();
        }

        private void previewTimer_Tick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void MergeDoubleLines_ResizeEnd(object sender, EventArgs e)
        {
            columnHeaderText.Width = -2;
        }

    }
}
