using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class MergeDoubleLines : PositionAndSizeForm
    {
        private Subtitle _subtitle;
        private bool _loading = true;
        private readonly Timer _previewTimer = new Timer();

        public int NumberOfMerges { get; private set; }
        public Subtitle MergedSubtitle { get; private set; }

        public MergeDoubleLines()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _previewTimer.Tick += previewTimer_Tick;
            _previewTimer.Interval = 250;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(Subtitle subtitle)
        {
            if (subtitle.Paragraphs.Count > 0)
            {
                subtitle.Renumber(subtitle.Paragraphs[0].Number);
            }

            Text = LanguageSettings.Current.MergeDoubleLines.Title;
            labelMaxMillisecondsBetweenLines.Text = LanguageSettings.Current.MergeDoubleLines.MaxMillisecondsBetweenLines;
            checkBoxIncludeIncrementing.Text = LanguageSettings.Current.MergeDoubleLines.IncludeIncrementing;
            numericUpDownMaxMillisecondsBetweenLines.Left = labelMaxMillisecondsBetweenLines.Left + labelMaxMillisecondsBetweenLines.Width + 3;
            checkBoxIncludeIncrementing.Left = numericUpDownMaxMillisecondsBetweenLines.Left + numericUpDownMaxMillisecondsBetweenLines.Width + 10;

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

            NumberOfMerges = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            MergedSubtitle = MergeLinesWithSameTextInSubtitle(_subtitle, out var count, true, checkBoxIncludeIncrementing.Checked, true, (int)numericUpDownMaxMillisecondsBetweenLines.Value);
            NumberOfMerges = count;

            SubtitleListview1.Fill(_subtitle);

            UpdateBackgroundColor();

            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MergedShortLines.NumberOfMergesX, NumberOfMerges);
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

        public Subtitle MergeLinesWithSameTextInSubtitle(Subtitle subtitle, out int numberOfMerges, bool clearFixes, bool fixIncrementing, bool lineAfterNext, int maxMsBetween)
        {
            var mergedIndexes = new List<int>();
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
            Paragraph p = null;
            var lineNumbers = new StringBuilder();
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (removed.Contains(i - 1))
                {
                    continue;
                }

                p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                mergedSubtitle.Paragraphs.Add(p);

                for (int j = i; j < subtitle.Paragraphs.Count; j++)
                {
                    if (removed.Contains(j))
                    {
                        continue;
                    }

                    var next = subtitle.GetParagraphOrDefault(j);
                    var incrementText = string.Empty;
                    if ((MergeLinesSameTextUtils.QualifiesForMerge(p, next, maxMsBetween) || fixIncrementing && MergeLinesSameTextUtils.QualifiesForMergeIncrement(p, next, maxMsBetween, out incrementText)) && IsFixAllowed(p))
                    {
                        p.Text = next.Text;
                        if (!string.IsNullOrEmpty(incrementText))
                        {
                            p.Text = incrementText;
                        }

                        p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                        if (lineNumbers.Length > 0)
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

                        removed.Add(j);
                        numberOfMerges++;
                        if (!mergedIndexes.Contains(j))
                        {
                            mergedIndexes.Add(j);
                        }

                        if (!mergedIndexes.Contains(i - 1))
                        {
                            mergedIndexes.Add(i - 1);
                        }
                    }
                }

                if (!removed.Contains(i - 1) && lineNumbers.Length > 0 && clearFixes)
                {
                    AddToListView(p, lineNumbers.ToString(), p.Text);
                    lineNumbers.Clear();
                }
            }

            if (lineNumbers.Length > 0 && clearFixes && p != null)
            {
                AddToListView(p, lineNumbers.ToString(), p.Text);
            }

            if (!mergedIndexes.Contains(subtitle.Paragraphs.Count - 1))
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
                var numbers = item.SubItems[1].Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string number in numbers)
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
            if (_loading)
            {
                return;
            }

            NumberOfMerges = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            MergedSubtitle = MergeLinesWithSameTextInSubtitle(_subtitle, out var count, false, checkBoxIncludeIncrementing.Checked, true, (int)numericUpDownMaxMillisecondsBetweenLines.Value);
            NumberOfMerges = count;
            SubtitleListview1.Fill(_subtitle);
            UpdateBackgroundColor();
            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MergedShortLines.NumberOfMergesX, NumberOfMerges);
        }

        private void UpdateBackgroundColor()
        {
            var colors = new List<Color>
            {
                Color.Green,
                Color.LimeGreen,
                Color.GreenYellow,
            };
            int colorIdx = 0;

            for (int i = 0; i < listViewFixes.Items.Count; i++)
            {
                ListViewItem item = listViewFixes.Items[i];
                var lineNumbers = item.SubItems[1].Text;
                if (item.Checked && !string.IsNullOrEmpty(lineNumbers))
                {
                    var numbers = lineNumbers.Split(',');
                    foreach (var number in numbers)
                    {
                        SubtitleListview1.SetBackgroundColor(Convert.ToInt32(number) - 1, colors[colorIdx]);
                    }

                    colorIdx++;
                    if (colorIdx >= colors.Count)
                    {
                        colorIdx = 0;
                    }
                }
            }
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
