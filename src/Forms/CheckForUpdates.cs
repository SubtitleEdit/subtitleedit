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
       
        public CheckForUpdates()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.CheckForUpdates.Title;
            labelStatus.Text = Configuration.Settings.Language.CheckForUpdates.CheckingForUpdates;
            buttonDownloadAndInstall.Text = Configuration.Settings.Language.CheckForUpdates.InstallUpdate;
            buttonDownloadAndInstall.Visible = false;
            textBoxChangeLog.Visible = false;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonCancel.Visible = false;
        }

        public CheckForUpdates(CheckForUpdatesHelper checkForUpdatesHelper)
        {
            InitializeComponent();

            _updatesHelper = checkForUpdatesHelper;
            ShowAvailableUpdate();
        }

        private void CheckForUpdates_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void CheckForUpdates_Shown(object sender, EventArgs e)
        {
            _updatesHelper = new CheckForUpdatesHelper();
            Application.DoEvents();
            Refresh();
            _updatesHelper.CheckForUpdates();
            timerCheckForUpdates.Start();
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
                    ShowAvailableUpdate();
                }
                else
                {
                    labelStatus.Text = Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesNoneAvailable;
                    Height = 550;
                    textBoxChangeLog.Text = _updatesHelper.LatestChangeLog;
                    textBoxChangeLog.Visible = true;
                    buttonCancel.Visible = true;
                }
            }
            _seconds += timerCheckForUpdates.Interval / 1000.0;
        }

        private void ShowAvailableUpdate()
        {
            Height = 550;
            textBoxChangeLog.Text = _updatesHelper.LatestChangeLog;
            textBoxChangeLog.Visible = true;
            labelStatus.Text = Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesNewVersion;
            buttonDownloadAndInstall.Visible = true;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonCancel.Visible = true;
        }

        private void buttonDownloadAndInstall_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SubtitleEdit/subtitleedit/releases");
        }

    }
}
