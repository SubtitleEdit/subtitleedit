using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class MergeDoubleLines : PositionAndSizeForm
    {
        private Subtitle _subtitle;
        private bool _loading = true;
        private readonly Timer _previewTimer = new Timer();
        private List<FixListItem> _fixItems;

        public class FixListItem
        {
            public List<int> LineNumbers { get; set; }
            public bool Checked { get; set; }

            public FixListItem()
            {
                LineNumbers = new List<int>();
            }
        }

        public int NumberOfMerges { get; private set; }
        public Subtitle MergedSubtitle { get; private set; }

        public MergeDoubleLines()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _fixItems = new List<FixListItem>();
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
        }

        private ListViewItem MakeListViewItem(Paragraph p, List<int> lineNumbers, string newText)
        {
            var fixItem = new FixListItem { Checked = true };
            fixItem.LineNumbers.AddRange(lineNumbers);
            _fixItems.Add(fixItem);

            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            item.SubItems.Add(string.Join(",", lineNumbers));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(newText));
            return item;
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
            foreach (var fixItem in _fixItems)
            {
                foreach (var lineNumber in fixItem.LineNumbers)
                {
                    if (p.Number == lineNumber)
                    {
                        return fixItem.Checked;
                    }
                }
            }

            return true;
        }

        public Subtitle MergeLinesWithSameTextInSubtitle(Subtitle subtitle, out int numberOfMerges, bool clearFixes, bool fixIncrementing, bool lineAfterNext, int maxMsBetween)
        {
            _fixItems = new List<FixListItem>();
            var mergedIndexes = new List<int>();
            var removed = new HashSet<int>();
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
            var lineNumbers = new List<int>();
            var listViewItems = new List<ListViewItem>();
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
                        if (lineNumbers.Count > 0)
                        {
                            lineNumbers.Add(next.Number);
                        }
                        else
                        {
                            lineNumbers.Add(p.Number);
                            lineNumbers.Add(next.Number);
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

                if (!removed.Contains(i - 1) && lineNumbers.Count > 0 && clearFixes)
                {
                    listViewItems.Add(MakeListViewItem(p, lineNumbers, p.Text));
                    lineNumbers.Clear();
                }
            }

            if (lineNumbers.Count > 0 && clearFixes && p != null)
            {
                listViewItems.Add(MakeListViewItem(p, lineNumbers, p.Text));
            }

            listViewFixes.Items.AddRange(listViewItems.ToArray());

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
            if (listViewFixes.SelectedIndices.Count <= 0)
            {
                return;
            }

            int index = listViewFixes.SelectedIndices[0];
            foreach (var number in _fixItems[index].LineNumbers)
            {
                foreach (var p in _subtitle.Paragraphs)
                {
                    if (p.Number == number)
                    {
                        index = _subtitle.GetIndex(p);
                        SubtitleListview1.EnsureVisible(index);
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

            _fixItems[e.Item.Index].Checked = e.Item.Checked;
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
            int colorIdx = 0;
            var colors = new List<Color>
            {
                Color.Green,
                Color.LimeGreen,
                Color.GreenYellow,
            };

            foreach (var fixItem in _fixItems.Where(p => p.Checked))
            {
                foreach (var number in fixItem.LineNumbers)
                {
                    SubtitleListview1.SetBackgroundColor(number - 1, colors[colorIdx]);
                }

                colorIdx++;
                if (colorIdx >= colors.Count)
                {
                    colorIdx = 0;
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
            MergeDoubleLines_ResizeEnd(sender, e);

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
                _fixItems[item.Index].Checked = true;
                item.Checked = true;
            }
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                _fixItems[item.Index].Checked = !item.Checked;
                item.Checked = !item.Checked;
            }
        }
    }
}
