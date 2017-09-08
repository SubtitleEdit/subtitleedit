using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class StatusLog : PositionAndSizeForm
    {
        private readonly IList<string> _log;
        private int _logCount = -1;

        public StatusLog(IList<string> log)
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.Main.StatusLog;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            _log = log;
            timer1_Tick(null, null);

            // Handle timer when the form is active/inactive
            Shown += delegate { timer1?.Start(); };
            FormClosed += delegate { timer1?.Stop(); };
        }

        private void StatusLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            timer1.Stop();
            if (_logCount != _log.Count)
            {
                var sb = new StringBuilder();
                for (int i = _log.Count - 1; i >= 0; i--)
                {
                    // Start from the most recent log
                    sb.AppendLine(_log[i]);
                }
                textBoxStatusLog.Text = sb.ToString();
                _logCount = _log.Count;
            }
            timer1.Start();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
