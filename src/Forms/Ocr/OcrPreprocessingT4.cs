using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic.Ocr;
using Bitmap = System.Drawing.Bitmap;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class OcrPreprocessingT4 : Form
    {
        private readonly NikseBitmap _source;
        public PreprocessingSettings PreprocessingSettings { get; }

        public OcrPreprocessingT4(Bitmap bitmap, PreprocessingSettings preprocessingSettings)
        {
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
                    BinaryImageCompareThreshold = Configuration.Settings.Tools.OcrBinaryImageCompareRgbThreshold
                };
            }

            numericUpDownThreshold.Value = PreprocessingSettings.BinaryImageCompareThreshold;

            Text = Configuration.Settings.Language.OcrPreprocessing.Title;
            groupBoxBinaryImageCompareThreshold.Text = Configuration.Settings.Language.OcrPreprocessing.BinaryThreshold;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            RefreshImage();
        }

        private void numericUpDownThreshold_ValueChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        public static Bitmap ResizeBitmap(Bitmap b, int width, int height)
        {
            var result = new Bitmap(width, height);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(b, 0, 0, width, height);
            }

            return result;
        }

        private void RefreshImage()
        {
            PreprocessingSettings.InvertColors = checkBoxInvertColors.Checked;
            PreprocessingSettings.BinaryImageCompareThreshold = (int)numericUpDownThreshold.Value;
            PreprocessingSettings.ScalingPercent = (int)numericUpDownScaling.Value;

            pictureBox1.Image?.Dispose();
            var n = new NikseBitmap(_source);

            if (PreprocessingSettings.InvertColors)
            {
                n.InvertColors();
            }

            n.MakeTwoColor((int)numericUpDownThreshold.Value, Color.White, Color.Black);

            if (PreprocessingSettings.ScalingPercent > 100)
            {
                var bTemp = n.GetBitmap();
                var f = PreprocessingSettings.ScalingPercent / 100.0;
                var b = ResizeBitmap(bTemp, (int)Math.Round(bTemp.Width * f), (int)Math.Round(bTemp.Height * f));
                bTemp.Dispose();
                pictureBox1.Image = b;
                return;
            }

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

        private void SetForeColorThreshold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void checkBoxInvertColors_CheckedChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void numericUpDownScaling_ValueChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }
    }
}
