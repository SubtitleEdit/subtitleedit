using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Controls
{
    public class NikseListBox : Panel
    {
        private readonly ListBox _listBox;
        private bool _loadingDone = false;

        public NikseListBox()
        {
            _listBox = new ListBox();
            _listBox.BorderStyle = BorderStyle.None;
            _listBox.Padding = new Padding(0);

            base.BorderStyle = BorderStyle.FixedSingle;
            Controls.Clear();
            Controls.Add(_listBox);
            _listBox.Dock = DockStyle.Fill;
            TabStop = false;
            _loadingDone = true;
        }

        public override Color BackColor
        {
            get
            {
                if (_listBox != null)
                {
                    return _listBox.BackColor;
                }

                return DefaultBackColor;
            }
            set
            {
                if (_listBox != null)
                {
                    _listBox.BackColor = value;
                }
            }
        }

        public override Color ForeColor
        {
            get
            {
                if (_listBox != null)
                {
                    return _listBox.ForeColor;
                }

                return DefaultForeColor;
            }
            set
            {
                if (_listBox != null)
                {
                    _listBox.ForeColor = value;
                }
            }
        }

        public override Font Font
        {
            get
            {
                if (!_loadingDone)
                {
                    return null;
                }

                //if (_listBox != null)
                //{
                //    return _listBox.Font;
                //}

                return base.Font;
            }
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
            get
            {
                if (_listBox != null)
                {
                    return _listBox.SelectedItem;
                }

                return null;
            }
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
            get
            {
                if (_listBox != null)
                {
                    return _listBox.Sorted;
                }

                return false;
            }
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
            get
            {
                if (_listBox != null)
                {
                    return _listBox.Text;
                }

                return string.Empty;
            }
            set
            {
                if (_listBox != null)
                {
                    _listBox.Text= value;
                }
            }
        }

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
            }
        }

        private static void NikseListBoxHandleCreated(object sender, EventArgs e) => DarkTheme.SetWindowThemeDark((Control)sender);
    }
}
