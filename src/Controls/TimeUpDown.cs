using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
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

        private const int NumericUpDownValue = 50;

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

        private void NumericUpDownValueChanged(object sender, EventArgs e)
        {
            double? milliseconds = GetTotalMilliseconds();
            if (milliseconds.HasValue)
            {
                if (milliseconds.Value >= TimeCode.MaxTime.TotalMilliseconds - 0.1)
                    milliseconds = 0;

                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (numericUpDown1.Value > NumericUpDownValue)
                    {
                        SetTotalMilliseconds(milliseconds.Value + 100);
                    }
                    else if (numericUpDown1.Value < NumericUpDownValue)
                    {
                        SetTotalMilliseconds(milliseconds.Value - 100);
                    }
                }
                else
                {
                    if (numericUpDown1.Value > NumericUpDownValue)
                    {
                        SetTotalMilliseconds(milliseconds.Value + Logic.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                    }
                    else if (numericUpDown1.Value < NumericUpDownValue)
                    {
                        if (milliseconds.Value - 100 > 0)
                            SetTotalMilliseconds(milliseconds.Value - Logic.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                        else if (milliseconds.Value > 0)
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
                if (milliseconds < 0)
                    maskedTextBox1.Mask = "-00:00:00.000";
                else
                    maskedTextBox1.Mask = "00:00:00.000";
                maskedTextBox1.Text = new TimeCode(milliseconds).ToString();
            }
            else
            {
                var tc = new TimeCode(milliseconds);
                maskedTextBox1.Text = tc.ToString().Substring(0, 9) + string.Format("{0:00}", Logic.SubtitleFormats.SubtitleFormat.MillisecondsToFrames(tc.Milliseconds));
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
                if (string.IsNullOrWhiteSpace(maskedTextBox1.Text.Replace(".", string.Empty).Replace(",", string.Empty).Replace(":", string.Empty)))
                    return TimeCode.MaxTime;

                string startTime = maskedTextBox1.Text;
                startTime = startTime.Replace(' ', '0');
                char[] splitChars = { ':', ',', '.' };
                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (startTime.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                        startTime += "000";

                    string[] times = startTime.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int hours;
                        int.TryParse(times[0], out hours);

                        int minutes;
                        int.TryParse(times[1], out minutes);

                        int seconds;
                        int.TryParse(times[2], out seconds);

                        int milliSeconds;
                        int.TryParse(times[3].PadRight(3, '0'), out milliSeconds);

                        var tc = new TimeCode(hours, minutes, seconds, milliSeconds);
                        if (hours < 0 && tc.TotalMilliseconds > 0)
                            tc.TotalMilliseconds *= -1;
                        return tc;
                    }
                }
                else
                {
                    if (startTime.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) || startTime.EndsWith(':'))
                        startTime += "00";

                    string[] times = startTime.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int hours;
                        int.TryParse(times[0], out hours);

                        int minutes;
                        int.TryParse(times[1], out minutes);

                        int seconds;
                        int.TryParse(times[2], out seconds);

                        int milliSeconds;
                        if (int.TryParse(times[3], out milliSeconds))
                        {
                            milliSeconds = Logic.SubtitleFormats.SubtitleFormat.FramesToMillisecondsMax999(milliSeconds);
                        }

                        return new TimeCode(hours, minutes, seconds, milliSeconds);
                    }
                }
                return null;
            }
            set
            {
                if (value == null || value.TotalMilliseconds >= TimeCode.MaxTime.TotalMilliseconds - 0.1)
                {
                    maskedTextBox1.Text = string.Empty;
                    return;
                }

                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (value.TotalMilliseconds < 0)
                        maskedTextBox1.Mask = "-00:00:00.000";
                    else
                        maskedTextBox1.Mask = "00:00:00.000";

                    maskedTextBox1.Text = value.ToString();
                }
                else
                {
                    maskedTextBox1.Mask = "00:00:00:00";
                    maskedTextBox1.Text = value.ToHHMMSSFF();
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
            else if (e.KeyCode == Keys.Enter)
            {
                if (TimeCodeChanged != null)
                    TimeCodeChanged.Invoke(this, e);
                e.SuppressKeyPress = true;
            }
        }

    }
}