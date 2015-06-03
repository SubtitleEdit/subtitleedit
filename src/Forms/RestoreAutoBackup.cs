using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class RestoreAutoBackup : PositionAndSizeForm
    {

        private string[] _files;
        public string AutoBackupFileName { get; set; }

        public RestoreAutoBackup()
        {
            InitializeComponent();
            labelStatus.Text = string.Empty;

            var l = Configuration.Settings.Language.RestoreAutoBackup;
            Text = l.Title;
            linkLabelOpenContainingFolder.Text = Configuration.Settings.Language.Main.Menu.File.OpenContainingFolder;
            listViewBackups.Columns[0].Text = l.DateAndTime;
            listViewBackups.Columns[1].Text = l.FileName;
            listViewBackups.Columns[2].Text = l.Extension;
            listViewBackups.Columns[3].Text = Configuration.Settings.Language.General.Size;
            labelInfo.Text = l.Information;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            Utilities.FixLargeFonts(this, buttonCancel);
        }

        private void RestoreAutoBackup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void RestoreAutoBackup_Shown(object sender, EventArgs e)
        {
            //2011-12-13_20-19-18_title
            var fileNamePattern = new Regex(@"^\d\d\d\d-\d\d-\d\d_\d\d-\d\d-\d\d", RegexOptions.Compiled);
            listViewBackups.Columns[2].Width = -2;
            if (Directory.Exists(Configuration.AutoBackupFolder))
            {
                _files = Directory.GetFiles(Configuration.AutoBackupFolder, "*.*");
                foreach (string fileName in _files)
                {
                    if (fileNamePattern.IsMatch(Path.GetFileName(fileName)))
                        AddBackupToListView(fileName);
                }
                listViewBackups.Sorting = SortOrder.Descending;
                listViewBackups.Sort();
                if (_files.Length > 0)
                    return;
            }
            linkLabelOpenContainingFolder.Visible = false;
            labelStatus.Left = linkLabelOpenContainingFolder.Left;
            labelStatus.Text = Configuration.Settings.Language.RestoreAutoBackup.NoBackedUpFilesFound;
        }

        private void AddBackupToListView(string fileName)
        {
            string displayDate = Path.GetFileName(fileName).Substring(0, 19).Replace('_', ' ');
            displayDate = displayDate.Remove(13, 1).Insert(13, ":");
            displayDate = displayDate.Remove(16, 1).Insert(16, ":");

            string displayName = Path.GetFileName(fileName).Remove(0, 20);

            if (displayName == "srt")
                displayName = "Untitled.srt";

            var item = new ListViewItem(displayDate);
            item.UseItemStyleForSubItems = false;
            item.Tag = fileName;

            var subItem = new ListViewItem.ListViewSubItem(item, Path.GetFileNameWithoutExtension(displayName));
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, Path.GetExtension(fileName));
            item.SubItems.Add(subItem);

            try
            {
                FileInfo fi = new FileInfo(fileName);
                subItem = new ListViewItem.ListViewSubItem(item, fi.Length + " bytes");
                item.SubItems.Add(subItem);
            }
            catch
            {
            }

            listViewBackups.Items.Add(item);
        }

        private void SetAutoBackupFileName()
        {
            AutoBackupFileName = listViewBackups.SelectedItems[0].Tag.ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewBackups.SelectedItems.Count == 1)
                SetAutoBackupFileName();
            DialogResult = DialogResult.OK;
        }

        private void listViewBackups_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewBackups.SelectedItems.Count == 1)
            {
                SetAutoBackupFileName();
                DialogResult = DialogResult.OK;
            }
        }

        private void RestoreAutoBackup_ResizeEnd(object sender, EventArgs e)
        {
            listViewBackups.Columns[2].Width = -2;
        }

        private void RestoreAutoBackup_SizeChanged(object sender, EventArgs e)
        {
            listViewBackups.Columns[2].Width = -2;
        }

        private void linkLabelOpenContainingFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string folderName = Configuration.AutoBackupFolder;
            if (Utilities.IsRunningOnMono())
            {
                System.Diagnostics.Process.Start(folderName);
            }
            else
            {
                if (listViewBackups.SelectedItems.Count == 1)
                {
                    string argument = @"/select, " + listViewBackups.SelectedItems[0].Tag;
                    System.Diagnostics.Process.Start("explorer.exe", argument);
                }
                else
                {
                    System.Diagnostics.Process.Start(folderName);
                }
            }
        }

    }
}
