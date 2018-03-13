using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class SetForeColorThreshold : Form
    {
        private readonly NikseBitmap _source;

        public SetForeColorThreshold(Bitmap bitmap)
        {
            InitializeComponent();
            _source = new NikseBitmap(bitmap);
            pictureBoxSubtitleImage.Image = bitmap;
            numericUpDownThreshold.Value = Configuration.Settings.Tools.OcrBinaryImageCompareRgbThreshold;
        }

        public int Threshold { get; set; }

        private void numericUpDownThreshold_ValueChanged(object sender, EventArgs e)
        {
            pictureBox1.Image?.Dispose();
            var n = new NikseBitmap(_source);
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
    }
}
