using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

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

            var l = Configuration.Settings.Language.Split;
            Text = l.Title;
            groupBoxSplitOptions.Text = l.SplitOptions;
            RadioButtonLines.Text = l.Lines;
            radioButtonCharacters.Text = l.Characters;
            labelNumberOfParts.Text = l.NumberOfEqualParts;
            groupBoxSubtitleInfo.Text = l.SubtitleInfo;
            groupBoxOutput.Text = l.Output;
            labelFileName.Text = l.FileName;
            labelChooseOutputFolder.Text = l.OutputFolder;
            labelOutputFormat.Text = Configuration.Settings.Language.Main.Controls.SubtitleFormat;
            labelEncoding.Text = Configuration.Settings.Language.Main.Controls.FileEncoding;
            groupBoxPreview.Text = Configuration.Settings.Language.General.Preview;
            buttonOpenOutputFolder.Text = Configuration.Settings.Language.Main.Menu.File.Open;

            listViewParts.Columns[0].Text = l.Lines;
            listViewParts.Columns[1].Text = l.Characters;
            listViewParts.Columns[2].Text = l.FileName;

            buttonSplit.Text = l.DoSplit;
            buttonBasic.Text = l.Basic;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

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
                textBoxFileName.Text = Configuration.Settings.Language.SplitSubtitle.Untitled;
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

            labelLines.Text = string.Format(Configuration.Settings.Language.Split.NumberOfLinesX, _subtitle.Paragraphs.Count);
            labelCharacters.Text = string.Format(Configuration.Settings.Language.Split.NumberOfCharactersX, _totalNumberOfCharacters);

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
            else
            {
                radioButtonCharacters.Checked = true;
            }

            UiUtil.InitializeSubtitleFormatComboBox(comboBoxSubtitleFormats, format.FriendlyName);

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);

            if (numericUpDownParts.Maximum > _subtitle.Paragraphs.Count)
            {
                numericUpDownParts.Maximum = (int)Math.Round(_subtitle.Paragraphs.Count / 2.0);
            }

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
            string fileNameNoExt = Path.GetFileNameWithoutExtension(textBoxFileName.Text);
            if (string.IsNullOrWhiteSpace(fileNameNoExt))
            {
                fileNameNoExt = Configuration.Settings.Language.SplitSubtitle.Untitled;
            }

            listViewParts.Items.Clear();
            int startNumber = 0;
            if (RadioButtonLines.Checked)
            {
                int partSize = (int)(_subtitle.Paragraphs.Count / numericUpDownParts.Value);
                for (int i = 0; i < numericUpDownParts.Value; i++)
                {
                    int noOfLines = partSize;
                    if (i == numericUpDownParts.Value - 1)
                    {
                        noOfLines = (int)(_subtitle.Paragraphs.Count - ((numericUpDownParts.Value - 1) * partSize));
                    }

                    var temp = new Subtitle { Header = _subtitle.Header };
                    int size = 0;
                    for (int number = 0; number < noOfLines; number++)
                    {
                        Paragraph p = _subtitle.Paragraphs[startNumber + number];
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
                int partSize = (int)(_totalNumberOfCharacters / numericUpDownParts.Value);
                int nextLimit = partSize;
                int currentSize = 0;
                Subtitle temp = new Subtitle { Header = _subtitle.Header };
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    Paragraph p = _subtitle.Paragraphs[i];
                    int size = HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
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
            _loading = false;
        }

        private void buttonBasic_Click(object sender, EventArgs e)
        {
            ShowBasic = true;
            DialogResult = DialogResult.Cancel;
        }

        private void buttonSplit_Click(object sender, EventArgs e)
        {
            bool overwrite = false;
            var format = Utilities.GetSubtitleFormatByFriendlyName(comboBoxSubtitleFormats.SelectedItem.ToString());
            string fileNameNoExt = Path.GetFileNameWithoutExtension(textBoxFileName.Text);
            if (string.IsNullOrWhiteSpace(fileNameNoExt))
            {
                fileNameNoExt = Configuration.Settings.Language.SplitSubtitle.Untitled;
            }

            int number = 1;
            try
            {
                foreach (Subtitle sub in _parts)
                {
                    string fileName = Path.Combine(textBoxOutputFolder.Text, fileNameNoExt + ".Part" + number + format.Extension);
                    string allText = sub.ToText(format);
                    if (File.Exists(fileName) && !overwrite)
                    {
                        if (MessageBox.Show(Configuration.Settings.Language.SplitSubtitle.OverwriteExistingFiles, "", MessageBoxButtons.YesNo) == DialogResult.No)
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
            CalculateParts();
        }

        private void Split_Resize(object sender, EventArgs e)
        {
            columnHeaderFileName.Width = -2;
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
                MessageBox.Show(string.Format(Configuration.Settings.Language.SplitSubtitle.FolderNotFoundX, textBoxOutputFolder.Text));
            }
        }

    }
}
