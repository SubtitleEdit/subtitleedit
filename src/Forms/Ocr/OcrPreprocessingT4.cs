using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic.Ocr;
using Bitmap = System.Drawing.Bitmap;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class OcrPreprocessingT4 : Form
    {
        private readonly bool _isBinaryImageCompare;
        private readonly NikseBitmap _source;
        public PreprocessingSettings PreprocessingSettings { get; }

        public OcrPreprocessingT4(Bitmap bitmap, bool isBinaryImageCompare, PreprocessingSettings preprocessingSettings)
        {
            _isBinaryImageCompare = isBinaryImageCompare;
            InitializeComponent();
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

            numericUpDownThreshold.Value = PreprocessingSettings.BinaryImageCompareThresshold;

            RefreshImage();
        }

        private void numericUpDownThreshold_ValueChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void RefreshImage()
        {
            PreprocessingSettings.BinaryImageCompareThresshold = (int)numericUpDownThreshold.Value;

            pictureBox1.Image?.Dispose();
            var n = new NikseBitmap(_source);
            n.MakeTwoColor((int)numericUpDownThreshold.Value, Color.White, Color.Black);
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
        }

        private void SetForeColorThreshold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void checkBoxCropTransparent_CheckedChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }
    }
}
