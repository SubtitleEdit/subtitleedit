using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Compare : Form
    {
        Subtitle _subtitle1;
        Subtitle _subtitle2;
        List<int> _differences;
        bool _listView2Focused;

        public Compare()
        {
            InitializeComponent();

            labelSubtitle2.Text = string.Empty;
            Text = Configuration.Settings.Language.CompareSubtitles.Title;
            buttonPreviousDifference.Text = Configuration.Settings.Language.CompareSubtitles.PreviousDifference;
            buttonNextDifference.Text = Configuration.Settings.Language.CompareSubtitles.NextDifference;
            checkBoxShowOnlyDifferences.Text = Configuration.Settings.Language.CompareSubtitles.ShowOnlyDifferences;
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.CompareSubtitles.OnlyLookForDifferencesInText))
                checkBoxOnlyListDifferencesInText.Text = Configuration.Settings.Language.CompareSubtitles.OnlyLookForDifferencesInText;
            else
                checkBoxOnlyListDifferencesInText.Visible = false;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListView2.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                subtitleListView1.InitializeTimeStampColumWidths(this);
                subtitleListView2.InitializeTimeStampColumWidths(this);
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(Subtitle subtitle1, string subtitleFileName1, string title)
        {
            subtitleListView1.UseSyntaxColoring = false;
            subtitleListView2.UseSyntaxColoring = false;

            Compare_Resize(null, null);
            labelStatus.Text = string.Empty;
            _subtitle1 = subtitle1;
            labelSubtitle1.Text = subtitleFileName1;
            if (!string.IsNullOrEmpty(title))
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
                }
            }

            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
        }

        public void Initialize(Subtitle subtitle1, string subtitleFileName1, Subtitle subtitle2, string subtitleFileName2)
        {
            Compare_Resize(null, null);
            labelStatus.Text = string.Empty;
            _subtitle1 = subtitle1;
            labelSubtitle1.Text = subtitleFileName1;

            _subtitle2 = subtitle2;
            labelSubtitle2.Text = subtitleFileName2;

            CompareSubtitles();

            if (string.IsNullOrEmpty(subtitleFileName1))
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(subtitleFileName1);
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
        }

        private void ButtonOpenSubtitle1Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (Main.HasVobSubHeader(openFileDialog1.FileName) || Main.IsBluRaySupFile(openFileDialog1.FileName))
                {
                    MessageBox.Show(Configuration.Settings.Language.CompareSubtitles.CannotCompareWithImageBasedSubtitles);
                    return;
                }
                _subtitle1 = new Subtitle();
                Encoding encoding;
                _subtitle1.LoadSubtitle(openFileDialog1.FileName, out encoding, null);
                subtitleListView1.Fill(_subtitle1);
                labelSubtitle1.Text = openFileDialog1.FileName;
                if (_subtitle1.Paragraphs.Count > 0)
                    CompareSubtitles();
            }
        }

        private void ButtonOpenSubtitle2Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (Main.HasVobSubHeader(openFileDialog1.FileName) || Main.IsBluRaySupFile(openFileDialog1.FileName))
                {
                    MessageBox.Show(Configuration.Settings.Language.CompareSubtitles.CannotCompareWithImageBasedSubtitles);
                    return;
                }
                _subtitle2 = new Subtitle();
                Encoding encoding;
                _subtitle2.LoadSubtitle(openFileDialog1.FileName, out encoding, null);
                subtitleListView2.Fill(_subtitle2);
                labelSubtitle2.Text = openFileDialog1.FileName;
                if (_subtitle2.Paragraphs.Count > 0)
                    CompareSubtitles();
            }
        }

        private void CompareSubtitles()
        {
            timer1.Stop();
            var sub1 = new Subtitle(_subtitle1);
            var sub2 = new Subtitle(_subtitle2);

            int index = 0;
            Paragraph p1 = sub1.GetParagraphOrDefault(index);
            Paragraph p2 = sub2.GetParagraphOrDefault(index);
            int max = sub1.Paragraphs.Count;
            if (max < sub2.Paragraphs.Count)
                max = sub2.Paragraphs.Count;
            while (index < max)
            {
                if (p1 != null && p2 != null)
                {
                    if (p1.ToString() == p2.ToString())
                    {
                    }
                    else
                    {
                        if (GetColumnsEqualExceptNumber(p1, p2) == 0)
                        {
                            int oldIndex = index;
                            for (int i = 1; oldIndex + i < max; i++)
                            {
                                if (GetColumnsEqualExceptNumber(sub1.GetParagraphOrDefault(index + i), p2) > 1)
                                {
                                    for (int j = 0; j < i; j++)
                                    {
                                        sub2.Paragraphs.Insert(index, new Paragraph());
                                        index++;
                                    }
                                    break;
                                }
                                else if (GetColumnsEqualExceptNumber(p1, sub2.GetParagraphOrDefault(index + i)) > 1)
                                {
                                    for (int j = 0; j < i; j++)
                                    {
                                        sub1.Paragraphs.Insert(index, new Paragraph());
                                        index++;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                index++;
                p1 = sub1.GetParagraphOrDefault(index);
                p2 = sub2.GetParagraphOrDefault(index);
            }
            subtitleListView1.Fill(sub1);
            subtitleListView2.Fill(sub2);


            // coloring + differences index list
            _differences = new List<int>();
            index = 0;
            p1 = sub1.GetParagraphOrDefault(index);
            p2 = sub2.GetParagraphOrDefault(index);

            if (checkBoxOnlyListDifferencesInText.Checked)
            {
                while (index < sub1.Paragraphs.Count || index < sub2.Paragraphs.Count)
                {
                    if (p1 != null && p2 != null)
                    {

                        if (p1.ToString() == p2.ToString() && p1.Number == p2.Number)
                        { // no differences
                        }
                        else if (p1.ToString() == new Paragraph().ToString())
                        {
                            _differences.Add(index);
                            subtitleListView1.ColorOut(index, Color.Salmon);
                        }
                        else if (p2.ToString() == new Paragraph().ToString())
                        {
                            _differences.Add(index);
                            subtitleListView2.ColorOut(index, Color.Salmon);
                        }
                        else if (p1.Text != p2.Text)
                        {
                            _differences.Add(index);
                            subtitleListView1.SetBackgroundColor(index, Color.LightGreen, SubtitleListView.ColumnIndexText);
                        }
                    }
                    else
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
                while (index < sub1.Paragraphs.Count || index < sub2.Paragraphs.Count)
                {
                    if (p1 != null && p2 != null)
                    {
                        if (p1.ToString() != p2.ToString())
                            _differences.Add(index);

                        if (p1.ToString() == p2.ToString() && p1.Number == p2.Number)
                        { // no differences
                        }
                        else if (p1.ToString() == new Paragraph().ToString())
                        {
                            subtitleListView1.ColorOut(index, Color.Salmon);
                        }
                        else if (p2.ToString() == new Paragraph().ToString())
                        {
                            subtitleListView2.ColorOut(index, Color.Salmon);
                        }
                        else
                        {
                            int columnsAlike = GetColumnsEqualExceptNumber(p1, p2);
                            if (columnsAlike > 0)
                            {
                                if (p1.StartTime.TotalMilliseconds != p2.StartTime.TotalMilliseconds)
                                {
                                    subtitleListView1.SetBackgroundColor(index, Color.LightGreen,
                                                                         SubtitleListView.ColumnIndexStart);
                                    subtitleListView2.SetBackgroundColor(index, Color.LightGreen,
                                                                         SubtitleListView.ColumnIndexStart);
                                }
                                if (p1.EndTime.TotalMilliseconds != p2.EndTime.TotalMilliseconds)
                                {
                                    subtitleListView1.SetBackgroundColor(index, Color.LightGreen,
                                                                         SubtitleListView.ColumnIndexEnd);
                                    subtitleListView2.SetBackgroundColor(index, Color.LightGreen,
                                                                         SubtitleListView.ColumnIndexEnd);
                                }
                                if (p1.Duration.TotalMilliseconds != p2.Duration.TotalMilliseconds)
                                {
                                    subtitleListView1.SetBackgroundColor(index, Color.LightGreen,
                                                                         SubtitleListView.ColumnIndexDuration);
                                    subtitleListView2.SetBackgroundColor(index, Color.LightGreen,
                                                                         SubtitleListView.ColumnIndexDuration);
                                }
                                if (p1.Text.Trim() != p2.Text.Trim())
                                {
                                    subtitleListView1.SetBackgroundColor(index, Color.LightGreen,
                                                                         SubtitleListView.ColumnIndexText);
                                    subtitleListView2.SetBackgroundColor(index, Color.LightGreen,
                                                                         SubtitleListView.ColumnIndexText);
                                }

                                if (p1.Number != p2.Number)
                                {
                                    subtitleListView1.SetBackgroundColor(index, Color.LightYellow,
                                                                         SubtitleListView.ColumnIndexNumber);
                                    subtitleListView2.SetBackgroundColor(index, Color.LightYellow,
                                                                         SubtitleListView.ColumnIndexNumber);
                                }
                            }
                        }
                    }
                    else
                    {
                        _differences.Add(index);
                    }
                    index++;
                    p1 = sub1.GetParagraphOrDefault(index);
                    p2 = sub2.GetParagraphOrDefault(index);
                }
            }
            UpdatePreviousAndNextButtons();

            if (max == _differences.Count)
            {
                labelStatus.Text = Configuration.Settings.Language.CompareSubtitles.SubtitlesNotAlike;
                labelStatus.Font = new Font(labelStatus.Font.FontFamily, labelStatus.Font.Size, FontStyle.Bold);
            }
            else
            {
                labelStatus.Text = string.Format(Configuration.Settings.Language.CompareSubtitles.XNumberOfDifference, _differences.Count);
                labelStatus.Font = new Font(labelStatus.Font.FontFamily, labelStatus.Font.Size);
            }

            if (checkBoxShowOnlyDifferences.Checked)
            { // Remove all lines with no difference
                subtitleListView1.BeginUpdate();
                subtitleListView2.BeginUpdate();
                if (max != _differences.Count)
                {
                    for (index = Math.Max(subtitleListView1.Items.Count, subtitleListView2.Items.Count); index >= 0; index--)
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
                for (index = 0; index < Math.Max(subtitleListView1.Items.Count, subtitleListView2.Items.Count); index++)
                    _differences.Add(index);
            }
            timer1.Start();
            subtitleListView1.FirstVisibleIndex = -1;
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private static int GetColumnsEqualExceptNumber(Paragraph p1, Paragraph p2)
        {
            if (p1 == null || p2 == null)
                return 0;

            int columnsEqual = 0;
            if (p1.StartTime.TotalMilliseconds == p2.StartTime.TotalMilliseconds)
                columnsEqual++;

            if (p1.EndTime.TotalMilliseconds == p2.EndTime.TotalMilliseconds)
                columnsEqual++;

            if (p1.Duration.TotalMilliseconds == p2.Duration.TotalMilliseconds)
                columnsEqual++;

            if (p1.Text.Trim() == p2.Text.Trim())
                columnsEqual++;

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

            //showdiff
            if (text1 == text2)
                return;

            if (text1.Trim().Length == 0 || text2.Trim().Length == 0)
                return;

            ShowTextDifference();
        }

        private void ShowTextDifference()
        {
            // from start
            int minLength = Math.Min(richTextBox1.Text.Length, richTextBox2.Text.Length);
            int startCharactersOk = 0;
            for (int i=0; i < minLength; i++)
            {
                if (richTextBox1.Text[i] == richTextBox2.Text[i])
                {
                    startCharactersOk++;
                }
                else
                {
                    if (richTextBox1.Text.Length > i+4 && richTextBox2.Text.Length > i+4 &&
                        richTextBox1.Text[i + 1] == richTextBox2.Text[i + 1] &&
                        richTextBox1.Text[i + 2] == richTextBox2.Text[i + 2] &&
                        richTextBox1.Text[i + 3] == richTextBox2.Text[i + 3] &&
                        richTextBox1.Text[i + 4] == richTextBox2.Text[i + 4])
                    {
                        startCharactersOk++;

                        richTextBox1.SelectionStart = i;
                        richTextBox1.SelectionLength = 1;
                        richTextBox1.SelectionColor = Color.Red;
                        if (richTextBox1.SelectedText.Trim() == string.Empty)
                            richTextBox1.SelectionBackColor = Color.Red;

                        richTextBox2.SelectionStart = i;
                        richTextBox2.SelectionLength = 1;
                        richTextBox2.SelectionColor = Color.Red;
                        if (richTextBox2.SelectedText.Trim() == string.Empty)
                            richTextBox2.SelectionBackColor = Color.Red;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            int maxLength = Math.Max(richTextBox1.Text.Length, richTextBox2.Text.Length);
            for (int i = startCharactersOk; i <= maxLength; i++)
            {
                if (i < richTextBox1.Text.Length)
                {
                    richTextBox1.SelectionStart = i;
                    richTextBox1.SelectionLength = 1;
                    richTextBox1.SelectionBackColor = Color.Red;
                    if (richTextBox1.SelectedText.Trim() == string.Empty)
                        richTextBox1.SelectionBackColor = Color.Red;
                }
                if (i < richTextBox2.Text.Length)
                {
                    richTextBox2.SelectionStart = i;
                    richTextBox2.SelectionLength = 1;
                    richTextBox2.SelectionColor = Color.Red;
                    if (richTextBox2.SelectedText.Trim() == string.Empty)
                        richTextBox2.SelectionBackColor = Color.Red;
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
                    if (i == 0 && i > 4 &&
                        richTextBox1.Text[richTextBox1.Text.Length - (i + 1)] == richTextBox2.Text[richTextBox2.Text.Length - (i + 1)] &&
                        richTextBox1.Text[richTextBox1.Text.Length - (i + 2)] == richTextBox2.Text[richTextBox2.Text.Length - (i + 2)] &&
                        richTextBox1.Text[richTextBox1.Text.Length - (i + 3)] == richTextBox2.Text[richTextBox2.Text.Length - (i + 3)] &&
                        richTextBox1.Text[richTextBox1.Text.Length - (i + 4)] == richTextBox2.Text[richTextBox2.Text.Length - (i + 4)])
                    {
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // special situation - equal, but one has more chars
            if (richTextBox1.Text.Length > richTextBox2.Text.Length)
            {
                if (richTextBox1.Text.StartsWith(richTextBox2.Text))
                {
                    richTextBox1.SelectionStart = richTextBox2.Text.Length;
                    richTextBox1.SelectionLength = richTextBox1.Text.Length - richTextBox2.Text.Length;
                    richTextBox1.SelectionBackColor = Color.Red;
                }
            }
            else if (richTextBox2.Text.Length > richTextBox1.Text.Length)
            {
                if (richTextBox2.Text.StartsWith(richTextBox1.Text))
                {
                    richTextBox2.SelectionStart = richTextBox1.Text.Length;
                    richTextBox2.SelectionLength = richTextBox2.Text.Length - richTextBox1.Text.Length;
                    richTextBox2.SelectionColor = Color.Red;
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
                    buttonNextDifference.Enabled = _differences[_differences.Count-1] > index;
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

        private void Compare_Shown(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(labelSubtitle2.Text))
                ButtonOpenSubtitle2Click(null, null);
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
                    for (int i = subtitleListView1.SelectedItems[0].Index-1; i >= 0; i--)
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
            if (subtitleListView1.SelectedItems.Count > 0 && _listView2Focused == false)
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
            else if (subtitleListView2.SelectedItems.Count > 0 && _listView2Focused)
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

        private void SubtitleListView2Enter(object sender, EventArgs e)
        {
            _listView2Focused = true;
        }

        private void SubtitleListView2Leave(object sender, EventArgs e)
        {
            _listView2Focused = false;
        }

        private void Compare_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }

        private void checkBoxShowOnlyDifferences_CheckedChanged(object sender, EventArgs e)
        {
            CompareSubtitles();
        }

        private void checkBoxOnlyListDifferencesInText_CheckedChanged(object sender, EventArgs e)
        {
            CompareSubtitles();
        }

    }
}