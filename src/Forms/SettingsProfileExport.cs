using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SettingsProfileExport : Form
    {
        public List<RulesProfile> ExportedProfiles { get; set; }

        public SettingsProfileExport(List<RulesProfile> profiles)
        {
            InitializeComponent();

            listViewExportStyles.Columns[0].Width = listViewExportStyles.Width - 20;
            foreach (var profile in profiles)
            {
                listViewExportStyles.Items.Add(new ListViewItem(profile.Name) { Checked = true, Tag = profile });
            }

            Text = Configuration.Settings.Language.Settings.ExportProfiles;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
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

            saveFileDialogStyle.Title = Configuration.Settings.Language.Settings.ExportProfiles;
            saveFileDialogStyle.InitialDirectory = Configuration.DataDirectory;
            saveFileDialogStyle.Filter = Configuration.Settings.Language.Settings.Profiles + "|*.profile";
            saveFileDialogStyle.FileName = "SE_Profiles.profile";

            if (saveFileDialogStyle.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var ser = new XmlSerializer(typeof(List<RulesProfile>));
            using (var writer = new StreamWriter(saveFileDialogStyle.FileName))
            {
                ser.Serialize(writer, ExportedProfiles);
                writer.Close();
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
