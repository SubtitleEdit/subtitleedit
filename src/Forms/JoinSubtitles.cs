using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class JoinSubtitles : PositionAndSizeForm
    {
        private readonly List<string> _fileNamesToJoin = new List<string>();
        public Subtitle JoinedSubtitle { get; set; }
        public SubtitleFormat JoinedFormat { get; private set; }

        public JoinSubtitles()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            JoinedSubtitle = new Subtitle();
            labelTotalLines.Text = string.Empty;

            listViewParts.Columns[0].Text = Configuration.Settings.Language.JoinSubtitles.NumberOfLines;
            listViewParts.Columns[1].Text = Configuration.Settings.Language.JoinSubtitles.StartTime;
            listViewParts.Columns[2].Text = Configuration.Settings.Language.JoinSubtitles.EndTime;
            listViewParts.Columns[3].Text = Configuration.Settings.Language.JoinSubtitles.FileName;

            buttonAddFile.Text = Configuration.Settings.Language.DvdSubRip.Add;
            buttonRemoveFile.Text = Configuration.Settings.Language.DvdSubRip.Remove;
            buttonClear.Text = Configuration.Settings.Language.DvdSubRip.Clear;

            Text = Configuration.Settings.Language.JoinSubtitles.Title;
            groupBoxPreview.Text = Configuration.Settings.Language.JoinSubtitles.Information;
            buttonJoin.Text = Configuration.Settings.Language.JoinSubtitles.Join;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            radioButtonJoinPlain.Text = Configuration.Settings.Language.JoinSubtitles.AlreadyCorrectTimeCodes;
            radioButtonJoinAddTime.Text = Configuration.Settings.Language.JoinSubtitles.AppendTimeCodes;
            labelAddTime.Text = Configuration.Settings.Language.JoinSubtitles.AddMs;

            labelAddTime.Left = radioButtonJoinAddTime.Left + radioButtonJoinAddTime.Width + 20;
            numericUpDownAddMs.Left = labelAddTime.Left + labelAddTime.Width + 5;

            UiUtil.FixLargeFonts(this, buttonCancel);

            if (Configuration.Settings.Tools.JoinCorrectTimeCodes)
            {
                radioButtonJoinPlain.Checked = true;
            }
            else
            {
                radioButtonJoinAddTime.Checked = true;
            }
            numericUpDownAddMs.Value = Configuration.Settings.Tools.JoinAddMs;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonSplit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void JoinSubtitles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void listViewParts_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listViewParts_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in files)
            {
                if (!_fileNamesToJoin.Any(file => file.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    _fileNamesToJoin.Add(fileName);
                }
            }
            SortAndLoad();
        }

        private void SortAndLoad()
        {
            JoinedFormat = new SubRip(); // default subtitle format
            string header = null;
            SubtitleFormat lastFormat = null;
            var subtitles = new List<Subtitle>();
            for (int k = 0; k < _fileNamesToJoin.Count; k++)
            {
                string fileName = _fileNamesToJoin[k];
                try
                {
                    var sub = new Subtitle();
                    SubtitleFormat format;
                    var lines = FileUtil.ReadAllLinesShared(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName));
                    if (lastFormat != null && lastFormat.IsMine(lines, fileName))
                    {
                        format = lastFormat;
                        format.LoadSubtitle(sub, lines, fileName);
                    }

                    format = sub.LoadSubtitle(fileName, out _, null);

                    if (format == null)
                    {
                        if (lines.Count > 0 && lines.Count < 10 && lines[0].Trim() == "WEBVTT")
                        {
                            format = new WebVTT(); // empty WebVTT
                        }
                    }

                    if (format == null)
                    {
                        foreach (var binaryFormat in SubtitleFormat.GetBinaryFormats(true))
                        {
                            if (binaryFormat.IsMine(null, fileName))
                            {
                                binaryFormat.LoadSubtitle(sub, null, fileName);
                                format = binaryFormat;
                                break;
                            }
                        }
                    }

                    if (format == null)
                    {
                        foreach (var f in SubtitleFormat.GetTextOtherFormats())
                        {
                            if (f.IsMine(lines, fileName))
                            {
                                f.LoadSubtitle(sub, lines, fileName);
                                format = f;
                                break;
                            }
                        }
                    }

                    if (format == null)
                    {
                        Revert(k, Configuration.Settings.Language.UnknownSubtitle.Title + Environment.NewLine + fileName);
                        break;
                    }

                    if (sub.Header != null)
                    {
                        header = sub.Header;
                    }

                    lastFormat = lastFormat == null || lastFormat.FriendlyName == format.FriendlyName ? format : new SubRip();

                    subtitles.Add(sub);
                }
                catch (Exception exception)
                {
                    Revert(k, exception.Message);
                    return;
                }
            }
            JoinedFormat = lastFormat;

            for (int outer = 0; outer < subtitles.Count; outer++)
            {
                for (int inner = 1; inner < subtitles.Count; inner++)
                {
                    var a = subtitles[inner - 1];
                    var b = subtitles[inner];
                    if (a.Paragraphs.Count > 0 && b.Paragraphs.Count > 0 && a.Paragraphs[0].StartTime.TotalMilliseconds > b.Paragraphs[0].StartTime.TotalMilliseconds)
                    {
                        string t1 = _fileNamesToJoin[inner - 1];
                        _fileNamesToJoin[inner - 1] = _fileNamesToJoin[inner];
                        _fileNamesToJoin[inner] = t1;

                        var t2 = subtitles[inner - 1];
                        subtitles[inner - 1] = subtitles[inner];
                        subtitles[inner] = t2;
                    }
                }
            }

            listViewParts.BeginUpdate();
            listViewParts.Items.Clear();
            int i = 0;
            foreach (string fileName in _fileNamesToJoin)
            {
                var sub = subtitles[i];
                var lvi = new ListViewItem($"{sub.Paragraphs.Count:#,###,###}");
                if (sub.Paragraphs.Count > 0)
                {
                    lvi.SubItems.Add(sub.Paragraphs[0].StartTime.ToString());
                    lvi.SubItems.Add(sub.Paragraphs[sub.Paragraphs.Count - 1].StartTime.ToString());
                }
                else
                {
                    lvi.SubItems.Add("-");
                    lvi.SubItems.Add("-");
                }
                lvi.SubItems.Add(fileName);
                listViewParts.Items.Add(lvi);
                i++;
            }
            listViewParts.EndUpdate();

            JoinedSubtitle = new Subtitle();
            if (JoinedFormat != null && JoinedFormat.FriendlyName != SubRip.NameOfFormat)
            {
                JoinedSubtitle.Header = header;
            }

            var addTime = radioButtonJoinAddTime.Checked;
            foreach (var sub in subtitles)
            {
                double addMs = 0;
                if (addTime && JoinedSubtitle.Paragraphs.Count > 0)
                {
                    addMs = JoinedSubtitle.Paragraphs.Last().EndTime.TotalMilliseconds + Convert.ToDouble(numericUpDownAddMs.Value);
                }
                foreach (var p in sub.Paragraphs)
                {
                    p.StartTime.TotalMilliseconds += addMs;
                    p.EndTime.TotalMilliseconds += addMs;
                    JoinedSubtitle.Paragraphs.Add(p);
                }
            }
            JoinedSubtitle.Renumber();
            labelTotalLines.Text = string.Format(Configuration.Settings.Language.JoinSubtitles.TotalNumberOfLinesX, JoinedSubtitle.Paragraphs.Count);
        }

        private void Revert(int idx, string message)
        {
            for (int i = _fileNamesToJoin.Count - 1; i >= idx; i--)
            {
                _fileNamesToJoin.RemoveAt(i);
            }
            MessageBox.Show(message);
        }

        private void JoinSubtitles_Resize(object sender, EventArgs e)
        {
            columnHeaderFileName.Width = -2;
        }

        private void ButtonAddSubtitleClick(object sender, EventArgs e)
        {
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var sb = new StringBuilder();
                foreach (string fileName in openFileDialog1.FileNames)
                {
                    Application.DoEvents();
                    if (File.Exists(fileName))
                    {
                        var fileInfo = new FileInfo(fileName);
                        if (fileInfo.Length < Subtitle.MaxFileSize)
                        {
                            if (!_fileNamesToJoin.Any(file => file.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                            {
                                _fileNamesToJoin.Add(fileName);
                            }
                        }
                        else
                        {
                            sb.AppendLine(string.Format(Configuration.Settings.Language.Main.FileXIsLargerThan10MB, fileName));
                        }
                    }
                }
                SortAndLoad();
                if (sb.Length > 0)
                {
                    MessageBox.Show(sb.ToString());
                }
                JoinSubtitles_Resize(sender, e);
            }
        }

        private void ButtonRemoveSubtitle_Click(object sender, EventArgs e)
        {
            for (int i = listViewParts.SelectedIndices.Count - 1; i >= 0; i--)
            {
                _fileNamesToJoin.RemoveAt(listViewParts.SelectedIndices[i]);
            }
            if (_fileNamesToJoin.Count == 0)
            {
                buttonClear_Click(null, null);
            }
            else
            {
                SortAndLoad();
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            _fileNamesToJoin.Clear();
            listViewParts.Items.Clear();
            JoinedSubtitle = new Subtitle();
        }

        private void JoinSubtitles_Shown(object sender, EventArgs e)
        {
            columnHeaderFileName.Width = -2;
        }

        private void JoinSubtitles_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.JoinCorrectTimeCodes = radioButtonJoinPlain.Checked;
            Configuration.Settings.Tools.JoinAddMs = (int)numericUpDownAddMs.Value;
        }

        private void RadioButtonJoinAddTime_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownAddMs.Enabled = radioButtonJoinAddTime.Checked;
            labelAddTime.Enabled = radioButtonJoinAddTime.Checked;
        }

        private void RadioButtonJoinPlain_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownAddMs.Enabled = radioButtonJoinAddTime.Checked;
            labelAddTime.Enabled = radioButtonJoinAddTime.Checked;
        }
    }
}
