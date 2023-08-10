using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public partial class NikseComboBoxPopUp : Form
    {
        private readonly ListView _listView;
        private bool _hasMouseOver = false;
        private long _startTicks;
        private bool _doClose;

        public bool DoClose
        {
            get => _doClose;
            set
            {
                _doClose = value;
            }
        }

        public NikseComboBoxPopUp(ListView listView, int selectedIndex, int x, int y)
        {
            InitializeComponent();

            _listView = listView;
            var selectedIndex1 = selectedIndex;
            Controls.Add(listView);

            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            Location = new Point(x, y);
            Width = _listView.Width + 2;
            Height = _listView.Height + 2;
            listView.Dock = DockStyle.Fill;

            _listView.BringToFront();

            if (selectedIndex1 >= 0)
            {
                _listView.Focus();
                _listView.Items[selectedIndex1].Selected = true;
                _listView.EnsureVisible(selectedIndex1);
                _listView.Items[selectedIndex1].Focused = true;
            }

            _startTicks = DateTime.UtcNow.Ticks;
            KeyPreview = true;
            KeyDown += NikseComboBoxPopUp_KeyDown;

            MouseEnter += (sender, args) =>
            {
                _hasMouseOver = true;
            };

            MouseMove += (sender, args) =>
            {
                _hasMouseOver = true;
            };

            MouseLeave += (sender, args) =>
            {
                _hasMouseOver = false;
            };

            var timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (sender, args) =>
            {
                timer.Interval = 100;
                if (DoClose)
                {
                    timer.Stop();
                    Controls.Remove(_listView);
                    DialogResult = DialogResult.Cancel;
                }

                if (MouseButtons == MouseButtons.Left || Control.MouseButtons == MouseButtons.Right)
                {


                    if (_hasMouseOver)
                    {
                        return;
                    }

                    if (Bounds.Contains(Cursor.Position))
                    {
                        return;
                    }

                    timer.Stop();
                    Controls.Remove(_listView);
                    DialogResult = DialogResult.Cancel;
                }
            };
            timer.Start();
        }

        private void NikseComboBoxPopUp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Controls.Remove(_listView);
                DialogResult = DialogResult.Cancel;
                e.Handled = true;
                return;
            }
        }
    }
}
