using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Bitmap = System.Drawing.Bitmap;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class SetForeColorThreshold : Form
    {
        private readonly bool _isBinaryImageCompare;
        private readonly NikseBitmap _source;

        public SetForeColorThreshold(Bitmap bitmap, bool isBinaryImageCompare)
        {
            _isBinaryImageCompare = isBinaryImageCompare;
            InitializeComponent();
            groupBoxBinaryImageCompareThresshold.Visible = isBinaryImageCompare;
            _source = new NikseBitmap(bitmap);
            pictureBoxSubtitleImage.Image = bitmap;
            ColorToRemove = Color.Transparent;
            ReplaceColorWithWhite = Color.Transparent;
            numericUpDownThreshold.Value = Configuration.Settings.Tools.OcrBinaryImageCompareRgbThreshold;
            panelColorToWhite.BackColor = Color.Transparent;
            panelColorToRemove.BackColor = Color.Transparent;
        }

        public int Threshold { get; set; }
        public Color ReplaceColorWithWhite { get; set; }
        public Color ColorToRemove { get; set; }

        private void numericUpDownThreshold_ValueChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void RefreshImage()
        {
            pictureBox1.Image?.Dispose();
            var n = new NikseBitmap(_source);
            n.CropTopTransparent(9999);
            if (panelColorToWhite.BackColor != Color.Transparent)
                n.ReplaceColor(panelColorToWhite.BackColor.A, panelColorToWhite.BackColor.R, panelColorToWhite.BackColor.G, panelColorToWhite.BackColor.B, 255, 255, 255, 255);
            if (panelColorToRemove.BackColor != Color.Transparent)
                n.ReplaceColor(panelColorToRemove.BackColor.A, panelColorToRemove.BackColor.R, panelColorToRemove.BackColor.G, panelColorToRemove.BackColor.B, Color.Transparent.A, Color.Transparent.R, Color.Transparent.G, Color.Transparent.B);
            if (_isBinaryImageCompare)
                n.MakeTwoColor((int)numericUpDownThreshold.Value);
            pictureBox1.Image = n.GetBitmap();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Threshold = (int)numericUpDownThreshold.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void pictureBoxSubtitleImage_Click(object sender, EventArgs e)
        {
            var bmp = pictureBoxSubtitleImage.Image as Bitmap;
            if (bmp == null)
                return;
            Text = MousePosition.X + ":" + MousePosition.Y;
        }

        private void ColorToWhite(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColorToWhite.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColorToWhite.BackColor = colorDialog1.Color;
                ReplaceColorWithWhite = colorDialog1.Color;
                RefreshImage();
            }
        }

        private void buttonColorToRemove_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColorToRemove.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColorToRemove.BackColor = colorDialog1.Color;
                ColorToRemove = colorDialog1.Color;
                RefreshImage();
            }
        }

        private void panelColorToRemove_Click(object sender, EventArgs e)
        {
            buttonColorToRemove_Click(sender, e);
        }

        private void SetForeColorThreshold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
