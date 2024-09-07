using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SplitLongLines : PositionAndSizeForm
    {
        private Subtitle _subtitle;

        public int NumberOfSplits { get; private set; }

        public Subtitle SplitSubtitle { get; private set; }

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

            Text = LanguageSettings.Current.SplitLongLines.Title;
            labelSingleLineMaxLength.Text = LanguageSettings.Current.SplitLongLines.SingleLineMaximumLength;
            labelLineMaxLength.Text = LanguageSettings.Current.SplitLongLines.LineMaximumLength;
            labelLineContinuationBeginEnd.Text = LanguageSettings.Current.SplitLongLines.LineContinuationBeginEndStrings;

            listViewFixes.Columns[0].Text = LanguageSettings.Current.General.Apply;
            listViewFixes.Columns[1].Text = LanguageSettings.Current.General.LineNumber;
            listViewFixes.Columns[2].Text = LanguageSettings.Current.General.Text;

            var continuationProfile = ContinuationUtilities.GetContinuationProfile(Configuration.Settings.General.ContinuationStyle);
            comboBoxLineContinuationBegin.Text = continuationProfile.Prefix;
            comboBoxLineContinuationEnd.Text = continuationProfile.Suffix;

            checkBoxSplitAtLineBreaks.Text = LanguageSettings.Current.SplitLongLines.SplitAtLineBreaks; ;
            toolStripMenuItemInverseSelection.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            SubtitleListview1.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            NumberOfSplits = 0;
            numericUpDownSingleLineMaxCharacters.Value = Configuration.Settings.General.SubtitleLineMaximumLength;
            UiUtil.SetNumericUpDownValue(numericUpDownLineMaxCharacters, Configuration.Settings.Tools.SplitLongLinesMax);
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

            var splitIndexes = new List<int>();
            var autoBreakIndexes = new List<int>();

            NumberOfSplits = 0;
            SubtitleListview1.BeginUpdate();
            SubtitleListview1.Items.Clear();
            if (checkBoxSplitAtLineBreaks.Checked)
            {
                SplitSubtitle = SplitAtLineBreak(_subtitle, splitIndexes, out var count, clearFixes);
                NumberOfSplits = count;
            }
            else
            {
                SplitSubtitle = SplitLongLinesInSubtitle(_subtitle, splitIndexes, autoBreakIndexes, out var count, (int)numericUpDownLineMaxCharacters.Value, (int)numericUpDownSingleLineMaxCharacters.Value, clearFixes);
                NumberOfSplits = count;
            }

            SubtitleListview1.Fill(SplitSubtitle);

            var greenColor = UiUtil.GreenBackgroundColor;
            var greenColorAlternate = UiUtil.GreenBackgroundColorAlternate;

            foreach (var index in splitIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, greenColor);
            }

            foreach (var index in autoBreakIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, greenColorAlternate);
            }

            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.SplitLongLines.NumberOfSplits, NumberOfSplits);
            UpdateLongestLinesInfo(SplitSubtitle);
        }

        private void UpdateLongestLinesInfo(Subtitle subtitle)
        {
            var maxLength = -1;
            var maxLengthIndex = -1;
            var singleLineMaxLength = -1;
            var singleLineMaxLengthIndex = -1;
            var i = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                var s = HtmlUtil.RemoveHtmlTags(p.Text, true);
                if (s.Length > maxLength)
                {
                    maxLength = s.Length;
                    maxLengthIndex = i;
                }

                var arr = s.SplitToLines();
                foreach (var line in arr)
                {
                    if (line.Length > singleLineMaxLengthIndex)
                    {
                        singleLineMaxLength = line.Length;
                        singleLineMaxLengthIndex = i;
                    }
                }
                i++;
            }

            labelMaxSingleLineLengthIs.Text = string.Format(LanguageSettings.Current.SplitLongLines.LongestSingleLineIsXAtY, singleLineMaxLength, singleLineMaxLengthIndex + 1);
            labelMaxSingleLineLengthIs.Tag = singleLineMaxLengthIndex.ToString(CultureInfo.InvariantCulture);
            labelMaxLineLengthIs.Text = string.Format(LanguageSettings.Current.SplitLongLines.LongestLineIsXAtY, maxLength, maxLengthIndex + 1);
            labelMaxLineLengthIs.Tag = maxLengthIndex.ToString(CultureInfo.InvariantCulture);
        }

        private bool IsFixAllowed(Paragraph p)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Tag as Paragraph == p)
                {
                    return item.Checked;
                }
            }

            return true;
        }

        private Subtitle SplitAtLineBreak(Subtitle subtitle, List<int> splitIndexes, out int numberOfSplits, bool clearFixes)
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            if (clearFixes)
            {
                listViewFixes.Items.Clear();
            }

            numberOfSplits = 0;
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var splitSubtitle = new Subtitle();
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var added = false;
                var p = subtitle.Paragraphs[i];
                if (p?.Text != null)
                {
                    var text = p.Text.Trim();
                    if (text.Contains(Environment.NewLine) && IsFixAllowed(p))
                    {
                        var oldText = HtmlUtil.RemoveHtmlTags(p.Text);

                        var arr = text.SplitToLines();
                        if (arr.Count == 2)
                        {
                            var spacing1 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                            var spacing2 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                            if (Configuration.Settings.General.MinimumMillisecondsBetweenLines % 2 == 1)
                            {
                                spacing2++;
                            }

                            var newParagraph1 = new Paragraph(p);
                            var newParagraph2 = new Paragraph(p);
                            newParagraph1.Text = Utilities.AutoBreakLine(arr[0], language);

                            var middle = p.StartTime.TotalMilliseconds + p.DurationTotalMilliseconds / 2;
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

                                middle = p.StartTime.TotalMilliseconds + p.DurationTotalMilliseconds * startFactor;
                            }

                            newParagraph1.EndTime.TotalMilliseconds = middle - spacing1;
                            newParagraph2.Text = Utilities.AutoBreakLine(arr[1], language);
                            newParagraph2.StartTime.TotalMilliseconds = newParagraph1.EndTime.TotalMilliseconds + spacing2;

                            if (clearFixes)
                            {
                                AddToListView(p, (splitSubtitle.Paragraphs.Count + 1).ToString(CultureInfo.InvariantCulture), oldText);
                            }

                            splitIndexes.Add(splitSubtitle.Paragraphs.Count);
                            splitIndexes.Add(splitSubtitle.Paragraphs.Count + 1);

                            var p1 = HtmlUtil.RemoveHtmlTags(newParagraph1.Text).TrimEnd();
                            var post = string.Empty;
                            if (!p1.EndsWith('.') && !p1.EndsWith('!') && !p1.EndsWith('?') && !p1.EndsWith(':') && !p1.EndsWith(')') && !p1.EndsWith(']') && !p1.EndsWith('♪'))
                            {
                                var endsWithComma = newParagraph1.Text.EndsWith(',') || newParagraph1.Text.EndsWith(",</i>", StringComparison.Ordinal);

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

                                var pre = string.Empty;
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

                            var isDialog = new DialogSplitMerge().IsDialog(new List<string>() { newParagraph1.Text, newParagraph2.Text }, p, subtitle.GetParagraphOrDefault(i - 1));
                            if (isDialog)
                            {
                                if (newParagraph1.Text.StartsWith("<i>-"))
                                {
                                    newParagraph1.Text = "<i>" + newParagraph1.Text.Remove(0, 4).TrimStart();
                                }

                                if (newParagraph2.Text.StartsWith("<i>-"))
                                {
                                    newParagraph2.Text = "<i>" + newParagraph2.Text.Remove(0, 4).TrimStart();
                                }

                                newParagraph1.Text = newParagraph1.Text.TrimStart('-').TrimStart();
                                newParagraph2.Text = newParagraph2.Text.TrimStart('-').TrimStart();
                            }

                            splitSubtitle.Paragraphs.Add(newParagraph1);
                            splitSubtitle.Paragraphs.Add(newParagraph2);
                            added = true;
                            numberOfSplits++;
                        }
                        else
                        {
                            var durationMs = p.DurationTotalMilliseconds / arr.Count;
                            for (var index = 0; index < arr.Count; index++)
                            {
                                var line = arr[index];
                                var newParagraph = new Paragraph();
                                newParagraph.Text = line;
                                newParagraph.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + durationMs * index;
                                newParagraph.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + durationMs * (index + 1);
                                splitSubtitle.Paragraphs.Add(newParagraph);
                                added = true;
                                numberOfSplits++;

                                if (index < arr.Count - 1)
                                {
                                    var minGap = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                                    if (minGap > 0 &&
                                        newParagraph.DurationTotalMilliseconds - minGap > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                                    {
                                        newParagraph.EndTime.TotalMilliseconds -= minGap;
                                    }
                                }
                            }
                        }
                    }

                    if (!added)
                    {
                        splitSubtitle.Paragraphs.Add(new Paragraph(p));
                    }
                }
            }

            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            splitSubtitle.Renumber();
            return splitSubtitle;
        }


        public Subtitle SplitLongLinesInSubtitle(Subtitle subtitle, List<int> splitIndexes, List<int> autoBreakIndexes, out int numberOfSplits, int totalLineMaxCharacters, int singleLineMaxCharacters, bool clearFixes)
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            if (clearFixes)
            {
                listViewFixes.Items.Clear();
            }

            numberOfSplits = 0;
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var splitSubtitle = new Subtitle();
            string[] expectedPunctuations = { ". -", "! -", "? -" };
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var added = false;
                var p = subtitle.Paragraphs[i];
                if (p?.Text != null)
                {
                    if (SplitLongLinesHelper.QualifiesForSplit(p.Text, singleLineMaxCharacters, totalLineMaxCharacters) && IsFixAllowed(p))
                    {
                        var oldText = HtmlUtil.RemoveHtmlTags(p.Text);
                        var isDialog = false;
                        var dialogText = string.Empty;
                        if (p.Text.Contains('-'))
                        {
                            dialogText = Utilities.AutoBreakLine(p.Text, 5, 1, language);

                            var tempText = p.Text.Replace(Environment.NewLine, " ").Replace("  ", " ");
                            if (Utilities.CountTagInText(tempText, '-') == 2 && (p.Text.StartsWith('-') || p.Text.StartsWith("<i>-", StringComparison.Ordinal)))
                            {
                                var idx = tempText.IndexOfAny(expectedPunctuations, StringComparison.Ordinal);
                                if (idx > 1)
                                {
                                    dialogText = tempText.Remove(idx + 1, 1).Insert(idx + 1, Environment.NewLine);
                                }
                            }

                            var dialogHelper = new DialogSplitMerge();
                            if (dialogHelper.IsDialog(dialogText.SplitToLines()))
                            {
                                isDialog = true;
                            }
                        }

                        if (!isDialog && !SplitLongLinesHelper.QualifiesForSplit(Utilities.AutoBreakLine(p.Text, language), singleLineMaxCharacters, totalLineMaxCharacters))
                        {
                            var newParagraph = new Paragraph(p) { Text = Utilities.AutoBreakLine(p.Text, language) };
                            if (clearFixes)
                            {
                                AddToListView(p, (splitSubtitle.Paragraphs.Count + 1).ToString(CultureInfo.InvariantCulture), oldText);
                            }

                            autoBreakIndexes.Add(splitSubtitle.Paragraphs.Count);
                            splitSubtitle.Paragraphs.Add(newParagraph);
                            added = true;
                            numberOfSplits++;
                        }
                        else
                        {
                            var text = Utilities.AutoBreakLine(p.Text, language);
                            if (isDialog)
                            {
                                text = dialogText;
                            }

                            if (isDialog || text.Contains(Environment.NewLine))
                            {
                                var arr = text.SplitToLines();
                                if (arr.Count == 2)
                                {
                                    var spacing1 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                                    var spacing2 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                                    if (Configuration.Settings.General.MinimumMillisecondsBetweenLines % 2 == 1)
                                    {
                                        spacing2++;
                                    }

                                    var newParagraph1 = new Paragraph(p);
                                    var newParagraph2 = new Paragraph(p);
                                    newParagraph1.Text = Utilities.AutoBreakLine(arr[0], language);

                                    var middle = p.StartTime.TotalMilliseconds + p.DurationTotalMilliseconds / 2;
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

                                        middle = p.StartTime.TotalMilliseconds + p.DurationTotalMilliseconds * startFactor;
                                    }

                                    newParagraph1.EndTime.TotalMilliseconds = middle - spacing1;
                                    newParagraph2.Text = Utilities.AutoBreakLine(arr[1], language);
                                    newParagraph2.StartTime.TotalMilliseconds = newParagraph1.EndTime.TotalMilliseconds + spacing2;

                                    if (Configuration.Settings.General.SplitRemovesDashes && isDialog)
                                    {
                                        newParagraph1.Text = DialogSplitMerge.RemoveStartDash(newParagraph1.Text);
                                        newParagraph2.Text = DialogSplitMerge.RemoveStartDash(newParagraph2.Text);
                                    }

                                    if (clearFixes)
                                    {
                                        AddToListView(p, (splitSubtitle.Paragraphs.Count + 1).ToString(CultureInfo.InvariantCulture), oldText);
                                    }

                                    splitIndexes.Add(splitSubtitle.Paragraphs.Count);
                                    splitIndexes.Add(splitSubtitle.Paragraphs.Count + 1);

                                    var p1 = HtmlUtil.RemoveHtmlTags(newParagraph1.Text).TrimEnd();
                                    if (!p1.EndsWith('.') && !p1.EndsWith('!') && !p1.EndsWith('?') && !p1.EndsWith(':') && !p1.EndsWith(')') && !p1.EndsWith(']') && !p1.EndsWith('♪'))
                                    {
                                        var endsWithComma = newParagraph1.Text.EndsWith(',') || newParagraph1.Text.EndsWith(",</i>", StringComparison.Ordinal);

                                        var post = string.Empty;
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

                                        var pre = string.Empty;
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

                                    splitSubtitle.Paragraphs.Add(newParagraph1);
                                    splitSubtitle.Paragraphs.Add(newParagraph2);
                                    added = true;
                                    numberOfSplits++;
                                }
                            }
                            else if ((language == "ja" || language == "zh") && !p.Text.Contains(Environment.NewLine))
                            {
                                var splitChars = ".!?:;。、；·！…";
                                var splitPos = (int)Math.Round(p.Text.Length / 2.0 + 0.5);
                                if (p.Text.Length > 12)
                                {
                                    for (var j = 0; j < 5; j++)
                                    {
                                        if (splitChars.Contains(p.Text[splitPos - j]))
                                        {
                                            splitPos = splitPos - j;
                                            break;
                                        }

                                        if (splitChars.Contains(p.Text[splitPos + j]))
                                        {
                                            splitPos = splitPos + j;
                                            break;
                                        }
                                    }
                                }

                                var spacing1 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                                var spacing2 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                                if (Configuration.Settings.General.MinimumMillisecondsBetweenLines % 2 == 1)
                                {
                                    spacing2++;
                                }

                                var newParagraph1 = new Paragraph(p);
                                var newParagraph2 = new Paragraph(p);

                                newParagraph1.Text = Utilities.AutoBreakLine(p.Text.Substring(0, splitPos + 1), language);

                                var middle = p.StartTime.TotalMilliseconds + p.DurationTotalMilliseconds / 2;
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

                                    middle = p.StartTime.TotalMilliseconds + p.DurationTotalMilliseconds * startFactor;
                                }

                                newParagraph1.EndTime.TotalMilliseconds = middle - spacing1;
                                newParagraph2.Text = Utilities.AutoBreakLine(p.Text.Substring(splitPos + 1), language);
                                newParagraph2.StartTime.TotalMilliseconds = newParagraph1.EndTime.TotalMilliseconds + spacing2;

                                if (Configuration.Settings.General.SplitRemovesDashes && isDialog)
                                {
                                    newParagraph1.Text = DialogSplitMerge.RemoveStartDash(newParagraph1.Text);
                                    newParagraph2.Text = DialogSplitMerge.RemoveStartDash(newParagraph2.Text);
                                }

                                if (clearFixes)
                                {
                                    AddToListView(p, (splitSubtitle.Paragraphs.Count + 1).ToString(CultureInfo.InvariantCulture), oldText);
                                }

                                splitIndexes.Add(splitSubtitle.Paragraphs.Count);
                                splitIndexes.Add(splitSubtitle.Paragraphs.Count + 1);

                                var p1 = HtmlUtil.RemoveHtmlTags(newParagraph1.Text).TrimEnd();
                                if (!p1.EndsWith('.') && !p1.EndsWith('!') && !p1.EndsWith('?') && !p1.EndsWith(':') && !p1.EndsWith(')') && !p1.EndsWith(']') && !p1.EndsWith('♪'))
                                {
                                    var endsWithComma = newParagraph1.Text.EndsWith(',') || newParagraph1.Text.EndsWith(",</i>", StringComparison.Ordinal);

                                    var post = string.Empty;
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

                                    var pre = string.Empty;
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

                                splitSubtitle.Paragraphs.Add(newParagraph1);
                                splitSubtitle.Paragraphs.Add(newParagraph2);
                                added = true;
                                numberOfSplits++;
                            }
                        }
                    }

                    if (!added)
                    {
                        splitSubtitle.Paragraphs.Add(new Paragraph(p));
                    }
                }
            }

            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            splitSubtitle.Renumber();
            return splitSubtitle;
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
                var index = listViewFixes.SelectedIndices[0];
                ListViewItem item = listViewFixes.Items[index];
                index = int.Parse(item.SubItems[1].Text) - 1;
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
            }
        }

        private void SplitLongLines_ResizeEnd(object sender, EventArgs e)
        {
            listViewFixes.AutoSizeLastColumn();
        }

        private void SplitLongLines_Shown(object sender, EventArgs e)
        {
            SplitLongLines_ResizeEnd(sender, e);

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
            var index = int.Parse(labelMaxSingleLineLengthIs.Tag.ToString());
            SubtitleListview1.SelectIndexAndEnsureVisible(index);
        }

        private void labelMaxLineLengthIs_Click(object sender, EventArgs e)
        {
            var index = int.Parse(labelMaxLineLengthIs.Tag.ToString());
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

        private void toolStripMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            listViewFixes.CheckAll();
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            GeneratePreview(false);
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            listViewFixes.InvertCheck();
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            GeneratePreview(false);
        }

        private void checkBoxSplitAtLineBreaks_CheckedChanged(object sender, EventArgs e)
        {
            var splitAtLineBreaks = checkBoxSplitAtLineBreaks.Checked;
            numericUpDownSingleLineMaxCharacters.Enabled = !splitAtLineBreaks;
            numericUpDownLineMaxCharacters.Enabled = !splitAtLineBreaks;
            Cursor = Cursors.WaitCursor;
            GeneratePreview(true);
            Cursor = Cursors.Default;
        }

        private void SplitLongLines_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.SplitLongLinesMax = (int)numericUpDownLineMaxCharacters.Value;
        }
    }
}
