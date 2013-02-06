using System;
using System.Diagnostics;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    partial class About : Form
    {
        LanguageStructure.About _language = Configuration.Settings.Language.About;
        LanguageStructure.General _languageGeneral = Configuration.Settings.Language.General;

        public About()
        {
            InitializeComponent();
            base.Text = _language.Title;
            string[] versionInfo = Utilities.AssemblyVersion.Split('.');
            string minorMinorVersion = string.Empty;
            if (versionInfo.Length >= 3 && versionInfo[2] != "0")
                minorMinorVersion = "." + versionInfo[2];
            labelProduct.Text = String.Format("{0} {1}.{2}{3} rev.{4}", _languageGeneral.Title, versionInfo[0], versionInfo[1], minorMinorVersion, versionInfo[3]);
            richTextBoxAbout1.Text = _language.AboutText1.TrimEnd() + Environment.NewLine +
                                     Environment.NewLine +
                                     _languageGeneral.TranslatedBy.Trim();
            okButton.Text = _languageGeneral.OK;

            float height = TextDraw.MeasureTextHeight(richTextBoxAbout1.Font, richTextBoxAbout1.Text, false);

            // Autosize height
            richTextBoxAbout1.Height = (int)(height + 25);
            Height = richTextBoxAbout1.Top + richTextBoxAbout1.Height + 100;
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

    }
}
