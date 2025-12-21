using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ShowEarlierLater : PositionAndSizeForm
    {
        public class ViewStatus
        {
            public bool AllowSelection { get; set; }
        }

        public delegate void AdjustEventHandler(double adjustMilliseconds, SelectionChoice selection, bool syncPlayer);
        public delegate void AllowSelectionHandler(object sender, ViewStatus viewStatus);
        public event AllowSelectionHandler AllowSelection;
        private TimeSpan _totalAdjustment;
        private AdjustEventHandler _adjustCallback;

        private bool _canSyncPlayer;

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
                    TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(10), () =>
                    {
                        timeUpDownAdjust.SetTotalMilliseconds(0);
                    });
                }
            };

            timerRefreshAllowSelection.Start();
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

        internal void Initialize(AdjustEventHandler adjustCallback, bool onlySelected, bool canSyncPlayer)
        {
            _canSyncPlayer = canSyncPlayer;
            checkBoxSyncPlayer.Visible = canSyncPlayer;

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
            Earlier(tc);
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
            Later(tc);
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            Text = ((RadioButton)sender).Text.RemoveChar('&');
        }

        private void ShowEarlierLater_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerRefreshAllowSelection.Stop();
            Configuration.Settings.Tools.LastShowEarlierOrLaterSelection = GetSelectionChoice().ToString();
        }

        private void timerRefreshAllowSelection_Tick(object sender, EventArgs e)
        {
            if (AllowSelection != null)
            {
                var viewStatus = new ViewStatus { AllowSelection = radioButtonAllLines.Enabled };
                AllowSelection.Invoke(this, viewStatus);
                if (viewStatus.AllowSelection)
                {
                    radioButtonSelectedLinesOnly.Enabled = true;
                    radioButtonSelectedLineAndForward.Enabled = true;
                    radioButtonAllLines.Enabled = true;
                }
                else
                {
                    radioButtonSelectedLinesOnly.Enabled = false;
                    radioButtonSelectedLineAndForward.Enabled = false;
                    radioButtonAllLines.Enabled = false;
                    radioButtonAllLines.Checked = true;
                }
            }
        }

        private void Earlier(TimeCode tc)
        {
            Earlier(tc?.TimeSpan ?? TimeSpan.Zero);
        }

        private void Earlier(TimeSpan offset)
        {
            Adjust(offset, -1);
        }

        private void Later(TimeCode tc)
        {
            Later(tc?.TimeSpan ?? TimeSpan.Zero);
        }

        private void Later(TimeSpan offset)
        {
            Adjust(offset);
        }

        private void Adjust(TimeSpan offset, double scale = 1)
        {
            var duration = offset.Duration();
            var durationMs = duration.TotalMilliseconds;
            Debug.Assert(durationMs >= 0);

            if (durationMs > 0)
            {
                durationMs = scale * durationMs;

                var shouldSyncWithPlayer = _canSyncPlayer && checkBoxSyncPlayer.Checked;

                _adjustCallback.Invoke(durationMs, GetSelectionChoice(), shouldSyncWithPlayer);
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds + durationMs);
                ShowTotalAdjustment();
                Configuration.Settings.General.DefaultAdjustMilliseconds = (int)Math.Abs(durationMs);
            }
        }

        private void buttonQuick10sEarlier_Click(object sender, EventArgs e)
        {
            Earlier(TimeSpan.FromSeconds(10));
        }

        private void buttonQuick10sLater_Click(object sender, EventArgs e)
        {
            Later(TimeSpan.FromSeconds(10));
        }

        private void buttonQuick1sEarlier_Click(object sender, EventArgs e)
        {
            Earlier(TimeSpan.FromSeconds(1));
        }

        private void buttonQuick1sLater_Click(object sender, EventArgs e)
        {
            Later(TimeSpan.FromSeconds(1));
        }

        private void buttonQuick500msEarlier_Click(object sender, EventArgs e)
        {
            Earlier(TimeSpan.FromMilliseconds(500));
        }

        private void buttonQuick500msLater_Click(object sender, EventArgs e)
        {
            Later(TimeSpan.FromMilliseconds(500));
        }

        private void buttonQuick100msEarlier_Click(object sender, EventArgs e)
        {
            Earlier(TimeSpan.FromMilliseconds(100));
        }

        private void buttonQuick100msLater_Click(object sender, EventArgs e)
        {
            Later(TimeSpan.FromMilliseconds(100));
        }

        private void buttonQuick10msEarlier_Click(object sender, EventArgs e)
        {
            Earlier(TimeSpan.FromMilliseconds(10));
        }

        private void buttonQuick10msLater_Click(object sender, EventArgs e)
        {
            Later(TimeSpan.FromMilliseconds(10));
        }

        private void buttonQuick1msEarlier_Click(object sender, EventArgs e)
        {
            Earlier(TimeSpan.FromMilliseconds(1));
        }

        private void buttonQuick1msLater_Click(object sender, EventArgs e)
        {
            Later(TimeSpan.FromMilliseconds(1));
        }
    }
}
