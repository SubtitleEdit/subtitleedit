using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class NetflixQCResult : Form
    {
        private string Message { get; set; }
        private List<string> FilesToLocate { get; set; }

        public NetflixQCResult(string message, List<string> filesToLocate)
        {
            InitializeComponent();

            Message = message;
            FilesToLocate = filesToLocate;

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
                    .ForEach(s => lnkLblText.Links.Add(Message.IndexOf(s), s.Length, s));
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
                buttonsPosMid = btnOpen.Left + ((btnOk.Left + btnOk.Width) - btnOpen.Left) / 2;
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
            Process.Start((string)e.Link.LinkData);
        }

        private void OpenFileLocation(string filePath)
        {
            Process.Start("explorer.exe", string.Format(@"/select,""{0}"" ", filePath));
        }
    }
}
