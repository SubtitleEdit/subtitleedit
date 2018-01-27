using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class frmAdjustSubtitleSyncTime : Form
    {
        public event EventHandler<AdjustSubtitleSyncTimeEventArgs> TimeEntered;

        public frmAdjustSubtitleSyncTime()
        {
            InitializeComponent();
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            TimeEntered?.Invoke(this, new AdjustSubtitleSyncTimeEventArgs { TimeCode = TimeUpDown.TimeCode });
            this.Close();
        }
    }
}
