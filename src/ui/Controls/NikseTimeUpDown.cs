using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Controls
{
    [Category("NikseTimeUpDown"), Description("Subtitle time with better support for color theme")]
    public sealed class NikseTimeUpDown : Control
    {
        // ReSharper disable once InconsistentNaming
        public EventHandler TimeCodeChanged;

        public enum TimeMode
        {
            HHMMSSMS,
            HHMMSSFF
        }

        private bool _loading = true;

        private const int NumericUpDownValue = 50;

        private bool _forceHHMMSSFF;

        public bool UseVideoOffset { get; set; }

        private static readonly char[] SplitChars = GetSplitChars();

        private bool _dirty;
        private double _initialTotalMilliseconds;

        internal void ForceHHMMSSFF()
        {
            _forceHHMMSSFF = true;
            _maskedTextBox.Mask = "00:00:00:00";
        }

        public void SetAutoWidth()
        {
            Invalidate();
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

        [Category("NikseTimeUpDown"), Description("Gets or sets the increment value"), DefaultValue(100)]
        public decimal Increment { get; set; }

        [Category("NikseTimeUpDown"), Description("Allow arrow keys to set increment/decrement value")]
        [DefaultValue(true)]
        public bool InterceptArrowKeys { get; set; }

        private Color _buttonForeColor;
        private Brush _buttonForeColorBrush;
        [Category("NikseTimeUpDown"), Description("Gets or sets the button foreground color"),
         RefreshProperties(RefreshProperties.Repaint)]
        public Color ButtonForeColor
        {
            get => _buttonForeColor;
            set
            {
                if (value.A == 0)
                {
                    return;
                }

                _buttonForeColor = value;
                _buttonForeColorBrush?.Dispose();
                _buttonForeColorBrush = new SolidBrush(_buttonForeColor);
                Invalidate();
            }
        }

        private Color _buttonForeColorOver;
        private Brush _buttonForeColorOverBrush;
        [Category("NikseTimeUpDown"), Description("Gets or sets the button foreground mouse over color"), RefreshProperties(RefreshProperties.Repaint)]
        public Color ButtonForeColorOver
        {
            get => _buttonForeColorOver;

            set
            {
                if (value.A == 0)
                {
                    return;
                }

                _buttonForeColorOver = value;
                _buttonForeColorOverBrush?.Dispose();
                _buttonForeColorOverBrush = new SolidBrush(_buttonForeColorOver);
                Invalidate();
            }
        }

        private Color _buttonForeColorDown;
        private Brush _buttonForeColorDownBrush;
        [Category("NikseTimeUpDown"), Description("Gets or sets the button foreground mouse down color"), RefreshProperties(RefreshProperties.Repaint)]
        public Color ButtonForeColorDown
        {
            get => _buttonForeColorDown;

            set
            {
                if (value.A == 0)
                {
                    return;
                }

                _buttonForeColorDown = value;
                _buttonForeColorDownBrush?.Dispose();
                _buttonForeColorDownBrush = new SolidBrush(_buttonForeColorDown);
                Invalidate();
            }
        }

        private Color _borderColor;
        [Category("NikseTimeUpDown"), Description("Gets or sets the border color"), RefreshProperties(RefreshProperties.Repaint)]
        public Color BorderColor
        {
            get => _borderColor;

            set
            {
                if (value.A == 0)
                {
                    return;
                }

                _borderColor = value;
                Invalidate();
            }
        }

        private Color _backColorDisabled;
        [Category("NikseTimeUpDown"), Description("Gets or sets the button foreground color"),
         RefreshProperties(RefreshProperties.Repaint)]
        public Color BackColorDisabled
        {
            get => _backColorDisabled;
            set
            {
                if (value.A == 0)
                {
                    return;
                }

                _backColorDisabled = value;
                Invalidate();
            }
        }

        private Color _borderColorDisabled;
        [Category("NikseTimeUpDown"), Description("Gets or sets the disabled border color"), RefreshProperties(RefreshProperties.Repaint)]
        public Color BorderColorDisabled
        {
            get => _borderColorDisabled;

            set
            {
                if (value.A == 0)
                {
                    return;
                }

                _borderColorDisabled = value;
                Invalidate();
            }
        }

        private readonly MaskedTextBox _maskedTextBox;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!_maskedTextBox.Focused && e.KeyCode == (Keys.Control | Keys.C))
            {
                _maskedTextBox.Copy();
                e.Handled = true;
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        // https://stackoverflow.com/questions/2612487/how-to-fix-the-flickering-in-user-controls
        //        // https://stackoverflow.com/questions/69908353/window-not-fully-painting-until-i-grab-and-move-it
        //        var cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
        //        return cp;

        //        //var parms = base.CreateParams;
        //        //parms.Style |= 0x02000000;  // Turn off WS_CLIPCHILDREN
        //        //return parms;
        //    }
        //}

        public NikseTimeUpDown()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            DoubleBuffered = true;

            _maskedTextBox = new MaskedTextBox();
            Height = 23;
            _maskedTextBox.FontChanged += (o, args) =>
            {
                base.OnFontChanged(args); 
                Invalidate();
            };
            _maskedTextBox.BorderStyle = BorderStyle.None;
            _maskedTextBox.Font = UiUtil.GetDefaultFont();
            _maskedTextBox.KeyPress += TextBox_KeyPress;
            _maskedTextBox.KeyDown += (sender, e) =>
            {
                if (InterceptArrowKeys && e.KeyCode == Keys.Down)
                {
                    AddValue(-Increment);
                    e.Handled = true;
                }
                else if (InterceptArrowKeys && e.KeyCode == Keys.Up)
                {
                    AddValue(Increment);
                    e.Handled = true;
                }
                else if (e.KeyData == Keys.Enter)
                {
                    if (!_maskedTextBox.MaskCompleted)
                    {
                        AddValue(0);
                    }

                    Invalidate();
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
                {
                    _maskedTextBox.SelectAll();
                    e.SuppressKeyPress = true;
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
                {
                    _maskedTextBox.Copy();
                    e.SuppressKeyPress = true;
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V)
                {
                    _maskedTextBox.Paste();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyData != (Keys.Tab | Keys.Shift) &&
                         e.KeyData != Keys.Tab &&
                         e.KeyData != Keys.Left &&
                         e.KeyData != Keys.Right)
                {
                    _dirty = true;
                    Invalidate();
                }
            };
            _maskedTextBox.LostFocus += (sender, args) =>
            {
                Invalidate();
            };
            _maskedTextBox.GotFocus += (sender, args) => {
                Invalidate();
            };
            _maskedTextBox.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _dirty = true;
                }
            };

            Controls.Add(_maskedTextBox);
            BackColor = new TextBox().BackColor;
            ButtonForeColor = DefaultForeColor;
            ButtonForeColorOver = Color.FromArgb(0, 120, 215);
            ButtonForeColorDown = Color.Orange;
            BorderColor = Color.FromArgb(171, 173, 179);
            BorderColorDisabled = Color.FromArgb(120, 120, 120);
            BackColorDisabled = Color.FromArgb(240, 240, 240);
            InterceptArrowKeys = true;
            Increment = 100;

            _repeatTimer = new Timer();
            _repeatTimer.Tick += (sender, args) =>
            {
                if (_repeatTimerArrowUp)
                {
                    AddValue(Increment);
                }
                else
                {
                    AddValue(-Increment);
                }

                _repeatCount++;
                _repeatTimer.Interval = _repeatCount < 8 ? 75 : 10;
            };

            LostFocus += (sender, args) => _repeatTimer.Stop();

            _maskedTextBox.InsertKeyMode = InsertKeyMode.Overwrite;
            _maskedTextBox.LostFocus += (sender, args) =>
            {
                AddValue(0);
            };

            TabStop = false;

            // having trouble to getting control drawn correctly at load...
            var startRenderTimer = new Timer();
            startRenderTimer.Interval = 397;
            startRenderTimer.Tick += (sender, args) =>
            {
                Invalidate();
                _startRenderCount++;
                if (_startRenderCount >= StartRenderMaxCount)
                {
                    startRenderTimer.Stop();
                    startRenderTimer.Dispose();
                }
            };
            startRenderTimer.Start();
            _loading = false;
        }

        public MaskedTextBox MaskedTextBox => _maskedTextBox;

        public void SetTotalMilliseconds(double milliseconds)
        {
            _dirty = false;
            _initialTotalMilliseconds = milliseconds;
            string mask;
            if (UseVideoOffset)
            {
                milliseconds += Configuration.Settings.General.CurrentVideoOffsetInMs;
            }

            if (Mode == TimeMode.HHMMSSMS)
            {
                mask = GetMask(milliseconds);
                if (_maskedTextBox.Mask != mask)
                {
                    _maskedTextBox.Mask = mask;
                }

                _maskedTextBox.Text = new TimeCode(milliseconds).ToString();
            }
            else
            {
                var tc = new TimeCode(milliseconds);
                mask = GetMaskFrames(milliseconds);
                if (_maskedTextBox.Mask != mask)
                {
                    _maskedTextBox.Mask = mask;
                }
                _maskedTextBox.Text = tc.ToString().Substring(0, 9) + $"{Core.SubtitleFormats.SubtitleFormat.MillisecondsToFrames(tc.Milliseconds):00}";
            }

            _dirty = false;
        }

        public double? GetTotalMilliseconds()
        {
            return _dirty ? TimeCode?.TotalMilliseconds : _initialTotalMilliseconds;
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        public TimeCode TimeCode
        {
            get
            {
                if (_loading)
                {
                    return new TimeCode();
                }

                if (string.IsNullOrWhiteSpace(_maskedTextBox.Text.RemoveChar('.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, string.Empty).RemoveChar(',', ':')))
                {
                    return new TimeCode(TimeCode.MaxTimeTotalMilliseconds);
                }

                if (!_dirty)
                {
                    return new TimeCode(_initialTotalMilliseconds);
                }

                var startTime = _maskedTextBox.Text;
                var isNegative = startTime.StartsWith('-');
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

                    var times = startTime.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);

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
                if (_loading)
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
                    _maskedTextBox.Text = string.Empty;
                    return;
                }

                var v = new TimeCode(value.TotalMilliseconds);
                if (UseVideoOffset)
                {
                    v.TotalMilliseconds += Configuration.Settings.General.CurrentVideoOffsetInMs;
                }

                if (Mode == TimeMode.HHMMSSMS)
                {
                    _maskedTextBox.Mask = GetMask(v.TotalMilliseconds);
                    _maskedTextBox.Text = v.ToString();
                }
                else
                {
                    _maskedTextBox.Mask = GetMaskFrames(v.TotalMilliseconds);
                    _maskedTextBox.Text = v.ToHHMMSSFF();
                }

                Invalidate();
            }
        }

        private static string GetMask(double val) => val >= 0 ? "00:00:00.000" : "-00:00:00.000";

        private static string GetMaskFrames(double val) => val >= 0 ? "00:00:00:00" : "-00:00:00:00";

        public void Theme()
        {
            var enabled = Enabled;
            Enabled = true;
            if (Configuration.Settings.General.UseDarkTheme)
            {
                BackColor = DarkTheme.BackColor;
                MaskedTextBox.BackColor = DarkTheme.BackColor;
                BackColor = DarkTheme.BackColor;
            }
            else
            {
                BackColor = DefaultBackColor;
                using (var tb = new TextBox())
                {
                    MaskedTextBox.BackColor = tb.BackColor;
                    BackColor = tb.BackColor;
                }
            }

            Enabled = enabled;
        }

        /// <summary>
        /// Allow only digits, Enter and Backspace key.
        /// </summary>
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                AddValue(0);
                e.Handled = false;
            }
            else if (char.IsDigit(e.KeyChar) || e.KeyChar == (char)Keys.Back)
            {
                e.Handled = false;
                Invalidate();
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Increment or decrement the TextBox value.
        /// </summary>
        /// <param name="value">Value to increment/decrement</param>
        private void AddValue(decimal value)
        {
            _dirty = true;
            var milliseconds = GetTotalMilliseconds();
            if (milliseconds.HasValue)
            {
                if (milliseconds.Value >= TimeCode.MaxTimeTotalMilliseconds - 0.1)
                {
                    milliseconds = 0;
                }

                if (Mode == TimeMode.HHMMSSMS)
                {
                    SetTotalMilliseconds(milliseconds.Value + (double)value);
                }
                else
                {
                    if (value == 0)
                    {
                        SetTotalMilliseconds(milliseconds.Value);
                    }
                    else if (value > NumericUpDownValue)
                    {
                        SetTotalMilliseconds(milliseconds.Value + Core.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                    }
                    else if (value < NumericUpDownValue)
                    {
                        SetTotalMilliseconds(milliseconds.Value - Core.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                    }
                }

                TimeCodeChanged?.Invoke(this, null);
            }
        }

        private bool _buttonUpActive;
        private bool _buttonDownActive;

        private bool _buttonLeftIsDown;

        private int _mouseX;
        private int _mouseY;

        private readonly Timer _repeatTimer;
        private bool _repeatTimerArrowUp;
        private int _repeatCount;

        private const int StartRenderMaxCount = 7;
        private int _startRenderCount;

        protected override void OnMouseEnter(EventArgs e)
        {
            _buttonUpActive = false;
            _buttonDownActive = false;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _buttonUpActive = false;
            _buttonDownActive = false;
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _buttonLeftIsDown = true;

                if (_buttonUpActive)
                {
                    AddValue(Increment);
                    _repeatTimerArrowUp = true;
                    _repeatTimer.Interval = 300;
                    _repeatCount = 0;
                    _repeatTimer.Start();
                }
                else if (_buttonDownActive)
                {
                    AddValue(-Increment);
                    _repeatTimerArrowUp = false;
                    _repeatTimer.Interval = 300;
                    _repeatCount = 0;
                    _repeatTimer.Start();
                }

                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _repeatTimer.Stop();

            if (_buttonLeftIsDown)
            {
                _buttonLeftIsDown = false;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var left = RightToLeft == RightToLeft.Yes ? 0 : Width - ButtonsWidth;
            var right = RightToLeft == RightToLeft.Yes ? ButtonsWidth : Width;
            var height = Height / 2 - 3;
            const int top = 2;

            _mouseX = e.X;
            _mouseY = e.Y;

            if (_mouseX >= left && _mouseX <= right)
            {
                if (_mouseY > top + height)
                {
                    if (!_buttonDownActive)
                    {
                        _buttonUpActive = false;
                        _buttonDownActive = true;
                        Invalidate();
                    }
                }
                else
                {
                    if (!_buttonUpActive)
                    {
                        _buttonUpActive = true;
                        _buttonDownActive = false;
                        Invalidate();
                    }
                }
            }
            else
            {
                if (_buttonUpActive || _buttonDownActive)
                {
                    _buttonUpActive = false;
                    _buttonDownActive = false;
                    Invalidate();
                }
                _repeatTimer.Stop();
            }

            base.OnMouseMove(e);
        }

        private const int ButtonsWidth = 13;

        [RefreshProperties(RefreshProperties.Repaint)]
        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                if (value == Enabled)
                {
                    return;
                }

                base.Enabled = value;
                Invalidate();
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        public new int Height
        {
            get => base.Height;
            set
            {
                if (_maskedTextBox != null)
                {
                    _maskedTextBox.Height = value - 4;
                }

                base.Height = value;
                Invalidate();
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        public new int Width
        {
            get => base.Width;
            set
            {
                if (_maskedTextBox != null)
                {
                    _maskedTextBox.Width = Width - ButtonsWidth - 3;
                }

                base.Width = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            _maskedTextBox.BackColor = BackColor;
            _maskedTextBox.ForeColor = ButtonForeColor;
            _maskedTextBox.Top = 2;
            _maskedTextBox.Left = RightToLeft == RightToLeft.Yes ? ButtonsWidth : 3;
            _maskedTextBox.Height = Height - 4;
            _maskedTextBox.Width = Width - ButtonsWidth - 3;
            _maskedTextBox.Invalidate();

            if (!Enabled)
            {
                DrawDisabled(e);
                return;
            }

            base.OnPaint(e);
            using (var pen = _maskedTextBox.Focused ? new Pen(_buttonForeColorOver, 1f) : new Pen(BorderColor, 1f))
            {
                var borderRectangle = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
                e.Graphics.DrawRectangle(pen, borderRectangle);
            }

            var brush = _buttonForeColorBrush;
            var left = RightToLeft == RightToLeft.Yes ? 3 : Width - ButtonsWidth;
            var height = e.ClipRectangle.Height / 2 - 4;
            var top = 2;
            if (_buttonUpActive)
            {
                brush = _buttonLeftIsDown ? _buttonForeColorDownBrush : _buttonForeColorOverBrush;
            }

            DrawArrowUp(e, brush, left, top, height);

            if (_buttonDownActive)
            {
                brush = _buttonLeftIsDown ? _buttonForeColorDownBrush : _buttonForeColorOverBrush;
            }
            else
            {
                brush = _buttonForeColorBrush;
            }

            top = height + 5;
            DrawArrowDown(e, brush, left, top, height);
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        public override RightToLeft RightToLeft
        {
            get => base.RightToLeft;
            set
            {
                base.RightToLeft = value;
                Application.DoEvents();
                Invalidate();
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
                _maskedTextBox.ForeColor = value;
                Application.DoEvents();
                Invalidate();
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                _maskedTextBox.BackColor = value;
                Application.DoEvents();
                Invalidate();
            }
        }

        private static void DrawArrowDown(PaintEventArgs e, Brush brush, int left, int top, int height)
        {
            e.Graphics.FillPolygon(brush,
                new[]
                {
                    new Point(left + 5, top + height),
                    new Point(left + 0, top + 0),
                    new Point(left + 10, top + 0)
                });
        }

        private static void DrawArrowUp(PaintEventArgs e, Brush brush, int left, int top, int height)
        {
            e.Graphics.FillPolygon(brush,
                new[]
                {
                    new Point(left + 5, top + 0),
                    new Point(left + 0, top + height),
                    new Point(left + 10, top + height)
                });
        }

        private void DrawDisabled(PaintEventArgs e)
        {
            using (var brushBg = new SolidBrush(BackColorDisabled))
            {
                e.Graphics.FillRectangle(brushBg, e.ClipRectangle);
            }

            using (var pen = new Pen(BorderColorDisabled, 1f))
            {
                var borderRectangle = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
                e.Graphics.DrawRectangle(pen, borderRectangle);
            }

            var left = RightToLeft == RightToLeft.Yes ? 3 : e.ClipRectangle.Width - ButtonsWidth;
            var height = e.ClipRectangle.Height / 2 - 4;
            var top = 2;
            using (var brush = new SolidBrush(BorderColorDisabled))
            {
                DrawArrowUp(e, brush, left, top, height);
                top = height + 5;
                DrawArrowDown(e, brush, left, top, height);
            }
        }

        private static char[] GetSplitChars()
        {
            var splitChars = new List<char> { ':', ',', '.' };
            var cultureSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
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

    }
}
