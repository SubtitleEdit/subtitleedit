using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class SettingsProfileExport : Form
    {
        public List<RulesProfile> ExportedProfiles { get; set; }

        public SettingsProfileExport(List<RulesProfile> profiles)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);

            listViewExportStyles.Columns[0].Width = listViewExportStyles.Width - 20;
            foreach (var profile in profiles)
            {
                listViewExportStyles.Items.Add(new ListViewItem(profile.Name) { Checked = false, Tag = profile });
            }

            selectAllToolStripMenuItem.Text = LanguageSettings.Current.FixCommonErrors.SelectAll;
            inverseSelectionToolStripMenuItem.Text = LanguageSettings.Current.FixCommonErrors.InverseSelection;

            Text = LanguageSettings.Current.Settings.ExportProfiles;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            ExportedProfiles = new List<RulesProfile>();
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                if (item.Checked)
                {
                    ExportedProfiles.Add(item.Tag as RulesProfile);
                }
            }
            if (ExportedProfiles.Count == 0)
            {
                return;
            }

            saveFileDialogStyle.Title = LanguageSettings.Current.Settings.ExportProfiles;
            saveFileDialogStyle.InitialDirectory = Configuration.DataDirectory;
            saveFileDialogStyle.Filter = LanguageSettings.Current.Settings.Profiles + "|*.profile";
            saveFileDialogStyle.FileName = "SE_Profiles.profile";

            if (saveFileDialogStyle.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            File.WriteAllText(saveFileDialogStyle.FileName, RulesProfile.Serialize(ExportedProfiles));

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SettingsProfileExport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                item.Checked = true;
            }
        }

        private void inverseSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                item.Checked = !item.Checked;
            }
        }
    }
}
