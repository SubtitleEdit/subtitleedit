﻿using Nikse.SubtitleEdit.Logic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    partial class About : Form
    {
        LanguageStructure.About _language = Configuration.Settings.Language.About;
        LanguageStructure.General _languageGeneral = Configuration.Settings.Language.General;

        public About()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            Text = _language.Title;
            okButton.Text = _languageGeneral.OK;
            string[] versionInfo = Utilities.AssemblyVersion.Split('.');
            string minorMinorVersion = string.Empty;
            if (versionInfo.Length >= 3 && versionInfo[2] != "0")
                minorMinorVersion = "." + versionInfo[2];
            labelProduct.Text = String.Format("{0} {1}.{2}{3}, build", _languageGeneral.Title, versionInfo[0], versionInfo[1], minorMinorVersion);            
            linkLabelGitBuildHash.Left = labelProduct.Left + labelProduct.Width - 5;
            linkLabelGitBuildHash.LinkColor = Color.FromArgb(0, 102, 204);
            linkLabelGitBuildHash.VisitedLinkColor = Color.FromArgb(0, 102, 204);

            string revisionNumber = "0";
            if (versionInfo.Length >= 4)
                revisionNumber = versionInfo[3];
            linkLabelGitBuildHash.Text = revisionNumber; // String.Format("{0}", Utilities.AssemblyDescription.Substring(0, 7));

            richTextBoxAbout1.Text = _language.AboutText1.TrimEnd() + Environment.NewLine +
                                     Environment.NewLine +
                                     _languageGeneral.TranslatedBy.Trim();
            var height = TextDraw.MeasureTextHeight(richTextBoxAbout1.Font, richTextBoxAbout1.Text, false) * 1.4 + 80;
            richTextBoxAbout1.Height = (int)height;
            Height = richTextBoxAbout1.Top + richTextBoxAbout1.Height + 90;
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void About_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp(null);
                e.SuppressKeyPress = true;
            }
        }

        private void RichTextBoxAbout1LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(e.LinkText);
            }
            catch
            {
                MessageBox.Show("Unable to start link: " + e.LinkText, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDonate_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.nikse.dk/Donate");
        }

        private void linkLabelGitBuildHash_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/subtitleedit/commit/" + Utilities.AssemblyDescription);
        }

    }
}
