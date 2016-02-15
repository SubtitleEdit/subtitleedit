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
            buttonExportLog.Text = Configuration.Settings.Language.Statistics.Export;
            textBoxStatusLog.Text = logText;
        }

        private void StatusLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;

            if (e.KeyData == (Keys.Control | Keys.A) && textBoxStatusLog.Text.Length > 0)
            {
                textBoxStatusLog.SelectAll();
            }
        }

        private void buttonExportLog_Click(object sender, System.EventArgs e)
        {
            var logMessages = textBoxStatusLog.Text;
            if (logMessages.Length == 0)
                return;
            using (var sfd = new SaveFileDialog() { FileName = "Subtitle-Log", DefaultExt = "txt", Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.WriteAllText(sfd.FileName, logMessages, System.Text.Encoding.UTF8);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
    }
}
