using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class NetflixQCResult : Form
    {
        private string Message { get; }
        private List<string> FilesToLocate { get; }

        public NetflixQCResult(string message, List<string> filesToLocate)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Message = message;
            FilesToLocate = filesToLocate;
            btnOk.Text = Configuration.Settings.Language.General.Ok;
            Text = Configuration.Settings.Language.Main.Menu.ToolBar.NetflixQualityCheck;
            btnOpen.Text = Configuration.Settings.Language.Main.Menu.ToolBar.Open;
            InitUi();
        }

        private void InitUi()
        {
            bool isAnyLinks = FilesToLocate.Count != 0;
            Label usedLabel;

            if (isAnyLinks)
            {
                lblText.Visible = false;
                lnkLblText.Text = Message;
                usedLabel = lnkLblText;

                FilesToLocate
                    .Where(Message.Contains)
                    .ToList()
                    .ForEach(s => lnkLblText.Links.Add(Message.IndexOf(s, StringComparison.Ordinal), s.Length, s));
            }
            else
            {
                lnkLblText.Visible = false;
                lblText.Text = Message;
                usedLabel = lblText;
            }

            Width = Math.Max(usedLabel.Width + usedLabel.Left * 3, 300);

            btnOpen.Visible = isAnyLinks;
            int buttonsPosMid;

            if (isAnyLinks)
            {
                buttonsPosMid = btnOpen.Left + (btnOk.Left + btnOk.Width - btnOpen.Left) / 2;
            }
            else
            {
                buttonsPosMid = btnOk.Left + btnOk.Width / 2;
            }

            btnOpen.Left = btnOpen.Left - buttonsPosMid + Width / 2;
            btnOk.Left = btnOk.Left - buttonsPosMid + Width / 2;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
            }

            return base.ProcessDialogKey(keyData);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileLocation(FilesToLocate[0]);
            Close();
        }

        private void lblText_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenURL((string)e.Link.LinkData);
        }

        private void OpenFileLocation(string filePath)
        {
            if (Configuration.IsRunningOnWindows)
            {
                Process.Start("explorer.exe", $@"/select,""{filePath}"" ");
            }
            else
            {
                UiUtil.OpenFolder(System.IO.Path.GetDirectoryName(filePath));
            }
        }
    }
}
