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

        public StatusLog(List<string> log)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.Main.StatusLog;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            _log = log;
            timer1_Tick(null, null);
        }

        private void StatusLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                buttonOK_Click(null, null);
            }
            else if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Control)
            {
                _log.Clear();
                timer1_Tick(null, null);
            }
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            timer1.Stop();
            if (_logCount != _log.Count)
            {
                _logCount = _log.Count;
                var sb = new StringBuilder();
                for (int i = _logCount - 1; i >= 0; i--)
                {
                    sb.AppendLine(_log[i]);
                }
                textBoxStatusLog.Text = sb.ToString();
            }
            timer1.Start();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}