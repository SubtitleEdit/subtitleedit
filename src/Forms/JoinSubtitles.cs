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
        public SubtitleFormat OutputFormat { get; private set; }

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

            buttonAddVobFile.Text = Configuration.Settings.Language.DvdSubRip.Add;
            ButtonRemoveVob.Text = Configuration.Settings.Language.DvdSubRip.Remove;
            buttonClear.Text = Configuration.Settings.Language.DvdSubRip.Clear;

            Text = Configuration.Settings.Language.JoinSubtitles.Title;
            labelNote.Text = Configuration.Settings.Language.JoinSubtitles.Note;
            groupBoxPreview.Text = Configuration.Settings.Language.JoinSubtitles.Information;
            buttonJoin.Text = Configuration.Settings.Language.JoinSubtitles.Join;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);
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
            int preCount = _fileNamesToJoin.Count;
            foreach (string fileName in files)
            {
                if (!_fileNamesToJoin.Any(f => f.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    _fileNamesToJoin.Add(fileName);
                }
            }
            // only refresh if new files are dropped
            if (preCount != _fileNamesToJoin.Count)
            {
                SortAndLoad();
            }
        }

        private static SubtitleFormat GetSubtitleFormat(string fileName, out Subtitle subtitle)
        {
            subtitle = new Subtitle();
            SubtitleFormat format = subtitle.LoadSubtitle(fileName, out _, Encoding.UTF8);

            // format found using text based formats
            if (format != null)
            {
                return format;
            }

            // check format using binary formats
            foreach (SubtitleFormat binaryFormat in new SubtitleFormat[] { new Ebu(), new Pac(), new Cavena890() })
            {
                if (binaryFormat.IsMine(null, fileName))
                {
                    binaryFormat.LoadSubtitle(subtitle, null, fileName);
                    return binaryFormat;
                }
            }

            // format not found
            return null;
        }

        private void CancelMergeOperation(int fileIdx, string errorFile)
        {
            for (; fileIdx < _fileNamesToJoin.Count; fileIdx++)
            {
                _fileNamesToJoin.RemoveAt(fileIdx);
            }

            MessageBox.Show("Unkown subtitle format: " + errorFile);
        }

        private void SortAndLoad()
        {
            OutputFormat = new SubRip(); // default subtitle format
            string header = null;
            var subtitles = new List<Subtitle>(_fileNamesToJoin.Count);
            SubtitleFormat lastFormat = null;

            for (int i = 0; i < _fileNamesToJoin.Count; i++)
            {
                string fileName = _fileNamesToJoin[i];
                try
                {
                    SubtitleFormat format = GetSubtitleFormat(fileName, out Subtitle sub);

                    // unable to find format stop the operation
                    if (format == null)
                    {
                        CancelMergeOperation(i, fileName);
                        return;
                    }

                    // use last subtitle header if available
                    if (!string.IsNullOrEmpty(sub.Header))
                    {
                        header = sub.Header;
                    }

                    // set last-format if not set yet
                    if (lastFormat == null)
                    {
                        lastFormat = format;
                    }
                    else
                    {
                        // uset default format as last if previous and current subtitle doesn't match formats
                        if (!lastFormat.FriendlyName.Equals(format.FriendlyName))
                        {
                            lastFormat = OutputFormat;
                        }
                    }

                    subtitles.Add(sub);
                }
                catch (Exception exception)
                {
                    CancelMergeOperation(i, fileName);
                    MessageBox.Show(exception.Message);
                    return;
                }
            }

            OutputFormat = lastFormat;

            var subtitleSorted = subtitles
                .Where(s => s.Paragraphs.Count > 0)
                .OrderBy(s => s.Paragraphs[0].StartTime.TotalMilliseconds).ToList();

            listViewParts.BeginUpdate();
            listViewParts.Items.Clear();
            foreach (Subtitle sub in subtitleSorted)
            {
                var lvi = new ListViewItem($"{sub.Paragraphs.Count:N}")
                {
                    SubItems =
                    {
                        sub.Paragraphs.First().StartTime.ToString(),
                        sub.Paragraphs.Last().StartTime.ToString(),
                        Path.GetFileName(sub.FileName)
                    }
                };
                listViewParts.Items.Add(lvi);
            }
            listViewParts.EndUpdate();

            if (OutputFormat != null && OutputFormat.FriendlyName != SubRip.NameOfFormat)
            {
                JoinedSubtitle.Header = header;
            }

            JoinedSubtitle.Paragraphs.AddRange(subtitleSorted.SelectMany(s => s.Paragraphs)
                .OrderBy(p => p.StartTime.TotalMilliseconds));
            JoinedSubtitle.Renumber();
            labelTotalLines.Text = string.Format(Configuration.Settings.Language.JoinSubtitles.TotalNumberOfLinesX, JoinedSubtitle.Paragraphs.Count);
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
                    if (File.Exists(fileName))
                    {
                        var fileInfo = new FileInfo(fileName);
                        if (fileInfo.Length < Subtitle.MaxFileSize)
                        {
                            if (!_fileNamesToJoin.Any(f => f.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
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
            }
        }

        private void ButtonRemoveVob_Click(object sender, EventArgs e)
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

    }
}
