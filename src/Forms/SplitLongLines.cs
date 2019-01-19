using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SplitLongLines : PositionAndSizeForm
    {
        private Subtitle _subtitle;
        private Subtitle _splittedSubtitle;

        public int NumberOfSplits { get; private set; }

        public Subtitle SplittedSubtitle => _splittedSubtitle;

        public SplitLongLines()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void SplitLongLines_KeyDown(object sender, KeyEventArgs e)
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
                subtitle.Renumber(subtitle.Paragraphs[0].Number);
            }

            Text = Configuration.Settings.Language.SplitLongLines.Title;
            labelSingleLineMaxLength.Text = Configuration.Settings.Language.SplitLongLines.SingleLineMaximumLength;
            labelLineMaxLength.Text = Configuration.Settings.Language.SplitLongLines.LineMaximumLength;
            labelLineContinuationBeginEnd.Text = Configuration.Settings.Language.SplitLongLines.LineContinuationBeginEndStrings;

            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Text;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            NumberOfSplits = 0;
            numericUpDownSingleLineMaxCharacters.Value = Configuration.Settings.General.SubtitleLineMaximumLength;
            _subtitle = subtitle;
        }

        private void AddToListView(Paragraph p, string lineNumbers, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            item.SubItems.Add(lineNumbers);
            item.SubItems.Add(UiUtil.GetListViewTextFromString(newText));
            listViewFixes.Items.Add(item);
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            GeneratePreview(false);
        }

        private void GeneratePreview(bool clearFixes)
        {
            if (_subtitle == null)
            {
                return;
            }

            var splittedIndexes = new List<int>();
            var autoBreakedIndexes = new List<int>();

            NumberOfSplits = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            _splittedSubtitle = SplitLongLinesInSubtitle(_subtitle, splittedIndexes, autoBreakedIndexes, out var count, (int)numericUpDownLineMaxCharacters.Value, (int)numericUpDownSingleLineMaxCharacters.Value, clearFixes);
            NumberOfSplits = count;

            SubtitleListview1.Fill(_splittedSubtitle);

            foreach (var index in splittedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, Color.Green);
            }

            foreach (var index in autoBreakedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, Color.LightGreen);
            }

            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.SplitLongLines.NumberOfSplits, NumberOfSplits);
            UpdateLongestLinesInfo(_splittedSubtitle);
        }

        private void UpdateLongestLinesInfo(Subtitle subtitle)
        {
            int maxLength = -1;
            int maxLengthIndex = -1;
            int singleLineMaxLength = -1;
            int singleLineMaxLengthIndex = -1;
            int i = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string s = HtmlUtil.RemoveHtmlTags(p.Text, true);
                if (s.Length > maxLength)
                {
                    maxLength = s.Length;
                    maxLengthIndex = i;
                }
                var arr = s.SplitToLines();
                foreach (string line in arr)
                {
                    if (line.Length > singleLineMaxLengthIndex)
                    {
                        singleLineMaxLength = line.Length;
                        singleLineMaxLengthIndex = i;
                    }
                }
                i++;
            }
            labelMaxSingleLineLengthIs.Text = string.Format(Configuration.Settings.Language.SplitLongLines.LongestSingleLineIsXAtY, singleLineMaxLength, singleLineMaxLengthIndex + 1);
            labelMaxSingleLineLengthIs.Tag = singleLineMaxLengthIndex.ToString(CultureInfo.InvariantCulture);
            labelMaxLineLengthIs.Text = string.Format(Configuration.Settings.Language.SplitLongLines.LongestLineIsXAtY, maxLength, maxLengthIndex + 1);
            labelMaxLineLengthIs.Tag = maxLengthIndex.ToString(CultureInfo.InvariantCulture);
        }

        private bool IsFixAllowed(Paragraph p)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if ((item.Tag as Paragraph) == p)
                {
                    return item.Checked;
                }
            }
            return true;
        }

        public Subtitle SplitLongLinesInSubtitle(Subtitle subtitle, List<int> splittedIndexes, List<int> autoBreakedIndexes, out int numberOfSplits, int totalLineMaxCharacters, int singleLineMaxCharacters, bool clearFixes)
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            if (clearFixes)
            {
                listViewFixes.Items.Clear();
            }

            numberOfSplits = 0;
            string language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var splittedSubtitle = new Subtitle();
            string[] expectedPunctuations = { ". -", "! -", "? -" };
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                bool added = false;
                var p = subtitle.Paragraphs[i];
                if (p?.Text != null)
                {
                    if (SplitLongLinesHelper.QualifiesForSplit(p.Text, singleLineMaxCharacters, totalLineMaxCharacters) && IsFixAllowed(p))
                    {
                        string oldText = HtmlUtil.RemoveHtmlTags(p.Text);
                        bool isDialog = false;
                        string dialogText = string.Empty;
                        if (p.Text.Contains('-'))
                        {
                            dialogText = Utilities.AutoBreakLine(p.Text, 5, 1, language);

                            var tempText = p.Text.Replace(Environment.NewLine, " ").Replace("  ", " ");
                            if (Utilities.CountTagInText(tempText, '-') == 2 && (p.Text.StartsWith('-') || p.Text.StartsWith("<i>-", StringComparison.Ordinal)))
                            {
                                int idx = tempText.IndexOfAny(expectedPunctuations, StringComparison.Ordinal);
                                if (idx > 1)
                                {
                                    dialogText = tempText.Remove(idx + 1, 1).Insert(idx + 1, Environment.NewLine);
                                }
                            }

                            var arr = dialogText.SplitToLines();
                            if (arr.Count == 2 && (arr[0].StartsWith('-') || arr[0].StartsWith("<i>-", StringComparison.Ordinal)) && (arr[1].StartsWith('-') || arr[1].StartsWith("<i>-", StringComparison.Ordinal)))
                            {
                                isDialog = true;
                            }
                        }

                        if (!isDialog && !SplitLongLinesHelper.QualifiesForSplit(Utilities.AutoBreakLine(p.Text, language), singleLineMaxCharacters, totalLineMaxCharacters))
                        {
                            var newParagraph = new Paragraph(p) { Text = Utilities.AutoBreakLine(p.Text, language) };
                            if (clearFixes)
                            {
                                AddToListView(p, (splittedSubtitle.Paragraphs.Count + 1).ToString(CultureInfo.InvariantCulture), oldText);
                            }

                            autoBreakedIndexes.Add(splittedSubtitle.Paragraphs.Count);
                            splittedSubtitle.Paragraphs.Add(newParagraph);
                            added = true;
                            numberOfSplits++;
                        }
                        else
                        {
                            string text = Utilities.AutoBreakLine(p.Text, language);
                            if (isDialog)
                            {
                                text = dialogText;
                            }

                            if (isDialog || text.Contains(Environment.NewLine))
                            {
                                var arr = text.SplitToLines();
                                if (arr.Count == 2)
                                {
                                    int spacing1 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                                    int spacing2 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                                    if (Configuration.Settings.General.MinimumMillisecondsBetweenLines % 2 == 1)
                                    {
                                        spacing2++;
                                    }

                                    var newParagraph1 = new Paragraph(p);
                                    var newParagraph2 = new Paragraph(p);
                                    newParagraph1.Text = Utilities.AutoBreakLine(arr[0], language);

                                    double middle = p.StartTime.TotalMilliseconds + (p.Duration.TotalMilliseconds / 2);
                                    if (!string.IsNullOrWhiteSpace(oldText))
                                    {
                                        var startFactor = (double)HtmlUtil.RemoveHtmlTags(newParagraph1.Text).Length / oldText.Length;
                                        if (startFactor < 0.25)
                                        {
                                            startFactor = 0.25;
                                        }

                                        if (startFactor > 0.75)
                                        {
                                            startFactor = 0.75;
                                        }

                                        middle = p.StartTime.TotalMilliseconds + (p.Duration.TotalMilliseconds * startFactor);
                                    }

                                    newParagraph1.EndTime.TotalMilliseconds = middle - spacing1;
                                    newParagraph2.Text = Utilities.AutoBreakLine(arr[1], language);
                                    newParagraph2.StartTime.TotalMilliseconds = newParagraph1.EndTime.TotalMilliseconds + spacing2;

                                    if (clearFixes)
                                    {
                                        AddToListView(p, (splittedSubtitle.Paragraphs.Count + 1).ToString(CultureInfo.InvariantCulture), oldText);
                                    }

                                    splittedIndexes.Add(splittedSubtitle.Paragraphs.Count);
                                    splittedIndexes.Add(splittedSubtitle.Paragraphs.Count + 1);

                                    string p1 = HtmlUtil.RemoveHtmlTags(newParagraph1.Text).TrimEnd();
                                    if (p1.EndsWith('.') || p1.EndsWith('!') || p1.EndsWith('?') || p1.EndsWith(':') || p1.EndsWith(')') || p1.EndsWith(']') || p1.EndsWith('♪'))
                                    {
                                        if (newParagraph1.Text.StartsWith('-') && newParagraph2.Text.StartsWith('-'))
                                        {
                                            newParagraph1.Text = newParagraph1.Text.Remove(0, 1).Trim();
                                            newParagraph2.Text = newParagraph2.Text.Remove(0, 1).Trim();
                                        }
                                        else if (newParagraph1.Text.StartsWith("<i>-", StringComparison.Ordinal) && newParagraph2.Text.StartsWith('-'))
                                        {
                                            newParagraph1.Text = newParagraph1.Text.Remove(3, 1).Trim();
                                            if (newParagraph1.Text.StartsWith("<i> ", StringComparison.Ordinal))
                                            {
                                                newParagraph1.Text = newParagraph1.Text.Remove(3, 1).Trim();
                                            }

                                            newParagraph2.Text = newParagraph2.Text.Remove(0, 1).Trim();
                                        }
                                    }
                                    else
                                    {
                                        bool endsWithComma = newParagraph1.Text.EndsWith(',') || newParagraph1.Text.EndsWith(",</i>", StringComparison.Ordinal);

                                        string post = string.Empty;
                                        if (newParagraph1.Text.EndsWith("</i>", StringComparison.Ordinal))
                                        {
                                            post = "</i>";
                                            newParagraph1.Text = newParagraph1.Text.Remove(newParagraph1.Text.Length - post.Length);
                                        }
                                        if (endsWithComma)
                                        {
                                            newParagraph1.Text += post;
                                        }
                                        else
                                        {
                                            newParagraph1.Text += comboBoxLineContinuationEnd.Text.TrimEnd() + post;
                                        }

                                        string pre = string.Empty;
                                        if (newParagraph2.Text.StartsWith("<i>", StringComparison.Ordinal))
                                        {
                                            pre = "<i>";
                                            newParagraph2.Text = newParagraph2.Text.Remove(0, pre.Length);
                                        }
                                        if (endsWithComma)
                                        {
                                            newParagraph2.Text = pre + newParagraph2.Text;
                                        }
                                        else
                                        {
                                            newParagraph2.Text = pre + comboBoxLineContinuationBegin.Text + newParagraph2.Text;
                                        }
                                    }

                                    var italicStart1 = newParagraph1.Text.IndexOf("<i>", StringComparison.Ordinal);
                                    if (italicStart1 >= 0 && italicStart1 < 10 && newParagraph1.Text.IndexOf("</i>", StringComparison.Ordinal) < 0 &&
                                        newParagraph2.Text.Contains("</i>") && newParagraph2.Text.IndexOf("<i>", StringComparison.Ordinal) < 0)
                                    {
                                        newParagraph1.Text += "</i>";
                                        newParagraph2.Text = "<i>" + newParagraph2.Text;
                                    }

                                    splittedSubtitle.Paragraphs.Add(newParagraph1);
                                    splittedSubtitle.Paragraphs.Add(newParagraph2);
                                    added = true;
                                    numberOfSplits++;
                                }
                            }
                        }
                    }
                    if (!added)
                    {
                        splittedSubtitle.Paragraphs.Add(new Paragraph(p));
                    }
                }
            }
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            splittedSubtitle.Renumber();
            return splittedSubtitle;
        }

        private void NumericUpDownMaxCharactersValueChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview(true);
            Cursor = Cursors.Default;
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedIndices.Count > 0)
            {
                int index = listViewFixes.SelectedIndices[0];
                ListViewItem item = listViewFixes.Items[index];
                index = int.Parse(item.SubItems[1].Text) - 1;
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
            }
        }

        private void SplitLongLines_Shown(object sender, EventArgs e)
        {
            GeneratePreview(true);
            listViewFixes.Focus();
            if (listViewFixes.Items.Count > 0)
            {
                listViewFixes.Items[0].Selected = true;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void labelMaxSingleLineLengthIs_Click(object sender, EventArgs e)
        {
            int index = int.Parse(labelMaxSingleLineLengthIs.Tag.ToString());
            SubtitleListview1.SelectIndexAndEnsureVisible(index);
        }

        private void labelMaxLineLengthIs_Click(object sender, EventArgs e)
        {
            int index = int.Parse(labelMaxLineLengthIs.Tag.ToString());
            SubtitleListview1.SelectIndexAndEnsureVisible(index);
        }

        private void ContinuationBeginEndChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (listViewFixes.SelectedItems.Count > 0)
            {
                var index = listViewFixes.SelectedItems[0].Index;
                listViewFixes.Items[index].Selected = true;
            }
            GeneratePreview(true);
            Cursor = Cursors.Default;
        }

    }
}
