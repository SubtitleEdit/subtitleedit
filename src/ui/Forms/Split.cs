﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Split : Form
    {
        private Subtitle _subtitle;
        public bool ShowBasic { get; private set; }
        private int _totalNumberOfCharacters;
        private bool _loading = true;
        private List<Subtitle> _parts;
        private string _fileName;

        public Split()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var l = LanguageSettings.Current.Split;
            Text = l.Title;
            groupBoxSplitOptions.Text = l.SplitOptions;
            RadioButtonLines.Text = l.Lines;
            radioButtonCharacters.Text = l.Characters;
            labelNumberOfParts.Text = l.NumberOfEqualParts;
            radioButtonTime.Text = LanguageSettings.Current.MeasurementConverter.Time;
            groupBoxSubtitleInfo.Text = l.SubtitleInfo;
            groupBoxOutput.Text = l.Output;
            labelFileName.Text = l.FileName;
            labelChooseOutputFolder.Text = l.OutputFolder;
            labelOutputFormat.Text = LanguageSettings.Current.Main.Controls.SubtitleFormat;
            labelEncoding.Text = LanguageSettings.Current.Main.Controls.FileEncoding;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;
            buttonOpenOutputFolder.Text = LanguageSettings.Current.Main.Menu.File.Open;

            listViewParts.Columns[0].Text = l.Lines;
            listViewParts.Columns[1].Text = l.Characters;
            listViewParts.Columns[2].Text = l.FileName;

            buttonSplit.Text = l.DoSplit;
            buttonBasic.Text = l.Basic;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            comboBoxSubtitleFormats.Left = labelOutputFormat.Left + labelOutputFormat.Width + 3;
            comboBoxEncoding.Left = labelEncoding.Left + labelEncoding.Width + 3;

            UiUtil.FixLargeFonts(this, buttonSplit);
        }

        public void Initialize(Subtitle subtitle, string fileName, SubtitleFormat format)
        {
            ShowBasic = false;
            _subtitle = subtitle;
            if (string.IsNullOrEmpty(fileName))
            {
                textBoxFileName.Text = LanguageSettings.Current.SplitSubtitle.Untitled;
            }
            else
            {
                textBoxFileName.Text = fileName;
            }

            _fileName = fileName;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                _totalNumberOfCharacters += HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
            }

            labelLines.Text = string.Format(LanguageSettings.Current.Split.NumberOfLinesX, _subtitle.Paragraphs.Count);
            labelCharacters.Text = string.Format(LanguageSettings.Current.Split.NumberOfCharactersX, _totalNumberOfCharacters);

            try
            {
                numericUpDownParts.Value = Configuration.Settings.Tools.SplitNumberOfParts;
            }
            catch
            {
                // ignored
            }

            if (Configuration.Settings.Tools.SplitVia.Trim().Equals("lines", StringComparison.OrdinalIgnoreCase))
            {
                RadioButtonLines.Checked = true;
            }
            else if (Configuration.Settings.Tools.SplitVia.Trim().Equals("time", StringComparison.OrdinalIgnoreCase))
            {
                radioButtonTime.Checked = true;
            }
            else
            {
                radioButtonCharacters.Checked = true;
            }

            UiUtil.InitializeSubtitleFormatComboBox(comboBoxSubtitleFormats, format.FriendlyName);

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);

            numericUpDownParts.Maximum = _subtitle.Paragraphs.Count;

            if (!string.IsNullOrEmpty(_fileName))
            {
                textBoxOutputFolder.Text = Path.GetDirectoryName(_fileName);
            }
            else if (string.IsNullOrEmpty(Configuration.Settings.Tools.SplitOutputFolder) || !Directory.Exists(Configuration.Settings.Tools.SplitOutputFolder))
            {
                textBoxOutputFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
                textBoxOutputFolder.Text = Configuration.Settings.Tools.SplitOutputFolder;
            }
        }

        private void CalculateParts()
        {
            if (_loading)
            {
                return;
            }

            _loading = true;
            _parts = new List<Subtitle>();
            if (string.IsNullOrEmpty(textBoxOutputFolder.Text) || !Directory.Exists(textBoxOutputFolder.Text))
            {
                textBoxOutputFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            var format = Utilities.GetSubtitleFormatByFriendlyName(comboBoxSubtitleFormats.SelectedItem.ToString());
            var fileNameNoExt = Path.GetFileNameWithoutExtension(textBoxFileName.Text);
            if (string.IsNullOrWhiteSpace(fileNameNoExt))
            {
                fileNameNoExt = LanguageSettings.Current.SplitSubtitle.Untitled;
            }

            if (numericUpDownParts.Value == 0)
            {
                buttonSplit.Enabled = false;
                return;
            }

            listViewParts.Items.Clear();
            var startNumber = 0;
            if (RadioButtonLines.Checked)
            {
                var partSize = (int)(_subtitle.Paragraphs.Count / numericUpDownParts.Value);
                for (var i = 0; i < numericUpDownParts.Value; i++)
                {
                    var noOfLines = partSize;
                    if (i == numericUpDownParts.Value - 1)
                    {
                        noOfLines = (int)(_subtitle.Paragraphs.Count - (numericUpDownParts.Value - 1) * partSize);
                    }

                    var temp = new Subtitle { Header = _subtitle.Header };
                    var size = 0;
                    for (var number = 0; number < noOfLines; number++)
                    {
                        var p = _subtitle.Paragraphs[startNumber + number];
                        temp.Paragraphs.Add(new Paragraph(p));
                        size += HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
                    }
                    startNumber += noOfLines;
                    _parts.Add(temp);

                    var lvi = new ListViewItem($"{noOfLines:#,###,###}");
                    lvi.SubItems.Add($"{size:#,###,###}");
                    lvi.SubItems.Add(fileNameNoExt + ".Part" + (i + 1) + format.Extension);
                    listViewParts.Items.Add(lvi);
                }
            }
            else if (radioButtonCharacters.Checked)
            {
                var partSize = (int)(_totalNumberOfCharacters / numericUpDownParts.Value);
                var nextLimit = partSize;
                var currentSize = 0;
                var temp = new Subtitle { Header = _subtitle.Header };
                for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    var p = _subtitle.Paragraphs[i];
                    var size = HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
                    if (currentSize + size > nextLimit + 4 && _parts.Count < numericUpDownParts.Value - 1)
                    {
                        _parts.Add(temp);
                        var lvi = new ListViewItem($"{temp.Paragraphs.Count:#,###,###}");
                        lvi.SubItems.Add($"{currentSize:#,###,###}");
                        lvi.SubItems.Add(fileNameNoExt + ".Part" + _parts.Count + format.Extension);
                        listViewParts.Items.Add(lvi);
                        currentSize = size;
                        temp = new Subtitle { Header = _subtitle.Header };
                        temp.Paragraphs.Add(new Paragraph(p));
                    }
                    else
                    {
                        currentSize += size;
                        temp.Paragraphs.Add(new Paragraph(p));
                    }
                }
                _parts.Add(temp);
                var lvi2 = new ListViewItem($"{temp.Paragraphs.Count:#,###,###}");
                lvi2.SubItems.Add($"{currentSize:#,###,###}");
                lvi2.SubItems.Add(fileNameNoExt + ".Part" + numericUpDownParts.Value + ".srt");
                listViewParts.Items.Add(lvi2);
            }
            else if (radioButtonTime.Checked)
            {
                var startMs = _subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
                var endMs = _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds;
                var partSize = (endMs - startMs) / (double)numericUpDownParts.Value;
                var nextLimit = startMs + partSize;
                var currentSize = 0;
                var temp = new Subtitle { Header = _subtitle.Header };
                for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    var p = _subtitle.Paragraphs[i];
                    var size = HtmlUtil.RemoveHtmlTags(p.Text, true).Replace("\r\n", "\n") .Length;
                    if (p.StartTime.TotalMilliseconds > nextLimit - 10 && _parts.Count < numericUpDownParts.Value - 1)
                    {
                        _parts.Add(temp);
                        var lvi = new ListViewItem($"{temp.Paragraphs.Count:#,###,###}");
                        lvi.SubItems.Add($"{currentSize:#,###,###}");
                        lvi.SubItems.Add(fileNameNoExt + ".Part" + _parts.Count + format.Extension);
                        listViewParts.Items.Add(lvi);
                        temp = new Subtitle { Header = _subtitle.Header };
                        temp.Paragraphs.Add(new Paragraph(p));
                        nextLimit += partSize;
                        currentSize = 0;
                    }
                    else
                    {
                        currentSize += size;
                        temp.Paragraphs.Add(new Paragraph(p));
                    }
                }
                _parts.Add(temp);
                var lvi2 = new ListViewItem($"{temp.Paragraphs.Count:#,###,###}");
                lvi2.SubItems.Add($"{currentSize:#,###,###}");
                lvi2.SubItems.Add(fileNameNoExt + ".Part" + numericUpDownParts.Value + ".srt");
                listViewParts.Items.Add(lvi2);
            }

            _loading = false;
        }

        private void buttonBasic_Click(object sender, EventArgs e)
        {
            ShowBasic = true;
            DialogResult = DialogResult.Cancel;
        }

        private void buttonSplit_Click(object sender, EventArgs e)
        {
            var overwrite = false;
            var format = Utilities.GetSubtitleFormatByFriendlyName(comboBoxSubtitleFormats.SelectedItem.ToString());
            var fileNameNoExt = Path.GetFileNameWithoutExtension(textBoxFileName.Text);
            if (string.IsNullOrWhiteSpace(fileNameNoExt))
            {
                fileNameNoExt = LanguageSettings.Current.SplitSubtitle.Untitled;
            }

            var number = 1;
            try
            {
                foreach (var sub in _parts)
                {
                    var fileName = Path.Combine(textBoxOutputFolder.Text, fileNameNoExt + ".Part" + number + format.Extension);
                    var allText = sub.ToText(format);
                    if (File.Exists(fileName) && !overwrite)
                    {
                        if (MessageBox.Show(LanguageSettings.Current.SplitSubtitle.OverwriteExistingFiles, "", MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            return;
                        }

                        overwrite = true;
                    }
                    FileUtil.WriteAllText(fileName, allText, GetCurrentEncoding());
                    number++;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            Configuration.Settings.Tools.SplitNumberOfParts = (int)numericUpDownParts.Value;
            Configuration.Settings.Tools.SplitOutputFolder = textBoxOutputFolder.Text;
            if (RadioButtonLines.Checked)
            {
                Configuration.Settings.Tools.SplitVia = "Lines";
            }
            else if (radioButtonTime.Checked)
            {
                Configuration.Settings.Tools.SplitVia = "Time";
            }
            else
            {
                Configuration.Settings.Tools.SplitVia = "Characters";
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void numericUpDownParts_ValueChanged(object sender, EventArgs e)
        {
            CalculateParts();
        }

        private void radioButtonCharacters_CheckedChanged(object sender, EventArgs e)
        {
            CalculateParts();
        }

        private void RadioButtonLines_CheckedChanged(object sender, EventArgs e)
        {
            CalculateParts();
        }

        private void textBoxOutputFolder_TextChanged(object sender, EventArgs e)
        {
            CalculateParts();
        }

        private void Split_ResizeEnd(object sender, EventArgs e)
        {
            columnHeaderFileName.Width = -2;
        }

        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxOutputFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private TextEncoding GetCurrentEncoding()
        {
            return UiUtil.GetTextEncodingComboBoxCurrentEncoding(comboBoxEncoding);
        }

        private void Split_Shown(object sender, EventArgs e)
        {
            _loading = false;
            Split_ResizeEnd(sender, e);
            CalculateParts();
        }

        private void Split_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void textBoxFileName_TextChanged(object sender, EventArgs e)
        {
            CalculateParts();
        }

        private void comboBoxSubtitleFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateParts();
        }

        private void buttonOpenOutputFolder_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxOutputFolder.Text))
            {
                UiUtil.OpenFolder(textBoxOutputFolder.Text);
            }
            else
            {
                MessageBox.Show(string.Format(LanguageSettings.Current.SplitSubtitle.FolderNotFoundX, textBoxOutputFolder.Text));
            }
        }
    }
}
