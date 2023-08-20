using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;
using CheckForUpdatesHelper = Nikse.SubtitleEdit.Logic.CheckForUpdatesHelper;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class CheckForUpdates : Form
    {
        private CheckForUpdatesHelper _updatesHelper;
        private double _seconds;
        private readonly bool _performCheckOnShown = true;
        private readonly Main _mainForm;

        public bool UpdatePlugins { get; set; }

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
            Text = LanguageSettings.Current.CheckForUpdates.Title;
            labelStatus.Text = LanguageSettings.Current.CheckForUpdates.CheckingForUpdates;
            buttonDownloadAndInstall.Text = LanguageSettings.Current.CheckForUpdates.InstallUpdate;
            buttonDownloadAndInstall.Visible = false;
            textBoxChangeLog.Visible = false;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonOK.Visible = false;
            buttonDontCheckUpdates.Text = LanguageSettings.Current.CheckForUpdates.NoUpdates;
            buttonDontCheckUpdates.Visible = false;

            labelPluginsHaveUpdates.Visible = false;
            linkLabelUpdatePlugins.Visible = false;

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
            ShowAvailableUpdate();
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

        private async void CheckForUpdates_Shown(object sender, EventArgs e)
        {
            if (!_performCheckOnShown)
            {
                Activate();
                return;
            }

            _updatesHelper = new CheckForUpdatesHelper();
            Refresh();
            await _updatesHelper.CheckForUpdates(true);
            timerCheckForUpdates.Start();

            buttonOK.Focus();
            Activate();
        }

        private void timerCheckForUpdates_Tick(object sender, EventArgs e)
        {
            if (_seconds > 10)
            {
                timerCheckForUpdates.Stop();
                labelStatus.Text = string.Format(LanguageSettings.Current.CheckForUpdates.CheckingForUpdatesFailedX, "Time out");
                buttonOK.Visible = true;
            }
            else if (_updatesHelper.Error != null)
            {
                timerCheckForUpdates.Stop();
                labelStatus.Text = string.Format(LanguageSettings.Current.CheckForUpdates.CheckingForUpdatesFailedX, _updatesHelper.Error);
                buttonOK.Visible = true;
            }
            else if (_updatesHelper.Done)
            {
                timerCheckForUpdates.Stop();
                ShowAvailableUpdate();
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

        private void ShowAvailableUpdate()
        {
            //TODO: clean this method a little up

            var hideChangeLog = !_updatesHelper.ManualCheck;
            if (_updatesHelper.IsNewSubtitleEditAvailable())
            {
                hideChangeLog = false;
            }

            if (_updatesHelper.PluginUpdates > 0)
            {
                if (_updatesHelper.PluginUpdates == 1)
                {
                    labelPluginsHaveUpdates.Text = LanguageSettings.Current.CheckForUpdates.OnePluginsHasAnUpdate;
                }
                else
                {
                    labelPluginsHaveUpdates.Text = string.Format(LanguageSettings.Current.CheckForUpdates.XPluginsHasAnUpdate, _updatesHelper.PluginUpdates);
                }

                linkLabelUpdatePlugins.Text = LanguageSettings.Current.CheckForUpdates.Update;
                labelPluginsHaveUpdates.Visible = true;
                linkLabelUpdatePlugins.Visible = true;

                if (hideChangeLog)
                {
                    MinimizeBox = false;
                    MaximizeBox = false;
                    FormBorderStyle = FormBorderStyle.FixedDialog;
                    labelStatus.Visible = false;
                    var w = linkLabelUpdatePlugins.Right + 75;
                    var h = 120;
                    MinimumSize = new System.Drawing.Size(w, h);
                    Height = h;
                    Width = w;
                }

                linkLabelUpdatePlugins.Left = labelPluginsHaveUpdates.Right - 2;
                linkLabelUpdatePlugins.BringToFront();
            }
            else
            {
                labelPluginsHaveUpdates.Visible = false;
                linkLabelUpdatePlugins.Visible = false;
                textBoxChangeLog.Height += 14;
            }

            if (!hideChangeLog)
            {
                Height = 600;
                MinimumSize = new System.Drawing.Size(500, 400);
            }

            buttonDownloadAndInstall.Visible = _updatesHelper.IsNewSubtitleEditAvailable();

            if (_updatesHelper.IsNewSubtitleEditAvailable() || _updatesHelper.ManualCheck)
            {
                textBoxChangeLog.Text = _updatesHelper.LatestChangeLog;
                textBoxChangeLog.Visible = true;
                labelStatus.Text = LanguageSettings.Current.CheckForUpdates.CheckingForUpdatesNewVersion;
                buttonOK.Visible = true;
                if (_updatesHelper.IsNewSubtitleEditAvailable())
                {
                    buttonDontCheckUpdates.Visible = true;
                }
                else
                {
                    buttonDontCheckUpdates.Visible = false;
                    buttonDownloadAndInstall.Left = buttonOK.Left - 6 - buttonDownloadAndInstall.Width;
                }
            }
            else
            {
                textBoxChangeLog.Visible = false;
            }
        }

        private void buttonDownloadAndInstall_Click(object sender, EventArgs e)
        {
            UiUtil.OpenUrl("https://github.com/SubtitleEdit/subtitleedit/releases");
        }

        private void buttonDontCheckUpdates_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.CheckForUpdates = false;
            DialogResult = DialogResult.Cancel;
        }

        private void linkLabelUpdatePlugins_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UpdatePlugins = true;
            DialogResult = DialogResult.OK;
        }
    }
}
