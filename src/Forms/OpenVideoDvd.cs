using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class OpenVideoDvd : PositionAndSizeForm
    {

        public string DvdPath { get; set; }

        public OpenVideoDvd()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = Configuration.Settings.Language.OpenVideoDvd.Title;
            groupBoxOpenDvdFrom.Text = Configuration.Settings.Language.OpenVideoDvd.OpenDvdFrom;
            radioButtonDisc.Text = Configuration.Settings.Language.OpenVideoDvd.Disc;
            radioButtonFolder.Text = Configuration.Settings.Language.OpenVideoDvd.Folder;
            labelChooseDrive.Text = Configuration.Settings.Language.OpenVideoDvd.ChooseDrive;
            labelChooseFolder.Text = Configuration.Settings.Language.OpenVideoDvd.ChooseFolder;
            PanelDrive.Enabled = false;
            UiUtil.FixLargeFonts(this, buttonOK);
            radioButtonDisc_CheckedChanged(null, null);

            PanelFolder.Left = PanelDrive.Left;
            PanelFolder.Top = PanelDrive.Top;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioButtonDisc.Checked)
            {
                string s = comboBoxDrive.Items[comboBoxDrive.SelectedIndex].ToString();
                if (s.Contains(' '))
                {
                    s = s.Substring(0, s.IndexOf(' '));
                }

                DvdPath = s;
            }
            else
            {
                DvdPath = textBoxFolder.Text;
            }
            DialogResult = DialogResult.OK;
        }

        private void radioButtonDisc_CheckedChanged(object sender, EventArgs e)
        {
            PanelDrive.Visible = radioButtonDisc.Checked;
            PanelFolder.Visible = radioButtonFolder.Checked;
        }

        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = true;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void OpenVideoDvd_Shown(object sender, EventArgs e)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.CDRom || drive.DriveType == DriveType.Removable)
                {
                    if (drive.IsReady)
                    {
                        try
                        {
                            comboBoxDrive.Items.Add(drive + "  " + drive.VolumeLabel);
                        }
                        catch
                        {
                            comboBoxDrive.Items.Add(drive.ToString());
                        }
                    }
                    else
                    {
                        comboBoxDrive.Items.Add(drive.ToString());
                    }
                }
            }
            if (comboBoxDrive.Items.Count > 0)
            {
                comboBoxDrive.SelectedIndex = 0;
            }
            else
            {
                radioButtonFolder.Checked = true;
            }

            PanelDrive.Enabled = true;
        }

        private void OpenVideoDvd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

    }
}
