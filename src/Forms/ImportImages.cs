using Nikse.SubtitleEdit.Core;
using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ImportImages : PositionAndSizeForm
    {
        public Subtitle Subtitle { get; private set; }
        private readonly HashSet<string> FilesAlreadyInList;
        public ImportImages()
        {
            InitializeComponent();
            Subtitle = new Subtitle();
            Text = Configuration.Settings.Language.ImportImages.Title;
            groupBoxInput.Text = Configuration.Settings.Language.ImportImages.Input;
            labelChooseInputFiles.Text = Configuration.Settings.Language.ImportImages.InputDescription;
            removeToolStripMenuItem.Text = Configuration.Settings.Language.ImportImages.Remove;
            removeAllToolStripMenuItem.Text = Configuration.Settings.Language.ImportImages.RemoveAll;
            columnHeaderFName.Text = Configuration.Settings.Language.JoinSubtitles.FileName;
            columnHeaderSize.Text = Configuration.Settings.Language.General.Size;
            columnHeaderStartTime.Text = Configuration.Settings.Language.General.StartTime;
            columnHeaderEndTime.Text = Configuration.Settings.Language.General.EndTime;
            columnHeaderDuration.Text = Configuration.Settings.Language.General.Duration;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FilesAlreadyInList = new HashSet<string>();
        }

        private void buttonInputBrowse_Click(object sender, EventArgs e)
        {
            buttonInputBrowse.Enabled = false;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Configuration.Settings.Language.ImportImages.ImageFiles + "|*.png;*.jpg;*.bmp;*.gif;*.tif;*.tiff";
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string fileName in openFileDialog1.FileNames)
                {
                    if (!FilesAlreadyInList.Contains(fileName))
                        AddInputFile(fileName);
                }
            }
            buttonInputBrowse.Enabled = true;
        }

        private void AddInputFile(string fileName)
        {
            try
            {
                var fi = new FileInfo(fileName);
                var item = new ListViewItem(fileName);
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                var ext = fi.Extension.ToLowerInvariant();
                if (ext == ".png" || ext == ".jpg" || ext == ".bmp" || ext == ".gif" || ext == ".tif" || ext == ".tiff")
                {
                    FilesAlreadyInList.Add(fileName);
                    SetTimeCodes(fileName, item);
                    listViewInputFiles.Items.Add(item);
                }
            }
            catch
            {
            }
        }

        private static void SetTimeCodes(string fileName, ListViewItem item)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            var p = new Paragraph();
            SetEndTimeAndStartTime(name, p);
            item.SubItems.Add(p.StartTime.ToString());
            item.SubItems.Add(p.EndTime.ToString());
            item.SubItems.Add(p.Duration.ToShortString());
        }

        private static void SetEndTimeAndStartTime(string name, Paragraph p)
        {
            if (name.Contains("-to-"))
            {
                var arr = name.Replace("-to-", "_").Split('_');
                if (arr.Length == 3)
                {
                    int startTime, endTime;
                    if (int.TryParse(arr[1], out startTime) && int.TryParse(arr[2], out endTime))
                    {
                        p.StartTime.TotalMilliseconds = startTime;
                        p.EndTime.TotalMilliseconds = endTime;
                    }
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewInputFiles.Items)
            {
                var p = new Paragraph();
                p.Text = item.Text;
                string name = Path.GetFileNameWithoutExtension(p.Text);
                SetEndTimeAndStartTime(name, p);
                Subtitle.Paragraphs.Add(p);
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in fileNames)
            {
                AddInputFile(fileName);
            }
        }

        private void ImportImages_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == (Keys.Control | Keys.O))
            {
                buttonInputBrowse_Click(null, EventArgs.Empty);
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0)
                e.Cancel = true;
            else
                removeToolStripMenuItem.Visible = listViewInputFiles.SelectedItems.Count > 0;
        }

        private void RemoveSelection(bool removeAll = false)
        {
            if (listViewInputFiles.Items.Count == 0)
                return;
            if (removeAll)
            {
                foreach (ListViewItem item in listViewInputFiles.Items)
                {
                    item.Remove();
                    FilesAlreadyInList.Remove(item.Text);
                }
            }
            else if (listViewInputFiles.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewInputFiles.SelectedItems)
                {
                    item.Remove();
                    FilesAlreadyInList.Remove(item.Text);
                }
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelection();
        }

        private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelection(true);
        }
    }
}