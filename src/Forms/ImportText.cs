using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ImportText : Form
    {
        Subtitle _subtitle;
        string _videoFileName;
        private readonly Timer _refreshTimer = new Timer();

        public Subtitle FixedSubtitle { get { return _subtitle; } }
        public string VideoFileName { get { return _videoFileName; } }

        public ImportText()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.ImportText.Title;
            groupBoxImportText.Text = Configuration.Settings.Language.ImportText.Title;
            buttonOpenText.Text = Configuration.Settings.Language.ImportText.OpenTextFile;
            groupBoxImportOptions.Text = Configuration.Settings.Language.ImportText.ImportOptions;
            groupBoxSplitting.Text = Configuration.Settings.Language.ImportText.Splitting;
            radioButtonAutoSplit.Text = Configuration.Settings.Language.ImportText.AutoSplitText;
            radioButtonLineMode.Text = Configuration.Settings.Language.ImportText.OneLineIsOneSubtitle;
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.ImportText.SplitAtBlankLines)) //TODO: Fix in SE 3.3
                radioButtonSplitAtBlankLines.Text = Configuration.Settings.Language.ImportText.SplitAtBlankLines;
            else
                radioButtonSplitAtBlankLines.Visible = false;
            checkBoxMergeShortLines.Text = Configuration.Settings.Language.ImportText.MergeShortLines;
            checkBoxRemoveEmptyLines.Text = Configuration.Settings.Language.ImportText.RemoveEmptyLines;
            checkBoxRemoveLinesWithoutLetters.Text = Configuration.Settings.Language.ImportText.RemoveLinesWithoutLetters;
            labelGapBetweenSubtitles.Text = Configuration.Settings.Language.ImportText.GapBetweenSubtitles;
            groupBoxDuration.Text = Configuration.Settings.Language.General.Duration;
            radioButtonDurationAuto.Text = Configuration.Settings.Language.ImportText.Auto;
            radioButtonDurationFixed.Text = Configuration.Settings.Language.ImportText.Fixed;
            buttonRefresh.Text = Configuration.Settings.Language.ImportText.Refresh;
            groupBoxImportResult.Text = Configuration.Settings.Language.General.Preview;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);

            numericUpDownDurationFixed.Enabled = radioButtonDurationFixed.Checked;
            FixLargeFonts();
            _refreshTimer.Interval = 400;
            _refreshTimer.Tick += RefreshTimerTick;
        }

        void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void ButtonOpenTextClick(object sender, EventArgs e)
        {
            openFileDialog1.Title = buttonOpenText.Text;
            openFileDialog1.Filter = Configuration.Settings.Language.ImportText.TextFiles + "|*.txt|Adobe Story|*.astx|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(openFileDialog1.FileName).ToLower();
                if (ext == ".astx")
                    LoadAdobeStory(openFileDialog1.FileName);
                else
                    LoadTextFile(openFileDialog1.FileName);
            }
        }

        private void GeneratePreview()
        {
            if (_refreshTimer.Enabled)
            {
                _refreshTimer.Stop();
                _refreshTimer.Start();
            }
            else
            {
                _refreshTimer.Start();
            }
        }

        private void GeneratePreviewReal()
        {
            _subtitle = new Subtitle();
            if (radioButtonLineMode.Checked)
                ImportLineMode(textBoxText.Lines);
            else if (radioButtonAutoSplit.Checked)
                ImportAutoSplit(textBoxText.Lines);
            else
                ImportSplitAtBlankLine(textBoxText.Lines);

            if (checkBoxMergeShortLines.Checked)
                MergeLinesWithContinuation();

            _subtitle.Renumber(1);
            FixDurations();
            MakePseudoStartTime();

            groupBoxImportResult.Text = string.Format(Configuration.Settings.Language.ImportText.PreviewLinesModifiedX, _subtitle.Paragraphs.Count);
            SubtitleListview1.Fill(_subtitle);
            SubtitleListview1.SelectIndexAndEnsureVisible(0);
        }

        private void MergeLinesWithContinuation()
        {
            var temp = new Subtitle();
            bool skipNext = false;
            for (int i=0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (!skipNext)
                {
                    Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);

                    bool merge = !(p.Text.Contains(Environment.NewLine) || next == null);

                    if (merge && (p.Text.TrimEnd().EndsWith("!") || p.Text.TrimEnd().EndsWith(".") || p.Text.TrimEnd().EndsWith("!")))
                    {
                        var st = new StripableText(p.Text);
                        if (st.StrippedText.Length > 0 && Utilities.UppercaseLetters.Contains(st.StrippedText[0].ToString(CultureInfo.InvariantCulture)))
                            merge = false;
                    }

                    if (merge && (p.Text.Length >= Configuration.Settings.General.SubtitleLineMaximumLength - 5 || next.Text.Length >= Configuration.Settings.General.SubtitleLineMaximumLength - 5))
                        merge = false;

                    if (merge)
                    {
                        temp.Paragraphs.Add(new Paragraph { Text = p.Text + Environment.NewLine + next.Text });
                        skipNext = true;
                    }
                    else
                    {
                        temp.Paragraphs.Add(new Paragraph(p));
                    }
                }
                else
                {
                    skipNext = false;
                }
            }
            _subtitle = temp;
        }

        private void MakePseudoStartTime()
        {
            double millisecondsInterval = (double)numericUpDownGapBetweenLines.Value;
            double millisecondsIndex = millisecondsInterval;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                p.EndTime.TotalMilliseconds = millisecondsIndex + p.Duration.TotalMilliseconds;
                p.StartTime.TotalMilliseconds = millisecondsIndex;

                millisecondsIndex += p.Duration.TotalMilliseconds + millisecondsInterval;
            }
        }

        private void FixDurations()
        {
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.Text.Length == 0)
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 2000;
                }
                else
                {
                    if (radioButtonDurationAuto.Checked)
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + (Utilities.GetDisplayMillisecondsFromText(p.Text) * 1.2);
                    else
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + ((double)numericUpDownDurationFixed.Value);
                }
            }
        }

        private void ImportLineMode(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                if (line.Trim().Length == 0)
                {
                    if (!checkBoxRemoveEmptyLines.Checked)
                        _subtitle.Paragraphs.Add(new Paragraph());
                }
                else if (!ContainsLetters(line))
                {
                    if (!checkBoxRemoveLinesWithoutLetters.Checked)
                        _subtitle.Paragraphs.Add(new Paragraph(0, 0, line.Trim()));
                }
                else
                {
                    _subtitle.Paragraphs.Add(new Paragraph(0, 0, line.Trim()));
                }
            }
        }

        private void ImportSplitAtBlankLine(IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {

                if (line.Trim().Length == 0)
                {
                    if (sb.Length > 0)
                        SplitSingle(sb);
                    sb = new StringBuilder();
                }
                else if (!ContainsLetters(line))
                {
                    if (!checkBoxRemoveLinesWithoutLetters.Checked)
                        sb.AppendLine(line.Trim());
                }
                else
                {
                    sb.AppendLine(line.Trim());
                }
            }
            if (sb.Length > 0)
                SplitSingle(sb);
        }

        private bool CanMakeThreeLiner(out string text, string input)
        {
            text = string.Empty;
            if (input.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 3 && input.Length > Configuration.Settings.General.SubtitleLineMaximumLength * 1.5)
            {
                var breaked = Utilities.AutoBreakLine(input).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (breaked.Length == 2 && (breaked[0].Length > Configuration.Settings.General.SubtitleLineMaximumLength || breaked[1].Length > Configuration.Settings.General.SubtitleLineMaximumLength))
                {
                    var first = new StringBuilder();
                    var second = new StringBuilder();
                    var third  = new StringBuilder();
                    foreach (string word in input.Replace(Environment.NewLine, " ").Replace("  ", " ").Split(' '))
                    {
                        if (first.Length + word.Length < Configuration.Settings.General.SubtitleLineMaximumLength)
                            first.Append(" " + word);
                        else if (second.Length + word.Length < Configuration.Settings.General.SubtitleLineMaximumLength)
                            second.Append(" " + word);
                        else
                            third.Append(" " + word);
                    }
                    if (first.Length <= Configuration.Settings.General.SubtitleLineMaximumLength &&
                        second.Length <= Configuration.Settings.General.SubtitleLineMaximumLength &&
                        third.Length <= Configuration.Settings.General.SubtitleLineMaximumLength &&
                        third.Length > 10)
                    {
                        if (second.Length > 15)
                        {
                            string ending = second.ToString().Substring(second.Length - 9);
                            int splitPos = -1;
                            if (ending.Contains(". "))
                                splitPos = ending.IndexOf(". ") + second.Length - 9;
                            else if (ending.Contains("! "))
                                splitPos = ending.IndexOf("! ") + second.Length - 9;
                            else if (ending.Contains(", "))
                                splitPos = ending.IndexOf(", ") + second.Length - 9;
                            else if (ending.Contains("? "))
                                splitPos = ending.IndexOf("? ") + second.Length - 9;
                            if (splitPos > 0)
                            {
                                text = Utilities.AutoBreakLine(first.ToString().Trim() + second.ToString().Substring(0, splitPos+1)).Trim() + Environment.NewLine + (second.ToString().Substring(splitPos+1) + third).Trim();
                                return true;
                            }
                        }


                        text = first + Environment.NewLine + second + Environment.NewLine + third;
                        return true;
                    }
                }
            }
            return false;
        }

        private void SplitSingle(StringBuilder sb)
        {
            Paragraph p = null;
            string threeliner;
            if (CanMakeThreeLiner(out threeliner, sb.ToString()))
            {
                var parts = threeliner.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                _subtitle.Paragraphs.Add(new Paragraph() { Text = parts[0] + Environment.NewLine + parts[1] });
                _subtitle.Paragraphs.Add(new Paragraph() { Text = parts[2].Trim()});
                return;
            }

            foreach (string text in Utilities.AutoBreakLineMoreThanTwoLines(sb.ToString(), Configuration.Settings.General.SubtitleLineMaximumLength).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (p == null)
                {
                    p = new Paragraph();
                    p.Text = text;
                }
                else if (p.Text.Contains(Environment.NewLine))
                {
                    _subtitle.Paragraphs.Add(p);
                    p = null;
                    p = new Paragraph();
                    p.Text = text;
                }
                else
                {
                    p.Text = p.Text + Environment.NewLine + text.Trim();
                }
            }
            if (p != null)
                _subtitle.Paragraphs.Add(p);
        }

        private void ImportAutoSplit(IEnumerable<string> textLines)
        {
            var sb = new StringBuilder();
            foreach (string line in textLines)
            {
                if (line.Trim().Length == 0)
                {
                    if (!checkBoxRemoveEmptyLines.Checked)
                        sb.AppendLine();
                }
                else if (!ContainsLetters(line.Trim()))
                {
                    if (!checkBoxRemoveLinesWithoutLetters.Checked)
                        sb.AppendLine(line);
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            string text = sb.ToString().Replace(Environment.NewLine, " ");

            while (text.Contains("  "))
                text = text.Replace("  ", " ");

             text = text.Replace("!", "_@EXM_");
             text = text.Replace("?", "_@QST_");
             text = text.Replace(".", "_@PER_");

            string[] lines = text.Split('.');

            for (int i=0; i<lines.Length; i++)
            {
                lines[i] = lines[i].Replace("_@EXM_", "!");
                lines[i] = lines[i].Replace("_@QST_", "?");
                lines[i] = lines[i].Replace("_@PER_", ".");
            }

            var list = new List<string>();
            foreach (string s in lines)
                AutoSplit(list, s);

            ImportLineMode(list);
        }

        private void AutoSplit(List<string> list, string line)
        {
            foreach (string split in Utilities.AutoBreakLine(line).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (split.Length <= Configuration.Settings.General.SubtitleLineMaximumLength)
                    list.Add(split);
                else if (split != line)
                    AutoSplit(list, split);
                else
                {
                    string s = split.Trim();
                    if (s.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                        s = s.Insert(split.Length/2, Environment.NewLine);
                    list.Add(s);
                }
            }
        }

        private bool ContainsLetters(string line)
        {
            if (line.Replace("0", string.Empty).Replace("1", string.Empty).Replace("2", string.Empty).Replace("3", string.Empty).Replace("4", string.Empty).Replace("5", string.Empty).Replace("6", string.Empty)
                .Replace("7", string.Empty).Replace("8", string.Empty).Replace("9", string.Empty).Replace(":", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty).
                Replace("-", string.Empty).Replace(">", string.Empty).Trim().Length == 0)
                return false;

            foreach (char ch in line)
            {
                if (!("\r\n\t .?" + Convert.ToChar(0)).Contains(ch.ToString(CultureInfo.InvariantCulture)))
                    return true;
            }
            return false;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (SubtitleListview1.Items.Count > 0)
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void CheckBoxRemoveLinesWithoutLettersOrNumbersCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void CheckBoxRemoveEmptyLinesCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void RadioButtonLineModeCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void RadioButtonAutoSplitCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void TextBoxTextDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void TextBoxTextDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                LoadTextFile(files[0]);
            }
        }

        private void LoadTextFile(string fileName)
        {
            try
            {
                Encoding encoding = Utilities.GetEncodingFromFile(fileName);
                textBoxText.Text = File.ReadAllText(fileName, encoding);
                SetVideoFileName(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            GeneratePreview();
        }

        private void LoadAdobeStory(string fileName)
        {
            try
            {
                var sb = new StringBuilder();
                var doc = new XmlDocument();
                doc.Load(fileName);
                foreach (XmlNode node in doc.DocumentElement.SelectNodes("//paragraph[@element='Dialog']")) // <paragraph objID="1:28" element="Dialog">
                {
                    XmlNode textRun = node.SelectSingleNode("textRun"); // <textRun objID="1:259">Yeah...I suppose</textRun>
                    if (textRun != null)
                        sb.AppendLine(textRun.InnerText);
                }
                textBoxText.Text = sb.ToString();
                SetVideoFileName(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            radioButtonLineMode.Checked = true;
            checkBoxMergeShortLines.Checked = false;
            GeneratePreview();
        }

        private void SetVideoFileName(string fileName)
        {
            _videoFileName = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length);
            if (_videoFileName.EndsWith(".en"))
                _videoFileName = _videoFileName.Remove(_videoFileName.Length - 3);
            if (File.Exists(_videoFileName + ".avi"))
            {
                _videoFileName += ".avi";
            }
            else if (File.Exists(_videoFileName + ".mkv"))
            {
                _videoFileName += ".mkv";
            }
            else
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(_videoFileName) + "*.avi");
                if (files.Length == 0)
                    files = Directory.GetFiles(Path.GetDirectoryName(fileName), "*.avi");
                if (files.Length == 0)
                    files = Directory.GetFiles(Path.GetDirectoryName(fileName), "*.mkv");
                if (files.Length > 0)
                    _videoFileName = files[0];
            }
        }

        private void ButtonRefreshClick(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void RadioButtonDurationFixedCheckedChanged(object sender, EventArgs e)
        {
            numericUpDownDurationFixed.Enabled = radioButtonDurationFixed.Checked;
            GeneratePreview();
        }

        private void CheckBoxMergeShortLinesCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void ImportTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void TextBoxTextTextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void NumericUpDownDurationFixedValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void NumericUpDownGapBetweenLinesValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void RadioButtonDurationAutoCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void radioButtonSplitAtBlankLines_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

    }
}
