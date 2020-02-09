using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class CheckForUpdates : Form
    {
        private CheckForUpdatesHelper _updatesHelper;
        private double _seconds;
        private readonly bool _performCheckOnShown = true;
        private readonly Main _mainForm;

        public CheckForUpdates(Main mainForm)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);

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
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonOK.Visible = false;
            buttonDontCheckUpdates.Text = Configuration.Settings.Language.CheckForUpdates.NoUpdates;
            buttonDontCheckUpdates.Visible = false;

            Location = new System.Drawing.Point(_mainForm.Location.X + (_mainForm.Width / 2) - (Width / 2), _mainForm.Location.Y + (_mainForm.Height / 2) - (Height / 2) - 200);
        }

        public CheckForUpdates(Main mainForm, CheckForUpdatesHelper checkForUpdatesHelper)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _mainForm = mainForm;
            _updatesHelper = checkForUpdatesHelper;
            InitLanguage();
            ShowAvailableUpdate(true);
            _performCheckOnShown = false;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void CheckForUpdates_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void CheckForUpdates_Shown(object sender, EventArgs e)
        {
            if (!_performCheckOnShown)
            {
                Activate();
                return;
            }

            _updatesHelper = new CheckForUpdatesHelper();
            Application.DoEvents();
            Refresh();
            _updatesHelper.CheckForUpdates();
            timerCheckForUpdates.Start();

            buttonOK.Focus();
            Activate();
        }

        private void timerCheckForUpdates_Tick(object sender, EventArgs e)
        {
            if (_seconds > 10)
            {
                timerCheckForUpdates.Stop();
                labelStatus.Text = string.Format(Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesFailedX, "Time out");
                buttonOK.Visible = true;
            }
            else if (_updatesHelper.Error != null)
            {
                timerCheckForUpdates.Stop();
                labelStatus.Text = string.Format(Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesFailedX, _updatesHelper.Error);
                buttonOK.Visible = true;
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
                    SetLargeSize();
                    textBoxChangeLog.Text = _updatesHelper.LatestChangeLog;
                    textBoxChangeLog.Visible = true;
                    buttonOK.Visible = true;
                }
            }
            _seconds += timerCheckForUpdates.Interval / TimeCode.BaseUnit;

            if (buttonDownloadAndInstall.Visible)
            {
                buttonDownloadAndInstall.Focus();
            }
            else if (buttonOK.Visible)
            {
                buttonOK.Focus();
            }
        }

        private void SetLargeSize()
        {
            Height = 600;
            MinimumSize = new System.Drawing.Size(500, 400);
        }

        private void ShowAvailableUpdate(bool fromAutoCheck)
        {
            SetLargeSize();
            textBoxChangeLog.Text = _updatesHelper.LatestChangeLog;
            textBoxChangeLog.Visible = true;
            labelStatus.Text = Configuration.Settings.Language.CheckForUpdates.CheckingForUpdatesNewVersion;
            buttonDownloadAndInstall.Visible = true;
            buttonOK.Visible = true;
            if (Configuration.Settings.General.CheckForUpdates && fromAutoCheck)
            {
                buttonDontCheckUpdates.Visible = true;
            }
            else
            {
                buttonDontCheckUpdates.Visible = false;
                buttonDownloadAndInstall.Left = buttonOK.Left - 6 - buttonDownloadAndInstall.Width;
            }
        }

        private void buttonDownloadAndInstall_Click(object sender, EventArgs e)
        {
            UiUtil.OpenURL("https://github.com/SubtitleEdit/subtitleedit/releases");
        }

        private void buttonDontCheckUpdates_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.CheckForUpdates = false;
            DialogResult = DialogResult.Cancel;
        }

    }
}
