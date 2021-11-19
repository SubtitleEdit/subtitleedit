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
    public sealed partial class SceneChangesList : Form
    {
        public List<double> SceneChanges { get; set; }
        private readonly string _subtitleFileName;
        public double SceneChangeSeconds { get; private set; }

        public SceneChangesList(string subtitleFileName, List<double> sceneChanges)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.Settings.WaveformRemoveOrExportSceneChanges;
            buttonExport.Text = LanguageSettings.Current.MultipleReplace.Export;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            columnHeaderNumber.Text = LanguageSettings.Current.General.NumberSymbol;
            columnHeaderStartTime.Text = LanguageSettings.Current.General.StartTime;
            removeToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;

            SceneChanges = new List<double>(sceneChanges);
            _subtitleFileName = subtitleFileName;

            FillListView();
        }

        private void FillListView()
        {
            var i = 0;
            listViewSceneChanges.BeginUpdate();
            listViewSceneChanges.Items.Clear();
            foreach (var sceneChange in SceneChanges)
            {
                i++;
                ListViewItem item = new ListViewItem("#" + i) { Tag = sceneChange };
                item.SubItems.Add(TimeCode.FromSeconds(sceneChange).ToDisplayString());
                listViewSceneChanges.Items.Add(item);
            }

            listViewSceneChanges.EndUpdate();
            labelCount.Text = $"{LanguageSettings.Current.FindDialog.Count}: {SceneChanges.Count}";
        }

        private void listViewSceneChanges_DoubleClick(object sender, EventArgs e)
        {
            if (listViewSceneChanges.SelectedItems.Count > 0)
            {
                SceneChangeSeconds = (double)listViewSceneChanges.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewSceneChanges.SelectedItems.Count > 0)
            {
                SceneChangeSeconds = (double)listViewSceneChanges.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
            }

            SceneChangeSeconds = -1;
            DialogResult = DialogResult.OK;
        }

        private void SceneChangesList_KeyDown(object sender, KeyEventArgs e)
        {
            if (listViewSceneChanges.Focused && e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewSceneChanges.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewSceneChanges.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift))
            {
                listViewSceneChanges.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        private void SceneChangesList_ResizeEnd(object sender, EventArgs e)
        {
            listViewSceneChanges.AutoSizeLastColumn();
        }

        private void SceneChangesList_Shown(object sender, EventArgs e)
        {
            SceneChangesList_ResizeEnd(sender, e);
            listViewSceneChanges.Focus();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            ExportSceneChanges(SceneChanges, this);
        }

        public void ExportSceneChanges(List<double> sceneChanges, Form form)
        {
            using (var saveDialog = new SaveFileDialog { FileName = GetFileName(), Filter = "Seconds|*.txt|Milliseconds|*.txt|Frames|*.txt" })
            {
                if (saveDialog.ShowDialog(form) != DialogResult.OK)
                {
                    return;
                }

                var sb = new StringBuilder();
                foreach (var sc in sceneChanges)
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
                return "scene_changes.txt";
            }

            return Path.GetFileNameWithoutExtension(_subtitleFileName) + "_scene_changes.txt";
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewSceneChanges.SelectedItems.Count == 0)
            {
                return;
            }

            string askText = listViewSceneChanges.SelectedItems.Count > 1 ?
                string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewSceneChanges.SelectedItems.Count) :
                LanguageSettings.Current.Main.DeleteOneLinePrompt;

            if (Configuration.Settings.General.PromptDeleteLines
                && MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            var indices = new List<int>();
            foreach (ListViewItem selectedItem in listViewSceneChanges.SelectedItems)
            {
                indices.Add(selectedItem.Index);
            }

            foreach (var index in indices.OrderByDescending(p=>p))
            {
                SceneChanges.RemoveAt(index);
            }

            FillListView();
        }
    }
}
