using Nikse.SubtitleEdit.Logic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public sealed partial class NikseComboBoxPopUp : Form
    {
        private readonly ListView _listView;
        private bool _hasMouseOver;

        public bool DoClose { get; set; }

        public NikseComboBoxPopUp(ListView listView, int selectedIndex, int x, int y)
        {
            InitializeComponent();

            _listView = listView;
            Controls.Add(listView);
            BackColor = UiUtil.BackColor;

            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            Location = new Point(x, y);
            Width = _listView.Width + 2;
            Height = _listView.Height + 2;
            listView.Dock = DockStyle.Fill;

            _listView.BringToFront();

            if (selectedIndex >= 0)
            {
                _listView.Focus();
                _listView.Items[selectedIndex].Selected = true;
                _listView.EnsureVisible(selectedIndex);
                _listView.Items[selectedIndex].Focused = true;
            }

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
                    DialogResult = DialogResult.Cancel;
                    Controls.Remove(_listView);
                }

                if (MouseButtons == MouseButtons.Left || MouseButtons == MouseButtons.Right)
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
                    DialogResult = DialogResult.Cancel;
                    Controls.Remove(_listView);
                }
            };
            timer.Start();
        }

        public bool BoundsContainsCursorPosition()
        {
            var coordinates = Cursor.Position;
            return new Rectangle(Bounds.Left - 25, Bounds.Top - 25, Bounds.Width + 50, Bounds.Height + 50).Contains(coordinates);
        }

        private void NikseComboBoxPopUp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                e.Handled = true;
                Controls.Remove(_listView);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
                e.Handled = true;
                Controls.Remove(_listView);
            }
        }
    }
}
