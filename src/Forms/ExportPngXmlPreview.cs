using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportPngXmlPreview : Form
    {
        private double _zoomFactor = 100;
        private readonly Bitmap _bmp;

        public ExportPngXmlPreview(Bitmap bmp)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            DoubleBuffered = true;

            _bmp = bmp;
            pictureBox1.Image = bmp;
            pictureBox1.Width = bmp.Width;
            pictureBox1.Height = bmp.Height;
            MaximumSize = new Size(bmp.Width + (Width - ClientSize.Width), bmp.Height + (Height - ClientSize.Height));
            var screen = Screen.FromPoint(Cursor.Position);
            if (screen.Bounds.Width > bmp.Width &&
                screen.Bounds.Height > bmp.Height)
            {
                ClientSize = new Size(bmp.Width, bmp.Height);
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }

            pictureBox2.Width = 1;
            pictureBox2.Height = 1;
            pictureBox2.Top = bmp.Height - 2;

            Text = $"{Configuration.Settings.Language.General.Preview} {bmp.Width}x{bmp.Height}";

            MouseWheel += MouseWheelHandler;
        }

        private void MouseWheelHandler(object sender, MouseEventArgs e)
        {
            Zoom(e.Delta / 50.0);
        }

        private void Zoom(double delta)
        {
            double newZoomFactor = _zoomFactor += delta;
            if (newZoomFactor < 25)
            {
                _zoomFactor = 25;
            }
            else if (newZoomFactor > 500)
            {
                _zoomFactor = 500;
            }
            else
            {
                _zoomFactor = newZoomFactor;
            }

            if (_zoomFactor > 99 && _zoomFactor < 101)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                _zoomFactor = 100.0;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }

            pictureBox1.Width = (int)(_bmp.Width * _zoomFactor / 100.0);
            pictureBox1.Height = (int)(_bmp.Height * _zoomFactor / 100.0);

            Text = string.Format("{0}  {1}x{2}  {3}%", Configuration.Settings.Language.General.Preview, _bmp.Width, _bmp.Height, (int)_zoomFactor);

            Invalidate();
        }

        private void ExportPngXmlPreview_Shown(object sender, System.EventArgs e)
        {
            panel1.ScrollControlIntoView(pictureBox2);
            pictureBox2.Visible = false;
        }

        private void ExportPngXmlPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.OK;
            }
            else if (e.KeyCode == Keys.Add)
            {
                Zoom(10);
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                Zoom(-10);
            }
            else if (e.KeyCode == Keys.Home)
            {
                _zoomFactor = 100;
                Zoom(0);
            }
        }

    }
}
