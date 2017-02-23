using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SettingsListViewColumns : Form
    {
        public bool ShowEndTime { get; set; }
        public bool ShowDuration { get; set; }
        public bool ShowCps { get; set; }
        public bool ShowWpm { get; set; }

        public SettingsListViewColumns()
        {
            InitializeComponent();
            checkBoxShowCps.Checked = Configuration.Settings.Tools.ListViewShowColumnCharsPerSec;
            checkBoxShowWpm.Checked = Configuration.Settings.Tools.ListViewShowColumnWordsPerMin;

            checkBoxShowNumber.Text = Configuration.Settings.Language.General.NumberSymbol;
            checkBoxShowStartTime.Text = Configuration.Settings.Language.General.StartTime;
            checkBoxShowEndTime.Text = Configuration.Settings.Language.General.EndTime;
            checkBoxShowDuration.Text = Configuration.Settings.Language.General.Duration;
            checkBoxShowCps.Text = Configuration.Settings.Language.General.CharsPerSec;
            checkBoxShowWpm.Text = Configuration.Settings.Language.General.WordsPerMin;
            checkBoxShowText.Text = Configuration.Settings.Language.General.Text;

            labelInfo.Text = Configuration.Settings.Language.Settings.MainListViewColumnsInfo;
            Text = Configuration.Settings.Language.Settings.MainListViewColumns;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void SettingsListViewColumns_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            ShowEndTime = checkBoxShowEndTime.Checked;
            ShowDuration = checkBoxShowDuration.Checked;
            ShowCps = checkBoxShowCps.Checked;
            ShowWpm = checkBoxShowWpm.Checked;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
