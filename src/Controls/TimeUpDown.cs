using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public sealed partial class TimeUpDown : UserControl
    {
        public enum TimeMode
        {
            HHMMSSMS,
            HHMMSSFF
        }

        // reusable time code
        private readonly TimeCode _timeCode = new TimeCode();
        private readonly bool _designMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        private const int NumericUpDownValue = 50;

        public EventHandler TimeCodeChanged;

        private bool _forceHHMMSSFF;
        public bool UseVideoOffset { get; set; }
        private readonly char[] _splitChars;

        private bool _shouldGetTimeFromMaskedTextBox;

        internal void ForceHHMMSSFF()
        {
            _forceHHMMSSFF = true;
            maskedTextBox1.Mask = GetFrameBasedMask(0);
        }

        public TimeMode Mode
        {
            get
            {
                if (_forceHHMMSSFF || Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
                {
                    return TimeMode.HHMMSSFF;
                }

                return TimeMode.HHMMSSMS;
            }
        }

        /// <summary>
        /// Minimum value in milliseconds.
        /// </summary>
        public double Minimum { get; set; }

        /// <summary>
        /// Maximum value in milliseconds.
        /// </summary>
        public double Maximum { get; set; }

        public TimeUpDown()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = UiUtil.GetDefaultFont();
            InitializeComponent();
            UiUtil.FixFonts(this);
            numericUpDown1.ValueChanged += NumericUpDownValueChanged;
            numericUpDown1.Value = NumericUpDownValue;
            maskedTextBox1.InsertKeyMode = InsertKeyMode.Overwrite;

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

            Minimum = 0;
            Maximum = TimeCode.MaxTimeTotalMilliseconds;
        }

        private void NumericUpDownValueChanged(object sender, EventArgs e)
        {
            double milliSeconds = TimeCode.TotalMilliseconds;

            int delta = Mode == TimeMode.HHMMSSMS ? 100 : Core.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1);
            if (numericUpDown1.Value < NumericUpDownValue)
            {
                delta *= -1;
            }

            double updateMilliSeconds = milliSeconds + delta;

            // after adding changed value, make sure that the time doesn't go above/below max/mim threshold
            updateMilliSeconds = Math.Min(updateMilliSeconds, Maximum);
            updateMilliSeconds = Math.Max(updateMilliSeconds, Minimum);

            UpdateView(updateMilliSeconds);
            TimeCodeChanged?.Invoke(this, e);

            // unhook handler to avoid unnecessary re-invoking this method when reseting value to default
            numericUpDown1.ValueChanged -= NumericUpDownValueChanged;
            numericUpDown1.Value = NumericUpDownValue;
            numericUpDown1.ValueChanged += NumericUpDownValueChanged;
        }

        public MaskedTextBox MaskedTextBox => maskedTextBox1;

        public void UpdateView(double milliseconds)
        {
            _shouldGetTimeFromMaskedTextBox = false;

            if (UseVideoOffset)
            {
                milliseconds += Configuration.Settings.General.CurrentVideoOffsetInMs;
            }

            // update cache
            _timeCode.TotalMilliseconds = milliseconds;

            if (Mode == TimeMode.HHMMSSMS)
            {
                maskedTextBox1.Mask = GetTimeBasedMask(milliseconds);
                maskedTextBox1.Text = _timeCode.ToString();
            }
            else
            {
                maskedTextBox1.Mask = GetFrameBasedMask(milliseconds);
                maskedTextBox1.Text = _timeCode.ToHHMMSSFF();
            }
        }

        public TimeCode TimeCode
        {
            get
            {
                if (_designMode)
                {
                    return new TimeCode();
                }

                // return cached time. No update happened by chaging the value from view manually
                if (!_shouldGetTimeFromMaskedTextBox)
                {
                    return new TimeCode(_timeCode.TotalMilliseconds);
                }

                // get time from parsing masked text box
                string maskTime = maskedTextBox1.Text;

                // check if time can be parsed
                if (maskTime.Length > 0)
                {
                    string tempMaskTime = maskTime.RemoveChar('.').RemoveChar(',').RemoveChar(':');
                    if (!",.:".Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                    {
                        tempMaskTime = tempMaskTime.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, string.Empty);
                    }
                    // return max-time if current masked-time is un-parsable
                    if (string.IsNullOrWhiteSpace(tempMaskTime))
                    {
                        return TimeCode.Maxtime;
                    }
                }

                TimeCode tc = GetTimeCodeFromTokens(GetTokensFromMask(maskTime));

                // update cache. when the _shouldGetTimeFromMaskedTextBox is false
                // timecode will be reconstructed from this cached timecode
                _timeCode.TotalMilliseconds = tc.TotalMilliseconds;

                // time is already parsed to avoid reparsing this is set to false
                _shouldGetTimeFromMaskedTextBox = false;

                return tc;
            }
            set
            {
                if (_designMode)
                {
                    _timeCode.TotalMilliseconds = 0;
                }
                else
                {
                    // adjust time when overlaping threshold
                    double totalMilliSeconds = value.TotalMilliseconds;
                    totalMilliSeconds = Math.Max(totalMilliSeconds, Minimum); // val < 0
                    totalMilliSeconds = Math.Min(totalMilliSeconds, Maximum); // val > max

                    UpdateView(totalMilliSeconds);
                }
            }
        }

        private string[] GetTokensFromMask(string timeStamp)
        {
            int len = timeStamp.Length;

            // try to reconstruct time if it ends with one of the expected symbols
            if (len > 0 && CharUtils.IsDigit(timeStamp[len - 1]))
            {
                // 00:00:00[.,:] => 00:00:00:0
                if (timeStamp.LastIndexOfAny(_splitChars, len - 1, 1) >= 0)
                {
                    timeStamp += "0";
                }
                timeStamp = timeStamp.Replace(' ', '0');
            }

            return timeStamp.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);
        }

        private TimeCode GetTimeCodeFromTokens(string[] tokens)
        {
            // invalid tokens
            if (tokens.Length < 4)
            {
                return new TimeCode();
            }

            int.TryParse(tokens[0], out int hours);
            int.TryParse(tokens[1], out int minutes);
            int.TryParse(tokens[2], out int seconds);

            // handle overlaps (happens when user press key 9 in minute / seconds position from UI)
            minutes = Math.Min(minutes, 59);
            seconds = Math.Min(seconds, 59);

            const int IndexMilliSecondsOrFrames = 3;

            int millisSeconds;
            if (Mode == TimeMode.HHMMSSMS)
            {
                int.TryParse(tokens[IndexMilliSecondsOrFrames].PadRight(3, '0'), out millisSeconds);
            }
            else if (int.TryParse(tokens[IndexMilliSecondsOrFrames], out millisSeconds)) // frame
            {
                millisSeconds = Core.SubtitleFormats.SubtitleFormat.FramesToMillisecondsMax999(millisSeconds);
            }

            var tc = new TimeCode(Math.Abs(hours), minutes, seconds, millisSeconds);

            // handle negative time
            if (hours < 0)
            {
                tc.TotalMilliseconds *= -1;
            }

            if (UseVideoOffset)
            {
                tc.TotalMilliseconds -= Configuration.Settings.General.CurrentVideoOffsetInMs;
            }

            return tc;
        }

        private void MaskedTextBox1KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Up)
            {
                numericUpDown1.UpButton();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Down)
            {
                numericUpDown1.DownButton();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Enter)
            {
                TimeCodeChanged?.Invoke(this, e);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData != (Keys.Tab | Keys.Shift) &&
                     e.KeyData != Keys.Tab &&
                     e.KeyData != Keys.Left &&
                     e.KeyData != Keys.Right)
            {
                _shouldGetTimeFromMaskedTextBox = true;
            }
        }

        private static string GetTimeBasedMask(double val) => val >= 0 ? "00:00:00.000" : "-00:00:00.000";

        private static string GetFrameBasedMask(double val) => val >= 0 ? "00:00:00:00" : "-00:00:00:00";

        private void maskedTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _shouldGetTimeFromMaskedTextBox = true;
            }
        }
    }
}