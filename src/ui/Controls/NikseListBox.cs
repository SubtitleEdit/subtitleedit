using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public class NikseListBox : Panel
    {
        private readonly ListBox _listBox;
        private readonly bool _loadingDone;

        public NikseListBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            _listBox = new ListBox();
            _listBox.BorderStyle = BorderStyle.None;
            _listBox.Padding = new Padding(0);

            BorderStyle = BorderStyle.FixedSingle;
            Controls.Clear();
            Controls.Add(_listBox);
            _listBox.Dock = DockStyle.Fill;
            TabStop = false;
            _loadingDone = true;
        }

        public new Color BackColor
        {
            get
            {
                if (!_loadingDone)
                {
                    return DefaultBackColor;
                }

                if (_listBox != null)
                {
                    return _listBox.BackColor;
                }

                return DefaultBackColor;
            }
            set
            {
                if (!_loadingDone)
                {
                    return;
                }

                if (_listBox != null)
                {
                    _listBox.BackColor = value;
                }
            }
        }

        public new Color ForeColor
        {
            get
            {
                if (!_loadingDone)
                {
                    return DefaultForeColor;
                }

                if (_listBox != null)
                {
                    return _listBox.ForeColor;
                }

                return DefaultForeColor;
            }
            set
            {
                if (!_loadingDone)
                {
                    return;
                }

                if (_listBox != null)
                {
                    _listBox.ForeColor = value;
                }
            }
        }

        public new Font Font
        {
            get => !_loadingDone ? DefaultFont : base.Font;
            set
            {
                if (!_loadingDone)
                {
                    return;
                }

                if (_listBox != null)
                {
                    _listBox.Font = value;
                }

                base.Font = value;
            }
        }

        public  ListBox.ObjectCollection Items
        {
            get
            {
                if (_listBox != null)
                {
                    return _listBox.Items;
                }

                return new ListBox.ObjectCollection(new ListBox());
            }
        }

        public override RightToLeft RightToLeft
        {
            get
            {
                if (!_loadingDone)
                {
                    return RightToLeft.Inherit;
                }

                //if (_listBox != null)
                //{
                //    return _listBox.RightToLeft;
                //}

                return base.RightToLeft;
            }
            set
            {
                if (_listBox != null)
                {
                    _listBox.RightToLeft = value;
                }

                base.RightToLeft = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                if (_listBox != null)
                {
                    return _listBox.SelectedIndex;
                }

                return -1;
            }
            set
            {
                if (_listBox != null)
                {
                    _listBox.SelectedIndex = value;
                }
            }
        }

        public int TopIndex
        {
            get
            {
                if (_listBox != null)
                {
                    return _listBox.TopIndex;
                }

                return 0;
            }
            set
            {
                if (_listBox != null)
                {
                    _listBox.TopIndex = value;
                }
            }
        }

        public object SelectedItem
        {
            get => _listBox?.SelectedItem;
            set
            {
                if (_listBox != null)
                {
                    _listBox.SelectedItem = value;
                }
            }
        }

        public bool Sorted
        {
            get => _listBox != null && _listBox.Sorted;
            set
            {
                if (_listBox != null)
                {
                    _listBox.Sorted = value;
                }
            }
        }

        public override string Text
        {
            get => _listBox != null ? _listBox.Text : string.Empty;
            set
            {
                if (_listBox != null)
                {
                    _listBox.Text= value;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public new event EventHandler TextChanged
        {
            add
            {
                if (_listBox != null)
                {
                    _listBox.TextChanged += value;
                }
            }
            remove
            {
                if (_listBox != null)
                {
                    _listBox.TextChanged -= value;
                }
            }
        }

        public ListBox.SelectedIndexCollection SelectedIndices => _listBox.SelectedIndices;

        public bool FormattingEnabled
        {
            get => _listBox != null && _listBox.FormattingEnabled;
            set
            {
                if (_listBox != null)
                {
                    _listBox.FormattingEnabled = value;
                }
            }
        }

        public SelectionMode SelectionMode
        {
            get
            {
                if (_listBox != null)
                {
                    return _listBox.SelectionMode;
                }

                return SelectionMode.None;
            }
            set
            {
                if (_listBox != null)
                {
                    _listBox.SelectionMode = value;
                }
            }
        }

        public ListBox.SelectedObjectCollection SelectedItems => _listBox.SelectedItems;

        public int ItemHeight
        {
            get => _listBox.ItemHeight;
            set => _listBox.ItemHeight = value;
        }

        // ReSharper disable once InconsistentNaming
        public new event EventHandler Click
        {
            add
            {
                if (_listBox != null)
                {
                    _listBox.Click += value;
                }
            }
            remove
            {
                if (_listBox != null)
                {
                    _listBox.Click -= value;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public event EventHandler SelectedIndexChanged
        {
            add
            {
                if (_listBox != null)
                {
                    _listBox.SelectedIndexChanged += value;
                }
            }
            remove
            {
                if (_listBox != null)
                {
                    _listBox.SelectedIndexChanged -= value;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public new event MouseEventHandler MouseClick
        {
            add
            {
                if (_listBox != null)
                {
                    _listBox.MouseClick += value;
                }
            }
            remove
            {
                if (_listBox != null)
                {
                    _listBox.MouseClick -= value;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public new event MouseEventHandler MouseDoubleClick
        {
            add
            {
                if (_listBox != null)
                {
                    _listBox.MouseDoubleClick += value;
                }
            }
            remove
            {
                if (_listBox != null)
                {
                    _listBox.MouseDoubleClick -= value;
                }
            }
        }

        public void BeginUpdate()
        {
            _listBox?.BeginUpdate();
        }

        public void EndUpdate()
        {
            _listBox?.EndUpdate();
        }

        public void SetSelected(int index, bool value)
        {
            _listBox?.SetSelected(index, value);
        }

        public void SelectAll()
        {
            _listBox?.SelectAll();
        }

        public void InverseSelection()
        {
            _listBox?.InverseSelection();
        }

        public void SetDarkTheme()
        {
            if (_listBox != null)
            {
                _listBox.BackColor = DarkTheme.BackColor;
                _listBox.ForeColor = DarkTheme.ForeColor;
                _listBox.HandleCreated += NikseListBoxHandleCreated;
                DarkTheme.SetWindowThemeDark(_listBox);
                DarkTheme.SetWindowThemeDark(this);

                _listBox.DrawMode = DrawMode.OwnerDrawFixed;
                _listBox.DrawItem += listBox1_DrawItem;
            }
        }

        public void UndoDarkTheme()
        {
            if (_listBox != null)
            {
                _listBox.BackColor = DefaultBackColor;
                _listBox.ForeColor = DefaultForeColor;
                _listBox.HandleCreated -= NikseListBoxHandleCreated;
                DarkTheme.SetWindowThemeNormal(_listBox);
                DarkTheme.SetWindowThemeNormal(this);

                _listBox.DrawMode = DrawMode.Normal;
                _listBox.DrawItem -= listBox1_DrawItem;
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e = new DrawItemEventArgs(e.Graphics,
                    e.Font,
                    e.Bounds,
                    e.Index,
                    e.State ^ DrawItemState.Selected,
                    e.ForeColor,
                    DarkTheme.DarkThemeSelectedBackgroundColor);
            }

            e.DrawBackground();

            using (var brush = new SolidBrush(DarkTheme.ForeColor))
            {
                e.Graphics.DrawString(_listBox.Items[e.Index].ToString(), e.Font, brush, e.Bounds, StringFormat.GenericDefault);
            }

            // Do not add "e.DrawFocusRectangle();"
        }

        private static void NikseListBoxHandleCreated(object sender, EventArgs e) => DarkTheme.SetWindowThemeDark((Control)sender);
    }
}
