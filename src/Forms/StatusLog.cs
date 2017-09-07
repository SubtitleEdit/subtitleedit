using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class StatusLog : PositionAndSizeForm
    {
        private readonly List<string> _log;
        private int _logCount = -1;

        public StatusLog(List<string> log)
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.Main.StatusLog;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            _log = log;
            timer1_Tick(null, null);
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
                foreach (var logEntry in _log.AsEnumerable().Reverse())
                {
                    sb.AppendLine(logEntry);
                }
                textBoxStatusLog.Text = sb.ToString();
            }
            _logCount = _log.Count;
            timer1.Start();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
