using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ImportImages : PositionAndSizeForm
    {
        // 0_00_01_042__0_00_03_919_01.jpeg
        private static readonly Regex TimeCodeFormat1 = new Regex(@"^\d+_\d+_\d+_\d+__\d+_\d+_\d+_\d+_\d+$", RegexOptions.Compiled);
        private static readonly Regex TimeCodeFormat2 = new Regex(@"^\d+_\d+_\d+_\d+__\d+_\d+_\d+_\d+$", RegexOptions.Compiled);

        public Subtitle Subtitle { get; private set; }
        private readonly HashSet<string> _filesAlreadyInList;
        public ImportImages()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
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
            _filesAlreadyInList = new HashSet<string>();
        }

        private void buttonInputBrowse_Click(object sender, EventArgs e)
        {
            buttonInputBrowse.Enabled = false;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Configuration.Settings.Language.ImportImages.ImageFiles + "|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff";
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string fileName in openFileDialog1.FileNames)
                {
                    if (!_filesAlreadyInList.Contains(fileName))
                    {
                        AddInputFile(fileName);
                    }
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
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif" || ext == ".tif" || ext == ".tiff")
                {
                    _filesAlreadyInList.Add(fileName);
                    SetTimeCodes(fileName, item);
                    listViewInputFiles.Items.Add(item);
                }
            }
            catch
            {
                // ignored
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

        public static void SetEndTimeAndStartTime(string name, Paragraph p)
        {
            if (name.Contains("-to-"))
            {
                var arr = name.Replace("-to-", "_").Split('_');
                if (arr.Length == 3 && int.TryParse(arr[1], out var startTime) && int.TryParse(arr[2], out var endTime))
                {
                    p.StartTime.TotalMilliseconds = startTime;
                    p.EndTime.TotalMilliseconds = endTime;
                }
            }
            else if (TimeCodeFormat1.IsMatch(name) || TimeCodeFormat2.IsMatch(name))
            {
                var arr = name.Replace("__", "_").Split('_');
                if (arr.Length >= 8)
                {
                    try
                    {
                        p.StartTime = new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3]));
                        p.EndTime = new TimeCode(int.Parse(arr[4]), int.Parse(arr[5]), int.Parse(arr[6]), int.Parse(arr[7]));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewInputFiles.Items)
            {
                var p = new Paragraph { Text = item.Text };
                string name = Path.GetFileNameWithoutExtension(p.Text);
                SetEndTimeAndStartTime(name, p);
                Subtitle.Paragraphs.Add(p);
            }
            Subtitle.Sort(SubtitleSortCriteria.StartTime);
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
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
            {
                e.Cancel = true;
            }
            else
            {
                removeToolStripMenuItem.Visible = listViewInputFiles.SelectedItems.Count > 0;
            }
        }

        private void RemoveSelection(bool removeAll = false)
        {
            if (listViewInputFiles.Items.Count == 0)
            {
                return;
            }

            if (removeAll)
            {
                foreach (ListViewItem item in listViewInputFiles.Items)
                {
                    item.Remove();
                    _filesAlreadyInList.Remove(item.Text);
                }
            }
            else if (listViewInputFiles.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewInputFiles.SelectedItems)
                {
                    item.Remove();
                    _filesAlreadyInList.Remove(item.Text);
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
