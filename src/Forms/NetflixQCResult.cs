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
        private string FileToLocate { get; set; }

        public NetflixQCResult(string message, string fileToLocate)
        {
            InitializeComponent();

            Message = message;
            FileToLocate = fileToLocate;

            lblText.Text = Message;
            Width = Math.Max(lblText.Width + lblText.Left * 3, 300);

            btnOpen.Visible = !string.IsNullOrEmpty(fileToLocate);
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
            Process.Start("explorer.exe", string.Format(@"/select,""{0}"" ", FileToLocate));
            Close();
        }
    }
}
