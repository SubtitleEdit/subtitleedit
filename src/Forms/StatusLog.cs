using Nikse.SubtitleEdit.Core;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class StatusLog : PositionAndSizeForm
    {
        public StatusLog(string logText)
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.Main.StatusLog;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            textBoxStatusLog.Text = logText;
        }

        private void StatusLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
