using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
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

        private bool _forceHHMMSSFF;

        public bool UseVideoOffset { get; set; }

        private static char[] _splitChars;

        internal void ForceHHMMSSFF()
        {
            _forceHHMMSSFF = true;
            maskedTextBox1.Mask = "00:00:00:00";
        }

        public TimeMode Mode
        {
            get
            {
                if (_forceHHMMSSFF || Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
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

            if (_splitChars == null)
            {
                var splitChars = new List<char> { ':', ',', '.' };
                string cultureSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                if (cultureSeparator.Length == 1)
                {
                    char ch = Convert.ToChar(cultureSeparator);
                    if (!splitChars.Contains(ch))
                    {
                        splitChars.Add(ch);
                    }
                }
                _splitChars = splitChars.ToArray();
            }
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
                        SetTotalMilliseconds(milliseconds.Value + Core.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                    }
                    else if (numericUpDown1.Value < NumericUpDownValue)
                    {
                        if (milliseconds.Value - 100 > 0)
                            SetTotalMilliseconds(milliseconds.Value - Core.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                        else if (milliseconds.Value > 0)
                            SetTotalMilliseconds(0);
                    }
                }
                TimeCodeChanged?.Invoke(this, e);
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
            if (UseVideoOffset)
            {
                milliseconds += Configuration.Settings.General.CurrentVideoOffsetInMs;
            }
            if (Mode == TimeMode.HHMMSSMS)
            {
                maskedTextBox1.Mask = GetMask(milliseconds);
                maskedTextBox1.Text = new TimeCode(milliseconds).ToString();
            }
            else
            {
                var tc = new TimeCode(milliseconds);
                maskedTextBox1.Text = tc.ToString().Substring(0, 9) + string.Format("{0:00}", Core.SubtitleFormats.SubtitleFormat.MillisecondsToFrames(tc.Milliseconds));
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
                if (string.IsNullOrWhiteSpace(maskedTextBox1.Text.Replace(".", string.Empty).Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, string.Empty).Replace(",", string.Empty).Replace(":", string.Empty)))
                    return TimeCode.MaxTime;

                string startTime = maskedTextBox1.Text;
                startTime = startTime.Replace(' ', '0');
                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (startTime.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal))
                        startTime += "000";

                    string[] times = startTime.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int hours;
                        int.TryParse(times[0], out hours);

                        int minutes;
                        int.TryParse(times[1], out minutes);
                        if (minutes > 59)
                            minutes = 59;

                        int seconds;
                        int.TryParse(times[2], out seconds);
                        if (seconds > 59)
                            seconds = 59;

                        int milliSeconds;
                        int.TryParse(times[3].PadRight(3, '0'), out milliSeconds);
                        var tc = new TimeCode(hours, minutes, seconds, milliSeconds);

                        if (UseVideoOffset)
                        {
                            tc.AddTime(-Configuration.Settings.General.CurrentVideoOffsetInMs);
                        }

                        if (hours < 0 && tc.TotalMilliseconds > 0)
                            tc.TotalMilliseconds *= -1;
                        return tc;
                    }
                }
                else
                {
                    if (startTime.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal) || startTime.EndsWith(':'))
                        startTime += "00";

                    string[] times = startTime.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);

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
                            milliSeconds = Core.SubtitleFormats.SubtitleFormat.FramesToMillisecondsMax999(milliSeconds);
                        }

                        var tc = new TimeCode(hours, minutes, seconds, milliSeconds);

                        if (UseVideoOffset)
                        {
                            tc.AddTime(-Configuration.Settings.General.CurrentVideoOffsetInMs);
                        }

                        return tc;
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

                var v = new TimeCode(value.TotalMilliseconds);
                if (UseVideoOffset)
                {
                    v.AddTime(Configuration.Settings.General.CurrentVideoOffsetInMs);
                }

                if (Mode == TimeMode.HHMMSSMS)
                {
                    maskedTextBox1.Mask = GetMask(v.TotalMilliseconds);
                    maskedTextBox1.Text = v.ToString();
                }
                else
                {
                    maskedTextBox1.Mask = "00:00:00:00";
                    maskedTextBox1.Text = v.ToHHMMSSFF();
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
                TimeCodeChanged?.Invoke(this, e);
                e.SuppressKeyPress = true;
            }
        }

        private string GetMask(double val) => val >= 0 ? "00:00:00.000" : "-00:00:00.000";
    }
}
