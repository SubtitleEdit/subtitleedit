using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public partial class TimeUpDown : UserControl
    {

        public enum TimeMode
        {
            HHMMSSMS,
            HHMMSSFF
        }

        const int NumericUpDownValue = 50;

        public EventHandler TimeCodeChanged;

        private bool _forceHHMMSSFF = false;

        internal void ForceHHMMSSFF()
        {
            _forceHHMMSSFF = true;
            maskedTextBox1.Mask = "00:00:00:00";
        }

        public TimeMode Mode
        {
            get
            {
                if (_forceHHMMSSFF)
                    return TimeMode.HHMMSSFF;

                if (Configuration.Settings == null)
                    return TimeMode.HHMMSSMS;
                if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                    return TimeMode.HHMMSSFF;
                return TimeMode.HHMMSSMS;
            }
        }

        public TimeUpDown()
        {
            InitializeComponent();
            numericUpDown1.ValueChanged += NumericUpDownValueChanged;
            numericUpDown1.Value = NumericUpDownValue;
            maskedTextBox1.InsertKeyMode = InsertKeyMode.Overwrite;
        }

        void NumericUpDownValueChanged(object sender, EventArgs e)
        {
            double? millisecs = GetTotalMilliseconds();
            if (millisecs.HasValue)
            {
                if (millisecs.Value >= TimeCode.MaxTime.TotalMilliseconds - 0.1)
                    millisecs = 0;

                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (numericUpDown1.Value > NumericUpDownValue)
                    {
                        SetTotalMilliseconds(millisecs.Value + 100);
                    }
                    else if (numericUpDown1.Value < NumericUpDownValue)
                    {
                        SetTotalMilliseconds(millisecs.Value - 100);
                    }
                }
                else
                {
                    if (numericUpDown1.Value > NumericUpDownValue)
                    {
                        SetTotalMilliseconds(millisecs.Value + Logic.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                    }
                    else if (numericUpDown1.Value < NumericUpDownValue)
                    {
                        if (millisecs.Value - 100 > 0)
                            SetTotalMilliseconds(millisecs.Value - Logic.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                        else if (millisecs.Value > 0)
                            SetTotalMilliseconds(0);
                    }
                }

                if (TimeCodeChanged != null)
                    TimeCodeChanged.Invoke(this, e);
            }
            numericUpDown1.Value = NumericUpDownValue;
        }

        public MaskedTextBox MaskedTextBox
        {
            get
            {
                return maskedTextBox1;
            }
        }

        public void SetTotalMilliseconds(double milliseconds)
        {
            if (Mode == TimeMode.HHMMSSMS)
            {
                if (Mode == TimeMode.HHMMSSMS && milliseconds < 0)
                    maskedTextBox1.Mask = "-00:00:00.000";
                else
                    maskedTextBox1.Mask = "00:00:00.000";
                maskedTextBox1.Text = new TimeCode(milliseconds).ToString();
            }
            else
            {
                maskedTextBox1.Text = new TimeCode(milliseconds).ToString().Substring(0, 9) + string.Format("{0:00}", Logic.SubtitleFormats.SubtitleFormat.MillisecondsToFrames(milliseconds));
            }
        }

        public double? GetTotalMilliseconds()
        {
            TimeCode tc = TimeCode;
            if (tc != null)
                return tc.TotalMilliseconds;
            return null;
        }

        public TimeCode TimeCode
        {
            get
            {
                if (maskedTextBox1.Text.Replace(".", string.Empty).Replace(",", string.Empty).Replace(":", string.Empty).Trim() == string.Empty)
                   return TimeCode.MaxTime;

                string startTime = maskedTextBox1.Text;
                startTime = startTime.Replace(' ', '0');

                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (startTime.EndsWith(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                        startTime += "000";

                    string[] times = startTime.Split(":,.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int hours = 0;
                        if (Utilities.IsInteger(times[0]))
                            hours = int.Parse(times[0]);

                        int minutes = 0;
                        if (Utilities.IsInteger(times[1]))
                            minutes = int.Parse(times[1]);

                        int seconds = 0;
                        if (Utilities.IsInteger(times[2]))
                            seconds = int.Parse(times[2]);

                        int milliSeconds = 0;
                        if (Utilities.IsInteger(times[3]))
                            milliSeconds = int.Parse(times[3].PadRight(3, '0'));

                        var tc = new TimeCode(hours, minutes, seconds, milliSeconds);
                        if (times[0].StartsWith("-") && tc.TotalMilliseconds > 0)
                            tc.TotalMilliseconds = tc.TotalMilliseconds * -1;
                        return tc;
                    }
                }
                else
                {
                    if (startTime.EndsWith(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) || startTime.EndsWith(":"))
                        startTime += "00";

                    string[] times = startTime.Split(":,.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int hours = 0;
                        if (Utilities.IsInteger(times[0]))
                            hours = int.Parse(times[0]);

                        int minutes = 0;
                        if (Utilities.IsInteger(times[1]))
                            minutes = int.Parse(times[1]);

                        int seconds = 0;
                        if (Utilities.IsInteger(times[2]))
                            seconds = int.Parse(times[2]);

                        int milliSeconds = 0;
                        if (Utilities.IsInteger(times[3]))
                            milliSeconds = Logic.SubtitleFormats.SubtitleFormat.FramesToMillisecondsMax999(int.Parse(times[3]));

                        return new TimeCode(hours, minutes, seconds, milliSeconds);
                    }
                }
                return null;
            }
            set
            {
                if (value.TotalMilliseconds >= TimeCode.MaxTime.TotalMilliseconds - 0.1)
                {
                    maskedTextBox1.Text = string.Empty;
                    return;
                }

                if (Mode == TimeMode.HHMMSSMS && value != null && value.TotalMilliseconds < 0)
                    maskedTextBox1.Mask = "-00:00:00.000";
                else if (Mode == TimeMode.HHMMSSMS)
                    maskedTextBox1.Mask = "00:00:00.000";
                else
                    maskedTextBox1.Mask = "00:00:00:00";
                if (value != null)
                {
                    if (Mode == TimeMode.HHMMSSMS)
                        maskedTextBox1.Text = value.ToString();
                    else
                        maskedTextBox1.Text = value.ToHHMMSSFF();
                }
                else
                {
                    maskedTextBox1.Text = new TimeCode(0).ToString();
                }
            }
        }

        private void MaskedTextBox1KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                numericUpDown1.UpButton();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                numericUpDown1.DownButton();
                e.SuppressKeyPress = true;
            }
        }

    }
}
