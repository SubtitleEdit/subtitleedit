using Nikse.SubtitleEdit.Core;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    partial class About : PositionAndSizeForm
    {
        private readonly LanguageStructure.About _language = Configuration.Settings.Language.About;
        private readonly LanguageStructure.General _languageGeneral = Configuration.Settings.Language.General;

        public About()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, okButton);
        }

        public void Initialize()
        {
            Text = _language.Title + " - " + (IntPtr.Size * 8) + "-bit";
            okButton.Text = _languageGeneral.Ok;
            string[] versionInfo = Utilities.AssemblyVersion.Split('.');
            string revisionNumber = "0";
            if (versionInfo.Length >= 4)
            {
                revisionNumber = versionInfo[3];
            }

            if (revisionNumber == "0" || revisionNumber == "1") // don't append build number for rev 0 - and also 1 in case first build goes wrong
            {
                labelProduct.Text = $"{_languageGeneral.Title} {versionInfo[0]}.{versionInfo[1]}.{versionInfo[2]}";
                linkLabelGitBuildHash.Hide();
            }
            else
            {
                labelProduct.Text = $"{_languageGeneral.Title} {versionInfo[0]}.{versionInfo[1]}.{versionInfo[2]} NEXT, beta";
                linkLabelGitBuildHash.Left = labelProduct.Left + labelProduct.Width;
                linkLabelGitBuildHash.Text = revisionNumber;
                tooltip.SetToolTip(linkLabelGitBuildHash, GetGitHubHashLink());
                linkLabelGitBuildHash.Font = labelProduct.Font;
            }

            string aboutText = _language.AboutText1.TrimEnd() + Environment.NewLine +
                               Environment.NewLine +
                               _languageGeneral.TranslatedBy.Trim();
            while (aboutText.Contains("\n ") || aboutText.Contains("\n\t"))
            {
                aboutText = aboutText.Replace("\n ", "\n");
                aboutText = aboutText.Replace("\n\t", "\n");
            }
            richTextBoxAbout1.Text = aboutText;

            SetHeight();
        }

        private void SetHeight()
        {
            using (var g = CreateGraphics())
            {
                double height = g.MeasureString(richTextBoxAbout1.Text, richTextBoxAbout1.Font).Height + 15;
                richTextBoxAbout1.Height = (int)height;
                Height = richTextBoxAbout1.Top + richTextBoxAbout1.Height + 90;
            }
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void About_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp(null);
                e.SuppressKeyPress = true;
            }
        }

        private void RichTextBoxAbout1LinkClicked(object sender, LinkClickedEventArgs e)
        {
            UiUtil.OpenURL(e.LinkText);
        }

        private void buttonDonate_Click(object sender, EventArgs e)
        {
            UiUtil.OpenURL("https://www.nikse.dk/Donate");
        }

        private void linkLabelGitBuildHash_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenURL(GetGitHubHashLink());
        }

        private static string GetGitHubHashLink()
        {
            try
            {
                return "https://github.com/SubtitleEdit/subtitleedit/commit/" + Utilities.AssemblyDescription.Substring(0, 7);
            }
            catch
            {
                return "https://github.com/SubtitleEdit/subtitleedit";
            }
        }

        private void About_Shown(object sender, EventArgs e)
        {
            SetHeight();
        }
    }
}
