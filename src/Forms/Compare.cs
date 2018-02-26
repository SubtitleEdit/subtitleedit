﻿using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Compare : Form
    {
        private Subtitle _subtitle1;
        private Subtitle _subtitle2;
        private List<int> _differences;
        private readonly Keys _mainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
        private string _language1;
        private readonly Color _backDifferenceColor = Color.FromArgb(255, 90, 90);
        private readonly Color _foregroundDifferenceColor = Color.FromArgb(225, 0, 0);
        private bool _loadingConfig = true;

        public Compare()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelSubtitle2.Text = string.Empty;
            Text = Configuration.Settings.Language.CompareSubtitles.Title;
            buttonPreviousDifference.Text = Configuration.Settings.Language.CompareSubtitles.PreviousDifference;
            buttonNextDifference.Text = Configuration.Settings.Language.CompareSubtitles.NextDifference;
            checkBoxShowOnlyDifferences.Text = Configuration.Settings.Language.CompareSubtitles.ShowOnlyDifferences;
            checkBoxIgnoreLineBreaks.Text = Configuration.Settings.Language.CompareSubtitles.IgnoreLineBreaks;
            checkBoxIgnoreFormatting.Text = Configuration.Settings.Language.CompareSubtitles.IgnoreFormatting;
            checkBoxOnlyListDifferencesInText.Text = Configuration.Settings.Language.CompareSubtitles.OnlyLookForDifferencesInText;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            copyTextToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.Copy;
            copyTextToolStripMenuItem1.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.Copy;
            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListView2.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListView1.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            UiUtil.InitializeSubtitleFont(subtitleListView2);
            subtitleListView2.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListView2.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            UiUtil.FixLargeFonts(this, buttonOK);
            subtitleListView1.UseSyntaxColoring = false;
            subtitleListView2.UseSyntaxColoring = false;
            openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
            LoadConfigurations();
        }

        public void Initialize(Subtitle subtitle1, string subtitleFileName1, string title)
        {
            Compare_Resize(null, null);
            labelStatus.Text = string.Empty;
            _subtitle1 = subtitle1;
            labelSubtitle1.Text = subtitleFileName1;
            if (string.IsNullOrEmpty(subtitleFileName1))
                labelSubtitle1.Text = title;
            subtitleListView1.Fill(subtitle1);

            if (!string.IsNullOrEmpty(subtitleFileName1))
            {
                try
                {
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(subtitleFileName1);
                }
                catch
                {
                    // ignored
                }
            }
            subtitleListView1.SelectIndexAndEnsureVisible(0);
            _language1 = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle1);
        }

        public void Initialize(Subtitle subtitle1, string subtitleFileName1, Subtitle subtitle2, string subtitleFileName2)
        {
            Compare_Resize(null, null);
            labelStatus.Text = string.Empty;
            _subtitle1 = subtitle1;
            labelSubtitle1.Text = subtitleFileName1;

            _subtitle2 = subtitle2;
            labelSubtitle2.Text = subtitleFileName2;

            _language1 = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle1);
            CompareSubtitles();

            if (!string.IsNullOrEmpty(subtitleFileName1) && File.Exists(subtitleFileName1))
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(subtitleFileName1);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
            subtitleListView2.SelectIndexAndEnsureVisible(0);
        }

        private void ButtonOpenSubtitle1Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (FileUtil.IsVobSub(openFileDialog1.FileName) || FileUtil.IsBluRaySup(openFileDialog1.FileName))
                {
                    MessageBox.Show(Configuration.Settings.Language.CompareSubtitles.CannotCompareWithImageBasedSubtitles);
                    return;
                }
                _subtitle1 = new Subtitle();
                Encoding encoding;
                var format = _subtitle1.LoadSubtitle(openFileDialog1.FileName, out encoding, null);
                if (format == null)
                {
                    var pac = new Pac();
                    if (pac.IsMine(null, openFileDialog1.FileName))
                    {
                        pac.BatchMode = true;
                        pac.LoadSubtitle(_subtitle1, null, openFileDialog1.FileName);
                        format = pac;
                    }
                }
                if (format == null)
                {
                    var cavena890 = new Cavena890();
                    if (cavena890.IsMine(null, openFileDialog1.FileName))
                    {
                        cavena890.LoadSubtitle(_subtitle1, null, openFileDialog1.FileName);
                    }
                }
                if (format == null)
                {
                    var spt = new Spt();
                    if (spt.IsMine(null, openFileDialog1.FileName))
                    {
                        spt.LoadSubtitle(_subtitle1, null, openFileDialog1.FileName);
                    }
                }
                if (format == null)
                {
                    var cheetahCaption = new CheetahCaption();
                    if (cheetahCaption.IsMine(null, openFileDialog1.FileName))
                    {
                        cheetahCaption.LoadSubtitle(_subtitle1, null, openFileDialog1.FileName);
                    }
                }
                if (format == null)
                {
                    var chk = new Chk();
                    if (chk.IsMine(null, openFileDialog1.FileName))
                    {
                        chk.LoadSubtitle(_subtitle1, null, openFileDialog1.FileName);
                    }
                }
                if (format == null)
                {
                    var asc = new TimeLineAscii();
                    if (asc.IsMine(null, openFileDialog1.FileName))
                    {
                        asc.LoadSubtitle(_subtitle1, null, openFileDialog1.FileName);
                    }
                }
                if (format == null)
                {
                    var asc = new TimeLineFootageAscii();
                    if (asc.IsMine(null, openFileDialog1.FileName))
                    {
                        asc.LoadSubtitle(_subtitle1, null, openFileDialog1.FileName);
                    }
                }
                subtitleListView1.Fill(_subtitle1);
                subtitleListView1.SelectIndexAndEnsureVisible(0);
                subtitleListView2.SelectIndexAndEnsureVisible(0);
                labelSubtitle1.Text = openFileDialog1.FileName;
                _language1 = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle1);
                if (_subtitle1.Paragraphs.Count > 0 && _subtitle2?.Paragraphs.Count > 0)
                    CompareSubtitles();
            }
        }

        private void ButtonOpenSubtitle2Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (FileUtil.IsVobSub(openFileDialog1.FileName) || FileUtil.IsBluRaySup(openFileDialog1.FileName))
                {
                    MessageBox.Show(Configuration.Settings.Language.CompareSubtitles.CannotCompareWithImageBasedSubtitles);
                    return;
                }
                _subtitle2 = new Subtitle();
                Encoding encoding;
                var format = _subtitle2.LoadSubtitle(openFileDialog1.FileName, out encoding, null);
                if (format == null)
                {
                    var pac = new Pac();
                    if (pac.IsMine(null, openFileDialog1.FileName))
                    {
                        pac.BatchMode = true;
                        pac.LoadSubtitle(_subtitle2, null, openFileDialog1.FileName);
                        format = pac;
                    }
                }
                if (format == null)
                {
                    var cavena890 = new Cavena890();
                    if (cavena890.IsMine(null, openFileDialog1.FileName))
                    {
                        cavena890.LoadSubtitle(_subtitle2, null, openFileDialog1.FileName);
                    }
                }

                subtitleListView2.Fill(_subtitle2);
                subtitleListView1.SelectIndexAndEnsureVisible(0);
                subtitleListView2.SelectIndexAndEnsureVisible(0);
                labelSubtitle2.Text = openFileDialog1.FileName;
                if (_subtitle2.Paragraphs.Count > 0 && _subtitle1?.Paragraphs.Count > 0)
                    CompareSubtitles();
            }
        }

        private void CompareSubtitles()
        {
            if (_loadingConfig || _subtitle2 == null || _subtitle2.Paragraphs.Count == 0)
            {
                return;
            }
            timer1.Stop();
            var sub1 = new Subtitle(_subtitle1);
            var sub2 = new Subtitle(_subtitle2);

            int index = 0;
            Paragraph p1 = sub1.GetParagraphOrDefault(index);
            Paragraph p2 = sub2.GetParagraphOrDefault(index);
            int max = Math.Max(sub1.Paragraphs.Count, sub2.Paragraphs.Count);
            while (index < max)
            {
                if (p1 != null && p2 != null && GetColumnsEqualExceptNumberAndDuration(p1, p2) == 0)
                {
                    for (int i = index + 1; i < max; i++)
                    {
                        // Try to find atleast two matching properties
                        if (GetColumnsEqualExceptNumber(sub1.GetParagraphOrDefault(i), p2) > 1)
                        {
                            for (int j = index; j < i; j++)
                            {
                                sub2.Paragraphs.Insert(index++, new Paragraph());
                            }
                            break;
                        }
                        if (GetColumnsEqualExceptNumber(p1, sub2.GetParagraphOrDefault(i)) > 1)
                        {
                            for (int j = index; j < i; j++)
                            {
                                sub1.Paragraphs.Insert(index++, new Paragraph());
                            }
                            break;
                        }
                    }
                }
                index++;
                p1 = sub1.GetParagraphOrDefault(index);
                p2 = sub2.GetParagraphOrDefault(index);
            }

            var minSub = sub1.Paragraphs.Count < sub2.Paragraphs.Count ? sub1 : sub2;
            for (var idx = minSub.Paragraphs.Count; idx < max; idx++)
            {
                minSub.Paragraphs.Insert(idx, new Paragraph());
            }

            subtitleListView1.Fill(sub1);
            subtitleListView2.Fill(sub2);

            // coloring + differences index list
            _differences = new List<int>();
            index = 0;
            p1 = sub1.GetParagraphOrDefault(index);
            p2 = sub2.GetParagraphOrDefault(index);
            int totalWords = 0;
            int wordsChanged = 0;
            string emptyParagraphAsString = new Paragraph().ToString();
            max = Math.Max(sub1.Paragraphs.Count, sub2.Paragraphs.Count);
            int min = Math.Min(sub1.Paragraphs.Count, sub2.Paragraphs.Count);
            var onlyTextDiff = checkBoxOnlyListDifferencesInText.Checked;

            if (onlyTextDiff)
            {
                while (index < min)
                {
                    bool addIndexToDifferences = false;
                    Utilities.GetTotalAndChangedWords(p1.Text, p2.Text, ref totalWords, ref wordsChanged, checkBoxIgnoreLineBreaks.Checked, checkBoxIgnoreFormatting.Checked, ShouldBreakToLetter());
                    if (p1.ToString() == emptyParagraphAsString)
                    {
                        addIndexToDifferences = true;
                        subtitleListView1.ColorOut(index, Color.Salmon);
                    }
                    else if (p2.ToString() == emptyParagraphAsString)
                    {
                        addIndexToDifferences = true;
                        subtitleListView2.ColorOut(index, Color.Salmon);
                    }
                    else if (FixWhitespace(p1.Text) != FixWhitespace(p2.Text))
                    {
                        addIndexToDifferences = true;
                        subtitleListView1.SetBackgroundColor(index, Color.LightGreen, subtitleListView1.ColumnIndexText);
                        subtitleListView2.SetBackgroundColor(index, Color.LightGreen, subtitleListView2.ColumnIndexText);
                    }
                    if (addIndexToDifferences)
                    {
                        _differences.Add(index);
                    }
                    index++;
                    p1 = sub1.GetParagraphOrDefault(index);
                    p2 = sub2.GetParagraphOrDefault(index);
                }
            }
            else
            {
                const double tolerance = 0.1;
                while (index < min)
                {
                    Utilities.GetTotalAndChangedWords(p1.Text, p2.Text, ref totalWords, ref wordsChanged, checkBoxIgnoreLineBreaks.Checked, checkBoxIgnoreFormatting.Checked, ShouldBreakToLetter());
                    bool addIndexToDifferences = false;
                    if (p1.ToString() == emptyParagraphAsString)
                    {
                        addIndexToDifferences = true;
                        subtitleListView1.ColorOut(index, Color.Salmon);
                    }
                    else if (p2.ToString() == emptyParagraphAsString)
                    {
                        addIndexToDifferences = true;
                        subtitleListView2.ColorOut(index, Color.Salmon);
                    }
                    else
                    {
                        int columnsAlike = GetColumnsEqualExceptNumber(p1, p2);
                        // Not alike paragraphs
                        if (columnsAlike == 0)
                        {
                            addIndexToDifferences = true;
                            subtitleListView1.SetBackgroundColor(index, Color.LightGreen);
                            subtitleListView2.SetBackgroundColor(index, Color.LightGreen);
                            subtitleListView1.SetBackgroundColor(index, subtitleListView1.BackColor, subtitleListView1.ColumnIndexNumber);
                            subtitleListView2.SetBackgroundColor(index, subtitleListView2.BackColor, subtitleListView2.ColumnIndexNumber);
                        }
                        else if (columnsAlike < 4)
                        {
                            addIndexToDifferences = true;
                            // Start time
                            if (Math.Abs(p1.StartTime.TotalMilliseconds - p2.StartTime.TotalMilliseconds) > tolerance)
                            {
                                subtitleListView1.SetBackgroundColor(index, Color.LightGreen, subtitleListView1.ColumnIndexStart);
                                subtitleListView2.SetBackgroundColor(index, Color.LightGreen, subtitleListView2.ColumnIndexStart);
                            }
                            // End time
                            if (Math.Abs(p1.EndTime.TotalMilliseconds - p2.EndTime.TotalMilliseconds) > tolerance)
                            {
                                subtitleListView1.SetBackgroundColor(index, Color.LightGreen, subtitleListView1.ColumnIndexEnd);
                                subtitleListView2.SetBackgroundColor(index, Color.LightGreen, subtitleListView2.ColumnIndexEnd);
                            }
                            // Duration
                            if (Math.Abs(p1.Duration.TotalMilliseconds - p2.Duration.TotalMilliseconds) > tolerance)
                            {
                                subtitleListView1.SetBackgroundColor(index, Color.LightGreen, subtitleListView1.ColumnIndexDuration);
                                subtitleListView2.SetBackgroundColor(index, Color.LightGreen, subtitleListView2.ColumnIndexDuration);
                            }
                            // Text
                            if (FixWhitespace(p1.Text.Trim()) != FixWhitespace(p2.Text.Trim()))
                            {
                                subtitleListView1.SetBackgroundColor(index, Color.LightGreen, subtitleListView1.ColumnIndexText);
                                subtitleListView2.SetBackgroundColor(index, Color.LightGreen, subtitleListView2.ColumnIndexText);
                            }
                        }
                        // Number
                        if (p1.Number != p2.Number)
                        {
                            addIndexToDifferences = true;
                            subtitleListView1.SetBackgroundColor(index, Color.FromArgb(255, 200, 100), subtitleListView1.ColumnIndexNumber);
                            subtitleListView2.SetBackgroundColor(index, Color.FromArgb(255, 200, 100), subtitleListView2.ColumnIndexNumber);
                        }
                    }
                    if (addIndexToDifferences)
                    {
                        _differences.Add(index);
                    }
                    index++;
                    p1 = sub1.GetParagraphOrDefault(index);
                    p2 = sub2.GetParagraphOrDefault(index);
                }
            }
            UpdatePreviousAndNextButtons();

            if (max > min) // color extra lines as has-difference 
            {
                var listView = subtitleListView1.Items.Count > subtitleListView2.Items.Count ? subtitleListView1 : subtitleListView2;
                for (int i = min; i < max; i++)
                {
                    if (!onlyTextDiff)
                    {
                        listView.SetBackgroundColor(i, Color.FromArgb(255, 200, 100), listView.ColumnIndexNumber);
                        listView.SetBackgroundColor(i, Color.LightGreen, listView.ColumnIndexStart);
                        listView.SetBackgroundColor(i, Color.LightGreen, listView.ColumnIndexEnd);
                        listView.SetBackgroundColor(i, Color.LightGreen, listView.ColumnIndexDuration);
                    }
                    listView.SetBackgroundColor(i, Color.LightGreen, listView.ColumnIndexText);
                }
            }

            if (_differences.Count >= min)
            {
                labelStatus.Text = Configuration.Settings.Language.CompareSubtitles.SubtitlesNotAlike;
                labelStatus.Font = new Font(labelStatus.Font.FontFamily, labelStatus.Font.Size, FontStyle.Bold);
            }
            else
            {
                if (wordsChanged != totalWords && wordsChanged > 0)
                {
                    string formatString = Configuration.Settings.Language.CompareSubtitles.XNumberOfDifferenceAndPercentChanged;
                    if (ShouldBreakToLetter())
                        formatString = Configuration.Settings.Language.CompareSubtitles.XNumberOfDifferenceAndPercentLettersChanged;

                    labelStatus.Text = string.Format(formatString, _differences.Count, wordsChanged * 100.00 / totalWords);
                }
                else
                {
                    labelStatus.Text = string.Format(Configuration.Settings.Language.CompareSubtitles.XNumberOfDifference, _differences.Count);
                }
                labelStatus.Font = new Font(labelStatus.Font.FontFamily, labelStatus.Font.Size);
            }

            if (checkBoxShowOnlyDifferences.Checked)
            { // Remove all lines with no difference
                subtitleListView1.BeginUpdate();
                subtitleListView2.BeginUpdate();
                if (_differences.Count < min)
                {
                    for (index = Math.Max(subtitleListView1.Items.Count, subtitleListView2.Items.Count) - 1; index >= 0; index--)
                    {
                        if (!_differences.Contains(index))
                        {
                            if (subtitleListView1.Items.Count > index)
                                subtitleListView1.Items.RemoveAt(index);
                            if (subtitleListView2.Items.Count > index)
                                subtitleListView2.Items.RemoveAt(index);
                        }
                    }
                }
                subtitleListView1.EndUpdate();
                subtitleListView2.EndUpdate();
                _differences = new List<int>();
                max = Math.Max(subtitleListView1.Items.Count, subtitleListView2.Items.Count);
                for (index = 0; index < max; index++)
                    _differences.Add(index);
            }
            timer1.Start();
            subtitleListView1.FirstVisibleIndex = -1;
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private bool ShouldBreakToLetter() => _language1 != null && (_language1 == "ja" || _language1 == "zh");

        private string FixWhitespace(string p)
        {
            if (checkBoxIgnoreLineBreaks.Checked)
            {
                p = p.Replace(Environment.NewLine, " ");
                while (p.Contains("  "))
                    p = p.Replace("  ", " ");
            }

            if (checkBoxIgnoreFormatting.Checked)
            {
                p = HtmlUtil.RemoveHtmlTags(p, true);
            }

            return p;
        }

        private int GetColumnsEqualExceptNumber(Paragraph p1, Paragraph p2)
        {
            if (p1 == null || p2 == null)
                return 0;

            const double tolerance = 0.1;

            int columnsEqual = 0;
            if (Math.Abs(p1.StartTime.TotalMilliseconds - p2.StartTime.TotalMilliseconds) < tolerance)
                columnsEqual++;

            if (Math.Abs(p1.EndTime.TotalMilliseconds - p2.EndTime.TotalMilliseconds) < tolerance)
                columnsEqual++;

            if (Math.Abs(p1.Duration.TotalMilliseconds - p2.Duration.TotalMilliseconds) < tolerance)
                columnsEqual++;

            if (p1.Text.Trim() == p2.Text.Trim() ||
                checkBoxIgnoreFormatting.Checked && HtmlUtil.RemoveHtmlTags(p1.Text.Trim()) == HtmlUtil.RemoveHtmlTags(p2.Text.Trim()))
            {
                columnsEqual++;
            }
            
            return columnsEqual;
        }

        private int GetColumnsEqualExceptNumberAndDuration(Paragraph p1, Paragraph p2)
        {
            if (p1 == null || p2 == null)
                return 0;

            const double tolerance = 0.1;

            int columnsEqual = 0;
            if (Math.Abs(p1.StartTime.TotalMilliseconds - p2.StartTime.TotalMilliseconds) < tolerance)
                columnsEqual++;

            if (Math.Abs(p1.EndTime.TotalMilliseconds - p2.EndTime.TotalMilliseconds) < tolerance)
                columnsEqual++;

            if (p1.Text.Trim() == p2.Text.Trim() ||
                checkBoxIgnoreFormatting.Checked && HtmlUtil.RemoveHtmlTags(p1.Text.Trim()) == HtmlUtil.RemoveHtmlTags(p2.Text.Trim()))
            {
                columnsEqual++;
            }

            return columnsEqual;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            Close();
        }

        private void Compare_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
            else if (e.KeyCode == Keys.Enter && buttonNextDifference.Enabled)
                ButtonNextDifferenceClick(null, null);
            else if (e.KeyCode == Keys.Right && buttonNextDifference.Enabled)
                ButtonNextDifferenceClick(null, null);
            else if (e.KeyCode == Keys.Left && buttonPreviousDifference.Enabled)
                ButtonPreviousDifferenceClick(null, null);
            else if (_mainGeneralGoToNextSubtitle == e.KeyData || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
            {
                SubtitleListView lv = subtitleListView1;
                if (subtitleListView2.Focused)
                    lv = subtitleListView2;
                int selectedIndex = 0;
                if (lv.SelectedItems.Count > 0)
                {
                    selectedIndex = lv.SelectedItems[0].Index;
                    selectedIndex++;
                }
                lv.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData || (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt))
            {
                SubtitleListView lv = subtitleListView1;
                if (subtitleListView2.Focused)
                    lv = subtitleListView2;
                int selectedIndex = 0;
                if (lv.SelectedItems.Count > 0)
                {
                    selectedIndex = lv.SelectedItems[0].Index;
                    selectedIndex--;
                }
                lv.SelectIndexAndEnsureVisible(selectedIndex);
            }
        }

        private void SubtitleListView1SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreviousAndNextButtons();
            ShowText();
        }

        private void ShowText()
        {
            string text1 = string.Empty;
            string text2 = string.Empty;

            if (subtitleListView1.SelectedItems.Count == 1)
            {
                text1 = subtitleListView1.GetText(subtitleListView1.SelectedItems[0].Index);

                if (subtitleListView2.Items.Count > subtitleListView1.SelectedItems[0].Index)
                    text2 = subtitleListView2.GetText(subtitleListView1.SelectedItems[0].Index);
            }
            richTextBox1.Text = text1;
            richTextBox2.Text = text2;

            // show diff
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2) || text1 == text2)
                return;

            ShowTextDifference();
        }

        private void ShowTextDifference()
        {
            // from start
            int minLength = Math.Min(richTextBox1.Text.Length, richTextBox2.Text.Length);
            int startCharactersOk = 0;
            for (int i = 0; i < minLength; i++)
            {
                if (richTextBox1.Text[i] == richTextBox2.Text[i])
                {
                    startCharactersOk++;
                }
                else
                {
                    if (richTextBox1.Text.Length > i + 4 && richTextBox2.Text.Length > i + 4 &&
                        richTextBox1.Text[i + 1] == richTextBox2.Text[i + 1] &&
                        richTextBox1.Text[i + 2] == richTextBox2.Text[i + 2] &&
                        richTextBox1.Text[i + 3] == richTextBox2.Text[i + 3] &&
                        richTextBox1.Text[i + 4] == richTextBox2.Text[i + 4])
                    {
                        startCharactersOk++;

                        richTextBox1.SelectionStart = i;
                        richTextBox1.SelectionLength = 1;
                        richTextBox1.SelectionColor = _foregroundDifferenceColor;
                        if (string.IsNullOrWhiteSpace(richTextBox1.SelectedText))
                            richTextBox1.SelectionBackColor = _backDifferenceColor;

                        richTextBox2.SelectionStart = i;
                        richTextBox2.SelectionLength = 1;
                        richTextBox2.SelectionColor = _foregroundDifferenceColor;
                        if (string.IsNullOrWhiteSpace(richTextBox2.SelectedText))
                            richTextBox2.SelectionBackColor = _backDifferenceColor;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            int maxLength = Math.Max(richTextBox1.Text.Length, richTextBox2.Text.Length);
            for (int i = startCharactersOk; i < maxLength; i++)
            {
                if (i < richTextBox1.Text.Length)
                {
                    richTextBox1.SelectionStart = i;
                    richTextBox1.SelectionLength = 1;
                    richTextBox1.SelectionBackColor = _backDifferenceColor;
                    if (string.IsNullOrWhiteSpace(richTextBox1.SelectedText))
                        richTextBox1.SelectionBackColor = _backDifferenceColor;
                }
                if (i < richTextBox2.Text.Length)
                {
                    richTextBox2.SelectionStart = i;
                    richTextBox2.SelectionLength = 1;
                    richTextBox2.SelectionColor = _foregroundDifferenceColor;
                    if (string.IsNullOrWhiteSpace(richTextBox2.SelectedText))
                        richTextBox2.SelectionBackColor = _backDifferenceColor;
                }
            }

            // from end
            for (int i = 1; i < minLength; i++)
            {
                if (richTextBox1.Text[richTextBox1.Text.Length - i] == richTextBox2.Text[richTextBox2.Text.Length - i])
                {
                    richTextBox1.SelectionStart = richTextBox1.Text.Length - i;
                    richTextBox1.SelectionLength = 1;
                    richTextBox1.SelectionColor = Color.Black;
                    richTextBox1.SelectionBackColor = richTextBox1.BackColor;

                    richTextBox2.SelectionStart = richTextBox2.Text.Length - i;
                    richTextBox2.SelectionLength = 1;
                    richTextBox2.SelectionColor = Color.Black;
                    richTextBox2.SelectionBackColor = richTextBox1.BackColor;
                }
                else
                {
                    break;
                }
            }

            // special situation - equal, but one has more chars
            if (richTextBox1.Text.Length > richTextBox2.Text.Length)
            {
                if (richTextBox1.Text.StartsWith(richTextBox2.Text, StringComparison.Ordinal))
                {
                    richTextBox1.SelectionStart = richTextBox2.Text.Length;
                    richTextBox1.SelectionLength = richTextBox1.Text.Length - richTextBox2.Text.Length;
                    richTextBox1.SelectionBackColor = _backDifferenceColor;
                }
            }
            else if (richTextBox2.Text.Length > richTextBox1.Text.Length)
            {
                if (richTextBox2.Text.StartsWith(richTextBox1.Text, StringComparison.Ordinal))
                {
                    richTextBox2.SelectionStart = richTextBox1.Text.Length;
                    richTextBox2.SelectionLength = richTextBox2.Text.Length - richTextBox1.Text.Length;
                    richTextBox2.SelectionColor = _foregroundDifferenceColor;
                }
            }
        }

        private void UpdatePreviousAndNextButtons()
        {
            if (subtitleListView1.Items.Count > 0 && subtitleListView2.Items.Count > 0 &&
                _differences != null && _differences.Count > 0)
            {
                if (subtitleListView1.SelectedItems.Count == 0)
                {
                    buttonPreviousDifference.Enabled = false;
                    buttonNextDifference.Enabled = true;
                }
                else
                {
                    int index = subtitleListView1.SelectedItems[0].Index;
                    buttonPreviousDifference.Enabled = _differences[0] < index;
                    buttonNextDifference.Enabled = _differences[_differences.Count - 1] > index;
                }
            }
            else
            {
                buttonPreviousDifference.Enabled = false;
                buttonNextDifference.Enabled = false;
            }
        }

        private void SubtitleListView2SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreviousAndNextButtons();
        }

        private void Compare_Resize(object sender, EventArgs e)
        {
            subtitleListView1.Width = (Width / 2) - 20;

            subtitleListView2.Left = (Width / 2) - 3;
            subtitleListView2.Width = (Width / 2) - 20;
            labelSubtitle2.Left = subtitleListView2.Left;
            buttonOpenSubtitle2.Left = subtitleListView2.Left;

            subtitleListView1.Height = Height - (subtitleListView1.Top + 140);
            subtitleListView2.Height = Height - (subtitleListView2.Top + 140);

            richTextBox1.Width = subtitleListView1.Width;
            richTextBox2.Width = subtitleListView2.Width;
            richTextBox2.Left = subtitleListView2.Left;
        }

        private void ButtonPreviousDifferenceClick(object sender, EventArgs e)
        {
            if (_differences != null && _differences.Count > 0)
            {
                if (subtitleListView1.SelectedItems.Count == 0)
                {
                    subtitleListView1.SelectIndexAndEnsureVisible(_differences[0]);
                }
                else
                {
                    for (int i = subtitleListView1.SelectedItems[0].Index - 1; i >= 0; i--)
                    {
                        if (_differences.Contains(i))
                        {
                            subtitleListView1.SelectIndexAndEnsureVisible(i - 2);
                            subtitleListView1.SelectIndexAndEnsureVisible(i + 2);
                            subtitleListView1.SelectIndexAndEnsureVisible(i);
                            break;
                        }
                    }
                }
            }
        }

        private void ButtonNextDifferenceClick(object sender, EventArgs e)
        {
            if (_differences != null && _differences.Count > 0)
            {
                if (subtitleListView1.SelectedItems.Count == 0)
                {
                    subtitleListView1.SelectIndexAndEnsureVisible(_differences[0]);
                }
                else
                {
                    for (int i = subtitleListView1.SelectedItems[0].Index + 1; i < subtitleListView1.Items.Count; i++)
                    {
                        if (_differences.Contains(i))
                        {
                            subtitleListView1.SelectIndexAndEnsureVisible(i - 2);
                            subtitleListView1.SelectIndexAndEnsureVisible(i + 2);
                            subtitleListView1.SelectIndexAndEnsureVisible(i);
                            subtitleListView1.Focus();
                            break;
                        }
                    }
                }
            }
        }

        private void Timer1Tick(object sender, EventArgs e)
        {
            if (subtitleListView1.TopItem == null || subtitleListView2.TopItem == null)
                return;

            char activeListView;
            var p = PointToClient(MousePosition);
            if (p.X >= subtitleListView1.Left && p.X <= subtitleListView1.Left + subtitleListView1.Width + 2)
                activeListView = 'L';
            else if (p.X >= subtitleListView2.Left && p.X <= subtitleListView2.Left + subtitleListView2.Width + 2)
                activeListView = 'R';
            else
                return;

            if (subtitleListView1.SelectedItems.Count > 0 && activeListView == 'L')
            {
                if (subtitleListView2.SelectedItems.Count > 0 &&
                    subtitleListView1.SelectedItems[0].Index == subtitleListView2.SelectedItems[0].Index)
                {
                    if (subtitleListView1.TopItem.Index != subtitleListView2.TopItem.Index &&
                        subtitleListView2.Items.Count > subtitleListView1.TopItem.Index)
                        subtitleListView2.TopItem = subtitleListView2.Items[subtitleListView1.TopItem.Index];
                    return;
                }
                subtitleListView2.SelectedIndexChanged -= SubtitleListView2SelectedIndexChanged;
                subtitleListView2.SelectIndexAndEnsureVisible(subtitleListView1.SelectedItems[0].Index);
                if (subtitleListView1.TopItem.Index != subtitleListView2.TopItem.Index &&
                    subtitleListView2.Items.Count > subtitleListView1.TopItem.Index)
                    subtitleListView2.TopItem = subtitleListView2.Items[subtitleListView1.TopItem.Index];
                subtitleListView2.SelectedIndexChanged += SubtitleListView2SelectedIndexChanged;
            }
            else if (subtitleListView2.SelectedItems.Count > 0 && activeListView == 'R')
            {
                if (subtitleListView1.SelectedItems.Count > 0 &&
                    subtitleListView2.SelectedItems[0].Index == subtitleListView1.SelectedItems[0].Index)
                {
                    if (subtitleListView2.TopItem.Index != subtitleListView1.TopItem.Index &&
                        subtitleListView1.Items.Count > subtitleListView2.TopItem.Index)
                        subtitleListView1.TopItem = subtitleListView1.Items[subtitleListView2.TopItem.Index];
                    return;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(subtitleListView2.SelectedItems[0].Index);
                if (subtitleListView2.TopItem.Index != subtitleListView1.TopItem.Index &&
                    subtitleListView1.Items.Count > subtitleListView2.TopItem.Index)
                    subtitleListView1.TopItem = subtitleListView1.Items[subtitleListView2.TopItem.Index];
            }
        }

        private void Compare_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            SaveConfigurations();
        }

        private void checkBoxShowOnlyDifferences_CheckedChanged(object sender, EventArgs e)
        {
            CompareSubtitles();
        }

        private void checkBoxOnlyListDifferencesInText_CheckedChanged(object sender, EventArgs e)
        {
            CompareSubtitles();
        }

        private void checkBoxIgnoreLineBreaks_CheckedChanged(object sender, EventArgs e)
        {
            CompareSubtitles();
        }

        private void labelSubtitle1_MouseHover(object sender, EventArgs e)
        {
            ShowTip(labelSubtitle1);
        }

        private void labelSubtitle2_MouseHover(object sender, EventArgs e)
        {
            ShowTip(labelSubtitle2);
        }

        private void ShowTip(Control control)
        {
            var text = control.Text;
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    text = Path.GetFileName(text);
                }
                catch
                {
                    // not a path
                }
                toolTip1.Show(text, control);
            }
        }

        private void subtitleListView1_DragEnter(object sender, DragEventArgs e)
        {
            VerifyDragEnter(e);
        }

        private void subtitleListView2_DragEnter(object sender, DragEventArgs e)
        {
            VerifyDragEnter(e);
        }

        private static void VerifyDragEnter(DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void subtitleListView1_DragDrop(object sender, DragEventArgs e)
        {
            VerifyDragDrop((sender as ListView), e);
        }

        private void subtitleListView2_DragDrop(object sender, DragEventArgs e)
        {
            VerifyDragDrop((sender as ListView), e);
        }

        private void VerifyDragDrop(ListView listView, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null)
            {
                return;
            }
            if (files.Length > 1)
            {
                MessageBox.Show(Configuration.Settings.Language.Main.DropOnlyOneFile,
                    string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string filePath = files[0];
            if (FileUtil.IsDirectory(filePath))
            {
                MessageBox.Show(Configuration.Settings.Language.Main.ErrorDirectoryDropNotAllowed,
                    string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var listExt = new List<string>();
            foreach (var s in UiUtil.SubtitleExtensionFilter.Value.Split(new[] { '*' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (s.EndsWith(';'))
                    listExt.Add(s.Trim(';'));
            }
            if (!listExt.Contains(Path.GetExtension(filePath)))
                return;
            if (FileUtil.IsVobSub(filePath) || FileUtil.IsBluRaySup(filePath))
            {
                MessageBox.Show(Configuration.Settings.Language.CompareSubtitles.CannotCompareWithImageBasedSubtitles);
                return;
            }
            Encoding encoding;
            if (listView.Name == "subtitleListView1")
            {
                _subtitle1 = new Subtitle();
                _subtitle1.LoadSubtitle(filePath, out encoding, null);
                subtitleListView1.Fill(_subtitle1);
                subtitleListView1.SelectIndexAndEnsureVisible(0);
                subtitleListView2.SelectIndexAndEnsureVisible(0);
                labelSubtitle1.Text = filePath;
                _language1 = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle1);
                if (_subtitle1.Paragraphs.Count > 0)
                    CompareSubtitles();
            }
            else
            {
                _subtitle2 = new Subtitle();
                _subtitle2.LoadSubtitle(filePath, out encoding, null);
                subtitleListView2.Fill(_subtitle2);
                subtitleListView1.SelectIndexAndEnsureVisible(0);
                subtitleListView2.SelectIndexAndEnsureVisible(0);
                labelSubtitle2.Text = filePath;
                if (_subtitle2.Paragraphs.Count > 0)
                    CompareSubtitles();
            }
        }

        private void copyTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyTextToClipboard(richTextBox1);
        }

        private void copyTextToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyTextToClipboard(richTextBox2);
        }

        private static void CopyTextToClipboard(RichTextBox sender)
        {
            if (string.IsNullOrWhiteSpace(sender.Text))
                return;
            if (sender.SelectedText.Length > 0)
            {
                Clipboard.SetText(sender.SelectedText);
                return;
            }
            Clipboard.SetText(sender.Text);
        }

        private void LoadConfigurations()
        {
            var config = Configuration.Settings.Compare;
            checkBoxShowOnlyDifferences.Checked = config.ShowOnlyDifferences;
            checkBoxOnlyListDifferencesInText.Checked = config.OnlyLookForDifferenceInText;
            checkBoxIgnoreLineBreaks.Checked = config.IgnoreLineBreaks;
            checkBoxIgnoreFormatting.Checked = config.IgnoreFormatting;
            _loadingConfig = false;
        }

        private void SaveConfigurations()
        {
            var config = Configuration.Settings.Compare;
            config.ShowOnlyDifferences = checkBoxShowOnlyDifferences.Checked;
            config.OnlyLookForDifferenceInText = checkBoxOnlyListDifferencesInText.Checked;
            config.IgnoreLineBreaks = checkBoxIgnoreLineBreaks.Checked;
            config.IgnoreFormatting = checkBoxIgnoreFormatting.Checked;
        }

        private void checkBoxIgnoreFormatting_CheckedChanged(object sender, EventArgs e)
        {
            CompareSubtitles();
        }

        private void Compare_Shown(object sender, EventArgs e)
        {
            Activate();
        }
    }
}
