using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class StatusLog : Form
    {
        public StatusLog(string logText)
        {
            InitializeComponent();
            textBoxStatusLog.Text = logText;
        }

        private void StatusLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
