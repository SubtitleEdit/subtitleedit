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

        private void RefreshImage()
        {
            PreprocessingSettings.BinaryImageCompareThreshold = (int)numericUpDownThreshold.Value;

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

        private void SetForeColorThreshold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
