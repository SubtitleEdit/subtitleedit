using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ShotChangesList : Form
    {
        public List<double> ShotChanges { get; set; }
        private readonly string _subtitleFileName;
        public double ShotChangeSeconds { get; private set; }

        public ShotChangesList(string subtitleFileName, List<double> shotChanges)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.Settings.WaveformRemoveOrExportShotChanges;
            buttonExport.Text = LanguageSettings.Current.MultipleReplace.Export;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            columnHeaderNumber.Text = LanguageSettings.Current.General.NumberSymbol;
            columnHeaderStartTime.Text = LanguageSettings.Current.General.StartTime;
            removeToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;

            ShotChanges = new List<double>(shotChanges);
            _subtitleFileName = subtitleFileName;

            FillListView();
        }

        private void FillListView()
        {
            var i = 0;
            listViewShotChanges.BeginUpdate();
            listViewShotChanges.Items.Clear();
            foreach (var shotChange in ShotChanges)
            {
                i++;
                ListViewItem item = new ListViewItem("#" + i) { Tag = shotChange };
                item.SubItems.Add(TimeCode.FromSeconds(shotChange).ToDisplayString());
                listViewShotChanges.Items.Add(item);
            }

            listViewShotChanges.EndUpdate();
            labelCount.Text = $"{LanguageSettings.Current.FindDialog.Count}: {ShotChanges.Count}";
        }

        private void listViewShotChanges_DoubleClick(object sender, EventArgs e)
        {
            if (listViewShotChanges.SelectedItems.Count > 0)
            {
                ShotChangeSeconds = (double)listViewShotChanges.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewShotChanges.SelectedItems.Count > 0)
            {
                ShotChangeSeconds = (double)listViewShotChanges.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
            }

            ShotChangeSeconds = -1;
            DialogResult = DialogResult.OK;
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
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewShotChanges.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewShotChanges.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift))
            {
                listViewShotChanges.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        private void ShotChangesList_ResizeEnd(object sender, EventArgs e)
        {
            listViewShotChanges.AutoSizeLastColumn();
        }

        private void ShotChangesList_Shown(object sender, EventArgs e)
        {
            ShotChangesList_ResizeEnd(sender, e);
            listViewShotChanges.Focus();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            ExportShotChanges(ShotChanges, this);
        }

        public void ExportShotChanges(List<double> shotChanges, Form form)
        {
            using (var saveDialog = new SaveFileDialog { FileName = GetFileName(), Filter = "Seconds|*.txt|Milliseconds|*.txt|Frames|*.txt" })
            {
                if (saveDialog.ShowDialog(form) != DialogResult.OK)
                {
                    return;
                }

                var sb = new StringBuilder();
                foreach (var sc in shotChanges)
                {
                    if (saveDialog.FilterIndex == 1)
                    {
                        sb.AppendLine(sc.ToString(CultureInfo.InvariantCulture));
                    }
                    else if (saveDialog.FilterIndex == 2)
                    {
                        sb.AppendLine(((long)Math.Round(sc * 1000.0)).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (saveDialog.FilterIndex == 3)
                    {
                        sb.AppendLine(SubtitleFormat.MillisecondsToFrames(sc * 1000.0).ToString(CultureInfo.InvariantCulture));
                    }
                }

                File.WriteAllText(saveDialog.FileName, sb.ToString());
            }
        }

        private string GetFileName()
        {
            if (string.IsNullOrEmpty(_subtitleFileName))
            {
                return "shot_changes.txt";
            }

            return Path.GetFileNameWithoutExtension(_subtitleFileName) + "_shot_changes.txt";
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewShotChanges.SelectedItems.Count == 0)
            {
                return;
            }

            string askText = listViewShotChanges.SelectedItems.Count > 1 ?
                string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewShotChanges.SelectedItems.Count) :
                LanguageSettings.Current.Main.DeleteOneLinePrompt;

            if (Configuration.Settings.General.PromptDeleteLines
                && MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            var indices = new List<int>();
            foreach (ListViewItem selectedItem in listViewShotChanges.SelectedItems)
            {
                indices.Add(selectedItem.Index);
            }

            foreach (var index in indices.OrderByDescending(p=>p))
            {
                ShotChanges.RemoveAt(index);
            }

            FillListView();
        }
    }
}
