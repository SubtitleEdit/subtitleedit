using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class RestoreAutoBackup : PositionAndSizeForm
    {
        private static readonly object Locker = new object();
        
        //2011-12-13_20-19-18_title
        private static readonly Regex RegexFileNamePattern = new Regex(@"^\d\d\d\d-\d\d-\d\d_\d\d-\d\d-\d\d", RegexOptions.Compiled);
        private string[] _files;
        public string AutoBackupFileName { get; set; }
        private static bool ShowAutoBackupError { get; set; }

    public RestoreAutoBackup()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            labelStatus.Text = string.Empty;

            var l = LanguageSettings.Current.RestoreAutoBackup;
            Text = l.Title;
            linkLabelOpenContainingFolder.Text = LanguageSettings.Current.Main.Menu.File.OpenContainingFolder;
            listViewBackups.Columns[0].Text = l.DateAndTime;
            listViewBackups.Columns[1].Text = l.FileName;
            listViewBackups.Columns[2].Text = l.Extension;
            listViewBackups.Columns[3].Text = LanguageSettings.Current.General.Size;
            labelInfo.Text = l.Information;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        public static bool SaveAutoBackup(Subtitle subtitle, SubtitleFormat saveFormat, string currentText)
        {
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                return false;
            }

            if (!Directory.Exists(Configuration.AutoBackupDirectory))
            {
                try
                {
                    Directory.CreateDirectory(Configuration.AutoBackupDirectory);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(string.Format(LanguageSettings.Current.Main.UnableToCreateBackupDirectory, Configuration.AutoBackupDirectory, exception.Message));
                    return false;
                }
            }

            var title = string.Empty;
            if (!string.IsNullOrEmpty(subtitle.FileName))
            {
                title = "_" + Path.GetFileNameWithoutExtension(subtitle.FileName);
            }

            var fileName = $"{Configuration.AutoBackupDirectory}{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}_{DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}{title}{saveFormat.Extension}";
            try
            {
                File.WriteAllText(fileName, currentText);
                return true;
            }
            catch (Exception exception)
            {
                if (ShowAutoBackupError)
                {
                    MessageBox.Show("Unable to save auto-backup to file: " + fileName + Environment.NewLine +
                                    Environment.NewLine +
                                    exception.Message + Environment.NewLine + exception.StackTrace);
                    ShowAutoBackupError = false;
                }

                return false;
            }
        }

        public static DateTime GetLatestSettingsTime()
        {
            var path = Path.Combine(Configuration.AutoBackupDirectory, "Settings");
            if (!Directory.Exists(path))
            {
                return DateTime.MinValue;
            }

            var files = Directory.GetFiles(path, "*.*");
            var fileNameList = new List<string>();
            foreach (var fileName in files)
            {
                var fileNameNoPath = Path.GetFileName(fileName);
                if (RegexFileNamePattern.IsMatch(fileNameNoPath))
                {
                    fileNameList.Add(fileNameNoPath);
                }
            }

            fileNameList.Sort();
            if (fileNameList.Count == 0)
            {
                return DateTime.MinValue;
            }

            if (DateTime.TryParse(fileNameList[0].Substring(0, 10), out var date))
            {
                return date;
            }

            return DateTime.MinValue;
        }

        public static bool SaveAutoBackupSettings(Settings settings)
        {
            if (settings == null)
            {
                return false;
            }

            if (!Directory.Exists(Configuration.AutoBackupDirectory))
            {
                try
                {
                    Directory.CreateDirectory(Configuration.AutoBackupDirectory);
                }
                catch
                {
                    return false;
                }
            }

            var path = Path.Combine(Configuration.AutoBackupDirectory, "Settings");
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    return false;
                }
            }

            var fileName = $"{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}_{DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}Settings.xml";
            try
            {
                Settings.CustomSerialize(Path.Combine(path, fileName), settings);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        private void RestoreAutoBackup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void RestoreAutoBackup_Shown(object sender, EventArgs e)
        {
            if (Directory.Exists(Configuration.AutoBackupDirectory))
            {
                _files = Directory.GetFiles(Configuration.AutoBackupDirectory, "*.*");
                foreach (var fileName in _files)
                {
                    var path = Path.GetFileName(fileName);
                    if (RegexFileNamePattern.IsMatch(path))
                    {
                        AddBackupToListView(fileName);
                    }
                }
                listViewBackups.Sorting = SortOrder.Descending;
                listViewBackups.Sort();
                if (_files.Length > 0)
                {
                    return;
                }
            }
            linkLabelOpenContainingFolder.Visible = false;
            labelStatus.Left = linkLabelOpenContainingFolder.Left;
            labelStatus.Text = LanguageSettings.Current.RestoreAutoBackup.NoBackedUpFilesFound;

            RestoreAutoBackup_ResizeEnd(sender, e);
        }

        private void AddBackupToListView(string fileName)
        {
            var path = Path.GetFileName(fileName);
            if (path == null)
            {
                return;
            }

            var displayDate = path.Substring(0, 19).Replace('_', ' ');
            displayDate = displayDate.Remove(13, 1).Insert(13, ":");
            displayDate = displayDate.Remove(16, 1).Insert(16, ":");

            var displayName = path.Remove(0, 20);
            if (displayName == "srt")
            {
                displayName = "Untitled.srt";
            }

            var item = new ListViewItem(displayDate)
            {
                UseItemStyleForSubItems = false,
                Tag = fileName
            };
            item.SubItems.Add(Path.GetFileNameWithoutExtension(displayName));
            item.SubItems.Add(Path.GetExtension(fileName));

            try
            {
                item.SubItems.Add(new FileInfo(fileName).Length + " bytes");
            }
            catch
            {
                // ignored
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
            {
                SetAutoBackupFileName();
            }

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
            listViewBackups.AutoSizeLastColumn();
        }

        private void linkLabelOpenContainingFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var folderName = Configuration.AutoBackupDirectory;
            if (Utilities.IsRunningOnMono())
            {
                UiUtil.OpenFolder(folderName);
            }
            else
            {
                if (listViewBackups.SelectedItems.Count == 1)
                {
                    var argument = @"/select, " + listViewBackups.SelectedItems[0].Tag;
                    System.Diagnostics.Process.Start("explorer.exe", argument);
                }
                else
                {
                    UiUtil.OpenFolder(folderName);
                }
            }
        }

        public static void CleanAutoBackupFolder(string autoBackupFolder, int autoBackupDeleteAfterMonths)
        {
            lock (Locker) // only allow one thread
            {
                if (Directory.Exists(autoBackupFolder))
                {
                    var targetDate = DateTime.Now.AddMonths(-autoBackupDeleteAfterMonths);
                    var files = Directory.GetFiles(autoBackupFolder, "*.*");
                    foreach (var fileName in files)
                    {
                        try
                        {
                            var name = Path.GetFileName(fileName);
                            if (RegexFileNamePattern.IsMatch(name) && Convert.ToDateTime(name.Substring(0, 10), CultureInfo.InvariantCulture) <= targetDate)
                            {
                                File.Delete(fileName);
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }
        }
    }
}
