using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class RestoreAutoBackup : Form
    {

        private string[] _files;
        public string AutoBackupFileName { get; set; }

        public RestoreAutoBackup()
        {
            InitializeComponent();
            labelStatus.Text = string.Empty;

            var l = Configuration.Settings.Language.RestoreAutoBackup;
            Text = l.Title;
            buttonOpenContainingFolder.Text = Configuration.Settings.Language.Main.Menu.File.OpenContainingFolder;
            listViewBackups.Columns[0].Text = l.DateAndTime;
            listViewBackups.Columns[1].Text = l.FileName;
            listViewBackups.Columns[2].Text = l.Extension;

            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            FixLargeFonts();                
        }

        private void FixLargeFonts()
        {
            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void RestoreAutoBackup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void RestoreAutoBackup_Shown(object sender, EventArgs e)
        {
            //2011-12-13_20-19-18_title
            var fileNamePattern = new Regex(@"^\d\d\d\d-\d\d-\d\d_\d\d-\d\d-\d\d_", RegexOptions.Compiled);

            if (Directory.Exists(Configuration.AutoBackupFolder))
            {
                labelStatus.Text = Configuration.Settings.Language.General.PleaseWait;
                Application.DoEvents();
                Refresh();
                Application.DoEvents();

                _files = Directory.GetFiles(Configuration.AutoBackupFolder, "*.*");
                foreach (string fileName in _files)
                {
                    if (fileNamePattern.IsMatch(Path.GetFileName(fileName)))
                        AddBackupToListView(fileName);
                }
                listViewBackups.Sorting = SortOrder.Descending;
                listViewBackups.Sort();
                labelStatus.Text = string.Empty;
            }
            else
            {
                buttonOpenContainingFolder.Visible = false;
                labelStatus.Text = Configuration.Settings.Language.RestoreAutoBackup.NoBackedUpFilesFound;
            }
            listViewBackups.Columns[2].Width = -2;
        }

        private void AddBackupToListView(string fileName)
        {
            string displayDate = Path.GetFileName(fileName).Substring(0, 19).Replace('_', ' ');
            string displayName = Path.GetFileName(fileName).Remove(0, 20);

            var item = new ListViewItem(Path.GetFileName(displayDate));
            item.UseItemStyleForSubItems = false;
            item.Tag = fileName;

            var subItem = new ListViewItem.ListViewSubItem(item, Path.GetFileNameWithoutExtension(displayName));
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, Path.GetExtension(fileName));
            item.SubItems.Add(subItem);

            listViewBackups.Items.Add(item);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewBackups.SelectedItems.Count == 1)
                AutoBackupFileName = listViewBackups.SelectedItems[0].Tag.ToString();
            DialogResult = DialogResult.OK;
        }

        private void listViewBackups_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewBackups.SelectedItems.Count == 1)
            {
                AutoBackupFileName = listViewBackups.SelectedItems[0].Tag.ToString();
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

        private void buttonOpenContainingFolder_Click(object sender, EventArgs e)
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
                    string argument = @"/select, " + listViewBackups.SelectedItems[0].Tag.ToString();
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
