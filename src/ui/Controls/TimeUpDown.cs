using Nikse.SubtitleEdit.Core.Common;
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

        private readonly bool _designMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private const int NumericUpDownValue = 50;

        public EventHandler TimeCodeChanged;

        private bool _forceHHMMSSFF;

        public bool UseVideoOffset { get; set; }

        private static readonly char[] SplitChars = GetSplitChars();

        private bool _dirty;
        double _initialTotalMilliseconds;

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
                {
                    return TimeMode.HHMMSSFF;
                }

                return TimeMode.HHMMSSMS;
            }
        }

        public void SetAutoWidth()
        {
            int width;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var widthOfUpDown = 25;
                if (Configuration.IsRunningOnLinux)
                {
                    widthOfUpDown += 20;
                }
                var actualWidth = g.MeasureString("00:00:00:000", Font).Width;
                width = (int)Math.Round(actualWidth + widthOfUpDown);
            }
            var diff = Width - width;
            maskedTextBox1.Width -= diff;
            numericUpDown1.Width -= diff;
        }

        private static char[] GetSplitChars()
        {
            var splitChars = new List<char> { ':', ',', '.' };
            string cultureSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (cultureSeparator.Length == 1)
            {
                var ch = Convert.ToChar(cultureSeparator);
                if (!splitChars.Contains(ch))
                {
                    splitChars.Add(ch);
                }
            }
            return splitChars.ToArray();
        }

        public TimeUpDown()
        {
            Font = UiUtil.GetDefaultFont();
            InitializeComponent();
            UiUtil.FixFonts(this);
            numericUpDown1.ValueChanged += NumericUpDownValueChanged;
            numericUpDown1.Value = NumericUpDownValue;
            maskedTextBox1.InsertKeyMode = InsertKeyMode.Overwrite;
        }

        private void NumericUpDownValueChanged(object sender, EventArgs e)
        {
            _dirty = true;
            double? milliseconds = GetTotalMilliseconds();
            if (milliseconds.HasValue)
            {
                if (milliseconds.Value >= TimeCode.MaxTimeTotalMilliseconds - 0.1)
                {
                    milliseconds = 0;
                }

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
                        SetTotalMilliseconds(milliseconds.Value - Core.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                    }
                }
                TimeCodeChanged?.Invoke(this, e);
            }
            numericUpDown1.Value = NumericUpDownValue;
        }

        public MaskedTextBox MaskedTextBox => maskedTextBox1;

        public void SetTotalMilliseconds(double milliseconds)
        {
            _dirty = false;
            _initialTotalMilliseconds = milliseconds;
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
                maskedTextBox1.Mask = GetMaskFrames(milliseconds);
                maskedTextBox1.Text = tc.ToString().Substring(0, 9) + $"{Core.SubtitleFormats.SubtitleFormat.MillisecondsToFrames(tc.Milliseconds):00}";
            }
            _dirty = false;
        }

        public double? GetTotalMilliseconds()
        {
            return _dirty ? TimeCode?.TotalMilliseconds : _initialTotalMilliseconds;
        }

        public TimeCode TimeCode
        {
            get
            {
                if (_designMode)
                {
                    return new TimeCode();
                }

                if (string.IsNullOrWhiteSpace(maskedTextBox1.Text.RemoveChar('.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, string.Empty).RemoveChar(',', ':')))
                {
                    return new TimeCode(TimeCode.MaxTimeTotalMilliseconds);
                }

                if (!_dirty)
                {
                    return new TimeCode(_initialTotalMilliseconds);
                }

                string startTime = maskedTextBox1.Text;
                bool isNegative = startTime.StartsWith('-');
                startTime = startTime.TrimStart('-').Replace(' ', '0');
                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (startTime.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal))
                    {
                        startTime += "000";
                    }

                    var times = startTime.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int.TryParse(times[0], out var hours);

                        int.TryParse(times[1], out var minutes);
                        if (minutes > 59)
                        {
                            minutes = 59;
                        }

                        int.TryParse(times[2], out var seconds);
                        if (seconds > 59)
                        {
                            seconds = 59;
                        }

                        int.TryParse(times[3].PadRight(3, '0'), out var milliseconds);
                        var tc = new TimeCode(hours, minutes, seconds, milliseconds);

                        if (UseVideoOffset)
                        {
                            tc.TotalMilliseconds -= Configuration.Settings.General.CurrentVideoOffsetInMs;
                        }

                        if (isNegative)
                        {
                            tc.TotalMilliseconds *= -1;
                        }

                        return tc;
                    }
                }
                else
                {
                    if (startTime.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal) || startTime.EndsWith(':'))
                    {
                        startTime += "00";
                    }

                    string[] times = startTime.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int.TryParse(times[0], out var hours);

                        int.TryParse(times[1], out var minutes);

                        int.TryParse(times[2], out var seconds);

                        if (int.TryParse(times[3], out var milliseconds))
                        {
                            milliseconds = Core.SubtitleFormats.SubtitleFormat.FramesToMillisecondsMax999(milliseconds);
                        }

                        var tc = new TimeCode(hours, minutes, seconds, milliseconds);

                        if (UseVideoOffset)
                        {
                            tc.TotalMilliseconds -= Configuration.Settings.General.CurrentVideoOffsetInMs;
                        }

                        if (isNegative)
                        {
                            tc.TotalMilliseconds *= -1;
                        }

                        return tc;
                    }
                }
                return null;
            }
            set
            {
                if (_designMode)
                {
                    return;
                }

                if (value != null)
                {
                    _dirty = false;
                    _initialTotalMilliseconds = value.TotalMilliseconds;
                }

                if (value == null || value.TotalMilliseconds >= TimeCode.MaxTimeTotalMilliseconds - 0.1)
                {
                    maskedTextBox1.Text = string.Empty;
                    return;
                }

                var v = new TimeCode(value.TotalMilliseconds);
                if (UseVideoOffset)
                {
                    v.TotalMilliseconds += Configuration.Settings.General.CurrentVideoOffsetInMs;
                }

                if (Mode == TimeMode.HHMMSSMS)
                {
                    maskedTextBox1.Mask = GetMask(v.TotalMilliseconds);
                    maskedTextBox1.Text = v.ToString();
                }
                else
                {
                    maskedTextBox1.Mask = GetMaskFrames(v.TotalMilliseconds);
                    maskedTextBox1.Text = v.ToHHMMSSFF();
                }
            }
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
                _dirty = true;
            }
        }

        private static string GetMask(double val) => val >= 0 ? "00:00:00.000" : "-00:00:00.000";

        private static string GetMaskFrames(double val) => val >= 0 ? "00:00:00:00" : "-00:00:00:00";

        private void maskedTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _dirty = true;
            }
        }
    }
}
