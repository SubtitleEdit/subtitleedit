using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Forms;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class CheckForUpdates : Form
    {
        private CheckForUpdatesHelper _updatesHelper;
        private double _seconds = 0;
        private bool _performCheckOnShown = true;
        Main _mainForm;

        public CheckForUpdates(Main mainForm)
        {
            InitializeComponent();

            _mainForm = mainForm;
            InitLanguage();
        }

        private void InitLanguage()
        {
            Text = Configuration.Settings.Language.CheckForUpdates.Title;
            labelStatus.Text = Configuration.Settings.Language.CheckForUpdates.CheckingForUpdates;
            buttonDownloadAndInstall.Text = Configuration.Settings.Language.CheckForUpdates.InstallUpdate;
            buttonDownloadAndInstall.Visible = false;
            textBoxChangeLog.Visible = false;
            buttonCancel.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Visible = false;
            buttonDontCheckUpdates.Text = Configuration.Settings.Language.CheckForUpdates.NoUpdates;
            buttonDontCheckUpdates.Visible = false;

            this.Location = new System.Drawing.Point(_mainForm.Location.X + (_mainForm.Width / 2) - (Width / 2), _mainForm.Location.Y + (_mainForm.Height / 2) - (Height / 2) - 200);
        }

        public CheckForUpdates(Main mainForm, CheckForUpdatesHelper checkForUpdatesHelper)
        {
            InitializeComponent();

            _mainForm = mainForm;
            _updatesHelper = checkForUpdatesHelper;
            InitLanguage();
            ShowAvailableUpdate(true);
            _performCheckOnShown = false;
        }

        private void CheckForUpdates_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void CheckForUpdates_Shown(object sender, EventArgs e)
        {
            if (!_performCheckOnShown)
                return;

            _updatesHelper = new CheckForUpdatesHelper();
            Application.DoEvents();
            Refresh();
            _updatesHelper.CheckForUpdates();
            timerCheckForUpdates.Start();

            buttonCancel.Focus();
        }

        private void timerCheckForUpdates_Tick(object sender, EventArgs e)
        {
            if (_seconds > 10)
            {
                timerCheckForUpdates.Stop();
                labelStatus.Text = string.Format(Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesFailedX, "Time out");
                buttonCancel.Visible = true;
            }
            else if (_updatesHelper.Error != null)
            {
                timerCheckForUpdates.Stop();
                labelStatus.Text = string.Format(Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesFailedX, _updatesHelper.Error);
                buttonCancel.Visible = true;
            }
            else if (_updatesHelper.Done)
            {
                timerCheckForUpdates.Stop();
                if (_updatesHelper.IsUpdateAvailable())
                {
                    ShowAvailableUpdate(false);
                }
                else
                {
                    labelStatus.Text = Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesNoneAvailable;
                    Height = 600;
                    textBoxChangeLog.Text = _updatesHelper.LatestChangeLog;
                    textBoxChangeLog.Visible = true;
                    buttonCancel.Visible = true;
                }
            }
            _seconds += timerCheckForUpdates.Interval / 1000.0;

            if (buttonDownloadAndInstall.Visible)
                buttonDownloadAndInstall.Focus();
            else if (buttonCancel.Visible)
                buttonCancel.Focus();
        }

        private void ShowAvailableUpdate(bool fromAutoCheck)
        {
            Height = 600;
            textBoxChangeLog.Text = _updatesHelper.LatestChangeLog;
            textBoxChangeLog.Visible = true;
            labelStatus.Text = Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesNewVersion;
            buttonDownloadAndInstall.Visible = true;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonCancel.Visible = true;
            if (Configuration.Settings.General.CheckForUpdates && fromAutoCheck)
            {
                buttonDontCheckUpdates.Visible = true;
            }
            else
            {
                buttonDontCheckUpdates.Visible = false;
                buttonDownloadAndInstall.Left = buttonCancel.Left - 6 - buttonDownloadAndInstall.Width;
            }
        }

        private void buttonDownloadAndInstall_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SubtitleEdit/subtitleedit/releases");
        }

        private void buttonDontCheckUpdates_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.CheckForUpdates = false;
            DialogResult = DialogResult.Cancel;
        }

    }
}
