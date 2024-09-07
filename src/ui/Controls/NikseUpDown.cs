using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    [Category("NikseUpDown"), Description("Numeric Up/Down with better support for color theme")]
    public sealed class NikseUpDown : Control
    {
        // ReSharper disable once InconsistentNaming
        public event EventHandler ValueChanged;

        // ReSharper disable once InconsistentNaming
        public new event KeyEventHandler KeyDown;

        private decimal _value;
        private bool _dirty;

        [Category("NikseUpDown"), Description("Gets or sets the default value in textBox"), RefreshProperties(RefreshProperties.Repaint)]
        public decimal Value
        {
            get => _value;
            set
            {
                if (value == _value)
                {
                    return;
                }

                if (DecimalPlaces == 0)
                {
                    _value = value;
                }
                else if (DecimalPlaces > 0)
                {
                    _value = Math.Round(value, DecimalPlaces);
                }

                SetText();
                _dirty = false;
                Invalidate();
                ValueChanged?.Invoke(this, null);
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


        public new Font Font
        {
            get => base.Font;
            set
            {
                if (_textBox != null)
                {
                    _textBox.Font = value;
                }

                base.Font = value;
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

        public static Color DefaultBackColorDisabled = Color.FromArgb(240, 240, 240);

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
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable |
                     ControlStyles.AllPaintingInWmPaint, true);

            InterceptArrowKeys = true;
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
                else if (e.KeyData != (Keys.Tab | Keys.Shift) &&
                         e.KeyData != Keys.Tab &&
                         e.KeyData != Keys.Left &&
                         e.KeyData != Keys.Right)
                {
                    _dirty = true;
                    KeyDown?.Invoke(sender, e);
                    Invalidate();
                }
                else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
                {
                    _textBox.SelectAll();
                    e.SuppressKeyPress = true;
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
                {
                    _textBox.Copy();
                    e.SuppressKeyPress = true;
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V)
                {
                    _textBox.Paste();
                    e.SuppressKeyPress = true;
                }
                else
                {
                    KeyDown?.Invoke(sender, e);
                }
            };
            _textBox.LostFocus += (sender, args) =>
            {
                _dirty = false;
                AddValue(0);
                SetText(true);
                Invalidate();
            };
            _textBox.GotFocus += (sender, args) => Invalidate();
            _textBox.TextChanged += _textBox_TextChanged;
            _textBox.MouseEnter += (sender, args) => { _upDownMouseEntered = true; };
            _textBox.MouseLeave += (sender, args) => { _upDownMouseEntered = false; };
            _textBox.BorderStyle = BorderStyle.None;

            Controls.Add(_textBox);
            BackColor = SystemColors.Window;
            ButtonForeColor = DefaultForeColor;
            ButtonForeColorOver = Color.FromArgb(0, 120, 215);
            ButtonForeColorDown = Color.Orange;
            BorderColor = Color.FromArgb(171, 173, 179);
            BorderColorDisabled = Color.FromArgb(120, 120, 120);
            BackColorDisabled = DefaultBackColorDisabled;

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

            MouseWheel += (sender, e) =>
            {
                if (_textBox == null)
                {
                    return;
                }

                if (e.Delta > 0)
                {
                    AddValue(Increment);
                }
                else if (e.Delta < 0)
                {
                    AddValue(-Increment);
                }
            };

            TabStop = false;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            if (_textBox != null)
            {
                _textBox.Focus();
                return;
            }

            base.OnGotFocus(e);
        }

        private void _textBox_TextChanged(object sender, EventArgs e)
        {
            if (_dirty)
            {
                return;
            }

            var text = _textBox.Text.Trim();
            if (decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.DefaultThreadCurrentCulture, out var result))
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
            _dirty = false;

            if (string.IsNullOrEmpty(_textBox.Text))
            {
                Value = 0 >= Minimum && 0 <= Maximum ? 0 : Minimum;
                SetText(true);
                return;
            }

            if (_textBox.TextLength > 25)
            {
                Value = Maximum;
                SetText(true);
                return;
            }

            var text = _textBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                text = "0";
            }

            if (decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.DefaultThreadCurrentCulture, out var result))
            {
                var newValue = Math.Round(result + value, DecimalPlaces);

                if (newValue < Minimum)
                {
                    Value = Minimum;
                }
                else if (newValue > Maximum)
                {
                    Value = Maximum;
                }
                else
                {
                    Value = newValue;
                    SetText();
                    return;
                }
            }

            SetText();
        }

        private bool _buttonUpActive;
        private bool _buttonDownActive;

        private bool _buttonLeftIsDown;

        private int _mouseX;
        private int _mouseY;

        private readonly Timer _repeatTimer;
        private bool _repeatTimerArrowUp;
        private int _repeatCount;
        private bool _upDownMouseEntered;

        protected override void OnMouseEnter(EventArgs e)
        {
            _buttonUpActive = false;
            _buttonDownActive = false;
            _upDownMouseEntered = true;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _buttonUpActive = false;
            _buttonDownActive = false;
            _upDownMouseEntered = false;
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
            base.OnMouseUp(e);
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
            if (!_dirty)
            {
                SetText();
            }

            if (!Enabled)
            {
                DrawDisabled(e);
                return;
            }

            e.Graphics.Clear(BackColor);
            using (var pen = _textBox.Focused || _upDownMouseEntered ? new Pen(_buttonForeColorOver, 1f) : new Pen(BorderColor, 1f))
            {
                var borderRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(pen, borderRectangle);
            }

            var brush = _buttonForeColorBrush;
            var left = RightToLeft == RightToLeft.Yes ? 3 : Width - ButtonsWidth;
            var height = Height / 2 - 4;
            var top = 2;
            if (_buttonUpActive)
            {
                brush = _buttonLeftIsDown ? _buttonForeColorDownBrush : _buttonForeColorOverBrush;
            }

            DrawArrowUp(e.Graphics, brush, left, top, height);

            if (_buttonDownActive)
            {
                brush = _buttonLeftIsDown ? _buttonForeColorDownBrush : _buttonForeColorOverBrush;
            }
            else
            {
                brush = _buttonForeColorBrush;
            }

            top = height + 5;
            DrawArrowDown(e.Graphics, brush, left, top, height);
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

        public static void DrawArrowUp(Graphics g, Brush brush, int left, int top, int height)
        {
            g.FillPolygon(brush,
                new[]
                {
                    // arrow head
                    new Point(left + 5, top + 2), // top
                    //new Point(left + 6, top + 2), // top right 
                    //new Point(left + 4, top + 2), // top left

                    new Point(left + 1, top + height), // left bottom
                    new Point(left + 9, top + height), // right bottom
                });
        }

        public static void DrawArrowDown(Graphics g, Brush brush, int left, int top, int height)
        {
            g.FillPolygon(brush,
                new[]
                {
                    new Point(left + 1, top), // left top
                    new Point(left + 9, top), // right top

                    // arrow head
                    new Point(left + 5, top + height -2), // bottom
                    //new Point(left + 6, top + height -2), // bottom right  
                    //new Point(left + 4, top + height -2), // bottom left
                });
        }

        private void DrawDisabled(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColorDisabled);


            using (var pen = new Pen(BorderColorDisabled, 1f))
            {
                var borderRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(pen, borderRectangle);
            }

            var left = RightToLeft == RightToLeft.Yes ? 3 : Width - ButtonsWidth;
            var height = Height / 2 - 4;
            var top = 2;
            using (var brush = new SolidBrush(BorderColorDisabled))
            {
                DrawArrowUp(e.Graphics, brush, left, top, height);
                top = height + 5;
                DrawArrowDown(e.Graphics, brush, left, top, height);
            }
        }
    }
}
