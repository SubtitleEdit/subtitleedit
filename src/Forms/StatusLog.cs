using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class StatusLog : Form
    {
        public StatusLog(string logText)
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.Main.StatusLog;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            textBoxStatusLog.Text = logText;
        }

        private void StatusLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
