using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    [Category("NikseUpDown"), Description("Numeric Up/Down with better support for color theme")]
    public sealed class NikseUpDown : Control
    {
        // ReSharper disable once InconsistentNaming
        public event EventHandler ValueChanged;

        private decimal _value;

        [Category("NikseUpDown"), Description("Gets or sets the default value in textBox"), RefreshProperties(RefreshProperties.Repaint)]
        public decimal Value
        {
            get => _value;
            set
            {
                if (DecimalPlaces == 0)
                {
                    _value = value;
                }
                else if (DecimalPlaces > 0)
                {
                    _value = Math.Round(value, DecimalPlaces);
                }

                Invalidate();
            }
        }

        private int _decimalPlaces;
        [Category("NikseUpDown"), Description("Gets or sets the decimal places (max 4)")]
        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set
            {
                if (value <= 0)
                {
                    _decimalPlaces = 0;
                }
                else if (value > 3)
                {
                    _decimalPlaces = 4;
                }
                else
                {
                    _decimalPlaces = value;
                }

                Invalidate();
            }
        }

        private bool _thousandsSeparator;

        [Category("NikseUpDown"), Description("Gets or sets the thousand seperator")]
        public bool ThousandsSeparator
        {
            get => _thousandsSeparator;
            set
            {
                _thousandsSeparator = value;
                Invalidate();
            }
        }

        [Category("NikseUpDown"), Description("Gets or sets the increment value"), DefaultValue(1)]
        public decimal Increment { get; set; } = 1;

        [Category("NikseUpDown"), Description("Gets or sets the Maximum value (max 25 significant digits)"), DefaultValue(100)]
        public decimal Maximum { get; set; } = 100;

        [Category("NikseUpDown"), Description("Gets or sets the Minimum value"), DefaultValue(0)]
        public decimal Minimum { get; set; } = 0;

        [Category("NikseUpDown"), Description("Allow arrow keys to set increment/decrement value")]
        [DefaultValue(true)]
        public bool InterceptArrowKeys { get; set; }

        private Color _buttonForeColor;
        private Brush _buttonForeColorBrush;
        [Category("NikseUpDown"), Description("Gets or sets the button foreground color"),
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
        [Category("NikseUpDown"), Description("Gets or sets the button foreground mouse over color"), RefreshProperties(RefreshProperties.Repaint)]
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
        [Category("NikseUpDown"), Description("Gets or sets the button foreground mouse down color"), RefreshProperties(RefreshProperties.Repaint)]
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
        [Category("NikseUpDown"), Description("Gets or sets the border color"), RefreshProperties(RefreshProperties.Repaint)]
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
        [Category("NikseUpDown"), Description("Gets or sets the button foreground color"),
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
        [Category("NikseUpDown"), Description("Gets or sets the disabled border color"), RefreshProperties(RefreshProperties.Repaint)]
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

        private readonly TextBox _textBox;

        public NikseUpDown()
        {
            Height = 23;
            _textBox = new TextBox();
            _textBox.KeyPress += TextBox_KeyPress;
            _textBox.KeyDown += (sender, e) =>
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
            };
            _textBox.LostFocus += (sender, args) =>
            {
                SetText(true);
                Invalidate();
            };
            _textBox.GotFocus += (sender, args) => Invalidate();
            _textBox.TextChanged += _textBox_TextChanged;
            _textBox.BorderStyle = BorderStyle.None;

            Controls.Add(_textBox);
            BackColor = SystemColors.Window;
            ButtonForeColor = DefaultForeColor;
            ButtonForeColorOver = Color.FromArgb(0, 120, 215);
            ButtonForeColorDown = Color.Orange;
            BorderColor = Color.FromArgb(171, 173, 179);
            BorderColorDisabled = Color.FromArgb(120, 120, 120);
            BackColorDisabled = Color.FromArgb(240, 240, 240);
            DoubleBuffered = true;
            InterceptArrowKeys = true;

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
            TabStop = false;
        }

        private void _textBox_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse("0" + _textBox.Text, out var result))
            {
                var v = Math.Round(result, DecimalPlaces);
                if (v == Value)
                {
                    return;
                }

                Value = v;

                if (Value < Minimum)
                {
                    Value = Minimum;
                    Invalidate();
                }
                else if (Value > Maximum)
                {
                    Value = Maximum;
                    Invalidate();
                }

                ValueChanged?.Invoke(this, null);
            }
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
            }
            else if (e.KeyChar == '.' || e.KeyChar == ',')
            {
                e.Handled = !(DecimalPlaces > 0);
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
            if (string.IsNullOrEmpty(_textBox.Text))
            {
                Value = 0 >= Minimum && 0 <= Maximum ? 0 : Minimum;
                SetText(true);
                ValueChanged?.Invoke(this, null);
                return;
            }

            if (_textBox.TextLength > 25)
            {
                Value = Maximum;
                SetText(true);
                ValueChanged?.Invoke(this, null);
                return;
            }

            if (decimal.TryParse("0" + _textBox.Text, out var result))
            {
                Value = Math.Round(result + value, DecimalPlaces);

                if (Value < Minimum)
                {
                    Value = Minimum;
                }
                else if (Value > Maximum)
                {
                    Value = Maximum;
                }
                else
                {
                    SetText();
                    ValueChanged?.Invoke(this, null);
                    return;
                }
            }

            SetText();
            ValueChanged?.Invoke(this, null);
        }

        private bool _buttonUpActive;
        private bool _buttonDownActive;

        private bool _buttonLeftIsDown;

        private int _mouseX;
        private int _mouseY;

        private readonly Timer _repeatTimer;
        private bool _repeatTimerArrowUp;
        private int _repeatCount;

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

        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            _textBox.BackColor = BackColor;
            _textBox.ForeColor = ButtonForeColor;
            _textBox.Top = 2;
            _textBox.Left = RightToLeft == RightToLeft.Yes ? ButtonsWidth : 3;
            _textBox.Height = Height - 4;
            _textBox.Width = Width - ButtonsWidth - 3;
            _textBox.Invalidate();
            SetText();

            if (!Enabled)
            {
                DrawDisabled(e);
                return;
            }

            base.OnPaint(e);
            using (var pen = _textBox.Focused ? new Pen(_buttonForeColorOver, 1f) : new Pen(BorderColor, 1f))
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

        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
                _textBox.ForeColor = value;
                Application.DoEvents();
                Invalidate();
            }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                _textBox.BackColor = value;
                Application.DoEvents();
                Invalidate();
            }
        }

        private void SetText(bool leaving = false)
        {
            var selectionStart = _textBox.SelectionStart;

            string newText;
            if (DecimalPlaces <= 0)
            {
                newText = ThousandsSeparator ? $"{Value:#,###,##0}" : $"{Value:########0}";
            }
            else if (DecimalPlaces == 1)
            {
                newText = ThousandsSeparator ? $"{Value:#,###,##0.0}" : $"{Value:########0.0}";
            }
            else if (DecimalPlaces == 2)
            {
                newText = ThousandsSeparator ? $"{Value:#,###,##0.00}" : $"{Value:#########0.00}";
            }
            else if (DecimalPlaces == 3)
            {
                newText = ThousandsSeparator ? $"{Value:#,###,##0.000}" : $"{Value:#########0.000}";
            }
            else
            {
                newText = ThousandsSeparator ? $"{Value:#,###,##0.0000}" : $"{Value:#########0.0000}";
            }

            if (newText == _textBox.Text)
            {
                return;
            }

            if (!leaving &&
                (_textBox.Text.StartsWith(",") || _textBox.Text.StartsWith(".")) &&
                (newText.StartsWith("0,") || newText.StartsWith("0.")))
            {
                return;
            }

            _textBox.Text = newText;
            _textBox.SelectionStart = selectionStart;
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
    }
}
