using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ShowEarlierLater : PositionAndSizeForm
    {
        public delegate void AdjustEventHandler(double adjustMilliseconds, SelectionChoice selection);

        private TimeSpan _totalAdjustment;
        private AdjustEventHandler _adjustCallback;

        public ShowEarlierLater()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            ResetTotalAdjustment();
            timeUpDownAdjust.MaskedTextBox.Text = "000000000";
            Text = LanguageSettings.Current.ShowEarlierLater.Title.RemoveChar('&');
            labelHourMinSecMilliSecond.Text = Configuration.Settings.General.UseTimeFormatHHMMSSFF ? 
                LanguageSettings.Current.General.HourMinutesSecondsFrames : 
                string.Format(LanguageSettings.Current.General.HourMinutesSecondsDecimalSeparatorMilliseconds, UiUtil.DecimalSeparator);
            buttonShowEarlier.Text = LanguageSettings.Current.ShowEarlierLater.ShowEarlier;
            buttonShowLater.Text = LanguageSettings.Current.ShowEarlierLater.ShowLater;
            radioButtonAllLines.Text = LanguageSettings.Current.ShowEarlierLater.AllLines;
            radioButtonSelectedLinesOnly.Text = LanguageSettings.Current.ShowEarlierLater.SelectedLinesOnly;
            radioButtonSelectedLineAndForward.Text = LanguageSettings.Current.ShowEarlierLater.SelectedLinesAndForward;
            UiUtil.FixLargeFonts(this, buttonShowEarlier);

            timeUpDownAdjust.MaskedTextBox.TextChanged += (sender, args) =>
            {
                if (timeUpDownAdjust.GetTotalMilliseconds() < 0)
                {
                    timeUpDownAdjust.SetTotalMilliseconds(0);
                    System.Threading.SynchronizationContext.Current.Post(TimeSpan.FromMilliseconds(10), () =>
                    {
                        timeUpDownAdjust.SetTotalMilliseconds(0);
                    });
                }
            };
        }

        public void ResetTotalAdjustment()
        {
            _totalAdjustment = TimeSpan.FromMilliseconds(0);
            labelTotalAdjustment.Text = string.Empty;
        }

        private void ShowEarlierLater_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#sync");
                e.SuppressKeyPress = true;
            }
        }

        internal void Initialize(AdjustEventHandler adjustCallback, bool onlySelected)
        {
            if (onlySelected)
            {
                radioButtonSelectedLinesOnly.Checked = true;
            }
            else if (Configuration.Settings.Tools.LastShowEarlierOrLaterSelection == SelectionChoice.SelectionAndForward.ToString())
            {
                radioButtonSelectedLineAndForward.Checked = true;
            }
            else
            {
                radioButtonAllLines.Checked = true;
            }

            _adjustCallback = adjustCallback;
            timeUpDownAdjust.TimeCode = new TimeCode(Configuration.Settings.General.DefaultAdjustMilliseconds);
        }

        private SelectionChoice GetSelectionChoice()
        {
            if (radioButtonSelectedLinesOnly.Checked)
            {
                return SelectionChoice.SelectionOnly;
            }

            if (radioButtonSelectedLineAndForward.Checked)
            {
                return SelectionChoice.SelectionAndForward;
            }

            return SelectionChoice.AllLines;
        }

        private void ButtonShowEarlierClick(object sender, EventArgs e)
        {
            TimeCode tc = timeUpDownAdjust.TimeCode;
            if (tc != null && tc.TotalMilliseconds > 0)
            {
                _adjustCallback.Invoke(-tc.TotalMilliseconds, GetSelectionChoice());
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds - tc.TotalMilliseconds);
                ShowTotalAdjustment();
                Configuration.Settings.General.DefaultAdjustMilliseconds = (int)tc.TotalMilliseconds;
            }
        }

        private void ShowTotalAdjustment()
        {
            var tc = new TimeCode(_totalAdjustment);

            if (Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
            {
                labelTotalAdjustment.Text = string.Format(LanguageSettings.Current.ShowEarlierLater.TotalAdjustmentX, tc.ToShortDisplayString());
                return;
            }

            if (tc.TotalSeconds < 60)
            {
                labelTotalAdjustment.Text = string.Format(LanguageSettings.Current.ShowEarlierLater.TotalAdjustmentX, string.Format(LanguageSettings.Current.General.XSeconds, tc.TotalSeconds));
                return;
            }

            labelTotalAdjustment.Text = string.Format(LanguageSettings.Current.ShowEarlierLater.TotalAdjustmentX, tc.ToShortString());
        }

        private void ButtonShowLaterClick(object sender, EventArgs e)
        {
            TimeCode tc = timeUpDownAdjust.TimeCode;
            if (tc != null && tc.TotalMilliseconds > 0)
            {
                _adjustCallback.Invoke(tc.TotalMilliseconds, GetSelectionChoice());
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds + tc.TotalMilliseconds);
                ShowTotalAdjustment();
                Configuration.Settings.General.DefaultAdjustMilliseconds = (int)tc.TotalMilliseconds;
            }
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            Text = ((RadioButton)sender).Text.RemoveChar('&');
        }

        private void ShowEarlierLater_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.LastShowEarlierOrLaterSelection = GetSelectionChoice().ToString();
        }

    }
}
