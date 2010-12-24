using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.IO;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ShowEarlierLater : Form
    {
        public delegate void AdjustEventHandler(double adjustMilliseconds);

        TimeSpan _totalAdjustment = TimeSpan.FromMilliseconds(0);
        AdjustEventHandler _adjustCallback;

        public ShowEarlierLater()
        {
            InitializeComponent();
            labelTotalAdjustment.Text = string.Empty;
            timeUpDownAdjust.MaskedTextBox.Text = "000000000";

            Text = Configuration.Settings.Language.ShowEarlierLater.Title;
            labelHoursMinSecsMilliSecs.Text = Configuration.Settings.Language.General.HourMinutesSecondsMilliseconds;
            buttonShowEarlier.Text = Configuration.Settings.Language.ShowEarlierLater.ShowEarlier;
            buttonShowLater.Text = Configuration.Settings.Language.ShowEarlierLater.ShowLater;
        }

        private void ShowEarlierLater_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp("#sync");
                e.SuppressKeyPress = true;
            }
        }

        internal void Initialize(AdjustEventHandler adjustCallback, bool onlySelected)
        {
            if (onlySelected)
                Text = Configuration.Settings.Language.ShowEarlierLater.Title;
            else
                Text = Configuration.Settings.Language.ShowEarlierLater.TitleAll;

            _adjustCallback = adjustCallback;
            timeUpDownAdjust.TimeCode = new TimeCode(TimeSpan.FromMilliseconds(Configuration.Settings.General.DefaultAdjustMilliseconds));
        }

        private void ButtonShowEarlierClick(object sender, EventArgs e)
        {
            TimeCode tc = timeUpDownAdjust.TimeCode;
            if (tc != null && tc.TotalMilliseconds > 0)
            {
                _adjustCallback.Invoke(-tc.TotalMilliseconds);
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds - tc.TotalMilliseconds);
                ShowTotalAdjustMent();
                Configuration.Settings.General.DefaultAdjustMilliseconds = (int)tc.TotalMilliseconds;
            }
        }

        private void ShowTotalAdjustMent()
        {
            TimeCode tc = new TimeCode(_totalAdjustment);
            labelTotalAdjustment.Text = string.Format(Configuration.Settings.Language.ShowEarlierLater.TotalAdjustmentX, tc.ToShortString());
        }

        private void ButtonShowLaterClick(object sender, EventArgs e)
        {
            TimeCode tc = timeUpDownAdjust.TimeCode;
            if (tc != null && tc.TotalMilliseconds > 0)
            {
                _adjustCallback.Invoke(tc.TotalMilliseconds);
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds + tc.TotalMilliseconds);
                ShowTotalAdjustMent();
                Configuration.Settings.General.DefaultAdjustMilliseconds = (int)tc.TotalMilliseconds;
            }
        }

    }
}
