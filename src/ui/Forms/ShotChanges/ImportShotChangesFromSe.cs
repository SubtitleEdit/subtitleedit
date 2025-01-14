using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.ShotChanges
{
    public sealed partial class ImportShotChangesFromSe : Form
    {
        public class ShotChangeItem
        {
            public string Updated { get; set; }
            public string DisplayVideoFileName { get; set; }
            public int ShotChangeCount { get; set; }
            public string FileName { get; set; }
            public List<double> ShotChanges { get; set; }
        }

        public List<ShotChangeItem> ShotChangeFiles { get; set; }
        public List<double> ShotChanges { get; set; }

        public ImportShotChangesFromSe()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.Settings.WaveformRemoveOrExportShotChanges;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            columnHeaderLastUpdated.Text = LanguageSettings.Current.General.NumberSymbol;
            columnHeaderVideoFileName.Text = LanguageSettings.Current.General.StartTime;

            ShotChangeFiles = new List<ShotChangeItem>();
            ShotChanges = new List<double>();
        }

        private void FillListView()
        {
            LoadShotChanges();

            var i = 0;
            listViewShotChanges.BeginUpdate();
            listViewShotChanges.Items.Clear();
            foreach (var file in ShotChangeFiles)
            {
                i++;
                var item = new ListViewItem(file.Updated) { Tag = file.ShotChanges };
                item.SubItems.Add(file.DisplayVideoFileName);
                item.SubItems.Add(file.ShotChangeCount.ToString());
                listViewShotChanges.Items.Add(item);
            }

            listViewShotChanges.EndUpdate();
        }

        private void LoadShotChanges()
        {
            var dir = Configuration.ShotChangesDirectory.TrimEnd(Path.DirectorySeparatorChar);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var searchFileName = "*.shotchanges";
            var files = Directory.GetFiles(dir, searchFileName);
            foreach (var file in files)
            {
                try
                {
                    var shotChanges = new List<double>();
                    var lines = File.ReadAllLines(file);
                    foreach (var line in lines)
                    {
                        if (double.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out var shotChange))
                        {
                            shotChanges.Add(shotChange);
                        }
                    }

                    ShotChangeFiles.Add(new ShotChangeItem
                    {
                        Updated = File.GetLastWriteTime(file).ToString(),
                        ShotChangeCount = shotChanges.Count,
                        FileName = file,
                        DisplayVideoFileName = Path.GetFileNameWithoutExtension(file),
                        ShotChanges = shotChanges
                    });
                }
                catch 
                {
                    // ignore
                }
            }
        }

        private void listViewShotChanges_DoubleClick(object sender, EventArgs e)
        {
            if (listViewShotChanges.SelectedItems.Count > 0)
            {
                ShotChanges = (List<double>)listViewShotChanges.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewShotChanges.SelectedItems.Count > 0)
            {
                ShotChanges = (List<double>)listViewShotChanges.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
                return;
            }

            DialogResult = DialogResult.Cancel;
        }

        private void ShotChangesList_KeyDown(object sender, KeyEventArgs e)
        {
            if (listViewShotChanges.Focused && e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ShotChangesList_ResizeEnd(object sender, EventArgs e)
        {
            listViewShotChanges.AutoSizeLastColumn();
        }

        private void ShotChangesList_Shown(object sender, EventArgs e)
        {
            FillListView();
            ShotChangesList_ResizeEnd(sender, e);
            listViewShotChanges.Focus();
        }
    }
}
