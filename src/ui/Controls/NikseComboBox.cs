using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Controls
{
    [Category("NikseComboBox"), Description("ComboBox with better support for color theme")]
    public class NikseComboBox : Control
    {
        // ReSharper disable once InconsistentNaming
        public event EventHandler SelectedIndexChanged;

        [Category("NikseComboBox"), Description("Gets or sets DropDownStyle"),
         RefreshProperties(RefreshProperties.Repaint)]
        public ComboBoxStyle DropDownStyle { get; set; }

        private bool _sorted;
        [Category("NikseComboBox"), Description("Gets or sets if elements are auto sorted"), DefaultValue(false)]
        public bool Sorted
        {
            get => _sorted;
            set
            {
                if (_sorted == value)
                {
                    return;
                }

                _sorted = value;
                if (_sorted && _items != null)
                {
                    _items = _items.OrderBy(p => p.ToString()).ToList();
                    //TODO: fix selected index!
                }
            }
        }

        private List<object> _items;
        public List<object> Items => _items ?? (_items = new List<object>());

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value == _selectedIndex)
                {
                    return;
                }

                _selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private string _text;
        public string SelectedText
        {
            get
            {
                if (DropDownStyle == ComboBoxStyle.DropDown)
                {
                    return _text;
                }

                if (_selectedIndex < 0)
                {
                    return null;
                }

                return _items[_selectedIndex].ToString();
            }
            set
            {
                if (DropDownStyle == ComboBoxStyle.DropDown)
                {
                    if (_text != value)
                    {
                        _text = value;
                        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    }

                    return;
                }

                var hit = _items.FirstOrDefault(p => p.ToString() == value);
                if (hit == null)
                {
                    return;
                }

                var idx = _items.IndexOf(hit);
                if (idx == _selectedIndex)
                {
                    return;
                }

                _text = value;
                _selectedIndex = idx;

                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private Color _buttonForeColor;
        private Brush _buttonForeColorBrush;
        [Category("NikseComboBox"), Description("Gets or sets the button foreground color"),
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
        [Category("NikseComboBox"), Description("Gets or sets the button foreground mouse over color"), RefreshProperties(RefreshProperties.Repaint)]
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
        [Category("NikseComboBox"), Description("Gets or sets the button foreground mouse down color"), RefreshProperties(RefreshProperties.Repaint)]
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
        [Category("NikseComboBox"), Description("Gets or sets the border color"), RefreshProperties(RefreshProperties.Repaint)]
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
        [Category("NikseComboBox"), Description("Gets or sets the button foreground color"),
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
        [Category("NikseComboBox"), Description("Gets or sets the disabled border color"), RefreshProperties(RefreshProperties.Repaint)]
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

        public NikseComboBox()
        {
            _textBox = new TextBox();
            _textBox.KeyPress += TextBox_KeyPress;
            _textBox.KeyDown += (sender, e) =>
            {
                if (_listView != null && _listViewShown)
                {
                    _listView.Focus();
                    if (_selectedIndex < 0 && _listView.SelectedItems.Count > 0)
                    {
                        _listView.Items[0].Selected = true;
                        _listView.Items[0].Focused = true;
                    }
                    else
                    {
                        _listView.Items[_selectedIndex].Selected = true;
                        _listView.Items[_selectedIndex].EnsureVisible();
                        _listView.Items[_selectedIndex].Focused = true;
                    }
                }
            };
            _textBox.LostFocus += (sender, args) => Invalidate();
            _textBox.GotFocus += (sender, args) => Invalidate();
            _textBox.TextChanged += _textBox_TextChanged;

            Controls.Add(_textBox);
            BackColor = new TextBox().BackColor;
            ButtonForeColor = DefaultForeColor;
            ButtonForeColorOver = Color.FromArgb(0, 120, 215);
            ButtonForeColorDown = Color.Orange;
            BorderColor = Color.FromArgb(171, 173, 179);
            BorderColorDisabled = Color.FromArgb(120, 120, 120);
            BackColorDisabled = Color.FromArgb(240, 240, 240);
            DoubleBuffered = true;

            _mouseLeaveTimer = new Timer();
            _mouseLeaveTimer.Interval = 200;
            _mouseLeaveTimer.Tick += (sender, args) =>
            {
                if (!_hasItemsMouseOver && _listView != null)
                {
                    Parent.Controls.Remove(_listView);
                    Parent.Invalidate();
                    _listViewMouseLeaveTimer?.Stop();
                    _listViewShown = false;
                }

                _mouseLeaveTimer.Stop();
            };

            _listViewMouseLeaveTimer = new Timer();
            _listViewMouseLeaveTimer.Interval = 50;
            _listViewMouseLeaveTimer.Tick += (sender, args) =>
            {
                var coordinates = Parent.PointToClient(Cursor.Position);
                if (_hasItemsMouseOver && !_listView.Bounds.Contains(coordinates) || !_listViewShown)
                {
                    _mouseLeaveTimer.Stop();
                    _listViewMouseLeaveTimer.Stop();
                    _listViewShown = false;
                    Parent.Controls.Remove(_listView);
                    Parent.Invalidate();
                    Invalidate();
                    return;
                }

                _hasItemsMouseOver = true;
            };

        }

        private void _textBox_TextChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Allow only digits, Enter and Backspace key.
        /// </summary>
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {

        }


        private bool _buttonDownActive;

        private bool _buttonLeftIsDown;

        private int _mouseX;

        private readonly Timer _mouseLeaveTimer;
        private readonly Timer _listViewMouseLeaveTimer;
        private bool _hasItemsMouseOver;


        protected override void OnMouseEnter(EventArgs e)
        {
            _buttonDownActive = false;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _buttonDownActive = false;
            if (_listView != null)
            {
                _mouseLeaveTimer.Start();
                _listViewMouseLeaveTimer.Start();
                _hasItemsMouseOver = false;
            }

            base.OnMouseLeave(e);
            Invalidate();
        }

        private ListView _listView;
        private bool _listViewShown;
        public int DropDownWidth { get; set; } = 100;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_buttonLeftIsDown == false)
                {
                    _buttonLeftIsDown = true;
                    Invalidate();
                }

                if (_listViewShown)
                {
                    _listViewMouseLeaveTimer.Stop();
                    Parent.Controls.Remove(_listView);
                    _listViewShown = false;
                    Parent.Invalidate();
                    Invalidate();
                    return;
                }

                if (_buttonDownActive)
                {
                    _listViewShown = true;
                    _textBox.Focus();

                    if (_listView == null)
                    {
                        _listView = new ListView();
                        _listView.View = View.Details;
                        _listView.Columns.Add("text", DropDownWidth - 4);
                        _listView.HeaderStyle = ColumnHeaderStyle.None;
                        _listView.FullRowSelect = true;
                        _listView.MultiSelect = false;
                        _listView.HideSelection = false;
                        if (Configuration.Settings.General.UseDarkTheme)
                        {
                            DarkTheme.SetDarkTheme(_listView);
                        }

                        _listView.MouseEnter += (sender, args) =>
                        {
                            _hasItemsMouseOver = true;
                        };
                        _listView.KeyDown += (sender, args) =>
                        {
                            if (args.KeyCode == Keys.Escape)
                            {
                                _listViewMouseLeaveTimer.Stop();
                                Parent.Controls.Remove(_listView);
                                _listViewShown = false;
                                Parent.Invalidate();
                            }
                            else if (args.KeyCode == Keys.Enter)
                            {
                                _listViewMouseLeaveTimer.Stop();
                                var item = _listView.SelectedItems[0];
                                _selectedIndex = item.Index;
                                _text = item.Text;
                                Invalidate();
                                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                                Parent.Controls.Remove(_listView);
                                _listViewShown = false;
                                Parent.Invalidate();
                            }
                        };
                        _listView.MouseClick += (sender, args) =>
                        {
                            if (args is MouseEventArgs mouseArgs)
                            {
                                for (var i = 0; i < _listView.Items.Count; i++)
                                {
                                    var rectangle = _listView.GetItemRect(i);
                                    if (rectangle.Contains(mouseArgs.Location))
                                    {
                                        _listViewMouseLeaveTimer.Stop();
                                        _selectedIndex = i;
                                        _text = _listView.Items[i].Text;
                                        Invalidate();
                                        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                                        Parent.Controls.Remove(_listView);
                                        _listViewShown = false;
                                        Parent.Invalidate();
                                        return;
                                    }
                                }
                            }
                        };
                    }
                    else
                    {
                        _listView.Items.Clear();
                    }

                    var listViewItems = new List<ListViewItem>();
                    foreach (var item in Items)
                    {
                        listViewItems.Add(new ListViewItem(item.ToString()));
                    }
                    _listView.Items.AddRange(listViewItems.ToArray());

                    Parent.Controls.Add(_listView);
                    _listView.BringToFront();
                    _listView.Width = DropDownWidth;
                    _listView.Height = 200;
                    _listView.Left = Left;
                    _listView.Top = Bottom;
                    if (_selectedIndex >= 0)
                    {
                        _listView.Focus();
                        _listView.Items[_selectedIndex].Selected = true;
                        _listView.EnsureVisible(_selectedIndex);
                        _listView.Items[_selectedIndex].Focused = true;
                    }
                }

                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
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

            _mouseX = e.X;

            if (_mouseX >= left && _mouseX <= right)
            {
                _buttonDownActive = true;
                Invalidate();
            }
            else
            {
                if (_buttonDownActive)
                {
                    _buttonDownActive = false;
                    Invalidate();
                }
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
            _textBox.BorderStyle = BorderStyle.None;
            _textBox.Top = 2;
            _textBox.Left = RightToLeft == RightToLeft.Yes ? ButtonsWidth : 3;
            _textBox.Height = Height - 4;
            _textBox.Width = Width - ButtonsWidth - 3;
            SetText();
            _textBox.Invalidate();


            if (!Enabled)
            {
                DrawDisabled(e);
                return;
            }

            base.OnPaint(e);
            using (var pen = (_textBox.Focused || (_listView != null && _listView.Focused)) ? new Pen(_buttonForeColorOver, 1f) : new Pen(BorderColor, 1f))
            {
                var borderRectangle = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
                e.Graphics.DrawRectangle(pen, borderRectangle);
            }

            Brush brush;
            if (_buttonDownActive)
            {
                brush = _buttonLeftIsDown ? _buttonForeColorDownBrush : _buttonForeColorOverBrush;
            }
            else
            {
                brush = _buttonForeColorBrush;
            }

            var left = RightToLeft == RightToLeft.Yes ? 3 : Width - ButtonsWidth;
            var height = e.ClipRectangle.Height / 2 - 4;
            var top = (height / 2) + 5;
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

        private void SetText()
        {
            _textBox.Text = _text;
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
            var top = (height / 2) + 5;
            using (var brush = new SolidBrush(BorderColorDisabled))
            {
                DrawArrowDown(e, brush, left, top, height);
            }
        }
    }
}
