using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private readonly Keys _mainGeneralGoToNextSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitlePlayTranslate);
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
        private readonly Keys _mainGeneralGoToPrevSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitlePlayTranslate);
        private string _language;
        private static readonly Color BackDifferenceColor = Color.FromArgb(255, 90, 90);
        private static readonly Color ForegroundDifferenceColor = Color.FromArgb(225, 0, 0);
        private static readonly Color ListViewGreen = Configuration.Settings.General.UseDarkTheme ? Color.Green : Color.LightGreen;
        private static readonly Color ListViewRed = Configuration.Settings.General.UseDarkTheme ? Color.DarkRed : Color.Salmon;
        private bool _loadingConfig = true;

        public Compare()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelSubtitle2.Text = string.Empty;
            Text = LanguageSettings.Current.CompareSubtitles.Title;
            buttonReloadSubtitle1.Text = LanguageSettings.Current.CompareSubtitles.Reload;
            buttonReloadSubtitle2.Text = LanguageSettings.Current.CompareSubtitles.Reload;
            using (var graphics = CreateGraphics())
            {
                var w = (int)graphics.MeasureString(buttonReloadSubtitle1.Text, Font).Width;
                buttonReloadSubtitle1.Width = w + 17;
                buttonReloadSubtitle2.Width = w + 17;
            }

            buttonPreviousDifference.Text = LanguageSettings.Current.CompareSubtitles.PreviousDifference;
            buttonNextDifference.Text = LanguageSettings.Current.CompareSubtitles.NextDifference;
            checkBoxShowOnlyDifferences.Text = LanguageSettings.Current.CompareSubtitles.ShowOnlyDifferences;
            checkBoxIgnoreLineBreaks.Text = LanguageSettings.Current.CompareSubtitles.IgnoreLineBreaks;
            checkBoxIgnoreWhitespace.Text = LanguageSettings.Current.CompareSubtitles.IgnoreWhitespace;
            checkBoxIgnoreFormatting.Text = LanguageSettings.Current.CompareSubtitles.IgnoreFormatting;
            checkBoxOnlyListDifferencesInText.Text = LanguageSettings.Current.CompareSubtitles.OnlyLookForDifferencesInText;
            buttonExport.Text = LanguageSettings.Current.Statistics.Export;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            copyTextToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Copy;
            copyTextToolStripMenuItem1.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Copy;
            subtitleListView1.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            subtitleListView2.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
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
            buttonExport.Enabled = false;
        }

        public void Initialize(Subtitle subtitle1, string subtitleFileName1, string title)
        {
            Compare_Resize(null, null);
            labelStatus.Text = string.Empty;
            _subtitle1 = subtitle1;
            labelSubtitle1.Text = subtitleFileName1;
            if (string.IsNullOrEmpty(subtitleFileName1))
            {
                labelSubtitle1.Text = title;
            }

            subtitleListView1.Fill(subtitle1);
            subtitleListView1.AutoSizeAllColumns(this);
            subtitleListView2.AutoSizeAllColumns(this);

            if (!string.IsNullOrEmpty(subtitleFileName1) && File.Exists(subtitleFileName1))
            {
                try
                {
                    buttonReloadSubtitle1.Enabled = true;
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(subtitleFileName1);
                }
                catch
                {
                    // ignored
                }
            }
            subtitleListView1.SelectIndexAndEnsureVisible(0);
            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle1);
        }

        public void Initialize(Subtitle subtitle1, string subtitleFileName1, Subtitle subtitle2, string subtitleFileName2)
        {
            Compare_Resize(null, null);
            labelStatus.Text = string.Empty;
            _subtitle1 = subtitle1;
            labelSubtitle1.Text = subtitleFileName1;

            _subtitle2 = subtitle2;
            labelSubtitle2.Text = subtitleFileName2;

            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle1);
            CompareSubtitles(0);

            if (!string.IsNullOrEmpty(subtitleFileName1) && File.Exists(subtitleFileName1))
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(subtitleFileName1);
            }

            subtitleListView1.SelectIndexAndEnsureVisible(0);
            subtitleListView2.SelectIndexAndEnsureVisible(0);
        }

        private void LoadAndCompare(int subtitleNumber, string fileName)
        {
            var selectedIndices = subtitleListView1.GetSelectedIndices();
            var selectedIndex = selectedIndices.Length > 0 ? selectedIndices[0] : 0;

            if (subtitleNumber == 1)
            {
                _subtitle1 = LoadSubtitle(fileName);
                subtitleListView1.Fill(_subtitle1);
            }
            else if (subtitleNumber == 2)
            {
                _subtitle2 = LoadSubtitle(fileName);
                subtitleListView2.Fill(_subtitle2);
            }

            if (_subtitle1.Paragraphs.Count < selectedIndex)
            {
                selectedIndex = 0;
            }

            if (_subtitle1.Paragraphs.Count > 0 && _subtitle2?.Paragraphs.Count > 0)
            {
                CompareSubtitles(selectedIndex);
            }
            else
            {
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
                subtitleListView2.SelectIndexAndEnsureVisible(selectedIndex);
            }
        }

        private void ButtonOpenSubtitle1Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (FileUtil.IsVobSub(openFileDialog1.FileName) || FileUtil.IsBluRaySup(openFileDialog1.FileName))
            {
                MessageBox.Show(LanguageSettings.Current.CompareSubtitles.CannotCompareWithImageBasedSubtitles);
                return;
            }

            LoadAndCompare(1, openFileDialog1.FileName);
            labelSubtitle1.Text = openFileDialog1.FileName;
            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle1);
            buttonReloadSubtitle1.Enabled = true;
        }

        private void ButtonReloadSubtitle1Click(object sender, EventArgs e)
        {
            var fileName = labelSubtitle1.Text;
            if (!string.IsNullOrEmpty(fileName))
            {
                LoadAndCompare(1, fileName);
            }
        }

        private void ButtonOpenSubtitle2Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (FileUtil.IsVobSub(openFileDialog1.FileName) || FileUtil.IsBluRaySup(openFileDialog1.FileName))
            {
                MessageBox.Show(LanguageSettings.Current.CompareSubtitles.CannotCompareWithImageBasedSubtitles);
                return;
            }

            LoadAndCompare(2, openFileDialog1.FileName);
            labelSubtitle2.Text = openFileDialog1.FileName;
            buttonReloadSubtitle2.Enabled = true;
        }

        private void ButtonReloadSubtitle2Click(object sender, EventArgs e)
        {
            var fileName = labelSubtitle2.Text;
            if (!string.IsNullOrEmpty(fileName))
            {
                LoadAndCompare(2, fileName);
            }
        }

        private static Subtitle LoadSubtitle(string fileName)
        {
            // EBU STL does not support eg drop frame, so we override the loading frame rate
            Ebu.OverrideReadFrameRate = Configuration.Settings.General.DefaultFrameRate;

            var subtitle = new Subtitle();
            var format = subtitle.LoadSubtitle(fileName, out _, null);
            if (format == null)
            {
                foreach (var f in SubtitleFormat.GetBinaryFormats(false))
                {
                    if (f.IsMine(null, fileName))
                    {
                        f.LoadSubtitle(subtitle, null, fileName);
                        break; // format found, exit the loop
                    }
                }
            }

            Ebu.OverrideReadFrameRate = 0;
            return subtitle;
        }

        private void CompareSubtitles(int selectedIndex)
        {
            if (_loadingConfig || _subtitle2 == null || _subtitle2.Paragraphs.Count == 0)
            {
                return;
            }
            buttonExport.Enabled = true;
            timer1.Stop();
            var sub1 = new Subtitle(_subtitle1);
            var sub2 = new Subtitle(_subtitle2);

            var index = 0;
            var p1 = sub1.GetParagraphOrDefault(index);
            var p2 = sub2.GetParagraphOrDefault(index);
            var max = Math.Max(sub1.Paragraphs.Count, sub2.Paragraphs.Count);
            while (index < max)
            {
                if (p1 != null && p2 != null && GetColumnsEqualExceptNumberAndDuration(p1, p2) == 0)
                {
                    for (var i = index + 1; i < max; i++)
                    {
                        // Try to find at least two matching properties
                        if (GetColumnsEqualExceptNumber(sub1.GetParagraphOrDefault(i), p2) > 1)
                        {
                            for (var j = index; j < i; j++)
                            {
                                sub2.Paragraphs.Insert(index++, new Paragraph());
                            }
                            break;
                        }

                        if (GetColumnsEqualExceptNumber(p1, sub2.GetParagraphOrDefault(i)) > 1)
                        {
                            for (var j = index; j < i; j++)
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
            var totalWords = 0;
            var wordsChanged = 0;
            max = Math.Max(sub1.Paragraphs.Count, sub2.Paragraphs.Count);
            var min = Math.Min(sub1.Paragraphs.Count, sub2.Paragraphs.Count);
            var onlyTextDiff = checkBoxOnlyListDifferencesInText.Checked;

            if (onlyTextDiff)
            {
                while (index < min)
                {
                    var addIndexToDifferences = false;
                    Utilities.GetTotalAndChangedWords(p1.Text, p2.Text, ref totalWords, ref wordsChanged, checkBoxIgnoreLineBreaks.Checked, checkBoxIgnoreFormatting.Checked, ShouldBreakToLetter());
                    if (p1.IsDefault)
                    {
                        addIndexToDifferences = true;
                        subtitleListView1.ColorOut(index, ListViewRed);
                    }
                    else if (p2.IsDefault)
                    {
                        addIndexToDifferences = true;
                        subtitleListView2.ColorOut(index, ListViewRed);
                    }
                    else if (IsTextDifferent(p1, p2))
                    {
                        addIndexToDifferences = true;
                        subtitleListView1.SetBackgroundColor(index, ListViewGreen, subtitleListView1.ColumnIndexText);
                        subtitleListView2.SetBackgroundColor(index, ListViewGreen, subtitleListView2.ColumnIndexText);
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
                while (index < min)
                {
                    Utilities.GetTotalAndChangedWords(p1.Text, p2.Text, ref totalWords, ref wordsChanged, checkBoxIgnoreLineBreaks.Checked, checkBoxIgnoreFormatting.Checked, ShouldBreakToLetter());
                    var addIndexToDifferences = false;
                    if (p1.IsDefault)
                    {
                        addIndexToDifferences = true;
                        subtitleListView1.ColorOut(index, ListViewRed);
                    }
                    else if (p2.IsDefault)
                    {
                        addIndexToDifferences = true;
                        subtitleListView2.ColorOut(index, ListViewRed);
                    }
                    else
                    {
                        var columnsAlike = GetColumnsEqualExceptNumber(p1, p2);
                        // Not alike paragraphs
                        if (columnsAlike == 0)
                        {
                            addIndexToDifferences = true;
                            subtitleListView1.SetBackgroundColor(index, ListViewGreen);
                            subtitleListView2.SetBackgroundColor(index, ListViewGreen);
                            subtitleListView1.SetBackgroundColor(index, subtitleListView1.BackColor, subtitleListView1.ColumnIndexNumber);
                            subtitleListView2.SetBackgroundColor(index, subtitleListView2.BackColor, subtitleListView2.ColumnIndexNumber);
                        }
                        else if (columnsAlike < 4)
                        {
                            addIndexToDifferences = true;
                            // Start time
                            if (!IsTimeEqual(p1.StartTime, p2.StartTime))
                            {
                                subtitleListView1.SetBackgroundColor(index, ListViewGreen, subtitleListView1.ColumnIndexStart);
                                subtitleListView2.SetBackgroundColor(index, ListViewGreen, subtitleListView2.ColumnIndexStart);
                            }
                            // End time
                            if (!IsTimeEqual(p1.EndTime, p2.EndTime))
                            {
                                subtitleListView1.SetBackgroundColor(index, ListViewGreen, subtitleListView1.ColumnIndexEnd);
                                subtitleListView2.SetBackgroundColor(index, ListViewGreen, subtitleListView2.ColumnIndexEnd);
                            }
                            // Duration
                            if (!IsTimeEqual(p1.Duration, p2.Duration))
                            {
                                subtitleListView1.SetBackgroundColor(index, ListViewGreen, subtitleListView1.ColumnIndexDuration);
                                subtitleListView2.SetBackgroundColor(index, ListViewGreen, subtitleListView2.ColumnIndexDuration);
                            }
                            // Text
                            else if (IsTextDifferent(p1, p2))
                            {
                                subtitleListView1.SetBackgroundColor(index, ListViewGreen, subtitleListView1.ColumnIndexText);
                                subtitleListView2.SetBackgroundColor(index, ListViewGreen, subtitleListView2.ColumnIndexText);
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
                for (var i = min; i < max; i++)
                {
                    if (!onlyTextDiff)
                    {
                        listView.SetBackgroundColor(i, Color.FromArgb(255, 200, 100), listView.ColumnIndexNumber);
                        listView.SetBackgroundColor(i, ListViewGreen, listView.ColumnIndexStart);
                        listView.SetBackgroundColor(i, ListViewGreen, listView.ColumnIndexEnd);
                        listView.SetBackgroundColor(i, ListViewGreen, listView.ColumnIndexDuration);
                    }
                    listView.SetBackgroundColor(i, ListViewGreen, listView.ColumnIndexText);
                }
            }

            if (_differences.Count >= min)
            {
                labelStatus.Text = LanguageSettings.Current.CompareSubtitles.SubtitlesNotAlike;
                labelStatus.Font = new Font(labelStatus.Font.FontFamily, labelStatus.Font.Size, FontStyle.Bold);
            }
            else
            {
                if (wordsChanged != totalWords && wordsChanged > 0)
                {
                    var formatString = LanguageSettings.Current.CompareSubtitles.XNumberOfDifferenceAndPercentChanged;
                    if (ShouldBreakToLetter())
                    {
                        formatString = LanguageSettings.Current.CompareSubtitles.XNumberOfDifferenceAndPercentLettersChanged;
                    }

                    labelStatus.Text = string.Format(formatString, _differences.Count, wordsChanged * 100.00 / totalWords);
                }
                else
                {
                    labelStatus.Text = string.Format(LanguageSettings.Current.CompareSubtitles.XNumberOfDifference, _differences.Count);
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
                            {
                                subtitleListView1.Items.RemoveAt(index);
                            }

                            if (subtitleListView2.Items.Count > index)
                            {
                                subtitleListView2.Items.RemoveAt(index);
                            }
                        }
                    }
                }
                subtitleListView1.EndUpdate();
                subtitleListView2.EndUpdate();
                _differences = new List<int>();
                max = Math.Max(subtitleListView1.Items.Count, subtitleListView2.Items.Count);
                for (index = 0; index < max; index++)
                {
                    _differences.Add(index);
                }
            }
            timer1.Start();
            subtitleListView1.FirstVisibleIndex = -1;

            if (subtitleListView1.Items.Count < selectedIndex)
            {
                selectedIndex = 0;
            }

            subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
            SelectLinesInBothListViews('L');
        }

        private bool ShouldBreakToLetter() => _language != null && (_language == "ja" || _language == "zh");

        private bool IsTextDifferent(Paragraph p1, Paragraph p2)
        {
            var t1 = p1.Text;
            var t2 = p2.Text;

            if (checkBoxIgnoreFormatting.Checked)
            {
                t1 = HtmlUtil.RemoveHtmlTags(t1, true);
                t2 = HtmlUtil.RemoveHtmlTags(t2, true);
            }

            else if (checkBoxIgnoreWhitespace.Checked)
            {
                t1 = RemoveWhitespace(t1);
                t2 = RemoveWhitespace(t2);
            }
            else if (checkBoxIgnoreLineBreaks.Checked)
            {
                t1 = t1.Replace(Environment.NewLine, " ");
                t2 = t2.Replace(Environment.NewLine, " ");

                t1 = RemoveDoubleSpace(t1);
                t2 = RemoveDoubleSpace(t2);
            }

            return t1 != t2;
        }

        private static string RemoveWhitespace(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        private static string RemoveDoubleSpace(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            s = s.Replace(Environment.NewLine, " ");
            while (s.Contains("  "))
            {
                s = s.Replace("  ", " ");
            }

            return s;
        }

        private int GetColumnsEqualExceptNumber(Paragraph p1, Paragraph p2)
        {
            if (p1 == null || p2 == null)
            {
                return 0;
            }

            var columnsEqual = 0;
            if (IsTimeEqual(p1.StartTime, p2.StartTime))
            {
                columnsEqual++;
            }

            if (IsTimeEqual(p1.EndTime, p2.EndTime))
            {
                columnsEqual++;
            }

            if (IsTimeEqual(p1.Duration, p2.Duration))
            {
                columnsEqual++;
            }

            if (p1.Text.Trim() == p2.Text.Trim() ||
                checkBoxIgnoreFormatting.Checked && HtmlUtil.RemoveHtmlTags(p1.Text.Trim()) == HtmlUtil.RemoveHtmlTags(p2.Text.Trim()))
            {
                columnsEqual++;
            }

            return columnsEqual;
        }

        private static bool IsTimeEqual(TimeCode t1, TimeCode t2)
        {
            if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
            {
                return t1.ToDisplayString() == t2.ToDisplayString();
            }

            const double tolerance = 0.1;
            return Math.Abs(t1.TotalMilliseconds - t2.TotalMilliseconds) < tolerance;
        }

        private int GetColumnsEqualExceptNumberAndDuration(Paragraph p1, Paragraph p2)
        {
            if (p1 == null || p2 == null)
            {
                return 0;
            }

            var columnsEqual = 0;
            if (IsTimeEqual(p1.StartTime, p2.StartTime))
            {
                columnsEqual++;
            }

            if (IsTimeEqual(p1.EndTime, p2.EndTime))
            {
                columnsEqual++;
            }

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
            {
                Close();
            }
            else if (e.KeyCode == Keys.Enter && buttonNextDifference.Enabled)
            {
                ButtonNextDifferenceClick(null, null);
            }
            else if (e.KeyCode == Keys.Right && buttonNextDifference.Enabled)
            {
                ButtonNextDifferenceClick(null, null);
            }
            else if (e.KeyCode == Keys.Left && buttonPreviousDifference.Enabled)
            {
                ButtonPreviousDifferenceClick(null, null);
            }
            else if (_mainGeneralGoToNextSubtitle == e.KeyData || _mainGeneralGoToNextSubtitlePlayTranslate == e.KeyData)
            {
                SubtitleListView lv = subtitleListView1;
                if (subtitleListView2.Focused)
                {
                    lv = subtitleListView2;
                }

                var selectedIndex = 0;
                if (lv.SelectedItems.Count > 0)
                {
                    selectedIndex = lv.SelectedItems[0].Index;
                    selectedIndex++;
                }
                lv.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData || _mainGeneralGoToPrevSubtitlePlayTranslate == e.KeyData)
            {
                SubtitleListView lv = subtitleListView1;
                if (subtitleListView2.Focused)
                {
                    lv = subtitleListView2;
                }

                var selectedIndex = 0;
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
            var text1 = string.Empty;
            var text2 = string.Empty;

            if (subtitleListView1.SelectedItems.Count == 1)
            {
                text1 = subtitleListView1.GetText(subtitleListView1.SelectedItems[0].Index);

                if (subtitleListView2.Items.Count > subtitleListView1.SelectedItems[0].Index)
                {
                    text2 = subtitleListView2.GetText(subtitleListView1.SelectedItems[0].Index);
                }
            }
            richTextBox1.Text = text1;
            richTextBox2.Text = text2;

            // show diff
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2) || text1 == text2)
            {
                return;
            }

            ShowTextDifference();
        }

        public class RemoveFormattingInfo
        {

            public RemoveFormattingInfo(int index, string tag)
            {
                Index = index;
                Tag = tag;
            }

            public int Index { get; set; }
            public string Tag { get; set; }
        }

        private void ShowTextDifference()
        {
            var tags1 = new List<RemoveFormattingInfo>();
            var tags2 = new List<RemoveFormattingInfo>();
            if (checkBoxIgnoreFormatting.Checked)
            {
                SeparateHtmlTags(richTextBox1, tags1);
                SeparateHtmlTags(richTextBox2, tags2);
            }

            // from start
            var minLength = Math.Min(richTextBox1.Text.RemoveChar('\r').Length, richTextBox2.Text.RemoveChar('\r').Length);
            var startCharactersOk = 0;
            for (var i = 0; i < minLength; i++)
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
                        richTextBox1.SelectionColor = ForegroundDifferenceColor;
                        if (" .,".Contains(richTextBox1.SelectedText))
                        {
                            richTextBox1.SelectionBackColor = BackDifferenceColor;
                            richTextBox1.SelectionColor = DefaultForeColor;
                        }

                        richTextBox2.SelectionStart = i;
                        richTextBox2.SelectionLength = 1;
                        richTextBox2.SelectionColor = ForegroundDifferenceColor;
                        if (" .,".Contains(richTextBox2.SelectedText))
                        {
                            richTextBox2.SelectionBackColor = BackDifferenceColor;
                            richTextBox2.SelectionColor = DefaultForeColor;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var maxLength = Math.Max(richTextBox1.Text.Length, richTextBox2.Text.Length);
            for (var i = startCharactersOk; i < maxLength; i++)
            {
                if (i < richTextBox1.Text.Length)
                {
                    richTextBox1.SelectionStart = i;
                    richTextBox1.SelectionLength = 1;
                    richTextBox1.SelectionColor = ForegroundDifferenceColor;
                    if (" .,".Contains(richTextBox1.SelectedText))
                    {
                        richTextBox1.SelectionBackColor = BackDifferenceColor;
                        richTextBox1.SelectionColor = DefaultForeColor;
                    }
                }
                if (i < richTextBox2.Text.Length)
                {
                    richTextBox2.SelectionStart = i;
                    richTextBox2.SelectionLength = 1;
                    richTextBox2.SelectionColor = ForegroundDifferenceColor;
                    if (" .,".Contains(richTextBox2.SelectedText))
                    {
                        richTextBox2.SelectionBackColor = BackDifferenceColor;
                        richTextBox2.SelectionColor = DefaultForeColor;
                    }
                }
            }

            // from end
            for (var i = 1; i < minLength; i++)
            {
                if (richTextBox1.Text.Length - i < startCharactersOk || richTextBox2.Text.Length - i < startCharactersOk)
                {
                    break;
                }

                if (richTextBox1.Text[richTextBox1.Text.Length - i] == richTextBox2.Text[richTextBox2.Text.Length - i])
                {
                    var selectionColor = Configuration.Settings.General.UseDarkTheme ? Configuration.Settings.General.DarkThemeForeColor : Color.Black;
                    richTextBox1.SelectionStart = richTextBox1.Text.Length - i;
                    richTextBox1.SelectionLength = 1;
                    richTextBox1.SelectionColor = selectionColor;
                    richTextBox1.SelectionBackColor = richTextBox1.BackColor;

                    richTextBox2.SelectionStart = richTextBox2.Text.Length - i;
                    richTextBox2.SelectionLength = 1;
                    richTextBox2.SelectionColor = selectionColor;
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
                    richTextBox1.SelectionBackColor = BackDifferenceColor;
                }
            }
            else if (richTextBox2.Text.Length > richTextBox1.Text.Length)
            {
                if (richTextBox2.Text.StartsWith(richTextBox1.Text, StringComparison.Ordinal))
                {
                    richTextBox2.SelectionStart = richTextBox1.Text.Length;
                    richTextBox2.SelectionLength = richTextBox2.Text.Length - richTextBox1.Text.Length;
                    richTextBox2.SelectionColor = ForegroundDifferenceColor;
                }
            }

            if (checkBoxIgnoreFormatting.Checked)
            {
                ReAddHtmlTags(richTextBox1, tags1);
                ReAddHtmlTags(richTextBox2, tags2);
            }
        }

        private static void SeparateHtmlTags(RichTextBox rtb, List<RemoveFormattingInfo> tags)
        {
            for (var i = rtb.Text.Length - 1; i >= 0; i--)
            {
                if (rtb.Text.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    tags.Add(new RemoveFormattingInfo(i, "<i>"));
                    rtb.Text = rtb.Text.Remove(i, 3);
                }
                else if (rtb.Text.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                {
                    tags.Add(new RemoveFormattingInfo(i, "</i>"));
                    rtb.Text = rtb.Text.Remove(i, 4);
                }
                if (rtb.Text.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    tags.Add(new RemoveFormattingInfo(i, "<b>"));
                    rtb.Text = rtb.Text.Remove(i, 3);
                }
                else if (rtb.Text.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                {
                    tags.Add(new RemoveFormattingInfo(i, "</b>"));
                    rtb.Text = rtb.Text.Remove(i, 4);
                }
                else if (rtb.Text.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                {
                    var endIdx = rtb.Text.Substring(i).IndexOf('>');
                    if (endIdx > 0)
                    {
                        var tag = rtb.Text.Substring(0, endIdx + 1);
                        tags.Add(new RemoveFormattingInfo(i, tag));
                        rtb.Text = rtb.Text.Remove(i, tag.Length);
                    }
                }
                else if (rtb.Text.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                {
                    tags.Add(new RemoveFormattingInfo(i, "</font>"));
                    rtb.Text = rtb.Text.Remove(i, 7);
                }
            }
        }

        private static void ReAddHtmlTags(RichTextBox rtb, List<RemoveFormattingInfo> tags)
        {
            foreach (var tag in tags.OrderByDescending(p => p.Index))
            {
                rtb.SelectionLength = 0;
                rtb.SelectionStart = tag.Index;
                rtb.SelectedText = tag.Tag;
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
                    var index = subtitleListView1.SelectedItems[0].Index;
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
            buttonReloadSubtitle2.Left = buttonOpenSubtitle2.Left + buttonOpenSubtitle2.Width + 7;

            subtitleListView1.Height = Height - (subtitleListView1.Top + 165);
            subtitleListView2.Height = Height - (subtitleListView2.Top + 165);

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
                    for (var i = subtitleListView1.SelectedItems[0].Index - 1; i >= 0; i--)
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
                    for (var i = subtitleListView1.SelectedItems[0].Index + 1; i < subtitleListView1.Items.Count; i++)
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
            {
                return;
            }

            char activeListView;
            var p = PointToClient(MousePosition);
            if (p.X >= subtitleListView1.Left && p.X <= subtitleListView1.Left + subtitleListView1.Width + 2)
            {
                activeListView = 'L';
            }
            else if (p.X >= subtitleListView2.Left && p.X <= subtitleListView2.Left + subtitleListView2.Width + 2)
            {
                activeListView = 'R';
            }
            else
            {
                return;
            }

            SelectLinesInBothListViews(activeListView);
        }

        private void SelectLinesInBothListViews(char activeListView)
        {
            if (subtitleListView1.SelectedItems.Count > 0 && activeListView == 'L')
            {
                if (subtitleListView2.SelectedItems.Count > 0 &&
                    subtitleListView1.SelectedItems[0].Index == subtitleListView2.SelectedItems[0].Index)
                {
                    if (subtitleListView1.TopItem.Index != subtitleListView2.TopItem.Index &&
                        subtitleListView2.Items.Count > subtitleListView1.TopItem.Index)
                    {
                        subtitleListView2.TopItem = subtitleListView2.Items[subtitleListView1.TopItem.Index];
                    }

                    return;
                }

                subtitleListView2.SelectedIndexChanged -= SubtitleListView2SelectedIndexChanged;
                subtitleListView2.SelectIndexAndEnsureVisible(subtitleListView1.SelectedItems[0].Index);
                if (subtitleListView1.TopItem.Index != subtitleListView2.TopItem.Index &&
                    subtitleListView2.Items.Count > subtitleListView1.TopItem.Index)
                {
                    subtitleListView2.TopItem = subtitleListView2.Items[subtitleListView1.TopItem.Index];
                }

                subtitleListView2.SelectedIndexChanged += SubtitleListView2SelectedIndexChanged;
            }
            else if (subtitleListView2.SelectedItems.Count > 0 && activeListView == 'R')
            {
                if (subtitleListView1.SelectedItems.Count > 0 &&
                    subtitleListView2.SelectedItems[0].Index == subtitleListView1.SelectedItems[0].Index)
                {
                    if (subtitleListView2.TopItem.Index != subtitleListView1.TopItem.Index &&
                        subtitleListView1.Items.Count > subtitleListView2.TopItem.Index)
                    {
                        subtitleListView1.TopItem = subtitleListView1.Items[subtitleListView2.TopItem.Index];
                    }

                    return;
                }

                subtitleListView1.SelectIndexAndEnsureVisible(subtitleListView2.SelectedItems[0].Index);
                if (subtitleListView2.TopItem.Index != subtitleListView1.TopItem.Index &&
                    subtitleListView1.Items.Count > subtitleListView2.TopItem.Index)
                {
                    subtitleListView1.TopItem = subtitleListView1.Items[subtitleListView2.TopItem.Index];
                }
            }
        }

        private void Compare_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            SaveConfigurations();
        }

        private void checkBoxShowOnlyDifferences_CheckedChanged(object sender, EventArgs e)
        {
            var selectedIndices = subtitleListView1.GetSelectedIndices();
            var selectedIndex = selectedIndices.Length > 0 ? selectedIndices[0] : 0;
            CompareSubtitles(selectedIndex);
        }

        private void checkBoxOnlyListDifferencesInText_CheckedChanged(object sender, EventArgs e)
        {
            var selectedIndices = subtitleListView1.GetSelectedIndices();
            var selectedIndex = selectedIndices.Length > 0 ? selectedIndices[0] : 0;
            CompareSubtitles(selectedIndex);
        }

        private void checkBoxIgnoreLineBreaks_CheckedChanged(object sender, EventArgs e)
        {
            var selectedIndices = subtitleListView1.GetSelectedIndices();
            var selectedIndex = selectedIndices.Length > 0 ? selectedIndices[0] : 0;
            CompareSubtitles(selectedIndex);
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
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] files))
            {
                return;
            }

            if (files.Length > 1)
            {
                MessageBox.Show(LanguageSettings.Current.Main.DropOnlyOneFile, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var filePath = files[0];
            if (FileUtil.IsDirectory(filePath))
            {
                MessageBox.Show(LanguageSettings.Current.Main.ErrorDirectoryDropNotAllowed, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var listExt = new List<string>();
            foreach (var s in UiUtil.SubtitleExtensionFilter.Value.Split(new[] { '*' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (s.EndsWith(';'))
                {
                    listExt.Add(s.Trim(';'));
                }
            }

            if (!listExt.Contains(Path.GetExtension(filePath)))
            {
                return;
            }

            if (FileUtil.IsVobSub(filePath) || FileUtil.IsBluRaySup(filePath))
            {
                MessageBox.Show(LanguageSettings.Current.CompareSubtitles.CannotCompareWithImageBasedSubtitles);
                return;
            }

            if (listView.Name == "subtitleListView1")
            {
                _subtitle1 = new Subtitle();
                _subtitle1.LoadSubtitle(filePath, out _, null);
                subtitleListView1.Fill(_subtitle1);
                subtitleListView1.SelectIndexAndEnsureVisible(0);
                subtitleListView2.SelectIndexAndEnsureVisible(0);
                labelSubtitle1.Text = filePath;
                buttonReloadSubtitle1.Enabled = true;
                _language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle1);
                if (_subtitle1.Paragraphs.Count > 0)
                {
                    var selectedIndices = subtitleListView1.GetSelectedIndices();
                    var selectedIndex = selectedIndices.Length > 0 ? selectedIndices[0] : 0;
                    CompareSubtitles(selectedIndex);
                }
            }
            else
            {
                _subtitle2 = new Subtitle();
                _subtitle2.LoadSubtitle(filePath, out _, null);
                subtitleListView2.Fill(_subtitle2);
                subtitleListView1.SelectIndexAndEnsureVisible(0);
                subtitleListView2.SelectIndexAndEnsureVisible(0);
                labelSubtitle2.Text = filePath;
                buttonReloadSubtitle2.Enabled = true;
                if (_subtitle2.Paragraphs.Count > 0)
                {
                    var selectedIndices = subtitleListView1.GetSelectedIndices();
                    var selectedIndex = selectedIndices.Length > 0 ? selectedIndices[0] : 0;
                    CompareSubtitles(selectedIndex);
                }
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
            {
                return;
            }

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
            checkBoxIgnoreWhitespace.Checked = config.IgnoreWhitespace;
            checkBoxIgnoreFormatting.Checked = config.IgnoreFormatting;
            _loadingConfig = false;
        }

        private void SaveConfigurations()
        {
            var config = Configuration.Settings.Compare;
            config.ShowOnlyDifferences = checkBoxShowOnlyDifferences.Checked;
            config.OnlyLookForDifferenceInText = checkBoxOnlyListDifferencesInText.Checked;
            config.IgnoreLineBreaks = checkBoxIgnoreLineBreaks.Checked;
            config.IgnoreWhitespace = checkBoxIgnoreWhitespace.Checked;
            config.IgnoreFormatting = checkBoxIgnoreFormatting.Checked;
        }

        private void checkBoxIgnoreFormatting_CheckedChanged(object sender, EventArgs e)
        {
            var selectedIndices = subtitleListView1.GetSelectedIndices();
            var selectedIndex = selectedIndices.Length > 0 ? selectedIndices[0] : 0;
            CompareSubtitles(selectedIndex);
        }

        private void Compare_Shown(object sender, EventArgs e)
        {
            Activate();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            using (var saveFile = new SaveFileDialog { Filter = "Html files|*.html" })
            {
                if (saveFile.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var fileName = saveFile.FileName;
                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html>");
                sb.AppendLine("  <head>");
                sb.AppendLine("    <title>Subtitle Edit compare</title>");
                sb.AppendLine("  </head>");
                sb.AppendLine("  <style>");
                sb.AppendLine("    td { font-family: Tahoma, Verdana, 'Noto Sans', Ubuntu }");
                sb.AppendLine("  </style>");
                sb.AppendLine("  <body>");
                sb.AppendLine("    <h1>Subtitle Edit compare</h1>");
                sb.AppendLine("    <table>");
                sb.AppendLine("    <tr>");
                sb.AppendLine("      <th colspan='4' style='text-align:left'>" + GetFileName(labelSubtitle1.Text) + "</th>");
                sb.AppendLine("      <th>&nbsp;</th>");
                sb.AppendLine("      <th colspan='4' style='text-align:left'>" + GetFileName(labelSubtitle2.Text) + "</th>");
                sb.AppendLine("    </tr>");
                for (var i = 0; i < subtitleListView1.Items.Count; i++)
                {
                    if (subtitleListView1.Items[i].Tag is Paragraph itemLeft &&
                        subtitleListView2.Items.Count > i &&
                        subtitleListView2.Items[i].Tag is Paragraph itemRight)
                    {
                        sb.AppendLine("    <tr>");
                        sb.AppendLine("      <td" + GetHtmlBackgroundColor(subtitleListView1.Items[i].SubItems[0]) + ">" + GetHtmlText(itemLeft, itemLeft.Number.ToString()) + "</td>");
                        sb.AppendLine("      <td" + GetHtmlBackgroundColor(subtitleListView1.Items[i].SubItems[1]) + ">" + GetHtmlText(itemLeft, itemLeft.StartTime.ToShortDisplayString()) + "</td>");
                        if (subtitleListView1.ColumnIndexEnd >= 0)
                        {
                            sb.AppendLine("      <td" + GetHtmlBackgroundColor(subtitleListView1.Items[i].SubItems[subtitleListView1.ColumnIndexEnd]) + ">" + GetHtmlText(itemLeft, itemLeft.EndTime.ToShortDisplayString()) + "</td>");
                        }
                        sb.AppendLine("      <td" + GetHtmlBackgroundColor(subtitleListView1.Items[i].SubItems[subtitleListView1.ColumnIndexText]) + ">" + GetHtmlText(itemLeft, itemLeft.Text) + "</td>");
                        sb.AppendLine("      <td>&nbsp;</td>");
                        sb.AppendLine("      <td" + GetHtmlBackgroundColor(subtitleListView2.Items[i].SubItems[0]) + ">" + GetHtmlText(itemRight, itemRight.Number.ToString()) + "</td>");
                        sb.AppendLine("      <td" + GetHtmlBackgroundColor(subtitleListView2.Items[i].SubItems[1]) + ">" + GetHtmlText(itemRight, itemRight.StartTime.ToShortDisplayString()) + "</td>");
                        if (subtitleListView2.ColumnIndexEnd >= 0)
                        {
                            sb.AppendLine("      <td" + GetHtmlBackgroundColor(subtitleListView2.Items[i].SubItems[subtitleListView1.ColumnIndexEnd]) + ">" + GetHtmlText(itemRight, itemRight.EndTime.ToShortDisplayString()) + "</td>");
                        }
                        sb.AppendLine("      <td" + GetHtmlBackgroundColor(subtitleListView2.Items[i].SubItems[subtitleListView1.ColumnIndexText]) + ">" + GetHtmlText(itemRight, itemRight.Text) + "</td>");
                        sb.AppendLine("    </tr>");
                    }
                }
                sb.AppendLine("    <tr>");
                sb.AppendLine("      <td colspan='9' style='text-align:left'><br />" + labelStatus.Text + "</td>");
                sb.AppendLine("    </tr>");
                sb.AppendLine("    </table>");
                sb.AppendLine("  </body>");
                sb.AppendLine("</html>");
                File.WriteAllText(fileName, sb.ToString());
            }
        }

        private static string GetFileName(string fileName)
        {
            return string.IsNullOrEmpty(fileName) ? string.Empty : Path.GetFileName(fileName);
        }

        private static string GetHtmlText(Paragraph p, string text)
        {
            return p.IsDefault ? string.Empty : HtmlUtil.EncodeNamed(text);
        }

        private static string GetHtmlBackgroundColor(ListViewItem.ListViewSubItem item)
        {
            if (item.BackColor == DefaultBackColor)
            {
                return string.Empty;
            }

            return " style='background-color:" + ColorTranslator.ToHtml(item.BackColor) + "'";
        }

        private void checkBoxIgnoreWhiteSpace_CheckedChanged(object sender, EventArgs e)
        {
            var selectedIndices = subtitleListView1.GetSelectedIndices();
            var selectedIndex = selectedIndices.Length > 0 ? selectedIndices[0] : 0;
            CompareSubtitles(selectedIndex);
        }
    }
}
