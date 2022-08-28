using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public partial class ImageColorPicker : Form
    {
        private Bitmap _image;
        private bool _colorPickerOn = true;
        private int _colorPickerX = -1;
        private int _colorPickerY = -1;

        public Color Color { get; set;  }

        public ImageColorPicker(Bitmap bitmap)
        {
            InitializeComponent();

            _image = bitmap;
            pictureBoxImage.Image = bitmap;
            labelInfo.Text = string.Empty;

            Width = _image.Width + 10;
            Height = _image.Height + (Height - pictureBoxImage.Height);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void pictureBoxImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_colorPickerOn)
            {
                return;
            }

            Cursor.Current = Cursors.Cross;
            var pos = pictureBoxImage.PointToClient(MousePosition);
            var x = pos.X;
            var y = pos.Y;
            if (x != _colorPickerX || y != _colorPickerY)
            {
                if (x < _image.Width && y < _image.Height)
                {
                    Color = _image.GetPixel(x, y);
                    panelMouseOverColor.BackColor = Color;
                    labelInfo.Text = $"R={Color.R} G={Color.G} B={Color.B}";
                }

                _colorPickerX = x;
                _colorPickerY = y;
            }
        }

        private void pictureBoxImage_Click(object sender, EventArgs e)
        {
            _colorPickerOn = !_colorPickerOn;

            if (!_colorPickerOn)
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ImageColorPicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
