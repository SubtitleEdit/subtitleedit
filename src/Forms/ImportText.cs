using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ImportText : Form
    {
        private Subtitle _subtitle;
        private string _videoFileName;
        private readonly Timer _refreshTimer = new Timer();

        public Subtitle FixedSubtitle => _subtitle;
        public string VideoFileName => _videoFileName;

        public ImportText(string fileName = null)
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.ImportText.Title;
            groupBoxImportText.Text = Configuration.Settings.Language.ImportText.Title;
            buttonOpenText.Text = Configuration.Settings.Language.ImportText.OpenTextFile;
            groupBoxImportOptions.Text = Configuration.Settings.Language.ImportText.ImportOptions;
            groupBoxSplitting.Text = Configuration.Settings.Language.ImportText.Splitting;
            radioButtonAutoSplit.Text = Configuration.Settings.Language.ImportText.AutoSplitText;
            radioButtonLineMode.Text = Configuration.Settings.Language.ImportText.OneLineIsOneSubtitle;
            labelLineBreak.Left = radioButtonLineMode.Left + radioButtonLineMode.Width + 10;
            labelLineBreak.Text = Configuration.Settings.Language.ImportText.LineBreak;
            columnHeaderFName.Text = Configuration.Settings.Language.JoinSubtitles.FileName;
            columnHeaderSize.Text = Configuration.Settings.Language.General.Size;
            comboBoxLineBreak.Left = labelLineBreak.Left + labelLineBreak.Width + 3;
            comboBoxLineBreak.Width = groupBoxSplitting.Width - comboBoxLineBreak.Left - 5;
            checkBoxMultipleFiles.AutoSize = true;
            checkBoxMultipleFiles.Left = buttonOpenText.Left - checkBoxMultipleFiles.Width - 9;
            checkBoxMultipleFiles.Text = Configuration.Settings.Language.ImportText.OneSubtitleIsOneFile;
            listViewInputFiles.Visible = false;

            radioButtonSplitAtBlankLines.Text = Configuration.Settings.Language.ImportText.SplitAtBlankLines;
            checkBoxMergeShortLines.Text = Configuration.Settings.Language.ImportText.MergeShortLines;
            checkBoxRemoveEmptyLines.Text = Configuration.Settings.Language.ImportText.RemoveEmptyLines;
            checkBoxRemoveLinesWithoutLetters.Text = Configuration.Settings.Language.ImportText.RemoveLinesWithoutLetters;
            checkBoxGenerateTimeCodes.Text = Configuration.Settings.Language.ImportText.GenerateTimeCodes;
            checkBoxAutoBreak.Text = Configuration.Settings.Language.Settings.MainTextBoxAutoBreak;
            labelGapBetweenSubtitles.Text = Configuration.Settings.Language.ImportText.GapBetweenSubtitles;
            numericUpDownGapBetweenLines.Left = labelGapBetweenSubtitles.Left + labelGapBetweenSubtitles.Width + 3;
            groupBoxDuration.Text = Configuration.Settings.Language.General.Duration;
            radioButtonDurationAuto.Text = Configuration.Settings.Language.ImportText.Auto;
            radioButtonDurationFixed.Text = Configuration.Settings.Language.ImportText.Fixed;
            buttonRefresh.Text = Configuration.Settings.Language.ImportText.Refresh;
            groupBoxTimeCodes.Text = Configuration.Settings.Language.ImportText.TimeCodes;
            groupBoxImportResult.Text = Configuration.Settings.Language.General.Preview;
            clearToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubRip.Clear;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.ImportTextSplitting))
                radioButtonAutoSplit.Checked = true;
            else if (Configuration.Settings.Tools.ImportTextSplitting.Equals("blank lines", StringComparison.OrdinalIgnoreCase))
                radioButtonSplitAtBlankLines.Checked = true;
            else if (Configuration.Settings.Tools.ImportTextSplitting.Equals("line", StringComparison.OrdinalIgnoreCase))
                radioButtonLineMode.Checked = true;
            comboBoxLineBreak.Text = Configuration.Settings.Tools.ImportTextLineBreak;
            checkBoxMergeShortLines.Checked = Configuration.Settings.Tools.ImportTextMergeShortLines;
            checkBoxRemoveEmptyLines.Checked = Configuration.Settings.Tools.ImportTextRemoveEmptyLines;
            checkBoxRemoveLinesWithoutLetters.Checked = Configuration.Settings.Tools.ImportTextRemoveLinesNoLetters;
            checkBoxGenerateTimeCodes.Checked = Configuration.Settings.Tools.ImportTextGenerateTimeCodes;
            checkBoxAutoBreak.Checked = Configuration.Settings.Tools.ImportTextAutoBreak;
            if (Configuration.Settings.Tools.ImportTextGap >= numericUpDownGapBetweenLines.Minimum && Configuration.Settings.Tools.ImportTextGap <= numericUpDownGapBetweenLines.Maximum)
            {
                numericUpDownGapBetweenLines.Value = Configuration.Settings.Tools.ImportTextGap;
            }
            if (Configuration.Settings.Tools.ImportTextDurationAuto)
            {
                radioButtonDurationAuto.Checked = true;
            }
            else
            {
                radioButtonDurationFixed.Checked = true;
            }
            numericUpDownDurationFixed.Enabled = radioButtonDurationFixed.Checked;
            if (Configuration.Settings.Tools.ImportTextFixedDuration >= numericUpDownDurationFixed.Minimum &&
                Configuration.Settings.Tools.ImportTextFixedDuration <= numericUpDownDurationFixed.Maximum)
            {
                numericUpDownDurationFixed.Value = Configuration.Settings.Tools.ImportTextFixedDuration;
            }
            UiUtil.FixLargeFonts(this, buttonOK);
            _refreshTimer.Interval = 400;
            _refreshTimer.Tick += RefreshTimerTick;

            if (fileName != null && File.Exists(fileName))
            {
                LoadTextFile(fileName);
                GeneratePreview();
            }
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        private void ButtonOpenTextClick(object sender, EventArgs e)
        {
            Text = Configuration.Settings.Language.ImportText.Title;
            openFileDialog1.Title = buttonOpenText.Text;
            if (checkBoxMultipleFiles.Visible && checkBoxMultipleFiles.Checked)
                openFileDialog1.Filter = Configuration.Settings.Language.ImportText.TextFiles + "|*.txt|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            else
                openFileDialog1.Filter = Configuration.Settings.Language.ImportText.TextFiles + "|*.txt|Adobe Story|*.astx|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Multiselect = checkBoxMultipleFiles.Visible && checkBoxMultipleFiles.Checked;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (checkBoxMultipleFiles.Visible && checkBoxMultipleFiles.Checked)
                {
                    foreach (string fileName in openFileDialog1.FileNames)
                        AddInputFile(fileName);
                }
                else
                {
                    string ext = string.Empty;
                    var extension = Path.GetExtension(openFileDialog1.FileName);
                    if (extension != null)
                        ext = extension.ToLowerInvariant();

                    if (ext == ".astx")
                        LoadAdobeStory(openFileDialog1.FileName);
                    else
                        LoadTextFile(openFileDialog1.FileName);
                }
                GeneratePreview();
            }
        }

        private void GeneratePreview()
        {
            if (radioButtonSplitAtBlankLines.Checked || radioButtonLineMode.Checked)
            {
                checkBoxAutoBreak.Enabled = true;
            }
            else
            {
                checkBoxAutoBreak.Enabled = false;
            }

            if (_refreshTimer.Enabled)
                _refreshTimer.Stop();
            _refreshTimer.Start();
        }

        private void GeneratePreviewReal()
        {
            _subtitle = new Subtitle();
            if (checkBoxMultipleFiles.Visible && checkBoxMultipleFiles.Checked)
                ImportMultipleFiles(listViewInputFiles.Items);
            else if (radioButtonLineMode.Checked)
                ImportLineMode(textBoxText.Lines);
            else if (radioButtonAutoSplit.Checked)
                ImportAutoSplit(textBoxText.Lines);
            else
                ImportSplitAtBlankLine(textBoxText.Lines);

            if (checkBoxMergeShortLines.Checked)
                MergeLinesWithContinuation();

            _subtitle.Renumber();
            if (checkBoxGenerateTimeCodes.Checked)
            {
                FixDurations();
                MakePseudoStartTime();
            }
            else
            {
                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    p.StartTime.TotalMilliseconds = TimeCode.MaxTime.TotalMilliseconds;
                    p.EndTime.TotalMilliseconds = TimeCode.MaxTime.TotalMilliseconds;
                }
            }

            groupBoxImportResult.Text = string.Format(Configuration.Settings.Language.ImportText.PreviewLinesModifiedX, _subtitle.Paragraphs.Count);
            SubtitleListview1.Fill(_subtitle);
            SubtitleListview1.SelectIndexAndEnsureVisible(0);
        }

        private void ImportMultipleFiles(ListView.ListViewItemCollection listViewItemCollection)
        {
            foreach (ListViewItem item in listViewItemCollection)
            {
                string line;
                try
                {
                    line = GetAllText(item.Text).Trim();
                }
                catch
                {
                    line = string.Empty;
                }

                line = line.Replace("|", Environment.NewLine);
                if (comboBoxLineBreak.Text.Length > 0)
                {
                    foreach (string splitter in comboBoxLineBreak.Text.Split(';'))
                    {
                        var tempSplitter = splitter.Trim();
                        if (tempSplitter.Length > 0)
                        {
                            line = line.Replace(tempSplitter, Environment.NewLine);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(line))
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

        private void MergeLinesWithContinuation()
        {
            var temp = new Subtitle();
            bool skipNext = false;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (!skipNext)
                {
                    Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);

                    bool merge = !(p.Text.Contains(Environment.NewLine) || next == null) && Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX > 1;

                    if (merge && (p.Text.TrimEnd().EndsWith('!') || p.Text.TrimEnd().EndsWith('.')))
                    {
                        var st = new StrippableText(p.Text);
                        if (st.StrippedText.Length > 0 && char.IsUpper(st.StrippedText[0]))
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
            var millisecondsInterval = (double)numericUpDownGapBetweenLines.Value;
            double millisecondsIndex = millisecondsInterval;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                p.EndTime.TotalMilliseconds = millisecondsIndex + p.Duration.TotalMilliseconds;
                p.StartTime.TotalMilliseconds = millisecondsIndex;
                millisecondsIndex += (p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds) + millisecondsInterval;
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
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + (Utilities.GetOptimalDisplayMilliseconds(p.Text));
                    else
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + ((double)numericUpDownDurationFixed.Value);
                }
            }
        }

        private void ImportLineMode(IEnumerable<string> lines)
        {
            foreach (string loopLine in lines)
            {
                // Replace user line break character with Environment.NewLine.
                string line = loopLine;
                if (comboBoxLineBreak.Text.Length > 0)
                {
                    foreach (string splitter in comboBoxLineBreak.Text.Split(';'))
                    {
                        var tempSplitter = splitter.Trim();
                        if (tempSplitter.Length > 0)
                        {
                            line = line.Replace(tempSplitter, Environment.NewLine);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(line))
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
                    string text = line.Trim();
                    if (checkBoxAutoBreak.Enabled && checkBoxAutoBreak.Checked)
                        text = Utilities.AutoBreakLine(text);
                    _subtitle.Paragraphs.Add(new Paragraph(0, 0, text));
                }
            }
        }

        private void ImportSplitAtBlankLine(IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (sb.Length > 0)
                    {
                        string text = sb.ToString().Trim();
                        if (checkBoxAutoBreak.Enabled && checkBoxAutoBreak.Checked)
                            text = Utilities.AutoBreakLine(text);
                        _subtitle.Paragraphs.Add(new Paragraph { Text = text });
                        sb.Clear();
                    }
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

        private static bool CanMakeThreeLiner(out string text, string input)
        {
            text = string.Empty;
            if (input.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 3 && input.Length > Configuration.Settings.General.SubtitleLineMaximumLength * 1.5)
            {
                var breaked = Utilities.AutoBreakLine(input).SplitToLines();
                if (breaked.Length == 2 && (breaked[0].Length > Configuration.Settings.General.SubtitleLineMaximumLength || breaked[1].Length > Configuration.Settings.General.SubtitleLineMaximumLength))
                {
                    var first = new StringBuilder();
                    var second = new StringBuilder();
                    var third = new StringBuilder();
                    foreach (string word in input.Replace(Environment.NewLine, " ").Replace("  ", " ").Split(' '))
                    {
                        if (first.Length + word.Length < Configuration.Settings.General.SubtitleLineMaximumLength)
                        {
                            first.Append(' ');
                            first.Append(word);
                        }
                        else if (second.Length + word.Length < Configuration.Settings.General.SubtitleLineMaximumLength)
                        {
                            second.Append(' ');
                            second.Append(word);
                        }
                        else
                        {
                            third.Append(' ');
                            third.Append(word);
                        }
                    }
                    if (third.Length <= Configuration.Settings.General.SubtitleLineMaximumLength && third.Length > 10)
                    {
                        if (second.Length > 15)
                        {
                            string ending = second.ToString().Substring(second.Length - 9);
                            int splitPos = -1;
                            if (ending.Contains(". "))
                                splitPos = ending.IndexOf(". ", StringComparison.Ordinal) + second.Length - 9;
                            else if (ending.Contains("! "))
                                splitPos = ending.IndexOf("! ", StringComparison.Ordinal) + second.Length - 9;
                            else if (ending.Contains(", "))
                                splitPos = ending.IndexOf(", ", StringComparison.Ordinal) + second.Length - 9;
                            else if (ending.Contains("? "))
                                splitPos = ending.IndexOf("? ", StringComparison.Ordinal) + second.Length - 9;
                            if (splitPos > 0)
                            {
                                text = Utilities.AutoBreakLine(first.ToString().Trim() + second.ToString().Substring(0, splitPos + 1)).Trim() + Environment.NewLine + (second.ToString().Substring(splitPos + 1) + third).Trim();
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
            string t = sb.ToString().Trim();
            string[] tarr = t.SplitToLines();
            if (checkBoxMergeShortLines.Checked == false && tarr.Length == 3 &&
                tarr[0].Length < Configuration.Settings.General.SubtitleLineMaximumLength &&
                tarr[1].Length < Configuration.Settings.General.SubtitleLineMaximumLength &&
                tarr[2].Length < Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                _subtitle.Paragraphs.Add(new Paragraph { Text = tarr[0] + Environment.NewLine + tarr[1] });
                return;
            }
            if (checkBoxMergeShortLines.Checked == false && tarr.Length == 2 &&
                tarr[0].Length < Configuration.Settings.General.SubtitleLineMaximumLength &&
                tarr[1].Length < Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                _subtitle.Paragraphs.Add(new Paragraph { Text = tarr[0] + Environment.NewLine + tarr[1] });
                return;
            }
            if (checkBoxMergeShortLines.Checked == false && tarr.Length == 1 && tarr[0].Length < Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                _subtitle.Paragraphs.Add(new Paragraph { Text = tarr[0].Trim() });
                return;
            }

            Paragraph p = null;
            string threeliner;
            if (CanMakeThreeLiner(out threeliner, sb.ToString()))
            {
                var parts = threeliner.SplitToLines();
                _subtitle.Paragraphs.Add(new Paragraph { Text = parts[0] + Environment.NewLine + parts[1] });
                _subtitle.Paragraphs.Add(new Paragraph { Text = parts[2].Trim() });
                return;
            }

            foreach (string text in Utilities.AutoBreakLineMoreThanTwoLines(sb.ToString(), Configuration.Settings.General.SubtitleLineMaximumLength, string.Empty).SplitToLines())
            {
                if (p == null)
                {
                    p = new Paragraph { Text = text };
                }
                else if (p.Text.Contains(Environment.NewLine))
                {
                    _subtitle.Paragraphs.Add(p);
                    p = new Paragraph();
                    if (text.Length >= Configuration.Settings.General.SubtitleLineMaximumLength)
                        p.Text = Utilities.AutoBreakLine(text);
                    else
                        p.Text = text;
                }
                else
                {
                    if (checkBoxMergeShortLines.Checked || p.Text.Length > Configuration.Settings.General.SubtitleLineMaximumLength || text.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                        p.Text = Utilities.AutoBreakLine(p.Text + Environment.NewLine + text.Trim());
                    else
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
                if (string.IsNullOrWhiteSpace(line))
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

            for (int i = 0; i < lines.Length; i++)
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
            foreach (string split in Utilities.AutoBreakLine(line).SplitToLines())
            {
                if (split.Length <= Configuration.Settings.General.SubtitleLineMaximumLength)
                    list.Add(split);
                else if (split != line)
                    AutoSplit(list, split);
                else
                {
                    string s = split.Trim();
                    if (s.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                        s = s.Insert(split.Length / 2, Environment.NewLine);
                    list.Add(s);
                }
            }
        }

        private static bool ContainsLetters(string line)
        {
            if (string.IsNullOrWhiteSpace(line.Replace("0", string.Empty).Replace("1", string.Empty).Replace("2", string.Empty).Replace("3", string.Empty).Replace("4", string.Empty).Replace("5", string.Empty).Replace("6", string.Empty)
                .Replace("7", string.Empty).Replace("8", string.Empty).Replace("9", string.Empty).Replace(":", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty).
                Replace("-", string.Empty).Replace(">", string.Empty).Replace("/", string.Empty)))
                return false;

            const string expectedChars = "\r\n\t .?\0";
            foreach (char ch in line)
            {
                if (!expectedChars.Contains(ch))
                    return true;
            }
            return false;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = SubtitleListview1.Items.Count > 0 ? DialogResult.OK : DialogResult.Cancel;
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
            // textBoxLineBreak and its label are enabled if radioButtonLineMode is checked.
            comboBoxLineBreak.Enabled = radioButtonLineMode.Checked;
            labelLineBreak.Enabled = radioButtonLineMode.Checked;
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
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                LoadTextFile(files[0]);
            }
        }

        private static string HtmlToPlainText(string html)
        {
            var stripFormattingRegex = new Regex(@"<[^>]*(>|$)", RegexOptions.Multiline); // match any character between '<' and '>', even when end tag is missing
            var tagWhiteSpaceRegex = new Regex(@"(>|$)(\W|\n|\r)+<", RegexOptions.Multiline); // matches one or more (white space or line breaks) between '>' and '<'

            // Decode html specific characters
            var text = System.Net.WebUtility.HtmlDecode(html);

            // Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");

            // Find new lines
            text = text.Replace("<BR>", Environment.NewLine);
            text = text.Replace("<br>", Environment.NewLine);
            text = text.Replace("<br />", Environment.NewLine);
            text = text.Replace("<br/>", Environment.NewLine);
            text = text.Replace("<HR>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("<hr>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("<hr />", Environment.NewLine + Environment.NewLine);
            text = text.Replace("<hr/>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("</p>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("</P>", Environment.NewLine + Environment.NewLine);
            text = text.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine);
            text = text.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine);

            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }

        private static string GetAllText(string fileName)
        {
            try
            {
                Encoding encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                var text = File.ReadAllText(fileName, encoding);
                if (fileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
                    text = HtmlToPlainText(text);
                return text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return string.Empty;
            }
        }

        private void LoadTextFile(string fileName)
        {
            textBoxText.Text = GetAllText(fileName);
            SetVideoFileName(fileName);
            Text = Configuration.Settings.Language.ImportText.Title + " - " + fileName;
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
                Text = Configuration.Settings.Language.ImportText.Title + " - " + fileName;
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
            if (_videoFileName.EndsWith(".en", StringComparison.Ordinal))
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
                var dir = Path.GetDirectoryName(fileName);
                if (dir != null)
                {
                    var files = Directory.GetFiles(dir, Path.GetFileNameWithoutExtension(_videoFileName) + "*.avi");
                    if (files.Length == 0)
                        files = Directory.GetFiles(dir, "*.avi");
                    if (files.Length == 0)
                        files = Directory.GetFiles(dir, "*.mkv");
                    if (files.Length > 0)
                        _videoFileName = files[0];
                }
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

        private void checkBoxGenerateTimeCodes_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxTimeCodes.Enabled = checkBoxGenerateTimeCodes.Checked;
            GeneratePreview();
        }

        private void ImportText_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (radioButtonSplitAtBlankLines.Checked)
                Configuration.Settings.Tools.ImportTextSplitting = "blank lines";
            else if (radioButtonLineMode.Checked)
                Configuration.Settings.Tools.ImportTextSplitting = "line";
            else
                Configuration.Settings.Tools.ImportTextSplitting = "auto";
            Configuration.Settings.Tools.ImportTextLineBreak = comboBoxLineBreak.Text.Trim();
            Configuration.Settings.Tools.ImportTextMergeShortLines = checkBoxMergeShortLines.Checked;
            Configuration.Settings.Tools.ImportTextRemoveEmptyLines = checkBoxRemoveEmptyLines.Checked;
            Configuration.Settings.Tools.ImportTextRemoveLinesNoLetters = checkBoxRemoveLinesWithoutLetters.Checked;
            Configuration.Settings.Tools.ImportTextGenerateTimeCodes = checkBoxGenerateTimeCodes.Checked;
            Configuration.Settings.Tools.ImportTextAutoBreak = checkBoxAutoBreak.Checked;
            Configuration.Settings.Tools.ImportTextGap = numericUpDownGapBetweenLines.Value;
            Configuration.Settings.Tools.ImportTextDurationAuto = radioButtonDurationAuto.Checked;
            Configuration.Settings.Tools.ImportTextFixedDuration = numericUpDownDurationFixed.Value;
        }

        private void checkBoxMultipleFiles_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxMultipleFiles.Checked)
            {
                listViewInputFiles.Visible = true;
                textBoxText.Visible = false;
                buttonOpenText.Text = Configuration.Settings.Language.ImportText.OpenTextFiles;
                groupBoxSplitting.Enabled = false;
            }
            else
            {
                listViewInputFiles.Visible = false;
                textBoxText.Visible = true;
                buttonOpenText.Text = Configuration.Settings.Language.ImportText.OpenTextFile;
                groupBoxSplitting.Enabled = true;
            }
            GeneratePreview();
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in fileNames.OrderBy(p => p))
            {
                AddInputFile(fileName);
            }
            GeneratePreview();
        }

        private void AddInputFile(string fileName)
        {
            try
            {
                var fi = new FileInfo(fileName);
                var item = new ListViewItem(fileName) { Tag = fileName };
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                if (fi.Length < 1024 * 1024) // max 1 mb
                {
                    listViewInputFiles.Items.Add(item);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void checkBoxAutoBreak_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void comboBoxLineBreak_TextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewInputFiles.Items.Clear();
        }

        private void ImportText_Shown(object sender, EventArgs e)
        {
            if (textBoxText.Visible && textBoxText.Text.Length > 20)
                buttonOK.Focus();
        }
    }
}