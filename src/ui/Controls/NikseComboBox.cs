using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    [Category("NikseComboBox"), Description("ComboBox with better support for color theme")]
    public class NikseComboBox : Control
    {
        // ReSharper disable once InconsistentNaming
        public event EventHandler SelectedIndexChanged;

        // ReSharper disable once InconsistentNaming
        public event EventHandler DropDown;

        // ReSharper disable once InconsistentNaming
        public event EventHandler DropDownClosed;

        // ReSharper disable once InconsistentNaming
        public new event KeyEventHandler KeyDown;

        [Category("NikseComboBox"), Description("Gets or sets DropDownStyle"), RefreshProperties(RefreshProperties.Repaint)]
        public ComboBoxStyle DropDownStyle
        {
            get => _dropDownStyle;
            set
            {
                _dropDownStyle = value;

                if (_textBox == null)
                {
                    return;
                }

                _textBox.ReadOnly = value == ComboBoxStyle.DropDownList;
                //TODO: Hide cursor?
            }
        }

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
                    _items.SortBy(p => p.ToString());
                }
            }
        }

        private readonly NikseComboBoxCollection _items;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [MergableProperty(false)]
        public NikseComboBoxCollection Items => _items;

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value == _selectedIndex)
                {
                    return;
                }

                if (value == -1)
                {
                    _selectedIndex = value;
                    _textBox.Text = string.Empty;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                    return;
                }

                _selectedIndex = value;
                _textBox.Text = Items[_selectedIndex].ToString();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        public object SelectedItem
        {
            get
            {
                if (_selectedIndex < 0)
                {
                    return null;
                }

                return _items[_selectedIndex];
            }
            set
            {
                var idx = _items.IndexOf(value);
                if (idx < 0)
                {
                    return;
                }

                SelectedIndex = idx;
            }
        }

        public override string Text
        {
            get => SelectedText;
            set => SelectedText = value;
        }

        public string SelectedText
        {
            get
            {
                if (_textBox == null)
                {
                    return string.Empty;
                }

                if (DropDownStyle == ComboBoxStyle.DropDown)
                {
                    return _textBox.Text;
                }

                if (_selectedIndex < 0)
                {
                    return string.Empty;
                }

                return _items[_selectedIndex].ToString();
            }
            set
            {
                if (_textBox == null)
                {
                    return;
                }

                if (DropDownStyle == ComboBoxStyle.DropDown)
                {
                    if (_textBox.Text != value)
                    {
                        _textBox.Text = value;
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

                _textBox.Text = value;
                _selectedIndex = idx;

                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Control DropDownControl => _listView;

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
                if (_textBox != null)
                {
                    _textBox.ForeColor = value;
                }

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
        [Category("NikseComboBox"), Description("Gets or sets the disabled background color"),
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

        [Category("NikseComboBox"), Description("Gets or sets the background color"), RefreshProperties(RefreshProperties.Repaint)]
        public new Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (value.A == 0)
                {
                    return;
                }

                base.BackColor = value;
                if (_textBox != null)
                {
                    _textBox.BackColor = value;
                }

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
            _textBox.Visible = false;
            _items = new NikseComboBoxCollection(this);

            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);

            base.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (_selectedIndex > 0)
                    {
                        SelectedIndex--;
                    }
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (_selectedIndex < Items.Count - 2)
                    {
                        SelectedIndex++;
                    }
                    e.SuppressKeyPress = true;
                }
                else
                {
                    KeyDown?.Invoke(this, e);
                }
            };

            _textBox.KeyDown += (sender, e) => { KeyDown?.Invoke(this, e); };

            MouseWheel += (sender, e) =>
            {
                if (_listViewShown)
                {
                    return;
                }

                if (e.Delta > 0)
                {
                    if (_selectedIndex > 0)
                    {
                        SelectedIndex--;
                    }
                }
                else if (e.Delta < 0)
                {
                    if (_selectedIndex < Items.Count - 2)
                    {
                        SelectedIndex++;
                    }
                }
            };

            _textBox.KeyDown += (sender, e) =>
            {
                if (DropDownStyle != ComboBoxStyle.DropDown)
                {

                    if (e.KeyCode == Keys.Up)
                    {
                        if (_selectedIndex > 0)
                        {
                            _selectedIndex--;
                            _textBox.Text = Items[_selectedIndex].ToString();
                            Invalidate();
                            SelectedIndexChanged?.Invoke(sender, e);
                        }
                        e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Down)
                    {
                        if (_selectedIndex < Items.Count - 2)
                        {
                            _selectedIndex++;
                            _textBox.Text = Items[_selectedIndex].ToString();
                            Invalidate();
                            SelectedIndexChanged?.Invoke(sender, e);
                        }
                        e.Handled = true;
                    }
                }

            };
            _textBox.LostFocus += (sender, args) => Invalidate();
            _textBox.GotFocus += (sender, args) => Invalidate();
            _textBox.TextChanged += _textBox_TextChanged;

            Controls.Add(_textBox);
            DropDownStyle = ComboBoxStyle.DropDown;
            BackColor = _textBox.BackColor;
            ButtonForeColor = DefaultForeColor;
            ButtonForeColorOver = Color.FromArgb(0, 120, 215);
            ButtonForeColorDown = Color.Orange;
            BorderColor = Color.FromArgb(171, 173, 179);
            BorderColorDisabled = Color.FromArgb(120, 120, 120);
            BackColorDisabled = Color.FromArgb(240, 240, 240);
            BackColor = SystemColors.Window;

            _mouseLeaveTimer = new Timer();
            _mouseLeaveTimer.Interval = 200;
            _mouseLeaveTimer.Tick += (sender, args) =>
            {
                if (!_hasItemsMouseOver && _listView != null)
                {
                    HideDropDown();
                }

                _mouseLeaveTimer.Stop();
            };

            _listViewMouseLeaveTimer = new Timer();
            _listViewMouseLeaveTimer.Interval = 50;
            _listViewMouseLeaveTimer.Tick += (sender, args) =>
            {
                var form = FindForm();
                if (form == null)
                {
                    return;
                }

                var coordinates = form.PointToClient(Cursor.Position);
                var listViewBounds = new Rectangle(
                    _listView.Bounds.X,
                    _listView.Bounds.Y,
                    _listView.Bounds.Width + 25,
                    _listView.Bounds.Height + 25);
                if (_hasItemsMouseOver &&
                    !(listViewBounds.Contains(coordinates) || Bounds.Contains(coordinates)) ||
                    !_listViewShown)
                {
                    HideDropDown();
                    return;
                }

                _hasItemsMouseOver = true;
            };

            LostFocus += (sender, args) =>
            {
                Invalidate();
            };
        }

        private void HideDropDown()
        {
            _listViewMouseLeaveTimer?.Stop();
            _mouseLeaveTimer?.Stop();
            if (_listViewShown)
            {
                DropDownClosed?.Invoke(this, EventArgs.Empty);
                _listViewShown = false;
            }

            var form = FindForm();
            if (form != null)
            {
                form.Controls.Remove(_listView);
                form.Invalidate();
            }

            Invalidate();

            if (_textBox.Visible)
            {
                _textBox.Focus();
                _textBox.SelectionLength = 0;
            }
            else
            {
                Focus();
            }
        }

        private void _textBox_TextChanged(object sender, EventArgs e)
        {
            Invalidate();
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

        private int? _dropDownWidth;
        private ComboBoxStyle _dropDownStyle;

        public int DropDownWidth
        {
            get => _dropDownWidth ?? Width;
            set => _dropDownWidth = value;
        }


        /// <summary>
        /// Max drop down height
        /// </summary>
        public int DropDownHeight { get; set; } = 400;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();

            if (e.Button == MouseButtons.Left)
            {
                if (_buttonLeftIsDown == false)
                {
                    _buttonLeftIsDown = true;
                    Invalidate();
                }

                if (_listViewShown)
                {
                    HideDropDown();
                    return;
                }

                if (_buttonDownActive || _dropDownStyle != ComboBoxStyle.DropDown)
                {
                    ShowListView();
                }

            }
            base.OnMouseDown(e);
        }

        private void ShowListView()
        {
            _textBox.Focus();

            _listViewShown = true;
            EnsureListViewInitialized();

            _listView.BeginUpdate();
            _listView.Items.Clear();
            var listViewItems = new List<ListViewItem>();
            foreach (var item in Items)
            {
                listViewItems.Add(new ListViewItem(item.ToString()));
            }

            _listView.Items.AddRange(listViewItems.ToArray());
            _listView.Width = DropDownWidth;
            _listView.EndUpdate();

            var lvHeight = 18;
            var form = FindForm();
            if (form == null)
            {
                return;
            }

            var ctl = (Control)this;
            var totalX = ctl.Left;
            var totalY = ctl.Top;
            while (ctl.Parent != form)
            {
                ctl = ctl.Parent;
                totalX += ctl.Left;
                totalY += ctl.Top;
            }
            var top = totalY + Height;

            if (listViewItems.Count > 0)
            {
                var itemHeight = _listView.GetItemRect(0).Height;
                lvHeight = itemHeight * listViewItems.Count + 6;
                var spaceInPixelsBottom = form.Height - (totalY + Height);
                var maxHeight = DropDownHeight;
                if (spaceInPixelsBottom >= DropDownHeight ||
                    spaceInPixelsBottom * 1.2 > totalY)
                {
                    top = totalY + Height;
                    maxHeight = Math.Min(maxHeight, form.Height - Bottom - 18 - SystemInformation.CaptionHeight);
                    lvHeight = Math.Min(lvHeight, maxHeight);
                }
                else
                {
                    maxHeight = Math.Min(maxHeight, totalY - 18 - SystemInformation.CaptionHeight);
                    lvHeight = Math.Min(lvHeight, maxHeight);
                    top = totalY - lvHeight;
                }
            }

            _listView.Height = lvHeight;

            _listView.Left = totalX;
            _listView.Top = top;
            form.Controls.Add(_listView);
            _listView.BringToFront();

            if (_selectedIndex >= 0)
            {
                _listView.Focus();
                _listView.Items[_selectedIndex].Selected = true;
                _listView.EnsureVisible(_selectedIndex);
                _listView.Items[_selectedIndex].Focused = true;
            }

            DropDown?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        private void EnsureListViewInitialized()
        {
            if (_listView != null)
            {
                return;
            }

            _listView = new ListView();
            _listView.View = View.Details;
            var widthNoScrollBar = DropDownWidth - SystemInformation.VerticalScrollBarWidth - SystemInformation.BorderSize.Width * 4;
            _listView.Columns.Add("text", widthNoScrollBar);
            _listView.HeaderStyle = ColumnHeaderStyle.None;
            _listView.FullRowSelect = true;
            _listView.MultiSelect = false;
            _listView.HideSelection = false;
            _listView.GridLines = false;

            if (Configuration.Settings.General.UseDarkTheme)
            {
                DarkTheme.SetDarkTheme(_listView);
            }

            _listView.MouseEnter += (sender, args) => { _hasItemsMouseOver = true; };

            _listView.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.Escape)
                {
                    HideDropDown();
                    args.SuppressKeyPress = true;
                }
                else if (args.KeyCode == Keys.Enter)
                {
                    _listViewMouseLeaveTimer.Stop();
                    var item = _listView.SelectedItems[0];
                    _selectedIndex = item.Index;
                    _textBox.Text = item.Text;
                    Invalidate();
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    HideDropDown();
                    args.SuppressKeyPress = true;
                }
            };

            _listView.MouseClick += (sender, mouseArgs) =>
            {
                if (mouseArgs == null)
                {
                    return;
                }

                var cachedCount = _listView.Items.Count;
                for (var i = 0; i < cachedCount; i++)
                {
                    var rectangle = _listView.GetItemRect(i);
                    if (rectangle.Contains(mouseArgs.Location))
                    {
                        _listViewMouseLeaveTimer.Stop();
                        _selectedIndex = i;
                        _textBox.Text = _listView.Items[i].Text;
                        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                        HideDropDown();
                        _textBox.Focus();
                        _textBox.SelectionLength = 0;
                        return;
                    }
                }
            };

            _listView.LostFocus += (sender, e) =>
            {
                if (_listViewShown && !Focused)
                {
                    HideDropDown();
                }
            };
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
            else if (_buttonDownActive)
            {
                _buttonDownActive = false;
                Invalidate();
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
            if (_skipPaint)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            _textBox.BackColor = BackColor;
            _textBox.BorderStyle = BorderStyle.None;
            _textBox.Top = 2;
            _textBox.Left = RightToLeft == RightToLeft.Yes ? ButtonsWidth : 3;
            _textBox.Height = Height - 4;
            _textBox.Width = Width - ButtonsWidth - 3;

            if (!Enabled)
            {
                DrawDisabled(e);
                return;
            }

            e.Graphics.Clear(BackColor);
            using (var pen = Focused || _textBox.Focused || (_listView != null && _listView.Focused) ? new Pen(_buttonForeColorOver, 1f) : new Pen(BorderColor, 1f))
            {
                var borderRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(pen, borderRectangle);
            }

            if (DropDownStyle == ComboBoxStyle.DropDown)
            {
                if (!_textBox.Visible)
                {
                    _textBox.Visible = true;
                }
                _textBox.Invalidate();
            }
            else
            {
                if (_textBox.Visible)
                {
                    _textBox.Visible = false;
                }

                using (var textBrush = new SolidBrush(ButtonForeColor))
                {
                    e.Graphics.DrawString(_textBox.Text, _textBox.Font, textBrush, _textBox.Bounds);
                }
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
            var height = Height / 2 - 4;
            var top = height / 2 + 5;
            DrawArrow(e, brush, left, top, height);
        }

        public override RightToLeft RightToLeft
        {
            get => base.RightToLeft;
            set
            {
                base.RightToLeft = value;
                Invalidate();
            }
        }

        public bool DroppedDown => _listViewShown;

        public bool FormattingEnabled { get; set; }

        private void DrawArrow(PaintEventArgs e, Brush brush, int left, int top, int height)
        {
            if (_listViewShown)
            {
                NikseUpDown.DrawArrowUp(e, brush, left, top - 1, height);
            }
            else
            {
                NikseUpDown.DrawArrowDown(e, brush, left, top, height);
            }
        }

        private void DrawDisabled(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColorDisabled);

            if (!_textBox.Visible)
            {
                _textBox.Visible = true;
            }

            using (var pen = new Pen(BorderColorDisabled, 1f))
            {
                var borderRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(pen, borderRectangle);
            }

            _textBox.Invalidate();

            var left = RightToLeft == RightToLeft.Yes ? 3 : Width - ButtonsWidth;
            var height = Height / 2 - 4;
            var top = (height / 2) + 5;
            using (var brush = new SolidBrush(BorderColorDisabled))
            {
                DrawArrow(e, brush, left, top, height);
            }
        }

        private bool _skipPaint;

        public void BeginUpdate()
        {
            _skipPaint = true;
        }

        public void EndUpdate()
        {
            _skipPaint = false;
            Invalidate();
        }
    }
}
