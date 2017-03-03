using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            lblText.Text = Message;

            FilesToLocate
                .Where(Message.Contains)
                .ToList()
                .ForEach(s => lblText.Links.Add(Message.IndexOf(s), s.Length, s));

            Width = Math.Max(lblText.Width + lblText.Left * 3, 300);

            btnOpen.Visible = FilesToLocate.Count != 0;
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
            Process.Start("explorer.exe", string.Format(@"/select,""{0}"" ", FilesToLocate[0]));
            Close();
        }

        private void lblText_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", string.Format(@"/select,""{0}"" ", e.Link.LinkData));
        }
    }
}
