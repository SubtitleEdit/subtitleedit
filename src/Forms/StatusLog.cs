using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class StatusLog : PositionAndSizeForm
    {
        private readonly List<string> _log;
        private int _logCount = -1;
        private readonly StringBuilder Sb = new StringBuilder();

        public StatusLog(List<string> log)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = Configuration.Settings.Language.Main.StatusLog;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            _log = log;
            timer1_Tick(null, null);
        }

        private void StatusLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Control)
            {
                _log.Clear();
                timer1_Tick(null, null);
            }
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (_logCount == _log.Count)
            {
                return;
            }
            _logCount = _log.Count;
            for (int i = _logCount - 1; i >= 0; i--)
            {
                Sb.AppendLine(_log[i]);
            }
            textBoxStatusLog.Text = Sb.ToString();
            Sb.Clear();
            if (!timer1.Enabled)
            {
                timer1.Start();
            }
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}