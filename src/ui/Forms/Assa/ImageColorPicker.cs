using Nikse.SubtitleEdit.Forms.Ocr;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class ImageColorPicker : Form
    {
        private readonly Bitmap _bitmap;
        private bool _colorPickerOn = true;
        private int _colorPickerX = -1;
        private int _colorPickerY = -1;

        public Color Color { get; set; }

        public ImageColorPicker(Bitmap bitmap)
        {
            InitializeComponent();

            _bitmap = bitmap;
            var screen = Screen.PrimaryScreen.WorkingArea.Size;
            while (_bitmap.Width + 10 >= screen.Width || _bitmap.Height + 40 >= screen.Height)
            {
                _bitmap = OcrPreprocessingT4.ResizeBitmap(_bitmap,
                    (int)Math.Round(_bitmap.Width * 0.75, MidpointRounding.AwayFromZero),
                    (int)Math.Round(_bitmap.Height * 0.75, MidpointRounding.AwayFromZero));
            }

            pictureBoxImage.Image = _bitmap;
            labelInfo.Text = string.Empty;

            Width = _bitmap.Width + 10;
            Height = _bitmap.Height + (Height - pictureBoxImage.Height);

            Text = LanguageSettings.Current.ImageColorPicker.Title;
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
                if (x < _bitmap.Width && y < _bitmap.Height)
                {
                    Color = _bitmap.GetPixel(x, y);
                    panelMouseOverColor.BackColor = Color;
                    labelInfo.Text = $"R={Color.R} G={Color.G} B={Color.B}";
                }

                _colorPickerX = x;
                _colorPickerY = y;
            }
        }

        private void pictureBoxImage_Click(object sender, EventArgs e)
        {
            _colorPickerOn = false;
            Cursor.Current = Cursors.Default;
            DialogResult = DialogResult.OK;
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
