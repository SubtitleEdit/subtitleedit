using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class GenerateVideoWithHardSubsOutFile : Form
    {
        public GenerateVideoWithHardSubsOutFile()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelSuffix.Text = LanguageSettings.Current.Settings.Suffix;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            radioButtonSaveInSourceFolder.Text = LanguageSettings.Current.BatchConvert.SaveInSourceFolder;
            radioButtonSaveInOutputFolder.Text = LanguageSettings.Current.BatchConvert.SaveInOutputFolder;

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.GenVideoOutputFolder) || !Directory.Exists(Configuration.Settings.Tools.GenVideoOutputFolder))
            {
                textBoxOutputFolder.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Output");
            }
            else
            {
                textBoxOutputFolder.Text = Configuration.Settings.Tools.GenVideoOutputFolder;
            }

            textBoxSuffix.Text = Configuration.Settings.Tools.GenVideoOutputFileSuffix;
            if (Configuration.Settings.Tools.GenVideoUseOutputFolder)
            {
                radioButtonSaveInOutputFolder.Checked = true;
            }
            else
            {
                radioButtonSaveInSourceFolder.Checked = true;
            }

            radioButtonSaveInSourceFolder_CheckedChanged(null, null);
        }

        private void GenerateVideoWithHardSubsOutFile_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioButtonSaveInOutputFolder.Checked && string.IsNullOrWhiteSpace(textBoxOutputFolder.Text))
            {
                MessageBox.Show("Please choose output folder");
                return;
            }


            try
            {
                if (!Directory.Exists(textBoxOutputFolder.Text))
                {
                    Directory.CreateDirectory(textBoxOutputFolder.Text);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Please choose output folder" + Environment.NewLine + exception.Message);
                return;
            }

            Configuration.Settings.Tools.GenVideoOutputFolder = textBoxOutputFolder.Text;
            Configuration.Settings.Tools.GenVideoOutputFileSuffix = textBoxSuffix.Text;
            Configuration.Settings.Tools.GenVideoUseOutputFolder = radioButtonSaveInOutputFolder.Checked;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = true;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxOutputFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void linkLabelOpenOutputFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!Directory.Exists(textBoxOutputFolder.Text))
            {
                try
                {
                    Directory.CreateDirectory(textBoxOutputFolder.Text);
                }
                catch
                {
                    // ignore
                }
            }

            if (Directory.Exists(textBoxOutputFolder.Text))
            {
                UiUtil.OpenFolder(textBoxOutputFolder.Text);
            }
            else
            {
                MessageBox.Show(string.Format(LanguageSettings.Current.SplitSubtitle.FolderNotFoundX, textBoxOutputFolder.Text));
            }
        }

        private void radioButtonSaveInSourceFolder_CheckedChanged(object sender, EventArgs e)
        {
            textBoxOutputFolder.Enabled = !radioButtonSaveInSourceFolder.Checked;
            buttonChooseFolder.Enabled = !radioButtonSaveInSourceFolder.Checked;
        }

        private void radioButtonSaveInOutputFolder_CheckedChanged(object sender, EventArgs e)
        {
            textBoxOutputFolder.Enabled = !radioButtonSaveInSourceFolder.Checked;
            buttonChooseFolder.Enabled = !radioButtonSaveInSourceFolder.Checked;
        }
    }
}
