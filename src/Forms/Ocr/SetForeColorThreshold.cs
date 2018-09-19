using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic.Ocr;
using Bitmap = System.Drawing.Bitmap;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class SetForeColorThreshold : Form
    {
        private readonly bool _isBinaryImageCompare;
        private readonly NikseBitmap _source;
        public PreprocessingSettings PreprocessingSettings { get; private set; }

        public SetForeColorThreshold(Bitmap bitmap, bool isBinaryImageCompare, PreprocessingSettings preprocessingSettings)
        {
            _isBinaryImageCompare = isBinaryImageCompare;
            InitializeComponent();
            groupBoxBinaryImageCompareThresshold.Visible = isBinaryImageCompare;
            _source = new NikseBitmap(bitmap);
            pictureBoxSubtitleImage.Image = bitmap;
            if (preprocessingSettings != null)
            {
                PreprocessingSettings = preprocessingSettings;
            }
            else
            {
                PreprocessingSettings = new PreprocessingSettings
                {
                    BinaryImageCompareThresshold = Configuration.Settings.Tools.OcrBinaryImageCompareRgbThreshold
                };
            }

            checkBoxInvertColors.Checked = PreprocessingSettings.InvertColors;
            panelColorToWhite.BackColor = PreprocessingSettings.ColorToWhite;
            panelColorToRemove.BackColor = PreprocessingSettings.ColorToRemove;
            checkBoxCropTransparent.Checked = PreprocessingSettings.CropTransparentColors;
            numericUpDownThreshold.Value = PreprocessingSettings.BinaryImageCompareThresshold;

            RefreshImage();
        }

        private void numericUpDownThreshold_ValueChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void RefreshImage()
        {
            PreprocessingSettings.InvertColors = checkBoxInvertColors.Checked;
            PreprocessingSettings.ColorToWhite = panelColorToWhite.BackColor;
            PreprocessingSettings.ColorToRemove = panelColorToRemove.BackColor;
            PreprocessingSettings.CropTransparentColors = checkBoxCropTransparent.Checked;
            PreprocessingSettings.BinaryImageCompareThresshold = (int)numericUpDownThreshold.Value;

            pictureBox1.Image?.Dispose();
            var n = new NikseBitmap(_source);

            if (PreprocessingSettings.CropTransparentColors)
            {
                n.CropSidesAndBottom(2, Color.Transparent, true);
                n.CropSidesAndBottom(2, Color.FromArgb(0, 0, 0, 0), true);
                n.CropTop(2, Color.Transparent);
                n.CropTop(2, Color.FromArgb(0, 0, 0, 0));
            }
            if (PreprocessingSettings.InvertColors)
            {
                n.InvertColors();
            }
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
                RefreshImage();
            }
        }

        private void buttonColorToRemove_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColorToRemove.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColorToRemove.BackColor = colorDialog1.Color;
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

        private void checkBoxInvertColors_CheckedChanged(object sender, EventArgs e)
        {
            buttonColorToWhite.Enabled = !checkBoxInvertColors.Checked;
            panelColorToWhite.Enabled = !checkBoxInvertColors.Checked;

            buttonColorToRemove.Enabled = !checkBoxInvertColors.Checked;
            panelColorToRemove.Enabled = !checkBoxInvertColors.Checked;

            RefreshImage();
        }

        private void checkBoxCropTransparent_CheckedChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }
    }
}
