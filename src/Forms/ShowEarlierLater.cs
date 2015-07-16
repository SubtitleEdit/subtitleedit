using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Enums;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ShowEarlierLater : PositionAndSizeForm
    {
        public delegate void AdjustEventHandler(double adjustMilliseconds, SelectionChoice selection);

        private TimeSpan _totalAdjustment;
        private AdjustEventHandler _adjustCallback;

        public ShowEarlierLater()
        {
            InitializeComponent();
            ResetTotalAdjustment();
            timeUpDownAdjust.MaskedTextBox.Text = "000000000";

            Text = Configuration.Settings.Language.ShowEarlierLater.Title;
            labelHourMinSecMilliSecond.Text = Configuration.Settings.Language.General.HourMinutesSecondsMilliseconds;
            buttonShowEarlier.Text = Configuration.Settings.Language.ShowEarlierLater.ShowEarlier;
            buttonShowLater.Text = Configuration.Settings.Language.ShowEarlierLater.ShowLater;
            radioButtonAllLines.Text = Configuration.Settings.Language.ShowEarlierLater.AllLines;
            radioButtonSelectedLinesOnly.Text = Configuration.Settings.Language.ShowEarlierLater.SelectedLinesOnly;
            radioButtonSelectedLineAndForward.Text = Configuration.Settings.Language.ShowEarlierLater.SelectedLinesAndForward;
            Utilities.FixLargeFonts(this, buttonShowEarlier);
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
                radioButtonSelectedLinesOnly.Checked = true;
            else if (Configuration.Settings.Tools.LastShowEarlierOrLaterSelection == SelectionChoice.SelectionAndForward.ToString())
                radioButtonSelectedLineAndForward.Checked = true;
            else
                radioButtonAllLines.Checked = true;

            _adjustCallback = adjustCallback;
            timeUpDownAdjust.TimeCode = new TimeCode(Configuration.Settings.General.DefaultAdjustMilliseconds);
        }

        private SelectionChoice GetSelectionChoice()
        {
            if (radioButtonSelectedLinesOnly.Checked)
                return SelectionChoice.SelectionOnly;
            else if (radioButtonSelectedLineAndForward.Checked)
                return SelectionChoice.SelectionAndForward;
            else
                return SelectionChoice.AllLines;
        }

        private void ButtonShowEarlierClick(object sender, EventArgs e)
        {
            TimeCode tc = timeUpDownAdjust.TimeCode;
            if (tc != null && tc.TotalMilliseconds > 0)
            {
                _adjustCallback.Invoke(-tc.TotalMilliseconds, GetSelectionChoice());
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds - tc.TotalMilliseconds);
                ShowTotalAdjustMent();
            }
        }

        private void ShowTotalAdjustMent()
        {
            TimeCode tc = new TimeCode(_totalAdjustment);
            labelTotalAdjustment.Text = string.Format(Configuration.Settings.Language.ShowEarlierLater.TotalAdjustmentX, tc.ToShortString());
            Configuration.Settings.General.DefaultAdjustMilliseconds = (int)tc.TotalMilliseconds;
        }

        private void ButtonShowLaterClick(object sender, EventArgs e)
        {
            TimeCode tc = timeUpDownAdjust.TimeCode;
            if (tc != null && tc.TotalMilliseconds > 0)
            {
                _adjustCallback.Invoke(tc.TotalMilliseconds, GetSelectionChoice());
                _totalAdjustment = TimeSpan.FromMilliseconds(_totalAdjustment.TotalMilliseconds + tc.TotalMilliseconds);
                ShowTotalAdjustMent();
            }
        }

        private void radioButtonAllLines_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSelectedLinesOnly.Checked)
                Text = Configuration.Settings.Language.ShowEarlierLater.Title;
            else
                Text = Configuration.Settings.Language.ShowEarlierLater.TitleAll;
        }

        private void ShowEarlierLater_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.LastShowEarlierOrLaterSelection = GetSelectionChoice().ToString();
        }

    }
}