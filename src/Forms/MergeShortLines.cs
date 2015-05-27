using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class MergeShortLines : PositionAndSizeForm
    {
        private Subtitle _subtitle;
        private Subtitle _mergedSubtitle;

        public int NumberOfMerges { get; private set; }

        public MergeShortLines()
        {
            InitializeComponent();
            Utilities.FixLargeFonts(this, buttonOK);
        }

        public Subtitle MergedSubtitle
        {
            get { return _mergedSubtitle; }
        }

        private void MergeShortLines_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        public void Initialize(Subtitle subtitle)
        {
            if (subtitle.Paragraphs.Count > 0)
                subtitle.Renumber(subtitle.Paragraphs[0].Number);

            Text = Configuration.Settings.Language.MergedShortLines.Title;
            labelMaxCharacters.Text = Configuration.Settings.Language.MergedShortLines.MaximumCharacters;
            labelMaxMillisecondsBetweenLines.Text = Configuration.Settings.Language.MergedShortLines.MaximumMillisecondsBetween;

            checkBoxOnlyContinuationLines.Text = Configuration.Settings.Language.MergedShortLines.OnlyMergeContinuationLines;

            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.MergedShortLines.MergedText;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            NumberOfMerges = 0;
            numericUpDownMaxCharacters.Value = Configuration.Settings.General.SubtitleLineMaximumLength;
            _subtitle = subtitle;
        }

        private void AddToListView(Paragraph p, string lineNumbers, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };

            var subItem = new ListViewItem.ListViewSubItem(item, lineNumbers.TrimEnd(','));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
                return;

            var mergedIndexes = new List<int>();

            NumberOfMerges = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            int count;
            _mergedSubtitle = MergeShortLinesInSubtitle(_subtitle, mergedIndexes, out count, (double)numericUpDownMaxMillisecondsBetweenLines.Value, (int)numericUpDownMaxCharacters.Value, true);
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
                        return item.Checked;
                }
            }
            return true;
        }

        public Subtitle MergeShortLinesInSubtitle(Subtitle subtitle, List<int> mergedIndexes, out int numberOfMerges, double maxMillisecondsBetweenLines, int maxCharacters, bool clearFixes)
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            if (clearFixes)
                listViewFixes.Items.Clear();
            numberOfMerges = 0;
            string language = Utilities.AutoDetectGoogleLanguage(subtitle);
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
                if (next != null)
                {
                    if (QualifiesForMerge(p, next, maxMillisecondsBetweenLines, maxCharacters) && IsFixAllowed(p))
                    {
                        if (GetStartTag(p.Text) == GetStartTag(next.Text) &&
                            GetEndTag(p.Text) == GetEndTag(next.Text))
                        {
                            string s1 = p.Text.Trim();
                            s1 = s1.Substring(0, s1.Length - GetEndTag(s1).Length);
                            string s2 = next.Text.Trim();
                            s2 = s2.Substring(GetStartTag(s2).Length);
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
                            mergedIndexes.Add(i);
                        if (!mergedIndexes.Contains(i - 1))
                            mergedIndexes.Add(i - 1);
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
                    lineNumbers = new StringBuilder();
                }
            }
            if (!lastMerged)
                mergedSubtitle.Paragraphs.Add(new Paragraph(subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1)));

            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            return mergedSubtitle;
        }

        private static string GetEndTag(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            text = text.Trim();
            if (!text.EndsWith('>'))
                return string.Empty;

            string endTag = string.Empty;
            int start = text.LastIndexOf("</", StringComparison.Ordinal);
            if (start > 0 && start >= text.Length - 8)
            {
                endTag = text.Substring(start);
            }
            return endTag;
        }

        private static string GetStartTag(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            text = text.Trim();
            if (!text.StartsWith('<'))
                return string.Empty;

            string startTag = string.Empty;
            int end = text.IndexOf('>');
            if (end > 0 && end < 25)
            {
                startTag = text.Substring(0, end + 1);
            }
            return startTag;
        }

        private bool QualifiesForMerge(Paragraph p, Paragraph next, double maximumMillisecondsBetweenLines, int maximumTotalLength)
        {
            if (p != null && p.Text != null && next != null && next.Text != null)
            {
                string s = HtmlUtil.RemoveHtmlTags(p.Text.Trim());

                if (p.Text.Length + next.Text.Length < maximumTotalLength && next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < maximumMillisecondsBetweenLines)
                {
                    if (string.IsNullOrEmpty(s))
                        return true;
                    bool isLineContinuation = s.EndsWith(',') || s.EndsWith('-') || s.EndsWith("...", StringComparison.Ordinal) || Utilities.AllLettersAndNumbers.Contains(s.Substring(s.Length - 1));

                    if (!checkBoxOnlyContinuationLines.Checked)
                        return true;

                    if (isLineContinuation)
                        return true;
                }
            }
            return false;
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
            _mergedSubtitle = MergeShortLinesInSubtitle(_subtitle, mergedIndexes, out count, (double)numericUpDownMaxMillisecondsBetweenLines.Value, (int)numericUpDownMaxCharacters.Value, false);
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
                listViewFixes.Items[0].Selected = true;
        }

        private void checkBoxOnlyContinuationLines_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }
    }

}
